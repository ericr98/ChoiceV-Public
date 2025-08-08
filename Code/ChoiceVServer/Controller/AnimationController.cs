using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class AnimationController : ChoiceVScript {
        private static Dictionary<string, Animation> AllAnimations = new Dictionary<string, Animation>();

        private static Dictionary<string, List<Animation>> AnimationsByGroup = new Dictionary<string, List<Animation>>();

        private static Dictionary<string, ItemAnimation> AllItemAnimations = new Dictionary<string, ItemAnimation>();
        private static Dictionary<int, ItemAnimation> AllItemAnimationsById = new Dictionary<int, ItemAnimation>();

        private static Dictionary<string, Animation> AllNormalAnimations = new Dictionary<string, Animation>();
        private static Dictionary<int, Animation> AllNormalAnimationsById = new Dictionary<int, Animation>();


        public AnimationController() {
            var placeholderCount = 1;
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.configitemanimations.Include(i => i.neededItemNavigation)) {
                    var anim = new ItemAnimation(row);
                    if(AllItemAnimations.ContainsKey(row.identifier)) {
                        row.identifier = row.identifier + placeholderCount;
                        placeholderCount++;
                    }

                    AllItemAnimations.Add(row.identifier, anim);

                    AllAnimations.Add(row.identifier, anim);
                    AllItemAnimationsById.Add(row.id, anim);

                    if(row.shown == 1) {
                        if(AnimationsByGroup.ContainsKey(row.group)) {
                            AnimationsByGroup[row.group].Add(anim);
                        } else {
                            var list = new List<Animation>();
                            list.Add(anim);
                            AnimationsByGroup[row.group] = list;
                        }
                    }
                }

                foreach(var row in db.configanimations) {
                    var anim = new Animation(row.dict, row.name, row.duration != -1 ? TimeSpan.FromMilliseconds(row.duration) : TimeSpan.FromMilliseconds(100000000), row.flag, row.startAtPercent, row.showName, row.group, row.shown == 1, row.id);

                    if(AllNormalAnimations.ContainsKey(row.identifier)) {
                        row.identifier = row.identifier + placeholderCount;
                        placeholderCount++;
                    }

                    AllNormalAnimations.Add(row.identifier, anim);

                    AllAnimations.Add(row.identifier, anim);
                    AllNormalAnimationsById.Add(row.id, anim);

                    if(row.shown == 1) {
                        if(AnimationsByGroup.ContainsKey(row.group)) {
                            AnimationsByGroup[row.group].Add(anim);
                        } else {
                            var list = new List<Animation>();
                            list.Add(anim);
                            AnimationsByGroup[row.group] = list;
                        }
                    }
                }
            }

            EventController.addKeyEvent("STOP_ANIM", ConsoleKey.NumPad0, "Animation Abbrechen", onStopAnim, true);

            DamageController.BackendPlayerDeathDelegate += onPlayerDeath;

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    "Animationen Menü",
                    getPlayerAnimationMenu,
                    (player) => !player.IsInVehicle
                )
            );
            EventController.addMenuEvent("ON_PLAYER_SELECT_ANIMATION", onPlayerSelectAnimation);

            EventController.addMenuEvent("ON_PLAYER_SELECT_ANIM_MENU_MODE", onPlayerSelectAnimMenuMode);
            EventController.addMenuEvent("ON_PLAYER_ANIMATION_ADD_TO_SET", onPlayerAnimationAddToSet);

            EventController.addMenuEvent("PLAYER_CHANGE_HOTKEY_FROM_ANIMSET", onPlayerChangeHoteKeyFromAnimSet);
            EventController.addMenuEvent("PLAYER_DELETE_ANIM_FROM_SET", onPlayerRemoveAnimFromSet);

            EventController.PlayerPastSuccessfullConnectionDelegate += onPlayerConnect;
            EventController.KeyPressOverrideDelegate += onKeyOverride;

            EventController.addEvent("ANIMATION_KEY_PRESSED", onAnimationKeyPressed);
            EventController.addKeyToggleEvent("CHANGE_ANIMATION_SET", ConsoleKey.Decimal, "Animsets ändern", onSwitchTroughAnimSets);
            EventController.addMenuEvent("ON_PLAYER_SELECT_ANIM_SET", onPlayerSelectAnimSet);

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;
        }
        
        private void onPlayerConnect(IPlayer player, character character) {
            var data = player.getCharacterData();
            foreach(var setanim in character.charactersetanimations) {
                if(setanim.hotKey != null) {
                    addAnimToSetForPlayer(player, (ConsoleKey)setanim.hotKey, setanim);
                }
            }

            data.AnimationSets = data.AnimationSets.OrderBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            if(data.AnimationSets.Count > 0) {
                player.setData("CURRENT_ANIM_SET", data.AnimationSets.First().Key);
                player.emitClientEvent("SET_ANIMATION_KEYS", data.AnimationSets.First().Value.Select(k => (int)k).ToList().ToJson());
            }
        }

        private Menu getPlayerAnimationMenu(IPlayer player) {
            var menu = new Menu("Animationen abspielen", "Welche Animationsgruppe?");

            var setVirtualMenu = new VirtualMenu("Sets bearbeiten", () => getAnimationSetMenu(player));
            menu.addMenuItem(new MenuMenuItem(setVirtualMenu.Name, setVirtualMenu));

            var l = new string[] { "Abspielen", "Bearbeiten" };
            if(player.hasData("ANIM_MENU_MODE") && ((string)player.getData("ANIM_MENU_MODE")) == "Bearbeiten") {
                l = new string[] { "Bearbeiten", "Abspielen" };
            }

            menu.addMenuItem(new ListMenuItem("Menü-Modus", "Aktiviere den Modus, um die Animation abzuspielen oder sie Sets hinzuzufügen", l, "ON_PLAYER_SELECT_ANIM_MENU_MODE", MenuItemStyle.normal, true, true));

            foreach(var group in AnimationsByGroup) {
                var virtMenu = new VirtualMenu(group.Key, () => {
                    var groupMenu = new Menu($"{group.Key}", "Wähle eine Animation");

                    foreach(var anim in group.Value) {
                        var data = new Dictionary<string, dynamic> {
                            {"Animation", anim },
                        };

                        var timeStr = "bis abgebrochen";
                        if(anim.Duration < TimeSpan.FromHours(1)) {
                            timeStr = $"{Math.Round(anim.Duration.TotalSeconds)}sek";
                        }

                        if(anim is ItemAnimation ianim && ianim.IsShown) {
                            if(ianim.ConfigItem == null || player.getInventory().hasItem(i => i.ConfigId == ianim.ConfigItem.configItemId)) {
                                groupMenu.addMenuItem(new ClickMenuItem($"{anim.ShowName}", $"Spiele diese Animation an, sie geht {timeStr}", timeStr, "ON_PLAYER_SELECT_ANIMATION", MenuItemStyle.normal, false, false).withData(data));
                            } else {
                                groupMenu.addMenuItem(new StaticMenuItem($"{anim.ShowName}", $"Um diese Animation abzuspielen benötigt du ein Item mit Namen: {ianim.ConfigItem.name}", $"{ianim.ConfigItem.name} fehlt!", MenuItemStyle.yellow));
                            }
                        } else {
                            groupMenu.addMenuItem(new ClickMenuItem($"{anim.ShowName}", $"Spiele diese Animation an, sie geht {timeStr}", timeStr, "ON_PLAYER_SELECT_ANIMATION", MenuItemStyle.normal, false, false).withData(data));
                        }
                    }

                    //If Item is ClickMenuItem Sort it up
                    groupMenu.sortMenuItems((item1, item2) => {
                        if(item1 is ClickMenuItem && item2 is ClickMenuItem) {
                            //Then by Name
                            return item1.Name.CompareTo(item2.Name);
                        }

                        if(item1 is ClickMenuItem) {
                            return -1;
                        }

                        if(item2 is ClickMenuItem) {
                            return 1;
                        }

                        return 0;
                    });
                    
                    return groupMenu;
                });
                menu.addMenuItem(new MenuMenuItem(group.Key, virtMenu));
            }

            return menu;
        }

        private bool onPlayerSelectAnimMenuMode(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;

            setPlayerAnimMenuMode(player, evt.currentElement);

            return true;
        }

        private void setPlayerAnimMenuMode(IPlayer player, string mode = "Abspielen") {
            player.setData("ANIM_MENU_MODE", mode);
        }

        private Menu getAnimationSetMenu(IPlayer player) {
            var menu = new Menu("Animationsset", "Was möchtest du tun?");

            var dic = new Dictionary<string, Menu>();

            using(var db = new ChoiceVDb()) {
                var charId = player.getCharacterId();
                var anims = db.charactersetanimations.Include(cas => cas.animation).Include(cas => cas.itemAnimation).Where(cas => cas.charId == charId);

                foreach(var anim in anims) {
                    var name = "Unbelegt: ";
                    if(anim.hotKey != null) {
                        name = $"{CharacterSettingsController.getConsoleKeyToString((ConsoleKey)anim.hotKey)}: ";
                    }

                    if(anim.itemAnimation != null) {
                        name += anim.itemAnimation.showName;
                    } else {
                        name += anim.animation.showName;
                    }

                    var animMenu = new Menu($"{name}", "Was möchtest du tun?");

                    var data = new Dictionary<string, dynamic> {
                        {"Animation", anim.animationId != null ? AllNormalAnimationsById[anim.animationId ?? -1] : AllItemAnimationsById[anim.itemAnimationId ?? -1] }
                    };

                    var timeStr = "bis abgebrochen";
                    if(anim.itemAnimation != null) {
                        if(anim.itemAnimation.duration < TimeSpan.FromHours(1).TotalMilliseconds && anim.itemAnimation.duration != -1) {
                            timeStr = $"{TimeSpan.FromMilliseconds(anim.itemAnimation.duration).TotalSeconds}sek";
                        }

                        if(anim.itemAnimation.neededItem == null || player.getInventory().hasItem(i => i.ConfigId == anim.itemAnimation.neededItem)) {
                            animMenu.addMenuItem(new ClickMenuItem($"Abspielen", $"Spiele diese Animation an, sie geht {timeStr}", timeStr, "ON_PLAYER_SELECT_ANIMATION", MenuItemStyle.green).withData(data));
                        } else {
                            animMenu.addMenuItem(new StaticMenuItem($"Nicht abspielbar", $"Um diese Animation abzuspielen benötigt du ein spezielles Item", $"Item fehlt!", MenuItemStyle.yellow));
                        }
                    } else {
                        if(anim.animation.duration < TimeSpan.FromHours(1).TotalMilliseconds && anim.animation.duration != -1) {
                            timeStr = $"{TimeSpan.FromMilliseconds(anim.animation.duration).TotalSeconds}sek";
                        }

                        animMenu.addMenuItem(new ClickMenuItem($"Abspielen", $"Spiele diese Animation an, sie geht {timeStr}", timeStr, "ON_PLAYER_SELECT_ANIMATION", MenuItemStyle.green).withData(data));
                    }

                    animMenu.addMenuItem(new ClickMenuItem("Taste festlegen", "Lege eine Taste zum schnellen Abspielen fest", "", "PLAYER_CHANGE_HOTKEY_FROM_ANIMSET").withData(new Dictionary<string, dynamic> { { "Anim", anim } }));
                    animMenu.addMenuItem(new ClickMenuItem("Aus Set löschen", "Lösche die Animation aus dem Set", "", "PLAYER_DELETE_ANIM_FROM_SET", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Anim", anim } }).needsConfirmation("Aus Set löschen?", "Wirklich aus Set löschen?"));

                    if(dic.ContainsKey(anim.setName)) {
                        dic[anim.setName].addMenuItem(new MenuMenuItem(animMenu.Name, animMenu));
                    } else {
                        var setMenu = new Menu($"{anim.setName}", "Was möchtest du tun?");

                        setMenu.addMenuItem(new MenuMenuItem(animMenu.Name, animMenu));

                        dic.Add(anim.setName, setMenu);
                    }
                }
            }

            foreach(var el in dic) {
                menu.addMenuItem(new MenuMenuItem(el.Key, el.Value));
            }

            return menu;
        }

        private bool onPlayerSelectAnimation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = (Animation)data["Animation"];

            if(!player.hasData("ANIM_MENU_MODE") || player.getData("ANIM_MENU_MODE") == "Abspielen") {
                player.playAnimation(anim, null, false);
            } else {
                var addToSetMenu = new Menu("Zu Set hinzufügen", "Füge die Animation zu einem Set hinzu");
                addToSetMenu.addMenuItem(new InputMenuItem("Set-Name", "Lege den Namen des Sets fest. Animation mit gleichem Setnamen werden gruppiert. Für eine nicht-alphabetische Sortierung empfielt sich ein System mit einer führenden Ziffer zu verwenden. z.B. 1: Allgemein, 2: Sitzen", "", ""));
                addToSetMenu.addMenuItem(new MenuStatsMenuItem("Zu Set hinzufügen", "Füge die Animation zu einem Set mit beschriebenen Namen hinzu", "ON_PLAYER_ANIMATION_ADD_TO_SET", MenuItemStyle.green).withData(data));
                player.showMenu(addToSetMenu);
            }

            return true;
        }

        private bool onPlayerAnimationAddToSet(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = (Animation)data["Animation"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var inputEvt = evt.elements[0].FromJson<InputMenuItemEvent>();

            using(var db = new ChoiceVDb()) {
                var charSetAnim = new charactersetanimation {
                    charId = player.getCharacterId(),
                    setName = inputEvt.input ?? "",
                    animationId = anim is ItemAnimation ? null : anim.DbId,
                    itemAnimationId = anim is ItemAnimation ? anim.DbId : null,
                    hotKey = null,
                };

                if(!player.getCharacterData().AnimationSets.ContainsKey(inputEvt.input ?? "")) {
                    player.getCharacterData().AnimationSets.Add(inputEvt.input ?? "", new List<ConsoleKey>());
                }

                db.charactersetanimations.Add(charSetAnim);
                db.SaveChanges();

                setPlayerAnimationKeys(player);

                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast die Animation: {anim.ShowName} zum Set mit Namen: {inputEvt.input} hinzugefügt", "Animation zu Set hinzugefügt");
            }

            return true;
        }

        private static void addAnimToSetForPlayer(IPlayer player, ConsoleKey key, charactersetanimation anim) {
            var data = player.getCharacterData();

            if(data.KeysToAnimSets.ContainsKey(key)) {
                data.KeysToAnimSets[key].Add(anim);
            } else {
                var l = new List<charactersetanimation>();
                l.Add(anim);
                data.KeysToAnimSets[key] = l;
            }

            if(!data.AnimationSets.ContainsKey(anim.setName)) {
                data.AnimationSets.Add(anim.setName, new List<ConsoleKey> { (ConsoleKey)anim.hotKey });
                data.AnimationSets = data.AnimationSets.OrderBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            } else {
                data.AnimationSets[anim.setName].Add((ConsoleKey)anim.hotKey);
            }
        }

        private static void removeAnimFromSetForPlayer(IPlayer player, ConsoleKey key, charactersetanimation anim) {
            var data = player.getCharacterData();

            if(data.KeysToAnimSets.ContainsKey(key)) {
                data.KeysToAnimSets[key].RemoveAll(a => a.animationId == anim.animationId);

                if(data.KeysToAnimSets[key].Count == 0) {
                    data.KeysToAnimSets.Remove(key);
                }
            }

            if(data.AnimationSets.ContainsKey(anim.setName)) {
                data.AnimationSets[anim.setName].Remove(key);

                if(data.AnimationSets[anim.setName].Count == 0) {
                    data.AnimationSets.Remove(anim.setName);
                    if((string)player.getData("CURRENT_ANIM_SET") == anim.setName) {
                        onSwitchTroughAnimSets(player, ConsoleKey.Z, true, null);
                    }
                }
            }
        }

        private static void setPlayerAnimationKeys(IPlayer player) {
            var data = player.getCharacterData();
            var current = (string)player.getData("CURRENT_ANIM_SET");
            if(current != null && data.AnimationSets.ContainsKey(current)) {
                player.emitClientEvent("SET_ANIMATION_KEYS", data.AnimationSets[current].Select(k => (int)k).ToList().ToJson());
            } else {
                player.emitClientEvent("SET_ANIMATION_KEYS", new List<int>().ToJson());

            }
        }

        private bool onPlayerRemoveAnimFromSet(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = (charactersetanimation)data["Anim"];

            using(var db = new ChoiceVDb()) {
                var charId = player.getCharacterId();
                var find = db.charactersetanimations.Find(anim.id);

                if(find != null) {
                    db.charactersetanimations.Remove(find);
                    db.SaveChanges();

                    if(anim.hotKey != null) {
                        removeAnimFromSetForPlayer(player, (ConsoleKey)find.hotKey, anim);

                        setPlayerAnimationKeys(player);
                    }

                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Animation erfolgreich aus Set {anim.setName} gelöscht!", "Animation aus Set löschen");
                }
            }

            return true;
        }

        private bool onPlayerChangeHoteKeyFromAnimSet(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = (charactersetanimation)data["Anim"];

            player.setData("ANIM_CHANGE_HOTEKEY", anim);
            player.emitClientEvent("ALLOW_ALL_KEYS_ONCE");
            player.addState(Constants.PlayerStates.ChangingKey);

            player.sendNotification(Constants.NotifactionTypes.Info, "Drücke jetzt die gewünschte Taste", "Taste drücken");

            return true;
        }

        private bool onKeyOverride(IPlayer player, ConsoleKey key) {
            if(!player.hasData("ANIM_CHANGE_HOTEKEY")) {
                return false;
            }

            player.removeState(Constants.PlayerStates.ChangingKey);
            var anim = (charactersetanimation)player.getData("ANIM_CHANGE_HOTEKEY");
            player.resetData("ANIM_CHANGE_HOTEKEY");

            using(var db = new ChoiceVDb()) {
                var charId = player.getCharacterId();
                var find = db.charactersetanimations.Find(anim.id);

                var prevHotKey = find.hotKey;

                if(find != null) {
                    find.hotKey = (int)key;
                    db.SaveChanges();

                    addAnimToSetForPlayer(player, key, find);

                    if(prevHotKey != null) {
                        removeAnimFromSetForPlayer(player, (ConsoleKey)prevHotKey, find);
                    }

                    setPlayerAnimationKeys(player);

                    player.sendNotification(Constants.NotifactionTypes.Success, $"Die Animation wurde auf die Taste: {key} gelegt", "Animation auf Hotkey gelegt");
                } else {
                    player.sendBlockNotification("Etwas ist schiefgelaufen! Melde dich im Support", "Melde dich im Support!");
                }
            }

            return true;
        }

        private static bool onSwitchTroughAnimSets(IPlayer player, ConsoleKey key, bool isPressed, string eventName) {
            if(isPressed) {
                player.setData("ANIM_SET_INVOKE", InvokeController.AddTimedInvoke("AnimationSwitchInvoke", (i) => {
                    player.resetData("ANIM_SET_INVOKE");

                    var menu = new Menu("Animationsset wählen", "Wähle ein Set");
                    foreach(var set in player.getCharacterData().AnimationSets.Keys) {
                        menu.addMenuItem(new ClickMenuItem(set, $"Wähle das Set mit Namen {set}", "", "ON_PLAYER_SELECT_ANIM_SET").withData(new Dictionary<string, dynamic> { { "SetName", set } }));
                    }

                    player.showMenu(menu);
                }, TimeSpan.FromSeconds(0.5f), false));
            } else {
                if(player.hasData("ANIM_SET_INVOKE")) {
                    var invoke = (IInvoke)player.getData("ANIM_SET_INVOKE");
                    invoke.EndSchedule();
                    player.resetData("ANIM_SET_INVOKE");

                    switchPlayerThroughAnimSets(player);
                }
            }
            return true;
        }

        private bool onPlayerSelectAnimSet(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var newSet = (string)data["SetName"];

            switchPlayerThroughAnimSets(player, newSet);

            return true;
        }

        private static void switchPlayerThroughAnimSets(IPlayer player, string newSet = null) {
            var data = player.getCharacterData();

            if(data.AnimationSets.Count > 0) {
                var current = (string)player.getData("CURRENT_ANIM_SET");
                var idx = data.AnimationSets.Keys.ToList().IndexOf(current);
                if(newSet == null) {
                    newSet = data.AnimationSets.Keys.ToList()[(idx + 1) % data.AnimationSets.Count];
                }

                player.setData("CURRENT_ANIM_SET", newSet);

                setPlayerAnimationKeys(player);
                player.sendNotification(Constants.NotifactionTypes.Info, $"Ausgewähltes Animationsset: {newSet}", $"Animationsset: {newSet}", Constants.NotifactionImages.System, "ANIM_SET_TOGGLE");
            } else {
                player.sendNotification(Constants.NotifactionTypes.Warning, "Keine Animationssets erstellt!", "Keine Animationssets!");
            }
        }

        private bool onAnimationKeyPressed(IPlayer player, string eventName, object[] args) {
            if(player.IsInVehicle) {
                return true;
            }

            var data = player.getCharacterData();
            var animSet = player.getData("CURRENT_ANIM_SET");

            var key = (ConsoleKey)((int.Parse(args[0].ToString())));
            if(data.KeysToAnimSets.ContainsKey(key)) {
                var anim = data.KeysToAnimSets[key].FirstOrDefault(a => a.setName == animSet);
                var backEndAnim = anim.animationId != null ? AllNormalAnimationsById[anim.animationId ?? -1] : AllItemAnimationsById[anim.itemAnimationId ?? -1];
                

                if(backEndAnim is not ItemAnimation itemAnim || itemAnim.ConfigItem == null || player.getInventory().hasItem(i => i.ConfigId == itemAnim.ConfigItem.configItemId)) {
                    player.sendNotification(Constants.NotifactionTypes.Info, $"Animation: {backEndAnim.ShowName} wird abgespielt", $"{backEndAnim.ShowName} abgespielt");
                    player.playAnimation(backEndAnim, null, false);
                }
            }

            return true;
        }

        private static bool onStopAnim(IPlayer player, ConsoleKey key, string eventName) {
            if((!player.getBusy() || player.hasState(Constants.PlayerStates.OnBusyMenu) || player.hasState(Constants.PlayerStates.InAnimationTask)) && !SittingController.isPlayerSittingOnChair(player)) {
                stopAllAnimationForPlayer(player, true);
                return true;
            } else {
                return false;
            }
        }

        public static void stopAllAnimationForPlayer(IPlayer player, bool forceEvenIfBusy) {
            if(!player.getBusy() || forceEvenIfBusy) {
                player.stopAnimation();
                stopScenario(player);
            }

            if(player.hasState(Constants.PlayerStates.InAnimationTask)) {
                player.stopAnimation();
                stopScenario(player);
                player.removeState(Constants.PlayerStates.InAnimationTask);

                var invk = (IInvoke)player.getData("ANIM_TASK_INVOKE");
                invk.EndSchedule();
                player.resetData("ANIM_TASK_INVOKE");
                player.sendBlockNotification("Die Aktion wurde abgebrochen!", "Aktion abgebrochen!");
            }

            stopBackgroundAnimation(player);
        }

        private static void onPlayerDisconnect(IPlayer player, string arg2) {
            stopAllAnimationForPlayer(player, true);
        }

        private void onPlayerDeath(IPlayer player) {
            if(player.hasState(Constants.PlayerStates.InAnimationTask)) {
                player.stopAnimation();
                AnimationController.stopScenario(player);
                player.removeState(Constants.PlayerStates.InAnimationTask);

                var invk = (IInvoke)player.getData("ANIM_TASK_INVOKE");
                invk.EndSchedule();
                player.resetData("ANIM_TASK_INVOKE");
                player.sendBlockNotification("Die Aktion wurde abgebrochen!", "Aktion abgebrochen!");
            } else {
                player.stopAnimation();
                AnimationController.stopScenario(player);
            }
        }

        /// <summary>
        /// Start an animation task. An animation will be played, and afterwards an callback is executed. Item Animations are possible!
        /// </summary>
        /// <param name="animation">The animation which will be played</param>
        /// <param name="action">The callback </param>
        /// <param name="facingRotation">The rotation the player shall be facing when doing the animation</param>
        public static void animationTask(IPlayer player, Animation animation, Action action, Rotation? facingRotation = null, bool playerBusy = true, int? flag = null, TimeSpan duration = default) {
            if(animation == null) {
                action.Invoke();
                return;
            }

            if(!(animation is ItemAnimation)) {
                animationTask(player, animation.Dictionary, animation.Name, duration != default ? duration : animation.Duration, flag ?? animation.Flag, action, Position.Zero, Rotation.Zero, null, -1, false, facingRotation, playerBusy, animation.StartAtPercent);
                
            } else {
                var ia = animation as ItemAnimation;
                animationTask(player, animation.Dictionary, animation.Name, duration != default ? duration : animation.Duration, flag ?? animation.Flag, action, ia.Offset, ia.Rotation, ia.Model, ia.Bone, true, facingRotation, playerBusy, animation.StartAtPercent, ia.AttachVertexOrder);
            }
            
            if(!string.IsNullOrEmpty(animation.AccompanyingFacialDict)) {
                player.playFacialAnimation(animation.AccompanyingFacialDict, animation.AccompanyingFacialName, duration != default ? duration.TotalMilliseconds : animation.Duration.TotalMilliseconds); 
            }
        }

        /// <summary>
        /// Start an animation task. An animation will be played, and afterwards an callback is executed. Item Animations are possible!
        /// </summary>
        /// <param name="action">The callback</param>
        /// <param name="facingRotation">The rotation the player shall be facing when doing the animation</param>
        public static void animationTask(IPlayer player, string dict, string name, TimeSpan duration, int flag, Action action, Position offset, Rotation rotation, string model, int bone, bool itemAnimation, Rotation? facingRotation = null, bool playerBusy = true, float time = 0, int attachVertexOrder = 2) {
            if(playerBusy) {
                player.addState(Constants.PlayerStates.InAnimation);
                player.addState(Constants.PlayerStates.InAnimationTask);
            }

            if(itemAnimation) {
                playItemAnimation(player, dict, name, duration, flag, offset, rotation, model, bone, time, facingRotation, playerBusy, attachVertexOrder);
            } else {
                if(facingRotation != null) {
                    player.Rotation = facingRotation ?? Rotation.Zero;
                }
                player.emitClientEvent(Constants.PlayerPlayAnimation, dict, name, duration.TotalMilliseconds, flag, -1, false, false, time);
            }

            var invk = InvokeController.AddTimedInvoke(player.getCharacterName() + "-AnimationTask", (ivk) => {
                if(playerBusy) {
                    player.removeState(Constants.PlayerStates.InAnimation);
                    player.removeState(Constants.PlayerStates.InAnimationTask);
                }

                action.Invoke();
            }, duration, false);
            player.setData("ANIM_TASK_INVOKE", invk);
        }


        /// <summary>
        /// Plays a specific item animation with the db name
        /// </summary>
        /// <param name="name">Name of animation in db</param>
        public static void playItemAnimation(IPlayer player, string name, Rotation? facingRotation = null, bool playerBusy = true) {
            try {
                playItemAnimation(player, AllItemAnimations[name], facingRotation, playerBusy);
            } catch(Exception) {
                Logger.logError($"playItemAnimation: Tried to use Animation which was not found! {name}",
                $"Fehler mit Item Animation: {name}, sie wurde nicht gefunden!", player);
            }
        }

        /// <summary>
        /// Plays a specific item animation
        /// </summary>
        public static void playItemAnimation(IPlayer player, ItemAnimation animation, Rotation? facingRotation = null, bool playerBusy = true) {
            playItemAnimation(player, animation.Dictionary, animation.Name, animation.Duration, animation.Flag, animation.Offset, animation.Rotation, animation.Model, animation.Bone, animation.StartAtPercent, facingRotation, playerBusy, animation.AttachVertexOrder);
        }

        /// <summary>
        /// Plays a specific item animation
        /// </summary>
        public static void playItemAnimation(IPlayer player, string dict, string name, TimeSpan duration, int flag, Position offset, DegreeRotation rotation, string model, int bone, float startAtPercent = 0, Rotation? facingRotation = null, bool playerBusy = true, int attachVertexOrder = 2) {
            if(playerBusy) {
                player.addState(Constants.PlayerStates.InAnimation);
            }

            if(player.hasData(Constants.DATA_CHARACTER_PLAYING_ITEM_ANIMATION)) {
                var alreadyObj = (Object)player.getData(Constants.DATA_CHARACTER_PLAYING_ITEM_ANIMATION);
                ObjectController.deleteObject(alreadyObj);
                player.resetData(Constants.DATA_CHARACTER_PLAYING_ITEM_ANIMATION);
            }

            player.playAnimation(dict, name, (int)duration.TotalMilliseconds, flag, facingRotation, startAtPercent);

            if (!player.IsInVehicle) {
                var obj = ObjectController.createObject(model, player, offset, rotation, bone, 200, false, attachVertexOrder);
                player.setData(Constants.DATA_CHARACTER_PLAYING_ITEM_ANIMATION, obj);
            }

            if(duration != TimeSpan.MaxValue) {
                InvokeController.AddTimedInvoke(player.getCharacterName() + "-AnimationTask-Delete", (ivk) => {
                    stopAnimation(player);
                }, duration, false);
            }
        }

        /// <summary>
        /// Stops any animation that is currently playing
        /// </summary>
        public static void stopAnimation(IPlayer player) {
            player.emitClientEvent(Constants.PlayerStopAnimation);

            if(player.hasState(Constants.PlayerStates.InAnimation)) {
                player.removeState(Constants.PlayerStates.InAnimation);
            }

            if(player.hasData(Constants.DATA_CHARACTER_PLAYING_ITEM_ANIMATION)) {
                var obj = (Object)player.getData(Constants.DATA_CHARACTER_PLAYING_ITEM_ANIMATION);
                ObjectController.deleteObject(obj);
                player.resetData(Constants.DATA_CHARACTER_PLAYING_ITEM_ANIMATION);
            }
        }

        /// <summary>
        /// Stops any animation that is currently playing in the background
        /// </summary>
        public static void stopBackgroundAnimation(IPlayer player) {
            player.emitClientEvent("STOP_BACKGROUND_ANIM");
        }

        /// <summary>
        /// Get an animation by db name
        /// </summary>
        /// <param name="name">Name of the animation in db</param>
        public static Animation getAnimationByName(string name) {
            if(name == null) {
                return null;
            }

            if(AllAnimations.ContainsKey(name)) {
                return AllAnimations[name];
            } else {
                Logger.logError($"getAnimationByName: Searched Animation not found! {name}",
                    $"Fehler mit Animation: {name}, sie wurde nicht gefunden!");

                return null;
            }
        }

        public static void playScenario(IPlayer player, string scenarioName) {
            //player.PlayScenario(scenarioName);
            player.emitClientEvent("PLAY_SCENARIO", scenarioName);
        }

        public static void playScenarioAtPosition(IPlayer player, string scenarioName, Position position, float heading, int duration, bool sittingScenario, bool teleport) {
            player.emitClientEvent("PLAY_SCENARIO_AT_POS", scenarioName, position.X, position.Y, position.Z, heading, duration, sittingScenario, teleport);
        }

        public static void stopScenario(IPlayer player) {
            player.emitClientEvent("STOP_SCENARIO");
        }
    }

    public class Animation {
        public int DbId;

        public string Dictionary;
        public string Name;

        public TimeSpan Duration;
        public int Flag;

        public float StartAtPercent;

        public string ShowName;
        public string Group;
        public bool IsShown;

        public string AccompanyingFacialDict;
        public string AccompanyingFacialName;

        public Animation(string dict, string name, TimeSpan duration, int flag, float startAtPercent, string showName = "NoName", string group = "None", bool isShown = false, int dbId = -1) {
            DbId = dbId;

            Dictionary = dict;
            Name = name;
            Duration = duration;
            Flag = flag;

            StartAtPercent = startAtPercent;

            ShowName = showName;
            Group = group;
            IsShown = isShown;
        }

        public Animation(configanimation configanimation) {
            Dictionary = configanimation.dict;
            Name = configanimation.name;
            Duration = TimeSpan.FromMilliseconds(configanimation.duration);
            Flag = configanimation.flag;
        }
    }

    public class ItemAnimation : Animation {
        public string Model;
        public int Bone;

        [JsonIgnore]
        public Position Offset;
        [JsonIgnore]
        public DegreeRotation Rotation;

        public configitem ConfigItem;

        public int AttachVertexOrder = 2;

        public ItemAnimation(configitemanimation dbAnim) : base(dbAnim.dict, dbAnim.name, dbAnim.duration != -1 ? TimeSpan.FromMilliseconds(dbAnim.duration) : TimeSpan.FromMilliseconds(100000000), dbAnim.flag, dbAnim.startAtPercent, dbAnim.showName, dbAnim.group, dbAnim.shown == 1, dbAnim.id) {
            Model = dbAnim.modelHash;
            Offset = dbAnim.position.FromJson();
            var rot = dbAnim.rotation.FromJson<Vector3>();
            Rotation = new DegreeRotation(rot.X, rot.Y, rot.Z);
            Bone = dbAnim.bone;

            ConfigItem = dbAnim.neededItemNavigation;

            AttachVertexOrder = dbAnim.attachVertexOrder;
        }

        public ItemAnimation(string dict, string name, TimeSpan duration, int flag, float startAtPercent, string model, Position offset, Vector3 rotation, int bone = -1, configitem configItem = null, string showName = "", string group = "", bool isShown = false, int dbId = -1) : base(dict, name, duration, flag, startAtPercent, showName, group, isShown, dbId) {
            Model = model;
            Offset = offset;
            Rotation = new DegreeRotation(rotation.X, rotation.Y, rotation.Z);
            Bone = bone;

            ConfigItem = configItem;
        }
    }
}
