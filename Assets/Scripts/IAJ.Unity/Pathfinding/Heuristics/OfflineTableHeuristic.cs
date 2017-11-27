using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class OfflineTableHeuristic 
    {

        private SerializableDictionary<Pair<string, string>, float> _table;
        
        private static OfflineTableHeuristic _instance;

        public static OfflineTableHeuristic Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OfflineTableHeuristic();
                return _instance;
            }
        }

        private OfflineTableHeuristic()
        {
            LoadTable();
            //InitGameObjects();
        }

        
        public bool GotEntry(string source, string target)
        {
            //Debug.Log(source + " - " + target);
            return _table.ContainsKey(new Pair<string, string>(source, target));
        }

        public float H(string source, string target)
        {
            //Debug.Log("Sai uma heuristica! " + source + " - " + target);
            return _table[new Pair<string, string>(source, target)];
        }

        public void LoadTable()
        {
            // We will assume a default name here

            string assetPathAndName =
                GoalBoundingTable.Path() + "/" + SceneManager.GetActiveScene().name + "_heuristic.bin";

            using (Stream stream =
                new FileStream(assetPathAndName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                IFormatter formatter = new BinaryFormatter();
                _table = formatter.Deserialize(stream) as SerializableDictionary<Pair<string, string>, float>;
            }

        }

        public static void SaveTable(SerializableDictionary<Pair<string, string>, float> table, string currentSceneName)
        {
            if(table == null) return;
            string assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(GoalBoundingTable.Path() + "/" +
                                                      currentSceneName + "_heuristic.bin");

            using (Stream stream = new FileStream(assetPathAndName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, table);
            }

        }

    }
}