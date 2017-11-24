using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;

namespace Assets.Scripts.DecisionMakingActions
{
    public class SwordAttack : WalkToTargetAndExecuteAction
    {
        private int hpChange;
        private int xpChange;

        public SwordAttack(AutonomousCharacter character, GameObject target) : base("SwordAttack",character,target)
        {
            xmlName = "Sword " + target.name;


            if (target.tag.Equals("Skeleton"))
            {
                this.hpChange = -5;
                this.xpChange = 5;
            }
            else if (target.tag.Equals("Orc"))
            {
                this.hpChange = -10;
                this.xpChange = 10;
            }
            else if (target.tag.Equals("Dragon"))
            {
                this.hpChange = -20;
                this.xpChange = 20;
            }
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += -this.hpChange;
            }
            else if (goal.Name == AutonomousCharacter.GAIN_XP_GOAL)
            {
                change += -this.xpChange;
            }
            
            return change;
        }


        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.SwordAttack(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var xpValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_XP_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_XP_GOAL,xpValue-this.xpChange); 

            var surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL,surviveValue-this.hpChange);

            var hp = (int)worldModel.GetProperty(Properties.HP);
            worldModel.SetProperty(Properties.HP,hp + this.hpChange);
            var xp = (int)worldModel.GetProperty(Properties.XP);
            worldModel.SetProperty(Properties.XP, xp + this.xpChange);
           

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name,false);
            
        }

        public override void ApplyActionEffects(NewWorldModel worldModel) {
            base.ApplyActionEffects(worldModel);

            //var xpValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_XP_GOAL);
            //worldModel.SetGoalValue(AutonomousCharacter.GAIN_XP_GOAL, xpValue - this.xpChange);

            //var surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            //worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue - this.hpChange);

            var hp = worldModel.stats.getStat(Stats.hp);
            //var hp = (int)worldModel.GetProperty(Properties.HP);
            worldModel.stats.setStat(Stats.hp,worldModel.stats.getStat(Stats.hp) + this.hpChange);
            //worldModel.SetProperty(Properties.HP, hp + this.hpChange);
            //var xp = (int)worldModel.GetProperty(Properties.XP);
            var xp = worldModel.stats.getStat(Stats.xp);
            worldModel.stats.setStat(Stats.xp, worldModel.stats.getStat(Stats.xp) + this.xpChange);
            //worldModel.SetProperty(Properties.XP, xp + this.xpChange);


            //disables the target object so that it can't be reused again
            string s = Target.name;
            int index;
            if (s.StartsWith("Dragon")) {
                s = s.Substring(6);
                index = int.Parse(s);
                worldModel.dragons[index - 1] = false;
            } else if (s.StartsWith("Skeleton")) {
                s = s.Substring(8);
                index = int.Parse(s);
                worldModel.skeletons[index - 1] = false;
            } else if (s.StartsWith("Orc")) {
                s = s.Substring(3);
                index = int.Parse(s);
                worldModel.orcs[index - 1] = false;
            } else {
                Debug.Log("Not valid target");  
            }
            

        }

    }
}
