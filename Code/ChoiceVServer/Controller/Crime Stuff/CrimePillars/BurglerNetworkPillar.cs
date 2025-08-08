using AltV.Net.Elements.Entities;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
     public class BurglarNetworkPillar : CrimeNetworkPillar {
        public BurglarNetworkPillar(int id) : base(id, "Einbrecher/Räuber") {

        }

        public override void onCrimeAction(IPlayer player, CrimeAction action, float amount, Dictionary<string, dynamic> data) { }

        public override void onTick() { }

        public override float getNeededReputation(string identifier) {
            switch(identifier) {
                //Level 0
                case "MISSIONS":
                    return 0;
                case "CRIME_BUYER":
                    return 0;
                case "BREAKING_CLOCK_MISSIONS":
                    return 0;
                case "BASE_SHOP":
                    return 0;
                
                case "BREAKING_CLOCK_MISSION_VARIATIONS":
                    return 30;



                //Level 1
                //case "SELECT_MISSION":
                //    return 1000;
                //case "BREAKING_CLOCK_MISSION_VARIATIONS":
                //    return 1000;
                //case "STORE_THEFT_MISSIONS":
                //    return 1000;

                //Level 2
                //case "CRIME_SPREE_MISSIONS":
                //    return 3000;
                //case "MISSIONS_FAVORS_REWARD":
                //    return 3000;
                //case "STORE_THEFT_MISSION_VARIATIONS":
                //    return 3000;
                //case "STORE_ROBBERY_MISSIONS":
                //    return 3000;

                //Level 3
                //case "STORE_ROBBERY_MISSION_VARIATIONS":
                //   return 6000;


                default:
                    return float.MaxValue;
            }
        }

        public override float getCrimeNetworkAllowedActionReputation(string identifier) {
            switch(identifier) {
                case "TODO":
                    return 0;

                default:
                    return float.MinValue;
            }
        }

        public override List<CrimeAction> allConnectedCrimeActions() {
            return new List<CrimeAction> {
                CrimeAction.ParkingClockBreaking,
                CrimeAction.StoreRobbingNpc,
                CrimeAction.StoreTheft,
            };
        }
    }
}