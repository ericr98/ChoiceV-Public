//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Numerics;
//using System.Security.Cryptography;
//using System.Text;
//using AltV.Net.Data;
//using ChoiceVDb = ChoiceVServer.Model.Database.ChoiceVDb;
//using Enum = System.Enum;
//using Type = System.Type;
//// ReSharper disable UnusedMember.Local
//// ReSharper disable UnusedMember.Global


//// ToDo: Neue Admin-Kommandos zur Registrierung von virtuellen Zimmern.

//namespace ChoiceVServer.Controller {
//    public class HotelCommandAttribute : Attribute {
//        public string Arguments;
//        public HotelCommandAttribute(string arguments) {
//            Arguments = arguments;
//        }
//    }

//    public enum HotelTypes {
//        MicroStay = 0,
//        Motel = 1,
//        ExtendedStay = 2,
//        LimitedServices = 3,
//        SelectService = 4,
//        FullService = 5,
//        LifestyleLuxury = 6,
//        InternationalLuxury = 7,
//        CapsuleHotel = 8,
//        LoveHotel = 9,
//        Apartments = 10,
//        Buildings = 11,
//    }

//    public enum RoomClasses {
//        Low = 0,
//        Medium = 1,
//        High = 2,
//        Superior = 3
//    }

//    public enum RoomTypes {
//        Apartment = 0,
//        Bungalow = 1,
//        SingleBedroom = 2,
//        DoubleBedroom = 3,
//        MultiBedroom = 4,
//        Cottage = 5,
//        JuniorSuite = 6,
//        Maisonette = 7,
//        Penthouse = 8,
//        Dormitory = 9,
//        Studio = 10,
//        Suite = 11
//    }

//    public enum HotelBuildingType {
//        NormalBuilding = 0,
//        TeleportBuilding = 1,
//    }

//    public enum RoomTypeIngame {
//        NormalRoom = 0,
//        TeleportRoom = 1,
//    }

//    public enum EvictionReason {
//        Eviction,
//        Canceling,
//        EndOfInterval
//    }

//    public class Hotel {
//        public Hotel(confighotelbuilding row) {
//            try {
//                Id = row.id;
//                Name = row.name;
//                if (row.hoteltype != null) HotelType = (HotelTypes)row.hoteltype;
//                OwnerId = row.ownerid;
//                IsAutomated = row.isautomated == 1;
//                HotelStars = row.hotelstars ?? 1;
//                DoorGroup = row.doorGroup;
//                DoorId = row.doorId;
//                IsLocked = row.islocked == 1;
//                LockReason = row.lockreason;
//                BankAccountId = row.bankAccountId;
//                DropoutPosition = row.dropout_position.FromJson();
//                PhoneNumber = row.phonenumber;
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public int Id { get; set; }
//        public string Name { get; set; }
//        public HotelTypes HotelType { get; set; }
//        public int? OwnerId { get; set; }
//        public bool IsAutomated { get; set; }
//        public int HotelStars { get; set; }
//        public string DoorGroup { get; set; }
//        public int? DoorId { get; set; }
//        public bool IsLocked { get; set; }
//        public string LockReason { get; set; }
//        public long BankAccountId { get; set; }
//        public long PhoneNumber { get; set; }
//        public Position DropoutPosition { get; set; }
//        public HotelBuildingType HotelBuildingType { get; set; }
//    }

//    public class HotelRoom {
//        public HotelRoom(confighotelroom row) {
//            try {
//                Id = row.id;
//                DoorId = row.doorId;
//                DoorGroup = row.doorGroup;
//                RoomType = (RoomTypes)row.roomtype;
//                RoomClass = (RoomClasses)row.roomclass;
//                RoomName = row.roomname;
//                IsLocked = row.islocked == 1;
//                LockReason = row.lockreason;
//                RoomTypeIngame = (RoomTypeIngame)row.ingameroomtype;
//                TeleportPosition = row.teleportposition.FromJson();
//                RoomAcceptsGuest = row.roomacceptsguests != 0;
//                Dimension = row.dimension ?? 0;
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public int Id { get; set; }
//        public string DoorGroup { get; set; }
//        public int? DoorId { get; set; }
//        public RoomTypes RoomType { get; set; }
//        public RoomClasses RoomClass { get; set; }
//        public string RoomName { get; set; }
//        public bool IsLocked { get; set; }
//        public string LockReason { get; set; }
//        public RoomTypeIngame RoomTypeIngame { get; set; }
//        public Position TeleportPosition { get; set; }
//        public bool RoomAcceptsGuest { get; set; }
//        public int Dimension { get; set; }
//    }

//    public class HotelRoomRate {
//        public HotelRoomRate(confighotelroomrate row) {
//            try {
//                Id = row.id;
//                HotelId = row.hotelid;
//                RoomType = (RoomTypes)row.roomtype;
//                RoomClass = (RoomClasses)row.roomclass;
//                Rate = row.rate;
//                RateTickInDays = row.ratetickindays;
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public int Id { get; set; }
//        public int HotelId { get; set; }
//        public RoomTypes RoomType { get; set; }
//        public RoomClasses RoomClass { get; set; }
//        public decimal Rate { get; set; }
//        public int RateTickInDays { get; set; }
//    }

//    public class HotelRoomRemoveTicket {
//        public int RoomId { get; set; }
//        public string PlayerAuthToken { get; set; }
//        public DateTime TicketTime { get; set; }
//    }

//    public class HotelRemoveTicket {
//        public int HotelId { get; set; }
//        public string PlayerAuthToken { get; set; }
//        public DateTime TicketTime { get; set; }
//    }

//    public class HotelTerminal {
//        public HotelTerminal(confighotelterminal row) {
//            try {
//                Hotel hotel = HotelController.AllHotels[row.hotelbuildingid];

//                Dictionary<string, dynamic> data = new Dictionary<string, dynamic> { { "terminal", row.id }, { "hotel", row.hotelbuildingid } };

//                CollisionShape shape = CollisionShape.Create(row.position.FromJson(), row.width,
//                    row.height, row.rotation, true, false, true, "USE_HOTELTERMINAL", data);

//                //Alle Shapes entfernen mit dem Placeholder-Terminal-Event an dieser Position.
//                CollisionShape.AllShapes.RemoveAll(cs =>
//                    cs.Interactable &&
//                    string.Equals(cs.EventName, "UseHotelTerminal", StringComparison.OrdinalIgnoreCase) &&
//                    Vector3.Distance(cs.Position, shape.Position) < 1);

//                Id = row.id;
//                CollisionShape = shape;
//                HotelBuilding = hotel;
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public CollisionShape CollisionShape { get; private set; }
//        public Hotel HotelBuilding { get; private set; }

//        public int Id { get; set; }
//    }

//    public class EvictionEntry {
//        public EvictionEntry(hotelevictionlog row) {
//            try {
//                Id = row.id;
//                EvictionDate = row.evictiondate;
//                BookingId = row.bookingid;
//                EvictedCharId = row.evictedchar;
//                Reason = row.reason;
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//        }

//        public int Id { get; set; }
//        public DateTime EvictionDate { get; set; }
//        public int BookingId { get; set; }
//        public int EvictedCharId { get; set; }
//        public string Reason { get; set; }
//    }

//    public class BookingEntry {
//        public BookingEntry(hotelbookings row) {
//            try {
//                Id = row.id;
//                RoomId = row.roomid;
//                GuestId = row.guestid;
//                StartDate = row.startdate;
//                EndDate = row.enddate;
//                CurrentRate = row.currentrate;
//                BookingEnded = row.bookingEnded != 0;
//                PhoneNumber = row.phoneNumber;
//                SocialSecurityNumber = row.socialSecurityNumber;
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public int Id { get; set; }
//        public int RoomId { get; set; }
//        public int GuestId { get; set; }
//        public DateTime StartDate { get; set; }
//        public DateTime? EndDate { get; set; }
//        public Decimal CurrentRate { get; set; }
//        public bool BookingEnded { get; set; }
//        public long PhoneNumber { get; set; }
//        public long SocialSecurityNumber { get; set; }
//    }

//    public class Assignment {
//        public int Hotel { get; set; }
//        public int Room { get; set; }
//    }

//    internal class HotelRoomTypeClassCombination {
//        public RoomTypes RoomType { get; set; }
//        public RoomClasses RoomClass { get; set; }
//    }

//    public static class HotelSettings {
//        private static decimal PenaltyChargeInternal;
//        private static decimal ReplaceKeyChargeInternal;
//        private static decimal ReplaceLockChargeInternal;
//        private static bool IsNotInitializedInternal = true;

//        private static void ensureInit() {
//            if (IsNotInitializedInternal) {
//                //EnsureDB-Properties, if not present, set default.
//                using (var db = new ChoiceVDb()) {
//                    bool doSaveDb = false;
//                    confighotelsettings penaltyChargeT =
//                        db.confighotelsettings.FirstOrDefault(s => s.settingname == "PenaltyCharge");
//                    confighotelsettings replaceKeyChargeT =
//                        db.confighotelsettings.FirstOrDefault(s => s.settingname == "ReplaceKeyCharge");
//                    confighotelsettings replaceLockChargeT =
//                        db.confighotelsettings.FirstOrDefault(s => s.settingname == "ReplaceLockCharge");

//                    if (penaltyChargeT == null) {
//                        penaltyChargeT = new confighotelsettings() { settingname = "PenaltyCharge", settingvalue = "1,5" };
//                        db.Add(penaltyChargeT);
//                        doSaveDb = true;
//                    }

//                    if (replaceKeyChargeT == null) {
//                        replaceKeyChargeT = new confighotelsettings() { settingname = "ReplaceKeyCharge", settingvalue = "100,00" };
//                        db.Add(replaceKeyChargeT);
//                        doSaveDb = true;
//                    }

//                    if (replaceLockChargeT == null) {
//                        replaceLockChargeT = new confighotelsettings() { settingname = "ReplaceLockCharge", settingvalue = "1000,00" };
//                        db.Add(replaceLockChargeT);
//                        doSaveDb = true;
//                    }

//                    if (doSaveDb) {
//                        db.SaveChanges();
//                    }

//                    PenaltyChargeInternal = decimal.Parse(penaltyChargeT.settingvalue);
//                    ReplaceKeyChargeInternal = decimal.Parse(replaceKeyChargeT.settingvalue);
//                    ReplaceLockChargeInternal = decimal.Parse(replaceLockChargeT.settingvalue);
//                }

//                IsNotInitializedInternal = false;
//            }
//        }

//        public static decimal PenaltyCharge { get { ensureInit(); return PenaltyChargeInternal; } }
//        public static decimal ReplaceKeyCharge { get { ensureInit(); return ReplaceKeyChargeInternal; } }
//        public static decimal ReplaceLockCharge { get { ensureInit(); return ReplaceLockChargeInternal; } }
//    }

//    public class HotelController : ChoiceVScript {
//        // ToDo:
//        // Offen & Wichtig:
//        // ======================
//        // Wohin mit dem Spieler bei einer Eviction (wenn noch im Raum)?

//        // Offenes für spätere Versionen:
//        // ======================
//        // Textnachrichten an Kunden in Json auslagern
//        // Korrekte Ansprache mit Herr / Frau
//        // Korrekte Formulierung mit Gebäude, Zimmer, Wohnung, Apartmentkomplex...

//        public static Dictionary<int, Hotel> AllHotels = new Dictionary<int, Hotel>();
//        public static Dictionary<int, HotelRoom> AllHotelRooms = new Dictionary<int, HotelRoom>();
//        public static Dictionary<int, HotelRoomRate> AllHotelRates = new Dictionary<int, HotelRoomRate>();
//        public static IList<Assignment> AllHotelAssignments = new List<Assignment>();
//        public static Dictionary<int, HotelTerminal> AllHotelTerminals = new Dictionary<int, HotelTerminal>();
//        public static Dictionary<int, EvictionEntry> AllEvictionEntries = new Dictionary<int, EvictionEntry>();
//        public static Dictionary<int, BookingEntry> AllBookingEntries = new Dictionary<int, BookingEntry>();
//        public static List<int> WarnedBookingEntries = new List<int>();

//        public static void initTimer() {
//            InvokeController.AddTimedInvoke("HotelValidation", onTimerCalled, TimeSpan.FromMinutes(5), true);
//        }

//        private static void onTimerCalled(IInvoke ivk) {
//            Stopwatch stopwatch = Stopwatch.StartNew();
//            try {
//                Logger.logTrace("HotelController: Räume Hotelbuchungen auf.");
//                List<int> endedBookingEntries;
//                lock (AllBookingEntries) {
//                    endedBookingEntries = AllBookingEntries.Where(b => !b.Value.BookingEnded && b.Value.EndDate != null && b.Value.EndDate.Value < DateTime.Now).Select(b => b.Key).ToList();
//                }

//                if (endedBookingEntries.Any()) {
//                    lock (AllBookingEntries) {
//                        foreach (int endedBookingEntry in endedBookingEntries) {
//                            evictHotelRoom(AllBookingEntries[endedBookingEntry].RoomId, AllBookingEntries[endedBookingEntry].GuestId, "Buchungsende", null);
//                        }
//                    }

//                    WarnedBookingEntries.RemoveAll(w => endedBookingEntries.Contains(w));
//                }

//                TimeSpan timespanWarningTime = TimeSpan.FromHours(2);
//                List<int> entriesToWarn = AllBookingEntries.Where(b => !b.Value.BookingEnded && b.Value.EndDate != null && !WarnedBookingEntries.Contains(b.Key) && (b.Value.EndDate.Value - DateTime.Now) < timespanWarningTime).Select(b => b.Key).ToList();

//                //Nicht mehr als 5 auf einmal warnen... könnte den Server sonst spammen!
//                if (entriesToWarn.Count > 5) {
//                    entriesToWarn = entriesToWarn.Take(5).ToList();
//                }

//                foreach (int bookingEntryToWarn in entriesToWarn) {
//                    int roomId = AllBookingEntries[bookingEntryToWarn].RoomId;
//                    Hotel hotel = getHotelToRoomId(roomId);
//                    sendTextMessageToPlayer(AllBookingEntries[bookingEntryToWarn].PhoneNumber, hotel, AllHotelRooms[roomId], $"Hinweis: Ihre Buchung bei {hotel.Name} endet in weniger als zwei Stunden. Wir würden uns freuen, wenn Sie uns länger erhalten bleiben und Ihre Buchung bei uns verlängern.");
//                    WarnedBookingEntries.Add(bookingEntryToWarn);
//                }
//                Logger.logTrace($"HotelController: Buchungen aufgeräumt. {endedBookingEntries.Count} Buchungen beendet, {entriesToWarn.Count} erhielten 2h-Warnung.");
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//            stopwatch.Stop();
//            Logger.logTrace($"HotelController: Dauer zum aufräumen => {stopwatch.ElapsedMilliseconds}ms");
//        }

//        private static characters getCharacterToId(int characterId) {
//            try {
//                if (characterId == -1) return null;

//                using (var db = new ChoiceVDb()) {
//                    return db.characters.FirstOrDefault(c => c.id == characterId);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//                return null;
//            }
//        }

//        private static characters getCurrentCharacterToPlayer(Player player) {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    return db.characters.FirstOrDefault(c => c.id == player.getCharacterId());
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//                return null;
//            }
//        }

//        public HotelController() {
//            try {
//                // Interaktions-Events
//                EventController.addEvent("REGISTER_HOTEL_DOOR", onRegisterHotelDoor);
//                EventController.addEvent("REGISTER_HOTEL_ROOM_DOOR", onRegisterHotelRoomDoor);

//                // Events mit Collision-Shapes
//                EventController.addCollisionShapeEvent("USE_HOTELTERMINAL", onUsingHotelTerminal);

//                // Menü-Events
//                EventController.addMenuEvent("HOTEL_BOOKINGLEFTDAYS", onMenuEventShowBookingLeftDays);
//                EventController.addMenuEvent("HOTEL_CANCELALLGUESTROOMS", onMenuEventCancelAllGuestRooms);
//                EventController.addMenuEvent("HOTEL_CANCELBOOKING", onMenuEventCancelBooking);
//                EventController.addMenuEvent("HOTEL_CREATESECONDROOMKEY", onMenuEventCreateSecondRoomKey);
//                EventController.addMenuEvent("HOTEL_CHANGEROOMLOCK", onMenuEventChangeRoomLock);
//                EventController.addMenuEvent("HOTEL_EVICTROOM", onMenuEventEvictRoom);
//                EventController.addMenuEvent("HOTEL_EXTENDBOOKING1", onMenuEventExtendedBooking1);
//                EventController.addMenuEvent("HOTEL_EXTENDBOOKING2", onMenuEventExtendedBooking2);
//                EventController.addMenuEvent("HOTEL_EXTENDBOOKING3", onMenuEventExtendedBooking3);
//                EventController.addMenuEvent("HOTEL_LOCKROOM", onMenuEventLockRoom);
//                EventController.addMenuEvent("HOTEL_LOCKHOTEL", onMenuEventLockHotel);
//                EventController.addMenuEvent("HOTEL_NEWBOOKING", onMenuEventNewBooking);
//                EventController.addMenuEvent("HOTEL_SHOWBOOKINGINFO", onMenuEventShowBookingInfo);
//                EventController.addMenuEvent("HOTEL_SHOWGUESTINFO", onMenuEventShowGuestInfo);
//                EventController.addMenuEvent("HOTEL_SHOWPRICING", onMenuEventShowPricing);
//                EventController.addMenuEvent("HOTEL_UNLOCKHOTEL", onMenuEventUnlockHotel);
//                EventController.addMenuEvent("HOTEL_UNLOCKROOM", onMenuEventUnlockRoom);
//                EventController.addMenuEvent("HOTEL_REG_ADD_ROOM", onMenuRegisterHotelRoomModeAddRoom);
//                EventController.addMenuEvent("HOTEL_ENTER_TELEPORT_ROOM", onMenuEnterTeleportRoom);
//                EventController.addMenuEvent("HOTEL_LEAVE_TELEPORT_ROOM", onMenuLeaveTeleportRoom);
//                EventController.addMenuEvent("HOTEL_SHOW_TELEPORT_ROOM_MENU", onMenuShowTeleportRoomMenu);
//                EventController.addMenuEvent("HOTEL_TOGGLE_TELEPORT_ROOM_GUESTSTATE", onMenuToggleTeleportRoomGuestState);

//                EventController.PlayerSuccessfullConnectionDelegate += loadCharacter;

//                using (var db = new ChoiceVDb()) {
//                    if (!db.phonenumbers.Any(p => p.number == 555555042210)) {
//                        phonenumbers dbNumber = new phonenumbers() { number = 555555042210, comment = "Hotelsystem" };
//                        db.Add(dbNumber);
//                        db.SaveChanges();
//                    }
//                }

//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private bool onMenuToggleTeleportRoomGuestState(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            try {
//                //Zimmer über Spieler-Dimension ermitteln.
//                if (player.Dimension < 10000000) return false;
//                int characterId = player.getCharacterId();
//                HotelRoom hotelRoom = AllHotelRooms.Values.FirstOrDefault(r => r.Dimension == player.Dimension);

//                if (hotelRoom == null || hotelRoom.RoomTypeIngame != RoomTypeIngame.TeleportRoom) {
//                    sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//                    return false;
//                }

//                Hotel hotel = getHotelToRoomId(hotelRoom.Id);

//                string roomType = "das Zimmer";
//                string roomType3 = "Zimmer";

//                if (hotel.HotelType == HotelTypes.Apartments) {
//                    roomType = "die Wohnung";
//                    roomType3 = "Wohnung";
//                } else if (hotel.HotelType == HotelTypes.Buildings) {
//                    roomType = "das Haus";
//                    roomType3 = "Haus";
//                }

//                if (AllBookingEntries.Values.Any(b =>
//                    !b.BookingEnded && b.RoomId == hotelRoom.Id && b.GuestId == characterId)) {
//                    using (var db = new ChoiceVDb()) {
//                        confighotelroom configHotelRoom = db.confighotelroom.FirstOrDefault(h => h.id == hotelRoom.Id);
//                        if (configHotelRoom == null) {
//                            sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//                            return false;
//                        }
//                        hotelRoom.RoomAcceptsGuest = !hotelRoom.RoomAcceptsGuest;
//                        configHotelRoom.roomacceptsguests = hotelRoom.RoomAcceptsGuest ? 1 : 0;
//                        db.SaveChanges();
//                        if (hotelRoom.RoomAcceptsGuest) {
//                            sendNotificationToPlayer(firstCharToUpper($"{roomType} wurde aufgesperrt. Du kannst nun Gäste empfangen."), $"{roomType3} aufgesperrt.", Constants.NotifactionTypes.Success, player);
//                            //ToDo: Sound spielen.
//                        } else {
//                            sendNotificationToPlayer(firstCharToUpper($"{roomType} wurde abgeschlossen. Gäste können nicht mehr empfangen werden."), $"{roomType3} abgeschlossen.", Constants.NotifactionTypes.Success, player);
//                            //ToDo: Sound spielen.
//                        }
//                    }
//                }
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//            }
//            return false;
//        }

//        public static bool onMenuShowTeleportRoomMenu(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                //Zimmer über Spieler-Dimension ermitteln.
//                if (player.Dimension < 10000000) return false;

//                HotelRoom hotelRoom = AllHotelRooms.Values.FirstOrDefault(r => r.Dimension == player.Dimension);

//                Menu menu = null;

//                if (hotelRoom == null || hotelRoom.RoomTypeIngame != RoomTypeIngame.TeleportRoom) {
//                    sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//                    return false;
//                }

//                Hotel hotel = getHotelToRoomId(hotelRoom.Id);

//                string roomType = "das Zimmer";
//                string roomType3 = "Zimmer";

//                if (hotel.HotelType == HotelTypes.Apartments) {
//                    roomType = "die Wohnung";
//                    roomType3 = "Wohnung";
//                } else if (hotel.HotelType == HotelTypes.Buildings) {
//                    roomType = "das Haus";
//                    roomType3 = "Haus";
//                }

//                menu = new Menu($"{roomType3} {hotelRoom.RoomName}", "Zimmerfunktionen");
//                menu.addMenuItem(new ClickMenuItem($"{roomType3} verlassen.", $"Du verlässt {roomType}.", string.Empty, "HOTEL_LEAVE_TELEPORT_ROOM", MenuItemStyle.green).withData(new Dictionary<string, dynamic>() { { "RoomId", hotelRoom.Id } }));

//                //int characterId = player.getCharacterId();
//                //if (AllBookingEntries.Values.Any(b =>
//                //    !b.BookingEnded && b.RoomId == hotelRoom.Id && b.GuestId == characterId)) {
//                //    if (hotelRoom.RoomAcceptsGuest) {
//                //        menu.addMenuItem(new ClickMenuItem($"{roomType3} schließen.", $"Andere können { roomType} nicht mehr betreten.", string.Empty, "HOTEL_TOGGLE_TELEPORT_ROOM_GUESTSTATE", MenuItemStyle.green).withData(new Dictionary<string, dynamic>() { { "RoomId", hotelRoom.Id } }));
//                //    } else {
//                //        menu.addMenuItem(new ClickMenuItem($"{roomType3} öffnen.", $"Andere können {roomType} betreten.", string.Empty, "HOTEL_TOGGLE_TELEPORT_ROOM_GUESTSTATE", MenuItemStyle.green).withData(new Dictionary<string, dynamic>() { { "RoomId", hotelRoom.Id } }));
//                //    }
//                //}
//                player.showMenu(menu);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//            }
//            return false;
//        }

//        private bool onMenuLeaveTeleportRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            // Raus geht immer, keine Prüfung :-)
//            try {
//                if (!data.ContainsKey("RoomId")) {
//                    sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//                    return false;
//                }
//                int roomId = (int)data["RoomId"];
//                Hotel hotel = getHotelToRoomId(roomId);
//                player.Dimension = 0;
//                player.SetPosition(hotel.DropoutPosition.X, hotel.DropoutPosition.Y, hotel.DropoutPosition.Z + 0.97f);
//                player.resetPermantData("BookingId");
//                player.setState(Constants.PlayerStates.None);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//            }
//            return false;
//        }

//        private bool onMenuEnterTeleportRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                if (!data.ContainsKey("RoomId")) {
//                    sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//                    return false;
//                }
//                int roomId = (int)data["RoomId"];

//                HotelRoom[] visitableHotelRoomsByInventory = getVisitableHotelRoomsByInventory(player.getCharacterId(), getHotelToRoomId(roomId), player);

//                HotelRoom hotelRoom = visitableHotelRoomsByInventory.FirstOrDefault(r => r.Id == roomId) ??
//                                      getVisitableHotelRoomsByInvite(getHotelToRoomId(roomId)).FirstOrDefault(r => r.Id == roomId);
//                BookingEntry bookingEntry = AllBookingEntries.Values.FirstOrDefault(b => !b.BookingEnded && b.RoomId == roomId);

//                if (hotelRoom == null || hotelRoom.RoomTypeIngame != RoomTypeIngame.TeleportRoom || bookingEntry == null) {
//                    sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//                    return false;
//                }

//                player.Dimension = hotelRoom.Dimension;
//                player.SetPosition(hotelRoom.TeleportPosition.X, hotelRoom.TeleportPosition.Y, hotelRoom.TeleportPosition.Z + 0.97f);
//                player.setPermanentData("BookingId", bookingEntry.Id.ToString());
//                player.setState(Constants.PlayerStates.Busy);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                sendNotificationToPlayer("Das hat leider nicht geklappt. Bitte probiere es noch einmal. Tritt dieses Problem wiederholt auf, wende dich bitte an den Support.", "Fehler beim Verlassen.", Constants.NotifactionTypes.Warning, player);
//            }
//            return false;
//        }

//        private bool onMenuRegisterHotelRoomModeAddRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            InputMenuItem.InputMenuItemEvent inputMenuItemEvent = (InputMenuItem.InputMenuItemEvent)menuItemCefEvent;
//            string room = inputMenuItemEvent.input;
//            int[] ints = InteractionController.hotelRegPlayerToHotel[player];
//            Hotel hotel = AllHotels[ints[0]];
//            RoomTypes roomType = (RoomTypes)ints[1];
//            RoomClasses roomClass = (RoomClasses)ints[2];
//            string modelHash = (string)data["modelHash"];
//            Position objectPosition = (Position)data["objectPosition"];

//            string doorGroup = $"{hotel.Name}_{room}".Replace(" ", "_").ToUpper();

//            using (var db = new ChoiceVDb()) {
//                if (!db.configdoorgroups.Any(g => string.Equals(g.identifier, doorGroup))) {
//                    var row = new configdoorgroups {
//                        identifier = doorGroup,
//                        description = $"Hoteltüren Zimmer {room} in {hotel.Name}",
//                        crackAble = 1
//                    };

//                    db.configdoorgroups.Add(row);
//                    db.SaveChanges();
//                    player.sendNotification(Constants.NotifactionTypes.Info, $"Türgruppe {doorGroup} registriert.", "Türgruppe", Constants.NotifactionImages.Hotel);
//                }

//                DoorController.Door hotelRoomDoor = DoorController.registerDoor(objectPosition, modelHash, doorGroup);
//                player.sendNotification(Constants.NotifactionTypes.Info, $"Tür registriert.", "Tür", Constants.NotifactionImages.Hotel);

//                int hotelRoomId = registerHotelRoom(room, roomType, roomClass, hotel.Id, player, doorGroup, hotelRoomDoor.Id);

//                if (hotelRoomId == -1) {
//                    return false;
//                }
//                player.sendNotification(Constants.NotifactionTypes.Info, $"Raum passt :-)", "Raum", Constants.NotifactionImages.Hotel);
//                sendNotificationToPlayer($"Für den Raum wurde nun die Tür-Id {hotelRoomDoor.Id} gesetzt.", "Tür für Raum gesetzt", Constants.NotifactionTypes.Success, player);

//                player.sendNotification(Constants.NotifactionTypes.Success, $"Registrierung hat geklappt!", "Raum registriert", Constants.NotifactionImages.Hotel);
//            }

//            return true;
//        }

//        private void loadCharacter(IPlayer player, characters character) {
//            if (player.hasData("BookingId")) {
//                int bookingId = int.Parse(player.getData("BookingId"));
//                if (bookingId <= 0) return;
//                if (!AllBookingEntries.ContainsKey(bookingId)) {
//                    Logger.logError("Fehler beim Laden des Spielers. Booking-ID ist ungültig.");
//                    // ToDo: Spieler an einen "Default" teleportieren. (Flughafen?)
//                    return;
//                }
//                BookingEntry bookingEntry = AllBookingEntries[bookingId];

//                HotelRoom hotelRoom = AllHotelRooms[bookingEntry.RoomId];

//                if (hotelRoom.RoomTypeIngame == RoomTypeIngame.NormalRoom) return;

//                if (bookingEntry.BookingEnded) {
//                    player.Dimension = 0;
//                    Position dropoutPosition = getHotelToRoomId(bookingEntry.RoomId).DropoutPosition;
//                    player.SetPosition(dropoutPosition.X, dropoutPosition.Y, dropoutPosition.Z + 0.97f);
//                    sendNotificationToPlayer("Deine Zimmerbuchung ist abgelaufen.", "Zimmerbuchung abgelaufen.", Constants.NotifactionTypes.Warning, player);
//                    return;
//                }

//                // Sicher gehen, dass Spieler an der richtigen Position & Dimension spawnt.
//                player.Dimension = hotelRoom.Dimension;
//                player.SetPosition(hotelRoom.TeleportPosition.X, hotelRoom.TeleportPosition.Y, hotelRoom.TeleportPosition.Z);
//            }
//        }

//        private bool onMenuEventChangeRoomLock(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                int listId = (int)data["guestBookingsListMenuItemId"];
//                int[] roomIds = (int[])data["roomIds"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-027.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-027: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                ListMenuItem.ListMenuItemEvent listMenuItemEvent = new ListMenuItem.ListMenuItemEvent();
//                bool elementFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != listId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, listMenuItemEvent);
//                        elementFound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-005.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-005: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                int roomId = roomIds[listMenuItemEvent.currentIndex];

//                Hotel hotel = getHotelToRoomId(roomId);

//                // Zu buchender Raum ermitteln.
//                HotelRoom roomForBooking = AllHotelRooms[roomId];

//                string roomType = "das Zimmer";

//                if (hotel.HotelType == HotelTypes.Apartments) {
//                    roomType = "die Wohnung";
//                } else if (hotel.HotelType == HotelTypes.Buildings) {
//                    roomType = "das Haus";
//                }


//                using (var db = new ChoiceVDb()) {
//                    //Bankkonto des Gasts ermitteln
//                    long bankAccount =
//                        long.Parse(db.characters.First(c => c.id == player.getCharacterId()).bankaccount);
//                    if (!BankController.accountIdExists(bankAccount)) {
//                        // Bannkonto existiert nicht!
//                        sendNotificationToPlayer(
//                            "Bitte versuchen Sie die Buchung erneut. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-029.",
//                            "Fehler bei der Buchung", Constants.NotifactionTypes.Danger, player);
//                        Logger.logException(new Exception($"HB-029: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                        return false;
//                    }

//                    //Kontostand prüfen.
//                    if (db.bankaccounts.First(b => b.id == bankAccount).balance < HotelSettings.ReplaceLockCharge) {
//                        sendNotificationToPlayer(
//                            "Fehler bei der Buchung. Leider ist Ihr persönliches Bankkonto nicht ausreichend gedeckt.",
//                            "Zu wenig Geld auf Konto", Constants.NotifactionTypes.Warning, player);
//                        return false;
//                    }

//                    //Geld transferieren
//                    if (!BankController.transferMoney(bankAccount, hotel.BankAccountId, HotelSettings.ReplaceLockCharge,
//                        $"Zweitschlüssel bei {hotel.Name} für {roomType} {roomForBooking.RoomName}.", player)
//                    ) {
//                        sendNotificationToPlayer(
//                            "Fehler bei der Buchung. Die Transaktion wurde durch die Bank abgebrochen. Versuchen Sie es bitte erneut oder wenden Sie sich an Ihre Bank.",
//                            "Transaktionsfehler", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }
//                }

//                // Tür / Türgruppe des Raums ermitteln (für Schlüssel)
//                DoorController.Door door;
//                int doorId = -1;
//                string doorGroup = "";

//                if (roomForBooking.DoorId.HasValue) {
//                    door = DoorController.AllDoors.Values.First(d => d.Id == roomForBooking.DoorId.Value);
//                    doorId = roomForBooking.DoorId.Value;
//                } else {
//                    door = DoorController.AllDoors.Values.First(d => d.GroupName == roomForBooking.DoorGroup);
//                    doorGroup = roomForBooking.DoorGroup;
//                }

//                //Schloss tauschen.
//                DoorController.ChangeLockOnDoor(door);

//                // Schlüssel anfertigen und dem Gast übergeben.
//                configitems item = InventoryController.AllConfigItems.First(i => string.Equals(i.codeItem, "DoorKey", StringComparison.InvariantCulture));
//                player.getInventory().addItem(new DoorKey(item, door.LockIndex, doorGroup, doorId, $"Schlüssel für {roomType} {roomForBooking.RoomName} in {hotel.Name}."));

//                sendNotificationToPlayer(
//                    $"Schloss getauscht und Schlüssel für {roomType} {roomForBooking.RoomName} erhalten.",
//                    "Schloss getauscht.", Constants.NotifactionTypes.Success, player);

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventCreateSecondRoomKey(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                int listId = (int)data["guestBookingsListMenuItemId"];
//                int[] roomIds = (int[])data["roomIds"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-028.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-028: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                ListMenuItem.ListMenuItemEvent listMenuItemEvent = new ListMenuItem.ListMenuItemEvent();
//                bool elementFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != listId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, listMenuItemEvent);
//                        elementFound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-025.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-025: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                int roomId = roomIds[listMenuItemEvent.currentIndex];

//                Hotel hotel = getHotelToRoomId(roomId);

//                // Zu buchender Raum ermitteln.
//                HotelRoom roomForBooking = AllHotelRooms[roomId];

//                string roomType = "das Zimmer";

//                if (hotel.HotelType == HotelTypes.Apartments) {
//                    roomType = "die Wohnung";
//                } else if (hotel.HotelType == HotelTypes.Buildings) {
//                    roomType = "das Haus";
//                }


//                using (var db = new ChoiceVDb()) {
//                    //Bankkonto des Gasts ermitteln
//                    long bankAccount =
//                        long.Parse(db.characters.First(c => c.id == player.getCharacterId()).bankaccount);
//                    if (!BankController.accountIdExists(bankAccount)) {
//                        // Bannkonto existiert nicht!
//                        sendNotificationToPlayer(
//                            "Bitte versuchen Sie die Buchung erneut. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-030.",
//                            "Fehler bei der Buchung", Constants.NotifactionTypes.Danger, player);
//                        Logger.logException(new Exception($"HB-030: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                        return false;
//                    }

//                    //Kontostand prüfen.
//                    if (db.bankaccounts.First(b => b.id == bankAccount).balance < HotelSettings.ReplaceKeyCharge) {
//                        sendNotificationToPlayer(
//                            "Fehler bei der Buchung. Leider ist Ihr persönliches Bankkonto nicht ausreichend gedeckt.",
//                            "Zu wenig Geld auf Konto", Constants.NotifactionTypes.Warning, player);
//                        return false;
//                    }

//                    //Geld transferieren
//                    if (!BankController.transferMoney(bankAccount, hotel.BankAccountId, HotelSettings.ReplaceKeyCharge,
//                        $"Zweitschlüssel bei {hotel.Name} für {roomType} {roomForBooking.RoomName}.", player)
//                    ) {
//                        sendNotificationToPlayer(
//                            "Fehler bei der Buchung. Die Transaktion wurde durch die Bank abgebrochen. Versuchen Sie es bitte erneut oder wenden Sie sich an Ihre Bank.",
//                            "Transaktionsfehler", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }
//                }

//                // Tür / Türgruppe des Raums ermitteln (für Schlüssel)
//                DoorController.Door door;
//                int doorId = -1;
//                string doorGroup = "";

//                if (roomForBooking.DoorId.HasValue) {
//                    door = DoorController.AllDoors.Values.First(d => d.Id == roomForBooking.DoorId.Value);
//                    doorId = roomForBooking.DoorId.Value;
//                } else {
//                    door = DoorController.AllDoors.Values.First(d => d.GroupName == roomForBooking.DoorGroup);
//                    doorGroup = roomForBooking.DoorGroup;
//                }

//                // Schlüssel anfertigen und dem Gast übergeben.
//                configitems item = InventoryController.AllConfigItems.First(i => string.Equals(i.codeItem, "DoorKey", StringComparison.InvariantCulture));
//                player.getInventory().addItem(new DoorKey(item, door.LockIndex, doorGroup, doorId, $"Schlüssel für {roomType} {roomForBooking.RoomName} in {hotel.Name}."));

//                sendNotificationToPlayer(
//                    $"Zweitschlüssel für {roomType} {roomForBooking.RoomName} erhalten.",
//                    "Zweitschlüssel erhalten.", Constants.NotifactionTypes.Success, player);

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        internal static bool tryGetHotelByString(in string hotel, out int hotelId) {
//            hotelId = -1;
//            try {
//                if (int.TryParse(hotel, out hotelId) && AllHotels.ContainsKey(hotelId)) {
//                    return true;
//                }

//                string hotelTemp = hotel; //Wird gebraucht wegen Lambda-Expression.
//                Hotel match = AllHotels.Values.FirstOrDefault(h =>
//                    string.Equals(h.Name, hotelTemp, StringComparison.OrdinalIgnoreCase));

//                if (match == null) {
//                    return false;
//                }

//                hotelId = match.Id;
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        internal static void sendNotificationToPlayer(string text, string shortText, Constants.NotifactionTypes notificationType, IPlayer player) {
//            try {
//                Logger.logDebug($"{shortText}\t{notificationType} {text}");
//                if (player == null) {
//                    return;
//                }
//                player.sendNotification(notificationType, text, shortText, Constants.NotifactionImages.Hotel);

//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void sendTextMessageToPlayer(long phoneNumber, Hotel hotel, HotelRoom hotelRoom, string text, IDictionary<string, string> keyValueReplacement = null) {
//            if (phoneNumber <= 0) { return; }

//            text = text.Replace("[hotelName]", hotel.Name, StringComparison.OrdinalIgnoreCase);
//            text = text.Replace("[roomName]", hotelRoom.RoomName, StringComparison.OrdinalIgnoreCase);
//            if (keyValueReplacement != null) {
//                foreach (KeyValuePair<string, string> keyValuePair in keyValueReplacement) {
//                    text = text.Replace($"{{{keyValuePair.Key}}}", keyValuePair.Value, StringComparison.OrdinalIgnoreCase);
//                }
//            }

//            long hotelNumber = hotel.PhoneNumber;
//            if (hotelNumber == -1) { hotelNumber = 555555042210; }

//            PhoneController.sendSMSToNumber(hotelNumber, phoneNumber, text);
//        }

//        internal bool onRegisterHotelRoomDoor(IPlayer player, string eventName, object[] args) {
//            try {
//                var hash = args[3].ToString();
//                var hotelRoomId = args[4].ToString();

//                if (int.TryParse(hotelRoomId, out var hotelRoomIdI) && AllHotelRooms.ContainsKey(hotelRoomIdI)) {
//                    HotelRoom hotelRoom = AllHotelRooms[hotelRoomIdI];
//                    DoorController.Door hotelRoomDoor = DoorController.AllDoors.Values.FirstOrDefault(door => string.Equals(door.ModelHash, hash, StringComparison.OrdinalIgnoreCase));
//                    if (hotelRoomDoor == null) {
//                        sendNotificationToPlayer($"Fehler: Gewähltes Objekt ist keine Tür / keine registrierte Tür.", "Element ist keine Tür.", Constants.NotifactionTypes.Warning, player);
//                        return true;
//                    }
//                    using (var db = new ChoiceVDb()) {
//                        confighotelroom configHotelRoom = db.confighotelroom.FirstOrDefault(c => c.id == hotelRoomIdI);
//                        if (hotelRoomDoor.GroupName == null) {
//                            hotelRoom.DoorGroup = hotelRoomDoor.GroupName;
//                            hotelRoom.DoorId = null;
//                            if (configHotelRoom != null) {
//                                configHotelRoom.doorGroup = hotelRoomDoor.GroupName;
//                                configHotelRoom.doorId = null;
//                            }
//                            sendNotificationToPlayer($"Für den Raum wurde nun die Türgruppe {hotelRoom.DoorGroup} gesetzt.", "Tür für Raum gesetzt", Constants.NotifactionTypes.Success, player);
//                        } else {
//                            hotelRoom.DoorGroup = null;
//                            hotelRoom.DoorId = hotelRoomDoor.Id;
//                            if (configHotelRoom != null) {
//                                configHotelRoom.doorGroup = null;
//                                configHotelRoom.doorId = hotelRoomDoor.Id;
//                            }
//                            sendNotificationToPlayer($"Für den Raum wurde nun die Tür-Id {hotelRoom.Id} gesetzt.", "Tür für Raum gesetzt", Constants.NotifactionTypes.Success, player);
//                        }
//                        db.SaveChanges();
//                        player.resetData("REGISTER_HOTEL_ROOM_DOOR_MODE");
//                        player.emitClientEvent("REGISTER_HOTEL_ROOM_DOOR_MODE", false, "");
//                        HotelController.sendNotificationToPlayer("Registrier-Modus für Hotel-Raum deaktiviert!", "Raum-Tür-Registriermodus beendet.",
//                            Constants.NotifactionTypes.Success, player);
//                    }
//                } else {
//                    sendNotificationToPlayer($"Fehler: Raum-ID ist ungültig. Gesendeter Wert: {hotelRoomId}", "Raum ungültig.", Constants.NotifactionTypes.Warning, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return true;
//        }

//        internal bool onRegisterHotelDoor(IPlayer player, string eventName, object[] args) {
//            try {
//                var hash = args[3].ToString();
//                var hotelId = args[4].ToString();

//                if (int.TryParse(hotelId, out var hotelIdI) && AllHotels.ContainsKey(hotelIdI)) {
//                    Hotel hotel = AllHotels[hotelIdI];
//                    DoorController.Door hotelDoor = DoorController.AllDoors.Values.FirstOrDefault(door => string.Equals(door.ModelHash, hash, StringComparison.OrdinalIgnoreCase));
//                    if (hotelDoor == null) {
//                        sendNotificationToPlayer($"Fehler: Gewähltes Objekt ist keine Tür / keine registrierte Tür.", "Element ist keine Tür.", Constants.NotifactionTypes.Warning, player);
//                        return true;
//                    }
//                    using (var db = new ChoiceVDb()) {
//                        confighotelbuilding configHotel = db.confighotelbuilding.FirstOrDefault(c => c.id == hotelIdI);
//                        if (hotelDoor.GroupName == null) {
//                            hotel.DoorGroup = hotelDoor.GroupName;
//                            hotel.DoorId = null;
//                            if (configHotel != null) {
//                                configHotel.doorGroup = hotelDoor.GroupName;
//                                configHotel.doorId = null;
//                            }

//                            sendNotificationToPlayer($"Für das Hotel wurde nun die Türgruppe {hotel.DoorGroup} gesetzt.", "Hotel für Raum gesetzt.", Constants.NotifactionTypes.Success, player);

//                        } else {
//                            hotel.DoorGroup = null;
//                            hotel.DoorId = hotelDoor.Id;
//                            if (configHotel != null) {
//                                configHotel.doorGroup = null;
//                                configHotel.doorId = hotelDoor.Id;
//                            }

//                            sendNotificationToPlayer($"Für das Hotel wurde nun die Tür-Id {hotel.Id} gesetzt.", "Hotel für Raum gesetzt.", Constants.NotifactionTypes.Success, player);
//                        }
//                        db.SaveChanges();
//                        player.resetData("REGISTER_HOTEL_DOOR_MODE");
//                        player.emitClientEvent("REGISTER_HOTEL_DOOR_MODE", false, "");
//                        sendNotificationToPlayer("Hotel-Tür-Registrier-Modus deaktiviert!", "Hotel-Tür-Registrier-Modus deaktiviert",
//                            Constants.NotifactionTypes.Info, player);
//                    }
//                } else {
//                    sendNotificationToPlayer($"Fehler: Hotel-ID ist ungültig. Gesendeter Wert: {hotelId}", "Hotel ungültig.", Constants.NotifactionTypes.Warning, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return true;
//        }

//        private bool onUsingHotelTerminal(IPlayer player, CollisionShape collisionShape, Dictionary<string, object> data) {

//            try {
//                int hotelId = -1;
//                if (data.ContainsKey("hotel")) {
//                    hotelId = (int)data["hotel"];
//                }
//                if (data.ContainsKey("AdditionalInfo")) {
//                    hotelId = int.Parse(data["AdditionalInfo"].ToString() ?? throw new InvalidOperationException());
//                }
//                showHotelBookingMenu(hotelId, player);
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }

//            return true;
//        }

//        internal static bool isDateNullOrFuture(DateTime? date) {
//            return (!date.HasValue || date.Value > DateTime.Now);
//        }

//        internal static int registerHotel(string hotelName, HotelTypes hotelType, int hotelStars, long hotelBankAccount, int? charOwnerId, IPlayer player) {
//            try {
//                if (AllHotels.Values.Any(h => string.Equals(h.Name, hotelName, StringComparison.OrdinalIgnoreCase))) {
//                    sendNotificationToPlayer($"Es besteht bereits ein Hotel mit dem Namen {hotelName}!", "Hotelname bereits vorhanden.", Constants.NotifactionTypes.Warning, player);
//                    return -1;
//                }
//                using (var db = new ChoiceVDb()) {
//                    confighotelbuilding hotelBuilding = new confighotelbuilding() { name = hotelName.Truncate(255), hoteltype = (int)hotelType, hotelstars = hotelStars, ownerid = charOwnerId, bankAccountId = hotelBankAccount };
//                    db.Add(hotelBuilding);
//                    db.SaveChanges();
//                    AllHotels.Add(hotelBuilding.id, new Hotel(hotelBuilding));
//                    sendNotificationToPlayer($"Hotel erfolgreich registriert. Hotel-ID: {hotelBuilding.id}", "Hotel registriert.", Constants.NotifactionTypes.Success, player);
//                    return hotelBuilding.id;
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//                return -1;
//            }
//        }

//        internal static bool getHotelId(IPlayer player) {
//            try {
//                HotelTerminal hotelTerminal = null;

//                IList<CollisionShape> collisionShapes = player.getCurrentCollisionShapes();
//                if (collisionShapes != null) {
//                    hotelTerminal = AllHotelTerminals.FirstOrDefault(e =>
//                        collisionShapes.Any(f => f.Position == e.Value.CollisionShape.Position)).Value;
//                }
//                if (hotelTerminal != null) {
//                    sendNotificationToPlayer($"Hotel gefunden! Hotel-ID: {hotelTerminal.HotelBuilding.Id}", $"Hotel gefunden ({hotelTerminal.HotelBuilding.Id})", Constants.NotifactionTypes.Success, player);
//                } else {
//                    sendNotificationToPlayer($"Kein Hotel gefunden. Stehen Sie im Collisionshape des Terminals!", "Kein Hotel gefunden.", Constants.NotifactionTypes.Success, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//            return true;
//        }

//        internal static int registerHotelRoom(string roomName, RoomTypes roomType, RoomClasses roomClass, int hotelId, IPlayer player, string doorGroup = null, int doorId = -1, RoomTypeIngame roomTypeIngame = RoomTypeIngame.NormalRoom, int dimension = 0) {
//            try {
//                IList<HotelRoom> matchesRoomNames = AllHotelRooms.Values
//                    .Where(hr => string.Equals(hr.RoomName, roomName, StringComparison.OrdinalIgnoreCase)).ToList();

//                var assignment = AllHotelAssignments.FirstOrDefault(ha =>
//                    ha.Hotel == hotelId && matchesRoomNames.Any(mrn => mrn.Id == ha.Room));
//                if (assignment != null) {
//                    sendNotificationToPlayer($"Der Raumname \"{roomName}\" ist bereits zum Hotel registriert. Der Name muss eindeutig sein.", "Raumname nicht eindeutig.", Constants.NotifactionTypes.Warning, player);
//                    return assignment.Room;
//                }

//                using (var db = new ChoiceVDb()) {
//                    confighotelroom hotelRoom = new confighotelroom() { roomname = roomName.Truncate(255), roomclass = (int)roomClass, roomtype = (int)roomType, doorGroup = doorGroup, doorId = doorId, ingameroomtype = (int)roomTypeIngame, dimension = dimension };
//                    if (roomTypeIngame == RoomTypeIngame.TeleportRoom) {
//                        hotelRoom.teleportposition = player.getCharacterData().LastPosition.ToJson();
//                    }
//                    db.Add(hotelRoom);
//                    db.SaveChanges();
//                    AllHotelRooms.Add(hotelRoom.id, new HotelRoom(hotelRoom));
//                    sendNotificationToPlayer($"Hotelraum erfolgreich registriert. Raum-ID: {hotelRoom.id}", "Raum registriert.", Constants.NotifactionTypes.Success, player);
//                    confighotelroomassignment hotelRoomAssignment = new confighotelroomassignment() { buildingid = hotelId, roomid = hotelRoom.id };
//                    db.Add(hotelRoomAssignment);
//                    db.SaveChanges();
//                    AllHotelAssignments.Add(new Assignment() { Hotel = hotelRoomAssignment.buildingid, Room = hotelRoomAssignment.roomid });
//                    sendNotificationToPlayer($"Hotelraum #{hotelRoom.id} erfolgreich mit Hotel #{hotelId} verknüpft.", "Raum verknüpft.", Constants.NotifactionTypes.Success, player);
//                    return hotelRoom.id;
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//                return -1;
//            }
//        }

//        internal static bool removeHotel(int hotelId, IPlayer player) {
//            try {
//                Hotel removedHotel;

//                // 1. Alle Gäste rausschmeissen (evictHotelRoom)
//                // 2. Alle Zimmer entfernen (removeHotelRoom)
//                IList<Assignment> hotelAssignments = AllHotelAssignments.Where(h => h.Hotel == hotelId).ToList();

//                foreach (Assignment hotelAssignment in hotelAssignments) {
//                    removeHotelRoom(hotelAssignment.Room, player);
//                }

//                using (var db = new ChoiceVDb()) {
//                    // 3. Hotelgebühren auflösen

//                    List<confighotelroomrate> configHotelRoomRates = db.confighotelroomrate.Where(h => h.hotelid == hotelId).ToList();
//                    db.confighotelroomrate.RemoveRange(configHotelRoomRates);
//                    AllHotelRates.RemoveWhere(h => h.Value.HotelId == hotelId);

//                    // 4. HotelTerminals auflösen
//                    List<confighotelterminal> confighotelterminals = db.confighotelterminal.Where(h => h.hotelbuildingid == hotelId).ToList();
//                    db.confighotelterminal.RemoveRange(confighotelterminals);
//                    AllHotelTerminals.RemoveWhere(h => h.Value.HotelBuilding.Id == hotelId);

//                    // 5. Hotel entfernen
//                    confighotelbuilding hotelBuilding = db.confighotelbuilding.FirstOrDefault(h => h.id == hotelId);
//                    if (hotelBuilding != null) {
//                        db.confighotelbuilding.Remove(hotelBuilding);
//                    }

//                    removedHotel = AllHotels[hotelId];

//                    AllHotels.Remove(hotelId);

//                    db.SaveChanges();
//                }
//                sendNotificationToPlayer(
//                    $"Hotel {removedHotel.Name}, #{removedHotel.Id} erfolgreich entfernt.", "Hotel entfernt.",
//                    Constants.NotifactionTypes.Success, player);

//            } catch (Exception e) {
//                Logger.logException(e);
//            }

//            return false;
//        }

//        internal static void removeHotelRoom(int roomId, IPlayer player) {
//            try {
//                // 1. Alle Gäste rausschmeissen (evictHotelRoom)
//                BookingEntry bookingEntry =
//                    AllBookingEntries.Values.FirstOrDefault(abe => abe.RoomId == roomId && !abe.BookingEnded);
//                if (bookingEntry != null) {
//                    evictHotelRoom(roomId, bookingEntry.GuestId, "Wartungsarbeiten an der Wohneinheit durch ChoiceV inc.",
//                        player);
//                }

//                using (var db = new ChoiceVDb()) {
//                    // 2. Zuordnung entfernen
//                    confighotelroomassignment configHotelRoomAssignment =
//                        db.confighotelroomassignment.FirstOrDefault(c => c.roomid == roomId);

//                    if (configHotelRoomAssignment != null) {
//                        db.confighotelroomassignment.Remove(configHotelRoomAssignment);
//                        Assignment assignment = AllHotelAssignments.FirstOrDefault(h => h.Room == roomId);
//                        if (assignment != null) {
//                            AllHotelAssignments.Remove(assignment);
//                        }
//                    } else {
//                        sendNotificationToPlayer($"Hotelzimmer mit Id #{roomId} war keinem Hotel zugeordnet.", "Zimmer ohne Hotelzuordnung!",
//                            Constants.NotifactionTypes.Warning, player);
//                    }

//                    // 3. Zimmer entfernen
//                    confighotelroom configHotelRoom = db.confighotelroom.FirstOrDefault(c => c.id == roomId);
//                    if (configHotelRoom == null) {
//                        sendNotificationToPlayer($"FEHLER IN DATENBANK! Hotelraum konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId}", "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                        throw new Exception($"FEHLER IN DATENBANK! Hotelraum konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId}");
//                    }

//                    db.confighotelroom.Remove(configHotelRoom);
//                    AllHotelRooms.Remove(roomId);

//                    db.SaveChanges();

//                    sendNotificationToPlayer(
//                        $"Hotelzimmer {configHotelRoom.roomname}, #{roomId} erfolgreich entfernt.", "Hotelzimmer gelöscht.",
//                        Constants.NotifactionTypes.Success, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void evictHotelRoom(int roomId, int characterId, string reason, IPlayer player, EvictionReason evictionReason = EvictionReason.Eviction) {
//            try {
//                if (AllHotelRooms.ContainsKey(roomId)) {
//                    BookingEntry bookingEntry =
//                        AllBookingEntries.Values.FirstOrDefault(abe => abe.RoomId == roomId && !abe.BookingEnded);
//                    {
//                        if (bookingEntry != null) {
//                            using (var db = new ChoiceVDb()) {
//                                hotelbookings hotelBooking =
//                                    db.hotelbookings.FirstOrDefault(hb => hb.id == bookingEntry.Id);
//                                if (hotelBooking != null) {
//                                    hotelBooking.enddate = DateTime.Now;
//                                    hotelBooking.bookingEnded = 1;
//                                    db.SaveChanges();
//                                    bookingEntry.EndDate = hotelBooking.enddate;
//                                    bookingEntry.BookingEnded = true;
//                                } else {
//                                    sendNotificationToPlayer(
//                                        $"FEHLER IN DATENBANK! Buchung konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId} Buchungs-ID:{bookingEntry.Id}", "Datenbankfehler",
//                                        Constants.NotifactionTypes.Danger, player);
//                                    throw new Exception(
//                                        $"FEHLER IN DATENBANK! Buchung konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId} Buchungs-ID:{bookingEntry.Id}");
//                                }
//                            }

//                            invalidateHotelRoomKeys(AllHotelRooms[roomId], player);

//                            if (bookingEntry.EndDate != null) {
//                                TimeSpan timeSpan = (bookingEntry.EndDate.Value - bookingEntry.StartDate) - (DateTime.Now - bookingEntry.StartDate);
//                                int repayingDays = timeSpan.Days;
//                                decimal repayAmount = bookingEntry.CurrentRate * repayingDays;
//                                Hotel hotel = getHotelToRoomId(bookingEntry.RoomId);

//                                using (var db = new ChoiceVDb()) {

//                                    long bankAccount =
//                                        long.Parse(db.characters.First(c => c.id == characterId).bankaccount);
//                                    if (!BankController.accountIdExists(bankAccount)) {
//                                        // Bannkonto existiert nicht!
//                                        sendNotificationToPlayer(
//                                            "Rückzahlung fehlgeschlagen.",
//                                            "Rückzahlung fehlgeschlagen.", Constants.NotifactionTypes.Danger, player);
//                                        return;
//                                    }
//                                    BankController.transferMoney(hotel.BankAccountId, bankAccount, repayAmount, $"Rückzahlung durch Stornierung bei {hotel.Name}.", null);
//                                }
//                            }

//                            HotelRoom hotelRoom = AllHotelRooms[bookingEntry.RoomId];
//                            List<DoorController.Door> doors = DoorController.AllDoors.Values.Where(d =>
//                                    (hotelRoom.DoorId.HasValue && hotelRoom.DoorId.Value == d.Id) ||
//                                    (string.Equals(hotelRoom.DoorGroup, d.GroupName,
//                                        StringComparison.OrdinalIgnoreCase)))
//                                .ToList();

//                            foreach (DoorController.Door door in doors) {
//                                door.lockDoor();
//                                door.changeLock();
//                            }

//                            string textMessage = $@"Sehr geehrte Kundin / sehr geehrter Kunde, Sie wurden automatisch aus [roomName] ausgebucht. Grund: {reason} Wir bedauern evtl. dadurch entstandene Unannehmlichkeiten und würden uns freuen, sie bald wieder bei uns begrüßen zu dürfen.  Mit freundlichen Grüßen Hotelmanagement [hotelName]";
//                            if (evictionReason == EvictionReason.Canceling) {
//                                textMessage = $@"Sehr geehrte Kundin / sehr geehrter Kunde, Sie wurden nun aus [roomName] ausgebucht. Grund: {reason} Wir würden uns freuen, sie bald wieder bei uns begrüßen zu dürfen.  Mit freundlichen Grüßen Hotelmanagement [hotelName]";
//                            }
//                            if (evictionReason == EvictionReason.EndOfInterval) {
//                                textMessage = $@"Sehr geehrte Kundin / sehr geehrter Kunde, Herzlichen Dank für Ihre Buchung. Da diese nun abgelaufen ist, wurden aus [roomName] ausgebucht. Wir würden uns freuen, sie bald wieder bei uns begrüßen zu dürfen.  Mit freundlichen Grüßen Hotelmanagement [hotelName]";
//                            }
//                            sendTextMessageToPlayer(bookingEntry.PhoneNumber, getHotelToRoomId(roomId), AllHotelRooms[roomId], textMessage);

//                        }
//                    }
//                } else {
//                    sendNotificationToPlayer($"Fehler: Hotelraum mit der ID #{roomId} ist nicht vorhanden.", "Hotelraum nicht gefunden.",
//                        Constants.NotifactionTypes.Warning, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void invalidateHotelRoomKeys(HotelRoom hotelRoom, IPlayer player) {
//            try {
//                if (hotelRoom.DoorId.HasValue) {
//                    DoorController.ChangeLockOnDoor(hotelRoom.DoorId.Value);
//                } else if (!string.IsNullOrEmpty(hotelRoom.DoorGroup)) {
//                    DoorController.ChangeLockOnDoor(hotelRoom.DoorGroup);
//                } else {
//                    sendNotificationToPlayer($"Der Raum {hotelRoom.RoomName}, #{hotelRoom.Id} hat keine registrierte Tür!", "Raum hat keine Tür!", Constants.NotifactionTypes.Warning, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static Hotel getHotelToRoomId(int roomId) {
//            try {
//                Assignment assignment = AllHotelAssignments.FirstOrDefault(aha => aha.Room == roomId);
//                if (assignment == null) {
//                    return null;
//                }

//                Hotel hotel = AllHotels[assignment.Hotel];
//                return hotel;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return null;
//            }
//        }

//        internal static bool setRoomRate(int hotelId, RoomTypes roomType, RoomClasses roomClass, decimal roomRate, int rateTick, IPlayer player) {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    //Gibt's die Rate bereits? Wenn ja, überschreiben.
//                    HotelRoomRate hotelRoomRate = AllHotelRates.Values.FirstOrDefault(hr => hr.HotelId == hotelId && hr.RoomClass == roomClass && hr.RoomType == roomType);

//                    if (hotelRoomRate == null) {
//                        confighotelroomrate newRoomRate = new confighotelroomrate() {
//                            hotelid = hotelId,
//                            rate = roomRate,
//                            ratetickindays = rateTick,
//                            roomclass = (int)roomClass,
//                            roomtype = (int)roomType
//                        };
//                        db.confighotelroomrate.Add(newRoomRate);
//                        db.SaveChanges();
//                        hotelRoomRate = new HotelRoomRate(newRoomRate);
//                        AllHotelRates.Add(hotelRoomRate.Id, hotelRoomRate);
//                    } else {
//                        hotelRoomRate.Rate = roomRate;
//                        confighotelroomrate changedRate =
//                            db.confighotelroomrate.FirstOrDefault(r => r.id == hotelRoomRate.Id);
//                        if (changedRate == null) {
//                            sendNotificationToPlayer($"FEHLER IN DATENBANK! Raum-Gebühr konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId} Gebühr-Id:{hotelRoomRate.Id}", "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                            throw new Exception($"FEHLER IN DATENBANK! Raum-Gebühr konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId} Gebühr-Id:{hotelRoomRate.Id}");
//                        }

//                        changedRate.rate = roomRate;
//                        db.SaveChanges();
//                    }
//                    sendNotificationToPlayer($"Gebühr wurde erfolgreich auf {roomRate} angepasst", "Gebühr angepasst.", Constants.NotifactionTypes.Success, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//            return true;
//        }

//        internal static void setHotelOwner(int hotelId, int charId, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];

//                using (var db = new ChoiceVDb()) {
//                    characters newOwnerCharacter = db.characters.FirstOrDefault(c => c.id == charId);

//                    if (newOwnerCharacter == null) {
//                        sendNotificationToPlayer($"Es konnte kein Spieler mit der ID #{charId} gefunden werden.", "Spieler nicht gefunden.", Constants.NotifactionTypes.Warning, player);
//                        return;
//                    }
//                    hotel.OwnerId = charId;

//                    confighotelbuilding holteBuilding = db.confighotelbuilding.FirstOrDefault(h => h.id == hotelId);
//                    if (holteBuilding == null) {
//                        sendNotificationToPlayer($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}", "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                        throw new Exception($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}");
//                    }
//                    holteBuilding.ownerid = charId;
//                    db.SaveChanges();
//                    sendNotificationToPlayer($"{newOwnerCharacter.firstname} {newOwnerCharacter.lastname} ist nun Eigentümer(in) von {hotel.Name}", "Hoteleigentümer gesetzt.", Constants.NotifactionTypes.Success, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void dumpHotelList(IPlayer player) {
//            try {
//                StringBuilder stringBuilder = new StringBuilder();
//                stringBuilder.AppendLine("Liste der Hotels von LosSantos:");
//                foreach (Hotel hotel in AllHotels.Values) {
//                    stringBuilder.AppendLine($"* {hotel.Name}");
//                }
//                sendNotificationToPlayer(stringBuilder.ToString(), "Liste an Hotels...", Constants.NotifactionTypes.Info, player);
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void dumpRoomListByHotel(int hotelId, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];
//                StringBuilder stringBuilder = new StringBuilder();
//                stringBuilder.AppendLine($"Liste der Räumlichkeiten von{hotel.Name}:");
//                IList<Assignment> assignments = AllHotelAssignments.Where(a => a.Hotel == hotelId).ToList();
//                IList<HotelRoom> hotelRooms = new List<HotelRoom>();
//                foreach (Assignment assignment in assignments) {
//                    hotelRooms.Add(AllHotelRooms[assignment.Room]);
//                }

//                IList<BookingEntry> bookingEntries = AllBookingEntries.Values
//                    .Where(b => assignments.Any(a => a.Room == b.RoomId) && !b.BookingEnded).ToList();

//                foreach (HotelRoom hotelRoom in hotelRooms) {
//                    stringBuilder.Append($"* {hotelRoom.RoomName} (#{hotelRoom.Id})");
//                    if (bookingEntries.Any(b => b.RoomId == hotelRoom.Id)) {
//                        stringBuilder.AppendLine(" [gebucht]");
//                    } else {
//                        stringBuilder.AppendLine(" [frei]");
//                    }
//                }

//                sendNotificationToPlayer(stringBuilder.ToString(), "Liste der Hotelzimmer...", Constants.NotifactionTypes.Info, player);

//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void clearHotelOwner(int hotelId, IPlayer player) {
//            try {
//                using (var db = new ChoiceVDb()) {
//                    Hotel hotel = AllHotels[hotelId];
//                    confighotelbuilding hotelBuilding = db.confighotelbuilding.FirstOrDefault(h => h.id == hotelId);
//                    if (hotelBuilding == null) {
//                        sendNotificationToPlayer($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}", "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                        throw new Exception($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}");
//                    }

//                    if (hotel.OwnerId == null && hotelBuilding.ownerid == null) {
//                        sendNotificationToPlayer($"Hotel {hotel.Name} hatte keinen Inhaber. Nichts zu ändern.", "Nichts zu ändern.", Constants.NotifactionTypes.Warning, player);
//                        return;
//                    }

//                    int oldOwner = -1;
//                    if (hotelBuilding.ownerid != null) {
//                        oldOwner = hotelBuilding.ownerid.Value;
//                    } else if (hotel.OwnerId != null) {
//                        oldOwner = hotel.OwnerId.Value;
//                    }

//                    hotelBuilding.ownerid = null;
//                    hotel.OwnerId = null;
//                    db.SaveChanges();
//                    characters oldOwnerCharacter = getCharacterToId(oldOwner);

//                    if (oldOwnerCharacter != null) {
//                        sendNotificationToPlayer($"Hotelinhaber entfernt. {oldOwnerCharacter.firstname} {oldOwnerCharacter.lastname} ist nicht mehr Inhaber von {hotel.Name}.", "Inhaber entfernt.", Constants.NotifactionTypes.Success, player);
//                    } else {
//                        sendNotificationToPlayer($"Hotelinhaber entfernt. [Unbekannte / gelöschte Person] ist nicht mehr Inhaber von {hotel.Name}.", "Inhaber entfernt.", Constants.NotifactionTypes.Success, player);
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void dumpEnums(Type type, IPlayer player) {
//            try {
//                string[] enumValues = Enum.GetNames(type);

//                StringBuilder stringBuilder = new StringBuilder();
//                stringBuilder.AppendLine($"Werte für {type}:");
//                foreach (string enumValue in enumValues) {
//                    stringBuilder.AppendLine($"* {enumValue}");
//                }
//                sendNotificationToPlayer(stringBuilder.ToString(), "Enum-Info", Constants.NotifactionTypes.Success, player);
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void getHotelRoomGuestByRoomId(int roomId, IPlayer player) {
//            try {
//                BookingEntry bookingEntry =
//                    AllBookingEntries.Values.FirstOrDefault(b => b.RoomId == roomId && !b.BookingEnded);

//                HotelRoom hotelRoom = AllHotelRooms[roomId];

//                if (bookingEntry == null) {
//                    List<BookingEntry> bookingEntries = AllBookingEntries.Values.Where(b => b.RoomId == roomId).ToList();
//                    bookingEntry = bookingEntries.OrderByDescending(t => t.EndDate).First();
//                    characters guest = getCharacterToId(bookingEntry.GuestId);
//                    sendNotificationToPlayer($"In {hotelRoom.RoomName} gastierte zuletzt {guest.firstname} {guest.lastname} ({guest.id}). Zimmer ist aktuell nicht belegt!", "Gast-Info", Constants.NotifactionTypes.Info, player);
//                } else {
//                    characters guest = getCharacterToId(bookingEntry.GuestId);
//                    sendNotificationToPlayer($"In {hotelRoom.RoomName} gastiert aktuell {guest.firstname} {guest.lastname} ({guest.id})", "Gast-Info", Constants.NotifactionTypes.Info, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void getHotelRoomListByCharId(int charId, IPlayer player) {
//            try {
//                List<int> hotelIdsPlayerOwns = AllHotels.Values.Where(h => h.OwnerId == player.getCharacterId()).Select(h => h.Id).ToList();

//                List<BookingEntry> bookingEntries =
//                    AllBookingEntries.Values.Where(b => b.GuestId == charId && !b.BookingEnded).ToList();

//                //Alle Einträge löschen bei denen der Spieler nicht Hoteleigner ist.
//                bookingEntries.RemoveAll(b =>
//                    !AllHotelAssignments.Any(a => a.Room == b.RoomId && hotelIdsPlayerOwns.Contains(a.Hotel)));

//                if (bookingEntries.Any()) {
//                    StringBuilder stringBuilder = new StringBuilder();
//                    characters character = getCharacterToId(charId);
//                    stringBuilder.AppendLine(
//                        $"{character.firstname} {character.lastname} gastiert aktuell in:");
//                    foreach (BookingEntry bookingEntry in bookingEntries) {
//                        Hotel hotel = getHotelToRoomId(bookingEntry.RoomId);
//                        HotelRoom hotelRoom = AllHotelRooms[bookingEntry.RoomId];
//                        stringBuilder.AppendLine($"* {hotelRoom.RoomName} in {hotel.Name}");
//                    }
//                    sendNotificationToPlayer(stringBuilder.ToString(), "Gast-Info", Constants.NotifactionTypes.Info, player);
//                } else {
//                    characters character = getCharacterToId(charId);
//                    sendNotificationToPlayer($"{character.firstname} {character.lastname} hat aktuell keinen bekannten Wohnsitz.", "Gast-Info", Constants.NotifactionTypes.Info, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void getRoomRates(int hotelId, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];
//                List<HotelRoomRate> hotelRoomRates = AllHotelRates.Values.Where(r => r.HotelId == hotelId).OrderBy(r => r.Rate).ToList();

//                if (hotelRoomRates.Any()) {
//                    StringBuilder stringBuilder = new StringBuilder();
//                    stringBuilder.AppendLine($"Preisliste für {hotel.Name}:");
//                    foreach (HotelRoomRate roomRate in hotelRoomRates) {
//                        stringBuilder.AppendLine(
//                            $"[{roomRate.RoomType}] [{roomRate.RoomClass}] => ${roomRate.Rate,0:0.00}");
//                    }
//                    sendNotificationToPlayer(stringBuilder.ToString(), "Preisliste...", Constants.NotifactionTypes.Info, player);
//                } else {
//                    sendNotificationToPlayer($"Keine Preisliste für {hotel.Name} vorhanden.", "Preisliste...", Constants.NotifactionTypes.Warning, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void lockHotel(int hotelId, string reason, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];

//                //Gesperrtes Hotel nicht nochmal sperren
//                if (!hotel.IsLocked) {
//                    using (var db = new ChoiceVDb()) {
//                        confighotelbuilding hotelBuilding = db.confighotelbuilding.FirstOrDefault(b => b.id == hotelId);
//                        if (hotelBuilding == null) {
//                            sendNotificationToPlayer($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}", "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                            throw new Exception($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}");
//                        }

//                        List<int> roomsToLock =
//                            AllHotelAssignments.Where(a => a.Hotel == hotelId).Select(s => s.Room).ToList();

//                        foreach (int roomToLock in roomsToLock) {
//                            // 1. Alle Gäste rausschmeissen.
//                            // 2. Alle Zimmer sperren (wenn noch nicht gesperrt).
//                            lockHotelRoom(roomToLock, reason, player);
//                        }

//                        // 3. Hotel Sperren
//                        hotelBuilding.islocked = 1;
//                        hotelBuilding.lockreason = reason;
//                        db.SaveChanges();
//                        hotel.IsLocked = true;
//                        hotel.LockReason = reason;
//                        sendNotificationToPlayer($"Hotel {hotel.Name} sowie alle Zimmer geschlossen.", "Hotel geschlossen",
//                            Constants.NotifactionTypes.Success, player);
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void lockHotelRoom(int roomId, string reason, IPlayer player) {
//            try {
//                HotelRoom hotelRoom = AllHotelRooms[roomId];

//                // Gesperrten Raum nicht nochmal sperren.
//                if (!hotelRoom.IsLocked) {
//                    using (var db = new ChoiceVDb()) {
//                        confighotelroom configHotelRoom = db.confighotelroom.FirstOrDefault(r => r.id == roomId);

//                        if (configHotelRoom == null) {
//                            sendNotificationToPlayer($"FEHLER IN DATENBANK! Hotelraum konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId}", "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                            throw new Exception($"FEHLER IN DATENBANK! Hotelraum konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId}");
//                        }

//                        BookingEntry bookingEntry =
//                            AllBookingEntries.Values.FirstOrDefault(b => b.RoomId == roomId && !b.BookingEnded);

//                        // 1. Gast rausschmeissen
//                        if (bookingEntry != null) {
//                            evictHotelRoom(roomId, bookingEntry.GuestId,
//                                $"Das Hotel wird geschlossen. Grund:{reason}", player);
//                        }

//                        // 2. Zimmer sperren
//                        configHotelRoom.islocked = 1;
//                        configHotelRoom.lockreason = reason;
//                        db.SaveChanges();
//                        hotelRoom.IsLocked = true;
//                        hotelRoom.LockReason = reason;
//                        sendNotificationToPlayer($"Zimmer {hotelRoom.RoomName} geschlossen.", "Zimmer geschlossen.", Constants.NotifactionTypes.Success, player);
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void unlockHotel(int hotelId, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];
//                if (hotel.IsLocked) {
//                    using (var db = new ChoiceVDb()) {
//                        confighotelbuilding hotelBuilding = db.confighotelbuilding.FirstOrDefault(b => b.id == hotelId);
//                        if (hotelBuilding == null) {
//                            sendNotificationToPlayer($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}", "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                            throw new Exception($"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}");
//                        }

//                        string lockreason = hotel.LockReason;
//                        List<int> roomsToUnLock =
//                            AllHotelAssignments.Where(a => a.Hotel == hotelId).Select(s => s.Room).ToList();

//                        foreach (int roomId in roomsToUnLock) {

//                            unlockHotelRoom(roomId, player, lockreason);
//                        }

//                        hotelBuilding.lockreason = null;
//                        hotelBuilding.islocked = 0;
//                        db.SaveChanges();
//                        hotel.IsLocked = false;
//                        hotel.LockReason = null;

//                        sendNotificationToPlayer($"Hotel {hotel.Name} sowie alle Zimmer (soweit nicht anderweitig gesperrt) geöffnet.", "Hotel wieder offen.",
//                            Constants.NotifactionTypes.Success, player);
//                    }
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void unlockHotelRoom(int roomId, IPlayer player, string unlockOnlyOnThisReason = null) {
//            try {
//                HotelRoom hotelRoom = AllHotelRooms[roomId];

//                if (hotelRoom.IsLocked && (unlockOnlyOnThisReason == null || string.Equals(hotelRoom.LockReason, unlockOnlyOnThisReason, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(hotelRoom.LockReason) || string.IsNullOrWhiteSpace(hotelRoom.LockReason))) {
//                    using (var db = new ChoiceVDb()) {
//                        confighotelroom configHotelRoom = db.confighotelroom.FirstOrDefault(r => r.id == roomId);

//                        if (configHotelRoom == null) {
//                            sendNotificationToPlayer(
//                                $"FEHLER IN DATENBANK! Hotelraum konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId}", "Datenbankfehler",
//                                Constants.NotifactionTypes.Danger, player);
//                            throw new Exception(
//                                $"FEHLER IN DATENBANK! Hotelraum konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Raum-ID: {roomId}");
//                        }

//                        configHotelRoom.lockreason = null;
//                        configHotelRoom.islocked = 0;
//                        db.SaveChanges();
//                        hotelRoom.LockReason = null;
//                        hotelRoom.IsLocked = false;
//                    }
//                } else if (unlockOnlyOnThisReason != null && !string.Equals(hotelRoom.LockReason, unlockOnlyOnThisReason,
//                      StringComparison.OrdinalIgnoreCase)) {
//                    sendNotificationToPlayer(
//                        $"Zimmer {hotelRoom.RoomName} (#{hotelRoom.Id}) bleibt gesperrt, da der Sperrgrund abweichend ist. Sperrgrund Zimmer: {hotelRoom.LockReason} Sperrgrund erwartet:{unlockOnlyOnThisReason}", "Zimmer bleibt gesperrt.",
//                        Constants.NotifactionTypes.Warning, player);
//                }

//                sendNotificationToPlayer($"Zimmer {hotelRoom.RoomName} (#{hotelRoom.Id}) wurde geöffnet.", "Zimmer wieder offen.",
//                    Constants.NotifactionTypes.Success, player);

//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void cancelBooking(int charId, int? roomId, IPlayer player) {
//            // Wenn CharId nicht passend zu PlayerId, dann muss player HotelOwner zum Raum sein bzw. nur Räume Kündigen, zu denen der HotelOwner passt.
//            try {
//                List<BookingEntry> bookingEntries;
//                if (roomId.HasValue) {
//                    bookingEntries = AllBookingEntries.Values.Where(b => b.GuestId == charId && !b.BookingEnded && b.RoomId == roomId).ToList();
//                } else {
//                    bookingEntries = AllBookingEntries.Values.Where(b => b.GuestId == charId && !b.BookingEnded).ToList();
//                }

//                if (bookingEntries.Any() && charId != player.getCharacterId()) {
//                    //Nicht der Spieler storniert. Dann darf das nur noch der Hotelinhaber. Liste der stornierbaren Buchungen wird auf Hotels gefiltert bei denen der ausführende Spieler Inhaber ist.
//                    bookingEntries.RemoveAll(b => getHotelToRoomId(b.RoomId).OwnerId != player.getCharacterId());
//                }


//                if (bookingEntries.Any()) {
//                    foreach (BookingEntry bookingEntry in bookingEntries) {
//                        string reason = "Stornierung der Buchung.";
//                        if (charId != player.getCharacterId()) {
//                            reason = "Stornierung der Buchung durch das Hotel.";
//                        }
//                        evictHotelRoom(bookingEntry.RoomId, charId, reason, player);
//                    }

//                    sendNotificationToPlayer($"Buchung wurde erfolgreich storniert. ({bookingEntries.Count}x)", "Buchung storniert.", Constants.NotifactionTypes.Success, player);
//                } else {
//                    sendNotificationToPlayer("Es liegt keine stornierbare Buchung vor.", "Kein Storno möglich.", Constants.NotifactionTypes.Warning, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void checkHotelIntegrity(int hotelId, IPlayer player) {
//            StringBuilder errorList = new StringBuilder();
//            int errorCount = 0;

//            try {
//                errorList.AppendLine($"Liste der Probleme mit Hotel #{hotelId}:");

//                if (!AllHotels.ContainsKey(hotelId)) {
//                    errorList.AppendLine($"!!! Hotel mit ID {hotelId} existiert nicht!");
//                    errorCount++;
//                } else {
//                    Hotel hotel = AllHotels[hotelId];
//                    if (string.IsNullOrEmpty(hotel.Name) || string.IsNullOrWhiteSpace(hotel.Name)) {
//                        errorList.AppendLine("!!! Das Hotel hat keinen gültigen Namen.");
//                        errorCount++;
//                    }

//                    if (hotel.Name != null && hotel.Name.Length > 32) {
//                        errorList.AppendLine("--! Hotelname sehr lang.");
//                        errorCount++;
//                    }

//                    if (!BankController.accountIdExists(hotel.BankAccountId)) {
//                        errorList.AppendLine("!!! Bankkonto des Hotels ist ungültig!");
//                        errorCount++;
//                    }

//                    if (string.IsNullOrEmpty(hotel.DoorGroup) && hotel.DoorId == null) {
//                        errorList.AppendLine("--! Hotel hat keine Tür.");
//                        errorCount++;
//                    }

//                    if (hotel.IsLocked && string.IsNullOrEmpty(hotel.LockReason)) {
//                        errorList.AppendLine("-!! Hotel ist ohne Grund gesperrt!");
//                        errorCount++;
//                    }

//                    List<Assignment> assignments = AllHotelAssignments.Where(a => a.Hotel == hotelId).ToList();
//                    if (!assignments.Any()) {
//                        errorList.AppendLine("!!! Hotel hat keine Zimmer zugeordnet.");
//                        errorCount++;
//                    }

//                    IList<HotelRoomTypeClassCombination> usedCombinations = new List<HotelRoomTypeClassCombination>();
//                    foreach (Assignment assignment in assignments) {
//                        checkHotelRoomIntegrity(assignment.Room, errorList, usedCombinations, ref errorCount);
//                    }

//                    foreach (HotelRoomTypeClassCombination hotelRoomTypeClassCombination in usedCombinations) {
//                        if (!AllHotelRates.Values.Any(r =>
//                            r.HotelId == hotelId && r.RoomType == hotelRoomTypeClassCombination.RoomType &&
//                            r.RoomClass == hotelRoomTypeClassCombination.RoomClass)) {
//                            errorList.AppendLine(
//                                $"!!! Es gibt noch keinen Preis für den Raumtyp {hotelRoomTypeClassCombination.RoomType} - {hotelRoomTypeClassCombination.RoomClass}!");
//                            errorCount++;
//                        }
//                    }
//                }

//                Constants.NotifactionTypes notificationType = Constants.NotifactionTypes.Warning;
//                if (errorCount == 0) {
//                    errorList.AppendLine("--- Keine Probleme & Auffälligkeiten gefunden.");
//                    notificationType = Constants.NotifactionTypes.Success;
//                }

//                sendNotificationToPlayer(errorList.ToString(), "Ergebnis der Hotelvalidierung.", notificationType, player);
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void checkHotelRoomIntegrity(int roomId, StringBuilder errorListToAppendTo, IList<HotelRoomTypeClassCombination> usedCombinations, ref int errorCount) {
//            if (!AllHotelRooms.ContainsKey(roomId)) {
//                errorListToAppendTo.AppendLine($"!!! Raum #{roomId} ist Hotel zugewiesen, existiert aber nicht!");
//                errorCount++;
//                return;
//            }

//            HotelRoom room = AllHotelRooms[roomId];


//            if (!usedCombinations.Any(c => c.RoomClass == room.RoomClass && c.RoomType == room.RoomType)) {
//                usedCombinations.Add(new HotelRoomTypeClassCombination() { RoomClass = room.RoomClass, RoomType = room.RoomType });
//            }

//            if (string.IsNullOrEmpty(room.RoomName) || string.IsNullOrWhiteSpace(room.RoomName)) {
//                errorListToAppendTo.AppendLine($"!!! Raum #{roomId} hat keinen richtigen Namen.");
//                errorCount++;
//            } else if (room.RoomName.Length > 30) {
//                errorListToAppendTo.AppendLine($"-!! Name von Raum {room.RoomName}/#{roomId} ist zu lang.");
//                errorCount++;
//            }

//            if (room.IsLocked && string.IsNullOrEmpty(room.LockReason)) {
//                errorListToAppendTo.AppendLine($"-!! Raum {room.RoomName}/#{roomId} ist ohne Grund gesperrt.");
//                errorCount++;
//            }

//            if (!room.DoorId.HasValue && string.IsNullOrEmpty(room.DoorGroup)) {
//                errorListToAppendTo.AppendLine($"!!! Raum {room.RoomName}/#{roomId} hat keine Tür!");
//                errorCount++;
//            }
//        }

//        internal static void assignTerminalToHotel(int hotelId, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];
//                CollisionShape playerCollisionShape = null;

//                List<CollisionShape> currentCollisionShapes = player.getCurrentCollisionShapes();

//                if (currentCollisionShapes != null) {
//                    playerCollisionShape = currentCollisionShapes.FirstOrDefault(pccs =>
//                        pccs.Interactable && string.Equals(pccs.EventName, "UseHotelTerminal",
//                            StringComparison.OrdinalIgnoreCase));
//                }

//                if (playerCollisionShape == null) {
//                    sendNotificationToPlayer("Spieler steht nicht in/an einem Terminal.", "Terminal nicht gefunden.", Constants.NotifactionTypes.Warning, player);
//                    return;
//                }


//                using (var db = new ChoiceVDb()) {
//                    confighotelterminal configHotelTerminal = new confighotelterminal() {
//                        height = playerCollisionShape.Height,
//                        width = playerCollisionShape.Width,
//                        hotelbuildingid = hotel.Id,
//                        isactive = 1,
//                        name = hotel.Name,
//                        position = playerCollisionShape.Position.ToJson(),
//                        rotation = playerCollisionShape.Rotation
//                    };

//                    db.confighotelterminal.Add(configHotelTerminal);
//                    db.SaveChanges();

//                    HotelTerminal hotelTerminal = new HotelTerminal(configHotelTerminal);
//                    AllHotelTerminals.Add(hotelTerminal.Id, hotelTerminal);

//                    sendNotificationToPlayer("Hotelterminal erfolgreich registriert.", "Terminal registriert", Constants.NotifactionTypes.Success, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void registerNewTerminal(confighotelterminal configHotelTerminal) {
//            try {

//                HotelTerminal hotelTerminal = new HotelTerminal(configHotelTerminal);

//                AllHotelTerminals.Add(hotelTerminal.Id, hotelTerminal);
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        internal static void showHotelBookingMenu(int hotelId, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];
//                int characterId = player.getCharacterId();
//                bool showAdminMenu = characterId == hotel.OwnerId || player.getCharacterData().AdminMode;  //Admins sehen auch das Verwaltungs-Hotelmenü

//                List<BookingEntry> existingBookingEntries = AllBookingEntries.Values.Where(b =>
//                    b.GuestId == characterId && !b.BookingEnded &&
//                    getHotelToRoomId(b.RoomId).Id == hotelId).ToList();
//                bool showRoomManagementMenu = existingBookingEntries.Any() && !hotel.IsLocked;

//                // [Submenu]    Administrieren (Nur für Hotelinhaber, Teilweise später auch für Polizei & Co.)
//                //     [TextBox]    Gast-Name / Gast Id
//                //     [Click]        Information zu Gast
//                //     [Click]        Gastbuchung stornieren
//                //     [TextBox]    Zimmername / Zimmer Id
//                //     [Click]        Buchungsinformation anzeigen
//                //     [TextBox]    Räumgrund Raum
//                //     [Click]        Raum räumen
//                //     [TextBox]    Sperrgrund Raum
//                //     [Click]        Raum sperren
//                //     [Click]        Raum entsperren
//                //     [TextBox]    Sperrgrund Hotel
//                //     [Click]        Hotel sperren
//                //     [Click]        Hotel entsperren
//                // [Submenu]    Verwalten (Wenn bereits Raum gebucht)
//                //     [Select]    {Liste der gebuchten Räume}
//                //     [Click]        Information über Resttage
//                //     [Select]    Verlängerungstage: 1,2,5,7,14,30
//                //     [Label]        Preis für Verlängerung
//                //     [Click]        Verlängern
//                //     [Click]        Stornieren
//                // [Click]    Preisliste (Immer)
//                // [Submenu]    Buchen (Immer)
//                //     [Select]    Vorhandene Raum-Typ-Klassen-Kombinationen
//                //     [Label]        Preis (+Aufschlag) Beispiel: $100 (+$125 Aufschlag 3fach Buchung)
//                //     [Click]        Buchen
//                // Nur wenn Teleport-Zimmer dabei
//                // [Submenu / Click] Auf's Zimmer gehen (Submenü, wenn mehrere Zimmer. Wird nur ab 1 gültigen Zimmerschlüssel angezeigt.)
//                // [Click] Zimmer besuchen. (Anzeige ab 1 V-Zimmer gebucht & freigeschaltet.)

//                string buildingTypeName = "das Hotel";
//                string guestType = "Gast";
//                string roomType = "das Zimmer";
//                string roomType2 = "s Zimmer";
//                string roomType3 = "Zimmer";
//                string roomType4 = "Zimmer";

//                if (hotel.HotelType == HotelTypes.LoveHotel) {
//                    buildingTypeName = "das Stundenhotel";
//                } else if (hotel.HotelType == HotelTypes.Motel) {
//                    buildingTypeName = "das Motel";
//                } else if (hotel.HotelType == HotelTypes.Apartments) {
//                    buildingTypeName = "die Vermietung";
//                    guestType = "Mieter";
//                    roomType = "die Wohnung";
//                    roomType2 = "e Wohnung";
//                    roomType3 = "Wohnung";
//                    roomType4 = "Wohnungen";
//                } else if (hotel.HotelType == HotelTypes.Buildings) {
//                    buildingTypeName = "die Vermietung";
//                    guestType = "Mieter";
//                    roomType = "das Haus";
//                    roomType2 = "s Haus";
//                    roomType3 = "Haus";
//                    roomType4 = "Häuser";
//                }


//                Menu menu = new Menu(hotel.Name, getShortHotelInfoText(hotel));
//                if (showAdminMenu) {
//                    //Submenü für "Administrieren" aufbauen.
//                    Menu adminSubMenu = new Menu(
//                        "Administrieren",
//                        $"Administration für {buildingTypeName}");

//                    InputMenuItem guestIdInputMenuItem = new InputMenuItem(
//                        $"{guestType}-Name/ {guestType}-ID",
//                        "{guestType} zu dem die nachfolgenden Aktionen ausgeführt werden sollen.",
//                        $"{guestType}-Name/ {guestType}-ID",
//                        null);
//                    adminSubMenu.addMenuItem(guestIdInputMenuItem);

//                    adminSubMenu.addMenuItem(
//                        new MenuStatsMenuItem(
//                                $"Information zu {guestType}",
//                                $"Gibt aus, welche{roomType2} zuletzt gebucht war, aktuell gebucht ist.",
//                                "HOTEL_SHOWGUESTINFO", MenuItemStyle.green)
//                            .withData(new Dictionary<string, dynamic>()
//                                {{"hotelId", hotelId}, {"guestIdInputMenuItemId", guestIdInputMenuItem.Id}}));

//                    if (!hotel.IsLocked) {
//                        adminSubMenu.addMenuItem(
//                            new MenuStatsMenuItem(
//                                    $"{guestType}buchung stornieren",
//                                    $"Storniert alle Buchungen des angegebenen {guestType}s.",
//                                    "HOTEL_CANCELALLGUESTROOMS")
//                                .needsConfirmation($"Alle Buchungen des {guestType}s stornieren?",
//                                    $"Alles von {guestType} stornieren.")
//                                .withData(new Dictionary<string, dynamic>()
//                                    {{"hotelId", hotelId}, {"guestIdInputMenuItemId", guestIdInputMenuItem.Id}}));
//                    }

//                    InputMenuItem roomIdInputMenuItem = new InputMenuItem(
//                        $"{roomType3} name / {roomType3} Id",
//                        $"{roomType3} zu dem die nachfolgenden Aktionen ausgeführt werden sollen.",
//                        $"{roomType3}name / {roomType3} Id",
//                        null);
//                    adminSubMenu.addMenuItem(roomIdInputMenuItem);

//                    adminSubMenu.addMenuItem(
//                        new MenuStatsMenuItem(
//                                "Buchungsinformation anzeigen",
//                                $"Gibt aus, durch wen {roomType} aktuell gebucht ist / zuletzt gebucht war.",
//                                "HOTEL_SHOWBOOKINGINFO")
//                            .withData(new Dictionary<string, dynamic>()
//                                {{"hotelId", hotelId}, {"roomIdInputMenuItemId", roomIdInputMenuItem.Id}}));

//                    if (!hotel.IsLocked) {

//                        InputMenuItem evictionReasonInputMenuItem = new InputMenuItem(
//                            "Räumungsgrund",
//                            $"Grund wieso {roomType} geräumt wird.",
//                            "Räumungsgrund",
//                            null,
//                            MenuItemStyle.red);
//                        adminSubMenu.addMenuItem(evictionReasonInputMenuItem);

//                        adminSubMenu.addMenuItem(
//                            new MenuStatsMenuItem(
//                                    "Räumung durchführen",
//                                    $"Räumt {roomType}",
//                                    "HOTEL_EVICTROOM",
//                                    MenuItemStyle.red)
//                                .needsConfirmation($"{roomType3} wirklich räumen?",
//                                    $"Du schmeisst den {guestType} raus!")
//                                .withData(new Dictionary<string, dynamic>()
//                                {
//                                    {"hotelId", hotelId},
//                                    {"evictionReasonInputMenuItemId", evictionReasonInputMenuItem.Id},
//                                    {"roomIdInputMenuItemId", roomIdInputMenuItem.Id}
//                                }));

//                        InputMenuItem roomLockReasonInputMenuItem = new InputMenuItem("Sperrgrund Raum",
//                            $"Grund wieso {roomType} gesperrt wird.",
//                            "Sperrgrund",
//                            null,
//                            MenuItemStyle.red);
//                        adminSubMenu.addMenuItem(roomLockReasonInputMenuItem);

//                        adminSubMenu.addMenuItem(
//                            new MenuStatsMenuItem(
//                                    "Sperrung durchführen",
//                                    $"Räumt & sperrt {roomType}",
//                                    "HOTEL_LOCKROOM",
//                                    MenuItemStyle.red)
//                                .needsConfirmation($"{roomType3} wirklich sperren?", $"{roomType3} nicht mehr buchbar.")
//                                .withData(new Dictionary<string, dynamic>()
//                                {
//                                    {"hotelId", hotelId},
//                                    {"roomLockReasonInputMenuItemId", roomLockReasonInputMenuItem.Id},
//                                    {"roomIdInputMenuItemId", roomIdInputMenuItem.Id}
//                                }));

//                        adminSubMenu.addMenuItem(
//                            new MenuStatsMenuItem(
//                                    "Entsperrung durchführen",
//                                    $"Entsperrt {roomType}",
//                                    "HOTEL_UNLOCKROOM",
//                                    MenuItemStyle.red)
//                                .withData(new Dictionary<string, dynamic>()
//                                    {{"hotelId", hotelId}, {"roomIdInputMenuItemId", roomIdInputMenuItem.Id}}));
//                    }

//                    if (hotel.IsLocked) {
//                        adminSubMenu.addMenuItem(
//                            new ClickMenuItem(
//                                    "Entsperrung durchführen",
//                                    $"Entsperrt {buildingTypeName}",
//                                    string.Empty,
//                                    "HOTEL_UNLOCKHOTEL",
//                                    MenuItemStyle.red)
//                                .withData(new Dictionary<string, dynamic>() { { "hotelId", hotelId } }));
//                    } else {
//                        InputMenuItem hotelLockReasonInputMenuItem = new InputMenuItem(
//                            "Sperrgrund Hotel",
//                            "Grund wieso das ganze Hotel gesperrt wird.",
//                            "Sperrgrund",
//                            null,
//                            MenuItemStyle.red);
//                        adminSubMenu.addMenuItem(hotelLockReasonInputMenuItem);

//                        adminSubMenu.addMenuItem(
//                            new MenuStatsMenuItem(
//                                    "Sperrung durchführen",
//                                    $"Räumt & sperrt {buildingTypeName}",
//                                    "HOTEL_LOCKHOTEL",
//                                    MenuItemStyle.red)
//                                .needsConfirmation(
//                                    $"Wollen Sie {buildingTypeName} und alle {roomType4} wirklich sperren?",
//                                    "Auch alle Gäste rausschmeissen.")
//                                .withData(new Dictionary<string, dynamic>()
//                                {
//                                    {"hotelId", hotelId},
//                                    {"hotelLockReasonInputMenuItemId", hotelLockReasonInputMenuItem.Id}
//                                }));
//                    }

//                    //Submenü hinzufügen
//                    menu.addMenuItem(new MenuMenuItem("Administrieren", adminSubMenu));
//                }

//                if (showRoomManagementMenu) {
//                    string[] rooms = existingBookingEntries.Select(b => AllHotelRooms[b.RoomId].RoomName).ToArray();
//                    int[] roomIds = existingBookingEntries.Select(b => AllHotelRooms[b.RoomId].Id).ToArray();

//                    //Submenü für "Buchungsverwaltung" aufbauen.
//                    Menu bookingSubMenu = new Menu("Verwalten", "Ihre Buchungen verwalten.");

//                    ListMenuItem guestBookingsListMenuItem = new ListMenuItem(
//                        "Ihre Buchungen",
//                        $"Liste ihrer Aktuellen Buchungen bei {hotel.Name}.",
//                        rooms,
//                        null);
//                    bookingSubMenu.addMenuItem(guestBookingsListMenuItem);

//                    bookingSubMenu.addMenuItem(
//                        new MenuStatsMenuItem(
//                            "Resttage anzeigen.",
//                            "Zeigt die Resttage der Buchung an.",
//                            "HOTEL_BOOKINGLEFTDAYS",
//                            MenuItemStyle.green).withData(new Dictionary<string, dynamic>() { { "hotelId", hotel.Id }, { "guestBookingsListMenuItemId", guestBookingsListMenuItem.Id }, { "roomIds", roomIds } }));

//                    string[] extensionDays = new[] { "1", "2", "3", "4", "5", "6", "7", "14", "30" };

//                    ListMenuItem extentionDaysListMenuItem = new ListMenuItem(
//                        "Verlängerungstage",
//                        "Anzahl der Tage um die die Buchung verlängert werden soll.",
//                        extensionDays,
//                        null);
//                    bookingSubMenu.addMenuItem(extentionDaysListMenuItem);

//                    bookingSubMenu.addMenuItem(
//                        new MenuStatsMenuItem("Zur Buchungsverlängerung",
//                                "Verlängert die Buchung um die angegebenen Tage.",
//                                "HOTEL_EXTENDBOOKING1",
//                                MenuItemStyle.green)
//                            .withData(new Dictionary<string, dynamic>() { { "hotelId", hotelId }, { "guestBookingsListMenuItemId", guestBookingsListMenuItem.Id }, { "roomIds", roomIds }, { "extentionDaysListMenuItemId", extentionDaysListMenuItem.Id }, { "extensionDays", extensionDays } }));

//                    bookingSubMenu.addMenuItem(
//                        new MenuStatsMenuItem(
//                                "Stornieren",
//                                $"Storniert die ausgewählte Buchung. Sie können {roomType} anschließend nicht mehr betreten.",
//                                "HOTEL_CANCELBOOKING",
//                                MenuItemStyle.red)
//                            .needsConfirmation("Buchung wirklich stornieren?",
//                                "Restbetrag wird ausgezahlt.")
//                            .withData(new Dictionary<string, dynamic>() { { "hotelId", hotelId }, { "guestBookingsListMenuItemId", guestBookingsListMenuItem.Id }, { "roomIds", roomIds } }));

//                    bookingSubMenu.addMenuItem(
//                        new MenuStatsMenuItem(
//                                "Zweitschlüssel kaufen.",
//                                $"Erstellt einen Zweitschlüssel für {roomType}. Kostenpunkt: ${HotelSettings.ReplaceKeyCharge}.",
//                                "HOTEL_CREATESECONDROOMKEY",
//                                MenuItemStyle.green)
//                            .needsConfirmation($"Zweitschlüssel für ${HotelSettings.ReplaceKeyCharge} kaufen?",
//                                "Sie erhalten einen Zweitschlüssel.")
//                            .withData(new Dictionary<string, dynamic>() { { "hotelId", hotelId }, { "guestBookingsListMenuItemId", guestBookingsListMenuItem.Id }, { "roomIds", roomIds } }));

//                    bookingSubMenu.addMenuItem(
//                        new MenuStatsMenuItem(
//                                "Schloss tauschen.",
//                                $"Tauscht das Schlüss für {roomType} aus. Kostenpunkt: ${HotelSettings.ReplaceLockCharge}.",
//                                "HOTEL_CHANGEROOMLOCK",
//                                MenuItemStyle.green)
//                            .needsConfirmation($"Wirklich das Schloss für ${HotelSettings.ReplaceLockCharge} tauschen?",
//                                "Sie erhalten einen(!) neuen Schlüssel.")
//                            .withData(new Dictionary<string, dynamic>() { { "hotelId", hotelId }, { "guestBookingsListMenuItemId", guestBookingsListMenuItem.Id }, { "roomIds", roomIds } }));

//                    //Submenü hinzufügen
//                    menu.addMenuItem(new MenuMenuItem("Verwalten", bookingSubMenu));
//                }

//                if (hotel.IsLocked) {
//                    menu.addMenuItem(new StaticMenuItem("Das Hotel ist gesperrt!", "Das Hotel ist gesperrt. Eine Buchung ist derzeit nicht möglich.", null, MenuItemStyle.red));
//                } else {
//                    menu.addMenuItem(
//                        new ClickMenuItem(
//                                "Preisliste", $"Zeigt die Preisliste für {buildingTypeName} an.",
//                                null,
//                                "HOTEL_SHOWPRICING",
//                                MenuItemStyle.green)
//                            .withData(new Dictionary<string, dynamic>() { { "hotelId", hotelId } }));

//                    //Submenü für "Raumbuchung" aufbauen.
//                    Menu newBookingSubMenu = new Menu("Neue Buchung", "Eine neue Buchung ausführen.");

//                    if (existingBookingEntries.Any()) {
//                        newBookingSubMenu.addMenuItem(
//                            new StaticMenuItem(
//                                "Hinweis zur Mehrfachbuchung.",
//                                "Bei Mehrfachbuchungen wird ein Aufschlag um zusätzliche 50% pro weiterer Buchung erhoben.",
//                                null,
//                                MenuItemStyle.red));
//                    }

//                    // Zimmerpreise holen
//                    List<HotelRoomRate> hotelRoomRates = AllHotelRates.Values.Where(r => r.HotelId == hotelId).ToList();

//                    IList<int> roomsOfHotel =
//                        AllHotelAssignments.Where(a => a.Hotel == hotelId).Select(a => a.Room).ToList();

//                    bool isAnyRoomAvailable = false;

//                    List<MenuItem> rateMenuItems = new List<MenuItem>();
//                    foreach (HotelRoomRate roomRate in hotelRoomRates) {
//                        decimal calculatedRoomRate = doCalculateRoomRate(roomRate.Rate, roomRate.RateTickInDays,
//                            existingBookingEntries.Count);

//                        int countRooms = AllHotelRooms.Count(r =>
//                            roomsOfHotel.Contains(r.Key) && r.Value.RoomType == roomRate.RoomType &&
//                            r.Value.RoomClass == roomRate.RoomClass && !r.Value.IsLocked && !AllBookingEntries.Values.Any(e => e.Id == r.Key && !e.BookingEnded));

//                        if (countRooms > 0) {
//                            string availableCount = $"Noch {countRooms} {roomType4} verfügbar.";
//                            if (countRooms == 1) availableCount = $"Noch {countRooms} {roomType3} verfügbar.";
//                            rateMenuItems.Add(
//                                new ClickMenuItem(
//                                        $"{roomTypeToText(roomRate.RoomType)} - {roomClassToText(roomRate.RoomClass)}",
//                                        availableCount,
//                                        $"(${calculatedRoomRate,0:0.00})",
//                                        "HOTEL_NEWBOOKING",
//                                        MenuItemStyle.green)
//                                    .withData(new Dictionary<string, dynamic>()
//                                        {{"roomRate", roomRate}, {"hotelId", hotelId}}));
//                            isAnyRoomAvailable = true;
//                        } else {
//                            rateMenuItems.Add(new StaticMenuItem(
//                                $"{roomTypeToText(roomRate.RoomType)} - {roomClassToText(roomRate.RoomClass)}",
//                                "Ausgebucht", $"(${calculatedRoomRate,0:0.00})"));
//                        }

//                    }

//                    foreach (MenuItem rateMenuItem in rateMenuItems) {
//                        newBookingSubMenu.addMenuItem(rateMenuItem);
//                    }

//                    if (isAnyRoomAvailable) {
//                        menu.addMenuItem(new MenuMenuItem("Neue Buchung", newBookingSubMenu));
//                    } else {
//                        menu.addMenuItem(new StaticMenuItem($"{firstCharToUpper(buildingTypeName)} ist ausgebucht.", $"Leider sind alle {roomType4} ausgebucht. Wir bitten um Verständnis.", null));
//                    }

//                    HotelRoom[] visitableHotelRooms = getVisitableHotelRoomsByInventory(characterId, hotel, player);

//                    if (visitableHotelRooms.Any()) {
//                        if (visitableHotelRooms.Length > 1) {
//                            Menu enterRoomMenu = new Menu($"{roomType3} betreten", $"{roomType3} wählen und betreten.");

//                            foreach (HotelRoom visitableHotelRoom in visitableHotelRooms) {
//                                enterRoomMenu.addMenuItem(
//                                    new ClickMenuItem($"{roomType3} {visitableHotelRoom.RoomName}",
//                                            $"{roomType3} {visitableHotelRoom.RoomName} betreten.", string.Empty,
//                                            "HOTEL_ENTER_TELEPORT_ROOM", MenuItemStyle.green)
//                                        .withData(new Dictionary<string, dynamic>()
//                                            {{"RoomId", visitableHotelRoom.Id}}));
//                            }
//                            menu.addMenuItem(new MenuMenuItem($"{roomType3} betreten", enterRoomMenu));
//                        } else {
//                            menu.addMenuItem(
//                                new ClickMenuItem($"{roomType3} {visitableHotelRooms[0].RoomName}",
//                                        $"{roomType3} {visitableHotelRooms[0].RoomName} betreten.", string.Empty,
//                                        "HOTEL_ENTER_TELEPORT_ROOM", MenuItemStyle.green)
//                                    .withData(new Dictionary<string, dynamic>()
//                                        {{"RoomId", visitableHotelRooms[0].Id}}));
//                        }
//                    }

//                    visitableHotelRooms = getVisitableHotelRoomsByInvite(hotel);
//                    if (visitableHotelRooms.Any()) {
//                        Menu enterRoomMenu = new Menu($"{roomType3} besuchen", $"{roomType3} wählen und besuchen.");

//                        foreach (HotelRoom visitableHotelRoom in visitableHotelRooms) {
//                            enterRoomMenu.addMenuItem(
//                                new ClickMenuItem($"{roomType3} {visitableHotelRoom.RoomName}",
//                                        $"{roomType3} {visitableHotelRoom.RoomName} besuchen.", string.Empty,
//                                        "HOTEL_ENTER_TELEPORT_ROOM", MenuItemStyle.green)
//                                    .withData(new Dictionary<string, dynamic>()
//                                        {{"RoomId", visitableHotelRoom.Id}}));
//                        }
//                        menu.addMenuItem(new MenuMenuItem($"{roomType3} betreten", enterRoomMenu));
//                    }
//                }
//                player.showMenu(menu);
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private static HotelRoom[] getVisitableHotelRoomsByInvite(Hotel hotel) {
//            return getHotelRoomListByHotel(hotel).Where(r => r.RoomTypeIngame == RoomTypeIngame.TeleportRoom && r.RoomAcceptsGuest).ToArray();
//        }

//        private static List<HotelRoom> getHotelRoomListByHotel(Hotel hotel) {
//            List<int> roomIds = AllHotelAssignments.Where(a => a.Hotel == hotel.Id).Select(a => a.Room).ToList();
//            return AllHotelRooms.Values.Where(r => roomIds.Contains(r.Id)).ToList();
//        }

//        private static HotelRoom[] getVisitableHotelRoomsByInventory(int characterId, Hotel hotel, IPlayer player) {
//            List<DoorKey> keys = player.getInventory().getItems(Constants.DOORKEY_DB_ID, -1, i => true).Select(i => i as DoorKey).ToList();
//            if (!keys.Any()) { return new HotelRoom[0]; }

//            List<HotelRoom> hotelRooms = AllBookingEntries.Values.Where(b => b.GuestId == characterId && !b.BookingEnded && getHotelToRoomId(b.RoomId) == hotel).Select(b => AllHotelRooms[b.RoomId]).Where(h => h.RoomTypeIngame == RoomTypeIngame.TeleportRoom).ToList();
//            if (!hotelRooms.Any()) { return new HotelRoom[0]; }

//            List<DoorController.Door> doors = DoorController.AllDoors.Values.Where(d => keys.Any(k => (k.DoorId == d.Id || string.Equals(k.DoorGroup, d.GroupName)) && k.LockIndex == d.LockIndex)).ToList();
//            return hotelRooms.Where(h => doors.Any(d => d.Id == h.DoorId || d.GroupName == h.DoorGroup)).ToArray();
//        }

//        private static readonly decimal MultiplikatorMultipleRate = HotelSettings.PenaltyCharge;

//        internal static decimal doCalculateRoomRate(decimal rateDecimal, int rateTickInDays, int existingBookingEntries) {
//            decimal calculatedRoomRate = rateDecimal / rateTickInDays;
//            for (int count = 0; count < existingBookingEntries; count++) {

//                calculatedRoomRate = decimal.Multiply(calculatedRoomRate, MultiplikatorMultipleRate);
//            }

//            return calculatedRoomRate;
//        }

//        private bool onMenuEventNewBooking(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            bool earlyFail = false;
//            try
//            {

//                long socialSecurityNumber = player.getSocialSecurityNumber();
//                if (socialSecurityNumber <= 0)
//                {
//                    sendNotificationToPlayer(
//                        "Keine Sozialversicherungskarte gefunden. (Muss sich im Inventar befinden)",
//                        "Keine Sozialversicherungskarte", Constants.NotifactionTypes.Warning, player);
//                    return false;
//                }


//                long phoneNumber = player.getMainPhoneNumber();
//                if (phoneNumber <= 0) {
//                    sendNotificationToPlayer("Zur Durchführung der Buchung benötigen wir eine Telefonnummer um Sie zu erreichen. Bitte richten Sie Ihr Telefon ein.", "Buchung abgebrochen.", Constants.NotifactionTypes.Warning, player);
//                    earlyFail = true;
//                }

//                using (var db = new ChoiceVDb()) {
//                    //Bankkonto des Gasts ermitteln
//                    long bankAccount =
//                        long.Parse(db.characters.First(c => c.id == player.getCharacterId()).bankaccount);
//                    if (!BankController.accountIdExists(bankAccount)) {
//                        // Bannkonto existiert nicht!
//                        sendNotificationToPlayer(
//                            "Zur Durchführung der Buchung benötigen Sie ein Girokonto bei einer Bank. Bitte richten Sie ein Girokonto z.B. am nächten ATM ein.",
//                            "Buchung abgebrochen.", Constants.NotifactionTypes.Warning, player);
//                        earlyFail = true;
//                    }
//                }

//                if (earlyFail) { return false; }

//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                // 1. Hotelrate holen.
//                HotelRoomRate selectedHotelRoomRate = (HotelRoomRate)data["roomRate"];
//                // Versuchen das korrekte Event zu bekommen.

//                // Ermitteln, welcher Raum frei ist.
//                int[] freeRoomIdsToHotelAndRate = getFreeRoomIdsToHotelAndRate(hotel.Id, selectedHotelRoomRate);

//                // Von den freien Räumen einen zufällig wählen.
//                int randomRoomId = freeRoomIdsToHotelAndRate[RandomNumberGenerator.GetInt32(freeRoomIdsToHotelAndRate.Length)];

//                // Zu buchender Raum ermitteln.
//                HotelRoom roomForBooking = AllHotelRooms[randomRoomId];

//                // Exakte Rate (mit ggf. Strafgebühren) errechnen.
//                // 1. Wie viele Buchungen hat der Gast bereits?
//                List<BookingEntry> existingBookingEntries = AllBookingEntries.Values.Where(b =>
//                    b.GuestId == player.getCharacterId() && !b.BookingEnded &&
//                    getHotelToRoomId(b.RoomId).Id == hotel.Id).ToList();

//                // 2. Rate ausrechnen.
//                Decimal roomRate = doCalculateRoomRate(selectedHotelRoomRate.Rate, selectedHotelRoomRate.RateTickInDays,
//                    existingBookingEntries.Count);

//                // Raumtyp ermitteln für Texte
//                string roomType = "Zimmer";
//                if (hotel.HotelType == HotelTypes.Apartments) {
//                    roomType = "Wohnung";
//                } else if (hotel.HotelType == HotelTypes.Buildings) {
//                    roomType = "Haus";
//                }

//                // Tür / Türgruppe des Raums ermitteln (für Schlüssel)
//                DoorController.Door door;
//                int doorId = -1;
//                string doorGroup = "";

//                if (roomForBooking.DoorId.HasValue) {
//                    door = DoorController.AllDoors.Values.First(d => d.Id == roomForBooking.DoorId.Value);
//                    doorId = roomForBooking.DoorId.Value;
//                } else {
//                    door = DoorController.AllDoors.Values.First(d => d.GroupName == roomForBooking.DoorGroup);
//                    doorGroup = roomForBooking.DoorGroup;
//                }

//                // Start des direkten DB-Zugriffs
//                using (var db = new ChoiceVDb()) {
//                    //Bankkonto des Gasts ermitteln
//                    long bankAccount = long.Parse(db.characters.First(c => c.id == player.getCharacterId()).bankaccount);
//                    if (!BankController.accountIdExists(bankAccount)) {
//                        // Bannkonto existiert nicht!
//                        sendNotificationToPlayer("Zur Durchführung der Buchung benötigen Sie ein Girokonto bei einer Bank. Bitte richten Sie ein Girokonto z.B. am nächten ATM ein.", "Buchung abgebrochen.", Constants.NotifactionTypes.Warning, player);
//                        return false;
//                    }

//                    //Kontostand prüfen.
//                    if (db.bankaccounts.First(b => b.id == bankAccount).balance < roomRate) {
//                        sendNotificationToPlayer("Fehler bei der Buchung. Leider ist Ihr persönliches Bankkonto nicht ausreichend gedeckt.", "Zu wenig Geld auf Konto", Constants.NotifactionTypes.Warning, player);
//                        return false;
//                    }

//                    //Geld transferieren
//                    if (!BankController.transferMoney(bankAccount, hotel.BankAccountId, roomRate,
//                        $"Raumbuchung bei {hotel.Name} für {roomType} {roomForBooking.RoomName}. ({selectedHotelRoomRate.RateTickInDays} Tag(e)).", player)
//                    ) {
//                        sendNotificationToPlayer("Fehler bei der Buchung. Die Transaktion wurde durch die Bank abgebrochen. Versuchen Sie es bitte erneut oder wenden Sie sich an Ihre Bank.", "Transaktionsfehler", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }

//                    //Neuen Buchungseintrag erstellen
//                    hotelbookings hotelBooking = new hotelbookings() {
//                        currentrate = roomRate,
//                        enddate = DateTime.Now + TimeSpan.FromDays(selectedHotelRoomRate.RateTickInDays) +
//                                  TimeSpan.FromHours(1),
//                        guestid = player.getCharacterId(),
//                        roomid = randomRoomId,
//                        startdate = DateTime.Now,
//                        phoneNumber = phoneNumber,
//                        socialSecurityNumber = socialSecurityNumber
//                    };
//                    db.Add(hotelBooking);
//                    db.SaveChanges();
//                    BookingEntry newBookingEntry = new BookingEntry(hotelBooking);
//                    AllBookingEntries.Add(hotelBooking.id, newBookingEntry);
//                }

//                // Schlüssel anfertigen und dem Gast übergeben.
//                configitems item = InventoryController.AllConfigItems.First(i => string.Equals(i.codeItem, "DoorKey", StringComparison.InvariantCulture));
//                player.getInventory().addItem(new DoorKey(item, door.LockIndex, doorGroup, doorId, $"Schlüssel für {roomType} {roomForBooking.RoomName} in {hotel.Name}."));

//                // Bestätigungen an Gast senden. FERTIG!
//                sendNotificationToPlayer($"Vielen Dank für Ihre Buchung! Sie haben soeben {roomType} {roomForBooking.RoomName} gebucht. Sie erhalten eine Buchungsbestätigung per SMS.", "Buchung erfolgreich", Constants.NotifactionTypes.Success, player);
//                sendTextMessageToPlayer(phoneNumber, hotel, AllHotelRooms[randomRoomId], $"Herzlichen Dank für Ihre Buchung. Sie haben bei {hotel.Name} - {roomType} {roomForBooking.RoomName} für {selectedHotelRoomRate.RateTickInDays} Tag(e) gebucht. Ihnen wurde ${roomRate} berechnet.");

//                return true;
//            } catch (Exception e) {
//                sendNotificationToPlayer("Bitte versuchen Sie die Buchung erneut. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-555.", "Fehler bei der Buchung", Constants.NotifactionTypes.Danger, player);
//                Logger.logException(new Exception($"HB-555: Fehler bei der Hotelbuchung, Crash bei Buchung: {e}"));
//                return false;
//            }
//        }

//        private static int[] getFreeRoomIdsToHotelAndRate(int hotelId, HotelRoomRate rate) {
//            List<int> roomsOfHotel = AllHotelAssignments.Where(a => a.Hotel == hotelId).Select(a => a.Room).ToList();

//            List<int> matchingRoomsToRates = AllHotelRooms.Values.Where(r =>
//                roomsOfHotel.Contains(r.Id) && r.RoomClass == rate.RoomClass && r.RoomType == rate.RoomType && !r.IsLocked).Select(r => r.Id).ToList();

//            List<int> occupiedMatchingRooms = AllBookingEntries.Values.Where(b => matchingRoomsToRates.Contains(b.Id) && !b.BookingEnded)
//                .Select(b => b.RoomId).ToList();

//            matchingRoomsToRates.RemoveAll(r => occupiedMatchingRooms.Contains(r));

//            return matchingRoomsToRates.ToArray();
//        }

//        private bool onMenuEventShowPricing(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                List<HotelRoomRate> hotelRoomRates = AllHotelRates.Values.Where(r => r.HotelId == hotel.Id).ToList();

//                hotelRoomRates.Sort((rate1, rate2) => rate1.Rate.CompareTo(rate2.Rate));
//                StringBuilder pricingListStringBuilder = new StringBuilder();
//                pricingListStringBuilder.AppendLine($"Preisliste von {hotel.Name}:");
//                foreach (HotelRoomRate hotelRoomRate in hotelRoomRates) {
//                    pricingListStringBuilder.AppendLine(
//                        $"${hotelRoomRate.Rate} - {roomTypeToText(hotelRoomRate.RoomType)}  {roomClassToText(hotelRoomRate.RoomClass)}");
//                }

//                sendNotificationToPlayer(pricingListStringBuilder.ToString(), $"Preisliste von {hotel.Name}", Constants.NotifactionTypes.Info, player);

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventCancelBooking(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                int listId = (int)data["guestBookingsListMenuItemId"];
//                int[] roomIds = (int[])data["roomIds"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-004.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-004: Fehler bei Abbruch der Hotelbuchung, CEF-Event nicht auffindbar."));
//                    return false;
//                }

//                ListMenuItem.ListMenuItemEvent listMenuItemEvent = new ListMenuItem.ListMenuItemEvent();
//                bool elementFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != listId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, listMenuItemEvent);
//                        elementFound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-026.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-026: Fehler bei Abbruch der Hotelbuchung, List-Menüitem nicht auffindbar."));
//                    return false;
//                }

//                int roomId = roomIds[listMenuItemEvent.currentIndex];

//                BookingEntry bookingEntry = AllBookingEntries.Values.First(b =>
//                    b.GuestId == player.getCharacterId() && b.RoomId == roomId && !b.BookingEnded);

//                evictHotelRoom(bookingEntry.RoomId, player.getCharacterId(), "Stornierung durch Gast.", player, EvictionReason.Canceling);

//                sendNotificationToPlayer(
//                    $"Die Stornierung wurde durchgeführt.",
//                    "Stornierungen durchgeführt.", Constants.NotifactionTypes.Success, player);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventExtendedBooking2(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            // Rate berechnen und dann durchführen.
//            try {
//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                int roomId = (int)data["roomId"];
//                int extentionDays = (int)data["extentionDays"];

//                HotelRoom hotelRoom = AllHotelRooms[roomId];

//                BookingEntry bookingEntry = AllBookingEntries.Values.First(b =>
//                    b.RoomId == roomId && b.GuestId == player.getCharacterId() && !b.BookingEnded);

//                decimal currentRoomRate = AllHotelRates.Values.First(r =>
//                        r.HotelId == hotel.Id && r.RoomClass == hotelRoom.RoomClass && r.RoomType == hotelRoom.RoomType)
//                    .Rate;

//                decimal dayRate = bookingEntry.CurrentRate;
//                if (currentRoomRate != bookingEntry.CurrentRate) {
//                    dayRate = currentRoomRate;
//                }
//                decimal calculatedRate = decimal.Multiply(dayRate, extentionDays);

//                string roomType = "Zimmer";
//                if (hotel.HotelType == HotelTypes.Apartments) {
//                    roomType = "Wohnung";
//                } else if (hotel.HotelType == HotelTypes.Buildings) {
//                    roomType = "Haus";
//                }

//                using (var db = new ChoiceVDb()) {
//                    long bankAccount = long.Parse(db.characters.First(c => c.id == player.getCharacterId()).bankaccount);

//                    //Kontostand prüfen.
//                    if (db.bankaccounts.First(b => b.id == bankAccount).balance < calculatedRate) {
//                        sendNotificationToPlayer("Fehler bei der Buchung. Leider ist Ihr persönliches Bankkonto nicht ausreichend gedeckt.", "Zu wenig Geld auf Konto", Constants.NotifactionTypes.Warning, player);
//                        return false;
//                    }

//                    //Geld transferieren
//                    if (!BankController.transferMoney(bankAccount, hotel.BankAccountId, calculatedRate,
//                        $"Raumbuchung bei {hotel.Name} für {roomType} {hotelRoom.RoomName}. ({extentionDays} Tag(e)).", player)
//                    ) {
//                        sendNotificationToPlayer("Fehler bei der Buchung. Die Transaktion wurde durch die Bank abgebrochen. Versuchen Sie es bitte erneut oder wenden Sie sich an Ihre Bank.", "Transaktionsfehler", Constants.NotifactionTypes.Danger, player);
//                        return false;
//                    }

//                    hotelbookings hotelBookings = db.hotelbookings.First(b => b.id == bookingEntry.Id);
//                    if (hotelBookings.enddate == null) {
//                        hotelBookings.enddate = DateTime.Now;
//                    }

//                    hotelBookings.enddate = hotelBookings.enddate + TimeSpan.FromDays(extentionDays);
//                    hotelBookings.currentrate = dayRate;

//                    bookingEntry.EndDate = hotelBookings.enddate;
//                    bookingEntry.CurrentRate = hotelBookings.currentrate;

//                    db.SaveChanges();
//                }

//                // Bestätigungen an Gast senden. FERTIG!
//                sendNotificationToPlayer($"Vielen Dank für Ihre Buchung! Sie haben soeben {roomType} {hotelRoom.RoomName} gebucht. Sie erhalten eine Buchungsbestätigung per SMS.", "Buchung erfolgreich", Constants.NotifactionTypes.Success, player);
//                sendTextMessageToPlayer(bookingEntry.PhoneNumber, hotel, hotelRoom, $"Herzlichen Dank für Ihre Buchung. Sie haben bei {hotel.Name} - {roomType} {hotelRoom.RoomName} für {extentionDays} Tag(e) gebucht. Ihnen wurde ${calculatedRate} berechnet.");

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventExtendedBooking1(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            // Rate berechnen und dann durchführen.
//            try {

//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                int guestBookingsListMenuItemId = (int)data["guestBookingsListMenuItemId"];
//                int extentionDaysListMenuItemId = (int)data["extentionDaysListMenuItemId"];
//                string[] extensionDays = (string[])data["extensionDays"];
//                int[] roomIds = (int[])data["roomIds"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-006.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-006: Fehler bei Abbruch der Hotelbuchung, CEF-Event nicht auffindbar."));
//                    return false;
//                }

//                ListMenuItem.ListMenuItemEvent guestBookingsListMenuItem = new ListMenuItem.ListMenuItemEvent();
//                ListMenuItem.ListMenuItemEvent extentionDaysListMenuItem = new ListMenuItem.ListMenuItemEvent();
//                bool guestBookingsListMenuItemFound = false;
//                bool extentionDaysListMenuItemFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id == guestBookingsListMenuItemId && !guestBookingsListMenuItemFound) {
//                        try {
//                            JsonConvert.PopulateObject(eventElement, guestBookingsListMenuItem);
//                            guestBookingsListMenuItemFound = true;
//                        } catch (Exception) {
//                            continue;
//                        }
//                    } else if (cefElement.id == extentionDaysListMenuItemId && !extentionDaysListMenuItemFound) {
//                        try {
//                            JsonConvert.PopulateObject(eventElement, extentionDaysListMenuItem);
//                            extentionDaysListMenuItemFound = true;
//                        } catch (Exception) {
//                            continue;
//                        }
//                    } else {
//                        continue;
//                    }

//                    if (guestBookingsListMenuItemFound && extentionDaysListMenuItemFound) {
//                        break;
//                    }
//                }

//                if (!guestBookingsListMenuItemFound || !extentionDaysListMenuItemFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-007.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-007: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                int roomId = roomIds[guestBookingsListMenuItem.currentIndex];

//                if (!Int32.TryParse(extensionDays[extentionDaysListMenuItem.currentIndex], out var extentionDays)) {
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-008.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-008: Fehler bei Verlängerung der Hotelbuchung. Bei Extention-Days steht {extensionDays[extentionDaysListMenuItem.currentIndex]}"));
//                    return false;
//                }

//                BookingEntry bookingEntry = AllBookingEntries.Values.First(b =>
//                    b.GuestId == player.getCharacterId() && b.RoomId == roomId && !b.BookingEnded);

//                HotelRoom hotelRoom = AllHotelRooms[roomId];

//                decimal currentRoomRate = AllHotelRates.Values.First(r =>
//                        r.HotelId == hotel.Id && r.RoomClass == hotelRoom.RoomClass && r.RoomType == hotelRoom.RoomType)
//                    .Rate;

//                decimal dayRate = bookingEntry.CurrentRate;

//                bool hasNewPrice = false;

//                if (currentRoomRate != bookingEntry.CurrentRate) {
//                    hasNewPrice = true;
//                    dayRate = currentRoomRate;
//                }

//                decimal calculatedRate = decimal.Multiply(dayRate, extentionDays);

//                StringBuilder priceStringBuilder = new StringBuilder();
//                priceStringBuilder.AppendLine($"Tagespreis: ${dayRate} Anzahl Tage: {extentionDays}");
//                if (hasNewPrice) {
//                    priceStringBuilder.Append(
//                        $"HINWEIS! Der Preis hat sich von ${bookingEntry.CurrentRate} auf ${dayRate} geändert.");
//                }


//                Menu menu = new Menu(hotel.Name, getShortHotelInfoText(hotel));
//                menu.addMenuItem(new StaticMenuItem($"Buchen für ${calculatedRate,0:0.00}?", priceStringBuilder.ToString(), null));
//                menu.addMenuItem(new ClickMenuItem("Verbindlich buchen.", null, null, "HOTEL_EXTENDBOOKING2", MenuItemStyle.green).withData(new Dictionary<string, dynamic>() { { "hotelId", hotel.Id }, { "roomId", roomId }, { "extentionDays", extentionDays } }));
//                menu.addMenuItem(new ClickMenuItem("Vorgang abbrechen.", null, null, "HOTEL_EXTENDBOOKING3", MenuItemStyle.red));

//                player.showMenu(menu);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventShowBookingLeftDays(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                int[] roomIds = (int[])data["roomIds"];
//                int listId = (int)data["guestBookingsListMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-009.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-009: Fehler bei Anzeige der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                ListMenuItem.ListMenuItemEvent listMenuItemEvent = new ListMenuItem.ListMenuItemEvent();
//                bool elementFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != listId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, listMenuItemEvent);
//                        elementFound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-010.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-010: Fehler bei Anzeige der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                HotelRoom hotelRoom = AllHotelRooms[roomIds[listMenuItemEvent.currentIndex]];

//                BookingEntry bookingEntry = AllBookingEntries.Values.FirstOrDefault(b =>
//                    b.RoomId == hotelRoom.Id && b.GuestId == player.getCharacterId() && !b.BookingEnded);

//                if (bookingEntry == null) {
//                    sendNotificationToPlayer("Es konnte keine Buchung festgestellt werden", "Keine Buchung gefunden.",
//                        Constants.NotifactionTypes.Warning, player);
//                    return false;
//                }

//                if (bookingEntry.EndDate == null) {
//                    sendNotificationToPlayer("Diese Buchung hat kein Enddatum. Sie wohnen hier auf unbestimmte Zeit.",
//                        "Buchung hat kein Enddatum", Constants.NotifactionTypes.Success, player);
//                    Logger.logWarning($"Buchung mit ID #{bookingEntry.Id} hat kein Enddatum!");
//                    return true;
//                }

//                TimeSpan timeLeft = bookingEntry.EndDate.Value - DateTime.Now;

//                if (timeLeft.Days == 0) {
//                    sendNotificationToPlayer(
//                        $"Diese Buchung endet in {timeLeft.Hours:00}:{timeLeft.Minutes:00}.{timeLeft.Seconds:00}",
//                        "Buchung endet bald.", Constants.NotifactionTypes.Success, player);
//                    return true;
//                } else if (timeLeft.Days == 1) {
//                    sendNotificationToPlayer(
//                        $"Diese Buchung endet in einem Tag und {timeLeft.Hours} Stunde(n).",
//                        "Buchung endet in 1d.", Constants.NotifactionTypes.Success, player);
//                    return true;
//                }

//                sendNotificationToPlayer(
//                    $"Diese Buchung endet in {timeLeft.Days:00} Tagen und {timeLeft.Hours} Stunde(n).",
//                    "Buchung endet in {timeLeft.Days}d.", Constants.NotifactionTypes.Success, player);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventUnlockHotel(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                Hotel hotel = AllHotels[(int)data["hotelId"]];

//                unlockHotel(hotel.Id, player);

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventLockHotel(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {

//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                int hotelLockReasonInputMenuItemId = (int)data["hotelLockReasonInputMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-011.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-011: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                InputMenuItem.InputMenuItemEvent hotelLockReasonInputMenuItem = new InputMenuItem.InputMenuItemEvent();
//                bool elementFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id == hotelLockReasonInputMenuItemId) {
//                        try {
//                            JsonConvert.PopulateObject(eventElement, hotelLockReasonInputMenuItem);
//                            elementFound = true;
//                            break;
//                        } catch (Exception) {
//                            // ReSharper disable once RedundantJumpStatement
//                            continue;
//                        }
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-012.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-012: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                lockHotel(hotel.Id, hotelLockReasonInputMenuItem.input, player);

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventUnlockRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                int roomIdInputMenuItemId = (int)data["roomIdInputMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-013.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-013: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                InputMenuItem.InputMenuItemEvent roomIdInputMenuItem = new InputMenuItem.InputMenuItemEvent();
//                bool elementFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != roomIdInputMenuItemId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, roomIdInputMenuItem);
//                        elementFound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-014.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-014: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }



//                int roomId;

//                if (!int.TryParse(roomIdInputMenuItem.input, out roomId)) {
//                    sendNotificationToPlayer($"Die eingegebene Room-ID {roomIdInputMenuItem.input} ist keine Zahl!", "Eingabe inkorrekt.", Constants.NotifactionTypes.Danger, player);
//                    return false;
//                }

//                unlockHotelRoom(roomId, player);

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventLockRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                int roomLockReasonInputMenuItemId = (int)data["roomLockReasonInputMenuItemId"];
//                int roomIdInputMenuItemId = (int)data["roomIdInputMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-015.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-015: Fehler beim Sperren des Hotelzimmers, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                InputMenuItem.InputMenuItemEvent roomLockReasonInputMenuItem = new InputMenuItem.InputMenuItemEvent();
//                InputMenuItem.InputMenuItemEvent roomIdInputMenuItem = new InputMenuItem.InputMenuItemEvent();

//                bool roomLockReasonInputMenuItemFound = false;
//                bool roomIdInputMenuItemFound = false;

//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id == roomLockReasonInputMenuItemId) {
//                        try {
//                            JsonConvert.PopulateObject(eventElement, roomLockReasonInputMenuItem);
//                            roomLockReasonInputMenuItemFound = true;
//                        } catch (Exception) {
//                            continue;
//                        }
//                    } else if (cefElement.id == roomIdInputMenuItemId) {
//                        try {
//                            JsonConvert.PopulateObject(eventElement, roomIdInputMenuItem);
//                            roomIdInputMenuItemFound = true;
//                        } catch (Exception) {
//                            continue;
//                        }
//                    } else {
//                        continue;
//                    }

//                    if (roomIdInputMenuItemFound && roomLockReasonInputMenuItemFound) {
//                        break;
//                    }
//                }

//                if (!roomIdInputMenuItemFound || !roomLockReasonInputMenuItemFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-016.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-016: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                if (!Int32.TryParse(roomIdInputMenuItem.input, out var roomId)) {
//                    sendNotificationToPlayer($"Die eingegebene Room-ID ({roomIdInputMenuItem.input}) ist keine Zahl.", "Fehler in Eingabe.", Constants.NotifactionTypes.Danger, player);
//                    return false;
//                }

//                lockHotelRoom(roomId, roomLockReasonInputMenuItem.input, player);

//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventEvictRoom(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {

//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                int evictionReasonInputMenuItemId = (int)data["evictionReasonInputMenuItemId"];
//                int roomIdInputMenuItemId = (int)data["roomIdInputMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-017.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-017: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                InputMenuItem.InputMenuItemEvent evictionReasonInputMenuItem = new InputMenuItem.InputMenuItemEvent();
//                InputMenuItem.InputMenuItemEvent roomIdInputMenuItem = new InputMenuItem.InputMenuItemEvent();

//                bool evictionReasonInputMenuItemFound = false;
//                bool roomIdInputMenuItemFound = false;

//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id == evictionReasonInputMenuItemId) {
//                        try {
//                            JsonConvert.PopulateObject(eventElement, evictionReasonInputMenuItem);
//                            evictionReasonInputMenuItemFound = true;
//                        } catch (Exception) {
//                            continue;
//                        }
//                    } else if (cefElement.id == roomIdInputMenuItemId) {
//                        try {
//                            JsonConvert.PopulateObject(eventElement, roomIdInputMenuItem);
//                            roomIdInputMenuItemFound = true;
//                        } catch (Exception) {
//                            continue;
//                        }
//                    } else {
//                        continue;
//                    }

//                    if (evictionReasonInputMenuItemFound && roomIdInputMenuItemFound) {
//                        break;
//                    }
//                }

//                if (!evictionReasonInputMenuItemFound || !roomIdInputMenuItemFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-018.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-018: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                int roomId;

//                if (!Int32.TryParse(roomIdInputMenuItem.input, out roomId)) {
//                    sendNotificationToPlayer(
//                        $"Die eingegebene Raum-ID ist keine Zahl. Ihre Eingabe: {roomIdInputMenuItem.input}.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    return false;
//                }

//                if (!AllHotelAssignments.Any(a => a.Hotel == hotel.Id && a.Room == roomId)) {
//                    sendNotificationToPlayer(
//                        $"Die eingegebene Raum-ID (#{roomId}) gehört nicht zu diesem Hotel.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    return false;
//                }

//                BookingEntry bookingEntry = AllBookingEntries.Values.FirstOrDefault(b => b.RoomId == roomId && !b.BookingEnded);

//                if (bookingEntry == null) {
//                    sendNotificationToPlayer($"Es besteht keine aktive Buchung zur Raum-ID #{roomId}.", "Raum bereits geräumt.", Constants.NotifactionTypes.Info, player);
//                    return true;
//                }

//                evictHotelRoom(roomId, bookingEntry.GuestId, evictionReasonInputMenuItem.input, player);

//                sendNotificationToPlayer(
//                    $"Räumung von Raum-ID #{roomId} durchgeführt.",
//                    "Räumung durchgeführt.", Constants.NotifactionTypes.Success, player);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventShowBookingInfo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                int roomIdInputMenuItemId = (int)data["roomIdInputMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-019.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-019: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                InputMenuItem.InputMenuItemEvent roomIdInputMenuItem = new InputMenuItem.InputMenuItemEvent();

//                bool elementFound = false;

//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != roomIdInputMenuItemId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, roomIdInputMenuItem);
//                        elementFound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-020.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-020: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }



//                int roomId;

//                if (!int.TryParse(roomIdInputMenuItem.input, out roomId)) {
//                    sendNotificationToPlayer($"Die eingegebene Room-ID {roomIdInputMenuItem.input} ist keine Zahl!", "Eingabe inkorrekt.", Constants.NotifactionTypes.Danger, player);
//                    return false;
//                }

//                BookingEntry bookingEntry = AllBookingEntries.Values.FirstOrDefault(b =>
//                    b.GuestId == player.getCharacterId() && b.RoomId == roomId && !b.BookingEnded);

//                if (bookingEntry == null && AllBookingEntries.Any()) {
//                    try {
//                        bookingEntry = AllBookingEntries.Values.Where(b =>
//                            b.GuestId == player.getCharacterId() && b.RoomId == roomId).Aggregate((b1, b2) => b1.EndDate > b2.EndDate ? b1 : b2);
//                    }
//                    // ReSharper disable once EmptyGeneralCatchClause
//                    catch (Exception) {
//                    }
//                }

//                if (bookingEntry == null) {
//                    sendNotificationToPlayer($"Der Raum {roomId} hat noch keine Buchungsinformationen.", "Keine Buchungsinfo.", Constants.NotifactionTypes.Info, player);
//                    return true;
//                }

//                characters character = getCharacterToId(bookingEntry.GuestId);
//                StringBuilder bookingInfoStringBuilder = new StringBuilder();
//                bookingInfoStringBuilder.AppendLine("Buchungsinfos:");
//                bookingInfoStringBuilder.AppendLine($"Hotel: {hotel.Name}");
//                bookingInfoStringBuilder.AppendLine($"Raum: {AllHotelRooms[roomId].RoomName}");
//                bookingInfoStringBuilder.AppendLine($"Gebucht durch: {character.firstname} {character.lastname} (#{character.id})");
//                bookingInfoStringBuilder.AppendLine($"Von: {bookingEntry.StartDate.ToShortDateString()}, {bookingEntry.StartDate.ToShortTimeString()}");
//                bookingInfoStringBuilder.AppendLine(
//                    bookingEntry.EndDate.HasValue
//                        ? $"Bis: {bookingEntry.EndDate.Value.ToShortDateString()}, {bookingEntry.EndDate.Value.ToShortTimeString()}"
//                        : "Bis: -offen-");

//                sendNotificationToPlayer(bookingInfoStringBuilder.ToString(), "Buchungsinformation",
//                    Constants.NotifactionTypes.Info, player);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventCancelAllGuestRooms(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {

//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                int menuId = (int)data["guestIdInputMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-021.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-021: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                InputMenuItem.InputMenuItemEvent inputMenuItemEvent = new InputMenuItem.InputMenuItemEvent();
//                bool elementFound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != menuId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, inputMenuItemEvent);
//                        elementFound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }
//                }

//                if (!elementFound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-022.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-022: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                int guestId;

//                if (!Int32.TryParse(inputMenuItemEvent.input, out guestId)) {
//                    sendNotificationToPlayer(
//                        "Die Gast-ID ist keine Zahl! Ihre Eingabe: {inputMenuItemEvent.input}",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    return false;
//                }

//                List<BookingEntry> bookingEntries = AllBookingEntries.Values.Where(b =>
//                    b.GuestId == guestId && !b.BookingEnded &&
//                    AllHotelAssignments.Any(a => a.Hotel == hotel.Id && a.Room == b.RoomId)).ToList();

//                foreach (BookingEntry entry in bookingEntries) {
//                    evictHotelRoom(entry.RoomId, guestId, "Stornierung ihrer Buchung durch das Management.", player, EvictionReason.Canceling);
//                }

//                sendNotificationToPlayer(
//                    $"{bookingEntries.Count} Stornierungen durchgeführt.",
//                    "Stornierungen durchgeführt.", Constants.NotifactionTypes.Success, player);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventShowGuestInfo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                Hotel hotel = AllHotels[(int)data["hotelId"]];
//                int guestIdInputMenuItemId = (int)data["guestIdInputMenuItemId"];

//                MenuStatsMenuItem.StatsMenuItemEvent cefEvent;
//                if (data.ContainsKey("PreviousCefEvent")) {
//                    cefEvent = data["PreviousCefEvent"] as MenuStatsMenuItem.StatsMenuItemEvent;
//                } else {
//                    cefEvent = menuItemCefEvent as MenuStatsMenuItem.StatsMenuItemEvent;
//                }

//                if (cefEvent == null) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-023.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-023: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }

//                InputMenuItem.InputMenuItemEvent guestIdInputMenuItem = new InputMenuItem.InputMenuItemEvent();
//                bool elementfound = false;
//                foreach (string eventElement in cefEvent.elements) {
//                    MenuItemCefEvent cefElement = new MenuItemCefEvent();

//                    try {
//                        JsonConvert.PopulateObject(eventElement, cefElement);
//                    } catch (Exception) {
//                        continue;
//                    }

//                    if (cefElement.id != guestIdInputMenuItemId) {
//                        continue;
//                    }

//                    try {
//                        JsonConvert.PopulateObject(eventElement, guestIdInputMenuItem);
//                        elementfound = true;
//                        break;
//                    } catch (Exception) {
//                        // ReSharper disable once RedundantJumpStatement
//                        continue;
//                    }

//                }

//                if (!elementfound) {
//                    //Menü-List-Item lässt sich nicht mehr ermitteln.
//                    sendNotificationToPlayer(
//                        "Bitte führen Sie den Vorgang erneut durch. Erscheint diese Meldung wiederholt, wenden Sie sich an den Support mit der Fehlermeldung #HB-024.",
//                        "Fehler in der Hotelfunktion", Constants.NotifactionTypes.Danger, player);
//                    Logger.logException(new Exception($"HB-024: Fehler bei Verlängerung der Hotelbuchung, CEF-Menüs nicht auffindbar."));
//                    return false;
//                }


//                if (!int.TryParse(guestIdInputMenuItem.input, out var guestId)) {
//                    sendNotificationToPlayer($"Die eingegebene Personen-ID {guestIdInputMenuItem.input} ist keine Zahl!", "Eingabe inkorrekt.", Constants.NotifactionTypes.Danger, player);
//                    return false;
//                }

//                characters characterToId = getCharacterToId(guestId);


//                List<BookingEntry> bookingEntries = AllBookingEntries.Values.Where(b => b.GuestId == guestId).ToList();

//                bookingEntries.RemoveAll(b => !AllHotelAssignments.Any(a => a.Hotel == hotel.Id && a.Room == b.RoomId));

//                if (bookingEntries.Any(b => !b.BookingEnded)) {
//                    bookingEntries.RemoveAll(b => b.BookingEnded);
//                } else if (bookingEntries.Any()) {
//                    //Nur den letzten Eintrag
//                    BookingEntry bookingEntry = bookingEntries.Aggregate((b1, b2) => b1.EndDate > b2.EndDate ? b1 : b2);
//                    bookingEntries.Clear();
//                    bookingEntries.Add(bookingEntry);
//                }

//                if (!bookingEntries.Any()) {
//                    sendNotificationToPlayer($"Der {characterToId.firstname} {characterToId.lastname} hat in diesem Hotel keine Buchungsinformationen.", "Keine Buchungsinfo.", Constants.NotifactionTypes.Info, player);
//                    return true;
//                }

//                StringBuilder bookingInfoStringBuilder = new StringBuilder();
//                bookingInfoStringBuilder.AppendLine("Buchungsinfos:");
//                bookingInfoStringBuilder.AppendLine($"Gast: {characterToId.firstname} {characterToId.lastname} (#{characterToId.id})");

//                foreach (BookingEntry bookingEntry in bookingEntries) {
//                    if (bookingEntry.EndDate.HasValue) {
//                        bookingInfoStringBuilder.AppendLine(
//                            $"Raum {AllHotelRooms[bookingEntry.Id]} (#{bookingEntry.Id}), {bookingEntry.StartDate.ToShortDateString()} {bookingEntry.StartDate.ToShortTimeString()} - {bookingEntry.EndDate.Value.ToShortDateString()} {bookingEntry.EndDate.Value.ToShortTimeString()}");
//                    } else {
//                        bookingInfoStringBuilder.AppendLine(
//                            $"Raum {AllHotelRooms[bookingEntry.Id]} (#{bookingEntry.Id}), {bookingEntry.StartDate.ToShortDateString()} {bookingEntry.StartDate.ToShortTimeString()} - -offen-");
//                    }
//                }

//                sendNotificationToPlayer(bookingInfoStringBuilder.ToString(), "Buchungsinformation",
//                    Constants.NotifactionTypes.Info, player);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }
//        }

//        private bool onMenuEventExtendedBooking3(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            try {
//                sendNotificationToPlayer("Vorgang wunschgemäß abgebrochen.", "Vorgang abgebrochen.", Constants.NotifactionTypes.Info, player);
//                return true;
//            } catch (Exception e) {
//                Logger.logException(e);
//                return false;
//            }

//        }

//        private static string getShortHotelInfoText(Hotel hotel) {
//            StringBuilder shortHotelInfoText = new StringBuilder();
//            if (hotel.HotelType == HotelTypes.Apartments) shortHotelInfoText.Append("Ihre Appartmentvermietung");
//            if (hotel.HotelType == HotelTypes.Buildings) shortHotelInfoText.Append("Ihre Gebäudevermietung");
//            if (hotel.HotelType == HotelTypes.CapsuleHotel) shortHotelInfoText.Append("Ihr Kapselhotel");
//            if (hotel.HotelType == HotelTypes.ExtendedStay) shortHotelInfoText.Append("Ihr Langzeithotel");
//            if (hotel.HotelType == HotelTypes.FullService) shortHotelInfoText.Append("Ihr Hotel");
//            if (hotel.HotelType == HotelTypes.InternationalLuxury) shortHotelInfoText.Append("Ihr Internationales-Luxushotel");
//            if (hotel.HotelType == HotelTypes.LifestyleLuxury) shortHotelInfoText.Append("Ihr Themen-Luxushotel");
//            if (hotel.HotelType == HotelTypes.LimitedServices) shortHotelInfoText.Append("Ihr Economy-Hotel");
//            if (hotel.HotelType == HotelTypes.LoveHotel) shortHotelInfoText.Append("Ihr Liebeshotel");
//            if (hotel.HotelType == HotelTypes.MicroStay) shortHotelInfoText.Append("Ihr Kurzzeithotel");
//            if (hotel.HotelType == HotelTypes.Motel) shortHotelInfoText.Append("Ihr Motel");
//            if (hotel.HotelType == HotelTypes.SelectService) shortHotelInfoText.Append("Ihr Businesshotel");
//            shortHotelInfoText.Append(" ");
//            if (hotel.HotelType == HotelTypes.CapsuleHotel || hotel.HotelType == HotelTypes.ExtendedStay ||
//                hotel.HotelType == HotelTypes.FullService || hotel.HotelType == HotelTypes.InternationalLuxury ||
//                hotel.HotelType == HotelTypes.LifestyleLuxury || hotel.HotelType == HotelTypes.LimitedServices ||
//                hotel.HotelType == HotelTypes.LoveHotel || hotel.HotelType == HotelTypes.MicroStay ||
//                hotel.HotelType == HotelTypes.SelectService || hotel.HotelType == HotelTypes.Motel) {
//                if (hotel.HotelStars == 1) {
//                    shortHotelInfoText.Append("(1 Stern)");
//                } else {
//                    shortHotelInfoText.Append($"({hotel.HotelStars} Sterne)");
//                }
//            } else {
//                if (hotel.HotelStars <= 1) shortHotelInfoText.Append("(Spartanisch)");
//                else if (hotel.HotelStars <= 3) shortHotelInfoText.Append("(Gehoben)");
//                else if (hotel.HotelStars <= 5) shortHotelInfoText.Append("(Luxuriös)");
//                else shortHotelInfoText.Append("(Extraklasse)");
//            }
//            return shortHotelInfoText.ToString();
//        }

//        internal static string roomTypeToText(RoomTypes roomType) {
//            if (roomType == RoomTypes.Apartment) return "Wohnung";
//            if (roomType == RoomTypes.Bungalow) return "Bungalow";
//            if (roomType == RoomTypes.Cottage) return "Häuschen";
//            if (roomType == RoomTypes.Dormitory) return "Schlafsaal";
//            if (roomType == RoomTypes.DoubleBedroom) return "Doppelzimmer";
//            if (roomType == RoomTypes.JuniorSuite) return "Junior Suite";
//            if (roomType == RoomTypes.Suite) return "Suite";
//            if (roomType == RoomTypes.Maisonette) return "Maisonette";
//            if (roomType == RoomTypes.MultiBedroom) return "Mehrbettzimmer";
//            if (roomType == RoomTypes.Penthouse) return "Penthouse";
//            if (roomType == RoomTypes.SingleBedroom) return "Einzelzimmer";
//            if (roomType == RoomTypes.Studio) return "Einzimmerwohnung";
//            throw new Exception("Unbekannter Raumtypus");
//        }

//        internal static string roomClassToText(RoomClasses roomClass) {
//            if (roomClass == RoomClasses.Low) return "Einfach";
//            if (roomClass == RoomClasses.Medium) return "Mittel";
//            if (roomClass == RoomClasses.High) return "Gehoben";
//            if (roomClass == RoomClasses.Superior) return "Stark gehoben";
//            throw new Exception("Unbekannte Raumklasse");
//        }

//        public static string firstCharToUpper(string input) =>
//            input switch
//            {
//                null => throw new ArgumentNullException(nameof(input)),
//                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
//                _ => input.First().ToString().ToUpper() + input.Substring(1)
//            };

//        public static void setHotelBankAccountId(int hotelId, long bankAccountId, IPlayer player) {
//            try {
//                Hotel hotel = AllHotels[hotelId];

//                using (var db = new ChoiceVDb()) {

//                    if (!BankController.accountIdExists(bankAccountId)) {
//                        sendNotificationToPlayer(
//                            $"Das angegebene Bankkonto #{bankAccountId} konnte nicht gefunden werden.",
//                            "Bankkonto ungültig.", Constants.NotifactionTypes.Warning, player);
//                        return;
//                    }

//                    hotel.BankAccountId = bankAccountId;

//                    confighotelbuilding holteBuilding = db.confighotelbuilding.FirstOrDefault(h => h.id == hotelId);
//                    if (holteBuilding == null) {
//                        sendNotificationToPlayer(
//                            $"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}",
//                            "Datenbankfehler", Constants.NotifactionTypes.Danger, player);
//                        throw new Exception(
//                            $"FEHLER IN DATENBANK! Hotel konnte nicht gefunden werden! Bitte Support mit diesen Daten informieren! Hotel-ID: {hotelId}");
//                    }

//                    holteBuilding.bankAccountId = bankAccountId;
//                    db.SaveChanges();
//                    sendNotificationToPlayer($"Das Konto von {hotel.Name} lautet nun #{bankAccountId}",
//                        "Bankkonto gesetzt.", Constants.NotifactionTypes.Success, player);
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public static void doLockAllUnbookedRooms() {
//            try {
//                List<int> bookedRooms = AllBookingEntries.Values.Where(b => !b.BookingEnded).Select(b => b.RoomId).ToList();
//                List<HotelRoom> hotelRoomsToLock = AllHotelRooms.Values.Where(r => !bookedRooms.Contains(r.Id)).ToList();

//                List<DoorController.Door> doors = DoorController.AllDoors.Values.Where(d => hotelRoomsToLock.Any(r =>
//                    (r.DoorId == null && string.Equals(r.DoorGroup, d.GroupName, StringComparison.OrdinalIgnoreCase) ||
//                     (r.DoorId.HasValue && d.Id == r.DoorId.Value)))).ToList();

//                DoorController.LockDoors(doors, true);

//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        public static void setHotelDropout(IPlayer player, int hotelId) {
//            try {
//                Hotel hotel = AllHotels[hotelId];

//                Position positionWithRotation = player.getCharacterData().LastPosition;

//                hotel.DropoutPosition = positionWithRotation;

//                using (var db = new ChoiceVDb()) {
//                    confighotelbuilding hotelBuilding = db.confighotelbuilding.First(h => h.id == hotelId);
//                    hotelBuilding.dropout_position = positionWithRotation.ToJson();
//                    db.SaveChanges();
//                }
//                sendNotificationToPlayer("Registrierung des Dropouts erfolgreich.", "Dropout gesetzt.", Constants.NotifactionTypes.Danger, player);
//                return;
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//            sendNotificationToPlayer("Registrierung des Dropouts nicht erfolgreich.", "Dropout nicht gesetzt.", Constants.NotifactionTypes.Danger, player);
//        }


//    }
//}