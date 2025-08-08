using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Crime_Stuff {
     public class NPCRobbingMissionTrigger : CrimeNetworkMissionTrigger {
        private string ZoneGroup;

        public NPCRobbingMissionTrigger(int id, CrimeAction type, CrimeNetworkPillar pillar, float amount, TimeSpan timeConstraint, Position location, float radius) : base(id, type, pillar, amount, timeConstraint, location, radius) { }

        protected override void afterSetSettings() {
           if(Settings.ContainsKey("ZoneGroup")) {
               ZoneGroup = Settings["ZoneGroup"];
           }
        }

        public override bool onTriggerProgressStep(IPlayer player, string name, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress) {
            if(ZoneGroup != null) {
                if(WorldController.getBigRegionName(player.Position) != ZoneGroup) {
                    return false;
                }
            }

            var robbedAmount = 0;
            if(currentProgress.has("RobbedAmount")) {
                robbedAmount = int.Parse(currentProgress.get("RobbedAmount"));
                robbedAmount += (int)amount;
                currentProgress.set("RobbedAmount", robbedAmount.ToString());
            } else {
                currentProgress.set("RobbedAmount", amount.ToString());
            }

            currentProgress.set("RobbedAmount", (robbedAmount + amount).ToString());
            if (amount + robbedAmount < Amount) {
                player.sendNotification(Constants.NotifactionTypes.Info, $"{name}: Du hast ${robbedAmount + amount}/${Amount} der für deinen Auftrag benötigten Geldwert erbeutet!", $"${robbedAmount + amount}/${Amount} ausgeraubt", Constants.NotifactionImages.Thief);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast genügend Geldwert geraubt. Begib dich zurück zu einem Auftraggeber!", $"Auftrag abgeschlossen", Constants.NotifactionImages.Thief);
                currentProgress.Status = CrimeMissionProgressStatus.Completed;
            }

            return true;
        }

        protected override List<MenuItem> getMenuItemInfo(IPlayer player, CrimeMissionProgress currentProgress) {
            var zoneStr = "irgendwo";
            if(ZoneGroup != null) {
                zoneStr = $"in {WorldController.getBigRegionDisplayName(ZoneGroup)}";
            }

            return [
                new StaticMenuItem("Geldwert geraubt", $"Du hast ${currentProgress.get("RobbedAmount")}/{Amount} der für deinen Auftrag benötigten Geldwert {zoneStr} erbeutet!", $"${currentProgress.get("RobbedAmount")}/{Amount}")
            ];
        }

        public override string getName() {
            return "Personen ausrauben";
        }

        public override string getPillarReputationName() {
            if(Position != Position.Zero || TimeConstraint != TimeSpan.Zero) {
                return "NPC_ROBBING_MISSIONS_VARATIONS";
            } else {
                return "NPC_ROBBING_MISSIONS";
            }
        }

        protected override string getPlayerSelectNotificationMessage(IPlayer player) {
            var zoneStr = "irgendwo";
            if(ZoneGroup != null) {
                zoneStr = $"in {WorldController.getBigRegionDisplayName(ZoneGroup)}";
            }
            return $"Raube Personen für einen Gesamtwert von {Amount} {zoneStr} aus!";
        }

        #region Create Stuff

        public override Dictionary<string, dynamic> getSettingsFromMenuStats(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
           var zoneGroup = evt.elements[0].FromJson<ListMenuItem.ListMenuItemEvent>();  

           return new Dictionary<string, dynamic> {
               { "ZoneGroup", zoneGroup.currentElement }
           };
        }

        #endregion
    }
}