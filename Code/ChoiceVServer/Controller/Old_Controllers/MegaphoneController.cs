//using AltV.Net.Elements.Entities;
//using AltV.Net.Enums;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ChoiceVServer.Controller {
//    public class MegaphoneController : ChoiceVScript {

//        public static List<listedVehicle> listedVehicle = new List<listedVehicle>();
//        public MegaphoneController() {
//            EventController.addEvent("VOICE_MEGAPHONE_START", onMegaphone);
//            EventController.addEvent("VOICE_MEGAPHONE_STOP", onMegaphone);
//            loadList();
//        }

//        private bool onMegaphone(IPlayer player, string eventName, object[] args) {
//            if (eventName == "VOICE_MEGAPHONE_START") {
//                var range = 0;
//                var volume = 1;
//                if (player.IsInVehicle) {
//                    var currentVehicle = player.Vehicle;
//                    var vehicleCheck = listedVehicle.FirstOrDefault(x => x.vehicleModel == currentVehicle.Model);
//                    if (vehicleCheck != null) {
//                        range = vehicleCheck.range;
//                    }
//                } else {
//                    var itemCheck = player.getInventory().hasItem(x => x.Name == "Megafon");
//                    if (itemCheck == false) {
//                        return true;
//                    }
//                }
//                VoiceChatController.enableMegaphoneEffect(player, range, volume);
//            } else if (eventName == "VOICE_MEGAPHONE_STOP") {
//                VoiceChatController.disableMegaphoneEffect(player);
//            }
//            return true;
//        }

//        private void loadList() {
//            using (var db = new ChoiceVDb()) {
//                foreach (var vehicle in db.vehiclesmodel) {
//                    if (vehicle.megaphonerange >= 1) {
//                        var name = vehicle.ModelName;
//                        var modeluint = (uint)Enum.Parse(typeof(VehicleModel), name);
//                        var newEntry = new listedVehicle {
//                            vehicleModel = modeluint,
//                            range = vehicle.megaphonerange,
//                        };
//                        listedVehicle.Add(newEntry);
//                    }
//                }
//            }
//        }
//    }

//    public class listedVehicle {
//        public uint vehicleModel { get; set; }
//        public int range { get; set; }
//    }

//}
