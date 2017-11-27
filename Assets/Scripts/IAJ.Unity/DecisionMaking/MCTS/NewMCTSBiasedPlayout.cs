

using Assets.Scripts.DecisionMakingActions;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS {
    class NewMCTSBiasedPlayout : NewMCTS{

        private const int ManaWeight = 10;
        private const int XPWeight = 400;
        private const int MaxHPWeight = 0;
        private const int HPWeight = 20;
        private const int MoneyWeight = 0;
        private const int TimeWeight = 0;
        private const int LevelWeight = 500;
        private const int PositionWeight = 0;

        
        private Dictionary<string, List<Pair<int, int>>> chestInfo;
        private Dictionary<string, List<int>> enemiesRewards;

        //public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        //{
        //}

        public NewMCTSBiasedPlayout(NewCurrentStateWorldModel currentStateWorldModel, Dictionary<string, List<Pair<int, int>>> chestInfo, Dictionary<string, List<int>> enemiesRewards) : base(currentStateWorldModel) {
            this.chestInfo = chestInfo;
            this.enemiesRewards = enemiesRewards;
        }

        protected override Action GuidedAction(NewWorldModel currentPlayoutState) {
            //var originalWorldModel = currentPlayoutState;
            List<Action> bestOverallAction = new List<GOB.Action>();
            List<int> weights = new List<int>();

            GameManager.GameManager gameManager = currentPlayoutState.gm;

            //precaution

            var mana = currentPlayoutState.stats.getStat(Stats.mn);
            var hp = currentPlayoutState.stats.getStat(Stats.hp);
            var lvl = currentPlayoutState.stats.getStat(Stats.lvl);
            var position = currentPlayoutState.stats.getPosition();

            //var enemiesNumber = currentPlayoutState.enemiesAlive;
            

            //var goodChest = false;
            foreach (var executableAction in currentPlayoutState.GetExecutableActions()) {


                var sum = 5;
                //var chestAction = executableAction as PickUpChest;
                //if (chestAction != null) {
                //
                //    List<string> value;
                //    if (!chestInfo.TryGetValue(chestAction.Target.name, out value)) {
                //        Debug.Log("This shouldn't happen, bias");
                //        return null;
                //    }
                //    var chestposition = chestAction.Target.transform.position;
                //    var alone = true;
                //    foreach(var enemy in value) {
                //        var s = enemy;
                //        //Debug.Log(s);
                //        int index;
                //        var worldModel = currentPlayoutState;
                //        bool alive = true;
                //        if (s.StartsWith("Dragon")) {
                //            s = s.Substring(6);
                //            index = int.Parse(s);
                //            alive = worldModel.dragons[index - 1];
                //        } else if (s.StartsWith("Skeleton")) {
                //            s = s.Substring(8);
                //            index = int.Parse(s);
                //            alive = worldModel.skeletons[index - 1];
                //        } else if (s.StartsWith("Orc")) {
                //            s = s.Substring(3);
                //            index = int.Parse(s);
                //            alive = worldModel.orcs[index - 1];
                //        } else {
                //            Debug.Log("Not valid target");
                //        }
                //        if (alive) {
                //            alone = false;
                //            break;
                //        }
                //    }
                //
                //    if (alone) {
                //        //Debug.Log("Entrei aqui");
                //        //goodChest = true;
                //        sum += 5000;
                //        //return executableAction;
                //    }
                //}

                var level = executableAction as LevelUp;
                if (level != null) {
                    return executableAction;
                }

                if (currentPlayoutState.enemiesAlive == 0) {
                    var hpAction = executableAction as GetHealthPotion;
                    var mpAction = executableAction as GetManaPotion;
                    if(hpAction != null || mpAction != null) {
                        continue;
                    }
                } else if (mana > 5) {
                    var fireAction = executableAction as Fireball;
                    if (fireAction != null) {
                        sum += 40;
                    }
                }
                
                //if (lvl < 3 && executableAction.Name.Contains("Dragon")) {
                //    continue;
                //}

                var swordAction = executableAction as SwordAttack;
                if (swordAction != null && hp + swordAction.hpChange <= 0 ) {
                    continue;
                }
                //private bool CheckRange(GameObject obj, float maximumSqrDistance) {
                //    return (obj.transform.position - this.characterData.CharacterGameObject.transform.position).sqrMagnitude <= maximumSqrDistance;
                //}
                //if (currentPlayoutState.


                bestOverallAction.Add(executableAction);
                if (weights.Count > 0) {
                    weights.Add(weights[weights.Count - 1] + sum);
                } else {
                    weights.Add(sum);
                }

            }
            if (bestOverallAction.Count == 0) {
                return null;
            }
            var bestValue = this.RandomGenerator.Next(weights[weights.Count - 1]);
            for (int i = 0; i < weights.Count; i++) {
                if (bestValue < weights[i]) {
                    return bestOverallAction[i];
                }
            }
            return null;
            //return bestOverallAction[this.RandomGenerator.Next(bestOverallAction.Count)];
        }
        
    }

    
}

