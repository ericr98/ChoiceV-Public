using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Bogus.DataSets;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class DamageDummyPlaceable : PlaceableObject {
        private string Model { get => (string)Data["Model"]; set { Data["Model"] = value; } }
        private string DummyModelShortSave { get => Data.hasKey("ShortSave") ? (string)Data["ShortSave"] : ""; set { Data["ShortSave"] = value; } }

        private DamageDummy DummyModel;

        public DamageDummyPlaceable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public DamageDummyPlaceable(IPlayer player, Position playerPosition, Rotation playerRotation, string model) : base(playerPosition, playerRotation, 2f, 2f, false, new Dictionary<string, dynamic>()) {
            Model = model;
        }

        public override void initialize(bool register = true) {
            var d = (DegreeRotation)Rotation;
            CollisionShape.HasNoHeight = true;
            Object = ObjectController.createObject(Model, Position, d, 100, true);

            if (register) {
                DummyModel = DamageDummyController.createDamageDummy();
            } else {
                DummyModel = DamageDummyController.createDamageDummy(DummyModelShortSave);
            }

            DummyModel.OnDummyUpdateDelegate += (d) => DummyModelShortSave = d.getShortSave();

            base.initialize(register);
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var data = new Dictionary<string, dynamic> {
                {"placeable", this }
            };

            var menu = new Menu("Medizinische Puppe", "Was möchtest du tun?");

            var dummyMenu = DummyModel.getInteractionMenu(player);
            menu.addMenuItem(new MenuMenuItem(dummyMenu.Name, dummyMenu));

            if(player.getInventory().hasItem<Stretcher>(i => true)) {
                menu.addMenuItem(new ClickMenuItem("Auf Trage legen", "Lege die medizinische Puppe auf die Trage", "", "PICK_DUMMY_TO_STRETCHER")
                    .withData(data)
                    .needsConfirmation("Puppe auf Trage tragen?", "Puppe wirklich tragen?"));
            }
            menu.addMenuItem(new ClickMenuItem("Über Schulter tragen", "Trage die Puppe über der Schulter", "", "PICK_DUMMY_TO_SHOULDER")
                .withData(data)
                .needsConfirmation("Puppe tragen?", "Puppe wirklich tragen?"));
            menu.addMenuItem(new ClickMenuItem("Aufheben", "Lege die medizinische Puppe wieder in dein Inventar", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(data));

            return menu;
        }

        public override bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) {
            var cfg = InventoryController.getConfigItemForType<MedicDummyItem>();

            return player.getInventory().addItem(new MedicDummyItem(cfg, 1, -1));
        }
    }
}
