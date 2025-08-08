using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.Money;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.GasRefuel;
using static ChoiceVServer.Controller.GasstationController;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public delegate void OnGasstationSpotInteract(IPlayer player, GasstationSpot spot);

    public class GasstationController : ChoiceVScript {
        public class GasstationRemainCash {
            public int CharId;
            public decimal RemainingCash;
            public DateTime Deadline;

            public bool Exists;

            public GasstationRemainCash(int charId, decimal remainCash, DateTime deadline) {
                CharId = charId;
                RemainingCash = remainCash;
                Deadline = deadline;

                Exists = true;
            }
        }

        public class Gasstation {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public long BankAccount { get; private set; }
            public List<GasstationSpot> GasstationSpots { get; private set; }
            public GasStationType GasStationType { get; private set; }
            public float PetrolPrice { get; private set; }
            public float DieselPrice { get; private set; }
            public float ElectricityPrice { get; private set; }
            public float KerosinPrice { get; private set; }
            public List<GasstationRemainCash> GasstationRemainCashList { get; private set; }
            public CollisionShape GasstationRemainCashPay { get; private set; }

            public Gasstation(int id, string name, long bankAccount, List<GasstationSpot> allSpots, GasStationType type, float petrolPrice, float dieselPrice, float electricityPrice, float kerosinPrice, Position position, float width, float height, float rotation) {
                Id = id;
                Name = name;
                BankAccount = bankAccount;
                GasstationSpots = allSpots;
                GasStationType = type;
                PetrolPrice = petrolPrice;
                DieselPrice = dieselPrice;
                ElectricityPrice = electricityPrice;
                KerosinPrice = kerosinPrice;

                GasstationRemainCashList = new List<GasstationRemainCash>();

                GasstationRemainCashPay = CollisionShape.Create(position, width, height, rotation, true, false, true);

                GasstationRemainCashPay.OnCollisionShapeInteraction += onRemainCashInteraction;
            }

            private bool onRemainCashInteraction(IPlayer player) {
                var entry = GasstationRemainCashList.FirstOrDefault(g => g.CharId == player.getCharacterId());
                if(player.getCharacterData().AdminMode && player.getAdminLevel() >= 3) {
                    var menu = new Menu("Tankstellen Support-Menü", "Was möchtest du tun?");
                    var data = new Dictionary<string, dynamic> {
                            { "Gasstation", this }
                    };

                    menu.addMenuItem(new StaticMenuItem("TankstellenId", $"Die Id der Tankstelle ist {Id}", $"Id: {Id}"));
                    menu.addMenuItem(new ClickMenuItem("Tankstelle entfernen", $"Entferne diese Tankstelle und alle Zapfsäulen", $"", "SUPPORT_REMOVE_GASSTATION", MenuItemStyle.red).withData(data).needsConfirmation("Wirklich entfernen?", "Tankstelle wirklich entfernen?"));
                    player.showMenu(menu);
                    return true;
                }

                if(entry != null) {
                    var menu = new Menu("Tankschuld bezahlen", "Bezahlen den übrigen Geldbetrag");

                    if(player.getCash() >= entry.RemainingCash) {
                        var data = new Dictionary<string, dynamic> {
                            { "Entry", entry },
                            { "Station", this },
                        };
                        menu.addMenuItem(new ClickMenuItem("Schuld begleichen", $"Die wird dich ${entry.RemainingCash} kosten", $"${entry.RemainingCash} bezahlen", "PAY_GASSTATION_REMAINING_CASH", MenuItemStyle.green).withData(data).needsConfirmation("Wirklich bezahlen?", $"Wirklich ${entry.RemainingCash} bezahlen"));
                    } else {
                        menu.addMenuItem(new StaticMenuItem("Nicht genug Geld!", $"Du brauchst mind. ${entry.RemainingCash} um deine Schuld zu begleichen", $"${entry.RemainingCash} benötigt", MenuItemStyle.red));
                    }
                    player.showMenu(menu);
                } else {
                    player.sendNotification(NotifactionTypes.Danger, "Du musst keinen Restbetrag bezahlen!", "Kein Restbetrag", NotifactionImages.Gasstation);
                }
                
                return true;
            }

            public void onSpotInteraction(IPlayer player, GasstationSpot spot) {
                var vehicle = spot.CollisionShape.AllEntities.FirstOrDefault(e => e is ChoiceVVehicle veh && veh.LockState == AltV.Net.Enums.VehicleLockState.Unlocked && veh.VehicleClass != VehicleClassesDbIds.Cycles) as ChoiceVVehicle;
                var jerryCan = player.getInventory().getItem<JerryCan>(jc => jc.FillStage < 1);

                if(jerryCan != null) {
                    var menu = new Menu($"Kanister füllen", "Welchen Benzintyp benutzen?");
                    if(jerryCan.FuelCatagory == FuelType.None) {
                        foreach(var fuelType in spot.AllFuelTypes) {
                            if(fuelType == GasstationSpotType.CarDiesel || fuelType == GasstationSpotType.CarPetrol) {
                                var data = new Dictionary<string, dynamic> {
                                { "Gasstation", this},
                                { "Spot", spot },
                                { "SelectedType", fuelType },
                            };
                                menu.addMenuItem(new ClickMenuItem(GasstationSpotTypesToName[fuelType], $"Betanke dein/en Kanister mit {GasstationSpotTypesToName[fuelType]}", $"${getPriceForType(fuelType)}", "SELECT_GASSTATION_FUELTYPE_JERRYCAN").withData(data));
                            }
                        }
                    } else {
                        GasstationSpotType currentFuel = GasstationSpotType.CarPetrol;
                        if(jerryCan.FuelCatagory == FuelType.Petrol)
                            currentFuel = GasstationSpotType.CarPetrol;
                        else if(jerryCan.FuelCatagory == FuelType.Diesel)
                            currentFuel = GasstationSpotType.CarDiesel;
                        else
                            return;

                        foreach(var fuelType in spot.AllFuelTypes) {
                            if(fuelType == currentFuel) {
                                var data = new Dictionary<string, dynamic> {
                                { "Gasstation", this},
                                { "Spot", spot },
                                { "SelectedType", fuelType },
                            };
                                menu.addMenuItem(new ClickMenuItem(GasstationSpotTypesToName[fuelType], $"Betanke dein/en Kanister mit {GasstationSpotTypesToName[fuelType]}", $"${getPriceForType(fuelType)}", "SELECT_GASSTATION_FUELTYPE_JERRYCAN").withData(data));
                            }
                        }
                    }


                    player.showMenu(menu, true);
                    player.stopAnimation();
                    return;
                }

                if(vehicle != null) {
                    if(player.getCharacterData().AdminMode && player.getAdminLevel() >= 3) {
                        player.sendNotification(NotifactionTypes.Warning, "Für das Support Menü Fahrzeug entfernen", "Support Menü geblockt", Constants.NotifactionImages.Car);
                    }

                    if(!vehicle.EngineOn) {
                        if(vehicle.FuelType != FuelType.Electricity) {
                            var menu = new Menu($"{vehicle.DbModel.ModelName} tanken", "Welchen Kraftstofftyp benutzen?");
                            foreach(var fuelType in spot.AllFuelTypes) {
                                var data = new Dictionary<string, dynamic> {
                                    { "Gasstation", this},
                                    { "Spot", spot },
                                    { "SelectedType", fuelType },
                                    { "Vehicle", vehicle },
                                };
                                menu.addMenuItem(new ClickMenuItem(GasstationSpotTypesToName[fuelType], $"Betanke dein/en {vehicle.DbModel.ModelName} mit {GasstationSpotTypesToName[fuelType]}", $"${getPriceForType(fuelType)}", "SELECT_GASSTATION_FUELTYPE").withData(data));
                            }

                            player.showMenu(menu, true);
                        } else {
                            if(spot.AllFuelTypes.Contains(GasstationSpotType.CarElecticity)) {
                                var anim = AnimationController.getAnimationByName("WORK_FRONT");
                                AnimationController.animationTask(player, anim, () => {
                                    if(!vehicle.hasData("ELECTRO_CHARGING_START")) {
                                        var timeAmount = Math.Round(TimeSpan.FromSeconds((vehicle.FuelTankSize * (1 - vehicle.Fuel)) / FillAmountPerSecond).TotalMinutes);

                                        vehicle.setData("ELECTRO_CHARGING_START", DateTime.Now.ToJson());

                                        player.sendNotification(NotifactionTypes.Info, $"Du hast dein Auto angeschlossen. Die Batterie zu füllen dauert ca. {timeAmount} min", $"Füllzeit: ca. {timeAmount} min", NotifactionImages.Car);
                                    } else {
                                        var startTime = ((string)vehicle.getData("ELECTRO_CHARGING_START")).FromJson<DateTime>();
                                        var diff = (float)(DateTime.Now - startTime).TotalSeconds;
                                        var fuelAmount = Math.Min(diff * FillAmountPerSecond, vehicle.FuelTankSize * (1 - vehicle.Fuel));

                                        vehicle.Fuel += fuelAmount / vehicle.FuelTankSize;

                                        vehicle.resetData("ELECTRO_CHARGING_START");

                                        var percent = (int)(vehicle.Fuel * 100);
                                        player.sendNotification(NotifactionTypes.Info, $"Du hast dein Auto vom Strom genommen. Dein Ladestand beträgt {percent}%", $"Ladestand: {percent}%", NotifactionImages.Car);
                                    }
                                }, null, true, 1);
                            } else {
                                player.sendBlockNotification($"Dein Fahrzeug kann nur mit Strom aufgeladen werden! Dies ist hier nicht möglich. Suche nach einem Ladepunkt", $"Elektroauto", NotifactionImages.Car);
                            }
                        }

                    } else {
                        player.sendBlockNotification("Schalte für das Tanken den Motor aus!", "Motor ausschalten", Constants.NotifactionImages.Car);
                    }
                } else {
                    if(player.getCharacterData().AdminMode && player.getAdminLevel() >= 3) {
                        var menu = new Menu("Zapfäulen Support-Menü", "Was möchtest du tun?");
                        var data = new Dictionary<string, dynamic> {
                            { "Gasstation", this },
                            { "GasstationSpot", spot }
                        };

                        menu.addMenuItem(new ClickMenuItem("Zapfsäule entfernen", $"Entferne diese Zapfsäule aus der Tankstelle", $"", "SUPPORT_REMOVE_GASSTATION_SPOT", MenuItemStyle.red).withData(data).needsConfirmation("Wirklich entfernen?", "Zapfsäule wirklich entfernen?"));
                        player.showMenu(menu);
                    } else {
                        player.sendBlockNotification("Kein offenes Fahrzeug nahe der Säule gefunden!", "Kein Fahrzeug gefunden", Constants.NotifactionImages.Car);
                    }

                }
            }

            public void addRemainCash(IPlayer player, decimal remainCash) {
                var entry = GasstationRemainCashList.FirstOrDefault(g => g.CharId == player.getCharacterId());
                if(entry == null) {
                    GasstationRemainCashList.Add(new GasstationRemainCash(player.getCharacterId(), remainCash, DateTime.Now + TimeSpan.FromMinutes(15)));
                } else {
                    entry.RemainingCash += remainCash;
                }
            }

            public void removeRemainCash(GasstationRemainCash remain) {
                GasstationRemainCashList.Remove(remain);
            }

            public float getPriceForType(GasstationSpotType type) {
                switch(type) {
                    case GasstationSpotType.CarPetrol:
                        return PetrolPrice;
                    case GasstationSpotType.CarDiesel:
                        return DieselPrice;
                    case GasstationSpotType.CarElecticity:
                        return 0;
                    case GasstationSpotType.Boat:
                        return DieselPrice;
                    case GasstationSpotType.PlaneHelicopter:
                        return KerosinPrice;
                    default:
                        return 0;
                }
            }

            public void onRemove() {
                GasstationRemainCashPay.Dispose();

                foreach(var spot in GasstationSpots) {
                    spot.onRemove();
                }
            }
        }

        public class GasstationSpot {
            public int Id { get; private set; }
            public CollisionShape CollisionShape { get; private set; }
            public List<GasstationSpotType> AllFuelTypes { get; private set; }

            public GasstationSpot(int id, List<GasstationSpotType> types, Position position, float width, float height, float rotation) {
                Id = id;
                CollisionShape = CollisionShape.Create(position, width, height, rotation, true, true, true);
                CollisionShape.HasNoHeight = true;
                AllFuelTypes = types;

                CollisionShape.OnCollisionShapeInteraction += onInteraction;
                CollisionShape.OnEntityExitShape += onVehicleExit;
            }

            private bool onInteraction(IPlayer player) {
                OnGasstationSpotInteract.Invoke(player, this);

                return true;
            }

            private void onVehicleExit(CollisionShape shape, IEntity entity) {
                if(entity is ChoiceVVehicle) {
                    (entity as ChoiceVVehicle).resetData("ELECTRO_CHARGING_START");
                }
            }

            public void onRemove() {
                CollisionShape.Dispose();
            }
        }

        public static List<Gasstation> AllGasstations;
        private static OnGasstationSpotInteract OnGasstationSpotInteract;

        public GasstationController() {
            loadGasstations();
            OnGasstationSpotInteract += onGasstationSpotInteract;

            EventController.addMenuEvent("SELECT_GASSTATION_FUELTYPE", onGasstationSelectFueltype);
            EventController.addMenuEvent("SELECT_GASSTATION_FUELTYPE_JERRYCAN", onGasstationSelectFueltypeJerryCan);
            EventController.addMenuEvent("PAY_GASSTATION_WITH_COMPANY", onPayGasstationWithCompany);
            EventController.addMenuEvent("PAY_GASSTATION_REMAINING_CASH", onPayGasstationRemainingCash);

            InvokeController.AddTimedInvoke("RemainCashChecker", onCheckRemainCash, TimeSpan.FromMinutes(1), true);

            VehicleController.addSelfMenuElement(
                new ConditionalVehicleSelfMenuElement(
                    () => new ClickMenuItem("Nächste Ladestation anzeigen", "Markiere dir die nächste nächste Ladestation anzeigen", "", "SHOW_CLOSEST_ELECTRIC_CHARGING_STATION"),
                    v => v.FuelType == FuelType.Electricity,
                    p => p.IsInVehicle && p.Vehicle.Driver == p
                )
            );
            EventController.addMenuEvent("SHOW_CLOSEST_ELECTRIC_CHARGING_STATION", onShowClosestChargingStation);

            #region Support Menu Stuff

            var gasStationMenu = new Menu("Tankstelle erstellen", "Erstelle eine Tankstelle");
            gasStationMenu.addMenuItem(new InputMenuItem("Name", "Namen der Tankstelle (Ort falls möglich)", "", ""));
            gasStationMenu.addMenuItem(new InputMenuItem("Tankstellenmarke", "0: Keine, 1: Ltd, 2: Ron, 3: GlobeOil, 4: XeroGas", "", ""));
            gasStationMenu.addMenuItem(new InputMenuItem("Bezinpreis in $", "Gib Bezinpreis in $ ein.", "1 - 2", ""));
            gasStationMenu.addMenuItem(new InputMenuItem("Dieselpreis in $", "Gib Dieselpreis in $ ein.", "0.75 - 1.75", ""));
            gasStationMenu.addMenuItem(new InputMenuItem("Strompreis in $", "Gib Strompreis in $ ein.", "0.25 - 1", ""));
            gasStationMenu.addMenuItem(new InputMenuItem("Kerosinpreis in $", "Gib Kerosinpreis in $ ein.", "3 - 4", ""));
            gasStationMenu.addMenuItem(new MenuStatsMenuItem("Bestätigen", "Tankstelle erstellen", "SUPPORT_CREATE_GASSTATION", MenuItemStyle.green));

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => gasStationMenu,
                    3,
                    SupportMenuCategories.Fahrzeuge,
                    "Tankstelle erstellen"
                )
             );
            EventController.addMenuEvent("SUPPORT_CREATE_GASSTATION", onSupportCreateGasstation);

            var gasStationSpotMenu = new Menu("Zapfsäule erstellen", "Erstelle eine Zapfsäule");
            gasStationSpotMenu.addMenuItem(new InputMenuItem("TankstellenId", "Gehe an den Bezahlspot der Tankstelle um die Id zu sehen", "", ""));
            gasStationSpotMenu.addMenuItem(new StaticMenuItem("Kraftstoffarten", "Benzin: 0, Diesel: 1, Strom: 2, Boote: 3, Kerosin: 4", ""));
            gasStationSpotMenu.addMenuItem(new InputMenuItem("mögl. Kraftstoffarten", "Liste mit , Trennen", "z.b. 1,3,4", ""));
            gasStationSpotMenu.addMenuItem(new MenuStatsMenuItem("Bestätigen", "Zapfsäule erstellen", "SUPPORT_CREATE_GASSTATION_SPOT", MenuItemStyle.green));

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => gasStationSpotMenu,
                    3,
                    SupportMenuCategories.Fahrzeuge,
                    "Zapfsäule erstellen"
                )
             );

            EventController.addMenuEvent("SUPPORT_CREATE_GASSTATION_SPOT", onSupportCreateGasstationSpot);

            EventController.addMenuEvent("SUPPORT_REMOVE_GASSTATION", onSupportRemoveGasstation);
            EventController.addMenuEvent("SUPPORT_REMOVE_GASSTATION_SPOT", onSupportRemoveGasstationSpot);
            #endregion
        }

        private bool onShowClosestChargingStation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var elecStation = AllGasstations.Find(g => g.GasStationType == GasStationType.ElectricCharging);

            if(elecStation != null) {
                GasstationSpot closest = null;
                var dist = float.MaxValue;

                foreach(var spot in elecStation.GasstationSpots) {
                    var currDist = player.Position.Distance(spot.CollisionShape.Position);

                    if(currDist < dist) {
                        dist = currDist;
                        closest = spot;
                    }
                }

                if(closest != null) {
                    BlipController.destroyBlipByName(player, $"CHARGING_STATION_BLIP_{player.getCharacterId()}");
                    BlipController.createPointBlip(player, "Ladesäule", closest.CollisionShape.Position, 5, 354, 255, $"CHARGING_STATION_BLIP_{player.getCharacterId()}");
                    BlipController.setWaypoint(player, closest.CollisionShape.Position.X, closest.CollisionShape.Position.Y);

                    InvokeController.AddTimedInvoke("Charging-Station-Blip-Remover", (i) => {
                        BlipController.destroyBlipByName(player, $"CHARGING_STATION_BLIP_{player.getCharacterId()}");
                    }, TimeSpan.FromMinutes(5), false);

                    player.sendNotification(NotifactionTypes.Success, $"Die nächste Ladestation wurde auf der Karte markiert", "Ladesäule markiert", NotifactionImages.Gasstation);
                }
            }

            return true;
        }

        private bool onGasstationSelectFueltypeJerryCan(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var station = (Gasstation)data["Gasstation"];
            var spot = (GasstationSpot)data["Spot"];
            var fuelType = (GasstationSpotType)data["SelectedType"];
            var item = player.getInventory().getItem<JerryCan>(jc => jc.FillStage < 1);
            float freeWeight = player.getInventory().MaxWeight - player.getInventory().getCombinedWeight();
            float jerryCanMaxFuel = 10f;
            if(freeWeight < 10f) {
                jerryCanMaxFuel = freeWeight;
                if(item.FillStage != 0f) {
                    jerryCanMaxFuel = item.FillStage * 10 + freeWeight;
                    if(jerryCanMaxFuel > 10f) {
                        jerryCanMaxFuel = 10f;
                    }
                }
            }
            var showComp = CompanyController.getCompanies(player).Any(c => CompanyController.hasPlayerPermission(player, c, "REFUEL_CAR"));

            AnimationController.playItemAnimation(player, "JERRYCANREFILLING", null, false);
            GasRefuelController.showGasRefuel(player, new GasRefuel(station.GasStationType, item.FillStage * 10f, jerryCanMaxFuel, fuelType, station.getPriceForType(fuelType), true, true, showComp), (player, action, account, fuelAmount, refuelObj) => {
                if(action == "Finished" || action == "Stopped") {
                    var fT = GasstationSpotTypesToFuelType[fuelType];
                    var cost = Math.Round(Convert.ToDecimal(fuelAmount * refuelObj.FuelPrice), 2);

                    if(fuelType == GasstationSpotType.CarPetrol) {
                        item.FillStage += fuelAmount / 10f;
                        item.FuelCatagory = FuelType.Petrol;
                        item.updateDescription();
                    } else if(fuelType == GasstationSpotType.CarDiesel) {
                        item.FillStage += fuelAmount / 10f;
                        item.FuelCatagory = FuelType.Diesel;
                        item.updateDescription();
                    }

                    player.stopAnimation();

                    switch(account) {
                        case "cash":
                            if(player.getCash() >= cost) {
                                player.removeCash(cost);
                                player.sendNotification(NotifactionTypes.Success, $"Der Kanister wurde betankt und du hast ${cost} bar bezahlt", $"${cost} bezahlt", NotifactionImages.Gasstation);
                            } else {
                                player.sendNotification(NotifactionTypes.Danger, $"Du hast nicht genug Bargeld dabei! Du hast 15min die ${cost} bar zu bezahlen", $"${cost} zu bezahlen!  ", NotifactionImages.Gasstation);
                                station.addRemainCash(player, cost);
                            }
                            break;
                        case "account":
                            if(BankController.transferMoney(player.getMainBankAccount(), station.BankAccount, cost, $"Tankkosten {Math.Round(fuelAmount, 2)}l {refuelObj.FuelName} mit Kanister", out var returnMessage, false)) {
                                player.sendNotification(NotifactionTypes.Success, $"Der Kanister wurde betankt und du hast ${cost} über dein Konto bezahlt", $"${cost} bezahlt", NotifactionImages.Gasstation);
                            } else {
                                player.sendNotification(NotifactionTypes.Danger, returnMessage, $"${cost} zu bezahlen!  ", NotifactionImages.Gasstation);
                                station.addRemainCash(player, cost);
                            }
                            break;
                        case "company":
                            var menu = new Menu("Firmenauswahl", "Wähle die Firma aus");
                            foreach(var company in CompanyController.getCompanies(player)) {
                                if(CompanyController.hasPlayerPermission(player, company, "REFUEL_CAR")) {
                                    var data = new Dictionary<string, dynamic> {
                                        { "Cost", cost },
                                        { "Company", company },
                                        { "Station", station },
                                        { "Message", $"Tankkosten {Math.Round(fuelAmount, 2)}l {refuelObj.FuelName} von {player.getCharacterName()} mit Kanister" }
                                    };
                                    menu.addMenuItem(new ClickMenuItem(company.Name, $"Bezahle ${cost} mit dem Firmenkonto von: {company.Name}", "", "PAY_GASSTATION_WITH_COMPANY").withData(data).needsConfirmation("Wirklich bezahlen?", $"Wirklich mit {company.Name} Konto bezahlen?"));
                                }
                            }
                            player.showMenu(menu);
                            return;
                    }
                }
            });
            return true;
        }

        public static void loadGasstations() {
            if(AllGasstations != null) {
                foreach(var station in AllGasstations) {
                    station.onRemove();
                }
            }

            AllGasstations = new List<Gasstation>();

            var bankAccount = BankController.getControllerBankaccounts(typeof(GasstationController));
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.configgasstations.Include(g => g.configgasstationspots)) {
                    var spotList = new List<GasstationSpot>();
                    foreach(var spot in row.configgasstationspots) {
                        var gastationSpot = new GasstationSpot(spot.id, spot.fuelTypeList.FromJson<List<GasstationSpotType>>(), spot.position.FromJson(), spot.width, spot.height, spot.rotation);
                        spotList.Add(gastationSpot);
                    }

                    if(row.bankAccount == null || row.bankAccount == -1 || !bankAccount.Any(b => b.id == row.bankAccount)) {
                        row.bankAccount = BankController.createBankAccount(typeof(GasstationController), row.name, BankAccountType.CompanyKonto, 0, BankController.getBankByType(BankCompanies.LibertyBank), true).id;
                        Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"Gasstation {row.name} had no bank account. Created one with id {row.bankAccount}");
                    }

                    var gasstation = new Gasstation(row.id, row.name, row.bankAccount ?? -1, spotList, (GasStationType)row.type, row.pricePetrol, row.priceDiesel, row.priceElecricity, row.priceKerosin, row.remainPosition.FromJson(), row.width, row.height, row.rotation);

                    AllGasstations.Add(gasstation);
                }

                db.SaveChanges();
            }
        }

        private void onCheckRemainCash(IInvoke obj) {
            foreach(var station in AllGasstations) {
                foreach(var entry in station.GasstationRemainCashList.Reverse<GasstationRemainCash>()) {
                    if(entry.Deadline <= DateTime.Now) {
                        //TODO DISPATCH AN DIE POLIZEI

                        entry.Exists = false;
                        station.GasstationRemainCashList.Remove(entry);
                    }
                }
            }
        }

        private void onGasstationSpotInteract(IPlayer player, GasstationSpot spot) {
            var gasstation = AllGasstations.FirstOrDefault(g => g.GasstationSpots.Contains(spot));

            if(gasstation != null) {
                gasstation.onSpotInteraction(player, spot);
            }
        }

        private bool onGasstationSelectFueltype(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var station = (Gasstation)data["Gasstation"];
            var spot = (GasstationSpot)data["Spot"];
            var fuelType = (GasstationSpotType)data["SelectedType"];
            var vehicle = (ChoiceVVehicle)data["Vehicle"];

            var showComp = CompanyController.getCompanies(player).Any(c => CompanyController.hasPlayerPermission(player, c, "REFUEL_CAR"));
            GasRefuelController.showGasRefuel(player, new GasRefuel(station.GasStationType, vehicle.Fuel * vehicle.FuelTankSize, vehicle.FuelTankSize, fuelType, station.getPriceForType(fuelType), true, true, showComp), (player, action, account, fuelAmount, refuelObj) => {
                if(action == "Stopped" || action == "Finished") {
                    var fT = GasstationSpotTypesToFuelType[fuelType];
                    var fuel = fuelAmount / vehicle.FuelTankSize;
                    var cost = Math.Round(Convert.ToDecimal(fuelAmount * refuelObj.FuelPrice), 2);

                    if(fT != vehicle.FuelType) {
                        InvokeController.AddTimedInvoke("WrongEngineFuel", (i) => {
                            VehicleMotorCompartmentController.applyMotorDamage(vehicle, 1);
                            if(player.Vehicle == vehicle) {
                                player.sendBlockNotification("Der Motor fängt an zu stottern und das Fahrzeug geht aus!", "Fahrzeug kaputt", NotifactionImages.Car);
                            }
                        }, TimeSpan.FromSeconds(25), false);
                    }

                    switch(account) {
                        case "cash":
                            if(player.getCash() >= cost) {
                                player.removeCash(cost);
                                player.sendNotification(NotifactionTypes.Success, $"Das Fahrzeug wurde betankt und du hast ${cost} bar bezahlt", $"${cost} bezahlt", NotifactionImages.Gasstation);
                            } else {
                                player.sendNotification(NotifactionTypes.Danger, $"Du hast nicht genug Bargeld dabei! Du hast 15min die ${cost} bar zu bezahlen", $"${cost} zu bezahlen!  ", NotifactionImages.Gasstation);
                                station.addRemainCash(player, cost);
                            }
                            break;
                        case "account":
                            if(BankController.transferMoney(player.getMainBankAccount(), station.BankAccount, cost, $"Tankkosten {Math.Round(fuelAmount, 2)}l {refuelObj.FuelName} mit {vehicle.NumberplateText}", out var returnMessage, false)) {
                                player.sendNotification(NotifactionTypes.Success, $"Das Fahrzeug wurde betankt und du hast ${cost} über dein Konto bezahlt", $"${cost} bezahlt", NotifactionImages.Gasstation);
                            } else {
                                player.sendNotification(NotifactionTypes.Danger, returnMessage, $"${cost} zu bezahlen!  ", NotifactionImages.Gasstation);
                                station.addRemainCash(player, cost);
                            }
                            break;
                        case "company":
                            var menu = new Menu("Firmenauswahl", "Wähle die Firma aus");
                            foreach(var company in CompanyController.getCompanies(player)) {
                                if(CompanyController.hasPlayerPermission(player, company, "REFUEL_CAR")) {
                                    var data = new Dictionary<string, dynamic> {
                                        { "Cost", cost },
                                        { "Company", company },
                                        { "Station", station },
                                        { "Fuel", fuel },
                                        { "Vehicle", vehicle },
                                        { "Message", $"Tankkosten {Math.Round(fuelAmount, 2)}l {refuelObj.FuelName} von {player.getCharacterName()} mit {vehicle.NumberplateText}" }
                                    };
                                    menu.addMenuItem(new ClickMenuItem(company.Name, $"Bezahle ${cost} mit dem Firmenkonto von: {company.Name}", "", "PAY_GASSTATION_WITH_COMPANY").withData(data).needsConfirmation("Wirklich bezahlen?", $"Wirklich mit {company.Name} Konto bezahlen?"));
                                }
                            }
                            player.showMenu(menu);
                            return;
                    }

                    vehicle.Fuel += fuel;

                }
            });

            return true;
        }

        private bool onPayGasstationWithCompany(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var company = (Company)data["Company"];
            var cost = (decimal)data["Cost"];
            var station = (Gasstation)data["Station"];
            var message = (string)data["Message"];
            var fuelAmount = (float)data["Fuel"];
            var vehicle = (ChoiceVVehicle)data["Vehicle"];

            if(BankController.transferMoney(company.CompanyBankAccount, station.BankAccount, cost, message, out var retMessage, false)) {
                player.sendNotification(NotifactionTypes.Success, $"Das Fahrzeug wurde betankt und du hast ${cost} über das Firmenkonto von {company.Name} bezahlt", $"${cost} bezahlt", NotifactionImages.Gasstation);
            } else {
                player.sendNotification(NotifactionTypes.Danger, retMessage, $"${cost} zu bezahlen!", NotifactionImages.Gasstation);
                station.addRemainCash(player, cost);
            }

            vehicle.Fuel += fuelAmount;

            return true;
        }

        private bool onPayGasstationRemainingCash(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var station = (Gasstation)data["Station"];
            var entry = (GasstationRemainCash)data["Entry"];

            if(entry.Exists) {
                if(player.removeCash(entry.RemainingCash)) {
                    player.sendNotification(NotifactionTypes.Success, $"Du hast deine Schuld von ${entry.RemainingCash} erfolgreich beglichen", "Schuld beglichen", NotifactionImages.Gasstation);
                    station.removeRemainCash(entry);
                } else {
                    player.ban("Betrugsversuch erkannt. Melde dich sofort im Support. SupportCode: WaterBear");
                }
            } else {
                player.sendBlockNotification("Du bist zu spät! Die Polizei wurde informiert", "Polizei informiert", NotifactionImages.Gasstation);
            }

            return true;
        }

        #region Support Menü Stuff

        private bool onSupportCreateGasstation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var typeEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var petrolEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var dieselEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
            var elecEvt = evt.elements[4].FromJson<InputMenuItemEvent>();
            var kerosinEvt = evt.elements[5].FromJson<InputMenuItemEvent>();

            try {
                var stationType = (GasStationType)int.Parse(typeEvt.input);
                var petrolPrice = float.Parse(petrolEvt.input);
                var dieselPrice = float.Parse(dieselEvt.input);
                var elecPrice = float.Parse(elecEvt.input);
                var kerosinPrice = float.Parse(kerosinEvt.input);

                GasstationCreator.createGasstation(player, nameEvt.input, stationType, petrolPrice, dieselPrice, elecPrice, kerosinPrice);
            } catch {
                player.sendBlockNotification("Eine Eingabe war nicht im richtigen Format!", "Eingabe falsch");
            }

            return true;
        }


        private bool onSupportRemoveGasstation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var station = (Gasstation)data["Gasstation"];

            using(var db = new ChoiceVDb()) {
                station.onRemove();
                var dbStation = db.configgasstations.FirstOrDefault(g => g.id == station.Id);
                if(dbStation != null) {
                    db.configgasstations.Remove(dbStation);
                }
                db.SaveChanges();
                player.sendNotification(NotifactionTypes.Warning, "Tankstelle erfolgreich entfernt!", "Tankstelle entfernt");
                loadGasstations();
            }

            return true;
        }

        private bool onSupportCreateGasstationSpot(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var gasstationEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var fuelTypesEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            try {
                var gasstationId = int.Parse(gasstationEvt.input);
                var fuelTypes = new List<GasstationSpotType>();
                foreach(var split in fuelTypesEvt.input.Split(',')) {
                    fuelTypes.Add((GasstationSpotType)int.Parse(split));
                }
                GasstationCreator.createGasstationSpot(player, gasstationId, fuelTypes);
            } catch {
                player.sendBlockNotification("Eine Eingabe war nicht im richtigen Format!", "Eingabe falsch");
            }

            return true;
        }

        private bool onSupportRemoveGasstationSpot(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var station = (Gasstation)data["Gasstation"];
            var spot = (GasstationSpot)data["GasstationSpot"];

            using(var db = new ChoiceVDb()) {
                station.GasstationSpots.Remove(spot);
                spot.onRemove();
                var dbStationSpot = db.configgasstationspots.FirstOrDefault(s => s.id == spot.Id);
                if(dbStationSpot != null) {
                    db.configgasstationspots.Remove(dbStationSpot);
                }
                db.SaveChanges();
                player.sendNotification(NotifactionTypes.Warning, "Zapfsäule erfolgreich entfernt!", "Zapfsäule entfernt");
                loadGasstations();
            }

            return true;
        }

        #endregion
    }
}