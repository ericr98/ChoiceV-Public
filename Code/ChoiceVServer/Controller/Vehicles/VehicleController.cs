using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Bogus.DataSets;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;
using System.Xml.Linq;
using BenchmarkDotNet.Attributes;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Model.FsDatabase;

namespace ChoiceVServer.Controller {

    [Flags]
    public enum SpecialVehicleModelFlag : long {
        IsRappelHelicopter = 1, // Is a helicopter from which the player can rappel
        IsEmergencyCarryVehicle = 2, // Is a vehicle which can collect dead players from afar
        IsFlatBed = 4, // Is a vehicle that can carry other vehicles
        IsFileSystemOpenVehicle = 8, // Allows to open the Filesystem inside the vehicle
        IsFireTruckWater = 16, // Allows to stop fires, sends water truck event to fire system on shoot
        IsFireTruckFoam = 32, // Allows to stop fires, sends foam truck event to fire system on shoot
        IsControlCenterVehicle = 64, // Is a vehicle that can open the control center
    }

    public enum VehicleClassesDbIds : int {
        Compacts = 0,
        Sedans = 1,
        SUVs = 2,
        Coupes = 3,
        Muscle = 4,
        Sport_Classics = 5,
        Sports = 6,
        Super = 7,
        MotorCycles = 8,
        Off_Road = 9,
        Industrial = 10,
        Utility = 11,
        Vans = 12,
        Cycles = 13,
        Boats = 14,
        Helicopters = 15,
        Planes = 16,
        Service = 17,
        Emergency = 18,
        Military = 19,
        Commercial = 20,
        Trains = 21,
        Open_Wheel = 22,
    }

    public delegate void VehicleSpawnDataSetDelegate(ChoiceVVehicle vehicle, vehiclesdatum data);

    public class VehicleController : ChoiceVScript {
        public static List<VehicleSelfMenuElement> AllVehicleSelfMenuElements = [];

        public static Dictionary<string, VehicleSpawnDataSetDelegate> AllOnSpawnDataInits = [];

        private static Dictionary<int, configvehiclesmodel> AllModels = [];
        private static Dictionary<int, configvehiclesclass> AllClasses = [];

        public static List<InjectInventoryPredicateDelegate> AllTrunkOpenInjects = [];

        public VehicleController() {
            EventController.MainReadyDelegate += loadVehicles;

            InvokeController.AddTimedInvoke("VehicleSaver", saveVehicles, TimeSpan.FromSeconds(30), true);
            InvokeController.AddTimedInvoke("VehicleVeryLongTick", veryLongTick, TimeSpan.FromSeconds(2.5), true);

            EventController.PlayerEnterVehicleDelegate += onPlayerEnterVehicle;
            EventController.PlayerChangeVehicleSeatDelegate += onPlayerChangeSeat;
            EventController.PlayerExitVehicleDelegate += onPlayerExitVehicle;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnected;

            EventController.LongTickDelegate += onLongTick;
            EventController.TickDelegate += onTick;

            EventController.addKeyEvent("LOCK_CAR", ConsoleKey.L, "Fahrzeugverriegelung", onVehicleLock);
            EventController.addKeyEvent("SEATBELT", ConsoleKey.Oem3, "Anschnallgurt", onVehicleSeatbelt); //Ö
            EventController.addKeyEvent("MOTOR", ConsoleKey.Y, "Fahrzeugmotor", onVehicleEngine);
            EventController.addKeyEvent("CAR_TRUNK", ConsoleKey.K, "Fahrzeugkofferraum", onVehicleTrunk);

            EventController.addKeyEvent("ENTER_DRIVER", ConsoleKey.F, "Als Fahrer einsteigen", onVehicleEnterDriverKeyPress);
            EventController.addKeyToggleEvent("ENTER_PASSENGER", ConsoleKey.G, "Als Beifahrer einsteigen", onVehicleEnterPassengerKeyPress, true);

            EventController.addKeyEvent("CAR_LIGHTS", ConsoleKey.H, "Scheinwerfer an/ausschalten", onVehicleToggleHeadlights);

            EventController.addEvent("VEHICLE_ENTER_DRIVER", onVehicleEnterDriver);
            EventController.addEvent("VEHICLE_ENTER_PASSENGER", onVehicleEnterPassenger);

            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Model-Info anpassen",
                    getSupportVehicleInfo,
                    p => p is IPlayer player && player.getCharacterData().AdminMode && player.getAdminLevel() >= 2,
                    t => true,
                    false,
                    MenuItemStyle.yellow
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Fahrzeug entfernen", "Entferne das Fahrzeug permanent", "", "REMOVE_VEHICLE_INTERACTION", MenuItemStyle.red).needsConfirmation("Wirklich entfernen?", "Fahrzeug wirklich entfernen?"),
                    p => p is IPlayer player && player.getCharacterData().AdminMode && player.getAdminLevel() >= 2,
                    t => true
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Fahrzeug reparieren", "Repriere das Fahrzeug komplett", "", "SUPPORT_REPAIR_VEHICLE", MenuItemStyle.green),
                    p => p is IPlayer player && player.getCharacterData().AdminMode && player.getAdminLevel() >= 2,
                    t => true
                )
            );
            EventController.addMenuEvent("SUPPORT_REPAIR_VEHICLE", onSupportRepairVehicle);

            addSelfMenuElement(
                new ConditionalVehicleGeneratedSelfMenuElement(
                    (v, p) => givePlayerInVehicleGenerator(v, p, "Beifahrer"),
                    v => v.PassengerList.Count > 1,
                    p => true
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    (sender, target) => givePlayerInVehicleGenerator(target as ChoiceVVehicle, sender as IPlayer, "Insasse"),
                    sender => true,
                    target => target is ChoiceVVehicle veh && veh.PassengerList.Count > 0
                )
            );

            EventController.addMenuEvent("FINISH_VEHICLE_MODEL_INTERACTION", finishVehicleInfoInteraction);
            EventController.addMenuEvent("REMOVE_VEHICLE_INTERACTION", removeVehicleInteraction);

            EventController.addMenuEvent("VEHICLE_GIVE_PASSENGER_ITEM", onVehicleGivePassengerItem);

            loadSelfElements();

            using(var db = new ChoiceVDb()) {
                AllModels = db.configvehiclesmodels.Include(m => m._class).ToDictionary(m => m.id, m => m);
                AllClasses = db.configvehiclesclasses.ToDictionary(c => c.classId, c => c);
            }

            addVehicleSpawnDataSetCallback("ACTIVATED_EXTRAS", onSpawnWithActivatedExtras);

            EventController.MainReadyDelegate += onMoveAllVehicles;

            EventController.VehicleDamageDelegate += onVehicleDamage;
            //EventController.ExplosionDelegate += onExplosion;
            EventController.VehicleDestroyDelegate += onVehicleDestroy;
        }

        private void onMoveAllVehicles() {
            foreach(var vehicle in ChoiceVAPI.GetAllVehicles()) {
                EventController.onVehicleMoved(this, vehicle, vehicle.Position, vehicle.Position, WorldController.getGridBlock(vehicle.Position), 0);
            }
        }

        private void loadSelfElements() {
            addSelfMenuElement(
                new ConditionalVehicleSelfMenuElement(
                    () => new ClickMenuItem("Von Helikopter abseilen", "Seile dich vom Helikopter ab", "", "RAPPEL_FROM_HELIKOPTER"),
                    v => hasVehicleSpecialFlag(v, SpecialVehicleModelFlag.IsRappelHelicopter),
                    p => p.Seat != 1 && p.Seat != 2
                )
            );

            EventController.addMenuEvent("RAPPEL_FROM_HELIKOPTER", onRappelFromHelikopter);
        }


        public static Dictionary<string, PlayerConnectDataSetDelegate> AllOnConnectDataInits = new Dictionary<string, PlayerConnectDataSetDelegate>();

        /// <summary>
        /// Adds a callback for a permanent data, triggered when a vehicle is spawned
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        public static void addVehicleSpawnDataSetCallback(string eventName, VehicleSpawnDataSetDelegate callback) {
            if(!AllOnSpawnDataInits.ContainsKey(eventName)) {
                AllOnSpawnDataInits.Add(eventName, callback);
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"addVehicleSpawnDataSetCallback: Tried to register dataSetCallback which was already registered!");
            }
        }

        private void onSpawnWithActivatedExtras(ChoiceVVehicle vehicle, vehiclesdatum data) {
            var list = data.value.FromJson<List<string>>();

            foreach(var extra in list) {
                vehicle.ToggleExtra(byte.Parse(extra), true);
            }
        }

        private bool onRappelFromHelikopter(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.emitClientEvent("RAPPEL_FROM_HELICOPTER", player);
            return true;
        }

        public static Menu getVehicleIndoorMenu(IPlayer player) {
            var veh = (ChoiceVVehicle)player.Vehicle;
            var menu = new Menu("Fahrzeug Innen-Interaktion", "Was möchtest du tun?", true);

            var data = player.getCharacterData();
            foreach(var element in AllVehicleSelfMenuElements) {
                if(element.ShowForTypes.Contains(data.CharacterType) && element.checkShow(veh, player)) {
                    var menuEl = element.getMenuElement(veh, player);
                    if(menuEl is MenuItem) {
                        var item = menuEl as MenuItem;
                        menu.addMenuItem(item);
                    } else {
                        var virtualMenu = menuEl as VirtualMenu;

                        //Make MenuItem only using generator, to reduce cpu power
                        menu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu));
                    }
                }
            }
            return menu;
        }

        /// <summary>
        /// Adds a MenuElement to the player "self" interaction menu
        /// </summary>
        public static void addSelfMenuElement(VehicleSelfMenuElement element, List<CharacterType> showForTypes = null) {
            element.ShowForTypes = showForTypes ?? [CharacterType.Player];
            AllVehicleSelfMenuElements.Add(element);
        }

        //Vehicle Remove Command
        private void loadVehicles() {
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.vehicles
                                    .Include(v => v.model)
                                        .ThenInclude(m => m._class)
                                    .Include(v => v.model)
                                        .ThenInclude(v => v.engineStartAnimNavigation)
                                    .Include(v => v.model)
                                        .ThenInclude(m => m.configvehiclesseats)
                                    .Include(v => v.vehiclesregistrations)
                                    .Include(v => v.vehiclesdamagebasis)
                                    .Include(v => v.vehiclestuningmods)
                                    .Include(v => v.vehiclestuningbase)
                                    .Include(v => v.vehiclescoloring)
                                    .Include(v => v.vehiclesdata)) {
                    try {
                        spawnVehicle(row);
                    } catch(Exception e) {
                        Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"Vehicle couldnt be spawned: vehicleId: {row.id}");
                    }
                }
            }
        }

        private void saveVehicles(IInvoke obj) {
            foreach(var vehicle in ChoiceVAPI.GetAllVehicles()) {
                saveVehicle(vehicle);
            }
        }

        private void onTick() {
            var timer = DateTime.Now;

            foreach(var vehicle in ChoiceVAPI.GetAllVehicles().Reverse<ChoiceVVehicle>()) {
                if(vehicle == null || !vehicle.Exists()) {
                    continue;
                }

                var aktPos = vehicle.Position;
                var dist = vehicle.LastPosition.Distance(aktPos);
                if(dist > 0.15f && vehicle.Position != Position.Zero && vehicle.LastPosition != Position.Zero) {
                    EventController.onVehicleMoved(this, vehicle, vehicle.LastPosition, aktPos, WorldController.getGridBlock(aktPos), dist);
                }

                vehicle.LastPosition = vehicle.Position;
            }

            var span = DateTime.Now.Subtract(timer).TotalMilliseconds;
            if(span > 50) Logger.logWarning(LogCategory.System, LogActionType.Event, "[ChoiceV] Vehicle tick runtime: " + span + "ms");
        }

        private void onLongTick() {
            foreach(var veh in ChoiceVAPI.GetAllVehicles().Reverse<ChoiceVVehicle>()) {
                if(veh == null || !veh.Exists()) {
                    continue;
                }

                if(!veh.CanDrive) {
                    veh.EngineOn = false;
                }

                if(veh.hasData("OLD_HEALTH")) {
                    var old = (uint)veh.getData("OLD_HEALTH");
                    if(old != veh.BodyHealth) {
                        EventController.VehicleHealthChangeDelegate?.Invoke(veh, old, veh.BodyHealth);
                    }
                }

                veh.setData("OLD_HEALTH", veh.BodyHealth);

                if(veh.LastPosition != Position.Zero && veh.EngineOn) {
                    var dist = veh.Position.Distance(veh.LastPosition) * 5;
                    if(dist > 0.1) {
                        //movement fuel consumption (not for cycles)
                        if(veh.VehicleClass != VehicleClassesDbIds.Cycles) {
                            var usedFuel = dist / Constants.ONE_FUEL_TANK_DISTANCE;
                            veh.reduceFuel(usedFuel);
                        }

                        veh.DrivenDistance += dist;
                    } else {
                        //running fuel consumption
                        veh.reduceFuel(0.00005f);
                    }
                }
            }
        }

        private void veryLongTick(IInvoke obj) {
            foreach(var vehicle in ChoiceVAPI.GetAllVehicles()) {
                try {
                    vehicle.verylongTickUpdate();
                } catch(Exception e) {
                    Logger.logException(e, "Error in veryLongTick");
                    continue;
                }
            }
        }

        public static void saveVehicle(ChoiceVVehicle vehicle) {
            using(var db = new ChoiceVDb()) {
                try {
                    if(vehicle.SkipNextSave) {
                        vehicle.SkipNextSave = false;
                        return;
                    }

                    var dbVeh = db.vehicles
                            .Include(v => v.vehiclesdamagebasis)
                            .Include(v => v.vehiclescoloring)
                       .FirstOrDefault(v => v.id == vehicle.VehicleId);

                    if(dbVeh != null) {
                        vehicle.updateDbDamage(ref dbVeh);
                        vehicle.updateDbVehicle(dbVeh);
                    }

                    db.SaveChanges();
                } catch(Exception) {
                    Logger.logError($"Vehicle save failed: {vehicle.VehicleId}",
                        $"Fehler beim Fahrzeug speichern: Fahrzeug DB speichern ist fehlgeschlagen: {vehicle.VehicleId}");
                }
            }
        }

        public static void saveVehicleColoring(ChoiceVVehicle vehicle) {
            using(var db = new ChoiceVDb()) {
                var dbVeh = db.vehicles
                   .Include(v => v.vehiclescoloring)
                .FirstOrDefault(v => v.id == vehicle.VehicleId);

                vehicle.updateDbColoring(dbVeh);

                db.SaveChanges();
            }
        }

        public static void saveVehicleTuning(ChoiceVVehicle vehicle) {
            using(var db = new ChoiceVDb()) {
                var dbVeh = db.vehicles
                   .Include(v => v.vehiclestuningbase)
                   .Include(v => v.vehiclestuningmods)
                .FirstOrDefault(v => v.id == vehicle.VehicleId);

                vehicle.updateDbTuning(dbVeh);

                db.SaveChanges();
            }
        }

        public static ChoiceVVehicle createVehicle(uint model, Position position, Rotation rotation, int dimension, byte startColor = 0, bool randomlySpawned = false, string modelName = null) {
            using(var db = new ChoiceVDb()) {
                var configModel = db.configvehiclesmodels.Include(c => c._class).Include(m => m.configvehiclesseats).FirstOrDefault(m => m.Model == (int)model && m.needsRecheck == 0);

                if(configModel == null) {
                    var testVehicle = ChoiceVAPI.CreateVehicle(model, position, rotation);
                    InvokeController.AddTimedInvoke("NewModelCreator", (i) => {
                        if(testVehicle != null) {
                            initModelCreation(ChoiceVAPI.FindNearbyPlayer(position, 50), testVehicle, modelName);
                            InvokeController.AddTimedInvoke("NewModelCreator", (i) => {
                                ChoiceVAPI.RemoveVehicle(testVehicle);
                                createVehicle(model, position, rotation, dimension);
                            }, TimeSpan.FromSeconds(4), false);
                        }
                    }, TimeSpan.FromSeconds(4), false);

                    return null;
                }

                var rnd = new Random();
                var chassisNumber = rnd.Next(111_111_111, 999_999_999);
                while(db.vehicles.FirstOrDefault(v => v.chassisNumber == chassisNumber) != null) {
                    chassisNumber = rnd.Next(111_111_111, 999_999_999);
                }

                var dbVeh = new vehicle {
                    model = configModel,
                    position = position.ToJson(),
                    rotation = rotation.ToJson(),
                    numberPlate = " ",
                    chassisNumber = chassisNumber,
                    dimension = dimension,
                    garageId = null,
                    drivenDistance = 0,
                    fuel = 1,
                    createDate = DateTime.Now,
                    lastMoved = DateTime.Now,
                };

                if(randomlySpawned) {
                    dbVeh.randomlySpawnedDate = DateTime.Now;
                }

                db.vehicles.Add(dbVeh);
                db.SaveChanges();

                var v = ChoiceVAPI.CreateVehicle((uint)configModel.Model, position, rotation);

                if(!randomlySpawned) {
                    v.setInventory(InventoryController.createInventory(dbVeh.id, configModel.InventorySize != -1 ? configModel.InventorySize : configModel._class.InventorySize, InventoryTypes.Vehicle));
                }

                v.init(dbVeh.id, dbVeh.chassisNumber, configModel, configModel.classId, dbVeh.numberPlate, dimension, dbVeh.fuel, dbVeh.drivenDistance, dbVeh.keyLockVersion, dbVeh.dirtLevel, DateTime.Now, DateTime.Now, CompanyController.findCompanyById(dbVeh.registeredCompanyId ?? -1));
                
                v.setVehicleTuning(new VehicleTuning());

                var tuningBase = new vehiclestuningbase {
                    vehicleId = dbVeh.id,
                    modKit = 1,
                };

                db.vehiclestuningbases.Add(tuningBase);

                var damage = new VehicleDamage();
                v.setVehicleDamage(damage, true);

                v.setVehicleColoring(new VehicleColoring(startColor));

                var coloring = new vehiclescoloring {
                    vehicleId = dbVeh.id,
                    primaryColor = startColor,
                    secondaryColor = 0,
                    primaryColorRGB = Rgba.Zero.ToJson(),
                    secondaryColorRGB = Rgba.Zero.ToJson(),
                    pearlColor = 0,
                };

                db.vehiclescolorings.Add(coloring);

                db.SaveChanges();

                EventController.onVehicleMoved(null, v, v.Position, v.Position, WorldController.getGridBlock(v.Position), 0);

                if(randomlySpawned) {
                    var numberPlate = ChoiceVAPI.randomString(6);
                    while(db.vehiclesregistrations.FirstOrDefault(v => v.end == null && v.numberPlate == numberPlate) != null) {
                        numberPlate = ChoiceVAPI.randomString(6);
                    }

                    var newHis = new vehiclesregistration {
                        vehicleId = dbVeh.id,
                        start = DateTime.Now - TimeSpan.FromDays(new Random().Next(0, 3000)),
                        end = null,
                        numberPlate = numberPlate,
                        ownerId = null,
                    };
                    db.vehiclesregistrations.Add(newHis);

                    dbVeh.numberPlate = numberPlate;

                    db.SaveChanges();

                    v.NumberplateText = numberPlate;
                }
                
                if(configModel.specialFlag != 0) {
                    v.SetStreamSyncedMetaData("SPECIAL_FLAG", configModel.specialFlag);
                }                        
                return v;
            }
        }

        public static void removeVehicle(ChoiceVVehicle vehicle) {
            InventoryController.destroyInventory(vehicle.Inventory);

            using(var db = new ChoiceVDb()) {
                var dbVeh = db.vehicles.FirstOrDefault(v => v.id == vehicle.VehicleId);

                if(dbVeh != null) {
                    db.vehicles.Remove(dbVeh);
                    db.SaveChanges();
                }

                despawnVehicle(vehicle);
            }

            using(var fsDb = new ChoiceVFsDb()) {
                var fsVeh = fsDb.executive_vehicle_files.FirstOrDefault(v => v.vehicleId == vehicle.VehicleId);
                if(fsVeh != null) {
                    fsDb.executive_vehicle_files.Remove(fsVeh);
                    fsDb.SaveChanges();
                }
            }
        }

        public static void removeVehicle(vehicle vehicle) {
            InventoryController.destroyInventory(vehicle);
            using(var db = new ChoiceVDb()) {
                var entry = db.vehicles.Find(vehicle.id);
                if(entry != null) {
                    db.vehicles.Remove(entry);
                    db.SaveChanges();
                }
            }
            
            using(var fsDb = new ChoiceVFsDb()) {
                var fsVeh = fsDb.executive_vehicle_files.FirstOrDefault(v => v.vehicleId == vehicle.id);
                if(fsVeh != null) {
                    fsDb.executive_vehicle_files.Remove(fsVeh);
                    fsDb.SaveChanges();
                }
            }
        }

        public static void setNumberPlateText(ChoiceVVehicle vehicle, string text) {
            vehicle.NumberplateText = text;

            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("SET_NUMBERPLATE", vehicle, text);
            }
            using(var db = new ChoiceVDb()) {
                var dbVeh = db.vehicles.FirstOrDefault(v => v.id == vehicle.VehicleId);

                if(dbVeh != null) {
                    dbVeh.numberPlate = text;
                    db.SaveChanges();
                }
            }
        }

        public static configvehiclesmodel initModelCreation(IPlayer player, ChoiceVVehicle testVehicle, string modelName = null) {
            if(player == null) {
                return null;
            }

            configvehiclesmodel model = null;

            CallbackController.getVehicleInfo(player, testVehicle, (p, veh, cl, pos1, pos2, extras, seats, seatsList, windows, doors, tyres, displayName) => {
                using(var db = new ChoiceVDb()) {
                    var modelId = -1;

                    var model = db.configvehiclesmodels.Include(v => v.configvehiclesseats).FirstOrDefault(m => m.Model == (int)veh.Model);
                    if(model != null) {
                        model.createDate = DateTime.Now;
                        model.Model = (int)veh.Model;
                        model.ModelName = modelName != null ? modelName : ((VehicleModel)veh.Model).ToString();
                        model.classId = cl;

                        model.windowCount = windows;
                        model.doorCount = doors;
                        model.tyreCount = tyres;

                        var alreadySeats = model.configvehiclesseats.ToArray();
                        db.configvehiclesseats.RemoveRange(alreadySeats);
                        db.SaveChanges();
                    } else {
                        model = new configvehiclesmodel {
                            createDate = DateTime.Now,
                            Model = (int)veh.Model,
                            ModelName = ((VehicleModel)veh.Model).ToString(),
                            DisplayName = displayName,
                            classId = cl,
                            InventorySize = 0,
                            FuelMax = -1,
                            FuelType = -1,
                            StartPoint = pos1.ToJson(),
                            EndPoint = pos2.ToJson(),
                            PowerMultiplier = 0,
                            Extras = extras.ToJson(),
                            Seats = seats,
                            needsRecheck = 1,

                            windowCount = windows,
                            doorCount = doors,
                            tyreCount = tyres,
                        };

                        db.configvehiclesmodels.Add(model);
                        db.SaveChanges();
                    }

                    modelId = model.id;
                    for(int i = 0; i < seatsList.Count(); i++) {
                        var worldPos = seatsList[i];
                        var rotatedWorldPos = ChoiceVAPI.rotatePointInRect(seatsList[i].X, seatsList[i].Y, veh.Position.X, veh.Position.Y, -veh.Rotation.Yaw);
                        var relativeRotatedWorldPos = new Vector2(rotatedWorldPos.X - veh.Position.X, rotatedWorldPos.Y - veh.Position.Y);
                        var dbSeat = new configvehiclesseat {
                            SeatNr = i,
                            createDate = DateTime.Now,
                            ModelId = modelId,
                            SeatPos = relativeRotatedWorldPos.ToJson(),
                        };

                        db.configvehiclesseats.Add(dbSeat);
                    }

                    model.needsRecheck = 0;

                    db.SaveChanges();
                }
            });

            return model;
        }

        public static ChoiceVVehicle spawnVehicle(vehicle vehicle) {
            if(vehicle.garageId == null) {
                var v = ChoiceVAPI.CreateVehicle((uint)vehicle.model.Model, vehicle.position.FromJson(), vehicle.rotation.FromJson<Rotation>());

                v.init(vehicle.id, vehicle.chassisNumber, vehicle.model, vehicle.model.classId, vehicle.numberPlate, vehicle.dimension, vehicle.fuel, vehicle.drivenDistance, vehicle.keyLockVersion, vehicle.dirtLevel, vehicle.randomlySpawnedDate, vehicle.lastMoved, CompanyController.findCompanyById(vehicle.registeredCompanyId ?? -1));
                
                var tuning = VehicleTuning.fromDb(vehicle.vehiclestuningbase, vehicle.vehiclestuningmods.ToList());
                v.setVehicleTuning(tuning);

                var coloring = VehicleColoring.fromDb(vehicle.vehiclescoloring);
                v.setVehicleColoring(coloring);

                var dmg = new VehicleDamage();
                dmg.fillFromDb(vehicle);
                //(vehicle.vehiclesdamagebasis, vehicle.vehiclesdamageparts.ToList(), vehicle.vehiclesdamagetyres.ToList(), vehicle.vehiclesdamagesimpleparts.ToList(), vehicle.vehiclesdamagebumpers.ToList());
                v.setVehicleDamage(dmg);

                foreach(var data in vehicle.vehiclesdata) {
                    v.setData(data.name, data.value);
                    if(AllOnSpawnDataInits.ContainsKey(data.name)) {
                        AllOnSpawnDataInits[data.name].Invoke(v, data);
                    }
                }


                foreach(var part in VehicleMotorCompartmentController.getAllDamagablePartIdentifiers()) {
                    if(v.hasData(part)) {
                        v.setCompartmentPartDamage(part, float.Parse(v.getData(part)), false);
                    }
                }

                VehicleMotorCompartmentController.onSpawnVehicle(v);
                
                v.setInventory(InventoryController.loadInventory(vehicle));
                
                EventController.onVehicleMoved(null, v, v.Position, v.Position, WorldController.getGridBlock(v.Position), 0);
                
                if(vehicle.model.specialFlag != 0) {
                    v.SetStreamSyncedMetaData("SPECIAL_FLAG", vehicle.model.specialFlag);
                } 
                return v;
            } else {
                return null;
            }
        }

        public async static void despawnVehicle(ChoiceVVehicle vehicle) {
            ChoiceVAPI.emitClientEventToAll("VEHICLE_DELETE", vehicle);

            await EventController.onVehicleMoved(null, vehicle, Position.Zero, Rotation.Zero, WorldController.getGridBlock(vehicle.Position), 0);

            ChoiceVAPI.RemoveVehicle(vehicle);
        }

        public static configvehiclesmodel getVehicleModelById(int modelId) {
            if(AllModels.ContainsKey(modelId)) {
                return AllModels[modelId];
            } else {
                return null;
            }
        }
        
        public static configvehiclesmodel getVehicleModelByName(string modelName) {
            return AllModels.Values.FirstOrDefault(m => m.ModelName.ToLower() == modelName.ToLower());
        }

        public static List<configvehiclesmodel> getAllVehicleModels() {
            return AllModels.Values.ToList();
        }

        public static configvehiclesclass getVehicleClassById(int classId) {
            if(AllClasses.ContainsKey(classId)) {
                return AllClasses[classId];
            } else {
                return null;
            }
        }

        public static List<configvehiclesclass> getAllVehicleClasses() {
            return AllClasses.Values.ToList();
        }

        public static configvehiclesclass getVehicleClass(Predicate<configvehiclesclass> predicate) {
            return AllClasses.Values.FirstOrDefault(c => predicate(c));
        }

        public static string getVehicleClassName(int id) {
            if(AllClasses.ContainsKey(id)) {
                return AllClasses[id].ClassName;
            } else {
                return "Unbekannt";
            }
        }

        private MenuItem givePlayerInVehicleGenerator(ChoiceVVehicle vehicle, IPlayer player, string name) {
            var arr = getVehiclePassengerList(vehicle, player, name);

            return new ListMenuItem($"{name} etwas geben", $"", arr.ToArray(), "VEHICLE_GIVE_PASSENGER_ITEM", MenuItemStyle.normal, true).withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } });
        }

        public static List<string> getVehiclePassengerList(ChoiceVVehicle vehicle, IPlayer ignore, string name) {
            var arr = new List<string>();

            for(var i = 0; i < vehicle.PassengerList.Count; i++) {
                if(vehicle.PassengerList.Values.ToList()[i] != ignore) {
                    arr.Add($"{name} {vehicle.PassengerList.Keys.ToList()[i] + 1}");
                }
            }

            return arr;
        }

        public static List<string> getVehicleEmptySeatsList(ChoiceVVehicle vehicle, string name) {
            var arr = new List<string>();
            foreach(var seat in vehicle.DbModel.configvehiclesseats.OrderBy(s => s.SeatNr)) {
                if(!vehicle.PassengerList.ContainsKey(seat.SeatNr ?? -1)) {
                    arr.Add($"{name} {seat.SeatNr + 1}");
                }
            }

            return arr;
        }

        public static bool hasVehicleSpecialFlag(ChoiceVVehicle vehicle, SpecialVehicleModelFlag flag) {
            return ((SpecialVehicleModelFlag)vehicle.DbModel.specialFlag).HasFlag(flag);
        }

        //private ChoiceVVehicle findVehiclePlayerIsLookingAt(IPlayer player, Rotation rotation, float maxDistance, Predicate<ChoiceVVehicle> otherConditions) {
        //    var vehicles = ChoiceVAPI.GetAullVehicles().Where(v => v.Position.Distance(player.Position) < maxDistance && otherConditions(v)).ToList();
        //    float nearestDistance = float.MaxValue;
        //    if(vehicles.Count > 0) {
        //        ChoiceVVehicle nearestVehicle = null;
        //        foreach(var vehicle in vehicles) {

        //            for(var i = 0f; i < maxDistance; i += 0.33f) {
        //                var forPos = ChoiceVAPI.getPositionInFront(player.Position, player.Rotation, i, true);

        //                var dist = vehicle.Position.Distance(forPos);
        //                if(dist < nearestDistance) {
        //                    nearestDistance = dist;
        //                    nearestVehicle = vehicle;
        //                }
        //            }

        //            //if(vehicle.DbModel != null) {
        //            //    // Get player position
        //            //    Vector3 ppo = player.Position;

        //            //    // Get date from vehicle model
        //            //    int sea = vehicle.DbModel.Seats;
        //            //    Vector3 pos1 = vehicle.DbModel.StartPoint.FromJson();
        //            //    Vector3 pos2 = vehicle.DbModel.EndPoint.FromJson();

        //            //    // Get position and rotation of vehicle
        //            //    Vector3 vpo = vehicle.Position;
        //            //    float rot = vehicle.Rotation.Yaw;

        //            //    // Get world coordinates of all four corners
        //            //    float x1 = vpo.X + pos1.X;
        //            //    float y1 = vpo.Y + pos1.Y;
        //            //    float z1 = vpo.Z + pos1.Z;
        //            //    float x2 = vpo.X + pos2.X;
        //            //    float y2 = vpo.Y + pos2.Y;
        //            //    float z2 = vpo.Z + pos2.Z;

        //            //    // Rotate corners in world
        //            //    Vector2 a = ChoiceVAPI.rotatePointInRect(x1, y1, vpo.X, vpo.Y, rot); // RightFront
        //            //    Vector2 b = ChoiceVAPI.rotatePointInRect(x2, y1, vpo.X, vpo.Y, rot); // RightBack
        //            //    Vector2 c = ChoiceVAPI.rotatePointInRect(x2, y2, vpo.X, vpo.Y, rot); // LeftBack
        //            //    Vector2 d = ChoiceVAPI.rotatePointInRect(x1, y2, vpo.X, vpo.Y, rot); // LeftFront

        //            //    // Rotate view points in world
        //            //    Vector2 e = ChoiceVAPI.rotatePointInRect(vpo.X, y2 - (pos2.Y / 4), vpo.X, vpo.Y, rot); // Front
        //            //    Vector2 f = ChoiceVAPI.rotatePointInRect(vpo.X, y1 + (pos2.Y / 4), vpo.X, vpo.Y, rot); // Back

        //            //    // Check distance to all view points
        //            //    float d1 = ChoiceVAPI.checkDistanceV2(ppo.X, ppo.Y, e.X, e.Y);     // Distance front
        //            //    float d2 = ChoiceVAPI.checkDistanceV2(ppo.X, ppo.Y, vpo.X, vpo.Y); // Distance center
        //            //    float d3 = ChoiceVAPI.checkDistanceV2(ppo.X, ppo.Y, f.X, f.Y);     // Distance back

        //            //    // Get minimal distance
        //            //    List<float> _min = new List<float> { d1, d2, d3 };
        //            //    float min = _min.Min() > 5f ? 5f : _min.Min();

        //            //    // Get maximal distance
        //            //    List<float> _max = new List<float> { d1, d2, d3 };
        //            //    float max = _max.Max() > 5f ? 5f : _max.Max();

        //            //    // Get min and max vector of players current viewpoint
        //            //    Vector3 pif1 = ChoiceVAPI.getPositionInFront(ppo, rotation, min);
        //            //    Vector3 pif2 = ChoiceVAPI.getPositionInFront(ppo, rotation, max);
        //            //    if((ChoiceVAPI.checkPointInRect(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y, pif1.X, pif1.Y) || ChoiceVAPI.checkPointInRect(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y, pif2.X, pif2.Y)) && ppo.Z < (vpo.Z + 2f) && ppo.Z > (vpo.Z - 3f)) {
        //            //        return vehicle;
        //            //    }
        //            //}
        //        }
        //        return nearestVehicle;
        //    }

        //    return null;
        //}

        #region CustomEvents

        /// <summary>
        /// Adds an Predicate that returns an inv if another inv shall be opened instead of the player one when opening the vehicle trunk
        /// </summary>
        public static void addTrunkOpenInject(InjectInventoryPredicateDelegate inject) {
            AllTrunkOpenInjects.Add(inject);
        }

        private bool onVehicleTrunk(IPlayer player, ConsoleKey key, string eventName) {
            var vehicles = ChoiceVAPI.FindNearbyVehicles(player, 3).ToList();
            foreach(var veh in vehicles) {
                var length = Math.Abs(-veh.DbModel.StartPoint.FromJson().Y + veh.DbModel.EndPoint.FromJson().Y);
                var backPos = ChoiceVAPI.getPositionInFront(veh.Position, veh.Rotation, -(length / 2) * 1.2f, true);
                if(player.Position.Distance(backPos) < 1.5f && veh.LockState == VehicleLockState.Unlocked) {
                    var inv = player.getInventory();

                    foreach(var el in AllTrunkOpenInjects) {
                        var retInv = el.Invoke(player);
                        if(retInv != null && retInv != veh.Inventory) {
                            inv = retInv;
                            player.sendNotification(NotifactionTypes.Info, "Ein anderes Inventar als dein eigenes wurde geöffnet", "Anderes Inventar geöffnet", NotifactionImages.Package);
                        }
                    }

                    InventoryController.showMoveInventory(player, inv, veh.Inventory, null, (p) => {
                        veh.setTrunkState(false);
                    });

                    veh.setTrunkState(true);

                    return true;
                }
            }

            return false;
        }

        private bool onVehicleLock(IPlayer player, ConsoleKey key, string eventName) {
            var veh = ChoiceVAPI.FindNearbyVehicle(player, 7.5f, v => player.getCharacterData().AdminMode || v.hasPlayerAccess(player));
            if(veh != null) {
                if(veh.LockState == VehicleLockState.Locked) {
                    unlockVehicle(veh, player);
                } else {
                    lockVehicle(veh, player);
                }
                return true;
            }

            return false;
        }

        public static void unlockVehicle(ChoiceVVehicle veh, IPlayer informPlayer = null) {
            veh.LockState = VehicleLockState.Unlocked;

            SoundController.playSoundAtCoords(veh.Position, 15, SoundController.Sounds.CarUnlock, 0.75f);
            if (informPlayer != null) {
                informPlayer.sendNotification(Constants.NotifactionTypes.Success, "Fahrzeug aufgeschlossen!", "Fahrzeug aufgeschlossen", Constants.NotifactionImages.Car, "CAR_LOCKING");
                informPlayer.emitClientEvent("VEHICLE_LOCKSTATE_INDICATOR", veh);
            }
        }

        public static void lockVehicle(ChoiceVVehicle veh, IPlayer informPlayer = null) {
            veh.LockState = VehicleLockState.Locked;
            SoundController.playSoundAtCoords(veh.Position, 15, SoundController.Sounds.CarLock, 0.75f);

            if (informPlayer != null) {
                informPlayer.sendNotification(Constants.NotifactionTypes.Warning, "Fahrzeug zugeschlossen!", "Fahrzeug zugeschlossen", Constants.NotifactionImages.Car, "CAR_LOCKING");
                informPlayer.emitClientEvent("VEHICLE_LOCKSTATE_INDICATOR", veh);
            }
        }

        private bool onVehicleEngine(IPlayer player, ConsoleKey key, string eventName) {
            if (player.IsInVehicle && player.Vehicle.Driver == player && (player.getCharacterData().AdminMode || ((ChoiceVVehicle)player.Vehicle).hasPlayerAccess(player, true)) && ((ChoiceVVehicle)player.Vehicle).VehicleClass != VehicleClassesDbIds.Cycles) {
                var vehicle = (ChoiceVVehicle)player.Vehicle;

                var anim = AnimationController.getAnimationByName(vehicle.DbModel.engineStartAnimNavigation?.identifier);
                AnimationController.animationTask(player, anim, () => {
                    if(vehicle.EngineOn) {
                        stopVehicleEngine(vehicle, player);
                    } else {
                        startVehicleEngine(vehicle, player);
                    }
                });
                return true;
            }

            return false;
        }

        public static void startVehicleEngine(ChoiceVVehicle vehicle, IPlayer informPlayer = null) {
            if (vehicle.CanDrive) {
                if (!vehicle.EngineOn) {
                    if (vehicle.FuelType != FuelType.Electricity) {
                        if (vehicle.DbModel.classId != 8) {
                            if (new Random().NextDouble() <= 0.001) {
                                if (informPlayer != null) {
                                    SoundController.playSoundAtCoords(informPlayer.Position, 7.5f, SoundController.Sounds.EngineOnSpecial, 0.15f, "wav");
                                    informPlayer.sendNotification(NotifactionTypes.Info, "Achievment freigeschaltet: \"Was ist den mit meinem Auto los?\"", "Achievment freigeschaltet", NotifactionImages.Car);
                                }
                            } else {
                                SoundController.playSoundAtCoords(informPlayer.Position, 7.5f, SoundController.Sounds.EngineOn, 0.15f, "mp3");
                            }
                        } else {
                            SoundController.playSoundAtCoords(vehicle.Position, 7.5f, SoundController.Sounds.BikeEngineOn, 0.1f, "mp3");
                        }
                    }
                }
                vehicle.EngineOn = true;
            } else {
                informPlayer?.sendBlockNotification("Der Motor reagiert nicht!", "Motor regiert nicht", Constants.NotifactionImages.Car);
            }
        }

        public static void stopVehicleEngine(ChoiceVVehicle vehicle, IPlayer informPlayer = null) {
            vehicle.EngineOn = false;
        }

        private bool onVehicleSeatbelt(IPlayer player, ConsoleKey key, string eventName) {
            if(player.IsInVehicle) {
                player.GetStreamSyncedMetaData("VEHICLE_SEATBELT", out bool seatbelt);
                player.SetStreamSyncedMetaData("VEHICLE_SEATBELT", !seatbelt);
                if(!seatbelt) {
                    SoundController.playSoundAtCoords(player.Position, 1f, SoundController.Sounds.SeatbeltOn, 0.4f);
                } else {
                    SoundController.playSoundAtCoords(player.Position, 1f, SoundController.Sounds.SeatbeltOff, 0.4f);
                }
                return false;
            }

            return false;
        }

        public Menu getSupportVehicleInfo(IEntity sender, IEntity target) {
            var player = sender as IPlayer;
            var vehicle = target as ChoiceVVehicle;

            var model = vehicle.DbModel;

            var data = new Dictionary<string, dynamic> {
                { "Vehicle", vehicle }
            };

            var menu = new Menu($"{model.ModelName} Editor", "Verändere die Backend Model Infos");
            menu.addMenuItem(new InputMenuItem("Reifenanzahl", "Gib die Reifenanzahl an", $"{model.tyreCount}", ""));
            menu.addMenuItem(new InputMenuItem("Fensteranzahl", "Gib die Fensteranzahl an", $"{model.windowCount}", ""));
            menu.addMenuItem(new InputMenuItem("Autohaus Bestellpreis", "Gib an wieviel ein Autohaus zahlen muss", $"{model.price}", ""));
            menu.addMenuItem(new InputMenuItem("Power-Multiplikator", "Gib den Powermultiplikator des Models an", $"{model.PowerMultiplier}", ""));
            menu.addMenuItem(new CheckBoxMenuItem("Auto kaufbar", "Gib an ob das Fahrzeug gekauft werden kann", model.useable == 1, ""));
            menu.addMenuItem(new CheckBoxMenuItem("Kofferraum hinten", "Hat das Fahrzeug den Kofferraumm hinten", model.trunkInBack == 1, ""));
            menu.addMenuItem(new CheckBoxMenuItem("Fahrzeug hat Kennzeichen", "Gib an ob das Fahrzeug Nummernkennzeichen unterstützt", model.hasNumberplate == 1, ""));
            menu.addMenuItem(new InputMenuItem("Markenname", "Gib den Markennamen des Fahrzeug an", $"{model.producerName}", ""));
            menu.addMenuItem(new InputMenuItem("Kofferaumgröße (kg)", "Überschreibe die Kofferraumgröße der Klasse", $"{model.InventorySize}kg", ""));
            menu.addMenuItem(new InputMenuItem("Benzintyp anders als Klasse", "0: Nix, 1: Benzin, 2: Diesel, 3: Strom, 4: Kerosin", $"{model.FuelType}", "", MenuItemStyle.yellow));
            menu.addMenuItem(new InputMenuItem("Tankgröße anders als Klasse", "Ändere die Tankgröße für das Model", $"{model.FuelMax}", "", MenuItemStyle.yellow));

            menu.addMenuItem(new MenuStatsMenuItem("Einstellung speichern", "Speichere die oben eingestellten Optionen", "FINISH_VEHICLE_MODEL_INTERACTION", MenuItemStyle.green).withData(data));

            return menu;
        }

        private bool finishVehicleInfoInteraction(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["Vehicle"];

            if(vehicle != null) {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
                var tyreEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var windowEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
                var priceEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
                var powerEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
                var useableEvt = evt.elements[4].FromJson<CheckBoxMenuItemEvent>();
                var trunkEvt = evt.elements[5].FromJson<CheckBoxMenuItemEvent>();
                var numberPlateEvt = evt.elements[6].FromJson<CheckBoxMenuItemEvent>();
                var producerEvt = evt.elements[7].FromJson<InputMenuItemEvent>();
                var invSizeEvt = evt.elements[8].FromJson<InputMenuItemEvent>();
                var fuelTypeEvt = evt.elements[9].FromJson<InputMenuItemEvent>();
                var fuelMaxEvt = evt.elements[10].FromJson<InputMenuItemEvent>();

                try {
                    var tyreCount = int.Parse(tyreEvt.input ?? "0");
                    var windowCount = int.Parse(windowEvt.input ?? "0");
                    var price = decimal.Parse(priceEvt.input ?? "1000000");
                    var powerMult = float.Parse(powerEvt.input ?? "1");
                    var invSize = int.Parse(invSizeEvt.input ?? "-1");
                    var fuelType = int.Parse(fuelTypeEvt.input ?? "-1");
                    var fuelMax = int.Parse(fuelMaxEvt.input ?? "-1");

                    using(var db = new ChoiceVDb()) {
                        var model = db.configvehiclesmodels.FirstOrDefault(m => m.id == vehicle.DbModel.id);
                        model.tyreCount = tyreCount;
                        model.windowCount = windowCount;
                        model.price = price;
                        model.PowerMultiplier = powerMult;
                        model.useable = useableEvt.check ? 1 : 0;
                        model.trunkInBack = trunkEvt.check ? 1 : 0;
                        model.producerName = producerEvt.input;
                        model.InventorySize = invSize;
                        model.FuelType = fuelType;
                        model.FuelMax = fuelMax;
                        model.hasNumberplate = numberPlateEvt.check ? 1 : 0;

                        db.SaveChanges();

                        vehicle.DbModel = model;

                        player.sendNotification(Constants.NotifactionTypes.Success, "Backend Info überarbeitet! Der Server muss neustarten um die Änderungen zu übernehmen!", "Server neustarten", Constants.NotifactionImages.Car);
                    }

                } catch(Exception) {
                    player.sendBlockNotification("Eine Texteingabe war nicht im richtigen Format!", "Falsche Eingabe", Constants.NotifactionImages.Car);
                }
            } else {
                player.sendBlockNotification("Fahrzeug wurde nicht gefunden!", "Fahrzeug weg", Constants.NotifactionImages.Car);
            }

            return true;
        }

        private List<Position> getSeatListToWorld(ChoiceVVehicle veh) {
            var list = new List<Position>();
            var min = veh.DbModel.StartPoint.FromJson();
            var max = veh.DbModel.EndPoint.FromJson();
            foreach(var seat in veh.DbModel.configvehiclesseats.OrderBy(s => s.SeatNr)) {
                var seatPos = seat.SeatPos.FromJson();
                var vec2 = ChoiceVAPI.rotatePointInRect(seatPos.X + veh.Position.X, seatPos.Y + veh.Position.Y, veh.Position.X, veh.Position.Y, veh.Rotation.Yaw);
                list.Add(new Position(vec2.X, vec2.Y, seatPos.Z + veh.Position.Z));
            }

            return list;
        }

        private static Position getSeatPosition(ChoiceVVehicle veh, int seatId) {
            var min = veh.DbModel.StartPoint.FromJson();
            var max = veh.DbModel.EndPoint.FromJson();

            var seat = veh.DbModel.configvehiclesseats.FirstOrDefault(s => s.SeatNr == seatId);
            if(seat != null) {
                var seatPos = seat.SeatPos.FromJson();
                var vec2 = ChoiceVAPI.rotatePointInRect(seatPos.X + veh.Position.X, seatPos.Y + veh.Position.Y, veh.Position.X, veh.Position.Y, veh.Rotation.Yaw);
                return new Position(vec2.X, vec2.Y, seatPos.Z + veh.Position.Z);
            } else {
                return Position.Zero;
            }
        }

        private bool onVehicleEnterPassengerKeyPress(IPlayer player, ConsoleKey key, bool isPressed, string eventName) {
            if(!player.getBusy() || player.hasState(PlayerStates.Handcuffed)) {
                player.emitClientEvent("REQUEST_VEHICLE_ENTER", 2, isPressed);
            }
            return true;
    }

        private bool onVehicleEnterPassenger(IPlayer player, string eventName, object[] args) {
            if(player.IsInVehicle || player.getBusy()) {
                return false;
            }

            if(player.hasData("BLOCK_NEXT_G")) {
                if(((DateTime)player.getData("BLOCK_NEXT_G")) > DateTime.Now) {
                    player.resetData("BLOCK_NEXT_G");
                    return false;
                }
                player.resetData("BLOCK_NEXT_G");
            }


            var pressed = (bool)args[0];

            //Vector3 camera = args[1].ToJson().FromJson<Vector3>();

            var veh = ChoiceVAPI.FindNearbyVehicle(player, 3, v => v.LockState == VehicleLockState.Unlocked, true);
            if(veh != null) {
                if(pressed) {
                    player.emitClientEvent("PLAYER_SHOW_SEATS", true, getSeatListToWorld(veh), -veh.DbModel.StartPoint.FromJson().Z + veh.DbModel.EndPoint.FromJson().Z);
                    return true;
                } else {
                    player.emitClientEvent("PLAYER_SHOW_SEATS", false);
                }

                var seats = args.Skip(2).ToArray();
                for(var i = 0; i < seats.Count(); i++) {
                    if((bool)seats[i] && !veh.PassengerList.ContainsKey(i)) {
                        player.setData("BLOCK_NEXT_G", DateTime.Now + TimeSpan.FromSeconds(1));
                        player.emitClientEvent("ENTER_VEHICLE", veh, i);
                        return true;
                    }
                }

                var seatList = getSeatListToWorld(veh);

                int nearestSeatIdx = -1;
                var nearestDist = float.MaxValue;
                for(var i = 1; i < seatList.Count; i++) {
                    var seatPos = seatList[i];
                    var playerPos = player.Position;

                    var dist = seatPos.Distance(playerPos);

                    if(!veh.PassengerList.ContainsKey(i) && dist < nearestDist) {
                        nearestDist = dist;
                        nearestSeatIdx = i;
                    }
                }

                if(nearestSeatIdx != -1) {
                    player.setData("BLOCK_NEXT_G", DateTime.Now + TimeSpan.FromSeconds(1));
                    player.emitClientEvent("ENTER_VEHICLE", veh, nearestSeatIdx - 1);
                } else {
                    player.sendBlockNotification("Kein Platz im Fahrzeug gefunden", "Kein Platz gefunden", Constants.NotifactionImages.Car);
                }
            } else {
                player.emitClientEvent("PLAYER_SHOW_SEATS", false);
                player.sendBlockNotification("Kein zugängliches Fahrzeug in der Nähe gefunden", "Kein Fahrzeug gefunden", Constants.NotifactionImages.Car);
            }

            return true;
        }

        private bool onVehicleEnterDriverKeyPress(IPlayer player, ConsoleKey key, string eventName) {
            player.emitClientEvent("REQUEST_VEHICLE_ENTER", 1);
            return true;
        }

        private bool onVehicleEnterDriver(IPlayer player, string eventName, object[] args) {
            if(player.IsInVehicle || player.getBusy()) {
                return false;
            }

            if(player.hasData("VEHICLE_REENTER_BLOCK") && (DateTime)player.getData("VEHICLE_REENTER_BLOCK") > DateTime.Now) {
                return false;
            }

            player.resetData("VEHICLE_REENTER_BLOCK");

            var veh = ChoiceVAPI.FindNearbyVehicle(player, 3, v => v.LockState == VehicleLockState.Unlocked && v.AttachedTo == null, true);
            if(veh != null) {
                if(veh.Driver == null) {
                    player.emitClientEvent("ENTER_VEHICLE", veh, -1);
                } else {
                    player.sendBlockNotification("Das Fahrzeug hat schon einen Fahrer", "Fahrersitz belegt!", Constants.NotifactionImages.Car);
                }
            }

            return true;
        }

        private bool onVehicleGivePassengerItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;

            var vehicle = data["Vehicle"] as ChoiceVVehicle;

            var seatId = int.Parse(evt.currentElement.ToCharArray().Last().ToString()) - 1;

            if(evt.action == "changed") {
                showPlayerPassengerPosition(player, vehicle, seatId);
            } else {
                var passenger = vehicle.PassengerList[seatId];

                if(passenger != null) {
                    InventoryController.showInventory(player, player.getInventory(), false, passenger.getCharacterId());
                }
            }

            return true;
        }

        public static void showPlayerPassengerPosition(IPlayer player, ChoiceVVehicle vehicle, int seatId) {
            player.emitClientEvent("VEHICLE_SHOW_PASSENGER", vehicle, seatId);
        }

        public static void showPlayerSeatPosition(IPlayer player, ChoiceVVehicle vehicle, int seatId) {
            var seatPos = getSeatPosition(vehicle, seatId);

            player.emitClientEvent("VEHICLE_SHOW_SEAT", seatPos);
        }


        private bool onVehicleToggleHeadlights(IPlayer player, ConsoleKey key, string eventName) {
            if(player.IsInVehicle && player.Vehicle.Driver == player) {
                var vehicle = (ChoiceVVehicle)player.Vehicle;

                vehicle.tryToggleLight();

                SoundController.playSoundInVehicle(vehicle, SoundController.Sounds.HeadlightSwitch, 0.15f, "mp3");
                
                return false;
            }

            return false;
        }


        #endregion

        #region AltV Events

        private void onPlayerEnterVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if(vehicle.VehicleClass == VehicleClassesDbIds.Cycles) {
                vehicle.EngineOn = true;
            }

            try {
                vehicle.PassengerList[seatId - 1] = player;
                vehicle.verylongTickUpdate();
            } catch(Exception e) {
                Logger.logException(e, "Error in veryLongTick");
            }

            var posStr = seatId - 1 == 0 ? "Fahrer" : "Mitfahrer";
            CamController.checkIfCamSawAction(player, "Fahrzeug betreten", $"Person betritt Fahrzeug ({vehicle.DbModel?.ModelName}:{vehicle.NumberplateText}) als {posStr}");
        }

        private void onPlayerChangeSeat(IPlayer player, ChoiceVVehicle vehicle, byte oldSeatId, byte newSeatId) {
            try {
                vehicle.PassengerList[oldSeatId - 1] = null;
                vehicle.PassengerList[newSeatId - 1] = player;
                vehicle.verylongTickUpdate();
            } catch(Exception e) {
                Logger.logException(e, "Error in veryLongTick");
            }
        }

        public class RemoveVehicleHudCefEvent : IPlayerCefEvent {
            public string Event { get; }

            public RemoveVehicleHudCefEvent() {
                Event = "REMOVE_CAR_HUD";
            }
        }

        private void onPlayerExitVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            player.setData("VEHICLE_REENTER_BLOCK", DateTime.Now + TimeSpan.FromSeconds(3));
            saveVehicle(vehicle);
            player.emitCefEventNoBlock(new RemoveVehicleHudCefEvent());
            player.SetStreamSyncedMetaData("VEHICLE_SEATBELT", false);
            vehicle.PassengerList.RemoveWhere(i => i.Value == player);

            CamController.checkIfCamSawAction(player, "Fahrzeug verlassen", $"Person verlässt Fahrzeug ({vehicle.DbModel?.ModelName}:{vehicle.NumberplateText})");

            if(vehicle.Inventory.hasItem<VehicleKey>(k => k.worksForCar(vehicle))) {
                player.sendNotification(NotifactionTypes.Warning, "Es befinden sich noch passende Schlüssel im Kofferraum. Dadurch kann jeder das Fahrzeug starten!", "Schlüssel im Kofferraum", NotifactionImages.Car);
            }
        }

        private void onPlayerDisconnected(IPlayer player, string reason) {
            if(player.IsInVehicle && player.Vehicle != null) {
                var vehicle = (ChoiceVVehicle)player.Vehicle;
                saveVehicle(vehicle);
                vehicle.PassengerList.RemoveWhere(i => i.Value == player);
            }
        }

        private void onVehicleDamage(IVehicle target, IEntity attacker, uint bodyHealthDamage, uint additionalBodyHealthDamage, uint engineHealthDamage, uint petrolTankDamage, uint weaponHash) {
            saveVehicle(target as ChoiceVVehicle);
        }

        private void onVehicleDestroy(IVehicle vehicle) {
            InvokeController.AddTimedInvoke("Vehicle_Respawner", (i) => {
                var veh = vehicle as ChoiceVVehicle;
                onVehicleDestroyed(veh);
            }, TimeSpan.FromSeconds(10), false);
        }

        public static void onVehicleDestroyed(ChoiceVVehicle vehicle) {
            //vehicle.Repair();
            
            //if(vehicle.NetworkOwner != null) {
            //     vehicle.NetworkOwner.emitClientEvent("VEHICLE_REPAIRED", vehicle);
            //}

            //InvokeController.AddTimedInvoke("Vehicle_Damager", (i) => {
                vehicle.VehicleDamage.destroyEverything(vehicle);

                VehicleMotorCompartmentController.destroyEverything(vehicle);
            //}, TimeSpan.FromSeconds(5), false);
        }



        //private bool onExplosion(IPlayer player, AltV.Net.Data.ExplosionType explosionType, Position position, uint explosionFx, IEntity targetEntity) {
        //    if(targetEntity is ChoiceVVehicle && (explosionType == AltV.Net.Data.ExplosionType.Car || explosionType == AltV.Net.Data.ExplosionType.Boat || explosionType == AltV.Net.Data.ExplosionType.Plane || explosionType == AltV.Net.Data.ExplosionType.Bike || explosionType == AltV.Net.Data.ExplosionType.Blimp)) {
        //        InvokeController.AddTimedInvoke("Vehicle_Respawner", (i) => {
        //            var vehicle = targetEntity as ChoiceVVehicle;
        //            vehicle.VehicleDamage.destroyEverything(vehicle);
        //            vehicle.reapplyDamage();
        //        }, TimeSpan.FromSeconds(20), false);
        //        return false;
        //    }

        //    return true;
        //}

        #endregion

        private bool removeVehicleInteraction(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicleId = (int)data["InteractionTargetId"];
            var vehicle = ChoiceVAPI.FindVehicleById(vehicleId);

            if(vehicle != null) {
                removeVehicle(vehicle);
            }

            return true;
        }

        private bool onSupportRepairVehicle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicleId = (int)data["InteractionTargetId"];
            var vehicle = ChoiceVAPI.FindVehicleById(vehicleId);

            vehicle.Repair();
            vehicle.repairAllDamages();
            VehicleMotorCompartmentController.repairEverything(vehicle);

            return true;
        }
    }
}
