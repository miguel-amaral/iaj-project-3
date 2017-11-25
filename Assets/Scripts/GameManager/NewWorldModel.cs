﻿using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameManager {

    public class NewWorldModel {
        
        protected GameManager gm;

        public bool[] skeletons;
        public bool[] orcs;
        public bool[] dragons;
        public bool[] chests;
        public bool[] manaPots;
        public bool[] healthPots;
        public Stats stats;

        private List<Action> actions;
        private int nextAction;
        private List<Action> availableActions;

        protected void PopulatePossibleActions() {
            this.nextAction = 0;
            availableActions = new List<Action>();

            foreach(var a in actions){
                if (a.CanExecute(this)) {
                    availableActions.Add(a);
                }
            }
        }


        public NewWorldModel(GameManager gm, List<Action> actions) {

            this.gm = gm;
            //foreach(var a in actions){
            //    if (a.CanExecute(this)) {
            //        availableActions.Add(a);
            //    }
            //}
            
            this.actions = new List<Action>(actions);
            this.availableActions = null;
            //this.availableActions = new List<Action>();
            
            //this.stats = new Stats(gm);
        }

        public NewWorldModel(NewWorldModel oldWorld) : this(oldWorld.gm, oldWorld.actions) {
            this.skeletons = (bool[]) oldWorld.skeletons.Clone();
            this.orcs = (bool[])oldWorld.orcs.Clone();
            this.dragons = (bool[])oldWorld.dragons.Clone();
            this.chests = (bool[])oldWorld.chests.Clone();
            this.manaPots = (bool[])oldWorld.manaPots.Clone();
            this.healthPots = (bool[])oldWorld.healthPots.Clone();
            this.stats = new Stats(oldWorld.stats);
        }

        public virtual NewWorldModel GenerateChildWorldModel() {
            return new NewWorldModel(this);
        }

        public virtual Action[] GetExecutableActions() {
            if(availableActions == null) {
                PopulatePossibleActions();
            }

            return this.availableActions.ToArray();
        }

        public virtual Action GetNextAction() {
            if (availableActions == null) {
                PopulatePossibleActions();
            }

            if(nextAction< this.availableActions.Count){
                var actionToReturn = this.availableActions[nextAction];
                nextAction++;
                return actionToReturn;
            } else {
                return null;
            }
        }

        public virtual bool IsTerminal() {
            return true;
        }


        public virtual float GetScore() {
            return 0.0f;
        }

        public virtual int GetNextPlayer() {
            return 0;
        }

        public virtual void CalculateNextPlayer() {
        }



        public string toString() {
            string toReturn = "NEWWORLDMODEL\n";
            toReturn += "\nDragons - ";
            foreach (var a in dragons) {
                toReturn += a + " : ";
            }
            toReturn += "\nSkeletons - ";
            foreach (var a in skeletons) {
                toReturn += a + " : ";
            }
            toReturn += "\nOrcs - ";
            foreach (var a in orcs) {
                toReturn += a + " : ";
            }
            toReturn += "\nChests - ";
            foreach (var a in chests) {
                toReturn += a + " : ";
            }
            toReturn += "\nHealthPots - ";
            foreach (var a in healthPots) {
                toReturn += a + " : ";
            }
            toReturn += "\nManaPots - ";
            foreach (var a in manaPots) {
                toReturn += a + " : ";
            }
            toReturn += "\nStats - ";
            toReturn += this.stats.toString();
            toReturn += "\n";
            //toReturn += "\nActions - ";
            //foreach (var a in actions) {
            //    toReturn += a + "\n : ";
            //}
            toReturn += "\nAvailableActions - ";
            foreach (var a in availableActions) {
                toReturn += a + "\n : ";
            }
            return toReturn;
        }

    }

    public class Stats {
        protected int[] stats = new int[size];
        private Vector3 position;
        private float time;

        public const int size = 6;
        public const int hp = 0;
        public const int mn = 1;
        public const int lvl = 2;
        public const int xp = 3;
        public const int money = 4;
        public const int maxhp = 5;

        public Stats(GameManager gm) {
            CharacterData cd = gm.characterData;
            stats[hp] = cd.HP;
            stats[mn] = cd.Mana;
            stats[lvl] = cd.Level;
            stats[xp] = cd.XP;
            stats[money] = cd.Money;
            stats[maxhp] = cd.MaxHP;

            position = cd.CharacterGameObject.transform.position;
            time = cd.Time;

        }

        public Stats(Stats stats) {
            this.stats = (int[])stats.stats.Clone();
            this.position = new Vector3(stats.position.x, stats.position.y, stats.position.z);
            this.time = stats.time;
        }

        public int getStat(int number) {
            return this.stats[number];
        }

        public void setStat(int number, int value) {
            this.stats[number] = value;
        }

        public float getTime() {
            return this.time;
        }

        public void setTime(float time) {
            this.time = time;
        }

        public Vector3 getPosition() {
            return this.position;
        }

        public void setPosition(Vector3 position) {
            this.position = position;
        }

        public string toString() {
            string toReturn = "";
            foreach (var a in stats) {
                toReturn += a + " : ";
            }
            toReturn += time + " : ";
            toReturn += position;
            return toReturn;
        }
    }
       
}
