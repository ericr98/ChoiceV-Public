using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Bogus.Extensions;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.InventorySystem {
    public class ContainerItemController : ChoiceVScript {
        public ContainerItemController() {
            EventController.addMenuEvent("TAKE_ITEM_FROM_CONTAINER", onTakeItemFromContainer);

            EventController.addMenuEvent("PLACE_CONTAINER_ITEM", onPlaceContainerItem);
        }

        private bool onTakeItemFromContainer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (ContainerItem)data["Item"];
            var configItemId = (int)data["ConfigItemId"];

            var anim = AnimationController.getAnimationByName("WORK_FRONT_SHORT");
            if(menuItemCefEvent is InputMenuItemEvent) {
                var input = ((InputMenuItemEvent)menuItemCefEvent).input;

                int amount = 0;
                if(input == null || input == "") {
                    amount = 1;
                }

                if((amount != 0 || int.TryParse(input, out amount)) && amount > 0) {
                    AnimationController.animationTask(player, anim, () => {
                        if(amount > item.getAmountForItem(configItemId)) {
                            item.takeItemsOut(player, configItemId, -1);
                        } else {
                            item.takeItemsOut(player, configItemId, amount);
                        }
                    });
                } else {
                    player.sendBlockNotification("Die Eingabe war keine gültige Zahl!", "Falsche Eingabe");
                }
            } else {
                AnimationController.animationTask(player, anim, () => {
                    item.takeItemsOut(player, configItemId, -1);
                });
            }

            return true;
        }

        private bool onPlaceContainerItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if (player.IsInVehicle) {
                player.sendBlockNotification("Im Fahrzeug nicht möglich!", "Nicht möglich", Constants.NotifactionImages.Car);
                return false;
            }

            var item = (ContainerItem)data["Item"];

            ObjectController.startObjectPlacerMode(player, item.ModelString, 0, (_, pos, h) => {
                var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
                AnimationController.animationTask(player, anim, () => {
                    var placeable = new ContainerItemPlaceable(item, pos, new DegreeRotation(0, 0, h));
                    placeable.initialize();
                });
            });

            return true;
        }
    }

    public class ContainerItem : Item {
        public DateTime RunoutDate { get => (DateTime)Data["RunoutDate"]; set { Data["RunoutDate"] = value; } }

        public List<int> ConfigItemIds;

        private float InitialWeight;

        public string ModelString;

        public ContainerItem(item dbItem) : base(dbItem) {
            foreach(var item in dbItem.config.configitemsitemcontainerinfoconfigItems) {
                ConfigItemIds = dbItem.config.configitemsitemcontainerinfoconfigItems.Select(i => i.subConfigItemId).ToList();
            }

            InitialWeight = dbItem.config.weight;
            Weight = getWeight();
        }

        public ContainerItem(configitem configItem, int amount, int quality) : base(configItem, quality, null) {
            init(configItem);

            if(configItem.durability != -1) {
                RunoutDate = DateTime.Now + TimeSpan.FromDays(configItem.durability);
            }

            updateDescription();
        }

        public override void use(IPlayer player) {
            var amounts = getAllSubItems();
            var menu = new Menu(Name, $"Insgesamt {amounts.Select(a => a.Amount).Aggregate((i, j) => i + j)} übrig");

            if(amounts.Count > 1) {
                foreach(var sub in amounts) { 
                    var subCfg = InventoryController.getConfigById(sub.ConfigId);
                    var subMenu = new Menu(subCfg.name, $"Noch {sub.Amount} übrig");
                    foreach(var item in getMenuItemsForSubItem(subCfg)) {
                        subMenu.addMenuItem(item);
                    }

                    menu.addMenuItem(new MenuMenuItem($"{subCfg.name} ({sub.Amount}x)", subMenu));
                }
            } else {
                var subCfg = InventoryController.getConfigById(amounts.First().ConfigId);

                foreach(var item in getMenuItemsForSubItem(subCfg)) {
                    menu.addMenuItem(item);
                }  
            }

            if(ModelString != null && ModelString != "") {
                menu.addMenuItem(new ClickMenuItem("Platzieren", $"Platziere den/das {Name}.", "", "PLACE_CONTAINER_ITEM", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Item", this } }));
            }

            player.showMenu(menu);

            base.use(player);
        }

        public List<MenuItem> getMenuItemsForSubItem(configitem sub) {
            return new List<MenuItem> {
                 new InputMenuItem($"{sub.name} nehmen", $"Nimm eine Menge an {sub.name} heraus", "frei für (1)", "TAKE_ITEM_FROM_CONTAINER")
                   .withData(new Dictionary<string, dynamic> { { "Item", this }, { "ConfigItemId", sub.configItemId } }),
                
                new ClickMenuItem($"Alle {sub.name} nehmen", $"Nimm alle {sub.name} heraus", "", "TAKE_ITEM_FROM_CONTAINER")
                 .withData(new Dictionary<string, dynamic> { { "Item", this }, { "ConfigItemId", sub.configItemId } })
                 .needsConfirmation("Alle herausnehmen?", "Wirklich alle herausnehmen?"),
            };
        }

        public void takeItemsOut(IPlayer player, int configItemId, int amount) {
            var sub = InventoryController.getConfigById(configItemId);

            if(amount < 0) {
                amount = getAmountForItem(sub.configItemId);
            }

            if(amount == 0) {
                player.sendBlockNotification("Es sind keine Items mehr übrig", "Keine übrig", Constants.NotifactionImages.Package);
                return;
            }

            var inv = player.getInventory();
            if(inv.MaxWeight - inv.CurrentWeight >= sub.weight * amount) {
                setAmountForItem(sub.configItemId, getAmountForItem(sub.configItemId) - amount);

                var items = InventoryController.createItems(sub, amount, Quality);

                if(items != null) {
                    foreach(var item in items) {
                        inv.addItem(item, true);
                    }

                    player.sendNotification(Constants.NotifactionTypes.Success, $"{amount}x {sub.name} herausgenommen!", $"{amount}x {sub.name} herausgenommen!", Constants.NotifactionImages.Package);
                }
            } else {
                player.sendBlockNotification("Nicht genug Platz im Inventar für die Menge an Items!", "Zu wenig Platz", Constants.NotifactionImages.Package);
            }

            Weight = getWeight();
            updateDescription();
        }

        private void init(configitem cfg) {
            foreach(var item in cfg.configitemsitemcontainerinfoconfigItems) {
                ConfigItemIds = cfg.configitemsitemcontainerinfoconfigItems.Select(i => i.subConfigItemId).ToList();
                setAmountForItem(item.subConfigItemId, item.subItemAmount);
            }

            InitialWeight = cfg.weight;
            Weight = getWeight();
        }

        public override void updateDescription() {
            Description = getAllSubItems().Select((i, index) => $"{i.Amount}x {InventoryController.getConfigById(i.ConfigId).name}").Aggregate((i, j) => $"{i}, {j}").CutToLength(75);
            base.updateDescription();
        }

        private float getWeight() {
            var weight = InitialWeight;
            foreach(var sub in getAllSubItems()) {
                weight += InventoryController.getConfigById(sub.ConfigId).weight * sub.Amount;
            }
            return weight;
        }

        internal int getAmountForItem(int configId) {
            return (int)Data[$"SubItemAmounts_{configId}"];
        }

        private void setAmountForItem(int configId, int amount) {
            Data[$"SubItemAmounts_{configId}"] = amount;
        }

        public record SubItem(int ConfigId, int Amount);
        public List<SubItem> getAllSubItems() {
            var list = new List<SubItem>();
            foreach(var key in Data.Items.Keys) {
                if(key.StartsWith("SubItemAmounts_")) {
                    list.Add(new SubItem(int.Parse(key.Split('_')[1]), (int)Data[key]));
                }
            }
            return list;
        }

        public override void processAdditionalInfo(string info) {
            ModelString = info;
        }
    }
}
