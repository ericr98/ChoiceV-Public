//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Controller {
//    class DrugController : ChoiceVScript {
//        public DrugController() {
//            DrugInvoke();
//            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnected;
//        }

//        #region onConnect
//        private void onPlayerConnected(IPlayer player, characters character) {
//            if (player.hasData("WeedEffect")) {  //Weed Effect On Connect
//                float strength = float.Parse(player.getData("WeedEffect"));
//                string effect = (string)player.getData("WeedEffectType");
//                player.setTimeCycle(effect, strength);
//                var invoke = InvokeController.AddTimedInvoke("Weed-Invoke", (ivk) => {
//                    player.stopTimeCycle();
//                    player.resetPermantData("WeedEffect");
//                }, TimeSpan.FromMinutes(5), false);
//                player.setPermanentData("DrugInvoke", invoke.ToString());
//            }
//            if (player.hasData("AlcoholEffect")) { }
//            if (player.hasData("CocaineEffect")) {
//                string effect = player.getData("CocaineEffect");
//                player.setTimeCycle(effect, 1f);
//                var invoke = InvokeController.AddTimedInvoke("Cocaine-Invoke", (ivk) => {
//                    player.stopTimeCycle();
//                    player.resetPermantData("CocaineEffect");
//                }, TimeSpan.FromMinutes(5), false);
//                player.setPermanentData("DrugInvoke", invoke.ToString());
//            }
//            if (player.hasData("MescalineEffect")) { }
//            if (player.hasData("CocaineEffect")) { }
//        }
//        #endregion

//        #region DrugInvoke
//        private static void DrugInvoke() {
//            InvokeController.AddTimedInvoke("Druglevel-Invoke", (ivk) => {
//                foreach (var player in ChoiceVAPI.GetAllPlayers()) {
//                    if (player.hasData("DrugCheck")) {
//                        refreshDrugLevels(player);
//                    }
//                }
//            }, TimeSpan.FromMinutes(30), true);
//        }

//        public static void refreshDrugLevels(IPlayer player) {
//            if (!(player.hasData("DrugCheck"))) {
//                return;
//            }
//            var weedLevel = getDrugLevel(player, DrugTypes.Weed);
//            var alcoholLevel = getDrugLevel(player, DrugTypes.Alcohol);
//            var cocaineLevel = getDrugLevel(player, DrugTypes.Cocaine);
//            var mescalineLevel = getDrugLevel(player, DrugTypes.Alcohol);
//            var frogLevel = getDrugLevel(player, DrugTypes.Frog);
//            if (weedLevel != 0) {
//                var newLevel = weedLevel;
//                if (weedLevel > 96) {
//                    newLevel -= 48;
//                } else {
//                    newLevel -= 3;
//                }
//                player.setPermanentData("WeedLevel", newLevel.ToString());
//                if (newLevel >= 0) {
//                    player.resetPermantData("WeedLevel");
//                    return;
//                }
//            }
//            if (alcoholLevel != 0) {
//                var newLevel = alcoholLevel - 0.1f;
//                player.setPermanentData("AlcoholLevel", newLevel.ToString());
//                if (newLevel >= 0) {
//                    player.resetPermantData("AlcoholLevel");
//                }
//            }
//            if (cocaineLevel != 0) {
//                var currentLevel = getDrugLevel(player, DrugTypes.Cocaine);
//                var newLevel = currentLevel - 5;
//                player.setPermanentData("CocaineLevel", newLevel.ToString());
//                if (newLevel >= 0) {
//                    player.resetPermantData("CocaineLevel");
//                }
//            }
//            if (player.hasData("MescalineLevel")) {
//                var currentLevel = getDrugLevel(player, DrugTypes.Mescaline);
//                var newLevel = currentLevel;
//                if (currentLevel > 30) {
//                    newLevel -= 30;
//                } else {
//                    newLevel -= 3;
//                }
//                player.setPermanentData("MescalineLevel", newLevel.ToString());
//                if (newLevel >= 0) {
//                    player.resetPermantData("MescalineLevel");
//                    return;
//                }
//            }
//            if (player.hasData("FrogLevel")) {

//            }
//            if (weedLevel < 96 || mescalineLevel < 30) {
//                player.resetPermantData("SmokeBlock");
//            }
//            if (!(player.hasData("WeedLevel") || (player.hasData("AlcoholLevel")) || (player.hasData("CocaineLevel")) || (player.hasData("MescalineLevel")) || (player.hasData("FrogLevel")))) {
//                player.resetPermantData("DrugCheck");
//            }
//        }
//        #endregion

//        #region setMethods
//        public static void setWeedLevel(IPlayer player, WeedItems weedItem) {
//            var defaultLevel = 0;
//            if (weedItem == WeedItems.Joint) { defaultLevel = 48; } //Milligramm
//            if (weedItem == WeedItems.Blunt) { defaultLevel = 60; }
//            if (weedItem == WeedItems.Bong) { defaultLevel = 72; }
//            var currentLevel = getDrugLevel(player, DrugTypes.Weed);
//            var newLevel = currentLevel + defaultLevel;
//            if (newLevel >= 144) {
//                player.setPermanentData("SmokeBlock", true.ToString());
//                newLevel = 144;
//            }
//            player.setPermanentData("WeedLevel", newLevel.ToString());
//            player.setPermanentData("DrugCheck", DateTime.Now.ToString());
//        }

//        public static void setAlcoholLevel(IPlayer player, float alcoholLevel) {
//            var currentLevel = getDrugLevel(player, DrugTypes.Alcohol);
//            var newLevel = currentLevel + alcoholLevel;
//            if (newLevel >= 2f) {
//                //Bewusstlos
//            }
//            player.setPermanentData("AlcoholLevel", newLevel.ToString());
//            player.setPermanentData("DrugCheck", DateTime.Now.ToString());
//        }

//        public static void setCocaineLevel(IPlayer player) {
//            var currentLevel = getDrugLevel(player, DrugTypes.Cocaine);
//            var newLevel = currentLevel + 30;
//            if (newLevel >= 120) {
//                //Bewusstlos
//            }
//            player.setPermanentData("CocaineLevel", newLevel.ToString());
//            player.setPermanentData("DrugCheck", DateTime.Now.ToString());
//        }

//        public static void setMescalineLevel(IPlayer player, bool weedCombine = false) {
//            var currentLevel = getDrugLevel(player, DrugTypes.Mescaline);
//            var newLevel = currentLevel + 30;
//            if (weedCombine) {
//                newLevel = newLevel * 1.5f;
//            }
//            if (newLevel >= 60) {
//                newLevel = 60;
//            }
//            player.setPermanentData("SmokeBlock", true.ToString());
//            player.setPermanentData("DrugCheck", DateTime.Now.ToString());
//            player.setPermanentData("MescalineLevel", newLevel.ToString());
//        }

//        public static void setFrogLevel(IPlayer player) {

//        }
//        #endregion

//        #region getMethods

//        public static float getDrugLevel(IPlayer player, DrugTypes drugType) {
//            var returnValue = 0f;
//            if (drugType == DrugTypes.Weed) {
//                if (player.hasData("WeedLevel")) {
//                    returnValue = float.Parse(player.getData("WeedLevel"));
//                } else {
//                    return 0;
//                }
//            }
//            if (drugType == DrugTypes.Alcohol) {
//                if (player.hasData("AlcoholLevel")) {
//                    returnValue = float.Parse(player.getData("AlcoholLevel"));
//                } else {
//                    return 0;
//                }
//            }
//            if (drugType == DrugTypes.Cocaine) {
//                if (player.hasData("CocaineLevel")) {
//                    returnValue = float.Parse(player.getData("CocaineLevel"));
//                } else {
//                    return 0;
//                }
//            }
//            if (drugType == DrugTypes.Mescaline) {
//                if (player.hasData("MescalineLevel")) {
//                    returnValue = float.Parse(player.getData("MescalineLevel"));
//                } else {
//                    return 0;
//                }
//            }
//            if (drugType == DrugTypes.Frog) {

//            }
//            if (returnValue < 0) {
//                return 0;
//            } else {
//                return returnValue;
//            }
//        }


//        #endregion

//        #region drugTrips
//        public static void setPlayerTrip(IPlayer player, TripTypes tripType) {
//            if (tripType == TripTypes.ClownTrip) {
//                //CLOWNTRIP
//            }
//            if (tripType == TripTypes.MexicanTrip) {
//                //MEXICANTRIP
//            }
//            if (tripType == TripTypes.AnimalTrip) {
//                //ANIMALTRIP
//            }
//            if (tripType == TripTypes.HeatTrip) {
//                //HEATRIP
//            }
//        }
//        #endregion
//    }
//}
