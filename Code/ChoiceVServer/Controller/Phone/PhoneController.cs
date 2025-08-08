using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.OrderSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Phone {
    public class PhoneController : ChoiceVScript {
        public class PhoneAnswerEvent : IPlayerCefEvent {
            public string Event { get; private set; }
            public string subEvt;

            public PhoneAnswerEvent(string subEvt) {
                Event = "PHONE_APP_EVENT";
                this.subEvt = subEvt;
            }
        }

        private static readonly long LOS_SANTOS_PRE_NUMBER = 12100000000;
        public PhoneController() {
            EventController.addKeyEvent("PHONE_OPEN", ConsoleKey.PageUp, "Smartphone öffnen", onOpenSmartphone);
            EventController.addKeyEvent("PHONE_CLOSE", ConsoleKey.PageDown, "Smartphone schließen", onCloseSmartphone, true, true);

            EventController.addMenuEvent("EQUIP_SMARTPHONE", onEquipSmartphone);
            EventController.addMenuEvent("SUBMIT_PIN_FOR_SMARTPHONE_EQUIP", onSubmitPinForPhoneEquip);
            EventController.addMenuEvent("UNEQUIP_SMARTPHONE", onUnequipSmartphone);
            EventController.addMenuEvent("EQUIP_SIM_CARD", onEquipSIMCard);
            EventController.addMenuEvent("SET_PIN_FOR_SIM_AND_EQUIP", setPinForSIMandEquip);
            EventController.addMenuEvent("SUBMIT_PIN_FOR_SIM_AND_EQUIP", submitPinForSimAndEquip);
            EventController.addMenuEvent("UNEQUIP_SIM_CARD", onUnequipSIMCard);

            EventController.addCefEvent("PHONE_CHANGE_APP", onSmartphoneChangeApp);

            CharacterSettingsController.addListCharacterSettingBlueprint(
                "PHONE_ANIMATION", "1", "Smartphone Animation", "Wähle aus welche Smartphone Animation benutzt werden soll",
                new Dictionary<string, string> { { "1", "Animation: Tippen" }, { "2", "Animation: Hochhalten" }, { "3", "Animation: Seitlich" } }
            );
        }

        private bool onOpenSmartphone(IPlayer player, ConsoleKey key, string eventName) {
            var hasPhone = player.getInventory().getItem<Smartphone>(i => i.Selected) != null;
            if(!player.getBusy() && hasPhone) {
                var evt = new OnlyEventCefEvent("OPEN_PHONE");

                player.addState(PlayerStates.OnPhone);
                player.emitCefEventNoBlock(evt);
                player.emitClientEvent("TOGGLE_SMARTPHONE", true);

                if(player.hasData("ANIMATION")) {
                    var playingAnim = (Animation)player.getData("ANIMATION");
                    var phoneAnim = AnimationController.getAnimationByName("PHONE");
                    if(playingAnim.Dictionary == phoneAnim.Dictionary && playingAnim.Name == phoneAnim.Name) {
                        return true;
                    }
                }

                switch(player.getCharSetting("PHONE_ANIMATION")) {
                    case "1":
                        player.playAnimation(AnimationController.getAnimationByName("PHONE_OPEN"), null, false);
                        break;
                    case "2":
                        player.playAnimation(AnimationController.getAnimationByName("PHONE_OPEN_2"), null, false);
                        break;
                    case "3":
                        player.playAnimation(AnimationController.getAnimationByName("PHONE_OPEN_3"), null, false);
                        break;
                }
            }

            return true;
        }

        private bool onCloseSmartphone(IPlayer player, ConsoleKey key, string eventName) {
            if(player.hasState(PlayerStates.OnPhone)) {
                if(player.hasData("ANIMATION")) {
                    var playingAnim = (Animation)player.getData("ANIMATION");
                    var phoneAnim = AnimationController.getAnimationByName("PHONE");
                    if(playingAnim.Dictionary != phoneAnim.Dictionary || playingAnim.Name != phoneAnim.Name) {
                        player.stopAnimation();
                    }
                } else {
                    player.stopAnimation();
                }

                var evt = new OnlyEventCefEvent("CLOSE_PHONE");

                player.removeState(PlayerStates.OnPhone);

                player.emitCefEventNoBlock(evt);
                player.emitClientEvent("TOGGLE_SMARTPHONE", false);
            }
            return true;
        }

        private bool onEquipSmartphone(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Smartphone)data["Item"];

            if(item.SIMCardInventory.hasItem<SIMCard>(s => true)) {
                var dbNumber = getDbForNumber(item.SIMCardInventory.getItem<SIMCard>(s => true).Number);

                if(dbNumber != null && dbNumber.pin != null) {
                    var menu = new Menu("Primär SIM PIN eingeben", "Gib die PIN für die Primär SIM ein!");
                    menu.addMenuItem(new InputMenuItem("PIN", "Gib den PIN für die SIM ein", "", InputMenuItemTypes.password, ""));

                    menu.addMenuItem(new MenuStatsMenuItem("PIN eingeben", "Gib den PIN für die SIM ein", "SUBMIT_PIN_FOR_SMARTPHONE_EQUIP", MenuItemStyle.green)
                                                            .withData(new Dictionary<string, dynamic> { { "Item", item } }));

                    player.showMenu(menu);
                } else {
                    item.equip(player);
                }
            } else {
                item.equip(player);
            }

            return true;
        }

        private phonenumber getDbForNumber(long number) {
            using(var db = new ChoiceVDb()) {
                var phoneNumber = db.phonenumbers.Find(number);

                if(phoneNumber != null) {
                    return phoneNumber;
                } else {
                    return null;
                }
            }
        }

        private bool onSubmitPinForPhoneEquip(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Smartphone)data["Item"];

            var sim = item.SIMCardInventory.getItem<SIMCard>(s => true);

            if(sim != null) {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
                var pinEvt = evt.elements[0].FromJson<InputMenuItemEvent>();

                var dbNumber = getDbForNumber(sim.Number);
                if(dbNumber == null || pinEvt.input != dbNumber.pin) {
                    player.sendBlockNotification("Diese PIN passt nicht zu der Nummer!", "Fehler beim Prüfen der PIN", NotifactionImages.Smartphone);
                } else {
                    item.equip(player);
                    player.sendNotification(NotifactionTypes.Success, "Smartphone erfolgreich ausgerüstet!", "Smartphone ausgerüstet", NotifactionImages.Smartphone);
                }
            } else {
                Logger.logError($"onSubmitPinForPhoneEquip: SIMCard not found: charid: {player.getCharacterId()}",
                                           $"Fehler im Smarthpone ausrüsten: SIMCard nicht gefunden", player);
            }

            return true;


        }

        private bool onUnequipSmartphone(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Smartphone)data["Item"];

            if(item != null) {
                item.unequip(player);
            } else {
                Logger.logError($"onUnequipSmartphone: smartphone not found: charid: {player.getCharacterId()}",
                        $"Fehler im Smarthpone abrüsten: Smartphone nicht gefunden", player);
            }

            return true;
        }

        private bool onEquipSIMCard(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Smartphone)data["Item"];
            var sim = (SIMCard)data["SIMCard"];
            var inv = (Inventory)data["Inventory"];
            var dbNumber = getDbForNumber(sim.Number);


            if(dbNumber.pin == null) {
                var menu = getPhonePINSetMenu("SET_PIN_FOR_SIM_AND_EQUIP", 
                    new Dictionary<string, dynamic> { { "Item", item }, { "SIMCard", sim }, { "Inventory", inv } }, 
                    true);
                
                player.showMenu(menu);
            } else {
                var menu = new Menu("SIM PIN eingeben", "Gib die PIN für die SIM ein");
                menu.addMenuItem(new InputMenuItem("PIN", "Gib den PIN für die SIM ein", "", InputMenuItemTypes.password, ""));

                menu.addMenuItem(new MenuStatsMenuItem("PIN eingeben", "Gib den PIN für die SIM ein", "SUBMIT_PIN_FOR_SIM_AND_EQUIP", MenuItemStyle.green)
                                                       .withData(new Dictionary<string, dynamic> { { "Item", item }, { "SIMCard", sim }, { "Inventory", inv } }));
                
                player.showMenu(menu);
            }

            return true;
        }

        public static Menu getPhonePINSetMenu(string evt, Dictionary<string, dynamic> data, bool numberPersonalise) {
            var menu = new Menu("SIM PIN setzen", "Setze den PIN für diese SIM");
            menu.addMenuItem(new StaticMenuItem("WICHTIG!", "Setzen Sie eine sichere PIN (auch mehr als 4 Zeichen). Sollte ihr Smartphone/ihre SIM abhanden kommen könnten andere Personen sonst ihre Chats etc. lesen!", "Sichere PIN wählen!"));
            menu.addMenuItem(new InputMenuItem("PIN", "Wähle eine PIN für die SIM. (Speichere sie z.B. in den Spielernotizen!)", "", InputMenuItemTypes.password, ""));
            menu.addMenuItem(new InputMenuItem("PIN bestätigen", "Bestätige die PIN für die SIM. (Speichere sie z.B. in den Spielernotizen!)", "", InputMenuItemTypes.password, ""));
            if(numberPersonalise) {
                menu.addMenuItem(new CheckBoxMenuItem("Nummer personalisieren?", "Möchten sie diese Nummer auf sie personalisieren? Dies verknüpft ihre persönlichen Daten mit der Nummer, ermöglicht es aber auch im Fall des Verlusts der SIM die Nummer zurückzuerhalten.", false, ""));
            }

            menu.addMenuItem(new MenuStatsMenuItem("PIN setzen", "Setze diesen PIN für die SIM", evt, MenuItemStyle.green)
                .needsConfirmation("PIN wirklich setzen?", "Möchtest du diesen PIN wirklich setzen?")
                .withData(data));
            
            return menu;
        }

        private bool setPinForSIMandEquip(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Smartphone)data["Item"];
            var sim = (SIMCard)data["SIMCard"];
            var inv = (Inventory)data["Inventory"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var pinEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var pinConfirmEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var personaliseEvt = evt.elements[3].FromJson<CheckBoxMenuItemEvent>();

            if(pinEvt.input != pinConfirmEvt.input) {
                player.sendBlockNotification("Die PINs stimmen nicht überein!", "Fehler beim Setzen der PIN", NotifactionImages.Smartphone);
            } else {
                using(var db = new ChoiceVDb()) {
                    var dbNumber = db.phonenumbers.Find(sim.Number);

                    if(dbNumber != null) {
                        dbNumber.pin = pinEvt.input;

                        if(personaliseEvt.check) {
                            dbNumber.characterOwnerId = player.getCharacterId();
                        }

                        db.SaveChanges();
                    } else {
                        player.sendBlockNotification("Es ist ein Fehler aufgetreten! Bitte melde dich im Support!", "Bitte im Support melden!");
                        return false;
                    }
                }

                player.getInventory().moveItem(inv, sim);
                item.updateDescription();

                player.sendNotification(NotifactionTypes.Success, "SIM-Karte erfolgreich eingelegt!", "SIM-Karte eingelegt", NotifactionImages.Smartphone);
            }

            return true;
        }


        private bool submitPinForSimAndEquip(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Smartphone)data["Item"];
            var sim = (SIMCard)data["SIMCard"];
            var inv = (Inventory)data["Inventory"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var pinEvt = evt.elements[0].FromJson<InputMenuItemEvent>();

            var dbNumber = getDbForNumber(sim.Number);
            if(dbNumber != null) {
                if(pinEvt.input != dbNumber.pin) {
                    player.sendBlockNotification($"Diese PIN passt nicht zu der SIM!", "Fehler beim Abfragen der PIN", NotifactionImages.Smartphone);
                } else {
                    player.getInventory().moveItem(inv, sim);
                    item.updateDescription();

                    player.sendNotification(NotifactionTypes.Success, "SIM-Karte erfolgreich eingelegt!", "SIM-Karte eingelegt", NotifactionImages.Smartphone);
                }
            }

            return true;
        }

        private bool onUnequipSIMCard(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Smartphone)data["Item"];
            var sim = (SIMCard)data["SIMCard"];
            var inv = (Inventory)data["Inventory"];

            inv.moveItem(player.getInventory(), sim);
            item.updateDescription();
            return true;
        }

        public static string formatPhoneNumber(long number) {
            var numberString = number.ToString();

            if(numberString.Length <= 3) {
                return numberString;
            } else if(numberString.Length <= 6) {
                return $"({numberString.Substring(0, 3)}) {numberString.Substring(3)}";
            } else {
                return $"({numberString.Substring(0, 3)}) {numberString.Substring(3, 3)}-{numberString.Substring(6)}";
            }
        }

        public static IPlayer findPlayerWithNumber(long number) {
            return PhoneCallController.findPlayerWithNumber(number);
        }

        public static void sendSMSToNumber(long from, long to, string text) {
            PhoneMessageController.sendSMSToNumber(from, to, text);
        }

        public static phonenumber createNewPhoneNumber(string comment) {
            var rand = new Random();
            long checkNumber = rand.Next(100000, 999999);
            checkNumber += LOS_SANTOS_PRE_NUMBER;

            using(var db = new ChoiceVDb()) {
                var dbNumber = db.phonenumbers.Find(checkNumber);
                while(dbNumber != null) {
                    checkNumber = rand.Next(100000, 999999);
                    checkNumber += LOS_SANTOS_PRE_NUMBER;
                    dbNumber = db.phonenumbers.Find(checkNumber);
                }

                var newNumber = new phonenumber {
                    number = checkNumber,
                    comment = comment,
                };

                db.phonenumbers.Add(newNumber);
                db.SaveChanges();

                return newNumber;
            }
        }

        public static phonenumber createNewPhoneNumber(long number, string comment) {
            using(var db = new ChoiceVDb()) {
                var already = db.phonenumbers.Find(number);
                if(already != null) {
                    return already;
                } else {
                    var newNumber = new phonenumber {
                        number = number,
                        comment = comment,
                    };

                    db.phonenumbers.Add(newNumber);
                    db.SaveChanges();

                    return newNumber;
                }
            }
        }

        public static phonenumber findPhoneNumberByComment(string comment) {
            using(var db = new ChoiceVDb()) {
                return db.phonenumbers.FirstOrDefault(p => p.comment == comment);
            }
        }

        private class PhoneNotificationEvent : IPlayerCefEvent {
            public string Event { get; private set; }

            public string title;

            public PhoneNotificationEvent(string title) {
                Event = "PHONE_NOTIFICATION";
                this.title = title;
            }
        }

        public static void sendPhoneNotificationToPlayer(IPlayer player, string message) {
            player.emitCefEventNoBlock(new PhoneNotificationEvent(message));
        }

        public static void removePlayerFromCall(PhoneCall call, IPlayer player) {
            PhoneCallController.removePlayerFromCall(call, player);
        }

        public static void sendCallListToPlayer(IPlayer player, long number) {
            PhoneCallController.sendCallListToPlayer(player, number);
        }

        #region ChangeApp

        private class PhoneChangeAppEvent {
            public int itemId;
            public bool stopsMovement;
        }

        private void onSmartphoneChangeApp(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneChangeAppEvent();
            cefData.PopulateJson(evt.Data);

            player.emitClientEvent("TOGGLE_NO_MOVE_APP", cefData.stopsMovement);
        }

        #endregion
    }
}