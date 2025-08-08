using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Bogus.DataSets;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public class WorldBackpack {
        public int Id { get; private set; }
        public CollisionShape CollisionShape;
        public Object Object;
        public Inventory Inventory;

        private DateTime LastInteractDate;

        public WorldBackpack(Position position, Inventory inventory, float yaw, DateTime lastInteractDate, bool withDb = true, int dbId = -1) {
            CollisionShape = CollisionShape.Create(position, 1.25f, 1.25f, 0, true, false, true);
            CollisionShape.OnCollisionShapeInteraction += onInteraction;
            Object = ObjectController.createObject("prop_cs_heist_bag_01", position, new DegreeRotation(15, -10, yaw), 200, false);
            Inventory = inventory;
            LastInteractDate = lastInteractDate;

            if(withDb) {
                using(var db = new ChoiceVDb()) {
                    var dbBack = new placedbackpack {
                        inventoryId = inventory.Id,
                        position = position.ToJson(),
                        lastInteractDate = DateTime.Now,
                    };

                    db.placedbackpacks.Add(dbBack);

                    db.SaveChanges();
                    Id = dbBack.id;
                }
            } else {
                Id = dbId;
            }
        }

        private bool onInteraction(IPlayer player) {
            if(DateTime.Now - LastInteractDate > TimeSpan.FromHours(1)) {
                if(Id != -1) {
                    using(var db = new ChoiceVDb()) {
                        var dbBack = db.placedbackpacks.Find(Id);
                        if(dbBack != null) {
                            dbBack.lastInteractDate = DateTime.Now;
                            db.SaveChanges();
                        } else {
                            Logger.logError($"WorldbackopackInteraction: Db, Backpack not found! DbId: {Id}, InvId: {Inventory.Id}",
                            $"Fehler mit Rucksack beim Interagieren: {Id}, er wurde nicht gefunden!", player);
                        }
                    }
                }
            }

            var menu = new Menu("Rucksack", "Was möchtest du tun");
            menu.addMenuItem(new ClickMenuItem("Rucksack öffnen", "Öffne das Inventar den Rucksacks", "", "OPEN_BACKPACK_GROUND").withData(new Dictionary<string, dynamic> { { "WorldBackpack", this } }));
            menu.addMenuItem(new ClickMenuItem("Rucksack aufsetzen", "Setze den Rucksack auf", "", "PICK_UP_BACKPACK_GROUND").withData(new Dictionary<string, dynamic> { { "WorldBackpack", this } }));

            player.showMenu(menu);
            return true;
        }

        public void remove(bool removeInventory) {
            using(var db = new ChoiceVDb()) {
                var dbBack = db.placedbackpacks.Find(Id);
                if(dbBack != null) {
                    db.placedbackpacks.Remove(dbBack);
                    db.SaveChanges();
                } else {
                    Logger.logError($"WorldbackopackRemove: Db, Backpack not found! DbId: {Id}, InvId: {Inventory.Id}",
                        $"Fehler mit Rucksack beim automatischen Entfernen: {Id}, er wurde nicht gefunden!");
                }
            }

            CollisionShape.Dispose();
            ObjectController.deleteObject(Object);
            if(removeInventory) {
               InventoryController.destroyInventory(Inventory); 
            }

            CollisionShape = null;
            Object = null;
            Inventory = null;
        }
    }

    public class BackpackController : ChoiceVScript {
        public static List<WorldBackpack> AllWorldBackpack = new List<WorldBackpack>();

        public BackpackController() {
            EventController.addMenuEvent("PUT_ON_BACKPACK", OnPutOnBackpack);
            EventController.addMenuEvent("PUT_OFF_BACKPACK", OnPutOffBackpack);

            EventController.addMenuEvent("PUT_BACKPACK_ON_GROUND", OnPutBackpackOnGround);

            EventController.addMenuEvent("OPEN_BACKPACK", OnOpenBackpack);
            EventController.addMenuEvent("SHOW_BACKPACK", OnShowBackpack);

            EventController.addMenuEvent("OPEN_BACKPACK_GROUND", OnOpenBackpackGround);
            EventController.addMenuEvent("PICK_UP_BACKPACK_GROUND", OnPickUpBackpackGround);

            ClothingController.addOnConnectClothesCheck(1, checkForBackpack);

            EventController.MainReadyDelegate += onMainReady;

            EventController.addKeyEvent("BACKPACK_WALKING", ConsoleKey.Y, "Rucksack öffnen (zu Fuß)", onTryOpenBackpack);
        }

        private void onMainReady() {
            loadWorldBackpacks();
        }

        private void checkForBackpack(IPlayer player, ref ClothingPlayer cloth) {
            //Backpack
            if(!cloth.Bag.Equals(ClothingComponent.Empty)) {
                var item = player.getInventory().getItem<Backpack>(i => true);

                if(item != null) {
                    var worked = false;
                    var comp = item.BackpackContent.CurrentWeight;
                    if(player.getInventory().MaxWeight - (player.getInventory().CurrentWeight - comp) > 0 || player.getAdminLevel() > 0) {
                        item.IsEquipped = true;
                        cloth.UpdateClothSlot(5, 45, 0);
                        item.Weight = 0f;
                        worked = true;
                    }

                    if(!worked) {
                        ChoiceVAPI.KickPlayer(player, "BackTrack", "Versuche den Login erneut, bei weiterem Fehler melde dich im Support!", "Probleme mit dem Rucksackbefüllen");
                    }
                } else {
                    cloth.Bag = ClothingComponent.Empty;
                }
            }
        }

        private void loadWorldBackpacks() {
            using(var db = new ChoiceVDb()) {
                var toRemove = new List<placedbackpack>();
                foreach(var bag in db.placedbackpacks) {
                    if(bag.lastInteractDate + TimeSpan.FromDays(14) > DateTime.Now) {
                        AllWorldBackpack.Add(new WorldBackpack(bag.position.FromJson(), InventoryController.loadInventory(bag.inventoryId), bag.yaw, bag.lastInteractDate, false, bag.id));
                    } else {
                        toRemove.Add(bag);
                        InventoryController.destroyUnloadedInventory(bag.inventoryId);
                    }
                }

                if(toRemove.Count > 0) {
                    db.placedbackpacks.RemoveRange(toRemove);
                    db.SaveChanges();
                }
            }
        }

        public bool OnPutOnBackpack(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Backpack)data["Backpack"];
            var clothing = player.getClothing();
            if(clothing.Bag.Equals(ClothingComponent.Empty)) {
                var anim = AnimationController.getAnimationByName(Constants.EQUIP_ANIMATION);
                AnimationController.animationTask(player, anim, () => {
                    item.IsEquipped = true;
                    clothing.UpdateClothSlot(5, 45, 0);
                    item.Weight = 0.1f;

                    ClothingController.loadPlayerClothing(player, clothing);
                    item.Description = "Auf dem Rücken";
                });
            } else {
                player.sendBlockNotification("Du hast schon einen Rucksack auf!", "Rucksack schon auf!", Constants.NotifactionImages.System);
            }

            return true;
        }

        private bool OnPutOffBackpack(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Backpack)data["Backpack"];
            var clothing = player.getClothing();
            if(!clothing.Bag.Equals(ClothingComponent.Empty)) {
                var comb = item.BackpackContent.CurrentWeight;

                if((player.getInventory().MaxWeight - player.getInventory().CurrentWeight) >= comb) {
                    var anim = AnimationController.getAnimationByName(Constants.EQUIP_ANIMATION);
                    AnimationController.animationTask(player, anim, () => {
                        item.IsEquipped = false;
                        item.Weight = comb;

                        clothing.UpdateClothSlot(5, 0, 0);
                        ClothingController.loadPlayerClothing(player, clothing);
                        item.updateDescription();
                    });
                } else {
                    player.sendBlockNotification("Du hast nicht genug Platz im Inventar! Entferne einige Items oder lege den Rucksack ab!", "Inventar voll!");
                }
            } else {
                player.sendBlockNotification("Du hast keinen Rucksack auf. Es muss sich um einen Fehler handeln, bitte reconnecte!", "Du hast keine Rucksack!", Constants.NotifactionImages.System);
            }
            return true;
        }

        private bool OnPutBackpackOnGround(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.IsInVehicle) {
                player.sendBlockNotification("Im Fahrzeug nicht möglich!", "Nicht möglich", Constants.NotifactionImages.Car);
                return false;
            }
            
            if(player.Dimension != Constants.GlobalDimension) {
                player.sendBlockNotification("Du kannst hier keinen Rucksack ablegen!", "Nicht möglich", Constants.NotifactionImages.System);
                return true;
            }
            
            var item = (Backpack)data["Backpack"];
            var pos = player.Position;
            var vec = player.getForwardVector();
            var newPos = new Position(pos.X + vec.X * 0.22f, pos.Y + vec.Y * 0.22f, pos.Z);
            CallbackController.getGroundZFromPos(player, newPos, (p, z, b1, b2) => {
                if(!b1 && !b2) {
                    var anim = AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
                    AnimationController.animationTask(player, anim, () => {
                        var clothing = player.getClothing();
                        clothing.UpdateClothSlot(5, 0, 0);
                        ClothingController.loadPlayerClothing(player, clothing);

                        newPos.Z = z + 0.3f;
                        var backpack = new WorldBackpack(newPos, item.BackpackContent, 0, DateTime.Now);
                        AllWorldBackpack.Add(backpack);

                        item.IsEquipped = false;
                        if(!player.getInventory().removeItem(item)) {
                            backpack.remove(true);
                            Logger.logError($"OnPutBackpackOnGround: player tried to put backpack on ground which was not in his inventory! charId: {player.getCharacterId()}",
                                $"Fehler mit Rucksack beim Absetzen: Der Spieler hatte den Rucksack nicht im Inventar!", player);
                        }
                    });
                } else {
                    player.sendBlockNotification("Du kannst hier kein Rucksack platzieren!", "Platzierung nicht möglich!");
                }
            });

            return true;
        }

        private bool OnShowBackpack(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Backpack)data["Backpack"];
            InventoryController.showInventory(player, item.BackpackContent, true);
            return true;
        }

        private bool OnOpenBackpack(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Backpack)data["Backpack"];
            InventoryController.showMoveInventory(player, player.getInventory(), item.BackpackContent);
            return true;
        }

        private bool OnOpenBackpackGround(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var spot = (WorldBackpack)data["WorldBackpack"];
            InventoryController.showMoveInventory(player, player.getInventory(), spot.Inventory);
            return true;
        }

        private bool OnPickUpBackpackGround(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var spot = (WorldBackpack)data["WorldBackpack"];
            var clothing = player.getClothing();
            if(clothing.Bag.Equals(ClothingComponent.Empty)) {
                var cf = InventoryController.getConfigItemForType<Backpack>();
                var item = new Backpack(cf, spot.Inventory);
                item.Weight = 0.1f;
                if(player.getInventory().addItem(item)) {
                    var anim = AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
                    player.getRotationTowardsPosition(spot.CollisionShape.Position, true);
                    AnimationController.animationTask(player, anim, () => {
                        item.IsEquipped = true;
                        clothing.UpdateClothSlot(5, 45, 0);

                        ClothingController.loadPlayerClothing(player, clothing);
                        item.Description = "Auf dem Rücken";

                        spot.remove(false);
                        AllWorldBackpack.Remove(spot);
                    });
                } else {
                    player.sendBlockNotification("Es ist kein Platz im Inventar!", "Inventar voll!", Constants.NotifactionImages.System);
                }
            } else {
                player.sendBlockNotification("Du hast schon einen Rucksack auf!", "Schon Rucksack auf!", Constants.NotifactionImages.System);
            }

            return true;
        }

        private bool onTryOpenBackpack(IPlayer player, ConsoleKey key, string eventName) {
            if(!player.IsInVehicle) {
                var backPack = player.getInventory().getItem<Backpack>(b => b.IsEquipped);

                if(backPack != null) {
                    InventoryController.showMoveInventory(player, player.getInventory(), backPack.BackpackContent);
                    return true;
                }
            }

            return false;
        }
    }
}
