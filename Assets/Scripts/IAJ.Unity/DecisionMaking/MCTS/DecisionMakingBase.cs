using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public interface DecisionMakingBase
    {
        GOB.Action[] BestActionSequence { get; }
        bool InProgress { get; }
        float TotalProcessingTime { get; }
        float ParcialProcessingTime { get; }

        GOB.Action BestAction { get; }

        Action ChooseAction();
        void InitializeDecisionMakingProcess();
    }
}