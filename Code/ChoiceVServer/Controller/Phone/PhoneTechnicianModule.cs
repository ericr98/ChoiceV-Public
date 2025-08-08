using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.HotelSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Phone {
    public class PhoneTechnicianController : ChoiceVScript {
        public PhoneTechnicianController() {            
            PedController.addNPCModuleGenerator("Telefontechniker", onSupportPhoneTechnicianGenerator, onSupportonSupportPhoneTechnicianCallback);

            EventController.addMenuEvent("PERSONALISE_SIM_NUMBER", onPersonaliseSimNumber);
            EventController.addMenuEvent("SUBMIT_PIN_FOR_NUMBER_PERSONALISING", onSubmitPinForNumberPersonalising);
            EventController.addMenuEvent("CHANGE_PERSONALISED_NUMBER_PIN", onChangePersonalisedNumberPin);
        }

        private List<MenuItem> onSupportPhoneTechnicianGenerator(ref Type codeType) {
            codeType = typeof(PhoneTechnicianNPCModule);

            return new List<MenuItem> { };
        }

        private void onSupportonSupportPhoneTechnicianCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { });
        }

        private bool onPersonaliseSimNumber(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var menu = new Menu("Nummer-PIN eingeben", "Gib den PIN der Nummer ein");
            menu.addMenuItem(new InputMenuItem("PIN", "Gib den PIN der Nummer ein", "", InputMenuItemTypes.password, ""));
            menu.addMenuItem(new MenuStatsMenuItem("PIN eingeben", "Gib den PIN für die SIM ein", "SUBMIT_PIN_FOR_NUMBER_PERSONALISING", MenuItemStyle.green)
                                                            .withData(data));
            player.showMenu(menu);

            return true;
        }

        private bool onSubmitPinForNumberPersonalising(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = data["Item"] as SIMCard;
            var number = data["Number"] as phonenumber;

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var pinEvt = evt.elements[0].FromJson<InputMenuItemEvent>();

            if(number.pin == pinEvt.input) {
                using(var db = new ChoiceVDb()) {
                    var dbNumber = db.phonenumbers.Find(number.number);

                    dbNumber.characterOwnerId = player.getCharacterId();

                    db.SaveChanges();

                    player.sendNotification(Constants.NotifactionTypes.Success, "Die Nummer wurde erfolgreich personalisiert!", "Nummer personalisiert");
                }
            } else {
                player.sendBlockNotification("Der eingegebene PIN ist falsch!", "PIN falsch", Constants.NotifactionImages.Smartphone);
            }

            return true;
        }

        private bool onChangePersonalisedNumberPin(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var number = data["Number"] as phonenumber;

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var pinEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var pinConfirmEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            if(pinEvt.input != pinConfirmEvt.input) {
                player.sendBlockNotification("Die PINs stimmen nicht überein!", "Fehler beim Setzen der PIN", NotifactionImages.Smartphone);
            } else {
                using(var db = new ChoiceVDb()) {
                    var dbNumber = db.phonenumbers.Find(number.number);

                    if(dbNumber != null) {
                        dbNumber.pin = pinEvt.input;

                        db.SaveChanges();
                    } else {
                        player.sendBlockNotification("Es ist ein Fehler aufgetreten! Bitte melde dich im Support!", "Bitte im Support melden!");
                        return false;
                    }
                }

                player.sendNotification(NotifactionTypes.Success, "SIM-PIN erfolgreich geändert!", "SIM-PIN geändert", NotifactionImages.Smartphone);
            }


            return true;
        }
    }

    public class PhoneTechnicianNPCModule : NPCModule {
        public PhoneTechnicianNPCModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped) {
            
        }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var menu = new Menu("Telefonnummer-Verwaltung", "Was möchtest du tun?");
            var sims = player.getInventory().getItems<SIMCard>(i => true);

            using(var db = new ChoiceVDb()) {
                foreach(var sim in sims) {
                    var dbNumber = db.phonenumbers.Find(sim.Number);
                    if(dbNumber.characterOwnerId == null) {
                        menu.addMenuItem(new ClickMenuItem("SIM-Nummer personalisieren", $"Personalisiere die Nummer {dbNumber.number} der SIM. Um weite Optionen für die Nummer zu ermöglichen (z.B. PIN ändern) muss die Nummer personalisiert werden.", $"{dbNumber.number}", "PERSONALISE_SIM_NUMBER")
                            .withData(new Dictionary<string, dynamic> { { "Number", dbNumber }, { "Item", sim } }));
                    }
                }

                var charId = player.getCharacterId();
                var phoneNumbers = db.phonenumbers.Where(p => p.characterOwnerId == charId);

                foreach(var number in phoneNumbers) {
                    var subMenu = new Menu($"Pers. Nummer: {number.number}", "Was möchtest du tun?");

                    subMenu.addMenuItem(new ClickMenuItem("Neue SIM Karte ausgeben", "Lasse dir eine neue SIM Karte für diese Nummer ausgeben", $"$15", "GET_NEW_SIM_FOR_NUMBER")
                        .needsConfirmation("Neue SIM kaufen?", "Wirklich neue SIM kaufen?"));

                    var changePinMenu = PhoneController.getPhonePINSetMenu("CHANGE_PERSONALISED_NUMBER_PIN", new Dictionary<string, dynamic> { { "Number", number } }, false);
                    subMenu.addMenuItem(new MenuMenuItem(changePinMenu.Name, changePinMenu));

                    menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                }
            }

            return new List<MenuItem> { new MenuMenuItem(menu.Name, menu) };
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Smartphone-Techniker Modul", $"Erlaubt es Spielern SIM Karten und Nummern auf sich zu registrieren und PINs zurückzusetzen", $"");
        }

        public override void onRemove() { }
    }
}
