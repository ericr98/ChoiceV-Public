//using AltV.Net.Elements.Entities;
//using AltV.Net.Enums;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Company;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Design;
//using System.Linq;
//using System.Text;

//namespace ChoiceVServer.Controller {
//    class VehicleRegistrationController : ChoiceVScript {
//        public VehicleRegistrationController() {
//            EventController.addCollisionShapeEvent("VEHICLE_REGISTRATION_INTERACT", onPedInteract);

//            EventController.addMenuEvent("VEHICLE_REGISTER_PLAYER", onVehicleRegisterOwner);
//            EventController.addMenuEvent("VEHICLE_UNREGISTER_PLAYER", onVehicleUnregisterOwner);
//            EventController.addMenuEvent("VEHICLE_OWNERCHANGE_PLAYER", onVehicleChangeOwner);

//            EventController.addMenuEvent("VEHICLE_REGISTER_COMPANY", onVehicleRegisterCompany);
//            EventController.addMenuEvent("VEHICLE_UNREGISTER_COMPANY", onVehicleUnregisterCompany);

//            EventController.addMenuEvent("VEHICLE_REGISTRATION_REGISTER", onVehicleExecuteRegister); 
//            EventController.addMenuEvent("VEHICLE_REGISTRATION_CHANGEOWNER", onVehicleExecuteChangeOwner);

//            EventController.addMenuEvent("VEHICLE_REGISTRATION_REGISTER_COMPANY", onVehicleExecuteCompanyRegister);
//            EventController.addMenuEvent("VEHICLE_REGISTRATION_UNREGISTER_COMPANY", onVehicleExecuteCompanyUnregister);

//            EventController.addMenuEvent("VEHICLEREGISTER_CREATE_NUMBERPLATE", onCreateNumberPlate);

//            EventController.addMenuEvent("VEHICLEREGISTER_CHOOSE", onVehicleChoose);
//            EventController.addMenuEvent("VEHICLEREGISTER_REGISTERITEM", onRegisterItem);

//            foreach (var shape in CollisionShape.AllShapes) {
//                if (shape.EventName == "VEHICLE_REGISTRATION_INTERACT") {
//                    shape.Height = 5;
//                    shape.Width = 5;
//                    shape.HasNoHeight = false;
//                }
//            }
//        }

//        private bool onRegisterItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            var model = Enum.GetName(typeof(VehicleModel), vehicle.Model);
//            var vehObj = vehicle.getObject();
//            var plate = data["Text"];
//            if(player.getCash() < 50) {
//                player.sendBlockNotification("Du hast nicht so viel Geld!", "");
//                return true;
//            } else {
//                player.sendNotification(Constants.NotifactionTypes.Success, $"{model} ist jetzt auf \"{plate}\" angemeldet", "Fahrzeug anmeldung");
//                player.sendNotification(Constants.NotifactionTypes.Info, $"Bitte schraube auch das passende Kennzeichen auf dein Auto!", "Fahrzeug Kennzeichen");
//                VehicleController.setOwner(vehicle, player.getCharacterId(), Constants.VehicleOwnerType.Player, player.getCharacterName());
//                vehObj.RegisteredPlate = plate;
//                vehObj.updateVehicleObject();
//            }

//            return true;
//        }

//        private bool onCreateNumberPlate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            using(var db = new ChoiceVDb()) {
//                if(player.getCash() < 350) {
//                    return true;
//                }
//                var dbCheck = db.numberplates.ToList().LastOrDefault();
//                var id = 1;
//                if (dbCheck != null) {
//                    id = dbCheck.Id + 1;
//                }
//                var index = $"LS{id}";
//                var newNumberplate = new numberplates { 
//                    Id = id,
//                    index = index,
//                };
//                db.numberplates.Add(newNumberplate);
//                db.SaveChanges();
//                var configItem = InventoryController.AllConfigItems.FirstOrDefault(x => x.codeItem == "NumberPlate");
//                var plateItem = new NumberPlate(configItem, index);
//                player.getInventory().addItem(plateItem);
//                player.removeCash(350);
//                player.sendNotification(Constants.NotifactionTypes.Success, "Du hast erfolgreich ein Kennzeichen gekauft!", "Kennzeichen");
//            }
//            return true;
//        }

//        private bool onVehicleExecuteCompanyRegister(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            var company = (Company)data["Company"];
//            VehicleController.setOwner(vehicle, company.Id, Constants.VehicleOwnerType.Company, company.Name);
//            player.sendNotification(Constants.NotifactionTypes.Success, "Fahrzeug erfolgreich als Firmenfahrzeug registriert", "Firmenfahrzeug");
//            return true;
//        }

//        private bool onVehicleExecuteCompanyUnregister(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            VehicleController.setOwner(vehicle, player.getCharacterId(), Constants.VehicleOwnerType.Player, player.getCharacterName());
//            player.sendNotification(Constants.NotifactionTypes.Success, "Firmenfahrzeug erfolgreich umgemeldet", "Firmenfahrzeug");
//            return true;
//        }

//        private bool onVehicleExecuteChangeOwner(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            var target = (IPlayer)data["Target"];
//            var vehObj = vehicle.getObject();
//            if (target.Position.Distance(player.Position) <= 10) {
//                VehicleController.setOwner(vehicle, target.getCharacterId(), Constants.VehicleOwnerType.Player, target.getCharacterName());
//                player.sendNotification(Constants.NotifactionTypes.Success, $"Fahrzeug erfolgreich an {target.getCharacterName()} überschrieben", "Fahrzeug überschrieben");
//                target.sendNotification(Constants.NotifactionTypes.Success, $"{player.getCharacterName()} hat dir ein Fahrzeug überschrieben", "Fahrzeug überschrieben");
//            } else {
//                player.sendBlockNotification("Der Spieler befindet sich zu weit weg", "");                
//            }
//            return true;
//        }

//        private bool onVehicleExecuteRegister(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            var menu = new Menu("Kennzeichenauswahl", "Auf welches Kennzeichen anmelden?");
//            foreach(var item in player.getInventory().getAllItems()) {
//                if (item is NumberPlate) {
//                    var plate = (NumberPlate)item;
//                    menu.addMenuItem(new ClickMenuItem($"{plate.NumberPlateContent}", "Melde das Auto auf das Kennzeichen", "50$", "VEHICLEREGISTER_REGISTERITEM").withData(new Dictionary<string, dynamic> { {"Vehicle", vehicle }, {"Text", plate.NumberPlateContent } }));
//                }
//            }
//            player.showMenu(menu);
//            return true;
//        }


//        private bool onVehicleChoose(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var eventString = (string)data["Event"];
//            var vehicles = ChoiceVAPI.FindNearbyVehicles(player, 100); //Check if Owner
//            var menu = new Menu("Autowahl", "Wähle ein Auto");
//            var counter = 0;
//            foreach (var vehicle in vehicles) {
//                var model = Enum.GetName(typeof(VehicleModel), vehicle.Model);
//                var numberPlate = "Kein Kennzeichen";
//                var vehobj = vehicle.getObject();
//                if (!(string.IsNullOrEmpty(vehicle.NumberplateText))) {
//                    numberPlate = vehicle.NumberplateText;
//                }
//                if (vehobj.OwnerId == player.getCharacterId()) {
//                    var status = "";
//                    if (vehobj.RegisteredPlate.Length == 0) {
//                        status = "Unangemeldet";
//                    } else {
//                        status = "Angemeldet";
//                    }
//                    menu.addMenuItem(new ClickMenuItem($"{model} ({status})", $"Kennzeichen: {numberPlate}; Angemeldet auf:{vehobj.RegisteredPlate}", $"{numberPlate}", eventString).withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }));
//                    counter++;
//                }
//                if (vehobj.OwnerType == Constants.VehicleOwnerType.Company) {
//                    var company = CompanyController.getCompany(vehobj.OwnerId);
//                    var employee = company.findEmployee(player.getCharacterId());
//                    if (employee != null && employee.JobLevel.IsCEO) {
//                        menu.addMenuItem(new ClickMenuItem($"{model}", $"", $"{numberPlate}", eventString).withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle } }));
//                        counter++;
//                    }
//                }
//            }
//            if (counter == 0) {
//                player.sendBlockNotification("Du hast kein Auto hier in der Nähe!", "Kein Auto");
//                return true;
//            }
//            player.showMenu(menu);
//            return true;
//        }

//        private bool onVehicleRegisterOwner(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            var vehObj = vehicle.getObject();
//            if (vehObj.RegisteredPlate.Length == 0) {
//                var menu = new Menu("Fahrzeug anmelden", "Melde dein Fahrzeug an");
//                menu.addMenuItem(new ClickMenuItem("Fahrzeug anmelden", "Meldet dein Fahrzeug auf ein beliebiges Kennzeichen an", "50$", "VEHICLE_REGISTRATION_REGISTER").withData(new Dictionary<string, dynamic> { {"Vehicle", vehicle }, { "NumberPlate", false } }));
//                player.showMenu(menu);
//            } else {
//                player.sendBlockNotification("Das Fahrzeug ist bereits angemeldet", "");
//                return true;
//            }
//            return true;
//        }

//        private bool onVehicleUnregisterOwner(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            var vehObj = vehicle.getObject();
//            if(vehObj.RegisteredPlate == "") {
//                player.sendBlockNotification("Das Fahrzeug ist bereits abgemeldet", "");
//                return true;
//            }
//            vehObj.RegisteredPlate = "";
//            vehObj.updateVehicleObject();
//            player.sendNotification(Constants.NotifactionTypes.Success, "Dein Fahrzeug wurde abgemeldet", "Fahrzeug abgemeldet");
//            return true;
//        }

//        private bool onVehicleChangeOwner(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            var vehObj = vehicle.getObject();
//            var playerMenu = new Menu("Spieler auswahl", "Wähle den passenden Spieler aus");
//            var counter = 0;
//            foreach (var target in ChoiceVAPI.GetAllPlayers()) {
//                if (target == player) {
//                    continue;
//                }
//                if (target.Position.Distance(player.Position) <= 10) {
//                    playerMenu.addMenuItem(new ClickMenuItem($"{target.getCharacterName()}", $"Überschreibt {Enum.GetName(typeof(VehicleModel), vehicle.Model)}", "150$", "VEHICLE_REGISTRATION_CHANGEOWNER").needsConfirmation("Fahrzeug sicher überschreiben?","").withData(new Dictionary<string, dynamic> { {"Vehicle", vehicle }, {"Target", target } }));
//                    counter++;
//                }
//            }
//            if (counter == 0) {
//                playerMenu.addMenuItem(new StaticMenuItem("Kein Spieler in der Nähe", "", "", MenuItemStyle.red));
//            }
//            player.showMenu(playerMenu);
//            return true;
//        }

//        private bool onVehicleRegisterCompany(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            if (vehicle.getObject().RegisteredPlate == "") {
//                player.sendBlockNotification("Dein Fahrzeug ist nicht angemeldet!", "");
//                return true;
//            }
//            if (vehicle.getObject().OwnerType == Constants.VehicleOwnerType.Company) {
//                player.sendBlockNotification("Das Auto ist bereits auf eine Firma zugelassen!", "Bereits zugelassen");
//                return true;
//            }
//            var companies = CompanyController.getCompanies(player);
//            var menu = new Menu("Firmenauswahl", "");
//            foreach (var company in companies) {
//                var employee = company.findEmployee(player.getCharacterId());
//                if (employee != null && employee.JobLevel.IsCEO) {
//                    menu.addMenuItem(new ClickMenuItem($"{company.Name}", "Registriert das Fahrzeug auf die ausgewählte Firma", "", "VEHICLE_REGISTRATION_REGISTER_COMPANY").withData(new Dictionary<string, dynamic> { {"Vehicle", vehicle }, {"Company", company } }));
//                }
//            }
//            player.showMenu(menu);
//            return true;
//        }

//        private bool onVehicleUnregisterCompany(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicle = (ChoiceVVehicle)data["Vehicle"];
//            if(vehicle.getObject().RegisteredPlate == "") {
//                player.sendBlockNotification("Dein Fahrzeug ist nicht angemeldet!", "");
//                return true;
//            }
//            if(vehicle.getObject().OwnerType != Constants.VehicleOwnerType.Company) {
//                player.sendBlockNotification("Das Auto ist auf keine Firma zugelassen!", "Keine Firmenzulassung");
//                return true;
//            }
//            var companies = CompanyController.getCompanies(player);
//            var menu = new Menu("Firmenauswahl", "");
//            foreach(var company in companies) {
//                var employee = company.findEmployee(player.getCharacterId());
//                if(employee != null && employee.JobLevel.IsCEO) {
//                    menu.addMenuItem(new ClickMenuItem($"{company.Name}", "Meldet das Fahrzeug von der Firma ab", "", "VEHICLE_REGISTRATION_UNREGISTER_COMPANY").withData(new Dictionary<string, dynamic> { { "Vehicle", vehicle }, { "Company", company } }));
//                }
//            }
//            player.showMenu(menu);
//            return true;
//        }

//        private bool onPedInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            var menu = new Menu("Automeldestelle", "Macht was mit deinem Auto");
//            menu.addMenuItem(new ClickMenuItem("Fahrzeug anmelden", "Registriert ein Fahrzeug auf ein Kennzeichen", "", "VEHICLEREGISTER_CHOOSE").withData(new Dictionary<string, dynamic> { { "Event", "VEHICLE_REGISTER_PLAYER" } })); //Kennzeichen eingabe
//            menu.addMenuItem(new ClickMenuItem("Fahrzeug abmelden", "Meldet ein registriertes Fahrzeug ab", "", "VEHICLEREGISTER_CHOOSE").withData(new Dictionary<string, dynamic> { { "Event", "VEHICLE_UNREGISTER_PLAYER" } }));
//            menu.addMenuItem(new ClickMenuItem("Fahrzeug ummelden", "Registriert dein Auto auf eine andere Person", "", "VEHICLEREGISTER_CHOOSE").withData(new Dictionary<string, dynamic> { { "Event", "VEHICLE_OWNERCHANGE_PLAYER" } }));
//            var companies = CompanyController.getCompanies(player);
//            var companyCheck = companies.FirstOrDefault(x => x.findEmployee(player.getCharacterId()) != null && x.findEmployee(player.getCharacterId()).JobLevel.IsCEO);
//            if (companyCheck != null) {
//                menu.addMenuItem(new ClickMenuItem("Auto auf Firma melden", "Registriert dein Auto auf eine Firma", "", "VEHICLEREGISTER_CHOOSE").withData(new Dictionary<string, dynamic> { { "Event", "VEHICLE_REGISTER_COMPANY" } }));
//                menu.addMenuItem(new ClickMenuItem("Auto von Firma abmelden", "Registriert deinen Firmenwagen auf eine Person", "", "VEHICLEREGISTER_CHOOSE").withData(new Dictionary<string, dynamic> { { "Event", "VEHICLE_UNREGISTER_COMPANY" } }));
//            }
//            menu.addMenuItem(new ClickMenuItem("Kennzeichen kaufen", "Erstellt ein neues Kennzeichen", "350$", "VEHICLEREGISTER_CREATE_NUMBERPLATE"));
//            player.showMenu(menu);
//            return true;
//        }
//    }
//}
