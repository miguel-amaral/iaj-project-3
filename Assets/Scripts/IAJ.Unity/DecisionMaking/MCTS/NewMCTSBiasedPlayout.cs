﻿

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

        private Dictionary<string, List<string>> chestInfo;
        //public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        //{
        //}

        public NewMCTSBiasedPlayout(NewCurrentStateWorldModel currentStateWorldModel, Dictionary<string, List<string>> chestInfo)
            :base(currentStateWorldModel){
            this.chestInfo = chestInfo;
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

            var enemiesNumber = currentPlayoutState.enemiesAlive;
            

            foreach (var executableAction in currentPlayoutState.GetExecutableActions()) {
                   


                var sum = 5;
                //if(executableAction)
                if (executableAction.Name.Contains("Chest")) {
                    var chestAction = executableAction as PickUpChest;

                    List<string> value;
                    if (!chestInfo.TryGetValue(chestAction.Target.name, out value)) {
                        Debug.Log("This shouldn't happen, bias");
                        return null;
                    }
                    var chestposition = chestAction.Target.transform.position;
                    var alone = true;
                    foreach(var enemy in value) {
                        var s = enemy;
                        //Debug.Log(s);
                        int index;
                        var worldModel = currentPlayoutState;
                        bool alive = true;
                        if (s.StartsWith("Dragon")) {
                            s = s.Substring(6);
                            index = int.Parse(s);
                            alive = worldModel.dragons[index - 1];
                        } else if (s.StartsWith("Skeleton")) {
                            s = s.Substring(8);
                            index = int.Parse(s);
                            alive = worldModel.skeletons[index - 1];
                        } else if (s.StartsWith("Orc")) {
                            s = s.Substring(3);
                            index = int.Parse(s);
                            alive = worldModel.orcs[index - 1];
                        } else {
                            Debug.Log("Not valid target");
                        }
                        if (alive) {
                            alone = false;
                        }
                    }
                    
                    if (alone) {
                        //Debug.Log("Entrei aqui");
                        return executableAction;
                    }
                }

                if (enemiesNumber == 0) {
                    if ((executableAction.Name.StartsWith("GetHealthPotion") || executableAction.Name.StartsWith("GetManaPotion"))) {
                        continue;
                    }
                } else if (mana > 5) {
                    if (executableAction.Name.StartsWith("Fire")) {
                        sum += 40;
                    }
                }
                if (lvl < 3 && executableAction.Name.Contains("Dragon")) {
                    continue;
                }

                if (hp <= 5 && executableAction.Name.StartsWith("Sword")) {
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

