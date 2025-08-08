using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class ContainerItemPlaceable : PlaceableObject {
        private Inventory Inventory { get => InventoryController.loadInventory((int)Data["InventoryId"]); set { Data["InventoryId"] = value.Id; } }

        public ContainerItemPlaceable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public ContainerItemPlaceable(ContainerItem placeableItem, Position frontPos, Rotation rotation) : base(frontPos, rotation, 3f, 3f, true, new Dictionary<string, dynamic>()) {
            Inventory = InventoryController.createInventory(placeableItem.Id ?? -1, 10000, InventoryTypes.PlaceableItemContainer);

            placeableItem.moveToInventory(Inventory, 1, true);
        }

        public override void initialize(bool register = true) {
            var item = getItem();
            Object = ObjectController.createObject(item.ModelString, Position, Rotation, 200, true);

            base.initialize(register);
        }

        internal ContainerItem getItem() {
            return Inventory.getItem<ContainerItem>(i => true);
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var containedItem = getItem();
            var amounts = containedItem.getAllSubItems();

            var menu = new Menu($"{containedItem.Name}", $"Insgesamt {amounts.Select(a => a.Amount).Aggregate((i, j) => i + j)} übrig");
            var data = new Dictionary<string, dynamic> {
                {"placeable", this }
            };

            if(containedItem != null) {
                var allSubs = containedItem.getAllSubItems();

                if(allSubs.Count > 1) {
                    foreach(var sub in allSubs) {
                        var subCfg = InventoryController.getConfigById(sub.ConfigId);
                        var subMenu = new Menu(subCfg.name, $"Noch {sub.Amount} übrig");

                        foreach(var el in containedItem.getMenuItemsForSubItem(subCfg)) {
                            subMenu.addMenuItem(el);
                        }

                        menu.addMenuItem(new MenuMenuItem($"{subCfg.name} ({sub.Amount}x)", subMenu));
                    }
                } else {
                    var cfg = InventoryController.getConfigById(allSubs.First().ConfigId);
                    foreach(var el in containedItem.getMenuItemsForSubItem(cfg)) {
                        menu.addMenuItem(el);
                    }
                }
            }

            if(player.getInventory().canFitItem(containedItem)) {
                menu.addMenuItem(new ClickMenuItem($"{containedItem.Name} aufnehmen", $"Lege {containedItem.Name} in dein Inventar", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(data));
            } else {
                menu.addMenuItem(new StaticMenuItem($"{containedItem.Name} aufnehmen nicht möglich", $"Du hast nicht genügend Platz um {containedItem.Name}  in dein Inventar zu nehmen", "", MenuItemStyle.yellow));
            }

            return menu;
        }

        public override bool onPickUp(IPlayer player, ref Constants.NotifactionImages img) {
            img = Constants.NotifactionImages.Package;
            var item = getItem();
            var worked = item.moveToInventory(player.getInventory());

            return worked;
        }

        public override void onRemove() {
            InventoryController.destroyInventory(Inventory);

            base.onRemove();
        }

        public override TimeSpan getAutomaticDeleteTimeSpan() {
            return TimeSpan.FromDays(3);
        }
    }
}
