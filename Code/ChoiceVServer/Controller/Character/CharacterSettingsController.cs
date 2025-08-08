using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    public enum CharacterSettingsType {
        FlagSetting = 0,
        ListSetting = 1,
    }

    public delegate void OnCharacterListSettingChange(IPlayer player, string settingName, string value);
    public delegate void OnCharacterFlagSettingChange(IPlayer player, string settingName, bool value);


    public class CharacterSettingsController : ChoiceVScript {
        private class CharacterSettingsBlueprint {
            public string Identifier;
            public string DefaultValue;
            public CharacterSettingsType Type;

            public string ShowName;
            public string ShowDescription;

            public dynamic AdditionalData;

            public OnCharacterListSettingChange ListCallback;
            public OnCharacterFlagSettingChange FlagCallback;

            public CharacterSettingsBlueprint(string identifier, string defaultValue, CharacterSettingsType type, string showName, string showDescription, dynamic additionalData, OnCharacterListSettingChange callback) {
                Identifier = identifier;
                DefaultValue = defaultValue;
                Type = type;
                ShowName = showName;
                ShowDescription = showDescription;

                AdditionalData = additionalData;

                ListCallback = callback;
            }

            public CharacterSettingsBlueprint(string identifier, string defaultValue, CharacterSettingsType type, string showName, string showDescription, dynamic additionalData, OnCharacterFlagSettingChange callback) {
                Identifier = identifier;
                DefaultValue = defaultValue;
                Type = type;
                ShowName = showName;
                ShowDescription = showDescription;

                AdditionalData = additionalData;

                FlagCallback = callback;
            }
        }

        private static Dictionary<string, CharacterSettingsBlueprint> CharacterSettingsBlueprints = new Dictionary<string, CharacterSettingsBlueprint>();
        public CharacterSettingsController() {
            CharacterController.addSelfMenuElement(new UnconditionalPlayerSelfMenuElement("Gameplay-Einstellungen", generateSettingsMenu));

            EventController.PlayerPreSuccessfullConnectionDelegate += onPlayerConnect;
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnectPast;

            EventController.addMenuEvent("CHARACTER_SETTING_LIST_CHANGE", onCharacterSettingListChange);
            EventController.addMenuEvent("CHARACTER_SETTING_CHECKBOX_CHANGE", onCharacterSettingCheckboxChange);

            EventController.KeyPressOverrideDelegate += onKeyPressOverride;
            EventController.addKeyEvent("RESET_LAYOUT", ConsoleKey.F12, "Tastaturbelegung zurücksetzen", onResetKeyboardLayout);

            EventController.addMenuEvent("PLAYER_CHANGE_KEYBOARD_LAYOUT", onPlayerChangeKeyboardLayout);
            EventController.addMenuEvent("PLAYER_CHANGE_KEYBOARD_LAYOUT_CONFIRM", onResetKeyboardLayoutConfirm);


            #region WalkStyle

            CharacterSettingsController.addListCharacterSettingBlueprint(
                "WALKING_STYLE", "DEFAULT", "Geh-Animation ändern", "Ändere den Stil, in dem der Charakter geht",
                       new Dictionary<string, string> { { "DEFAULT", "Standard" }, { "move_p_m_one", "energisch" }, { "move_m@generic" , "gener. männlich" }, { "move_f@multiplayer", "gener. weiblich" },
                         { "move_f@fat@a", "stämmig" }, { "move_m@drunk@slightlydrunk" , "leicht betrunken" }, { "move_m@drunk@moderatedrunk" , "moderat betrunken" },
                         { "move_m@drunk@verydrunk" , "sehr betrunken" }, { "move_m@gangster@generic" , "Gangster 1 (M)" }, { "move_f@gangster@ng" , "Gangster 1 (F)" },
                         { "move_m@tough_guy@" , "Gangster 2 (M)" }, { "move_f@tough_guy@" , "Gangster 2 (F)" }, { "move_m@hurry@a" , "ängstlich eilend" },
                         { "move_m@injured" , "verletzt" }, { "move_m@hiking" , "Rucksack" } },
                onChangeWalkStyle
             );

            CharacterSettingsController.addListCharacterSettingBlueprint(
                "RUNNING_STYLE", "DEFAULT", "Lauf-Animation ändern", "Ändere den Stil, in dem der Charakter läuft",
                     new Dictionary<string, string> { { "DEFAULT", "Standard" }, { "move_p_m_one", "energisch" }, { "move_m@generic" , "gener. männlich" }, { "move_f@multiplayer" , "gener. weiblich" },
                         { "move_f@fat@a" , "stämmig" }, { "move_m@drunk@slightlydrunk" , "leicht betrunken" }, { "move_m@drunk@moderatedrunk" , "moderat betrunken" },
                         { "move_m@drunk@verydrunk" , "sehr betrunken" }, { "move_m@gangster@generic" , "Gangster 1 (M)" }, { "move_m@hurry@a" , "ängstlich eilend" },
                         { "move_m@injured" , "verletzt" } },
                onChangeRunStyle
            );

            #endregion
        }


        #region WalkStyle

        private void onChangeWalkStyle(IPlayer player, string settingName, string value) {
            if(value == "DEFAULT") {
                player.emitClientEvent("SET_WALKING_ANIMATION", "walk", null);
            } else {
                player.emitClientEvent("SET_WALKING_ANIMATION", "walk", value);
            }
        }

        private void onChangeRunStyle(IPlayer player, string settingName, string value) {
            if(value == "DEFAULT") {
                player.emitClientEvent("SET_WALKING_ANIMATION", "run", null);
            } else {
                player.emitClientEvent("SET_WALKING_ANIMATION", "run", value);
            }
        }

        #endregion

        private void onPlayerConnect(IPlayer player, character character) {
            player.getCharacterData().Settings = character.charactersettings.ToDictionary(c => c.name, c => c.value);
        }

        private void onPlayerConnectPast(IPlayer player, character character) {
            var walkSett = player.getCharSetting("WALKING_STYLE");
            if(walkSett == "DEFAULT") {
                if(player.getCharacterData().Gender == 'F') {
                    player.emitClientEvent("SET_WALKING_ANIMATION", "walk", "move_f@multiplayer");
                } else {
                    player.emitClientEvent("SET_WALKING_ANIMATION", "walk", "move_m@generic");
                }
            } else {
                player.emitClientEvent("SET_WALKING_ANIMATION", "walk", walkSett);
            }

            var runSett = player.getCharSetting("RUNNING_STYLE");
            if(runSett == "DEFAULT") {
                if(player.getCharacterData().Gender == 'F') {
                    player.emitClientEvent("SET_WALKING_ANIMATION", "run", "move_f@multiplayer");
                } else {
                    player.emitClientEvent("SET_WALKING_ANIMATION", "run", "move_m@generic");
                }
            } else {
                player.emitClientEvent("SET_WALKING_ANIMATION", "run", runSett);
            }

            //KeyLayout
            var mappings = new Dictionary<ConsoleKey, List<string>>();
            var mappingsByIdentifier = new Dictionary<string, ConsoleKey>();

            foreach(var el in character.account.accountkeymappings) {
                if(mappings.ContainsKey((ConsoleKey)el.toKey)) {
                    mappings[(ConsoleKey)el.toKey].Add(el.identifier);
                } else {
                    mappings[(ConsoleKey)el.toKey] = new List<string> { el.identifier };
                }
                mappingsByIdentifier[el.identifier] = (ConsoleKey)el.toKey;
            }

            player.getCharacterData().ChangeKeyMappings = mappings;
            player.getCharacterData().ChangeMappingsByIdentifier = mappingsByIdentifier;

            CharacterSettingsController.setPlayerAllowedKeys(player);
        }

        /// <summary>
        /// Adds a list selection Character Setting
        /// </summary>
        public static void addFlagCharacterSettingBlueprint(string identifier, bool defaultValue, string showName, string showDescription, OnCharacterFlagSettingChange callback = null) {
            CharacterSettingsBlueprints.Add(identifier, new CharacterSettingsBlueprint(identifier, defaultValue.ToString(), CharacterSettingsType.FlagSetting, showName, showDescription, null, callback));
        }

        /// <summary>
        /// Adds a list selection Character Setting
        /// </summary>
        /// <param name="selections">is a map of SELECTION_NAME to ShowName in the menu. The SelectionName is later used to identify the selected option</param>
        public static void addListCharacterSettingBlueprint(string identifier, string defaultValue, string showName, string showDescription, Dictionary<string, string> selections, OnCharacterListSettingChange callback = null) {
            CharacterSettingsBlueprints.Add(identifier, new CharacterSettingsBlueprint(identifier, defaultValue, CharacterSettingsType.ListSetting, showName, showDescription, selections, callback));
        }

        private Menu generateSettingsMenu(IPlayer player) {
            var menu = new Menu("Gameplay-Einstellungen", "Was möchtest du tun?");
            var charSettings = player.getCharacterData().Settings;

            menu.addMenuItem(new MenuMenuItem("Tastaturbelegung ändern", new VirtualMenu("Tastaturbelegung ändern", () => getKeyLayoutChange(player))));

            foreach(var setting in CharacterSettingsBlueprints.Values) {
                var data = new Dictionary<string, dynamic> {
                    { "Setting", setting },
                };

                string value;
                if(charSettings.ContainsKey(setting.Identifier)) {
                    value = charSettings[setting.Identifier];
                } else {
                    value = setting.DefaultValue;
                }

                switch(setting.Type) {
                    case CharacterSettingsType.FlagSetting:
                        var isChecked = value == "True";
                        menu.addMenuItem(new CheckBoxMenuItem(setting.ShowName, setting.ShowDescription, isChecked, "CHARACTER_SETTING_CHECKBOX_CHANGE", MenuItemStyle.normal, false, false).withData(data));
                        break;
                    case CharacterSettingsType.ListSetting:
                        var selections = (Dictionary<string, string>)setting.AdditionalData;
                        var list = selections.Values.ToList();
                        var count = selections.Keys.ToList().FindIndex(v => v.Equals(value));
                        menu.addMenuItem(new ListMenuItem(setting.ShowName, setting.ShowDescription, list.ShiftLeft(count).ToArray(), "CHARACTER_SETTING_LIST_CHANGE", MenuItemStyle.normal, true, true).withData(data));
                        break;
                }
            }

            return menu;
        }

        public static string getCharacterSettingValue(IPlayer player, string identifier) {
            try {
                var charSettings = player.getCharacterData().Settings;
                if(charSettings.ContainsKey(identifier)) {
                    return player.getCharacterData().Settings.Get(identifier);
                } else {
                    return CharacterSettingsBlueprints[identifier].DefaultValue;
                }
            } catch(Exception) {
                return CharacterSettingsBlueprints[identifier].DefaultValue;
            }
        }

        private bool onCharacterSettingListChange(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;
            var setting = (CharacterSettingsBlueprint)data["Setting"];

            var optionsList = (Dictionary<string, string>)setting.AdditionalData;
            var value = optionsList.FirstOrDefault(o => o.Value == evt.currentElement).Key;

            var settings = player.getCharacterData().Settings;

            if(!(settings.ContainsKey(setting.Identifier) && settings[setting.Identifier] == value) && !(!settings.ContainsKey(setting.Identifier) && value == setting.DefaultValue)) {
                settings[setting.Identifier] = value;

                using(var db = new ChoiceVDb()) {
                    var charId = player.getCharacterId();
                    var already = db.charactersettings.FirstOrDefault(s => s.characterid == charId && s.name == setting.Identifier);

                    if(already != null) {
                        already.value = value;
                    } else {
                        var dbSett = new charactersetting {
                            characterid = charId,
                            name = setting.Identifier,
                            value = value
                        };

                        db.charactersettings.Add(dbSett);
                    }

                    db.SaveChanges();
                }

                player.sendNotification(Constants.NotifactionTypes.Warning, $"Die Einstellung {setting.ShowName} wurde zu {evt.currentElement} gewechselt!", $"Einstellung {setting.ShowName} geändert!", Constants.NotifactionImages.System, $"GAMEPLAY_CHANGE_{setting.ShowName}");

                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"Changed Setting: {setting.ShowName} to {evt.currentElement}");

                if(setting.ListCallback != null) {
                    setting.ListCallback.Invoke(player, setting.Identifier, value);
                }
            }

            return true;
        }

        private bool onCharacterSettingCheckboxChange(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as CheckBoxMenuItemEvent;
            var setting = (CharacterSettingsBlueprint)data["Setting"];

            var settings = player.getCharacterData().Settings;

            var newValue = evt.check;

            using(var db = new ChoiceVDb()) {
                settings[setting.Identifier] = newValue.ToString();
                var charId = player.getCharacterId();
                var already = db.charactersettings.FirstOrDefault(s => s.characterid == charId && s.name == setting.Identifier);

                if(already != null) {
                    already.value = newValue.ToString();
                } else {
                    var dbSett = new charactersetting {
                        characterid = charId,
                        name = setting.Identifier,
                        value = newValue.ToString()
                    };

                    db.charactersettings.Add(dbSett);
                }

                db.SaveChanges();
            }

            var str = "Nein";
            if(newValue) str = "Ja";

            player.sendNotification(Constants.NotifactionTypes.Warning, $"Die Einstellung {setting.ShowName} wurde zu {str} gewechselt!", $"Einstellung {setting.ShowName} geändert!");

            Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"Changed Setting: {setting.ShowName} to {newValue}");

            setting.FlagCallback?.Invoke(player, setting.Identifier, newValue);

            return true;
        }

        #region Key Layout Change

        public static void setPlayerAllowedKeys(IPlayer player) {
            var keyList = new List<int>();
            var keyToggleList = new List<int>();
            var keyIgnoresBusyList = new List<int>();

            var mapping = player.getCharacterData().ChangeMappingsByIdentifier;

            foreach(var key in EventController.RegisteredKeyEvents) {
                foreach(var keyEvt in key.Value) {
                    var realKey = key.Key;
                    if(mapping.ContainsKey(keyEvt.Identifier)) {
                        realKey = mapping[keyEvt.Identifier];
                    }

                    if(keyEvt.IgnoresKeyLock) {
                        keyIgnoresBusyList.Add((int)realKey);
                    } else {
                        keyList.Add((int)realKey);
                    }
                }
            }

            foreach (var key in EventController.RegisteredKeyToggleEvents) {
                foreach (var keyEvt in key.Value) {
                    var realKey = key.Key;
                    if (mapping.ContainsKey(keyEvt.Identifier)) {
                        realKey = mapping[keyEvt.Identifier];
                    }

                    keyToggleList.Add((int)realKey);
                }
            }

            player.emitClientEvent("SET_KEY_LIST", keyList.Distinct().ToArray().ToJson(), keyToggleList.Distinct().ToArray().ToJson(), keyIgnoresBusyList.ToArray().ToJson());
        }

        private Menu getKeyLayoutChange(IPlayer player) {
            var menu = new Menu("Tastaturbelegung", "Ändere die Tastaturbelegung");
            var mappingsByIdentifier = player.getCharacterData().ChangeMappingsByIdentifier;

            foreach (var keyEvt in EventController.RegisteredKeyEvents) {
                foreach (var callback in keyEvt.Value) {
                    var key = keyEvt.Key;
                    if (mappingsByIdentifier.ContainsKey(callback.Identifier)) {
                        key = mappingsByIdentifier[callback.Identifier];
                    }

                    var data = new Dictionary<string, dynamic> {
                        { "OriginalKey", keyEvt.Key },
                        { "CurrentKey", key },
                        { "Identifier", callback.Identifier },
                        { "Info", callback.Name },
                    };

                    menu.addMenuItem(new ClickMenuItem(callback.Name, $"Belegung von {callback.Name} ändern", getConsoleKeyToString(key), "PLAYER_CHANGE_KEYBOARD_LAYOUT").withData(data));
                }
            }

            foreach (var keyEvt in EventController.RegisteredKeyToggleEvents) {
                foreach (var callback in keyEvt.Value) {
                    var key = keyEvt.Key;
                    if (mappingsByIdentifier.ContainsKey(callback.Identifier)) {
                        key = mappingsByIdentifier[callback.Identifier];
                    }

                    var data = new Dictionary<string, dynamic> {
                        { "OriginalKey", keyEvt.Key },
                        { "CurrentKey", key },
                        { "Identifier", callback.Identifier },
                        { "Info", callback.Name },
                    };

                    menu.addMenuItem(new ClickMenuItem(callback.Name, $"Belegung von {callback.Name} ändern. Halten ist möglich!", $"{getConsoleKeyToString(key)} (Halten mögl.)", "PLAYER_CHANGE_KEYBOARD_LAYOUT").withData(data));
                }
            }

            return menu;
        }

        public static string getConsoleKeyToString(ConsoleKey key) {
            var keyStr = GetChar(key);
            if(keyStr == "" || key.ToString().StartsWith("NumPad")) {
                return key.ToString();
            } else if(key == ConsoleKey.Decimal) {
                return "Numpad,";
            } else if(key == ConsoleKey.Add) {
                return "Numpad+";
            } else if(key == ConsoleKey.Subtract) {
                return "Numpad-";
            } else if(key == ConsoleKey.Multiply) {
                return "Numpad*";
            } else if(key == ConsoleKey.Divide) {
                return "Numpad/";
            } else {
                return keyStr;
            }
        }

        public static string GetChar(ConsoleKey key) {
            var germanKeyboardMapping = new Dictionary<ConsoleKey, string> {
# region Mappings
                    { ConsoleKey.Oem1, "ü" },
                    { ConsoleKey.OemPlus, "+" },
                    { ConsoleKey.Oem3, "Ö" },
                    { ConsoleKey.Oem7, "Ä" },
                    { ConsoleKey.Oem2, "#" },
                    { ConsoleKey.OemComma, "," },
                    { ConsoleKey.OemPeriod, "." },
                    { ConsoleKey.OemMinus, "-" },
                    { ConsoleKey.Oem5, "<>" },
#endregion    
            };

            if (germanKeyboardMapping.ContainsKey(key)) {
                return germanKeyboardMapping[key];
            } else {
                return key.ToString().ToUpper();
            }
        }

        private class KeyChangeInfo {
            public ConsoleKey OriginalKey;
            public ConsoleKey CurrentKey;
            public string Identifier;
            public string Info;
            public KeyChangeInfo(ConsoleKey originalKey, ConsoleKey currentKey, string identifier, string info) {
                OriginalKey = originalKey;
                CurrentKey = currentKey;
                Identifier = identifier;
                Info = info;
            }
        }

        private bool onPlayerChangeKeyboardLayout(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var origKey = (ConsoleKey)data["OriginalKey"];
            var currentKey = (ConsoleKey)data["CurrentKey"];
            var identifier = (string)data["Identifier"];
            var info = (string)data["Info"];

            player.sendNotification(Constants.NotifactionTypes.Info, "Drücke jetzt die gewünschte Taste", "Taste drücken");

            player.setData("PLAYER_CHANGE_KEY", new KeyChangeInfo(origKey, currentKey, identifier, info));
            player.emitClientEvent("ALLOW_ALL_KEYS_ONCE");
            player.addState(Constants.PlayerStates.ChangingKey);
            return true;
        }

        private bool onKeyPressOverride(IPlayer player, ConsoleKey newKey) {
            lock(player) {
                if(!player.hasData("PLAYER_CHANGE_KEY")) {
                    return false;
                }

                player.removeState(Constants.PlayerStates.ChangingKey);

                var el = (KeyChangeInfo)player.getData("PLAYER_CHANGE_KEY");
                player.resetData("PLAYER_CHANGE_KEY");

                if(el.CurrentKey == newKey) {
                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Die Aktion {el.Info} war bereits auf die Taste {getConsoleKeyToString(newKey)} und wurde deswegen nicht geändert!", "Taste identisch");
                    return true;
                }

                var mapping = player.getCharacterData().ChangeKeyMappings;
                var mappingByIdentifier = player.getCharacterData().ChangeMappingsByIdentifier;

                if (mapping.ContainsKey(el.CurrentKey)) {
                    mapping[el.CurrentKey].Remove(el.Identifier);
                    if (mapping[el.CurrentKey].Count <= 0) {
                        mapping.Remove(el.CurrentKey);
                    }
                }
                mappingByIdentifier.Remove(el.Identifier);

                using (var db = new ChoiceVDb()) {
                    var dbFind = db.accountkeymappings.FirstOrDefault(m => m.accountId == player.getAccountId() && m.identifier == el.Identifier);
                    if(dbFind != null) {
                        db.accountkeymappings.Remove(dbFind);
                    }

                    if(newKey != el.OriginalKey) {
                        if(mapping.ContainsKey(newKey)) {
                            var list = mapping[newKey];
                            list.Add(el.Identifier);
                        } else {
                            var newList = new List<string> { el.Identifier };
                            mapping[newKey] = newList;
                        }
                        mappingByIdentifier[el.Identifier] = newKey;

                        db.accountkeymappings.Add(new accountkeymapping {
                            accountId = player.getAccountId(),
                            toKey = (int)newKey,
                            identifier = el.Identifier,
                        });
                    }

                    db.SaveChanges();
                }

                setPlayerAllowedKeys(player);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Erfolgreich die Belegung von {getConsoleKeyToString(el.CurrentKey)} auf {getConsoleKeyToString(newKey)} geändert", "Belegung geändert", Constants.NotifactionImages.System);
                return true;
            }
        }

        private bool onResetKeyboardLayout(IPlayer player, ConsoleKey key, string eventName) {
            player.showMenu(MenuController.getConfirmationMenu("Tastaturbelegung zurücksetzen?", "Setze die eigene Belegung zurück", "PLAYER_CHANGE_KEYBOARD_LAYOUT_CONFIRM"));
            return true;
        }

        private bool onResetKeyboardLayoutConfirm(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            using(var db = new ChoiceVDb()) {
                var allEntries = db.accountkeymappings.Where(m => m.accountId == player.getAccountId()).ToList();
                db.accountkeymappings.RemoveRange(allEntries);

                db.SaveChanges();

                player.getCharacterData().ChangeKeyMappings = new Dictionary<ConsoleKey, List<string>>();
                player.getCharacterData().ChangeMappingsByIdentifier = new Dictionary<string, ConsoleKey>();
            }

            player.sendNotification(Constants.NotifactionTypes.Warning, "Tastaturbelegung zurückgesetzt", "");
            return true;
        }

        #endregion
    }
}
