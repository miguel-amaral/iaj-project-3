﻿using Assets.Scripts.DecisionMakingActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameManager
{
    public class FutureStateWorldModel : WorldModel
    {
        public GameManager GameManager { get; protected set; }
        protected int NextPlayer { get; set; }
        protected Action NextEnemyAction { get; set; }
        protected Action[] NextEnemyActions { get; set; }

        public FutureStateWorldModel(GameManager gameManager, List<Action> actions) : base(actions)
        {
            this.GameManager = gameManager;
            this.NextPlayer = 0;
        }

        public FutureStateWorldModel(FutureStateWorldModel parent) : base(parent)
        {
            this.GameManager = parent.GameManager;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new FutureStateWorldModel(this);
        }


        public override bool IsTerminal()
        {
            int HP = (int)this.GetProperty(Properties.HP);
            float time = (float)this.GetProperty(Properties.TIME);
            int money = (int)this.GetProperty(Properties.MONEY);

            return (this.NextPlayer == 0) && (HP <= 0 ||  time >= 200 || money == 25);
        }

        public override float GetScore()
        {
            int money = (int)this.GetProperty(Properties.MONEY);
            int hp = (int) GetProperty(Properties.HP);
            int mhp = (int) GetProperty(Properties.MAXHP);
            float time = (float)this.GetProperty(Properties.TIME);

            if (money == 25 && hp > 0) {
                //Debug.Log("Ganhei");
                return 125 + 200 + 200 - (time > 200 ? 200 : time); //0.5f + 0.0025f * (200 -time);
           
            } else { 
                return money;
            }
            //if (money > 0)
            //{
                
            //}
            //else return 0.0f;
        }

        public override int GetNextPlayer()
        {
            return this.NextPlayer;
        }

        public override void CalculateNextPlayer()
        {
            Vector3 position = (Vector3)this.GetProperty(Properties.POSITION);
            bool enemyEnabled;

            //basically if the character is close enough to an enemy, the next player will be the enemy.
            foreach (var enemy in this.GameManager.enemies)
            {
                enemyEnabled = (bool) this.GetProperty(enemy.name);
                if (enemyEnabled && (enemy.transform.position - position).sqrMagnitude <= 400)
                {
                    this.NextPlayer = 1;
                    this.NextEnemyAction = new SwordAttack(this.GameManager.autonomousCharacter, enemy);
                    this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                    return; 
                }
            }
            this.NextPlayer = 0;
            //if not, then the next player will be player 0
        }

        public override Action GetNextAction()
        {
            Action action;
            if (this.NextPlayer == 1)
            {
                action = this.NextEnemyAction;
                this.NextEnemyAction = null;
                return action;
            }
            else return base.GetNextAction();
        }

        public override Action[] GetExecutableActions()
        {
            if (this.NextPlayer == 1)
            {
                return this.NextEnemyActions;
            }
            else return base.GetExecutableActions();
        }

    }
}
