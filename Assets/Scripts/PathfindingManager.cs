using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using RAIN.Navigation;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;
using UnityEngine;
using Bounds = Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding.Bounds;
using System;

namespace Assets.Scripts {
    public class PathfindingManager : MonoBehaviour {
        private const int paceNodesPerFrame = 1;
        private uint NodesPerFrame = 1;

        private bool debugStepByStep = false;
        private bool canAdvanceStep = true;
        private KeyCode advanceToNextFrameKey = KeyCode.S;
        private KeyCode toggleDebugStepByStepKey = KeyCode.D;

        private bool drawBounds = false;
        private bool drawDirectChildren = false;
        private bool drawAllNodesOfBounds = false;
        private KeyCode drawBoundsKey = KeyCode.Q;
        private KeyCode drawDirectChildrenKey = KeyCode.W;
        private KeyCode drawAllNodesOfBoundsKey = KeyCode.E;




        //public fields to be set in Unity Editor
        public GameObject endDebugSphere;
        public GameObject startDebugSphere;
        public Camera camera;
        public GameObject p1;
        public GameObject p2;
        public GameObject p3;
        public GameObject p4;
        public GameObject p5;
        public GameObject p6;

        private KeyCode drawNavMeshKey = KeyCode.M;
        private bool drawNavMesh = false;

        //private fields for internal use only
        private Vector3 startPosition;
        private Vector3 endPosition;
        private NavMeshPathGraph navMesh;
        private int currentClickNumber;
        private GoalBoundsDijkstraMapFlooding mapFloodingAlgorithm;
        private GlobalPath currentSolution;
        private GlobalPath smoothedSolution;
        private bool draw = true;
        private int solutionIndex = 0;
        private int frameCount = 0;
        private int numberOfSteps = 0;
        //public properties
        public AStarPathfinding PathFinding { get; private set; }

        private PathSmoothing pathSmoothing = new PathSmoothing();
        private AStarPathfinding aStarPathfinding;
        private NodeArrayAStarPathFinding nodeArrayPathFinding;
        private GoalBoundingTable goalBoundTable;
        private GoalBoundingPathfinding goalBoundingPathfinding;
        private readonly KeyCode NormalAStarKeyStart = KeyCode.A;
        private readonly KeyCode NodeArrayKeyStart = KeyCode.N;
        private readonly KeyCode GoalBoundKeyStart = KeyCode.G;
        private readonly KeyCode ClearKey = KeyCode.C;
        private readonly KeyCode DecreaseNodesPerFrameKey = KeyCode.L;
        private readonly KeyCode IncreaseNodesPerFrameKey = KeyCode.O;
        private readonly KeyCode Decrease5TimesNodesPerFrameKey = KeyCode.K;
        private readonly KeyCode Increase5TimesNodesPerFrameKey = KeyCode.I;
        private GUIStyle guiStyle = new GUIStyle(); //to change font size
        private Bounds[] boundsInformationContainer;
        private Color[] c = new Color[] { new Color(128f/255f, 128f/255f, 0f/255f) ,
                                            new Color(230f/255f, 25f/255f, 75f/255f)   ,
                                            new Color(60f/255f, 180f/255f, 75f/255f)   ,
                                            new Color(255f/255f, 225f/255f, 25f/255f)  ,
                                            new Color(0f/255f, 130f/255f, 200f/255f)   ,
                                            new Color(245f/255f, 130f/255f, 48f/255f)  ,
                                            new Color(145f/255f, 30f/255f, 180f/255f)  ,
                                            new Color(70f/255f, 240f/255f, 240f/255f)  ,
                                            new Color(240f/255f, 50f/255f, 230f/255f)  ,
                                            new Color(210f/255f, 245f/255f, 60f/255f)  ,
                                            new Color(250f/255f, 190f/255f, 190f/255f) ,
                                            new Color(0f/255f, 128f/255f, 128f/255f)   ,
                                            new Color(230f/255f, 190f/255f, 255f/255f) ,
                                            new Color(170f/255f, 110f/255f, 40f/255f)  ,
                                            new Color(255f/255f, 250f/255f, 200f/255f) ,
                                            new Color(128f/255f, 0f/255f, 0f/255f   ) ,
                                            new Color(170f/255f, 255f/255f, 195f/255f) ,
                                            new Color(255f/255f, 215f/255f, 180f/255f) ,
                                            new Color(0f/255f, 0f/255f, 128f/255f) ,
                                            new Color(0f/255f, 0f/255f, 0         ),
                                            new Color(128f/255f, 128f/255f, 128f/255f)   
                        };
        private bool reallyDrawAllNodes = true;
        private KeyCode reallyDrawAllNodesKey = KeyCode.Comma;
        private KeyCode liveCalculationKey = KeyCode.F8;
        private bool liveCalculation = false;
        private int realSize;


        //Olive
        //Red
        //Green
        //Yellow
        //Blue
        //Orange
        //Purple
        //Cyan
        //Magenta
        //Lime
        //Pink
        //Teal
        //Lavender
        //Brown
        //Beige
        //Maroon
        //Mint
        //Coral
        //Navy
        //Black
        //Grey


        private void setNodesPerFrame(uint number) {
            aStarPathfinding.NodesPerFrame = number;
            nodeArrayPathFinding.NodesPerFrame = number;
            goalBoundingPathfinding.NodesPerFrame = number;
            this.NodesPerFrame = number;
        }

        public void Initialize(NavMeshPathGraph navMeshGraph, AStarPathfinding pathfindingAlgorithm) {
            guiStyle.fontSize = 20;
            this.draw = true;
            this.navMesh = navMeshGraph;

            this.PathFinding = pathfindingAlgorithm;
            setNodesPerFrame(NodesPerFrame);
        }

        // Use this for initialization
        void Awake() {
            this.currentClickNumber = 1;
            navMesh = GameObject.Find("Navigation Mesh").GetComponent<NavMeshRig>().NavMesh.Graph;
            mapFloodingAlgorithm = new GoalBoundsDijkstraMapFlooding(navMesh);

            calculateNavMeshSize(navMesh);

            aStarPathfinding =
                new AStarPathfinding(navMesh, new NodePriorityHeap(), new HashMapNodeList(), new EuclidianHeuristic());
            nodeArrayPathFinding =
                new NodeArrayAStarPathFinding(navMesh, new EuclidianHeuristic());
            goalBoundTable = ScriptableObject.CreateInstance<GoalBoundingTable>();
            var startTime = System.DateTime.Now;
            goalBoundTable.LoadOptimized();
            Debug.Log("GoalBoundTable loading time: " + (System.DateTime.Now - startTime).Milliseconds +" ms");


            //goalBoundTable = Resources.Load<GoalBoundingTable>("GoalBoundingTable");
            
            goalBoundingPathfinding =
                new GoalBoundingPathfinding(navMesh, new EuclidianHeuristic(), goalBoundTable);
            this.Initialize(navMesh, goalBoundingPathfinding);
        }

        private void calculateNavMeshSize(NavMeshPathGraph navMesh) {
            realSize = 0;
            for (int index = 0; index < navMesh.Size; index++) {
                var node = navMesh.GetNode(index);
                if(node is NavMeshEdge) {
                    realSize++;
                }
            }
        }



        // Update is called once per frame
        void Update() {
            Vector3 position;

            if (Input.GetMouseButtonDown(0)) {
                //if there is a valid position
                if (this.MouseClickPosition(out position)) {
                    //if this is the first click we're setting the start point
                    if (this.currentClickNumber == 1) {
                        //show the start sphere, hide the end one
                        //this is just a small adjustment to better see the debug sphere
                        this.startDebugSphere.transform.position = position + Vector3.up;
                        this.startDebugSphere.SetActive(true);
                        this.endDebugSphere.SetActive(false);
                        this.currentClickNumber = 2;
                        this.startPosition = position;
                        this.currentSolution = null;
                        this.draw = false;
                    } else {
                        //we're setting the end point
                        //this is just a small adjustment to better see the debug sphere
                        this.endDebugSphere.transform.position = position + Vector3.up;
                        this.endDebugSphere.SetActive(true);
                        this.currentClickNumber = 1;
                        this.endPosition = position;
                        this.draw = true;
                        //initialize the search algorithm
                        this.PathFinding.InitializePathfindingSearch(this.startPosition, this.endPosition);
                    }
                }
            } else if (Input.GetKeyDown(KeyCode.Alpha1)) {
                this.startPosition = this.p5.transform.localPosition;
                this.endPosition = this.p6.transform.localPosition;
                this.InitializePathFinding(this.startPosition, endPosition);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                this.startPosition = this.p1.transform.localPosition;
                this.endPosition = this.p2.transform.localPosition;
                this.InitializePathFinding(this.startPosition, endPosition);
            } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                this.startPosition = this.p2.transform.localPosition;
                this.endPosition = this.p4.transform.localPosition;
                this.InitializePathFinding(this.startPosition, endPosition);
            } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                this.startPosition = this.p2.transform.localPosition;
                this.endPosition = this.p5.transform.localPosition;
                this.InitializePathFinding(this.startPosition, endPosition);
            } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
                this.startPosition = this.p1.transform.localPosition;
                this.endPosition = this.p3.transform.localPosition;
                this.InitializePathFinding(this.p1.transform.localPosition, this.p3.transform.localPosition);
            } else if (Input.GetKeyDown(KeyCode.Alpha6)) {
                this.startPosition = this.p3.transform.localPosition;
                this.endPosition = this.p4.transform.localPosition;
                this.InitializePathFinding(this.startPosition, endPosition);


            } else if (Input.GetKeyDown(drawNavMeshKey)) {
                this.drawNavMesh = !this.drawNavMesh;
            } else if (Input.GetKeyDown(NodeArrayKeyStart)) {
                this.PathFinding = nodeArrayPathFinding;
                this.InitializePathFinding(this.startPosition, endPosition);

            } else if (Input.GetKeyDown(GoalBoundKeyStart)) {
                this.PathFinding = goalBoundingPathfinding;
                this.InitializePathFinding(this.startPosition, endPosition);
            } else if (Input.GetKeyDown(NormalAStarKeyStart)) {
                this.PathFinding = aStarPathfinding;
                this.InitializePathFinding(this.startPosition, endPosition);


            } else if (Input.GetKeyDown(ClearKey)) {
                this.currentSolution = null;
                this.PathFinding.InProgress = false;
            } else if (Input.GetKeyDown(IncreaseNodesPerFrameKey)) {
                this.setNodesPerFrame(NodesPerFrame + paceNodesPerFrame);
            } else if (Input.GetKeyDown(DecreaseNodesPerFrameKey)) {
                if (NodesPerFrame > paceNodesPerFrame) {
                    this.setNodesPerFrame(NodesPerFrame - paceNodesPerFrame);
                }
            } else if (Input.GetKeyDown(Increase5TimesNodesPerFrameKey)) {
                this.setNodesPerFrame(NodesPerFrame + paceNodesPerFrame * 5);
            } else if (Input.GetKeyDown(Decrease5TimesNodesPerFrameKey)) {
                if (NodesPerFrame > paceNodesPerFrame * 5) {
                    this.setNodesPerFrame(NodesPerFrame - paceNodesPerFrame * 5);
                }
            } else if (Input.GetKeyDown(advanceToNextFrameKey)) {
                canAdvanceStep = true;
                numberOfSteps++;
            } else if (Input.GetKeyDown(toggleDebugStepByStepKey)) {
                debugStepByStep = !debugStepByStep;
            } else if (Input.GetKeyDown(drawBoundsKey)){
                drawBounds = !drawBounds;
            } else if (Input.GetKeyDown(drawDirectChildrenKey)){
                drawDirectChildren = !drawDirectChildren;
            } else if (Input.GetKeyDown(drawAllNodesOfBoundsKey)) {
                drawAllNodesOfBounds = !drawAllNodesOfBounds;
            } else if (Input.GetKeyDown(reallyDrawAllNodesKey)) {
                reallyDrawAllNodes = !reallyDrawAllNodes;
            } else if (Input.GetKeyDown(liveCalculationKey)) {
                liveCalculation = !liveCalculation;
                this.PathFinding.LiveCalculation = liveCalculation;
            }




            //call the pathfinding method if the user specified a new goal
            if (this.PathFinding.InProgress) {
                if(!debugStepByStep || (debugStepByStep && canAdvanceStep)){
                    canAdvanceStep = false;
                    var finished = this.PathFinding.Search(out this.currentSolution, true);
                    if (finished && currentSolution != null) {
                        //currentSolution.PathPositions.Insert(0, startPosition);
                        //solutionIndex = 0;
                        //Smooth it

                        smoothedSolution = pathSmoothing.Smooth(startPosition,currentSolution);
                        smoothedSolution = pathSmoothing.Smooth(startPosition,smoothedSolution);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.H)) {
                draw = true;
                var StartNode = navMesh.QuantizeToNode(this.startPosition, 1.0f);
                //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
                if (StartNode == null) return;

                var a = new NodeGoalBounds();
                var outConnectionsStart = StartNode.OutEdgeCount;
                a.connectionBounds = new Bounds[outConnectionsStart];
                for (int i = 0; i < a.connectionBounds.Length; i++) {
                    a.connectionBounds[i] = new Bounds();
                }
                mapFloodingAlgorithm.Search(StartNode, a);
                this.boundsInformationContainer = a.connectionBounds;
                Debug.Log(a.connectionBounds.Length);

            }
            if (Input.GetKeyDown(KeyCode.J)) {
                draw = true;
                var StartNode = navMesh.QuantizeToNode(this.endPosition, 1.0f);
                //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
                if (StartNode == null) return;

                var a = new NodeGoalBounds();
                var outConnectionsStart = StartNode.OutEdgeCount;
                a.connectionBounds = new Bounds[outConnectionsStart];
                for (int i = 0; i < a.connectionBounds.Length; i++) {
                    a.connectionBounds[i] = new Bounds();
                }
                mapFloodingAlgorithm.Search(StartNode, a);

                this.boundsInformationContainer = a.connectionBounds;
                Debug.Log(a.connectionBounds.Length);
            }
        }



        public void OnGUI() {

            var activePathFinding = PathFinding.AlgorithmName;
            guiStyle.normal.textColor = Color.blue;
            guiStyle.fontSize = 30;
            GUI.Label(new Rect(10, 10, 300, 20), activePathFinding, guiStyle);


            var alwaysOnText = "Normal A* -> " + NormalAStarKeyStart.ToString()
                                    + "\nNodeArray -> " + NodeArrayKeyStart.ToString()
                                    + "\nGoalBound -> " + GoalBoundKeyStart.ToString()
                                    + "\n\nUsage: "
                                    + "\n  1st Select Algorithm"
                                    + "\n  2nd Choose The points (1 - 6)"
                                    + "\n\n\nDraw All Nodes -> " + drawNavMeshKey.ToString()
                                    + "\nStep By Step: " + debugStepByStep + " (" + toggleDebugStepByStepKey + " to toggle)"
                                    + "\nNext Step: " + advanceToNextFrameKey
                                    + "\nSteps Done: " + numberOfSteps
                                    + "\nNodes Per Frame (+"+IncreaseNodesPerFrameKey + " *5" + Increase5TimesNodesPerFrameKey+"),(-"+DecreaseNodesPerFrameKey+" *5"+ Decrease5TimesNodesPerFrameKey+")"
                                    + "\nTotalFrames" + frameCount
                                    + "\n\nBounds only:"
                                    + "\n  Bounds Limits: "  +drawBoundsKey           + " : " + drawBounds
                                    + "\n  DirectChildren: " +drawDirectChildrenKey   + " : " + drawDirectChildren
                                    + "\n  All nodes: "      +drawAllNodesOfBoundsKey + " : " + drawAllNodesOfBounds
                                    + "\n  Live Calculation: "+liveCalculationKey     + " : " + liveCalculation
                                    
                ;
            guiStyle.normal.textColor = Color.black;
            guiStyle.fontSize = 20;





            var rightSideText = "TotalMeshNodes:" + navMesh.Size + " : " + realSize
                            + "\nVisitedNodes: " + this.PathFinding.TotalExploredNodes + " (" + (((this.PathFinding.TotalExploredNodes * 1.0f) / realSize) * 100) + "%)"
                            + "\nVisited + Open: " + (this.PathFinding.TotalExploredNodes + this.PathFinding.Open.All().Count).ToString() + " (" + ((((this.PathFinding.TotalExploredNodes + this.PathFinding.Open.All().Count) * 1.0f) / realSize) * 100) + "%)"
                            + "\n";

            if (this.currentSolution != null) {
                var time = this.PathFinding.TotalProcessingTime * 1000;
                float timePerNode;
                if (this.PathFinding.TotalExploredNodes > 0) {
                    timePerNode = time / this.PathFinding.TotalExploredNodes;
                } else {
                    timePerNode = 0;
                }


                rightSideText += "\n\nNodesPerFrame: " + NodesPerFrame
                           + "\nMaximum Open Size: " + this.PathFinding.MaxOpenNodes
                           + "\nCurrent Open Size: " + this.PathFinding.Open.All().Count 
                           + "\nProcessing time (ms): " + time.ToString("F")
                           + "\nReal Processing time (ms): " + (PathFinding.PureTotalTime * 1000).ToString("F")
                           + "\nTime per Node (ms):" + timePerNode.ToString("F4")
                           
                           ;

                //guiStyle.normal.textColor = Color.black;
                //guiStyle.fontSize = 20;
                //GUI.Label(new Rect(10, 440, 300, 200), text, guiStyle);
            }
            GUI.Label(new Rect(10, 40, 300, 250), alwaysOnText, guiStyle);

           


            var goalBoundPathFinding = this.PathFinding as GoalBoundingPathfinding;
            if(goalBoundPathFinding != null) {
                rightSideText += "\n\nGoulBounding stats\n  Considered Edges: " + goalBoundingPathfinding.TotalEdges
                                   + "\n  Visited Edges: " + (goalBoundingPathfinding.TotalEdges - goalBoundingPathfinding.DiscardedEdges)
                                   + "\n  Discarded Edges: " + goalBoundingPathfinding.DiscardedEdges + " ( " + Mathf.Floor((goalBoundingPathfinding.DiscardedEdges * 1.0f) / (goalBoundingPathfinding.TotalEdges * 1.0f) * 100) + "% )"
                                   + "\n  NullNodes: " + goalBoundingPathfinding.NullTableNodesCount;
            }
            GUI.Label(new Rect(Screen.currentResolution.width- 380, 40, 400, 250), rightSideText, guiStyle);
        }

        public void OnDrawGizmos() {
            frameCount++;
            if (this.draw) {
                //draw the current Solution Path if any (for debug purposes)
                if (this.currentSolution != null) {
                    var colorLine = Color.red;
                    var previousPosition = this.startPosition;
                    foreach (var pathPosition in this.currentSolution.PathPositions) {
                        Debug.DrawLine(previousPosition, pathPosition, colorLine);
                        previousPosition = pathPosition;
                    }


                    if (this.currentSolution.Smoothed) {
                        GlobalPath pathToPrint = smoothedSolution;
                        colorLine = Color.yellow;
                        if (frameCount % 10 == 0) {
                            solutionIndex++;
                            if (solutionIndex == pathToPrint.PathPositions.Count) {
                                solutionIndex = 0;
                            }
                        }
                        Gizmos.DrawSphere(pathToPrint.PathPositions[solutionIndex], 10.0f);

                        previousPosition = this.startPosition;
                        foreach (var pathPosition in this.smoothedSolution.PathPositions) {
                            Debug.DrawLine(previousPosition, pathPosition, colorLine);
                            previousPosition = pathPosition;
                        }
                    }
                }
                //draw the nodes in Open and Closed Sets
                if (this.PathFinding != null && this.currentSolution != null) {
                    Gizmos.color = Color.magenta;
                    if (this.PathFinding.Open != null) {
                        foreach (var nodeRecord in this.PathFinding.Open.All()) {
                            //if(!(nodeRecord.node is NavMeshEdge)) {
                            //    Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 2.5f);
                            //} else {
                                Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 2.5f);
                            //}
                        }
                    }
                    Gizmos.color = Color.blue;
                    if (this.PathFinding.Closed != null) {
                        foreach (var nodeRecord in this.PathFinding.Closed.All()) {
                            //if (!(nodeRecord.node is NavMeshEdge)) {
                            //    Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 2.5f);
                            //} else {
                                Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 2.5f);
                            //}
                        }
                    }
                }
            }

            var boundBoxAlgorithm = PathFinding as GoalBoundingPathfinding;
            if(boundBoxAlgorithm != null) {

                mapFloodingAlgorithm = boundBoxAlgorithm.digjstra;
                if(boundBoxAlgorithm.toPrintNodeGoulBounds != null) {
                    this.boundsInformationContainer = boundBoxAlgorithm.toPrintNodeGoulBounds.connectionBounds;
                }

            }

            if (this.boundsInformationContainer != null && mapFloodingAlgorithm != null && mapFloodingAlgorithm.Closed != null) {
                var colorrrr = Color.black;
                if (drawAllNodesOfBounds) {
                    foreach (var node in mapFloodingAlgorithm.Closed.All()) {
                        var boundIndex = node.StartNodeOutConnectionIndex;
                        if (boundIndex >= c.Length) {
                            colorrrr = Color.black;
                        } else {
                            colorrrr = c[boundIndex];
                        }
                        Gizmos.color = colorrrr;
                        Gizmos.DrawSphere(node.node.LocalPosition, 1f);
                    }
                }
                if (drawDirectChildren) {
                    for (int i = 0; i < mapFloodingAlgorithm.StartNode.OutEdgeCount; i++) {
                        var rainNode = mapFloodingAlgorithm.StartNode.EdgeOut(i).ToNode;

                        var nodeToPrint = mapFloodingAlgorithm.NodeRecordArray.GetNodeRecord(rainNode);
                        var boundIndex = nodeToPrint.StartNodeOutConnectionIndex;
                        if (boundIndex >= c.Length) {
                            colorrrr = Color.black;
                        } else {
                            colorrrr = c[boundIndex];
                        }
                        Gizmos.color = colorrrr;
                        Gizmos.DrawSphere(nodeToPrint.node.LocalPosition, 2f);
                    }
                }
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(mapFloodingAlgorithm.NodeRecordArray.GetNodeRecord(mapFloodingAlgorithm.StartNode).node.LocalPosition, 0.5f);


                var nrOfPoints = 0;
                var index = 0;
                colorrrr = Color.black;
                string boxes = "";
                if (this.drawBounds) {
                    foreach (var bound in boundsInformationContainer) {
                        if (bound == null)
                        {
                            Debug.Log("NULL bound" + index);
                        }
                        if (index >= c.Length) {
                            colorrrr = Color.black;
                        } else {
                            colorrrr = c[index];
                        }
                        boxes += "\n" + index + " : " + bound.minx + " -> " + bound.maxx + " _z_ " + +bound.minz + " -> " + bound.maxz;
                        if (bound.minx == bound.maxx && bound.minz == bound.maxz) {
                            nrOfPoints++;
                            Gizmos.color = Color.cyan;
                            Gizmos.DrawSphere(new Vector3(bound.minx, 0, bound.minz), 0.5f);
                        } else {
                            Debug.DrawLine(new Vector3(bound.minx, 0, bound.minz), new Vector3(bound.minx, 0, bound.maxz), colorrrr);
                            Debug.DrawLine(new Vector3(bound.minx, 0, bound.minz), new Vector3(bound.maxx, 0, bound.minz), colorrrr);
                            Debug.DrawLine(new Vector3(bound.minx, 0, bound.maxz), new Vector3(bound.maxx, 0, bound.maxz), colorrrr);
                            Debug.DrawLine(new Vector3(bound.maxx, 0, bound.maxz), new Vector3(bound.maxx, 0, bound.minz), colorrrr);
                        }

                        index++;
                    }
                }
                //Debug.Log(boxes);
                //Debug.Log("nrOfPoints: " + nrOfPoints);

            }
            if (this.drawNavMesh) {
                var navMesh = this.PathFinding.NavMeshGraph;

                //var lista = (this.PathFinding as GoalBoundingPathfinding).GetNodesHack(navMesh);
                //foreach(var node in lista) { 

                List<int> lista = new List<int>();

                for (int index = 0; index < navMesh.Size; index++) {
                    var node = navMesh.GetNode(index);

                    if (index != node.NodeIndex) {
                        Debug.Log("Index: " + index + " NodeIndex: " + node.NodeIndex);
                    }

                    if (goalBoundTable.table[index] != null || reallyDrawAllNodes) {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(node.LocalPosition, 0.4f);
                    } else if (goalBoundTable.table[index] == null && !(node is NavMeshEdge)) {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(node.LocalPosition, 1.5f);
                    } else if (goalBoundTable.table[index] == null && (node is NavMeshEdge)) {
                        lista.Add(index);

                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(node.LocalPosition, 1.5f);
                    }

                }
            }
        }

        private bool MouseClickPosition(out Vector3 position) {
            RaycastHit hit;

            var ray = this.camera.ScreenPointToRay(Input.mousePosition);
            //test intersection with objects in the scene
            if (Physics.Raycast(ray, out hit)) {
                //if there is a collision, we will get the collision point
                position = hit.point;
                return true;
            }

            position = Vector3.zero;
            //if not the point is not valid
            return false;
        }

        private void InitializePathFinding(Vector3 p1, Vector3 p2) {
            //If the algorithm changed we want to removed the node from the mesh
            this.PathFinding.CleanUp();

            //show the start sphere, hide the end one
            //this is just a small adjustment to better see the debug sphere
            this.startDebugSphere.transform.position = p1 + Vector3.up;
            this.startDebugSphere.SetActive(true);
            this.endDebugSphere.transform.position = p2 + Vector3.up;
            this.endDebugSphere.SetActive(true);
            this.currentClickNumber = 1;
            this.startPosition = p1;
            this.endPosition = p2;

            this.currentSolution = null;
            this.draw = true;
            numberOfSteps = 0;
            this.PathFinding.LiveCalculation = liveCalculation;
            this.PathFinding.InitializePathfindingSearch(this.startPosition, this.endPosition);
        }
    }
}
