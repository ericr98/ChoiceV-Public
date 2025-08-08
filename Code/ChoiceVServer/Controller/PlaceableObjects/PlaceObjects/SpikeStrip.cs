using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class SpikeStrip : PlaceableObject {
        private int ConfigId { get => (int)Data["ConfigId"]; set { Data["ConfigId"] = value; } }
        public SpikeStrip(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public SpikeStrip(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(playerPosition, playerRotation, 5f, 4f, true, new Dictionary<string, dynamic>()) {
            ConfigId = placeableItem.ConfigId;
        }

        public override void initialize(bool register = true) {
            Object = ObjectController.createObject("p_ld_stinger_s", Position, Rotation, 200, false);

            base.initialize(register);
        }

        public override void onEntityEnterShape(CollisionShape shape, IEntity entity) {
            base.onEntityEnterShape(shape, entity);
            if(entity.Type == BaseObjectType.Vehicle) {
                var vehicle = (ChoiceVVehicle)entity;

                vehicle.burstTyre(0);
                vehicle.burstTyre(1);
            }
        }

        public override void onEntityExitShape(CollisionShape shape, IEntity entity) {
            base.onEntityExitShape(shape, entity);

            if(entity.Type == BaseObjectType.Vehicle) {
                var vehicle = (ChoiceVVehicle)entity;

                vehicle.burstTyre(2);
                vehicle.burstTyre(3);
                vehicle.burstTyre(4);
                vehicle.burstTyre(5);
            }
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var menu = new Menu("Nagelband", "Was möchtest du tun?");
            var data = new Dictionary<string, dynamic> {
                {"placeable", this }
            };

            var companies = CompanyController.getCompanies(player);
            var policeCheck = companies.FirstOrDefault(c => c.hasFunctionality<PoliceFunctionality>());
            if(policeCheck != null) {
                menu.addMenuItem(new ClickMenuItem("Aufheben", "Lege das Nagelband wieder in dein Inventar", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(data));
            }

            menu.addMenuItem(new ClickMenuItem("Zerstören", "Zerstöre das Nagelband", "", "DESTROY_PLACABLE", MenuItemStyle.red).withData(data));

            return menu;
        }

        public override bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) {
            var configItem = InventoryController.getConfigById(ConfigId);
            return player.getInventory().addItem(new PlaceableObjectItem(configItem));
        }

        public override TimeSpan getAutomaticDeleteTimeSpan() {
            return TimeSpan.FromDays(2);
        }
    }
}
