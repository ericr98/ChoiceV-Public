//using System;
//using System.Numerics;
//using System.Linq;
//using System.Collections.Generic;
//using AltV.Net.Data;
//using ChoiceVServer.Base;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Controller;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Model.Vehicle 
//{
//    public class VehicleSeats {

//        // Public class data
//        public int Id = 0;
//        public DateTime createDate = DateTime.Now;

//        public int ModelId = 0;
//        public int SeatNr = 0;
//        public Vector2 SeatPos = Vector2.Zero;

//        public VehicleSeats() { }

//        public VehicleSeats(int modelid, int seatnr, Vector2 seatpos) {
//            createDate = DateTime.Now;

//            ModelId = modelid;
//            SeatNr = seatnr;
//            SeatPos = seatpos;
//        }

//        public void createVehicleSeat() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = new vehiclesseat {
//                        createDate = createDate,

//                        ModelId = ModelId,
//                        SeatNr = SeatNr,
//                        SeatPos = SeatPos.ToJson(),
//                    };

//                    db.vehiclesseat.Add(row);
//                    db.SaveChanges();

//                    Id = row.id;

//                    VehicleController.AllVehicleSeats.Add(this);

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e,"createVehicleSeat");
//            }
//        }

//        public void updateVehicleSeat() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = db.vehiclesseat.FirstOrDefault(r => r.id == Id);
//                    if (row != null) {
//                        row.ModelId = ModelId;
//                        row.SeatNr = SeatNr;
//                        row.SeatPos = SeatPos.ToJson();

//                        db.SaveChanges();
//                    }

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e, "updateVehicleSeat");
//            }
//        }

//        public void deleteVehicleSeat() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    var row = db.vehiclesseat.FirstOrDefault(r => r.id == Id);
//                    if (row != null) {
//                        db.vehiclesseat.Remove(row);
//                        db.SaveChanges();
//                    }

//                    if (VehicleController.AllVehicleSeats.Contains(this))
//                        VehicleController.AllVehicleSeats.Remove(this);

//                    db.Dispose();
//                }
//            } catch (Exception e) {
//                Logger.logException(e, "deleteVehicleSeat");
//            }
//        }
//    }
//}
