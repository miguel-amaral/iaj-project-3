﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.Graph;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class OfflineTableHeuristic 
    {

        private SerializableDictionary<Pair<string, string>, float> _table = null;
        
        private static OfflineTableHeuristic _instance;

        private OfflineTableHeuristic()
        {
            LoadTable();
            InitGameObjects();
        }

        public static OfflineTableHeuristic Instance
        {
            get
            {
                if (OfflineTableHeuristic._instance == null)
                    OfflineTableHeuristic._instance = new OfflineTableHeuristic();
                return OfflineTableHeuristic._instance;
            }
        }

        public float H(string name1, string name2)
        {
            return _table[new Pair<string, string>(name1, name2)];
        }

        public void LoadTable()
        {
            // We will assume a default name here

            string assetPathAndName =
                GoalBoundingTable.Path() + "/" + typeof(OfflineTableHeuristic).Name.ToString() + ".bin";

            using (Stream stream =
                new FileStream(assetPathAndName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                IFormatter formatter = new BinaryFormatter();
                _table = formatter.Deserialize(stream) as SerializableDictionary<Pair<string, string>, float>;
            }

        }

        public static void SaveTable(SerializableDictionary<Pair<string, string>, float> table)
        {
            if(table == null) return;
            string assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(GoalBoundingTable.Path() + "/" +
                                                      typeof(OfflineTableHeuristic).Name.ToString() + ".bin");

            using (Stream stream = new FileStream(assetPathAndName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, table);
            }

        }

        private List<Pair<string, Vector3>> _ourGameObjects;

        public List<Pair<string, Vector3>> OurGameObjects()
        {
            if (_ourGameObjects == null)
            {
                InitGameObjects();
            }
            return _ourGameObjects;
        }

        private void InitGameObjects()
        {
            var hPots = GameObject.FindGameObjectsWithTag("HealthPotion");
            var mPots = GameObject.FindGameObjectsWithTag("ManaPotion");
            var chests = GameObject.FindGameObjectsWithTag("Chest");
            var orcs = GameObject.FindGameObjectsWithTag("Orc");
            var skeletons = GameObject.FindGameObjectsWithTag("Skeleton");
            var dragons = GameObject.FindGameObjectsWithTag("Dragon");
            var ourGameObjects = hPots.Concat(mPots).Concat(chests).Concat(orcs).Concat(skeletons).Concat(dragons)
                .ToList();
            _ourGameObjects = new List<Pair<string, Vector3>>();
            foreach (var ourGameObject in ourGameObjects)
            {
                _ourGameObjects.Add(new Pair<string, Vector3>(ourGameObject.name, ourGameObject.transform.position));
            }
        }
    }
}