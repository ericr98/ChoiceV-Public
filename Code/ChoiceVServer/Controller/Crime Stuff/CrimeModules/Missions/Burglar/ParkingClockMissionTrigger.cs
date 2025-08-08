using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class ParkingClockMissionTrigger : CrimeNetworkMissionTrigger {
        public ParkingClockMissionTrigger(int id, CrimeAction type, CrimeNetworkPillar pillar, float amount, TimeSpan timeConstraint, Position location, float radius) : base(id, type, pillar, amount, timeConstraint, location, radius) { }

        public override bool onTriggerProgressStep(IPlayer player, string name, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress) {
            var clockAmount = 0;
            if(currentProgress.has("ClockAmount")) {
                clockAmount = int.Parse(currentProgress.get("ClockAmount"));
            }
            
            currentProgress.set("ClockAmount", (clockAmount + amount).ToString());
            if(amount + clockAmount < Amount) {
                player.sendNotification(Constants.NotifactionTypes.Info, $"{name}: Du hast {(int)(clockAmount + amount)}/{(int)Amount} der für deinen Auftrag benötigten Parkuhren geknackt!", $"{(int)(clockAmount + amount)}/{(int)Amount} Parkuhren", Constants.NotifactionImages.Thief);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast genügend Parkuhren geknackt! Kehre zu einem Auftragssteller zurück!", $"Auftrag abgeschlossen", Constants.NotifactionImages.Thief);
                currentProgress.Status = CrimeMissionProgressStatus.Completed;
            }

            return true;
        }

        protected override List<MenuItem> getMenuItemInfo(IPlayer player, CrimeMissionProgress currentProgress) {
            var list = new List<MenuItem>();
            
            var clockAmount = 0;
            if(currentProgress.has("ClockAmount")) {
                clockAmount = int.Parse(currentProgress.get("ClockAmount"));
            }

            list.Add(new StaticMenuItem("Geknackte Parkuhren", $"Du hast {clockAmount}/{(int)Amount} der für deinen Auftrag benötigten Parkuhren geknackt!", $"{clockAmount}/{(int)Amount}"));

            return list;
        }

        public override string getName() {
            return "Parkuhren aufbrechen";
        }

        public override string getPillarReputationName() {
            if(Position != Position.Zero || TimeConstraint != TimeSpan.Zero) {
                return "BREAKING_CLOCK_MISSION_VARIATIONS";
            } else {
                return "BREAKING_CLOCK_MISSIONS";
            }
        }

        protected override string getPlayerSelectNotificationMessage(IPlayer player) {
            return $"\"Parkuhr aufbrechen\" Auftrag erhalten. Knacke {Amount} Parkuhren auf!";
        }
    }
}