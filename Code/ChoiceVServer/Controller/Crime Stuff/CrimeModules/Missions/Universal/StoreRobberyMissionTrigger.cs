//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller.Crime_Stuff;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//
//namespace ChoiceVServer.Controller.Crime_Stuff {
//     public class StoreRobberyMissionTrigger : CrimeNetworkMissionTrigger {
//
//        public StoreRobberyMissionTrigger(int id, CrimeAction type, float amount, TimeSpan timeConstraint, Position location, float radius) : base(id, type, amount, timeConstraint, location, radius) { }
//
//        public override CrimeMissionTriggerResult onTriggerProgressStep(IPlayer player, string name, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress) {
//            if(currentProgress >= 0 && amount + currentProgress < Amount) {
//                player.sendNotification(Constants.NotifactionTypes.Info, $"{name}: Du hast ${(int)(currentProgress + amount)}/{(int)Amount} aus Kassenräuben gesammelt!", $"${(int)(currentProgress + amount)}/{(int)Amount} gestohlen", Constants.NotifactionImages.Thief);
//                return CrimeMissionTriggerResult.Add;
//            } else {
//                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast genügend Bargeld aus Kassenräuben erhalten! Kehre zu einem Auftragssteller zurück!", $"Auftrag abgeschlossen", Constants.NotifactionImages.Thief);
//                return CrimeMissionTriggerResult.Finish;
//            }
//        }
//
//        protected override List<MenuItem> getMenuItemInfo(IPlayer player, float currentProgress) {
//            var list = new List<MenuItem>();
//
//            list.Add(new StaticMenuItem("Kassen ausrauben", $"Du hast ${(int)currentProgress}/{(int)Amount} des für deinen Auftrag benötigten Bargeldes geraubt!", $"${(int)currentProgress}/{(int)Amount}"));
//
//            return list;
//        }
//
//        public override string getName() {
//            return "Kassen ausrauben";
//        }
//
//        public override string getPillarReputationName() {
//            if(Position != Position.Zero || TimeConstraint != TimeSpan.Zero) {
//                return "STORE_ROBBERY_MISSION_VARIATIONS";
//            } else {
//                return "STORE_ROBBERY_MISSIONS";
//            }
//        }
//
//        protected override string getPlayerSelectNotificationMessage(IPlayer player) {
//            return $"\"Kassen ausrauben\" Auftrag erhalten. Stiel ${Amount} Bargeld!";
//        }
//    }
//}