using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.LockerSystem.Model {
    public class ServerAccessedDrawer : LockerDrawer {
        public bool IsActive;
        public DateTime HasBeenActivedDate;
        public bool IsReceivingItems;
        public string AccessedOwner;

        public DateTime LastInventoryClose;

        private static TimeSpan MAX_STORAGE_TIME = TimeSpan.FromDays(5);

        public ServerAccessedDrawer(configlockerdrawer dbDrawer, Locker owner) : base(dbDrawer.id, owner, dbDrawer.displayNumber, dbDrawer.combination, dbDrawer.inventoryId, dbDrawer.ownerId) {
            LastInventoryClose = DateTime.MinValue;

            if(dbDrawer.data != "") {
                var split = dbDrawer.data.Split('#');
                IsActive = bool.Parse(split[0]);
                HasBeenActivedDate = split[1].FromJson<DateTime>();
                IsReceivingItems = bool.Parse(split[2]);
            } else {
                IsActive = false;

                setNewCombination();
            }

            if(IsActive && !IsReceivingItems) {
                Inventory.addBlockStatement(new InventoryAddBlockStatement(this, i => true));
            }
        }

        public string setToGiveItems(int charId, List<Item> items, string accessedOwner) {
            if(Inventory == null) {
                createInventoryIfNull(30);
            }

            OwnerId = charId;

            foreach(var item in items) {
                Inventory.addItem(item, true);
            }

            setActive(false, accessedOwner);

            //Set, that no items can be added
            Inventory.addBlockStatement(new InventoryAddBlockStatement(this, i => true));

            return Combination;
        }

        public string setToReceiveItems(int charId, string accessedOwner) {
            if(Inventory == null) {
                createInventoryIfNull(30);
            }

            OwnerId = charId;

            setActive(true, accessedOwner);

            Inventory?.clearBlockStatements();

            return Combination;
        }

        public void freeDrawer() {
            Inventory?.clearAllItems();

            OwnerId = null;
            Inventory?.clearBlockStatements();

            IsActive = false;

            setNewCombination();

            updateDbData();
        }

        private void setActive(bool isReceiving, string accessedOwner) {
            IsActive = true;
            IsReceivingItems = isReceiving;
            HasBeenActivedDate = DateTime.Now;
            LastInventoryClose = DateTime.MaxValue;
            AccessedOwner = accessedOwner;

            setNewCombination();

            updateDbData();
        }

        public override VirtualMenu getShowMenu(IPlayer player) {
            var name = $"{DisplayNumber}: In Benutzung";

            return new VirtualMenu(name, () => {
                var menu = new Menu(name, $"Was möchtest du tun?");

                var data = new Dictionary<string, dynamic> {
                    { "LockerDrawer", this }
                };

                menu.addMenuItem(new ClickMenuItem("Öffnen", "Öffne das Schließfach mit der richtigen Kombination", "", "OPEN_LOCKER_DRAWER").withData(data));

                return menu;
            });
        }
        public override void onPlayerTryOpen(IPlayer player) {
            CombinationLockController.requestPlayerCombination(player, Combination, (p, d) => {
                onPlayerOpen(player);
            });
        }

        public override void onPlayerOpen(IPlayer player) {
            onPlayerOpenInventory(player);
        }

        public override void onPlayerOpenInventory(IPlayer player) {
            if(Inventory == null) {
                createInventoryIfNull(30f);
            }

            InventoryController.showMoveInventory(player, player.getInventory(), Inventory, null, onInventoryClose, $"Schließfach Nr. {DisplayNumber}", true);
        }

        private void onInventoryClose(IPlayer player) {
            LastInventoryClose = DateTime.Now;
            LockerController.ServerAccessedDrawerAccessedDelegate.Invoke(this);
        }

        public override void onUpdate() {
            if(IsActive) {
                if(HasBeenActivedDate + MAX_STORAGE_TIME < DateTime.Now) {
                    //Irrelevant if receiving or giving items
                    LockerController.ServerAccessedDrawerTaskIgnoredDelegate?.Invoke(this);

                    setNewCombination();

                    IsActive = false;
                    HasBeenActivedDate = DateTime.MinValue;

                    Inventory.clearAllItems();

                    updateDbData();
                }
            }
        }

        private void setNewCombination() {
            var rnd = new Random();
            Combination = rnd.Next(11111, 66666).ToString();
        }

        protected override string getDbData() {
            return $"{IsActive}#{HasBeenActivedDate.ToJson()}#{IsReceivingItems}#{AccessedOwner}";
        }
    }
}
