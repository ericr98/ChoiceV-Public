//using System;
//using System.Linq;
//using ChoiceVServer.Base;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Controller;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Model.Vehicle 
//{
//    public class VehicleClasses {

//        // Public class data
//        public int Id = 0;
//        public DateTime createDate = DateTime.Now;

//        public int Class = 0;
//        public string ClassName = "";
//        public int InventorySize = 0;
//        public float FuelMax = 0;
//        public float FuelPerKm = 0;
//        public float FuelPerMin = 0;
//        public FuelType FuelType = FuelType.Petrol;
//        public float EnginePerKm = 0;
//        public float WheelPerKm = 0;

//        public VehicleClasses() { }

//        public VehicleClasses(int vehclass, string classname, int invsize, float fuelmax, float fuelperkm, float fuelpermin, FuelType fueltype, float engineperkm, float wheelperkm) {
//            createDate = DateTime.Now;

//            Class = vehclass;
//            ClassName = classname;

//            InventorySize = invsize;

//            FuelMax = fuelmax;
//            FuelPerKm = fuelperkm;
//            FuelPerMin = fuelpermin;
//            FuelType = fueltype;
//            EnginePerKm = engineperkm;
//            WheelPerKm = wheelperkm;
//        }

//        public void createVehicleClass() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = new vehiclesclass {
//                        createDate = createDate,

//                        Class = Class,
//                        ClassName = ClassName,
//                        InventorySize = InventorySize,
//                        FuelMax = FuelMax,
//                        FuelPerKm = FuelPerKm,
//                        FuelPerMin = FuelPerMin,
//                        FuelType = (int)FuelType,
//                        EnginePerKm = EnginePerKm,
//                        WheelPerKm = WheelPerKm,
//                    };

//                    db.vehiclesclass.Add(row);
//                    db.SaveChanges();

//                    Id = row.id;

//                    VehicleController.AllVehicleClasses.Add(this);

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public void updateVehicleClass() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = db.vehiclesclass.FirstOrDefault(r => r.id == Id);
//                    if (row != null) {
//                        row.Class = Class;
//                        row.ClassName = ClassName;
//                        row.InventorySize = InventorySize;
//                        row.FuelMax = FuelMax;
//                        row.FuelPerKm = FuelPerKm;
//                        row.FuelPerMin = FuelPerMin;
//                        row.FuelType = (int)FuelType;
//                        row.EnginePerKm = EnginePerKm;
//                        row.WheelPerKm = WheelPerKm;

//                        db.SaveChanges();
//                    }

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public void deleteVehicleClass() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = db.vehiclesclass.FirstOrDefault(r => r.id == Id);
//                    if (row != null) {
//                        db.vehiclesclass.Remove(row);
//                        db.SaveChanges();

//                        if (VehicleController.AllVehicleClasses.Contains(this))
//                            VehicleController.AllVehicleClasses.Remove(this);
//                    }

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }
//    }
//}
