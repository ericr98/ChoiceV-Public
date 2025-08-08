using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.LockerSystem.Model {
    public class CompanyLockerDrawer : LockerDrawer {
        private string Name;

        public CompanyLockerDrawer(configlockerdrawer dbDrawer, Locker owner) : base(dbDrawer.id, owner, dbDrawer.displayNumber, dbDrawer.combination, dbDrawer.inventoryId, dbDrawer.ownerId) {
            Name = dbDrawer.data;
        }

        public override VirtualMenu getShowMenu(IPlayer player) {
            var name = $"{DisplayNumber}: Frei";

            if(InUse) {
                name = $"{DisplayNumber}: {Name}";
            }

            return new VirtualMenu(name, () => {
                var menu = new Menu(name, $"Was möchtest du tun?");

                var data = new Dictionary<string, dynamic> {
                    { "LockerDrawer", this }
                };

                if(Combination != "") {
                    menu.addMenuItem(new ClickMenuItem("Öffnen", "Öffne das Schließfach mit der richtigen Kombination", "", "OPEN_LOCKER_DRAWER").withData(data));
                }

                if(CompanyController.hasPlayerPermission(player, Parent.Company, "EDIT_LOCKER")) {
                    menu.addMenuItem(new ClickMenuItem("Öffnen (administrativ)", "Öffne das Schließfach mit der Administrativ-Kombination. Diese Aktion wird vom Sicherheitssystem erfasst!", "", "OPEN_LOCKER_DRAWER_ADMINISTRATIVE", MenuItemStyle.yellow).withData(data));
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
            var menu = new Menu($"Schließfach {DisplayNumber}", "Was möchtest du tun?");

            var data = new Dictionary<string, dynamic> {
                { "LockerDrawer", this }
            };

            if(InUse) {
                menu.addMenuItem(new ClickMenuItem("Öffnen", "Öffne das Fach.", "", "OPEN_LOCKER_DRAWER_INVENTORY", MenuItemStyle.green).withData(data));
            } else {
                menu.addMenuItem(new StaticMenuItem("Öffnen nicht möglich", "Der Spind ist noch auf niemanden registriert", "", MenuItemStyle.yellow));
            }

            if(CompanyController.hasPlayerPermission(player, Parent.Company, "EDIT_LOCKER")) {
                menu.addMenuItem(new InputMenuItem("Namen ändern", "Ändere den Anzeigenamen des Schließfaches. Er darf kein # enthalten und sollte weniger als 10 Zeichen haben", $"{Name}", "CHANGE_COMPANY_LOCKER_DRAWER_NAME").withData(data));
            }

            menu.addMenuItem(new InputMenuItem("Kombination ändern", "Ändere die Kombination des Schließfaches (max 6 Stellen). Befugtes Personal kann den Spind jedoch weiterhin mit dem Universalcode öffnen.", $"", InputMenuItemTypes.password, "CHANGE_LOCKER_DRAWER_COMNBINATION", MenuItemStyle.yellow).withData(data).needsConfirmation("Kombination ändern?", "Kombination wirklich ändern?"));
            menu.addMenuItem(new ClickMenuItem("Auf dich registrieren", "Registriere den Spind auf dich. ", "", "REGISTER_COMPANY_LOCKER_DRAWER").withData(data));

            player.showMenu(menu);
        }

        public override void onPlayerOpenInventory(IPlayer player) {
            if(Inventory == null) {
                createInventoryIfNull(40f);
            }

            InventoryController.showMoveInventory(player, player.getInventory(), Inventory, null, null, $"Schließfach Nr. {DisplayNumber}", true);
        }

        public override void onUpdate() {
            return;
        }

        protected override string getDbData() {
            return $"{Name}";
        }

        public void onChangeName(IPlayer player, string name) {
            if(name.Length > 10) {
                player.sendBlockNotification("Der Name ist zu lang für die Anzeige", "Name zu lang", NotifactionImages.Lock);
                return;
            }

            if(name.Contains('#')) {
                player.sendBlockNotification("Der Name darf kein # enthalten", "Name falsch", NotifactionImages.Lock);
                return;
            }

            Name = name;

            updateDbData();

            player.sendNotification(NotifactionTypes.Success, "Fachname erfolgreich geändert!", "Fachname geändert");
        }

        public void onRegisterOwner(IPlayer player) {
            OwnerId = player.getCharacterId();

            updateDbData();

            player.sendNotification(NotifactionTypes.Success, "Erfolgreich als Fachbesitzer registriert!", "Als Fachbesitzer gesetzt");
        }
    }
}
