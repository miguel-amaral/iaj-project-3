﻿
using Assets.Scripts.DecisionMakingActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameManager {
    public class NewFutureStateWorld : NewWorldModel{
        protected int NextPlayer { get; set; }
        protected Action NextEnemyAction { get; set; }
        protected Action[] NextEnemyActions { get; set; }
        public Action LastAction;
        

        public NewFutureStateWorld(GameManager gm, List<Action> actions) :base(gm, actions) {
            this.NextPlayer = 0;
            //PopulatePossibleActions();
        }

        public NewFutureStateWorld(NewFutureStateWorld old) : base(old) {
            this.NextPlayer = old.GetNextPlayer();
            this.LastAction = old.LastAction;
            //copy stats maybe?
            //PopulatePossibleActions();
        }



 
        public override NewWorldModel GenerateChildWorldModel() {
            return new NewFutureStateWorld(this);
        }

        public override bool IsTerminal() {
            int HP = this.stats.getStat(Stats.hp);
            float time = this.stats.getTime();
            int money = this.stats.getStat(Stats.money);

            //return (this.NextPlayer == 0) && (HP <= 0 || time >= 200 || money == 25);
            return (HP <= 0 || time >= 200 || money == 25);
        }

        public override float GetScore() {
            int money = this.stats.getStat(Stats.money);
            int hp = this.stats.getStat(Stats.hp);
            float time = this.stats.getTime();

            var maxTime = 125 + 400;

            if (money == 25 && hp > 0 && time < 200) {
                return (125 + 400 - time*2)/maxTime; //0.5f + 0.0025f * (200 -time);

            } else {
                return money/maxTime;
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
