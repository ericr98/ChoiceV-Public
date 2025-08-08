using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.AnalyseSpots;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    public class AnalyseSpotFunctionality : CompanyFunctionality {
        private List<AnalyseSpot> Spots;

        public AnalyseSpotFunctionality() : base() {
            Spots = new List<AnalyseSpot>();
        }

        public AnalyseSpotFunctionality(Company company) : base(company) {
            Spots = new List<AnalyseSpot>();
            Company = company;
        }

        public override string getIdentifier() {
            return "ANALYSE_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Analysespot Anbindung", "Ermöglicht es Spots für die Beweisanalyse zu setzen");
        }

        public override void onLoad() {
            Company.registerCompanyAdminElement(
                "CREATE_ANALYSE_SPOT",
                onCreateAnalyseSpotGenerator,
                OnCreateAnalyseSpotCallback
            );

            var settings = Company.getSettings("ANALYSE_SPOTS");
            using(var db = new ChoiceVDb()) {
                foreach(var setting in settings) {
                    var dbSpot = db.configanalysespots.Find(int.Parse(setting.settingsValue));
                    if(dbSpot != null) {
                        var spot = AnalyseSpotController.loadAnalyseSpot(dbSpot);
                        Spots.Add(spot);
                    }
                }
            }
        }

        private MenuElement onCreateAnalyseSpotGenerator(IPlayer player) {
            var menu = new Menu("Analsysespot erstellen", "Was möchtest du tun?");

            menu.addMenuItem(new StaticMenuItem("Information", "Blut und Waffenbeweisspots sollten um eine bestimmte Werk/Labortische gesetzt werden. Für Fahrzeuglackspots sollte ein Parkplatz und angrenzrendes Terminal enthalten sein, da das Fahrzeug zum abgleich im Spot sein muss.", ""));
            menu.addMenuItem(new ListMenuItem("Analysespot erstellen", "Erstelle eine Analysespot mit vom gegebenen Typ", Enum.GetValues<EvidenceType>().Select(t => t.ToString()).ToArray(), "ON_START_CREATION"));

            var listMenu = new Menu("Vorhandene Spots", "Welches möchtest du löschen?");

            foreach(var spot in Spots) {
                var x = Math.Round(spot.CollisionShape.Position.X, 2);
                var y = Math.Round(spot.CollisionShape.Position.Y, 2);
                var z = Math.Round(spot.CollisionShape.Position.Z, 2);

                listMenu.addMenuItem(new ClickMenuItem($"{spot.GetType().Name}", $"Der Spot vom Typ {spot.GetType().Name} hat seinen Mittelpunkt in X: {x}, Y: {y}, Z: {z}", $"X: {x}, Y: {y}, Z: {z}", "DELETE_ANALYSESPOT").needsConfirmation("Spot löschen?", "Spot wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Spot", spot } }));
            }

            menu.addMenuItem(new MenuMenuItem(listMenu.Name, listMenu));

            return menu;
        }

        private void OnCreateAnalyseSpotCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "ON_START_CREATION") {
                var evt = menuItemCefEvent as ListMenuItemEvent;
                var type = Enum.Parse<EvidenceType>(evt.currentElement);

                player.sendNotification(NotifactionTypes.Info, "Setze nun den Spot", "");
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {

                    using(var db = new ChoiceVDb()) {
                        var spot = new configanalysespot {
                            position = p.ToJson(),
                            height = h,
                            width = w,
                            rotation = r,
                            trackVehicles = type == EvidenceType.CarPaint ? 1 : 0,
                            codeItem = AnalyseSpotController.getCodeItemFromEvidenceString(type)
                        };

                        db.configanalysespots.Add(spot);
                        db.SaveChanges();

                        var inv = InventoryController.createInventory(spot.id, 1000, InventoryTypes.AnalyseSpot);
                        spot.inventoryId = inv.Id;

                        db.SaveChanges();

                        Spots.Add(AnalyseSpotController.loadAnalyseSpot(spot));

                        Company.setSetting($"ANALYSE_SPOT_{spot.id}", spot.id.ToString(), "ANALYSE_SPOTS");

                        player.sendNotification(NotifactionTypes.Success, $"Der Analysespot mit ID {spot.id} wurde erfolgreich erstellt", $"Analysespot {spot.id} erstellt");
                    }
                });
            } else if(subEvent == "DELETE_ANALYSESPOT") {
                var spot = (AnalyseSpot)data["Spot"];

                using(var db = new ChoiceVDb()) {
                    var dbSpot = db.configanalysespots.Find(spot.Id);

                    if(dbSpot != null) {
                        spot.CollisionShape.Dispose();
                        db.configanalysespots.Remove(dbSpot);

                        InventoryController.destroyInventory(spot.AnalyseInventory);

                        Spots.Remove(spot);

                        db.SaveChanges();
                        player.sendNotification(NotifactionTypes.Info, "Der Spot wurde erfolgreich gelöscht", "");
                    }
                }
            }
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("CREATE_ANALYSE_SPOT");

            using(var db = new ChoiceVDb()) {
                foreach(var spot in Spots) {
                    var dbSpot = db.configanalysespots.Find(spot.Id);

                    if(dbSpot != null) {
                        spot.CollisionShape.Dispose();
                        db.configanalysespots.Remove(dbSpot);

                        InventoryController.destroyInventory(spot.AnalyseInventory);
                    }
                }

                Spots = null;

                db.SaveChanges();
            }
        }
    }

    public class AnalyseSpotController : ChoiceVScript {
        public AnalyseSpotController() {
            EventController.addCollisionShapeEvent("INTERACT_ANALYSPOT", onInteractAnalyseSpot);
            EventController.addMenuEvent("ANALYSE_EVIDENCE", onAnalyseEvidence);
            EventController.addMenuEvent("END_ANALYSE", onEndAnalyseEvidence);
            EventController.addMenuEvent("ALIGN_EVIDENCE", onAlignEvidence);
        }


        public static string getCodeItemFromEvidenceString(EvidenceType type) {
            switch(type) {
                case EvidenceType.Blood:
                    return typeof(DnaSpot).Name;
                case EvidenceType.CartridgeCase:
                    return typeof(WeaponSpot).Name;
                case EvidenceType.CarPaint:
                    return typeof(CarPaintSpot).Name;
                default:
                    return null;
            }
        }

        internal static AnalyseSpot loadAnalyseSpot(configanalysespot row) {
            var shape = CollisionShape.Create(row.position.FromJson(), row.width, row.height, row.rotation, true, row.trackVehicles == 1, true, "INTERACT_ANALYSPOT");
            var type = Type.GetType("ChoiceVServer.Model.AnalyseSpots." + row.codeItem, false);

            if(row.inventoryId == -1) {
                var inv = InventoryController.createInventory(row.id, 1000, InventoryTypes.AnalyseSpot);
                row.inventoryId = inv.Id;
            }

            AnalyseSpot spot = Activator.CreateInstance(type, shape, row.inventoryId) as AnalyseSpot;
            spot.Id = row.id;
            spot.AnalyseEnd = row.analyseEnd ?? DateTime.MinValue;

            return spot;
        }

        private bool onInteractAnalyseSpot(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
            var spot = (AnalyseSpot)data["spot"];

            spot.onInteraction(player);

            return true;
        }

        private bool onAnalyseEvidence(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var ev = (Evidence)data["Evidence"];
            var spot = (AnalyseSpot)data["Spot"];

            if(spot.analyseInProgress()) {
                Logger.logWarning(LogCategory.Player, LogActionType.Event, $"Player tried to analyse something while Analyse in progess!: charId: {player.getCharacterId()}", $"{player.getCharacterId()}");
                return false;
            }

            if(player.getInventory().moveItem(spot.AnalyseInventory, ev)) {
                spot.AnalyseEnd = DateTime.Now + EVIDENCE_ANALYSE_TIME;
                using(var db = new ChoiceVDb()) {
                    var dbSpot = db.configanalysespots.FirstOrDefault(s => s.id == spot.Id);
                    dbSpot.analyseEnd = DateTime.Now + EVIDENCE_ANALYSE_TIME;
                    db.SaveChanges();
                }

                ev.Analyzed = true;
                player.sendNotification(NotifactionTypes.Success, "Du hast einen Beweis in Analyse gegeben!", "Beweis abgegeben", NotifactionImages.MagnifyingGlass);
            } else {
                player.sendBlockNotification("Du konntest den Beweis nicht in analyse geben!", "Beweis blockiert!", NotifactionImages.MagnifyingGlass);
            }

            Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"Player gave Evidence to analyse: charId: {player.getCharacterId()}, ev: {ev.ToJson()}");
            return true;
        }

        private bool onEndAnalyseEvidence(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var spot = (AnalyseSpot)data["Spot"];

            var item = spot.AnalyseInventory.getItem<Evidence>(e => true);

            if(item != null) {
                if(spot.AnalyseInventory.moveItem(player.getInventory(), item)) {
                    player.sendNotification(NotifactionTypes.Success, "Du hast den analysierten Beweis wieder aufgenommen!", "Beweis analysiert!", NotifactionImages.MagnifyingGlass);
                    return true;
                } else {
                    player.sendBlockNotification("Du konntest den Beweis nicht aufnehmen!", "Inventar blockiert!", NotifactionImages.MagnifyingGlass);
                }
            } else {
                Logger.logError($"onEndAnalyseEvidence: Something went wrong",
                    $"Fehler bei Beweisanalyse: Item konnte nicht gefunden werden", player);
            }

            return false;
        }

        private bool onAlignEvidence(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var spot = (AnalyseSpot)data["Spot"];
            var evidence = (Evidence)data["Evidence"];
            spot.onAlignWithEvidence(player, evidence);

            return true;
        }
    }
}
