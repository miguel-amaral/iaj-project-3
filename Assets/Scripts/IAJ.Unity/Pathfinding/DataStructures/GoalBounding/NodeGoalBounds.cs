using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding
{

    public class NodeGoalBounds : ScriptableObject
    {
        public Bounds[] connectionBounds;

        public NodeGoalBounds() : base()
        {
            
        }

        public NodeGoalBounds(int size)
        {
            connectionBounds = new Bounds[size];
        }

        public void Init(int size)
        {
            connectionBounds = new Bounds[size];
        }
    }
}
