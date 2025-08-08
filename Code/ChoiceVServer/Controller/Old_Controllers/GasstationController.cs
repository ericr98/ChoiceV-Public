//using System;
//using System.Linq;
//using System.Collections.Generic;
//using AltV.Net.Data;
//using AltV.Net.Enums;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using ChoiceVServer.Model.Vehicle;
//using ChoiceVServer.Model.Color;
//using ChoiceVServer.Model.Company;
//using ChoiceVServer.InventorySystem;
//using static ChoiceVServer.Base.Constants;
//using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
//using ChoiceVServer.Companies;
//using System.Drawing;
//using ChoiceVServer.Model.Gasstation;

//namespace ChoiceVServer.Controller {

//    class Gasstation {
//        public int Id;
//        public string Name;
//        public int Type;
//        public GasstationOwnerType OwnerType;
//        public int OwnerId;
//        public string OwnerName;
//        public float PricePetrol = 0;
//        public float PriceDiesel = 0;
//        public float PriceKerosene = 0;
//        public float PriceGas = 0;
//        public float PriceElectricity = 0;
//        public float PriceAlternative = 0;

//        public Gasstation(int id, string name, int gasstationtype, GasstationOwnerType ownerType, int ownerId, string ownerName, float pricepetrol, float pricediesel, float pricekerosene, float pricegas, float priceelectricity, float pricealternative) {
//            Id = id;
//            Name = name;
//            Type = gasstationtype;

//            OwnerType = ownerType;
//            OwnerId = ownerId;
//            OwnerName = ownerName;

//            PricePetrol = pricepetrol;
//            PriceDiesel = pricediesel;
//            PriceKerosene = pricekerosene;
//            PriceGas = pricegas;
//            PriceElectricity = priceelectricity;
//            PriceAlternative = pricealternative;
//        }
//    }

//    class GasstationSpot {
//        public int Id;
//        public int gasstationId;
//        public int spotType;
//        public Position colPosition = Position.Zero;
//        public Rotation colRotation = Rotation.Zero;
//        public float colWidth;
//        public float colHeight;

//        public CollisionShape CollisionShape;

//        public List<IPlayer> PlayerStartedRefuel = new List<IPlayer>();

//        public GasstationSpot(int id, int gasstationid, int spottype, Position pos, Rotation rot, float width, float height) {
//            Id = id;
//            gasstationId = gasstationid;
//            spotType = spottype;

//            colPosition = pos;
//            colRotation = rot;
//            colWidth = width;
//            colHeight = height;

//            CollisionShape = CollisionShape.Create(pos, width, height, rot.Yaw, true, true, true, "GASSTATION_SPOT");
//            CollisionShape.Owner = this;
//            CollisionShape.TrackPlayersInVehicles = true;
//            CollisionShape.OnEntityEnterShape += onEnterGasstationSpot;
//            CollisionShape.OnEntityExitShape += onExitGasstationSpot;
//        }

//        private void onEnterGasstationSpot(CollisionShape shape, IEntity entity) {
//            if (entity.Type == BaseObjectType.Player) {
//                IPlayer player = (IPlayer)entity;

//                if (player != null && player.Exists() && PlayerStartedRefuel.Contains(player)) {
//                    PlayerStartedRefuel.Remove(player);
//                }
//            }
//        }

//        private void onExitGasstationSpot(CollisionShape shape, IEntity entity) {
//            if (entity.Type == BaseObjectType.Player) {
//                IPlayer player = (IPlayer)entity;

//                if (player != null && player.Exists() && PlayerStartedRefuel.Contains(player)){
//                    player.closeGasRefuel();
//                    PlayerStartedRefuel.Remove(player);

//                    player.sendNotification(NotifactionTypes.Info, $"Der Tankvorgang wurde abgebrochen.", "", NotifactionImages.Gasstation);
//                }
//            }
//        }
//    }

//    class GasstationController : ChoiceVScript {
//        public static List<Gasstation> AllGasstations = new List<Gasstation>();
//        public static List<GasstationSpot> AllGasstationSpots = new List<GasstationSpot>();

//        public GasstationController() {
//            EventController.addCollisionShapeEvent("GASSTATION_SPOT", onGasstationSpotInteract);
//            EventController.addGasRefuelEvent("GASSTATION_EVENT", onRefuelVehicle);

//            // EventController.addMenuEvent("GASSTATION_SPOT_DIAGNOSE", onDiagnose);

//            loadGasstations();
//        }

//        private bool onGasstationSpotInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> collisionData) {
//            if (player != null && player.Exists() && !player.IsInVehicle) {
//                GasstationSpot spot = AllGasstationSpots.FirstOrDefault(p => p.CollisionShape.Id == collisionShape.Id);

//                if (spot != null) {
//                    Gasstation station = AllGasstations.FirstOrDefault(p => p.Id == spot.gasstationId);

//                    if (station != null) {
//                        ChoiceVVehicle vehicle = null;

//                        foreach (var veh in ChoiceVAPI.FindNearbyVehicles(player, 10)) {
//                            if (veh != null && player.getInventory() != null) {
//                                if (player.getInventory().getItem<VehicleKey>(i => i.VehicleId == veh.getId()) != null) {
//                                    vehicle = veh;
//                                    break;
//                                }
//                            }
//                        }

//                        if (vehicle != null) {
//                            int vehcls = vehicle.getClass();
//                            string vehtype = Convert.ToString((byte)station.Type, 2).PadLeft(8, '0');

//                            if (VehicleTypeVehicle.Contains(vehcls)) {
//                                if (vehtype.Substring(7, 1) == "0") {
//                                    player.sendNotification(NotifactionTypes.Info, $"Autos sind hier nicht zugelassen.", "", NotifactionImages.Gasstation);
//                                    return false;
//                                }
//                            } else if (vehcls == 8) {
//                                if (vehtype.Substring(6, 1) == "0") {
//                                    player.sendNotification(NotifactionTypes.Info, $"Motorräder sind hier nicht zugelassen.", "", NotifactionImages.Gasstation);
//                                    return false;
//                                }
//                            } else if (vehcls == 20) {
//                                if (vehtype.Substring(5, 1) == "0") {
//                                    player.sendNotification(NotifactionTypes.Info, $"Lastwagen sind hier nicht zugelassen.", "", NotifactionImages.Gasstation);
//                                    return false;
//                                }
//                            } else if (vehcls == 19) {
//                                if (vehtype.Substring(4, 1) == "0") {
//                                    player.sendNotification(NotifactionTypes.Info, $"Militärfahrzeuge sind hier nicht zugelassen.", "", NotifactionImages.Gasstation);
//                                    return false;
//                                }
//                            } else if (vehcls == 15) {
//                                if (vehtype.Substring(3, 1) == "0") {
//                                    player.sendNotification(NotifactionTypes.Info, $"Helikopter sind hier nicht zugelassen.", "", NotifactionImages.Gasstation);
//                                    return false;
//                                }
//                            } else if (vehcls == 14) {
//                                if (vehtype.Substring(2, 1) == "0") {
//                                    player.sendNotification(NotifactionTypes.Info, $"Boote/Schiffe sind hier nicht zugelassen.", "", NotifactionImages.Gasstation);
//                                    return false;
//                                }
//                            } else {
//                                player.sendNotification(NotifactionTypes.Info, $"Fahrzeugtyp ist hier nicht zugelassen.", "", NotifactionImages.Gasstation);
//                                return false;
//                            }

//                            var vehobj = vehicle.getObject();
//                            if (vehobj != null) {
//                                string fueltype = Convert.ToString((byte)spot.spotType, 2).PadLeft(8, '0');
//                                float fuelprice = 0f;
//                                string fuelname = "";

//                                if (vehobj.FuelType == FuelType.Petrol) {
//                                    if (fueltype.Substring(7, 1) == "0") {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier keinen Benzin.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    } else if (station.PricePetrol > 0f) {
//                                        fuelname = "Benzin";
//                                        fuelprice = station.PricePetrol;
//                                    } else {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier zur Zeit keinen Benzin.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    }

//                                } else if (vehobj.FuelType == FuelType.Diesel) {
//                                    if (fueltype.Substring(6, 1) == "0") {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier keinen Diesel.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    } else if (station.PriceDiesel > 0f) {
//                                        fuelname = "Diesel";
//                                        fuelprice = station.PriceDiesel;
//                                    } else {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier zur Zeit keinen Diesel.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    }

//                                } else if (vehobj.FuelType == FuelType.Kerosene) {
//                                    if (fueltype.Substring(5, 1) == "0") {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier kein Kerosin.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    } else if (station.PriceKerosene > 0f) {
//                                        fuelname = "Kerosin";
//                                        fuelprice = station.PriceKerosene;
//                                    } else {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier zur Zeit kein Kerosin.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    }

//                                } else if (vehobj.FuelType == FuelType.Gas) {
//                                    if (fueltype.Substring(4, 1) == "0") {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier kein Gas.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    } else if (station.PriceGas > 0f) {
//                                        fuelname = "LPG";
//                                        fuelprice = station.PriceGas;
//                                    } else {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier zur Zeit kein Gas.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    }

//                                } else if (vehobj.FuelType == FuelType.Electricity) {
//                                    if (fueltype.Substring(3, 1) == "0") {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier keine Lademöglichkeit.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    } else if (station.PriceElectricity > 0f) {
//                                        fuelname = "Elektrizität";
//                                        fuelprice = station.PriceElectricity;
//                                    } else {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier zur Zeit keine Lademöglichkeit.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    }

//                                } else if (vehobj.FuelType == FuelType.Alternative) {
//                                    if (fueltype.Substring(2, 1) == "0") {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier keinen alternativen Treibstoff.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    } else if (station.PriceAlternative > 0f) {
//                                        fuelname = "Alternativ";
//                                        fuelprice = station.PriceAlternative;
//                                    } else {
//                                        player.sendNotification(NotifactionTypes.Info, $"Es gibt hier zur Zeit keinen alternativen Treibstoff.", "", NotifactionImages.Gasstation);
//                                        return false;
//                                    }

//                                } else {
//                                    player.sendNotification(NotifactionTypes.Info, $"Das Fahrzeug kann/muss nicht betankt werden.", "", NotifactionImages.Gasstation);
//                                    return false;
//                                }

//                                float fuel = vehobj.getFuel();
//                                float fuelmax = vehobj.getMaxFuel();

//                                if (fuel <= (fuelmax - 1f)) {
//                                    float fuelneeded = (fuelmax - fuel);
//                                    decimal price = (decimal)Math.Round(fuelneeded * fuelprice, 2);

//                                    long charbank = player.getMainBankAccount();
//                                    long compbank = -1;
//                                    long destbank = -1;

//                                    decimal amountbar = player.getCash();
//                                    decimal amountbank = (charbank > 0 ? BankController.getAccountBalance(player, charbank) : 0);
//                                    decimal amountcomp = 0;

//                                    if ((vehobj.OwnerType == VehicleOwnerType.Company || vehobj.OwnerType == VehicleOwnerType.State) && vehobj.OwnerId > 0) {
//                                        int ownerid = vehobj.OwnerId;
//                                        int charid = player.getCharacterId();

//                                        if (charid > 0) {
//                                            var company = CompanyController.getCompany(ownerid);

//                                            if (company != null && company.CompanyBankAccount > 0) {
//                                                var employee = company.Employees.FirstOrDefault(v => v.CharacterId == charid && v.InDuty);

//                                                if (employee != null) {
//                                                    compbank = company.CompanyBankAccount;
//                                                    amountcomp = BankController.getCompanyAccountBalance(ownerid);
//                                                }
//                                            }
//                                        }
//                                    }

//                                    if (amountbar >= price || amountbank >= price || amountcomp >= price) {
//                                        if (station.OwnerType == GasstationOwnerType.Player && station.OwnerId > 0) {
//                                            destbank = player.getMainBankAccount();

//                                        } else if ((station.OwnerType == GasstationOwnerType.Company || station.OwnerType == GasstationOwnerType.State) && station.OwnerId > 0) {
//                                            var company = CompanyController.getCompany(station.OwnerId);
//                                            if (company != null && company.CompanyBankAccount > 0)
//                                                destbank = company.CompanyBankAccount;
//                                        }

//                                        if (destbank > 0) {
//                                            var data = new Dictionary<string, dynamic> { { "station", station }, { "spot", spot }, { "vehicle", vehicle }, { "bankaccount", charbank }, { "compaccount", compbank }, { "destaccount", destbank } };

//                                            if (station.Name.ToLower().Contains("ltd"))
//                                                player.showGasRefuel(new GasRefuel(GasStationType.Ltd, fuel, fuelmax, fuelname, fuelprice, amountbar >= price, amountbank >= price, amountcomp >= price).withData(data));
//                                            else if (station.Name.ToLower().Contains("ron"))
//                                                player.showGasRefuel(new GasRefuel(GasStationType.Ron, fuel, fuelmax, fuelname, fuelprice, amountbar >= price, amountbank >= price, amountcomp >= price).withData(data));
//                                            else if (station.Name.ToLower().Contains("globe"))
//                                                player.showGasRefuel(new GasRefuel(GasStationType.GlobeOil, fuel, fuelmax, fuelname, fuelprice, amountbar >= price, amountbank >= price, amountcomp >= price).withData(data));
//                                            else if (station.Name.ToLower().Contains("xero"))
//                                                player.showGasRefuel(new GasRefuel(GasStationType.XeroGas, fuel, fuelmax, fuelname, fuelprice, amountbar >= price, amountbank >= price, amountcomp >= price).withData(data));

//                                            return true;
//                                        }
//                                    } else {
//                                        player.sendNotification(NotifactionTypes.Info, $"Sie haben zu wenig Geld.", "Zu wenig Geld", NotifactionImages.Gasstation);
//                                    }
//                                } else {
//                                    player.sendNotification(NotifactionTypes.Info, $"Tank ist bereits voll. Mindestabgabe 1 Liter.", "Tank voll", NotifactionImages.Gasstation);
//                                }
//                            }
//                        } else {
//                            player.sendNotification(NotifactionTypes.Info, $"Kein Fahrzeug zum Tanken gefunden.", "Fahrzeug nicht gefunden", NotifactionImages.Gasstation);
//                        }
//                    } else {
//                        Logger.logError($"onGasstationSpotInteract: Gasstation not found!");
//                    }
//                } else {
//                    Logger.logError($"onGasstationSpotInteract: Gasstationspot not found!");
//                }
//            }

//            return false;
//        }

//        private bool onRefuelVehicle(IPlayer player, string itemEvent, int itemId, Dictionary<string, dynamic> data, string Action, string Account, float FuelPrice, float FuelAmmount, string FuelType, GasRefuelCefEvent gasrefuelCefEvent) {
//            if (data.ContainsKey("station") && data.ContainsKey("spot") && data.ContainsKey("vehicle") && data.ContainsKey("bankaccount") && data.ContainsKey("compaccount") && data.ContainsKey("destaccount")) {
//                Gasstation station = data["station"];
//                GasstationSpot spot = data["spot"];
//                ChoiceVVehicle vehicle = data["vehicle"];
//                long bankaccount = data["bankaccount"];
//                long compaccount = data["compaccount"];
//                long destaccount = data["destaccount"];

//                if (station != null && spot != null && vehicle != null) {
//                    if (Action == "Started") {
//                        if (!spot.PlayerStartedRefuel.Contains(player))
//                            spot.PlayerStartedRefuel.Add(player);

//                    } else if (Action == "Finished" || Action == "Stopped" || Action == "Closed") {
//                        if (spot.PlayerStartedRefuel.Contains(player))
//                            spot.PlayerStartedRefuel.Remove(player);

//                        if (FuelAmmount > 0) {
//                            string FuelUnit = (FuelType != "Elektrizität" ? "Liter" : "kWh");
//                            float Money = (FuelAmmount * FuelPrice);
//                            bool status = false;

//                            if (Account == "cash" && player.getCharacterData() != null) {
//                                player.getCharacterData().Cash -= (decimal)Money;
//                                status = true;
//                            } else if (Account == "bank" && bankaccount > 0 && destaccount > 0) {
//                                status = BankController.transferMoney(bankaccount, destaccount, (decimal)Money, $"Tanken bei {station.Name}, {Math.Round(FuelAmmount, 2)} {FuelUnit} {FuelType} zu {Math.Round(FuelPrice * FuelAmmount, 2)}$", player);
//                            } else if (Account == "company" && compaccount > 0 && destaccount > 0) {
//                                status = BankController.transferMoney(compaccount, destaccount, (decimal)Money, $"Tanken bei {station.Name}, {Math.Round(FuelAmmount, 2)} {FuelUnit} {FuelType} zu {Math.Round(FuelPrice * FuelAmmount, 2)}$", player);
//                            }

//                            if (status) {
//                                VehicleObject vehobj = vehicle.getObject();
//                                if (vehobj != null) {
//                                    vehobj.setFuel(FuelAmmount);

//                                    if (Action == "Finished")
//                                        player.sendNotification(NotifactionTypes.Info, $"Tankenvorgang abgeschlossen. Es wurde für {Math.Round(FuelPrice * FuelAmmount, 2)}$ getankt.", "Tankvorgang abgeschlossen", NotifactionImages.Gasstation);
//                                    else
//                                        player.sendNotification(NotifactionTypes.Info, $"Tankenvorgang unterbrochen. Es wurde für {Math.Round(FuelPrice * FuelAmmount, 2)}$ getankt.", "Tankvorgang unterbrochen", NotifactionImages.Gasstation);

//                                    return true;

//                                } else {
//                                    player.sendNotification(NotifactionTypes.Info, $"Tankvorgang konnte nicht abgeschlossen werden.", "Fehler beim Tanken", NotifactionImages.Gasstation);
//                                    Logger.logError($"onRefuelVehicle: Refuel could not finish!");
//                                }
//                            } else {
//                                player.sendNotification(NotifactionTypes.Info, $"Bezahlvorgang konnte nicht abgeschlossen werden.", "Fehler beim Bezahlen", NotifactionImages.Gasstation);
//                                Logger.logError($"onRefuelVehicle: Money transfer could not finish!");
//                            }
//                        }
//                    }

//                }
//            }

//            return false;
//        }

//        public static int createGasstation(string name, int gasstationtype, GasstationOwnerType ownertype, int ownerid, string ownername) {
//            try {
//                int id = 0;

//                using (var db = new ChoiceVDb()) {
//                    var stations = new configgasstations {
//                        name = name,
//                        type = gasstationtype,
//                        ownerId = ownerid,
//                        ownerType = (int)ownertype,
//                        ownerName = ownername,
//                    };

//                    db.configgasstations.Add(stations);
//                    db.SaveChanges();

//                    id = stations.id;

//                    if (id > 0) {
//                        Gasstation station = new Gasstation(id, name, gasstationtype, ownertype, ownerid, ownername, 0, 0, 0, 0, 0, 0);
//                        if (station != null) {
//                            AllGasstations.Add(station);

//                            return id;
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return -1;
//        }

//        public static int createGasstationSpot(int gasstationid, int spottype, Position pos, Rotation rot, float width, float height) {
//            try {
//                int id = 0;

//                Gasstation station = AllGasstations.FirstOrDefault(v => v.Id == gasstationid);
//                if (station != null) {
//                    rot.Yaw = (180 - ChoiceVAPI.radiansToDegrees(rot.Yaw));

//                    using (var db = new ChoiceVDb()) {
//                        var spots = new configgasstationspots {
//                            gasstationId = gasstationid,
//                            spotType = spottype,
//                            position = pos.ToJson(),
//                            rotation = rot.ToJson(),
//                            width = width,
//                            height = height,
//                        };

//                        db.configgasstationspots.Add(spots);
//                        db.SaveChanges();

//                        id = spots.id;

//                        if (id > 0) {
//                            GasstationSpot spot = new GasstationSpot(id, gasstationid, spottype, pos, rot, width, height);
//                            if (spot != null) {
//                                AllGasstationSpots.Add(spot);

//                                return id;
//                            }
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return -1;
//        }

//        public static void deleteGasstation(int gasstationid) {
//            try {
//                var station = AllGasstations.FirstOrDefault(v => v.Id == gasstationid);
//                if (station != null) {
//                    using (var db = new ChoiceVDb()) {
//                        var row = db.configgasstationspots.Where(v => v.gasstationId == gasstationid);
//                        if (row != null) {
//                            db.configgasstationspots.RemoveRange(row);
//                            db.SaveChanges();
//                        }
//                    }

//                    AllGasstationSpots.RemoveAll(v => v.gasstationId == gasstationid);

//                    using (var db = new ChoiceVDb()) {
//                        var row = db.configgasstations.FirstOrDefault(v => v.id == gasstationid);
//                        if (row != null) {
//                            db.configgasstations.Remove(row);
//                            db.SaveChanges();
//                        }
//                    }

//                    AllGasstations.RemoveAll(v => v.Id == gasstationid);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public static void deleteGasstationSpot(int gasstationid, int spotid) {
//            try {
//                var station = AllGasstations.FirstOrDefault(v => v.Id == gasstationid);
//                if (station != null) {
//                    using (var db = new ChoiceVDb()) {
//                        var row = db.configgasstationspots.Where(v => v.id == spotid);
//                        if (row != null) {
//                            db.configgasstationspots.RemoveRange(row);
//                            db.SaveChanges();
//                        }
//                    }

//                    AllGasstationSpots.RemoveAll(v => v.Id == spotid);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public static void reloadGasstations() {
//            try {
//                AllGasstationSpots.Clear();
//                AllGasstations.Clear();

//                using (var db = new ChoiceVDb()) {
//                    foreach (var row in db.configgasstations) {
//                        Gasstation station = new Gasstation(row.id, row.name, row.type, (GasstationOwnerType)row.ownerType, row.ownerId, row.ownerName, row.PricePetrol, row.PriceDiesel, row.PriceKerosene, row.PriceGas, row.PriceElectricity, row.PriceAlternative);
//                        if (station != null) {
//                            AllGasstations.Add(station);

//                            using (var tmpdb = new ChoiceVDb()) {
//                                foreach (var tmprow in tmpdb.configgasstationspots.Where(r => r.gasstationId == row.id)) {
//                                    GasstationSpot spot = new GasstationSpot(tmprow.id, tmprow.gasstationId, tmprow.spotType, tmprow.position.FromJson<Position>(), tmprow.rotation.FromJson<Rotation>(), tmprow.width, tmprow.height);

//                                    if (spot != null)
//                                        AllGasstationSpots.Add(spot);
//                                }
//                            }
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private static void loadGasstations() {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    foreach (var row in db.configgasstations) {
//                        Gasstation station = new Gasstation(row.id, row.name, row.type, (GasstationOwnerType)row.ownerType, row.ownerId, row.ownerName, row.PricePetrol, row.PriceDiesel, row.PriceKerosene, row.PriceGas, row.PriceElectricity, row.PriceAlternative);
//                        if (station != null) {
//                            AllGasstations.Add(station);

//                            using (var tmpdb = new ChoiceVDb()) {
//                                foreach (var tmprow in tmpdb.configgasstationspots.Where(r => r.gasstationId == row.id)) {
//                                    GasstationSpot spot = new GasstationSpot(tmprow.id, tmprow.gasstationId, tmprow.spotType, tmprow.position.FromJson<Position>(), tmprow.rotation.FromJson<Rotation>(), tmprow.width, tmprow.height);

//                                    if (spot != null)
//                                        AllGasstationSpots.Add(spot);
//                                }
//                            }
//                        }
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }
//    }
//}
