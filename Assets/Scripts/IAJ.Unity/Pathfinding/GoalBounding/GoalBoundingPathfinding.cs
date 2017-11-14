using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.NavMesh;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using RAIN.Navigation.Graph;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Bounds = Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding.Bounds;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding
{
    public class GoalBoundingPathfinding : NodeArrayAStarPathFinding
    {
        public GoalBoundingTable GoalBoundingTable { get; protected set;}
        
        //public int DiscardedEdges { get; protected set; }
		public int TotalEdges { get; protected set; }

        private NodeGoalBounds startingNodeGoalBounds;

        public GoalBoundsDijkstraMapFlooding digjstra;
        public NodeGoalBounds toPrintNodeGoulBounds;


        public override string AlgorithmName {
            get {
                return "GoalBoundingPathfinding";
            }
        }


        public GoalBoundingPathfinding(NavMeshPathGraph graph, IHeuristic heuristic, GoalBoundingTable goalBoundsTable) : base(graph, heuristic)
        {

            this.LiveCalculation = false;
            this.GoalBoundingTable = goalBoundsTable;
            digjstra = new GoalBoundsDijkstraMapFlooding(graph);

        }

        public override void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition) {
            this.DiscardedEdges = 0;
            this.TotalEdges = 0;
            this.NullTableNodesCount = 0;
            base.InitializePathfindingSearch(startPosition, goalPosition);
        }

        protected override void ProcessChildNode(NodeRecord parentNode, NavigationGraphEdge connectionEdge, int edgeIndex) {
            //TODO: Implement this method for the GoalBoundingPathfinding to Work. 
            // If you implemented the NodeArrayAStar properly, you wont need to change the search method.
            //var childNode = connectionEdge.ToNode;
            //var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);
            TotalEdges++;
            //var table = GoalBoundingTable;
            //var xD = table.table;
            //var b = xD;
            //NodeGoalBounds pls = GoalBoundingTable.table[0] as NodeGoalBounds;
            //Debug.Log(parentNode.node.NodeIndex);
            //Debug.Log(pls);
            //var a = pls;

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            //perhaps do the dikstra on the fly?
            if (this.StartNode.Equals(parentNode.node) || this.StartNode.Equals(childNode)) {
                base.ProcessChildNode(parentNode, connectionEdge, edgeIndex);
                return;
            }

            if (this.GoalNode.Equals(childNodeRecord.node) || this.GoalNode.Equals(childNode)) {
                base.ProcessChildNode(parentNode, connectionEdge, edgeIndex);
                return;
            }

            var computedLive = false;
            if (!LiveCalculation) {
                toPrintNodeGoulBounds = GoalBoundingTable.table[parentNode.node.NodeIndex];

                if (toPrintNodeGoulBounds == null) {
                    if (!(parentNode.node is NavMeshEdge)) {
                        Debug.Log("Parent is not NavMeshEdge");
                    }
                    Debug.Log("NULL BOY on: " + parentNode.node.NodeIndex);
                    //base.ProcessChildNode(parentNode, connectionEdge, edgeIndex);
                    NullTableNodesCount++;
                }
            }
            //for debug purposes
            digjstra.StartNode = parentNode.node;
            if (LiveCalculation) {
                computedLive = true;
                NodeGoalBounds ngb = (NodeGoalBounds)ScriptableObject.CreateInstance(typeof(NodeGoalBounds));
                var outConnectionsStart = parentNode.node.OutEdgeCount;
                ngb.connectionBounds = new DataStructures.GoalBounding.Bounds[outConnectionsStart];
                for (int i = 0; i < ngb.connectionBounds.Length; i++) {
                    ngb.connectionBounds[i] = (DataStructures.GoalBounding.Bounds)ScriptableObject.CreateInstance(typeof(DataStructures.GoalBounding.Bounds));
                }
                digjstra.Search(parentNode.node, ngb);
                toPrintNodeGoulBounds = ngb;

            } else if (toPrintNodeGoulBounds == null){
                //null entries on table are processed as normal A*
                base.ProcessChildNode(parentNode, connectionEdge, edgeIndex);
                return;
            }
            var varToPrintNodeGoulBounds = toPrintNodeGoulBounds;
            if(edgeIndex >= varToPrintNodeGoulBounds.connectionBounds.Length) {
                Debug.Log("Why You DO this to me");
            }
            //Debug.Log("ComputedLive: " + computedLive);
            //Debug.Log("Index:" + edgeIndex + " x:" + toPrintNodeGoulBounds.connectionBounds[edgeIndex].minx + " > " + toPrintNodeGoulBounds.connectionBounds[edgeIndex].maxx 
            //    + " z: " + toPrintNodeGoulBounds.connectionBounds[edgeIndex].minz + " > " + toPrintNodeGoulBounds.connectionBounds[edgeIndex].maxz 
            //    + " :GoalPosition: " + GoalPosition + " insideBound?:" + toPrintNodeGoulBounds.connectionBounds[edgeIndex].PositionInsideBounds(GoalPosition));
            if (toPrintNodeGoulBounds.connectionBounds[edgeIndex].PositionInsideBounds(GoalPosition)) {
                base.ProcessChildNode(parentNode, connectionEdge, edgeIndex);
                return;
            }
            DiscardedEdges++;
        }
        

    }
}
