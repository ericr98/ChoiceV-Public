using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Garages.Model;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Garages {
    public class NPCGarageModule : NPCModule {
        private Garage Garage;

        public NPCGarageModule(ChoiceVPed ped, Garage garage) : base(ped) {
            Garage = garage;
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var modelName = "";
            var garage = Garage;
            if(!player.getCharacterData().AdminMode && player.getAdminLevel() < 3) {
                if(garage.OwnerType == Constants.GarageOwnerType.Player && garage.OwnerId != -1) {
                    if(player.getCharacterId() != garage.OwnerId) {
                        return new List<MenuItem> { new StaticMenuItem("Nicht der Besitzer", $"Sie sind nicht der Besitzer von {garage.Name}.", "", MenuItemStyle.red) };
                    }
                }

                if((garage.OwnerType == Constants.GarageOwnerType.Company) && garage.OwnerId != -1) {
                    var company = CompanyController.findCompanyById(garage.OwnerId);

                    if(company == null || !CompanyController.hasPlayerPermission(player, company, "GARAGE_ACCESS")) {
                        string name = garage.Name;

                        return new List<MenuItem> { new StaticMenuItem("Keinen Zugriff", $"Kein Zugriff auf diese Garage!", "", MenuItemStyle.red) };
                    }
                }
            }

            var subparkin = new Menu("Fahrzeug einparken", "Parke Fahrzeuge ein");
            foreach(var vehicle in ChoiceVAPI.FindNearbyVehicles(player, 15, v => (player.getCharacterData().AdminMode && player.getAdminLevel() >= 2) 
                    || v.hasPlayerAccess(player)
                    || player.isCop()).Concat(Garage.getAllVehiclesInSpots()).Distinct()) {

                if(vehicle != null) {
                    var vehcls = vehicle.DbModel._class;
                    bool allowed = true;

                    if(garage.Type == Constants.GarageType.AirVehicle) {
                        if(vehcls.classId == 14 || vehcls.classId != 15 || vehcls.classId != 16) {
                            allowed = false;
                        }
                    } else if(garage.Type == Constants.GarageType.WaterVehicle) {
                        if(vehcls.classId != 14 || vehcls.classId == 15 || vehcls.classId == 16) {
                            allowed = false;
                        }
                    } else {
                        if(vehcls.classId == 14 || vehcls.classId == 15 || vehcls.classId == 16) {
                            allowed = false;
                        }
                    }

                    if(allowed) {
                        var vehModel = vehicle.DbModel;
                        modelName = vehModel.DisplayName;

                        var subdata = new Dictionary<string, dynamic> { { "garage", Garage.Id }, { "vehicle", vehicle } };
                        if(modelName.Length > 0)
                            subparkin.addMenuItem(new ClickMenuItem($"Fahrzeug {modelName} ({vehicle.NumberplateText})", "", "", "GARAGE_INTERACTION_PARK_IN").withData(subdata));
                        else
                            subparkin.addMenuItem(new ClickMenuItem($"Fahrzeug {vehicle.NumberplateText}", "", "", "GARAGE_INTERACTION_PARK_IN").withData(subdata));
                    }
                }
            }

            var subparkout = new Menu("Fahrzeuge ausparken", "Parke Fahrzeuge aus", false);
            var companyGarageAllowance = garage.OwnerType == Constants.GarageOwnerType.Company;
            if(companyGarageAllowance) {
                var comp = CompanyController.findCompanyById(garage.OwnerId);
                companyGarageAllowance = CompanyController.hasPlayerPermission(player, comp, "GARAGE_ACCESS");
            }

            using(var db = new ChoiceVDb()) {
                foreach(var vehicle in db.vehicles.Include(v => v.model).Where(v => v.garageId == Garage.Id)) {
                    if(vehicle != null && vehicle.id != -1 && player.getInventory() != null) {
                        var vehModel = vehicle.model;
                        modelName = vehModel.DisplayName;

                        if(companyGarageAllowance || vehicle.hasPlayerAccess(player) 
                            || player.getCharacterData().AdminMode && player.getAdminLevel() >= 2
                            || player.isCop()) {
                            var subdata = new Dictionary<string, dynamic> { { "garage", Garage.Id }, { "vehicle", vehicle.id } };

                            if(modelName.Length > 0)
                                subparkout.addMenuItem(new ClickMenuItem($"Fahrzeug {modelName} ({vehicle.numberPlate})", "", "", "GARAGE_INTERACTION_PARK_OUT").withData(subdata));
                            else
                                subparkout.addMenuItem(new ClickMenuItem($"Fahrzeug {vehicle.numberPlate}", "", "", "GARAGE_INTERACTION_PARK_OUT").withData(subdata));
                        }
                    }
                }
            }

            if(subparkin.getMenuItemCount() > 0 || subparkout.getMenuItemCount() > 0) {
                var menu = new Menu("Garage", "Fahrzeuge ein- und ausparken");

                var list = new List<MenuItem>();

                if(subparkin.getMenuItemCount() > 0)
                    list.Add(new MenuMenuItem("Fahrzeug einparken", subparkin));
                else
                    list.Add(new StaticMenuItem("Fahrzeug einparken", "Kein Fahrzeug zum Einparken gefunden", "", MenuItemStyle.normal));

                if(subparkout.getMenuItemCount() > 0)
                    list.Add(new MenuMenuItem("Fahrzeug ausparken", subparkout));
                else
                    list.Add(new StaticMenuItem("Fahrzeug ausparken", "Kein Fahrzeug zum Ausparken gefunden", "", MenuItemStyle.normal));

                return list;
            } else {
                return new List<MenuItem> { new StaticMenuItem("Kein Fahrzeug gefunden", "Kein Fahrzeug zum Ein- oder Ausparken gefunden.", "", MenuItemStyle.yellow) };
            }
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Garagen Modul", $"Ein Modul welches das Öffnen der Garage: {Garage.Name} ermöglicht", "");
        }

        public override void onRemove() { }
    }
}
