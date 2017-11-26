using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;

namespace Assets.Scripts.DecisionMakingActions
{
    public class PickUpChest : WalkToTargetAndExecuteAction
    {
        private int indexInList;

        public PickUpChest(AutonomousCharacter character, GameObject target) : base("PickUpChest",character,target)
        {
            xmlName = target.name;
            var s = this.Target.name;
            s = s.Substring(5);
            indexInList = int.Parse(s) - 1;
        }


        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            if (goal.Name == AutonomousCharacter.GET_RICH_GOAL) change -= 5.0f;
            return change;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            return true;
        }

        public override bool CanExecute(NewWorldModel worldModel) {
            if (this.Target == null) {
                return false;
            }
            return worldModel.chests[indexInList];            
        }




        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.PickUpChest(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.GET_RICH_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GET_RICH_GOAL, goalValue - 5.0f);

            var money = (int)worldModel.GetProperty(Properties.MONEY);
            worldModel.SetProperty(Properties.MONEY, money + 5);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override void ApplyActionEffects(NewWorldModel worldModel) {
            base.ApplyActionEffects(worldModel);

            var money = worldModel.stats.getStat(Stats.money);
            worldModel.stats.setStat(Stats.money, money + 5);

            //disables the target object so that it can't be reused again
            worldModel.chests[indexInList] = false;

            worldModel.SetLastAction(this);
        }

        public override void RemoveEffect(NewWorldModel worldModel) {
            base.RemoveEffect(worldModel);
            var money = worldModel.stats.getStat(Stats.money);
            worldModel.stats.setStat(Stats.money, money - 5);
        }

        

        }
}
