using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Shopsystem {
    public record ShopPedBuyItem(int ConfigItemId, decimal Price, bool PricePerUnitNotWeight);
    
    public class ShopBuyPedModule : NPCModule {
        private string Name;
        internal List<ShopPedBuyItem> BuyItems;
        private CollisionShape LoadingZone;
        
        public ShopBuyPedModule(ChoiceVPed ped) : base(ped) { }

        public ShopBuyPedModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
            Name = settings["Name"];
            LoadingZone = settings["LoadingZone"] == "" ? null : (CollisionShape)CollisionShape.Create(settings["LoadingZone"]);
            if(settings.ContainsKey("BuyItems")) {
                BuyItems = ((string)settings["BuyItems"]).FromJson<List<ShopPedBuyItem>>();
            } else {
                BuyItems = [];
            }
        }
        
        public override List<MenuItem> getMenuItems(IPlayer player) {
            var list = new List<MenuItem>();
            var sellMenu = new Menu(Name, "Was möchtest du verkaufen?");

            if(player.getCharacterData().AdminMode && player.getAdminLevel() >= 3) {
                list.Add(new MenuMenuItem("Admin", getAdminMenu(), MenuItemStyle.yellow));
            }
            
            sellMenu.addMenuItem(new MenuMenuItem("Inventar", getSellMenuForInventory(player, "Inventar", player.getInventory())));
            
            var vehicle = LoadingZone?.getAllEntities().FirstOrDefault(e => e is ChoiceVVehicle veh && veh.hasPlayerAccess(player) && veh.LockState == VehicleLockState.Unlocked) as ChoiceVVehicle;
            if(vehicle != null) {
                var vehicleMenu = getSellMenuForInventory(player, $"Fahrz. {vehicle.NumberplateText}", vehicle.Inventory);
                sellMenu.addMenuItem(new MenuMenuItem(vehicleMenu.Name, vehicleMenu));
            }
            
            list.Add(new MenuMenuItem(sellMenu.Name, sellMenu));

            return list;
        }
        
        private Menu getSellMenuForInventory(IPlayer player, string name, Inventory inventory) {
            var menu = new Menu(name, "Was möchtest du verkaufen?");
            foreach(var sellItem in BuyItems) {
                var cfg = InventoryController.getConfigById(sellItem.ConfigItemId);

                if(cfg.suspicious == 1 && !player.hasCrimeFlag()){
                    continue;
                }

                var invAmount = inventory.getItems(i => i.ConfigId == cfg.configItemId).Sum(i => i.StackAmount ?? 1);
                if(invAmount > 0) {
                    if(sellItem.PricePerUnitNotWeight) {
                        menu.addMenuItem(new InputMenuItem($"Verkaufe {cfg.name}", $"Verkaufe {cfg.name} für ${sellItem.Price} pro Stück. Du hast maximal {invAmount} verfügbar.", $"verfügbar: {invAmount}", InputMenuItemTypes.number, "SHOP_PED_SELL_ITEM")
                            .withData(new Dictionary<string, dynamic> {
                                {"Item", sellItem},
                                {"PerUnit", true},
                                {"Inventory", inventory},
                            }));
                    } else {
                        var weight = inventory.getItems(i => i.ConfigId == cfg.configItemId).Sum(i => i.Weight * i.StackAmount ?? 1);
                        menu.addMenuItem(new ClickMenuItem($"Verkaufe {cfg.name}", $"Verkaufe {cfg.name} für ${sellItem.Price} pro kg. Du hast maximal {weight} verfügbar.", $"verfügbar: {weight}", "SHOP_PED_SELL_ITEM")
                            .withData(new Dictionary<string, dynamic> {
                                {"Item", sellItem},
                                {"PerUnit", false},
                                {"Inventory", inventory},
                            }));
                    }
                } else {
                    var price = sellItem.PricePerUnitNotWeight ? $"${sellItem.Price} pro Stück" : $"${sellItem.Price} pro kg";
                    menu.addMenuItem(new StaticMenuItem($"Verkaufe {cfg.name}", $"Du hast keine {cfg.name} zum Verkauf.", $"Preis: {price}"));
                }
            }

            return menu;
        }

        public bool hasItem(int configItemId) {
            return BuyItems.Any(i => i.ConfigItemId == configItemId);
        }
        
        public override void onRemove() {
            LoadingZone.Dispose();
            LoadingZone = null;
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Ankauf Shop", "Ein Module bei welchem der NPC bestimmte Items ankauft", "");
        }


        private Menu getAdminMenu() {
            var adminMenu = new Menu("Admineinstellungen", "Was möchtest du tun?");
                            
            var createItemMenu = new Menu("Item hinzufügen", "Was möchtest du tun?");
            createItemMenu.addMenuItem(new InputMenuItem("ConfigItem", "ConfigId des Items", "", "").withOptions(InventoryController.getAllConfigItems().Select(i => $"{i.configItemId}: {i.name}").ToArray()));
            createItemMenu.addMenuItem(new InputMenuItem("Preis", "Preis des Items. Entweder pro Stück oder pro Kilogram", "", InputMenuItemTypes.number, "")); 
            createItemMenu.addMenuItem(new CheckBoxMenuItem("Preis pro Stück (sonst per kg)", "Ist der Preis pro Stück oder pro Kilogram?", true, ""));
            createItemMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das angegebene Item", "SUPPORT_SHOP_PED_CREATE_ITEM", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> {
                    {"Shop", this},
                }));
            adminMenu.addMenuItem(new MenuMenuItem("Item hinzufügen", createItemMenu));
            
            foreach(var item in BuyItems) {
                var cfg = InventoryController.getConfigById(item.ConfigItemId);
                var itemMenu = new Menu($"{cfg.name}", "Was möchtest du tun?");
                itemMenu.addMenuItem(new InputMenuItem("Item", "Ändere das Item", "", "")
                    .withOptions(InventoryController.getAllConfigItems().Select(i => $"{i.configItemId}: {i.name}").ToArray())
                    .withStartValue($"{cfg.configItemId}: {cfg.name}"));
                itemMenu.addMenuItem(new InputMenuItem("Preis pro Stück", "Ändere den Preis pro Stück", "", InputMenuItemTypes.number, "")
                    .withStartValue(item.Price.ToString()));
                itemMenu.addMenuItem(new CheckBoxMenuItem("Preis pro Stück (sonst per kg)", "Ist der Preis pro Stück oder pro Kilogram?", item.PricePerUnitNotWeight, ""));
                itemMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändere das Item", "SUPPORT_SHOP_PED_CHANGE_ITEM", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> {
                        {"Shop", this},
                        {"Item", item},
                    }));
                itemMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche das Item", "", "SUPPORT_SHOP_PED_DELETE_ITEM", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> {
                        {"Shop", this},
                        {"Item", item},
                    }));
                adminMenu.addMenuItem(new MenuMenuItem(itemMenu.Name, itemMenu));
            }

            return adminMenu;
        }
    }
}