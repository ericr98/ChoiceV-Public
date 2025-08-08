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
     public class StoreTheftMissionTrigger : CrimeNetworkMissionTrigger {
        private List<int> RequiredItems;

        public StoreTheftMissionTrigger(int id, CrimeAction type, CrimeNetworkPillar pillar, float amount, TimeSpan timeConstraint, Position location, float radius) : base(id, type, pillar, amount, timeConstraint, location, radius) { }

        protected override void afterSetSettings() {
            if(Settings.ContainsKey("RequiredItems")) {
                RequiredItems = Settings["RequiredItems"].FromJson<List<int>>();
            }
        }

        public override bool onTriggerProgressStep(IPlayer player, string name, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress) {
            var itemAmount = 0;
            if(currentProgress.has("ItemAmount")) {
                itemAmount = int.Parse(currentProgress.get("ItemAmount"));
            }
            
            var item = (configitem)data["Item"];
            if(RequiredItems == null || RequiredItems.Contains(item.configItemId)) {
                currentProgress.set("ItemAmount", (itemAmount + amount).ToString());
                
                if(amount + itemAmount < Amount) {
                    player.sendNotification(Constants.NotifactionTypes.Info, $"{name}: Du hast {(int)(itemAmount + amount)}/{(int)Amount} der für deinen Auftrag benötigten Waren gestohlen!", $"{(int)(itemAmount + amount)}/{(int)Amount} Waren", Constants.NotifactionImages.Thief);
                } else {
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast genügend Waren gestohlen! Kehre zu einem Auftragssteller zurück!", $"Auftrag abgeschlossen", Constants.NotifactionImages.Thief);
                    currentProgress.Status = CrimeMissionProgressStatus.Completed;
                }

                return true;
            } else {
                return false;
            }
        }

        protected override List<MenuItem> getMenuItemInfo(IPlayer player, CrimeMissionProgress currentProgress) {
            var list = new List<MenuItem>();
            
            var itemAmount = 0;
            if(currentProgress.has("ItemAmount")) {
                itemAmount = int.Parse(currentProgress.get("ItemAmount"));
            }

            list.Add(new StaticMenuItem("Waren stehlen", $"Du hast {itemAmount}/{(int)Amount} der für deinen Auftrag benötigten Waren gestohlen!", $"{itemAmount}/{(int)Amount}"));

            var viableMenu = new Menu("Akzeptierte Waren", "Diese Waren werden akzeptiert");
            foreach(var id in RequiredItems) {
                var cfg = InventoryController.getConfigById(id);

                viableMenu.addMenuItem(new StaticMenuItem(cfg.name, $"{cfg.name} können gestohlen werden.", ""));
            }
            list.Add(new MenuMenuItem(viableMenu.Name, viableMenu));

            return list;
        }

        public override string getName() {
            return "Waren stehlen";
        }

        public override string getPillarReputationName() {
            if(Position != Position.Zero || TimeConstraint != TimeSpan.Zero) {
                return "STORE_THEFT_MISSION_VARIATIONS";
            } else {
                return "STORE_THEFT_MISSIONS";
            }
        }

        protected override string getPlayerSelectNotificationMessage(IPlayer player) {
            var variableStr = "";
            if(RequiredItems != null) {
                variableStr = "Es handelt sich dabei um bestimmte Produkte!";
            }
            return $"\"Waren stehlen\" Auftrag erhalten. Stiel {Amount} Waren! {variableStr}";
        }

        #region Create Stuff

        public override Dictionary<string, dynamic> getSettingsFromMenuStats(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
            var itemsEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();

            if(itemsEvt.input != null && itemsEvt.input != "") {
                return new Dictionary<string, dynamic> { { "RequiredItems", itemsEvt.input.Split(',').Select(i => int.Parse(i)).ToList().ToJson() } };
            } else {
                return new Dictionary<string, dynamic>();
            }
        }

        #endregion
    }
}