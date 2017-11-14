using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Assets.Resources.Editor
{
    public class IajMenuItemsThreaded
    {
        public static int Progress;
        public static GoalBoundingTable GoalBoundingTable;

        private static readonly int AuxThreads = SystemInfo.processorCount;

        [MenuItem("IAJ/Calculate Goal Bounds (Threaded)")]
        private static void CalculateGoalBounds()
        {
            WriteTimestampToFile("Start");
            //get the NavMeshGraph from the current scene
            NavMeshPathGraph navMesh = GameObject.Find("Navigation Mesh").GetComponent<NavMeshRig>().NavMesh.Graph;

            //this is needed because RAIN AI does some initialization the first time the QuantizeToNode method is called
            //if this method is not called, the connections in the navigationgraph are not properly initialized
            navMesh.QuantizeToNode (new Vector3 (0, 0, 0), 1.0f);


            GoalBoundingTable = ScriptableObject.CreateInstance<GoalBoundingTable>();
            var nodes = GetNodesHack(navMesh);
            GoalBoundingTable.table = new NodeGoalBounds[nodes.Count];

            // Init Multithread
            var doneEvents = new ManualResetEvent[AuxThreads];

            //calculate goal bounds for each edge
            EditorUtility.DisplayProgressBar("GoalBounding precomputation progress",
                "Calculating goal bounds for each edge", 0);

            int doneEventsIterator = 0;
            for (int i=0; i < nodes.Count; i++)
            {
                if (nodes[i] is NavMeshEdge)
                {
                    //initialize the GoalBounds structure for the edge
                    var auxGoalBounds = ScriptableObject.CreateInstance<NodeGoalBounds>();
                    auxGoalBounds.connectionBounds =
                        new Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding.Bounds[nodes[i].OutEdgeCount];
                    for (int j = 0; j < nodes[i].OutEdgeCount; j++)
                    {
                        auxGoalBounds.connectionBounds[j] =
                            ScriptableObject.CreateInstance<Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding.Bounds>();
                        auxGoalBounds.connectionBounds[j].InitializeBounds(nodes[i].Position);
                    }


                    doneEvents[doneEventsIterator] = new ManualResetEvent(false);
                    var task = new IAJTask(doneEvents[doneEventsIterator], navMesh, 
                        nodes[i], auxGoalBounds);
                    ThreadPool.QueueUserWorkItem(task.ThreadPoolCallback, i);

                    if (doneEventsIterator == AuxThreads - 1)
                    {
                        float percentage = (float)IajMenuItemsThreaded.Progress / (float)nodes.Count;
                        EditorUtility.DisplayProgressBar("GoalBounding precomputation progress",
                            "Calculating goal bounds for each edge", percentage);
                        WaitHandle.WaitAll(doneEvents);
                        doneEventsIterator = 0;
                    }
                    else
                    {
                        doneEventsIterator++;
                    }
                }

            }
            WaitHandle.WaitAll(doneEvents);
            EditorUtility.DisplayProgressBar("GoalBounding precomputation progress",
                "Calculating goal bounds for each edge", 1.0f);
            Progress = 0;

            //saving the assets, this takes forever using Unity's serialization mechanism
            WriteTimestampToFile("End of GoalBoundsDijkstraMapFlooding");

            GoalBoundingTable.SaveToAssetDatabaseOptimized();
            WriteTimestampToFile("End of Storing");
            EditorUtility.ClearProgressBar();
        }

        public static void WriteTimestampToFile(string tag, string filename = "threaded.txt", bool clearFile = false)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + filename;
            if (File.Exists(path))
            {
                if (clearFile)
                {
                    File.Delete(path);
                }
                using (var streamWriter = File.AppendText(path))
                {
                    streamWriter.WriteLine(tag + " - " + System.DateTime.Now.ToLongTimeString());
                }
            }
            else
            {
                using (var streamWriter = File.CreateText(path))
                {
                    streamWriter.WriteLine(tag + " - " + System.DateTime.Now.ToLongTimeString());
                }
            }

        }

        private static List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>)Assets.Scripts.IAJ.Unity.Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }
    }


    public class IAJTask
    {
        private readonly ManualResetEvent _doneEvent;
        private readonly GoalBoundsDijkstraMapFlooding _dijkstra;
        private NodeGoalBounds _goalBoundingTableItem;
        private NavigationGraphNode _node;
        private readonly NodeGoalBounds _auxGoalBounds;

        public IAJTask(ManualResetEvent doneEvent, NavMeshPathGraph navMesh, 
           NavigationGraphNode node, NodeGoalBounds auxGoalBounds)
        {
            _doneEvent = doneEvent;
            _dijkstra= new GoalBoundsDijkstraMapFlooding(navMesh);
            _node = node;
            _auxGoalBounds = auxGoalBounds;
        }

        public void ThreadPoolCallback(object state)
        {
            //run a Dijkstra mapflooding for each node
            _dijkstra.Search(_node, _auxGoalBounds);
            // progress
            IajMenuItemsThreaded.Progress++;
            

            int index = (int) state;
            IajMenuItemsThreaded.GoalBoundingTable.table[index] = _auxGoalBounds;
            _doneEvent.Set();
        }

    
    }
}