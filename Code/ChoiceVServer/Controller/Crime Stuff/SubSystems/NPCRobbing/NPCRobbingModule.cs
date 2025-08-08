using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class NPCRobbingModule : NPCModule {
        private decimal MaximalMonetaryValue;
        private int MaximalMonetaryRefillInHours;

        public NPCRobbingModule(ChoiceVPed ped) : base(ped) { }
        public NPCRobbingModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
            MaximalMonetaryValue = Convert.ToDecimal(settings["MaximalMonetaryValue"]);
            MaximalMonetaryRefillInHours = Convert.ToInt32(settings["MaximalMonetaryRegainInHours"]);
        }

        public NPCRobbingModule(ChoiceVPed ped, decimal maximalMonetaryValue, int maximalMonetaryRefillInHours) : base(ped) {
            MaximalMonetaryValue = maximalMonetaryValue;
            MaximalMonetaryRefillInHours = maximalMonetaryRefillInHours;
        }
        
        public override List<MenuItem> getMenuItems(IPlayer player) {
            if(Ped.IsBeingThreatend) {
                return [
                    new ClickMenuItem("Person ausrauben", "Raube diese Person aus.", "", "ON_ROB_NPC", MenuItemStyle.yellow)
                    .needsConfirmation("Person ausrauben?", "Diese Person wirklich ausrauben?")
                    .withData(new Dictionary<string, dynamic>{
                        { "Module", this }
                    })
                ];
            } else {
                return [];
            }
        }

        public decimal getCurrentMonetaryValue() {
            var lastRobbed = DateTime.MinValue;
            if(Ped.Data.hasKey("LastRobbed")) {
                lastRobbed = Ped.Data["LastRobbed"];
            }

            var difference = DateTime.Now - lastRobbed;
            return Convert.ToDecimal(Math.Max(0, (double)MaximalMonetaryValue * Math.Min(1, difference.TotalHours / MaximalMonetaryRefillInHours)));
        }

        public ChoiceVPed getPed() {
            return Ped;
        }

        public void pedWasJustRobbed() {
            Ped.Data["LastRobbed"] = DateTime.Now;
        }

        public override void onRemove() { }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("NPC-Ausraubbar-Modul", "Macht den NPC ausraubbar. Dies gewährt Items und Bargeld", "");
        }

        public override Dictionary<string, dynamic> getSettings() {
            return new Dictionary<string, dynamic> {
                { "MaximalMonetaryValue", MaximalMonetaryValue },
                { "MaximalMonetaryRegainInHours", MaximalMonetaryRefillInHours }
            };
        }
    }
}