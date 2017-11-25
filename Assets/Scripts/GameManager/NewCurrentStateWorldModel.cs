
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameManager {


    public class NewCurrentStateWorldModel : NewFutureStateWorld {


        public void Initialize() {
            //this.ActionEnumerator.Reset();
        }
        public NewCurrentStateWorldModel(GameManager gm, List<Action> actions): base(gm, actions) {
            copyStats(gm);
            this.stats = new Stats(gm);
            //PopulatePossibleActions();
        }

        private void copyStats(GameManager gm) {
            
            this.skeletons = new bool[gm.skeletons.Count];
            for(int i= 0; i<gm.skeletons.Count; i++) {
                this.skeletons[i] = (gm.skeletons[i] == null ? false : true);
            }

            this.orcs = new bool[gm.orcs.Count];
            for (int i = 0; i < gm.orcs.Count; i++) {
                this.orcs[i] = (gm.orcs[i] == null ? false : true);
            }

            this.dragons = new bool[gm.dragons.Count];
            for (int i = 0; i < gm.dragons.Count; i++) {
                this.dragons[i] = (gm.dragons[i] == null ? false : true);
            }

            this.chests = new bool[gm.chests.Count];
            for (int i = 0; i < gm.chests.Count; i++) {
                this.chests[i] = (gm.chests[i] == null ? false : true);
            }

            this.manaPots = new bool[gm.manaPots.Count];
            for (int i = 0; i < gm.manaPots.Count; i++) {
                this.manaPots[i] = (gm.manaPots[i] == null ? false : true);
            }

            this.healthPots = new bool[gm.healthPots.Count];
            for (int i = 0; i < gm.healthPots.Count; i++) {
                this.healthPots[i] = (gm.healthPots[i] == null ? false : true);
            }
            
            this.stats = new Stats(this.gm);
        }

        public void UpdateCurrentStateWorldModel() {
            copyStats(this.gm);
        }


        

        public override int GetNextPlayer() {
            //in the current state, the next player is always player 0
            return 0;
        }
    }
}
