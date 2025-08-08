using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using Enum = System.Enum;
using Type = System.Type;
using ChoiceVServer.Controller.DamageSystem.Model;
using System.Collections.Concurrent;

namespace ChoiceVServer.Controller {
    public delegate bool OnInventoryMoveCallback(IPlayer player, Inventory fromInventory, Inventory toInventory, Item item, int amount);
    public delegate void OnInventoryCloseCallback(IPlayer player);

    public delegate void UnloadInventoryDelegate(Inventory fromInventory);

    public delegate Inventory InjectInventoryPredicateDelegate(IPlayer player);

    public delegate void ItemUsedDelegate(IPlayer player, Item item);

    public class InventoryController : ChoiceVScript {
        public static UnloadInventoryDelegate UnloadInventoryDelegate;
        public static ItemUsedDelegate ItemUsedDelegate;

        public static ConcurrentDictionary<int, Inventory> AllInventories = new();

        private static ConcurrentDictionary<int, configitem> AllConfigItems { get; set; }
        private static ConcurrentDictionary<string, configitem> AllConfigItemsByCodeIdentifier;
        private static ConcurrentDictionary<Type, List<configitem>> AllConfigItemsByTypes;

        public static List<InjectInventoryPredicateDelegate> AllInventorySpotInjects = new List<InjectInventoryPredicateDelegate>();

        public InventoryController() {
            AllConfigItems = new();

            loadConfigItems();

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnected;

            EventController.addCefEvent("USE_ITEM", onPlayerUseItem);
            EventController.addMenuEvent("CONFIRM_ITEM_USE", onConfirmItemUse);
            EventController.addCefEvent("EQUIP_ITEM", onPlayerEquipItem);
            EventController.addCefEvent("UNEQUIP_ITEM", onPlayerUnequipItem);
            EventController.addCefEvent("DELETE_ITEM", onPlayerDeleteItem);
            EventController.addCefEvent("MOVE_ITEM", onPlayerMoveItem);

            EventController.addCefEvent("GIVE_ITEM", onPlayerGiveItem);

            EventController.addCefEvent("INVENTORY_CLOSED", onPlayerInventoryClosed);

            EventController.addKeyEvent("OPEN_INVENTORY", ConsoleKey.I, "Inventar öffnen", onPlayerOpenInventory);

            EventController.addCollisionShapeEvent(Constants.OnPlayerInteractionInventorySpot, onInventorySpotInteraction);
            EventController.addEvent(Constants.OnPlayerCombinationLockAccessed, onInventorySpotAccessGranted);

            Inventory.InventoryAddItemDelegate += addItemToInv;
            Inventory.InventoryRemoveItemDelegate += removeItemFromInv;
            Inventory.ItemAmountChangeDelegate += amountChangeInv;
            Inventory.InventoryInventoryChangeItemDelegate += inventoryChangeInv;

            Item.ItemDataChange += onItemDataChange;
            Item.ItemDataRemoved += onItemDataRemoved;
            Item.ItemDescriptionChange += onItemDescriptionChange;
            Item.ItemRemoveTriggered += onItemRemoveTriggered;
            Item.ItemAmountChangeDelegate += onItemAmountChange;
            ToolItem.ItemWearChangeDelegate += onToolItemWearChange;

            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                   () => new ClickMenuItem("Spieler etwas geben", "Gib dem Spieler ein Item aus deinem Inventar", "", "SELECT_PLAYER_ITEM_GIVE"),
                   sender => sender is IPlayer player && !player.getBusy() && !player.hasState(PlayerStates.InTerminal),
                   target => target is IPlayer player && !player.getBusy()
                )
            );

            EventController.addMenuEvent("SELECT_PLAYER_ITEM_GIVE", onSelectPlayerItemGive);
        }

        private void onPlayerDisconnected(IPlayer player, string reason) {
            if(player.getCharacterFullyLoaded()) {
                var keepEquippedItems = player.getInventory()?.getItems<EquipItem>(i => i is not NoKeepEquippedItem && i.IsEquipped);
                foreach(var item in keepEquippedItems) {
                    item.Data["KeepEquipped"] = true;
                }

                unloadInventory(player);
            }
        }

        public static void loadConfigItems() {
            AllConfigItems = new();
            AllConfigItemsByCodeIdentifier = new();
            AllConfigItemsByTypes = new();

            using(var db = new ChoiceVDb()) {
                var cfgItems = db.configitems
                    .Include(i => i.itemAnimationNavigation)
                    .Include(i => i.configitemsfoodadditionalinfo)
                    .Include(i => i.configitemsfoodadditionalinfo.categoryNavigation)
                    .Include(i => i.configitemsitemcontainerinfoconfigItems).ToList();
                foreach(var row in cfgItems) {
                    AllConfigItems.Add(row.configItemId, row);
                    if(row.codeIdentifier != null) {
                        AllConfigItemsByCodeIdentifier.Add(row.codeIdentifier, row);
                    }

                    var type = Type.GetType("ChoiceVServer.InventorySystem." + row.codeItem);
                    if(type != null) {
                        if(AllConfigItemsByTypes.ContainsKey(Type.GetType("ChoiceVServer.InventorySystem." + row.codeItem))) {
                            AllConfigItemsByTypes[Type.GetType("ChoiceVServer.InventorySystem." + row.codeItem)].Add(row);
                        } else {
                            AllConfigItemsByTypes.Add(Type.GetType("ChoiceVServer.InventorySystem." + row.codeItem), new List<configitem> { row });
                        }
                    }
                }
            }
        }

        private void amountChangeInv(Item item) {
            if(item.StackAmount <= 0) {
                unregisterItem(item);
                return;
            }

            using(var db = new ChoiceVDb()) {
                var dbItem = db.items.Find(item.Id);
                if(dbItem != null) {
                    dbItem.amount = item.StackAmount;
                    db.SaveChanges();
                } else {
                    Logger.logError("amountChangeInv: item not found!",
                        $"Fehler beim Item Menge ändern: Das Item existiert in der DB nicht! Item-Id: {item.Id}, Item-Name: {item.Name}");
                }
            }
        }

        private void onItemAmountChange(Item item) {
            using(var db = new ChoiceVDb()) {
                var dbItem = db.items.Find(item.Id);
                if(dbItem != null) {
                    dbItem.amount = item.StackAmount;
                    db.SaveChanges();
                } else {
                    Logger.logError("amountChangeInv: item not found!",
                        $"Fehler beim Item Inventar ändern: Das Item existiert in der DB nicht! Item-Id: {item.Id}, Item-Name: {item.Name}");
                }
            }
        }

        private void onToolItemWearChange(ToolItem item, int newWear) {
            using(var db = new ChoiceVDb()) {
                var dbItem = db.items.Find(item.Id);
                if(dbItem != null) {
                    dbItem.wear = item.Wear;
                    db.SaveChanges();
                } else {
                    Logger.logError("amountChangeInv: item not found!",
                        $"Fehler beim Item Wear ändern: Das Item existiert in der DB nicht! Item-Id: {item.Id}, Item-Name: {item.Name}");
                }
            }
        }

        private void inventoryChangeInv(Inventory oldInventory, Inventory newInventory, Item item) {
            using(var db = new ChoiceVDb()) {
                var dbItem = db.items.Find(item.Id);
                if(dbItem != null) {
                    dbItem.inventoryId = newInventory.Id;
                    db.SaveChanges();
                } else {
                    Logger.logError("inventoryChangeInv: item not found!",
                        $"Fehler beim Item Inventar ändern: Das Item existiert in der DB nicht! Item-Id: {item.Id}, Item-Name: {item.Name}");
                }
            }
        }

        private void addItemToInv(Inventory inventory, Item item, int? amount, bool saveToDb) {
            if(!saveToDb) {
                return;
            }

            if(item.Id == null) {
                registerItem(inventory, item);
            } else {
                if(amount != null) {
                    using(var db = new ChoiceVDb()) {
                        var dbItem = db.items.Find(item.Id);
                        if(dbItem != null) {
                            dbItem.inventoryId = inventory.Id;
                            dbItem.amount = amount;
                            db.SaveChanges();
                        } else {
                            Logger.logError("addItemToInv: Item could not be found in database!",
                                $"Fehler beim Item zum Inventar hinzufügen: Das Item existiert in der DB nicht! Item-Id: {item.Id}, Item-Name: {item.Name}");
                        }
                    }
                }
            }
        }

        private void removeItemFromInv(Inventory inventory, Item item, int? newAmount, bool saveToDb) {
            if(!saveToDb) {
                return;
            }
            if(newAmount == null) {
                using(var db = new ChoiceVDb()) {
                    var dbItem = db.items.Find(item.Id);
                    if(dbItem != null) {
                        db.items.Remove(dbItem);
                        db.SaveChanges();
                    } else {
                        Logger.logError("removeItemToInv: Item could not be found in database!",
                            $"Fehler beim Item aus Inventar entfernen: Das Item existiert in der DB nicht! Item-Id: {item.Id}, Item-Name: {item.Name}");
                    }
                }
            } else {
                using(var db = new ChoiceVDb()) {
                    var dbItem = db.items.Find(item.Id);
                    if(dbItem != null) {
                        dbItem.amount = newAmount;
                        db.SaveChanges();
                    } else {
                        Logger.logError("removeItemToInv: Item could not be found in database!",
                            $"Fehler beim Item aus Inventar entfernen: Das Item existiert in der DB nicht! Item-Id: {item.Id}, Item-Name: {item.Name}");
                    }
                }
            }
        }

        private void registerItem(Inventory inventory, Item item) {
            var dbItem = new item {
                configId = item.ConfigId,
                inventoryId = inventory.Id,
                quality = item.Quality,
                durability = item.Durability,
                amount = item.StackAmount,
                description = item.Description,
            };

            if(item is ToolItem) {
                dbItem.wear = (item as ToolItem).Wear;
            }

            using(var db = new ChoiceVDb()) {
                db.items.Add(dbItem);

                if(item.Data != null) {
                    var allData = new List<itemsdatum>();
                    foreach(var pair in item.Data.Items) {
                        allData.Add(new itemsdatum {
                            item = dbItem,
                            parameter = pair.Key,
                            value = ((object)pair.Value).ToJson(),
                        });
                    }

                    dbItem.itemsdata = allData;
                }

                db.SaveChanges();
                item.Id = dbItem.id;
            }
        }

        private void onItemDataChange(Item item, string parameter, dynamic value) {
            if(item.Id != null) {
                using(var db = new ChoiceVDb()) {
                    var data = db.itemsdata.Find(parameter, item.Id);
                    if(data != null) {
                        data.value = ((object)value).ToJson();
                        db.SaveChanges();
                    } else {
                        var newDat = new itemsdatum {
                            itemId = item.Id ?? -1,
                            parameter = parameter,
                            value = ((object)value).ToJson(),
                        };
                        db.itemsdata.Add(newDat);
                        db.SaveChanges();
                    }
                }
            } else {
                //Initial Invoke can be ignored, data will be saved in db after add to inventory
            }
        }

        private bool onItemDataRemoved(Item item, string parameter) {
            if(item.Id != null) {
                using(var db = new ChoiceVDb()) {
                    db.itemsdata.Remove(db.itemsdata.Find(parameter, item.Id));
                    db.SaveChanges();
                }
            } else {
                //Initial Invoke can be ignored, data will be saved in db after add to inventory
            }

            return true;
        }

        private void onItemDescriptionChange(Item item) {
            if (item.Id != null) {
                using (var db = new ChoiceVDb()) {
                    var dbItem = db.items.Find(item.Id);
                    if (dbItem != null) {
                        dbItem.description = item.Description;
                        db.SaveChanges();
                    } else {
                        // Initial Invoke can be ignored, data will be saved in db after add to inventory
                    }
                }
            }
        }


        private void onItemRemoveTriggered(Item item) {
            unregisterItem(item);
        }

        /// <summary>
        /// Creates an Inventory which registers and saves it automaticlly
        /// </summary>
        public static Inventory createInventory(int ownerId, float maxWeight, InventoryTypes inventoryType) {
            using(var db = new ChoiceVDb()) {
                var dbInv = new inventory {
                    ownerId = ownerId,
                    maxWeight = maxWeight,
                    inventoryType = (int)inventoryType,
                };
                db.inventories.Add(dbInv);
                db.SaveChanges();

                var inv = new Inventory(dbInv.id, ownerId, maxWeight, inventoryType);

                AllInventories.Add(dbInv.id, inv);

                return inv;
            }
        }

        /// <summary>
        /// Inventory.removeItem calls this method automaticly! Removes a item from the Database. It is not removed from a inventoryModel.
        /// </summary>
        public static void unregisterItem(Item item, bool removeFromInventory = true) {
            using(var db = new ChoiceVDb()) {
                var dbItem = db.items.FirstOrDefault(i => i.id == item.Id);

                if(dbItem != null) {
                    db.items.Remove(dbItem);
                    db.SaveChanges();

                    if(AllInventories.ContainsKey(dbItem.inventoryId)) {
                        var inv = AllInventories[dbItem.inventoryId];
                        if(removeFromInventory) {
                            inv.simpleRemoveItem(item);
                        }
                    } else {
                        Logger.logError($"registerItem: Tried to unregister item of Inventory that is not loaded or has been removed: itemId {item.Id}",
                            $"Fehler beim Item entfernen: Das Inventar vom Item existiert nicht! Inventar-Id: {dbItem.inventoryId}, Id: {item}, Item-Typ: {item.GetType().Name}");
                    }
                } else {
                    Logger.logError($"registerItem: Tried to unregister item that doesnt exist: {item.ToJson()}",
                            $"Fehler beim Item entfernen: Das Item existiert nicht! Id: {item}, Item-Typ: {item.GetType().Name}");
                }
            }
        }

        /// <summary>
        /// Loads the inventory for a specific player
        /// </summary>
        public static Inventory loadInventory(IPlayer player) {
            inventory inv;

            using(var db = new ChoiceVDb()) {
                inv = db.inventories.FirstOrDefault(i => i.ownerId == player.getCharacterId() && (InventoryTypes)i.inventoryType == InventoryTypes.Player);
            }

            var playerInv = loadInventory(inv);

            return playerInv;
        }

        /// <summary>
        /// Loads the inventory for a specific vehicle
        /// </summary>
        public static Inventory loadInventory(vehicle vehicle) {
            inventory inv;

            using(var db = new ChoiceVDb()) {
                inv = db.inventories.FirstOrDefault(i => i.ownerId == vehicle.id && (InventoryTypes)i.inventoryType == InventoryTypes.Vehicle);
            }

            return loadInventory(inv);
        }

        /// <summary>
        /// Loads the inventory by inventory Id
        /// </summary>
        public static Inventory loadInventory(int inventoryId) {
            if(AllInventories.ContainsKey(inventoryId)) {
                return AllInventories[inventoryId];
            }

            inventory inv;

            using(var db = new ChoiceVDb()) {
                inv = db.inventories.FirstOrDefault(i => i.id == inventoryId);
            }

            return loadInventory(inv);
        }

        /// <summary>
        /// USE loadInventory overloads if you do not have the row loaded already. Loads a inventory by inventories database row.
        /// </summary>
        public static Inventory loadInventory(inventory inv) {
            using(var db = new ChoiceVDb()) {
                //var inv = db.inventories.FirstOrDefault(i => i.id == inventoryId);
                if(inv == null) {
                    return null;
                }

                if(AllInventories.ContainsKey(inv.id)) {
                    return AllInventories[inv.id];
                }

                var inventory = new Inventory(inv.id, inv.ownerId, inv.maxWeight, (InventoryTypes)inv.inventoryType);
                var items = db.items
                    .Include(i => i.config)
                    .Include(i => i.config.configitemsfoodadditionalinfo)
                    .Include(i => i.config.configitemsfoodadditionalinfo.categoryNavigation)
                    .Include(i => i.config.configitemsitemcontainerinfoconfigItems)
                    .Include(i => i.config.itemAnimationNavigation)
                    .Include(i => i.itemsdata).Where(it => it.inventoryId == inv.id);
                foreach(var item in items) {
                    Item newItem = null;
                    try {
                        newItem = loadItem(item);
                    } catch(Exception e) {
                        Logger.logException(e, "loadInventory: Item could not be loaded");
                    }

                    if(newItem != null) {
                        inventory.addItem(newItem, true, false);
                    }
                }

                AllInventories.Add(inventory.Id, inventory);

                return inventory;
            }
        }

        /// <summary>
        /// Unloads the inventory for a specific player
        /// </summary>
        public static void unloadInventory(IPlayer player) {
            if(player.getInventory() != null) {
                unloadInventory(player.getInventory());
            }
        }

        /// <summary>
        /// Unloads the inventory for a specific vehicle
        /// </summary>
        public static void unloadInventory(ChoiceVVehicle vehicle) {
            unloadInventory(vehicle.Inventory);
        }

        /// <summary>
        /// Unloads the inventory by inventory Id
        /// </summary>
        public static void unloadInventory(int inventoryId) {
            if(AllInventories.Keys.Contains(inventoryId)) {
                unloadInventory(AllInventories.Keys.FirstOrDefault(i => i == inventoryId));
            } else {
                Logger.logError($"unloadInventory: No inventory with that Id found: Id: {inventoryId}",
                        $"Fehler beim Inventar ausladen: Das Inventar war nicht geladen! Id: {inventoryId}");
            }
        }
        /// <summary>
        /// Removes a Inventory from the System
        /// </summary>
        public static void unloadInventory(Inventory inv) {
            foreach(var item in inv.getAllItems()) {
                item.onUnloaded();

                if(item is InventoryAuraItem aura) {
                    aura.onExitInventory(inv, true);
                }
            }

            if(AllInventories.ContainsKey(inv.Id)) {
                UnloadInventoryDelegate?.Invoke(inv);
                AllInventories.Remove(inv.Id);
                inv.Dispose();
                Logger.logDebug(LogCategory.System, LogActionType.Removed, $"unloadInventory: Inventory unloaded: Id: {inv.Id}");
            } else {
                Logger.logError($"unloadInventory: Tried to unload Inventory which was not loaded! {inv.ToJson()}",
                        $"Fehler beim Inventar ausladen: Das Inventar war nicht geladen! Id: {inv.Id}");
            }
        }

        public static void destroyInventory(Inventory inv) {
            if(inv == null) {
                return;
            }

            try {
                foreach(var item in inv.getAllItems().Reverse<Item>()) {
                    item.destroy(false);
                }

                using(var db = new ChoiceVDb()) {
                    var ids = inv.Items.Distinct().Select(i => i.Id).ToList();
                    var items = db.items.Where(i => ids.Contains(i.id));
                    db.RemoveRange(items);

                    var dbInv = db.inventories.FirstOrDefault(i => i.id == inv.Id);
                    if(dbInv != null) {
                        db.inventories.Remove(dbInv);
                    } else {
                        Logger.logError($"DbInv of destroyed inv not found!: {inv.Id}",
                            $"Fehler beim Inventar entfernen: Das Inventar existiert in der DB nicht! Inventar-Id: {inv.Id}");
                    }
                    db.SaveChanges();

                    unloadInventory(inv);
                }
            } catch(Exception e) {
                Logger.logException(e, "destroyInventory: Inventory could not be removed");
            }
        }


        public static void destroyInventory(vehicle vehicle) {
            using(var db = new ChoiceVDb()) {
                var inv = db.inventories.FirstOrDefault(i => i.ownerId == vehicle.id && (InventoryTypes)i.inventoryType == InventoryTypes.Vehicle);

                if(inv != null) {
                    db.inventories.Remove(inv);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Be sure when using this inventory that it is not loaded anymore! Otherwise an exception will be thrown!
        /// </summary>
        /// <param name="inventoryId"></param>
        public static void destroyUnloadedInventory(int inventoryId) {
            if(AllInventories.ContainsKey(inventoryId)) {
                throw new Exception("destroyUnloadedInventory: Inventory is still loaded!");
            }

            using(var db = new ChoiceVDb()) {
                var dbInv = db.inventories.FirstOrDefault(i => i.id == inventoryId);
                if(dbInv != null) {
                    db.inventories.Remove(dbInv);
                } else {
                    Logger.logError($"DbInv of destroyed inv not found!: {inventoryId}",
                            $"Fehler beim Inventar entfernen: Das Inventar existiert in der DB nicht! Inventar-Id: {inventoryId}");
                }
                db.SaveChanges();
            }
        }
        
        public static List<configitem> getAllConfigItems() {
            return AllConfigItems.Values.ToList();
        }

        public static configitem getConfigById(int id) {
            if(!AllConfigItems.ContainsKey(id)) {
                Logger.logError($"getConfigById: ConfigItem could not be found: {id}",
                    $"Fehler beim ConfigItem laden: Das ConfigItem existiert nicht! Id: {id}");
                return null;
            }
            
            return AllConfigItems[id];
        }

        public static configitem getConfigItem(Predicate<configitem> predicate) {
            try {
                return AllConfigItems.Values.FirstOrDefault(c => predicate(c));
            } catch(Exception e) {
                Logger.logException(e, "getConfigItemForType: ConfigItem could not be found");
                return null;
            }
        }

        public static IEnumerable<configitem> getConfigItems(Predicate<configitem> predicate) {
            try {
                return AllConfigItems.Values.Where(c => predicate(c));
            } catch(Exception e) {
                Logger.logException(e, "getConfigItemForType: ConfigItem could not be found");
                return null;
            }
        }

        public static configitem getConfigItemForType<T>() {
            if(AllConfigItemsByTypes.ContainsKey(typeof(T))) {
                return AllConfigItemsByTypes[typeof(T)].FirstOrDefault();
            } else {
                return null;
            }
        }

        public static IEnumerable<configitem> getConfigItemsForType<T>() {
            if(AllConfigItemsByTypes.ContainsKey(typeof(T))) {
                return AllConfigItemsByTypes[typeof(T)];
            } else {
                return null;
            }
        }

        public static configitem getConfigItemForType(Type type, Predicate<configitem> predicate) {
            try {
                if(AllConfigItemsByTypes.ContainsKey(type)) {
                    return AllConfigItemsByTypes[type].FirstOrDefault(i => predicate(i));
                } else {
                    return null;
                }
            } catch(Exception e) {
                Logger.logException(e, "getConfigItemForType: ConfigItem could not be found");
                return null;
            }
        }

        public static configitem getConfigItemForType<T>(Predicate<configitem> predicate) {
            if(AllConfigItemsByTypes.ContainsKey(typeof(T))) {
                return AllConfigItemsByTypes[typeof(T)].FirstOrDefault(i => predicate(i));
            } else {
                return null;
            }
        }

        public static IEnumerable<configitem> getConfigItemsForType<T>(Predicate<configitem> predicate) {
            try {
                if(AllConfigItemsByTypes.ContainsKey(typeof(T))) {
                    return AllConfigItemsByTypes[typeof(T)].Where(c => c.codeItem == typeof(T).Name && predicate(c));
                } else {
                    return null;
                }
            } catch(Exception e) {
                Logger.logException(e, "getConfigItemForType: ConfigItem could not be found");
                return null;
            }
        }

        public static configitem getConfigItemByCodeIdentifier(string codeIdentifier) {
            try {
                return AllConfigItemsByCodeIdentifier[codeIdentifier];
            } catch(Exception e) {
                Logger.logException(e, "getConfigItemByCodeIdentifier: ConfigItem could not be found");
                return null;
            }
        }

        public static InputMenuItem getConfigItemSelectMenuItem(string name, string evt, bool addNoneOption = false) {
            var options = AllConfigItems.Values.Select(i => $"{i.configItemId}: {i.name}").ToList(); 
            if(addNoneOption) {
                options.Insert(0, "-1: Kein Item");
            }
            
            return new InputMenuItem(name, "Wähle ein ConfigItem aus", "", evt).withOptions(options.ToArray());
        }

        public static configitem getConfigItemFromSelectMenuItemInput(string input) {
            if(string.IsNullOrEmpty(input)) {
                return null;
            }
            
            var id = input.Split(":")[0];
            if(id == "-1") {
                return null;
            } else {
                return getConfigById(int.Parse(id));
            }
        }

        public static Item createItem(configitem configItem, int amount, int quality = -1) {
            var type = Type.GetType("ChoiceVServer.InventorySystem." + configItem.codeItem, false);
            if(type == null) {
                type = Type.GetType("ChoiceVServer.InventorySystem.StaticItem");
            }

            return Activator.CreateInstance(type, configItem, amount, quality) as Item;
        }

        public static List<Item> createItems(configitem configItem, int amount, int quality = -1, Dictionary<string, dynamic> createDate = null) {
            if(amount <= 0) {
                return null;
            }

            var type = Type.GetType("ChoiceVServer.InventorySystem." + configItem.codeItem, false);
            if(type == null) {
                type = Type.GetType("ChoiceVServer.InventorySystem.StaticItem");
            }

            var list = new List<Item>();

            var firstItem = Activator.CreateInstance(type, configItem, amount, quality) as Item;
            firstItem.setCreateData(createDate);
            list.Add(firstItem);
            
            if(firstItem.StackAmount == null) {
                var counter = 0;
                while(amount - 1 > counter) {
                    counter++;
                    var item = Activator.CreateInstance(type, configItem, amount, quality) as Item;
                    item.setCreateData(createDate);
                    list.Add(item);
                }
            }

            return list;
        }

        public static Item createGenericItem(configitem configItem) {
            var type = Type.GetType("ChoiceVServer.InventorySystem." + configItem.codeItem, false);
            if(type == null) {
                type = Type.GetType("ChoiceVServer.InventorySystem.StaticItem");
            }

            var itemObject = Activator.CreateInstance(type, configItem) as Item;

            itemObject.processAdditionalInfo(configItem.additionalInfo);

            itemObject.check();

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"Type {itemObject.GetType()} item createGenericItem: id {itemObject.Id}");

            return itemObject;
        }

        public static Item createGenericStackableItem(configitem configItem, int amount, int quality) {
            var type = Type.GetType("ChoiceVServer.InventorySystem." + configItem.codeItem, false);
            if(type == null) {
                type = Type.GetType("ChoiceVServer.InventorySystem.StaticItem");
            }

            var itemObject = Activator.CreateInstance(type, configItem, amount, quality) as Item;

            itemObject.processAdditionalInfo(configItem.additionalInfo);

            itemObject.check();

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"Type {itemObject.GetType()} item createGenericItem: id {itemObject.Id}");

            return itemObject;
        }

        private static Item loadItem(item item) {
            var type = Type.GetType("ChoiceVServer.InventorySystem." + item.config.codeItem, false);
            if(type == null) {
                type = Type.GetType("ChoiceVServer.InventorySystem.StaticItem");
            }

            var itemObject = Activator.CreateInstance(type, item) as Item;

            //If detectable for metal scanners
            if(item.config.detectable == 1) {
                itemObject.Detectable = true;
            }

            itemObject.processAdditionalInfo(item.config.additionalInfo);

            itemObject.check();

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"Type {itemObject.GetType().ToString()} item loaded: id {item.id}");

            return itemObject;
        }

        /// <summary>
        /// Shows a player a inventory where only interaction with the items is possible
        /// </summary>
        /// <param name="onlyStatic">If true the inventory is not interactable with</param>
        /// <param name="giveItemTarget">If not -1 the id it the target player for given items</param>
        public static void showInventory(IPlayer player, Inventory inventory, bool onlyStatic = false, int giveItemTarget = -1) {
            try {
                var items = new List<HtmlInventoryItemModel>();

                foreach(var item in inventory.Items) {
                    var it = items.FirstOrDefault(i => i.name == item.Name
                    && i.quality == item.Quality
                    && (i.description == item.Description || (i.description == "" && item.Description == null))
                    && (item is not EquipItem || (item is EquipItem && i.isEquipped == (item as EquipItem).IsEquipped))
                    && i.additionalInfo == item.getInventoryAdditionalInfo());
                    if(it != null) {
                        it.amount++;
                    } else {
                        items.Add(new HtmlInventoryItemModel {
                            configId = item.ConfigId,
                            name = item.Name,
                            category = item.Category,
                            quality = item.Quality,
                            //If Item is stackable send right Amount down
                            amount = item.StackAmount ?? 1,
                            description = item.Description != null ? item.Description : "",
                            weight = item.Weight,
                            isEquipped = item is EquipItem ? (item as EquipItem).IsEquipped : false,
                            equipSlot = item is EquipItem equip && equip.canBeEquippedCharacterTypes().Contains(player.getCharacterType()) ? equip.EquipType : "",
                            useable = item.Type > 0 && item.Type != ItemTypes.Accessoire && item.Type != ItemTypes.Tool && item.Type != ItemTypes.Static && item.canBeUsedByCharacterTypes().Contains(player.getCharacterType()),
                            SortClass = getSortClass(item.Type, item is EquipItem ? (item as EquipItem).IsEquipped : false),
                            additionalInfo = item.getInventoryAdditionalInfo()
                        });

                    }
                }

                items = items.OrderBy(x => x.SortClass).ThenBy(x => x.category).ThenBy(x => x.name).ThenBy(x => x.description).ToList();

                var evt = new SingleInventoryEvent(inventory.Id, inventory.MaxWeight, items, onlyStatic, player.getCharacterData().Cash, "TODO", player.getLastInfo(), giveItemTarget);
                evt.setHealthData(player.getCharacterData().CharacterDamage);
                player.emitCefEventWithBlock(evt, "INVENTORY");


                //CharacterData
                var data = player.getCharacterData();
                player.emitClientEvent(Constants.PlayerSetCharacterInfo, data.Cash.ToString());
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        /// <summary>
        /// Shows a player two inventories on which movement of items will be saved. USE THE LEFT INVENTORY FOR THE PLAYERS INVENTORY
        /// </summary>
        /// <param name="potentialMoveInfo">A callback called when a move action is performed</param>
        public static void showMoveInventory(IPlayer player, Inventory leftInventory, Inventory rightInventory, OnInventoryMoveCallback potentialMoveInfo = null, OnInventoryCloseCallback potentialCloseInfo = null, string rightName = "", bool showRightSearchbar = false) {
            var leftItems = new List<HtmlInventoryItemModel>();
            var rightItems = new List<HtmlInventoryItemModel>();

            foreach(var item in leftInventory.Items) {
                var it = leftItems.FirstOrDefault(i => i.name == item.Name
                && i.quality == item.Quality
                && (i.description == item.Description || (i.description == "" && item.Description == null))
                && (item is not EquipItem || (item is EquipItem && i.isEquipped == (item as EquipItem).IsEquipped))
                && i.additionalInfo == item.getInventoryAdditionalInfo());

                if(it != null) {
                    it.amount = it.amount + 1;
                } else {
                    leftItems.Add(new HtmlInventoryItemModel {
                        configId = item.ConfigId,
                        name = item.Name,
                        quality = item.Quality,
                        category = item.Category,
                        amount = item.StackAmount ?? 1,
                        description = item.Description != null ? item.Description : "",
                        weight = item.Weight,
                        isEquipped = item is EquipItem ? (item as EquipItem).IsEquipped : false,
                        equipSlot = item is EquipItem equip && equip.canBeEquippedCharacterTypes().Contains(player.getCharacterType()) ? equip.EquipType : "",
                        useable = item.Type > 0 && item.Type != ItemTypes.Accessoire && item.Type != ItemTypes.Tool && item.Type != ItemTypes.Static && item.canBeUsedByCharacterTypes().Contains(player.getCharacterType()),
                        SortClass = getSortClass(item.Type, item is EquipItem ? (item as EquipItem).IsEquipped : false),
                        additionalInfo = item.getInventoryAdditionalInfo()
                    });
                }
            }

            foreach(var item in rightInventory.Items) {
                var it = rightItems.FirstOrDefault(i => i.name == item.Name
                && i.quality == item.Quality
                && (i.description == item.Description || (i.description == "" && item.Description == null))
                && (item is not EquipItem || (item is EquipItem && i.isEquipped == (item as EquipItem).IsEquipped))
                && i.additionalInfo == item.getInventoryAdditionalInfo());

                if(it != null) {
                    it.amount = it.amount + 1;
                } else {
                    rightItems.Add(new HtmlInventoryItemModel {
                        configId = item.ConfigId,
                        name = item.Name,
                        quality = item.Quality,
                        category = item.Category,
                        amount = item.StackAmount ?? 1,
                        description = item.Description != null ? item.Description : "",
                        weight = item.Weight,
                        isEquipped = item is EquipItem ? (item as EquipItem).IsEquipped : false,
                        equipSlot = item is EquipItem equip && equip.canBeEquippedCharacterTypes().Contains(player.getCharacterType()) ? equip.EquipType : "",
                        useable = item.Type > 0 && item.Type != ItemTypes.Accessoire && item.Type != ItemTypes.Tool && item.Type != ItemTypes.Static && item.canBeUsedByCharacterTypes().Contains(player.getCharacterType()),
                        SortClass = getSortClass(item.Type, item is EquipItem ? (item as EquipItem).IsEquipped : false),
                        additionalInfo = item.getInventoryAdditionalInfo()
                    });
                }
            }

            leftItems = leftItems.OrderBy(x => x.SortClass).ThenBy(x => x.category).ThenBy(x => x.name).ThenBy(x => x.description).ToList();
            rightItems = rightItems.OrderBy(x => x.SortClass).ThenBy(x => x.category).ThenBy(x => x.name).ThenBy(x => x.description).ToList();

            if(player.hasData("MOVE_INVENTORY_INFO")) {
                player.resetData("MOVE_INVENTORY_INFO");
            }

            if(player.hasData("CLOSE_INVENTORY_INFO")) {
                player.resetData("CLOSE_INVENTORY_INFO");
            }

            if(potentialMoveInfo != null) {
                player.setData("MOVE_INVENTORY_INFO", potentialMoveInfo);
            }

            if(potentialCloseInfo != null) {
                player.setData("CLOSE_INVENTORY_INFO", potentialCloseInfo);
            }

            player.emitCefEventWithBlock(new DoubleInventoryEvent(leftInventory.Id, rightInventory.Id, rightName, showRightSearchbar, leftInventory.MaxWeight, rightInventory.MaxWeight, leftItems, rightItems), "INVENTORY");
            //player.emitCefEvent(Constants.PlayerOpenInventory, leftInventory.Id, rightInventory.Id, leftItems.ToArray().ToJson(), rightItems.ToArray().ToJson(), leftInventory.MaxWeight, rightInventory.MaxWeight);
        }

        private bool onSelectPlayerItemGive(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var baseType = (BaseObjectType)data["InteractionTargetBaseType"];
            var targetId = (int)data["InteractionTargetId"];

            showInventory(player, player.getInventory(), false, targetId);

            return true;
        }

        #region Events

        public class GiveItemEvent {
            public string item;
            public int amount;
            public int invId;
            public int giveItemTarget;
        }

        private void onPlayerGiveItem(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(player.getBusy()) {
                Logger.logTrace(LogCategory.Player, LogActionType.Blocked, player, $"Player tried to use item while he was busy");
                return;
            }

            if(player.hasState(PlayerStates.InTerminal)) {
                player.sendBlockNotification("Du kannst im Terminal niemandem etwas geben!", "Deaktivierte Aktion", NotifactionImages.Plane);
                player.closeInventory();
                return;
            }

            var cefData = new GiveItemEvent();
            JsonConvert.PopulateObject(evt.Data, cefData);
            var htmlItem = cefData.item.FromJson<HtmlInventoryItemModel>();


            if(htmlItem.description == "") {
                htmlItem.description = null;
            }

            if(!AllInventories.ContainsKey(cefData.invId)) {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerGiveItem: An inventory was not found!");
                player.closeInventory();
                return;
            }

            var inv = AllInventories[cefData.invId];

            //Players cannot use items that are not in their inventory!
            if(player.getInventory() != inv) {
                return;
            }

            var item = inv.getItem(htmlItem.configId, i => !i.isBlocked() && htmlItem.isSimelarTo(i));
            if(item != null) {
                var target = ChoiceVAPI.FindPlayerByCharId(cefData.giveItemTarget);
                if(target != null) {
                    if(target.Position.Distance(player.Position) < 3) {
                        if(target.getBusy() || !player.getInventory().moveItems(target.getInventory(), item, cefData.amount)) {
                            player.sendBlockNotification("Du konntest der Person das Item nicht geben!", "Geben fehlgeschlagen", Constants.NotifactionImages.System);
                        } else {
                            var anim = AnimationController.getAnimationByName("GIVE_STUFF");
                            player.playAnimation(anim);

                            var itemStr = "Unbekanntes Objekt";
                            if(item.Weight >= 0.5f) {
                                itemStr = item.Name;
                            }

                            CamController.checkIfCamSawAction(player, "Objekt übergeben", $"Die Person hat einer anderen Person {cefData.amount}x {itemStr} übergeben");
                            CamController.checkIfCamSawAction(target, "Objekt erhalten", $"Die Person von einer anderen Person {cefData.amount}x {itemStr} erhalten");

                            player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast der Person {cefData.amount} {item.Name} gegeben", $"{item.Name} gegeben", Constants.NotifactionImages.System);
                            target.sendNotification(Constants.NotifactionTypes.Info, $"Du hast {cefData.amount} {item.Name} erhalten", $"{item.Name} erhalten", Constants.NotifactionImages.System);
                            return;
                        }
                    } else {
                        player.sendBlockNotification("Die Person ist zu weit entfernt!", "Geben fehlgeschlagen", Constants.NotifactionImages.System);
                    }
                } else {
                    Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerGiveItem: player tried to give item to player which was not there: charId: {player.getCharacterId()}, data:{evt.Data}");
                    player.sendBlockNotification("Die Person ist weg!", "Geben fehlgeschlagen", Constants.NotifactionImages.System);
                }
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerGiveItem: player tried to give item he did not have: charId: {player.getCharacterId()}, data:{evt.Data}");
                player.sendBlockNotification("Item nicht gefunden!", "Geben fehlgeschlagen", Constants.NotifactionImages.System);
            }

            player.closeInventory();
        }

        public class UseItemEvent {
            public string item;
            public int amount;
            public int invId;
        }

        private void onPlayerUseItem(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(player.getBusy()) {
                Logger.logTrace(LogCategory.Player, LogActionType.Blocked, player, $"Player tried to use item while he was busy");
                return;
            }

            var cefData = new UseItemEvent();
            JsonConvert.PopulateObject(evt.Data, cefData);
            var htmlItem = cefData.item.FromJson<HtmlInventoryItemModel>();
            
            if(htmlItem.description == "") {
                htmlItem.description = null;
            }

            if(!AllInventories.ContainsKey(cefData.invId)) {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"onPlayerUseItem: An inventory was not found!");
                player.closeInventory();
                return;
            }

            var inv = AllInventories[cefData.invId];

            //Players cannot use items that are not in their inventory!
            if(player.getInventory() != inv) {
                return;
            }

            var item = inv.getItem(htmlItem.configId, i => htmlItem.isSimelarTo(i));
            if(item != null) {
                player.closeInventory();

                if(!item.UsingHasToBeConfirmed) {
                    useItem(player, item);
                } else {
                    player.showMenu(MenuController.getConfirmationMenu($"{item.Name} verwenden?", "Wirklich benutzen?", "CONFIRM_ITEM_USE", new Dictionary<string, dynamic> { { "Item", item } }));
                }
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerUseItem: Player tried to use item which was not found! Htmlitem: {htmlItem.ToJson()}");
            }
        }

        private bool onConfirmItemUse(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            useItem(player, (Item)data["Item"]);
            return true;
        }

        private void useItem(IPlayer player, Item item) {
            if(item is NoEquipSlotItem) {
                (item as EquipItem).evaluateEquip(player);
            } else {
                ItemUsedDelegate?.Invoke(player, item);
                item.evaluateUse(player);
            }
        }

        public class PlayerEquipItemEvent {
            public string item;
            public int amount;
            public int invId;
        }

        private void onPlayerEquipItem(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(player.getBusy()) {
                Logger.logTrace(LogCategory.Player, LogActionType.Blocked, player, $"Player tried to use item while he was busy");
                return;
            }

            var cefData = new PlayerEquipItemEvent();
            JsonConvert.PopulateObject(evt.Data, cefData);
            var htmlItem = cefData.item.FromJson<HtmlInventoryItemModel>();

            if(htmlItem.description == "") {
                htmlItem.description = null;
            }

            if(!AllInventories.ContainsKey(cefData.invId)) {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerUseItem: An inventory was not found!");
                player.closeInventory();
                return;
            }

            var inv = AllInventories[cefData.invId];

            //Players cannot use items that are not in their inventory!
            if(player.getInventory() != inv) {
                return;
            }

            var item = inv.getItem<EquipItem>(i => !i.isBlocked() && htmlItem.isSimelarTo(i));
            //player.closeInventory();
            //ITEM ALREADY USED
            if(item != null) {
                player.closeInventory();
                
                item.evaluateEquip(player);
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerUseItem: Player tried to use item which was not found! Htmlitem: {htmlItem.ToJson()}");
            }
        }

        public class PlayerUnequipItemEvent {
            public string item; //Is actually equipSlot!
            public int invId;
        }

        private void onPlayerUnequipItem(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(player.getBusy()) {
                Logger.logTrace(LogCategory.Player, LogActionType.Blocked, player, $"Player tried to use item while he was busy");
                return;
            }

            var cefData = new PlayerUnequipItemEvent();
            JsonConvert.PopulateObject(evt.Data, cefData);

            if(!AllInventories.ContainsKey(cefData.invId)) {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onItemThrowAway: An inventory was not found!");
                player.closeInventory();
                return;
            }
            var slot = cefData.item.Trim('"');
            var inv = AllInventories[cefData.invId];

            var item = inv.getItem<EquipItem>(i => i.EquipType == slot && i.IsEquipped);

            if(item != null) {
                item.evaluateEquip(player);
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerUnequipItem: The item the player tried to unequip was not in his inventory!");
            }
        }

        public class PlayerThrowAwayItemEvent {
            public string item;
            public int amount;
            public int invId;
        }

        private void onPlayerDeleteItem(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(player.getBusy()) {
                Logger.logTrace(LogCategory.Player, LogActionType.Blocked, player, $"Player tried to use item while he was busy");
                return;
            }

            var cefData = new PlayerThrowAwayItemEvent();
            JsonConvert.PopulateObject(evt.Data, cefData);
            var htmlItem = cefData.item.FromJson<HtmlInventoryItemModel>();

            if(htmlItem.description == "") {
                htmlItem.description = null;
            }

            if(!AllInventories.ContainsKey(cefData.invId)) {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onItemThrowAway: An inventory was not found!");
                player.closeInventory();
                return;
            }

            var inv = AllInventories[cefData.invId];

            //Players cannot use items that are not in their inventory!
            if(player.getInventory() != inv) {
                return;
            }

            var item = inv.getItem(htmlItem.configId, i => !i.isBlocked() &&htmlItem.isSimelarTo(i));
            if(item != null) {
                if(!item.isBlocked()) {
                    player.closeInventory();
                    TrashController.playerThrowItemAway(player, item, cefData.amount);
                } else {
                    player.sendBlockNotification("Das Item ist blockiert!", "Item blockiert", Constants.NotifactionImages.System);
                }
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerItemThrowAway: Player tried to throw away item which was not found! Htmlitem: {htmlItem.ToJson()}");
            }
        }

        public class PlayerMoveItemEvent {
            public string item;
            public int amount;
            public int fromInvId;
            public int toInvId;
        }

        private void onPlayerMoveItem(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PlayerMoveItemEvent();
            JsonConvert.PopulateObject(evt.Data, cefData);
            var htmlItem = cefData.item.FromJson<HtmlInventoryItemModel>();

            var amount = cefData.amount;
            Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"ItemTryMoved: item: {htmlItem.ToJson()} amount: {amount} fromdId: {cefData.fromInvId} toId: {cefData.toInvId}");
            if(!AllInventories.ContainsKey(cefData.fromInvId) || !AllInventories.ContainsKey(cefData.toInvId)) {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerMoveItem: An inventory was not found!");
                player.closeInventory();
                return;
            }

            if(htmlItem.isEquipped) {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onPlayerMoveItem: player tried to move item which was equipped!");
                return;
            }

            if(htmlItem.description == "") {
                htmlItem.description = null;
            }

            var fromInv = AllInventories[cefData.fromInvId];
            var toInv = AllInventories[cefData.toInvId];

            var item = fromInv.getItem(htmlItem.configId, i => !i.isBlocked() && htmlItem.isSimelarTo(i));
            if(item != null) {
                //Check potential condition. If condition is false => End
                if(player.hasData("MOVE_INVENTORY_INFO")) {
                    var pot = (OnInventoryMoveCallback)player.getData("MOVE_INVENTORY_INFO");
                    if(!pot.Invoke(player, fromInv, toInv, item, amount)) {
                        player.closeInventory();
                        return;
                    }
                }

                if(!fromInv.moveItems(toInv, item, amount)) {
                    player.sendBlockNotification("Item konnte nicht bewegt werden!", "Item blockiert!");

                    player.closeInventory();
                }
            } else {
                Logger.logError("onPlayerMoveItem: Item not found",
                     $"Fehler beim Item moven: Das Item exisitiert nicht im Inventar! Item-ConfigId: {htmlItem.configId}, Item-Type: {htmlItem.category}, Item-Name: {htmlItem.name}", player);
            }
        }

        private bool onPlayerOpenInventory(IPlayer player, ConsoleKey key, string eventName) {
            showInventory(player, player.getInventory());
            return true;
        }

        private void onPlayerInventoryClosed(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(player.hasData("MOVE_INVENTORY_INFO")) {
                player.resetData("MOVE_INVENTORY_INFO");
            }

            if(player.hasData("CLOSE_INVENTORY_INFO")) {
                var callback = (OnInventoryCloseCallback)player.getData("CLOSE_INVENTORY_INFO");
                player.resetData("CLOSE_INVENTORY_INFO");
                callback?.Invoke(player);
            }
        }

        //InventoryVault Handling
        /// <summary>
        /// Adds an Predicate that returns an inv if another inv shall be opened instead of the player one when interacting with an inv spot
        /// </summary>
        public static void addInventorySpotInjectInventory(InjectInventoryPredicateDelegate inject) {
            AllInventorySpotInjects.Add(inject);
        }

        private bool onInventorySpotInteraction(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
            var spot = (InventorySpot)data["spot"];
            var inv = player.getInventory();

            foreach(var el in AllInventorySpotInjects) {
                var retInv = el.Invoke(player);
                if(retInv != null) {
                    inv = retInv;
                    player.sendNotification(NotifactionTypes.Info, "Ein anderes Inventar als dein eigenes wurde geöffnet", "Anderes Inventar geöffnet", NotifactionImages.Package);
                }
            }

            spot.showInventorySpotOpen(player, inv);
            return true;
        }

        private bool onInventorySpotAccessGranted(IPlayer player, string eventName, object[] args) {
            var id = int.Parse(args[0].ToString());
            var vault = InventorySpot.getById(id);

            showMoveInventory(player, player.getInventory(), vault.Inventory, null, null, vault.Name, true);

            return true;
        }

        private static int getSortClass(ItemTypes type, bool isEquipped) {
            if(isEquipped) {
                return 7;
            }

            switch(type) {
                case ItemTypes.Weapon:
                    return 1;

                case ItemTypes.Tool:
                    return 2;

                case ItemTypes.Accessoire:
                    return 3;

                case ItemTypes.Useable:
                    return 4;

                case ItemTypes.Food:
                    return 5;

                default:
                    return 6;
            }
        }

        #endregion
    }

    public class HtmlInventoryItemModel {
        public int configId;
        public string name;
        public int quality = 0;

        public string category;

        public double weight;
        public int amount;
        public string description;

        public bool isEquipped;
        public string equipSlot;
        public bool useable;

        public string additionalInfo;

        [JsonIgnore]
        public int SortClass;

        public bool isSimelarTo(Item item) {
            return item.ConfigId == configId && item.Name == name && item.Quality == quality && item.Weight == weight && item.Description == description;
        }
    }

    public class SingleInventoryEvent : IPlayerCefEvent {
        public string Event { get; }
        public int id;
        public string[] items;
        public double maxWeight;
        public bool onlyStatic;

        public decimal cash;
        public string duty;
        public string info;

        public int giveItemTarget;

        public float painPercent;
        public string[] parts;

        public string wastedPain;
        public string[] meds;

        public SingleInventoryEvent(int id, double maxWeight, List<HtmlInventoryItemModel> items, bool onlyStatic, decimal cash, string duty, string info, int giveItemTarget) {
            Event = "LOAD_SINGLE_INVENTORY";
            this.id = id;
            this.maxWeight = maxWeight;
            this.items = items.Select(e => e.ToJson()).ToArray();
            this.onlyStatic = onlyStatic;

            this.cash = cash;
            this.duty = duty;
            this.info = info;

            this.giveItemTarget = giveItemTarget;
        }

        private class HTMLHealthPart {
            //ShowData
            public string name;
            public string type;
            public float painLevel;
            public bool multiple;

            public HTMLHealthPart(string name, string type, float painLevel, bool multiple) {
                this.name = name;
                this.type = type;
                this.painLevel = painLevel;
                this.multiple = multiple;
            }
        }

        public void setHealthData(CharacterDamage damage) {
            this.painPercent = (Math.Max((float)damage.getPainLevel(), 1)) / 100;

            var htmlParts = new List<string>();
            foreach(CharacterBodyPart part in Enum.GetValues(typeof(CharacterBodyPart))) {
                var selInjuries = damage.getInjuriesOfPart(part);
                foreach(var selInjury in selInjuries.GroupBy(i => i.Type).ToList()) {
                    var type = selInjury.First().Type;
                    var painLevel = Math.Min((int)Math.Round(((float)damage.getSevernessLevel(part, type)).Map(0, 60, 1, 6)), 6);
                    var multiple = false;
                    if(selInjury.Count() > 1) {
                        multiple = true;
                    }

                    htmlParts.Add(new HTMLHealthPart(Constants.CharacterBodyPartToString[part], DamageTypeToString[type], painLevel, multiple).ToJson());
                }
            }

            this.parts = htmlParts.ToArray();


            //PainMed and WastedPain Stuff
            var wastedPain = damage.getWastedPain();
            var str = "Du fühlst dich noch körperlich fit.";
            if(wastedPain > 75) {
                str = "Die Verletzungen setzen dir extrem zu, du hälst wohl nicht mehr lange durch";
            } else if(wastedPain > 50) {
                str = "Die Verletzungen machen dir zu schaffen, es ist aber noch auszuhalten.";
            } else if(wastedPain > 25) {
                str = "Du fühlst dich wegen deiner Verletzungen recht angeschlagen";
            }

            this.wastedPain = str;
        }

    }

    public class DoubleInventoryEvent : IPlayerCefEvent {
        public string Event { get; }
        public int idLeft;
        public int idRight;

        public double maxWeightLeft;
        public double maxWeightRight;

        public string[] itemsLeft;
        public string[] itemsRight;

        public string rightName;
        public bool showRightSearchbar;

        public DoubleInventoryEvent(int leftId, int rightId, string rightName, bool showRightSearchbar, double maxWeightLeft, double maxWeightRight, List<HtmlInventoryItemModel> leftItems, List<HtmlInventoryItemModel> rightItems) {
            Event = "LOAD_DOUBLE_INVENTORY";
            this.idLeft = leftId;
            this.idRight = rightId;
            this.rightName = rightName;
            this.showRightSearchbar = showRightSearchbar;
            this.maxWeightLeft = maxWeightLeft;
            this.maxWeightRight = maxWeightRight;
            this.itemsLeft = leftItems.Select(e => e.ToJson()).ToArray();
            this.itemsRight = rightItems.Select(e => e.ToJson()).ToArray();
        }

    }
}
