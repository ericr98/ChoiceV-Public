using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.LockerSystem.Model;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public enum LockerDrawerType {
        PublicDrawer = 0,
        ServerAccessedDrawer = 1,
        CompanyDrawer = 2,
    }

    public delegate bool ServerAccessedDrawerTaskUpdateDelegate(ServerAccessedDrawer drawer);

    public class LockerController : ChoiceVScript {
        public static ServerAccessedDrawerTaskUpdateDelegate ServerAccessedDrawerTaskIgnoredDelegate;
        public static ServerAccessedDrawerTaskUpdateDelegate ServerAccessedDrawerAccessedDelegate;

        private static List<Locker> AllLockers;

        private static ChoiceVPed LockerMaster;

        private static Position LockerMasterPosition = new Position(671, -2667.5f, 5.07f);

        public LockerController() {
            AllLockers = new List<Locker>();

            using (var db = new ChoiceVDb()) {
                foreach (var dbLocker in db.configlockers.Include(l => l.configlockerdrawers)) {
                    if(dbLocker.phoneNumber == null) {
                        dbLocker.phoneNumber = PhoneController.createNewPhoneNumber($"Locker: {dbLocker.name}").number;
                    }

                    var locker = new Locker(dbLocker.id, dbLocker.name, dbLocker.phoneNumber ?? -1, dbLocker.dayPrice, dbLocker.companyId, CollisionShape.Create(dbLocker.colShapeStr));

                    foreach (var dbLockerDrawer in dbLocker.configlockerdrawers) {
                        var drawer = LockerDrawer.createLockerFromDb(dbLockerDrawer, locker);

                        locker.addLockerDrawer(drawer);
                    }

                    AllLockers.Add(locker);
                }

                db.SaveChanges();
            }

            EventController.addMenuEvent("OPEN_LOCKER_DRAWER", openLockerDrawer);
            EventController.addMenuEvent("OPEN_LOCKER_DRAWER_ADMINISTRATIVE", openLockerDrawerAdministrative);
            EventController.addMenuEvent("OPEN_LOCKER_DRAWER_INVENTORY", onLockerDrawerOpen);
            EventController.addMenuEvent("CHANGE_LOCKER_DRAWER_COMNBINATION", onChangeLockerDrawerCombination);

            EventController.addMenuEvent("RENT_PUBLIC_LOCKER_DRAWER", onRentPublicDrawer);
            EventController.addMenuEvent("UNRENT_PUBLIC_LOCKER_DRAWER", onUnrentPublicDrawer);

            EventController.addMenuEvent("CHANGE_COMPANY_LOCKER_DRAWER_NAME", onChangeCompanyDrawerName);
            EventController.addMenuEvent("REGISTER_COMPANY_LOCKER_DRAWER", onRegisterCompanyDrawer);

            InvokeController.AddTimedInvoke("LockerUpdater", updateLocker, TimeSpan.FromMinutes(5), true);

            LockerMaster = PedController.createPed("Spindmeister Randy", "s_m_m_cntrybar_01", LockerMasterPosition, 90f);
            LockerMaster.addModule(new NPCMenuItemsModule(LockerMaster, [onLockerMasterInteract]));

            EventController.addMenuEvent("LOCKER_SHOW_LOCKER_MASTER", onLockerShowLockerMaster);
            EventController.addMenuEvent("OPEN_LOCKER_MASTER_BOX", onOpenLockerMasterBox);


            #region Support Stuff

            SupportController.addSupportMenuElement(new GeneratedSupportMenuElement(3, SupportMenuCategories.Misc, "Spindmenü", generateLockerMenu));

            EventController.addMenuEvent("SUPPORT_CREATE_LOCKER", onSupportCreateLocker);
            EventController.addMenuEvent("SUPPORT_DELETE_LOCKER", onSupportDeleteLocker);

            EventController.addMenuEvent("SUPPORT_CREATE_DRAWER", onSupportCreateDrawer);

            #endregion
        }

        private static readonly object GetFreeServerAccessedDrawerMutex = new();
        public static ServerAccessedDrawer getFreeServerAccessedDrawer() {
            lock (GetFreeServerAccessedDrawerMutex) {
                var options = AllLockers.SelectMany(l => l.getFreeServerAccessedDrawers()).Where(d => d != null).ToList();

                if (options.Count > 0) {
                    var rnd = new Random();

                    return options[rnd.Next(options.Count)];
                } else {
                    return null;
                }
            }
        }

        public static void freeServerAccessedDrawer(int lockerId, int drawerId) {
            var locker = AllLockers.FirstOrDefault(l => l.Id == lockerId);
            if(locker == null) {
                return;
            }
            
            var drawer = locker.Drawers.FirstOrDefault(d => d.Id == drawerId);
            if(drawer == null || drawer is not ServerAccessedDrawer sDrawer) {
                return;
            }

            sDrawer.freeDrawer();
        }

        public static Inventory getLockerDrawerInventory(int lockerId, int drawerId) {
            var locker = AllLockers.FirstOrDefault(l => l.Id == lockerId);
            if(locker == null) {
                return null;
            }
            
            var drawer = locker.Drawers.FirstOrDefault(d => d.Id == drawerId);
            if(drawer == null) {
                return null;
            }

            return drawer.Inventory;
        }

        private void updateLocker(IInvoke obj) {
            foreach (var locker in AllLockers) {
                locker.onUpdate();
            }
        }

        private MenuItem onLockerMasterInteract(IPlayer player) {
            var menu = new Menu("Spindmeister", "Was möchtest du tun?");

            using (var db = new ChoiceVDb()) {
                var charId = player.getCharacterId();
                var viable = db.lockersaveboxes.Where(l => l.ownerId == charId).ToList();

                foreach (var saveBox in viable) {
                    var price = Convert.ToDecimal(Math.Ceiling((DateTime.Now - saveBox.createDate).TotalDays)) * saveBox.price * 1.15m;

                    var priceString = $"${price}";
                    var now = DateTime.Now;
                    var last = saveBox.lastOpened;
                    if (last.Year == now.Year && last.Month == now.Month && last.Day == now.Day) {
                        price = 0;
                        priceString = "Heute gratis";
                    }

                    var data = new Dictionary<string, dynamic> {
                        { "Price", price },
                        { "SaveBox", saveBox }
                    };

                    menu.addMenuItem(new ClickMenuItem(saveBox.origin, $"Öffne die Box mit den Sachen welche am {saveBox.createDate:dd.MM hh:mm tt} aus dem Schließfach: {saveBox.origin} genommen wurden.", priceString, "OPEN_LOCKER_MASTER_BOX").withData(data).needsConfirmation("Schließfach öffnen?", $"Wirklich für ${price} öffnen?"));
                }
            }

            return new MenuMenuItem(menu.Name, menu);
        }

        private bool onLockerShowLockerMaster(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var id = BlipController.createPointBlip(player, "Spind-Überzuglager", LockerMasterPosition, 20, 50, 255, true, false, $"Locker-{player.getCharacterId()}");

            InvokeController.AddTimedInvoke("Locker-Master-Remover", (i) => {
                BlipController.destroyBlipByName(player, id);
            }, TimeSpan.FromMinutes(2), false);

            return true;
        }

        private bool onOpenLockerMasterBox(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var saveBox = (lockersavebox)data["SaveBox"];
            var price = (decimal)data["Price"];

            if (player.removeCash(price)) {
                using (var db = new ChoiceVDb()) {
                    var find = db.lockersaveboxes.Find(saveBox.id);
                    if (find != null) {
                        find.lastOpened = DateTime.Now;
                        db.SaveChanges();
                    }
                }

                var inv = InventoryController.loadInventory(saveBox.inventoryId);

                inv.addBlockStatement(new InventoryAddBlockStatement(this, i => true));

                InventoryController.showMoveInventory(player, player.getInventory(), inv, null, (player) => {
                    if (inv.getAllItems().Count <= 0) {
                        using (var db = new ChoiceVDb()) {
                            var find = db.lockersaveboxes.Find(saveBox.id);
                            if (find != null) {
                                db.lockersaveboxes.Remove(find);
                                db.SaveChanges();
                            }
                        }
                    }
                }, $"{saveBox.origin}", true);
            } else {
                player.sendBlockNotification("Nicht genügend Geld dabei!", "Nicht genügend Geld", NotifactionImages.Lock);
            }
            return true;
        }

        private bool openLockerDrawer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var drawer = (LockerDrawer)data["LockerDrawer"];

            drawer.onPlayerTryOpen(player);

            return true;
        }

        private bool onLockerDrawerOpen(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var drawer = (LockerDrawer)data["LockerDrawer"];

            drawer.onPlayerOpenInventory(player);

            return true;
        }

        private bool openLockerDrawerAdministrative(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var drawer = (LockerDrawer)data["LockerDrawer"];

            drawer.onPlayerOpen(player);

            return true;
        }

        private bool onChangeLockerDrawerCombination(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;
            var drawer = (LockerDrawer)data["LockerDrawer"];

            drawer.onChangeCombination(player, evt.input);

            return true;
        }

        private bool onRentPublicDrawer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var drawer = (PublicLockerDrawer)data["LockerDrawer"];

            if (menuItemCefEvent is MenuStatsMenuItemEvent) {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

                var timeEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
                var combinationEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
                var phoneNumberEvt = evt.elements[3].FromJson<CheckBoxMenuItemEvent>();

                if (!int.TryParse(combinationEvt.input, out int n)) {
                    player.sendBlockNotification("Die Kombination muss eine Zahl sein!", "Komnbination kein Zahl", NotifactionImages.Lock);
                    return true;
                }

                try {
                    drawer.rent(player, true, PublicLockerDrawer.RentTimeDays[timeEvt.currentIndex], combinationEvt.input, phoneNumberEvt.check);
                } catch (Exception) {
                    player.sendBlockNotification("Die angebene Kombination war keine Zahl!", "Kein Zahl!", NotifactionImages.Lock);
                }
            } else {
                var evt = menuItemCefEvent as ListMenuItemEvent;

                drawer.rent(player, false, PublicLockerDrawer.RentTimeDays[evt.currentIndex], "-1", false);
            }

            return true;
        }

        private bool onUnrentPublicDrawer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var drawer = (PublicLockerDrawer)data["LockerDrawer"];
            var backMoney = (decimal)data["BackMoney"];

            drawer.closeLockerDrawer(false);

            player.addCash(backMoney);

            player.sendNotification(NotifactionTypes.Info, $"Schließfach-Miete erfolgreich beendet. Du hast ${backMoney} zurückerhalten", $"${backMoney} von Spindfach zurückerhalten", NotifactionImages.Lock);

            return true;
        }

        private bool onRegisterCompanyDrawer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var drawer = (CompanyLockerDrawer)data["LockerDrawer"];

            drawer.onRegisterOwner(player);

            return true;
        }


        private bool onChangeCompanyDrawerName(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;
            var drawer = (CompanyLockerDrawer)data["LockerDrawer"];

            drawer.onChangeName(player, evt.input);

            return true;
        }

        #region Support Stuff

        private Menu generateLockerMenu(IPlayer player) {
            return getGenerateMenuForLockerCreation(null);
        }

        public static Menu getGenerateMenuForLockerCreation(Company company) {
            var menu = new Menu("Spindmenü", "Was möchtest du tun?");

            var createMenu = new Menu("Spind erstellen", "Gib die Daten ein");
            createMenu.addMenuItem(new InputMenuItem("Name", "Gib den Namen des Spindes ein", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Preis in $", "Gib den Preis für die Miete pro Tag in Dollar ein. Falls nicht benötigt 0 eintragen", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Spind", "SUPPORT_CREATE_LOCKER", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Company", company } }).needsConfirmation("Spind erstellen?", "Wirklich erstellen?"));

            menu.addMenuItem(new MenuMenuItem("Spind hinzufügen", createMenu, MenuItemStyle.green));

            var list = AllLockers;
            if (company != null) {
                list = AllLockers.Where(l => l.CompanyId == company.Id).ToList();
            }

            foreach (var locker in list) {
                var data = new Dictionary<string, dynamic> {
                    { "Locker", locker },
                };

                var lockerMenu = new Menu(locker.Name, "Was möchtest du tun?");


                var drawerCreateMenu = new Menu("Spindfach erstellen", "Trage die Daten ein");

                var types = ((LockerDrawerType[])Enum.GetValues(typeof(LockerDrawerType))).Select(ldt => ldt.ToString()).ToArray();
                drawerCreateMenu.addMenuItem(new ListMenuItem("Typ", "Der Typ der hinzugefügten Schließfächer", types, ""));
                drawerCreateMenu.addMenuItem(new InputMenuItem("Anzahl", "Wieviele Schließfächer erstellt werden sollen", "", ""));
                drawerCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Wieviele Schließfächer erstellt werden sollen", "SUPPORT_CREATE_DRAWER", MenuItemStyle.green).withData(data).needsConfirmation("Fächer erstellen?", "Fächer wirklich erstellen?"));

                lockerMenu.addMenuItem(new MenuMenuItem(drawerCreateMenu.Name, drawerCreateMenu, MenuItemStyle.green));

                var drawerMenu = new Menu("Spindfächer", "Was möchtest du tun?");
                foreach (var drawer in locker.Drawers) {
                    drawerMenu.addMenuItem(new StaticMenuItem($"{drawer.DisplayNumber}-{drawer.GetType().Name}", $"Dies ist ein {drawer.GetType().Name} mit Nummer {drawer.DisplayNumber} mit Datenbank-Id {drawer.Id} ", ""));
                }

                lockerMenu.addMenuItem(new ClickMenuItem("Spind löschen", "Lösche den Spind", "", "SUPPORT_DELETE_LOCKER", MenuItemStyle.red).withData(data).needsConfirmation("Spind löschen?", "Wirklich löschen?"));

                menu.addMenuItem(new MenuMenuItem(lockerMenu.Name, lockerMenu));
            }

            return menu;
        }

        private bool onSupportCreateLocker(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var company = (Company)data["Company"];
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var priceEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (position, width, height, rotation) => {
                using (var db = new ChoiceVDb()) {
                    var colShape = CollisionShape.Create(position, width, height, rotation, true, false, true, "");

                    var dbLocker = new configlocker {
                        name = nameEvt.input,
                        dayPrice = decimal.Parse(priceEvt.input),
                        colShapeStr = colShape.toShortSave(),
                        phoneNumber = PhoneController.createNewPhoneNumber($"Locker: {nameEvt.input}").number,
                        companyId = company != null ? company.Id : null
                    };

                    db.configlockers.Add(dbLocker);

                    db.SaveChanges();

                    player.sendNotification(NotifactionTypes.Success, $"Spind mit Namen: {nameEvt.input} erfolgreich erstellt!", "Spind erstellt");
                    AllLockers.Add(new Locker(dbLocker.id, dbLocker.name, dbLocker.phoneNumber ?? -1, dbLocker.dayPrice, dbLocker.companyId, colShape));
                }
            });
            player.sendNotification(NotifactionTypes.Info, "Wähle die Position und Größe des Spindes", "Spind erstellen");

            return true;
        }

        private bool onSupportDeleteLocker(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var locker = (Locker)data["Locker"];

            player.sendNotification(NotifactionTypes.Warning, $"Spind erfolgreich gelöscht!", "Spind gelöscht");

            using (var db = new ChoiceVDb()) {
                var dbLocker = db.configlockers.Find(locker.Id);

                db.configlockers.Remove(dbLocker);

                db.SaveChanges();
            }

            foreach (var drawer in locker.Drawers) {
                InventoryController.destroyInventory(drawer.Inventory);
            }

            locker.Shape.Dispose();

            AllLockers.Remove(locker);

            return true;
        }

        private bool onSupportCreateDrawer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var locker = (Locker)data["Locker"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var typeEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var countEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var drawerList = new List<configlockerdrawer>();

            using (var db = new ChoiceVDb()) {

                var biggestNumber = 0;
                try {
                    var biggest = db.configlockerdrawers.Where(ld => ld.lockerId == locker.Id).ToList().Aggregate((i1, i2) => i1.displayNumber > i2.displayNumber ? i1 : i2);

                    if (biggest != null) {
                        biggestNumber = biggest.displayNumber;
                    }
                } catch (Exception e) {

                }

                var type = (LockerDrawerType)Enum.Parse(typeof(LockerDrawerType), typeEvt.currentElement);

                var count = int.Parse(countEvt.input);

                for (var i = 0; i < count; i++) {
                    var dbDrawer = new configlockerdrawer {
                        combination = "",
                        lockerId = locker.Id,
                        displayNumber = biggestNumber + i + 1,
                        data = "",
                        type = (int)type,
                    };

                    db.configlockerdrawers.Add(dbDrawer);

                    drawerList.Add(dbDrawer);
                }

                db.SaveChanges();

                foreach (var dbDrawer in drawerList) {
                    var drawer = LockerDrawer.createLockerFromDb(dbDrawer, locker);
                    locker.addLockerDrawer(drawer);
                }

                player.sendNotification(NotifactionTypes.Success, $"Erfolgreich {count} Spindfächer vom Typ: {type} erstellt!", "Spindfächer erstellt!");
            }


            return true;
        }

        #endregion
    }
}