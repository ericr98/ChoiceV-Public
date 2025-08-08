using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class ElevatorController : ChoiceVScript {
        private static List<Elevator> AllElevators = new List<Elevator>();

        public ElevatorController() {
            EventController.addMenuEvent("MENU_SELECT_ELEVATOR_FLOOR", onMenuSelectElevatorFloor);

            EventController.MainReadyDelegate += onMainReady;

            var menu = new Menu("Fahrstuhl Menü", "Erstelle, lösche und editiere Fahrstühle");
            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Fahrstühle,
                    "Fahrstühle",
                    generateSupportElevators
                )
            );

            #region Support

            EventController.addMenuEvent("SUPPORT_CREATE_NEW_ELEVATOR", supportCreateNewElevator);
            EventController.addMenuEvent("SUPPORT_CHANGE_ELEVATOR_NAME", supportChangeElevatorName);
            EventController.addMenuEvent("SUPPORT_DELETE_ELEVATOR", supportDeleteElevator);

            EventController.addMenuEvent("SUPPORT_CREATE_NEW_ELEVATORFLOOR", supportCreateElevatorFloor);
            EventController.addMenuEvent("SUPPORT_DELETE_ELEVATORFLOOR", supportDeleteElevatorFloor);

            #endregion
        }

        #region Support

        private Menu generateSupportElevators(IPlayer player) {
            var menu = new Menu("Fahrstühle", "Alle Fahrstühle");
            menu.addMenuItem(new InputMenuItem("Fahrstuhl erstellen", "Erstelle einen Fahrstuhl mit dem gegeben Namen", "", "SUPPORT_CREATE_NEW_ELEVATOR"));

            SupportController.setCurrentSupportFastAction(player, () => player.showMenu(generateSupportElevators(player)));

            foreach(var elv in AllElevators) {
                var elvData = new Dictionary<string, dynamic> {
                    { "Elevator", elv },
                };

                var elevatorMenu = new Menu(elv.Name, $"Editiere den {elv.Name}");
                elevatorMenu.addMenuItem(new InputMenuItem("Namen ändern", "Ändere den Namen des Fahrstuhles", $"{elv.Name}", "SUPPORT_CHANGE_ELEVATOR_NAME", MenuItemStyle.green).withData(elvData));

                var floorMenu = new Menu("Ebenen erstellen, löschen", "Erstelle und lösche Ebenen");

                var floorCreateMenu = new Menu("Ebene erstellen", "Gibt die Daten ein");
                floorCreateMenu.addMenuItem(new InputMenuItem("Level", "Das Level der Ebene. Jede Ebene darf nur einmal existieren. Gibt die Sortierung an, bzw den Namen, wenn kein anderer eingegeben ist", $"z.B. 1, 99, etc.", ""));
                floorCreateMenu.addMenuItem(new InputMenuItem("Name", "Der Name der Ebene", $"z.B. EG, 1. OG, etc.", ""));
                var codeTypes = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => !t.IsAbstract && t.IsClass && typeof(ElevatorFloor).IsAssignableFrom(t)).Select(t => t.Name).ToArray();

                floorCreateMenu.addMenuItem(new ListMenuItem("Backend-Art", "Wähle welche Art von Ebene es im Code sein soll", codeTypes, ""));
                floorCreateMenu.addMenuItem(new MenuStatsMenuItem("Ebene erstellen", "Erstelle die Ebene", "SUPPORT_CREATE_NEW_ELEVATORFLOOR", MenuItemStyle.green).withData(elvData));

                floorMenu.addMenuItem(new MenuMenuItem(floorCreateMenu.Name, floorCreateMenu));

                foreach(var floor in elv.Floors) {
                    var floorData = new Dictionary<string, dynamic> {
                        { "Floor", floor },
                    };

                    floorMenu.addMenuItem(new ClickMenuItem($"Ebene {floor.Level} löschen", $"Lösche {floor.Level} die Ebene", "", "SUPPORT_DELETE_ELEVATORFLOOR", MenuItemStyle.red).withData(floorData).needsConfirmation("Ebene löschen?", "Ebene wirklich löschen?"));
                }

                elevatorMenu.addMenuItem(new MenuMenuItem(floorMenu.Name, floorMenu));
                elevatorMenu.addMenuItem(new ClickMenuItem($"Fahrstuhl löschen", "Lösche den Fahrstuhl", "", "SUPPORT_DELETE_ELEVATOR", MenuItemStyle.red).withData(elvData).needsConfirmation("Fahrstuhl löschen?", "Fahrstuhl wirklich löschen?"));

                menu.addMenuItem(new MenuMenuItem(elevatorMenu.Name, elevatorMenu));
            }

            return menu;
        }

        private bool supportCreateNewElevator(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var name = (menuItemCefEvent as InputMenuItemEvent).input;
            using(var db = new ChoiceVDb()) {
                var newDbEl = new configelevator {
                    name = name,
                };

                db.configelevators.Add(newDbEl);
                db.SaveChanges();

                AllElevators.Add(new Elevator(newDbEl.id, name));

                player.sendNotification(Constants.NotifactionTypes.Success, "Fahrstuhl erfolgreich erstellt", "Fahrstuhl erstellt");
            }

            return true;
        }

        private bool supportChangeElevatorName(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var elv = (Elevator)data["Elevator"];
            var name = (menuItemCefEvent as InputMenuItemEvent).input;

            using(var db = new ChoiceVDb()) {
                var dbEl = db.configelevators.FirstOrDefault(e => e.id == elv.Id);
                dbEl.name = name;

                db.SaveChanges();

                elv.Name = name;

                player.sendNotification(Constants.NotifactionTypes.Success, "Fahrstuhl erfolgreich umbenannt", "Fahrstuhl umbenannt");
            }

            return true;
        }

        private bool supportDeleteElevator(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var elv = (Elevator)data["Elevator"];

            using(var db = new ChoiceVDb()) {
                AllElevators.Remove(elv);

                var dbEl = db.configelevators.FirstOrDefault(e => e.id == elv.Id);
                db.configelevators.Remove(dbEl);

                db.SaveChanges();

                Logger.logDebug(LogCategory.Support, LogActionType.Updated, player, $"just deleted Elevator {elv.Id}: {elv.Name}");

                player.sendNotification(Constants.NotifactionTypes.Warning, "Fahrstuhl erfolgreich gelöscht", "Fahrstuhl gelöscht");
            }

            return true;
        }

        private bool supportCreateElevatorFloor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var elv = (Elevator)data["Elevator"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var levelEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var nameEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var codeFloor = evt.elements[2].FromJson<ListMenuItemEvent>();

            try {
                var level = int.Parse(levelEvt.input);
                var name = nameEvt.input;
                var codeName = codeFloor.currentElement;

                player.sendNotification(Constants.NotifactionTypes.Info, "Erstelle nun die Fläche zum Rufen des Fahrstuhles", "");
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                    player.sendNotification(Constants.NotifactionTypes.Info, "Erstelle nun den Innenraum des Fahrstuhles. Sei SO GENAU wie möglich!", "");
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p2, w2, h2, r2) => {
                        player.sendNotification(Constants.NotifactionTypes.Info, "Registriere nun die eine Tür der Ebene!", "");
                        DoorController.startDoorRegistrationWithCallback(player, $"{elv.Name} {level}", (d) => {
                            player.sendNotification(Constants.NotifactionTypes.Info, "Registriere nun die andere Tür des Fahrstuhls!", "");
                            DoorController.startDoorRegistrationWithCallback(player, $"{elv.Name} {level}", (d2) => {
                                using(var db = new ChoiceVDb()) {
                                    var newDbFloor = new configelevatorfloor {
                                        elevatorId = elv.Id,
                                        level = level,
                                        name = name,
                                        door1Id = d.Id,
                                        door2Id = d2.Id,
                                        callCollPos = p.ToJson(),
                                        callCollWidth = w,
                                        callCollHeight = h,
                                        callCollRotation = r,
                                        insideCollPos = p2.ToJson(),
                                        insideCollWidth = w2,
                                        insideCollHeight = h2,
                                        insideCollRotation = r2,
                                        codeElevator = codeName,
                                    };

                                    db.configelevatorfloors.Add(newDbFloor);

                                    db.SaveChanges();

                                    var floor = createFloorFromDb(elv, newDbFloor);
                                    elv.Floors.Add(floor);

                                    Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"just created Elevator {elv.Id}: {elv.Name}");
                                    player.sendNotification(Constants.NotifactionTypes.Success, "Ebene erfolgreich erstellt!", "");
                                }
                            });
                        });
                    });
                });
            } catch(Exception) {
                player.sendBlockNotification("Etwas ist schiefgelaufen!", "");
            }
            return true;
        }

        private bool supportDeleteElevatorFloor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var floor = (ElevatorFloor)data["Floor"];

            using(var db = new ChoiceVDb()) {
                floor.Main.Floors.Remove(floor);

                var dbFloor = db.configelevatorfloors.FirstOrDefault(f => f.elevatorId == floor.Main.Id && floor.Level == f.level);
                db.configelevatorfloors.Remove(dbFloor);

                db.SaveChanges();

                Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"just deleted floor {floor.Level} from elevator {floor.Main.Id}: {floor.Main.Name}");
                player.sendNotification(Constants.NotifactionTypes.Warning, "Ebene erfolgreich gelöscht", "Ebene gelöscht");
            }

            return true;
        }

        #endregion

        private void onMainReady() {
            loadElevators();
        }

        private void loadElevators() {
            Dictionary<int, Elevator> tempSave = new Dictionary<int, Elevator>();

            using(var db = new ChoiceVDb()) {
                foreach(var elevator in db.configelevators) {
                    var elv = new Elevator(elevator.id, elevator.name);

                    tempSave.Add(elv.Id, elv);

                    AllElevators.Add(elv);
                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"Elevator loaded: id: {elv.Id}, name: {elv.Name}");
                }

                foreach(var dbFloor in db.configelevatorfloors) {
                    var elv = tempSave[dbFloor.elevatorId];

                    ElevatorFloor floor = createFloorFromDb(elv, dbFloor);

                    elv.Floors.Add(floor);

                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"ElevatorFloor loaded: id: {dbFloor.elevatorId}, level: {dbFloor.level}");
                }
            }
        }

        private ElevatorFloor createFloorFromDb(Elevator elv, configelevatorfloor dbFloor) {
            var doorList = DoorController.AllDoorsByPosition.Values.Where(d => d.Id == dbFloor.door1Id || d.Id == dbFloor.door2Id).ToList();

            var call = CollisionShape.Create(dbFloor.callCollPos.FromJson(), dbFloor.callCollWidth, dbFloor.callCollHeight, dbFloor.callCollRotation, true, true, true);
            var inside = CollisionShape.Create(dbFloor.insideCollPos.FromJson(), dbFloor.insideCollWidth, dbFloor.insideCollHeight, dbFloor.insideCollRotation, true, true, true);

            switch(dbFloor.codeElevator) {
                case nameof(PhysicalElevatorFloor):
                    return new PhysicalElevatorFloor(elv, dbFloor.level, dbFloor.name, doorList, call, inside);
                case nameof(DimensionElevatorFloor):
                    return new DimensionElevatorFloor(elv, dbFloor.level, doorList, call, inside);
                case nameof(SelectiveDimensionElevatorFloor):
                    return new SelectiveDimensionElevatorFloor(elv, dbFloor.level, dbFloor.name, doorList, call, inside);
                default:
                    return null;
            }
        }

        private bool onMenuSelectElevatorFloor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var elv = (Elevator)data["elevator"];
            var level = (int)data["level"];

            var newFloor = elv.Floors.FirstOrDefault(f => f.Level == level);

            if(menuItemCefEvent is InputMenuItemEvent) {
                var input = ((InputMenuItemEvent)menuItemCefEvent).input;
                try {
                    newFloor.Main.onElevatorSelectFloor(player, newFloor, int.Parse(input));
                } catch(Exception) {
                    player.sendBlockNotification("Die Eingabe war keine gültige Raumnummer", "Ungültige Eingabe");
                }
            } else {
                var dimension = Constants.GlobalDimension;
                if(data.ContainsKey("SetDimension")) {
                    dimension = level;
                }

                if(newFloor is PhysicalElevatorFloor) {
                    newFloor.Main.onElevatorSelectFloor(player, newFloor, Constants.GlobalDimension);
                } else {
                    newFloor.Main.onElevatorSelectFloor(player, newFloor, level);
                }
            }



            return true;
        }
    }

    public class Elevator {
        public int Id;
        public string Name;
        public List<ElevatorFloor> Floors { get; protected set; }
        public bool IsInUse;
        public bool IsOnWay;

        public IInvoke CallInvoke;

        public Elevator(int id, string name) {
            Id = id;
            Name = name;
            IsInUse = false;
            IsOnWay = false;

            Floors = new List<ElevatorFloor>();
        }

        public void onElevatorCall(IPlayer player, ElevatorFloor floor, int dimension) {
            if(IsInUse) {
                player.sendBlockNotification("Der Fahrstuhl ist gerade in Benutzung!", "Fahrstuhl besetzt!");
                return;
            }

            player.sendNotification(Constants.NotifactionTypes.Info, "Der Fahrstuhl ist auf dem Weg!", "Fahrstuhl auf dem Weg");

            var level = getNearestFloor(player.Position);
            IsInUse = true;
            IsOnWay = true;
            InvokeController.AddTimedInvoke("Elevator" + Name, (ivk) => {
                floor.elevatorArriveAtFloor(level, dimension);
                IsOnWay = false;
                CallInvoke = InvokeController.AddTimedInvoke("Elevator" + Name, (ivk) => {
                    IsInUse = false;
                    level.closeDoors();
                    ivk.EndSchedule();
                }, TimeSpan.FromSeconds(10), true);
            }, TimeSpan.FromSeconds(5), false);
        }

        public void onElevatorSelectFloor(IPlayer player, ElevatorFloor newFloor, int dimension) {
            if(IsOnWay) {
                player.sendBlockNotification("Jemand anderes hat den Fahrstuhl gerade gerufen!", "Fahrstuhl besetzt!");
                return;
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"Player just called elevator {Id}: {Name} to floor {newFloor.Level}");
            player.sendNotification(Constants.NotifactionTypes.Info, "Der Fahrstuhl ist losgefahren warte kurz!", "Fahrstuhl fährt");

            if(CallInvoke != null) {
                CallInvoke.EndSchedule();
                CallInvoke = null;
            }

            IsInUse = true;
            IsOnWay = true;
            var level = getNearestFloor(player.Position);
            foreach(var floor in Floors) {
                floor.closeDoors();
            }

            foreach(var entity in getAllEntitiesInElevator()) {
                if(entity is IPlayer) {
                    SoundController.playSound(entity as IPlayer, SoundController.Sounds.ElevatorMusic, 0.075f);
                }
            }

            InvokeController.AddTimedInvoke("Elevator" + Name, (ivk) => {
                newFloor.driveToLevel(level, dimension);
                InvokeController.AddTimedInvoke("Elevator" + Name, (ivk) => {
                    IsInUse = false;
                    IsOnWay = false;
                    newFloor.closeDoors();
                    ivk.EndSchedule();
                }, TimeSpan.FromSeconds(7.5), true);
            }, TimeSpan.FromSeconds(10), false);
        }

        public ElevatorFloor getNearestFloor(Position position) {
            ElevatorFloor currentFloor = null;
            float maxDis = 999999f;
            foreach(var floor in Floors) {
                var dis = position.Distance(floor.CallCollisionShape.Position);
                if(dis < maxDis) {
                    maxDis = dis;
                    currentFloor = floor;
                }
            }

            return currentFloor;
        }

        public IEntity[] getAllEntitiesInElevator() {
            var combinedList = new List<IEntity>();

            foreach(var floor in Floors) {
                var allEntities = floor.InSideCollisionShape.getAllEntitiesList();
                combinedList = combinedList.Concat(allEntities).ToList();
            }

            return combinedList.Distinct((e, e2) => e.NativePointer == e2.NativePointer).ToArray();
        }

        public void setFloors(List<ElevatorFloor> floors) {
            Floors = floors;
        }
    }

    public abstract class ElevatorFloor {
        public int Level;
        public List<Door> Doors;

        [JsonIgnore]
        public Elevator Main;
        public CollisionShape CallCollisionShape;

        public CollisionShape InSideCollisionShape { get; private set; }

        public ElevatorFloor(Elevator main, int level, List<Door> doors, CollisionShape callCollisionShape, CollisionShape inSideCollisionShape) {
            Main = main;
            Level = level;
            Doors = doors;

            closeDoors();

            CallCollisionShape = callCollisionShape;
            CallCollisionShape.InteractableOnBusy = true;
            CallCollisionShape.OnCollisionShapeInteraction += onInteractCall;

            InSideCollisionShape = inSideCollisionShape;
            InSideCollisionShape.InteractableOnBusy = true;
            InSideCollisionShape.OnCollisionShapeInteraction += onInteractInside;

            CallCollisionShape.InteractData = new Dictionary<string, dynamic> { { "elevator", main }, { "floor", this } };
        }

        private bool onInteractCall(IPlayer player) {
            Main.onElevatorCall(player, this, player.Dimension);

            return true;
        }

        private bool onInteractInside(IPlayer player) {
            var menu = new Menu(Main.Name, "Wähle eine Etage aus");

            foreach(var f in Main.Floors) {
                menu.addMenuItem(f.getMenuItem());
            }

            player.showMenu(menu);
            return true;
        }

        public void elevatorArriveAtFloor(ElevatorFloor oldFloor, int dimension) {
            openDoors();

            foreach(var entity in Main.getAllEntitiesInElevator()) {
                entityArriveAtDestination(entity, oldFloor, dimension);
            }

            SoundController.playSoundAtCoords(Doors.First().Position, 4.5f, SoundController.Sounds.ElevatorBell, 0.5f);
        }

        public void driveToLevel(ElevatorFloor oldFloor, int dimension) {
            foreach(var entity in Main.getAllEntitiesInElevator()) {
                entityArriveAtDestination(entity, oldFloor, dimension);
            }

            SoundController.playSoundAtCoords(Doors.First().Position, 4.5f, SoundController.Sounds.ElevatorBell, 0.5f);

            openDoors();
        }

        public void setInsideCollisionShape(CollisionShape shape) {
            InSideCollisionShape = shape;
        }

        public void entityArriveAtDestination(IEntity entity, ElevatorFloor oldFloor, int dimension) {
            //Dimensions are always negative
            if(entity is IPlayer player) {
                player.changeDimension(-Math.Abs(dimension));
                if(player.IsInVehicle) {
                    return;
                }

                var inPos = InSideCollisionShape.Position;
                var oldInPos = oldFloor.InSideCollisionShape.Position;
                var ePos = player.Position;
                if(!CarryController.isOnCarry(entity as IPlayer)) {
                    player.Position = new Position((ePos.X - oldInPos.X) + inPos.X, (ePos.Y - oldInPos.Y) + inPos.Y, inPos.Z);
                    if(CarryController.isPersonCarrier(player)) {
                        CarryController.reapplyAnimation(player);
                    }
                }

            } else if(entity is ChoiceVVehicle vehicle) {
                vehicle.changeDimension(-Math.Abs(dimension));
                var min = vehicle.DbModel.StartPoint.FromJson();


                var inPos = InSideCollisionShape.Position;
                var oldInPos = oldFloor.InSideCollisionShape.Position;

                var ePos = vehicle.Position;
                vehicle.Position = new Position((ePos.X - oldInPos.X) + inPos.X, (ePos.Y - oldInPos.Y) + inPos.Y, inPos.Z - 1 + Math.Abs(min.Z));
            }
        }

        public void closeDoors() {
            foreach(var door in Doors) {
                door.lockDoor();
            }
        }

        public void openDoors() {
            foreach(var door in Doors) {
                door.unlockDoor();
            }
        }

        public abstract MenuItem getMenuItem();
    }

    public class PhysicalElevatorFloor : ElevatorFloor {
        public string Name;

        public PhysicalElevatorFloor(Elevator main, int level, string name, List<Door> doors, CollisionShape callCollisionShape, CollisionShape insideCollisionShape) : base(main, level, doors, callCollisionShape, insideCollisionShape) {
            Name = name;
        }

        public override MenuItem getMenuItem() {
            if(Name.Equals("")) {
                return new ClickMenuItem($"Etage {Level}", $"Fahre in Etage {Level}", "", "MENU_SELECT_ELEVATOR_FLOOR").withData(new Dictionary<string, dynamic> { { "elevator", Main }, { "level", Level } });
            } else {
                return new ClickMenuItem(Name, $"Fahre in/ins {Name}", "", "MENU_SELECT_ELEVATOR_FLOOR").withData(new Dictionary<string, dynamic> { { "elevator", Main }, { "level", Level } });
            }
        }
    }

    public class DimensionElevatorFloor : ElevatorFloor {
        public DimensionElevatorFloor(Elevator main, int level, List<Door> doors, CollisionShape callCollisionShape, CollisionShape insideCollisionShape) : base(main, level, doors, callCollisionShape, insideCollisionShape) { }

        public override MenuItem getMenuItem() {
            return new InputMenuItem($"Etage wählen", $"Fahre auf die gewählte Etage. Wähle die Etagennummer", "Etagennummer auswählen", "MENU_SELECT_ELEVATOR_FLOOR").withData(new Dictionary<string, dynamic> { { "elevator", Main }, { "level", Level } });
        }
    }

    public class SelectiveDimensionElevatorFloor : ElevatorFloor {
        public string Name;

        public SelectiveDimensionElevatorFloor(Elevator main, int level, string name, List<Door> doors, CollisionShape callCollisionShape, CollisionShape insideCollisionShape) : base(main, level, doors, callCollisionShape, insideCollisionShape) {
            Name = name;
        }

        public override MenuItem getMenuItem() {
            if(Name.Equals("")) {
                return new ClickMenuItem($"Etage {Level}", $"Fahre in Etage {Level}", "", "MENU_SELECT_ELEVATOR_FLOOR").withData(new Dictionary<string, dynamic> { { "elevator", Main }, { "level", Level }, { "SetDimension", true } });
            } else {
                return new ClickMenuItem(Name, $"Fahre in/ins {Name}", "", "MENU_SELECT_ELEVATOR_FLOOR").withData(new Dictionary<string, dynamic> { { "elevator", Main }, { "level", Level }, { "SetDimension", true } });
            }
        }
    }
}
