using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System;
using UnityEngine;

namespace Assets.Scripts.DecisionMakingActions
{
    public class GetHealthPotion : WalkToTargetAndExecuteAction
    {
        public GetHealthPotion(AutonomousCharacter character, GameObject target) : base("GetHealthPotion",character,target)
        {
            xmlName = target.name;

        }

        public override bool CanExecute()
		{
		    if (!base.CanExecute()) return false;
		    return this.Character.GameManager.characterData.HP < this.Character.GameManager.characterData.MaxHP;
        }

		public override bool CanExecute(WorldModel worldModel)
		{
		    if (!base.CanExecute(worldModel)) return false;

		    var hp = (int)worldModel.GetProperty(Properties.HP);
		    return hp < (int)worldModel.GetProperty(Properties.MAXHP);
        }

        public override bool CanExecute(NewWorldModel worldModel) {
            if (!base.CanExecute(worldModel)) return false;

            var hp = worldModel.stats.getStat(Stats.hp);

            return hp < worldModel.stats.getStat(Stats.maxhp);
        }


        public override void Execute()
		{
		    base.Execute();
		    this.Character.GameManager.GetHealthPotion(this.Target);
        }

		public override void ApplyActionEffects(WorldModel worldModel)
		{
		    base.ApplyActionEffects(worldModel);
		    worldModel.SetProperty(Properties.HP, Character.GameManager.characterData.MaxHP);
		    //disables the target object so that it can't be reused again
		    worldModel.SetProperty(this.Target.name, false);
        }

        public override void ApplyActionEffects(NewWorldModel worldModel) {
            base.ApplyActionEffects(worldModel);

            worldModel.stats.setStat(Stats.hp, worldModel.stats.getStat(Stats.maxhp));
            //disables the target object so that it can't be reused again
            var s = this.Target.name;
            s = s.Substring(12);
            var index = int.Parse(s);
            worldModel.healthPots[index - 1] = false;
        }
    }
}
