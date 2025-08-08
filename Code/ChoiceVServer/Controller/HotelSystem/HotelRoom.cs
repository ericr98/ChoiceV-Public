using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.HotelSystem {
    public class HotelRoom : IDisposable {
        public int Id { get; private set; }
        public int Level { get; private set; }
        public int RoomNumber { get; private set; }
        public CollisionShape CollisionShape;

        public Hotel Hotel { get; private set; }


        public List<HotelRoomFeature> Features { get; private set; }

        public List<Door> AllDoors;

        public int MaxBookings { get; private set; }

        public decimal Price { get; private set; }

        public HotelRoom(int id, Hotel hotel, int level, int roomNumber, int maxBookings, decimal price, CollisionShape collisionShape, List<Door> doors, List<HotelRoomFeature> features) {
            Id = id;
            Hotel = hotel;
            Level = level;
            RoomNumber = roomNumber;
            CollisionShape = collisionShape;
            AllDoors = doors;
            Features = features;
            MaxBookings = maxBookings;
            Price = price;
        }

        public bool keyWorksForRoom(int triggerDimension, DoorKey key) {
            foreach(var door in AllDoors) {
                if(door.fitsForKey(triggerDimension, key)) {
                    return true;
                }
            }

            return false;
        }

        public void addFeature(HotelRoomFeature feature) {
            Features.Add(feature);
        }

        public void Dispose() {
            CollisionShape.Dispose();
            foreach(var feature in Features) {
                feature.Dispose();
            }

            Features.Clear();
        }
    }

    public class HotelRoomBooking : IDisposable {
        public int Id;
        public int CharId;

        public HotelRoom HotelRoom { get; private set; }
        public DateTime NextPay { get; private set; }

        private string ReserverName;
        public long ContactPhoneNumber { get; private set; }
        public long BankAccount { get; private set; }

        public decimal PrepaidAmount { get; private set; }

        public List<HotelRoomFeatureInstance> FeatureInstances { get; private set; }

        public int Dimension { get; private set; }

        public HotelRoomBooking(int id, int charId, string reserverName, long bankAccount, long contactPhoneNumber, HotelRoom hotelRoom, int dimension, DateTime nextPay) {
            Id = id;
            ReserverName = reserverName;
            ContactPhoneNumber = contactPhoneNumber;
            BankAccount = bankAccount;
            PrepaidAmount = 0;
            CharId = charId;
            HotelRoom = hotelRoom;
            Dimension = dimension;
            NextPay = nextPay;

            FeatureInstances = new List<HotelRoomFeatureInstance>();
        }

        public HotelRoomBooking withPrepaidAmount(decimal amount) {
            PrepaidAmount = amount;
            return this;
        }

        public void setFeatureInstances(List<HotelRoomFeatureInstance> featureInstances) {
            FeatureInstances = featureInstances;
        }

        public void setContactPhoneNumber(long newNumber) {
            ContactPhoneNumber = newNumber;

            using(var db = new ChoiceVDb()) {
                var dbBooking = db.hotelroombookings.Find(Id);
                if(dbBooking != null) {
                    dbBooking.contactPhoneNumber = newNumber;
                    db.SaveChanges();
                }
            }
        }

        public void setBankaccount(long newBankaccount) {
            BankAccount = newBankaccount;

            using(var db = new ChoiceVDb()) {
                var dbBooking = db.hotelroombookings.Find(Id);
                if(dbBooking != null) {
                    dbBooking.bankAccount = newBankaccount;
                    db.SaveChanges();
                }
            }
        }

        public void changePrepaidAmount(decimal amount) {
            PrepaidAmount += amount;

            using(var db = new ChoiceVDb()) {
                var dbBooking = db.hotelroombookings.Find(Id);
                if(dbBooking != null) {
                    dbBooking.prepaidAmount = PrepaidAmount;
                    db.SaveChanges();
                }
            }
        }

        public void changeNextPay(TimeSpan amount) {
            NextPay += amount;

            using(var db = new ChoiceVDb()) {
                var dbBooking = db.hotelroombookings.Find(Id);
                if(dbBooking != null) {
                    dbBooking.nextPay = NextPay;
                    db.SaveChanges();
                }
            }
        }

        public int getRoomNumber() {
            return HotelRoom.RoomNumber;
        }

        public Menu getBookingMenu(IPlayer player) {
            var data = new Dictionary<string, dynamic> {
                { "Booking", this },
            };

            var bookingEditMenu = new Menu("Buchungen bearbeiten", "Bearbeite deine Buchungen");

            bookingEditMenu.addMenuItem(new StaticMenuItem("Rauminfos", $"Die Buchung ist für Raum {HotelRoom.RoomNumber} auf Etage {Math.Abs(Dimension)}", $"Nummer {HotelRoom.RoomNumber}, Etage {Math.Abs(Dimension)}"));
            bookingEditMenu.addMenuItem(new StaticMenuItem("Namen von Kunden", $"Der hinterlegte Name ist {ReserverName}", $"{ReserverName}"));
            bookingEditMenu.addMenuItem(new StaticMenuItem("Preis", $"Der Preis pro Buchungszyklus beträgt: ${HotelRoom.Price}", $"${HotelRoom.Price}"));
            var nextPayHour = Math.Abs(Math.Round((DateTime.Now - NextPay).TotalHours));
            bookingEditMenu.addMenuItem(new StaticMenuItem("Nächste Bezahlung", $"Deine nächste Bezahlung ist in ca. {nextPayHour}h fällig", $"in {nextPayHour}h"));

            var longPhoneStr = "Du hast keine Kontakttelefonnumer gewählt";
            var shortPhoneStr = "Akt. keine gewählt";
            if(ContactPhoneNumber != -1) {
                longPhoneStr = $"Das akt. ausgewählte Kontakttelefonnumer ist {ContactPhoneNumber}";
                shortPhoneStr = $"{ContactPhoneNumber}";
            }

            bookingEditMenu.addMenuItem(new InputMenuItem("Kontaktnummer ändern", longPhoneStr, shortPhoneStr, "HOTEL_CHANGE_BOOKING_PHONENUMBER").withData(data).needsConfirmation("Kontaktnummer ändern?", "Kontaktnummer wirklich ändern?"));

            var longBankStr = "Du hast kein Bankkonto für automatische Zahlung gewählt";
            var shortBankStr = "Akt. keins gewählt";
            if(BankAccount != -1) {
                longBankStr = $"Das akt. ausgewählte Bankkonto für automatische Zahlung ist {BankAccount}";
                shortBankStr = $"{BankAccount}";
            }

            bookingEditMenu.addMenuItem(new ClickMenuItem("Neues Standardkonto", longBankStr, shortBankStr, "HOTEL_CHANGE_BOOKING_BANKACCOUNT").withData(data).needsConfirmation("Bankkonto ändern?", "Neues Standardkonto auswählen?"));
            bookingEditMenu.addMenuItem(new InputMenuItem("Bargeld vorzahlen", "Zahle Bargeld ein um deinen Raum vorzubezahlen in $", $"aktuell: {PrepaidAmount}", "HOTEL_ADD_BOOKING_PREPAID_AMOUNT").withData(data).needsConfirmation("Bargeld einzahlen?", "Bargeld wirklich einzahlen?"));
            bookingEditMenu.addMenuItem(new ClickMenuItem("Neuen Schlüssel kaufen", "Kaufe dir einen weiteren Schlüssel für dein Raum", "$50", "HOTEL_CREATE_NEW_BOOKING_KEY").withData(data).needsConfirmation("Neuen Schlüssel kaufen?", "Schlüssel wirklich für $50 kaufen?"));
            bookingEditMenu.addMenuItem(new ClickMenuItem("Schloss austauschen lassen", "Lasse das Schloß austauschen. Macht alle Schlüssel ungültig", "$200", "HOTEL_CHANGE_BOOKING_LOCK_INDEX").withData(data).needsConfirmation("Schloss ändern?", "Schloss wirklich für $200 ändern?"));

            bookingEditMenu.addMenuItem(new ClickMenuItem("Buchung auflösen", "Löse die Buchung auf. Du gibst deine Schlüssel wieder ab!", "", "HOTEL_DELETE_BOOKING", MenuItemStyle.red).withData(data).needsConfirmation("Buchung auflösen?", "Buchung wirklich auflösen?"));

            return bookingEditMenu;
        }

        public void Dispose() {
            HotelRoom = null;
            foreach(var instance in FeatureInstances) {
                instance.Dispose();
            }
            FeatureInstances.Clear();
        }
    }
}
