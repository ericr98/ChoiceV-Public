using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class StealVehiclePartMissionTrigger : CrimeNetworkMissionTrigger {
        private List<int> RequiredVehicleClasses;

        public StealVehiclePartMissionTrigger(int id, CrimeAction type, CrimeNetworkPillar pillar, float amount, TimeSpan timeConstraint, Position location, float radius) : base(id, type, pillar, amount, timeConstraint, location, radius) {}

        protected override void afterSetSettings() {
           if(Settings.TryGetValue("VehicleClasses", out string value)) {
                RequiredVehicleClasses = value.FromJson<List<int>>();
           }
        }

        public override string getName() {
            return "Fahrzeugteile stehlen";
        }

        public override string getPillarReputationName() {
           if(RequiredVehicleClasses == null) {
                return "STEAL_VEHICLE_PART_MISSIONS";
           } else {
                return "STEAL_VEHICLE_PART_MISSIONS_VARIATIONS";
           }
        }

        public override bool onTriggerProgressStep(IPlayer player, string name, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress) {
            var veh = (ChoiceVVehicle)data["Vehicle"];
            var part = (VehicleMods)data["Part"];
            var item = (VehicleTuningItem)data["Item"];

            if(RequiredVehicleClasses != null && !RequiredVehicleClasses.Contains(veh.VehicleClassId)) {
                return false;
            }

            var stolenAmount = 0;
            if(currentProgress.has("StolenAmount")) {
                stolenAmount = int.Parse(currentProgress.get("StolenAmount"));
                stolenAmount += (int)amount;
                currentProgress.set("StolenAmount", stolenAmount.ToString());
            } else {
                currentProgress.set("StolenAmount", amount.ToString());
            }
            currentProgress.set("StolenAmount", (stolenAmount + amount).ToString());

            if(amount + stolenAmount < Amount) {
                player.sendNotification(Constants.NotifactionTypes.Info, $"{name}: Du hast {stolenAmount + amount}/{Amount} der für deinen Auftrag benötigten Fahrzeugteile gestohlen!", $"{stolenAmount + amount}/{Amount} gestohlen", Constants.NotifactionImages.Thief);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast genügend Fahrzeugteile gestohlen. Begib dich zurück zu einem Auftraggeber!", "Auftrag abgeschlossen", Constants.NotifactionImages.Thief);
                currentProgress.Status = CrimeMissionProgressStatus.Completed;
            }

            return true;
        }

        protected override List<MenuItem> getMenuItemInfo(IPlayer player, CrimeMissionProgress currentProgress) {
            var list = new List<MenuItem> {
                new StaticMenuItem("Fahrzeugteile stehlen", $"Fahrzeugteile: {Amount}", Amount.ToString())
            };

            if(RequiredVehicleClasses != null) {
                var classStr = "";
                foreach(var vehicleClass in RequiredVehicleClasses) {
                    var className = VehicleController.getVehicleClassName(vehicleClass);
                    classStr += $"{className}, ";
                }
                classStr = classStr.Substring(0, classStr.Length - 2);

                list.Add(new StaticMenuItem("Mögliche Fahrzeugklassen", $"Fahrzeugklassen: {classStr}", classStr));
            }

            return list;
        }

        protected override string getPlayerSelectNotificationMessage(IPlayer player) {
            var classStr = "";

            if(RequiredVehicleClasses != null) {
                classStr = " für folgende Fahrzeugklassen: ";
                foreach(var vehicleClass in RequiredVehicleClasses) {
                    var className = VehicleController.getVehicleClassName(vehicleClass);
                    classStr += $"{className}, ";
                }
                classStr = classStr.Substring(0, classStr.Length - 2);
            }

            return $"Stehle {Amount} Fahrzeugteile{classStr}";
        }

        public override Dictionary<string, dynamic> getSettingsFromMenuStats(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
            var classesIds = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>().input;

            if (string.IsNullOrWhiteSpace(classesIds)) {
                return new Dictionary<string, dynamic>();
            } else {
                return new Dictionary<string, dynamic> {
                    { "VehicleClasses", classesIds.Split(",").Select(int.Parse).ToList().ToJson() },
                };
            }
        }
    }
}