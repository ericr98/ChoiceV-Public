//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChoiceVServer.Controller {
//    public class RentalController : ChoiceVScript {
//        public List<CollisionShape> SpawnSpots;
//        public List<RentalCar> RentalCars;

//        public CollisionShape InteractionSpot;

//        public RentalController() {
//            //TODO LOAD RENTAL CARS

//            EventController.addCollisionShapeEvent("RENTAL_CAR_INTERACTION", onInteraction);
//        }

//        private bool onInteraction(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            //TODO
//            var menu = new Menu("Mietwagen Automat", "Miete deinen Traumwagen!");
//            return true;
//        }
//    }

//    public class RentalCar {
//        public int VehicleId;
//        public DateTime ReturnDate;
//        public int CharId;
//    }
//}
