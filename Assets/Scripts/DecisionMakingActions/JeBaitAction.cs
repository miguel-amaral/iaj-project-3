using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;

namespace Assets.Scripts.DecisionMakingActions
{
    public class JeBaitAction : WalkToTargetAndExecuteAction
    {

        public JeBaitAction(string targetName) : base(targetName){ }

        public void RenameTargetName(string newName) {
            this.target_name = newName;
        }
    }
}
