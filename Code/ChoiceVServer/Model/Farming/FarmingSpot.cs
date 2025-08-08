using AltV.Net.Elements.Entities;
using System;

namespace ChoiceVServer.Model.Farming {
    public class FarmingSpot {
        public int Id;
        public ICheckpoint Checkpoint;
        public int CurrentAmount;
        public DateTime NextRefill;

        /// <summary>
        /// Triggers when Player interacts with Checkpoint. With a given Skillset the amount of farmed goods is given
        /// </summary>
        public int farmingInteraction(IPlayer player) {
            if(true) { //TODO Add Skill system Influence
                int amount = 10; //TODO ADD Skillcheck
                return amount;
            } else
            //Skillcheck failed, no Items found but CurrentAmountPool is reduced
            {
                //CurrentAmount = (int)(CurrentAmount - CurrentAmount * 0.1);
                //return -1;
            }
        }

        /// <summary>
        /// Initiates Regrow process for any FarmSpot. Calculates the Farmspot variables with the given Values
        /// </summary>
        /// <param name="ntLevel">Defines how much will regrow in the next run</param>
        /// <param name="phLevel">Defines how much will regrow in the next run</param>
        /// <param name="poLevel">Defines how fast the plant will regrow</param>
        /// <param name="waterLevel">Defines how fast the plant will regrow</param>
        /// <param name="maxAmount">Gives how much eg. fruit a farmspot can maximal regrow</param>
        /// <param name="bestRefillTimeSpan">Gives the best regrow time</param>
        public void initiateRegrow(float ntLevel, float phLevel, float poLevel, float waterLevel, int maxAmount, TimeSpan bestRefillTimeSpan) {
            if(NextRefill < DateTime.Now) {

                //Calculates the regrow amount with Nitrogen and Phosphorat Level
                var amount = (int)(maxAmount - maxAmount * (2 - (ntLevel + phLevel)));

                amount = Math.Abs(amount);

                if(CurrentAmount + amount > maxAmount) {
                    CurrentAmount = maxAmount;
                } else {
                    CurrentAmount = CurrentAmount + maxAmount;
                }

                //Calculates new RefillTime with Potash and Water Level
                NextRefill = DateTime.Now + bestRefillTimeSpan + (bestRefillTimeSpan * Math.Abs((2 - (poLevel + waterLevel))));
            }
        }

        /// <summary>
        /// Regrow method for Farm that do not use fertilizers
        /// </summary>
        public void initiateRegrow(int maxAmount, int amount, TimeSpan refilltime) {
            if(NextRefill < DateTime.Now) {
                if(CurrentAmount + amount < maxAmount) {
                    CurrentAmount = CurrentAmount + amount;
                } else {
                    CurrentAmount = maxAmount;
                }

                NextRefill = DateTime.Now + refilltime;
            }
        }
    }
}
