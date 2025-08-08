using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
    public class TerminaItemKit {
        public string Identifier;
        public string Name;
        public string Description;
        public int Price;

        public List<configitem> ConfigItems;
        public List<int> ItemsAmount;

        public TerminaItemKit(string identifier, string name, string description, int price) {
            Identifier = identifier;
            Name = name;
            Description = description;
            Price = price;
            ConfigItems = [];
            ItemsAmount = [];
        }

        public void addItemToKit(configitem item, int amount) {
            ConfigItems.Add(item);
            ItemsAmount.Add(amount);
        }
    }

    public class TerminalItemShop : TerminalShop {
        private List<TerminaItemKit> AllKits;

        public TerminalItemShop() : base("ITEM_SHOP", "Duty-Free Laden") {
            AllKits = new List<TerminaItemKit>();
        }

        public void addKit(TerminaItemKit kit) {
            AllKits.Add(kit);
        }

        public override Menu getBuyableOptions(IPlayer player) {
            var menu = new Menu("Duty-Free Kits kaufen", "Was möchtest du kaufen?");
            menu.addMenuItem(new StaticMenuItem("Information", "Alle gekauften Item-Kits werden erst NACH dem Landen des Flugzeuges dem Inventar hinzugefügt!", "siehe Beschreibung"));

            var tokens = TerminalShopController.getPlayerTokens(player);

            List<string> list = new();
            if(player.hasData("TERMINAL_KIT_LIST")) {
                list = ((string)player.getData("TERMINAL_KIT_LIST")).FromJson<List<string>>();
            }

            foreach(var kit in AllKits) {
                if(list.Contains(kit.Identifier)) {
                    menu.addMenuItem(new StaticMenuItem(kit.Name, kit.Description, $"bereits erworben!"));
                } else {
                    if(tokens >= kit.Price) {
                        menu.addMenuItem(new ClickMenuItem(kit.Name, kit.Description, $"{kit.Price} Marken", "TERMINAL_BUY_ITEM_KIT").withData(new Dictionary<string, dynamic> { { "Kit", kit } }).needsConfirmation($"Kit kaufen?", $"{kit.Name} für {kit.Price}M kaufen?"));
                    } else {
                        menu.addMenuItem(new StaticMenuItem(kit.Name, kit.Description, $"zu teuer! ({kit.Price}M)", MenuItemStyle.yellow));
                    }
                }
            }

            return menu;
        }

        public override Menu getAlreadyBoughtListing(IPlayer player) {
            var menu = new Menu("Bereits Gekauftes zurückgeben", "Was möchtest du kaufen?");

            var list = ((string)player.getData("TERMINAL_KIT_LIST")).FromJson<List<string>>();

            foreach(var kit in AllKits.Where(k => list.Contains(k.Identifier))) {
                menu.addMenuItem(new ClickMenuItem(kit.Name, $"Klicke um das Kit zurückzugeben. Du erhältst die Marken zurück.", $"{kit.Price} Marken", "TERMINAL_UNBUY_ITEM_KIT").withData(new Dictionary<string, dynamic> { { "Kit", kit } }).needsConfirmation($"Kit zuückgeben?", $"{kit.Name} für {kit.Price}M zurückgeben?"));
            }

            return menu;
        }

        public override bool hasPlayerBoughtSomething(IPlayer player) {
            return player.hasData("TERMINAL_KIT_LIST");
        }

        public override void onPlayerLand(IPlayer player, ref executive_person_file file) {
            if(player.hasData("TERMINAL_KIT_LIST")) {
                foreach(var identifier in ((string)player.getData("TERMINAL_KIT_LIST")).FromJson<List<string>>()) {
                    var kit = AllKits.FirstOrDefault(k => k.Identifier == identifier);

                    if(kit != null) {
                        for(var i = 0; i < kit.ConfigItems.Count; i++) {
                            var items = InventoryController.createItems(kit.ConfigItems[i], kit.ItemsAmount[i], -1);

                            foreach(var item in items) {
                                player.getInventory().addItem(item, true);
                            }
                        }
                    }
                }

                player.resetPermantData("TERMINAL_KIT_LIST");
            }
        }
    }

    public class TerminalItemShopEventController : ChoiceVScript {
        private static TerminalItemShop Shop;
        public TerminalItemShopEventController() {
            EventController.addMenuEvent("TERMINAL_BUY_ITEM_KIT", onTerminalBuyItemKit);
            EventController.addMenuEvent("TERMINAL_UNBUY_ITEM_KIT", onTerminanUnbuyItemKit);

            Shop = new TerminalItemShop();
            TerminalShopController.addTerminalShop(Shop);
            EventController.MainAfterReadyDelegate += mainAfterReady;            
        }

        private void mainAfterReady() {
            var phoneKit = new TerminaItemKit("PHONE_KIT", "Smartphone Kit", "Enthält ein Smarthone und 2 SIM-Karten", 1);
            phoneKit.addItemToKit(InventoryController.getConfigItemForType<Smartphone>(), 1);
            phoneKit.addItemToKit(InventoryController.getConfigItemForType<SIMCard>(), 2);

            var bagKit = new TerminaItemKit("BAG_KIT", "Extra-Handgepäck Kit", "Enthält einen Rucksack, etwas Verpflegung und andere nützliche Gegenstände", 3);
            bagKit.addItemToKit(InventoryController.getConfigItemForType<Backpack>(), 1);
            List<configitem> items = new List<configitem>();
            //2x Random Wasser
            for(int i = 0; i < 2; i++) {
                items.Add(InventoryController.getConfigById(getRandomItem<FoodItem>(true, "WATER")));
            }
            //2x SOFT_DRINK
            for(int i = 0; i < 2; i++) {
                items.Add(InventoryController.getConfigById(getRandomItem<FoodItem>(true, "SOFT_DRINK")));
            }
            //4x STORE_FAST_FOOD
            for(int i = 0; i < 4; i++) {
                items.Add(InventoryController.getConfigById(getRandomItem<FoodItem>(true, "STORE_FAST_FOOD")));
            }
            //1x Snack_Unhealthy
            items.Add(InventoryController.getConfigById(getRandomItem<FoodItem>(true, "SNACK_UNHEALTHY")));
            //1x SNACK_Healthy
            items.Add(InventoryController.getConfigById(getRandomItem<FoodItem>(true, "SNACK_HEALTHY")));
            //1x 447 insted of Note
            items.Add(InventoryController.getConfigById(447));
            //1x CigaretteBox
            items.Add(InventoryController.getConfigById(getRandomItemByCategory("CigaretteBox")));

            foreach(var thing in items) {
                bagKit.addItemToKit(thing, 1);
            }
            Shop.addKit(phoneKit);
            Shop.addKit(bagKit);
        }

        private int getRandomItem<T>(bool isFood = false, string foodAdditionalCategory = null) {
            int item = -1;
            List<configitem> configitems = new List<configitem>();
            if (isFood) {
                configitems = InventoryController.getConfigItemsForType<T>(i => i.configitemsfoodadditionalinfo?.category == foodAdditionalCategory).GetRandomElements(1).ToList();
            } else {
                configitems = InventoryController.getConfigItemsForType<T>(i => true).GetRandomElements(1).ToList();
            }
            if (configitems.Count > 0) {
                item = configitems.First().configItemId;
            }
            return item;
        }

        private int getRandomItemByCategory(string category, bool isFood = false, string foodAdditionalCategory = null) {
            int item = -1;
            List<configitem> configitems = new List<configitem>();
            if (isFood) {
                configitems = InventoryController.getConfigItems(i => i.category == category && i.configitemsfoodadditionalinfo?.category == foodAdditionalCategory).GetRandomElements(1).ToList();
            } else {
                configitems = InventoryController.getConfigItems(i => i.category == category).GetRandomElements(1).ToList();
            }
            if (configitems.Count > 0) {
                item = configitems.First().configItemId;
            }
            return item;
        }

        private bool onTerminalBuyItemKit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var kit = (TerminaItemKit)data["Kit"];

            if(TerminalShopController.addOrRemovePlayerTokens(player, -kit.Price)) {
                List<string> list = new();
                if(player.hasData("TERMINAL_KIT_LIST")) {
                    list = ((string)player.getData("TERMINAL_KIT_LIST")).FromJson<List<string>>();
                }
                list.Add(kit.Identifier);
                player.setPermanentData("TERMINAL_KIT_LIST", list.ToJson());
                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast erfolgreich das {kit.Name} gekauft! Es hat {kit.Price} Marken gekostet! Die Waren werden dir NACH dem Landeanflug ausgehändigt!", "Kit gekauft!", Constants.NotifactionImages.Plane);
            } else {
                player.sendBlockNotification("Du hattest nicht genügend Marken!", "Zu wenig Marken", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        private bool onTerminanUnbuyItemKit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var kit = (TerminaItemKit)data["Kit"];

            if(TerminalShopController.addOrRemovePlayerTokens(player, kit.Price)) {
                var list = ((string)player.getData("TERMINAL_KIT_LIST")).FromJson<List<string>>();
                list.Remove(kit.Identifier);
                player.setPermanentData("TERMINAL_KIT_LIST", list.ToJson());
                player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast erfolgreich das {kit.Name} zuückgegeben. Du hast {kit.Price} Marken zurückerhalten!", "Kit zurückgegeben!", Constants.NotifactionImages.Plane);
            }

            return true;
        }
    }
}
