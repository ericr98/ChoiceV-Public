//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model.Clothing;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Controller {
//    class RadioController : ChoiceVScript {
//        public static List<ActiveRadio> activeRadios = new List<ActiveRadio>();
//        public static int radioCounter;
//        public RadioController() {

//            EventController.addMenuEvent("JOIN_RADIO", onRadioJoin);
//            EventController.addMenuEvent("RADIO_STATUS", onRadioStatus);
//            EventController.addMenuEvent("RADIO_HEADPHONES", onHeadPhone);
//            EventController.addMenuEvent("RADIO_SHOW_ACTIVE", onRadioShowActive);
//            EventController.addMenuEvent("RADIO_INTERACT", onRadioInteract);

//            EventController.addMenuEvent("JOIN_RADIO_STRESSTEST", onRadioJoinStresstest);

//            EventController.addKeyEvent(ConsoleKey.Oem1, onRadioMenu);

//            EventController.MainReadyDelegate += getCounter;
//        }

//        private bool onRadioJoinStresstest(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var primary = data["RadioPrimary"];
//            var radio = (Radio)data["Radio"];
//            var radioType = data["RadioType"];
//            var channel = "";
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            if (inputItem != null) {
//                var tryParse = double.TryParse(inputItem.input, out var amount);
//                if (tryParse) {
//                    if (amount <= 0) {
//                        player.sendBlockNotification("Keine negativen Frequenzen!", "");
//                        return true;
//                    } else {
//                        channel = inputItem.input;
//                        VoiceChatController.JoinRadio(player, inputItem.input, false, true);
//                    }
//                } else {
//                    player.sendBlockNotification("Die Frequenz konnt nicht aufgelöst werden!", "");
//                    return true;
//                }
//                var radioCheck = activeRadios.FirstOrDefault(x => x.radio == radio);
//                if (radioCheck != null) {
//                    if (primary) {
//                        radioCheck.primaryChannel = channel;
//                        radio.LastPrimaryChannel = channel;
//                    } else {
//                        radioCheck.secondaryChannel = channel;
//                        radio.LastSecondaryChannel = channel;
//                    }
//                }
//            } else {
//                channel = data["RadioChannel"];
//                VoiceChatController.JoinRadio(player, channel, primary, true);
//                var radioCheck = activeRadios.FirstOrDefault(x => x.radio == radio);
//                if (radioCheck != null) {
//                    if (primary) {
//                        radioCheck.primaryChannel = channel;
//                        radio.LastPrimaryChannel = channel;
//                    } else {
//                        radioCheck.secondaryChannel = channel;
//                        radio.LastSecondaryChannel = channel;
//                    }
//                }
//            }
//            return true;
//        }

//        private bool onRadioInteract(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var radio = (Radio)data["Radio"];
//            var radioType = (string)data["RadioType"];
//            var status = (bool)data["Deactivate"];
//            if (status) {
//                if (radioType == "State") {
//                    radio.EmergencyBlock = true;
//                }
//                if (radioType == "MerryWeather") {
//                    radio.MerryWeatherBlock = true;
//                }
//                player.sendNotification(NotifactionTypes.Success, $"Radio mit der ID: {radio.RadioId} gesperrt!", "Radio gesperrt");
//                var radioCheck = activeRadios.FirstOrDefault(x => x.radio == radio);
//                if (radioCheck != null) {
//                    var target = ChoiceVAPI.FindPlayerByCharId(radioCheck.charId);
//                    if (target != null) {
//                        VoiceChatController.LeaveRadio(target, true);
//                        VoiceChatController.LeaveRadio(target, false);
//                    }
//                }
//            } else {
//                if (radioType == "State") {
//                    radio.EmergencyBlock = false;
//                }
//                if (radioType == "MerryWeather") {
//                    radio.MerryWeatherBlock = false;
//                }
//            }
//            return true;
//        }

//        private bool onRadioShowActive(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var menu = new Menu("Funkgerät verwaltung", "Verwaltet die Funkgeräte");
//            var activeMenu = getActiveRadio(player, false);
//            var deactivatedMenu = getActiveRadio(player, true);
//            menu.addMenuItem(new MenuMenuItem("Aktive Funkgeräte", activeMenu));
//            menu.addMenuItem(new MenuMenuItem("Gespertte aktive Funkgeräte", deactivatedMenu));
//            player.showMenu(menu);
//            return true;
//        }

//        private bool onRadioMenu(IPlayer player, ConsoleKey key, string eventName) {
//            var item = (Radio)player.getInventory().getItem(x => x is Radio);
//            if (item != null) {
//                item.use(player);
//            }
//            return true;
//        }

//        private bool onHeadPhone(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var toggle = data["Radio_Speaker"];
//            VoiceChatController.setRadioSpeaker(player, toggle);
//            if (toggle) {
//                if (player.hasData("Radio_Headphones")) {
//                    player.resetPermantData("Radio_Headphones");
//                    var configItem = InventoryController.AllConfigItems.FirstOrDefault(i => i.codeItem == typeof(RadioHeadPhones).Name);
//                    var item = new RadioHeadPhones(configItem);
//                    player.getInventory().addItem(item);
//                    var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//                    clothing.UpdateClothSlot(1, 0, 0);
//                    ClothingController.loadPlayerClothing(player, clothing);
//                }
//            } else {
//                player.setPermanentData("Radio_Headphones", "true");
//                var item = player.getInventory().getItem(x => x is RadioHeadPhones);
//                player.getInventory().removeItem(item);
//                var clothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_SAVE);
//                clothing.UpdateClothSlot(1, 121, 0);
//                ClothingController.loadPlayerClothing(player, clothing);
//            }
//            return true;
//        }

//        private bool onRadioStatus(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var radio = (Radio)data["Radio"];
//            var status = data["Status"];
//            if (radio != null) {
//                radio.RadioStatus = status;
//            }
//            if (status) {
//                player.sendNotification(Constants.NotifactionTypes.Info, "Du hast dein Funkgerät eingeschaltet", "Funkgerät");
//            } else {
//                player.sendNotification(Constants.NotifactionTypes.Info, "Du hast dein Funkgerät ausgeschaltet", "Funkgerät");
//                VoiceChatController.LeaveRadio(player, true);
//                VoiceChatController.LeaveRadio(player, false);
//            }
//            return true;
//        }

//        private bool onRadioJoin(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            var primary = data["RadioPrimary"];
//            var channel = data["RadioChannel"];
//            var radio = (Radio)data["Radio"];
//            var radioType = data["RadioType"];
//            if (channel == "Own") {
//                if (string.IsNullOrEmpty(inputItem.input)) {
//                    player.sendBlockNotification("Der Input darf nicht leer sein", "");
//                    return true;
//                } else {
//                    channel = inputItem.input;
//                }
//            }
//            if (radioType == "State" && radio.EmergencyBlock) {
//                player.sendNotification(NotifactionTypes.Info, "Dein Funkgerät konnte keine Verbindung herstellen", "Keine Verbindung");
//                return true;
//            }
//            if (radioType == "MerryWeather" && radio.EmergencyBlock) {
//                player.sendNotification(NotifactionTypes.Info, "Dein Funkgerät konnte keine Verbindung herstellen", "Keine Verbindung");
//                return true;
//            }
//            var radioCheck = activeRadios.FirstOrDefault(x => x.radio == radio);
//            if (radioCheck != null) {
//                if (primary) {
//                    radioCheck.primaryChannel = channel;
//                    radio.LastPrimaryChannel = channel;
//                } else {
//                    radioCheck.secondaryChannel = channel;
//                    radio.LastSecondaryChannel = channel;
//                }
//            }
//            VoiceChatController.JoinRadio(player, channel, primary, true);
//            return true;
//        }

//        public static Menu getChannelMenu(IPlayer player, Radio radio, bool primaryChannel) {
//            var menu = new Menu("Funkchannel", "Wähle deine Aktion");
//            var companies = CompanyController.getCompanies(player);
//            if (primaryChannel) {
//                menu.addMenuItem(new ClickMenuItem("ChoiceV Funkkreis", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "Emergency" }, { "RadioPrimary", true }, { "Radio", radio }, { "RadioType", "State" } }));
//            } else {
//                menu.addMenuItem(new InputMenuItem("Eigener Funkkreis", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "Own" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//            }
//            /*foreach (var company in companies) {
//                if (company.Type == Model.Company.CompanyType.Fbi) {
//                    if (primaryChannel) {
//                        menu.addMenuItem(new ClickMenuItem("Emergency Funkkreis", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "Emergency" }, { "RadioPrimary", true }, { "Radio", radio }, { "RadioType", "State" } }));
//                    } else if (!(primaryChannel)){
//                        var fbiMenu = new Menu("FBI Funk", "Funk für FBI-Mitarbeiter");
//                        fbiMenu.addMenuItem(new ClickMenuItem("FBI Funkkreis", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "FBI1" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        menu.addMenuItem(new MenuMenuItem("FBI Funk", fbiMenu));
//                    }
//                }
//                if (company.Type == Model.Company.CompanyType.Police || company.Type == Model.Company.CompanyType.Fbi) {
//                    if (primaryChannel && !(company.Type == Model.Company.CompanyType.Fbi)) {
//                        menu.addMenuItem(new ClickMenuItem("Emergency Funkkreis", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "Emergency" }, { "RadioPrimary", true }, { "Radio", radio }, { "RadioType", "State" } }));
//                    } else if (!(primaryChannel)) {
//                        var pdMenu = new Menu("LSPD Funk", "Funk für LSPD-Mitarbeiter");
//                        pdMenu.addMenuItem(new ClickMenuItem("LSPD Funkkreis 1", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "LSPD1" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        pdMenu.addMenuItem(new ClickMenuItem("LSPD Funkkreis 2", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "LSPD2" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        pdMenu.addMenuItem(new ClickMenuItem("LSPD Funkkreis 3", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "LSPD3" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        pdMenu.addMenuItem(new ClickMenuItem("Polizei Ausbildung", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "PD_Academy" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        pdMenu.addMenuItem(new ClickMenuItem("Polizei Sonderfunk", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "PD_Extra" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));

//                        var sdMenu = new Menu("LSSD Funk", "Funk für LSSD-Mitarbeiter");
//                        sdMenu.addMenuItem(new ClickMenuItem("LSSD Funkkreis 1", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "LSSD1" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        sdMenu.addMenuItem(new ClickMenuItem("LSSD Funkkreis 2", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "LSSD2" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        sdMenu.addMenuItem(new ClickMenuItem("Polizei Ausbildung", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "PD_Academy" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        sdMenu.addMenuItem(new ClickMenuItem("Polizei Sonderfunk", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "PD_Extra" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));

//                        menu.addMenuItem(new MenuMenuItem("LSPD Funk", pdMenu));
//                        menu.addMenuItem(new MenuMenuItem("LSSD Funk", sdMenu));
//                    }
//                }
//                if (company.Type == Model.Company.CompanyType.Fire || company.Type == Model.Company.CompanyType.Fbi) {
//                    if (primaryChannel && !(company.Type == Model.Company.CompanyType.Fbi)) {
//                        menu.addMenuItem(new ClickMenuItem("Emergency Funk", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "Emergency" }, { "RadioPrimary", true }, { "Radio", radio }, { "RadioType", "State" } }));
//                    } else if (!(primaryChannel)) {
//                        var fdMenu = new Menu("LSFD Funk", "Funk für LSFD-Mitarbeiter");
//                        fdMenu.addMenuItem(new ClickMenuItem("LSFD Funkkreis 1", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "LSFD1" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        fdMenu.addMenuItem(new ClickMenuItem("LSFD Funkkreis 2", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "LSFD2" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        fdMenu.addMenuItem(new ClickMenuItem("LSFD Ausbildung", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "FD_Academy" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "State" } }));
//                        menu.addMenuItem(new MenuMenuItem("LSFD Funk", fdMenu));
//                    }
//                }
//                if (company.Type == Model.Company.CompanyType.Merryweather) {
//                    if (primaryChannel) {
//                        menu.addMenuItem(new ClickMenuItem("Merryweather Funkkreis", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "MWP1" }, { "RadioPrimary", true }, { "Radio", radio }, { "RadioType", "MerryWeather" } }));
//                    } else if (!(primaryChannel)) {
//                        menu.addMenuItem(new ClickMenuItem("Merryweather Alfa", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "MW1" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "MerryWeather" } }));
//                        menu.addMenuItem(new ClickMenuItem("Merryweather Bravo", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "MW2" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "MerryWeather" } }));
//                        menu.addMenuItem(new ClickMenuItem("Merryweather Charlie", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "MW3" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "MerryWeather" } }));
//                        menu.addMenuItem(new ClickMenuItem("Merryweather Delta", "", "", "JOIN_RADIO").withData(new Dictionary<string, dynamic> { { "RadioChannel", "MW4" }, { "RadioPrimary", false }, { "Radio", radio }, { "RadioType", "MerryWeather" } }));
//                    } 
//                } 
//            } */
//            return menu;
//        }

//        public static void showRadioMenu(IPlayer player, bool status, Radio radio) {
//            var menu = new Menu("Funk", "Wähle deine Aktion!");
//            if (status) {
//                menu.addMenuItem(new MenuMenuItem("Primären Funkchannel wählen", getChannelMenu(player, radio, true)));
//                menu.addMenuItem(new MenuMenuItem("Sekundären Funkchannel wählen", getChannelMenu(player, radio, false)));
//                menu.addMenuItem(new ClickMenuItem("Funkgerät abschalten", "", "", "RADIO_STATUS").withData(new Dictionary<string, dynamic> { { "Radio", radio }, { "Status", false } }));
//                if (player.getInventory().hasItem(x => x is RadioHeadPhones) && !(player.hasData("Radio_Headphones"))) {
//                    menu.addMenuItem(new ClickMenuItem("Kopfhöhrer verwenden", "Schließt die Kopfhöhrer an das Funkgerät an", "", "RADIO_HEADPHONES").withData(new Dictionary<string, dynamic> { { "Radio_Speaker", false } }));
//                }
//                if (player.hasData("Radio_Headphones")) {
//                    menu.addMenuItem(new ClickMenuItem("Kopfhöhrer wegpacken", "Entfernt die Kopfhöhrer von dem Funkgerät", "", "RADIO_HEADPHONES").withData(new Dictionary<string, dynamic> { { "Radio_Speaker", true } }));
//                }
//                menu.addMenuItem(new ClickMenuItem("Funkgerät verwaltung", "Zeigt dir alle aktiven Funkgeräte in deinen Funkkreisen", "", "RADIO_SHOW_ACTIVE"));
//            } else {
//                menu.addMenuItem(new ClickMenuItem("Funkgerät einschalten", "", "", "RADIO_STATUS").withData(new Dictionary<string, dynamic> { { "Radio", radio }, { "Status", true } }));
//            }
//            player.showMenu(menu);
//        }

//        public static Menu getActiveRadio(IPlayer player, bool blocked) {
//            var menu = new Menu("Aktive Funkgeräte", "");
//            var policeCheck = false;
//            var fbiCheck = false;
//            var fireCheck = false;
//            var merryCheck = false;
//            var companies = CompanyController.getCompanies(player);
//            foreach (var company in companies) {
//                if (company.Type == Model.Company.CompanyType.Police) { policeCheck = true; }
//                if (company.Type == Model.Company.CompanyType.Fbi) { fbiCheck = true; }
//                if (company.Type == Model.Company.CompanyType.Fire) { fireCheck = true; }
//                if (company.Type == Model.Company.CompanyType.Merryweather) { merryCheck = true; }
//            }
//            if (!(blocked)) {
//                foreach (var channel in activeRadios) {
//                    if ((channel.secondaryChannel.Contains("LSPD") || channel.secondaryChannel.Contains("LSSD") || channel.secondaryChannel.Contains("Polizei") && policeCheck) || fbiCheck) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät deaktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "State" }, { "Deactivate", true } }));
//                        continue;
//                    }
//                    if ((channel.secondaryChannel.Contains("LSFD") && fireCheck) || fbiCheck) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät deaktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "State" }, { "Deactivate", true } }));
//                        continue;
//                    }
//                    if (channel.secondaryChannel.Contains("FBI") && fbiCheck) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät deaktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "State" }, { "Deactivate", true } }));
//                        continue;
//                    }
//                    if (channel.secondaryChannel.Contains("Merryweather") && merryCheck) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät deaktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "MerryWeather" }, { "Deactivate", true } }));
//                        continue;
//                    }
//                }
//            } else {
//                foreach (var channel in activeRadios) {
//                    if (((channel.secondaryChannel.Contains("LSPD") || channel.secondaryChannel.Contains("LSSD") || channel.secondaryChannel.Contains("Polizei") && policeCheck) || fbiCheck) && channel.radio.EmergencyBlock) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät aktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "State" }, { "Deactivate", false } }));
//                        continue;
//                    }
//                    if (((channel.secondaryChannel.Contains("LSFD") && fireCheck) || fbiCheck) && channel.radio.EmergencyBlock) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät aktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "State" }, { "Deactivate", false } }));
//                        continue;
//                    }
//                    if ((channel.secondaryChannel.Contains("FBI") && fbiCheck) && channel.radio.EmergencyBlock) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät aktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "State" }, { "Deactivate", false } }));
//                        continue;
//                    }
//                    if ((channel.secondaryChannel.Contains("Merryweather") && merryCheck) && channel.radio.MerryWeatherBlock) {
//                        menu.addMenuItem(new ClickMenuItem($"Funkgerät ID: {channel.radio.RadioId}", "Funkgerät aktivieren", $"{channel.secondaryChannel}", "RADIO_INTERACT").withData(new Dictionary<string, dynamic> { { "Radio", channel.radio }, { "RadioType", "MerryWeather" }, { "Deactivate", false } }));
//                        continue;
//                    }
//                }
//            }
//            return menu;
//        }

//        public static void getCounter() {
//            using (var db = new ChoiceVDb()) {
//                var radio = db.configitems.FirstOrDefault(x => x.codeItem == typeof(Radio).Name);
//                radioCounter = int.Parse(radio.additionalInfo);
//            }
//        }
//    }

//    public class ActiveRadio {
//        public int charId { get; set; }
//        public Radio radio { get; set; }
//        public int radioId { get; set; }
//        public string primaryChannel { get; set; }
//        public string secondaryChannel { get; set; }
//    }
//}
