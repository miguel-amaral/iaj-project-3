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

            //get the NavMeshGraph from the current scene
            NavMeshPathGraph navMesh = GameObject.Find("Navigation Mesh").GetComponent<NavMeshRig>().NavMesh.Graph;

            //this is needed because RAIN AI does some initialization the first time the QuantizeToNode method is called
            //if this method is not called, the connections in the navigationgraph are not properly initialized
            navMesh.QuantizeToNode(new Vector3(0, 0, 0), 1.0f);

            GoalBoundingPathfinding goalBoundingPathfinding = 
                new GoalBoundingPathfinding(navMesh, new EuclidianHeuristic(), goalBoundTable);

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

                    if (ourGameObject.Equals(otherOurGameObject))
                    {
                        continue;
                    }

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
    }
}
