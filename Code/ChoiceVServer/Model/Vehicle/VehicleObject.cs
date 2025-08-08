//using System;
//using System.Linq;
//using System.Collections.Generic;
//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Controller;
//using static ChoiceVServer.Base.Constants;
//using Microsoft.EntityFrameworkCore;

//namespace ChoiceVServer.Model.Vehicle {

//    public class VehicleObject {

//        // Public class data
//        public int VehicleId = 0;
//        public DateTime createDate = DateTime.Now;

//        public uint ModelId = 0;
//        public int GarageId = -1;
//        public int OwnerId = 0;
//        public VehicleOwnerType OwnerType = VehicleOwnerType.None;
//        public string OwnerName = "";
//        public string OwnerHistory = "";
//        public Position colPosition = Position.Zero;
//        public Rotation colRotation = Rotation.Zero;
//        public int Dimension = 0;
//        public string NumberPlate = "";
//        public string RegisteredPlate = "";
//        public string ChassisNumber = "";
//        public string LastMoved = "";
//        public float Fuel = 5;
//        public float DrivenDistance = 0;
//        public float DrivenTime = 0;
//        public int RepairCount = 0;
//        public bool Locked = false;

//        public float FuelMax = 0;
//        public float FuelPerKm = 0;
//        public float FuelPerMin = 0;
//        public FuelType FuelType = FuelType.None;
//        public float EnginePerKm = 0;
//        public float WheelPerKm = 0;
//        public float DirtPerKm = DIRT_PER_KILOMETER;

//        public bool FuelEmpty = false;
//        public bool EngineOn = false;
//        public bool SilencerOn = true;
//        public bool NeonLightOn = false;

//        public Dictionary<int, bool> DoorState = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false }, { 3, false } };
//        public Dictionary<int, bool> WindowState = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false }, { 3, false } };
//        public Dictionary<int, bool> SeatbeltState = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false }, { 3, false } };
//        public List<bool> IndicatorState = new List<bool> { false, false, false };

//        public Position LastPosition = Position.Zero;
//        public DateTime LastTime = DateTime.MinValue;
//        public bool DataChanged = false;

//        public VehicleObject() { }

//        public VehicleObject(uint modelid, int garageid, int ownerid, VehicleOwnerType ownertype, string ownername, string ownerhistory, Position position, Rotation rotation, int dimension, string numberplate, string registeredplate, string chassisnumber, string lastmoved, float fuel, float drivendistance, float driventime, int repaircount, bool locked) {
//            createDate = DateTime.Now;

//            ModelId = modelid;
//            GarageId = garageid;
//            OwnerId = ownerid;
//            OwnerType = ownertype;
//            OwnerName = ownername;
//            OwnerHistory = ownerhistory;
//            colPosition = position;
//            colRotation = rotation;
//            Dimension = dimension;
//            NumberPlate = numberplate;
//            RegisteredPlate = registeredplate;
//            ChassisNumber = chassisnumber;
//            LastMoved = lastmoved;
//            Fuel = fuel;
//            DrivenDistance = drivendistance;
//            DrivenTime = driventime;
//            RepairCount = repaircount;
//            Locked = locked;

//            FuelEmpty = false;
//            EngineOn = false;
//            SilencerOn = true;
//            NeonLightOn = false;

//            if (Fuel <= 0) {
//                Fuel = 0;
//                FuelEmpty = true;
//            }
//        }

//        public bool createVehicleObject() {
//            try {
//                string id = "";
//                string hashSolution = "";

//                // Create record in vehicle database
//                using (var db = new ChoiceVDb()) {
//                    var row = new vehicles {
//                        createDate = createDate,

//                        modelId = (int)ModelId,
//                        garageId = GarageId,
//                        ownerId = OwnerId,
//                        ownerType = (int)OwnerType,
//                        ownerName = OwnerName,
//                        ownerHistory = OwnerHistory,
//                        position = colPosition.ToJson(),
//                        rotation = colRotation.ToJson(),
//                        dimension = Dimension,
//                        numberPlate = NumberPlate,
//                        registeredPlate = RegisteredPlate,
//                        chassisNumber = ChassisNumber,
//                        lastMoved = LastMoved,
//                        fuel = Fuel,
//                        drivenDistance = DrivenDistance,
//                        drivenTime = DrivenTime,
//                        repairCount = RepairCount,
//                        Locked = Locked ? 1 : 0,
//                    };

//                    db.vehicles.Add(row);
//                    db.SaveChanges();

//                    VehicleId = row.id;

//                    id = row.id.ToString();
//                    hashSolution = String.Format("{0:X}", id.GetHashCode()).Trim();

//                    if (hashSolution.Length > 8)
//                        hashSolution = hashSolution.Substring(0, 8);

//                    if (row.numberPlate.Length == 0)
//                        row.numberPlate = hashSolution;

//                    if (row.chassisNumber.Length == 0)
//                        row.chassisNumber = hashSolution;

//                    NumberPlate = hashSolution;
//                    RegisteredPlate = "";
//                    ChassisNumber = hashSolution;

//                    FuelEmpty = (Fuel > 0 ? false : true);
//                    EngineOn = false;
//                    SilencerOn = true;
//                    NeonLightOn = false;

//                    LastPosition = colPosition;
//                    LastTime = DateTime.Now;

//                    db.SaveChanges();
//                    db.Dispose();

//                    DataChanged = false;

//                    // Logger.logDebug($"Vehicle model for ID {VehicleId} has been created.");

//                    return true;
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return false;
//        }

//        public void updateVehicleObject() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = db.vehicles.FirstOrDefault(v => v.id == VehicleId);
//                    if (row != null) {
//                        row.modelId = (int)ModelId;
//                        row.garageId = GarageId;
//                        row.ownerId = OwnerId;
//                        row.ownerType = (int)OwnerType;
//                        row.ownerName = OwnerName;
//                        row.ownerHistory = OwnerHistory;
//                        row.position = colPosition.ToJson();
//                        row.rotation = colRotation.ToJson();
//                        row.dimension = Dimension;
//                        row.numberPlate = NumberPlate;
//                        row.registeredPlate = RegisteredPlate;
//                        row.chassisNumber = ChassisNumber;
//                        row.lastMoved = LastMoved;
//                        row.fuel = Fuel;
//                        row.drivenDistance = DrivenDistance;
//                        row.drivenTime = DrivenTime;
//                        row.repairCount = RepairCount;
//                        row.Locked = Locked ? 1 : 0;

//                        db.SaveChanges();

//                        DataChanged = false;

//                        // Logger.logDebug($"Vehicle model for ID {VehicleId} has been saved.");
//                    }

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public void deleteVehicleObject(IPlayer player, ChoiceVVehicle vehicle) {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row1 = db.vehiclesdamage.FirstOrDefault(r => r.vehicleId == VehicleId);
//                    if (row1 != null) {
//                        db.vehiclesdamage.Remove(row1);
//                        db.SaveChanges();
//                    }

//                    var row2 = db.vehiclescoloring.FirstOrDefault(r => r.vehicleId == VehicleId);
//                    if (row2 != null) {
//                        db.vehiclescoloring.Remove(row2);
//                        db.SaveChanges();
//                    }

//                    var row3 = db.vehiclestuning.FirstOrDefault(r => r.vehicleId == VehicleId);
//                    if (row3 != null) {
//                        db.vehiclestuning.Remove(row3);
//                        db.SaveChanges();
//                    }

//                    var row4 = db.vehicles.FirstOrDefault(r => r.id == VehicleId);
//                    if (row4 != null) {
//                        db.vehicles.Remove(row4);
//                        db.SaveChanges();
//                    }

//                    db.Dispose();

//                    DataChanged = false;

//                    // Logger.logDebug($"Vehicle model for ID {VehicleId} has been deleted.");
//                }

//                // Remove vehicle from all collision shapes
//                foreach (var shape in CollisionShape.AllShapes) {
//                    if (shape.AllEntities.Contains(vehicle)) {
//                        shape.AllEntities.Remove(vehicle);
//                    }
//                }

//                // Remove vehicle from objects and seats
//                VehicleController.AllVehicleObjects.RemoveAll(v => v.VehicleId == VehicleId);
//                VehicleController.AllVehicleUsedSeats.RemoveAll(v => v.VehicleId == VehicleId);

//                // Remove inventory and despawn vehicle
//                if (vehicle.getInventory() != null)
//                    InventoryController.destroyInventory(vehicle.getInventory());

//                // Remove classes from vehicle
//                vehicle.resetData(DATA_VEHICLE_MODEL);
//                vehicle.resetData(DATA_VEHICLE_CLASS);
//                vehicle.resetData(DATA_VEHICLE_INVENTORY);
//                vehicle.resetData(DATA_VEHICLE_OBJECT);
//                vehicle.resetData(DATA_VEHICLE_DAMAGE);
//                vehicle.resetData(DATA_VEHICLE_COLORING);
//                vehicle.resetData(DATA_VEHICLE_TUNING);

//                // Despawn vehicle
//                VehicleController.despawnVehicle(vehicle, true);

//                player.sendNotification(Constants.NotifactionTypes.Info, "Fahrzeug wurde entfernt.", "Fahrzeug entfernt", Constants.NotifactionImages.Car);

//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public void processDrivenDistance(ChoiceVVehicle vehicle, float distance) {
//            Random rand = new Random();

//            LastMoved = DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
//            DrivenDistance += distance;

//            if (FuelPerKm > 0) {
//                if (Fuel > 0) {
//                    Fuel -= ((FuelPerKm / 1000) * distance);
//                    DataChanged = true;

//                } else if (!FuelEmpty) {
//                    Fuel = 0;
//                    FuelEmpty = true;
//                    DataChanged = true;
//                }
//            }

//            VehicleDamage damage = vehicle.getDamage();
//            if (damage != null) {
//                if (DirtPerKm > 0) {
//                    if (damage.DirtLevel < 255) {
//                        damage.DirtLevel += ((DirtPerKm / 1000) * distance);
//                        damage.DataChanged = true;

//                        if (vehicle.DirtLevel != Math.Floor(damage.DirtLevel)) {
//                            vehicle.DirtLevel = (byte)damage.DirtLevel;
//                            vehicle.SetSyncedMetaData("VehicleDirtLevel", (damage.DirtLevel / 17));
//                        }
//                    }
//                }

//                if (EnginePerKm > 0) {
//                    if (damage.EngineHealth > 0) {
//                        damage.EngineHealth -= ((EnginePerKm / 1000) * distance);
//                        damage.DataChanged = true;

//                        if (vehicle.EngineHealth != Math.Ceiling(damage.EngineHealth))
//                            vehicle.EngineHealth = (int)damage.EngineHealth;

//                    } else if (EngineOn) {
//                        double r = (rand.NextDouble() * 100);

//                        if (r < ENGINE_STOP_CHANCE_PERCENT)
//                            EngineOn = false;
//                    }
//                }

//                if (WheelPerKm > 0) {
//                    for (byte i = 0; i < vehicle.WheelsCount; i++) {
//                        if (!damage.WheelHealth.ContainsKey(i))
//                            damage.WheelHealth.Add(i, 1000f);

//                        if (damage.WheelHealth[i] > 0) {
//                            damage.WheelHealth[i] -= ((WheelPerKm / 1000) * distance);
//                            damage.DataChanged = true;

//                            if (Math.Ceiling(vehicle.GetWheelHealth(i)) != Math.Ceiling(damage.WheelHealth[i]))
//                                vehicle.SetWheelHealth(i, damage.WheelHealth[i]);

//                        } else if (!vehicle.IsWheelBurst(i)) {
//                            double r = (rand.NextDouble() * 100);

//                            if (r < TIRE_BURST_CHANCE_PERCENT)
//                                vehicle.SetWheelBurst(i, true);
//                        }
//                    }
//                }
//            }

//        }

//        public void processDrivenTime(int time) {
//            DrivenTime += time;

//            if (FuelPerMin > 0) {
//                if (Fuel > 0) {
//                    Fuel -= ((FuelPerMin / 60) * time);
//                    DataChanged = true;

//                } else if (!FuelEmpty) {
//                    Fuel = 0;
//                    FuelEmpty = true;
//                    DataChanged = true;
//                }
//            }
//        }

//        public bool setFuel(float fuel) {
//            if (fuel != 0) {
//                Fuel += fuel;

//                FuelEmpty = false;
//                DataChanged = true;

//                if (Fuel <= 0) {
//                    Fuel = 0;
//                    FuelEmpty = true;
//                    return true;

//                } else if (Fuel > FuelMax) {
//                    Fuel = FuelMax;
//                    return false;
//                }
//            }

//            return true;
//        }

//        public float getFuel() {
//            return Fuel;
//        }

//        public float getMaxFuel() {
//            return FuelMax;
//        }
//    }
//}
