using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public enum InventoryTypes {
        Player = 0,
        Vehicle = 1,
        Company = 2,
        Storage = 3,
        InteractSpot = 4,
        Fridge = 5,
        HeistReward = 6,
        EvidenceBox = 7,
        AnalyseSpot = 8,
        Backpack = 9,
        WeaponCase = 10,
        SIMCardSlot = 11,
        ProcessingObject = 12,
        PlasticBag = 13,
        Dealer = 14,
        Locker = 15,
        PrisonBox = 16,
        FoldingTable = 17,
        FlaskUtensil = 18,
        SuctionFilterUtensil = 19,
        ExsikkatorUtensil = 20,
        PlaceableItemContainer = 21,
        PrisonCell = 22,
        WorldProcessMachine = 23,
        DealerNPC = 24,
        BoxItem = 25,
        CraftingSpot = 26,
    }

    public delegate bool InventoryEmptyAfterMoveDelegate(Inventory inventory);
    public delegate void InventoryItemsMovedIntoThisInventoryDelegate(Inventory oldInventory, Item item, int canBeStackedAmount = 1);

    public delegate void InventoryAddItemDelegate(Inventory inventory, Item item, int? amount = null, bool saveToDb = true);
    public delegate void InventoryRemoveItemDelegate(Inventory inventory, Item item, int? amount = null, bool saveToDb = true);
    public delegate void InventoryInventoryChangeItemDelegate(Inventory oldInventory, Inventory newInventory, Item item);
    public delegate void ItemAmountChangeDelegate(Item item);


    public delegate void InventoryMoveItemDelegate(Inventory oldInventory, Inventory newInventory, Item item, Item alreadyItem, Item cloneItem, bool onlyInvChange, bool oldItemRemoved);

    public class Inventory : IDisposable {
        public int Id;
        public int OwnerId;
        public float MaxWeight;
        public float CurrentWeight { get => getCombinedWeight(); }
        public float FreeWeight { get => MaxWeight - CurrentWeight; }
        
        public InventoryTypes InventoryType;

        public List<Item> Items = new List<Item>();

        public static InventoryEmptyAfterMoveDelegate InventoryEmptyAfterMoveDelegate;

        public static InventoryAddItemDelegate InventoryAddItemDelegate;
        public static InventoryRemoveItemDelegate InventoryRemoveItemDelegate;
        public static InventoryInventoryChangeItemDelegate InventoryInventoryChangeItemDelegate;
        public static ItemAmountChangeDelegate ItemAmountChangeDelegate;
        public static ItemWearChangeDelegate ItemWearChangeDelegate;

        public InventoryItemsMovedIntoThisInventoryDelegate ThisInventoryItemsMovedIntoThisInventoryDelegate;
        public InventoryAddItemDelegate ThisInventoryAddItemDelegate;
        public InventoryEmptyAfterMoveDelegate ThisInventoryEmptyAfterMoveDelegate;

        public List<InventoryAddBlockStatement> BlockStatements = new List<InventoryAddBlockStatement>();

        public Inventory(int id, int ownerId, float maxWeight, InventoryTypes inventoryTypes) {
            Id = id;
            OwnerId = ownerId;
            MaxWeight = maxWeight;
            InventoryType = inventoryTypes;
        }

        private bool blockedByStatements(Item item) {
            var ret = false;
            BlockStatements.ForEach(st => ret = ret || st.check(item));
            return ret;
        }

        public bool addItem(Item item, bool ignoreWeight = false, bool saveToDb = true) {
            //Check if there is place in Inventory         
            if((CurrentWeight + item.Weight * (item.StackAmount ?? 1) > MaxWeight) && !ignoreWeight) {
                return false;
            }

            if(item.isBlocked() || blockedByStatements(item)) {
                return false;
            }

            finallyAddItem(item, saveToDb);
            return true;
        }

        /// <summary>
        /// Adds multiple Items to the inventory. Returns false if ONE item could not be added. Will not add any item if one could not be added.
        /// </summary>
        /// <returns></returns>
        public bool addItems(List<Item> items, bool ignoreWeight = false, bool saveToDb = true) {
            var combinedWeight = 0f;
            foreach(var item in items) {
                if(item.isBlocked() || blockedByStatements(item)) {
                    return false;
                }

                combinedWeight += item.Weight * (item.StackAmount ?? 1);
            }

            if((CurrentWeight + combinedWeight > MaxWeight) && !ignoreWeight) {
                return false;
            }

            foreach(var item in items) {
                finallyAddItem(item, saveToDb);
            }

            return true;
        }

        private void finallyAddItem(Item item, bool saveToDb) {
            if(item.CanBeStacked) {
                var already = Items.FirstOrDefault(i => i.isSimelarTo(item));
                if(already != null) {
                    already.StackAmount += item.StackAmount;

                    InventoryAddItemDelegate?.Invoke(this, already, already.StackAmount, saveToDb);
                    ThisInventoryAddItemDelegate?.Invoke(this, already, already.StackAmount, saveToDb);

                    var cfgInner = InventoryController.getConfigById(item.ConfigId);
                    if(cfgInner != null) {
                        item.processAdditionalInfo(cfgInner.additionalInfo);
                    }

                    return;
                }
                item.ResidingInventory = this;
                Items.Add(item);
            } else {
                item.ResidingInventory = this;
                Items.Add(item);
            }

            var cfg = InventoryController.getConfigById(item.ConfigId);
            if(cfg != null) {
                item.processAdditionalInfo(cfg.additionalInfo);
            }

            InventoryAddItemDelegate?.Invoke(this, item, null, saveToDb);
            ThisInventoryAddItemDelegate?.Invoke(this, item, null, saveToDb);
        }

        public bool removeItem(Item item, int canBeStackedAmount = 1, bool saveToDb = true) {
            if(item.isBlocked()) {
                return false;
            }

            if(item.CanBeStacked) {
                var found = Items.FirstOrDefault(i => i.isSimelarTo(item));
                if(found != null) {
                    if(found.StackAmount > canBeStackedAmount) {
                        found.StackAmount -= canBeStackedAmount;
                        InventoryRemoveItemDelegate?.Invoke(this, item, found.StackAmount, saveToDb);

                        checkEmpty();
                        item.ResidingInventory = null;

                        return true;
                    }

                    if(found.StackAmount < canBeStackedAmount) {
                        return false;
                    }

                    if(!Items.Remove(item)) {
                        return false;
                    }
                } else {
                    return false;
                }
            } else {
                if(!Items.Remove(item)) {
                    return false;
                }
            }

            checkEmpty();
            InventoryRemoveItemDelegate?.Invoke(this, item, null, saveToDb);
            item.ResidingInventory = null;

            return true;
        }

        public bool simpleRemoveItem(Item item) {
            if(!Items.Remove(item)) {
                return false;
            }

            checkEmpty();
            item.ResidingInventory = null;

            return true;
        }

        public bool hasItem(Predicate<Item> predicate) {
            //No Item of this kind in List
            return Items.Any(item => predicate(item));
        }

        public bool hasItem<T>(Predicate<T> predicate) {
            return getAllItems().Any(i => {
                if(i is not T) {
                    return false;
                }
                //if(i.GetType() != typeof(T) && !i.GetType().IsSubclassOf(typeof(T))) {
                //    return false;
                //}
                i.TryCast<T>(out var item);

                return predicate(item);
            });
        }

        public bool hasItems<T>(Predicate<T> predicate, int amount) {
            return getAllItems().Where(i => {
                if(i.GetType() != typeof(T) && !(i.GetType().IsSubclassOf(typeof(T)))) {
                    return false;
                }

                i.TryCast(out T newItem);

                return predicate(newItem);
            }).ToList().Sum(i => i.StackAmount ?? 1) >= amount;
        }

        /// <summary>
        /// Only use if you don't know the config id!
        /// </summary>
        public Item getItem(Func<Item, bool> predicate) {
            var item = getAllItems().FirstOrDefault(predicate);

            return item;
        }

        /// <summary>
        /// Only use if you don't know the config id!
        /// </summary>
        public T getItem<T>(Predicate<T> predicate) {
            var item = getAllItems().FirstOrDefault(i => {
                if(i.GetType() != typeof(T) && !i.GetType().IsSubclassOf(typeof(T))) {
                    return false;
                }

                i.TryCast(out T newItem);

                return predicate(newItem);
            });

            item.TryCast(out T returnItem);

            return returnItem;
        }

        public Item getItem(int configId, Predicate<Item> predicate) {
            return Items.FirstOrDefault(item => item.ConfigId == configId && predicate(item));
        }

        /// <summary>
        /// Return a List of Items specified by a given predicate. Returns null if something failed
        /// </summary>
        /// <param name="configId">Which type of item should be get.</param>
        /// <param name="amount">How many Items shall be selected. Type -1 for all</param>
        /// <param name="predicate">Type in i => true, to get no predicate</param>
        public List<Item> getItems(int configId, int amount, Predicate<Item> predicate) {
            if(amount == -1) {
                return Items.Where(item => item.ConfigId == configId && predicate(item)).ToList();
            }

            var items = Items.Where(item => item is not null && item.ConfigId == configId && predicate(item));
            try {
                return items.Take(amount).ToList();
            } catch(Exception ex) {
                Logger.logException(ex, "getItems: Could not take amount of items out of Inventory.");
                return new List<Item>();
            }
        }

        /// <summary>
        /// Return a List of Items specified by a given predicate. Returns null if something failed
        /// </summary>
        /// <param name="amount">How many Items shall be selected. Type -1 for all</param>
        /// <param name="predicate">Type in i => true, to get no predicate</param>
        /// <returns></returns>
        public List<Item> getItems(int amount, Predicate<Item> predicate) {
            //No Items of this kind in List

            if(amount == -1) {
                return getItems(predicate);
            }

            var items = getAllItems().Where(item => item is not null && predicate(item));
            try {
                return items.Take(amount).ToList();
            } catch(Exception ex) {
                Logger.logException(ex, "getItems: Could not take amount of items out of Inventory.");
                return new List<Item>();
            }
        }

        /// <summary>
        /// Return a List of Items specified by a given predicate. Returns null if something failed
        /// </summary>
        /// <param name="predicate">Type in i => true, to get no predicate</param>
        /// <returns></returns>
        public List<Item> getItems(Predicate<Item> predicate) {
            return getAllItems().Where(item => item is not null && predicate(item)).ToList();
        }

        /// <summary>
        /// Only use if you don't know the config id!
        /// </summary>
        public IEnumerable<T> getItems<T>(Predicate<T> predicate, int amount = -1) {
            var items = Items.OfType<T>().Where(item => predicate(item)).ToList();

            return amount == -1
                ? items
                : items.Take(amount);
        }

        /// <summary>
        /// Moves Items from this inventory to another
        /// </summary>
        /// <param name="canBeStackedAmount">If Item is stackable, move multiple Items</param>
        /// <returns></returns>
        public bool moveItem(Inventory newInventory, Item item, int canBeStackedAmount = 1, bool ignoreWeight = false) {
            if(!ignoreWeight && Math.Round(newInventory.MaxWeight - newInventory.CurrentWeight, 2) < Math.Round(item.Weight * canBeStackedAmount, 2)) {
                return false;
            }

            //if(newInventory.InventoryType == InventoryTypes.Player && item is OnlySingleItemInInventory && newInventory.getItem(item.ConfigId, i => true) != null) {
            //    return false;
            //}

            if(item.isBlocked() || newInventory.blockedByStatements(item)) {
                return false;
            }

            if(item.CanBeStacked) {
                if(item.StackAmount == canBeStackedAmount) {
                    if(removeItem(item, canBeStackedAmount)) {
                        checkEmpty();
                    } else {
                        return false;
                    }
                } else if(item.StackAmount > canBeStackedAmount) {
                    item.StackAmount -= canBeStackedAmount;
                    ItemAmountChangeDelegate?.Invoke(item);
                } else {
                    return false;
                }

                var already = newInventory.Items.FirstOrDefault(i => i.isSimelarTo(item));
                if(already == null) {
                    var clone = (Item)item.Clone();
                    clone.Id = null;
                    clone.StackAmount = canBeStackedAmount;
                    if(!newInventory.addItem(clone)) {
                        return false;
                    }
                } else {
                    already.StackAmount += canBeStackedAmount;
                    ItemAmountChangeDelegate?.Invoke(already);
                    newInventory.ThisInventoryAddItemDelegate.Invoke(newInventory, already, canBeStackedAmount);
                }
            } else {
                if(Items.Remove(item)) {
                    newInventory.Items.Add(item);
                    item.ResidingInventory = newInventory;
                    InventoryInventoryChangeItemDelegate.Invoke(this, newInventory, item);
                    newInventory.ThisInventoryAddItemDelegate?.Invoke(newInventory, item);
                    checkEmpty();
                } else {
                    return false;
                }
            }

            newInventory.ThisInventoryItemsMovedIntoThisInventoryDelegate?.Invoke(this, item, canBeStackedAmount);

            return true;
        }

        /// <summary>
        /// Selects amount items based on the original Item and moves them
        /// </summary>
        /// <param name="newInventory"></param>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool moveItems(Inventory newInventory, Item item, int amount = 1) {
            if(item.CanBeStacked) {
                return moveItem(newInventory, item, amount);
            }
            var similar = getSimilarItems(item, amount).ToList();
            if(!(similar.Count() < amount)) {
                foreach(var simItem in similar) {
                    if(!moveItem(newInventory, simItem)) {
                        return false;
                    }
                }
            } else {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Selects amount items based on the original Item and removes them
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool removeSimelarItems(Item item, int amount = 1) {
            if(item.CanBeStacked) {
                return removeItem(item, amount);
            }

            var similar = getSimilarItems(item, amount).ToList();
            if(!(similar.Count() < amount)) {
                foreach(var simItem in similar) {
                    if(!removeItem(simItem)) {
                        return false;
                    }
                }
            } else {
                return false;
            }

            return true;
        }
        
        public int getSimilarItemsAmount(Item item) {
            return getSimilarItems(item, int.MaxValue).Sum(i => i.StackAmount ?? 1);
        }

        private void checkEmpty() {
            if(InventoryEmptyAfterMoveDelegate != null && Items.Count == 0) {
                InventoryEmptyAfterMoveDelegate.Invoke(this);
            }

            if(ThisInventoryEmptyAfterMoveDelegate != null && Items.Count == 0) {
                ThisInventoryEmptyAfterMoveDelegate.Invoke(this);
            }
        }

        public List<Item> getAllItems() {
            return Items;
        }

        public IEnumerable<Item> getSimilarItems(Item item, int amount) {
            return Items.Where(i => !i.isBlocked() && i.isSimelarTo(item)).Take(amount);
        }

        public int getCount() {
            return Items.Count;
        }

        public float getCombinedWeight() {
            float count = 0;
            foreach(var item in Items) {
                count += item.Weight * (item.StackAmount ?? 1);
            }

            return count;
        }

        public void addBlockStatement(InventoryAddBlockStatement statement) {
            if(BlockStatements.FirstOrDefault(b => b.Equals(statement)) == null) {
                BlockStatements.Add(statement);
            }
        }

        public bool removeBlockStatement(InventoryAddBlockStatement statement) {
            return BlockStatements.Remove(statement);
        }

        public bool canFitItem(Item item) {
            return CurrentWeight + item.Weight <= MaxWeight;
        }

        public bool canFitItem(configitem cfgItem) {
            return CurrentWeight + cfgItem.weight <= MaxWeight;
        }

        public void clearBlockStatements() {
            BlockStatements.Clear();
        }

        public override string ToString() {
            return $"Id: {Id}, OwnerId: {OwnerId}, InventoryType: {InventoryType}, Items: {Items.ToJson()}";
        }

        public void clearAllItems() {
            for(int i = Items.Count - 1; i >= 0; i--) {
                removeItem(Items[i], Items[i].StackAmount ?? 1);
            }
        }

        public void Dispose() {
            Items.Clear();
        }
    }

    public class InventoryAddBlockStatement {
        public object Owner;
        public Predicate<Item> Predicate;

        public InventoryAddBlockStatement(object owner, Predicate<Item> predicate) {
            Owner = owner;
            Predicate = predicate;
        }

        public bool check(Item item) {
            return Predicate(item);
        }

        public override bool Equals(object obj) {
            return (obj is InventoryAddBlockStatement) && (obj as InventoryAddBlockStatement).Predicate == Predicate;
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }
}
