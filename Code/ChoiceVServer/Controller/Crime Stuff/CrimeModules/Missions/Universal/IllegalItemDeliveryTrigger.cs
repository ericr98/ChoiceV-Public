using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public record IllegalItemDeliveryObjective(bool SellNotBuy, int ToSellAmount, int? ConfigItemId, string ZoneGroup);

    public class IllegalItemDeliveryTrigger : CrimeNetworkMissionTrigger {
        private List<IllegalItemDeliveryObjective> Objectives;
        
        public IllegalItemDeliveryTrigger(int id, CrimeAction type, CrimeNetworkPillar pillar, float amount, TimeSpan timeConstraint, Position location, float radius) : base(id, type, pillar, amount, timeConstraint, location, radius) { }
        
        protected override void afterSetSettings() {
            if(Settings.ContainsKey("Objectives")) {
                Objectives = Settings["Objectives"].FromJson<List<IllegalItemDeliveryObjective>>();
            }
        }

        public override bool onTriggerProgressStep(IPlayer player, string name, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress) {
            var item = (configitem)data["ConfigItem"];
            if(item == null) {
                return false;
            }

            var position = (Position)data["Position"];
            var sellNotBuy = (bool)data["SellNotBuy"];
            var bigZone = WorldController.getBigRegionName(position);

            var objective = Objectives.FirstOrDefault(o => o.SellNotBuy == sellNotBuy && (o.ZoneGroup == null || o.ZoneGroup == bigZone) && (o.ConfigItemId == null || o.ConfigItemId == item.configItemId));
            if(objective != null) {
                var sellOrBuyStr = sellNotBuy ? "geliefert" : "besorgt"; 

                var idx = Objectives.IndexOf(objective);
                var progressArray = new int[Objectives.Count]; 
                if(currentProgress.has("Progress")) {
                    progressArray = currentProgress.get("Progress").FromJson<int[]>();
                } 
                  
                var (itemNameStr, zoneStr) = getObjectiveInfo(idx);
                progressArray[idx] += (int)amount;
                currentProgress.set("Progress", progressArray.ToJson());

                if(progressArray[idx] < objective.ToSellAmount) {
                    player.sendNotification(Constants.NotifactionTypes.Info, $"{name}: Du hast {progressArray[idx]}/{objective.ToSellAmount} {itemNameStr} der für deinen Auftrag {zoneStr} benötigten {itemNameStr} {sellOrBuyStr}!", $"{progressArray[idx]}/{objective.ToSellAmount} {sellOrBuyStr}", Constants.NotifactionImages.Thief);
                } else {
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast genügend {itemNameStr} {zoneStr} {sellOrBuyStr}!", $"Teilauftrag abgeschlossen", Constants.NotifactionImages.Thief);

                    var all = true;
                    for(var i = 0; i < Objectives.Count; i++) {
                        var obj = Objectives[i];
                        if(progressArray[i] < obj.ToSellAmount) {
                            all = false;
                            break;
                        }
                    }
                    
                    if(all) {
                        player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast alle Teilaufträge abgeschlossen! Kehre zum Auftragsteller zurück!", $"Mission abgeschlossen", Constants.NotifactionImages.Thief);
                        currentProgress.Status = CrimeMissionProgressStatus.Completed;
                    }
                }

                return true;
            } else {
                return false;
            }
        }

        protected override List<MenuItem> getMenuItemInfo(IPlayer player, CrimeMissionProgress currentProgress) {
            var list = new List<MenuItem>();

            var progressArray = new int[Objectives.Count]; 
            if(currentProgress.has("Progress")) {
                progressArray = currentProgress.get("Progress").FromJson<int[]>();
            }
            
            for(var i = 0; i < Objectives.Count; i++) {
                var objective = Objectives[i];
                var (itemNameStr, zoneStr) = getObjectiveInfo(i);

               if(objective.SellNotBuy) {
                list.Add(new StaticMenuItem($"{itemNameStr} ausliefern", $"Liefere {objective.ToSellAmount} {itemNameStr} {zoneStr} aus! Du hast aktuell {progressArray[i]}/{objective.ToSellAmount} davon ausgeliefert.", $"{progressArray[i]}/{objective.ToSellAmount}"));
               } else {
                list.Add(new StaticMenuItem($"{itemNameStr} besorgen", $"Besorge {objective.ToSellAmount} {itemNameStr} {zoneStr}! Du hast aktuell {progressArray[i]}/{objective.ToSellAmount} davon besorgt.", $"{progressArray[i]}/{objective.ToSellAmount}"));
               }
            }

            return list;
        }
        
        private (string itemName, string zoneName) getObjectiveInfo(int idx) {
            var objective = Objectives[idx];
            var itemNameStr = "Waren";
            if(objective.ConfigItemId != null) {
                var item = InventoryController.getConfigById((int)objective.ConfigItemId);
                itemNameStr = item.name;
            }

            var zoneStr = "egal wo";
            if(objective.ZoneGroup != null) {
                var zoneName = WorldController.getBigRegionDisplayName(objective.ZoneGroup);
                zoneStr = $"in {zoneName}";
            }

            return (itemNameStr, zoneStr);
        }

        public override string getName() {
            return "Illegale Waren ausliefern";
        }

        public override string getPillarReputationName() {
            if(Position != Position.Zero || TimeConstraint != TimeSpan.Zero) {
                return "ILLEGALE_ITEM_DELIVERY_MISSION_VARIATIONS";
            } else {
                return "ILLEGALE_ITEM_DELIVERY_MISSIONS";
            }
        }

        protected override string getPlayerSelectNotificationMessage(IPlayer player) {
            return $"\"Illegale Waren Lieferung\" Auftrag erhalten. Liefere/besorge illegale Waren aus!";
        }

        #region Create Stuff

        public override Dictionary<string, dynamic> getSettingsFromMenuStats(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
            var list = new List<IllegalItemDeliveryObjective>(); 
            for (var i = 0; i < 10 * 4; i += 4) {
                var sellNotBuy = evt.elements[i].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
                var item = evt.elements[i + 1].FromJson<InputMenuItem.InputMenuItemEvent>();
                var amount = evt.elements[i + 2].FromJson<InputMenuItem.InputMenuItemEvent>();
                var zone = evt.elements[i + 3].FromJson<InputMenuItem.InputMenuItemEvent>();

                if(sellNotBuy == null || item == null || amount == null || zone == null) {
                    break;
                }

                if(amount.input == "" || zone.input == "") {
                    continue;
                }

                list.Add(new IllegalItemDeliveryObjective (
                    sellNotBuy.check,
                    int.Parse(amount.input),
                    InventoryController.getConfigItemFromSelectMenuItemInput(item.input)?.configItemId,
                    WorldController.getBigRegionIdentifierFromSelectMenuItemInput(zone.input)
                ));
                
                if(evt.elements.Length <= i + 4 || evt.elements[i + 4] == null || evt.elements[i + 4] == "null" || evt.elements[i + 4] == "") {
                    break;
                }
            }
            
            return new Dictionary<string, dynamic> { { "Objectives", list.ToJson() } };
        }

        public override List<MenuItem> getCreateListMenuItems() {
            var list = new List<MenuItem>();

            for(var i = 0; i < Objectives.Count; i++) {
                var menu = new Menu($"Teilauftrag: {i}", "Siehe dir die Daten ein");
                
                menu.addMenuItem(new StaticMenuItem("Anzahl", Objectives[i].ToSellAmount.ToString(), Objectives[i].ToSellAmount.ToString()));
                
                var item = InventoryController.getConfigById(Objectives[i].ConfigItemId ?? -1);
                menu.addMenuItem(new StaticMenuItem("Zu beschaffenes/verkaufendes Item", item?.name ?? "Egal", ""));
                
                var zone = WorldController.getBigRegionDisplayName(Objectives[i].ZoneGroup);
                menu.addMenuItem(new StaticMenuItem("Zone", zone, zone));
                
                list.Add(new MenuMenuItem(menu.Name, menu));
            }
            
            return list;
        }

        #endregion
    }
}