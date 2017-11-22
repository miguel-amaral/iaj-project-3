using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSNode
    {
        public WorldModel State { get; private set; }
        public MCTSNode Parent { get; set; }
        public GOB.Action Action { get; set; }
        public int PlayerID { get; set; }
        public List<MCTSNode> ChildNodes { get; private set; }
        public int N { get; set; }
        public float Q { get; set; }


        public MCTSNode(WorldModel state)
        {
            this.State = state;
            this.ChildNodes = new List<MCTSNode>();
        }

        public string ToXML(int depth) {

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
            //if (ChildNodes.Count > 0) {
            //    toReturn += tabSpaces + " <Number_Childs> " + ChildNodes.Count + " </Number_Childs>";
            //}
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
    }
}
