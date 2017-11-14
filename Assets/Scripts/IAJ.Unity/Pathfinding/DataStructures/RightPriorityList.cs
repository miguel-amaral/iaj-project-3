using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures {
    public class RightPriorityList : IOpenSet, IComparer<NodeRecord> {
        private List<NodeRecord> Open { get; set; }

        public RightPriorityList() {
            this.Open = new List<NodeRecord>();
        }
        public void Initialize() {
            this.Open.Clear();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace) {
            this.RemoveFromOpen(nodeToBeReplaced);
            this.AddToOpen(nodeToReplace);
        }

        public NodeRecord GetBestAndRemove() {
            var best = this.PeekBest();
            this.Open.RemoveAt(this.Open.Count - 1);
            return best;
        }

        public NodeRecord PeekBest() {
            return this.Open[this.Open.Count - 1];
        }

        public void AddToOpen(NodeRecord nodeRecord) {
            //a little help here, notice the difference between this method and the one for the LeftPriority list
            //...this one uses a different comparer with an explicit compare function (which you will have to define below)
            int index = this.Open.BinarySearch(nodeRecord, this);
            if (index < 0) {
                this.Open.Insert(~index, nodeRecord);
            }
        }


        //Possivelmente pode vir a ser mudado
        public void RemoveFromOpen(NodeRecord nodeRecord) {
            this.Open.Remove(nodeRecord);
            //int index = this.Open.BinarySearch(nodeRecord, this);
            
            //if (index >= 0) {
            //    this.Open.RemoveAt(index);
            //}
        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord) {
            return this.Open.FirstOrDefault(n => n.Equals(nodeRecord));
        }

        public ICollection<NodeRecord> All() {
            return new List<NodeRecord>(this.Open);
        }

        public int CountOpen() {
            return this.Open.Count;
        }

        public int Compare(NodeRecord x, NodeRecord y) {
            //Less than zero x is less than y.
            //Zero x equals y.
            //Greater than zero x is greater than y.
            return -(x.CompareTo(y));
        }
    }
}
