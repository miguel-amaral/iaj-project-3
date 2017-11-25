using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using System;

namespace Assets.Scripts.DecisionMakingActions
{
    public class Fireball : WalkToTargetAndExecuteAction
    {
        private int xpChange;
        private int manaChange;


		public Fireball(AutonomousCharacter character, GameObject target) : base("Fireball",character,target)
		{
            xmlName = "Fire" + target.name;
            manaChange = 5;
            if (target.tag.Equals("Skeleton"))
		    {
		        this.xpChange = 5;
		        
		    }
		    else if (target.tag.Equals("Orc"))
		    {
		        this.xpChange = 10;
		    }
		    else if (target.tag.Equals("Dragon"))
		    {
		        this.xpChange = 0;
                //this.xpChange = 20;
            }
        }

		public override float GetGoalChange(Goal goal)
		{
		    var change = base.GetGoalChange(goal);

		    if (goal.Name == AutonomousCharacter.GAIN_XP_GOAL)
		    {
		        change -= this.xpChange;
		    }
		    return change;
        }

		public override bool CanExecute()
		{
		    if (!base.CanExecute()) return false;
		    return this.Character.GameManager.characterData.Mana >= 5;
        }

        public override bool CanExecute(WorldModel worldModel) {
            if (!base.CanExecute(worldModel)) return false;

            var mana = (int)worldModel.GetProperty(Properties.MANA);
            return mana >= 5;
        }
        public override bool CanExecute(NewWorldModel worldModel) {
            if (!base.CanExecute(worldModel)) return false;

            var mana = (int)worldModel.stats.getStat(Stats.mn);
            return mana >= 5;
        }



		public override void Execute()
		{
		    base.Execute();
		    this.Character.GameManager.Fireball(this.Target);
        }


		public override void ApplyActionEffects(WorldModel worldModel)
		{
		    base.ApplyActionEffects(worldModel);

		    var xpValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_XP_GOAL);
		    worldModel.SetGoalValue(AutonomousCharacter.GAIN_XP_GOAL, xpValue - this.xpChange);

		    var xp = (int) worldModel.GetProperty(Properties.XP);
		    worldModel.SetProperty(Properties.XP, xp + this.xpChange);

            var mp = (int) worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, mp-manaChange);

		    if (!this.Target.CompareTag("Dragon"))
		    {
		        //disables the target object so that it can't be reused again
		        worldModel.SetProperty(this.Target.name, false);
		    }
		}

        public override void ApplyActionEffects(NewWorldModel worldModel) {
            base.ApplyActionEffects(worldModel);

            var hp = worldModel.stats.getStat(Stats.hp);

            //disables the target object so that it can't be reused again
            string s = Target.name;
            int index;
            if (s.StartsWith("Dragon")) {
                //Does not die 
            } else {
                var xp = worldModel.stats.getStat(Stats.xp);
                worldModel.stats.setStat(Stats.xp, worldModel.stats.getStat(Stats.xp) + this.xpChange);
                var mp = (int)worldModel.stats.getStat(Stats.mn);
                worldModel.stats.setStat(Stats.mn, mp - manaChange);

                if (s.StartsWith("Skeleton")) {
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
            worldModel.SetLastAction(this);
        }
        

    }
}
