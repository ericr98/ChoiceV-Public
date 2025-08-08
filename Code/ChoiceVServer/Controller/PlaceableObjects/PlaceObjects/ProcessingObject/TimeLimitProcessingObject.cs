using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class TimeLimitProcessingObject : ProcessingObject {
        public DateTime RunoutDate { get => (DateTime)Data["RunoutDate"]; set { Data["RunoutDate"] = value; } }
        public TimeSpan FuelAdvanceTime;
        public TimeLimitProcessingObject(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) {

        }

        public override void onInterval(TimeSpan tickLength) {
            if(RunoutDate <= DateTime.Now) {
                if(checkForFuel()) {
                    RunoutDate += FuelAdvanceTime;
                } else {
                    onFuelRunOut();
                }
            }

        }
        public TimeLimitProcessingObject(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation, TimeSpan fuelAdvanceTime) : base(model, placeableItem, player, playerPosition, playerRotation) {
            FuelAdvanceTime = fuelAdvanceTime;
            RunoutDate = DateTime.Now + FuelAdvanceTime;
        }

        public virtual bool checkForFuel() {
            return false;
        }

        public virtual void onFuelRunOut() {
            onRemove();
        }

        private string getFuelRestTimeString(ref MenuItemStyle style) {
            var num = Math.Round((RunoutDate - DateTime.Now).TotalMinutes);
            if(num > 5) {
                style = MenuItemStyle.green;
                return num.ToString() + "min";
            } else {
                style = MenuItemStyle.red;
                return "weniger als 5min!";
            }
        }
        public override Menu getLayerMenu() {
            var menu = base.getLayerMenu();
            var style = MenuItemStyle.normal;
            var time = getFuelRestTimeString(ref style);
            menu.insertMenuItem(new StaticMenuItem("Objektdauer", $"Das Objekt hält noch {time}", $"{time}", style));
            return menu;
        }
    }
}
