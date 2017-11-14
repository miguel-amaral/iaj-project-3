using System;
using Assets.Scripts.GameManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 4;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public CurrentStateWorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] ActionPerLevel { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public double Delta = 1;


        public DepthLimitedGOAPDecisionMaking(CurrentStateWorldModel currentStateWorldModel, List<Action> actions, List<Goal> goals)
        {
            this.ActionCombinationsProcessedPerFrame = 200;
            this.Goals = goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.ActionPerLevel = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
			
			var processedActions = 0;

			var startTime = Time.realtimeSinceStartup;
            
            while (CurrentDepth >= 0)
            {
                if (CurrentDepth >= MAX_DEPTH)
                {
                    var currentValue = Models[CurrentDepth].CalculateDiscontentment(this.Goals);
                    if (currentValue < BestDiscontentmentValue - Delta )
                    {
                        this.BestDiscontentmentValue = currentValue;
                        this.BestAction = ActionPerLevel[0]; //BestActionSequence[0];
                        this.BestActionSequence = this.ActionPerLevel.ToArray();
                    }else if (Math.Abs(BestDiscontentmentValue - currentValue) < Delta)
                    {
                        var tempModel = Models[0].GenerateChildWorldModel();
                        this.BestActionSequence[0].ApplyActionEffects(tempModel);

                        var tempModel2 = Models[0].GenerateChildWorldModel();
                        this.ActionPerLevel[0].ApplyActionEffects(tempModel2);

                        var discont1 = tempModel.CalculateDiscontentment(this.Goals);
                        var discont2 = tempModel2.CalculateDiscontentment(this.Goals);
                        if (discont2 < discont1)
                        {
                            this.BestDiscontentmentValue = currentValue;
                            this.BestAction = ActionPerLevel[0]; //BestActionSequence[0];
                            this.BestActionSequence = this.ActionPerLevel.ToArray();
                        }
                    }
                    CurrentDepth -= 1;
                    continue;
                }
                var nextAction = Models[CurrentDepth].GetNextAction();
                if (nextAction != null)
                {
                    Models[CurrentDepth + 1] = Models[CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(Models[CurrentDepth + 1]);
                    ActionPerLevel[CurrentDepth] = nextAction;
                    //BestActionSequence[CurrentDepth] = nextAction;
                    CurrentDepth += 1;
                }
                else
                {
                    CurrentDepth -= 1;
                }
            }
			

			this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
			this.InProgress = false;
			return this.BestAction;
        }

    }
}
