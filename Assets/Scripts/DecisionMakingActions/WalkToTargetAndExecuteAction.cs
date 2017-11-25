using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Text.RegularExpressions;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEditor;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.GOB.Action;

namespace Assets.Scripts.DecisionMakingActions
{
    public abstract class WalkToTargetAndExecuteAction : Action
    {
        private const float RadiusOfDetection = 9f;

        protected AutonomousCharacter Character { get; set; }

        public GameObject Target { get; protected set; }

        protected WalkToTargetAndExecuteAction(string actionName, AutonomousCharacter character, GameObject target) : base(actionName + "(" + target.name + ")")
        {
            this.Character = character;
            this.Target = target;
        }

        public override float GetDuration()
        {
            return this.GetDuration(this.Character.Character.KinematicData.position);
        }

        public override float GetDuration(WorldModel worldModel)
        {
            var position = (Vector3)worldModel.GetProperty(Properties.POSITION);
            return this.GetDuration(position);
        }

        public override float GetDuration(NewWorldModel worldModel) {
            var position = worldModel.stats.getPosition();
            return this.GetDuration(position);
        }

        private float GetDuration(Vector3 currentPosition)
        {
            float distance;
            var heuristic = OfflineTableHeuristic.Instance;

            if (heuristic.GotEntry(Character.PreviousTargetName, Target.gameObject.name))
            {
                distance = heuristic.H(Character.PreviousTargetName, Target.gameObject.name);
            }
            else
            {
                distance = (this.Target.transform.position - this.Character.Character.KinematicData.position).magnitude;
            }
            return distance / this.Character.Character.MaxSpeed;
            //if ((closestObjectName = GetClosestObjectName(this.Character)) != null)
            //{
            //Debug.Log("Using offline heuristic!");
            //try
            //{
            //    distance = OfflineTableHeuristic.Instance.H(Character.PreviousTargetName, Target.gameObject.name);
            //}
            //catch (KeyNotFoundException)
            //{
            //Debug.Log("Ups! Using Ecli! PrevTargetName: " + Character.PreviousTargetName);
            //distance = (this.Target.transform.position - this.Character.Character.KinematicData.position).magnitude;
               // }
       //     }
       //     else
       //     {
			    //distance = (this.Target.transform.position - this.Character.Character.KinematicData.position).magnitude;
       //     }
        }

        
        //private string GetClosestObjectName(AutonomousCharacter charGameObject, float radius = RadiusOfDetection)
        //{

        //    string closest = null;
        //    float minDist = Single.MaxValue;
        //    var ourGameObjects = OfflineTableHeuristic.Instance.OurGameObjects();
        //    foreach (var gameObject in ourGameObjects)
        //    {
        //        float distance;
        //        if ((distance = Vector3.Distance(charGameObject.transform.position, gameObject.Right)) <=
        //            RadiusOfDetection && distance < minDist)
        //        {
        //            minDist = distance;
        //            closest = gameObject.Left;
        //        }
        //    }
        //    return closest;
        //}

        public override float GetGoalChange(Goal goal)
        {
            if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                return this.GetDuration();
            }
            else return 0;
        }

        public override bool CanExecute()
        {
            return this.Target != null;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (this.Target == null) {
                //Debug.Log("xD");
                return false;
            }
            
            
            var targetEnabled = (bool)worldModel.GetProperty(this.Target.name);
            return targetEnabled;
        }

        public override bool CanExecute(NewWorldModel worldModel) {
            if (this.Target == null) {
                //Debug.Log("xD");
                return false;
            }

            string s = Target.name;
            int index;
            //s = s.Remove(Regex.Match(s, "[0-9]").Index);
            if (s.StartsWith("Dragon")) {
                s = s.Substring(6);
                index = int.Parse(s);
                return worldModel.dragons[index - 1];
            }else if (s.StartsWith("Skeleton")) {
                s = s.Substring(8);
                index = int.Parse(s);
                return worldModel.skeletons[index - 1];
            } else if (s.StartsWith("Orc")) {
                s = s.Substring(3);
                index = int.Parse(s);
                return worldModel.orcs[index - 1];
            } else if (s.StartsWith("HealthPotion")) {
                s = s.Substring(12);
                index = int.Parse(s);
                return worldModel.healthPots[index - 1];
            } else if (s.StartsWith("ManaPotion")) {
                s = s.Substring(10);
                index = int.Parse(s);
                return worldModel.manaPots[index - 1];
            } else if (s.StartsWith("Chest")) {
                s = s.Substring(5);
                index = int.Parse(s);
                return worldModel.chests[index - 1];
            }else {
                Debug.Log("Not valid target");
                return false;
            }
                return false;
        }

        public override void Execute()
        {
            this.Character.StartPathfinding(this.Target);
        }


        public override void ApplyActionEffects(WorldModel worldModel)
        {
            var duration = this.GetDuration(worldModel);

            var quicknessValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quicknessValue + duration * 0.1f);

            var time = (float)worldModel.GetProperty(Properties.TIME);
            worldModel.SetProperty(Properties.TIME, time + duration);

            worldModel.SetProperty(Properties.POSITION, Target.transform.position);
        }

        public override void ApplyActionEffects(NewWorldModel worldModel) {
            var duration = this.GetDuration(worldModel);

            //var quicknessValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            //worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quicknessValue + duration * 0.1f);

            var time = worldModel.stats.getTime();
            //var time = (float)worldModel.GetProperty(Properties.TIME);
            worldModel.stats.setTime(worldModel.stats.getTime()+duration);
            //worldModel.SetProperty(Properties.TIME, time + duration);
            worldModel.stats.setPosition(Target.transform.position);
            //worldModel.SetProperty(Properties.POSITION, Target.transform.position);
            
        }


    }
}