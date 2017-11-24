using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;

namespace Assets.Scripts.DecisionMakingActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {
        public GetManaPotion(AutonomousCharacter character, GameObject target) : base("GetManaPotion",character,target)
        {
            xmlName = target.name;

        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return this.Character.GameManager.characterData.Mana < 10;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;

            var mana = (int)worldModel.GetProperty(Properties.MANA);
            return mana < 10;
        }

        public override bool CanExecute(NewWorldModel worldModel) {
            if (!base.CanExecute(worldModel)) return false;

            var mana = worldModel.stats.getStat(Stats.mn);
            
            return mana < 10;
        }


        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.GetManaPotion(this.Target);
        }


        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            worldModel.SetProperty(Properties.MANA, 10);
            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override void ApplyActionEffects(NewWorldModel worldModel) {
            base.ApplyActionEffects(worldModel);
            worldModel.stats.setStat(Stats.mn, 10);
            //disables the target object so that it can't be reused again
            var s = this.Target.name;
            s = s.Substring(10);
            var index = int.Parse(s);
            worldModel.manaPots[index - 1] = false;
        }
    }
}
