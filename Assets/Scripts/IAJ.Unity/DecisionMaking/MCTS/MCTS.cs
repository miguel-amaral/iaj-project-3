using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<GOB.Action> BestActionSequence { get; private set; }


        private int CurrentIterations { get; set; }
        private int CurrentIterationsInFrame { get; set; }
        private int CurrentDepth { get; set; }

        private CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        private MCTSNode InitialNode { get; set; }
        private System.Random RandomGenerator { get; set; }
        
        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 100;
            this.MaxIterationsProcessedPerFrame = 10;
            this.RandomGenerator = new System.Random();
        }


        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<GOB.Action>();
        }

        public GOB.Action Run()
        {
            MCTSNode selectedNode;
            Reward reward;

            var startTime = Time.realtimeSinceStartup;
            this.CurrentIterationsInFrame = 0;
            MCTSNode rootNode =  new MCTSNode(CurrentStateWorldModel);

            while (CurrentIterationsInFrame < MaxIterationsProcessedPerFrame && this.CurrentIterations < this.MaxIterations) {
                selectedNode = Selection(rootNode);
                reward = Playout(selectedNode.State);
                Backpropagate(selectedNode, reward);
                CurrentIterations++;
                CurrentIterationsInFrame++;
            }

            return BestUCTChild(rootNode).Action;
        }

        private MCTSNode Selection(MCTSNode initialNode)
        {
            GOB.Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild;

            while (!currentNode.State.IsTerminal()) {
                int count = currentNode.ChildNodes.Count;
                if (count < currentNode.N) {
                    // if(currentNode.ChildNodes.Count == 0)
                    //nextAction = currentNode.ChildNodes[RandomGenerator.Next(count)].Action;
                    var all_actions = currentNode.State.GetExecutableActions();//[RandomGenerator.Next(count)]                    
                    var not_tried = new List<GOB.Action>();
                    foreach(var action in all_actions) {
                        bool tried_already = false; 
                        foreach(var child in currentNode.ChildNodes) {
                            if(action.Equals(child.Action)) {
                                tried_already = true;
                                break;
                            }
                        }
                        if(!tried_already) {
                            not_tried.Add(action);
                        }
                    }
                    return Expand(currentNode, not_tried[RandomGenerator.Next(not_tried.Count)]);
                } else {
                    bestChild = BestChild(currentNode);
                }
            }

            return currentNode;
        }

        private Reward Playout(WorldModel initialPlayoutState) {
            while (!initialPlayoutState.IsTerminal()) {
                int count = initialPlayoutState.GetExecutableActions().Length;
                var action = initialPlayoutState.GetExecutableActions()[RandomGenerator.Next(count)];
                var childModel = new WorldModel(initialPlayoutState);
                action.ApplyActionEffects(childModel);
                initialPlayoutState = childModel;
            }
            return new Reward() {
                PlayerID = initialPlayoutState.GetNextPlayer(),
                Value = initialPlayoutState.GetScore()
            };
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
            MCTSNode child = new MCTSNode(childModel) {
                Parent = parent,
                PlayerID = parent.PlayerID,
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
            //TODO: implement
            throw new NotImplementedException();
        }

        //this method is very similar to the bestUCTChild,
        // but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        private MCTSNode BestChild(MCTSNode node)
        {
            //TODO: implement
            throw new NotImplementedException();
        }
    }
}
