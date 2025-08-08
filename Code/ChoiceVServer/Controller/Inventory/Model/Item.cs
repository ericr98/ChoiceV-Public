using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public enum ItemTypes : int {
        StaticNoStack = -1,
        Static = 0,
        Useable = 1,
        Food = 2,
        Weapon = 3,
        Tool = 4,
        Accessoire = 5
    }

    public delegate void ItemDataChange(Item item, string parameter, dynamic value);
    public delegate bool ItemDataRemoved(Item item, string parameter);
    public delegate void ItemDescriptionChange(Item item);
    public delegate void ItemRemoveTriggered(Item item);

    public abstract class Item : ICloneable {
        public int? Id = null;
        public int ConfigId;
        [JsonIgnore]
        public configitem ConfigItem;
        public string Name;
        public string Category;

        public float Weight = 0.1f;

        public ItemTypes Type;

        public int Quality;
        public DateTime? Durability;

        public bool CurrentlyMoveable = true;

        public bool CanBeStacked = false;
        public int? StackAmount = null;

        public ExtendedDictionary<string, dynamic> Data;

        public string Description = null;

        public bool Detectable = false;

        private bool DestroyWhenUsed;

        [JsonIgnore]
        public Animation ItemAnimation = null;

        public static ItemDataChange ItemDataChange;
        public static ItemDataRemoved ItemDataRemoved;
        public static ItemRemoveTriggered ItemRemoveTriggered;
        public static ItemDescriptionChange ItemDescriptionChange;
        public static ItemAmountChangeDelegate ItemAmountChangeDelegate;

        public bool UsingHasToBeConfirmed = false;

        [JsonIgnore]
        public Inventory ResidingInventory;

        public Item(item item) {
            Id = item.id;
            ConfigId = item.config.configItemId;
            ConfigItem = item.config;
            Name = item.config.name;
            Category = item.config.category;
            Weight = item.config.weight;
            Type = (ItemTypes)item.config.itemType;
            Quality = item.quality;
            Description = item.description;
            CurrentlyMoveable = true;

            CanBeStacked = item.amount == null ? false : true;
            StackAmount = item.amount;

            DestroyWhenUsed = item.config.destroyedWhenUsed == 1;

            if(item.config.description != null && item.config.description != "") {
                Description = item.description;
            }

            if(item.itemsdata != null) {
                var dic = new Dictionary<string, dynamic>();
                foreach(var row in item.itemsdata) {
                    dic[row.parameter] = row.value.FromJson<dynamic>();
                }

                Data = new ExtendedDictionary<string, dynamic>(dic);
                Data.OnValueChanged += (key, value) => {
                    if(ItemDataChange != null) {
                        ItemDataChange.Invoke(this, key, value);
                    }
                };

                Data.OnValueRemoved += (key) => {
                    if(ItemDataRemoved != null) {
                        return ItemDataRemoved.Invoke(this, key);
                    } else {
                        return false;
                    }
                };
            }

            if(item.config.itemAnimation != null) {
                var anim = item.config.itemAnimationNavigation;
                ItemAnimation = new ItemAnimation(anim);
            }
        }

        public Item(configitem configItem, int quality = -1, int? stackAmount = null) {
            ConfigId = configItem.configItemId;
            ConfigItem = configItem;
            Name = configItem.name;
            Category = configItem.category;
            Weight = configItem.weight;
            Type = (ItemTypes)configItem.itemType;
            Quality = quality;
            Description = null;
            CurrentlyMoveable = true;

            CanBeStacked = stackAmount == null ? false : true;
            StackAmount = stackAmount;

            DestroyWhenUsed = configItem.destroyedWhenUsed == 1;

            if(!string.IsNullOrEmpty(configItem.description)) {
                Description = configItem.description;
            }

            if(configItem.durability != -1) {
                Durability = DateTime.Now + TimeSpan.FromDays(configItem.durability);
            } else {
                Durability = null;
            }

            if(!CanBeStacked) {
                Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());
                Data.OnValueChanged += (key, value) => {
                    if(ItemDataChange != null) {
                        ItemDataChange.Invoke(this, key, value);
                    }
                };
            }

            if(configItem.itemAnimation != null) {
                var anim = configItem.itemAnimationNavigation;
                ItemAnimation = new ItemAnimation(anim);
            }
        }

        public void evaluateUse(IPlayer player) {
            if(!canBeUsedByCharacterTypes().Contains(player.getCharacterType())) {
                player.sendBlockNotification("Deine Art Charakter kann dieses Item nicht benutzen", "Benutzen fehlgeschlagen");
                return;
            }
            
            if(getUseAnimation(player) != null) {
                AnimationController.animationTask(player, getUseAnimation(player), () => {
                    use(player);
                });
            } else {
                use(player);
            }
        }
        
        protected virtual Animation getUseAnimation(IPlayer player) {
            return ItemAnimation;
        }
        
        public virtual void use(IPlayer player) {
            if(!DestroyWhenUsed) {
                return;
            }

            if(CanBeStacked) {
                if(StackAmount is null) {
                    return;
                }

                StackAmount--;
                if(StackAmount <= 0) {
                    potenitallyCreateTrashItem();
                    destroy();
                } else {
                    ItemAmountChangeDelegate?.Invoke(this);
                }
            } else {
                potenitallyCreateTrashItem();
                destroy();
            }
        }

        /// <summary>
        /// Use if a e.g. Controller is using the Item, and the stuff in the use function should not be executed
        /// </summary>
        public virtual void externalUse() {
            if(!DestroyWhenUsed) {
                return;
            }

            if(CanBeStacked) {
                if(StackAmount is null) {
                    return;
                }

                StackAmount--;
                if(StackAmount <= 0) {
                    potenitallyCreateTrashItem();
                    destroy();
                } else {
                    ItemAmountChangeDelegate?.Invoke(this);
                }
            } else {
                potenitallyCreateTrashItem();
                destroy();
            }
        }

        private void potenitallyCreateTrashItem() {
            //TODO Create TrashItem
            //if(ConfigItem.configitemstrashitemmainItems != null && ConfigItem.configitemstrashitemmainItems.Count > 0) {
            //    foreach(var trashCfg in ConfigItem.configitemstrashitemmainItems ) {
            //        foreach(var item in InventoryController.createItems(trashCfg.trashOutcome, trashCfg.trashOutcomeAmount, -1)) {
            //            ResidingInventory.addItem(item, true);
            //        }
            //    } 
            //}
        }
        
        public virtual void processAdditionalInfo(string info) { }

        public virtual void destroy(bool toDb = true) {
            if(toDb) {
                ItemRemoveTriggered?.Invoke(this);
            }
        }

        public void check() {
            //TODO CHECK IF ITEM IS EXPIRED ETC.
        }

        public virtual void updateDescription() {
            ItemDescriptionChange?.Invoke(this);
        }

        public void saveDescription() {
            ItemDescriptionChange?.Invoke(this);
        }

        public virtual bool isBlocked() {
            return false;
        }

        public virtual string getInfo() {
            return Name;
        }

        public virtual object Clone() {
            return this.MemberwiseClone();
        }

        public static bool isGenericStackable(configitem configItem) {
            return (ItemTypes)configItem.itemType == ItemTypes.Static || (ItemTypes)configItem.itemType == ItemTypes.Food;
        }

        public bool isSimelarTo(Item item) {
            return item.ConfigId == ConfigId && item.Name == Name && item.Description == Description && item.Quality == Quality && Math.Abs(item.Weight - Weight) < 0.0001;
        }

        public bool moveToInventory(Inventory newInventory, int canBeStackedAmount = 1, bool ignoreWeight = false) {
            if(ResidingInventory != null) {
                return ResidingInventory.moveItem(newInventory, this, canBeStackedAmount, ignoreWeight);
            }

            return false;
        }

        public virtual void setOrderData(OrderItem orderItem) { }
        
        public virtual void setCreateData(Dictionary<string, dynamic> data) { }
        
        public virtual void onUnloaded() {

        }

        protected bool isEvidence() {
            return Data.hasKey("EVIDENCE_ID");
        }

        /// <summary>
        /// Used for displaying additional Info in the Inventory
        /// </summary>
        public virtual string getInventoryAdditionalInfo() {
            if(Data != null && Data.hasKey("EVIDENCE_ID")) {
                return $"Beweis: {(int)Data["EVIDENCE_ID"]}";
            } else {
                return null;
            }
        }

        public void setAsEvidence(int evidenceId) {
            if(!isEvidence()) {
                Data["EVIDENCE_ID"] = evidenceId;
            }
        }

        public int getEvidenceId() {
            if(isEvidence()) {
                return (int)Data["EVIDENCE_ID"];
            } else {
                return -1;
            }
        }

        public virtual List<CharacterType> canBeUsedByCharacterTypes() {
            return [CharacterType.Player];
        }
    }
}
