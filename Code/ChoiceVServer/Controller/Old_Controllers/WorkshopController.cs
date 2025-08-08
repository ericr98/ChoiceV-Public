//using System;
//using System.Linq;
//using System.Collections.Generic;
//using AltV.Net.Data;
//using AltV.Net.Enums;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using ChoiceVServer.Model.Vehicle;
//using ChoiceVServer.Model.Color;
//using ChoiceVServer.Model.Company;
//using static ChoiceVServer.Base.Constants;
//using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
//using ChoiceVServer.Companies;
//using System.Drawing;

//namespace ChoiceVServer.Controller {

//    class Workshop {
//        public int Id;
//        public string Name;
//        public int Type;
//        public WorkshopOwnerType OwnerType;
//        public int OwnerId;
//        public string OwnerName;

//        public Ped Manager;
//        public int ManagerInventoryId;
//        public Inventory ManagerInventory = null;

//        public Workshop(int id, string name, int workshoptype, WorkshopOwnerType ownerType, int ownerId, string ownerName, Position pos, Rotation rot, int inventoryId) {
//            Id = id;
//            Name = name;
//            Type = workshoptype;

//            OwnerType = ownerType;
//            OwnerId = ownerId;
//            OwnerName = ownerName;

//            Manager = PedController.createPed("s_m_m_gardener_01", pos, rot.Yaw, "WORKSHOP_INVENTORY_SPOT", Id.ToString());
//            ManagerInventory = null;
//            ManagerInventoryId = 0;

//            if (inventoryId == 0) {
//                ManagerInventory = InventoryController.createInventory(-1, 999999, InventoryTypes.InteractSpot);

//                if (ManagerInventory != null) {
//                    ManagerInventoryId = ManagerInventory.Id;

//                    using (var db = new ChoiceVDb()) {
//                        var row = db.configworkshops.FirstOrDefault(i => i.id == Id);
//                        if (row != null) {
//                            row.inventoryId = ManagerInventoryId;
//                            db.SaveChanges();
//                        }
//                    }
//                }
//            } else {
//                ManagerInventory = InventoryController.loadInventory(inventoryId);

//                if (ManagerInventory != null)
//                    ManagerInventoryId = ManagerInventory.Id;
//            }
//        }
//    }

//    class WorkshopSpot {
//        public int Id;
//        public int workshopId;
//        public WorkshopSpotType spotType;
//        public Position colPosition = Position.Zero;
//        public Rotation colRotation = Rotation.Zero;
//        public float colWidth;
//        public float colHeight;

//        public CollisionShape CollisionShape;

//        public Dictionary<ChoiceVVehicle, VehicleTuning> VehicleTuningClone = new Dictionary<ChoiceVVehicle, VehicleTuning>();
//        public Dictionary<ChoiceVVehicle, VehicleColoring> VehicleColoringClone = new Dictionary<ChoiceVVehicle, VehicleColoring>();

//        public WorkshopSpot(int id, int workshopid, WorkshopSpotType spottype, Position pos, Rotation rot, float width, float height) {
//            Id = id;
//            workshopId = workshopid;
//            spotType = spottype;

//            colPosition = pos;
//            colRotation = rot;
//            colWidth = width;
//            colHeight = height;

//            CollisionShape = CollisionShape.Create(pos, width, height, rot.Yaw, true, true, true, "WORKSHOP_SPOT");
//            CollisionShape.Owner = this;
//            CollisionShape.TrackPlayersInVehicles = true;
//            CollisionShape.OnEntityEnterShape += onEnterWorkshopSpot;
//            CollisionShape.OnEntityExitShape += onExitWorkshopSpot;

//            Logger.logDebug($"WorkshopSpot created: id {id}, workshopId {workshopId}.");
//        }

//        private void onEnterWorkshopSpot(CollisionShape shape, IEntity entity) { }

//        private void onExitWorkshopSpot(CollisionShape shape, IEntity entity) {
//            if (entity.Type == BaseObjectType.Vehicle) {
//                ChoiceVVehicle vehicle = (ChoiceVVehicle)entity;

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (vehicle != null) {
//                    if (spotType == WorkshopSpotType.Repair) {
//                        VehicleDamage damage = vehicle.getDamage();
//                        if (damage != null) {
//                            damage.Diagnosing = false;
//                            damage.Diagnosed = false;

//                            damage.Initialized = false;
//                        }

//                    } else if (spotType == WorkshopSpotType.Tuning) {
//                        if (VehicleTuningClone.ContainsKey(vehicle)) {
//                            VehicleTuning tuning = VehicleTuningClone[vehicle].Clone();
//                            if (tuning != null) {
//                                vehicle.setData(DATA_VEHICLE_TUNING, tuning);

//                                tuning.NoSave = false;
//                                tuning.Initialized = false;

//                                VehicleTuningClone.Remove(vehicle);
//                            }
//                        }

//                        if (VehicleColoringClone.ContainsKey(vehicle)) {
//                            VehicleColoring coloring = VehicleColoringClone[vehicle].Clone();
//                            if (coloring != null) {
//                                vehicle.setData(DATA_VEHICLE_COLORING, coloring);

//                                coloring.Sanding = false;
//                                coloring.Sanded1 = false;
//                                coloring.Sanded2 = false;
//                                coloring.Sanded3 = false;

//                                coloring.NoSave = false;
//                                coloring.Initialized = false;

//                                VehicleTuningClone.Remove(vehicle);
//                            }
//                        }

//                    } else if (spotType == WorkshopSpotType.Coloring) {
//                        if (VehicleTuningClone.ContainsKey(vehicle)) {
//                            VehicleTuning tuning = VehicleTuningClone[vehicle].Clone();
//                            if (tuning != null) {
//                                vehicle.setData(DATA_VEHICLE_TUNING, tuning);

//                                tuning.NoSave = false;
//                                tuning.Initialized = false;

//                                VehicleTuningClone.Remove(vehicle);
//                            }
//                        }

//                        if (VehicleColoringClone.ContainsKey(vehicle)) {
//                            VehicleColoring coloring = VehicleColoringClone[vehicle].Clone();
//                            if (coloring != null) {
//                                vehicle.setData(DATA_VEHICLE_COLORING, coloring);

//                                coloring.Sanding = false;
//                                coloring.Sanded1 = false;
//                                coloring.Sanded2 = false;
//                                coloring.Sanded3 = false;

//                                coloring.NoSave = false;
//                                coloring.Initialized = false;

//                                VehicleTuningClone.Remove(vehicle);
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }

//    class WorkshopController : ChoiceVScript {
//        public static List<Workshop> AllWorkshops = new List<Workshop>();
//        public static List<WorkshopSpot> AllWorkshopSpots = new List<WorkshopSpot>();

//        private static List<string> VehicleBrakes = new List<string> {
//            "Serienmäßige Bremsen",
//            "Straßenbremsen",
//            "Sportremsen",
//            "Rennbremsen",
//            "Wettkampf Bremsen",
//            "Tuning Bremsen 1",
//            "Tuning Bremsen 2",
//            "Tuning Bremsen 3",
//        };

//        private static List<string> VehicleSuspension = new List<string> {
//            "Serienmäßige Federung",
//            "Tiefergelegte Federung",
//            "Straßenfederung",
//            "Sportfederung",
//            "Wettkampf Federung",
//            "Tieferlegungs Federung",
//            "Tuning Federung 1",
//            "Tuning Federung 2",
//            "Tuning Federung 3",
//        };

//        private static List<string> VehicleTransmission = new List<string> {
//            "Serienmäßiges Getriebe",
//            "Straßengetriebe",
//            "Sportgetriebe",
//            "Renngetriebe",
//            "Wettkampf Getriebe",
//            "Tuning Getriebe 1",
//            "Tuning Getriebe 2",
//            "Tuning Getriebe 3",
//        };

//        private static List<string> VehicleTurbo = new List<string> {
//            "Kein Turbo",
//            "Turbotuning",
//        };

//        private static List<string> VehicleBoost = new List<string> {
//            "Kein Boost",
//            "Nitro Boost 20%",
//            "Nitro Boost 60%",
//            "Nitro Boost 100%",
//            "Ram Boost",
//        };

//        private static List<string> VehicleXenon = new List<string> {
//            "Serienmäßige Scheinwerfer",
//            "Xenon Scheinwerfer",
//        };

//        private static List<string> VehicleWindows = new List<string> {
//            "Keine Tönung",
//            "Limousine",
//            "Dunkles Rauchglas",
//            "Helles Rauchglas",
//        };

//        private static List<string> VehicleArmor = new List<string> {
//            "Keine Rüstung",
//            "20% Rüstung",
//            "40% Rüstung",
//            "60% Rüstung",
//            "80% Rüstung",
//            "100% Rüstung",
//        };

//        private static List<string> VehiclePlate = new List<string> {
//            "Standard Nummernschilder",
//            "Blau auf Weiss 1",
//            "Blau auf Weiss 2",
//            "Blau auf Weiss 3",
//            "Gelb auf Blau",
//            "Gelb auf Schwarz",
//        };

//        private static List<string> VehicleHeadlightColor = new List<string> {
//            "Weiß",
//            "Blau",
//            "Hell Blau",
//            "Grün",
//            "Hell Grün",
//            "Hell Gelb",
//            "Gelb",
//            "Orange",
//            "Rot",
//            "Hell Pink",
//            "Pink",
//            "Lila",
//            "Hell Lila"
//        };

//        private static Dictionary<int, string> VehicleWheeltype = new Dictionary<int, string> {
//            {-1, "Serienmäßige Felgen" },
//            {0, "Sport" },
//            {1, "Muscle Car" },
//            {2, "Lowrider" },
//            {3, "SUV" },
//            {4, "Gelände" },
//            {7, "Luxus" },
//            {5, "Tuner 1" },
//            {8, "Tuner 2" },
//            {9, "Tuner 3" },
//            {10, "Formel" },
//        };

//        private static Dictionary<int, string> MotorcycleWheeltype = new Dictionary<int, string> {
//            {-1, "Serienmäßige Felgen" },
//            {6, "Motorrad" },
//        };

//        public WorkshopController() {
//            EventController.addCollisionShapeEvent("WORKSHOP_INVENTORY_SPOT", onWorkshopInventoryInteract);
//            EventController.addCollisionShapeEvent("WORKSHOP_SPOT", onWorkshopSpotInteract);
//            EventController.addColorPickerEvent("COLOR_EVENT", onColorSelected);

//            EventController.addMenuEvent("WORKSHOP_SPOT_DIAGNOSE", onDiagnose);
//            EventController.addMenuEvent("WORKSHOP_SPOT_REPORT", onReport);
//            EventController.addMenuEvent("WORKSHOP_SPOT_SAND", onSanding);

//            EventController.addMenuEvent("WORKSHOP_SERVICE", onService);
//            EventController.addMenuEvent("WORKSHOP_REPAIR", onRepair);

//            EventController.addMenuEvent("WORKSHOP_TUNING", onTuning);
//            EventController.addMenuEvent("WORKSHOP_COLOR", onColoring);

//            loadWorkshops();
//        }

//        private bool onWorkshopInventoryInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            try {
//                if (data.ContainsKey("AdditionalInfo")) {
//                    int workshopId = int.Parse(data["AdditionalInfo"]);

//                    Workshop shop = AllWorkshops.FirstOrDefault(p => p.Id == workshopId);
//                    if (shop != null) {

//                        if (!player.getCharacterData().AdminMode && player.getAdminLevel() < 3) {
//                            if (shop.OwnerType == WorkshopOwnerType.Player && shop.OwnerId > 0) {
//                                if (player.getCharacterId() != shop.OwnerId) {
//                                    player.sendNotification(NotifactionTypes.Info, $"Sie sind nicht der Besitzer von {shop.Name}.", "", NotifactionImages.Car);
//                                    return false;
//                                }
//                            }

//                            if ((shop.OwnerType == WorkshopOwnerType.Company || shop.OwnerType == WorkshopOwnerType.State) && shop.OwnerId > 0) {
//                                if (!CompanyWorkshopController.isEmployee(player, shop.OwnerId)) {
//                                    string name = shop.Name;

//                                    var company = CompanyController.getCompany(shop.OwnerId);
//                                    if (company != null)
//                                        name = company.Name;

//                                    player.sendNotification(NotifactionTypes.Info, $"Sie sind kein Angestellter von {name}.", "", NotifactionImages.Car);
//                                    return false;
//                                }

//                                if (!CompanyWorkshopController.isEmployeeInDuty(player, shop.OwnerId)) {
//                                    player.sendNotification(NotifactionTypes.Info, $"Sie sind nicht im Dienst.", "", NotifactionImages.Car);
//                                    return false;
//                                }
//                            }
//                        }

//                        if (shop.ManagerInventory != null) {
//                            InventoryController.showMoveInventory(player, player.getInventory(), shop.ManagerInventory);
//                        } else {
//                            Logger.logError($"onWorkshopInventoryInteract: Inventory not found!");
//                            return false;
//                        }

//                        return true;
//                    } else {
//                        Logger.logError($"onWorkshopInventoryInteract: Workshop not found!");
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return false;
//        }

//        private bool onWorkshopSpotInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> collisionData) {
//            ChoiceVVehicle vehicle = (ChoiceVVehicle)collisionShape.AllEntities.Where(p => p.Type == BaseObjectType.Vehicle && p.Exists() && p.Position.Distance(player.Position) <= 10f).OrderBy(v => player.Position.Distance(v.Position)).FirstOrDefault();

//            if (vehicle != null) {
//                WorkshopSpot spot = AllWorkshopSpots.FirstOrDefault(p => p.CollisionShape.Id == collisionShape.Id);

//                if (spot != null) {
//                    Workshop shop = AllWorkshops.FirstOrDefault(p => p.Id == spot.workshopId);

//                    if (shop != null) {
//                        int vehcls = vehicle.getClass();
//                        string vehtype = Convert.ToString((byte)shop.Type, 2);
//                        vehtype = vehtype.PadLeft(8, '0');

//                        if (!player.getCharacterData().AdminMode && player.getAdminLevel() < 3) {
//                            if (shop.OwnerType == WorkshopOwnerType.Player && shop.OwnerId > 0) {
//                                if (player.getCharacterId() != shop.OwnerId) {
//                                    player.sendNotification(NotifactionTypes.Info, $"Sie sind nicht der Besitzer von {shop.Name}.", "", NotifactionImages.Car);
//                                    return false;
//                                }
//                            }

//                            if (!player.getCharacterData().AdminMode && player.getAdminLevel() < 3) {
//                                if ((shop.OwnerType == WorkshopOwnerType.Company || shop.OwnerType == WorkshopOwnerType.State) && shop.OwnerId > 0) {
//                                    if (!CompanyWorkshopController.isEmployee(player, shop.OwnerId)) {
//                                        string name = shop.Name;

//                                        var company = CompanyController.getCompany(shop.OwnerId);
//                                        if (company != null)
//                                            name = company.Name;

//                                        player.sendNotification(NotifactionTypes.Info, $"Sie sind kein Angestellter von {name}.", "", NotifactionImages.Car);
//                                        return false;
//                                    }

//                                    if (!CompanyWorkshopController.isEmployeeInDuty(player, shop.OwnerId)) {
//                                        player.sendNotification(NotifactionTypes.Info, $"Sie sind nicht im Dienst.", "", NotifactionImages.Car);
//                                        return false;
//                                    }
//                                }
//                            }
//                        }

//                        if (VehicleTypeVehicle.Contains(vehcls)) {
//                            if (vehtype.Substring(7, 1) == "0") {
//                                player.sendNotification(NotifactionTypes.Info, $"Autos sind hier nicht zugelassen.", "", NotifactionImages.Car);
//                                return false;
//                            }
//                        } else if (vehcls == 8) {
//                            if (vehtype.Substring(6, 1) == "0") {
//                                player.sendNotification(NotifactionTypes.Info, $"Motorräder sind hier nicht zugelassen.", "", NotifactionImages.Car);
//                                return false;
//                            }
//                        } else if (vehcls == 20) {
//                            if (vehtype.Substring(5, 1) == "0") {
//                                player.sendNotification(NotifactionTypes.Info, $"Lastwagen sind hier nicht zugelassen.", "", NotifactionImages.Car);
//                                return false;
//                            }
//                        } else if (vehcls == 19) {
//                            if (vehtype.Substring(4, 1) == "0") {
//                                player.sendNotification(NotifactionTypes.Info, $"Militärfahrzeuge sind hier nicht zugelassen.", "", NotifactionImages.Car);
//                                return false;
//                            }
//                        } else if (vehcls == 15) {
//                            if (vehtype.Substring(3, 1) == "0") {
//                                player.sendNotification(NotifactionTypes.Info, $"Helikopter sind hier nicht zugelassen.", "", NotifactionImages.Car);
//                                return false;
//                            }
//                        } else if (vehcls == 14) {
//                            if (vehtype.Substring(2, 1) == "0") {
//                                player.sendNotification(NotifactionTypes.Info, $"Boote/Schiffe sind hier nicht zugelassen.", "", NotifactionImages.Car);
//                                return false;
//                            }
//                        } else {
//                            player.sendNotification(NotifactionTypes.Info, $"Fahrzeugtyp ist hier nicht zugelassen.", "", NotifactionImages.Car);
//                            return false;
//                        }

//                        if (spot.spotType == WorkshopSpotType.Repair) {
//                            return repairMenu(player, vehicle, spot);

//                        } else if (spot.spotType == WorkshopSpotType.Coloring) {
//                            return coloringMenu(player, vehicle, spot);

//                        } else if (spot.spotType == WorkshopSpotType.Tuning) {
//                            return tuningMenu(player, vehicle, spot);
//                        }

//                    } else {
//                        Logger.logError($"onWorkshopSpotInteract: Workshop not found!");
//                    }
//                } else {
//                    Logger.logError($"onWorkshopSpotInteract: Workshopspot not found!");
//                }
//            } else {
//                Logger.logError($"onWorkshopSpotInteract: Vehicle not found!");
//            }

//            return false;
//        }

//        #region Vehicle diagnose methods

//        private bool onDiagnose(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (data.ContainsKey("vehicle")) {
//                WorkshopSpot spot = data["spot"];
//                var vehicle = (ChoiceVVehicle)data["vehicle"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (vehicle != null) {
//                    VehicleDamage damage = vehicle.getDamage();

//                    if (damage != null) {
//                        player.sendNotification(NotifactionTypes.Info, $"Fahrzeug wird diagnostiziert (Dauer ca. 30 Sekunden).", "", NotifactionImages.Car);
//                        InvokeController.AddTimedInvoke("VehicleDiagnoseRunning", (ivk) => onDiagnoseStop(player, vehicle), TimeSpan.FromSeconds(30), false);

//                        damage.Diagnosing = true;

//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        private bool onDiagnoseStop(IPlayer player, ChoiceVVehicle vehicle) {
//            if (vehicle != null && vehicle.Exists() == false)
//                vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//            if (vehicle != null) {
//                VehicleDamage damage = vehicle.getDamage();

//                if (damage != null) {
//                    if (damage.Diagnosing && !damage.Diagnosed) {
//                        player.sendNotification(NotifactionTypes.Success, $"Fahrzeugdiagnose abgeschlossen.", "", NotifactionImages.Car);

//                        damage.Diagnosing = false;
//                        damage.Diagnosed = true;
//                        damage.DataChanged = true;

//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        private bool onReport(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (data.ContainsKey("vehicle")) {
//                var vehicle = (ChoiceVVehicle)data["vehicle"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (vehicle != null) {
//                    player.sendNotification(NotifactionTypes.Success, $"Kostenvoranschlag wird erstellt.", "", NotifactionImages.Car);

//                    return true;
//                }
//            }

//            return false;
//        }

//        #endregion

//        #region Vehicle sanding methods

//        private bool onSanding(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (data.ContainsKey("vehicle")) {
//                WorkshopSpot spot = data["spot"];
//                var vehicle = (ChoiceVVehicle)data["vehicle"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (vehicle != null) {
//                    VehicleColoring coloring = vehicle.getColoring();
//                    VehicleTuning tuning = vehicle.getTuning();

//                    if (coloring != null && tuning != null) {
//                        if (spot != null) {
//                            if (!spot.VehicleColoringClone.ContainsKey(vehicle)) {
//                                spot.VehicleColoringClone.Add(vehicle, coloring);

//                                coloring = coloring.Clone();
//                                coloring.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_COLORING, coloring);
//                            }

//                            if (!spot.VehicleTuningClone.ContainsKey(vehicle)) {
//                                spot.VehicleTuningClone.Add(vehicle, tuning);

//                                tuning = tuning.Clone();
//                                tuning.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_TUNING, tuning);
//                            }
//                        }

//                        if (!coloring.Sanded1) {
//                            if (vehicle.GetMod(VehicleModType.Livery) > 0)
//                                player.sendNotification(NotifactionTypes.Info, $"Folie wird entfernt und Oberfläche abgeschliffen (Dauer ca. 30 Sekunden).", "", NotifactionImages.Car);
//                            else
//                                player.sendNotification(NotifactionTypes.Info, $"Fahrzeug wird abgeschliffen mit Stufe 1 (Dauer ca. 30 Sekunden).", "", NotifactionImages.Car);

//                            InvokeController.AddTimedInvoke("VehicleSandingRunning", (ivk) => onSandingStop(player, vehicle), TimeSpan.FromSeconds(30), false);

//                            coloring.Sanding = true;

//                            return true;
//                        }

//                        if (!coloring.Sanded2) {
//                            player.sendNotification(NotifactionTypes.Info, $"Fahrzeug wird abgeschliffen mit Stufe 2 (Dauer ca. 30 Sekunden).", "", NotifactionImages.Car);
//                            InvokeController.AddTimedInvoke("VehicleSandingRunning", (ivk) => onSandingStop(player, vehicle), TimeSpan.FromSeconds(30), false);

//                            coloring.Sanding = true;

//                            return true;
//                        }

//                        if (!coloring.Sanded3) {
//                            player.sendNotification(NotifactionTypes.Info, $"Fahrzeug wird abgeschliffen mit Stufe 3 (Dauer ca. 30 Sekunden).", "", NotifactionImages.Car);
//                            InvokeController.AddTimedInvoke("VehicleSandingRunning", (ivk) => onSandingStop(player, vehicle), TimeSpan.FromSeconds(30), false);

//                            coloring.Sanding = true;

//                            return true;
//                        }
//                    }
//                }
//            }

//            return false;
//        }

//        private bool onSandingStop(IPlayer player, ChoiceVVehicle vehicle) {
//            if (vehicle != null && vehicle.Exists() == false)
//                vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//            if (vehicle != null) {
//                VehicleColoring coloring = vehicle.getColoring();
//                VehicleTuning tuning = vehicle.getTuning();
//                VehicleDamage damage = vehicle.getDamage();

//                if (coloring != null && tuning != null && damage != null) {
//                    if (coloring.Sanding && !coloring.Sanded1) {
//                        player.sendNotification(NotifactionTypes.Success, $"Abschleifen Stufe 1 abgeschlossen.", "", NotifactionImages.Car);

//                        coloring.Sanded1 = true;
//                        coloring.Sanding = false;

//                        damage.repairDirtLevel(vehicle);

//                        tuning.setLivery(vehicle, 0);

//                        coloring.setPrimaryColor(vehicle, 118);
//                        coloring.setSecondaryColor(vehicle, 118);
//                        coloring.setRoofLivery(vehicle, 0);

//                        return true;
//                    }

//                    if (coloring.Sanding && !coloring.Sanded2) {
//                        player.sendNotification(NotifactionTypes.Success, $"Abschleifen Stufe 2 abgeschlossen.", "", NotifactionImages.Car);

//                        coloring.Sanded2 = true;
//                        coloring.Sanding = false;

//                        damage.repairDirtLevel(vehicle);

//                        tuning.setLivery(vehicle, 0);

//                        coloring.setPrimaryColor(vehicle, 117);
//                        coloring.setSecondaryColor(vehicle, 117);
//                        coloring.setRoofLivery(vehicle, 0);

//                        return true;
//                    }

//                    if (coloring.Sanding && !coloring.Sanded3) {
//                        player.sendNotification(NotifactionTypes.Success, $"Abschleifen Stufe 3 abgeschlossen.", "", NotifactionImages.Car);

//                        coloring.Sanded3 = true;
//                        coloring.Sanding = false;

//                        damage.repairDirtLevel(vehicle);

//                        tuning.setLivery(vehicle, 0);

//                        coloring.setPrimaryColor(vehicle, 119);
//                        coloring.setSecondaryColor(vehicle, 119);
//                        coloring.setRoofLivery(vehicle, 0);

//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        #endregion

//        #region Vehicle damage methods

//        public static bool repairMenu(IPlayer player, ChoiceVVehicle vehicle, WorkshopSpot spot) {
//            if (vehicle != null && vehicle.Exists() == false)
//                vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//            if (vehicle != null) {
//                if (VehicleController.AllVehicleUsedSeats.Find(v => v.VehicleId == vehicle.getId()) == null) {
//                    VehicleDamage damage = vehicle.getDamage();

//                    if (damage != null && !damage.Diagnosing) {
//                        var menu = new Menu("Reperaturplatz", "Hier können Fahrzeuge repariert werden");
//                        var data = new Dictionary<string, dynamic> { { "spot", spot }, { "vehicle", vehicle } };
//                        var subdata = new Dictionary<string, dynamic>();

//                        if (spot != null && !damage.Diagnosed) {
//                            menu.addMenuItem(new ClickMenuItem("Diagnose", "", "", "WORKSHOP_SPOT_DIAGNOSE", MenuItemStyle.normal).withData(data));

//                            player.showMenu(menu);
//                            return true;
//                        }

//                        bool hasframe = false;
//                        bool hasrepair = false;
//                        bool hasservice = false;
//                        bool wheelrepair = false;
//                        bool wheelservice = false;

//                        for (byte i = 0; i < vehicle.WheelsCount; i++) {
//                            if (!damage.WheelHasTire.ContainsKey(i))
//                                damage.WheelHasTire.Add(i, true);
//                            if (!damage.WheelDetached.ContainsKey(i))
//                                damage.WheelDetached.Add(i, false);
//                            if (!damage.WheelHealth.ContainsKey(i))
//                                damage.WheelHealth.Add(i, 1000f);
//                            if (!damage.WheelBurst.ContainsKey(i))
//                                damage.WheelBurst.Add(i, false);

//                            if (!damage.WheelHasTire[i] || damage.WheelDetached[i] || damage.WheelBurst[i] || damage.WheelHealth[i] < 100f)
//                                wheelrepair = true;
//                            else if (damage.WheelHasTire[i] && !damage.WheelDetached[i] && !damage.WheelBurst[i] && damage.WheelHealth[i] >= 100f && damage.WheelHealth[i] < 500f)
//                                wheelservice = true;
//                        }

//                        var submenu1 = new Menu("Fahrzeug warten", "");
//                        if (damage.EngineHealth >= 300f && damage.EngineHealth < 600f) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "engine" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu1.addMenuItem(new ClickMenuItem("Motorwartung durchführen", "", "", "WORKSHOP_SERVICE", MenuItemStyle.normal).withData(subdata));
//                            hasservice = true;
//                        }
//                        if (wheelservice && !wheelrepair) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "wheels" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu1.addMenuItem(new ClickMenuItem("Reifen wechseln", "", "", "WORKSHOP_SERVICE", MenuItemStyle.normal).withData(subdata));
//                            hasservice = true;
//                        }
//                        if (damage.DirtLevel > 10f) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "dirtlevel" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu1.addMenuItem(new ClickMenuItem("Fahrzeug reinigen", "", "", "WORKSHOP_SERVICE", MenuItemStyle.normal).withData(subdata));
//                            hasservice = true;
//                        }

//                        var submenu2 = new Menu("Fahrzeug reparieren", "");
//                        if (damage.EngineHealth <= 0f) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "enginechange" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Motor austauschen", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                        } else if (damage.EngineHealth < 300f) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "enginerepair" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Motor reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                        }
//                        if (damage.PetrolTankHealth < 300) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "tank" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Tank reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                        }
//                        if (wheelrepair) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "wheels" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Räder/Felgen reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                        }
//                        if (damage.BumperDamageLevel[VehicleBumper.Front] > 0) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "frontbumper" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Frontstoßstange reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                            hasframe = true;
//                        }
//                        if (damage.BumperDamageLevel[VehicleBumper.Rear] > 0) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "rearbumper" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Heckstoßstange reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                            hasframe = true;
//                        }
//                        if (damage.PartDamageLevel[VehiclePart.FrontLeft] > 0 || damage.PartDamageLevel[VehiclePart.FrontRight] > 0 || damage.PartDamageLevel[VehiclePart.MiddleLeft] > 0 || damage.PartDamageLevel[VehiclePart.MiddleRight] > 0 || damage.PartDamageLevel[VehiclePart.RearLeft] > 0 || damage.PartDamageLevel[VehiclePart.RearRight] > 0) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "frame" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Karosserie reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                            hasframe = true;
//                        }
//                        if (damage.DoorState[VehicleDoorExt.DriverFront] > 0 || damage.DoorState[VehicleDoorExt.DriverRear] > 0 || damage.DoorState[VehicleDoorExt.PassengerFront] > 0 || damage.DoorState[VehicleDoorExt.PassengerRear] > 0) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "doors" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Türen reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                            hasframe = true;
//                        }
//                        if (damage.DoorState[VehicleDoorExt.Hood] > 0) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "hood" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Motorhaube reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                        }
//                        if (damage.DoorState[VehicleDoorExt.Trunk] > 0) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "trunk" }, { "spot", spot }, { "vehicle", vehicle } };
//                            submenu2.addMenuItem(new ClickMenuItem("Kofferraum reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                            hasrepair = true;
//                        }
//                        if (damage.LightDamaged[VehicleLight.FrontLeft] || damage.LightDamaged[VehicleLight.FrontRight] || damage.LightDamaged[VehicleLight.RearLeft] || damage.LightDamaged[VehicleLight.RearRight]) {
//                            if (!hasframe) {
//                                subdata = new Dictionary<string, dynamic> { { "target", "lights" }, { "spot", spot }, { "vehicle", vehicle } };
//                                submenu2.addMenuItem(new ClickMenuItem("Beleuchtung reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                                hasrepair = true;
//                            } else {
//                                submenu2.addMenuItem(new StaticMenuItem("Beleuchtung reparieren", "", "", MenuItemStyle.red));
//                            }
//                        }
//                        if (damage.WindowDamaged[VehicleWindow.Back] || damage.WindowDamaged[VehicleWindow.Front] || damage.WindowDamaged[VehicleWindow.FrontLeft] || damage.WindowDamaged[VehicleWindow.FrontRight] || damage.WindowDamaged[VehicleWindow.RearLeft] || damage.WindowDamaged[VehicleWindow.RearRight]) {
//                            if (!hasframe) {
//                                subdata = new Dictionary<string, dynamic> { { "target", "windows" }, { "spot", spot }, { "vehicle", vehicle } };
//                                submenu2.addMenuItem(new ClickMenuItem("Scheiben reparieren", "", "", "WORKSHOP_REPAIR", MenuItemStyle.normal).withData(subdata));
//                                hasrepair = true;
//                            } else {
//                                submenu2.addMenuItem(new StaticMenuItem("Scheiben reparieren", "", "", MenuItemStyle.red));
//                            }
//                        }

//                        // if (hasservice || hasrepair)
//                        //    menu.addMenuItem(new ClickMenuItem("Kostenvoranschlag", "", "", "WORKSHOP_SPOT_REPORT", MenuItemStyle.normal).withData(data));

//                        if (submenu1.MenuItems.Count() > 0 && hasservice) {
//                            menu.addMenuItem(new MenuMenuItem("Fahrzeug warten", submenu1));
//                        } else {
//                            menu.addMenuItem(new StaticMenuItem("Fahrzeug warten", "Fahrzeugwartung ist nicht nötig", "", MenuItemStyle.normal));
//                        }

//                        if (submenu2.MenuItems.Count() > 0 && hasrepair) {
//                            menu.addMenuItem(new MenuMenuItem("Fahrzeug reparieren", submenu2));
//                        } else {
//                            menu.addMenuItem(new StaticMenuItem("Fahrzeug reparieren", "Fahrzeugreperatur ist nicht nötig", "", MenuItemStyle.normal));
//                        }

//                        player.showMenu(menu, false);
//                        return true;
//                    }
//                } else {
//                    player.sendNotification(Constants.NotifactionTypes.Info, "Alle Passagiere bitte aussteigen.", "Alle aussteigen", Constants.NotifactionImages.Car);
//                }
//            }

//            return false;
//        }

//        private bool onService(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (data.ContainsKey("target") && data.ContainsKey("spot") && data.ContainsKey("vehicle")) {
//                string target = data["target"];
//                WorkshopSpot spot = data["spot"];
//                ChoiceVVehicle vehicle = data["vehicle"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (target != "" && vehicle != null) {
//                    VehicleDamage damage = vehicle.getDamage();
//                    if (damage != null) {
//                        if (target == "engine")
//                            return damage.repairEngine(vehicle);
//                        else if (target == "wheels")
//                            return damage.repairWheels(vehicle);
//                        else if (target == "dirtlevel")
//                            return damage.repairDirtLevel(vehicle);
//                    }
//                }
//            }

//            return false;
//        }

//        private bool onRepair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (data.ContainsKey("target") && data.ContainsKey("spot") && data.ContainsKey("vehicle")) {
//                string target = data["target"];
//                WorkshopSpot spot = data["spot"];
//                ChoiceVVehicle vehicle = data["vehicle"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (target != "" && vehicle != null) {
//                    VehicleDamage damage = vehicle.getDamage();
//                    VehicleTuning tuning = vehicle.getTuning();
//                    VehicleColoring coloring = vehicle.getColoring();
//                    VehicleObject vehobj = vehicle.getObject();

//                    if (damage != null && tuning != null && coloring != null && vehobj != null) {
//                        if (target == "enginechange") {
//                            tuning.setEngine(vehicle, 0);
//                            tuning.setEngineBlock(vehicle, 0);
//                            tuning.setTurbo(vehicle, 0);
//                            tuning.setBoost(vehicle, 0);

//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairEngine(vehicle);

//                        } else if (target == "enginerepair") {
//                            tuning.setTurbo(vehicle, 0);
//                            tuning.setBoost(vehicle, 0);

//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairEngine(vehicle);

//                        } else if (target == "tank") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairTank(vehicle);

//                        } else if (target == "wheels") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairWheels(vehicle);

//                        } else if (target == "frontbumper") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairFrontBumper(vehicle);

//                        } else if (target == "rearbumper") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairRearBumper(vehicle);

//                        } else if (target == "frame") {
//                            tuning.setHood(vehicle, 0);
//                            tuning.setRoof(vehicle, 0);

//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairFrame(vehicle);

//                        } else if (target == "doors") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairDoors(vehicle);

//                        } else if (target == "hood") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            tuning.setHood(vehicle, 0);

//                            return damage.repairHood(vehicle);

//                        } else if (target == "trunk") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            tuning.setSpoilers(vehicle, 0);

//                            return damage.repairTrunk(vehicle);

//                        } else if (target == "lights") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            return damage.repairLights(vehicle);

//                        } else if (target == "windows") {
//                            vehobj.RepairCount++;
//                            vehobj.DataChanged = true;

//                            tuning.setWindowTint(vehicle, 0);

//                            return damage.repairWindows(vehicle);
//                        }
//                    }
//                }
//            }

//            return false;
//        }

//        #endregion

//        #region Vehicle tuning methods

//        public static bool tuningMenu(IPlayer player, ChoiceVVehicle vehicle, WorkshopSpot spot) {
//            if (vehicle != null && vehicle.Exists() == false)
//                vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//            if (vehicle != null) {
//                VehicleTuning tuning = vehicle.getTuning();

//                if (tuning != null) {
//                    int vehcls = vehicle.getClass();

//                    var menu = new Menu("Tuningplatz", "Hier können Fahrzeuge getuned werden");
//                    var data = new Dictionary<string, dynamic> { { "spot", spot }, { "vehicle", vehicle } };
//                    var subdata = new Dictionary<string, dynamic>();

//                    var subengine = new Menu("Motorabstimmung", "");
//                    if (vehicle.GetModsCount(VehicleModType.Engine) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Engine); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "engine" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subengine.addMenuItem(new ClickMenuItem(i == 0 ? "Keine Verbesserung" : $"EMS Verbesserung {i}", "", "", "WORKSHOP_TUNING", tuning.Engine == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subbrakes = new Menu("Bremsen", "");
//                    if (vehicle.GetModsCount(VehicleModType.Brakes) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Brakes); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "brakes" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subbrakes.addMenuItem(new ClickMenuItem(VehicleBrakes[i], "", "", "WORKSHOP_TUNING", tuning.Brakes == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subsuspension = new Menu("Federung", "");
//                    if (vehicle.GetModsCount(VehicleModType.Suspension) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Suspension); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "suspension" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subsuspension.addMenuItem(new ClickMenuItem(VehicleSuspension[i], "", "", "WORKSHOP_TUNING", tuning.Suspension == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subtransmission = new Menu("Getriebe", "");
//                    if (vehicle.GetModsCount(VehicleModType.Transmission) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Transmission); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "transmission" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subtransmission.addMenuItem(new ClickMenuItem(VehicleTransmission[i], "", "", "WORKSHOP_TUNING", tuning.Transmission == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subturbo = new Menu("Turbo", "");
//                    if (vehicle.GetModsCount(VehicleModType.Turbo) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Turbo); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "turbo" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subturbo.addMenuItem(new ClickMenuItem(VehicleTurbo[i], "", "", "WORKSHOP_TUNING", tuning.Turbo == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subboost = new Menu("Boost", "");
//                    if (vehicle.GetModsCount(VehicleModType.Boost) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Boost); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "boost" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subboost.addMenuItem(new ClickMenuItem(VehicleBoost[i], "", "", "WORKSHOP_TUNING", tuning.Boost == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subexhaust = new Menu("Auspuffanlage", "");
//                    if (vehicle.GetModsCount(VehicleModType.Exhaust) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Exhaust); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "exhaust" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subexhaust.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Auspuff" : $"Auspuff Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Exhaust == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subspoiler = new Menu(vehcls == 8 ? "Antrieb" : "Spoiler", "");
//                    if (vehicle.GetModsCount(VehicleModType.Spoilers) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Spoilers); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "spoilers" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subspoiler.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Antrieb" : $"Antrieb Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Spoilers == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subspoiler.addMenuItem(new ClickMenuItem(i == 0 ? "Kein Spoiler" : $"Spoiler Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Spoilers == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subsideskirt = new Menu(vehcls == 8 ? "Luftfilter" : "Seitenverkleidung", "");
//                    if (vehicle.GetModsCount(VehicleModType.SideSkirt) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.SideSkirt); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "sideskirt" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subsideskirt.addMenuItem(new ClickMenuItem(i == 0 ? "Serien Luftfilter" : $"Luftfilter Variante {i}", "", "", "WORKSHOP_TUNING", tuning.SideSkirt == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subsideskirt.addMenuItem(new ClickMenuItem(i == 0 ? "Serien Seitenverkleidung" : $"Seitenverkleidung Variante {i}", "", "", "WORKSHOP_TUNING", tuning.SideSkirt == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subfrontbumper = new Menu(vehcls == 8 ? "Frontkotflügel" : "Frontstoßstange", "");
//                    if (vehicle.GetModsCount(VehicleModType.FrontBumper) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.FrontBumper); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "frontbumper" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subfrontbumper.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Frontkotflügel" : $"Frontkotflügel Variante {i}", "", "", "WORKSHOP_TUNING", tuning.FrontBumper == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subfrontbumper.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Frontstoßstange" : $"Frontstoßstange Variante {i}", "", "", "WORKSHOP_TUNING", tuning.FrontBumper == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subrearbumper = new Menu(vehcls == 8 ? "Heckkotflügel" : "Heckstoßstange", "");
//                    if (vehicle.GetModsCount(VehicleModType.RearBumper) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.RearBumper); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "rearbumper" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subrearbumper.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Heckkotflügel" : $"Heckkotflügel Variante {i}", "", "", "WORKSHOP_TUNING", tuning.RearBumper == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subrearbumper.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Heckstoßstange" : $"Heckstoßstange Variante {i}", "", "", "WORKSHOP_TUNING", tuning.RearBumper == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subgrille = new Menu(vehcls == 8 ? "Anbauteile" : "Kühlergrill", "");
//                    if (vehicle.GetModsCount(VehicleModType.Grille) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Grille); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "grille" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subgrille.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Anbauteile" : $"Anbauteile Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Grille == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subgrille.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Kühlergrill" : $"Kühlergrill Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Grille == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subhood = new Menu("Motorhaube", "");
//                    if (vehicle.GetModsCount(VehicleModType.Hood) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Hood); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "hood" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subhood.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Motorhaube" : $"Motorhaube Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Hood == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subroof = new Menu(vehcls == 8 ? "Tank" : "Dach", "");
//                    if (vehicle.GetModsCount(VehicleModType.Roof) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Roof); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "roof" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subroof.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Tank" : $"Tank Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Roof == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subroof.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiges Dach" : $"Dach Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Roof == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subfender = new Menu(vehcls == 8 ? "Rahmenteile" : "Tuningteile 1", "");
//                    if (vehicle.GetModsCount(VehicleModType.Fender) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Fender); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "fender" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subfender.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Rahmenteile" : $"Rahmenteile Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Fender == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subfender.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Tuningteile 1" : $"Tuningteile 1 Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Fender == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subrightfender = new Menu("Tuningteile 2", "");
//                    if (vehicle.GetModsCount(VehicleModType.RightFender) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.RightFender); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "rightfender" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subrightfender.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Tuningteile 2" : $"Tuningteile 2 Variante {i}", "", "", "WORKSHOP_TUNING", tuning.RightFender == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subfrontwheels = new Menu("Vorderräder", "");
//                    if (vehicle.GetModsCount(VehicleModType.FrontWheels) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.FrontWheels); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "frontwheels" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subfrontwheels.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Vorderräder" : $"Vorderräder Variante {i}", "", "", "WORKSHOP_TUNING", tuning.FrontWheels == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subbackwheels = new Menu("Hinterräder", "");
//                    if (vehicle.GetModsCount(VehicleModType.BackWheels) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.BackWheels); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "backwheels" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subbackwheels.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiges Hinterräder" : $"Hinterräder Variante {i}", "", "", "WORKSHOP_TUNING", tuning.BackWheels == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subframe = new Menu(vehcls == 8 ? "Motorblock" : "Rahmen", "");
//                    if (vehicle.GetModsCount(VehicleModType.Frame) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Frame); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "frame" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            if (vehcls == 8)
//                                subframe.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Motorblock" : $"Motorblock Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Frame == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            else
//                                subframe.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Rahmen" : $"Rahmen Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Frame == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subxenon = new Menu("Xenon", "");
//                    if (vehicle.GetModsCount(VehicleModType.Xenon) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Xenon); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "xenon" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subxenon.addMenuItem(new ClickMenuItem(VehicleXenon[i], "", "", "WORKSHOP_TUNING", tuning.Xenon == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subxenoncolor = new Menu("Xenon Farbe", "");
//                    for (int i = 0; i < VehicleHeadlightColor.Count(); i++) {
//                        subdata = new Dictionary<string, dynamic> { { "target", "xenoncolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                        subxenoncolor.addMenuItem(new ClickMenuItem(VehicleHeadlightColor[i], "", "", "WORKSHOP_TUNING", tuning.HeadlightColor == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                    }

//                    var subwindowtint = new Menu("Scheibentönung", "");
//                    for (int i = 0; i < VehicleWindows.Count(); i++) {
//                        subdata = new Dictionary<string, dynamic> { { "target", "windowtint" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                        subwindowtint.addMenuItem(new ClickMenuItem(VehicleWindows[i], "", "", "WORKSHOP_TUNING", tuning.WindowTint == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                    }

//                    var subarmor = new Menu("Rüstung", "");
//                    if (vehicle.GetModsCount(VehicleModType.Armor) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Armor); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "armor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subarmor.addMenuItem(new ClickMenuItem(VehicleArmor[i], "", "", "WORKSHOP_TUNING", tuning.Armor == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subtrimdesign = new Menu("Interieur", "");
//                    if (vehicle.GetModsCount(VehicleModType.TrimDesign) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.TrimDesign); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "trimdesign" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subtrimdesign.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiges Interieur" : $"Interieur Variante {i}", "", "", "WORKSHOP_TUNING", tuning.TrimDesign == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subornaments = new Menu("Verziehrung", "");
//                    if (vehicle.GetModsCount(VehicleModType.Ornaments) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Ornaments); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "ornaments" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subornaments.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Verzierung" : $"Verzierung Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Ornaments == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subdialdesign = new Menu("Armaturen", "");
//                    if (vehicle.GetModsCount(VehicleModType.DialDesign) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.DialDesign); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "dialdesign" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subdialdesign.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Armaturen" : $"Armaturen Variante {i}", "", "", "WORKSHOP_TUNING", tuning.DialDesign == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var substeeringwheel = new Menu("Lenkrad", "");
//                    if (vehicle.GetModsCount(VehicleModType.SteeringWheel) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.SteeringWheel); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "steeringwheel" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            substeeringwheel.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiges Lenkrad" : $"Lenkrad Variante {i}", "", "", "WORKSHOP_TUNING", tuning.SteeringWheel == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subshiftlever = new Menu("Ganghebel", "");
//                    if (vehicle.GetModsCount(VehicleModType.ShiftLever) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.ShiftLever); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "subshiftlever" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subshiftlever.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Ganghebel" : $"Ganghebel Variante {i}", "", "", "WORKSHOP_TUNING", tuning.ShiftLever == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subplaques = new Menu("Plakette", "");
//                    if (vehicle.GetModsCount(VehicleModType.Plaques) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Plaques); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "plaques" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subplaques.addMenuItem(new ClickMenuItem(i == 0 ? "Keine Plakette" : $"Plakette Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Plaques == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subhydraulics = new Menu("Hydraulik", "");
//                    if (vehicle.GetModsCount(VehicleModType.Hydraulics) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Hydraulics); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "hydraulics" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subhydraulics.addMenuItem(new ClickMenuItem(i == 0 ? "Keine Hydraulik" : $"Hydraulik Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Hydraulics == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var sublivery = new Menu("Folierung", "");
//                    if (vehicle.GetModsCount(VehicleModType.Livery) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Livery); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "livery" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            sublivery.addMenuItem(new ClickMenuItem(i == 0 ? "Keine Folierung" : $"Folierung Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Livery == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subplate = new Menu("Nummernschilder", "");
//                    if (vehicle.GetModsCount(VehicleModType.Plate) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Plate); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "plate" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subplate.addMenuItem(new ClickMenuItem(VehiclePlate[i], "", "", "WORKSHOP_TUNING", tuning.Plate == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subplateholders = new Menu("Schilderhalter", "");
//                    if (vehicle.GetModsCount(VehicleModType.PlateHolders) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.PlateHolders); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "plateholders" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subplateholders.addMenuItem(new ClickMenuItem(i == 0 ? "Standard Schilderhalter" : $"Schilderhalter Variante {i}", "", "", "WORKSHOP_TUNING", tuning.PlateHolders == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subcolor1 = new Menu("Primärfarbe", "");
//                    if (vehicle.GetModsCount(VehicleModType.Color1) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Color1); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "color1" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subcolor1.addMenuItem(new ClickMenuItem(i == 0 ? "Standard Primärfarbe" : $"Primärfarbe Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Color1 == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subcolor2 = new Menu("Sekundärfarbe", "");
//                    if (vehicle.GetModsCount(VehicleModType.Color2) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Color2); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "color2" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subcolor2.addMenuItem(new ClickMenuItem(i == 0 ? "Standard Sekundärfarbe" : $"Sekundärfarbe Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Color2 == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subdashboardcolor = new Menu("Armaturenbrett", "");
//                    if (vehicle.GetModsCount(VehicleModType.DashboardColor) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.DashboardColor); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "dashboardcolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subdashboardcolor.addMenuItem(new ClickMenuItem(i == 0 ? "Standard Armaturenbrett" : $"Armaturenbrett Variante {i}", "", "", "WORKSHOP_TUNING", tuning.DashboardColor == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subtrimcolor = new Menu("Interieurdesign", "");
//                    if (vehicle.GetModsCount(VehicleModType.TrimColor) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.TrimColor); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "trimcolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subtrimcolor.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiges Interieurdesign" : $"Interieurdesign {i}", "", "", "WORKSHOP_TUNING", tuning.TrimColor == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subvanityplates = new Menu("Tuning Schilder", "");
//                    if (vehicle.GetModsCount(26) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(26); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "vanityplates" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subvanityplates.addMenuItem(new ClickMenuItem(i == 0 ? "Keine Tuning Schilder" : $"Tuning Schilder Variante {i}", "", "", "WORKSHOP_TUNING", tuning.VanityPlates == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subdoorspeaker = new Menu("Türlautsprecher", "");
//                    if (vehicle.GetModsCount(31) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(31); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "doorspeaker" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subdoorspeaker.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Türlautsprecher" : $"Türlautsprecher Variante {i}", "", "", "WORKSHOP_TUNING", tuning.DoorSpeaker == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subseats = new Menu("Sitze", "");
//                    if (vehicle.GetModsCount(32) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(32); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "seats" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subseats.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Sitze" : $"Sitze Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Seats == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subspeakers = new Menu("Lautsprecher", "");
//                    if (vehicle.GetModsCount(36) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(36); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "speakers" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subspeakers.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Lautsprecher" : $"Lautsprecher Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Speakers == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subtrunk = new Menu("Kofferraum", "");
//                    if (vehicle.GetModsCount(37) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(37); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "trunk" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subtrunk.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Kofferraum" : $"Kofferraum Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Trunk == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subengineblock = new Menu("Motorblock", "");
//                    if (vehicle.GetModsCount(39) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(39); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "engineblock" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subengineblock.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Motorblock" : $"Motorblock Variante {i}", "", "", "WORKSHOP_TUNING", tuning.EngineBlock == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var substruts = new Menu("Stabilisatoren", "");
//                    if (vehicle.GetModsCount(41) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(41); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "struts" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            substruts.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Stabilisatoren" : $"Stabilisatoren Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Struts == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subarchcover = new Menu("Radhaus", "");
//                    if (vehicle.GetModsCount(42) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(42); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "archcover" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subarchcover.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiges Radhaus" : $"Radhaus Variante {i}", "", "", "WORKSHOP_TUNING", tuning.ArchCover == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subaerials = new Menu("Antenne", "");
//                    if (vehicle.GetModsCount(43) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(43); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "aerials" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subaerials.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Antenne" : $"Antenne Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Aerials == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subtrim = new Menu("Zierleisten", "");
//                    if (vehicle.GetModsCount(44) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(44); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "trim" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subtrim.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Zierleisten" : $"Zierleisten Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Trim == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subtank = new Menu("Tank", "");
//                    if (vehicle.GetModsCount(45) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(45); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "tank" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subtank.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßiger Tank" : $"Tank Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Tank == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subwindows = new Menu("Verglasung", "");
//                    if (vehicle.GetModsCount(46) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(46); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "windows" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subwindows.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Verglasung" : $"Fenster Verglasung {i}", "", "", "WORKSHOP_TUNING", tuning.Windows == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subwheels = new Menu("Felgen", "");
//                    if (vehcls == 8) {
//                        for (int i = 0; i < MotorcycleWheeltype.Count; i++) {
//                            if (i == 0) {
//                                subdata = new Dictionary<string, dynamic> { { "target", "wheels" }, { "spot", spot }, { "vehicle", vehicle }, { "type", MotorcycleWheeltype.ElementAt(i).Key }, { "value", 0 } };
//                                subwheels.addMenuItem(new ClickMenuItem(MotorcycleWheeltype.ElementAt(i).Value, "", "", "WORKSHOP_TUNING", tuning.WheelType == MotorcycleWheeltype.ElementAt(i).Key ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            } else {
//                                var subwheeltype = new Menu(MotorcycleWheeltype.ElementAt(i).Value, "");
//                                for (int j = 1; j <= 100; j++) {
//                                    subdata = new Dictionary<string, dynamic> { { "target", "wheels" }, { "spot", spot }, { "vehicle", vehicle }, { "type", MotorcycleWheeltype.ElementAt(i).Key }, { "value", j } };
//                                    subwheeltype.addMenuItem(new ClickMenuItem($"Felgen Variante {j}", "", "", "WORKSHOP_TUNING", tuning.WheelType == MotorcycleWheeltype.ElementAt(i).Key && tuning.WheelVariation == j ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                                }
//                                subwheels.addMenuItem(new MenuMenuItem(MotorcycleWheeltype.ElementAt(i).Value, subwheeltype, tuning.WheelType == MotorcycleWheeltype.ElementAt(i).Key ? MenuItemStyle.green : MenuItemStyle.normal));
//                            }
//                        }
//                    } else if (VehicleTypeVehicle.Contains(vehcls)) {
//                        for (int i = 0; i < VehicleWheeltype.Count; i++) {
//                            if (i == 0) {
//                                subdata = new Dictionary<string, dynamic> { { "target", "wheels" }, { "spot", spot }, { "vehicle", vehicle }, { "type", VehicleWheeltype.ElementAt(i).Key }, { "value", 0 } };
//                                subwheels.addMenuItem(new ClickMenuItem(VehicleWheeltype.ElementAt(i).Value, "", "", "WORKSHOP_TUNING", tuning.WheelType == VehicleWheeltype.ElementAt(i).Key ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                            } else {
//                                var subwheeltype = new Menu(VehicleWheeltype.ElementAt(i).Value, "");
//                                for (int j = 1; j <= 100; j++) {
//                                    subdata = new Dictionary<string, dynamic> { { "target", "wheels" }, { "spot", spot }, { "vehicle", vehicle }, { "type", VehicleWheeltype.ElementAt(i).Key }, { "value", j } };
//                                    subwheeltype.addMenuItem(new ClickMenuItem($"Felgen Variante {j}", "", "", "WORKSHOP_TUNING", tuning.WheelType == VehicleWheeltype.ElementAt(i).Key && tuning.WheelVariation == j ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                                }
//                                subwheels.addMenuItem(new MenuMenuItem(VehicleWheeltype.ElementAt(i).Value, subwheeltype, tuning.WheelType == VehicleWheeltype.ElementAt(i).Key ? MenuItemStyle.green : MenuItemStyle.normal));
//                            }
//                        }
//                    }

//                    var subhorns = new Menu("Hupe", "");
//                    if (vehicle.GetModsCount(VehicleModType.Horns) > 0) {
//                        for (int i = 0; i <= vehicle.GetModsCount(VehicleModType.Horns); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "horns" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                            subhorns.addMenuItem(new ClickMenuItem(i == 0 ? "Serienmäßige Hupe" : $"Hupe Variante {i}", "", "", "WORKSHOP_TUNING", tuning.Horns == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                        }
//                    }

//                    var subextrasmodel = new Menu("Extras Fahrzeugmodell", "");
//                    var subextras = new Menu("Extras Fahrzeug", "");

//                    VehicleModels model = vehicle.getModel();
//                    if (model != null) {
//                        for (int i = 0; i < model.Extras.Count(); i++) {
//                            subdata = new Dictionary<string, dynamic> { { "target", "extrasmodel" }, { "spot", spot }, { "vehicle", vehicle }, { "value", model.Extras.ElementAt(i).Key } };
//                            subextrasmodel.addMenuItem(new CheckBoxMenuItem($"Fahrzeugmodell Extra {i + 1}", "", model.Extras.ElementAt(i).Value, "WORKSHOP_TUNING", MenuItemStyle.normal, true).withData(subdata));
//                        }

//                        if (tuning.Extras.Count() > 0) {
//                            for (int i = 0; i < tuning.Extras.Count(); i++) {
//                                subdata = new Dictionary<string, dynamic> { { "target", "extras" }, { "spot", spot }, { "vehicle", vehicle }, { "value", tuning.Extras.ElementAt(i).Key } };
//                                subextras.addMenuItem(new CheckBoxMenuItem($"Extra {i + 1}", "", tuning.Extras.ElementAt(i).Value, "WORKSHOP_TUNING", MenuItemStyle.normal, true).withData(subdata));
//                            }
//                        } else {
//                            for (int i = 0; i < model.Extras.Count(); i++) {
//                                subdata = new Dictionary<string, dynamic> { { "target", "extras" }, { "spot", spot }, { "vehicle", vehicle }, { "value", model.Extras.ElementAt(i).Key } };
//                                subextras.addMenuItem(new CheckBoxMenuItem($"Extra {i + 1}", "", model.Extras.ElementAt(i).Value, "WORKSHOP_TUNING", MenuItemStyle.normal, true).withData(subdata));
//                            }
//                        }
//                    }

//                    if (subengine.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Motorabstimmung", subengine));
//                    if (subbrakes.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Bremsen", subbrakes));
//                    if (subsuspension.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Federung", subsuspension));
//                    if (subtransmission.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Getriebe", subtransmission));
//                    if (subturbo.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Turbo", subturbo));
//                    if (subboost.MenuItems.Count() > 0 && player.getCharacterData().AdminMode)
//                        menu.addMenuItem(new MenuMenuItem("Boost", subboost));
//                    if (subexhaust.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Auspuffanlage", subexhaust));
//                    if (subspoiler.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Antrieb" : "Spoiler", subspoiler));
//                    if (subsideskirt.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Luftfilter" : "Seitenverkleidung", subsideskirt));
//                    if (subfrontbumper.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Frontkotflügel" : "Frontstoßstange", subfrontbumper));
//                    if (subrearbumper.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Heckkotflügel" : "Heckstoßstange", subrearbumper));
//                    if (subgrille.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Anbauteile" : "Kühlergrill", subgrille));
//                    if (subhood.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Motorhaube", subhood));
//                    if (subroof.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Tank" : "Dach", subroof));
//                    if (subfender.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Rahmenteile" : "Tuningteile 1", subfender));
//                    if (subrightfender.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Tuningteile 2", subrightfender));
//                    if (subfrontwheels.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Vorderrad", subfrontwheels));
//                    if (subbackwheels.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Hinterrad", subbackwheels));
//                    if (subframe.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem(vehcls == 8 ? "Motorblock" : "Rahmen", subframe));
//                    if (subxenon.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Xenon", subxenon));
//                    if (subxenoncolor.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Xenon Farbe", subxenoncolor));

//                    if (VehicleTypeVehicle.Contains(vehcls)) {
//                        subdata = new Dictionary<string, dynamic> { { "target", "neoncolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 0 } };
//                        menu.addMenuItem(new ClickMenuItem($"Neon Farbe", "", "", "WORKSHOP_TUNING", MenuItemStyle.normal).withData(subdata));

//                        subdata = new Dictionary<string, dynamic> { { "target", "interiorcolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 0 } };
//                        menu.addMenuItem(new ClickMenuItem($"Interieur Farbe", "", "", "WORKSHOP_TUNING", MenuItemStyle.normal).withData(subdata));
//                    }

//                    if (subwindowtint.MenuItems.Count() > 0 && vehcls != 8)
//                        menu.addMenuItem(new MenuMenuItem("Scheibentönung", subwindowtint));
//                    if (subarmor.MenuItems.Count() > 0 && player.getCharacterData().AdminMode)
//                        menu.addMenuItem(new MenuMenuItem("Rüstung", subarmor));
//                    if (subtrimdesign.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Interieur", subtrimdesign));
//                    if (subornaments.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Verziehrung", subornaments));
//                    if (subdialdesign.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Armaturen", subdialdesign));
//                    if (substeeringwheel.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Lenkrad", substeeringwheel));
//                    if (subshiftlever.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Ganghebel", subshiftlever));
//                    if (subplaques.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Plakette", subplaques));
//                    if (subhydraulics.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Hydraulik", subhydraulics));
//                    if (sublivery.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Folierung", sublivery));
//                    if (subplate.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Nummernschider", subplate));
//                    if (subplateholders.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Schilderhalter", subplateholders));
//                    if (subcolor1.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Primärfarbe", subcolor1));
//                    if (subcolor2.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Sekundärfarbe", subcolor2));
//                    if (subdashboardcolor.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Armaturenbrett", subdashboardcolor));
//                    if (subtrimcolor.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Interieurdesign", subtrimcolor));
//                    if (subvanityplates.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Tuning Schilder", subvanityplates));
//                    if (subdoorspeaker.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Türlautsprecher", subdoorspeaker));
//                    if (subseats.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Sitze", subseats));
//                    if (subspeakers.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Lautsprecher", subspeakers));
//                    if (subtrunk.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Kofferraum", subtrunk));
//                    if (subengineblock.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Motorblock", subengineblock));
//                    if (substruts.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Stabilisatoren", substruts));
//                    if (subarchcover.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Radhaus", subarchcover));
//                    if (subaerials.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Antenne", subaerials));
//                    if (subtrim.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Zierleisten", subtrim));
//                    if (subtank.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Tank", subtank));
//                    if (subwindows.MenuItems.Count() > 0 && vehcls != 8)
//                        menu.addMenuItem(new MenuMenuItem("Verglasung", subwindows));
//                    if (subwheels.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Felgen", subwheels));

//                    if (player.getCharacterData().AdminMode) { // TODO - funktioniert nicht
//                        subdata = new Dictionary<string, dynamic> { { "target", "tiresmokecolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 0 } };
//                        menu.addMenuItem(new ClickMenuItem($"Reifenrauch Farbe", "", "", "WORKSHOP_TUNING", MenuItemStyle.normal).withData(subdata));
//                    }

//                    if (subhorns.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Hupe", subhorns));

//                    if (subextrasmodel.MenuItems.Count() > 0 && player.getCharacterData().AdminMode)
//                        menu.addMenuItem(new MenuMenuItem("Extras Fahrzeugmodell", subextrasmodel));
//                    if (subextras.MenuItems.Count() > 0 && player.getCharacterData().AdminMode)
//                        menu.addMenuItem(new MenuMenuItem("Extras Fahrzeug", subextras));

//                    player.showMenu(menu, false);
//                    return true;
//                }
//            }

//            return false;
//        }

//        private bool onTuning(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (data.ContainsKey("target") && data.ContainsKey("spot") && data.ContainsKey("vehicle") && data.ContainsKey("value")) {
//                string target = data["target"];
//                WorkshopSpot spot = data["spot"];
//                ChoiceVVehicle vehicle = data["vehicle"];
//                byte value = (byte)data["value"];
//                int type = -1;
//                bool check = false;

//                if (menuItemCefEvent is CheckBoxMenuItemEvent)
//                    check = ((CheckBoxMenuItemEvent)menuItemCefEvent).check;

//                if (data.ContainsKey("type"))
//                    type = data["type"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (target != "" && vehicle != null) {
//                    VehicleColoring coloring = vehicle.getColoring();
//                    VehicleTuning tuning = vehicle.getTuning();

//                    if (coloring != null && tuning != null) {
//                        if (spot != null) {
//                            if (!spot.VehicleColoringClone.ContainsKey(vehicle)) {
//                                spot.VehicleColoringClone.Add(vehicle, coloring);

//                                coloring = coloring.Clone();
//                                coloring.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_COLORING, coloring);
//                            }

//                            if (!spot.VehicleTuningClone.ContainsKey(vehicle)) {
//                                spot.VehicleTuningClone.Add(vehicle, tuning);

//                                tuning = tuning.Clone();
//                                tuning.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_TUNING, tuning);
//                            }
//                        }

//                        if (target == "engine")
//                            return tuning.setEngine(vehicle, value);
//                        else if (target == "brakes")
//                            return tuning.setBrakes(vehicle, value);
//                        else if (target == "suspension")
//                            return tuning.setSuspension(vehicle, value);
//                        else if (target == "transmission")
//                            return tuning.setTransmission(vehicle, value);
//                        else if (target == "turbo")
//                            return tuning.setTurbo(vehicle, value);
//                        else if (target == "boost")
//                            return tuning.setBoost(vehicle, value);
//                        else if (target == "exhaust")
//                            return tuning.setExhaust(vehicle, value);
//                        else if (target == "spoilers")
//                            return tuning.setSpoilers(vehicle, value);
//                        else if (target == "sideskirt")
//                            return tuning.setSideSkirt(vehicle, value);
//                        else if (target == "frontbumper")
//                            return tuning.setFrontBumper(vehicle, value);
//                        else if (target == "rearbumper")
//                            return tuning.setRearBumper(vehicle, value);
//                        else if (target == "grille")
//                            return tuning.setGrille(vehicle, value);
//                        else if (target == "hood")
//                            return tuning.setHood(vehicle, value);
//                        else if (target == "roof")
//                            return tuning.setRoof(vehicle, value);
//                        else if (target == "fender")
//                            return tuning.setFender(vehicle, value);
//                        else if (target == "rightfender")
//                            return tuning.setRightFender(vehicle, value);
//                        else if (target == "frontwheels")
//                            return tuning.setFrontWheels(vehicle, value);
//                        else if (target == "backwheels")
//                            return tuning.setBackWheels(vehicle, value);
//                        else if (target == "frame")
//                            return tuning.setFrame(vehicle, value);
//                        else if (target == "xenon")
//                            return tuning.setXenon(vehicle, value);
//                        else if (target == "armor")
//                            return tuning.setArmor(vehicle, value);
//                        else if (target == "trimdesign")
//                            return tuning.setTrimDesign(vehicle, value);
//                        else if (target == "ornaments")
//                            return tuning.setOrnaments(vehicle, value);
//                        else if (target == "dialdesign")
//                            return tuning.setDialDesign(vehicle, value);
//                        else if (target == "steeringwheel")
//                            return tuning.setSteeringWheel(vehicle, value);
//                        else if (target == "subshiftlever")
//                            return tuning.setShiftLever(vehicle, value);
//                        else if (target == "plaques")
//                            return tuning.setPlaques(vehicle, value);
//                        else if (target == "hydraulics")
//                            return tuning.setHydraulics(vehicle, value);
//                        else if (target == "livery")
//                            return tuning.setLivery(vehicle, value);
//                        else if (target == "plate")
//                            return tuning.setPlate(vehicle, value);
//                        else if (target == "plateholders")
//                            return tuning.setPlateHolders(vehicle, value);
//                        else if (target == "color1")
//                            return tuning.setColor1(vehicle, value);
//                        else if (target == "color2")
//                            return tuning.setColor2(vehicle, value);
//                        else if (target == "dashboardcolor")
//                            return tuning.setDashboardColor(vehicle, value);
//                        else if (target == "trimcolor")
//                            return tuning.setTrimColor(vehicle, value);
//                        else if (target == "vanityplates")
//                            return tuning.setVanityPlates(vehicle, value);
//                        else if (target == "doorspeaker")
//                            return tuning.setDoorSpeaker(vehicle, value);
//                        else if (target == "seats")
//                            return tuning.setSeats(vehicle, value);
//                        else if (target == "speakers")
//                            return tuning.setSpeakers(vehicle, value);
//                        else if (target == "trunk")
//                            return tuning.setTrunk(vehicle, value);
//                        else if (target == "engineblock")
//                            return tuning.setEngineBlock(vehicle, value);
//                        else if (target == "struts")
//                            return tuning.setStruts(vehicle, value);
//                        else if (target == "archcover")
//                            return tuning.setArchCover(vehicle, value);
//                        else if (target == "aerials")
//                            return tuning.setAerials(vehicle, value);
//                        else if (target == "trim")
//                            return tuning.setTrim(vehicle, value);
//                        else if (target == "tank")
//                            return tuning.setTank(vehicle, value);
//                        else if (target == "windows")
//                            return tuning.setWindows(vehicle, value);
//                        else if (target == "wheels")
//                            return tuning.setWheels(vehicle, type, value);
//                        else if (target == "tiresmokecolor")
//                            player.showColorPicker(new ColorPicker(ColorPickerType.SketchPicker, tuning.NeonColor, VehicleAllColors.ToArray()).withData(data));
//                        else if (target == "xenoncolor")
//                            return tuning.setLightColor(vehicle, value);
//                        else if (target == "windowtint")
//                            return tuning.setWindowTint(vehicle, value);
//                        else if (target == "neoncolor")
//                            player.showColorPicker(new ColorPicker(ColorPickerType.SketchPicker, tuning.NeonColor, VehicleAllColors.ToArray()).withData(data));
//                        else if (target == "interiorcolor")
//                            player.showColorPicker(new ColorPicker(ColorPickerType.SmallCirclePicker, VehicleAllColors[tuning.InteriorColor], VehicleAllColors.ToArray()).withData(data));
//                        else if (target == "extrasmodel")
//                            return tuning.setExtrasModel(vehicle, value, check);
//                        else if (target == "extras")
//                            return tuning.setExtras(vehicle, value, check);

//                        else if (target == "horns") {
//                            tuning.setHorns(vehicle, value);
//                            ChoiceVAPI.emitClientEventToAll("TEST_HORN", vehicle);

//                        } else if (target == "removehood") {
//                            VehicleDamage damage = vehicle.getDamage();
//                            if (damage != null) {
//                                return damage.removeHood(vehicle);
//                            }

//                        } else if (target == "removetrunk") {
//                            VehicleDamage damage = vehicle.getDamage();
//                            if (damage != null) {
//                                return damage.removeTrunk(vehicle);
//                            }
//                        }
//                    }
//                }
//            }

//            return false;
//        }

//        #endregion

//        #region Vehicle coloring methods

//        public static bool coloringMenu(IPlayer player, ChoiceVVehicle vehicle, WorkshopSpot spot) {
//            if (vehicle != null && vehicle.Exists() == false)
//                vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//            if (vehicle != null) {
//                VehicleColoring coloring = vehicle.getColoring();

//                if (coloring != null && !coloring.Sanding) {
//                    int vehcls = vehicle.getClass();

//                    var menu = new Menu("Lackierplatz", "Hier können Fahrzeuge lackiert werden.");
//                    var data = new Dictionary<string, dynamic> { { "spot", spot }, { "vehicle", vehicle } };
//                    var subdata = new Dictionary<string, dynamic>();

//                    if (spot != null) {
//                        if (!coloring.Sanded1) {
//                            if (vehicle.GetMod(VehicleModType.Livery) > 0)
//                                menu.addMenuItem(new ClickMenuItem("Folie entfernen und reinigen", "", "", "WORKSHOP_SPOT_SAND", MenuItemStyle.normal).withData(data));
//                            else
//                                menu.addMenuItem(new ClickMenuItem("Abschleifen Stufe 1", "", "", "WORKSHOP_SPOT_SAND", MenuItemStyle.normal).withData(data));

//                            player.showMenu(menu);
//                            return true;
//                        }

//                        if (!coloring.Sanded2) {
//                            menu.addMenuItem(new ClickMenuItem("Abschleifen Stufe 2", "", "", "WORKSHOP_SPOT_SAND", MenuItemStyle.normal).withData(data));

//                            player.showMenu(menu);
//                            return true;
//                        }

//                        if (!coloring.Sanded3) {
//                            menu.addMenuItem(new ClickMenuItem("Abschleifen Stufe 3", "", "", "WORKSHOP_SPOT_SAND", MenuItemStyle.normal).withData(data));

//                            player.showMenu(menu);
//                            return true;
//                        }
//                    }

//                    // Create submenus
//                    var subprimarycolor = new Menu("Primärfarbe", "");
//                    subdata = new Dictionary<string, dynamic> { { "target", "primarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 1 } };
//                    subprimarycolor.addMenuItem(new ClickMenuItem("Lackieren Standard", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "primarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 2 } };
//                    subprimarycolor.addMenuItem(new ClickMenuItem("Lackieren Metallic", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "primarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 3 } };
//                    subprimarycolor.addMenuItem(new ClickMenuItem("Lackieren Matt", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "primarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 4 } };
//                    subprimarycolor.addMenuItem(new ClickMenuItem("Lackieren Metall", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "primarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 5 } };
//                    subprimarycolor.addMenuItem(new ClickMenuItem("Lackieren Chrome", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "primarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 6 } };
//                    subprimarycolor.addMenuItem(new ClickMenuItem("Lackieren Farbmischer", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    var subseconarycolor = new Menu("Sekundärfarbe", "");
//                    subdata = new Dictionary<string, dynamic> { { "target", "secondarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 1 } };
//                    subseconarycolor.addMenuItem(new ClickMenuItem("Lackieren Standard", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "secondarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 2 } };
//                    subseconarycolor.addMenuItem(new ClickMenuItem("Lackieren Metallic", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "secondarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 3 } };
//                    subseconarycolor.addMenuItem(new ClickMenuItem("Lackieren Matt", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "secondarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 4 } };
//                    subseconarycolor.addMenuItem(new ClickMenuItem("Lackieren Metall", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "secondarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 5 } };
//                    subseconarycolor.addMenuItem(new ClickMenuItem("Lackieren Chrome", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "secondarycolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 6 } };
//                    subseconarycolor.addMenuItem(new ClickMenuItem("Lackieren Farbmischer", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    var sublivery = new Menu("Sonderlackierung", "");
//                    for (int i = 1; i <= 50; i++) {
//                        subdata = new Dictionary<string, dynamic> { { "target", "livery" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                        sublivery.addMenuItem(new ClickMenuItem(i == 1 ? "Keine Sonderlackierung" : $"Sonderlackierung Variante {i - 1}", "", "", "WORKSHOP_COLOR", coloring.Livery == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                    }

//                    var subrooflivery = new Menu("Dachlackierung", "");
//                    for (int i = 1; i <= 50; i++) {
//                        subdata = new Dictionary<string, dynamic> { { "target", "rooflivery" }, { "spot", spot }, { "vehicle", vehicle }, { "value", i } };
//                        subrooflivery.addMenuItem(new ClickMenuItem(i == 1 ? "Keine Dachlackierung" : $"Dachlackierung Variante {i - 1}", "", "", "WORKSHOP_COLOR", coloring.RoofLivery == i ? MenuItemStyle.green : MenuItemStyle.normal, true).withData(subdata));
//                    }

//                    // Add submenus to menu
//                    if (subprimarycolor.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Primärfarbe", subprimarycolor));
//                    if (subseconarycolor.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Sekundärfarbe", subseconarycolor));

//                    subdata = new Dictionary<string, dynamic> { { "target", "pearl" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 0 } };
//                    menu.addMenuItem(new ClickMenuItem($"Perl-Effekt", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    subdata = new Dictionary<string, dynamic> { { "target", "wheelcolor" }, { "spot", spot }, { "vehicle", vehicle }, { "value", 0 } };
//                    menu.addMenuItem(new ClickMenuItem($"Felgen Farbe", "", "", "WORKSHOP_COLOR", MenuItemStyle.normal).withData(subdata));

//                    if (sublivery.MenuItems.Count() > 0)
//                        menu.addMenuItem(new MenuMenuItem("Sonderlackierung", sublivery));
//                    if (subrooflivery.MenuItems.Count() > 0 && vehcls != 8)
//                        menu.addMenuItem(new MenuMenuItem("Dachlackierung", subrooflivery));

//                    player.showMenu(menu, false);
//                    return true;
//                }
//            }

//            return false;
//        }

//        private bool onColorSelected(IPlayer player, string itemEvent, int itemId, Dictionary<string, dynamic> data, Rgba colorRgb, string colorHex, ColorPickerCefEvent colorpickerCefEvent) {
//            if (data.ContainsKey("target") && data.ContainsKey("spot") && data.ContainsKey("vehicle") && data.ContainsKey("value")) {
//                string target = data["target"];
//                WorkshopSpot spot = data["spot"];
//                ChoiceVVehicle vehicle = data["vehicle"];
//                byte value = (byte)data["value"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (target != "" && vehicle != null) {
//                    VehicleColoring coloring = vehicle.getColoring();
//                    VehicleTuning tuning = vehicle.getTuning();
//                    VehicleDamage damage = vehicle.getDamage();

//                    if (coloring != null && tuning != null && damage != null) {
//                        if (spot != null) {
//                            if (!spot.VehicleColoringClone.ContainsKey(vehicle)) {
//                                spot.VehicleColoringClone.Add(vehicle, coloring);

//                                coloring = coloring.Clone();
//                                coloring.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_COLORING, coloring);
//                            }

//                            if (!spot.VehicleTuningClone.ContainsKey(vehicle)) {
//                                spot.VehicleTuningClone.Add(vehicle, tuning);

//                                tuning = tuning.Clone();
//                                tuning.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_TUNING, tuning);
//                            }
//                        }

//                        if (target == "primarycolor") {
//                            damage.repairDirtLevel(vehicle);

//                            if (value >= 1 && value <= 4) {
//                                int index = VehicleAllColors.IndexOf(colorHex.ToLower());
//                                if (index >= 0)
//                                    coloring.setPrimaryColor(vehicle, (byte)index);
//                                else
//                                    coloring.setPrimaryColor(vehicle, colorRgb);

//                            } else if (value == 6) {
//                                coloring.setPrimaryColor(vehicle, colorRgb);
//                            }

//                        } else if (target == "secondarycolor") {
//                            damage.repairDirtLevel(vehicle);

//                            if (value >= 1 && value <= 4) {
//                                int index = VehicleAllColors.IndexOf(colorHex.ToLower());
//                                if (index >= 0)
//                                    coloring.setSecondaryColor(vehicle, (byte)index);
//                                else
//                                    coloring.setSecondaryColor(vehicle, colorRgb);

//                            } else if (value == 6) {
//                                coloring.setSecondaryColor(vehicle, colorRgb);
//                            }

//                        } else if (target == "pearl") {
//                            damage.repairDirtLevel(vehicle);

//                            int index = VehicleAllColors.IndexOf(colorHex.ToLower());
//                            if (index >= 0)
//                                coloring.setPearlColor(vehicle, (byte)index);

//                        } else if (target == "neoncolor") {
//                            tuning.setNeonColor(vehicle, colorRgb);

//                        } else if (target == "interiorcolor") {
//                            int index = VehicleAllColors.IndexOf(colorHex.ToLower());
//                            if (index >= 0)
//                                tuning.setInteriorColor(vehicle, (byte)index);

//                        } else if (target == "wheelcolor") {
//                            int index = VehicleAllColors.IndexOf(colorHex.ToLower());
//                            if (index >= 0)
//                                coloring.setWheelColor(vehicle, (byte)index);

//                        } else if (target == "tiresmokecolor") {
//                            tuning.setTireSmokeColor(vehicle, colorRgb);
//                        }

//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        private bool onColoring(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (data.ContainsKey("target") && data.ContainsKey("spot") && data.ContainsKey("vehicle") && data.ContainsKey("value")) {
//                string target = data["target"];
//                WorkshopSpot spot = data["spot"];
//                ChoiceVVehicle vehicle = data["vehicle"];
//                byte value = (byte)data["value"];

//                if (vehicle != null && vehicle.Exists() == false)
//                    vehicle = ChoiceVAPI.FindVehicleById(vehicle.getId());

//                if (target != "" && vehicle != null) {
//                    VehicleColoring coloring = vehicle.getColoring();
//                    VehicleTuning tuning = vehicle.getTuning();

//                    if (coloring != null && tuning != null) {
//                        if (spot != null) {
//                            if (!spot.VehicleColoringClone.ContainsKey(vehicle)) {
//                                spot.VehicleColoringClone.Add(vehicle, coloring);

//                                coloring = coloring.Clone();
//                                coloring.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_COLORING, coloring);
//                            }

//                            if (!spot.VehicleTuningClone.ContainsKey(vehicle)) {
//                                spot.VehicleTuningClone.Add(vehicle, tuning);

//                                tuning = tuning.Clone();
//                                tuning.NoSave = true;

//                                vehicle.setData(DATA_VEHICLE_TUNING, tuning);
//                            }
//                        }

//                        if (target == "primarycolor") {
//                            if (value == 1)     // Standard
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleDefaultColors.ToArray()).withData(data));
//                            if (value == 2)     // Metallic
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleMetallicColors.ToArray()).withData(data));
//                            if (value == 3)     // Mate
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleMatteColors.ToArray()).withData(data));
//                            if (value == 4)     // Metal
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleMetalColors.ToArray()).withData(data));
//                            if (value == 5)     // Chrome
//                                coloring.setPrimaryColor(vehicle, 120);
//                            if (value == 6)     // Rgb
//                                player.showColorPicker(new ColorPicker(ColorPickerType.SketchPicker, coloring.PrimaryColorRGBA).withData(data));

//                        } else if (target == "secondarycolor") {
//                            if (value == 1)     // Standard
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleDefaultColors.ToArray()).withData(data));
//                            if (value == 2)     // Metallic
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleMetallicColors.ToArray()).withData(data));
//                            if (value == 3)     // Mate
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleMatteColors.ToArray()).withData(data));
//                            if (value == 4)     // Metal
//                                player.showColorPicker(new ColorPicker(ColorPickerType.CirclePicker, VehicleAllColors[coloring.PrimaryColor], VehicleMetalColors.ToArray()).withData(data));
//                            if (value == 5)     // Chrome
//                                coloring.setSecondaryColor(vehicle, 120);
//                            if (value == 6)     // Rgb
//                                player.showColorPicker(new ColorPicker(ColorPickerType.SketchPicker, coloring.PrimaryColorRGBA).withData(data));

//                        } else if (target == "pearl") {
//                            player.showColorPicker(new ColorPicker(ColorPickerType.SmallCirclePicker, VehicleAllColors[coloring.PearlColor], VehicleAllColors.ToArray()).withData(data));

//                        } else if (target == "wheelcolor") {
//                            player.showColorPicker(new ColorPicker(ColorPickerType.SmallCirclePicker, VehicleAllColors[coloring.PearlColor], VehicleAllColors.ToArray()).withData(data));

//                        } else if (target == "livery") {
//                            coloring.setLivery(vehicle, value);

//                        } else if (target == "rooflivery") {
//                            coloring.setRoofLivery(vehicle, value);
//                        }

//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        #endregion

//        public static int createWorkshop(IPlayer player, string name, int workshoptype, WorkshopOwnerType ownertype, int ownerid, string ownername) {
//            try {
//                int id = 0;
//                var pos = player.Position;
//                var rot = player.Rotation;

//                pos.Z -= 1f;
//                rot.Yaw = (180 - ChoiceVAPI.radiansToDegrees(rot.Yaw));

//                using (var db = new ChoiceVDb()) {
//                    var shops = new configworkshops {
//                        name = name,
//                        type = workshoptype,
//                        ownerId = ownerid,
//                        ownerType = (int)ownertype,
//                        ownerName = ownername,
//                        position = pos.ToJson(),
//                        rotation = rot.ToJson(),
//                        inventoryId = 0,
//                    };

//                    db.configworkshops.Add(shops);
//                    db.SaveChanges();

//                    id = shops.id;

//                    if (id > 0) {
//                        Workshop shop = new Workshop(id, name, workshoptype, ownertype, ownerid, ownername, pos, rot, 0);
//                        if (shop != null) {
//                            AllWorkshops.Add(shop);

//                            return id;
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return -1;
//        }

//        public static int createWorkshopSpot(int workshopid, WorkshopSpotType spottype, Position pos, Rotation rot, float width, float height) {
//            try {
//                int id = 0;

//                Workshop shop = AllWorkshops.FirstOrDefault(v => v.Id == workshopid);
//                if (shop != null) {
//                    rot.Yaw = (180 - ChoiceVAPI.radiansToDegrees(rot.Yaw));

//                    using (var db = new ChoiceVDb()) {
//                        var spots = new configworkshopspots {
//                            workshopId = workshopid,
//                            spotType = (int)spottype,
//                            position = pos.ToJson(),
//                            rotation = rot.ToJson(),
//                            width = width,
//                            height = height,
//                        };

//                        db.configworkshopspots.Add(spots);
//                        db.SaveChanges();

//                        id = spots.id;

//                        if (id > 0) {
//                            WorkshopSpot spot = new WorkshopSpot(id, workshopid, spottype, pos, rot, width, height);
//                            if (spot != null) {
//                                AllWorkshopSpots.Add(spot);

//                                return id;
//                            }
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return -1;
//        }

//        public static void deleteWorkshop(int workshopid) {
//            try {
//                var shop = AllWorkshops.FirstOrDefault(v => v.Id == workshopid);
//                if (shop != null) {
//                    if (shop.Manager != null)
//                        PedController.destroyPed(shop.Manager);

//                    if (shop.ManagerInventory != null)
//                        InventoryController.unloadInventory(shop.ManagerInventory);

//                    using (var db = new ChoiceVDb()) {
//                        var row = db.configworkshopspots.Where(v => v.workshopId == workshopid);
//                        if (row != null) {
//                            db.configworkshopspots.RemoveRange(row);
//                            db.SaveChanges();
//                        }
//                    }

//                    AllWorkshopSpots.RemoveAll(v => v.workshopId == workshopid);

//                    using (var db = new ChoiceVDb()) {
//                        var row = db.configworkshops.FirstOrDefault(v => v.id == workshopid);
//                        if (row != null) {
//                            db.configworkshops.Remove(row);
//                            db.SaveChanges();
//                        }
//                    }

//                    AllWorkshops.RemoveAll(v => v.Id == workshopid);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public static void deleteWorkshopSpot(int workshopid, int spotid) {
//            try {
//                var shop = AllWorkshops.FirstOrDefault(v => v.Id == workshopid);
//                if (shop != null) {
//                    using (var db = new ChoiceVDb()) {
//                        var row = db.configworkshopspots.Where(v => v.id == spotid);
//                        if (row != null) {
//                            db.configworkshopspots.RemoveRange(row);
//                            db.SaveChanges();
//                        }
//                    }

//                    AllWorkshopSpots.RemoveAll(v => v.Id == spotid);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public static void reloadWorkshops() {
//            try {
//                foreach (var shop in AllWorkshops) {
//                    if (shop.Manager != null)
//                        PedController.destroyPed(shop.Manager);

//                    if (shop.ManagerInventory != null)
//                        InventoryController.unloadInventory(shop.ManagerInventory);
//                }

//                AllWorkshopSpots.Clear();
//                AllWorkshops.Clear();

//                using (var db = new ChoiceVDb()) {
//                    foreach (var row in db.configworkshops) {
//                        Workshop shop = new Workshop(row.id, row.name, row.type, (WorkshopOwnerType)row.ownerType, row.ownerId, row.ownerName, row.position.FromJson<Position>(), row.rotation.FromJson<Rotation>(), row.inventoryId);
//                        if (shop != null) {
//                            AllWorkshops.Add(shop);

//                            using (var tmpdb = new ChoiceVDb()) {
//                                foreach (var tmprow in tmpdb.configworkshopspots.Where(r => r.workshopId == row.id)) {
//                                    WorkshopSpot spot = new WorkshopSpot(tmprow.id, tmprow.workshopId, (WorkshopSpotType)tmprow.spotType, tmprow.position.FromJson<Position>(), tmprow.rotation.FromJson<Rotation>(), tmprow.width, tmprow.height);

//                                    if (spot != null)
//                                        AllWorkshopSpots.Add(spot);
//                                }
//                            }
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private static void loadWorkshops() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    foreach (var row in db.configworkshops) {
//                        Workshop shop = new Workshop(row.id, row.name, row.type, (WorkshopOwnerType)row.ownerType, row.ownerId, row.ownerName, row.position.FromJson<Position>(), row.rotation.FromJson<Rotation>(), row.inventoryId);
//                        if (shop != null) {
//                            AllWorkshops.Add(shop);

//                            using (var tmpdb = new ChoiceVDb()) {
//                                foreach (var tmprow in tmpdb.configworkshopspots.Where(r => r.workshopId == row.id)) {
//                                    WorkshopSpot spot = new WorkshopSpot(tmprow.id, tmprow.workshopId, (WorkshopSpotType)tmprow.spotType, tmprow.position.FromJson<Position>(), tmprow.rotation.FromJson<Rotation>(), tmprow.width, tmprow.height);

//                                    if (spot != null)
//                                        AllWorkshopSpots.Add(spot);
//                                }
//                            }
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }
//    }
//}
