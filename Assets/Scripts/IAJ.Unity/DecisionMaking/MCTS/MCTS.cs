using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.GOB.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS : DecisionMakingBase
    {
        public const float C = 1.4f * 250;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }

        public float PlayoutNodes { get; private set; }

        public float ParcialProcessingTime { get; private set; }
        
        public MCTSNode BestFirstChild { get; set; }
        public GOB.Action[] BestActionSequence { get; private set; }
        public GOB.Action BestAction { get; private set; }

        private int CurrentIterations { get; set; }
        private int CurrentIterationsInFrame { get; set; }
        private int CurrentDepth { get; set; }

        private CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        private MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }


        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 10000;
            this.MaxIterationsProcessedPerFrame = 100;
            this.RandomGenerator = new System.Random();
            this.TotalProcessingTime = 0;

            this.PlayoutNodes = 0;
        }


        public void InitializeDecisionMakingProcess()
        {
            foreach( var a in this.CurrentStateWorldModel.GetExecutableActions()) {
                Debug.Log(a);
            }
            

            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel.GenerateChildWorldModel())
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.ParcialProcessingTime = 0;

            // this.BestActionSequence = new List<GOB.Action>();
        }

        public GOB.Action ChooseAction()
        {
            var frameBegin  = Time.realtimeSinceStartup;
            
            MCTSNode selectedNode;
            Reward reward;

            this.CurrentIterationsInFrame = 0;
            MCTSNode rootNode = InitialNode;
            //MCTSNode rootNode =  new MCTSNode(CurrentStateWorldModel.GenerateChildWorldModel());

            while (CurrentIterationsInFrame < MaxIterationsProcessedPerFrame
                && this.CurrentIterations < this.MaxIterations) {
                selectedNode = Selection(rootNode);
                reward = Playout(selectedNode.State);
                Backpropagate(selectedNode, reward);
                CurrentIterations++;
                CurrentIterationsInFrame++;
            }
            var frameEnd = Time.realtimeSinceStartup;
            var thisFrameTime = frameEnd - frameBegin;

            TotalProcessingTime += thisFrameTime;
            ParcialProcessingTime += thisFrameTime;
            if (CurrentIterations >= MaxIterations)
            {
                InProgress = false;
                printXMLTree(rootNode);

                List<GOB.Action> temp = new List<GOB.Action>();
                var currNode = rootNode;
                var bestChild = BestChild(currNode); 
                while (bestChild != null)
                {
                    temp.Add(bestChild.Action);
                    bestChild = BestChild(bestChild);
                }
                this.BestActionSequence = temp.ToArray();
                var toReturn = BestChild(rootNode);
                if (toReturn != null)
                {
                    this.BestAction = toReturn.Action;
                    return BestAction;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        private MCTSNode Selection(MCTSNode initialNode)
        {
            GOB.Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild;

            while (!currentNode.State.IsTerminal())
            {
                nextAction = currentNode.State.GetNextAction();
                if (nextAction != null) {
                    return Expand(currentNode, nextAction);
                } else {
                    
                    if (currentNode.ChildNodes.Count == 0) {
                        //DEBUG CODE
                        string xmlTree = initialNode.ToXML(0);
                        int numero = initialNode.RecursiveNumberOfChilds();
                        System.IO.File.WriteAllText(@"C:\treeXml\tree.xml", xmlTree);
                        Debug.Log("Escrita Arvore");
                        Debug.Log("Arvore nos : " + numero);
                    } else {
                        currentNode = BestUCTChild(currentNode);
                    }
                }
            }

            return currentNode;
        }

        private Reward Playout(WorldModel currentPlayoutState) {
            while (!currentPlayoutState.IsTerminal()) {
                this.PlayoutNodes++;
                var action = GuidedAction(currentPlayoutState);
                if(action == null) {
                    return new Reward {
                        PlayerID = currentPlayoutState.GetNextPlayer(),
                        Value = 0
                    };
                }
                var childModel = currentPlayoutState.GenerateChildWorldModel();
                action.ApplyActionEffects(childModel);
                childModel.CalculateNextPlayer();
                currentPlayoutState = childModel;
            }
            return new Reward
            {
                PlayerID = currentPlayoutState.GetNextPlayer(),
                Value = currentPlayoutState.GetScore()
            };
        }

        protected virtual Action GuidedAction(WorldModel currentPlayoutState)
        {
            var possibleActions = currentPlayoutState.GetExecutableActions();

            var number = RandomGenerator.Next(possibleActions.Length);
            return possibleActions[number];
        }

        private void Backpropagate(MCTSNode node, Reward reward)
        {
            while(node != null) {
                node.N++;
                node.Q += reward.Value;
                node = node.Parent;
            }
        }

        private MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
            WorldModel childModel = parent.State.GenerateChildWorldModel();
            action.ApplyActionEffects(childModel);
            childModel.CalculateNextPlayer();
            MCTSNode child = new MCTSNode(childModel) {
                Parent = parent,
                PlayerID = childModel.GetNextPlayer(),
                Action = action,
                N = 0,
                Q = 0f,
            };
            parent.ChildNodes.Add(child);
            return child;          
        }

        //gets the best child of a node, using the UCT formula
        private MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode best = null;
            var bestUCT = double.MinValue;
            foreach (var nodeChildNode in node.ChildNodes)
            {
                var firstPart = nodeChildNode.Q / nodeChildNode.N;
                var secondPart = C * Math.Sqrt(Math.Log(nodeChildNode.Parent.N) / nodeChildNode.N);
                var sum = firstPart + secondPart;
                if (sum > bestUCT)
                {
                    bestUCT = sum;
                    best = nodeChildNode;
                }
            }
            return best;
        }

        // this method is very similar to the bestUCTChild,
        // but it is used to return the final action of the MCTS search, and so we do not care about
        // the exploration factor
        [CanBeNull]
        private MCTSNode BestChild(MCTSNode node)
        {
            MCTSNode best = null;
            var bestUCT = double.MinValue;
            foreach (var nodeChildNode in node.ChildNodes)
            {
                var firstPart = nodeChildNode.Q / nodeChildNode.N;
                if (bestUCT < firstPart)
                {
                    bestUCT = firstPart;
                    best = nodeChildNode;
                }
            }
            return best;
        }

        private void printXMLTree(MCTSNode initialNode) {
            //Guid uid = Guid.NewGuid();
            string xmlTree = initialNode.ToXML(0);
            int numero = initialNode.RecursiveNumberOfChilds();
            System.IO.File.WriteAllText(@"C:\treeXml\tree.xml", xmlTree);
            //Debug.Log("Escrita Arvore");
            Debug.Log("Arvore nos : " + numero);
        }
    }
}
