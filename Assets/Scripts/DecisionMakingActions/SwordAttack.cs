using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;

namespace Assets.Scripts.DecisionMakingActions
{
    public class SwordAttack : WalkToTargetAndExecuteAction
    {
        public int hpChange;
        public int xpChange;

        private int indexInList;
        private int worldModelListNr;

        public SwordAttack(AutonomousCharacter character, GameObject target) : base("SwordAttack",character,target)
        {
            xmlName = "Sword " + target.name;
            var s = target.name;
            
            if (s.StartsWith("Dragon")) {
                s = s.Substring(6);
                indexInList = int.Parse(s)-1;
                worldModelListNr = 1;
            } else if (s.StartsWith("Skeleton")) {
                s = s.Substring(8);
                indexInList = int.Parse(s) -1;
                worldModelListNr = 2;
            } else if (s.StartsWith("Orc")) {
                s = s.Substring(3);
                indexInList = int.Parse(s) - 1;
                worldModelListNr = 3;
            }

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

        public override bool CanExecute(NewWorldModel worldModel) {
            if(this.Target == null) {
                return false;
            }
            //else {
            //    string s = this.Target.name;
            //    int size = s.Length;
            //    string name;
            //    int index;
            //    if(size == 7) {
            //      //Dragon
            //        index = int.Parse(s[6].ToString());
            //        return worldModel.dragons[index - 1];
            //    }selse if (size == 10) {
            //        //Skeletons
            //        index = int.Parse(s[9].ToString());
            //        return worldModel.skeletons[index - 1];
            //    }else if (size == 4) {
            //        //orcs
            //        index = int.Parse(s[3].ToString());
            //        return worldModel.orcs[index - 1];
            //    } else {
            //        return false;
            //    }
            //}
            else {
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
            if (this.worldModelListNr == 1) {
                worldModel.dragons[indexInList] = false;
            } else if (this.worldModelListNr == 2) {
                worldModel.skeletons[indexInList] = false;
            } else if (this.worldModelListNr == 3) {
                worldModel.orcs[indexInList] = false;
            } else {
                Debug.LogError("Invalid Target Sword Attack");
            }

            if(worldModel.GetNextPlayer() == 1) {
                //Debug.Log("Sou burro");
                if (worldModel.IsTerminal()) {
                    worldModel.RemoveLastActionEffect();
                }
            }

            worldModel.SetLastAction(this);
        }
        

    }
}
