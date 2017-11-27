using System;
using System.Collections.Generic;
using Assets.Scripts.DecisionMakingActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using RAIN.Navigation;
using RAIN.Navigation.NavMesh;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.GOB.Action;
using System.Linq;

namespace Assets.Scripts
{
    public class AutonomousCharacter : MonoBehaviour
    {
        //constants
        public const string SURVIVE_GOAL = "Survive";
        public const string GAIN_XP_GOAL = "GainXP";
        public const string BE_QUICK_GOAL = "BeQuick";
        public const string GET_RICH_GOAL = "GetRich";

        public const float DECISION_MAKING_INTERVAL = 20.0f;
        //public fields to be set in Unity Editor
        public GameManager.GameManager GameManager;
        public Text SurviveGoalText;
        public Text GainXPGoalText;
        public Text BeQuickGoalText;
        public Text GetRichGoalText;
        public Text TotalProcessingTimeText;
        public Text BestDiscontentmentText;
        public Text ProcessedActionsText;
        public Text BestActionText;
        public bool MCTSActive;


        public Goal BeQuickGoal { get; private set; }
        public Goal SurviveGoal { get; private set; }
        public Goal GetRichGoal { get; private set; }
        public Goal GainXPGoal { get; private set; }
        public List<Goal> Goals { get; set; }
        public List<Action> Actions { get; set; }
        public Action CurrentAction { get; private set; }
        public DynamicCharacter Character { get; private set; }
        public DecisionMakingBase GOAPDecisionMaking { get; set; }

        public AStarPathfinding AStarPathFinding;
        private PathSmoothing pathSmoothing = new PathSmoothing();
        private GoalBoundingTable goalBoundTable;

        //private fields for internal use only
        private Vector3 startPosition;
        private GlobalPath currentSolution;
        private GlobalPath currentSmoothedSolution;
        private NavMeshPathGraph navMesh;
        
        private bool draw;
        private float nextUpdateTime = 0.0f;
        private float previousGold = 0.0f;
        private int previousXP = 0;

        public Vector3 PreviousTargetPosition { get; set; }
        public string PreviousTargetName { get; set; }

		private Animator characterAnimator;
        private bool alreadyCalculatedNextReconsider;

        private NewCurrentStateWorldModel currentNewWorldModelDebug;
        private NewCurrentStateWorldModel newWorldModel;
        private NewWorldModel debugModel2;
        private float TotalTime;

        public void Initialize(NavMeshPathGraph navMeshGraph, AStarPathfinding pathfindingAlgorithm)
        {
            this.draw = true;
            this.navMesh = navMeshGraph;
            this.AStarPathFinding = pathfindingAlgorithm;
            this.AStarPathFinding.NodesPerFrame = 100;

			this.characterAnimator = this.GetComponentInChildren<Animator> ();
        }

        public void Start() {


            this.draw = true;

            this.navMesh = NavigationManager.Instance.NavMeshGraphs[0];
            this.Character = new DynamicCharacter(this.gameObject);

            //initialize your pathfinding algorithm here!
            //use goalBoundingPathfinding for a more efficient algorithm
            goalBoundTable = ScriptableObject.CreateInstance<GoalBoundingTable>();
            goalBoundTable.LoadOptimized();
            GoalBoundingPathfinding goalBoundingPathfinding = new GoalBoundingPathfinding(NavigationManager.Instance.NavMeshGraphs[0], new EuclidianHeuristic(), goalBoundTable);
            this.Initialize(NavigationManager.Instance.NavMeshGraphs[0], goalBoundingPathfinding);

            //initialization of the GOB decision making
            //let's start by creating 4 main goals

            this.SurviveGoal = new Goal(SURVIVE_GOAL, 2.0f);

            this.GainXPGoal = new Goal(GAIN_XP_GOAL, 5.0f) {
                InsistenceValue = 5.0f,
                ChangeRate = 0.5f
            };

            this.GetRichGoal = new Goal(GET_RICH_GOAL, 1.0f) {
                InsistenceValue = 5.0f,
                ChangeRate = 0.2f
            };

            this.BeQuickGoal = new Goal(BE_QUICK_GOAL, 1.0f) {
                ChangeRate = 0.1f
            };

            this.Goals = new List<Goal>();
            this.Goals.Add(this.SurviveGoal);
            this.Goals.Add(this.BeQuickGoal);
            this.Goals.Add(this.GetRichGoal);
            this.Goals.Add(this.GainXPGoal);

            //initialize the available actions

            this.Actions = new List<Action>();


            var Skeletons = GameObject.FindGameObjectsWithTag("Skeleton");
            var Orcs = GameObject.FindGameObjectsWithTag("Orc");
            var Dragons = GameObject.FindGameObjectsWithTag("Dragon");
            var enemies = (Skeletons.Concat(Orcs).Concat(Dragons)).ToArray();

            Dictionary<string, List<Pair<int,int>>> chestInfo = new Dictionary<string, List<Pair<int, int>>>();
            Dictionary<string, List<int>> enemiesRewards = new Dictionary<string, List<int>>();

            foreach (var enemy in enemies.ToArray()) {
                enemiesRewards[enemy.name] = new List<int>();
            }



            foreach (var chest in GameObject.FindGameObjectsWithTag("Chest")) {
                this.Actions.Add(new PickUpChest(this, chest));

                //MCTSBIASED STUFF
                List<Pair<int, int>> chestEnemies = new List<Pair<int, int>>(); 
                var chestPosition = chest.transform.position;

                var chest_name = chest.name;
                chest_name = chest_name.Substring(5);
                int indexInListChest = int.Parse(chest_name) - 1;

                foreach (var enemy in enemies.ToArray()) {
                    
                    if ((enemy.transform.position - chestPosition).sqrMagnitude <= 400) {

                        
                        var enemy_name = enemy.name;
                        int index_in_list_enemy = 0;
                        int type_of_enemy = 0;
                        if (enemy.CompareTag("Dragon")) {
                            enemy_name = enemy_name.Substring(6);
                            index_in_list_enemy = int.Parse(enemy_name);
                            type_of_enemy = 1;
                        } else if (enemy.CompareTag("Skeleton")) {
                            enemy_name = enemy_name.Substring(8);
                            index_in_list_enemy = int.Parse(enemy_name);
                            type_of_enemy = 2;
                        } else if (enemy.CompareTag("Orc")) {
                            enemy_name = enemy_name.Substring(3);
                            index_in_list_enemy = int.Parse(enemy_name);
                            type_of_enemy = 3;
                        } else {
                            continue;
                        }
                        chestEnemies.Add(new Pair<int, int>(type_of_enemy, index_in_list_enemy - 1));
                        enemiesRewards[enemy.name].Add(indexInListChest);
                    }
                }
                chestInfo.Add(chest.name, chestEnemies);
            }

            foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion")) {
                this.Actions.Add(new GetManaPotion(this, potion));
            }

            foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion")) {
                this.Actions.Add(new GetHealthPotion(this, potion));
            }

            foreach (var enemy in Skeletons) {
                this.Actions.Add(new SwordAttack(this, enemy));
                this.Actions.Add(new Fireball(this, enemy));
            }

            foreach (var enemy in Orcs) {
                this.Actions.Add(new SwordAttack(this, enemy));
                this.Actions.Add(new Fireball(this, enemy));
            }

            foreach (var enemy in Dragons) {
                this.Actions.Add(new SwordAttack(this, enemy));
                this.Actions.Add(new Fireball(this, enemy));
            }
            this.Actions.Add(new LevelUp(this));
            var worldModel = new CurrentStateWorldModel(this.GameManager, this.Actions, this.Goals);


            //currentNewWorldModelDebug = new NewCurrentStateWorldModel(this.GameManager, this.Actions);


            newWorldModel = new NewCurrentStateWorldModel(this.GameManager, this.Actions, chestInfo, enemiesRewards);
            //debugModel2 = currentNewWorldModelDebug.GenerateChildWorldModel();

            //new SwordAttack(this, GameObject.FindGameObjectsWithTag("Orc")[0]).ApplyActionEffects(debugModel2);
            PreviousTargetName = "";

            //this.GOAPDecisionMaking = new MCTSBiasedPlayout(worldModel);
            //this.GOAPDecisionMaking = new MCTSBiasedPlayout(newWorldModel);
            
            //this.GOAPDecisionMaking = new MCTS(worldModel);
            //this.GOAPDecisionMaking = new NewMCTS(newWorldModel);
            this.GOAPDecisionMaking = new NewMCTSBiasedPlayout(newWorldModel, chestInfo, enemiesRewards);
            //this.GOAPDecisionMaking = new DepthLimitedGOAPDecisionMaking(worldModel, this.Actions, this.Goals);
            //this.GOAPDecisionMaking.InProgress = false;

        }
        

        void Update()
        {
            //var bgTime = Time.realtimeSinceStartup;
           
            if (Time.time > this.nextUpdateTime || this.GameManager.WorldChanged)
            {
                //Debug.Log(this.TotalTime);

                if (this.GameManager.characterData.HP <= 0 || this.GameManager.characterData.Time >= 200) {
                    Debug.Log("Lost");
                    Debug.Log(this.GameManager.characterData.Time);
                    Debug.Log("Total: " + this.GOAPDecisionMaking.TotalProcessingTime);
                    var mctsLog = this.GOAPDecisionMaking as NewMCTS;
                    if (mctsLog != null) {
                        Debug.Log("ActionsCut: " + mctsLog.ActionsCut);
                    }
                    this.GameManager.WorldChanged = false;
                    this.nextUpdateTime = float.MaxValue;
                } else if (this.GameManager.characterData.Money == 25) {
                    Debug.Log("Victory");
                    Debug.Log(this.GameManager.characterData.Time);
                    Debug.Log("Total: " + this.GOAPDecisionMaking.TotalProcessingTime);
                    var mctsLog = this.GOAPDecisionMaking as NewMCTS;
                    if (mctsLog != null) {
                        Debug.Log("ActionsCut: " + mctsLog.ActionsCut);
                    }
                    this.GameManager.WorldChanged = false;
                    this.nextUpdateTime = float.MaxValue;
                }
                Debug.Log("Reconsidering");


                //currentNewWorldModelDebug.UpdateCurrentStateWorldModel();
                
                
                newWorldModel.UpdateCurrentStateWorldModel();
                
                //Debug.Log(debugModel2.toString());
                //Debug.Log(debugModel2.toString());
                //new SwordAttack(this, GameObject.FindGameObjectWithTag("Skeleton")).ApplyActionEffects(debugModel2);
                //new PickUpChest(this, GameObject.FindGameObjectWithTag("Chest")).ApplyActionEffects(debugModel2);
                //new GetManaPotion(this, GameObject.FindGameObjectWithTag("ManaPotion")).ApplyActionEffects(debugModel2);
                //debugModel.GetNextAction();
                //Debug.Log(debugModel.toString());

                this.GameManager.WorldChanged = false;
                //this.nextUpdateTime = Time.time + DECISION_MAKING_INTERVAL;

                //first step, perceptions
                //update the agent's goals based on the state of the world
                this.SurviveGoal.InsistenceValue = this.GameManager.characterData.MaxHP - this.GameManager.characterData.HP;

                this.BeQuickGoal.InsistenceValue += DECISION_MAKING_INTERVAL * this.BeQuickGoal.ChangeRate;
                if(this.BeQuickGoal.InsistenceValue > 10.0f)
                {
                    this.BeQuickGoal.InsistenceValue = 10.0f;
                }

                this.GainXPGoal.InsistenceValue += this.GainXPGoal.ChangeRate; //increase in goal over time
                if(this.GameManager.characterData.XP > this.previousXP)
                {
                    this.GainXPGoal.InsistenceValue -= this.GameManager.characterData.XP - this.previousXP;
                    this.previousXP = this.GameManager.characterData.XP;
                }

                this.GetRichGoal.InsistenceValue += this.GetRichGoal.ChangeRate; //increase in goal over time
                if (this.GetRichGoal.InsistenceValue > 10)
                {
                    this.GetRichGoal.InsistenceValue = 10.0f;
                }

                if (this.GameManager.characterData.Money > this.previousGold)
                {
                    this.GetRichGoal.InsistenceValue -= this.GameManager.characterData.Money - this.previousGold;
                    this.previousGold = this.GameManager.characterData.Money;
                }

                this.SurviveGoalText.text = "Survive: " + this.SurviveGoal.InsistenceValue;
                this.GainXPGoalText.text = "Gain XP: " + this.GainXPGoal.InsistenceValue.ToString("F1");
                this.BeQuickGoalText.text = "Be Quick: " + this.BeQuickGoal.InsistenceValue.ToString("F1");
                this.GetRichGoalText.text = "GetRich: " + this.GetRichGoal.InsistenceValue.ToString("F1");

                //initialize Decision Making Proccess
                this.CurrentAction = null;
                this.GOAPDecisionMaking.InitializeDecisionMakingProcess();
                PreviousTargetPosition = new Vector3(Single.NaN, Single.NaN, Single.NaN);
            }

            
            this.UpdateDLGOAP();

            if (this.CurrentAction == null)
            {
                //Debug.Log("Action is null");
            }
            if(this.CurrentAction != null)
            {
                if(this.CurrentAction.CanExecute())
                {
                    this.CurrentAction.Execute();
                }
            }

            //call the pathfinding method if the user specified a new goal
            if (this.AStarPathFinding.InProgress)
            {
                Debug.Log("AStar In Progress");
                var finished = this.AStarPathFinding.Search(out this.currentSolution);
                if (finished && this.currentSolution != null)
                {
                    //lets smooth out the Path
                    this.startPosition = this.Character.KinematicData.position;
                    //this.currentSmoothedSolution = pathSmoothing.Smooth(currentSolution);
                    //To have a smoother path
                    //this.currentSmoothedSolution = pathSmoothing.Smooth(currentSmoothedSolution);
                    this.currentSmoothedSolution = this.currentSolution;

                    var smoother = new PathSmoothing();
                    this.currentSmoothedSolution = smoother.Smooth(this.currentSolution);
                    this.currentSmoothedSolution.CalculateLocalPathsFromPathPositions(this.Character.KinematicData.position);
                    var maxSpeed = 50.0f;
					this.Character.Movement = new DynamicFollowPath(this.Character.KinematicData, this.currentSmoothedSolution)
                    {
                        MaxAcceleration = 200.0f,
                        MaxSpeed = maxSpeed
                    };
                    this.Character.MaxSpeed = maxSpeed;
                }
            }


            this.Character.Update();
			//manage the character's animation
			if (this.Character.KinematicData.velocity.sqrMagnitude > 0.1) 
			{
				this.characterAnimator.SetBool ("Walking", true);
			} 
			else 
			{
				this.characterAnimator.SetBool ("Walking", false);
			}

            //var endtime = Time.realtimeSinceStartup;
            //this.TotalTime += endtime - bgTime;
        }

        

        private void UpdateDLGOAP()
        {
            if (this.GOAPDecisionMaking.InProgress)
            {
                //Debug.Log("GOB in progress");
                //choose an action using the GOB Decision Making process
                var action = this.GOAPDecisionMaking.ChooseAction();
                if (action != null)
                {
                    //Debug.Log("GOAP action NOT null");
                    this.CurrentAction = action;
                }
                else
                {
                    //Debug.Log("GOAP action is null");

                }
                //alreadyCalculatedNextReconsider = false;
                this.nextUpdateTime = Time.time + DECISION_MAKING_INTERVAL;
            } else {
                this.TotalProcessingTimeText.text = "Total: " + this.GOAPDecisionMaking.TotalProcessingTime.ToString("F")
                                              + " Partial: " + this.GOAPDecisionMaking.ParcialProcessingTime.ToString("F");

                var mcts = this.GOAPDecisionMaking as MCTS;
                if (mcts != null) {
                    //this.TotalProcessingTimeText.text += "\nActions Cut: " + mcts.PlayoutNodes;// + ": "+ mcts.TotalPlayoutNodes ;
                } else {
                    var newmcts = this.GOAPDecisionMaking as NewMCTS;
                    this.TotalProcessingTimeText.text += "\nActions Cut: " + newmcts.ActionsCut;// + ": " + newmcts.TotalPlayoutNodes;
                }
                //this.BestDiscontentmentText.text = "Best Discontentment: " + this.GOAPDecisionMaking.BestDiscontentmentValue.ToString("F");
                //this.ProcessedActionsText.text = "Act. comb. processed: " + this.GOAPDecisionMaking.TotalActionCombinationsProcessed;

                if (this.GOAPDecisionMaking.BestAction != null) {
                    var actionText = "";
                    foreach (var action in this.GOAPDecisionMaking.BestActionSequence) {
                        actionText += "\n" + action.Name;
                    }
                    this.BestActionText.text = "Best Action Sequence: " + actionText;
                } else {
                    this.BestActionText.text = "Best Action Sequence:\nNone";
                }
            }



            
        }

        public void StartPathfinding(GameObject target)
        {
            //if the targetPosition received is the same as a previous target, then this a request for the same target
            //no need to redo the pathfinding search
            var targetPosition = target.transform.position;
            if (!this.PreviousTargetPosition.Equals(targetPosition) || this.GameManager.WorldChanged)
            {
                this.AStarPathFinding.InitializePathfindingSearch(this.Character.KinematicData.position, targetPosition);
                    PreviousTargetPosition = targetPosition;
                PreviousTargetName = target.name;
            }
        }


        public void OnDrawGizmos()
		{
			if (this.draw)
			{
				//draw the current Solution Path if any (for debug purposes)
				if (this.currentSolution != null)
				{
					var previousPosition = this.startPosition;
					foreach (var pathPosition in this.currentSolution.PathPositions)
					{
						Debug.DrawLine(previousPosition, pathPosition, Color.red);
						previousPosition = pathPosition;
					}

					previousPosition = this.startPosition;
					foreach (var pathPosition in this.currentSmoothedSolution.PathPositions)
					{
						Debug.DrawLine(previousPosition, pathPosition, Color.green);
						previousPosition = pathPosition;
					}
                    var walk = this.CurrentAction as WalkToTargetAndExecuteAction;
                    if (walk != null) {
                        Gizmos.color = Color.black;
                        // FIXME: AMARAL isto rebenta
                     //   Gizmos.DrawWireSphere(walk.Target.transform.position, 3.0F);
                    }
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(this.AStarPathFinding.GoalPosition, 3.0F);
                    
				}
			}
		}
    }
}
