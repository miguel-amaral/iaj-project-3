using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using System;

namespace Assets.Scripts.DecisionMakingActions
{
    public class Fireball : WalkToTargetAndExecuteAction
    {
        protected int xpChange;
        protected int manaChange;

        private int indexInList;
        private int worldModelListNr;

        public Fireball(AutonomousCharacter character, GameObject target) : base("Fireball",character,target)
		{
            xmlName = "Fire" + target.name;
            manaChange = 5;

            var s = target.name;

            if (s.StartsWith("Dragon")) {
                s = s.Substring(6);
                indexInList = int.Parse(s) - 1;
                worldModelListNr = 1;
            } else if (s.StartsWith("Skeleton")) {
                s = s.Substring(8);
                indexInList = int.Parse(s) - 1;
                worldModelListNr = 2;
            } else if (s.StartsWith("Orc")) {
                s = s.Substring(3);
                indexInList = int.Parse(s) - 1;
                worldModelListNr = 3;
            }

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
            if (this.Target == null) {
                return false;
            }
            var mana = (int)worldModel.stats.getStat(Stats.mn);
            if(mana < 5) {
                return false;
            }

            if (this.Target == null) {
                return false;
            }else {
                if (this.worldModelListNr == 1) {
                    return worldModel.dragons[indexInList];
                } else if (this.worldModelListNr == 2) {
                    return worldModel.skeletons[indexInList];
                } else if (this.worldModelListNr == 3) {
                    return worldModel.orcs[indexInList];
                } else {
                    return false;
                }
            }

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
            if (this.worldModelListNr == 1) { 
                //Does not die dragon
            } else {
                var xp = worldModel.stats.getStat(Stats.xp);
                worldModel.stats.setStat(Stats.xp, worldModel.stats.getStat(Stats.xp) + this.xpChange);
                var mp = (int)worldModel.stats.getStat(Stats.mn);
                worldModel.stats.setStat(Stats.mn, mp - manaChange);

                if (this.worldModelListNr == 2) {
                    worldModel.skeletons[indexInList] = false;
                } else if (this.worldModelListNr == 3) {
                    worldModel.orcs[indexInList] = false;
                } else {
                    Debug.LogError("Invalid Target Sword Attack");
                }
            }
            worldModel.SetLastAction(this);
        }
        

    }
}
