using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using ChoiceVServer.Model.Database;
using NLog.Targets;
using System.Collections;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public enum FoldTableVisualCategory {
        None = 0,
        Lab = 1,
    }

    public class FoldingTableController : ChoiceVScript {
        public FoldingTableController() {
            EventController.addMenuEvent("FOLDING_TABLE_PUT_UTENSIL", onFoldingTablePutUtensil);
            EventController.addMenuEvent("FOLDING_TABLE_PULL_UTENSIL", onFoldingTablePullUtensil);

            EventController.addMenuEvent("FOLDING_TABLE_OPEN_INVENTORY", onFoldingTableOpenInventory);
        }

        private bool onFoldingTablePutUtensil(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");

            AnimationController.animationTask(player, anim, () => {
                var table = (FoldingTable)data["Table"];
                var utensil = (FoldTableUtensil)data["Utensil"];

                if(player.getInventory().moveItem(table.UtensilInventory, utensil)) {
                    table.updateUtensilView();
                    player.sendNotification(Constants.NotifactionTypes.Success, $"{utensil.Name} erfolgreich auf den Tisch gestellt!", "Utensil auf Tisch gestellt");
                } else {
                    player.sendBlockNotification("Etwas ist schiefgelaufen!", "");
                }
            }, null, true, 1);

            return true;
        }

        private bool onFoldingTablePullUtensil(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = AnimationController.getAnimationByName("TAKE_STUFF");

            AnimationController.animationTask(player, anim, () => {
                var table = (FoldingTable)data["Table"];
                var utensil = (FoldTableUtensil)data["Utensil"];

                if(table.UtensilInventory.moveItem(player.getInventory(), utensil)) {
                    table.updateUtensilView();
                    player.sendNotification(Constants.NotifactionTypes.Success, $"{utensil.Name} erfolgreich von Tisch genommen!", "Utensil von Tisch genommen");
                } else {
                    player.sendBlockNotification("Du hast keinen Platz im Inventar, oder ein anderer Fehler ist aufgetreten!", "Fehlgeschlagen");
                }
            }, null, true, 1);

            return true;
        }

        private bool onFoldingTableOpenInventory(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var table = (FoldingTable)data["Table"];

            InventoryController.showMoveInventory(player, player.getInventory(), table.ItemInventory, (p, _, _, _, _) => {
                var anim = AnimationController.getAnimationByName("TAKE_STUFF");
                player.playAnimation(anim);
                return true;
            }, null, "Klapptisch Ablage", true);

            return true;
        }

    }

    public class FoldingTable : PlaceableObject {
        private int ConfigId { get => (int)Data["ConfigId"]; set { Data["ConfigId"] = value; } }
        public Inventory ItemInventory { get => InventoryController.loadInventory((int)Data["ItemInventoryId"]); set { Data["ItemInventoryId"] = value.Id; } }
        public Inventory UtensilInventory { get => InventoryController.loadInventory((int)Data["UtensilInventoryId"]); set { Data["UtensilInventoryId"] = value.Id; } }

        public FoldingTable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) {
            IntervalPlaceable = true;
        }

        public FoldingTable(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(playerPosition, playerRotation, 4f, 3f, true, new Dictionary<string, dynamic>()) {
            IntervalPlaceable = true;

            ConfigId = placeableItem.ConfigId;

            ItemInventory = InventoryController.createInventory(placeableItem.Id ?? -1, 8, InventoryTypes.FoldingTable);
            UtensilInventory = InventoryController.createInventory(placeableItem.Id ?? -1, 1000, InventoryTypes.FoldingTable);
        }

        public override void initialize(bool register = true) {
            spawnProp(getVisualData());

            base.initialize(register);
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var menu = new Menu("Klapptisch", "Was möchtest du tun?");

            var utensilMenu = new Menu("Utensilien benutzen", "Benutze die Utensilien auf dem Tisch");
            foreach(var utensil in UtensilInventory.getItems<FoldTableUtensil>(u => true)) {
                var virtMenu = new VirtualMenu(utensil.Name, () => {
                    var men = utensil.getOnTableMenu(player, this);

                    if(utensil.canBePulledOff(this)) {
                        men.addMenuItem(new ClickMenuItem("Utensilie herunternehmen", "Nimm die Untensilie zurück in dein Inventar", "", "FOLDING_TABLE_PULL_UTENSIL", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Table", this }, { "Utensil", utensil } }).needsConfirmation($"{utensil.Name} von Tisch nehmen?", "Wirklich von Tisch nehmen?"));
                    } else {
                        men.addMenuItem(new StaticMenuItem("Herunternehmen nicht möglich!", "Die Utensilie kann aktuell nicht heruntergenommen werden!", "", MenuItemStyle.yellow));
                    }
                    return men;
                });
                utensilMenu.addMenuItem(new MenuMenuItem(virtMenu.Name, virtMenu));
            }
            menu.addMenuItem(new MenuMenuItem(utensilMenu.Name, utensilMenu));

            menu.addMenuItem(new ClickMenuItem("Ablage öffnen", "Öffne die Ablage des Tisches. Lege zu verwendendete Items dort hin", "", "FOLDING_TABLE_OPEN_INVENTORY").withData(new Dictionary<string, dynamic> { { "Table", this } }));

            var putMenu = new Menu("Utensilien draustellen", "Stelle Utensilien auf den Tisch");
            if(UtensilInventory.getCount() < 8) {
                foreach(var item in player.getInventory().getItems<FoldTableUtensil>(u => true)) {
                    putMenu.addMenuItem(new ClickMenuItem($"{item.Name} raufstellen", $"Stelle ein/eine {item.Name} auf den Tisch: Die Beschreibung ist: {item.Description}", "", "FOLDING_TABLE_PUT_UTENSIL").withData(new Dictionary<string, dynamic> { { "Table", this }, { "Utensil", item } }).needsConfirmation($"{item.Name} auf Tisch legen?", "Wirklich auf Tisch legen?"));
                }
            } else {
                putMenu.addMenuItem(new StaticMenuItem("Nicht mehr möglich!", "Es ist nur Platz für insgesamt 8 Utensilien!", "max. 8", MenuItemStyle.yellow));
            }
            menu.addMenuItem(new MenuMenuItem(putMenu.Name, putMenu));
            if(ItemInventory.getCount() <= 0 && UtensilInventory.getCombinedWeight() <= 0) {
                var data = new Dictionary<string, dynamic> {
                    {"placeable", this }
                };

                menu.addMenuItem(new ClickMenuItem("Tisch zusammenklappen", "Klappe den Tisch zusammen und packe ihn ein", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(data));
            } else {
                menu.addMenuItem(new StaticMenuItem("Aufnehmen nicht möglich!", "Es befinden sich noch Objekte auf dem Tisch. Er kann nicht zusammengeklappt werden!", "TODO", MenuItemStyle.yellow));
            }

            return menu;
        }

        public void updateUtensilView() {
            var data = getVisualData();

            if(Object.ModelName != data.PropName) {
                ObjectController.deleteObject(Object);
                spawnProp(data);
            }
        }

        private void spawnProp(VisualData data) {
            var d = (DegreeRotation)Rotation;
            Object = ObjectController.createObject(data.PropName, Position, d, 200, true);
        }

        private record VisualData(string PropName);
        private VisualData getVisualData() {
            var dic = new Dictionary<FoldTableVisualCategory, int>();

            foreach(var item in UtensilInventory.getItems<FoldTableUtensil>(u => true)) {
                var cat = item.getVisualCategory();
                if(cat == FoldTableVisualCategory.None) {
                    continue;
                }

                if(dic.ContainsKey(cat)) {
                    dic[cat]++;
                } else {
                    dic[cat] = 1;
                }
            }

            if(dic.Count > 0) {
                var current = dic.First();
                foreach(var d in dic) {
                    if(d.Value > current.Value) {
                        current = d;
                    }
                }

                switch(current.Key) {
                    case FoldTableVisualCategory.Lab:
                        return new VisualData("v_ret_ml_tableb");
                }
            }

            return new VisualData("h4_prop_h4_table_isl_01a");
        }

        public List<Item> getContributedItems(FoldTableUtensil ignore = null) {
            return ItemInventory.getAllItems().Concat(UtensilInventory.getItems<FoldTableUtensil>(u => true).SelectMany(u => u != ignore ? u.getContributedItems() : new List<Item>())).ToList();
        }

        public bool hasTableFunctionality(string functionality) {
            return UtensilInventory.getItems<FoldTableUtensil>(u => true).Any(u => u.getContributedFunctionalities().Contains(functionality));
        }

        public override void onInterval(TimeSpan tickLength) {
            UtensilInventory.getItems<FoldTableUtensil>(i => true).ForEach(u => u.onTick(this, tickLength));
        }

        public override void onRemove() {
            InventoryController.destroyInventory(ItemInventory);
            InventoryController.destroyInventory(UtensilInventory);
            
            base.onRemove();
        }
    }
}
