using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {

    public class ProcessingCampFire : TimeLimitProcessingObject {

        public ProcessingCampFire(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) {

        }

        public ProcessingCampFire(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(model, placeableItem, player, playerPosition, playerRotation, TimeSpan.FromMinutes(15)) {
            ProcessingType = ProcessingRecipePartTypes.CampFire;
        }

        public override void initialize(bool register = true) {
            Object = ObjectController.createObject("prop_hobo_stove_01", Position, new DegreeRotation(0, 0, 0), 200, true);
            if(register) {
                Inventory = InventoryController.createInventory(Id, 10, InventoryTypes.ProcessingObject);
            }
            base.initialize(register);
        }

        public override bool checkForFuel() {
            var fireWoordItem = Inventory.getItem(i => i.Name == "Feuerholz");
            if(fireWoordItem != null) {
                Inventory.removeItem(fireWoordItem, 1);
                return true;
            } else {
                return false;
            }
        }
    }
}
