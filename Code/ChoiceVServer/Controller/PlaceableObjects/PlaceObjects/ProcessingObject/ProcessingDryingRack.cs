using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {

    public class ProcessingDryingRack : ProcessingObject {
        public ProcessingDryingRack(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) {

        }

        public ProcessingDryingRack(string model,Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(model, placeableItem, player, playerPosition, playerRotation) {
            ProcessingType = ProcessingRecipePartTypes.DryingRack;
        }

        public override void initialize(bool register = true) {
            if(register) {
                Object = ObjectController.createObject("erk_dryingrack_a", Position - new Position(0, 0, 0.0f), Rotation, 200, true);
                Inventory = InventoryController.createInventory(Id, 10, InventoryTypes.ProcessingObject);
            } else {
                Object = ObjectController.createObject(ModelName, Position - new Position(0, 0, 0.0f), Rotation, 200, true);
            }
            base.initialize(register);
        }

        public override void resetModel() {
            base.resetModel();
            Object = ObjectController.createObject("erk_dryingrack_a", Position - new Position(0, 0, 0.0f), Rotation, 200, true);
            PlaceableObjectModelChange?.Invoke(this, "erk_dryingrack_a");
        }
    }
}
