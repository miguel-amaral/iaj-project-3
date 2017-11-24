using System;
using System.Linq;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.GOB.Action;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    class MCTSBiasedPlayout : MCTS
    {

        private const int ManaWeight = 10;
        private const int XPWeight = 400;
        private const int MaxHPWeight = 0;
        private const int HPWeight = 20;
        private const int MoneyWeight = 0;
        private const int TimeWeight = 0;
        private const int LevelWeight = 500;
        private const int PositionWeight = 0;

        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected override Action GuidedAction(WorldModel currentPlayoutState)
        {
            //var originalWorldModel = currentPlayoutState;
            List<Action> bestOverallAction = new List<GOB.Action>();
            List<int> weights = new List<int>();
            var ola = currentPlayoutState as FutureStateWorldModel;
            GameManager.GameManager gameManager = null;
            if(ola == null) {
                Debug.Log("Aspergers, mctsBiasedPlayout");
                return null;
            } else {
                gameManager = ola.GameManager;
            }

            //precaution
            

            var mana = (int)currentPlayoutState.GetProperty(Properties.MANA);
            var hp = (int)currentPlayoutState.GetProperty(Properties.HP);
            var lvl = (int)currentPlayoutState.GetProperty(Properties.LEVEL);
            var position = (Vector3)currentPlayoutState.GetProperty(Properties.POSITION);

            foreach (var executableAction in currentPlayoutState.GetExecutableActions())
            {
                //apply to get next state and check who is next player (maybe)
                //trully fucking expensive    
                
                
                var sum = 5;
                //if(executableAction)
                if(gameManager.enemies.Count == 0) {
                    if((executableAction.Name.StartsWith("GetHealthPotion") || executableAction.Name.StartsWith("GetManaPotion"))) {
                        continue;
                    }
                }else if(mana > 5) {
                    if (executableAction.Name.StartsWith("Fire")){
                        sum += 40;
                    }
                }
                if(lvl<3 && executableAction.Name.Contains("Dragon")) {
                    continue;
                }

                if(hp <= 5 && executableAction.Name.StartsWith("Sword")) {
                    continue;
                }
                //private bool CheckRange(GameObject obj, float maximumSqrDistance) {
                //    return (obj.transform.position - this.characterData.CharacterGameObject.transform.position).sqrMagnitude <= maximumSqrDistance;
                //}
                //if (currentPlayoutState.


                bestOverallAction.Add(executableAction);
                if(weights.Count > 0) {
                    weights.Add(weights[weights.Count - 1] + sum);
                } else {
                    weights.Add(sum);
                }

            }
            if (bestOverallAction.Count == 0) {
                return null;
            }
            var bestValue = this.RandomGenerator.Next(weights[weights.Count - 1]);
            for(int i =0; i< weights.Count; i++) {
                if (bestValue < weights[i]) {
                    return bestOverallAction[i];
                }
            }
            return null;
            //return bestOverallAction[this.RandomGenerator.Next(bestOverallAction.Count)];
        }
        //protected override Action GuidedAction(WorldModel currentPlayoutState)
        //{
        //    //var originalWorldModel = currentPlayoutState;
        //    Action bestOverallAction = null;
        //    var bestHeuristicValue = int.MinValue;

        //    foreach (var executableAction in currentPlayoutState.GetExecutableActions())
        //    {
        //        var childWorldModel = currentPlayoutState.GenerateChildWorldModel();
        //        executableAction.ApplyActionEffects(childWorldModel);
        //        childWorldModel.CalculateNextPlayer();

        //        var properties =
        //new Pair<int, int>[]
        //{
        //    new Pair<int, int>((int) childWorldModel.GetProperty(Properties.MANA), ManaWeight),
        //    new Pair<int, int>((int) childWorldModel.GetProperty(Properties.MAXHP), MaxHPWeight),
        //    new Pair<int, int>((int) childWorldModel.GetProperty(Properties.XP), XPWeight),
        //    new Pair<int, int>((int) childWorldModel.GetProperty(Properties.HP), HPWeight),
        //    new Pair<int, int>((int) childWorldModel.GetProperty(Properties.MONEY), MoneyWeight),
        //    // new Pair<object, int>(currentPlayoutState.GetProperty(Properties.TIME), TimeWeight),
        //    new Pair<int, int>((int) childWorldModel.GetProperty(Properties.LEVEL), LevelWeight),
        //    //new Pair<object, int>(currentPlayoutState.GetProperty(Properties.POSITION), PositionWeight),
        //};

    //        var sum = properties.Sum(property => property.First * property.Second);

    //        if (sum > bestHeuristicValue)
    //        {
    //            bestHeuristicValue = sum;
    //            bestOverallAction = executableAction;
    //        }

    //    }
    //    Debug.Log(bestHeuristicValue + " ac> " + bestOverallAction);

    //    return bestOverallAction;
    //}

    //if (propertyName.Equals(Properties.MANA)) return this.GameManager.characterData.Mana;

    //if (propertyName.Equals(Properties.XP)) return this.GameManager.characterData.XP;

    //if (propertyName.Equals(Properties.MAXHP)) return this.GameManager.characterData.MaxHP;

    //if (propertyName.Equals(Properties.HP)) return this.GameManager.characterData.HP;

    //if (propertyName.Equals(Properties.MONEY)) return this.GameManager.characterData.Money;

    //if (propertyName.Equals(Properties.TIME)) return this.GameManager.characterData.Time;

    //if (propertyName.Equals(Properties.LEVEL)) return this.GameManager.characterData.Level;

    //if (propertyName.Equals(Properties.POSITION))
}

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    };
}
