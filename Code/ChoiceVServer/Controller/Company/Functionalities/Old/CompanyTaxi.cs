//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.FsDatabase;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChoiceVServer.Controller.Company.Functionalities.Old {
//    public class TaxiController {
//        public TaxiController() {
//            var menu = new Menu("Taximeter setzen", "Setze die Einstellungen des Taximeters");
//            menu.addMenuItem(new InputMenuItem("Startpauschale", "Setze die Startpauschale der Fahrt", "", ""));
//            menu.addMenuItem(new InputMenuItem("Preis pro 100m", "Setze den Fahrtpreis pro 100m", "", ""));
//            menu.addMenuItem(new MenuStatsMenuItem("Setzen", "Setze die aktuellen Einstellungen in dein Taximeter", "TAXI_SET_TAXIMETER", MenuItemStyle.green));

//            VehicleController.addSelfMenuElement(
//                new ConditionalVehicleSelfMenuElement(
//                    "Taximeter setzen",
//                    () => menu,
//                    v => v.DbModel.ModelName == "Taxi",
//                    p => true
//                )
//            );

//            EventController.PlayerEnterVehicleDelegate += onPlayerEnterVehicle;
//            EventController.PlayerExitVehicleDelegate += onPlayerExitVehicle;
//        }

//        private void onPlayerEnterVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {

//        }

//        private void onPlayerExitVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {

//        }
//    }

//    public class CompanyTaxi : Company {
//        public CompanyTaxi(company dbComp, system system, List<companysetting> allSettings) : base(dbComp, system, allSettings) {

//        }
//    }
//}
