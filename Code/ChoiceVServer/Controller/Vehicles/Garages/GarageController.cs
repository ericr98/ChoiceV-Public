using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Garages.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.AnalyseSpots;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace ChoiceVServer.Controller.Garages {
        class GarageController : ChoiceVScript {
        public static Dictionary<int, Garage> AllGarages = new Dictionary<int, Garage>();

        public GarageController() {
            EventController.addMenuEvent("GARAGE_INTERACTION_PARK_IN", onGarageParkIn);
            EventController.addMenuEvent("GARAGE_INTERACTION_PARK_OUT", onGarageParkOut);

            reloadGarages();

            InvokeController.AddTimedInvoke("GarageUpdater", updateGarages, TimeSpan.FromMinutes(10), true);
            InvokeController.AddTimedInvoke("GarageLongerUpdate", updateLongerGarage, TimeSpan.FromHours(1), true);
            
            #region SupportStuff

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Fahrzeuge,
                    "Garagen",
                    generateGarageMenu
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_GARAGE", onSupportCreateGarage);
            EventController.addMenuEvent("SUPPORT_CREATE_GARAGE_SPOT", onSupportCreateGarageSpot);

            EventController.addMenuEvent("SUPPORT_GARAGE_TELEPORT", onSupportGarageTeleport);
            EventController.addMenuEvent("SUPPORT_DELETE_GARAGE", onSupportDeleteGarage);

            EventController.addMenuEvent("SUPPORT_GARAGE_SPOT_TELEPORT", onSupportGarageSpotTeleport);
            EventController.addMenuEvent("SUPPORT_DELETE_GARAGE_SPOT", onSupportDeleteGarageSpot);

            #endregion

        }

        private void updateGarages(IInvoke obj) {
            foreach(var garage in AllGarages.Values) {
                garage.update();
            }
        }

        private void updateLongerGarage(IInvoke invoke) {
            var toRemoveList = new List<vehicle>();

            using (var db = new ChoiceVDb()) {

                foreach (var garage in db.configgarages.Include(g => g.vehicles)) {
                    foreach (var veh in garage.vehicles.Where(v => v.lastMoved + TimeSpan.FromHours(12) < DateTime.Now)) {
                        if (veh.randomlySpawnedDate != null) {
                            toRemoveList.Add(veh);
                        }
                    }
                }
            }

            foreach (var veh in toRemoveList) {
                VehicleController.removeVehicle(veh);
            }
        }

        public static Garage createGarage(IPlayer player, string name, Constants.GarageType garageType, Constants.GarageOwnerType ownerType, int ownerId, int slots, bool withNpc) {
            try {
                Position pos = Position.Zero;
                DegreeRotation rot = Rotation.Zero;

                if(withNpc) {
                    pos = player.Position;
                    rot = (DegreeRotation)player.Rotation;
                    
                    pos.Z -= 1f;
                }
                
                Garage garage;

                using(var db = new ChoiceVDb()) {
                    var dbGarage = new configgarage {
                        name = name,
                        ownerId = ownerId,
                        ownerType = (int)ownerType,
                        position = pos.ToJson(),
                        rotation = rot.Yaw,
                        slots = slots,
                        type = (int)garageType,
                    };

                    db.configgarages.Add(dbGarage);
                    db.SaveChanges();

                    garage = new Garage(dbGarage.id, name, garageType, ownerType, ownerId, slots, slots, pos, rot.Yaw);

                    AllGarages.Add(dbGarage.id, garage);

                    player.sendNotification(Constants.NotifactionTypes.Success, $"Garage erstellt: Id:{dbGarage.id}, Name:{name}, OwnerId:{ownerId}, OwnerType:{ownerType}, Type:{garageType}, Slots:{slots}", "Garage erstellt");
                }

                if(garage.OwnerType == Constants.GarageOwnerType.Company) {
                    var company = CompanyController.findCompanyById(ownerId);
                    if(company != null) {
                        var module = company.getFunctionality<GarageFunctionality>();

                        if(module != null) {
                            module.addGarage(garage);
                        }
                    }
                }


                return garage;
            } catch(Exception e) {
                Logger.logException(e);
                return null;
            }
        }

        public static GarageSpawnSpot createGarageSpot(IPlayer player, int garageId, Position position, float heading, float width, float height) {
            try {
                var pos = position;

                using(var db = new ChoiceVDb()) {
                    var newSpot = new configgaragespawnspot {
                        garageId = garageId,
                        width = width,
                        height = height,
                        position = position.ToJson(),
                        rotation = heading,
                    };

                    db.configgaragespawnspots.Add(newSpot);
                    db.SaveChanges();

                    var garSpot = new GarageSpawnSpot(newSpot.id, garageId, pos, heading, newSpot);

                    AllGarages[garageId].SpawnPositions.Add(garSpot);

                    ChoiceVAPI.SendChatMessageToPlayer(player, $"Garagenspot erstellt: Id:{newSpot.id}, GarageId:{garageId}");
                    return garSpot;
                }
            } catch(Exception e) {
                Logger.logException(e);
                return null;
            }
        }

        public static bool parkVehicleInGarage(ChoiceVVehicle vehicle, int garageId) {
            if(!AllGarages.ContainsKey(garageId)) {
                return false;
            }

            var garage = AllGarages[garageId];
            if(garage is null) {
                return false;
            }

            if(garage.SlotsFree <= 0) {
                return false;
            }

            using(var db = new ChoiceVDb()) {
                var dbVeh = db.vehicles.Find(vehicle.VehicleId);
                if(dbVeh is null) {
                    return false;
                }
                
                dbVeh.lastMoved = DateTime.Now;
                dbVeh.garageId = garageId;
                
                db.SaveChanges();
            }

            garage.SlotsFree++;
            
            VehicleController.despawnVehicle(vehicle);

            return true;

        }

        /// <summary>
        /// Unparks a vehicle by its vehicleId
        /// </summary>
        public static bool unparkVehicleFromGarage(int vehicleId) {
            using(var db = new ChoiceVDb()) {
                var vehicle = db.vehicles
                                    .Include(v => v.model)
                                        .ThenInclude(m => m._class)
                                    .Include(v => v.model)
                                        .ThenInclude(m => m.configvehiclesseats)
                                    .Include(v => v.vehiclesdamagebasis)
                                    .Include(v => v.vehiclestuningmods)
                                    .Include(v => v.vehiclestuningbase)
                                    .Include(v => v.vehiclescoloring)
                                    .Include(v => v.vehiclesdata).FirstOrDefault(veh => veh.id == vehicleId);

                if(vehicle != null && vehicle.garageId != null && AllGarages.ContainsKey(vehicle.garageId ?? -1)) {
                    var garage = AllGarages[vehicle.garageId ?? -1];
                    if(garage != null) {
                        var spawnSpot = findEmptySpawnSpot(garage);

                        if(garage != null && spawnSpot != null) {
                            Position pos = spawnSpot.SpawnPosition + new Position(0, 0, 0.5f);
                            Rotation rot = new Rotation(0, 0, spawnSpot.SpawnRotation);

                            if(rot.Yaw > 180)
                                rot.Yaw = ChoiceVAPI.degreesToRadians(rot.Yaw - 360);
                            else
                                rot.Yaw = ChoiceVAPI.degreesToRadians(rot.Yaw);

                            vehicle.garageId = null;
                            vehicle.position = pos.ToJson();
                            vehicle.rotation = rot.ToJson();

                            db.SaveChanges();

                            var spawnVehicle = VehicleController.spawnVehicle(vehicle);

                            garage.SlotsFree--;

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static Position getGaragePosition(int id) {
            return AllGarages[id].Manager.Position;
        }

        //finds first empty SpawnSpot 
        private static GarageSpawnSpot findEmptySpawnSpot(Garage garage) {
            return garage.SpawnPositions.FirstOrDefault(p => p.isOccupied() == false);
        }

        private bool onGarageParkIn(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            try {
                if(data.ContainsKey("garage") && data.ContainsKey("vehicle")) {
                    int garageId = data["garage"];
                    ChoiceVVehicle vehicle = data["vehicle"];

                    if(vehicle != null) {
                        parkVehicleInGarage(vehicle, garageId);
                        return true;
                    }
                }
            } catch(Exception e) {
                Logger.logException(e);
            }

            return false;
        }

        private bool onGarageParkOut(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            try {
                if(data.ContainsKey("garage") && data.ContainsKey("vehicle")) {
                    int garageId = data["garage"];
                    int vehicle = data["vehicle"];

                    if(garageId != -1 && vehicle != -1) {
                        if(!unparkVehicleFromGarage(vehicle)) {
                            player.sendBlockNotification("Fahrzeug konnte nicht aus der Garage ausgeparkt werden", "Ausparken nicht möglich", Constants.NotifactionImages.Car);
                        }
                    }
                }
            } catch(Exception e) {
                Logger.logException(e);
            }

            return false;
        }

        public static void deleteGarage(int garageid) {
            try {
                if(AllGarages != null && AllGarages.ContainsKey(garageid)) {
                    var garage = AllGarages[garageid];
                    if(garage != null) {
                        if(garage.Manager != null)
                            PedController.destroyPed(garage.Manager);

                        using(var db = new ChoiceVDb()) {
                            var row = db.configgaragespawnspots.Where(v => v.garageId == garageid);
                            if(row != null) {
                                db.configgaragespawnspots.RemoveRange(row);
                                db.SaveChanges();
                            }
                        }

                        using(var db = new ChoiceVDb()) {
                            var row = db.configgarages.FirstOrDefault(v => v.id == garageid);
                            if(row != null) {
                                db.configgarages.RemoveRange(row);
                                db.SaveChanges();
                            }
                        }

                        AllGarages.Remove(garageid);
                    }
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        public static void deleteGarageSpot(int garageid, int spotid) {
            try {
                if(AllGarages != null && AllGarages.ContainsKey(garageid)) {
                    var garage = AllGarages[garageid];
                    if(garage != null && garage.SpawnPositions != null) {
                        using(var db = new ChoiceVDb()) {
                            var row = db.configgaragespawnspots.Where(v => v.garageId == garageid && v.id == spotid);
                            if(row != null) {
                                db.configgaragespawnspots.RemoveRange(row);
                                db.SaveChanges();
                            }
                        }

                        for(int i = 0; i < garage.SpawnPositions.Count(); i++) {
                            if(garage.SpawnPositions[i].GarageId == garageid && garage.SpawnPositions[i].Id == spotid)
                                garage.SpawnPositions.RemoveAt(i);
                        }
                    }
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        public static void reloadGarages() {
            try {
                foreach(var garage in AllGarages.Values) {
                    if(garage.Manager != null)
                        PedController.destroyPed(garage.Manager);
                }

                AllGarages.Clear();

                using(var db = new ChoiceVDb()) {
                    foreach(var row in db.configgarages.Include(g => g.vehicles)) {
                        AllGarages.Add(row.id, new Garage(row.id, row.name, (Constants.GarageType)row.type, (Constants.GarageOwnerType)row.ownerType, row.ownerId, row.slots, row.slots - row.vehicles.Count, row.position.FromJson<Position>(), row.rotation));
                    }

                    foreach(var row in db.configgaragespawnspots) {
                        if(AllGarages.ContainsKey(row.garageId))
                            AllGarages[row.garageId].SpawnPositions.Add(new GarageSpawnSpot(row.id, row.garageId, row.position.FromJson(), row.rotation, row));
                    }
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        public static Garage findGarageById(int garageId) {
            if(AllGarages.ContainsKey(garageId)) {
                return AllGarages[garageId];
            } else {
                return null;
            }
        }

        #region SupportStuff

        private Menu generateGarageMenu(IPlayer player) {
            var menu = new Menu("Garagen", "Garagenverwaltung");
            var garageCreateMenu = getGarageGenerateMenu(player, Constants.GarageOwnerType.Public, -1);
            menu.addMenuItem(new MenuMenuItem(garageCreateMenu.Name, garageCreateMenu, MenuItemStyle.green));

            foreach(var garage in AllGarages.Values) {
                var garageMenu = garage.getSupportMenu();
                menu.addMenuItem(new MenuMenuItem(garage.Name, garageMenu, MenuItemStyle.green));
            }
            
            return menu;
        }
        
        public static Menu getGarageGenerateMenu(IPlayer player, Constants.GarageOwnerType ownerType, int ownerId) {
            var createMenu = new Menu("Garage erstellen", "Erstelle eine Garage");
            createMenu.addMenuItem(new InputMenuItem("Name", "Name der Garage", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Slots", "Anzahl der Slots", "", InputMenuItemTypes.number, ""));
            createMenu.addMenuItem(new ListMenuItem("Typ", "Garagentyp", Enum.GetValues<Constants.GarageType>().Select(v => v.ToString()).ToArray(), ""));
            createMenu.addMenuItem(new CheckBoxMenuItem("Mit NPC", "Garage mit NPC erstellen", true, ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die Garage. Deine Position und Rotation wird für den Manager genommen", "", "SUPPORT_CREATE_GARAGE", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> {
                    { "OwnerType", ownerType },
                    { "OwnerId", ownerId },
                })
                .needsConfirmation("Garage erstellen", "Garage wirklich erstellen?"));
            
            return createMenu;
        }
        

        private bool onSupportGarageTeleport(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var garage = data["GarageId"] as Garage;
            
            if(garage != null) {
                player.Position = garage.Manager.Position; 
                Logger.logDebug(LogCategory.Support, LogActionType.Event, player, $"Teleported to {garage.Id}");
            }
            
            return true;
        }
        
        private bool onSupportDeleteGarage(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var garage = data["GarageId"] as Garage;
            
            if(garage != null) {
                deleteGarage(garage.Id);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Garage {garage.Name} gelöscht", "Garage gelöscht");
                Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"Garage {garage.Name} deleted");
            }
            
            return true;
        }
        
        private bool onSupportGarageSpotTeleport(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var spot = data["Spot"] as GarageSpawnSpot;
            
            if(spot != null) {
                player.Position = spot.SpawnPosition;
                Logger.logDebug(LogCategory.Support, LogActionType.Event, player, $"Teleported to {spot.Id}");
            }
            
            return true;
        }
        
        private bool onSupportDeleteGarageSpot(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var spot = data["Spot"] as GarageSpawnSpot;
            
            if(spot != null) {
                deleteGarageSpot(spot.GarageId, spot.Id);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Spot {spot.Id} gelöscht", "Garage gelöscht");
                Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"Garage Spot {spot.Id} deleted");
            }
            
            return true;
        }
        
        private bool onSupportCreateGarage(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var ownerType = (Constants.GarageOwnerType)data["OwnerType"];
            var ownerId = (int)data["OwnerId"];

            var evt = menuitemcefevent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();
            var slotsEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var typeEvt = evt.elements[2].FromJson<ListMenuItem.ListMenuItemEvent>();
            var npcEvt = evt.elements[3].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
            
            createGarage(player, nameEvt.input, Enum.Parse<Constants.GarageType>(typeEvt.currentElement), ownerType, ownerId, int.Parse(slotsEvt.input), npcEvt.check);
            
            return true;
        }

        private bool onSupportCreateGarageSpot(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var garage = data["Garage"] as Garage;

            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                createGarageSpot(player, garage.Id, p, r, w, h);
            });


            return true;
        }

        #endregion
    }
}
