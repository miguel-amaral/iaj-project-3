using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEditor;
using UnityEngine;
//using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;

namespace Assets.Resources.Editor
{
    public class IAJMenuItems 
    {
        [MenuItem("IAJ/Calculate Goal Bounds")]
        public static void CalculateGoalBounds()
        {
            IajMenuItemsThreaded.WriteTimestampToFile("Start", "no-thread.txt");

            //get the NavMeshGraph from the current scene
            NavMeshPathGraph navMesh = GameObject.Find("Navigation Mesh").GetComponent<NavMeshRig>().NavMesh.Graph;

            //this is needed because RAIN AI does some initialization the first time the QuantizeToNode method is called
            //if this method is not called, the connections in the navigationgraph are not properly initialized
            navMesh.QuantizeToNode (new Vector3 (0, 0, 0), 1.0f);

            var dijkstra = new GoalBoundsDijkstraMapFlooding(navMesh);

            GoalBoundingTable goalBoundingTable = ScriptableObject.CreateInstance<GoalBoundingTable>();
            var nodes = GetNodesHack(navMesh);
            goalBoundingTable.table = new NodeGoalBounds[nodes.Count];

            NodeGoalBounds auxGoalBounds;

            //calculate goal bounds for each edge
            for (int i=0; i < nodes.Count; i++)
            {
                //if(nodes[i] is NavMeshEdge)
                //{
                    //initialize the GoalBounds structure for the edge
                    auxGoalBounds = ScriptableObject.CreateInstance<NodeGoalBounds>();
                    auxGoalBounds.connectionBounds = new Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding.Bounds[nodes[i].OutEdgeCount];
                    for (int j = 0; j < nodes[i].OutEdgeCount; j++)
                    {
                        auxGoalBounds.connectionBounds[j] = ScriptableObject.CreateInstance<Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding.Bounds>();
                        auxGoalBounds.connectionBounds[j].InitializeBounds(nodes[i].Position);
                    }

                    if(i%10 == 0)
                    {
                        float percentage = (float)i / (float)nodes.Count;
                        EditorUtility.DisplayProgressBar("GoalBounding precomputation progress", "Calculating goal bounds for each edge", percentage);
                    }

                    //run a Dijkstra mapflooding for each node
                    dijkstra.Search(nodes[i], auxGoalBounds);

                    goalBoundingTable.table[i] = auxGoalBounds;

                    //edgeIndex++;
                //}
            }
        
            //saving the assets, this takes forever using Unity's serialization mechanism
            IajMenuItemsThreaded.WriteTimestampToFile("End of GoalBoundsDijkstraMapFlooding", "no-thread.txt");
            goalBoundingTable.SaveToAssetDatabase();
            IajMenuItemsThreaded.WriteTimestampToFile("End of Storing", "no-thread.txt");
            EditorUtility.ClearProgressBar();
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
}
