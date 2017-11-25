using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;

namespace Assets.Resources.Editor
{
    public class IajMenuHeuristic 
    {
        [MenuItem("IAJ/Heuristic Calculation")]
        public static void CalculateHeuristic()
        {
            var table = new SerializableDictionary<Pair<string, string>, float>();
            var goalBoundTable = ScriptableObject.CreateInstance<GoalBoundingTable>();
            goalBoundTable.LoadOptimized();

            //EditorSceneManager.OpenScene("Assets/Scenes/dungeon.unity");
            //EditorApplication.ExecuteMenuItem("Edit/Play");

            //while (UnityEditor.EditorApplication.isPlaying == false)
            //{
            //}

            GoalBoundingPathfinding goalBoundingPathfinding = 
                new GoalBoundingPathfinding(NavigationManager.Instance.NavMeshGraphs[0], new EuclidianHeuristic(), goalBoundTable);

            var pathsmoother = new PathSmoothing();

            var ourGameObjects = OurGameObjects();

            int progress = 0;

            foreach (var ourGameObject in ourGameObjects)
            {
                foreach (var otherOurGameObject in ourGameObjects)
                {
                    progress++;
                    if (progress % 5 == 0)
                    {
                        float percentage = (float)progress / (float)(2 * ourGameObjects.Count());

                        EditorUtility.DisplayProgressBar("Heuristic precomputation progress", 
                            "Calculating distance for each object", percentage);
                    }

                    if (ourGameObject.Equals(otherOurGameObject)) continue;
                    goalBoundingPathfinding.InitializePathfindingSearch(ourGameObject.transform.position, 
                        otherOurGameObject.transform.position);
                    GlobalPath solution;
                    goalBoundingPathfinding.Search(out solution);
                    solution = pathsmoother.Smooth(ourGameObject.transform.position, solution);
                    table.Add(new Pair<string, string>(ourGameObject.name, otherOurGameObject.name), solution.Length);
                    goalBoundingPathfinding.CleanUp();
                }
            }
            OfflineTableHeuristic.SaveTable(table);
            EditorUtility.ClearProgressBar();
           // Application.Quit();
        }


        private static IEnumerable<GameObject> OurGameObjects()
        {
            var hPots = GameObject.FindGameObjectsWithTag("HealthPotion");
            var mPots = GameObject.FindGameObjectsWithTag("ManaPotion");
            var chests = GameObject.FindGameObjectsWithTag("Chest");
            var orcs = GameObject.FindGameObjectsWithTag("Orc");
            var skeletons = GameObject.FindGameObjectsWithTag("Skeleton");
            var dragons = GameObject.FindGameObjectsWithTag("Dragon");
            return hPots.Concat(mPots).Concat(chests).Concat(orcs).Concat(skeletons).Concat(dragons);
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
