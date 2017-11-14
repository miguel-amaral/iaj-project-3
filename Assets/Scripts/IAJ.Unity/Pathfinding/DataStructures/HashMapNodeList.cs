using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class HashMapNodeList : IClosedSet
    {
        private Dictionary<NodeRecord, NodeRecord> NodeRecords { get; set; }

        public HashMapNodeList()
        {
            this.NodeRecords = new Dictionary<NodeRecord, NodeRecord>();
        }

        public void Initialize()
        {
            this.NodeRecords.Clear(); 
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            nodeRecord.status = NodeStatus.Closed;
            this.NodeRecords.Add(nodeRecord, nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            nodeRecord.status = NodeStatus.Unvisited;
            this.NodeRecords.Remove(nodeRecord);
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            NodeRecord value;
            NodeRecords.TryGetValue(nodeRecord, out value);
            return value;
        }

       

        public ICollection<NodeRecord> All()
        {
            return this.NodeRecords.Values;
        }

       
    }
}
