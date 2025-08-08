using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.LockerSystem.Model {
    public class PublicLockerDrawer : LockerDrawer {
        public static string[] RentTimes = new string[] { "1 Tag", "3 Tage", "7 Tage", "14 Tage", "30 Tage" };
        public static int[] RentTimeDays = new int[] { 1, 3, 7, 14, 30 };

        public DateTime RentEndDate;
        public long ConnectedPhoneNumber;
        public bool NotificationHasBeenSent;

        public PublicLockerDrawer(configlockerdrawer dbDrawer, Locker owner) : base(dbDrawer.id, owner, dbDrawer.displayNumber, dbDrawer.combination, dbDrawer.inventoryId, dbDrawer.ownerId) {
            if(dbDrawer.data != "") {
                var split = dbDrawer.data.Split('#');
                RentEndDate = split[0].FromJson<DateTime>();
                ConnectedPhoneNumber = long.Parse(split[1]);
                NotificationHasBeenSent = bool.Parse(split[2]);
            } else {
                ConnectedPhoneNumber = -1;
            }
        }

        public override VirtualMenu getShowMenu(IPlayer player) {
            var name = $"{DisplayNumber}: Frei";

            if(InUse) {
                name = $"{DisplayNumber}: In Benutzung";
            }

            return new VirtualMenu(name, () => {
                var menu = new Menu(name, $"Was möchtest du tun?");

                var data = new Dictionary<string, dynamic> {
                    { "LockerDrawer", this }
                };

                if(InUse) {
                    menu.addMenuItem(new ClickMenuItem("Öffnen", "Öffne das Schließfach mit der richtigen Kombination", "", "OPEN_LOCKER_DRAWER").withData(data));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Mietpreis", $"Der Preis pro Tag an diesem Schließfach beträgt: ${Parent.DayPrice}.", $"${Parent.DayPrice}/Tag"));
                    menu.addMenuItem(new ListMenuItem("Mietdauer", "Miete das Schließfach", RentTimes, ""));
                    menu.addMenuItem(new InputMenuItem("Kombination", "Gib die Kombination des Schließfaches an. Es sollte mind. 4 Stellen haben.", "", InputMenuItemTypes.password, ""));
                    menu.addMenuItem(new CheckBoxMenuItem("Telefonnummer eingeben", "Telefonnummer eingeben an welche wichtige Informationen über das Fach geschickt werden.", false, ""));
                    menu.addMenuItem(new MenuStatsMenuItem("Mieten", "Miete das Schließfach für die gewählte Zeit", "RENT_PUBLIC_LOCKER_DRAWER", MenuItemStyle.green).withData(data).needsConfirmation("Schließfach mieten?", "Schließfach wirklich mieten?"));
                }

                return menu;
            });
        }

        public override void onPlayerTryOpen(IPlayer player) {
            CombinationLockController.requestPlayerCombination(player, Combination, (p, d) => {
                onPlayerOpen(player);
            });
        }

        public override void onPlayerOpen(IPlayer player) {
            var menu = new Menu($"Geöffnetes Schließfach: {DisplayNumber}", "Was möchtest du tun?");

            var data = new Dictionary<string, dynamic> {
                { "LockerDrawer", this }
            };

            menu.addMenuItem(new ClickMenuItem("Fach öffnen", "Öffne das Fach.", "", "OPEN_LOCKER_DRAWER_INVENTORY", MenuItemStyle.green).withData(data));
            menu.addMenuItem(new StaticMenuItem("Mietende", $"Das Schließfach läuft {RentEndDate:dd.MM HH:mm tt} ab.", $"{RentEndDate:dd.MM HH:mm tt}"));
            menu.addMenuItem(new StaticMenuItem("Mietpreis", $"Der Preis pro Tag an diesem Schließfach beträgt: ${Parent.DayPrice}.", $"${Parent.DayPrice}/Tag"));

            menu.addMenuItem(new ListMenuItem("Mietdauer verlängern", "Verlängere die Mietdauer des Schließfaches. ACHTUNG: Es sind insgesamt maximal 30 Tage Mietdauer (von heute) möglich. Zusätzlich eingeworfenes Geld wird nicht erstattet!", RentTimes, "RENT_PUBLIC_LOCKER_DRAWER").withData(data).needsConfirmation("Schließfach mieten?", "Mietdauer wirklich verlängern?"));
            menu.addMenuItem(new InputMenuItem("Kombination ändern", "Ändere die Kombination des Schließfaches (max 6 Stellen).", "", InputMenuItemTypes.password, "CHANGE_LOCKER_DRAWER_COMNBINATION", MenuItemStyle.yellow).withData(data).needsConfirmation("Kombination ändern?", "Kombination wirklich ändern?"));

            if(Inventory == null || Inventory.getAllItems().Count <= 0) {
                var restTime = Convert.ToDecimal(Math.Floor((RentEndDate - DateTime.Now).TotalDays));
                var backMoney = Math.Round(restTime * Parent.DayPrice, 2);

                data.Add("BackMoney", backMoney);

                menu.addMenuItem(new ClickMenuItem("Miete beenden", "Es befinden sich noch Gegenstände im Schließfach. Restgeldauszahlung nicht möglich!", $"+${backMoney}", "UNRENT_PUBLIC_LOCKER_DRAWER", MenuItemStyle.yellow).withData(data).needsConfirmation("Spindmiete abbrechen?", $"Abbrechen und ${backMoney} zurückerhalten"));
            } else {
                menu.addMenuItem(new StaticMenuItem("Miete beenden", "Es befinden sich noch Gegenstände im Schließfach. Restgeldauszahlung nicht möglich!", "Nicht möglich", MenuItemStyle.red));
            }

            player.showMenu(menu);
        }

        public override void onPlayerOpenInventory(IPlayer player) {
            if(Inventory == null) {
                createInventoryIfNull(7.5f);
            }

            InventoryController.showMoveInventory(player, player.getInventory(), Inventory, null, null, $"Schließfach Nr. {DisplayNumber}", true);
        }

        public void rent(IPlayer player, bool initialRent, int days, string combination, bool saveNumber) {
            if(player.removeCash(Parent.DayPrice * days)) {
                var alreadyTime = RentEndDate - DateTime.Now;

                if(alreadyTime.TotalMilliseconds < 0) {
                    alreadyTime = TimeSpan.Zero;
                }

                var timeSpan = alreadyTime + TimeSpan.FromDays(days);

                if(timeSpan.TotalDays > 30) {
                    timeSpan = TimeSpan.FromDays(30);
                }

                RentEndDate = DateTime.Now + timeSpan;

                if(initialRent) {
                    Combination = combination;
                    OwnerId = player.getCharacterId();
                    if(saveNumber) {
                        ConnectedPhoneNumber = player.getMainPhoneNumber();
                    }
                }

                updateDbData();

                player.sendNotification(NotifactionTypes.Success, $"Das Schließfach ist bis {RentEndDate:dd.MM HH:mm tt} gemietet.", "Schließfach gemietet", NotifactionImages.Lock);

                if(ConnectedPhoneNumber != -1) {
                    PhoneController.sendSMSToNumber(Parent.PhoneNumber, ConnectedPhoneNumber, $"Schließfach Nr. {DisplayNumber} am {Parent.Name} erfolgreich gemietet/verlängert. Das Schließfach ist bis {RentEndDate:dd.MM HH:mm tt} gemietet");
                }

                onPlayerOpen(player);
            } else {
                player.sendBlockNotification("Miete konnte nicht bezahlt werden!", "Schließfachmieten fehlgeschlagen", NotifactionImages.Lock);
            }
        }

        protected override string getDbData() {
            return $"{RentEndDate.ToJson()}#{ConnectedPhoneNumber}#{NotificationHasBeenSent}";
        }

        public override void onUpdate() {
            if(InUse && ConnectedPhoneNumber != -1 && !NotificationHasBeenSent && RentEndDate < DateTime.Now + TimeSpan.FromDays(1)) {
                NotificationHasBeenSent = true;
                PhoneController.sendSMSToNumber(Parent.PhoneNumber, ConnectedPhoneNumber, $"Das Schließfach Nr. {DisplayNumber} am {Parent.Name} läuft in einem Tag ab. Hinterlassene Gegenstände werden eingelagert und können gegen eine steigende Gebühr wieder abgeholt werden.");

                updateDbData();
            }


            if(InUse && RentEndDate < DateTime.Now) {
                closeLockerDrawer(true);
            }
        }

        public void closeLockerDrawer(bool overDueReason) {
            if(ConnectedPhoneNumber != -1) {
                if(overDueReason) {
                    PhoneController.sendSMSToNumber(Parent.PhoneNumber, ConnectedPhoneNumber, $"Die Miete für das Schließfach Nr. {DisplayNumber} am {Parent.Name} wurde überschritten. Hinterlassene Gegenstände werden eingelagert und können gegen eine steigende Gebühr in der Nähe des Hafens wieder abgeholt werden.");
                } else {
                    PhoneController.sendSMSToNumber(Parent.PhoneNumber, ConnectedPhoneNumber, $"Vielen Dank, dass Sie unseren Schließfachservice genutzt haben. Ihre Telefonnummer wird nun aus dem Syste gelöscht.");
                }
            }


            //TODO Schließfächer werden nach Ablauf gelöscht!
            using(var db = new ChoiceVDb()) {
                var find = db.configlockerdrawers.Include(ld => ld.inventory).ThenInclude(iv => iv.items).ThenInclude(it => it.config).FirstOrDefault(i => i.id == Id);

                if(find != null) {
                    find.combination = "";
                    find.data = "";
                    find.inventoryId = null;
                    find.ownerId = null;

                    if(Inventory != null) {
                        if(Inventory.getAllItems().Count > 0) {
                            if(find.inventory.items.Any(i => i.config.suspicious == 1)) {
                                //TODO Send dispatch 
                            }

                            var newBox = new lockersavebox {
                                inventoryId = InventoryId ?? -1,
                                combination = Combination,
                                origin = $"Nr. {DisplayNumber} - {Parent.Name}",
                                price = Parent.DayPrice,
                                createDate = DateTime.Now,
                                lastOpened = DateTime.MinValue,
                                ownerId = OwnerId ?? -1
                            };

                            db.lockersaveboxes.Add(newBox);
                        } else {
                            InventoryController.destroyInventory(Inventory);
                        }
                    }

                    db.SaveChanges();
                }

                InventoryId = null;
                OwnerId = null;
            }
        }
    }
}
