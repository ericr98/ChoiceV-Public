using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.HotelSystem {
    public class Hotel : IDisposable {
        public static string[] PayOptions = { "Bankkonto", "Bar" };

        public int Id { get; private set; }
        public string Name { get; private set; }

        public List<HotelRoom> Rooms { get; private set; }
        public Dictionary<int, HotelRoomBooking> HotelRoomBookings { get; private set; }

        public long BankAccount { get; private set; }
        public long PhoneNumber { get; private set; }

        public Position CheckInPosition { get; private set; }

        public Hotel(int id, string name, long phoneNumber, Position checkInPosition, long bankAccount, Dictionary<int, HotelRoomBooking> hotelRoomBookings) {
            Id = id;
            Name = name;
            PhoneNumber = phoneNumber;
            BankAccount = bankAccount;
            HotelRoomBookings = hotelRoomBookings;

            CheckInPosition = checkInPosition;

            Rooms = new List<HotelRoom>();
        }

        public void setRooms(List<HotelRoom> rooms) {
            Rooms = rooms;
        }

        public Menu getCheckInMenu(IPlayer player) {
            var data = new Dictionary<string, dynamic> {
                { "Hotel", this },
            };

            var menu = new Menu("Hotel Check-In", "Was möchtest du tun?");

            if(HotelRoomBookings.ContainsKey(player.getCharacterId())) {
                var bookingMenu = HotelRoomBookings[player.getCharacterId()].getBookingMenu(player);
                menu.addMenuItem(new MenuMenuItem(bookingMenu.Name, bookingMenu));
            } else {
                var distinctRooms = Rooms.Where(r => hasRoomVacancy(r)).Select(r => r.Price.ToString()).Distinct().ToArray();
                if(distinctRooms.Length != 0) {
                    var newBookingMenu = new Menu("Zimmer buchen", "Buche ein Zimmer");
                    newBookingMenu.addMenuItem(new InputMenuItem("Namen hinterlegen", "Hinterlege deinen Namen für das Hotelzimmer", "", ""));
                    newBookingMenu.addMenuItem(new ListMenuItem("Preisklasse wählen", "Wähle die Preisklasse für dein Zimmer", distinctRooms, ""));
                    newBookingMenu.addMenuItem(new ListMenuItem("Zahloptionen", "Wähle wie das Zimmer bezahlt wird. Bei Barzahlung werden 3 Tage mind. hinterlegt", PayOptions, ""));
                    newBookingMenu.addMenuItem(new CheckBoxMenuItem("Telefonnumer hinterlegen", $"Hinterlege die Nummer {player.getMainPhoneNumber()} für Infos. Kann noch geändert werden.", true, ""));

                    newBookingMenu.addMenuItem(new MenuStatsMenuItem("Buchung bestätigen", "Bestätige die aktuelle Buchung.", "HOTEL_CREATE_NEW_BOOKING", MenuItemStyle.green).withData(data).needsConfirmation("Buchung durchführen?", "Buchung wirklich durchführen?"));
                    menu.addMenuItem(new MenuMenuItem(newBookingMenu.Name, newBookingMenu));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Das Hotel ist voll!", "Alle Zimmer dieses Hotels sind ausgebucht!", ""));
                }
            }

            var remainMenu = new Menu("Hotelsammelstelle", "Erhalte zurückgelassene Sachen zurück");
            using(var db = new ChoiceVDb()) {
                var charId = player.getCharacterId();
                var dbInstances = db.hotelroomfeatureinstances.Include(h => h.hotelroomfeatureinstancesdata).Where(h => h.savedCharId == charId).ToList();

                foreach(var dbInstance in dbInstances) {
                    var type = Type.GetType("ChoiceVServer.Controller.HotelSystem." + dbInstance.codeFeature, false);
                    var instance = Activator.CreateInstance(type, dbInstance, null, null) as HotelRoomFeatureInstance;
                    remainMenu.addMenuItem(instance.createLateMenuItem().withData(new Dictionary<string, dynamic> { { "Instance", instance } }));
                }
            }

            menu.addMenuItem(new MenuMenuItem(remainMenu.Name, remainMenu));

            return menu;
        }

        public bool hasRoomVacancy(HotelRoom room) {
            return room.MaxBookings > HotelRoomBookings.Values.Where(b => b.HotelRoom == room).Count();
        }

        public void addBooking(HotelRoomBooking booking) {
            HotelRoomBookings.Add(booking.CharId, booking);
        }

        public void removeBooking(HotelRoomBooking booking) {
            HotelRoomBookings.Remove(booking.CharId);
        }

        public void setBankAccount(long bankAccount) {
            BankAccount = bankAccount;
        }

        public void update(DateTime now) {
            foreach(var booking in HotelRoomBookings.Values.Reverse()) {
                if(booking.NextPay <= now) {
                    if(booking.BankAccount != -1) {
                        if(BankController.transferMoney(booking.BankAccount, BankAccount, booking.HotelRoom.Price, $"Reservierung Zimmer {booking.getRoomNumber()} im {Name}", out var returnMsg, false)) {
                            if(booking.ContactPhoneNumber != -1) {
                                PhoneController.sendSMSToNumber(PhoneNumber, booking.ContactPhoneNumber, $"Es wurden ${booking.HotelRoom.Price} für ihre Reservierung im {Name} von ihrem Konto abgehoben.");
                            }
                            booking.changeNextPay(TimeSpan.FromDays(1));

                            continue;
                        }
                    }

                    if (booking.PrepaidAmount >= booking.HotelRoom.Price) {
                        booking.changePrepaidAmount(-booking.HotelRoom.Price);
                        if (booking.ContactPhoneNumber != -1) {
                            PhoneController.sendSMSToNumber(PhoneNumber, booking.ContactPhoneNumber, $"Es wurden ${booking.HotelRoom.Price} für ihre Reservierung im {Name} von ihren Vorzahlungen abgehoben.");
                        }
                        booking.changeNextPay(TimeSpan.FromDays(1));
                    } else {
                        HotelController.deleteBooking(booking, true);
                    }
                }
            }
        }

        public void Dispose() {
            foreach(var room in Rooms) {
                room.Dispose();
            }
            Rooms.Clear();

            foreach(var booking in HotelRoomBookings.Values) {
                booking.Dispose();
            }
            HotelRoomBookings = null;
        }
    }
}
