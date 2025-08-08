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
    public abstract class LockerDrawer {
        public int Id;

        public Locker Parent;

        public int DisplayNumber;
        public string Combination;
        protected int? InventoryId;
        public Inventory Inventory { get => InventoryController.loadInventory(InventoryId ?? -1); }

        public int? OwnerId;
        public bool InUse { get => OwnerId != null; }

        public LockerDrawer(int id, Locker owner, int displayNumber, string combination, int? inventoryId, int? ownerId) {
            Id = id;
            Parent = owner;
            DisplayNumber = displayNumber;
            Combination = combination;
            InventoryId = inventoryId;

            OwnerId = ownerId;
        }

        public static LockerDrawer createLockerFromDb(configlockerdrawer lockerDrawer, Locker owner) {
            switch((LockerDrawerType)lockerDrawer.type) {
                case LockerDrawerType.PublicDrawer:
                    return new PublicLockerDrawer(lockerDrawer, owner);

                case LockerDrawerType.ServerAccessedDrawer:
                    return new ServerAccessedDrawer(lockerDrawer, owner);

                case LockerDrawerType.CompanyDrawer:
                    return new CompanyLockerDrawer(lockerDrawer, owner);

                default:
                    return null;
            }
        }

        public abstract VirtualMenu getShowMenu(IPlayer player);

        public abstract void onPlayerTryOpen(IPlayer player);
        public abstract void onPlayerOpen(IPlayer player);
        public abstract void onPlayerOpenInventory(IPlayer player);
        public abstract void onUpdate();

        protected void updateDbData() {
            using(var db = new ChoiceVDb()) {
                var dbDrawer = db.configlockerdrawers.Find(Id);

                if(dbDrawer != null) {
                    dbDrawer.ownerId = OwnerId;
                    dbDrawer.combination = Combination;
                    dbDrawer.data = getDbData();

                    db.SaveChanges();
                }
            }
        }

        protected void createInventoryIfNull(float weight) {
            var inv = InventoryController.createInventory(Id, weight, InventoryTypes.Locker);

            using(var db = new ChoiceVDb()) {
                var dbDrawer = db.configlockerdrawers.Find(Id);
                dbDrawer.inventoryId = inv.Id;

                db.SaveChanges();
            }

            InventoryId = inv.Id;
        }

        public bool onChangeCombination(IPlayer player, string combination) {
            if(combination.Length > 6) {
                player?.sendBlockNotification("Die Kombination sollte nicht mehr als 6 Stellen haben", "Kombination zu lang", NotifactionImages.Lock);
                return false;
            }

            if(!int.TryParse(combination, out int n)) {
                player?.sendBlockNotification("Die Kombination muss eine Zahl sein!", "Komnbination kein Zahl", NotifactionImages.Lock);
                return false;
            }

            Combination = combination;

            updateDbData();

            player?.sendNotification(NotifactionTypes.Success, "Fachkombination erfolgreich geändert!", "Kombination erfolgreich geändert");
            
            return true;
        }

        protected abstract string getDbData();
    }
}
