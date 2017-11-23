using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuickGraph;
using System.IO;
using System.Threading;

namespace GrarphTutorial {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private IBidirectionalGraph<object, IEdge<object>> _graphToVisualize;
        public IBidirectionalGraph<object, IEdge<object>> GraphToVisualize {
            get { return _graphToVisualize; }
        }

        public object lastUID { get; private set; }

        public MainWindow()
        {

            Console.WriteLine("Cenas");
            var g = ReadGraph();
            CreateGraphToVisualize(g);
            //CreateGraphToVisualize();

            //            lastUID = "";
            //            while (true) {
            //                var graphRoot = ReadGraph();
            //                if(graphRoot != null) {
            //                    CreateGraphToVisualize(graphRoot);
            //                    InitializeComponent();
            //                    
            //                }
            //                Thread.Sleep(5000);
            //            }
        }


        private GraphNode ReadGraph() {
            string[] readText = File.ReadAllLines(@"C:\treeXml\tree.xml");
            //var newUID = readText[0];
            //if (newUID.Equals(lastUID)) {
            //    return null;
            //} else {
            //    lastUID = newUID;
            //}
            GraphNode rootNode = new GraphNode(null,new List<int>());
            rootNode.closeNode();



            GraphNode curNode = rootNode;
            List<int> position = new List<int>();



            bool closeInRow = false;
            bool openInRow = true;
            var line = "";
            for (int i = 1; i < readText.Length; i++) {
                line = readText[i].Trim();
                if (line.Equals("<Node>")) {
                    closeInRow= false;
                    if (openInRow) {
                        //child
                        position.Add(0);
                    } else {
                        openInRow = true;
                        //brother
                        position[position.Count - 1]++;
                    }

                    var child = new GraphNode(curNode,position);
                    curNode.childs.Add(child);
                    curNode = child;
                } else if (line.Contains("<Action>")) {
                    line = Trimmer.TrimStart(line, "<Action>");
                    line = Trimmer.TrimEnd(line, "</Action>");
                    curNode.addAction(line);
                } else if (line.Contains("<Q>")) {
                    line = Trimmer.TrimStart(line, "<Q>");
                    line = Trimmer.TrimEnd(line, "</Q>");
                    curNode.addQuality(line);
                } else if (line.Contains("<N>")) {
                    line = Trimmer.TrimStart(line, "<N>");
                    line = Trimmer.TrimEnd(line, "</N>");
                    curNode.addNumber(line);
                } else if (line.Contains("<Q_N_div>")) {
                    line = Trimmer.TrimStart(line, "<Q_N_div>");
                    line = Trimmer.TrimEnd(line, "</Q_N_div>");
                    curNode.addDivision(line);
                } else if (line.Contains("<Terminal>")) {
                    line = Trimmer.TrimStart(line, "<Terminal>");
                    line = Trimmer.TrimEnd(line, "</Terminal>");
                    curNode.addIsTerminal(line);
                } else if (line.Contains("<BestUTC>")) {
                    line = Trimmer.TrimStart(line, "<BestUTC>");
                    line = Trimmer.TrimEnd(line, "</BestUTC>");
                    curNode.addBestUTC(line);
                } else if (line.Contains("</Node>")) {
                    curNode.closeNode();
                    openInRow = false;
                    if (closeInRow) {
                        //going to father
                        position.RemoveAt(position.Count-1);
                    } else {
                        //might go to brother or fgather
                        closeInRow = true;
                    }
                    curNode = curNode.parent;
                }
            }
            return rootNode;
        }


        private void CreateGraphToVisualize(GraphNode graphRoot) {
            var g = new BidirectionalGraph<object, IEdge<object>>();
            graphRoot.putYourselfInGraph(g);
            _graphToVisualize = g;
        }
        private void CreateGraphToVisualize() {
            var g = new BidirectionalGraph<object, IEdge<object>>();

            string[] vertices = new string[5];
            for (int i = 0; i < 5; i++) {
                vertices[i] = "Bola Nº " + i.ToString();
                g.AddVertex(vertices[i]);
            }

            g.AddEdge(new Edge<object>(vertices[0], vertices[1]));
            g.AddEdge(new Edge<object>(vertices[2], vertices[3]));
            g.AddEdge(new Edge<object>(vertices[2], vertices[4]));
            g.AddEdge(new Edge<object>(vertices[1], vertices[2]));
            g.AddEdge(new Edge<object>(vertices[0], vertices[3]));
            
            _graphToVisualize = g;
        }
    }
    //public class MyEdge : TypedEdge<Object> {
    //    public String Id { get; set; }

    //    public Color EdgeColor { get; set; }

    //    public MyEdge(Object source, Object target) : base(source, target, EdgeTypes.General) { }
    //}

    //public class EdgeColorConverter : IValueConverter {

    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    //        return new SolidColorBrush((Color)value);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    //        throw new NotImplementedException();
    //    }
    //}
}
