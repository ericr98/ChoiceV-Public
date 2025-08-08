using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Elements.Pools;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Discord;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Base {
    public static class ChoiceVAPI {
        #region Custom API Methods
        /// <summary>
        /// Gets the Client with the given ChracterId. Returns null if Client is not on the Server.
        /// </summary>
        /// <param name="characterId">CharacterId identifying the Client</param>
        public static IPlayer getClientByCharacterId(int characterId) {
            return GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == characterId);
        }

        /// <summary>
        /// Gets the currently active Client with the given AccountId. Returns null if no client of the account is on the Server.
        /// </summary>
        /// <param name="accountId">AccountId identifying the Client</param>
        public static IPlayer getClientByAccountId(int accountId) {
            return GetAllPlayers().FirstOrDefault(p => p.getAccountId() == accountId);
        }

        /// <summary>
        /// This Methods return the Sha256 hashed version of a given string. 
        /// </summary>
        /// <returns>Hashed version of a string. eg for passwords</returns>
        public static string GetHashSha256(string input, string salt) {
            using(SHA256 sha256Hash = SHA256.Create()) {
                //Concatinate the string with a random (but predefined) salt
                var saltedInput = string.Concat(input, salt);

                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(saltedInput));
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < bytes.Length; i++) {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets Player nearest to Position within the given maxDistance. Given predecate is also checked.
        /// </summary>
        public static IPlayer FindNearbyPlayer(Position position, float maxDistance = 15, Predicate<IPlayer> predicate = null) {
            try {
                if(predicate == null)
                    predicate = new Predicate<IPlayer>(target => true);

                return Alt.GetAllPlayers().Where(veh => (veh.Position.Distance(position) <= maxDistance) && predicate(veh)).OrderBy(v => position.Distance(v.Position)).FirstOrDefault();
            } catch(Exception e) {
                Logger.logException(e, $"FindNearbyPlayer: Exception thrown with position {position.ToJson()}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets Players nearest to Position within the given maxDistance. Given predecate is also checked.
        /// </summary>
        public static IEnumerable<IPlayer> FindNearbyPlayers(Position position, float maxDistance = 15, Predicate<IPlayer> predicate = null) {
            try {
                if(predicate == null)
                    predicate = new Predicate<IPlayer>(target => true);

                return Alt.GetAllPlayers().Where(player => (player.Position.Distance(position) <= maxDistance) && predicate(player));
            } catch(Exception e) {
                Logger.logException(e, $"FindNearbyPlayer: Exception thrown with position {position.ToJson()}");
                return null;
            }
        }

        /// <summary>
        /// Gets Vehicles nearest to player within the given maxDistance. Given predecate is also checked.
        /// </summary>
        public static ChoiceVVehicle FindNearbyVehicle(IPlayer player, float maxDistance = 15, Predicate<ChoiceVVehicle> predicate = null, bool factorInSize = false) {
            try {
                if(predicate == null)
                    predicate = new Predicate<ChoiceVVehicle>(target => true);

                return Alt.GetAllVehicles().Cast<ChoiceVVehicle>().Where(veh => (veh.Dimension == player.Dimension) && (veh.Position.Distance(player.Position) <= maxDistance + veh.getSize()) && predicate(veh)).OrderBy(v => player.Position.Distance(v.Position)).FirstOrDefault();
            } catch(Exception e) {
                Logger.logException(e, $"FindNearbyVehicle: Exception thrown with player {player.getCharacterId()}");
                return null;
            }
        }

        /// <summary>
        /// Gets Vehicles near to player within the given maxDistance. Given predecate is also checked.
        /// </summary>
        public static IEnumerable<ChoiceVVehicle> FindNearbyVehicles(IPlayer player, float maxDistance = 15, Predicate<ChoiceVVehicle> predicate = null) {
            try {
                if(predicate == null)
                    predicate = new Predicate<ChoiceVVehicle>(target => true);

                return Alt.GetAllVehicles().Cast<ChoiceVVehicle>().Where(veh => (veh.Dimension == player.Dimension) && (veh.Position.Distance(player.Position) <= maxDistance + veh.getSize()) && predicate(veh)).OrderBy(v => player.Position.Distance(v.Position)).Select(v => v);
            } catch(Exception e) {
                Logger.logException(e, $"FindNearbyVehicles: Exception thrown with player {player.getCharacterId()}");
                return null;
            }
        }

        /// <summary>
        /// Gets Vehicles near to give Position within the given maxDistance. Given predecate is also checked.
        /// </summary>
        public static ChoiceVVehicle FindNearbyVehicleAtPosition(IPlayer player, Position position, float maxDistance = 15, Predicate<ChoiceVVehicle> predicate = null) {
            try {
                if(predicate == null)
                    predicate = new Predicate<ChoiceVVehicle>(target => true);

                return Alt.GetAllVehicles().Cast<ChoiceVVehicle>().Where(veh => (veh.Dimension == player.Dimension) && (veh.Position.Distance(position) <= maxDistance) && predicate(veh)).OrderBy(v => position.Distance(v.Position)).FirstOrDefault();
            } catch(Exception e) {
                Logger.logException(e, $"FindNearbyVehicle: Exception thrown with player {player.getCharacterId()}");
                return null;
            }
        }

        /// <summary>
        /// Get the Forward Vector from a position
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Vector2 getForwardVector(float heading, bool correctAltVHeading = false) {
            if(correctAltVHeading) {
                heading = heading + (float)Math.PI / 2;
            }
            return Vector2.Normalize(new Vector2((float)Math.Cos((heading)), (float)Math.Sin((heading))));
        }

        /// <summary>
        /// Get the GTA Radians Value of a Degree Value
        /// </summary>
        public static float getGTARadians(float heading) {
            return (((180 - heading) * 3.14159f) / 180);
        }

        public static float radiansToDegrees(float radians) {
            return (radians * (180 / 3.14159f));
        }

        public static float degreesToRadians(float degrees) {
            return (degrees * (3.14159f / 180));
        }
        
        public static float checkDistanceV2(float x1, float y1, float x2, float y2) {
            float a = x1 - x2;
            float b = y1 - y2;

            return (float)Math.Sqrt(a * a + b * b);
        }

        public static float getAreaTriangle(float x1, float y1, float x2, float y2, float x3, float y3) {
            return (float)Math.Abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);
        }

        public static bool checkPointInRect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, float x, float y) {
            float A1 = getAreaTriangle(x1, y1, x2, y2, x3, y3) + getAreaTriangle(x1, y1, x4, y4, x3, y3);
            float A2 = getAreaTriangle(x, y, x1, y1, x2, y2);
            float A3 = getAreaTriangle(x, y, x2, y2, x3, y3);
            float A4 = getAreaTriangle(x, y, x3, y3, x4, y4);
            float A5 = getAreaTriangle(x, y, x1, y1, x4, y4);

            float A = (float)Math.Round(A1, 3);
            float B = (float)Math.Round(A2 + A3 + A4 + A5, 3);

            return (A == B);
        }

        public static Vector2 rotatePointInRect(float pointX, float pointY, float originX, float originY, float angle) {
            return new Vector2((float)(Math.Cos(angle) * (pointX - originX) - Math.Sin(angle) * (pointY - originY) + originX), (float)(Math.Sin(angle) * (pointX - originX) + Math.Cos(angle) * (pointY - originY) + originY));
        }

        public static Vector3 forwardVectorFromRotation(Rotation rotation, bool corAltVRot = false) {
            return new Vector3((float)(Math.Cos(rotation.Yaw + (corAltVRot ? 3.14f / 2 : 0)) * Math.Abs(Math.Cos(rotation.Pitch))), (float)(Math.Sin(rotation.Yaw + (corAltVRot ? 3.14f / 2 : 0)) * Math.Cos(rotation.Pitch)), (float)(Math.Sin(rotation.Pitch)));
        }

        public static Vector3 getPositionInFront(Vector3 position, Vector3 rotation, float distance, bool correctAltVHeading = false) {
            Vector3 fove = forwardVectorFromRotation(rotation, correctAltVHeading);
            Vector3 scal = new Vector3(fove.X * distance, fove.Y * distance, fove.Z * distance);

            return new Vector3(position.X + scal.X, position.Y + scal.Y, position.Z + scal.Z);
        }


        private static Random random = new Random();
        public static string randomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static long longRandom(long min, long max, Random rand) {
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }
        
        public static List<int> getRange(int start, int end) {
            List<int> range = new List<int>();
            for(int i = start; i <= end; i++) {
                range.Add(i);
            }
            return range;
        }

        #endregion

        #region Player

        /// <summary>
        /// Returns socialclubname of a given client. ChoiceV Extension for the NAPI.Player.GetSocialClubName method.
        /// </summary>
        /// <returns>return a string containing the socialClubname of the Player</returns>
        public static string GetName(IPlayer player) {
            if(player.Exists()) {
                return player.SocialClubName;
            } else {
                return "";
            }
        }

        /// <summary>
        /// Kicks a player with a specified reason. ChoiceV Extension for the NAPI.Player.KickPlayer method.
        /// </summary>
        public static void KickPlayer(IPlayer player, string kickCode, string description, string reason, ulong discordId = 0) {
            try {
                Logger.logWarning(LogCategory.Player, LogActionType.Event, player, $"Player with SocialClub {GetName(player)} has been kicked. The Reason was {reason}");

                if(player.hasData("DISCORD_ID") || discordId != 0) {
                    if(player.hasData("DISCORD_ID")) {
                        discordId = player.getData("DISCORD_ID");
                    }

                    DiscordController.sendEmbedToUser(discordId, $"Du wurdest gekickt! Code: {kickCode}", $"{description} Du kannst jederzeit in den Support gehen und nach dem Code fragen.");
                }

                if(player.getCharacterData() == null || player.getAdminLevel() <= 3) {                    
                    player.emitClientEvent("KICK_MESSAGE", reason);
                    player.Kick($"Code: {kickCode}, Beschreibung: {description} Du kannst jederzeit in den Support gehen und nach dem Code fragen.");
                }
            } catch(Exception) {
                if(player.Exists() && player.getAdminLevel() <= 3) {
                    player.Kick(reason);
                }
            }
        }

        /// <summary>
        /// Sets a clothing set for a player.
        /// </summary>
        public static void SetPlayerClothes(IPlayer player, int slot, int drawable, int texture, string dlcName = null) {
            if(!player.hasData(Constants.DATA_CLOTHING_CURRENT)) {
                player.setData(Constants.DATA_CLOTHING_CURRENT, new ClothingPlayer());
            }

            var currentClothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_CURRENT);
            currentClothing.UpdateClothSlot(slot, drawable, texture, dlcName);

            if(dlcName != null) {
                player.SetDlcClothes(Convert.ToByte(slot), Convert.ToUInt16(drawable), Convert.ToByte(texture), 0, ChoiceVAPI.Hash(dlcName));
            } else {
                player.SetClothes(Convert.ToByte(slot), Convert.ToUInt16(drawable), Convert.ToByte(texture), 0);
            }
            //player.emitClientEvent(Constants.PlayerSetClothes, slot, drawable, texture);
        }

        /// <summary>
        /// Sets a accessoire set for a player.
        /// </summary>
        public static void SetPlayerClothingProp(IPlayer player, int slot, int drawable, int texture, string dlcName = null) {
            if(!player.hasData(Constants.DATA_CLOTHING_CURRENT)) {
                player.setData(Constants.DATA_CLOTHING_CURRENT, new ClothingPlayer());
            }

            var currentClothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_CURRENT);
            currentClothing.UpdateAccessorySlot(slot, drawable, texture, dlcName);

            if(drawable == -1) {
                player.ClearProps(Convert.ToByte(slot));
            } else {
                if(dlcName != null) {
                    player.SetDlcProps(Convert.ToByte(slot), Convert.ToUInt16(drawable), Convert.ToByte(texture), ChoiceVAPI.Hash(dlcName));
                } else {
                    player.SetProps(Convert.ToByte(slot), Convert.ToUInt16(drawable), Convert.ToByte(texture));
                }
            }
        }

        public static ClothingComponent getPlayerCurrentClothes(IPlayer player, int slot) {
            var currentClothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_CURRENT);
            return currentClothing.GetSlot(slot, false);
        }

        public static ClothingComponent getPlayerCurrentAccessory(IPlayer player, int slot) {
            var currentClothing = (ClothingPlayer)player.getData(Constants.DATA_CLOTHING_CURRENT);
            return currentClothing.GetSlot(slot, true);
        }

        /// <summary>
        /// Returns List of all Clients. ChoiceV Extension for the NAPI.POOLS.GetAllPlayers method.
        /// </summary>
        public static List<IPlayer> GetAllPlayersIncludeNotFullyLoaded() {
            return Alt.GetAllPlayers().ToList();
        }

        public static List<IPlayer> GetAllPlayers(bool includeNotFullyLoaded = false) {
            return Alt.GetAllPlayers().Where(p => includeNotFullyLoaded || p.getCharacterFullyLoaded()).ToList();
        }

        public class FunctionCallback<T>: IBaseObjectCallback<T> where T : IBaseObject {
            private readonly Action<T> _callback;

            public FunctionCallback(Action<T> callback) {
                _callback = callback;
            }

            public void OnBaseObject(T baseObject) {
                _callback(baseObject);
            }
        }

        public static void ForAllPlayers(Action<IPlayer> action) {
            var callback = new FunctionCallback<IPlayer>(player => {
                action.Invoke(player);
            });
            Alt.ForEachPlayers(callback);
        }

        /// <summary>
        /// Returns List of all Clients. ChoiceV Extension for the NAPI.POOLS.GetAllPlayers method.
        /// </summary>
        public static List<IPlayer> GetAllPlayersOnIsland(Islands island) {
            return Alt.GetAllPlayers().Where(p => p.getCharacterFullyLoaded() && p.getIsland() == island).ToList();
        }

        /// <summary>
        /// Returns the client with the corresponding charId. Returns null if not found
        /// </summary>
        public static IPlayer FindPlayerByCharId(int charId, bool includeNotFullyLoaded = false) {
            return GetAllPlayers(includeNotFullyLoaded).FirstOrDefault(p => p.getCharacterId() == charId);
        }

        /// <summary>
        /// Returns the client with the corresponding accountId. Returns null if not found
        /// </summary>
        public static IPlayer FindPlayerByAccountId(int accountId) {
            return GetAllPlayers().FirstOrDefault(p => p.getAccountId() == accountId);
        }

        /// <summary>
        /// Returns the Vehicle with the corresponding id. Returns null if not found
        /// </summary>
        public static ChoiceVVehicle FindVehicleById(int vehId) {
            return Alt.GetAllVehicles().Cast<ChoiceVVehicle>().FirstOrDefault(v => v.VehicleId == vehId);
        }

        /// <summary>
        /// Returns Dictionary of all Clients.
        /// </summary>
        public static Dictionary<int, IPlayer> GetAllPlayerDictionary() {
            var arr = Alt.GetAllPlayers().ToArray();
            var dic = arr.ToDictionary(p => p.getCharacterId(), p => p);

            return dic;
        }

        /// <summary>
        /// Trigger a Client Event for all player.
        /// </summary>
        /// <param name="eventname">The Event that should be triggered</param>
        /// <param name="args">The Event event args sent to the player</param>
        public static void emitClientEventToAll(string eventname, params object[] args) {
            Alt.EmitAllClients(eventname, args);
        }

        /// <summary>
        /// Creates a Enumerable containing all numbers from a start to an end with a given increment
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<int> getIncValues(int start, int end, int increment) {
            for(int i = start; i <= end; i += increment)
                yield return i;
        }

        #endregion

        #region Notification

        /// <summary>
        /// Send a Notification to a client, observable over the minimap. ChoiceV Extension for the NAPI.Notification.SendNotificationToPlayer method.
        /// </summary>
        public static void SendBlockNotificationToPlayer(IPlayer player, string message, NotifactionImages image = NotifactionImages.System) {
            player.emitClientEvent("SHOW_NOTIFICATION", NotifactionTypes.Danger.ToString(), "Fehler", message, image.ToString());
        }

        #endregion

        #region Vehicle

        public static ChoiceVVehicle CreateVehicle(uint model, Position position, Rotation rotation) {
            return (ChoiceVVehicle)Alt.CreateVehicle(model, position, rotation);
        }

        /// <summary>
        /// Deletes an given Entity by handle. ChoiceV Extension for Alt.RemoveVehicle method
        /// </summary>
        public static void RemoveVehicle(ChoiceVVehicle vehicle) {
            vehicle.Destroy();
        }

        /// <summary>
        /// Sets door condition of a vehicle. ChoiceV Extension for NAPI.Vehicle.BreakVehicleDoor method
        /// </summary>
        /// <param name="vehicle">The handle of the vehicle</param>
        /// <param name="index">The index of the door</param>
        /// <param name="breakDoor">Break the door or not</param>
        public static void BreakVehicleDoor(ChoiceVVehicle vehicle, int door, bool breakDoor) {
            //ALTV Not Implemented Yet
        }

        /// <summary>
        /// Sets window condition of a vehicle. ChoiceV Extension for NAPI.Vehicle.BreakVehicleWindow method
        /// </summary>
        /// <param name="vehicle">The handle of the vehicle</param>
        /// <param name="index">The index of the window</param>
        /// <param name="breakWindow">Break the window or not</param>
        public static void BreakVehicleWindow(ChoiceVVehicle vehicle, int window, bool breakWindow) {
            //ALTV Not Implemented Yet
        }

        /// <summary>
        /// Sets tyre condition of a vehicle. ChoiceV Extension for NAPI.Vehicle.PopVehicleTyre method
        /// </summary>
        /// <param name="vehicle">The handle of the vehicle</param>
        /// <param name="tyre">The index of the tyre</param>
        /// <param name="pop">Pop the tyre or not</param>
        public static void PopVehicleTyre(ChoiceVVehicle vehicle, int tyre, bool pop) {
            //ALTV Not Implemented Yet
        }

        /// <summary>
        /// Gets the overhaul health of the Vehicle. ChoiceV Extension for NAPI.Vehicle.getVehicleHealth method
        /// </summary>
        /// <param name="vehicle">The handle of the vehicle</param>
        public static float GetVehicleHealth(ChoiceVVehicle vehicle) {
            //ALT V Not IMplkemetend Yet
            return 100.0f;
        }

        /// <summary>
        /// Gets the overhaul health of the Vehicle. ChoiceV Extension for NAPI.Vehicle.getVehicleHealth method
        /// </summary>
        /// <param name="vehicle">The handle of the vehicle</param>
        public static List<ChoiceVVehicle> GetAllVehicles() {
            return Alt.GetAllVehicles().Cast<ChoiceVVehicle>().ToList();
        }

        /// <summary>
        /// Gets the overhaul health of the Vehicle. ChoiceV Extension for NAPI.Vehicle.getVehicleHealth method
        /// </summary>
        /// <param name="vehicle">The handle of the vehicle</param>
        public static List<ChoiceVVehicle> GetVehicles(Func<ChoiceVVehicle, bool> predicate) {
            return Alt.GetAllVehicles().Cast<ChoiceVVehicle>().Where(predicate).ToList();
        }

        #endregion

        #region Util

        /// <summary>
        /// Logs a given text with given color in the API Console. ChoiceV Extension for NAPI.Util.Output method
        /// </summary>
        public static void Log(string text, ConsoleColor color) {
            Console.ForegroundColor = color;
            //Alt.Log(text);
            Console.WriteLine(text);
        }

        /// <summary>
        /// Gives the Hash of a string
        /// </summary>
        public static uint Hash(string text) {
            return Alt.Hash(text);
        }

        #endregion

        #region Chat

        /// <summary>
        /// Sends a chat message to a player.
        /// </summary>
        public static void SendChatMessageToPlayer(IPlayer player, string message) {
            player.emitClientEvent("chatmessage", null, message);
        }

        #endregion

        #region Weapons

        /// <summary>
        /// Gives all Weapons to the Player
        /// </summary>
        public static void giveAllWeaponsToPlayer(IPlayer player) {
            foreach(var weaponName in Constants.WeaponNames) {
                player.GiveWeapon(Alt.Hash(weaponName), 9999, false);
            }
        }

        /// <summary>
        /// Gives a specific Weapon to a player with given ammonation
        /// </summary>
        /// <param name="weapon">Name of the weapon eg. "WEAPON_POOLCUE"</param>
        /// <param name="ammo">Amount of Ammunation the playerr gets</param>
        public static void giveWeaponToPlayer(IPlayer player, string weapon, int ammo, bool instantEquip) {
            player.GiveWeapon(Alt.Hash(weapon), ammo, instantEquip);
            //player.emitClientEvent(PlayerGiveWeapon, weapon, ammo);
        }

        /// <summary>
        /// Removes a specific Weapon for a player
        /// </summary>
        /// <param name="weapon">Name of the weapon eg. "WEAPON_POOLCUE"</param>
        public static void removeWeaponFromPlayer(IPlayer player, string weapon) {
             player.SetWeaponAmmo(Alt.Hash(weapon), 0);
            player.RemoveWeapon(Alt.Hash(weapon));
            //player.emitClientEvent(PlayerRemoveWeapon, weapon);
        }

        /// <summary>
        /// Gives a specific WeaponComponent to a player with given weapon
        /// </summary>
        /// <param name="weapon">Name of the weapon eg. "WEAPON_POOLCUE"</param>
        /// <param name="component">The Component the player gets e.g "COMPONENT_PISTOL50_CLIP_01"</param>
        public static void giveWeaponComponentToPlayer(IPlayer player, string weapon, string component) {
            //player.emitClientEvent(PlayerGiveWeaponComponent, weapon, component);
            player.AddWeaponComponent(Alt.Hash(weapon), Alt.Hash(component));
        }

        /// <summary>
        /// Removes a specific WeaponComponent to a player with given weapon
        /// </summary>
        /// <param name="weapon">Name of the weapon eg. "WEAPON_POOLCUE"</param>
        /// <param name="component">The Component the player gets e.g "COMPONENT_PISTOL50_CLIP_01"</param>
        public static void removeWeaponComponentFromPlayer(IPlayer player, string weapon, string component) {
            //player.emitClientEvent(PlayerRemoveWeaponComponent, weapon, component);
            player.RemoveWeaponComponent(Alt.Hash(weapon), Alt.Hash(component));
        }

        /// <summary>
        /// Sets a specific WeaponComponent to a player with given weapon
        /// </summary>
        /// <param name="weapon">Name of the weapon eg. "WEAPON_POOLCUE"</param>
        public static void setPlayerAmmo(IPlayer player, string weapon, int ammo) {
            player.emitClientEvent(PlayerSetAmmo, weapon, ammo);
        }

        /// <summary>
        /// Sets an amount of Armor for a Player
        /// </summary>
        public static void setPlayerArmor(IPlayer player, ushort amount) {
            player.Armor = amount;
            //player.emitClientEvent(PlayerSetArmour, amount);
        }

        /// <summary>
        /// Creating a Blip at a given location
        /// </summary>
        public static void createPointBlip(IPlayer player, string description, Position position, int color, int spriteId, int alpha, bool flashes, bool shortRange, string uniqueId) {
            player.emitClientEvent(PlayerCreatePointBlip, description, position.X, position.Y, position.Z, color, spriteId, alpha, flashes, shortRange, uniqueId);
        }

        /// <summary>
        /// Creating a Blip at a given location
        /// </summary>
        public static void createRouteBlip(IPlayer player, string description, Position position, int color, int spriteId, int alpha, bool flashes, bool shortRange, int routeColor) {
            player.emitClientEvent(PlayerCreateRouteBlip, description, position.X, position.Y, position.Z, color, spriteId, alpha, flashes, shortRange, routeColor);
        }

        /// <summary>
        /// Creating a Area-Blip at a given location
        /// </summary>
        public static void createAreaBlip(IPlayer player, Position position, float width, float height, int color, int alpha) {
            player.emitClientEvent(PlayerCreateAreaBlip, position.X, position.Y, position.Z, width, height, color, alpha);
        }

        /// <summary>
        /// Creating a Radius-Blip at a given location
        /// </summary>
        public static void createRadiusBlip(IPlayer player, Position position, float radius, int color, int alpha, string uniqueId) {
            player.emitClientEvent(PlayerCreateRadiusBlip, position.X, position.Y, position.Z, radius, color, alpha, uniqueId);
        }

        /// <summary>
        /// Creating a Waypoint-Blip at a given location
        /// </summary>
        public static void createWaypointBlip(IPlayer player, float x, float y) {
            player.emitClientEvent(PlayerCreateWaypointBlip, x, y);
        }

        /// <summary>
        /// Creating a Waypoint-Blip at a given location
        /// </summary>
        public static void createWaypointBlip(IPlayer player, Position position) {
            player.emitClientEvent(PlayerCreateWaypointBlip, position.X, position.Y);
        }

        /// <summary>
        /// Creating a Blip at a given location
        /// </summary>
        public static void destroyBlip(IPlayer player, Position position) {
            player.emitClientEvent(PlayerDestroyBlip, position.X, position.Y, position.Z);
        }

        /// <summary>
        /// Creating a Blip at a given location
        /// </summary>
        public static void destroyBlipByName(IPlayer player, string id) {
            if(player.Exists()) {
                player.emitClientEvent(PlayerDestroyBlipName, id);
            }
        }

        /// <summary>
        /// Sets the damage multiplier for a weapon, by weapon_name
        /// </summary>
        public static void setPlayerWeaponDamageMult(IPlayer player, string weaponName, float mult) {
            player.emitClientEvent("SET_WEAPON_DAMAGE_MULT", weaponName, mult);
        }

        /// <summary>
        /// Sets the damage multiplier for a list of weapon, by weapon_name
        /// </summary>
        public static void setPlayerWeaponsDamageMult(IPlayer player, string[] weaponNames, float[] mults) {
            player.emitClientEvent("SET_WEAPONS_DAMAGE_MULT", weaponNames, mults);
        }

        #endregion

        #region World

        /// <summary>
        /// Sets a weather change over time for all players
        /// </summary>
        /// <param name="oldW">Old Weather Name</param>
        /// <param name="newW">New Weather Name</param>
        /// <param name="mix">The Mix percentage mix: newW, 1-mix: oldW</param>
        public static void setWeatherTransition(string oldW, string newW, float mix) {
            foreach(var player in GetAllPlayers()) {
                player.emitClientEvent(Constants.PlayerWeatherTransition, oldW, newW, 1);
                //player.emitClientEvent(Constants.PlayerWeatherTransition, oldW, newW, mix);
            }

            InvokeController.AddTimedInvoke("Weather-Transitioner", (i) => {
                foreach (var player in GetAllPlayers()) {
                    player.SetWeather(WeatherController.getWeatherIdByType(newW));
                }
            }, TimeSpan.FromSeconds(35), false);
        }

        /// <summary>
        /// Sets a weather mix for a specific player
        /// </summary>
        /// <param name="oldW">Old Weather Name</param>
        /// <param name="newW">New Weather Name</param>
        /// <param name="mix">The Mix percentage mix: newW, 1-mix: oldW</param>
        public static void setWeatherMixForPlayer(IPlayer player, string oldW, string newW, float mix) {
            player.SetWeather(WeatherController.getWeatherIdByType(newW));
            //player.emitClientEvent(Constants.PlayerWeatherMix, oldW, newW, mix);
        }


        public static void sendServerWideMessage(string title, string message) {
            foreach(var player in GetAllPlayers()) {
                player.emitClientEvent("SHOW_WASTED_SCREEN", $"~r~ {title}", message, false);
            }
        }

        #endregion
    }
}
