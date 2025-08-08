using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Companies {
    public class CompanyCityAdministrationController : ChoiceVScript {
        public CompanyCityAdministrationController() {
            EventController.addMenuEvent("ON_VEHICLE_UNREGISTER", onVehicleUnregister);
        }

        private bool onVehicleUnregister(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
           var module = (CompanyCityAdministrationFunctionality)data["Module"];

           module.onDMVInteract(null, player, "ON_VEHICLE_UNREGISTER", data, menuItemCefEvent);

           return true;
        }
    }

    public class CompanyCityAdministrationFunctionality : CompanyFunctionality {
        public static decimal VEHICLE_REGISTER_COST = 150;
        public static decimal LICENSE_CARD_CREATE_COST = 75;

        private ChoiceVPed VehicleRegistrationPed;
        private ChoiceVPed LicensePed;

        public CompanyCityAdministrationFunctionality() : base() { }

        public CompanyCityAdministrationFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "CITY_ADMIN_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Stadtverwaltung", "Füge die Funktionalität der Stadtverwaltung hinzu. Dies umfasst z.B. Autoanmeldung");
        }

        public override void onLoad() {
            Company.registerCompanyAdminElement(
               "CITY_ADMINISTRATION_PED_CREATION",
               getPedCreatorGenerator,
               onPedCreator
           );

            Company.registerCompanyInteractElement(
               "CITY_ADMINISTRATION_DMV_MENU",
               getDMVInteractionGenerator,
               onDMVInteract
           );

            Company.registerCompanyInteractElement(
               "CITY_ADMINISTRATION_LICENSE_MENU",
               getLicenseInteractionGenerator,
               onLicenseInteract
            );

            Company.registerCompanyInteractElement(
               "CITY_ADMINISTRATION_MINIJOB_MENU",
               getMinijobMenuGenerator,
               onMinijobCallback
            );
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("CITY_ADMINISTRATION_PED_CREATION");
            Company.unregisterCompanyElement("CITY_ADMINISTRATION_DMV_MENU");
            Company.unregisterCompanyElement("CITY_ADMINISTRATION_LICENSE_MENU");
        }

        #region Admin Stuff

        #region Peds

        private MenuElement getPedCreatorGenerator(IPlayer player) {
            var menu = new Menu("Peds setzen", "Setze die verschiedenen Peds");
            menu.addMenuItem(new ClickMenuItem("Fahrzeugan/ummeldung setzen", "Setze den DMV Ped. Nimmt die aktuelle Position und Rotation!", "", "DMV"));
            menu.addMenuItem(new ClickMenuItem("Lizenzverwaltung setzen", "Setze den Lizenzverwaltungs Ped. Nimmt die aktuelle Position und Rotation!", "", "LICENSE"));
            return menu;
        }

        private void onPedCreator(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            switch(subEvent) {
                case "DMV":
                    Company.setSetting("DMV_POSITION", new Position(player.Position.X, player.Position.Y, player.Position.Z - 1f).ToJson());
                    Company.setSetting("DMV_ROTATION", player.Rotation.Yaw.ToJson());
                    break;
                case "LICENSE":
                    Company.setSetting("LICENSE_POSITION", new Position(player.Position.X, player.Position.Y, player.Position.Z - 1f).ToJson());
                    Company.setSetting("LICENSE_ROTATION", player.Rotation.Yaw.ToJson());
                    break;
            }
        }

        #endregion

        #endregion

        private MenuElement getDMVInteractionGenerator(IPlayer player, IPlayer target) {
            return getDMVMenu(player, target, player == null);
        }

        private MenuElement getLicenseInteractionGenerator(IPlayer player, IPlayer target) {
            return getLicenseMenu(player, target, player == null);
        }
        
        private MenuElement getMinijobMenuGenerator(IPlayer player, IPlayer target) {
            return MiniJobController.getMinijobMenu(MiniJobController.MiniJobTypes.CityAdmin, target);
        }

        public void onDMVInteract(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "ON_REGISTER_VEHICLE") {
                var item = (VehicleRegistrationCard)data["Item"];
                var target = (IPlayer)data["Target"];
                var fromPed = (bool)data["FromNPC"];
                var hasNumberPlate = (bool)data["HasNumberPlate"];
                var registerCompany = data.ContainsKey("Company") ? (Company)data["Company"] : null;
                
                if(!fromPed || target.removeCash(VEHICLE_REGISTER_COST)) {
                    using(var db = new ChoiceVDb()) {
                        var numberPlate = "";
                        if(!fromPed) {
                            if(hasNumberPlate) {
                                var evt = data["PreviousCefEvent"] as InputMenuItemEvent;
                                if(evt.input != "" && evt.input != null) {
                                    if(db.vehiclesregistrations.FirstOrDefault(v => v.end == null && v.numberPlate == numberPlate) != null) {
                                        player.sendBlockNotification("Das Kennzeichen ist bereits vergeben!", "", Constants.NotifactionImages.Car);
                                        return;
                                    } else {
                                        numberPlate = evt.input;
                                    }
                                }
                            } else {
                                numberPlate = "Nicht verfügbar";
                            }
                        }

                        if(numberPlate == "") {
                            numberPlate = ChoiceVAPI.randomString(6);
                            while(db.vehiclesregistrations.FirstOrDefault(v => v.end == null && v.numberPlate == numberPlate) != null) {
                                numberPlate = ChoiceVAPI.randomString(6);
                            }
                        }

                        var added = false;
                        if(hasNumberPlate) {
                            var cfg = InventoryController.getConfigItemForType<NumberPlate>();
                            var numberPlateItem = new NumberPlate(cfg, numberPlate);
                            added = player.getInventory().addItem(numberPlateItem);
                        }

                        if(!hasNumberPlate || added) {
                            if(item != null) {
                                var issuerName = "";
                                if(player != target) {
                                    var rand = new Random();
                                    issuerName = DriversLicense.FIRST_NAME_LIST[rand.Next(0, DriversLicense.FIRST_NAME_LIST.Count - 1)] + " " + DriversLicense.LAST_NAME_LIST[rand.Next(0, DriversLicense.LAST_NAME_LIST.Count - 1)];
                                } else {
                                    issuerName = player.getCharacterShortName();
                                }

                                var already = db.vehiclesregistrations.FirstOrDefault(v => v.vehicleId == item.VehicleId && v.end == null);
                                if(already != null) {
                                    already.end = DateTime.Now;
                                }

                                var newHis = new vehiclesregistration {
                                    vehicleId = item.VehicleId,
                                    start = DateTime.Now,
                                    end = null,
                                    numberPlate = numberPlate,
                                    ownerId = registerCompany == null ? target.getCharacterId() : null,
                                    companyOwnerId = registerCompany?.Id,
                                };
                                db.vehiclesregistrations.Add(newHis);

                                db.SaveChanges();

                                if(registerCompany != null) {
                                    item.changeInfos(VehicleOwnerType.Company, registerCompany.Id, registerCompany.ShortName, numberPlate, issuerName);
                                } else {
                                    item.changeInfos(VehicleOwnerType.Player, target.getCharacterId(), target.getCharacterShortName(), numberPlate, issuerName);
                                }

                                if(hasNumberPlate) {
                                    player.sendNotification(Constants.NotifactionTypes.Success, "Du hast das Fahrzeug angemeldet und ein Nummernschild erhalten", "Fahrzeug angemeldet", Constants.NotifactionImages.Car);
                                } else {
                                    player.sendNotification(Constants.NotifactionTypes.Success, "Du hast das Fahrzeug angemeldet. Da das Modell unterstützt jedoch kein Nummernkennzeichen!", "Fahrzeug angemeldet", Constants.NotifactionImages.Car);
                                }
                            }
                        } else {
                            player.sendBlockNotification("Du hast kein Platz für ein Nummernkennzeichen im Inventar", "Kein Platz", Constants.NotifactionImages.Car);
                        }
                    }
                } else {
                    player.sendBlockNotification($"Er hast nicht genug Bargeld dabei. Du brauchst ${VEHICLE_REGISTER_COST}", "Nicht genug Geld", Constants.NotifactionImages.Car);
                }
            } else {
                var item = (VehicleRegistrationCard)data["Item"];
                var target = (IPlayer)data["Target"];
                var fromPed = (bool)data["FromNPC"];
                var submit = (bool)data["Submit"];

                using(var db = new ChoiceVDb()) {
                    var info = item.getRegistrationInfo();
                    var nmbPlate = target.getInventory().getItem<NumberPlate>(n => n.NumberPlateContent == info.NumberplateText);

                    if(nmbPlate == null && !submit) {
                        data["Submit"] = true;
                        data["Module"] = this;
                        var menu = MenuController.getConfirmationMenu("Kein Nummernkennzeichen dabei!", "Ohne Nummernkennzeichen abmelden?", "ON_VEHICLE_UNREGISTER", data);
                        player.showMenu(menu);
                        return;
                    } else {
                        if(nmbPlate != null) {
                            player.getInventory().removeItem(nmbPlate);
                        }
                    }

                    var already = db.vehiclesregistrations.FirstOrDefault(v => v.vehicleId == item.VehicleId && v.end == null);
                    if(already != null) {
                        already.end = DateTime.Now;
                    }

                    var onServerVehicle = ChoiceVAPI.GetVehicles(v => v.Id == item.VehicleId);
                    if(onServerVehicle.Count() != 0) {
                        var veh = onServerVehicle.First();
                        veh.RegisteredCompany = null;
                    } else {
                        var veh = db.vehicles.FirstOrDefault(v => v.id == item.VehicleId);
                        if(veh != null) {
                            veh.registeredCompanyId = null;
                        }
                    }

                    item.changeInfos(VehicleOwnerType.Player, target.getCharacterId(), "", " ", player.getCharacterShortName());

                    db.SaveChanges();
                    player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast das Fahrzeug erfolgreich abgemeldet.", "Fahrzeug abgemeldet", Constants.NotifactionImages.Car);
                }
            }
        }

        private void onLicenseInteract(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["Target"];
            var fromPed = (bool)data["FromNPC"];

            if (subEvent == "ON_CREATE_SSN_CARD") {
                if (!fromPed || target.removeCash(LICENSE_CARD_CREATE_COST)) {
                    player.addCash(LICENSE_CARD_CREATE_COST);
                    var ssn = new SocialSecurityCard(InventoryController.getConfigItemForType<SocialSecurityCard>(), target);
                    player.getInventory().addItem(ssn, true);
                    player.sendNotification(Constants.NotifactionTypes.Success, "Du hast dir eine neue Social Security Karte ausstellen lassen!", "Neue Karte ausgestellt");
                } else {
                    player.sendBlockNotification("Nicht genug Bargeld dabei!", "Nicht genug Geld");
                }
            } else {
                if(!target.removeCash(LICENSE_CARD_CREATE_COST)) {
                    player.sendBlockNotification("Nicht genug Bargeld dabei!", "Nicht genug Geld");
                    return;
                }
                player.addCash(LICENSE_CARD_CREATE_COST);

                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
                var typeEvt = evt.elements[0].FromJson<ListMenuItemEvent>();

                var type = (DriverLicenseClasses)Enum.Parse(typeof(DriverLicenseClasses), typeEvt.currentElement);

                var license = new DriversLicense(InventoryController.getConfigItemForType<DriversLicense>(), target, type);
                player.getInventory().addItem(license, true);

                player.sendNotification(Constants.NotifactionTypes.Success, "Du hast dir einen neuen Führerschein ausstellen lassen!", "Neue Karte ausgestellt");
            }
        }

        private MenuItem onDMVPedInteract(IPlayer player) {
            return Company.getCompanyInteractMenuElement(player, "CITY_ADMINISTRATION_DMV_MENU");
        }

        private MenuItem onLicensePedInteract(IPlayer player) {
            var menu = Company.getCompanyMenuElementByIdentifier("CITY_ADMINISTRATION_LICENSE_MENU", false, player, player) as Menu;

            return new MenuMenuItem(menu.Name, menu);
        }

        public override void onLastEmployeeLeaveDuty() {
            base.onLastEmployeeLeaveDuty();

            VehicleRegistrationPed = PedController.createPed("DMV-Mitarbeiter", "ig_andreas", Company.getSetting<Position>("DMV_POSITION"), ChoiceVAPI.radiansToDegrees(Company.getSetting<float>("DMV_ROTATION")));
            VehicleRegistrationPed.addModule(new NPCMenuItemsModule(VehicleRegistrationPed, new List<PlayerMenuItemGenerator> { onDMVPedInteract }));

            LicensePed = PedController.createPed("Stadtverwaltungsmitarbeiter", "ig_bankman", Company.getSetting<Position>("LICENSE_POSITION"), ChoiceVAPI.radiansToDegrees(Company.getSetting<float>("LICENSE_ROTATION")));
            LicensePed.addModule(new NPCMenuItemsModule(LicensePed, new List<PlayerMenuItemGenerator> { onLicensePedInteract }));
        }

        public override void onFirstEmployeeEnterDuty() {
            if(VehicleRegistrationPed != null) {
                PedController.destroyPed(VehicleRegistrationPed);
                VehicleRegistrationPed = null;
            }

            if(LicensePed != null) {
                PedController.destroyPed(LicensePed);
                LicensePed = null;
            }
        }

        public Menu getDMVMenu(IPlayer player, IPlayer target, bool fromNPC) {
            var menu = new Menu("Fahrzeugverwaltung", "Melde ein Fahrzeug an und um");

            var items = target.getInventory().getItems<VehicleRegistrationCard>(i => true);
            if(items.Count() != 0) {
                foreach(var item in items) {
                    var info = item.getRegistrationInfo();

                    var subMenu = new Menu($"{info.VehicleName}", $"Chassisnummer: {info.ChassisNumber}");
                    var subData = new Dictionary<string, dynamic> {
                        {"Item", item },
                        {"Target", target },
                        {"FromNPC", fromNPC },
                        {"Submit", false },
                        {"HasNumberPlate", info.hasNumberPlate }
                    };

                    if(info.NumberplateText == " ") {
                        if(fromNPC) {
                            if(target.getCash() >= VEHICLE_REGISTER_COST) {
                                subMenu.addMenuItem(new ClickMenuItem("Fahrzeug anmelden", "Melde das Fahrzeug an. Möglicherweise fallen dadurch Steuern an!", $"Kosten: ${VEHICLE_REGISTER_COST}", "ON_REGISTER_VEHICLE").withData(subData).needsConfirmation("Fahrzeug anmelden?", "Fahrzeug wirklich anmelden?"));
                            } else {
                                subMenu.addMenuItem(new StaticMenuItem("Fahrzeug anmelden", "Melde das Fahrzeug an. Möglicherweise fallen dadurch Steuern an!", "Nicht genug Geld!", MenuItemStyle.red));
                            }
                        } else {
                            if(info.hasNumberPlate) {
                                subMenu.addMenuItem(new InputMenuItem("Auf Person anmelden", "Melde das Fahrzeug an. Lasse das Eingabefeld leer um ein Nummernkennzeichen zu erzeugen", "", "ON_REGISTER_VEHICLE").withData(subData).needsConfirmation("Fahrzeug anmelden?", "Fahrzeug wirklich anmelden?"));
                            } else {
                                subMenu.addMenuItem(new ClickMenuItem("Auf Person anmelden", "Melde das Fahrzeug an. Das Fahrzeugmodell unterstützt kein Nummernkennzeichen", "Ohne Kennzeichen", "ON_REGISTER_VEHICLE").withData(subData).needsConfirmation("Fahrzeug anmelden?", "Fahrzeug wirklich anmelden?"));
                            }

                            var companies = CompanyController.getCompaniesWithPermission(player, "COMPANY_VEHICLE_CONTROL");

                            foreach(var company in companies) {
                                if(info.hasNumberPlate) {
                                    subMenu.addMenuItem(new InputMenuItem($"Auf {company.ShortName} anmelden", "Melde das Fahrzeug an. Lasse das Eingabefeld leer um ein Nummernkennzeichen zu erzeugen", "", "ON_REGISTER_VEHICLE")
                                        .withData(subData.Concat(new Dictionary<string, dynamic> { { "Company", company } }).ToDictionary(k => k.Key, k => k.Value))
                                        .needsConfirmation("Fahrzeug anmelden?", "Fahrzeug wirklich anmelden?"));
                                } else {
                                    subMenu.addMenuItem(new ClickMenuItem($"Auf {company.ShortName} anmelden", "Melde das Fahrzeug an. Das Fahrzeugmodell unterstützt kein Nummernkennzeichen", "Ohne Kennzeichen", "ON_REGISTER_VEHICLE")
                                        .withData(subData.Concat(new Dictionary<string, dynamic> { { "Company", company } }).ToDictionary(k => k.Key, k => k.Value))
                                        .needsConfirmation("Fahrzeug anmelden?", "Fahrzeug wirklich anmelden?"));
                                }
                            }
                        }
                    } else {
                        subMenu.addMenuItem(new ClickMenuItem("Fahrzeug abmelden", "Melde das Fahrzeug ab. Da durch fallen bestehende Steuern weg. Habe wenn möglich das Nummernkennzeichen dabei!", "", "ON_VEHICLE_UNREGISTER")
                            .withData(subData)
                            .needsConfirmation("Fahrzeug abmelden?", "Fahrzeug wirklich anmelden?"));
                    }

                    menu.addMenuItem(new MenuMenuItem($"{info.VehicleName}: {info.NumberplateText}", subMenu));
                }
            } else {
                menu.addMenuItem(new StaticMenuItem("Keine Fahrzeuginhaberkarte", "Du benötigst eine Fahrzeuginhaberkarte um Fahrzeuge zu verwalten. Sollte dir diese abhanden gekommen sein, gehen sie bitte zur Polizei!", "", MenuItemStyle.red));
            }

            return menu;
        }

        public Menu getLicenseMenu(IPlayer player, IPlayer target, bool fromNPC) {
            var menu = new Menu("Lizenzverwaltung", "Verwalte alle deine Identifikationskarten");


            var data = new Dictionary<string, dynamic> {
                {"Target", target },
                {"FromNPC", fromNPC },
            };

            if(target.hasSocialSecurityNumber()) {
                if(fromNPC) {
                    menu.addMenuItem(new ClickMenuItem("SSN-Karte anfertigen lassen", "Lasse dir eine Social Security Karte anfertigen", $"${LICENSE_CARD_CREATE_COST}", "ON_CREATE_SSN_CARD")
                        .withData(data)
                        .needsConfirmation("Ersatzkarte anfertigen lassen?", "Karte wirklich anfertigen lassen?"));
                } else {
                    menu.addMenuItem(new ClickMenuItem("SSN-Karte anfertigen lassen", "Lasse dir eine Social Security Karte anfertigen", "", "ON_CREATE_SSN_CARD")
                        .withData(data)
                        .needsConfirmation("Ersatzkarte anfertigen lassen?", "Karte wirklich anfertigen lassen?"));
                }
            } else {
                menu.addMenuItem(new StaticMenuItem("Person nicht registriert!", "Die Person hat keine Sozialversicherungsnummer!", "", MenuItemStyle.red));
            }

            if(!fromNPC) {
                var driverMenu = new Menu("Führerschein anfertigen", "Fertige dir einen Führerschein an");

                var typesList = Enum.GetValues(typeof(DriverLicenseClasses)).Cast<DriverLicenseClasses>().ToList();
                driverMenu.addMenuItem(new ListMenuItem("Führerscheinklasse", "Wähle die Klasse deines Führerscheins", typesList.Select(t => t.ToString()).ToArray(), "ON_SELECT_LICENSE_CLASS").withData(data));
                driverMenu.addMenuItem(new MenuStatsMenuItem("Führerschein anfertigen", "Fertige dir einen Führerschein an", "ON_CREATE_DRIVER_CARD", MenuItemStyle.green)
                    .withData(data)
                    .needsConfirmation("Führerschein anfertigen?", "Führerschein wirklich anfertigen lassen?"));
                
                menu.addMenuItem(new MenuMenuItem("Führerschein anfertigen", driverMenu));

            }

            return menu;
        }

        private void onMinijobCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = data["Player"] as IPlayer;
            MiniJobController.onMinijobTakeJob(target, subEvent, -1, data, menuItemCefEvent);
        }
    }
}
