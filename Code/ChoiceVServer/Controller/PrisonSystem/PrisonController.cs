using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using ChoiceVServer.Controller.PrisonSystem.Model;
using AltV.Net.Elements.Entities;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model;
using ChoiceVServer.Admin.Tools;
using Microsoft.EntityFrameworkCore;
using AltV.Net.Data;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;

namespace ChoiceVServer.Controller.PrisonSystem {
    public class PrisonController : ChoiceVScript {
        private const float HOURS_PER_IMPRISON_UNIT = 1;

        public const float TIMELEFT_ACTIVE_PERCENTAGE = 0.15f;
        public const float PASSIV_TIME_MULTIPLIER = 3f;

        private static readonly TimeSpan PRISON_UPDATER_TIME = TimeSpan.FromMinutes(2);

        private static readonly List<Prison> AllPrisons = new();

        public static readonly TimeSpan FOOD_HOW_OFTEN_TIMESPAN = TimeSpan.FromMinutes(20);

        //Knast Mini-Jobs:
        // Müll aufsammeln
        // Putzen, etc.

        //Knast Jobs: Verarbeiterprozesse
        //Wäscherei:
        //      Schmutzige Wäsche (Syntetic, Natur, etc.)
        //      -> Waschen mit richtigem Waschmittel
        //      -> Trocknen (ggf, oder aufhängen)
        //      -> Falten
        //      -> Verpacken
        //      -> Mit Label versehen

        //Holzwerkstadt: 
        //      Holzstücke (4 by 2)
        //      -> Schneiden
        //      -> Schleifen
        //      -> Vorbohren
        //      -> Zusammenschrauben

        //Küche:
        //      Rohstoffe (Kartoffeln, Fleisch, etc.)
        //      -> Schneiden
        //      -> Kochen (je nach Rezept)
        
        public PrisonController() {
            onLoad();

            EventController.addMenuEvent("OPEN_INMATE_BELONGINGS", onOpenInmateBelongings);
            EventController.addMenuEvent("PRISON_START_RELEASE", onPrisonStartRelease);

            EventController.addMenuEvent("PRISON_OPEN_CELL_STORAGE", onPrisonOpenCellStorage);
            EventController.addMenuEvent("PRISON_CHANGE_CELL_COMBINATION", onPrisonChangeCellCombination);
            InvokeController.AddTimedInvoke("PrisonUpdater", updatePrisons, PRISON_UPDATER_TIME, true);

            EventController.addMenuEvent("PRISON_ORDER_FOOD", onPrisonOrderFood);
            
            EventController.addMenuEvent("PRISON_CELL_SET_INMATE", onPrisonCellSetInmate);
            
            #region Support Stuff

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Firmen,
                    "Gefängnisse verwalten",
                    generateSupportPrisonMenu
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_PRISON", onSuppotCreatePrison);
            EventController.addMenuEvent("SUPPORT_DELETE_PRISON", onSupportDeletePrison);

            EventController.addMenuEvent("SUPPORT_PRISON_CREATE_NEW_SPOT", onSupportCreateSpot);
            EventController.addMenuEvent("SUPPORT_PRISON_DELETE_SPOT", onSupportDeleteSpot);

            EventController.addMenuEvent("SUPPORT_MANAGE_GET_OUT_POINTS", onSupportManageGetOutPoints);
            EventController.addMenuEvent("SUPPORT_MANAGE_CELLS", onSupportManageCells);

            PedController.addNPCModuleGenerator("Gefängniswärter Modul", prisonGuardNpcModuleGenerator, prisonGuardNpcModuleCallback);
            PedController.addNPCModuleGenerator("Gefängnisessensausgabe Modul", prisonFoodNpcModuleGenerator, prisonFoodNpcModuleCallback);

            #endregion
        }

        private static void onLoad() {
            using(var db = new ChoiceVDb()) {
                foreach(var dbPrison in db.configprisons.Include(p => p.configprisoncells)) {
                    var prison = new Prison(dbPrison.id, dbPrison.name);

                    // Set outline
                    var outlineSpotStrings = dbPrison.outlines?.Split("---");
                    foreach(var spotString in outlineSpotStrings ?? Enumerable.Empty<string>()) {
                        prison.addOutline(CollisionShape.Create(spotString));
                    }

                    // Set register spots
                    var registerSpotStrings = dbPrison.registerSpots?.Split("---");
                    foreach(var spotString in registerSpotStrings ?? Enumerable.Empty<string>()) {
                        prison.addRegisterSpot(CollisionShape.Create(spotString));
                    }

                    // Set belongings spots
                    var belongingsSpotStrings = dbPrison.belongingsSpots?.Split("---");
                    foreach(var spotString in belongingsSpotStrings ?? Enumerable.Empty<string>()) {
                        prison.addBelongingSpot(CollisionShape.Create(spotString));
                    }
                    
                    // Set get out points
                    var getOutPointStrings = dbPrison.getOutPoints?.Split("---");
                    foreach(var spotString in getOutPointStrings ?? Enumerable.Empty<string>()) {
                        var parts = spotString.Split('|');
                        var fromShape = CollisionShape.Create(parts[0]);
                        var toPos = new Position(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
                        var message = parts[4];
                        var isFinalPoint = parts.Length > 5 && bool.Parse(parts[5]);

                        prison.addGetOutPoint(fromShape, toPos, message, isFinalPoint);
                    }
                    
                    // Set cells
                    foreach(var dbCell in dbPrison.configprisoncells) {
                        var inv = InventoryController.loadInventory(dbCell.inventoryId ?? -1);
                        if(inv == null) {
                            var newInv = InventoryController.createInventory(dbCell.id, 10, InventoryTypes.PrisonCell);
                            dbCell.inventoryId = newInv.Id;
                        }

                        var cell = new PrisonCell(dbCell.id, dbCell.name, CollisionShape.Create(dbCell.collisionShape), inv, dbCell.combination.FromJson<int[]>());
                        prison.addCell(cell);
                    }

                    AllPrisons.Add(prison);
                }
                
                db.SaveChanges();

                foreach(var dbInmate in db.prisoninmates.Include(c => c._char)) {
                    var prison = AllPrisons.Find(p => p.Id == dbInmate.prisonId);
                    var inmate = new PrisonInmate(dbInmate.id, dbInmate.charId, dbInmate.name, dbInmate.timeLeftOnline, dbInmate.timeLeftOffline, dbInmate.inventoryId, dbInmate.clearedForExit, dbInmate.escaped, dbInmate.releasedDate, dbInmate.createdDate);
                    prison?.addInmate(inmate);

                    foreach(var dbCell in dbInmate.configprisoncells) {
                        var cell = prison?.Cells.FirstOrDefault(p => p.Id == dbCell.id);

                        cell?.addPrisonInmate(inmate);
                    }
                    
                    if(prison != null && !dbInmate.clearedForExit && !prison.isPositionInPrison(dbInmate._char.position.FromJson())) {
                        Position pos = Position.Zero;
                        if(prison.Outline.Count > 0) {
                            pos = prison.Outline.First().Position;
                        }
                        inmate.playerEscapedPrison(pos);
                    }
                }
            }
        }
            
        private static void updatePrisons(IInvoke invoke) {
            foreach(var prison in AllPrisons) {
                prison.update(PRISON_UPDATER_TIME);
            }
        }

        public static TimeSpan getTimeSpanFromImprisonUnits(float units) {
            return TimeSpan.FromHours(units * HOURS_PER_IMPRISON_UNIT);
        }

        public static int getImprisonUnitsFromTimeSpan(float timeLeftOnline, float timeLeftOffline) {
            return (int)((timeLeftOnline + timeLeftOffline) / (HOURS_PER_IMPRISON_UNIT * 60));
        }

        public static bool hasPlayerAccessToPrison(IPlayer player, Prison prison) {
            var companies = CompanyController.getCompaniesWithPermission(player, "PRISON_CONTROL");
            return companies.Any(c => c.hasFunctionality<CompanyPrisonFunctionality>(f => f.hasPrison(prison)));
        }

        public static List<Prison> getAllPrisons() {
            return AllPrisons;
        }

        public static List<Prison> getPrisons(Predicate<Prison> predicate) {
            return AllPrisons.FindAll(predicate);
        }

        public static Prison getPrisonById(int id) {
            return AllPrisons.Find(p => p.Id == id);
        }

        #region Events

        private bool onOpenInmateBelongings(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            PrisonInmate inmate = data["Inmate"];

            if(inmate != null) {
                inmate.openBelongings(player);
            } else {
                player.sendBlockNotification("Ein Fehler ist aufgetreten. Melde dich im Support. Code: PRISONBEAR 1.", "Kein Insasse");
            }

            return true;
        }

        private bool onPrisonStartRelease(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            PrisonInmate inmate = data["Inmate"];

            if(inmate.FreeToGo) {
                using(var db = new ChoiceVDb()) {
                    var dbInmate = db.prisoninmates.Find(inmate.Id);

                    if(dbInmate != null) {
                        dbInmate.releasedDate = DateTime.Now;
                        db.SaveChanges();
                    }
                    
                    inmate.ReleasedDate = DateTime.Now;
                }

                player.sendNotification(Constants.NotifactionTypes.Success, "Die Freilassung wurde begonnen. Du kannst das Gefängnis nun verlassen.", "Freilassung begonnen", Constants.NotifactionImages.Prison);
            }

            return true;
        }

        
        private static bool onPrisonOpenCellStorage(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var cell = (PrisonCell)data["Cell"];
            InventoryController.showMoveInventory(player, player.getInventory(), cell.Inventory, null, null, cell.Name, true);

            return true;
        }
        
        private static bool onPrisonChangeCellCombination(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var cell = (PrisonCell)data["Cell"];

            if (menuitemcefevent is InputMenuItemEvent evt) {
                var combination = evt.input;
            
                var newCombination = new List<int>();
                foreach(var c in combination) {
                    if(int.TryParse(c.ToString(), out var i)) {
                        newCombination.Add(i);
                    } else {
                        player.sendBlockNotification("Die Kombination darf nur aus Zahlen bestehen", "Falsche Eingabe");
                        return false;
                    }
                }
                cell.Combination = newCombination.ToArray();

                using(var db = new ChoiceVDb()) {
                    var dbCell = db.configprisoncells.Find(cell.Id);
                    if(dbCell != null) {
                        dbCell.combination = cell.Combination.ToJson();
                    } else {
                        player.sendBlockNotification("Ein Fehler ist aufgetreten. Melde dich im Support. Code: PRISONBEAR 3.", "Fehler");
                        return false;
                    }
                    db.SaveChanges();
                }
            
                player.sendNotification(Constants.NotifactionTypes.Info, $"Die Kombination wurde erfolgreich zu {combination} geändert.", "Kombination geändert");
            }
            return true;
        }
        
        private bool onPrisonOrderFood(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var prison = (Prison)data["Prison"];

            var cfgItem = InventoryController.getConfigItemByCodeIdentifier("PRISON_FOOD");
            var item = InventoryController.createItem(cfgItem, 1);

            if(player.getInventory().addItem(item)) {
                player.sendNotification(Constants.NotifactionTypes.Info, "Du hast dein Essen erhalten. Guten ähh.. Appetit!", "Essen (?) erhalten", Constants.NotifactionImages.Prison);
                player.setData("PRISON_LAST_MEAL", DateTime.Now);
            }
            
            
            return true;
        }
        
        private bool onPrisonCellSetInmate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            PrisonCell cell = data["Cell"];
            PrisonInmate inmate = data["Inmate"];
            
            if(cell != null) {
                cell.addPrisonInmate(inmate);

                using(var db = new ChoiceVDb()) {
                    var dbCell = db.configprisoncells.Find(cell.Id);
                    
                    if(dbCell != null) {
                        dbCell.inmateId = inmate?.Id;
                        db.SaveChanges();
                    }
                }
                if(inmate != null) {
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Die Zelle wurde erfolgreich {inmate.Name} zugewiesen.", "Zelle zugewiesen");
                } else {
                    player.sendNotification(Constants.NotifactionTypes.Success, "Die Zelle wurde erfolgreich zurückgenommen.", "Zelle zurückgenommen");
                }
            } else {
                player.sendBlockNotification("Ein Fehler ist aufgetreten. Melde dich im Support. Code: PRISONBEAR 4.", "Kein Insasse");
            }
            
            return true;
        }
        
        #endregion

        #region Support Stuff

        private static void flushPrisonToDb(Prison prison) {
            using(var db = new ChoiceVDb()) {
                var dbPrison = db.configprisons.Find(prison.Id);

                if(dbPrison != null) {
                    dbPrison.outlines = prison.getOutlineString();
                    dbPrison.registerSpots = prison.getRegisterSpotsString();
                    dbPrison.belongingsSpots = prison.getBelongingsSpotsString();
                    dbPrison.getOutPoints = prison.getGetOutPointsString();

                    foreach(var cell in prison.Cells ?? Enumerable.Empty<PrisonCell>()) {
                        var dbCell = dbPrison.configprisoncells.FirstOrDefault(c => c.id == cell.Id);

                        if(dbCell != null) {
                            dbCell.name = cell.Name;
                            dbCell.prisonId = prison.Id;
                            dbCell.collisionShape = cell.StorageSpot.toShortSave();
                            dbCell.inventoryId = cell.Inventory.Id;
                            dbCell.combination = cell.Combination.ToJson();
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        private Menu generateSupportPrisonMenu(IPlayer player) {
            var menu = new Menu("Gefängnisse erstellen", "Was möchtest du tun?");

            var createMenu = new Menu("Gefängnis erstellen", "Gib die Daten ein");
            createMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Gefängnisses", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das Gefängnis wie angegeben", "SUPPORT_CREATE_PRISON", MenuItemStyle.green)
                .needsConfirmation("Gefängnis wirklich erstellen?", "Gefängnis so erstellen?"));

            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            foreach(var prison in AllPrisons) {
                var virtMenu = new VirtualMenu(prison.Name, () => {
                    var prisonMenu = new Menu(prison.Name, "Was möchtest du tun?");

                    #region Outline

                    var outlineSpotMenu = new Menu("Outline", "Was möchtest du tun?");
                    outlineSpotMenu.addMenuItem(new ClickMenuItem("Neuen Spot erstellen", "Erstelle einen neuen Outlinepunkt", "", "SUPPORT_PRISON_CREATE_NEW_SPOT", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Type", "OUTLINE" } }));

                    foreach(var spot in prison.Outline) {
                        outlineSpotMenu.addMenuItem(new ClickMenuItem($"{spot.Position.ToJson()}", "Spot", "", "SUPPORT_PRISON_DELETE_SPOT", MenuItemStyle.red)
                            .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Spot", spot }, { "Type", "OUTLINE" } })
                            .needsConfirmation("Spot wirklich löschen?", "Spot so löschen?"));
                    }
                    prisonMenu.addMenuItem(new MenuMenuItem(outlineSpotMenu.Name, outlineSpotMenu));

                    #endregion

                    #region RegisterSpots

                    var registerSpotMenu = new Menu("Registrierungspunkte", "Was möchtest du tun?");
                    registerSpotMenu.addMenuItem(new ClickMenuItem("Neuen Spot erstellen", "Erstelle einen neuen Registrierungspunkt", "", "SUPPORT_PRISON_CREATE_NEW_SPOT", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Type", "REGISTER" } }));

                    foreach(var spot in prison.RegisterSpots) {
                        registerSpotMenu.addMenuItem(new ClickMenuItem($"{spot.Position.ToJson()}", "Spot", "", "SUPPORT_PRISON_DELETE_SPOT", MenuItemStyle.red)
                            .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Spot", spot }, { "Type", "REGISTER" } })
                            .needsConfirmation("Spot wirklich löschen?", "Spot so löschen?"));
                    }
                    prisonMenu.addMenuItem(new MenuMenuItem(registerSpotMenu.Name, registerSpotMenu));

                    #endregion

                    #region BelongingsSpots

                    var belongingsSpotMenu = new Menu("Besitztümerpunkte", "Was möchtest du tun?");
                    belongingsSpotMenu.addMenuItem(new ClickMenuItem("Neuen Spot erstellen", "Erstelle einen neuen Besitztümerpunkt", "", "SUPPORT_PRISON_CREATE_NEW_SPOT", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Type", "BELONGING" } }));

                    foreach(var spot in prison.BelongingsSpots) {
                        belongingsSpotMenu.addMenuItem(new ClickMenuItem($"{spot.Position.ToJson()}", "Spot", "", "SUPPORT_PRISON_DELETE_SPOT", MenuItemStyle.red)
                            .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Spot", spot }, { "Type", "BELONGING" } })
                            .needsConfirmation("Spot wirklich löschen?", "Spot so löschen?"));
                    }
                    prisonMenu.addMenuItem(new MenuMenuItem(belongingsSpotMenu.Name, belongingsSpotMenu));

                    #endregion

                    #region GetOutPoints

                    var getOutPointsMenu = new Menu("Ausgangspunkte", "Was möchtest du tun?");
                    var getOutPointCreateMenu = new Menu("Neuen Ausgangspunkt erstellen", "Erstelle einen neuen Ausgangspunk");
                    getOutPointCreateMenu.addMenuItem(new InputMenuItem("Nachricht", "Die Nachricht die nach dem Porten gesendet werden soll", "", ""));
                    getOutPointCreateMenu.addMenuItem(new CheckBoxMenuItem("Letzter Punkt", "Ist dies der letzte Punkt?", false, ""));

                    getOutPointCreateMenu.addMenuItem(new MenuStatsMenuItem("Neuen Spot erstellen", "Erstelle einen neuen Ausgangspunkt. DEINE AKTUELLE POSITION IST DIE PORTPOSITION!", "", "SUPPORT_MANAGE_GET_OUT_POINTS", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Action", "CREATE" } }));

                    getOutPointsMenu.addMenuItem(new MenuMenuItem(getOutPointCreateMenu.Name, getOutPointCreateMenu, MenuItemStyle.green));

                    foreach(var spot in prison.GetOutPoints) {
                        getOutPointsMenu.addMenuItem(new ClickMenuItem($"{spot.FromShape.Position.ToJson()}", "Spot", "", "SUPPORT_MANAGE_GET_OUT_POINTS", MenuItemStyle.red)
                            .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Spot", spot }, { "Action", "DELETE" } })
                            .needsConfirmation("Spot wirklich löschen?", "Spot so löschen?"));
                    }
                    prisonMenu.addMenuItem(new MenuMenuItem(getOutPointsMenu.Name, getOutPointsMenu));

                    #endregion

                    #region Cell

                    var cellSpotMenu = new Menu("Zellen hinzufügen", "Was möchtest du tun?");
                    cellSpotMenu.addMenuItem(new InputMenuItem("Name", "Der Name der Zelle", "", ""));
                    cellSpotMenu.addMenuItem(new MenuStatsMenuItem("Neue Zelle erstellen", "Erstelle eine neue Zelle", "", "SUPPORT_MANAGE_CELLS", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Action", "CREATE" } }));

                    foreach(var cell in prison.Cells) {
                        cellSpotMenu.addMenuItem(new ClickMenuItem(cell.Name, "Spot", "", "SUPPORT_MANAGE_CELLS", MenuItemStyle.red)
                            .withData(new Dictionary<string, dynamic> { { "Prison", prison }, { "Cell", cell }, { "Action", "DELETE" } })
                            .needsConfirmation("Spot wirklich löschen?", "Spot so löschen?"));
                    }
                    prisonMenu.addMenuItem(new MenuMenuItem(cellSpotMenu.Name, cellSpotMenu));

                    #endregion

                    if(player.getAdminLevel() >= 3) {
                        prisonMenu.addMenuItem(new ClickMenuItem("Gefängnis löschen", "Lösche das ausgewählte Gefängnis", "", "SUPPORT_DELETE_PRISON", MenuItemStyle.red)
                            .withData(new Dictionary<string, dynamic> { { "Prison", prison } })
                            .needsConfirmation("Gefängnis löschen?", "Gefängnis wirklich löschen?"));
                    }
                    return prisonMenu;
                });

                menu.addMenuItem(new MenuMenuItem(virtMenu.Name, virtMenu));
            }

            return menu;
        }

        private static bool onSuppotCreatePrison(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if (menuItemCefEvent is MenuStatsMenuItemEvent evt) {
                var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();

                using(var db = new ChoiceVDb()) {
                    var dbPrison = new configprison {
                        name = nameEvt.input,
                    };
                    db.configprisons.Add(dbPrison);
                    db.SaveChanges();

                    var prison = new Prison(dbPrison.id, nameEvt.input);
                    AllPrisons.Add(prison);

                    player.sendNotification(Constants.NotifactionTypes.Success, "Gefängnis erstellt", "Das Gefängnis wurde erfolgreich erstellt.");
                }
            }

            return true;
        }

        private static bool onSupportDeletePrison(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var prison = (Prison)data["Prison"];
            using(var db = new ChoiceVDb()) {
                var dbPrison = db.configprisons.Find(prison.Id);
                if(dbPrison != null) {
                    db.configprisons.Remove(dbPrison);
                    db.SaveChanges();
                }
            }

            AllPrisons.Remove(prison);
            prison.onDelete();

            player.sendNotification(Constants.NotifactionTypes.Warning, "Gefängnis gelöscht", "Das Gefängnis wurde erfolgreich gelöscht.");

            return true;
        }

        #region RegisterSpots

        private bool onSupportCreateSpot(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            Prison prison = (Prison)data["Prison"];
            string type = (string)data["Type"];

            player.sendNotification(Constants.NotifactionTypes.Info, "Erstellen nun den Collisionshape", "");
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                var colShape = CollisionShape.Create(p, w, h, r, true, false, true);

                switch(type) {
                    case "OUTLINE":
                        colShape.Interactable = false;
                        colShape.HasNoHeight = true;
                        prison.addOutline(colShape);
                        break;
                    case "REGISTER":
                        colShape.Interactable = false;
                        prison.addRegisterSpot(colShape);
                        break;
                    case "BELONGING":
                        prison.addBelongingSpot(colShape);
                        break;
                }
               
                flushPrisonToDb(prison);

                player.sendNotification(Constants.NotifactionTypes.Success, "Spot erstellt", "Der Spot wurde erfolgreich erstellt.");
            }, 10, 10, null, true);
            return true;
        }

        private bool onSupportDeleteSpot(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            Prison prison = (Prison)data["Prison"];
            CollisionShape spot = (CollisionShape)data["Spot"];
            string type = (string)data["Type"];

            switch(type) {
                case "OUTLINE":
                    prison.removeOutline(spot);
                    break;
                case "REGISTER":
                    prison.removeRegisterSpot(spot);
                    break;
                case "BELONGING":
                    prison.removeBelongingSpot(spot);
                    break;
            }

            flushPrisonToDb(prison);

            return true;
        }

        #endregion

        private static bool onSupportManageGetOutPoints(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var prison = (Prison)data["Prison"];
            var action = (string)data["Action"];

            if(action == "CREATE") {
                if (menuItemCefEvent is MenuStatsMenuItemEvent evt) {
                    var message = evt.elements[0].FromJson<InputMenuItemEvent>().input;
                    var isFinal = evt.elements[1].FromJson<CheckBoxMenuItemEvent>().check;

                    player.sendNotification(Constants.NotifactionTypes.Info, "Deine aktuelle Position wurde als Port punkt gesetzt. Setzte nun die Ausgangskollision", "");
                    var pos = player.Position;
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                        var colShape = CollisionShape.Create(p, w, h, r, true, false);

                        prison.addGetOutPoint(colShape, pos, message, isFinal);
                        flushPrisonToDb(prison);

                        player.sendNotification(Constants.NotifactionTypes.Success, "Ausgangspunkt erstellt", "Der Ausgangspunkt wurde erfolgreich erstellt.");
                    }, 10, 10, null, true);
                }
            } else if(action == "DELETE") {
                var spot = (PrisonGetOutPoint)data["Spot"];
                prison.removeGetOutPoint(spot);

                flushPrisonToDb(prison);

                player.sendNotification(Constants.NotifactionTypes.Warning, "Ausgangspunkt gelöscht", "Der Ausgangspunkt wurde erfolgreich gelöscht.");
            }

            return true;
        }

        private bool onSupportManageCells(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var prison = (Prison)data["Prison"];
            var action = (string)data["Action"];

            if(action == "CREATE") {
                if (menuitemcefevent is MenuStatsMenuItemEvent evt) {
                    var name = evt.elements[0].FromJson<InputMenuItemEvent>().input;
                
                    player.sendNotification(Constants.NotifactionTypes.Info, "Erstelle nun die die Kollisionsform für die Lagerung in der Zelle", "");
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (position, width, height, rotation) => {
                        var storageSpot = CollisionShape.Create(position, width, height, rotation, true, false, true);
                        var inventory = InventoryController.createInventory(prison.Id, 10, InventoryTypes.PrisonCell);
                        var combination = new List<int> { new Random().Next(0, 9), new Random().Next(0, 9), new Random().Next(0, 9) }.ToArray();

                        using(var db = new ChoiceVDb()) {
                            var newCell = new configprisoncell {
                                name = name,
                                prisonId = prison.Id,
                                collisionShape = storageSpot.toShortSave(),
                                inventoryId = inventory.Id,
                                combination = combination.ToJson(),
                            };
                        
                            db.configprisoncells.Add(newCell);
                            db.SaveChanges();
                        
                            var prisonCell = new PrisonCell(newCell.id, name, storageSpot, inventory, combination);
                    
                            prison.addCell(prisonCell);
                            flushPrisonToDb(prison);
                        }
                    
                        player.sendNotification(Constants.NotifactionTypes.Info, "Die Zelle wurde erfolgreich erstellt", "Zelle erstellt.");
                    });
                }
            } else if(action == "DELETE") {
                var cell = (PrisonCell)data["Cell"];
                prison.removeCell(cell);
                
                flushPrisonToDb(prison);
                player.sendNotification(Constants.NotifactionTypes.Warning, "Die Zelle wurde erfolgreich gelöscht", "Zelle gelöscht.");
            }

            return true;
        }

        private static List<MenuItem> prisonGuardNpcModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCPrisonGuardModule);
            
            return [new ListMenuItem("Gefängnis", "Wähle das Gefängnis aus", AllPrisons.Select(p => p.Name).ToArray(), "")];
        }

        private static void prisonGuardNpcModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var prison = AllPrisons.Find(p => p.Name == evt.elements[0].FromJson<ListMenuItemEvent>().currentElement);

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "PrisonId", prison.Id } });
        }
        
        private static List<MenuItem> prisonFoodNpcModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCPrisonFoodModule);
            
            return [new ListMenuItem("Gefängnis", "Wähle das Gefängnis aus", AllPrisons.Select(p => p.Name).ToArray(), "")];
        }

        private void prisonFoodNpcModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var prison = AllPrisons.Find(p => p.Name == evt.elements[0].FromJson<ListMenuItemEvent>().currentElement);

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "PrisonId", prison.Id } });
        }

        #endregion
    }
}
