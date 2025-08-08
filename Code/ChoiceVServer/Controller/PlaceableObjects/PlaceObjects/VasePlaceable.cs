using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoiceVServer.Base;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class VasePlaceable : PlaceableObject {
        public VasePlaceable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public VasePlaceable(Position playerPosition, Rotation playerRotation, string modelName) : base(playerPosition, playerRotation, 2f, 2f, true, new Dictionary<string, dynamic>()) {
            ModelName = modelName;
        }

        public override void initialize(bool register = true) {
            var d = (DegreeRotation)Rotation;
            Object = ObjectController.createObject(ModelName, Position, d, 200, true);

            base.initialize(register);
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var menu = new Menu("Blumenvase", "Was möchtest du tun?");
            var data = new Dictionary<string, dynamic> {
                {"placeable", this }
            };

            menu.addMenuItem(new ClickMenuItem("Aufheben", "Lege die Blumenwase in dein Inventar.", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(data));

            return menu;
        }

        public override bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) {
            var configItem = InventoryController.getConfigItemForType<VaseFlower>();
            var item = new Vase(configItem, ModelName);
            item.updateDescription();

            return player.getInventory().addItem(item);
        }

        public override TimeSpan getAutomaticDeleteTimeSpan() {
            return TimeSpan.FromDays(10);
        }
    }
}
