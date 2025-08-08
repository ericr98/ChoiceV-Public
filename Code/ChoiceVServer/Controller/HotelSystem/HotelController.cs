using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.HotelSystem;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class HotelCheckInNPCModule : NPCModule {
        private Hotel _hotel;
        private Hotel Hotel { get => _hotel != null ? _hotel : getHotel(); }

        private int HotelId;
        private Hotel getHotel() {
            _hotel = HotelController.getHotelById(HotelId);
            return _hotel;
        }

        public HotelCheckInNPCModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped) {
            HotelId = int.Parse(settings["HotelId"].ToString());
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Hotel-Check-In-Modul", $"Ein Hotel-Check-In-Modul für das {Hotel.Name}", $"{Hotel.Name}");
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            return new List<MenuItem> { new MenuMenuItem($"{Hotel.Name} Check-In", Hotel.getCheckInMenu(player)) };
        }

        public override void onRemove() { }
    }

    //TODO IF PLAYER ENTERS HOTEL ROOM COLLISIONSHAPE WITH DOORS CLOSED, THEN SEND INFO!
    public class HotelController : ChoiceVScript {
        public static List<Hotel> Hotels;

        public HotelController() {
            EventController.addMenuEvent("HOTEL_CREATE_NEW_BOOKING", onCreateNewBooking);
            EventController.addMenuEvent("HOTEL_DELETE_BOOKING", onDeleteBooking);

            EventController.addMenuEvent("HOTEL_CHANGE_BOOKING_PHONENUMBER", onChangeBookingNumber);
            EventController.addMenuEvent("HOTEL_CHANGE_BOOKING_BANKACCOUNT", onChangeBookingBankaccount);
            EventController.addMenuEvent("HOTEL_ADD_BOOKING_PREPAID_AMOUNT", onAddBookingPrepaidAmont);

            EventController.addMenuEvent("HOTEL_CHANGE_BOOKING_LOCK_INDEX", onChangeBookingLockIndex);
            EventController.addMenuEvent("HOTEL_CREATE_NEW_BOOKING_KEY", onCreateNewBookingKey);

            EventController.addMenuEvent("HOTEL_OPEN_LATE_FEATURE", onOpenFeatureInstanceLate);

            EventController.MainReadyDelegate += onMainReady;

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Hotels,
                    "Hotelmenü",
                    hotelMenuGenerator
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_HOTEL", onSupportCreateHotel);
            EventController.addMenuEvent("SUPPORT_CHANGE_HOTEL_BANKKONTO", onSupporChangeHotelBankaccount);
            EventController.addMenuEvent("SUPPORT_DELETE_HOTEL", onSupportDeleteHotel);

            EventController.addMenuEvent("SUPPORT_CREATE_HOTELROOM", onSupportCreateRoom);
            EventController.addMenuEvent("SUPPORT_DELETE_HOTELROOM", onSupportDeleteRoom);

            EventController.addMenuEvent("SUPPORT_HOTEL_CREATE_FEATURE", onSupportCreateFeature);

            HotelRoomFeatureInstance.HotelRoomFeatureInstanceDataChange += onInstanceDataChange;

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;


            PedController.addNPCModuleGenerator("Hotel-Check-In-Module", hotelModuleGenerator, hotelModuleGeneratorCallback);
        }

        #region NpcModule

        private List<MenuItem> hotelModuleGenerator(ref Type codeType) {
            codeType = typeof(HotelCheckInNPCModule);

            var namesList = Hotels.Select(h => h.Name).ToArray();

            return new List<MenuItem> { new ListMenuItem("Hotel", "Wähle das Hotel aus, welches der NPC bedient", namesList, "") };
        }

        private void hotelModuleGeneratorCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var hotelNameEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var hotel = Hotels.FirstOrDefault(h => h.Name == hotelNameEvt.currentElement);

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "HotelId", hotel.Id } });
        }

        #endregion

        private void onPlayerConnect(IPlayer player, character character) {
            foreach(var hotel in Hotels) {
                foreach(var room in hotel.Rooms) {
                    if(room.CollisionShape.IsInShape(player.Position)) {
                        if(!player.getInventory().hasItem<DoorKey>(k => room.keyWorksForRoom(player.Dimension, k))) {
                            player.Position = hotel.CheckInPosition;

                            player.sendBlockNotification("Du wurdest sicherheitshalber aus deinem Hotelzimmer geportet! Wenn dies ein Fehler war, melde dich im Support!", "Aus Hotel geportet!");

                            InvokeController.AddTimedInvoke("Hotel-Port-Reminder", (i) => {
                                player.sendBlockNotification("Du wurdest sicherheitshalber aus deinem Hotelzimmer geportet! Wenn dies ein Fehler war, melde dich im Support!", "Aus Hotel geportet!");
                            }, TimeSpan.FromSeconds(30), false);
                        }

                        return;
                    }
                }
            }
        }

        private void updateHotels(IInvoke obj) {
            var now = DateTime.Now;
            foreach(var hotel in Hotels) {
                hotel.update(now);
            }

            using(var db = new ChoiceVDb()) {
                var toRemoves = db.hotelroomfeatureinstances.Where(i => i.savedDeleteDate < now).ToList();
                foreach(var toRemove in toRemoves) {
                    var type = Type.GetType("ChoiceVServer.Controller.HotelSystem." + toRemove.codeFeature, false);
                    var instance = Activator.CreateInstance(type, toRemove, null, null) as HotelRoomFeatureInstance;
                    instance.deleteLate();
                }

                db.hotelroomfeatureinstances.RemoveRange(toRemoves);
                db.SaveChanges();
            }
        }

        private void onInstanceDataChange(HotelRoomFeatureInstance instance, string parameter, dynamic value) {
            using(var db = new ChoiceVDb()) {
                var data = db.hotelroomfeatureinstancesdata.Find(instance.Id, parameter);
                if(data != null) {
                    data.value = ((object)value).ToJson();
                    db.SaveChanges();
                } else {
                    var newData = new hotelroomfeatureinstancesdatum {
                        instanceId = instance.Id,
                        parameter = parameter,
                        value = ((object)value).ToJson(),
                    };

                    db.hotelroomfeatureinstancesdata.Add(newData);
                    db.SaveChanges();
                }
            }
        }

        private string getRoomDoorGroup(Hotel hotel, int roomNumber) {
            return $"{hotel.Name.ToUpper().Replace(' ', '_')}_{roomNumber}";
        }

        public static Hotel getHotelById(int id) {
            return Hotels.Find(h => h.Id == id);
        }

        private Menu hotelMenuGenerator(IPlayer player) {
            var menu = new Menu("Hotelmenü", "Erstelle, editiere und lösche Hotels");

            var createHotelMenu = new Menu("Hotel erstellen", "Erstelle ein neues Hotel");
            createHotelMenu.addMenuItem(new InputMenuItem("Hotelname", "Der Name des Hotels", "", ""));
            createHotelMenu.addMenuItem(new InputMenuItem("Bankkonto", "Trage -1 ein um ein neues Konto zu erstellen", "-1", ""));
            createHotelMenu.addMenuItem(new InputMenuItem("Telefonnummer", "Trage -1 ein um eine neue Nummer zu erstellen", "-1", ""));
            createHotelMenu.addMenuItem(new MenuStatsMenuItem("Hotel erstellen", "Erstelle das angegebene Hotel", "SUPPORT_CREATE_HOTEL", MenuItemStyle.green).needsConfirmation("Hotel erstellen?", "Hotel wirklich erstellen?"));
            menu.addMenuItem(new MenuMenuItem(createHotelMenu.Name, createHotelMenu, MenuItemStyle.green));

            foreach(var hotel in Hotels) {
                var hotelMenu = new Menu(hotel.Name, $"Editiere und lösche das {hotel.Name}");
                hotelMenu.addMenuItem(new InputMenuItem("Bankkonto ändern", "Ändere das Bankkonto für Überweisungen. Trage -1 für ein neues Konto ein.", $"{hotel.BankAccount}", "SUPPORT_CHANGE_HOTEL_BANKKONTO"));

                var roomMenu = new Menu("Hotelräume", "Erstelle, editiere und lösche Räume");

                var createRoomMenu = new Menu("Raum erstellen", "Erstelle ein neuen Raum");
                createRoomMenu.addMenuItem(new InputMenuItem("Ebene", "Eine Ebene sind mehrere Räume auf gleicher Höhe.", "", ""));
                createRoomMenu.addMenuItem(new InputMenuItem("Raumnummer", "Die Nummer des Raumes", "", ""));
                createRoomMenu.addMenuItem(new InputMenuItem("max. Buchungen", "Gibt an wie oft der Raum gebucht werden kann. (für Dimensionen)", "", ""));
                createRoomMenu.addMenuItem(new InputMenuItem("Preis", "Der Preis des Zimmers in $", "", ""));
                createRoomMenu.addMenuItem(new MenuStatsMenuItem("Raum erstellen", "Erstelle den angegebenen Raum", "SUPPORT_CREATE_HOTELROOM", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Hotel", hotel } }).needsConfirmation("Raum erstellen?", "Raum wirklich erstellen?"));
                roomMenu.addMenuItem(new MenuMenuItem(createRoomMenu.Name, createRoomMenu, MenuItemStyle.green));

                foreach(var room in hotel.Rooms) {
                    var specRoomMenu = new Menu($"Raum {room.RoomNumber}", "Editiere, lösche den Raum");
                    var data = new Dictionary<string, dynamic> { { "Hotel", hotel }, { "Room", room } };
                    specRoomMenu.addMenuItem(new InputMenuItem("Preis ändern", $"Ändere den Preis des Raums. Aktuell ${room.Price}", $"${room.Price}", "SUPPORT_CHANGE_HOTELROOM_PRICE").withData(data));

                    var createFeatureMenu = new Menu("Feature erstellen", "Erstelle ein Feature");

                    var allFeatureTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(HotelRoomFeatureInstance))).ToList();
                    foreach(var item in allFeatureTypes) {
                        var temp = Activator.CreateInstance(item);
                        var featureName = (string)temp.GetType().GetMethod("getDisplayName").Invoke(temp, null);
                        var featureInfo = (string)temp.GetType().GetMethod("getDisplayInfo").Invoke(temp, null);
                        var featureMenu = HotelRoomFeature.getCreateMenu(featureName, featureInfo, item, room);
                        createFeatureMenu.addMenuItem(new MenuMenuItem(featureMenu.Name, featureMenu));
                    }
                    specRoomMenu.addMenuItem(new MenuMenuItem(createFeatureMenu.Name, createFeatureMenu));

                    var alreadyFeatureMenu = new Menu("Feature Liste", "Sieht dir aktuelle Features an");
                    foreach(var feature in room.Features) {
                        alreadyFeatureMenu.addMenuItem(new StaticMenuItem(feature.Name, "", ""));
                    }
                    specRoomMenu.addMenuItem(new MenuMenuItem(alreadyFeatureMenu.Name, alreadyFeatureMenu));

                    specRoomMenu.addMenuItem(new ClickMenuItem("Raum löschen", "Lösche den ausgewählten Raum", "", "SUPPORT_DELETE_HOTELROOM", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Hotel", hotel } }).needsConfirmation("Raum löschen?", "Raum wirklich löschen?"));
                    roomMenu.addMenuItem(new MenuMenuItem(specRoomMenu.Name, specRoomMenu));
                }

                hotelMenu.addMenuItem(new MenuMenuItem(roomMenu.Name, roomMenu));
                hotelMenu.addMenuItem(new ClickMenuItem("Hotel löschen", "Lösche das ausgewählte Hotel", "", "SUPPORT_DELETE_HOTEL", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Hotel", hotel } }).needsConfirmation("Hotel löschen?", "Hotel wirklich löschen?"));

                menu.addMenuItem(new MenuMenuItem(hotelMenu.Name, hotelMenu));
            }

            return menu;
        }

        private void onMainReady() {
            Hotels = new List<Hotel>();
            using(var db = new ChoiceVDb()) {
                var list = db.confighotels
                    .Include(h => h.confighotelrooms)
                        .ThenInclude(h => h.hotelroombookings)
                    .Include(h => h.confighotelrooms)
                        .ThenInclude(h => h.confighotelroomfeatures)
                            .ThenInclude(b => b.hotelroomfeatureinstances)
                                .ThenInclude(i => i.hotelroomfeatureinstancesdata).ToList();
                list.Reverse();
                foreach(var dbHotel in list) {
                    var roomList = new List<HotelRoom>();

                    if(dbHotel.bankAccount == null) {
                        var bank = BankController.getBankByType(Constants.BankCompanies.LibertyBank);
                        dbHotel.bankAccount = BankController.createBankAccount(typeof(HotelController), dbHotel.name, BankAccountType.CompanyKonto, 0, bank, true).id;
                        db.SaveChanges();
                    }

                    if(dbHotel.phoneNumber == null) {
                        dbHotel.phoneNumber = PhoneController.createNewPhoneNumber($"{dbHotel.id}: {dbHotel.name}").number;
                        db.SaveChanges();
                    }

                    var hotel = new Hotel(dbHotel.id, dbHotel.name, dbHotel.phoneNumber ?? -1, dbHotel.checkInCollPosition.FromJson(), dbHotel.bankAccount ?? -1, new Dictionary<int, HotelRoomBooking>());
                    foreach(var dbRoom in dbHotel.confighotelrooms) {
                        var doors = DoorController.getDoorsByIds(new List<int> { dbRoom.door1Id, dbRoom.door2Id ?? -1, dbRoom.door3Id ?? -1 });
                        var roomColShape = CollisionShape.Create(dbRoom.collPosition.FromJson(), dbRoom.collWidth, dbRoom.collHeight, dbRoom.collRotation, true, false).withRestrictedActions();

                        var featureList = dbRoom.confighotelroomfeatures.Select(f => new HotelRoomFeature(f)).ToList();
                        var room = new HotelRoom(dbRoom.id, hotel, dbRoom.level, dbRoom.roomNumber, dbRoom.maxBookings, dbRoom.price, roomColShape, doors, featureList);
                        roomList.Add(room);

                        var bookingsList = new List<HotelRoomBooking>();
                        //Add Bookings
                        foreach(var dbBooking in dbRoom.hotelroombookings) {
                            var booking = new HotelRoomBooking(dbBooking.id, dbBooking.charId, dbBooking.reserverName, dbBooking.bankAccount, dbBooking.contactPhoneNumber, room, dbBooking.dimension, dbBooking.nextPay)
                                                              .withPrepaidAmount(dbBooking.prepaidAmount);

                            var featureInstancesList = new List<HotelRoomFeatureInstance>();
                            foreach(var row in dbBooking.hotelroomfeatureinstances) {
                                var main = featureList.FirstOrDefault(f => f.Id == row.featureId);

                                var type = Type.GetType("ChoiceVServer.Controller.HotelSystem." + row.codeFeature, false);
                                var instance = Activator.CreateInstance(type, row, main, booking) as HotelRoomFeatureInstance;

                                main.addInstance(instance);
                                featureInstancesList.Add(instance);
                            }

                            booking.setFeatureInstances(featureInstancesList);
                            hotel.addBooking(booking);
                        }
                    }

                    hotel.setRooms(roomList);
                    Hotels.Add(hotel);
                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"Hotel {dbHotel.name} with {dbHotel.confighotelrooms.Count} rooms loaded");
                }
            }

            InvokeController.AddTimedInvoke("Hotel-Updater", updateHotels, TimeSpan.FromMinutes(15), true);
        }

        #region CheckInEvents

        private bool onCreateNewBooking(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var hotel = (Hotel)data["Hotel"];
            lock(hotel) {
                var rnd = new Random();
                var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;
                var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var priceEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
                var price = decimal.Parse(priceEvt.currentElement);
                var payOptionEvt = evt.elements[2].FromJson<ListMenuItemEvent>();
                var numberEvt = evt.elements[3].FromJson<CheckBoxMenuItemEvent>();

                var viableRooms = hotel.Rooms.Where(r => r.Price == price).OrderBy(r => r.RoomNumber).ToList();
                var bookings = hotel.HotelRoomBookings.Values.Where(r => viableRooms.Contains(r.HotelRoom)).GroupBy(r => Math.Abs(r.Dimension)).ToDictionary(r => r.Key, r => r.ToList());
                var maxRoomNumber = viableRooms.Count;

                var etage = 0;
                HotelRoom room = null;
                while(true) {
                    etage++;

                    var levelBookings = bookings.ContainsKey(etage) ? bookings[etage] : [];
                    if(levelBookings.Count == 0) {
                        room = viableRooms.First();
                        break;
                    }

                    for(var i = 0; i < viableRooms.Count; i++) {
                        if(!levelBookings.Any(b => b.HotelRoom.Id == viableRooms[i].Id)) {
                            room = viableRooms[i];
                            break;
                        }
                    }

                    if(room != null) {
                        break;
                    }
                }

                var newDim = -etage;
                var number = numberEvt.check ? player.getMainPhoneNumber() : -1;
                var bankAccount = -1L;
                decimal prepaidAmount = 0;
                //Check for payment
                if(payOptionEvt.currentElement == "Bankkonto") {
                    var acc = player.getMainBankAccount();
                    if(acc == -1) {
                        player.sendBlockNotification("Du hast kein Standardbankkonto festgelegt!", "Kein Standardbankkonto", Constants.NotifactionImages.Hotel);
                        return false;
                    } else {
                        if(!BankController.transferMoney(acc, hotel.BankAccount, room.Price, $"Erstbezahlung Raum {room.RoomNumber} in Etage {etage} im {hotel.Name}", out var failMessage, false)) {
                            player.sendBlockNotification($"Überweisung des Erstbetrages gescheitert. Der Fehler war: {failMessage}!", "Überweisung gescheitert", Constants.NotifactionImages.Hotel);
                            return false;
                        }

                        bankAccount = player.getMainBankAccount();
                    }
                } else if(payOptionEvt.currentElement == "Bar") {
                    if(!player.removeCash(room.Price * 3)) {
                        prepaidAmount = room.Price * 2;
                        player.sendBlockNotification($"Du hast nicht genug Bargeld dabei!", "Zu wenig Geld", Constants.NotifactionImages.Hotel);
                        return false;
                    }
                }

                var cfg = InventoryController.getConfigItemForType<DoorKey>();
                var doorAddDim = room.AllDoors.First().AdditionalDimensions;
                var idx = 0;
                if(doorAddDim.ContainsKey(newDim)) {
                    idx = doorAddDim[newDim].LockIndex;
                }
                var doorKey = new DoorKey(cfg, room.AllDoors.First().getLockIndexInDimension(newDim), newDim, room.AllDoors.First().GroupName, -1, $"Schlüssel für Zimmer {room.RoomNumber} in Etage {etage} Schloss-Idx:({idx}) im {hotel.Name}");

                if(player.getInventory().addItem(doorKey)) {
                    using(var db = new ChoiceVDb()) {
                        var dbBooking = new hotelroombooking {
                            charId = player.getCharacterId(),
                            hotelRoomId = room.Id,
                            reserverName = nameEvt.input,
                            nextPay = DateTime.Now + TimeSpan.FromDays(1),
                            bankAccount = bankAccount,
                            contactPhoneNumber = number,
                            dimension = newDim,
                            prepaidAmount = prepaidAmount,
                        };

                        db.hotelroombookings.Add(dbBooking);

                        db.SaveChanges();


                        var booking = new HotelRoomBooking(dbBooking.id, player.getCharacterId(), nameEvt.input, bankAccount, number, room, newDim, DateTime.Now + TimeSpan.FromDays(1));

                        var instanceList = new List<HotelRoomFeatureInstance>();
                        foreach(var feature in room.Features) {
                            var dbInstance = new hotelroomfeatureinstance {
                                bookingId = booking.Id,
                                featureId = feature.Id,
                                codeFeature = feature.CodeInstance,
                            };

                            db.hotelroomfeatureinstances.Add(dbInstance);
                            db.SaveChanges();

                            var type = Type.GetType("ChoiceVServer.Controller.HotelSystem." + feature.CodeInstance, false);
                            var instance = (HotelRoomFeatureInstance)Activator.CreateInstance(type, new object[] { dbInstance.id, feature, booking });
                            feature.addInstance(instance);
                            instanceList.Add(instance);
                        }

                        booking.setFeatureInstances(instanceList);
                        hotel.addBooking(booking);

                        foreach(var door in booking.HotelRoom.AllDoors) {
                            door.addAdditionalDimension(new DoorDimension(newDim, 0, true));
                        }
                    }

                    if(number != -1) {
                        PhoneController.sendSMSToNumber(hotel.PhoneNumber, number, $"Ihre Buchung im {hotel.Name} war erfolgreich. Wir wünschen ihnen einen angenehmen Aufenthalt.");
                    }

                    player.sendNotification(Constants.NotifactionTypes.Success, $"Willkommen {nameEvt.input} im {hotel.Name}. Ihre Raumnummer ist {room.RoomNumber} in Etage {etage}. Sie haben ${room.Price} bezahlt.", "Hotelraum gebucht", Constants.NotifactionImages.Hotel);
                } else {
                    player.sendBlockNotification("Es war kein Platz im Inventar für den Schlüssel!", "Inventar voll", Constants.NotifactionImages.Hotel);
                }

                return true;
            }
        }

        private bool onDeleteBooking(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var booking = (HotelRoomBooking)data["Booking"];

            var keys = player.getInventory().getItems<DoorKey>(k => booking.HotelRoom.keyWorksForRoom(player.Dimension, k) && k.Dimension == booking.Dimension).Reverse();

            foreach(var key in keys) {
                player.getInventory().removeItem(key);
            }

            deleteBooking(booking, false);
            player.sendNotification(Constants.NotifactionTypes.Warning, "Du hast erfolgreich ausgecheckt!", "Erfolgreich ausgecheckt", Constants.NotifactionImages.Hotel);

            return true;
        }

        public static void deleteBooking(HotelRoomBooking booking, bool forced) {
            foreach(var door in booking.HotelRoom.AllDoors) {
                door.lockForDimension(booking.Dimension);
                door.changeLockInDimension(booking.Dimension);
            }

            foreach(var entity in booking.HotelRoom.CollisionShape.getAllEntities()) {
                if(entity is IPlayer) {
                    var player = entity as IPlayer;
                    player.Position = booking.HotelRoom.Hotel.CheckInPosition;
                    player.changeDimension(Constants.GlobalDimension);
                    player.sendNotification(Constants.NotifactionTypes.Danger, "Du wurdest aus dem Hotelzimmer geschmissen!", "Aus Hotel raugeschmießen", Constants.NotifactionImages.Hotel);
                }
            }

            using(var db = new ChoiceVDb()) {
                var dbBooking = db.hotelroombookings.Include(b => b.hotelroomfeatureinstances).FirstOrDefault(b => b.id == booking.Id);
                if(dbBooking != null) {
                    foreach(var instance in booking.FeatureInstances) {
                        var dbInstance = dbBooking.hotelroomfeatureinstances.FirstOrDefault(i => i.id == instance.Id);
                        if(instance.needsToBeSaved()) {
                            dbInstance.isSaved = 1;
                            dbInstance.savedCharId = booking.CharId;
                            dbInstance.savedDeleteDate = DateTime.Now + TimeSpan.FromDays(30);
                        } else {
                            instance.deleteLate();
                            db.hotelroomfeatureinstances.Remove(dbInstance);
                        }
                    }

                    db.SaveChanges();

                    db.hotelroombookings.Remove(dbBooking);

                    db.SaveChanges();
                }

                booking.HotelRoom.Hotel.removeBooking(booking);

            }

            if(booking.ContactPhoneNumber != -1) {
                if(forced) {
                    PhoneController.sendSMSToNumber(booking.HotelRoom.Hotel.PhoneNumber, booking.ContactPhoneNumber, $"Ihre Buchung im {booking.HotelRoom.Hotel.Name} wurde aufgrund fehlender Geldmittel aufgelöst. Um mögliche Hinterlassenschaften abzuholen kommen sie zum Hotel.");
                } else {
                    PhoneController.sendSMSToNumber(booking.HotelRoom.Hotel.PhoneNumber, booking.ContactPhoneNumber, $"Ihre Buchung im {booking.HotelRoom.Hotel.Name} wurde von ihnen aufgelöst. Um mögliche Hinterlassenschaften abzuholen kommen sie zum Hotel.");
                }
            }

            booking.Dispose();
        }

        private bool onChangeBookingNumber(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var booking = (HotelRoomBooking)data["Booking"];

            var smartphone = player.getInventory().getItem<Smartphone>(i => i.IsEquipped);
            if(smartphone != null && smartphone.SIMCardInventory.getCount() > 0) {
                try {
                    booking.setContactPhoneNumber(smartphone.SIMCardInventory.getItem<SIMCard>(s => true).Number);
                } catch(Exception) {
                    player.sendBlockNotification("Die Eingabe war keine gültige Telefonnummer!", "Ungültige Eingabe", Constants.NotifactionImages.Hotel);
                }
            } else {
                player.sendBlockNotification("Du hast kein Smartphone/Sim ausgerüstet!", "Kein Smartphone/Sim ausgerüstet");
            }

            return true;
        }

        private bool onChangeBookingBankaccount(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var accountEvt = menuItemCefEvent as InputMenuItemEvent;
            var booking = (HotelRoomBooking)data["Booking"];

            try {
                var acc = player.getMainBankAccount();
                booking.setBankaccount(acc);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Bankkonto auf {acc} geändert.", "Neues Konto", Constants.NotifactionImages.Hotel);
            } catch(Exception) {
                player.sendBlockNotification("Die Eingabe war kein gültiges Konto!", "Ungültige Eingabe", Constants.NotifactionImages.Hotel);
            }

            return true;
        }

        private bool onCreateNewBookingKey(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var booking = (HotelRoomBooking)data["Booking"];

            if(player.removeCash(50)) {
                var roomInfo = booking.Dimension == Constants.GlobalDimension ? booking.HotelRoom.RoomNumber.ToString() : $"{booking.HotelRoom.RoomNumber}, Etage: {Math.Abs(booking.Dimension)}, Schloss-Idx:({booking.HotelRoom.AllDoors[0].AdditionalDimensions[booking.Dimension].LockIndex})";
                var cfg = InventoryController.getConfigItemForType<DoorKey>();
                var idx = booking.HotelRoom.AllDoors.First().getLockIndexInDimension(booking.Dimension);
                var newKey = new DoorKey(cfg, idx, booking.Dimension, booking.HotelRoom.AllDoors.First().GroupName, -1, $"Zweitschlüssel für Zimmer {roomInfo} im {booking.HotelRoom.Hotel.Name}, Schloss-Idx: ({idx})");

                player.getInventory().addItem(newKey);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Neuen Schlüssel für Zimmer {roomInfo} erworben", "Schlüssel erworben", Constants.NotifactionImages.Hotel);
            } else {
                player.sendBlockNotification("Du hast nicht genug Bargeld dabei!", "Nicht genug Bargeld", Constants.NotifactionImages.Hotel);
            }

            return true;
        }

        private bool onChangeBookingLockIndex(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var booking = (HotelRoomBooking)data["Booking"];

            if(player.removeCash(200)) {
                var keys = player.getInventory().getItems<DoorKey>(k => booking.HotelRoom.keyWorksForRoom(player.Dimension, k) && k.Dimension == booking.Dimension).Reverse();

                foreach(var key in keys) {
                    player.getInventory().removeItem(key);
                }

                foreach(var door in booking.HotelRoom.AllDoors) {
                    door.lockForDimension(booking.Dimension);
                    door.changeLockInDimension(booking.Dimension);
                }

                var roomInfo = booking.Dimension == Constants.GlobalDimension ? booking.HotelRoom.RoomNumber : Math.Abs(booking.Dimension);
                var cfg = InventoryController.getConfigItemForType<DoorKey>();
                var idx = booking.HotelRoom.AllDoors.First().getLockIndexInDimension(booking.Dimension);
                var newKey = new DoorKey(cfg, idx, booking.Dimension, booking.HotelRoom.AllDoors.First().GroupName, -1, $"{booking.HotelRoom.RoomNumber}, Etage: {Math.Abs(booking.Dimension)}, Schloss-Idx:({idx})");

                player.getInventory().addItem(newKey);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Schloss der Tür wurde gewechselt. Du hast einen neuen Schlüssel erhalten", "Schloss gewechselt", Constants.NotifactionImages.Hotel);
            } else {
                player.sendBlockNotification("Du hast nicht genug Bargeld dabei!", "Nicht genug Bargeld", Constants.NotifactionImages.Hotel);
            }

            return true;
        }

        private bool onAddBookingPrepaidAmont(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var amountEvt = (InputMenuItemEvent)data["PreviousCefEvent"];
            var booking = (HotelRoomBooking)data["Booking"];

            try {
                var amount = Math.Abs(decimal.Parse(amountEvt.input));
                if(player.removeCash(amount)) {
                    booking.changePrepaidAmount(amount);
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast erfolgreich ${amount} vorgezahlt", "${amount} vorgezahlt", Constants.NotifactionImages.Hotel);
                } else {
                    var rest = amount - player.getCash();
                    player.sendBlockNotification($"Du hast nicht genug Bargeld. Dir fehlen ${rest} dafür!", $"Dir fehlen ${rest}");
                }
            } catch(Exception) {
                player.sendBlockNotification("Die Eingabe war ungültig", "Ungültige Eingabe", Constants.NotifactionImages.Hotel);
            }

            return true;
        }

        private bool onOpenFeatureInstanceLate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var instance = (HotelRoomFeatureInstance)data["Instance"];
            instance.onLateInteract(player);
            return true;
        }

        #endregion

        #region SupportMenuEvents

        private bool onSupportCreateHotel(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;
            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var bankEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var phoneNumberEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            try {
                var bankId = long.Parse(bankEvt.input);
                var number = long.Parse(phoneNumberEvt.input);

                long bankAccount;
                if(bankId == -1) {
                    var bank = BankController.getBankByType(Constants.BankCompanies.LibertyBank);
                    bankAccount = BankController.createBankAccount(typeof(HotelController), nameEvt.input, BankAccountType.CompanyKonto, 0, bank, true).id;
                } else {
                    if(BankController.verifyBankaccount(bankId, b => b.accountType == (int)BankAccountType.CompanyKonto)) {
                        bankAccount = bankId;
                    } else {
                        player.sendBlockNotification("Das angegebene Bankkonto war nicht gültig. Es muss sich um ein Firmenkonto handeln.", "Ungültiges Konto", Constants.NotifactionImages.Hotel);
                        return false;
                    }
                }

                long phoneNumber;
                if(number != -1) {
                    phoneNumber = PhoneController.createNewPhoneNumber(number, nameEvt.input).number;
                } else {
                    phoneNumber = PhoneController.createNewPhoneNumber(nameEvt.input).number;
                }

                using(var db = new ChoiceVDb()) {
                    var dbHotel = new confighotel {
                        name = nameEvt.input,
                        phoneNumber = phoneNumber,
                        bankAccount = bankAccount,
                        checkInCollPosition = player.Position.ToJson(),
                        checkInCollWidth = 0,
                        checkInCollHeight = 0,
                        checkInCollRotation = 0,
                    };

                    db.confighotels.Add(dbHotel);

                    db.SaveChanges();

                    Hotels.Add(new Hotel(dbHotel.id, dbHotel.name, dbHotel.phoneNumber ?? -1, dbHotel.checkInCollPosition.FromJson(), bankAccount, new Dictionary<int, HotelRoomBooking>()));
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Hotel mit Namen {nameEvt.input} erfolgreich erstellt. Die Lobby-Position wurde an deinen aktuellen Standort gesetzt!", "", Constants.NotifactionImages.Hotel);
                }
            } catch(Exception) {
                player.sendBlockNotification("Eine der Eingaben war ungültig.", "Ungültige Eingabe", Constants.NotifactionImages.Hotel);
                return false;
            }


            return true;
        }

        private bool onSupporChangeHotelBankaccount(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var hotel = (Hotel)data["Hotel"];
            var evt = menuItemCefEvent as InputMenuItemEvent;
            try {
                var bankId = long.Parse(evt.input);

                long bankAccount;
                if(bankId == -1) {
                    var bank = BankController.getBankByType(Constants.BankCompanies.LibertyBank);
                    bankAccount = BankController.createBankAccount(typeof(HotelController), hotel.Name, BankAccountType.CompanyKonto, 0, bank, true).id;
                } else {
                    if(BankController.verifyBankaccount(bankId, b => b.accountType == (int)BankAccountType.CompanyKonto)) {
                        bankAccount = bankId;
                    } else {
                        player.sendBlockNotification("Das angegebene Bankkonto war nicht gültig. Es muss sich um ein Firmenkonto handeln.", "Ungültiges Konto", Constants.NotifactionImages.Hotel);
                        return false;
                    }
                }

                using(var db = new ChoiceVDb()) {
                    var dbHotel = db.confighotels.Find(hotel.Id);
                    dbHotel.bankAccount = bankAccount;
                    hotel.setBankAccount(bankAccount);
                    db.SaveChanges();
                }
            } catch(Exception) {
                player.sendBlockNotification("Eine der Eingaben war ungültig.", "Ungültige Eingabe", Constants.NotifactionImages.Hotel);
                return false;
            }
            return true;
        }

        private bool onSupportDeleteHotel(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var hotel = (Hotel)data["Hotel"];

            using(var db = new ChoiceVDb()) {
                var dbHotel = db.confighotels.Find(hotel.Id);

                if(dbHotel != null) {
                    db.confighotels.Remove(dbHotel);

                    db.SaveChanges();

                    Hotels.Remove(hotel);
                    hotel.Dispose();
                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Hotel mit Namen {hotel.Name} erfolgreich gelöscht", "", Constants.NotifactionImages.Hotel);
                } else {
                    player.sendBlockNotification("Das Hotel konnte nicht gelöscht werden.", "Fehler");
                }
            }

            return true;
        }

        private bool onSupportCreateRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var hotel = (Hotel)data["Hotel"];

            var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;
            var levelEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var roomNumberEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var maxBookingEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var priceEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            try {
                var level = int.Parse(levelEvt.input);
                var roomNumber = int.Parse(roomNumberEvt.input);
                var maxBooking = int.Parse(maxBookingEvt.input);
                var price = decimal.Parse(priceEvt.input);

                player.sendNotification(Constants.NotifactionTypes.Info, $"Setze nun die Außenwände des Zimmers. Das ganze Zimmer muss enthalten sein, nicht jedoch der Gang!", "", Constants.NotifactionImages.Hotel);
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                    player.sendNotification(Constants.NotifactionTypes.Info, $"Setze nun die Haupttür des Raumes. Zusätzliche Türen können später gesetzt werden", "", Constants.NotifactionImages.Hotel);
                    DoorController.startDoorRegistrationWithCallback(player, getRoomDoorGroup(hotel, roomNumber), (d) => {
                        using(var db = new ChoiceVDb()) {
                            var newDbRoom = new confighotelroom {
                                hotelId = hotel.Id,
                                roomNumber = roomNumber,
                                maxBookings = maxBooking,
                                price = price,
                                collPosition = p.ToJson(),
                                collWidth = w,
                                collHeight = h,
                                collRotation = r,
                                door1Id = d.Id,
                            };

                            db.confighotelrooms.Add(newDbRoom);
                            db.SaveChanges();

                            hotel.Rooms.Add(new HotelRoom(newDbRoom.id, hotel, level, roomNumber, maxBooking, price,
                                CollisionShape.Create(p, w, h, r, true, false),
                                new List<Door> { d },
                                new List<HotelRoomFeature>()));
                            player.sendNotification(Constants.NotifactionTypes.Success, $"Raum wurde erfolgreich erstellt!", "", Constants.NotifactionImages.Hotel);
                        }
                    });
                });
            } catch(Exception) {
                player.sendBlockNotification("Eine der Eingaben war ungültig.", "Ungültige Eingabe", Constants.NotifactionImages.Hotel);
                return false;
            }

            return true;
        }


        private bool onSupportDeleteRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var hotel = (Hotel)data["Hotel"];
            var room = (HotelRoom)data["Room"];

            using(var db = new ChoiceVDb()) {
                var dbRoom = db.confighotelrooms.Find(room.Id);

                if(dbRoom != null) {
                    db.confighotelrooms.Remove(dbRoom);

                    db.SaveChanges();

                    hotel.Rooms.Remove(room);
                    room.Dispose();
                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Raum mit Nummer {room.RoomNumber} erfolgreich gelöscht", "", Constants.NotifactionImages.Hotel);
                } else {
                    player.sendBlockNotification("Das Hotel konnte nicht gelöscht werden.", "Fehler");
                }
            }

            return true;
        }

        private bool onSupportCreateFeature(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = (Type)data["Type"];
            var room = (HotelRoom)data["Room"];

            var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;
            var inputEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            evt.elements = evt.elements.Skip(1).ToArray();
            var settings = HotelRoomFeature.getCreateSettings(type, evt);

            player.sendNotification(Constants.NotifactionTypes.Info, "Erstelle nun die Kollision für das Feature", "");
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                using(var db = new ChoiceVDb()) {
                    var dbFeature = new confighotelroomfeature {
                        hotelRoomId = room.Id,
                        colPos = p.ToJson(),
                        colWidth = w,
                        colHeight = h,
                        colRotation = r,
                        codeInstance = type.Name,
                        name = inputEvt.input,
                        settings = settings.ToJson(),
                    };

                    db.confighotelroomfeatures.Add(dbFeature);
                    db.SaveChanges();

                    room.addFeature(new HotelRoomFeature(dbFeature));
                    player.sendNotification(Constants.NotifactionTypes.Success, $"{inputEvt.input} erfolgreich erstellt", "");
                }
            });

            return true;
        }

        #endregion
    }
}
