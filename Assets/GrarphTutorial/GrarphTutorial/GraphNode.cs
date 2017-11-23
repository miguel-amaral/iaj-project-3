using System;
using System.Collections.Generic;
using QuickGraph;
using System.Globalization;

namespace GrarphTutorial {
    class GraphNode {
        public string label = "";
        public string str_position = "";
        public string action = "";
        public int quality = 0;
        public int number = 0;
        public float division= 0;
        public GraphNode parent;
        public List<GraphNode> childs;
        public Container container;
        private bool isTerminal;
        private float bestUTC;

        public GraphNode(GraphNode parent, List<int> position) {
            this.parent = parent;
            childs = new List<GraphNode>();
            foreach(int i in position) {
                if (i > 9) {
                    str_position += ":";
                }
                str_position += i.ToString() ;
                if (i > 9) {
                    str_position += ":";
                }

            }
        }

        internal void putYourselfInGraph(BidirectionalGraph<object, IEdge<object>> g) {
            container = new Container(label);
            g.AddVertex(container);
            foreach (GraphNode child in childs) {
                if (!child.label.Equals("")) {
                    child.putYourselfInGraph(g);
                    g.AddEdge(new Edge<object>(container, child.container) {

                    });
                }
            }
        }

        internal void addAction(string line) {
            action = line;
        }

        internal void addQuality(string line) {
            quality = Int32.Parse(line);
        }

        internal void closeNode() {
            if (printThisNode()) {
                label = infoToDisplay();
            }
        }

        private string infoToDisplay() {
            if(parent!= null) {
                //return "Action\nQ : N\nDivision\nbestUTC\nisTerminal";

                return action + "\n" + quality + " : " + number + "\n" + division + "\n" + bestUTC + (isTerminal ? "\n______" : "");
            } else {
                return "Action\nQ : N\nDivision\nbestUTC\nisTerminal";
            }
        }

        private bool printThisNode() {
        
            if(parent == null) {
                return true;
            }
            if(parent.parent == null) {
                return true;
            }

            if(quality == 0) {
                return false;
            } else if(childs.Count == 0) {
                return true;
            } else if(number > 20) {
                return true;
            }
            return false;
        }

        internal void addNumber(string line) {
            number = Int32.Parse(line);
        }

        internal void addDivision(string line) {
            division = float.Parse(line,CultureInfo.InvariantCulture);
        }

        internal void addIsTerminal(string line) {
            isTerminal = bool.Parse(line);
        }

        internal void addBestUTC(string line) {
            bestUTC = float.Parse(line, CultureInfo.InvariantCulture);
        }
    }
}
