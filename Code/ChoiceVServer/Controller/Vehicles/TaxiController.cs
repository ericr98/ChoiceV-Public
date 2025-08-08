using AltV;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Vehicles {

    //TODO: only useful vehicles should become taxis // atm. only check for Vehicle with seats > 1
    //Bug: to zero reset while saving to permanent data ?? Issue not reproducible
    // -> happend when using other taxi then startet task in. ToDo: try killing data-obj after leaving veh
    public class TaxiController : ChoiceVScript {
        private static Dictionary<int, Taxi> taxiDict;
        public TaxiController() {
            EventController.MainAfterReadyDelegate += init;
            VehicleController.addSelfMenuElement(new ConditionalVehicleGeneratedSelfMenuElement("Taxometer Optionen", menuGenerator, taxiVehiclePredicate, taxiPlayerPredicate));
            VehicleController.addSelfMenuElement(new ConditionalVehicleGeneratedSelfMenuElement("Taxometer Einbau", menuGeneratorInstallation, taxiVehicleInstallation, taxiPlayerInstallation));
            // MenuEvents for starting Taxitask
            EventController.addMenuEvent("TAXOMETER_START_WITH_DEPLOYCOSTS", onTaxometerStartWithDeploycost);
            EventController.addMenuEvent("TAXOMETER_START_WITHOUT_DEPLOYCOSTS", onTaxometerStartWithOutDeploycost);
            // MenuEvents for showing Taxometer
            EventController.addMenuEvent("TAXOMETER_SHOW", onTaxometerShow);
            EventController.addMenuEvent("TAXOMETER_NOSHOW", onTaxometerNoShow);
            EventController.addMenuEvent("TAXOMETER_ABORT_TASK", onTaxometerEndTask);
            EventController.addMenuEvent("TAXOMETER_INPUT", onTaxometerInput);
            EventController.addMenuEvent("TAXOMETER_PAYMENT_SELECTION", onTaxometerPaymentSelection);
            // Payment MenuEvent
            EventController.addMenuEvent("TAXOMETER_PAYMENT_BY_BANK", onTaxometerPaymentByBank);
            EventController.addMenuEvent("TAXOMETER_PAYMENT_BY_CASH", onTaxometerPaymentByCash);
            EventController.addMenuEvent("TAXOMETER_PAYMENT_ABORT", onTaxometerPaymentAbort);
            //Taxometer Installation
            EventController.addMenuEvent("TAXOMETER_INSTALLTION", onTaxometerInstallation);
        }



        #region Menu
        private Menu menuGenerator(ChoiceVVehicle vehicle, IPlayer player) {
            //MainMenu for taxiInteraction
            Menu menu = new Menu("Taxometer Optionen", "Was möchtest du Tun?");
            if (player.Vehicle.Driver == player) {
                if (!Convert.ToBoolean(vehicle.getData("TAXI_ACTIVTASK_PERMANENT"))) {
                    menu.addMenuItem(new ClickMenuItem("Starte Fahrt mit Anfahrtkosten", "Startet eine Fahrt und rechnet die Anfahrtskosten ein.", "", "TAXOMETER_START_WITH_DEPLOYCOSTS"));
                    menu.addMenuItem(new ClickMenuItem("Starte Fahrt ohne Anfahrtkosten", "Startet eine Fahrt ohne Anfahrtskosten", "", "TAXOMETER_START_WITHOUT_DEPLOYCOSTS"));
                } else {
                    List<string> usedSeats = new List<string>();
                    foreach (var key in vehicle.PassengerList) {
                        usedSeats.Add("Sitz " + key.Key);
                    }
                    menu.addMenuItem(new ListMenuItem("Fahrt abrechnen", "Rechnet mit dem Passagier auf dem ausgewählten Sitz ab.", usedSeats.ToArray(), "TAXOMETER_PAYMENT_SELECTION", MenuItemStyle.normal, true));
                    menu.addMenuItem(new ClickMenuItem("Auftrag Abbrechen", "Bricht den aktuellen Auftrag ab.", "", "TAXOMETER_ABORT_TASK", MenuItemStyle.red));
                }
                if (Convert.ToBoolean(vehicle.getData("TAXI_COMPANY_CONTROLLED_PERMANENT")) || Convert.ToBoolean(vehicle.getData("TAXI_ACTIVTASK_PERMANENT"))) {
                    menu.addMenuItem(new InputMenuItem("Anfahrtskosten", "Kosten die einmalig für die Anfahrt berechnet werden", "", "").withStartValue(vehicle.getData("TAXI_DEPLOYCOST_PERMANENT")).withDisabledInputField(true));
                    menu.addMenuItem(new InputMenuItem("Kosten/100m", "Kosten die für jede 100m Fahrt berechnet werden.", "", "").withStartValue(vehicle.getData("TAXI_PRICE_PERMANENT")).withDisabledInputField(true));
                } else {
                    menu.addMenuItem(new InputMenuItem("Anfahrtskosten", "Kosten die einmalig für die Anfahrt berechnet werden", "", "").withStartValue(taxiDict[vehicle.VehicleId].DeployCost.ToString()));
                    menu.addMenuItem(new InputMenuItem("Kosten/100m", "Kosten die für jede 100m Fahrt berechnet werden.", "", "").withStartValue(taxiDict[vehicle.VehicleId].PricePer100.ToString()));
                    menu.addMenuItem(new MenuStatsMenuItem("Kosten setzen", "Bestätige deine Änderungen", "TAXOMETER_INPUT", MenuItemStyle.green));
                }
                //MenuStatsMenuItem
            } else {
                if (taxiDict[vehicle.VehicleId].PLayerInvokes.ContainsKey(player.Id)) {
                    menu.addMenuItem(new ClickMenuItem("Schau weg vom Taxometer", "Verbirgt das Taxometer", "", "TAXOMETER_NOSHOW"));
                } else {
                    menu.addMenuItem(new ClickMenuItem("Schau das Taxometer an", "Zeigt das Taxometer an", "", "TAXOMETER_SHOW"));
                }


            }
            //Condition sens on seat (driver or not)
            return menu;
        }

        private bool taxiVehiclePredicate(ChoiceVVehicle obj) {
            if (obj.hasData("TAXI_IS_TAXI")) {
                if (Convert.ToBoolean(obj.getData("TAXI_IS_TAXI"))) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }

        }

        private bool taxiPlayerPredicate(IPlayer obj) {
            return true;
        }
        private Menu menuGeneratorInstallation(ChoiceVVehicle vehicle, IPlayer player) {
            Menu menu = new Menu("Taxometer Einbau", "Was möchtest du Tun?");

            if (player.Vehicle.Driver == player) {
                var cfg = InventoryController.getConfigItemByCodeIdentifier("TAXOMETER_BASIC");
                StaticItem item = null;
                if (cfg != null) {
                    item = player.getInventory().getItem<StaticItem>(i => i.ConfigId == cfg.configItemId);
                }
                var screwDriver = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.Screwdriver);
                var automotivePliers = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.AutomotivePliers);
                if (item != null && screwDriver != null && automotivePliers != null) {
                    menu.addMenuItem(new ClickMenuItem("Baue Taxometer ein", "Baut ein Taxometer ein.", "", "TAXOMETER_INSTALLTION"));
                }
            }
            return menu;
        }
        private bool taxiVehicleInstallation(ChoiceVVehicle obj) {
            if (!obj.hasData("TAXI_IS_TAXI")) {
                return true;
            } else {
                return false;
            }
        }
        private bool taxiPlayerInstallation(IPlayer obj) {
            return true;
        }
        private bool onTaxometerStartWithDeploycost(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle vehicle = (ChoiceVVehicle)player.Vehicle;
            taxiDict[vehicle.VehicleId].startTask(true);
            return true;
        }

        private bool onTaxometerStartWithOutDeploycost(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle vehicle = (ChoiceVVehicle)player.Vehicle;
            taxiDict[vehicle.VehicleId].startTask(false);
            return true;
        }
        private bool onTaxometerEndTask(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle vehicle = (ChoiceVVehicle)player.Vehicle;
            taxiDict[vehicle.VehicleId].stopTask();
            player.sendNotification(NotifactionTypes.Warning, "Die Fahrt wurde abgebrochen ohne Zahlung!", "Fahrt abgebrochen!");
            return true;
        }
        private bool onTaxometerShow(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle temp = (ChoiceVVehicle)player.Vehicle;
            taxiDict[temp.VehicleId].enterTaxi(player);
            return true;
        }

        private bool onTaxometerNoShow(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle temp = (ChoiceVVehicle)player.Vehicle;
            taxiDict[temp.VehicleId].leaveTaxi(player);
            return true;
        }
        private bool onTaxometerInput(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var deployCostEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var pricePer100mEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
            ChoiceVVehicle temp = (ChoiceVVehicle)player.Vehicle;
            taxiDict[temp.VehicleId].DeployCost = float.Parse(deployCostEvt.input);
            taxiDict[temp.VehicleId].PricePer100 = float.Parse(pricePer100mEvt.input);
            return true;
        }

        private bool onTaxometerPaymentSelection(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;
            var seatId = int.Parse(evt.currentElement.ToCharArray().Last().ToString());
            if (evt.action == "changed") {
                player.emitClientEvent("VEHICLE_SHOW_PASSENGER", seatId);
            } else {
                var passenger = ((ChoiceVVehicle)player.Vehicle).PassengerList[seatId];

                if (passenger != null) {
                    triggerPaymentMenu(passenger, data);
                }
            }
            return true;
        }
        private bool onTaxometerInstallation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle temp = (ChoiceVVehicle)player.Vehicle;
            var cfg = InventoryController.getConfigItemByCodeIdentifier("TAXOMETER_BASIC");
            var item = player.getInventory().getItem<StaticItem>(i => i.ConfigId == cfg.configItemId);
            var screwDriver = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.Screwdriver);
            var automotivePliers = player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.AutomotivePliers);
            newTaxi(temp);
            taxiDict[temp.VehicleId].enterTaxi(player);
            item.use(player);
            screwDriver.use(player);
            automotivePliers.use(player);
            return true;
        }


        private void triggerPaymentMenu(IPlayer passenger, Dictionary<string, dynamic> data) {
            Menu menu = new Menu("Bitte Taxirechnung bezahlen", "Du hast eine Rechnung bekommen.");
            if (data.ContainsKey("BANK_FAILED")) {
                if (!data["BANK_FAILED"]) {
                    menu.addMenuItem(new ClickMenuItem("Mit Karte bezahlen", "Bezahle mit deiner Kontokarte.", "", "TAXOMETER_PAYMENT_BY_BANK").withData(data).needsConfirmation("Wirklich bezahlen?", "Bestätigen"));
                }
            } else {
                menu.addMenuItem(new ClickMenuItem("Mit Karte bezahlen", "Bezahle mit deiner Kontokarte.", "", "TAXOMETER_PAYMENT_BY_BANK").withData(data).needsConfirmation("Wirklich bezahlen?", "Bestätigen"));
            }
            if (data.ContainsKey("CASH_FAILED")) {
                if (!data["CASH_FAILED"]) {
                    menu.addMenuItem(new ClickMenuItem("Mit Bargeld bezahlen", "Bezahle mit dem Geld in deiner Tasche.", "", "TAXOMETER_PAYMENT_BY_CASH").withData(data).needsConfirmation("Wirklich bezahlen?", "Bestätigen"));
                }
            } else {
                menu.addMenuItem(new ClickMenuItem("Mit Bargeld bezahlen", "Bezahle mit dem Geld in deiner Tasche.", "", "TAXOMETER_PAYMENT_BY_CASH").withData(data).needsConfirmation("Wirklich bezahlen?", "Bestätigen"));
            }
            menu.addMenuItem(new ClickMenuItem("Bezahlung ablehnen", "Du weißt die Aufforderung zurück.", "", "TAXOMETER_PAYMENT_ABORT").withData(data).needsConfirmation("Wirklich abbrechen?", "Bestätigen"));
            passenger.showMenu(menu);
        }

        private bool onTaxometerPaymentByBank(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle temp = (ChoiceVVehicle)player.Vehicle;
            if (taxiDict[temp.VehicleId].Company > -1) {
                if (BankController.transferMoney(player.getMainBankAccount(), CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).CompanyBankAccount,
                    (decimal)taxiDict[temp.VehicleId].getPrice(), $"Taxifahrt in {temp.NumberplateText}/{CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).Name}. Danke für Ihren Auftrag!", out var mess, false)) {
                    player.sendNotification(NotifactionTypes.Info, mess, "");
                    temp.Driver.sendNotification(NotifactionTypes.Success, $"Bankantwort: {mess}", "Überweisung erfolgreich", NotifactionImages.ATM);
                    InvoiceProduct prod = new InvoiceProduct(1, (decimal)taxiDict[temp.VehicleId].getPrice(), "Taxibeförderung");
                    InvoiceFile invoice = InvoiceController.createInvoice(CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company), new List<InvoiceProduct> { prod });

                    invoice.SellerSignature = $"{CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).Name}";
                    invoice.PaymentInfo = $"Überweisung über {player.getMainBankAccount()}";
                    invoice.Date = DateTime.Now.ToString("dd.MM.yyyy");
                    invoice.HasBeenPayed = true;

                    var file = new InvoiceFile(invoice);
                    if (CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).Safe != null) {
                        CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).Safe.addItem(file, true);
                    } else {
                        temp.Driver.getInventory().addItem(file);
                    }
                    player.getInventory().addItem(file, true);
                    taxiDict[temp.VehicleId].stopTask();
                    validatedPayment();
                } else {
                    player.sendNotification(NotifactionTypes.Danger, mess, "");
                    data["BANK_FAILED"] = true;
                    triggerPaymentMenu(player, data);
                }
            } else {
                if (player.getMainBankAccount() > 1000000 && temp.Driver.getMainBankAccount() > 1000000) {
                    if (BankController.transferMoney(player.getMainBankAccount(), temp.Driver.getMainBankAccount(), (decimal)taxiDict[temp.VehicleId].getPrice(),
                        $"Taxifahrt in {temp.NumberplateText}. Danke für Ihren Auftrag!", out var mess, false)) {
                        player.sendNotification(NotifactionTypes.Info, mess, "");
                        temp.Driver.sendNotification(NotifactionTypes.Success, $"Bankantwort: {mess}", "Überweisung erfolgreich", NotifactionImages.ATM);
                        taxiDict[temp.VehicleId].stopTask();
                        validatedPayment();
                    } else {
                        player.sendNotification(NotifactionTypes.Danger, mess, "");
                        data["BANK_FAILED"] = true;
                        triggerPaymentMenu(player, data);
                    }
                } else {
                    player.sendNotification(NotifactionTypes.Danger, "Fehler im Kartenlesegerät. Ein Konto konnte nicht erkannt werden.", "");
                    data["BANK_FAILED"] = true;
                    triggerPaymentMenu(player, data);
                }
            }
            return true;
        }

        private bool onTaxometerPaymentByCash(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle temp = (ChoiceVVehicle)player.Vehicle;
            decimal cash = player.getCash();

            if (taxiDict[temp.VehicleId].Company > -1) {
                //style is nesseccary because of float/decimal comparison issues
                if (player.removeCash((decimal)taxiDict[temp.VehicleId].getPrice())) {
                    temp.Driver.addCash((decimal)taxiDict[temp.VehicleId].getPrice());
                    InvoiceProduct prod = new InvoiceProduct((int)taxiDict[temp.VehicleId].Distance, (decimal)taxiDict[temp.VehicleId].getPrice(), "Taxibeförderung");
                    InvoiceFile invoice = InvoiceController.createInvoice(CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company), new List<InvoiceProduct> { prod });

                    invoice.SellerSignature = $"{CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).Name}";
                    invoice.PaymentInfo = $"Barzahlung an Fahrer.";
                    invoice.Date = DateTime.Now.ToString("dd.MM.yyyy");
                    invoice.HasBeenPayed = true;

                    var file = new InvoiceFile(invoice);
                    if (CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).Safe != null) {
                        CompanyController.getCompanyById(taxiDict[temp.VehicleId].Company).Safe.addItem(file, true);
                    } else {
                        temp.Driver.getInventory().addItem(file);
                    }
                    player.getInventory().addItem(file, true);
                    player.sendNotification(NotifactionTypes.Success, "Du hast bezahlt und einen Beleg bekommen.", "Rechnung bezahlt");
                    temp.Driver.sendNotification(NotifactionTypes.Success, "Die Summe wurde bezahlt. Du hast einen Beleg rausgegeben.", "Rechnung bezahlt");
                    taxiDict[temp.VehicleId].stopTask();
                    validatedPayment();
                } else {
                    player.sendNotification(NotifactionTypes.Danger, "Du hast nicht genug Bargeld dabei!", "Zahlung nicht möglich");
                    data["CASH_FAILED"] = true;
                    triggerPaymentMenu(player, data);
                }

            } else {
                if (player.removeCash((decimal)taxiDict[temp.VehicleId].getPrice())) {
                    temp.Driver.addCash((decimal)taxiDict[temp.VehicleId].getPrice());
                    player.sendNotification(NotifactionTypes.Success, "Du hast bezahlt.", "Preis bezahlt");
                    temp.Driver.sendNotification(NotifactionTypes.Success, "Die Summe wurde bezahlt.", "Preis bezahlt");
                    taxiDict[temp.VehicleId].stopTask();
                    validatedPayment();
                } else {
                    player.sendNotification(NotifactionTypes.Danger, "Du hast nicht genug Bargeld dabei!", "Zahlung nicht möglich");
                    data["CASH_FAILED"] = true;
                    triggerPaymentMenu(player, data);
                }
            }
            return true;
        }

        private bool onTaxometerPaymentAbort(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ChoiceVVehicle temp = (ChoiceVVehicle)player.Vehicle;
            player.sendNotification(NotifactionTypes.Danger, "Du hast nicht bezahlt!", "Zahlung verweigert");
            temp.Driver.sendNotification(NotifactionTypes.Danger, "Die Summe wurde nicht bezahlt.", "Zahlung verweigert");
            return true;
        }

        #endregion
        private void init() {
            taxiDict = new Dictionary<int, Taxi>();
            getTaxiFromDB();
            EventController.VehicleMovedDelegate += onMove;
            EventController.PlayerEnterVehicleDelegate += onEnter;
            EventController.PlayerExitVehicleDelegate += onLeave;
            EventController.PlayerChangeVehicleSeatDelegate += onChangeSeat;
        }

        private void getTaxiFromDB() {
            List<ChoiceVVehicle> temp = ChoiceVAPI.GetAllVehicles().Cast<ChoiceVVehicle>().ToList<ChoiceVVehicle>();
            foreach (var vehicle in temp) {
                if (vehicle.hasData("TAXI_IS_TAXI")) {
                    Taxi tempTaxi = new Taxi(vehicle, float.Parse(vehicle.getData("TAXI_PRICE_PERMANENT")),
                        float.Parse(vehicle.getData("TAXI_DISTANCE_PERMANENT")), float.Parse(vehicle.getData("TAXI_DEPLOYCOST_PERMANENT")), Convert.ToInt32(vehicle.getData("TAXI_COMPANY_PERMANENT")),
                        Convert.ToBoolean(vehicle.getData("TAXI_ACTIVTASK_PERMANENT")), Convert.ToBoolean(vehicle.getData("TAXI_RATE_PERMANENT")), Convert.ToBoolean(vehicle.getData("TAXI_COMPANY_CONTROLLED_PERMANENT")));
                    taxiDict.Add(tempTaxi.Veh.VehicleId, tempTaxi);
                }
            }
        }

        private void onMove(object sender, ChoiceVVehicle vehicle, Position moveFromPos, Position moveToPosition, float distance) {
            if (vehicle.hasData("TAXI_IS_TAXI") && taxiDict[vehicle.VehicleId].Active) {
                taxiDict[vehicle.VehicleId].updateTask(distance);
            }
        }

        private void onEnter(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if (vehicle.hasData("TAXI_IS_TAXI")) {
                bool test = Convert.ToBoolean(vehicle.getData("TAXI_IS_TAXI"));
                if (Convert.ToBoolean(vehicle.getData("TAXI_IS_TAXI"))) {
                    if (taxiDict.ContainsKey(vehicle.VehicleId)) {
                        //if (seatId == 1) {
                        taxiDict[vehicle.VehicleId].enterTaxi(player);
                        //}
                    }
                }
            } else {

            }
        }

        private void onChangeSeat(IPlayer player, ChoiceVVehicle vehicle, byte oldSeatId, byte newSeatId) {
            onLeave(player, vehicle, newSeatId);
            onEnter(player, vehicle, newSeatId);
        }

        private void onLeave(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if (vehicle.hasData("TAXI_IS_TAXI")) {
                if (Convert.ToBoolean(vehicle.getData("TAXI_IS_TAXI"))) {
                    if (taxiDict.ContainsKey(vehicle.VehicleId)) {
                        taxiDict[vehicle.VehicleId].leaveTaxi(player);
                    }
                }
            }
        }

        //Setup a new Taxi in Controller
        public static void newTaxi(ChoiceVVehicle vehicle, int company = -1) {
            if (vehicle.DbModel.Seats > 1) {
                if (!vehicle.hasData("TAXI_IS_TAXI")) {
                    float price = 100f;
                    float distance = 0;
                    float deployCost = 1000f;
                    bool activeTask = false;
                    vehicle.setPermanentData("TAXI_IS_TAXI", true.ToString());
                    vehicle.setPermanentData("TAXI_COMPANY_PERMANENT", company.ToString());
                    vehicle.setPermanentData("TAXI_PRICE_PERMANENT", price.ToString());
                    vehicle.setPermanentData("TAXI_DISTANCE_PERMANENT", distance.ToString());
                    vehicle.setPermanentData("TAXI_DEPLOYCOST_PERMANENT", deployCost.ToString());
                    vehicle.setPermanentData("TAXI_ACTIVTASK_PERMANENT", activeTask.ToString());
                    vehicle.setPermanentData("TAXI_DEPLOY_ACTIV_PERMANENT", false.ToString());
                    vehicle.setPermanentData("TAXI_RATE_PERMANENT", false.ToString());
                    vehicle.setPermanentData("TAXI_COMPANY_CONTROLLED_PERMANENT", false.ToString());
                    Taxi newTaxi = new Taxi(vehicle, price, distance, deployCost, company);
                    if (!taxiDict.ContainsKey(newTaxi.Veh.VehicleId)) {
                        taxiDict.Add(newTaxi.Veh.VehicleId, newTaxi);
                    }
                    taxiDict[vehicle.VehicleId].enterTaxi(vehicle.Driver);
                    foreach (var player in vehicle.PassengerList) {
                        IPlayer temp = player.Value;
                        taxiDict[vehicle.VehicleId].enterTaxi(temp);
                    }
                }
            }
        }

        //Start a new Taxi-Task
        public static void startTaxiTask(IPlayer player, ChoiceVVehicle vehicle, bool withDeployCost = false) {
            if (taxiDict.ContainsKey(vehicle.VehicleId)) {
                taxiDict[vehicle.VehicleId].startTask(withDeployCost);
            }
        }

        //Stop Taxi-Task without payment
        public static void stopTaxiTask(IPlayer player, ChoiceVVehicle vehicle) {
            if (taxiDict.ContainsKey(vehicle.VehicleId)) {
                taxiDict[vehicle.VehicleId].stopTask();
            }
        }

        //Remove Taxi from System
        public static void removeTaxi(ChoiceVVehicle vehicle) {
            vehicle.resetPermantData("TAXI_IS_TAXI");
            vehicle.resetPermantData("TAXI_COMPANY_PERMANENT");
            vehicle.resetPermantData("TAXI_PRICE_PERMANENT");
            vehicle.resetPermantData("TAXI_DISTANCE_PERMANENT");
            vehicle.resetPermantData("TAXI_DEPLOYCOST_PERMANENT");
            vehicle.resetPermantData("TAXI_ACTIVTASK_PERMANENT");
            vehicle.resetPermantData("TAXI_DEPLOY_ACTIV_PERMANENT");
            vehicle.resetPermantData("TAXI_RATE_PERMANENT");
            vehicle.resetPermantData("TAXI_COMPANY_CONTROLLED_PERMANENT");
            taxiDict[vehicle.VehicleId].leaveTaxi(vehicle.Driver);
            foreach (var player in vehicle.PassengerList) {
                IPlayer temp = player.Value;
                taxiDict[vehicle.VehicleId].leaveTaxi(temp);
            }
            taxiDict.Remove(vehicle.VehicleId);
        }
        //switch for a taxi to get it company controlled or not. Could be easy when using as toggle, but shall be used like it is designed.
        public void setCompanyControlled(ChoiceVVehicle vehicle, bool companyControlled = false) {
            if (!companyControlled) {
                taxiDict[vehicle.VehicleId].CompanyControlled = false;
            } else {
                taxiDict[vehicle.VehicleId].CompanyControlled = true;
            }
        }

        //Method to trigger npc economy and ghost payment when validly payed a taxi task
        public void validatedPayment() {
            //dummie method for npc economy and ghost payment
        }
    }

    internal class Taxi {
        public ChoiceVVehicle Veh { get; set; }
        public int Company { get; set; }
        public bool CompanyControlled { get; set; }
        public float PricePer100 { get; set; }
        public float Distance { get; set; }
        public float DeployCost { get; set; }
        public bool Active { get; set; }
        public bool DeployActive { get; set; }
        public IInvoke ActiveInvoke { get; set; }
        public Dictionary<uint, IInvoke> PLayerInvokes { get; set; }

        public Taxi() {
            PLayerInvokes = new Dictionary<uint, IInvoke>();

        }

        //Const for Taxi
        public Taxi(ChoiceVVehicle vehicle, float pricePer100, float distance, float deployCost, int company = -1, bool active = false, bool deployActive = false, bool companyControlled = false) {
            PLayerInvokes = new Dictionary<uint, IInvoke>();
            Veh = vehicle;
            Company = company;
            PricePer100 = pricePer100;
            Distance = distance;
            DeployCost = deployCost;
            Active = active;
            saveTaxi(Veh);
            DeployActive = deployActive;
            CompanyControlled = companyControlled;
        }

        //Abs price for actual taxitask
        public float getPrice() {
            float price = Distance * PricePer100;
            if (DeployActive) {
                price += DeployCost;
            }
            return price;
        }

        //Start a taxi task
        //Param: Start Task with additional cost
        public void startTask(bool setDeployment = false) {
            Active = true;
            Distance = 0;
            if (setDeployment) {
                setDeployActive();
            }
            foreach (var player in Veh.PassengerList) {
                IPlayer temp = player.Value;
                leaveTaxi(temp);
                enterTaxi(temp);
            }
            registerSaveInvoke(Veh);
        }

        //Update task in Taxi
        public void updateTask(float distance) {
            this.Distance += distance / 100;
        }

        //Activate additional costs
        public void setDeployActive() {
            this.DeployActive = true;
        }

        //Deactivade additional costs
        public void setDeployInActive() {
            this.DeployActive = false;
        }

        //Toggle for wether Taxi is only controlled by company or by driver
        public void toggleCompnayControlled() {
            this.CompanyControlled = !this.CompanyControlled;
        }

        //Stop TaxiTask 
        public void stopTask() {
            Active = false;
            Distance = 0;
            setDeployInActive();
            //reload new state of Taxi
            foreach (var player in Veh.PassengerList) {
                IPlayer temp = player.Value;
                leaveTaxi(temp);
                enterTaxi(temp);
            }
            saveTaxi(Veh);
            removeSavedInvoke();
        }

        //load client taxometer when entering in Taxi
        public void enterTaxi(IPlayer player) {
            float rate = Distance * PricePer100 / 100;
            if (DeployActive) { rate += DeployCost; }
            TaxiData data = new TaxiData(PricePer100, rate, Distance);
            player.emitCefEventNoBlock(data);
            registerPLayerTaxiUpdate(player, (ChoiceVVehicle)player.Vehicle);
        }

        //update clients taxometer
        public void updateTaxi(IPlayer player) {
            if (player.Exists()) {
                TaxiUpdate data = new TaxiUpdate(Distance);
                var temp = Distance;
                player.emitCefEventNoBlock(data);
            } else {
                removePlayerTaxiUpdate(player);
            }
        }

        //remove taxometer in client
        public void leaveTaxi(IPlayer player) {
            TaxiStop data = new TaxiStop();
            player.emitCefEventNoBlock(data);
            removePlayerTaxiUpdate(player);

        }

        private void registerSaveInvoke(ChoiceVVehicle Veh) {
            ActiveInvoke = InvokeController.AddTimedInvoke(Veh.Id.ToString() + "_taxijob", (i) => {
                saveTaxi(Veh);
            }, TimeSpan.FromSeconds(3), true);
        }

        private void registerPLayerTaxiUpdate(IPlayer player, ChoiceVVehicle veh) {
            IInvoke tempInvoke = InvokeController.AddTimedInvoke(Veh.Id.ToString() + "_taxiuser_" + player.Id, (i) => {
                updateTaxi(player);
            }, TimeSpan.FromSeconds(1), true); //changed for debug reasons
            if (PLayerInvokes.ContainsKey(player.Id)) {
                PLayerInvokes[player.Id] = tempInvoke;
            } else {
                PLayerInvokes.Add(player.Id, tempInvoke);
            }

        }

        private void removePlayerTaxiUpdate(IPlayer player) {
            if (PLayerInvokes.ContainsKey(player.Id)) {
                PLayerInvokes[player.Id].EndSchedule();
                PLayerInvokes.Remove(player.Id);
            }
        }

        private void removeSavedInvoke() {
            ActiveInvoke.EndSchedule();
            ActiveInvoke = null;
        }
        private void saveTaxi(ChoiceVVehicle Veh) {
            Veh.setPermanentData("TAXI_COMPANY_PERMANENT", Company.ToString());
            Veh.setPermanentData("TAXI_PRICE_PERMANENT", PricePer100.ToString());
            Veh.setPermanentData("TAXI_DISTANCE_PERMANENT", Distance.ToString());
            Veh.setPermanentData("TAXI_DEPLOYCOST_PERMANENT", DeployCost.ToString());
            Veh.setPermanentData("TAXI_ACTIVTASK_PERMANENT", Active.ToString());
            Veh.setPermanentData("TAXI_DEPLOY_ACTIV_PERMANENT", DeployActive.ToString());
            Veh.setPermanentData("TAXI_RATE_PERMANENT", DeployActive.ToString());
            Veh.setPermanentData("TAXI_COMPANY_CONTROLLED_PERMANENT", DeployActive.ToString());
        }

    }

    //CefEvent for init Taxometer Data in Client
    public class TaxiData : IPlayerCefEvent {
        public string Event => "START_TAXOMETER";
        public float price;
        public float rate;
        public float distance;

        public TaxiData() { }
        public TaxiData(float price, float rate, float distance) {
            this.price = price;
            this.rate = rate;
            this.distance = distance;
        }

    }

    //CefEvent for updating Taxometer Data in Client
    public class TaxiUpdate : IPlayerCefEvent {
        public string Event => "UPDATE_TAXOMETER";
        public float distance;

        public TaxiUpdate() { }
        public TaxiUpdate(float distance) { this.distance = distance; }
    }

    //CefEvent for stopping Taxometer in Client
    public class TaxiStop : IPlayerCefEvent {
        public string Event => "STOP_TAXOMETER";

        public TaxiStop() { }
    }
}
