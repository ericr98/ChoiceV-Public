using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Model.Menu;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.Controller.Shopsystem.Model {
    public class ShopModel : IDisposable {
        public int Id;
        public string Name;
        public ShopTypes ShopType;
        public ChoiceVPed ShopKeeper;
        public CollisionShape CollisionShape;
        public List<ShopItem> AllItemsInShop;
        private List<ShopItem> AllItemsAtCounter;
        private List<ShopItem> AllItemsInShelves;
        public List<ShopShelf> Shelves;

        public decimal MaxRobValue;
        public decimal RobValue;

        public Inventory Inventory;

        public ShopModel(int id, string name, ShopTypes shopType, List<ShopItem> availableItems, List<ShopShelf> allShelves, int inventoryId = -1) {
            Id = id;
            Name = name;
            ShopType = shopType;
            AllItemsInShop = availableItems;
            Shelves = allShelves;
            if(inventoryId != -1) {
                Inventory = InventoryController.loadInventory(inventoryId);
            }

            setItemsAtCounter();
        }

        public ShopModel(int id, string name, ShopTypes shopType, Position position, float width, float height, float rotation, List<ShopItem> availableItems,  List<ShopShelf> allShelves, int inventoryId = -1, decimal maxRobValue = 0, decimal robValue = 0) {
            Id = id;
            Name = name;
            ShopType = shopType;
            CollisionShape = CollisionShape.Create(position, width, height, rotation, true, false, false);
            CollisionShape.OnEntityEnterShape += onEnterShape;
            CollisionShape.OnEntityExitShape += onExitShape;

            ShopKeeper = PedController.findPed(p => CollisionShape.IsInShape(p.Position));
            if(ShopKeeper != null) {
                ShopKeeper.addModule(new NPCShopModule(ShopKeeper, this));
            } else {
                CollisionShape.Interactable = true;
                CollisionShape.OnCollisionShapeInteraction += openShop;
            }

            Shelves = allShelves;
            AllItemsInShop = availableItems;

            if(inventoryId != -1) {
                Inventory = InventoryController.loadInventory(inventoryId);
            }

            MaxRobValue = maxRobValue;
            RobValue = robValue;
            
            setItemsAtCounter();
        }

        private void setItemsAtCounter() {
            AllItemsAtCounter = AllItemsInShop.Where(i => !Shelves.Any(s => s.AvailableItems.Any(si => si.configItemId == i.ConfigItem.configItemId))).ToList();
            AllItemsInShelves = AllItemsInShop.Where(i => !AllItemsAtCounter.Contains(i)).ToList();
        }

        private void onEnterShape(CollisionShape shape, IEntity entity) {
            (entity as IPlayer).setData("CURRENT_SHOP", this);

            if(ShopKeeper != null) {
                var modules = ShopKeeper.getNPCModulesByType<NPCShopModule>();

                foreach(var module in modules) {
                    module.playerEnterShop(entity as IPlayer);
                }
            }
        }

        
        private void onExitShape(CollisionShape shape, IEntity entity) {
            if(entity is IPlayer player && player.hasData("SHOPPING_CART")) {
                player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast den Laden verlassen, hast aber noch Sachen im Warenkorb. Diese bleiben dann im Shop.", "Warenkorb nicht leer");
            }
            (entity as IPlayer).resetData("CURRENT_SHOP");
        }

        public void Dispose() {
            CollisionShape.Dispose();
            if(Inventory != null) {
                InventoryController.unloadInventory(Inventory);
            }

            if(ShopKeeper != null) {
                ShopKeeper.removeModuleByType<NPCShopModule>();
            }
        }

        public Menu getShopMenu(IPlayer player, MenuCloseEventDelegate closeCallback = null) {
            var menu = new Menu(Name, "Was möchtest du tun?", closeCallback);

            if(player.hasData("SHOPPING_CART")) {
                var cart = (ShoppingCart)player.getData("SHOPPING_CART");

                if(cart.Shop == this) {
                    menu.addMenuItem(new MenuMenuItem("Warenkorb", new VirtualMenu("Warenkorb", () => cart.getMenu(player, Name))));
                }
            }
            

            // Items at Counter
            var noCategories = AllItemsAtCounter.Select(i => i.Category).Distinct().Count() == 1;
            var categoriesDictionary = new Dictionary<string, Menu>();
            foreach(var shopItem in AllItemsAtCounter) {
                var data = new Dictionary<string, dynamic> {
                    { "Shop", this },
                    { "ShopItem", shopItem },
                };

                MenuItem item = null;
                if(Inventory != null) {
                    var items = Inventory.getItems(shopItem.ConfigItem.configItemId, -1, i => true);
                    if(items.Count > 0) {
                        var count = 0;
                        items.ForEach(i => count += i.StackAmount ?? 1);

                        item = new ClickMenuItem(shopItem.getName(), $"Lege {shopItem.getName()} in den Warenkorb. Der Laden hat noch {count} übrig", $"{shopItem.Price}", "SHOP_SHOPPING_CART_ITEM_SELECT").withData(data);
                    } else {
                        item = new StaticMenuItem(shopItem.getName(), $"Der Laden hat kein/e {shopItem.getName()} mehr auf Lager", $"{shopItem.Price}", MenuItemStyle.red);
                    }
                } else {
                    item = new ClickMenuItem(shopItem.getName(), $"Lege {shopItem.ConfigItem.name} in den Warenkorb. Der Laden hat genug auf Lager", $"{shopItem.Price}", "SHOP_SHOPPING_CART_ITEM_SELECT").withData(data);
                }

                if(noCategories) {
                    menu.addMenuItem(item);
                } else {
                    if(categoriesDictionary.ContainsKey(shopItem.Category)) {
                        categoriesDictionary[shopItem.Category].addMenuItem(item);
                    } else {
                        var categoryMenu = new Menu(shopItem.Category, "Was möchstest du kaufen?");
                        menu.addMenuItem(new MenuMenuItem(categoryMenu.Name, categoryMenu));
                        categoryMenu.addMenuItem(item);
                        categoriesDictionary.Add(shopItem.Category, categoryMenu);
                    }
                }
            }
            
            // Items in Shelves
            var availableItemsVirtMenu = new VirtualMenu("Produkte nur verfügbar in Regalen", () => {
                var virtMenu = new Menu("Produkte in Regalen", "Die folgenden Produkte gibt es NUR in Regalen:");

                var noCategories = AllItemsAtCounter.Select(i => i.Category).Distinct().Count() == 1;
                var categoriesDictionary = new Dictionary<string, Menu>();
                
                foreach(var shopItem in AllItemsInShelves) {
                    MenuItem item = new StaticMenuItem(shopItem.getName(), $"Im Laden kann man {shopItem.getName()} in den Regalen finden.", $"{shopItem.Price}", MenuItemStyle.normal);
  
                    if(noCategories) {
                        virtMenu.addMenuItem(item);
                    } else {
                        if(categoriesDictionary.ContainsKey(shopItem.Category)) {
                            categoriesDictionary[shopItem.Category].addMenuItem(item);
                        } else {
                            var categoryMenu = new Menu(shopItem.Category, "Diese Produkte gibt es:");
                            virtMenu.addMenuItem(new MenuMenuItem(categoryMenu.Name, categoryMenu));
                            categoryMenu.addMenuItem(item);
                            categoriesDictionary.Add(shopItem.Category, categoryMenu);
                        }
                    }
                }
                
                return virtMenu;
            });
            
            if(AllItemsInShelves.Count > 0) {
                menu.addMenuItem(new MenuMenuItem(availableItemsVirtMenu.Name, availableItemsVirtMenu));
            }
            
            return menu;
        }

        public bool openShop(IPlayer player) {
            player.showMenu(getShopMenu(player));
            
            return true;
        }
        
        //Checks if a shelf is registered in the shop
        public void makeShelfCheck(IPlayer player, ShopShelf shelf) {
            if(Shelves.Contains(shelf)) {
                return;
            }

            using(var db = new ChoiceVDb()) {
                var shop = db.configshops.Find(Id);
                var dbShelf = db.configshopshelfs.Find(shelf.Id);
                
                if(shop == null || dbShelf == null || shop.shelves.Contains(dbShelf)) {
                    return;
                }
                
                shop.shelves.Add(dbShelf);
                
                db.SaveChanges();
                
                Shelves.Add(shelf);
                setItemsAtCounter();
                
                player.sendNotification(Constants.NotifactionTypes.Info, $"Regal {shelf.Name} zu Shop {Name} hinzugefügt. Bitte interagiere mit allen anderen Regalen, um sie zu registrieren. (Falls noch nicht geschehen)", "Regal hinzugefügt");
            }
        }
    }
}
