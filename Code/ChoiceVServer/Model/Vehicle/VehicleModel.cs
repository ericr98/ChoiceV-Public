//using AltV.Net.Data;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Model.Vehicle {
//    public class VehicleModels {

//        // Public class data
//        public int Id = 0;
//        public DateTime createDate = DateTime.Now;

//        public uint Model = 0;
//        public string ModelName = "";
//        public int Class = 0;
//        public int Seats = 0;
//        public int InventorySize = 0;
//        public string Trunk = "";
//        public string Motor = "";
//        public float FuelMax = 0;
//        public float FuelPerKm = 0;
//        public float FuelPerMin = 0;
//        public FuelType FuelType = FuelType.Petrol;
//        public float EnginePerKm = 0;
//        public float WheelPerKm = 0;
//        public float PowerMultiplier = 1;
//        public float MaxSpeed = 0;
//        public int MegaPhoneRange = 0;

//        public Position StartPoint = Position.Zero;
//        public Position EndPoint = Position.Zero;

//        public Dictionary<byte, bool> Extras = new Dictionary<byte, bool>();
//        public bool Locked = false;
//        public int EmergencyType = 0;

//        public VehicleModels() { }

//        public VehicleModels(uint model, string modelname, int vehclass, int seats, int invsize, string trunk, string motor, float fuelmax, float fuelperkm, float fuelpermin, FuelType fueltype, float engineperkm, float wheelperkm, float powermultiplier, float maxspeed, int megaphonerange, Position startpoint, Position endpoint, string extras, bool locked, int emergencytype) {
//            string name = modelname.First().ToString().ToUpper() + modelname.Substring(1).ToLower();

//            createDate = DateTime.Now;

//            Model = model;
//            ModelName = name;

//            Class = vehclass;
//            Seats = seats;
//            InventorySize = invsize;
//            Trunk = trunk;
//            Motor = motor;
//            FuelMax = fuelmax;
//            FuelPerKm = fuelperkm;
//            FuelPerMin = fuelpermin;
//            FuelType = fueltype;
//            EnginePerKm = engineperkm;
//            WheelPerKm = wheelperkm;
//            PowerMultiplier = powermultiplier;
//            MaxSpeed = maxspeed;
//            MegaPhoneRange = megaphonerange;

//            StartPoint = startpoint;
//            EndPoint = endpoint;

//            Extras = extras.FromJson<Dictionary<byte, bool>>();

//            Locked = locked;
//            EmergencyType = emergencytype;
//        }

//        public void createVehicleModel() {
//            try {
//                string name = ModelName.First().ToString().ToUpper() + ModelName.Substring(1).ToLower();

//                using (var db = new ChoiceVDb()) {
//                    var row = new vehiclesmodel {
//                        createDate = createDate,

//                        Model = (int)Model,
//                        ModelName = name,
//                        Class = Class,
//                        Seats = Seats,
//                        InventorySize = InventorySize,
//                        Trunk = Trunk,
//                        Motor = Motor,

//                        FuelMax = FuelMax,
//                        FuelPerKm = FuelPerKm,
//                        FuelPerMin = FuelPerMin,
//                        FuelType = (int)FuelType,
//                        EnginePerKm = EnginePerKm,
//                        WheelPerKm = WheelPerKm,

//                        PowerMultiplier = PowerMultiplier,
//                        MaxSpeed = MaxSpeed,
//                        megaphonerange = MegaPhoneRange,

//                        StartPoint = StartPoint.ToJson(),
//                        EndPoint = EndPoint.ToJson(),

//                        Extras = Extras.ToJson(),

//                        Locked = Locked ? 1 : 0,
//                        EmergencyType = EmergencyType,
//                    };

//                    db.vehiclesmodel.Add(row);
//                    db.SaveChanges();

//                    Id = row.id;

//                    VehicleController.AllVehicleModels.Add(this);

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public void updateVehicleModel() {
//            try {
//                string name = ModelName.First().ToString().ToUpper() + ModelName.Substring(1).ToLower();

//                using (var db = new ChoiceVDb()) {
//                    var row = db.vehiclesmodel.FirstOrDefault(r => r.id == Id);
//                    if (row != null) {
//                        row.Model = (int)Model;
//                        row.ModelName = name;
//                        row.Class = Class;
//                        row.Seats = Seats;
//                        row.InventorySize = InventorySize;
//                        row.Trunk = Trunk;
//                        row.Motor = Motor;

//                        row.FuelMax = FuelMax;
//                        row.FuelPerKm = FuelPerKm;
//                        row.FuelPerMin = FuelPerMin;
//                        row.FuelType = (int)FuelType;
//                        row.EnginePerKm = EnginePerKm;
//                        row.WheelPerKm = WheelPerKm;

//                        row.PowerMultiplier = PowerMultiplier;
//                        row.MaxSpeed = MaxSpeed;
//                        row.megaphonerange = MegaPhoneRange;

//                        row.StartPoint = StartPoint.ToJson();
//                        row.EndPoint = EndPoint.ToJson();

//                        row.Extras = Extras.ToJson();

//                        row.Locked = Locked ? 1 : 0;
//                        row.EmergencyType = EmergencyType;

//                        db.SaveChanges();
//                    }

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public void deleteVehicleModel() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = db.vehiclesmodel.FirstOrDefault(r => r.id == Id);
//                    if (row != null) {
//                        db.vehiclesmodel.Remove(row);
//                        db.SaveChanges();

//                        db.vehiclesseat.RemoveRange(db.vehiclesseat.Where(r => r.ModelId == Id));
//                        db.SaveChanges();

//                        if (VehicleController.AllVehicleModels.Contains(this))
//                            VehicleController.AllVehicleModels.Remove(this);
//                    }

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }
//    }
//}
