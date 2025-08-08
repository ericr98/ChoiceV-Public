using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.Companies {

    //Angemeldete Fahrzeuge werden bei jemden angezeigt!

    //TODO Können bei Waffenkammer Anfragen stellen (Dienstwaffe, Standard-Outfit, Custom-Outfits, etc.), die werden dann gespeichert (mit Nachrichtenfeld!)
    //TODO Und Leute mit Permission können das dann bestätigen und Anfrage landet entweder in Spielerinventar (wenn nah und direkt) sonst nach 5 - 10min im Spind (auch nach Menge vlt?)

    public class PoliceController : ChoiceVScript {
        public PoliceController() { }

        public static int getNextItemEvidenceId() {
            using(var db = new ChoiceVDb()) {
                var dItem = db.configitems.FirstOrDefault(i => i.codeItem == typeof(Evidence).Name);
                var cItem = InventoryController.getConfigItemForType<Evidence>();

                var split = cItem.additionalInfo.Split('#');
                var value = int.Parse(split[1]);
                value += 1;

                cItem.additionalInfo = split[0] + "#" + value;
                dItem.additionalInfo = split[0] + "#" + value;

                db.SaveChanges();

                return value;
            }
        }
    }

    public class PoliceFunctionality : CompanyFunctionality {

        public PoliceFunctionality() { }
        public PoliceFunctionality(Company company) : base(company) { }

        public override string getIdentifier() {
            return "POLICE_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Polizei Funktionen", "Füg die Polizeifunktionen hinzu. Wie z.B. Beweise markieren, oder den Detective Modus");
        }

        public override List<string> getSinglePermissionsGranted() {
            return ["DETECTIVE_MODE", "MARK_EVIDENCE", "VEHICLES_ACCESS", "OPEN_DOORS_FORCEFULLY", "POLICE_CAMERA_MODE"];
        }

        public override void onLoad() {
            Company.registerCompanySelfElement(
                "POLICE_MARK_EVIDENCE",
                getEvidenceMenu,
                onMarkEvidence,
                "MARK_EVIDENCE"
            );

            Company.registerCompanySelfElement(
                "POLICE_DETECTIVE_MODE",
                (c, p) => new ClickMenuItem("Detektive Modus", "Falls nicht benötigt bitte wieder abschalten!", "", "TOGGLE_DETECTIVE_MODE"),
                onToggleDetectiveMode,
                "DETECTIVE_MODE"
            );

            Company.registerCompanySelfElement(
                "POLICE_CAMERA_MODE",
                (c, p) => new ClickMenuItem("Kameras anzeigen", "Lasse dir die Kameras in der Umgebung und ihre Protokolle ausgeben!", "", "POLICE_CAMERA_MODE"),
                onShowCameraMode,
                "POLICE_CAMERA_MODE"
            );

        }

        public override void onRemove() {
            Company.unregisterCompanyElement("POLICE_MARK_EVIDENCE");
            Company.unregisterCompanyElement("POLICE_DETECTIVE_MODE");
        }

        private Menu getEvidenceMenu(Company company, IPlayer player) {
            var items = player.getInventory().getAllItems();

            var menu = new Menu("Beweis markieren", "Ein Item als Beweis markieren");

            foreach(var item in items) {
                if(!item.isBlocked() && !item.CanBeStacked) {
                    var itemData = new Dictionary<string, dynamic> {
                        {"Item", item }
                    };
                    menu.addMenuItem(((ClickMenuItem)new ClickMenuItem(item.Name, $"{item.Name} mit Beschreibung: \"{item.Description}\" als Beweis markieren", "", "MARK_ITEM_AS_EVIDENCE").withData(itemData)).needsConfirmation("Wirklich kennzeichnen?", "Dieses Item als Beweis markieren?"));
                }
            }

            return menu;
        }

        private void onMarkEvidence(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Item)data["Item"];

            if(!item.isBlocked() && !item.CanBeStacked) {
                if(item.Description == null || !item.Description.StartsWith("Beweis: ")) {
                    var id = PoliceController.getNextItemEvidenceId();
                    item.setAsEvidence(id);

                    player.sendNotification(NotifactionTypes.Success, $"Item als Beweis {id} registriert!", "Beweis registriert!", NotifactionImages.MagnifyingGlass);
                } else {
                    player.sendBlockNotification("Dieses Item ist bereits ein Beweis!", "Ist bereits Beweis!", NotifactionImages.MagnifyingGlass);
                }
            }
        }

        private void onToggleDetectiveMode(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(EvidenceController.activateDetectiveMode(player)) {
                player.sendNotification(NotifactionTypes.Info, "Detektive-Modus gestartet", "Detektive gestartet", NotifactionImages.MagnifyingGlass);
            } else {
                EvidenceController.deactivateDetectiveMode(player);
                player.sendNotification(NotifactionTypes.Info, "Detektive-Modus gestoppt", "Detektive gestoppt", NotifactionImages.MagnifyingGlass);
            }
        }

        private void onShowCameraMode(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.showMenu(CamController.getCamProtocolMenu(player), false);
        }

        public override void onLastEmployeeLeaveDuty() {
            //TODO Check if in Patrol, then remove from patrol and if last member: remove patrol
        }
    }
}
