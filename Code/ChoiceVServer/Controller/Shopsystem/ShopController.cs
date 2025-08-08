using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Shopsystem.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Shopsystem {
    public enum ShopTypes {
        //Vending Machines
        SnackVendingMachine = 1,
        DrinkVendingMachine = 2,
        WaterVendingMachine = 3,
        CoffeeVendingMachine = 4,
        CigaretteVendingMachine = 9,

        //Standard-Shops
        TwentyFourSeven = 5,
        Gasstation = 6,
        LiquorStore = 7,
        FruitStand = 8,
        Ammunation = 17,

        //Special-Shops
        YouTool = 10,
        SimmetAlleyFoodVendor = 11,
        HotDogVendor = 12, //Hat auch Sodas!
        BurgerVendor = 13, //Hat auch Sodas!
        MedicalDepartmentVendor = 14, //Hat auch Sodas!
        RandyMcKoi = 15,
        PaletoHotelOma = 16,
    }

    public class ShopController : ChoiceVScript {
        public static List<ShopModel> AllShops = new List<ShopModel>();
        public static Dictionary<ShopTypes, List<ShopItem>> AllShopItems = new Dictionary<ShopTypes, List<ShopItem>>();

        private static bankaccount ShopBankaccount;

        public ShopController() {
            EventController.addMenuEvent("SHOP_SHOPPING_CART_ITEM_SELECT", onShoppingCartShopItem);
            EventController.addMenuEvent("SHOP_CONFIRM_SHOPPING_CART_ITEM", onShoppingCartShopItemConfirmation);

            EventController.addMenuEvent("SHOP_EMPTY_SHOPPING_CART", onShoppingCartEmptyCart);
            EventController.addMenuEvent("SHOP_REMOVE_ITEM_FROM_CART", onShoppingCartRemoveItem);

            EventController.MainAfterReadyDelegate += loadShops;

            ClothingController.OnPlayerPutOnClothesDelegate += onPlayerChangeClothing;

            var accL = BankController.getControllerBankaccounts(typeof(ShopController));
            ShopBankaccount = accL is { Count: > 0 }
                ? accL.First()
                : BankController.createBankAccount(typeof(ShopController), "Shop-Konto", BankAccountType.CompanyKonto, 0, BankController.getBankByType(BankCompanies.LibertyBank), true);

            #region Support Stuff

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    getSupportShopCreateMenu,
                    3,
                    SupportMenuCategories.ItemSystem,
                    "Shop Erstellen"
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_SHOP", onSupportCreateShop);
            EventController.addMenuEvent("SUPPORT_DELETE_SHOP", onSupportDeleteShop);

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    getSupportShopTypesMenu,
                    3,
                    SupportMenuCategories.ItemSystem,
                    "Shop-Typen"
                )
            );

            EventController.addMenuEvent("SUPPORT_ADD_SHOP_ITEM", onSupportAddShopItem);
            EventController.addMenuEvent("SHOP_REMOVE_SHOP_ITEM", onSupportRemoveShopItem);

            #endregion
        }

        public static long getShopBankAccountId() {
            return ShopBankaccount.id;
        }

        public static void openShopByType(IPlayer player, ShopTypes type) {
            var shop = AllShops.FirstOrDefault(s => s.ShopType == type);

            if (shop != null) {
                shop.openShop(player);
            } else {
                Logger.logError($"openShopByType: shop with type not found! type: {type}, charId: {player.getCharacterId()}, pos: {player.Position.ToString()}",
                    $"Fehler im Shop öffnen: Shop mit dem Typ wurde nicht gefunden: {type}", player);
            }
        }

        public static Menu getShopMenuByType(IPlayer player, ShopTypes type) {
            var shop = AllShops.FirstOrDefault(s => s.ShopType == type);

            if(shop != null) {
                return shop.getShopMenu(player);
            } else {
                Logger.logError($"openShopByType: shop with type not found! type: {type}, charId: {player.getCharacterId()}, pos: {player.Position.ToString()}",
                    $"Fehler im Shop öffnen: Shop mit dem Typ wurde nicht gefunden: {type}", player);
                return null;
            }
        }

        private static void loadShops() {
            AllShops.ForEach(s => s.Dispose());
            AllShops.Clear();
            AllShopItems.Clear();

            using (var db = new ChoiceVDb()) {
                foreach (var item in db.configshopitems
                    .Include(i => i.item)
                    .ThenInclude(i => i.configitemsfoodadditionalinfo)
                    .ThenInclude(fai => fai.categoryNavigation)
                    .Include(i => i.item)
                    .ThenInclude(i => i.configitemsitemcontainerinfoconfigItems)
                    .ThenInclude(i => i.subConfigItem)
                    .Include(i => i.item)
                    .ThenInclude(i => i.itemAnimationNavigation)) {
                    var shopItem = new ShopItem(item.item, item.quality, item.price, (ShopTypes)item.shopType, item.category);
                    if (AllShopItems.ContainsKey(shopItem.ShopType)) {
                        var list = AllShopItems[shopItem.ShopType];
                        list.Add(shopItem);
                    } else {
                        var list = new List<ShopItem> {
                            shopItem
                        };
                        AllShopItems[shopItem.ShopType] = list;
                    }
                }

                var allShelves = ShopShelfController.AllShelfs.Values.Concat(ShopShelfController.AllShelfsNoModel);
                foreach (var dbShop in db.configshops.Include(s => s.shelves)) {
                    if (dbShop.position != null) {
                        AllShops.Add(new ShopModel(
                            dbShop.id,
                            dbShop.name,
                            (ShopTypes)dbShop.type,
                            dbShop.position.FromJson(),
                            dbShop.width,
                            dbShop.height,
                            dbShop.rotation,
                            AllShopItems.ContainsKey((ShopTypes)dbShop.type) ? AllShopItems[(ShopTypes)dbShop.type] : new List<ShopItem>(),
                            allShelves.Where(ss => dbShop.shelves.Any(ds => ds.id == ss.Id)).ToList(),
                            dbShop.inventoryId ?? -1,
                            dbShop.maxRovValue,
                            dbShop.robValue)
                         );
                    } else {
                        var shopNew = new ShopModel(
                            dbShop.id,
                            dbShop.name,
                            (ShopTypes)dbShop.type,
                            AllShopItems[(ShopTypes)dbShop.type],
                            allShelves.Where(ss => dbShop.shelves.Any(ds => ds.id == ss.Id)).ToList(),
                            dbShop.inventoryId ?? -1
                        );
                                                    
                        shopNew.MaxRobValue = dbShop.maxRovValue;
                        shopNew.RobValue = dbShop.robValue;
                        
                        AllShops.Add(shopNew);
                    }
                }
            }
        }

        public static void createShop(string name, ShopTypes type, Position position, float width, float height, float rotation, int? inventoryId, decimal robValue = 0) {
            using (var db = new ChoiceVDb()) {
                var configshop = new configshop {
                    name = name,
                    type = (int)type,
                    position = position.ToJson(),
                    width = width,
                    height = height,
                    rotation = rotation,
                    inventoryId = inventoryId,
                    robValue = robValue,
                };

                db.configshops.Add(configshop);
                db.SaveChanges();
            }

            loadShops();
        }

        public static void addShopItem(ShopTypes type, int itemId, decimal price) {
            using (var db = new ChoiceVDb()) {
                var shopItem = new configshopitem {
                    itemId = itemId,
                    price = price,
                    shopType = (int)type,
                };

                db.configshopitems.Add(shopItem);
                db.SaveChanges();
            }

            loadShops();
        }

        public static void removeShopItem(ShopTypes type, ShopItem item) {
            using (var db = new ChoiceVDb()) {
                var dbItem = db.configshopitems.Find((int)type, item.ConfigItem.configItemId);
                db.configshopitems.Remove(dbItem);
                db.SaveChanges();
            }

            loadShops();
        }

        #region Events

        private bool onShoppingCartEmptyCart(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.resetData("SHOPPING_CART");

            player.sendNotification(NotifactionTypes.Info, "Du hast deinen Warenkorb geleert!", "Warenkorb geleert", NotifactionImages.Shop);

            return true;
        }

        private bool onShoppingCartRemoveItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shoppingCart = (ShoppingCart)data["Cart"];
            var item = (ShoppingCartItem)data["Item"];

            shoppingCart.Items.Remove(item);

            player.sendNotification(NotifactionTypes.Info, $"{item.Amount}x {item.Item.getName()} wurden aus dem Warenkorb entfernt", $"{item.Item.getName()} entfernt", NotifactionImages.Shop);

            return true;
        }

        //private bool onShoppingCartBuyCart(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        //    var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
        //    var paymentMethod = evt.elements[1].FromJson<ListMenuItemEvent>();

        //    var shoppingCart = (ShoppingCart)data["Cart"];

        //    if(paymentMethod.currentElement == "Bar") {
        //        var price = 0m;
        //        foreach(var item in shoppingCart.Items) {
        //            if(player.removeCash(item.Amount * item.Item.Price)) {
        //                price += item.Amount * item.Item.Price;

        //                var items = InventoryController.createItems(item.Item.ConfigItem, item.Amount, item.Item.Quality);

        //                foreach(var createItem in items) {
        //                    if(!player.getInventory().addItem(createItem)) {
        //                        player.sendBlockNotification($"Etwas ist schiefgelaufen! {item.Item.ConfigItem.name} konnte nicht ausgegeben werden!", "Fehler aufgetreten", NotifactionImages.Shop);
        //                        player.addCash(item.Item.Price);
        //                        break;
        //                    }
        //                }
        //            } else {
        //                player.sendBlockNotification($"Etwas ist schiefgelaufen! Es ist nicht genügend Bargeld verfügbar!", "Fehler aufgetreten", NotifactionImages.Shop);
        //                break;
        //            }
        //        }
        //        player.resetData("SHOPPING_CART");
        //        player.sendNotification(NotifactionTypes.Success, $"Warenkorb erfolgreich für ${price} bar gekauft", "Warenkorb gekauft", NotifactionImages.Shop);

        //    } else if(paymentMethod.currentElement == "Hauptkonto") {
        //        var shopName = (string)data["ShopName"];

        //        var price = 0m;
        //        foreach(var item in shoppingCart.Items) {
        //            price += item.Amount * item.Item.Price;
        //        }

        //        if(BankController.transferMoney(player.getMainBankAccount(), ShopBankaccount.id, price, $"Bezahlung {shopName}", out var failMessage)) {
        //            foreach(var item in shoppingCart.Items) {
        //                var items = InventoryController.createItems(item.Item.ConfigItem, item.Amount, item.Item.Quality);

        //                foreach(var createItem in items) {
        //                    if(!player.getInventory().addItem(createItem)) {
        //                        player.sendBlockNotification($"Etwas ist schiefgelaufen! {item.Item.ConfigItem.name} konnte nicht ausgegeben werden!", "Fehler aufgetreten", NotifactionImages.Shop);
        //                        Logger.logWarning(LogCategory.Player, LogActionType.Event, player, $"onShoppingCartBuyCart: Item could not be added to inventory! Item: {item.Item.ConfigItem.name}, Amount: {item.Amount}, CharId: {player.getCharacterId()}");
        //                        break;
        //                    }
        //                }
        //            }

        //            player.resetData("SHOPPING_CART");
        //            player.sendNotification(NotifactionTypes.Success, $"Warenkorb erfolgreich für ${price} mittels des Hauptkontos gekauft", "Warenkorb gekauft", NotifactionImages.Shop);
        //        } else {
        //            player.sendBlockNotification($"Der Einkauf konnte nicht abgeschlossen werden! {failMessage}", "Fehler aufgetreten", NotifactionImages.Shop);
        //        }
        //    }
        //    return true;
        //}

        public static void onPlayerBuyShoppingCart(IPlayer player, ShoppingCart shoppingCart) {
            foreach(var item in shoppingCart.Items) {
                var items = InventoryController.createItems(item.Item.ConfigItem, item.Amount, item.Item.Quality);

                foreach(var createItem in items) {
                    if(!player.getInventory().addItem(createItem, true)) {
                        player.sendBlockNotification($"Etwas ist schiefgelaufen! {item.Item.ConfigItem.name} konnte nicht ausgegeben werden!", "Fehler aufgetreten", NotifactionImages.Shop);
                        Logger.logWarning(LogCategory.Player, LogActionType.Event, player, $"onShoppingCartBuyCart: Item could not be added to inventory! Item: {item.Item.ConfigItem.name}, Amount: {item.Amount}, CharId: {player.getCharacterId()}");
                        break;
                    }
                }
            }

            player.resetData("SHOPPING_CART");
        }

        private bool onShoppingCartShopItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shop = (ShopModel)data["Shop"];
            var shopItem = (ShopItem)data["ShopItem"];

            var menu = new Menu(shopItem.ConfigItem.name, "Wieviel möchtest du in den Warenkorb legen?");

            menu.addMenuItem(new InputMenuItem("Anzahl", "Gib an wieviel du in den Warenkorb legen möchtest", "", InputMenuItemTypes.number, "" ));

            var menDat = new Dictionary<string, dynamic> { { "Shop", shop }, { "ShopItem", shopItem } };
            if (data.ContainsKey("Shelf")) {
                menDat.Add("Shelf", data["Shelf"]);
            }
            menu.addMenuItem(new MenuStatsMenuItem("Bestätigen", "Lege die gewählte Anzahl in den Warenkorb", "SHOP_CONFIRM_SHOPPING_CART_ITEM", MenuItemStyle.green).withData(menDat));

            player.showMenu(menu);

            return true;
        }

        private bool onShoppingCartShopItemConfirmation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shop = (ShopModel)data["Shop"];
            var item = (ShopItem)data["ShopItem"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            try {
                var amount = int.Parse(evt.elements[0].FromJson<InputMenuItemEvent>().input);

                ShoppingCart cart;
                if (player.hasData("SHOPPING_CART")) {
                    var Tcart = (ShoppingCart)player.getData("SHOPPING_CART");
                    if (Tcart.Shop == shop) {
                        cart = Tcart;
                    } else {
                        cart = new ShoppingCart(shop);
                        player.setData("SHOPPING_CART", cart);
                    }
                } else {
                    cart = new ShoppingCart(shop);
                    player.setData("SHOPPING_CART", cart);
                }

                cart.addItem(item, amount);

                player.sendNotification(NotifactionTypes.Info, $"Es wurden erfolgreich {amount}x {item.ConfigItem.name} dem Warenkorb hinzugefügt", "Warenkorb erweitert", NotifactionImages.Shop);


                if (!data.ContainsKey("Shelf")) {
                    shop.openShop(player);
                } else {
                    var shelf = (ShopShelf)data["Shelf"];
                    shelf.openShelf(player, shop);
                }

            } catch (Exception) {
                player.sendBlockNotification("Eingabe falsch!", "Falsche Eingabe", NotifactionImages.Shop);
            }

            return true;
        }


        #endregion

        private void onPlayerChangeClothing(IPlayer player, ClothingPlayer newClothing) {
            if (player.hasData("CURRENT_SHOP")) {
                var shop = (ShopModel)player.getData("CURRENT_SHOP");

                if (shop.ShopKeeper != null) {
                    var modules = shop.ShopKeeper.getNPCModulesByType<NPCShopModule>();
                    modules.ForEach(m => m.onPlayerPutOnClothing(player, newClothing));
                }
            }
        }

        #region Support Stuff

        private Menu getSupportShopCreateMenu() {
            var menu = new Menu("Shop Menü", "Was möchtest du tun?");

            var createMenu = new Menu("Shop erstellen", "Gib die Daten ein");

            createMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Shops.", "", ""));
            var types = Enum.GetValues<ShopTypes>().Select(s => s.ToString()).ToArray();
            createMenu.addMenuItem(new ListMenuItem("Typ", "Der Typ des Shops bestimmt welche Items dort gekauft werden können", types, ""));
            createMenu.addMenuItem(new InputMenuItem("Inventar-Id", "Gib die Id des Inventars des Shops an. Leerlassen um einen Shop ohne Inventar (unendliche Items) zu erstellen", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Ausraubwert", "Gib an wieviel es Wert ist den Shop auszurauben. Dieser Wert gilt für das Ausrauben des gesamten Shops", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den beschriebenen Shop", "SUPPORT_CREATE_SHOP", MenuItemStyle.green));

            menu.addMenuItem(new MenuMenuItem("Erstellen", createMenu));

            var listMenu = new Menu("Liste", "Was möchtest du tun?");
            foreach (var shop in AllShops) {
                var shopMenu = new Menu(shop.Name, $"was möchtest du tun?");

                shopMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche diesen Shop", "", "SUPPORT_DELETE_SHOP", MenuItemStyle.red).needsConfirmation("Shop löschen?", "Wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Shop", shop } }));

                listMenu.addMenuItem(new MenuMenuItem(shopMenu.Name, shopMenu));
            }
            menu.addMenuItem(new MenuMenuItem(listMenu.Name, listMenu));
            return menu;
        }

        private bool onSupportCreateShop(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var typeEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
            var inventoryIdEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var robValueEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            int? invId = null;
            if (inventoryIdEvt.input != "" && inventoryIdEvt.input != null) {
                try {
                    invId = int.Parse(inventoryIdEvt.input);
                } catch (Exception) {
                    player.sendBlockNotification("Eingabe falsch!", "");
                    return false;
                }
            }

            player.sendNotification(NotifactionTypes.Info, "Erstelle nun die Collision des Shops", "");
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                createShop(nameEvt.input, Enum.Parse<ShopTypes>(typeEvt.currentElement), p, w, h, r, invId);
                player.sendNotification(NotifactionTypes.Success, "Shop erfolgreich erstellt", "");
            });


            return true;
        }

        private bool onSupportDeleteShop(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shop = (ShopModel)data["Shop"];

            shop.Dispose();
            AllShops.Remove(shop);

            using (var db = new ChoiceVDb()) {
                var dbShop = db.configshops.Find(shop.Id);

                db.configshops.Remove(dbShop);
                db.SaveChanges();
            }

            loadShops();
            player.sendNotification(NotifactionTypes.Warning, "Shop erfolgreich gelöscht!", "");

            return true;
        }


        private Menu getSupportShopTypesMenu() {
            var menu = new Menu("Shop Typen", "Was möchtest du tun?");

            foreach (var type in Enum.GetValues<ShopTypes>().ToList()) {
                var typeMenu = new Menu($"{type}", "Was möchtest du tun?");

                var addMenu = new Menu("Item hinzufügen", "Füge ein Item hinzu");
                addMenu.addMenuItem(new InputMenuItem("Item-Id", "Gib die Id des Items an", "", ""));
                addMenu.addMenuItem(new InputMenuItem("Preis", "Gib den Preis des Items an", "", ""));
                addMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das angegeben Shop-Item", "SUPPORT_ADD_SHOP_ITEM", MenuItemStyle.green).needsConfirmation("Shop Item hinzufügen?", "Item wirklich hinzufügen?").withData(new Dictionary<string, dynamic> { { "Type", type } }));
                typeMenu.addMenuItem(new MenuMenuItem(addMenu.Name, addMenu));

                var itemsMenu = new Menu("Items", "Wähle aus welches Item gelöscht werden soll");
                if (AllShopItems.ContainsKey(type)) {
                    foreach (var item in AllShopItems[type]) {
                        itemsMenu.addMenuItem(new ClickMenuItem(item.ConfigItem.name, $"Das Item: {item.ConfigItem.name} kostet ${item.Price} in diesem ShopTypen. Klicke um das Item zu entfernen!", $"${item.Price}", "SHOP_REMOVE_SHOP_ITEM").needsConfirmation($"{item.ConfigItem.name} entfernen?", $"{item.ConfigItem.name} wirklich entfernen?").withData(new Dictionary<string, dynamic> { { "Type", type }, { "Item", item } }));
                    }
                }

                typeMenu.addMenuItem(new MenuMenuItem(itemsMenu.Name, itemsMenu));

                menu.addMenuItem(new MenuMenuItem(typeMenu.Name, typeMenu));
            }

            return menu;
        }

        private bool onSupportAddShopItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = (ShopTypes)data["Type"];
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var itemIdEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var priceEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var itemId = 0;
            var price = 0m;
            try {
                itemId = int.Parse(itemIdEvt.input);
                price = decimal.Parse(priceEvt.input);
            } catch (Exception) {
                player.sendBlockNotification("Eingabe falsch!", "");
            }

            addShopItem(type, itemId, price);
            player.sendNotification(NotifactionTypes.Success, "Item erfolgreich hinzugefügt!", "");

            return true;
        }

        private bool onSupportRemoveShopItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = (ShopTypes)data["Type"];
            var item = (ShopItem)data["Item"];

            AllShopItems[type].Remove(item);

            using (var db = new ChoiceVDb()) {
                var dbItem = db.configshopitems.Find((int)type, item.ConfigItem.configItemId);

                db.configshopitems.Remove(dbItem);
                db.SaveChanges();
            }

            loadShops();
            player.sendNotification(NotifactionTypes.Warning, "Item erfolgreich gelöscht!", "");

            return true;
        }

        #endregion
    }
}
