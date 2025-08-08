using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class SimplePlaceable : PlaceableObject {
        private int ConfigId { get => (int)Data["ConfigId"]; set { Data["ConfigId"] = value; } }

        public SimplePlaceable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public SimplePlaceable(IPlayer player, Position playerPosition, Rotation playerRotation, string modelName, int configId, float colDimension) : base(playerPosition, playerRotation, colDimension, colDimension, true, new Dictionary<string, dynamic>()) {
            ModelName = modelName;
            ConfigId = configId;
        }

        public override void initialize(bool register = true) {
            var d = (DegreeRotation)Rotation;

            //Position given is player position
            Object = ObjectController.createObject(ModelName, Position, d, 200, true);

            base.initialize(register);
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var name = PlaceableObjectsController.getPlaceableObjectMenuName(this);
            var menu = new Menu(name, "Was möchtest du tun?");
            var data = new Dictionary<string, dynamic> {
                {"placeable", this }
            };

            menu.addMenuItem(new ClickMenuItem("Aufheben", "Lege das Objekt wieder in dein Inventar", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(data));
            menu.addMenuItem(new ClickMenuItem("Zerstören", "Zerstöre das Objekt", "", "DESTROY_PLACABLE", MenuItemStyle.red).withData(data));

            return menu;
        }

        public override bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) {
            return player.getInventory().addItem(new PlaceableObjectItem(InventoryController.getConfigById(ConfigId)));
        }
    }
}
