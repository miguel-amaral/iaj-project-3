using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.NavMesh;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Path = System.IO.Path;

//using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;

namespace Assets.Editor
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

            var paths = GetScenePaths();

            //for (var i = 0; i < paths.Count; i++)
            //{
            //    var scenePath = paths[i];
            //    Debug.Log(scenePath);
            //    //EditorSceneManager.OpenScene(scenePath);


                var ourGameObjects = OurGameObjects();

                int progress = 0;

                foreach (var ourGameObject in ourGameObjects)
                {
                    foreach (var otherOurGameObject in ourGameObjects)
                    {
                        progress++;
                        if (progress % 5 == 0)
                        {
                            var percentage = (float) progress / (float) (2 * ourGameObjects.Length);

                            EditorUtility.DisplayProgressBar("Heuristic precomputation progress",
                                "Calculating distance for each object", percentage);
                        }

                        if (ourGameObject.name.Equals(otherOurGameObject.name))
                        {
                            table.Add(new Pair<string, string>(ourGameObject.name, otherOurGameObject.name), 0);
                            continue;
                        }

                        goalBoundingPathfinding.InitializePathfindingSearch(ourGameObject.transform.position,
                            otherOurGameObject.transform.position);
                        GlobalPath solution;
                        goalBoundingPathfinding.Search(out solution);
                        solution = pathsmoother.Smooth(solution);
                        table.Add(new Pair<string, string>(ourGameObject.name, otherOurGameObject.name),
                            solution.PathLength());
                        goalBoundingPathfinding.CleanUp();
                    }
                }
                OfflineTableHeuristic.SaveTable(table, EditorSceneManager.GetActiveScene().name);
                EditorUtility.ClearProgressBar();
            //}

        }


        private static List<string> GetScenePaths()
        {
            //var path = GoalBoundingTable.Path() + "/Scenes";
            //var dir = new DirectoryInfo(path);
            //return (from fileInfo in dir.GetFiles() where fileInfo.Name.EndsWith(".unity") select (path + "/" + fileInfo.Name)).ToList();
            

            return new List<string>
            {
                EditorSceneManager.GetActiveScene().path,
            };
        }



        private static GameObject[] OurGameObjects()
        {
            var hPots = GameObject.FindGameObjectsWithTag("HealthPotion");
            var mPots = GameObject.FindGameObjectsWithTag("ManaPotion");
            var chests = GameObject.FindGameObjectsWithTag("Chest");
            var orcs = GameObject.FindGameObjectsWithTag("Orc");
            var skeletons = GameObject.FindGameObjectsWithTag("Skeleton");
            var dragons = GameObject.FindGameObjectsWithTag("Dragon");
            var player = GameObject.FindGameObjectsWithTag("Player");
            return hPots.Concat(mPots).Concat(chests).Concat(orcs).Concat(skeletons).Concat(dragons).Concat(player).ToArray();
        }
    }
}
