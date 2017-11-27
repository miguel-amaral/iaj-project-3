
using Assets.Scripts.DecisionMakingActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS;

namespace Assets.Scripts.GameManager {
    public class NewFutureStateWorld : NewWorldModel{
        protected int NextPlayer { get; set; }
        protected Action NextEnemyAction { get; set; }
        protected Action[] NextEnemyActions { get; set; }
        public Action LastAction;
        private Dictionary<string, List<Pair<int, int>>> chestInfo;
        private Dictionary<string, List<int>> enemiesRewards;



        public NewFutureStateWorld(NewFutureStateWorld old) : base(old) {
            this.NextPlayer = old.GetNextPlayer();
            this.LastAction = old.LastAction;
            this.chestInfo = old.chestInfo;
            this.enemiesRewards = old.enemiesRewards;
            //copy stats maybe?
            //PopulatePossibleActions();
        }

        public NewFutureStateWorld(GameManager gm, List<Action> actions, Dictionary<string, List<Pair<int, int>>> chestInfo, Dictionary<string, List<int>> enemiesRewards) : base(gm, actions) {
            this.chestInfo = chestInfo;
            this.enemiesRewards = enemiesRewards;
            this.NextPlayer = 0;
        }

        public override NewWorldModel GenerateChildWorldModel() {
            return new NewFutureStateWorld(this);
        }

        public override void BiasedSelection() {
            
            var lastAsSword = LastAction as WalkToTargetAndExecuteAction;
            if(lastAsSword != null) {
                var name = lastAsSword.target_name;
                List<int> chestsIndexes;
                enemiesRewards.TryGetValue(name, out chestsIndexes);
                if(chestsIndexes != null) {
                    foreach(var index in chestsIndexes) {
                        if (chests[index]) {
                            List<Pair<int, int>> possiblesEnemies;
                            var chest_target_name = "Chest" + (index + 1);
                            chestInfo.TryGetValue(chest_target_name, out possiblesEnemies);
                            if(possiblesEnemies != null) {
                                var theEnemyIsAlive = false;
                                foreach (var enemy in possiblesEnemies) {
                                    if (enemy.First == 1) {
                                        if (dragons[enemy.Second]) {
                                            theEnemyIsAlive = true;
                                            break;
                                        }
                                    } else if (enemy.First == 2) {
                                        if (skeletons[enemy.Second]) {
                                            theEnemyIsAlive = true;
                                            break;
                                        }
                                    } else if (enemy.First == 3) {
                                        if (orcs[enemy.Second]) {
                                            theEnemyIsAlive = true;
                                            break;
                                        }
                                    }
                                }
                                if (theEnemyIsAlive) {
                                    continue;
                                } else {
                                    //THIS IS REALLY A GOOD MOVE....
                                    foreach(var action in allActions) {
                                        var pickUp = action as PickUpChest;
                                        if(pickUp != null && pickUp.target_name .Equals( chest_target_name) ){
                                            if (pickUp.CanExecute()) {
                                                this.availableActions = new List<Action>() ;
                                                availableActions.Add(action);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            base.BiasedSelection();
        }

        public override bool IsTerminal() {
            int HP = this.stats.getStat(Stats.hp);
            float time = this.stats.getTime();
            int money = this.stats.getStat(Stats.money);

            return HP <= 0 || time >= 200 || (this.NextPlayer == 0 && money == 25);
            //return (HP <= 0 || time >= 200 || money == 25);
        }

        public override float GetScore() {
            int money = this.stats.getStat(Stats.money);
            int hp = this.stats.getStat(Stats.hp);
            float time = this.stats.getTime();

            float maxTime = 125.0f + 400.0f;

            if (money == 25 && hp > 0 && time < 200) {
                return (125 + 400 - time*2); //0.5f + 0.0025f * (200 -time);
            } else {
                //if (this.NextPlayer == 0) {
                    this.defeat = true;
                //}
                return money;
            }
        }

        public override void RemoveLastActionEffect() {
            if(this.LastAction != null) {
                this.LastAction.RemoveEffect(this);

            }
        }

        public override Action GetLastAction() {
            return this.LastAction;
        }

        public override void SetLastAction(Action action) {
            this.LastAction = action;
        }


        //public Action GetLastTarget() {
        //    return this.LastTarget;
        //}

        //public override void SetLastTarget(GameObject target) {
        //    this.LastTarget = target;
        //}

        public override int GetNextPlayer() {
            return this.NextPlayer;
        }

        public override void CalculateNextPlayer() {
            Vector3 position = this.stats.getPosition();
            bool enemyEnabled;

            //basically if the character is close enough to an enemy, the next player will be the enemy.
            foreach (var enemy in this.gm.enemies) {
                enemyEnabled = (enemy == null ? false : true);
                if (!enemyEnabled) {
                    continue;
                }

                var s = enemy.name;
                int index;
                var worldModel = this;
                bool enabledOnArray = false;

                
                if (enemy.CompareTag("Dragon")) {
                    s = s.Substring(6);
                    index = int.Parse(s);
                    enabledOnArray = worldModel.dragons[index - 1];
                } else if (enemy.CompareTag("Skeleton")) {
                    s = s.Substring(8);
                    index = int.Parse(s);
                    enabledOnArray = worldModel.skeletons[index - 1];
                } else if (enemy.CompareTag("Orc")) {
                    s = s.Substring(3);
                    index = int.Parse(s);
                    enabledOnArray = worldModel.orcs[index - 1];
                } else {
                    Debug.Log("Not valid target");
                }

                if (enabledOnArray && (enemy.transform.position - position).sqrMagnitude <= 400) {
                    //Debug.Log("eu sou autisma");
                    this.NextPlayer = 1;
                    this.NextEnemyAction = new SwordAttack(this.gm.autonomousCharacter, enemy);
                    this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                    return;
                }
            }
            this.NextPlayer = 0;
            //if not, then the next player will be player 0
        }

        public override Action GetNextAction() {
            Action action;
            if (this.NextPlayer == 1) {
                action = this.NextEnemyAction;
                this.NextEnemyAction = null;
                return action;
            } else return base.GetNextAction();
        }

        public override Action[] GetExecutableActions() {
            if (this.NextPlayer == 1) {
                return this.NextEnemyActions;
            } else return base.GetExecutableActions();
        }

    }
}
