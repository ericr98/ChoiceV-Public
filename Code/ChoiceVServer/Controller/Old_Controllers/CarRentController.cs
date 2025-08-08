namespace ChoiceVServer.Controller {
    //class CarRentController : ChoiceVScript {
    //    public static List<ParkedVehicle> parkedList = new List<ParkedVehicle>();
    //    public static List<configrentablecars> CarList = new List<configrentablecars>();
    //    public static List<RentSpot> SpotList = new List<RentSpot>();
    //    public CarRentController() {
    //        EventController.addCollisionShapeEvent("CAR_TALK", OnInteract);
    //        EventController.addMenuEvent("CAR_RENT", CarRent);
    //        EventController.addMenuEvent("CAR_BACK", CarBack);
    //        EventController.addMenuEvent("CAR_TIME", CarTime);
    //        EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnected;
    //        CreateShape();
    //        FillCarList();
    //        FillSpotList();
    //    }

    //    private void FillSpotList() {
    //        var spot1 = new RentSpot {
    //            position = new Position(-964.07f, -2697.178f, 13.828f),
    //            rotation = new Rotation(0, 0, 2.5f),
    //        };
    //        SpotList.Add(spot1);

    //        var spot2 = new RentSpot {
    //            position = new Position(-969f, -2693f, 13f),
    //            rotation = new Rotation(0, 0, 2.5f),
    //        };
    //        SpotList.Add(spot2);

    //        var spot3 = new RentSpot {
    //            position = new Position(-989f, -2707f, 13f),
    //            rotation = new Rotation(0, 0, 2.5f),
    //        };
    //        SpotList.Add(spot3);

    //        var spot4 = new RentSpot {
    //            position = new Position(-961f, -2686f, 16f),
    //            rotation = new Rotation(0, 0, 2.5f),
    //        };
    //        SpotList.Add(spot4);
    //    }

    //    private void FillCarList() {
    //        using (var db = new ChoiceVDb()) {
    //            CarList = db.configrentablecars.ToList();
    //        }

    //    }

    //    private void onPlayerConnected(IPlayer player, characters character) {
    //        InvokeController.AddTimedInvoke("ConnectRentCheck", (ivk) => {
    //            rentcheck(player);
    //        }, TimeSpan.FromSeconds(5), false);
    //    }

    //    public static void rentcheck(IPlayer player) {
    //        using (var db = new ChoiceVDb()) {
    //            var players = ChoiceVAPI.GetAllPlayers();
    //            foreach (var vehicle in db.carrents) {
    //                if (player.getCharacterId() == vehicle.charId) {
    //                    var rentaldate = vehicle.rentalDate;
    //                    var nowtime = rentaldate.AddDays(vehicle.rentalTimePaid);
    //                    if (nowtime < DateTime.Now) {
    //                        if (vehicle.rentalTime <= vehicle.rentalTimePaid) {
    //                            player.sendNotification(NotifactionTypes.Info, "Dein Mietwagen wurde abgeholt!", "");
    //                            var car = ChoiceVAPI.FindVehicleById(vehicle.vehicleId);
    //                            VehicleController.deleteVehicle(player, car);
    //                            db.Remove(vehicle);
    //                        }
    //                    } else {
    //                        player.sendNotification(Constants.NotifactionTypes.Info, "Dein Mietwagen ist bezahlt bis: " + nowtime.ToString(), "");
    //                    }
    //                }
    //            }

    //            db.SaveChanges();
    //        }
    //    }


    //    private bool CarTime(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
    //        configrentablecars vehicle = data["VEHICLE_DB"];
    //        int time;
    //        ListMenuItem.ListMenuItemEvent listitem = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;
    //        var input = listitem.currentElement;
    //        if (input != null) {
    //            time = int.Parse(input);
    //        } else {
    //            time = 1;
    //        }
    //        var cash = player.getCharacterData().Cash;
    //        var price = 350 * time;
    //        if ((cash - price) >= 0) {


    //            var pos = new Position(-961f, -2686f, 16f);
    //            var name = vehicle.model;
    //            var modeluint = (uint)Enum.Parse(typeof(VehicleModel), name);
    //            var modelint = (int)modeluint;
    //            var obj = SpotList.FirstOrDefault(x => ChoiceVAPI.FindNearbyVehicleAtPosition(player, x.position, 2) == null);
    //            if (obj == null) {
    //                player.sendBlockNotification("Null Referenz", "");
    //            }
    //            createVehicle(player, vehicle, modeluint, time, obj.position, obj.rotation);
    //            if (obj.position == pos) {
    //                player.sendNotification(NotifactionTypes.Info, "Der Parkplatz ist anscheinend voll. Das Auto findest du im Parkhaus in der untersten Etage!", "Das Auto findest du in dem Parkhaus");
    //            }
    //            player.getCharacterData().Cash -= price;
    //            return true;
    //        } else {
    //            player.sendNotification(NotifactionTypes.Info, "Ohne Moß nix los", "");
    //            return true;
    //        }

    //    }

    //    private void createVehicle(IPlayer player, configrentablecars vehicle, uint modeluint, int time, Position position, Rotation rotation) {
    //        var vehobj = new VehicleObject {
    //            ModelId = modeluint,
    //            colPosition = position,
    //            colRotation = rotation,
    //            GarageId = -1,
    //        };
    //        var veh = VehicleController.createVehicle(player, vehobj);
    //        var color = veh.getColoring();
    //        color.setPrimaryColor(veh, (byte)new Random().Next(0, 60));
    //        var datetime = DateTime.Now;
    //        using (var db = new ChoiceVDb()) {
    //            var car = new carrents {
    //                modelId = (int)modeluint,
    //                charId = player.getCharacterId(),
    //                vehicleId = veh.getId(),
    //                carName = vehicle.model,
    //                showName = vehicle.showName,
    //                rentalDate = datetime,
    //                rentalTime = time,
    //                rentalTimePaid = time,
    //            };
    //            db.Add(car);
    //            db.SaveChanges();
    //        }
    //        player.sendNotification(Constants.NotifactionTypes.Info, "Dein " + vehicle.showName + " ist  gemietet bis zum " + DateTime.Now.AddDays(time) + "! Vergiss nicht ihn rechtzeitig zurück zu bringen!", "Bring dein Mietwagen rechtzeitig zurück!");

    //    }

    //    private bool CarBack(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
    //        using (var db = new ChoiceVDb()) {
    //            var check = false;
    //            int vehicleID = 0;
    //            string vehiclename = "";
    //            var vehicleNumberPlate = "";
    //            vehicles vehiclesVehicle = null;
    //            var vehicle = db.carrents.FirstOrDefault(x => x.charId == player.getCharacterId());
    //            if (vehicle != null) {
    //                vehicleID = vehicle.vehicleId;
    //                vehiclename = vehicle.showName;
    //            } else {
    //                player.sendNotification(Constants.NotifactionTypes.Info, "Wie wärs wenn du ein Fahrzeug mietest, bevor du eins zurück geben willst?!", "Kein Fahrzeug gemietet");
    //                return true;
    //            }
    //            var obj = parkedList.FirstOrDefault(x => x.VehicleID == vehicleID);
    //            if (obj != null) {
    //                foreach (var vehicles in db.vehicles) {
    //                    if (vehicleID == vehicles.id) {
    //                        check = true;
    //                        vehicleNumberPlate = vehicles.numberPlate;
    //                        vehiclesVehicle = vehicles;
    //                    }
    //                }

    //            }

    //            if (check == true) {
    //                player.sendNotification(Constants.NotifactionTypes.Info, "Dein " + vehiclename + " wurde zurück gegeben!", "Mietwagen " + vehiclename + " wurde abgegeben");
    //                ChoiceVVehicle vehiclecheck = ChoiceVAPI.FindNearbyVehicle(player, 100, x => x.getId() == vehicleID);
    //                VehicleController.deleteVehicle(player, vehiclecheck);
    //                db.carrents.Remove(vehicle);
    //                var item = player.getInventory().getItem(VEHICLE_KEY_DB_ID, i => i.Description.Contains(vehicleNumberPlate));
    //                if (item != null) {
    //                    player.getInventory().removeItem(item);
    //                }
    //                db.SaveChanges();
    //                return true;
    //            } else {
    //                player.sendNotification(Constants.NotifactionTypes.Info, "Abholen tu ich die Karre nicht! Park sie hier aufm Parkplatz oder Zahl weiter!", "Fahr den Mietwagen zum Vermieter");
    //                db.SaveChanges();
    //                return true;
    //            }

    //        }
    //    }

    //    private bool CarRent(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
    //        using (var db = new ChoiceVDb()) {
    //            foreach (var vehicleCheck in db.carrents) {
    //                if (vehicleCheck.charId == player.getCharacterId()) {
    //                    player.sendNotification(Constants.NotifactionTypes.Warning, "Du kannst nur einen Mietwagen mieten!", "Nur ein Mietwagen pro Person!");
    //                    return true;
    //                }
    //            }
    //            configrentablecars vehicle = data["CAR"];
    //            var RentTime = new Menu("Wie lange möchten sie das Auto mieten?", "Maximal 4 tage!");
    //            RentTime.addMenuItem(new ListMenuItem("Anzahl der Tage: ", "", new string[] { "1", "2", "3", "4" }, "CAR_TIME").withData(new Dictionary<string, dynamic> { { "VEHICLE_DB", vehicle } }));
    //            player.showMenu(RentTime);
    //            return true;
    //        }
    //    }

    //    public bool CreateShape() {
    //        var pos = new Position(-974.951f, -2704.76f, 12.86f);
    //        var colShapeRent = CollisionShape.Create(pos, 28, 27, 60, false, true);
    //        colShapeRent.OnEntityEnterShape += onEnter;
    //        colShapeRent.OnEntityExitShape += onExit;
    //        return true;
    //    }

    //    private void onExit(CollisionShape shape, IEntity entity) {
    //        if (entity.Type == BaseObjectType.Vehicle) {
    //            using (var db = new ChoiceVDb()) {
    //                var car = (ChoiceVVehicle)entity;
    //                var obj = parkedList.FirstOrDefault(x => x.VehicleID == car.getId());
    //                if (obj != null) {
    //                    parkedList.Remove(obj);
    //                }
    //            }
    //        }
    //    }

    //    private void onEnter(CollisionShape shape, IEntity entity) {
    //        if (entity.Type == BaseObjectType.Vehicle) {
    //            using (var db = new ChoiceVDb()) {
    //                var car = (ChoiceVVehicle)entity;
    //                var obj = parkedList.FirstOrDefault(x => car.getId() == x.VehicleID);
    //                if (obj == null) {
    //                    var vehicle = new ParkedVehicle {
    //                        VehicleID = car.getId(),
    //                    };
    //                    parkedList.Add(vehicle);
    //                }
    //            }
    //        }
    //    }

    //    private bool OnInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
    //        using (var db = new ChoiceVDb()) {
    //            var RentMenu = new Menu("Auto Vermietung Palushke", "Palushkes beste´s");
    //            var RentList = new Menu("Auto mieten", "Miete deinen Wagen!");
    //            var ActiveList = new Menu("Aktive Mietverträge", "Deine Aktiven Verträge!");
    //            foreach (var vehicle in CarList) {
    //                RentList.addMenuItem(new ClickMenuItem(vehicle.showName, "", vehicle.price + "$ /24h", "CAR_RENT").withData(new Dictionary<string, dynamic> { { "CAR", vehicle } }));
    //            }
    //            foreach (var vehicle in db.carrents) {
    //                if (vehicle.charId == player.getCharacterId()) {
    //                    var time = vehicle.rentalDate;
    //                    time.AddDays(vehicle.rentalTime);
    //                    ActiveList.addMenuItem(new ClickMenuItem(vehicle.showName, "", time.ToString(), "CAR_RENT"));
    //                }
    //            }

    //            var back = new ClickMenuItem("Fahrzeug zurück geben", "Gibt dein Fahrzeug zurück", "", "CAR_BACK", MenuItemStyle.red);
    //            back.needsConfirmation("Wirklich das Fahrzeug abgeben?", "Du bekommst kein Geld zurück!");
    //            RentMenu.addMenuItem(new MenuMenuItem("Auto mieten", RentList));
    //            RentMenu.addMenuItem(new MenuMenuItem("Aktive Mietverträge", ActiveList));
    //            RentMenu.addMenuItem(back);
    //            player.showMenu(RentMenu);
    //            return true;
    //        }
    //    }

    //    public static void GetList(IPlayer player) {
    //        foreach (var list in parkedList) {
    //            player.sendBlockNotification(list.VehicleID.ToString(), "");
    //        }
    //    }
    //}

    //public class RentSpot {
    //    public Position position { get; set; }
    //    public Rotation rotation { get; set; }
    //}

    //public class ParkedVehicle {
    //    public int VehicleID { get; set; }
    //}
}


