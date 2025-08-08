using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Controller.Shopsystem.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Shopsystem {
    public class ShopShelfController : ChoiceVScript {
        internal static Dictionary<string, ShopShelf> AllShelfs = new Dictionary<string, ShopShelf>();
        internal static List<ShopShelf> AllShelfsNoModel = new List<ShopShelf>();

        public ShopShelfController() {
            InteractionController.addObjectInteractionCallback(
                "OPEN_SHOP_SHELF",
                "Shop Regal öffnen",
                onOpenShopShelf
            );

            EventController.addMenuEvent("SHOP_STEAL_ITEM", onShopStealItem);
            EventController.addMenuEvent("SHOP_STEAL_SHOP_MONEY", onShopStealShopMoney);

            EventController.MainReadyDelegate += onLoadShopShelfs;

            #region Support

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    getSupportShopShelfMenu,
                    3,
                    SupportMenuCategories.ItemSystem,
                    "Shop-Regale"
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_SHOP_SHELF", onSupportCreateShelf);
            EventController.addMenuEvent("SUPPORT_SHOP_SHELF_ADD_ITEM", onSupportAddShelfItem);
            EventController.addMenuEvent("SUPPORT_SHOP_SHELF_DELETE_SHELF", onSupportDeleteShelf);
            EventController.addMenuEvent("SUPPORT_SHOP_SHELF_REMOVE_ITEM", onSupportRemoveShelfItem);

            EventController.addMenuEvent("SUPPORT_CREATE_COLLISION_SHAPE_FOR_SHELF", onCreateCollsionShapeForShelf);
            EventController.addMenuEvent("SUPPORT_DELETE_COLLISION_SHAPE_FOR_SHELF", onDeleteCollisionShapeForShelf);

            #endregion
        }

        private void onLoadShopShelfs() {
            AllShelfs.Clear();
            AllShelfsNoModel.Clear();
            
            using(var db = new ChoiceVDb()) {
                foreach(var dbShelf in db.configshopshelfs.Include(s => s.items)) {
                    var colShapeStrs = dbShelf.colShapesStr.FromJson<List<string>>() ?? new List<string>();
                    var colShapes = new List<CollisionShape>();
                    foreach(var colShapeStr in colShapeStrs) {
                        var colShape = CollisionShape.Create(colShapeStr);
                        colShapes.Add(colShape);
                    }

                    if(dbShelf.modelName == "") {
                        AllShelfsNoModel.Add(new ShopShelf(dbShelf.id, dbShelf.name, dbShelf.modelName, dbShelf.items.ToList(), colShapes));
                    } else {
                        AllShelfs.Add(ChoiceVAPI.Hash(dbShelf.modelName).ToString(), new ShopShelf(dbShelf.id, dbShelf.name, dbShelf.modelName, dbShelf.items.ToList(), colShapes)); 
                    }
                }
            }

            InteractionController.addInteractableObjects(AllShelfs.Select(s => ChoiceVAPI.Hash(s.Value.ModelName).ToString()).ToList(), "OPEN_SHOP_SHELF");
        }
        
        private void onOpenShopShelf(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            if(AllShelfs.ContainsKey(modelName) || AllShelfs.ContainsKey(ChoiceVAPI.Hash(modelName).ToString())) {
                var shelf = AllShelfs[modelName];

                if(player.hasData("CURRENT_SHOP")) {
                    shelf.openShelf(player, (ShopModel)player.getData("CURRENT_SHOP"));
                }
            }
        }

        private bool onShopStealItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shop = (ShopModel)data["Shop"];
            var shopItem = (ShopItem)data["ShopItem"];
            var shelf = (ShopShelf)data["Shelf"];

            var anim = AnimationController.getAnimationByName("TAKE_STUFF");
            AnimationController.animationTask(player, anim, () => {
                if(shop.ShopKeeper != null) {
                    var modules = shop.ShopKeeper.getNPCModulesByType<NPCShopModule>();
                    var worked = true;
                    foreach(var module in modules) {
                        if(!module.playerMakesStealInteraction(player, shopItem.ConfigItem)) {
                            worked = false;
                        }
                    }

                    if(worked) {
                        var item = InventoryController.createItem(shopItem.ConfigItem, 1, shopItem.Quality);
                        if(player.getInventory().addItem(item)) {
                            CrimeNetworkController.OnPlayerCrimeActionDelegate.Invoke(player, CrimeAction.StoreTheft, 1, new Dictionary<string, dynamic> { { "Item", shopItem.ConfigItem } });
                            player.sendNotification(NotifactionTypes.Warning, $"Du hast erfolgreich ein/eine {item.Name} gestohlen.", $"{item.Name} gestohlen", NotifactionImages.Shop);
                        } else {
                            player.sendBlockNotification("Es war nicht genug Platz im Inventar um das Produkt zu stehlen", "Zu wenig Platz", NotifactionImages.Shop);
                        }
                    } else {
                        player.sendBlockNotification("Du hast hier zu oft gestohlen. Suche dir einen anderen Ort!", "Zu oft gestohlen!", NotifactionImages.Shop);
                    }
                }
            });

            return true;
        }

        private bool onShopStealShopMoney(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shop = (ShopModel)data["Shop"];

            var modules = shop.ShopKeeper.getNPCModulesByType<NPCShopModule>();

            modules.ForEach((m) => m.startRobbing(player));

            return true;
        }

        #region Support

        private Menu getSupportShopShelfMenu() {
            var menu = new Menu("Shop Regale einstellen", "Was möchtest du tun?");

            var createMenu = new Menu("Erstellen", "Gib die Daten ein");
            createMenu.addMenuItem(new InputMenuItem("Name", "Gib den Anzeigenamen des Regaltypen an", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Modelname", "Gib den GTA Namen des Regals an. Leerlassen um eine Collission zu setzen!", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das angegeben Regal", "SUPPORT_CREATE_SHOP_SHELF", MenuItemStyle.green).needsConfirmation($"Regal erstellen?", "Wirklich erstellen?"));

            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            var listMenu = new Menu("Liste", "Was möchtest du tun?");
            foreach(var shelf in AllShelfs.Values.Concat(AllShelfsNoModel)) {
                var shelfVirtMenu = new VirtualMenu(shelf.Name, () => {
                    var shelfMenu = new Menu(shelf.Name, "Was möchtest du tun?");

                    shelfMenu.addMenuItem(new InputMenuItem("Item hinzufügen", "Füge ein Item zu dem Regal hinzu. Gib hierzu die Item-Id an", "Item-Id", "SUPPORT_SHOP_SHELF_ADD_ITEM", MenuItemStyle.green)
                        .needsConfirmation("Item hinzufügen?", "Wirklich hinzufügen?")
                        .withData(new Dictionary<string, dynamic> { { "Shelf", shelf } }));
                    
                    var colShapesMenu = new Menu("Collisions", "Welche möchtest du bearbeiten?");
                    colShapesMenu.addMenuItem(new ClickMenuItem("Erstellen", "Erstelle eine Collision", "", "SUPPORT_CREATE_COLLISION_SHAPE_FOR_SHELF", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Shelf", shelf } }));

                    foreach(var colShape in shelf.ColShapes) {
                        colShapesMenu.addMenuItem(new ClickMenuItem($"{colShape.Position.ToJson()}", colShape.toShortSave(), "", "SUPPORT_DELETE_COLLISION_SHAPE_FOR_SHELF", MenuItemStyle.red)
                            .needsConfirmation("Collision löschen?", "Wirklich löschen?")
                            .withData(new Dictionary<string, dynamic> { { "Shelf", shelf }, { "ColShape", colShape } }));
                    }
                    shelfMenu.addMenuItem(new MenuMenuItem(colShapesMenu.Name, colShapesMenu));
                    
                    var itemListMenu = new Menu("Liste", "welches möchtest du entfernen?");

                    foreach(var item in shelf.AvailableItems) {
                        itemListMenu.addMenuItem(new ClickMenuItem(item.name, "Entferne dieses Item aus dem Regal", "", "SUPPORT_SHOP_SHELF_REMOVE_ITEM", MenuItemStyle.red).needsConfirmation($"{item.name} entfernen?", "Wirklich entfernen?").withData(new Dictionary<string, dynamic> { { "Shelf", shelf }, { "Item", item } }));
                    }
                    shelfMenu.addMenuItem(new MenuMenuItem(itemListMenu.Name, itemListMenu));
                    shelfMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche das Regal", "", "SUPPORT_SHOP_SHELF_DELETE_SHELF", MenuItemStyle.red).needsConfirmation($"{shelf.Name} löschen?", "Wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Shelf", shelf } }));

                    return shelfMenu;
                });


                listMenu.addMenuItem(new MenuMenuItem(shelfVirtMenu.Name, shelfVirtMenu));
            }
            menu.addMenuItem(new MenuMenuItem(listMenu.Name, listMenu));
            return menu;
        }

        private bool onSupportCreateShelf(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var modelEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            if(modelEvt.input != null || modelEvt.input == "") {
                player.sendNotification(NotifactionTypes.Info, "Es wurde kein Modelname gesetzt, deswegen setze nun ein ", "");
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (position, width, height, rotation) => {
                    using(var db = new ChoiceVDb()) {
                        db.configshopshelfs.Add(new configshopshelf {
                            modelName = "",
                            colShapesStr = new List<string> { CollisionShape.Create(position, width, height, rotation, true, false, true).toShortSave() }.ToJson(),
                            name = nameEvt.input,
                        });

                        db.SaveChanges();
                    }
                    player.sendNotification(NotifactionTypes.Success, "Regal erfolgreich erstellt", "");
                    onLoadShopShelfs();
                });
            } else {
                using(var db = new ChoiceVDb()) {
                    var already = db.configshopshelfs.FirstOrDefault(s => s.modelName == modelEvt.input);

                    if(already != null) {
                        player.sendBlockNotification("Für dieses Objekt existiert schon ein Regal!", "");
                        return true;
                    } else {
                        db.configshopshelfs.Add(new configshopshelf {
                            modelName = modelEvt.input,
                            colShapesStr = new List<string>().ToJson(),
                            name = nameEvt.input,
                        });

                        db.SaveChanges();
                    }
                }
                player.sendNotification(NotifactionTypes.Success, "Regal erfolgreich erstellt", "");
            }
            onLoadShopShelfs();

            return true;
        }

        private bool onSupportAddShelfItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shelf = (ShopShelf)data["Shelf"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            using(var db = new ChoiceVDb()) {
                var dbShelf = db.configshopshelfs.Find(shelf.Id);
                var cfgItem = db.configitems.Find(int.Parse(evt.input));

                dbShelf.items.Add(cfgItem);

                db.SaveChanges();

                onLoadShopShelfs();

                player.sendNotification(NotifactionTypes.Success, $"{cfgItem.name} erfolgreich hinzugefügt", "");
            }



            return true;
        }

        private bool onSupportDeleteShelf(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shelf = (ShopShelf)data["Shelf"];

            AllShelfs.Remove(shelf.ModelName);

            using(var db = new ChoiceVDb()) {
                var dbShelf = db.configshopshelfs.Find(shelf.Id);
                db.configshopshelfs.Remove(dbShelf);
                db.SaveChanges();

                player.sendNotification(NotifactionTypes.Warning, "Regal erfolgreich gelöscht!", "");
            }

            return true;
        }

        private bool onSupportRemoveShelfItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var shelf = (ShopShelf)data["Shelf"];
            var item = (configitem)data["Item"];

            shelf.AvailableItems.Remove(item);

            using(var db = new ChoiceVDb()) {
                var dbShelf = db.configshopshelfs.Find(shelf.Id);
                dbShelf.items.Remove(item);

                db.SaveChanges();
                player.sendNotification(NotifactionTypes.Warning, $"{item.name} erfolgreich entfernt!", "");
            }

            return true;
        }
        
        private bool onCreateCollsionShapeForShelf(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var shelf = (ShopShelf)data["Shelf"];

            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (position, width, height, rotation) => {
                var shape = CollisionShape.Create(position, width, height, rotation, true, false, true);
                shelf.ColShapes.Add(shape);
                shape.OnCollisionShapeInteraction += shelf.onInteract;
                using(var db = new ChoiceVDb()) {
                    var dbShelf = db.configshopshelfs.Find(shelf.Id);
                    dbShelf.colShapesStr = shelf.ColShapes.Select(c => c.toShortSave()).ToJson();
                
                    db.SaveChanges();
                }
                
                player.sendNotification(NotifactionTypes.Success, "Collision erfolgreich erstellt", "");
                onLoadShopShelfs();
            });

            return true;
        }
        
        private bool onDeleteCollisionShapeForShelf(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var shelf = (ShopShelf)data["Shelf"];
            var colShape = (CollisionShape)data["ColShape"];
            
            shelf.ColShapes.Remove(colShape);
            colShape.Dispose();
            using(var db = new ChoiceVDb()) {
                var dbShelf = db.configshopshelfs.Find(shelf.Id);
                dbShelf.colShapesStr = shelf.ColShapes.Select(c => c.toShortSave()).ToJson();

                db.SaveChanges();
            }
            
            player.sendNotification(NotifactionTypes.Warning, "Collision erfolgreich gelöscht", "");
            onLoadShopShelfs();
            
            return true;
        }
        
        #endregion
    }
}
