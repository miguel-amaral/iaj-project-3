using UnityEngine;
using UnityEditor;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using System.Threading;
using System;

public class PathSmoothing {
    //private readonly Collider _collider;

    //public PathSmoothing() {
    //    _collider = new Collider();
    //}

    public GlobalPath Smooth(Vector3 position, GlobalPath globalPath) {
        globalPath.Smoothed = true;
        GlobalPath toReturn = new GlobalPath();
        toReturn.PathPositions.Add(position);
        if(globalPath.PathPositions.Count < 2) {
            return globalPath;
        }

        var previousSavedNode = globalPath.PathPositions[0];
        toReturn.PathPositions.Add(previousSavedNode);

        var lastIgnoredNode = globalPath.PathPositions[1];

        for (int index = 2; index < globalPath.PathPositions.Count; index++) {
            var nodeCurrentlyBeingConsidered = globalPath.PathPositions[index];
            if (IsThereCollisionBetween(previousSavedNode, nodeCurrentlyBeingConsidered)) {
                toReturn.PathPositions.Add(lastIgnoredNode);
                previousSavedNode = lastIgnoredNode;
            }
            lastIgnoredNode = nodeCurrentlyBeingConsidered;
        }
        toReturn.PathPositions.Add(globalPath.PathPositions[globalPath.PathPositions.Count-1]);


        return toReturn;
    }

    private bool IsThereCollisionBetween(Vector3 start, Vector3 end) {
        var direction = end - start;
        float maxDistance = direction.magnitude;
        return Physics.Raycast(start, direction.normalized, maxDistance);
    }


}