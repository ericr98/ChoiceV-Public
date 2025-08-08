using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.Garages;
using ChoiceVServer.Controller.Garages.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.Controller {
    public class VehicleCarShopController : ChoiceVScript {
        public VehicleCarShopController() {
            EventController.addMenuEvent("SHOW_VEHICLE_IN_SHOWROOM", onShowVehicleInShowRoom);
            EventController.addMenuEvent("SHOW_VEHICLE_BUY", onShowVehicleBuy);
        }

        private bool onShowVehicleInShowRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (vehicle)data["Vehicle"];
            var functionality = (CarShopFunctionality)data["Company"];

            var model = vehicle.model;

            var color = VehicleColoring.fromDb(vehicle.vehiclescoloring);
            var damage = new VehicleDamage();
            damage.fillFromDb(vehicle);
            var tuning = VehicleTuning.fromDb(vehicle.vehiclestuningbase, vehicle.vehiclestuningmods.ToList());

            functionality.showCar(player, model, color, damage, tuning);

            return true;
        }

        private bool onShowVehicleBuy(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (vehicle)data["Vehicle"];
            var functionality = (CarShopFunctionality)data["Company"];
            var price = (decimal)data["Price"];

            functionality.buyCar(player, vehicle, price);

            return true;
        }
    }

    public class CarShopFunctionality : CompanyFunctionality {
        private static readonly decimal MAX_NPC_SELL_PRICE = 8000;

        private ChoiceVPed CompanyCarPed;

        public Garage SellGarage;
        public ChoiceVVehicle ShowCar;
        public Position ShowPosition;

        public CarShopFunctionality() : base() { }

        public CarShopFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "CAR_SHOP_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Fahrzeughandel", "Ermöglicht es der Firma Fahrzeuge zu verkaufen");
        }

        public override void onLoad() {
            var showPos = Company.getSetting<Position>("CAR_SHOW_ROOM_POSITION");
            if(showPos != default) {
                ShowPosition = showPos;
            }

            var garageId = Company.getSetting<int>("CAR_SHOP_SHOW_GARAGE");
            if(garageId != default) {
                SellGarage = GarageController.findGarageById(garageId);
            }

            //Peds
            Company.registerCompanyAdminElement(
                "CAR_SHOP_CREATE_PED",
                getPedCreatorGenerator,
                onPedCreator
            );

            //ShowRoom Stuff
            Company.registerCompanyAdminElement(
                "CAR_SHOP_CREATE_SHOW_ROOM",
                getShowRoomCreateGenerator,
                onShowRoomCreator
            );
        }

        public override void onRemove() {
            Company.deleteSetting("CAR_SHOW_ROOM_POSITION");
            Company.deleteSetting("CAR_SHOP_SHOW_GARAGE");

            Company.unregisterCompanyElement("CAR_SHOP_CREATE_PED");
            Company.unregisterCompanyElement("CAR_SHOP_CREATE_SHOW_ROOM");
        }

        private MenuItem onCarShopPedInteract(IPlayer player) {
            var buyMenu = new Menu("Fahrzeug kaufen", "Welches Fahrzeug willst du kaufen?");

            foreach(var v in SellGarage.getAllParkedVehicles()) {
                vehicle vehicle;
                using(var db = new ChoiceVDb()) {
                    vehicle = db.vehicles.Find(v.id);
                    db.Entry(vehicle).References.ForEach(c => c.Load());
                    db.Entry(vehicle).Collections.ForEach(c => c.Load());
                }

                var prePrice = OrderController.getPriceOfVehicle(vehicle.model.id);
                if(prePrice == null) {
                    continue;
                }
                var price = Math.Round((decimal)(prePrice * 1.07m), 2);

                var dispPrice = $"${price}";
                if(price > MAX_NPC_SELL_PRICE) {
                    dispPrice = "N.A";
                }

                var subMenu = new Menu($"{vehicle.model.DisplayName}: {dispPrice}", "Kaufe dieses Fahrzeug");

                if(price <= MAX_NPC_SELL_PRICE) {
                    subMenu.addMenuItem(new StaticMenuItem("Preis", $"Der Preis des Fahrzeuges ist ${price}", $"${price}"));
                } else {
                    subMenu.addMenuItem(new StaticMenuItem("Preis", "Dieses Fahrzeug wird nicht über diesen Mitarbeiter verkauft.", "N.A.", MenuItemStyle.yellow));
                }

                subMenu.addMenuItem(new StaticMenuItem("Modell", $"Das Modell des Fahrzeuges ist {vehicle.model.DisplayName}", $"{vehicle.model.DisplayName}"));
                var milage = Math.Round((double)(vehicle.drivenDistance / 1000), 2);
                subMenu.addMenuItem(new StaticMenuItem("Kilometerstand", $"Der Kilometerstand des Fahrzeuges beträgt: {milage}km", $"{milage}km"));

                var color = VehicleColoringController.getVehicleColorById(vehicle.vehiclescoloring.primaryColor);
                subMenu.addMenuItem(new StaticMenuItem("Farbe", $"Das Fahrzeug kommt in der Farbe: {color.name}", $"{color.name}"));

                //TODO VEHICLE_DAMAGE
                //var damage = VehicleDamage.fromDb(vehicle.vehiclesdamagebasis, vehicle.vehiclesdamageparts.ToList(), vehicle.vehiclesdamagetyres.ToList(), vehicle.vehiclesdamagesimpleparts.ToList(), vehicle.vehiclesdamagebumpers.ToList());

                //switch(damage.getDamageLevel()) {
                //    case 0:
                //        subMenu.addMenuItem(new StaticMenuItem("Beschädigung", "Das Fahrzeug ist nicht beschädigt", "Unbeschädigt"));
                //        break;
                //    case 1:
                //        subMenu.addMenuItem(new StaticMenuItem("Beschädigung", "Das Fahrzeug ist leicht beschädigt!", "Leicht beschädigt", MenuItemStyle.yellow));
                //        break;
                //    case 2:
                //        subMenu.addMenuItem(new StaticMenuItem("Beschädigung", "Das Fahrzeug ist schwer beschädigt!", "Stark beschädigt", MenuItemStyle.red));
                //        break;
                //}

                if(vehicle.vehiclestuningmods.Count == 0) {
                    subMenu.addMenuItem(new StaticMenuItem("Tuning", "Das Fahrzeug ist ungetuned", "Ungetuned"));
                } else {
                    subMenu.addMenuItem(new StaticMenuItem("Tuning", "Das Fahrzeug ist getuned!", "Getuned", MenuItemStyle.yellow));
                }

                var trunk = InventoryController.loadInventory(vehicle);

                var hasDocs = trunk.hasItems<VehicleKey>(vk => vk.VehicleId == vehicle.id && vk.LockVersion == vehicle.keyLockVersion, 2) && trunk.hasItem<VehicleRegistrationCard>(vc => vc.VehicleId == vehicle.id);

                InventoryController.unloadInventory(trunk);

                var data = new Dictionary<string, dynamic> {
                    { "Company", this },
                    { "Vehicle", vehicle },
                    { "Price", price },
                };

                subMenu.addMenuItem(new ClickMenuItem("Anzeigen", "Lasse dir das Modell anzeigen", "", "SHOW_VEHICLE_IN_SHOWROOM", MenuItemStyle.green).withData(data));

                if(price < MAX_NPC_SELL_PRICE && hasDocs) {
                    subMenu.addMenuItem(new ClickMenuItem("Kaufen", "Kaufe das Fahrzeug. Dazu wird dein Hauptbankkonto benutzt.", "Per Überweisung!", "SHOW_VEHICLE_BUY", MenuItemStyle.green).withData(data).needsConfirmation($"{vehicle.model.DisplayName} kaufen?", $"Wirklich für ${price} kaufen?"));
                } else {
                    if(hasDocs) {
                        subMenu.addMenuItem(new StaticMenuItem("Nicht kaufbar", $"Kann nicht hier gekauft werden. Dieser Mitarbeiter verkauft nur Fahrzeuge bis ${MAX_NPC_SELL_PRICE}", "Zu teuer", MenuItemStyle.red));
                    } else {
                        subMenu.addMenuItem(new StaticMenuItem("Nicht kaufbar", "Kann nicht hier gekauft werden. Die nötigen Papiere bzw. Schlüssel für das Fahrzeug sind nicht verfügbar!", "Zubehör fehlt", MenuItemStyle.yellow));
                    }
                }

                buyMenu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
            }

            if(buyMenu.getMenuItemCount() == 0) {   
                buyMenu.addMenuItem(new StaticMenuItem("Keine Fahrzeuge", "Es sind aktuell keine Fahrzeuge verfügbar", "Keine Fahrzeuge", MenuItemStyle.yellow));
            }

            return new MenuMenuItem(buyMenu.Name, buyMenu);
        }

        public void showCar(IPlayer player, configvehiclesmodel model, VehicleColoring coloring, VehicleDamage damage, VehicleTuning tuning) {
            if(ShowCar != null) {
                player.sendBlockNotification("Es wird aktuell schon ein Fahrzeug angezeigt!", "Spot blockiert", Constants.NotifactionImages.Car);
                return;
            }

            ShowCar = ChoiceVAPI.CreateVehicle(ChoiceVAPI.Hash(model.ModelName), ShowPosition, Rotation.Zero);
            ShowCar.NumberplateText = "BUY ME";
            ShowCar.applyVehicleColoring(coloring);
            ShowCar.applyVehicleDamage(damage);
            ShowCar.applyVehicleTuning(tuning);
            ShowCar.Frozen = true;
            ShowCar.Collision = false;

            ShowCar.LockState = VehicleLockState.Locked;
            ShowCar.SetNetworkOwner(player, false);

            player.sendNotification(Constants.NotifactionTypes.Success, "Das Fahrzeug wird angezeigt!", "Fahrzeug wird angezeigt!", Constants.NotifactionImages.Car);

            InvokeController.AddTimedInvoke("Vehicle-Spinner", i => {
                player.emitClientEvent("MAKE_VEHICLE_DISPLAY", ShowCar);
            }, TimeSpan.FromSeconds(1), false);

            InvokeController.AddTimedInvoke("Vehicle-Spinner-Remove", i => {
                ChoiceVAPI.RemoveVehicle(ShowCar);
                ShowCar = null;
            }, TimeSpan.FromSeconds(20), false);
        }

        public void buyCar(IPlayer player, vehicle vehicle, decimal price) {
            if(SellGarage.hasAFreeSpace()) {
                var mainAccount = player.getMainBankAccount();
                if(!BankController.transferMoney(player.getMainBankAccount(), Company.CompanyBankAccount, price, $"Kauf von {vehicle.model.DisplayName} mit Chassisnummer: {vehicle.chassisNumber}", out var message, false)) {
                    player.sendBlockNotification($"Kauf fehlgeschlagen! Die Überweisung ist fehlgeschlagen mit der Meldung: {message}", "Kauf fehlgeschlagen", Constants.NotifactionImages.Car);
                    return;
                }

                var invoiceFile = InvoiceController.createInvoice(Company, new List<InvoiceProduct> { new(1, price, $"{vehicle.model.DisplayName}") });

                invoiceFile.SellerSignature = $"{Company.Name}";
                invoiceFile.BuyerSignature = $"{player.getCharacterShortName()}";
                invoiceFile.PaymentInfo = $"Überweisung über {mainAccount}";
                invoiceFile.Date = DateTime.Now.ToString("dd.MM.yyyy");
                invoiceFile.AdditionalInfo = $"Fahrzeug: {vehicle.model.DisplayName} mit Chassisnummer: {vehicle.chassisNumber}. Inklusive waren: 2 Schlüssel sowie die Eigentümerkarte.";
                invoiceFile.HasBeenPayed = true;

                var copy = new InvoiceFile(invoiceFile);

                Company.Safe?.addItem(copy, true);

                player.getInventory().addItem(invoiceFile, true);

                GarageController.unparkVehicleFromGarage(vehicle.id);
                player.sendNotification(Constants.NotifactionTypes.Success, "Fahrzeug erfolgreich gekauft. Du hast eine Rechnung erhalten. Es befinden sich noch einige Dokumente/Schlüssel im Kofferraum", "Fahrzeug gekauft!", Constants.NotifactionImages.Car);
                player.sendNotification(Constants.NotifactionTypes.Warning, "Fahrzeuge bleiben aufgeschlossen, wenn sie einen eigenen Schlüssel im Kofferraum haben. Nimm also beide heraus!", "Fahrzeug gekauft!", Constants.NotifactionImages.Car);
            } else {
                player.sendBlockNotification("Kauf fehlgeschlagen! Es ist kein freier Ausparkpunkt verfügbar!", "Kauf fehlgeschlagen", Constants.NotifactionImages.Car);
            }
        }

        #region Peds

        private MenuElement getPedCreatorGenerator(IPlayer player) {
            var menu = new Menu("Peds setzen", "Setze die verschiedenen Peds");
            menu.addMenuItem(new ClickMenuItem("NPC-Verkäufer setzen", "Setze den NPC-Verkäufer. Bei ihm können in Abwesenheit billige Fahrzeuge gekauft werden", "", "CAR_SHOP_PED"));
            return menu;
        }

        private void onPedCreator(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            switch(subEvent) {
                case "CAR_SHOP_PED":
                    Company.setSetting("CAR_SHOP_PED_POSITION", player.Position.ToJson());
                    Company.setSetting("CAR_SHOP_PED_HEADING", player.Rotation.Yaw.ToJson());

                    player.sendNotification(Constants.NotifactionTypes.Success, "Ped erfolgreich gesetzt", "Ped gesetzt");
                    break;
            }
        }

        #endregion

        #region ShowRoom

        private MenuElement getShowRoomCreateGenerator(IPlayer player) {
            var menu = new Menu("Show-Room setzen", "Setze die Daten für den Show Room");
            menu.addMenuItem(new ClickMenuItem("Show Position setzen", "Setze den Auto-Showroom. An diesem Punkt können die verschiedenen Fahrzeuge angezeigt werden", "", "CAR_SHOP_SHOW_POSITION"));
            menu.addMenuItem(new InputMenuItem("Garage setzen", "Setze die Garage für den Show Room", "Name", "CAR_SHOP_SHOW_GARAGE"));
            menu.addMenuItem(new ClickMenuItem("Ausparkspot setzen", "Setze den Ausparkspot für die Showgarage", "", "CAR_SHOP_SHOW_GARAGE_PARK_OUT"));
            return menu;
        }

        private void onShowRoomCreator(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            switch(subEvent) {
                case "CAR_SHOP_SHOW_POSITION":
                    Company.setSetting("CAR_SHOW_ROOM_POSITION", player.Position.ToJson());
                    ShowPosition = player.Position;
                    player.sendNotification(Constants.NotifactionTypes.Success, "Position erfolgreich gesetzt", "Position gesetzt");
                    break;
                case "CAR_SHOP_SHOW_GARAGE":
                    var evt = menuItemCefEvent as InputMenuItemEvent;
                    var garageId = Company.getSetting<int>("CAR_SHOP_SHOW_GARAGE");

                    if(garageId != default) {
                        GarageController.deleteGarage(garageId);
                    }

                    var garage = GarageController.createGarage(player, evt.input, Constants.GarageType.GroundVehicle, Constants.GarageOwnerType.Company, Company.Id, 100, true);
                    Company.setSetting("CAR_SHOP_SHOW_GARAGE", garage.Id.ToString());
                    SellGarage = garage;
                    player.sendNotification(Constants.NotifactionTypes.Success, "Garage erfolgreich gesetzt", "Garage gesetzt");
                    break;
                case "CAR_SHOP_SHOW_GARAGE_PARK_OUT":
                    var garageId2 = Company.getSetting<int>("CAR_SHOP_SHOW_GARAGE");

                    if(garageId2 == default) {
                        player.sendBlockNotification("Setze erst eine Garage!", "Keine Garage!");
                        return;
                    }

                    var garage2 = GarageController.findGarageById(garageId2);

                    if(garage2.SpawnPositions.Count > 0) {
                        GarageController.deleteGarageSpot(garage2.Id, garage2.SpawnPositions.First().Id);
                    }

                    player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun den Spot!", "");
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                        GarageController.createGarageSpot(player, garage2.Id, p, r, w, h);
                        player.sendNotification(Constants.NotifactionTypes.Success, "Spot erfolgreich gesetzt", "");
                    });

                    break;
            }
        }

        #endregion

        #region Employee Enter/Exit

        public override void onLastEmployeeLeaveDuty() {
            if(CompanyCarPed == null) {
                var posString = Company.getSetting("CAR_SHOP_PED_POSITION");
                if(posString != null) {
                    var pos = posString.FromJson();
                    pos.Z -= 1;
                    var heading = float.Parse(Company.getSetting("CAR_SHOP_PED_HEADING"));
                    CompanyCarPed = PedController.createPed("Fahrzeughändler", "u_m_y_antonb", pos, ChoiceVAPI.radiansToDegrees(heading));
                    CompanyCarPed.addModule(new NPCMenuItemsModule(CompanyCarPed, [onCarShopPedInteract]));
                }
            }
        }

        public override void onFirstEmployeeEnterDuty() {
            if(CompanyCarPed != null) {
                PedController.destroyPed(CompanyCarPed);
                CompanyCarPed = null;
            }
        }

        #endregion
    }
}
