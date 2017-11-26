using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using System.Linq;
using System;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class NewMCTSNode
    {
        public const float C = 1.4f;// * 525;
        
        public NewWorldModel State { get; private set; }
        public NewMCTSNode Parent { get; set; }
        public GOB.Action Action { get; set; }
        public int PlayerID { get; set; }
        public List<NewMCTSNode> ChildNodes { get; private set; }
        public int N { get; set; }
        public float Q { get; set; }
        public double utcValue { get; private set; }
        public int bestChildIndex;
        public int indexInParent;
        public List<Pair<int, double>> bestNodesSorted ;


        public NewMCTSNode(NewWorldModel state,int indexInParent)
        {
            this.State = state;
            this.ChildNodes = new List<NewMCTSNode>();
                                        // index of children, utcOfChildren
            this.bestNodesSorted = new List<Pair<int, double>>();
            this.indexInParent = indexInParent;
        }

        public string ToXML(int depth) {

            //if (ChildNodes.Count > 0) {
            //    toReturn += tabSpaces + " <Number_Childs> " + ChildNodes.Count + " </Number_Childs>";
            //}
            string tabSpaces = "\n";
            for(int i = 0; i < depth; i++) {
                tabSpaces += " ";
            }

            string toReturn = tabSpaces + "<Node>";
            if(this.Action != null) {
                toReturn += tabSpaces + " <Action> " + this.Action.xmlName+ " </Action>";
            }
            toReturn += tabSpaces + " <N>" +(int)N +"</N>";
            toReturn += tabSpaces + " <Q>" + (int)Q + "</Q>";
            toReturn += tabSpaces + " <Q_N_div>"+ (Q / N) + "</Q_N_div>";
            toReturn += tabSpaces + " <Terminal>"+ State.IsTerminal() + "</Terminal>";
            if(this.Parent != null) {

            var firstPart = this.Q / this.N;
            var secondPart = 1.4f * 400 * Math.Sqrt(Math.Log(this.Parent.N) / this.N);
            toReturn += tabSpaces + " <BestUTC>"+ (firstPart + secondPart)+ "</BestUTC>";
            } else {
                toReturn += tabSpaces + " <BestUTC>" + 0 + "</BestUTC>";
            }
            foreach (var node in ChildNodes) {
                toReturn += node.ToXML(depth + 1);
            }
            toReturn += tabSpaces + "</Node>";
            return toReturn;
        }

        internal int RecursiveNumberOfChilds() {
            int toReturn = 1;
            foreach (var node in ChildNodes) {
                toReturn += node.RecursiveNumberOfChilds();
            }
            return toReturn;
        }

        public void RecalculateUTC() {
            if (this.Parent != null) {
                var firstPart = this.Q / this.N;
                var secondPart = C * Math.Sqrt(Math.Log(this.Parent.N) / this.N);
                utcValue = firstPart + secondPart;
                //this.Parent.recalculateChildPosition(this.indexInParent, utcValue);
            }
        }

        private void recalculateChildPosition(int childIndex, double childNewValue) {
            //needs testing

            //Pair<int,double> old = this.bestNodesSorted

            this.bestNodesSorted.RemoveAt(0);
            int index = 0;
            foreach(var node in bestNodesSorted) {
                if (childNewValue > node.Second) {
                    break;
                }
                index++; 
            }
            bestNodesSorted.Add(new Pair<int, double>(childIndex, childNewValue));
        }
    }
}
