using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathFinding : AStarPathfinding
    {
        protected NodeRecordArray NodeRecordArray { get; set; }
        public NodeArrayAStarPathFinding(NavMeshPathGraph graph, IHeuristic heuristic) : base(graph,null,null,heuristic)
        {
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
        }
        public override string AlgorithmName {
            get {
                return "NodeArrayAStarPathFinding";
            }
        }

        protected override void ProcessChildNode(NodeRecord bestNode, NavigationGraphEdge connectionEdge, int edgeIndex)
        {

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

          

            var childNodeStatus = childNodeRecord.status;
            float g = bestNode.gValue + (childNode.LocalPosition - bestNode.node.LocalPosition).magnitude;
            float h = this.Heuristic.H(childNode, this.GoalNode);
            float f = F(g, h);

            //We can only update inside the ifs because otherwise we might be making the node worse

            if (childNodeStatus == NodeStatus.Unvisited) {
                UpdateNode(bestNode, childNodeRecord, g, h, f);
                Open.AddToOpen(childNodeRecord);
            } else if (childNodeStatus == NodeStatus.Open && childNodeRecord.fValue > f) {
                UpdateNode(bestNode, childNodeRecord, g, h, f);
                Open.Replace(childNodeRecord, childNodeRecord);
            } else if (childNodeStatus == NodeStatus.Closed && childNodeRecord.fValue > f) {
                UpdateNode(bestNode, childNodeRecord, g, h, f);
                Closed.RemoveFromClosed(childNodeRecord);
                TotalExploredNodes--;
                Open.AddToOpen(childNodeRecord);
            }
            
        }

        protected void UpdateNode(NodeRecord bestNode, NodeRecord childNode, float g, float h, float f)
        {
            childNode.gValue = g;
            childNode.hValue = h;
            childNode.fValue = f;
            childNode.parent = bestNode;
        }

        public List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>) Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }
    }
}
