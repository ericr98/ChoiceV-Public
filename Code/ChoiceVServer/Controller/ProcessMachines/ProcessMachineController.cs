using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.ProcessMachines.Model;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Controller.Voice;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.SoundSystem.SoundController;

namespace ChoiceVServer.Controller.ProcessMachines {
    public class ProcessMachineController : ChoiceVScript {
        private static TimeSpan PROCESS_MACHINE_INTERVAL = TimeSpan.FromSeconds(0.5);

        private static Dictionary<int, ProcessMachine> ProcessMachines;
        private static Dictionary<int, WorldProcessMachine> WorldProcessMachines;

        public ProcessMachineController() {
            ProcessMachines = new Dictionary<int, ProcessMachine>();
            WorldProcessMachines = new Dictionary<int, WorldProcessMachine>();
            InvokeController.AddTimedInvoke("ProcessMachine-Invoker", updateWorldProcessMachines, PROCESS_MACHINE_INTERVAL, true);

            EventController.addMenuEvent("PROCESS_MACHINE_CONFIG_CHANGE", onProcessMachineConfigChange);
            EventController.addMenuEvent("PROCESS_START_STOP", onProcessStartStop);
            EventController.addMenuEvent("OPEN_PROCESS_INVENTORY", onOpenProcessInventory);

            loadFromDb();
            
            #region Support Stuff

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Registrieren,
                    "Prozessmaschinen",
                    generateSupportMenu
                )
            );

            EventController.addMenuEvent("SUPPORT_PROCESS_MACHINE_PLACE", onPlaceProcessMachine);
            EventController.addMenuEvent("SUPPORT_PROCESS_MACHINE_OPEN", onOpenProcessMachine);
            EventController.addMenuEvent("SUPPORT_PROCESS_MACHINE_TELEPORT", onTeleportToMachine);
            EventController.addMenuEvent("SUPPORT_PROCESS_MACHINE_DELETE", onDeleteMachine);

            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_EXAMPLE", onCreateExampleProcessMachine);
            
            EventController.addMenuEvent("PROCESS_MACHINE_CREATE", onCreateProcessMachine);
            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_VARIABLE", onCreateProcessMachineVariable);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_VARIABLE", onChangeProcessMachineVariable);

            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_INVENTORY", onCreateProcessMachineInventory);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_INVENTORY", onChangeProcessMachineInventory);

            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_DISPLAY", onCreateProcessMachineDisplay);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_DISPLAY", onChangeProcessMachineDisplay);
            
            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_CONFIG_POINT", onCreateProcessMachineConfigPoint);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_CONFIG_POINT", onChangeProcessMachineConfigPoint);
            
            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_PROCESS_STEP", onCreateProcessMachineProcessStep);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_PROCESS_STEP", onChangeProcessMachineProcessStep);
            
            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_TRANSFORMATION", onCreateProcessMachineTransformation);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_TRANSFORMATION", onChangeProcessMachineTransformation);
            
            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_TRANSFORMER", onCreateProcessMachineTransformer);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_TRANSFORMER", onChangeProcessMachineTransformer);
            
            EventController.addMenuEvent("PROCESS_MACHINE_CREATE_SOUND", onCreateProcessMachineSound);
            EventController.addMenuEvent("PROCESS_MACHINE_CHANGE_SOUND", onChangeProcessMachineSound);

            EventController.addMenuEvent("PROCESS_MACHINE_DELETE_ELEMENT", onDeleteProcessMachineElement);
            
            #endregion
        }

        private void loadFromDb() {
            using(var db = new ChoiceVDb()) {
                foreach(var dbMachine in db.configprocessmachines) {
                    var variables = dbMachine.variables.FromJson<Dictionary<string, StateValue>>();
                    var displays = loadListFromDbString<ProcessDisplay>(dbMachine.displays);
                    var processSteps = loadListFromDbString<ProcessStep>(dbMachine.steps);
                    var configPoints = loadListFromDbString<ProcessConfigPoint>(dbMachine.configPoints);
                    var inventories = loadListFromDbString<ProcessInventory>(dbMachine.inventories);
                    var processTransformers = loadListFromDbString<ProcessTransformer>(dbMachine.transformers);
                    var processSounds = loadListFromDbString<ProcessSound>(dbMachine.sounds);

                    var processMachine = new ProcessMachine(dbMachine.id, dbMachine.name, variables, displays, processSteps, configPoints, inventories, processTransformers, processSounds);

                    ProcessMachines.Add(dbMachine.id, processMachine);

                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"Loaded ProcessMachine {processMachine.Name}");
                }

                
                Logger.logInfo(LogCategory.ServerStartup, LogActionType.Created, $"Loaded {ProcessMachines.Count} ProcessMachines");    

                foreach(var dbWorldMachine in db.worldprocessmachines.Include(m => m.worldprocessmachinesdata).Include(m => m.worldprocessmachinesinventories)) {
                    var worldProcessMachine = new WorldProcessMachine(dbWorldMachine);
                    WorldProcessMachines.Add(worldProcessMachine.Id, worldProcessMachine);

                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"Loaded WorldProcessMachine {worldProcessMachine.Id}");
                }
    
                Logger.logInfo(LogCategory.ServerStartup, LogActionType.Created, $"Loaded {WorldProcessMachines.Count} WorldProcessMachines");
            }
            
        }

        private static List<T> loadListFromDbString<T>(string dbString) {
            var list = new List<T>();
            try {
                if(dbString == "") return list;

                var splits = dbString.Split("~~~");

                foreach(var tString in splits) {
                    var type = Type.GetType(tString.Split("###")[0]);

                    // remove the type information and the 3 ### from the beginning
                    var t = (T)Activator.CreateInstance(type, tString.Remove(0, type.FullName.Length + 3));

                    list.Add(t);
                }
            } catch(Exception e) {
                Logger.logException(e, $"Fehler beim laden von ProcessMaschine mit string {dbString}");
            }
            
            return list;
        }
        
        public WorldProcessMachine createWorldProcessMachine(ProcessMachine machine, WorldProcessMachineType type, CollisionShape shape, CollisionShape loadingZone) {
            using(var db = new ChoiceVDb()) {
                var processMachine = new WorldProcessMachine(-1, type, machine, shape, loadingZone);

                var dbRep = processMachine.getDbRepresentation();
                var entry = db.worldprocessmachines.Add(dbRep);

                db.SaveChanges();

                processMachine.Id = entry.Entity.id;

                WorldProcessMachines.Add(processMachine.Id, processMachine);

                return processMachine;
            }
        }

        private void updateWorldProcessMachines(IInvoke obj) {
            foreach(var worldProcessMachine in WorldProcessMachines.Values) {
                worldProcessMachine.onInterval();
            }
        }

        public static ProcessMachine getProcessMachine(int id) {
            return ProcessMachines[id];
        }

        public static void updateWorldProcessMachineDbValue(WorldProcessMachine machine, string key, object value) {
            using(var db = new ChoiceVDb()) {
                var entry = db.worldprocessmachinesdata.Find(machine.Id, key);

                if(entry != null) {
                    entry.value = value.ToString();

                    db.SaveChanges();
                } else {
                    Logger.logError($"Could not find entry for {machine.Id} and {key}");
                }
            }
        }

        private bool onProcessMachineConfigChange(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var processConfigPoint = data["CONFIG_POINT"] as ProcessConfigPoint;
            var worldProcessMachine = data["WORLD_PROCESS_MACHINE"] as WorldProcessMachine;

            processConfigPoint.onChangeConfig(player, ref worldProcessMachine.VirtualState, menuitemcefevent, ref worldProcessMachine.ErrorMessages);

            return true;
        }

        private bool onProcessStartStop(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var worldProcessMachine = data["Machine"] as WorldProcessMachine;

            if(data.ContainsKey("Confirmed")) {
                if(!worldProcessMachine.IsRunning) {
                    worldProcessMachine.start(player);
                } else {
                    worldProcessMachine.stop(player);
                }
                
                worldProcessMachine.onInteraction(player);
            } else {            
                var evt = menuitemcefevent as ListMenuItem.ListMenuItemEvent;
                var startStop = evt.currentElement == "An";
            
                if(worldProcessMachine.IsRunning == startStop) {
                    return true;
                }
                
                var str = worldProcessMachine.IsRunning ? "AUSSCHALTEN" : "ANSCHALTEN";
                data["Confirmed"] = true;
                player.showMenu(MenuController.getConfirmationMenu($"Maschine {str}?", $"Maschine wirklich {str}", "PROCESS_START_STOP", data));
            }
            
            return true;
        }

        private bool onOpenProcessInventory(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var identifier = data["Identifier"] as string;
            var worldProcessMachine = data["ProcessMachine"] as WorldProcessMachine;

            worldProcessMachine.openInventory(player, identifier);

            return true;
        }

        #region Support Stuff

        private Menu generateSupportMenu(IPlayer player) {
            var menu = new Menu("Prozessmaschinen", "Was möchtest du tun?");
            
            //Place process machine
            var placeProcessMachineMenu = new Menu("Prozessmaschine platzieren", "Was möchtest du tun?");
          
            var worldProcessMachineMenu = new Menu("Welt-Prozessmaschinen erstellen", "Welche Welt-Prozessmaschine platzieren?");
            
           worldProcessMachineMenu.addMenuItem(new InputMenuItem("Prozessmaschine", "Welche Prozessmaschine möchtest du platzieren?", "", "").withOptions(ProcessMachines.Values.Select(m => m.Name).ToArray()));
           worldProcessMachineMenu.addMenuItem(new CheckBoxMenuItem("Fahrzeugladezone", "Soll eine Ladezone miterstellt werden?", false, ""));
           worldProcessMachineMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die Maschine", "SUPPORT_PROCESS_MACHINE_PLACE", MenuItemStyle.green));
           placeProcessMachineMenu.addMenuItem(new MenuMenuItem(worldProcessMachineMenu.Name, worldProcessMachineMenu));
         
            foreach(var machine in WorldProcessMachines.Values) {
                var virtMaschineMenu = new VirtualMenu($"{machine.Id} {machine.Name}", () => {
                    return getWorldProcessMachineMenu(player, machine);
                });

                placeProcessMachineMenu.addMenuItem(new MenuMenuItem($"{machine.Id} {machine.Name}", virtMaschineMenu));
            }

            menu.addMenuItem(new MenuMenuItem(placeProcessMachineMenu.Name, placeProcessMachineMenu));

            // Create Process Machine Menu
            var processMachineMenu = new Menu("Prozessmaschinen-Konfiguration", "Was möchtest du tun?");

            var processMachineCreateMenu = new Menu("Prozessmaschine erstellen", "Was möchtest du tun?");
            processMachineCreateMenu.addMenuItem(new InputMenuItem("Name", "Name", "Name der Prozessmaschine", "Prozessmaschine"));
            processMachineCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die Prozessmaschine", "", "PROCESS_MACHINE_CREATE", MenuItemStyle.green));
            processMachineMenu.addMenuItem(new MenuMenuItem(processMachineCreateMenu.Name, processMachineCreateMenu));

            foreach(var machine in ProcessMachines.Values) {
                var virtMaschineMenu = new VirtualMenu(machine.Name, () => {
                    return getProcessMaschineMenu(player, machine);
                });

                processMachineMenu.addMenuItem(new MenuMenuItem(machine.Name, virtMaschineMenu));
            }

            menu.addMenuItem(new MenuMenuItem(processMachineMenu.Name, processMachineMenu));

            return menu;
        }

        private static Menu getWorldProcessMachineMenu(IPlayer player, WorldProcessMachine machine) {
            var machineMenu = new Menu(machine.Id.ToString(), "Was möchtest du tun?");

            machineMenu.addMenuItem(new ClickMenuItem("Öffnen", "Öffne die Maschine", "", "SUPPORT_PROCESS_MACHINE_OPEN", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));

            machineMenu.addMenuItem(new ClickMenuItem("Zu Maschine teleportieren", "Teleportiere dich zur Maschine", "", "SUPPORT_PROCESS_MACHINE_TELEPORT", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));

            if(machine.Type == WorldProcessMachineType.Static) {
                machineMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Maschine", "", "SUPPORT_PROCESS_MACHINE_DELETE", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine } })
                    .needsConfirmation($"{machine.Id} löschen?", "Maschine wirklich löschen?"));
            }

            return machineMenu;
        }

        public static void flushToDb(ProcessMachine machine) {
            using(var db = new ChoiceVDb()) {
                var dbProcessMachine = db.configprocessmachines.Find(machine.Id);

                if(dbProcessMachine != null) {
                    dbProcessMachine.name = machine.Name;
                    dbProcessMachine.displays = string.Join("~~~", machine.Displays.Select(display => display.getDbRepresentation()));
                    dbProcessMachine.steps = string.Join("~~~", machine.ProcessSteps.Select(step => step.getDbRepresentation()));
                    dbProcessMachine.configPoints = string.Join("~~~", machine.ConfigPoints.Select(configPoint => configPoint.getDbRepresentation()));
                    dbProcessMachine.inventories = string.Join("~~~", machine.Inventories.Select(inventory => inventory.getDbRepresentation()));
                    dbProcessMachine.transformers = string.Join("~~~", machine.ProcessTransformers.Select(transformer => transformer.getDbRepresentation()));
                    dbProcessMachine.variables = machine.VariableStandardValues.ToJson(); 
                    dbProcessMachine.sounds = string.Join("~~~", machine.ProcessSounds.Select(sound => sound.getDbRepresentation()));
                } else {
                    db.configprocessmachines.Add(new configprocessmachine {
                        id = machine.Id,
                        name = machine.Name,
                        displays = string.Join("~~~", machine.Displays.Select(display => display.getDbRepresentation())),
                        steps = string.Join("~~~", machine.ProcessSteps.Select(step => step.getDbRepresentation())),
                        configPoints = string.Join("~~~", machine.ConfigPoints.Select(configPoint => configPoint.getDbRepresentation())),
                        inventories = string.Join("~~~", machine.Inventories.Select(inventory => inventory.getDbRepresentation())),
                        transformers = string.Join("~~~", machine.ProcessTransformers.Select(transformer => transformer.getDbRepresentation())),
                        variables = machine.VariableStandardValues.ToJson(), 
                        sounds = string.Join("~~~", machine.ProcessSounds.Select(sound => sound.getDbRepresentation()))
                    });
                }
                db.SaveChanges();
            }
        }

        #region Process Machine Creation

        private bool onPlaceProcessMachine(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machineNameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var createZoneEvt = evt.elements[1].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
            
            var machine = ProcessMachines.Values.FirstOrDefault(m => m.Name == machineNameEvt.input);

            player.sendNotification(Constants.NotifactionTypes.Info, "Platziere die Maschine an der gewünschten Position", "");
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                var colShape = CollisionShape.Create(p, w, h, r, true, false, true);

                if(createZoneEvt.check) {
                    player.sendNotification(Constants.NotifactionTypes.Info, "Platziere die Ladezone an der gewünschten Position", "");
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p2, w2, h2, r2) => {
                        var loadingZone = CollisionShape.Create(p2, w2, h2, r2, false, true, false);
                        var worldMachine = createWorldProcessMachine(machine, WorldProcessMachineType.Static, colShape, loadingZone);
 
                        player.sendNotification(Constants.NotifactionTypes.Success, "Maschine platziert", "");
                        Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"Created WorldProcessMachine {worldMachine.Id}");                       
                    });
                } else {
                    var worldMachine = createWorldProcessMachine(machine, WorldProcessMachineType.Static, colShape, null);
                    
                    player.sendNotification(Constants.NotifactionTypes.Success, "Maschine platziert", "");
                    Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"Created WorldProcessMachine {worldMachine.Id}");
                }
            });

            return true;
        }

        private bool onOpenProcessMachine(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var machine = data["Machine"] as WorldProcessMachine;

            machine.onInteraction(player);
            Logger.logDebug(LogCategory.Support, LogActionType.Event, player, $"Opened WorldProcessMachine {machine.Id}");
            return true;
        }

        private bool onTeleportToMachine(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var machine = data["Machine"] as WorldProcessMachine;

            player.Position = machine.Position;
            Logger.logDebug(LogCategory.Support, LogActionType.Event, player, $"Teleported to WorldProcessMachine {machine.Id}");

            return true;
        }

        private bool onDeleteMachine(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var machine = data["Machine"] as WorldProcessMachine;

            machine.Dispose();

            using(var db = new ChoiceVDb()) {
                var dbMachine = db.worldprocessmachines.Find(machine.Id);
                db.worldprocessmachines.Remove(dbMachine);
                db.SaveChanges();
            }

            WorldProcessMachines.Remove(machine.Id);
            player.sendNotification(Constants.NotifactionTypes.Warning, "Maschine gelöscht", "");
            Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"Deleted WorldProcessMachine {machine.Id}");
            return true;
        }

        #endregion

        private static Menu getProcessMaschineMenu(IPlayer player, ProcessMachine machine) {
            SupportController.setCurrentSupportFastAction(player, () => player.showMenu(getProcessMaschineMenu(player, machine)));

            var machineMenu = new Menu(machine.Name, "Was möchtest du tun?");
            machineMenu.addMenuItem(new ClickMenuItem("Beispielmaschine erstellen", "Erstelle diese Maschine an der aktuellen Position. Die wird entfernt, wenn du eine neue Maschine erstellst", "", "PROCESS_MACHINE_CREATE_EXAMPLE", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));
            
            #region Variables

            //Variables
            var variablesMenu = new Menu("Variablen", "Was möchtest du tun?");

            //Create Variables Menu
            var variablesCreateMenu = new Menu("Variable erstellen", "Was möchtest du tun?");
            variablesCreateMenu.addMenuItem(new InputMenuItem("Name", "Der Name der Variable. NUR Buchstaben verwenden und KEINE Leerzeichen oder andere Sonderzeichen", "", ""));
            variablesCreateMenu.addMenuItem(new ListMenuItem("Typ", "Der Typ der Variable", new string[] { "Zahl", "Text" }, ""));
            variablesCreateMenu.addMenuItem(new InputMenuItem("Wert", "Standardwert der Variable", "", ""));
            variablesCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstellen", "", "PROCESS_MACHINE_CREATE_VARIABLE", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));
            variablesMenu.addMenuItem(new MenuMenuItem(variablesCreateMenu.Name, variablesCreateMenu));

            //Existing Variables Menu
            foreach(var variable in machine.VariableStandardValues) {
                var variableMenu = new Menu(variable.Key, "Was möchtest du tun?");
                variableMenu.addMenuItem(new InputMenuItem(variable.Key, "Ändere den Standardwert der Variable", "", "PROCESS_MACHINE_CHANGE_VARIABLE").withStartValue(variable.Value.ToString())
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"VariableName", variable.Key}, { "VariableValue", variable.Value} })
                    .needsConfirmation($"{variable.Key} ändern?", "Variable wirklich ändern?"));
                
                variableMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Variable", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "VARIABLE"}, { "Variable", variable.Key } })
                    .needsConfirmation($"{variable.Key} löschen?", "Variable wirklich löschen?"));
                
                variablesMenu.addMenuItem(new MenuMenuItem(variableMenu.Name, variableMenu));
            }

            machineMenu.addMenuItem(new MenuMenuItem(variablesMenu.Name, variablesMenu));

            #endregion

            #region Inventories

            var inventoryMenu = new Menu("Inventare", "Was möchtest du tun?");
            var inventoryCreateMenu = new Menu("Inventar erstellen", "Was möchtest du tun?");
            inventoryCreateMenu.addMenuItem(new InputMenuItem("Identifier", "Der Identifier des Inventars", "", ""));
            inventoryCreateMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Inventars", "", ""));
            inventoryCreateMenu.addMenuItem(new InputMenuItem("Beschreibung", "Die Beschreibung des Inventars", "", ""));
            inventoryCreateMenu.addMenuItem(new InputMenuItem("Bedingung", "Die Bedingung, die erfüllt sein muss, damit das Inventar geöffnet werden kann", "", ""));
            inventoryCreateMenu.addMenuItem(new InputMenuItem("Standardgröße", "Die Standardgröße des Inventars", "", InputMenuItemTypes.number, ""));
            inventoryCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstellen", "", "PROCESS_MACHINE_CREATE_INVENTORY", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));

            inventoryMenu.addMenuItem(new MenuMenuItem(inventoryCreateMenu.Name, inventoryCreateMenu));

            foreach(var inventory in machine.Inventories) {
                var changeInventoryMenu = new Menu(inventory.Identifier, "Was möchtest du tun?");
                changeInventoryMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Inventars", "", "").withStartValue(inventory.Name));
                changeInventoryMenu.addMenuItem(new InputMenuItem("Beschreibung", "Die Beschreibung des Inventars", "", "").withStartValue(inventory.Description));
                changeInventoryMenu.addMenuItem(new InputMenuItem("Bedingung", "Die Bedingung, die erfüllt sein muss, damit das Inventar geöffnet werden kann", "", "").withStartValue(inventory.ConditionFormula));
                changeInventoryMenu.addMenuItem(new InputMenuItem("Standardgröße", "Die Standardgröße des Inventars", "", InputMenuItemTypes.number, "").withStartValue(inventory.StandardSize.ToString()));
                changeInventoryMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändern", "", "PROCESS_MACHINE_CHANGE_INVENTORY", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "Inventory", inventory } }));

                changeInventoryMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche das Inventar", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "INVENTORY"}, { "Inventory", inventory.Identifier } })
                    .needsConfirmation($"{inventory.Identifier} löschen?", "Inventar wirklich löschen?"));
                
                inventoryMenu.addMenuItem(new MenuMenuItem(changeInventoryMenu.Name, changeInventoryMenu));
            }

            machineMenu.addMenuItem(new MenuMenuItem(inventoryMenu.Name, inventoryMenu));

            #endregion
            
            #region Displays
            
            var displaysMenu = new Menu("Anzeigen", "Was möchtest du tun?");
            var displayCreateMenu = new Menu("Anzeige erstellen", "Was möchtest du tun?");
            displayCreateMenu.addMenuItem(new InputMenuItem("Name", "Der Name der Anzeige", "", ""));
            displayCreateMenu.addMenuItem(new InputMenuItem("Beschreibung", "Die Beschreibung der Anzeige", "", ""));
            displayCreateMenu.addMenuItem(new InputMenuItem("Einheit", "Die Einheit der Anzeige", "", ""));
            displayCreateMenu.addMenuItem(new InputMenuItem("Darstellung", "Die Darstellung der Anzeige", "", ""));
            displayCreateMenu.addMenuItem(new MenuStatsMenuItem(displayCreateMenu.Name, "Erstellen", "", "PROCESS_MACHINE_CREATE_DISPLAY", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));
            displaysMenu.addMenuItem(new MenuMenuItem(displayCreateMenu.Name, displayCreateMenu));
            
            foreach(var display in machine.Displays) {
                var changeDisplayMenu = new Menu(display.Name, "Was möchtest du tun?");
                changeDisplayMenu.addMenuItem(new InputMenuItem("Name", "Der Name der Anzeige", "", "").withStartValue(display.Name));
                changeDisplayMenu.addMenuItem(new InputMenuItem("Beschreibung", "Die Beschreibung der Anzeige", "", "").withStartValue(display.Description));
                changeDisplayMenu.addMenuItem(new InputMenuItem("Einheit", "Die Einheit der Anzeige", "", "").withStartValue(display.Unit));
                changeDisplayMenu.addMenuItem(new InputMenuItem("Darstellung", "Die Darstellung der Anzeige", "", "").withStartValue(display.RepresentationFormula));
                changeDisplayMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändern", "", "PROCESS_MACHINE_CHANGE_DISPLAY", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "Display", display } }));
                
                changeDisplayMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Anzeige", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "DISPLAY"}, { "Display", display } })
                    .needsConfirmation($"{display.Name} löschen?", "Anzeige wirklich löschen?"));
                
                displaysMenu.addMenuItem(new MenuMenuItem(changeDisplayMenu.Name, changeDisplayMenu));
            }

            machineMenu.addMenuItem(new MenuMenuItem(displaysMenu.Name, displaysMenu));
            
            #endregion
            
            #region Config Points
            
            var configPointsMenu = new Menu("Konfigurationspunkte", "Was möchtest du tun?");
            var createPointMenu = new Menu("Konfigurationspunkt erstellen", "Welchen Typ möchtest du erstellen?");

            var variations = new List<string> { "Map", "Range", "Text" };
            foreach(var variation in variations) {
                var configPointCreateMenu = new Menu($"{variation}-Konfigurationspunkt erstellen", "Was möchtest du tun?");
                configPointCreateMenu.addMenuItem(new InputMenuItem("Zielvariable", "Die Variable, die verändert werden soll", "", "")
                    .withOptions(machine.VariableStandardValues.Keys.ToArray()));
                configPointCreateMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Konfigurationspunkts", "", ""));
                configPointCreateMenu.addMenuItem(new InputMenuItem("Beschreibung", "Die Beschreibung des Konfigurationspunkts", "", ""));
                configPointCreateMenu.addMenuItem(new InputMenuItem("Einheit", "Die Einheit des Konfigurationspunkts", "", ""));
                configPointCreateMenu.addMenuItem(new InputMenuItem("Blockbedingung", "Die Bedingung, die erfüllt sein muss, damit der Konfigurationspunkt geändert werden kann", "", ""));

                if(variation == "Range") {
                    configPointCreateMenu.addMenuItem(new InputMenuItem("Werteliste", "Die Liste an Werten getrennt durch , und OHNE Leerzeichen zwischen den Werten", "", ""));
                } else if (variation == "Map") {
                    configPointCreateMenu.addMenuItem(new InputMenuItem("Map-Schlüssel", "Die Zuordnung der Werte. Die Schlüssel. Getrennt durch , und OHNE Leerzeichen zwischen den Leerzeichen", "", ""));
                    configPointCreateMenu.addMenuItem(new InputMenuItem("Map-Werte", "Die Zuordnung der Werte. Die Werte. Getrennt durch , und OHNE Leerzeichen zwischen den Leerzeichen", "", ""));
                } else if(variation == "Text") {
                    //No more information required
                }
                
                configPointCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Konfigurationspunkt", "", "PROCESS_MACHINE_CREATE_CONFIG_POINT", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "Variation", variation } }));
                
                createPointMenu.addMenuItem(new MenuMenuItem(configPointCreateMenu.Name, configPointCreateMenu));
            }
            
            configPointsMenu.addMenuItem(new MenuMenuItem(createPointMenu.Name, createPointMenu));
            
            foreach(var configPoint in machine.ConfigPoints) {
                var configPointsChangeMenu = new Menu(configPoint.Name, "Was möchtest du tun?");
                
                configPointsChangeMenu.addMenuItem(new InputMenuItem("Zielvariable", "Die Variable, die verändert werden soll", "", "").withOptions(machine.VariableStandardValues.Keys.ToArray()).withStartValue(configPoint.TargetVariableName));
                configPointsChangeMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Konfigurationspunkts", "", "").withStartValue(configPoint.Name));
                configPointsChangeMenu.addMenuItem(new InputMenuItem("Beschreibung", "Die Beschreibung des Konfigurationspunkts", "", "").withStartValue(configPoint.Description));
                configPointsChangeMenu.addMenuItem(new InputMenuItem("Einheit", "Die Einheit des Konfigurationspunkts", "", "").withStartValue(configPoint.Unit));
                configPointsChangeMenu.addMenuItem(new InputMenuItem("Blockbedingung", "Die Bedingung, die erfüllt sein muss, damit der Konfigurationspunkt geändert werden kann", "", "").withStartValue(configPoint.BlockCondition));
                
                if(configPoint is ProcessRangeConfigPoint rangeConfigPoint) {
                    configPointsChangeMenu.addMenuItem(new InputMenuItem("Werteliste", "Die Liste an Werten getrennt durch , und OHNE Leerzeichen zwischen den Werten", "", "").withStartValue(string.Join(",", rangeConfigPoint.Values.Select(v => v.Replace(rangeConfigPoint.Unit, "").ToArray()))));
                } else if (configPoint is ProcessMapConfigPoint mapConfigPoint) {
                    var keys = string.Join(",", mapConfigPoint.Values.Select(v => v.from));
                    var values = string.Join(",", mapConfigPoint.Values.Select(v => v.to));
                    configPointsChangeMenu.addMenuItem(new InputMenuItem("Map-Schlüssel", "Die Zuordnung der Werte. Die Schlüssel. Getrennt durch , und OHNE Leerzeichen zwischen den Leerzeichen", "", "").withStartValue(keys));
                    configPointsChangeMenu.addMenuItem(new InputMenuItem("Map-Werte", "Die Zuordnung der Werte. Die Werte. Getrennt durch , und OHNE Leerzeichen zwischen den Leerzeichen", "", "").withStartValue(values));
                }
                
                configPointsChangeMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändere den Konfigurationspunkt", "", "PROCESS_MACHINE_CHANGE_CONFIG_POINT", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "ConfigPoint", configPoint } }));
                
                configPointsChangeMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Anzeige", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "CONFIG_POINT"}, { "ConfigPoint", configPoint } })
                    .needsConfirmation($"{configPoint.Name} löschen?", "Anzeige wirklich löschen?"));
                
                
                configPointsMenu.addMenuItem(new MenuMenuItem(configPointsChangeMenu.Name, configPointsChangeMenu));   
            }
            machineMenu.addMenuItem(new MenuMenuItem(configPointsMenu.Name, configPointsMenu));
            
            #endregion

            #region Process Steps
            
            var processStepsMenu = new Menu("Prozessschritte", "Was möchtest du tun?");
            var processStepCreateMenu = new Menu("Prozessschritt erstellen", "Was möchtest du tun?");
            processStepCreateMenu.addMenuItem(new InputMenuItem("Dauer in Sekunden", "Die Dauer des Prozessschritts in Sekunden", "", InputMenuItemTypes.number, ""));
            processStepCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Prozessschritt", "", "PROCESS_MACHINE_CREATE_PROCESS_STEP", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));
            processStepsMenu.addMenuItem(new MenuMenuItem(processStepCreateMenu.Name, processStepCreateMenu));

            foreach(var processStep in machine.ProcessSteps) {
                var processStepChangeMenu = new Menu(processStep.Id.ToString(), "Was möchtest du tun?");
                
                var transformationMenu = new Menu("Transformationen", "Was möchtest du tun?");
                var transformationCreateMenu = new Menu("Transformation erstellen", "Was möchtest du tun?");
                transformationCreateMenu.addMenuItem(new InputMenuItem("Zielvariable", "Die Variable, die verändert werden soll", "", "").withOptions(machine.VariableStandardValues.Keys.ToArray()));
                transformationCreateMenu.addMenuItem(new InputMenuItem("Condition", "Die Bedingung, die erfüllt sein muss, damit die Transformation ausgeführt wird", "", ""));
                transformationCreateMenu.addMenuItem(new InputMenuItem("Transformation", "Die Transformation, die ausgeführt werden soll", "", ""));
                transformationCreateMenu.addMenuItem(new CheckBoxMenuItem("Auch durchführen, wenn Maschine aus", "Soll die Transformation auch ausgeführt werden, wenn die Maschine deaktiviert ist?", false, ""));
                transformationCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die Transformation", "", "PROCESS_MACHINE_CREATE_TRANSFORMATION", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "ProcessStep", processStep } }));
                transformationMenu.addMenuItem(new MenuMenuItem(transformationCreateMenu.Name, transformationCreateMenu));

                foreach(var transformation in processStep.Transformations) {
                    var transformationChangeMenu = new Menu($"Zu {transformation.OutputVariabe}", "Was möchtest du tun?");
                    transformationChangeMenu.addMenuItem(new InputMenuItem("Zielvariable", "Die Variable, die verändert werden soll", "", "").withOptions(machine.VariableStandardValues.Keys.ToArray()).withStartValue(transformation.OutputVariabe));
                    transformationChangeMenu.addMenuItem(new InputMenuItem("Condition", "Die Bedingung, die erfüllt sein muss, damit die Transformation ausgeführt wird", "", "").withStartValue(transformation.ConditionFormula));
                    transformationChangeMenu.addMenuItem(new InputMenuItem("Transformation", "Die Transformation, die ausgeführt werden soll", "", "").withStartValue(transformation.TransformationFormula));
                    transformationChangeMenu.addMenuItem(new CheckBoxMenuItem("Auch durchführen, wenn Maschine aus", "Soll die Transformation auch ausgeführt werden, wenn die Maschine deaktiviert ist?", transformation.DoIfMachineOff, ""));
                    transformationChangeMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändere die Transformation", "", "PROCESS_MACHINE_CHANGE_TRANSFORMATION", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "ProcessStep", processStep }, { "Transformation", transformation } }));
                    
                    transformationChangeMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Transformation", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                        .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "TRANSFORMATION"}, {"ProcessStep", processStep }, { "Transformation", transformation } })
                        .needsConfirmation("Transformation löschen?", "Transformation wirklich löschen?"));
                    
                    transformationMenu.addMenuItem(new MenuMenuItem(transformationChangeMenu.Name, transformationChangeMenu));
                }
                
                processStepChangeMenu.addMenuItem(new MenuMenuItem(transformationMenu.Name, transformationMenu));
                
                processStepChangeMenu.addMenuItem(new InputMenuItem("Dauer in Sekunden", "Die Dauer des Prozessschritts in Sekunden", "", InputMenuItemTypes.number, "").withStartValue(processStep.DurationSeconds.ToString()));
                processStepChangeMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändere den Prozessschritt", "", "PROCESS_MACHINE_CHANGE_PROCESS_STEP", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "ProcessStep", processStep } }));

                processStepChangeMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche den Prozessschritt", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "PROCESS_STEP"}, { "ProcessStep", processStep } })
                    .needsConfirmation("Prozessschritt löschen?", "Prozessschritt wirklich löschen?"));
                
                processStepsMenu.addMenuItem(new MenuMenuItem(processStepChangeMenu.Name, processStepChangeMenu));
            }
            
            machineMenu.addMenuItem(new MenuMenuItem(processStepsMenu.Name, processStepsMenu));
            
            #endregion
            
            #region Transformer
            
            var transformerMenu = new Menu("Transformationen", "Was möchtest du tun?");
            
            var transformerCreateMenu = new Menu("Transformation erstellen", "Was möchtest du tun?");

            foreach(var variation in new List<string> { "Inventory", "Reset-Variable" }) {
                var transformerVariationCreateMenu = new Menu($"{variation}-Transformation erstellen", "Was möchtest du tun?");
                transformerVariationCreateMenu.addMenuItem(new InputMenuItem("Zielvariable", "Die Variable, die verändert werden soll", "", "").withOptions(machine.VariableStandardValues.Keys.ToArray()));
                transformerVariationCreateMenu.addMenuItem(new CheckBoxMenuItem("Bei Interval updaten", "Soll die Transformation bei jedem Interval ausgeführt werden?", false, ""));

                if(variation == "Inventory") {
                    transformerVariationCreateMenu.addMenuItem(new InputMenuItem("Inventar", "Das Inventar, das verändert werden soll", "", "").withOptions(machine.Inventories.Select(inventory => inventory.Identifier).ToArray()));
                    transformerVariationCreateMenu.addMenuItem(new InputMenuItem("Item", "Das Item welches transfomiert wird", "", "").withOptions(InventoryController.getAllConfigItems().Select(item => $"{item.configItemId} {item.name}").ToArray()));
                    transformerVariationCreateMenu.addMenuItem(new InputMenuItem("Variablen", "Die Variablen die bei der Itemerstellung übermittelt werden sollen, durch Komma getrennt.", "", ""));
                } else if(variation == "Reset-Variable") {
                    transformerVariationCreateMenu.addMenuItem(new InputMenuItem("Wert", "Der Wert, auf den die Variable gesetzt werden soll", "", InputMenuItemTypes.number, ""));
                }
                
                transformerVariationCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die Transformation", "", "PROCESS_MACHINE_CREATE_TRANSFORMER", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "Variation", variation } }));
                transformerCreateMenu.addMenuItem(new MenuMenuItem(transformerVariationCreateMenu.Name, transformerVariationCreateMenu));
            }
            transformerMenu.addMenuItem(new MenuMenuItem(transformerCreateMenu.Name, transformerCreateMenu));

            foreach(var transformer in machine.ProcessTransformers) {
                var transformerChangeMenu = new Menu($"Zu {transformer.VariableIdentifier}", "Was möchtest du tun?");

                transformerChangeMenu.addMenuItem(new InputMenuItem("Zielvariable", "Die Variable, die verändert werden soll", "", "").withOptions(machine.VariableStandardValues.Keys.ToArray()).withStartValue(transformer.VariableIdentifier));

                if(transformer is ProcessInventoryTransformer inventoryTransformer) {
                    transformerChangeMenu.addMenuItem(new InputMenuItem("Inventar", "Das Inventar, das verändert werden soll", "", "")
                        .withOptions(machine.Inventories.Select(inventory => inventory.Identifier).ToArray())
                        .withStartValue(inventoryTransformer.InventoryIdentifier));
                    transformerChangeMenu.addMenuItem(new InputMenuItem("Item", "Das Item welches transfomiert wird", "", "")
                        .withOptions(InventoryController.getAllConfigItems()?.Select(item => $"{item?.configItemId} {item?.name}").ToArray())
                        .withStartValue($"{inventoryTransformer.Item?.configItemId} {inventoryTransformer.Item?.name}"));
                    transformerChangeMenu.addMenuItem(
                        new InputMenuItem("Variablen", "Die Variablen die bei der Itemerstellung übermittelt werden sollen", "", "").withStartValue(string.Join(", ",
                            inventoryTransformer.PassVariablesIdentifiers)));
                }
                
                transformerChangeMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändere die Transformation", "", "PROCESS_MACHINE_CHANGE_TRANSFORMER", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Variation", transformer.GetType() }, { "Transformer", transformer } }));
                
                transformerChangeMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Transformation", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "TRANSFORMER"}, { "Transformer", transformer } })
                    .needsConfirmation("Transformation löschen?", "Transformation wirklich löschen?"));
                
                transformerMenu.addMenuItem(new MenuMenuItem(transformerChangeMenu.Name, transformerChangeMenu));
            }
            
            machineMenu.addMenuItem(new MenuMenuItem(transformerMenu.Name, transformerMenu));
            
            #endregion
            
            #region Sounds

            var soundsMenu = new Menu("Sounds", "Was möchtest du tun?");
            var soundsCreateMenu = new Menu("Sound erstellen", "Was möchtest du tun?");
            soundsCreateMenu.addMenuItem(new InputMenuItem("Identifier", "Der Identifier des Sounds", "", ""));
            soundsCreateMenu.addMenuItem(new InputMenuItem("Sound", "Der Sound", "", "")
                .withOptions(Enum.GetValues<Sounds>().Select(s => s.ToString()).ToArray()));
            soundsCreateMenu.addMenuItem(new InputMenuItem("Dateiendung", "Die Dateiendung des Sounds", "", ""));
            soundsCreateMenu.addMenuItem(new InputMenuItem("Bedingung", "Die Bedingung, die erfüllt sein muss, damit der Sound abgespielt wird", "", ""));
            soundsCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstellen", "", "PROCESS_MACHINE_CREATE_SOUND", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Machine", machine } }));

            soundsMenu.addMenuItem(new MenuMenuItem(soundsCreateMenu.Name, soundsCreateMenu));

            foreach(var sound in machine.ProcessSounds) {
                var changeSoundsMenu = new Menu(sound.Identifier, "Was möchtest du tun?");
                changeSoundsMenu.addMenuItem(new InputMenuItem("Identifier", "Der Identifier des Sounds", "", "").withStartValue(sound.Identifier));
                changeSoundsMenu.addMenuItem(new InputMenuItem("Sound", "Der Sound", "", "")
                    .withOptions(Enum.GetValues<Sounds>().Select(s => s.ToString()).ToArray())
                    .withStartValue(sound.Sound.ToString()));
                changeSoundsMenu.addMenuItem(new InputMenuItem("Dateiendung", "Die Dateiendung des Sounds", "", "").withStartValue(sound.FileExtension));
                changeSoundsMenu.addMenuItem(new InputMenuItem("Bedingung", "Die Bedingung, die erfüllt sein muss, damit der Sound abgespielt wird", "", "").withStartValue(sound.ConditionFormula));
                changeSoundsMenu.addMenuItem(new MenuStatsMenuItem("Ändern", "Ändern", "", "PROCESS_MACHINE_CHANGE_SOUND", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, { "Sound", sound } }));

                changeSoundsMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche das Inventar", "", "PROCESS_MACHINE_DELETE_ELEMENT", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Machine", machine }, {"Type", "INVENTORY"}, { "Sound", sound } })
                    .needsConfirmation($"{sound.Identifier} löschen?", "Sound wirklich löschen?"));
                
                soundsMenu.addMenuItem(new MenuMenuItem(changeSoundsMenu.Name, changeSoundsMenu));
            }

            machineMenu.addMenuItem(new MenuMenuItem(soundsMenu.Name, soundsMenu));

            #endregion
            

            return machineMenu;
        }
        
        private static int SupportMachineCounter = int.MaxValue;
        private bool onCreateExampleProcessMachine(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var machine = data["Machine"] as ProcessMachine;
            
            var worldProcessMachine = new WorldProcessMachine(SupportMachineCounter--, WorldProcessMachineType.Temporary, machine, CollisionShape.Create(player.Position, 3, 3, 0, true, false, true));
            
            if(player.hasData("SupportMachine")) {
                var oldMachine = player.getData("SupportMachine") as WorldProcessMachine;
                oldMachine.Dispose();
                WorldProcessMachines.RemoveWhere(e => e.Value == oldMachine);
                player.setData("SupportMachine", worldProcessMachine);
            } else {
                player.setData("SupportMachine", worldProcessMachine);
            }
            
            WorldProcessMachines.Add(worldProcessMachine.Id, worldProcessMachine);
            player.sendNotification(Constants.NotifactionTypes.Success, "Maschine erstellt", "Die Maschine wurde erstellt.");
            
            return true;
        }


        #region Create Process Machine

        private bool onCreateProcessMachine(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();

            using(var db = new ChoiceVDb()) {
                var newDbMachine = new configprocessmachine {
                    name = nameEvt.input,
                    variables = new Dictionary<string, double>().ToJson(),
                    displays = new List<string>().ToJson(),
                    steps = new List<string>().ToJson(),
                    configPoints = new List<string>().ToJson(),
                    inventories = new List<string>().ToJson(),
                    transformers = new List<string>().ToJson(),
                    sounds = new List<string>().ToJson(),
                };

                db.configprocessmachines.Add(newDbMachine);

                db.SaveChanges();

                var newMachine = new ProcessMachine(newDbMachine.id, nameEvt.input, new Dictionary<string, StateValue>(), new List<ProcessDisplay>(), new List<ProcessStep>(), new List<ProcessConfigPoint>(), new List<ProcessInventory>(), new List<ProcessTransformer>(), new List<ProcessSound>());
                ProcessMachines.Add(newDbMachine.id, newMachine);

                player.sendNotification(Constants.NotifactionTypes.Success, "Prozessmaschine erstellt", "Die Prozessmaschine wurde erstellt.");
            }

            return true;
        }

        #endregion

        #region Process Variables

        private bool onCreateProcessMachineVariable(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var machine = data["Machine"] as ProcessMachine;

            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var typeEvt = evt.elements[1].FromJson<ListMenuItem.ListMenuItemEvent>();
            var valueEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();

            if(typeEvt.currentElement == "Text") {
                machine.setVariable(nameEvt.input, new StateValue(ProcessStateValueType.String, valueEvt.input));
            } else {
                machine.setVariable(nameEvt.input, new StateValue(ProcessStateValueType.Number, double.Parse(valueEvt.input)));
            }

            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"{nameEvt.input} erstellt mit Wert: {valueEvt.input}", "Die Variable wurde erstellt.");

            return true;
        }

        private bool onChangeProcessMachineVariable(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as InputMenuItem.InputMenuItemEvent;

            var machine = data["Machine"] as ProcessMachine;
            var variableName = data["VariableName"] as string;
            var variableValue = data["VariableValue"] as StateValue;

            if(variableValue.Type == ProcessStateValueType.Number) {
                machine.setVariable(variableName, new StateValue(ProcessStateValueType.Number, double.Parse(evt.input)));
            } else {
                machine.setVariable(variableName, new StateValue(ProcessStateValueType.String, evt.input));
            }
                
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"{variableName} geändert auf Wert: {evt.input}", "Die Variable wurde geändert.");

            return true;
        }

        #endregion

        #region Inventory

        private bool onCreateProcessMachineInventory(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var machine = data["Machine"] as ProcessMachine;

            var identifierEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var nameEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var descriptionEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var conditionEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();
            var sizeEvt = evt.elements[4].FromJson<InputMenuItem.InputMenuItemEvent>();

            var newInventory = new ProcessInventory(nameEvt.input, descriptionEvt.input, int.Parse(sizeEvt.input), identifierEvt.input, conditionEvt.input);

            machine.setInventory(identifierEvt.input, newInventory);

            flushToDb(machine);

            player.sendNotification(Constants.NotifactionTypes.Success, $"{nameEvt.input} erstellt", "Das Inventar wurde erstellt.");

            return true;
        }

        private bool onChangeProcessMachineInventory(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var machine = data["Machine"] as ProcessMachine;
            var inventory = data["Inventory"] as ProcessInventory;

            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var conditionEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var sizeEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();

            inventory.Name = nameEvt.input;
            inventory.Description = descriptionEvt.input;
            inventory.ConditionFormula = conditionEvt.input;
            inventory.StandardSize = int.Parse(sizeEvt.input);

            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"{inventory.Name} geändert", "Das Inventar wurde geändert.");

            return true;
        }

        #endregion

        #region Displays
        
        
        private bool onCreateProcessMachineDisplay(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            
            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var unitEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var representationEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            var newDisplay = new ProcessDisplay(nameEvt.input, descriptionEvt.input, unitEvt.input, representationEvt.input);
            
            machine.addDisplay(newDisplay);
            
            flushToDb(machine);
            
            player.sendNotification(Constants.NotifactionTypes.Success, $"{nameEvt.input} erstellt", "Die Anzeige wurde erstellt.");

            return true;
        }
        
        private bool onChangeProcessMachineDisplay(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            
            var display = data["Display"] as ProcessDisplay;
            
            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var unitEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var representationEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            display.Name = nameEvt.input;
            display.Description = descriptionEvt.input;
            display.Unit = unitEvt.input;
            display.RepresentationFormula = representationEvt.input;

            display.updateShowOnlyVariable();
            
            flushToDb(machine);
            
            player.sendNotification(Constants.NotifactionTypes.Success, $"{display.Name} geändert", "Die Anzeige wurde geändert.");
            
            return true;
        }
        
        #endregion
        
        #region Config Points
        
        private bool onCreateProcessMachineConfigPoint(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            var variation = data["Variation"] as string;
            
            var targetVariableEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var nameEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var descriptionEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var unitEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();
            var blockConditionEvt = evt.elements[4].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            if(variation == "Range") {
                var valuesEvt = evt.elements[5].FromJson<InputMenuItem.InputMenuItemEvent>();
                
                var values = valuesEvt.input.Split(",").Select(v => double.Parse(v)).ToList();
                
                var newConfigPoint = new ProcessRangeConfigPoint(targetVariableEvt.input, nameEvt.input, descriptionEvt.input, unitEvt.input, values, blockConditionEvt.input);
                
                machine.addConfigPoint(newConfigPoint);
            } else if (variation == "Map") {
                var keysEvt = evt.elements[5].FromJson<InputMenuItem.InputMenuItemEvent>();
                var valuesEvt = evt.elements[6].FromJson<InputMenuItem.InputMenuItemEvent>();
                
                var keys = keysEvt.input.Split(",").ToList();
                var values = valuesEvt.input.Split(",").Select(v => double.Parse(v)).ToList();

                var list = new List<(string, double)>();
                for(var i = 0; i < keys.Count; i++) {
                    list.Add((keys[i], values[i]));
                }
                
                var newConfigPoint = new ProcessMapConfigPoint(targetVariableEvt.input, nameEvt.input, descriptionEvt.input, unitEvt.input, list, blockConditionEvt.input);
                
                machine.addConfigPoint(newConfigPoint);
            } else {
                var newConfigPoint = new ProcessTextConfigPoint(targetVariableEvt.input, nameEvt.input, descriptionEvt.input, unitEvt.input, blockConditionEvt.input);
                
                machine.addConfigPoint(newConfigPoint);
            }
            
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Konfigurationspunkt {nameEvt.input} erstellt", "Der Konfigurationspunkt wurde erstellt.");

            return true;
        }
        
        
        private bool onChangeProcessMachineConfigPoint(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            var configPoint = data["ConfigPoint"] as ProcessConfigPoint;
            
            var targetVariableEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var nameEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var descriptionEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var unitEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();
            var blockConditionEvt = evt.elements[4].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            configPoint.TargetVariableName = targetVariableEvt.input;
            configPoint.Name = nameEvt.input;
            configPoint.Description = descriptionEvt.input;
            configPoint.Unit = unitEvt.input;
            configPoint.BlockCondition = blockConditionEvt.input;
            if(configPoint is ProcessRangeConfigPoint rangeConfigPoint) {
                var valuesEvt = evt.elements[5].FromJson<InputMenuItem.InputMenuItemEvent>();
                var values = valuesEvt.input.Split(",").Select(v => double.Parse(v)).ToList();
                rangeConfigPoint.Values = values.Select(v => $"{v} {unitEvt.input}").ToList();
            } else if(configPoint is ProcessMapConfigPoint mapConfigPoint) {
                var keysEvt = evt.elements[5].FromJson<InputMenuItem.InputMenuItemEvent>();
                var valuesEvt = evt.elements[6].FromJson<InputMenuItem.InputMenuItemEvent>();
                
                var keys = keysEvt.input.Split(",").ToList();
                var values = valuesEvt.input.Split(",").Select(v => double.Parse(v)).ToList();
                
                var list = new List<(string, double)>();
                for(var i = 0; i < keys.Count; i++) {
                    list.Add((keys[i], values[i]));
                }
                
                mapConfigPoint.Values = list;
            }

            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Konfigurationspunkt {nameEvt.input} geändert", "Der Konfigurationspunkt wurde geändert.");
            
            return true;
        }
        
        #endregion

        #region Process Steps
        
        private bool onCreateProcessMachineProcessStep(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            
            var durationEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            var newProcessStep = new ProcessStep(Guid.NewGuid(), new List<ProcessStepTransformation>(), int.Parse(durationEvt.input));

            machine.addProcessStep(newProcessStep);
            
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Prozessschritt erstellt", "Der Prozessschritt wurde erstellt.");
            
            return true;
        }
        
        private bool onChangeProcessMachineProcessStep(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            var processStep = data["ProcessStep"] as ProcessStep;
            
            var durationEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            processStep.DurationSeconds = int.Parse(durationEvt.input);
            
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Prozessschritt geändert", "Der Prozessschritt wurde geändert.");
            
            return true;
        }
        
        private bool onCreateProcessMachineTransformation(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            var processStep = data["ProcessStep"] as ProcessStep;
            
            var targetVariableEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var conditionEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var transformationEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var doIfMachineOffEvt = evt.elements[3].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
            
            var newTransformation = new ProcessStepTransformation(transformationEvt.input, conditionEvt.input, targetVariableEvt.input, doIfMachineOffEvt.check);
            
            processStep.addTransformation(newTransformation);
            
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Transformation erstellt", "Die Transformation wurde erstellt.");
            
            return true;
        }

        private bool onChangeProcessMachineTransformation(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            var processStep = data["ProcessStep"] as ProcessStep;
            var transformation = data["Transformation"] as ProcessStepTransformation;
            
            var targetVariableEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var conditionEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var transformationEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var doIfMachineOffEvt = evt.elements[3].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
            
            transformation.OutputVariabe = targetVariableEvt.input;
            transformation.ConditionFormula = conditionEvt.input;
            transformation.TransformationFormula = transformationEvt.input;
            transformation.DoIfMachineOff = doIfMachineOffEvt.check;
            
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Transformation geändert", "Die Transformation wurde geändert.");
            
            return true;
        }
        
        #endregion

        #region Transformer
        
        private bool onCreateProcessMachineTransformer(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;
            
            var machine = data["Machine"] as ProcessMachine;
            var variation = data["Variation"] as string;
            
            var targetVariableEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            
            if(variation == "Inventory") {
                var inventoryEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
                var itemEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();
                var variablesEvt = evt.elements[4].FromJson<InputMenuItem.InputMenuItemEvent>();

                var variables = variablesEvt.input.Split(",").Select(v => v.Trim()).ToList(); 
                var item = InventoryController.getConfigItem(i => $"{i.configItemId} {i.name}" == itemEvt.input);
                
                var newTransformer = new ProcessInventoryTransformer(targetVariableEvt.input, item, inventoryEvt.input, variables);
                
                machine.addTransformer(newTransformer);
            } else if(variation == "Reset-Variable") {
                var valueEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
                
                var newTransformer = new ProcessResetVariableTransformer(targetVariableEvt.input, double.Parse(valueEvt.input));
                
                machine.addTransformer(newTransformer);
            }
            
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"Transformation erstellt", "Die Transformation wurde erstellt.");
            
            return true;
        }
        
        private bool onChangeProcessMachineTransformer(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var machine = data["Machine"] as ProcessMachine;
            var transformer = data["Transformer"] as ProcessTransformer;
            var variation = data["Variation"] as Type;

            var targetVariableEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();

            if(variation == typeof(ProcessInventoryTransformer)) {
                var inventoryEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
                var itemEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
                var variablesEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();

                var variables = variablesEvt.input.Split(",").Select(v => v.Trim()).ToList();
                var item = InventoryController.getConfigItem(i => $"{i.configItemId} {i.name}" == itemEvt.input);

                var inventoryTransformer = transformer as ProcessInventoryTransformer;
                inventoryTransformer.InventoryIdentifier = inventoryEvt.input;
                inventoryTransformer.Item = item;
                inventoryTransformer.VariableIdentifier = targetVariableEvt.input;
                inventoryTransformer.PassVariablesIdentifiers = variables;
            } else if(variation == typeof(ProcessResetVariableTransformer)) {
                var valueEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
                var resetVariableTransformer = transformer as ProcessResetVariableTransformer;
                resetVariableTransformer.ResetValue = double.Parse(valueEvt.input);
            }
            
            flushToDb(machine);
            
            player.sendNotification(Constants.NotifactionTypes.Success, $"Transformation geändert", "Die Transformation wurde geändert.");
            
            return true;
        }
        
        #endregion

        #region Sounds

        private bool onCreateProcessMachineSound(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var machine = data["Machine"] as ProcessMachine;

            var identifierEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var soundEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var fileExtensionEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var conditionEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();

            var sound = Enum.Parse<Sounds>(soundEvt.input);

            var newSound = new ProcessSound(identifierEvt.input, sound, fileExtensionEvt.input, conditionEvt.input);

            machine.addSound(newSound);

            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"{identifierEvt.input} erstellt", "Der Sound wurde erstellt.");

            return true;
        }

        private bool onChangeProcessMachineSound(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var machine = data["Machine"] as ProcessMachine;
            var sound = data["Sound"] as ProcessSound;

            var identifierEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var soundEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var fileExtensionEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var conditionEvt = evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>();

            sound.Identifier = identifierEvt.input;
            sound.Sound = Enum.Parse<Sounds>(soundEvt.input);
            sound.FileExtension = fileExtensionEvt.input;
            sound.ConditionFormula = conditionEvt.input;

            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, $"{identifierEvt.input} geändert", "Der Sound wurde geändert.");

            return true;
        }

        #endregion

        #region Delete
        
        private bool onDeleteProcessMachineElement(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var type = data["Type"] as string;
            var machine = data["Machine"] as ProcessMachine;
            
            if(type == "VARIABLE") {
                var variable = data["Variable"] as string;
                
                machine.removeVariable(variable);
            } else if (type == "INVENTORY") {
                var inventory = data["Inventory"] as string;
                
                machine.removeInventory(inventory);
            } else if (type == "DISPLAY") {
                var display = data["Display"] as ProcessDisplay;
                
                machine.removeDisplay(display);
            } else if (type == "CONFIG_POINT") {
                var configPoint = data["ConfigPoint"] as ProcessConfigPoint;
                
                machine.removeConfigPoint(configPoint);
            } else if (type == "PROCESS_STEP") {
                var processStep = data["ProcessStep"] as ProcessStep;
                
                machine.removeProcessStep(processStep);
            } else if (type == "TRANSFORMATION") {
                var processStep = data["ProcessStep"] as ProcessStep;
                var transformation = data["Transformation"] as ProcessStepTransformation;
                
                processStep.removeTransformation(transformation);
            } else if (type == "TRANSFORMER") {
                var transformer = data["Transformer"] as ProcessTransformer;
                
                machine.removeTransformer(transformer);
            } else if(type == "SOUND") {
                var sound = data["Sound"] as ProcessSound;
                
                machine.removeSound(sound);
            }
            
            flushToDb(machine);
            player.sendNotification(Constants.NotifactionTypes.Success, "Element gelöscht", "Das Element wurde gelöscht.");
            
            return true;
        }
        
        #endregion
        
        #endregion
    }
}
