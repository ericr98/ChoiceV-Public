using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using System;
using System.Linq;
using ChoiceVServer.Controller.Voice;

namespace ChoiceVServer.InventorySystem {
    public class RadioItemController : ChoiceVScript {
        public RadioItemController() {
            EventController.addMenuEvent("PLAYER_ACTIVATE_RADIO", onPlayerActivateRadio);
            EventController.addMenuEvent("PLAYER_DEACTIVATE_RADIO", onPlayerDeactivateRadio);

            EventController.addMenuEvent("PLAYER_JOIN_RADIO_FREQUENCY", onPlayerJoinRadioFrequency); 
            EventController.addMenuEvent("PLAYER_MODIFY_RADIO_FREQUENCY", onPlayerModifyRadioFrequency);

            EventController.addKeyToggleEvent("RADIO_SEND", ConsoleKey.Oem102, "Funkgerät senden", onRadioStartSending, true);
            CharacterSettingsController.addListCharacterSettingBlueprint("RADIO_ANIMATION", "RADIO_SET", "Funkgerät Animation", "Welche Animation beim Funken gezeigt wird",
                new Dictionary<string, string>{{ "RADIO_SET", "An Ohr (groß)" }, { "HOLD_RADIO_1", "Chillig halten" }, { "HOLD_RADIO_2", "Actiongriff" }});
        }


        private bool onPlayerActivateRadio(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var radio = (RadioItem)data["Item"];
            radio.Activated = true;
            player.sendNotification(Constants.NotifactionTypes.Success, "Fungerät angeschalten", "Fungerät angeschalten", Constants.NotifactionImages.Radio);

            radio.use(player);
            return true;
        }

        private bool onPlayerDeactivateRadio(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var radio = (RadioItem)data["Item"];
            radio.Activated = false;

            foreach(var frequency in radio.JointFrequencies) {
                VoiceController.leaveRadioChannel(player, frequency.ToString());
            }

            player.sendNotification(Constants.NotifactionTypes.Warning, "Funkgerät abgeschaltet", "Funkgerät abgeschaltet", Constants.NotifactionImages.Radio);
            return true;
        }

        private bool onPlayerJoinRadioFrequency(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var radio = (RadioItem)data["Item"];

            var evt = menuItemCefEvent as InputMenuItemEvent;
            var frequencyStr = evt.input;
            var index = frequencyStr.IndexOf("Mhz");
            if(index != -1) {
                frequencyStr = frequencyStr.Substring(0, index);
            }

            if(float.TryParse(frequencyStr.ToLower(), out var frequency)) {
                frequency = Convert.ToSingle(Math.Round(frequency, 3));
                frequencyStr = frequency.ToString(); 
                
                if((frequency >= RadioItem.FrequencyStart && frequency <= RadioItem.FrequencyStop) || 
                        CompanyController.hasPlayerCompanyWithPredicate(player, c => c.hasFunctionality<CompanyRadioFrequencyFunctionality>(f => f.hasFrequency(frequency)))) {
                    if(!radio.JointFrequencies.Contains(frequencyStr)) {
                        radio.JointFrequencies.Add(frequencyStr);
                        VoiceController.joinRadioChannel(player, frequencyStr, $"📡 {frequencyStr} Mhz");
                        player.sendNotification(Constants.NotifactionTypes.Success, $"Frequenz {frequencyStr}MHz beigetreten", $"Frequenz {frequencyStr}MHz beigetreten", Constants.NotifactionImages.Radio);

                        radio.use(player);
                    } else {
                        player.sendBlockNotification("Frequenz bereits beigetreten", "Frequenz bereits beigetreten", Constants.NotifactionImages.Radio);
                    }
                } else {
                    player.sendBlockNotification("Frequenz ungültig", "Frequenz ungültig", Constants.NotifactionImages.Radio);
                }
            } else {
                player.sendBlockNotification("Frequenz ungültig", "Frequenz ungültig", Constants.NotifactionImages.Radio);
            }

            return true;
        }

        private bool onPlayerModifyRadioFrequency(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var radio = (RadioItem)data["Item"];
            var frequencyStr = (string)data["Frequency"];

            var evt = menuItemCefEvent as ListMenuItemEvent;

            if (evt.action == "enter") {
                VoiceController.leaveRadioChannel(player, frequencyStr.ToString());

                radio.JointFrequencies.Remove(frequencyStr);
                radio.TalkingFrequencies.Remove(frequencyStr);

                player.sendNotification(Constants.NotifactionTypes.Warning, $"Frequenz {frequencyStr}MHz verlassen", "Frequenz verlassen", Constants.NotifactionImages.Radio);

            } else {
                if (evt.currentElement == "Mikrofon aktiviert") {
                    if (radio.TalkingFrequencies.Contains(frequencyStr)) return true;

                    radio.TalkingFrequencies.Add(frequencyStr);
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Mikrofon auf Frequenz {frequencyStr}MHz aktiviert", $"Mikrofon aktiviert", Constants.NotifactionImages.Radio);
                } else {
                    if (!radio.TalkingFrequencies.Contains(frequencyStr)) return true;

                    radio.TalkingFrequencies.Remove(frequencyStr);
                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Mikrofon auf Frequenz {frequencyStr}MHz deaktiviert", $"Mikrofon deaktiviert", Constants.NotifactionImages.Radio);
                }
            }

            return true;
        }

        private static bool onRadioStartSending(IPlayer player, ConsoleKey key, bool isPressed, string eventName) {
            if(!player.getInventory().hasItem<RadioItem>(i => i.Activated)) {
                if(isPressed) {
                    player.sendBlockNotification("Du hast kein aktiviertes Funkgerät dabei!", "Kein Funkgerät", Constants.NotifactionImages.Radio);
                }
                return false;
            }

            lock(player) {
                if(!player.getBusy([Constants.PlayerStates.InAnimation, Constants.PlayerStates.OnBusyMenu])) {
                    var talkingChannels = player.getInventory().getItem<RadioItem>(r => r.Activated).TalkingFrequencies.Select(f => f.ToString()).ToList();

                    if(isPressed) {
                        var anim = AnimationController.getAnimationByName(CharacterSettingsController.getCharacterSettingValue(player, "RADIO_ANIMATION"));
                        player.playAnimation(anim);
                        foreach(var channel in talkingChannels) {
                            VoiceController.unmuteInRadioChannel(player, channel);
                        }
                    } else {
                        InvokeController.AddTimedInvoke("onRadioStartSending", (i) => {
                            AnimationController.stopAnimation(player);

                            foreach(var channel in talkingChannels) {
                                VoiceController.muteInRadioChannel(player, channel);
                            }
                        }, TimeSpan.FromMilliseconds(400), false);
                    }
                }
            }
            return true;
        }
    }

    public class RadioItem : Item {
        public bool Activated;
        public List<string> JointFrequencies = [];
        public List<string> TalkingFrequencies = [];

        public static readonly float FrequencyStart = 26.965f;
        public static readonly float FrequencyStop = 27.075f;

        public RadioItem(item dbItem) : base(dbItem) { }

        public RadioItem(configitem configItem, int amount, int quality) : base(configItem) { }

        public RadioItem(configitem configItem, Company company) : base(configItem) { }

        public override void use(IPlayer player) {
            var already = player.getInventory().getItem<RadioItem>(r => r.Activated && r != this);
            if(already != null) {
                already.use(player);
                return;
            }

            base.use(player);

            var menu = new Menu("Funkgerät", "Was möchtest du tun?");

            var data = new Dictionary<string, dynamic> {
                { "Item", this },
            };

            if(!Activated) {
                menu.addMenuItem(new ClickMenuItem("Funkgerät einschalten", "Schalte das Funkgerät ein", "", "PLAYER_ACTIVATE_RADIO", MenuItemStyle.green).withData(data));
            } else {
                menu.addMenuItem(new InputMenuItem("Neuer Frequenz beitreten", $"Tritt einer neuen Frequenz bei, um mit anderen zu sprechen. Du bist standardmäßig auf dieser Frequenz nur Zuhörer. Wähle eine Frequenz von {FrequencyStart}Mhz bis {FrequencyStop}Mhz.", "", "PLAYER_JOIN_RADIO_FREQUENCY", MenuItemStyle.green)
                .withData(data));

                foreach(var frequency in JointFrequencies) {
                    var frequencyData = new Dictionary<string, dynamic> {
                        { "Item", this },
                        { "Frequency", frequency },
                    };

                    var list = new string[] { "Mikrofon deaktiviert", "Mikrofon aktiviert" };
                    if(TalkingFrequencies.Contains(frequency)) {
                        list = [ "Mikrofon aktiviert", "Mikrofon deaktiviert" ];
                    }

                    menu.addMenuItem(new ListMenuItem($"Frequenz {frequency}MHz", $"Verwalte die Frequenz {frequency}MHz. Aktiviere das Mikrofon. Durch Klicken entfernst du die Frequenz.", list, "PLAYER_MODIFY_RADIO_FREQUENCY", MenuItemStyle.normal, true).withData(frequencyData));
                }

                menu.addMenuItem(new ClickMenuItem("Funkgerät abschalten", "Schalte das Funkgerät ab", "", "PLAYER_DEACTIVATE_RADIO", MenuItemStyle.red).withData(data));
            }

            player.showMenu(menu);
        }
    }
}
