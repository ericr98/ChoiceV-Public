using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Controller.Discord;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Color;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChoiceVServer.Controller.Web.ExternApi.Account;
using static ChoiceVServer.Base.Constants;
using BenchmarkDotNet.Attributes;

namespace ChoiceVServer.Base {
    public static class PlayerExtensions {

        #region API Method Extensions

        /// <summary>
        /// Trigger a Client Event.
        /// </summary>
        /// <param name="eventname">The Event that should be triggered</param>
        /// <param name="args">The Event event args sent to the player</param>
        public static void emitClientEvent(this IPlayer player, string eventname, params object[] args) {
            if(!player.Exists()) {
                return;
            }

            if(player.hasData(DATA_CHARACTER_ID)) {
                Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"Event sent: {eventname}");
            }

            player.Emit(eventname, args);
        }

        /// <summary>
        /// Trigger a Cef Event.
        /// </summary>
        /// <param name="eventname">The Event that should be triggered</param>
        /// <param name="args">The Event event args sent to the player</param>
        public static void emitCefEventWithBlock(this IPlayer player, IPlayerCefEvent data, string blockMovementIdentifier) {
            if(player.Exists()) {
                WebController.emitCefEvent(player, data, true, blockMovementIdentifier);
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Event, player, $"Cef Event send to player that didnt exist! {data.Event}");
            }
        }
        
        public static void emitCefEventNoBlock(this IPlayer player, IPlayerCefEvent data) {
            if(player.Exists()) {
                WebController.emitCefEvent(player, data, false, "");
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Event, player, $"Cef Event send to player that didnt exist! {data.Event}");
            }
        }

        /// <summary>
        /// Changes the dimension of the player. Use this instead of player.Dimension!
        /// </summary>
        public static int? changeDimension(this IPlayer player, int newDimension) {
            if(!player.Exists()) return null;

            var oldDimension = player.Dimension;
            if(player.Dimension == newDimension) return null;
            
            player.Dimension = newDimension;

            EventController.onPlayerChangeDimension(player, oldDimension, newDimension);

            using(var db = new ChoiceVDb()) {
                var charId = player.getCharacterId();
                var dbChar = db.characters.FirstOrDefault(c => c.id == charId);
                if (dbChar != null) {
                    dbChar.dimension = newDimension;
                    db.SaveChanges();
                }
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"played dimension from {oldDimension} to {newDimension}");
            return oldDimension;
        }

        #endregion

        #region Custom Extensions

        /// <summary>
        /// Sets if everything is loaded for a player
        /// </summary>
        public static void setCharacterFullyLoaded(this IPlayer player, bool suc) {
            player.setData(Constants.DATA_PLAYER_FULLY_LOADED, suc);
        }

        /// <summary>
        /// Gets the CharacterData of a Player
        /// </summary>
        public static bool getCharacterFullyLoaded(this IPlayer player) {
            if(player.hasData(Constants.DATA_PLAYER_FULLY_LOADED))
                return player.getData(Constants.DATA_PLAYER_FULLY_LOADED);
            else
                return false;
        }

        /// <summary>
        /// Method to get the CharacterId of a specific ChoiceV character (set at Character Creation).
        /// </summary>
        public static int getCharacterId(this IPlayer player) {
            if(player.hasData(Constants.DATA_CHARACTER_ID))
                return player.getData(Constants.DATA_CHARACTER_ID);
            else
                return -1;
        }

        /// <summary>
        /// Get the main bankaccount of the player
        /// </summary>
        public static long getMainBankAccount(this IPlayer player) {
            try {
                using(var db = new ChoiceVDb()) {
                    return db.characters.First(c => c.id == player.getCharacterId()).bankaccount;
                }
            } catch {
                return -1;
            }

        }

        /// <summary>
        /// Gets the phone Number of the smartphone the player has equipped. This is currently his main phonenumber
        /// </summary>
        public static long getMainPhoneNumber(this IPlayer player) {
            Inventory inventory = player.getInventory();
            if(inventory != null) {
                Smartphone smartphone = inventory.getItem<Smartphone>(s => s.IsEquipped);
                if(smartphone != null)
                    return smartphone.CurrentNumber;
            }

            return -1;
        }

        /// <summary>
        /// Method to get the CharacterId of a specific ChoiceV account (set at Account Creation).
        /// </summary>
        public static int getAccountId(this IPlayer player) {
            if(player.hasData(Constants.DATA_ACCOUNT_ID))
                return player.getData(Constants.DATA_ACCOUNT_ID);
            else
                return -1;
        }

        /// <summary>
        /// Method to get the CharacterName for a specific character
        /// </summary>
        public static string getCharacterName(this IPlayer player) {
            if(player.getCharacterData() != null)
                return (player.getCharacterData().Title + " " + player.getCharacterData().FirstName + " " + player.getCharacterData().MiddleNames + " " + player.getCharacterData().LastName);
            else
                return "";
        }

        /// <summary>
        /// Method to get the CharacterName for a specific character
        /// </summary>
        public static string getCharacterShortenedName(this IPlayer player) {
            if(player.getCharacterData() != null) {
                var initials = "";
                if(!string.IsNullOrEmpty(player.getCharacterData().MiddleNames)) {
                    player.getCharacterData().MiddleNames.Split(" ").Select(s => s[0]).ForEach(c => initials += $" {c}.");
                    initials.Remove(0, 1);
                }
                return (player.getCharacterData().FirstName + " " + initials + " " + player.getCharacterData().LastName);
            } else
                return "";
        }

        /// <summary>
        /// Method to get the CharacterName for a specific character
        /// </summary>
        public static string getCharacterShortName(this IPlayer player) {
            if(player.getCharacterData() != null) {
                return (player.getCharacterData().FirstName + " " + player.getCharacterData().LastName);
            } else
                return "";
        }

        /// <summary>
        /// Get Social Security Number
        /// </summary>
        public static string getSocialSecurityNumber(this IPlayer player) {
            return player.getCharacterData().SocialSecurityNumber;
        }

        public static bool hasSocialSecurityNumber(this IPlayer player) {
            return player.getCharacterData().SocialSecurityNumber.Length > 3;
        }

        /// <summary>
        /// Method to get the Clothing of a specific character
        /// </summary>
        public static ClothingPlayer getClothing(this IPlayer player) {
            if(player.hasData(Constants.DATA_CLOTHING_SAVE))
                return player.getData(Constants.DATA_CLOTHING_SAVE);
            else
                return new ClothingPlayer();
        }

        /// <summary>
        /// Sets Inventory for a specific character
        /// </summary>
        public static void setInventory(this IPlayer player, Inventory inventory) {
            player.setData(Constants.DATA_PLAYER_INVENTORY, inventory);
        }

        /// <summary>
        /// Method to get the Inventory of a specific character
        /// </summary>
        public static Inventory getInventory(this IPlayer player) {
            if(player.hasData(Constants.DATA_PLAYER_INVENTORY)) {
                return player.getData(Constants.DATA_PLAYER_INVENTORY);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Method to get the Hunger of a specific character
        /// </summary>
        public static double getHunger(this IPlayer player) {
            if(player.getCharacterData() != null)
                return player.getCharacterData().Hunger;
            else
                return 0;
        }

        /// <summary>
        /// Method to set the Hunger of a specific character
        /// </summary>
        public static void setHunger(this IPlayer player, double hunger) {
            if(player.getCharacterData() != null)
                player.getCharacterData().Hunger = hunger;
        }

        /// <summary>
        /// Method to change the Hunger of a specific character by an amount
        /// </summary>
        public static void changeHunger(this IPlayer player, double change) {
            if(player.getCharacterData() != null) {
                player.getCharacterData().Hunger = player.getCharacterData().Hunger + change;

                if(player.getCharacterData().Hunger < 0) {
                    player.getCharacterData().Hunger = 0;
                }
            }
        }

        /// <summary>
        /// Method to get the Thirst of a specific character
        /// </summary>
        public static double getThirst(this IPlayer player) {
            if(player.getCharacterData() != null)
                return player.getCharacterData().Thirst;
            else
                return 0;
        }

        /// <summary>
        /// Method to set the Thirst of a specific character
        /// </summary>
        public static void setThirst(this IPlayer player, double thirst) {
            if(player.getCharacterData() != null)
                player.getCharacterData().Thirst = thirst;
        }

        /// <summary>
        /// Method to change the Hunger of a specific character by an amount
        /// </summary>
        public static void changeThirst(this IPlayer player, double change) {
            if(player.getCharacterData() != null) {
                player.getCharacterData().Thirst = player.getCharacterData().Thirst + change;

                if(player.getCharacterData().Thirst < 0) {
                    player.getCharacterData().Thirst = 0;
                }
            }
        }

        /// <summary>
        /// Set a CharacterData for a Player
        /// </summary>
        public static void setCharacterData(this IPlayer player, CharacterData characterData) {
            player.setData(Constants.DATA_CHARACTER_MODEL, characterData);
        }

        /// <summary>
        /// Gets the CharacterData of a Player
        /// </summary>
        public static CharacterData getCharacterData(this IPlayer player) {
            if(player.hasData(Constants.DATA_CHARACTER_MODEL))
                return player.getData(Constants.DATA_CHARACTER_MODEL);
            else
                return null;
        }

        private class PlayerFoodHudCefEvent : IPlayerCefEvent {
            public string Event { get; private set; }
            public int hunger;
            public int thirst;
            public int energy;

            public PlayerFoodHudCefEvent(IPlayer player) {
                Event = "UPDATE_FOOD_HUD";
                var data = player.getCharacterData();
                hunger = (int)data.Hunger;
                thirst = (int)data.Thirst;
                energy = (int)data.Energy;
            }
        }

        /// <summary>
        /// Updates the Hud of the Player
        /// </summary>
        public static void updateHud(this IPlayer player) {
            player.emitCefEventNoBlock(new PlayerFoodHudCefEvent(player));
        }

        /// <summary>
        /// Shows a specific Menu to a player
        /// </summary>
        public static void showMenu(this IPlayer player, Menu menu, bool blockmovement = true, bool playerBusy = true) {
            if(menu.getMenuItemCount() == 1 && menu.getMenuItemByIndex(0) is MenuMenuItem menuMenuItem && menuMenuItem.SubMenu != null) {
                var onlySubMenu = menu.getMenuItemByIndex(0) as MenuMenuItem;
                MenuController.showMenu(player, onlySubMenu.SubMenu, blockmovement, playerBusy);
            } else {
                MenuController.showMenu(player, menu, blockmovement, playerBusy);
            }
        }
        
        public static void showUpdatingMenu(this IPlayer player, Menu menu, string menuUpdateIdentifier, bool blockMovement = true, bool playerBusy = true) {
            if(menu.getMenuItemCount() == 1 && menu.getMenuItemByIndex(0) is MenuMenuItem menuMenuItem && menuMenuItem.SubMenu != null) {
                var onlySubMenu = menu.getMenuItemByIndex(0) as MenuMenuItem;
                
                onlySubMenu.SubMenu.UpdatingIdentifier = menuUpdateIdentifier;
                MenuController.showMenu(player, onlySubMenu.SubMenu, blockMovement, playerBusy, menuUpdateIdentifier);
            } else {
                menu.UpdatingIdentifier = menuUpdateIdentifier;
                MenuController.showMenu(player, menu, blockMovement, playerBusy, menuUpdateIdentifier);
            }
        }


        /// <summary>
        /// Shows a specific Menu to a player
        /// </summary>
        public static void closeMenu(this IPlayer player) {
            MenuController.closeMenu(player);
        }

        /// <summary>
        /// Shows a specific colorpicker to a player
        /// </summary>
        public static void showColorPicker(this IPlayer player, ColorPicker color, bool blockMovement = true) {
            ColorPickerController.showColorPicker(player, color, blockMovement);
        }

        /// <summary>
        /// Closes the colorpicker to a player
        /// </summary>
        public static void closeColorPicker(this IPlayer player) {
            ColorPickerController.closeColorPicker(player);
        }

        /// <summary>
        /// Shows Player a specific Inventory
        /// </summary>
        public static void openInventory(this IPlayer player, Inventory inventory) {
            if(inventory != null) {
                InventoryController.showInventory(player, inventory);
            }
        }

        /// <summary>
        /// Close Player Inventory
        /// </summary>
        public static void closeInventory(this IPlayer player) {
            WebController.sendClosePlayerCefElements(player, new OnlyEventCefEvent("CLOSE_INVENTORY"));
        }

        /// <summary>
        /// Adds a state to the player. Depending on all current states the player has he may be blocked/allowed to do some actions (interact, attack, etc.)
        /// </summary>
        public static void addState(this IPlayer player, PlayerStates state) {
            lock(player) {
                if(player.hasData(Constants.DATA_CHARACTER_STATE)) {
                    var list = (List<PlayerStates>)player.getData(Constants.DATA_CHARACTER_STATE);
                    if(!list.Contains(state)) {
                        list.Add(state);
                    }
                    player.setData(Constants.DATA_CHARACTER_STATE, list);
                } else {
                    var list = new List<PlayerStates> {
                        state
                    };
                    player.setData(Constants.DATA_CHARACTER_STATE, list);
                }

                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player got state {state} added");
            }
        }

        /// <summary>
        /// Removes a specific state.
        /// </summary>
        public static void removeState(this IPlayer player, PlayerStates state) {
            lock(player) {
                if(player.hasData(Constants.DATA_CHARACTER_STATE)) {
                    var list = (List<PlayerStates>)player.getData(Constants.DATA_CHARACTER_STATE);
                    list.Remove(state);

                    Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player got state {state} removed");
                }
            }
        }

        /// <summary>
        /// Returns if the player has a specific state set
        /// </summary>
        public static bool hasState(this IPlayer player, PlayerStates state) {
            lock(player) {
                if(player.hasData(Constants.DATA_CHARACTER_STATE)) {
                    var list = (List<PlayerStates>)player.getData(Constants.DATA_CHARACTER_STATE);
                    return list.Contains(state);
                } else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets if the current state of the player makes him busy. This is done by combining the states and checking if any state is higher than 20!
        /// </summary>
        public static bool getBusy(this IPlayer player, List<PlayerStates> ignoredStates = null) {
            if(player.getData(Constants.DATA_CHARACTER_STATE) != null) {
                var list = (List<PlayerStates>)player.getData(Constants.DATA_CHARACTER_STATE);
                return list.Any(s => (int)s >= 20 && (ignoredStates == null || !ignoredStates.Any(ss => ss == s)));
            } else {
                return false;
            }
        }

        /// <summary>
        /// Gets if the current state of the player makes him knocked out (Anesthesia, Dead, etc.)
        /// </summary>
        public static bool isKnockedOut(this IPlayer player) {
            if(player.getData(Constants.DATA_CHARACTER_STATE) != null) {
                var list = (List<PlayerStates>)player.getData(Constants.DATA_CHARACTER_STATE);
                return list.Any(s => (int)s >= 45);
            } else {
                return false;
            }
        }

        /// <summary>
        /// Bans the selected player
        /// </summary>
        public static bool ban(this IPlayer player, string reason, bool overrideAdmin = false) {
            var isBanned = AccountHelper.ban(player.getAccountId(), reason, overrideAdmin);
            
            player.Kick(reason);

            return isBanned;
        }

        /// <summary>
        /// Plays a player anim for a specific duration
        /// </summary>
        /// <param name="animDict">The Dictionary of the animation</param>
        /// <param name="animName">The name of the animation</param>
        /// <param name="duration">The duration the animation shall be played</param>
        /// <param name="time">From 0 to 1. Defines at which percent the animation shall start</param>
        public static void playAnimation(this IPlayer player, string animDict, string animName, int duration, int flag, Rotation? facingRotation = null, float time = 0) {
            if(facingRotation != null) {
                player.Rotation = facingRotation ?? Rotation.Zero;
            }

            player.setData("ANIMATION", new Animation(animDict, animName, TimeSpan.FromMilliseconds(duration), flag, time));
            player.emitClientEvent(Constants.PlayerPlayAnimation, animDict, animName, duration, flag, facingRotation != null ? facingRotation.Value.Yaw : -1, false, false, time);
        }

        /// <summary>
        /// Plays a player anim for a specific duration
        /// </summary>
        /// <param name="animDict">The Dictionary of the animation</param>
        /// <param name="animName">The name of the animation</param>
        /// <param name="duration">The duration the animation shall be played</param>
        public static void playBackgroundAnimation(this IPlayer player, string animDict, string animName, int duration, int flag, Rotation? facingRotation = null, float time = 0) {
            if(facingRotation != null) {
                player.Rotation = facingRotation ?? Rotation.Zero;
            }
            player.emitClientEvent(Constants.PlayerPlayAnimation, animDict, animName, duration, flag, facingRotation != null ? facingRotation.Value.Yaw : -1, true, false, time);
        }

        /// <summary>
        /// Plays a player anim for a specific duration
        /// </summary>
        /// <param name="animDict">The Dictionary of the animation</param>
        /// <param name="animName">The name of the animation</param>
        /// <param name="duration">The duration the animation shall be played</param>
        /// <param name="time">From 0 to 1. Defines at which percent the animation shall start</param>
        public static void forceAnimation(this IPlayer player, string animDict, string animName, int duration, int flag, Rotation? facingRotation = null, float time = 0) {
            if(facingRotation != null) {
                player.Rotation = facingRotation ?? Rotation.Zero;
            }

            player.emitClientEvent(Constants.PlayerPlayAnimation, animDict, animName, duration, flag, facingRotation != null ? facingRotation.Value.Yaw : -1, false, true, time);
        }


        /// <summary>
        /// Plays a player anim for a specific duration
        /// </summary>
        /// <param name="anim">The Animation that should be played</param>
        public static void playAnimation(this IPlayer player, Animation anim, Rotation? facingRotation = null, bool playerBusy = true) {
            if(anim is null) return;

            if(anim is ItemAnimation) {
                AnimationController.playItemAnimation(player, anim as ItemAnimation, facingRotation, playerBusy);
            } else {
                player.setData("ANIMATION", anim);
                playAnimation(player, anim.Dictionary, anim.Name, (int)anim.Duration.TotalMilliseconds, anim.Flag, facingRotation, anim.StartAtPercent);
            }
        }

        /// <summary>
        /// Plays a player anim for a specific duration
        /// </summary>
        /// <param name="animDict">The Dictionary of the animation</param>
        /// <param name="animName">The name of the animation</param>
        /// <param name="duration">The duration the animation shall be played</param>
        /// <param name="time">From 0 to 1. Defines at which percent the animation shall start</param>
        public static void forceAnimation(this IPlayer player, Animation anim, Rotation? facingRotation = null, bool playerBusy = true) {
            if(facingRotation != null) {
                player.Rotation = facingRotation ?? Rotation.Zero;
            }

            player.emitClientEvent(Constants.PlayerPlayAnimation, anim.Dictionary, anim.Name, (int)anim.Duration.TotalMilliseconds, anim.Flag, facingRotation != null ? facingRotation.Value.Yaw : -1, false, true, anim.StartAtPercent);
        }

        /// <summary>
        /// Plays a player anim for a specific duration
        /// </summary>
        /// <param name="animDict">The Dictionary of the animation</param>
        /// <param name="animName">The name of the animation</param>
        /// <param name="duration">The duration the animation shall be played</param>
        public static void playBackgroundAnimation(this IPlayer player, Animation anim, Rotation? facingRotation = null) {
            player.playBackgroundAnimation(anim.Dictionary, anim.Name, (int)anim.Duration.TotalMilliseconds, anim.Flag, facingRotation);
        }
        
        public static void playFacialAnimation(this IPlayer player, string animDict, string animName, double duration) {
            player.emitClientEvent("PLAY_FACIAL_ANIM", animDict, animName, duration);
        }
        
        
        /// <summary>
        /// Stops the current Animation
        /// </summary>
        public static void stopAnimation(this IPlayer player) {
            AnimationController.stopAnimation(player);
        }

        public static Animation getInteractAnimation(this IPlayer player, Position position) {
            if(position.Z < player.Position.Z) {
                return AnimationController.getAnimationByName("KNEEL_DOWN");
            } else { 
                return AnimationController.getAnimationByName("WORK_FRONT");
            }
        }

        /// <summary>
        /// Sets player currently interacting
        /// </summary>
        public static void setPlayerInteracting(this IPlayer player, bool toggle) {
            player.setData(Constants.DATA_CHARATCER_INTERACTING, toggle);
        }

        /// <summary>
        /// Gets player currently interacting
        /// </summary>
        public static bool getPlayerInteracting(this IPlayer player) {
            if(player.hasData(Constants.DATA_CHARATCER_INTERACTING)) {
                return player.getData(Constants.DATA_CHARATCER_INTERACTING);
            } else {
                return false;

            }
        }

        /// <summary>
        /// Sets Health for Player. In a scale from -1 to 100, where -1 is death
        /// </summary>
        public static void setHealth(this IPlayer player, float health, bool ignoreDamage) {
            if(ignoreDamage) {
                player.ignoreNextDamage();
            }
            player.Health = Convert.ToUInt16(Math.Min(health + 100, 200));
        }

        /// <summary>
        /// Sets Health for Player. In a scale from 0 to 100
        /// </summary>
        public static float getHealth(this IPlayer player) {
            return (player.Health - 100);
        }

        /// <summary>
        /// Ignores the next time the player takes damage
        /// </summary>
        public static void ignoreNextDamage(this IPlayer player) {
            player.setData("IGNORE_NEXT_DAMAGE", true);
        }

        /// <summary>
        /// Get a Rotation (only containing a yaw) were the player will face towards the position on a 2d (x,y) plane
        /// </summary>
        public static DegreeRotation getRotationTowardsPosition(this IPlayer player, Position towards, bool setRotationForPlayer = false) {
            Vector2 pixelpos = new Vector2(towards.X - player.Position.X, towards.Y - player.Position.Y);
            var yaw = Math.Atan2(pixelpos.Y, pixelpos.X);

            yaw = 180 * yaw / Math.PI;
            yaw = ((360 + Math.Round(yaw)) % 360) - 90;

            if(setRotationForPlayer) {
                player.emitClientEvent(Constants.PlayerSetHeading, yaw);
            }

            return new DegreeRotation(0, 0, (float)yaw);
        }

        /// <summary>
        /// Gets the forward vector (2d) of the current player based on the character orientation
        /// </summary>
        public static Vector2 getForwardVector(this IPlayer player) {
            var rot = player.Rotation;
            return Vector2.Normalize(new Vector2((float)Math.Cos((rot.Yaw + Math.PI / 2)), (float)Math.Sin((rot.Yaw + Math.PI / 2))));
        }

        /// <summary>
        /// Gets the forward vector (2d) based on the camera rotation
        /// </summary>
        public static void getCameraForwardVector(this IPlayer player, Action<IPlayer, Vector2> callback) {
            CallbackController.getPlayerCameraHeading(player, (p, heading) => {
                var yaw = (ChoiceVAPI.degreesToRadians(heading));
                var forward = Vector2.Normalize(new Vector2((float)Math.Cos(yaw + Math.PI / 2), (float)Math.Sin(yaw + Math.PI / 2)));
                callback.Invoke(player, forward);
            });
        }

        private class NotificationCefEvent : IPlayerCefEvent {
            public string Event { get; set; }

            public string title;
            public string message;
            public string imgName;
            public string type;
            public string replaceCategory;

            public NotificationCefEvent(string title, string message, NotifactionImages image, NotifactionTypes type, string replaceCategory) {
                Event = "CREATE_NOTIFICATION";
                this.title = title;
                this.message = message;
                this.imgName = image.ToString() + ".png";
                this.type = type.ToString();
                this.replaceCategory = replaceCategory;
            }
        }

        /// <summary>
        /// Send a Notification to a client, observable over the minimap.
        /// </summary>
        /// <param name="replaceCategory">Can be used to replace an already shown Notification with the same type</param>
        public static void sendNotification(this IPlayer player, NotifactionTypes type, string message, string shortInfo, NotifactionImages image = NotifactionImages.System, string replaceCategory = null) {
            player.emitCefEventNoBlock(new NotificationCefEvent("Info", message, image, type, replaceCategory));
            player.setData("LAST_SHORT_INFO", shortInfo);
        }

        public static string getLastInfo(this IPlayer player) {
            if(player.hasData("LAST_SHORT_INFO")) {
                return player.getData("LAST_SHORT_INFO");
            } else {
                return "Kein Info";
            }
        }

        /// <summary>
        /// Send a Notification to a client, observable over the minimap.
        /// </summary>
        public static void sendBlockNotification(this IPlayer player, string message, string shortInfo, NotifactionImages image = NotifactionImages.System, string replaceCategory = null) {
            player.emitCefEventNoBlock(new NotificationCefEvent("Fehler", message, image, NotifactionTypes.Danger, replaceCategory));
            player.setData("LAST_SHORT_INFO", shortInfo);
        }

        /// <summary>
        /// Sets a timecycle filter with given strength for a player
        /// </summary>
        public static void setTimeCycle(this IPlayer player, string id, string timecycle, float strength) {
            player.emitClientEvent("SET_TIMECYCLE", id, timecycle, strength);
        }

        /// <summary>
        /// Removes current TimeCycle for a aplayer
        /// </summary>
        public static void stopTimeCycle(this IPlayer player, string id) {
            player.emitClientEvent("REMOVE_TIMECYCLE", id);
        }

        /// <summary>
        /// Sets a timecycle filter with given strength for a player
        /// </summary>
        public static void setAdditionalTimeCycle(this IPlayer player, string id, string timecycle) {
            player.emitClientEvent("SET_ADDITIONAL_TIMECYCLE", id, timecycle);
        }

        public static void stopAdditionalTimeCycle(this IPlayer player, string id) {
            player.emitClientEvent("REMOVE_ADDITIONAL_TIMECYCLE", id);
        }


        /// <summary>
        /// Fades the screen of the player to be black or from black to screen
        /// </summary>
        /// <param name="fadeOut">if true, the screen goes black, if false the screen fades in</param>
        /// <param name="duration">in milliseconds</param>
        public static void fadeScreen(this IPlayer player, bool fadeOut, int duration) {
            player.emitClientEvent("FADE_SCREEN", fadeOut, duration);

            Logger.logTrace(LogCategory.System, LogActionType.Updated, player, $"player screen was faded {(fadeOut ? "out" : "in")} for {duration}ms");
        }

        /// <summary>
        /// Gets the cash the player currently has
        /// </summary>
        public static decimal getCash(this IPlayer player) {
            return player.getCharacterData().Cash;
        }

        /// <summary>
        /// Adds cash to the player
        /// </summary>
        public static void addCash(this IPlayer player, decimal amount) {
            lock(player.getCharacterData()) {
                amount = decimal.Round(amount, 2);
                player.getCharacterData().Cash += amount;

                Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"player got {amount} cash ADDED and now has {player.getCharacterData().Cash}");
            }
        }

        /// <summary>
        /// Removes cash from the player
        /// </summary>
        /// <returns>If player has enough cash</returns>
        public static bool removeCash(this IPlayer player, decimal amount) {
            var data = player.getCharacterData();
            lock(data) {
                amount = decimal.Round(amount, 2);
                if(data.Cash >= amount) {
                    data.Cash -= amount;

                    Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"player got {amount} cash REMOVED and know has {player.getCharacterData().Cash}");
                    return true;
                } else {
                    return false;
                }
            }
        }

        //Permanent Data
        public static void setPermanentData(this IPlayer player, string name, string value) {
            player.setData(name, value);

            using(var db = new ChoiceVDb()) {
                var already = db.characterdata.Find(player.getCharacterId(), name);
                if(already != null) {
                    already.value = value;
                } else {
                    var newData = new characterdatum {
                        charId = player.getCharacterId(),
                        name = name,
                        value = value,
                    };

                    db.characterdata.Add(newData);
                }

                db.SaveChanges();

                Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"player got permanent data {name} set to {value}");
            }
        }

        public static void resetPermantData(this IPlayer player, string name) {
            player.resetData(name);

            using(var db = new ChoiceVDb()) {
                var data = db.characterdata.Find(player.getCharacterId(), name);
                if(data != null) {
                    db.characterdata.Remove(data);
                } else {
                    Logger.logWarning(LogCategory.Player, LogActionType.Blocked, $"resetPermantData: Tried to remove data {name} which was not found!");
                }

                db.SaveChanges();

                Logger.logDebug(LogCategory.Player, LogActionType.Removed, player, $"player got permanent data {name} reset");
            }
        }

        public static void setPedRagdoll(this IPlayer player) {
            player.emitClientEvent("SET_RAGDOLL");
        }

        public static void stopPedRagdoll(this IPlayer player) {
            player.emitClientEvent("STOP_RAGDOLL");
        }

        public static Islands getIsland(this IPlayer player) {
            return player.getCharacterData().Island;
        }

        public static void setStyle(this IPlayer player, characterstyle style) {
            player.SetHeadBlendData((uint)style.faceFather, (uint)style.faceMother, 0, (uint)style.faceFather, (uint)style.faceMother, 0, style.faceShape, style.faceSkin, 0);

            for(byte i = 1; i <= 12; i++) {
                var value = ((int)style.GetType().GetProperty($"overlay_{i}").GetValue(style));
                var overrideFloat = false;
                if(value <= 0) {
                    value = 0;
                    overrideFloat = true;
                }
                if(i == 4 || i == 8) {
                    player.SetHeadOverlay(i, value.ToByte(), overrideFloat ? 0 : (float)style.GetType().GetProperty($"overlay_{i}_opacity").GetValue(style));
                } else {
                    player.SetHeadOverlay(i, value.ToByte(), overrideFloat ? 0 : 1f);
                }
            }

            player.SetHeadOverlayColor(1, 1, (byte)style.overlaycolor_1, (byte)style.overlayhighlight_1);
            player.SetHeadOverlayColor(2, 1, (byte)style.overlaycolor_2, (byte)style.overlayhighlight_2);
            player.SetHeadOverlayColor(4, 2, (byte)style.overlaycolor_4, (byte)style.overlayhighlight_4);
            player.SetHeadOverlayColor(5, 2, (byte)style.overlaycolor_5, (byte)style.overlayhighlight_5);
            player.SetHeadOverlayColor(8, 2, (byte)style.overlaycolor_8, (byte)style.overlayhighlight_8);
            player.SetHeadOverlayColor(10, 1, (byte)style.overlaycolor_10, 0);

            ChoiceVAPI.SetPlayerClothes(player, 2, (ushort)style.hairStyle, 0);
            player.HairColor = style.hairColor.ToByte();
            player.HairHighlightColor = style.hairHighlight.ToByte();
            player.SetEyeColor(style.faceEyes.ToByte());

            for(byte i = 0; i <= 18; i++) {
                player.SetFaceFeature(i, (float)style.GetType().GetProperty($"faceFeature_{i}").GetValue(style));
            }

            Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"player got style set");

            //player.emitClientEvent(Constants.PlayerSetStyle, style.ToJson());
        }


        private record PlayerOverlay(string Type, string Dict, string Name);
        /// <summary>
        /// Sets a overlay for a player. Is a GTA Decoration, so for removal puproses (tattoos are decorations too), give a type 
        /// </summary>
        public static void setOverlay(this IPlayer player, string type, string overlayDict, string overlayName) {
            var stringList = new Dictionary<string, List<PlayerOverlay>>();

            if(player.hasData("DECORATION_LIST")) {
                stringList = (Dictionary<string, List<PlayerOverlay>>)player.getData("DECORATION_LIST");
            } 

            if(stringList.ContainsKey(type)) {
                var element = stringList[type];
                element.Add(new PlayerOverlay(type, overlayDict, overlayName));
                stringList[type] = element;
            } else {
                stringList[type] = new List<PlayerOverlay> { new PlayerOverlay(type, overlayDict, overlayName) };
            }

            player.setData("DECORATION_LIST", stringList);
            
            player.AddDecoration(ChoiceVAPI.Hash(overlayDict), ChoiceVAPI.Hash(overlayName));
            //player.emitClientEvent("SET_PLAYER_DECORATION", $"{type}#{overlayDict}#{overlayName}");
        }

        /// <summary>
        /// Sets a overlay for a player. Is a GTA Decoration, so for removal puproses (tattoos are decorations too), give a type 
        /// </summary>
        /// 
        public static void resetOverlayType(this IPlayer player, string type) {
            if(player.hasData("DECORATION_LIST")) {
                var overlays = (Dictionary<string, List<PlayerOverlay>>)player.getData("DECORATION_LIST");

                if(overlays.ContainsKey(type)) {
                    foreach(var overlay in overlays[type]) {
                        player.RemoveDecoration(ChoiceVAPI.Hash(overlay.Dict), ChoiceVAPI.Hash(overlay.Name));
                    }

                    overlays.Remove(type);
                }
            }

            //player.emitClientEvent("RESET_PLAYER_DECORATION_TYPE", type);
        }

        public static List<string> getOverlayStringForType(this IPlayer player, string type) {
            List<string> list = new();
            if(player.hasData("DECORATION_LIST")) {
                var overlays = (Dictionary<string, List<PlayerOverlay>>)player.getData("DECORATION_LIST");

                if(overlays.ContainsKey(type)) {
                    foreach(var part in overlays[type]) {
                        list.Add($"{type}#{part.Dict}#{part.Name}");
                    }
                }
            }

            return list;
        }


        //Old Overlay methods only delete if everything works with altv sync

        //public static void setOverlay(this IPlayer player, string type, string overlayDict, string overlayName) {
        //    var stringList = "";
        //    if(player.HasStreamSyncedMetaData("DECORATION_LIST")) {
        //        player.GetStreamSyncedMetaData("DECORATION_LIST", out stringList);
        //    }

        //    if(stringList != "") {
        //        stringList += $",{type}#{overlayDict}#{overlayName}";
        //    } else {
        //        stringList = $"{type}#{overlayDict}#{overlayName}";
        //    }

        //    player.SetStreamSyncedMetaData("DECORATION_LIST", stringList);
        //    //player.emitClientEvent("SET_PLAYER_DECORATION", player, type, overlayDict, overlayName);

        //    Logger.logTrace(player, $"player got overlay added {type} {overlayDict} {overlayName}");
        //}

        //public static void resetOverlayType(this IPlayer player, string type) {
        //    List<string> list = new();
        //    if(player.HasStreamSyncedMetaData("DECORATION_LIST")) {
        //        player.GetStreamSyncedMetaData("DECORATION_LIST", out object stringList);

        //        foreach(var part in stringList.ToString().Split(",")) {
        //            list.Add(part);
        //        }

        //        list.RemoveAll(p => p.StartsWith(type));
        //        if(list.Count > 0) {
        //            var newStrList = list.First();
        //            list.Skip(1).ForEach(p => newStrList += $",{p}");
        //            player.SetStreamSyncedMetaData("DECORATION_LIST", list);
        //        } else {
        //            player.DeleteStreamSyncedMetaData("DECORATION_LIST");
        //        }
        //    }

        //    Logger.logTrace(player, $"player got overlay type reset {type}");

        //    player.emitClientEvent("REMOVE_PLAYER_DECORATION_BY_TYPE", player, type);
        //}

        //public static List<string> getOverlayStringForType(this IPlayer player, string type) {
        //    List<string> list = new();
        //    if(player.HasStreamSyncedMetaData("DECORATION_LIST")) {
        //        player.GetStreamSyncedMetaData("DECORATION_LIST", out object stringList);

        //        foreach(var part in stringList.ToString().Split(",")) {
        //            if(part.StartsWith(type)) {
        //                list.Add(part);
        //            }
        //        }
        //    }

        //    return list;
        //}

        public static string getCharSetting(this IPlayer player, string identifier) {
            return CharacterSettingsController.getCharacterSettingValue(player, identifier);
        }

        public static bool getCharFlagSetting(this IPlayer player, string identifier) {
            return bool.Parse(CharacterSettingsController.getCharacterSettingValue(player, identifier));
        }

        /// <summary>
        /// Get Char-Drug with specific identifier
        /// </summary>
        public static PlayerDrugLevel getDrug(this IPlayer player, string identifier) {
            var dat = player.getCharacterData();
            if(dat.DrugLevels.ContainsKey(identifier)) {
                return dat.DrugLevels[identifier];
            } else {
                return null;
            }
        }

        public static int getMedicatedPainLevel(this IPlayer player) {
            var meds = DrugController.getDrugsByPredicate<MedicationDrug>(d => true);
            return PainMedicationController.getMedicatedPainLevel(meds, player);
        }

        public static bool isInfiniteAirToggled(this IPlayer player) {
            return player.hasData("INFINITE_AIR_TOGGLE");
        }

        public static void toggleInfiniteAir(this IPlayer player, bool toggle, bool setData = true) {
            if(setData) {
                if(toggle) {
                    player.setData("INFINITE_AIR_TOGGLE", toggle);
                } else {
                    player.resetData("INFINITE_AIR_TOGGLE");
                }
            }

            player.emitClientEvent("TOGGLE_INFINITE_AIR", toggle);

            Logger.logTrace(LogCategory.System, LogActionType.Updated, player, $"player got toggled infinite air {toggle}");
        }

        public static bool isFireProofToggled(this IPlayer player) {
            return player.hasData("FIRE_PROOF_TOGGLE");
        }

        public static void toggleFireProof(this IPlayer player, bool toggle, bool setData = true) {
            if(setData) {
                if(toggle) {
                    player.setData("FIRE_PROOF_TOGGLE", toggle);
                } else {
                    player.resetData("FIRE_PROOF_TOGGLE");
                }
            }

            player.emitClientEvent("TOGGLE_FIRE_PROOF", toggle);

            Logger.logTrace(LogCategory.System, LogActionType.Updated, player, $"player got toggled fireproof {toggle}");
        }

        public static bool getIsAttackToggled(this IPlayer player) {
            return player.hasData("TOOGLE_NO_ATTACK");
        }

        public static void toggleCannotAttack(this IPlayer player, bool toggle, string setter) {
            var list = new List<string>();
            if(player.hasData("TOOGLE_NO_ATTACK_SETTER")) {
                list = (List<string>)player.getData("TOOGLE_NO_ATTACK_SETTER");
            }

            if(toggle) {
                if(!list.Contains(setter)) {
                    list.Add(setter);
                }
            } else {
                list.Remove(setter);
            }

            player.setData("TOOGLE_NO_ATTACK_SETTER", list);

            if(toggle || (!toggle && list.Count == 0)) {
                player.emitClientEvent("TOOGLE_NO_ATTACK", toggle);
                Logger.logTrace(LogCategory.System, LogActionType.Updated, player, $"player got toggled can attack {toggle} by {setter}");
            } else {
                Logger.logTrace(LogCategory.System, LogActionType.Updated, player, $"player got toggled can attack {toggle} by {setter} but was blocked by {list.ToJson()}");
            }
        }

        public static PlayerCrimeReputation getCrimeReputation(this IPlayer player) {
            return player.getData("CRIME_REPUTATION");
        }

        public static bool hasCrimeFlag(this IPlayer player) {
            return player.getCharacterData().CharacterFlag.HasFlag(CharacterFlag.CrimeFlag); 
        }

        public static bool isCopOrRobber(this IPlayer player) {
            return hasCrimeFlag(player) || isCop(player);
        }

        public static bool isCop(this IPlayer player) {
            return CompanyController.getCompanies(player).Any(c => c.CompanyType == CompanyType.Fbi || c.CompanyType == CompanyType.Police || c.CompanyType == CompanyType.Sheriff);
        }

        public static bool hasLiteMode(this IPlayer player) {
            return ((AccountFlag)player.getData("ACCOUNT_FLAG")).HasFlag(AccountFlag.LiteModeActivated);
        }

        public static ulong getDiscordId(this IPlayer player) {
            return (ulong)player.getData("DISCORD_ID");
        }

        public static string getLoginToken(this IPlayer player) {
            return (string)player.getData("ENCRYPT_KEY");
        }

        public static string getAccountToken(this IPlayer player) {
            return (string)player.getData("ACCOUNT_TOKEN");
        }

        public static int getAdminLevel(this IPlayer player) {
            if(!Config.IsDiscordBotEnabled) {
                return Config.IsDevServer ? 5 : 0;
            }
            
            var levelTask = DiscordController.getAdminLevelOfUser(player);
            levelTask.Wait();
            
            return levelTask.Result;
        }
        
        public static CharacterType getCharacterType(this IPlayer player) {
            return player.getCharacterData().CharacterType;
        }

        public static bool isAnimal(this IPlayer player) {
            var type = player.getCharacterData()?.CharacterType;
            return type != null && (type == CharacterType.Dog || type == CharacterType.Cat);
        }

        #endregion 
    }
}
