using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {
    public class Blip {
        public int Id;
        public string Description;
        public Position Position;
        public int SpriteId;
        public int Color;
        public int Alpha;
        public Islands Island;

        public Blip(int id, string description, Position position, int spriteId, int color, int alpha, Islands island = Islands.SanAndreas) {
            Id = id;
            Description = description;
            Position = position;
            SpriteId = spriteId;
            Color = color;
            Alpha = alpha;
            Island = island;
        }
    }

    public class BlipController : ChoiceVScript {
        private static List<Blip> AllBlips = new List<Blip>();

        public static int BlipId = 0;
        private static int TemporaryBlipId = 0;

        public BlipController() {
            loadBlips();

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            IslandController.PlayerIslandChangeDelegate += onPlayerChangeIsland;
        }

        private void onPlayerChangeIsland(IPlayer player, Islands previousIsland, Islands newIsland) {
            foreach(var blip in AllBlips.Where(b => b.Island == previousIsland)) {
                destroyBlipByName(player, blip.Id.ToString());
            }

            foreach(var blip in AllBlips.Where(b => b.Island == newIsland)) {
                createPointBlip(player, blip.Description, blip.Position, blip.Color, blip.SpriteId, blip.Alpha, blip.Id.ToString());
            }
        }

        private static void loadBlips() {
            using(var db = new ChoiceVDb()) {
                var blips = db.configblips;

                foreach(var dbBlip in blips) {
                    var blip = new Blip(dbBlip.id, dbBlip.name, dbBlip.position.FromJson(), dbBlip.sprite, dbBlip.colorId, 255, (Islands)dbBlip.island);

                    if(dbBlip.id > BlipId) {
                        BlipId = dbBlip.id + 1;
                    }

                    AllBlips.Add(blip);
                }
            }
        }

        private void onPlayerConnect(IPlayer player, character character) {
            var island = player.getIsland();
            foreach(var blip in AllBlips.Where(b => b.Island == island).ToList()) {
                createPointBlip(player, blip.Description, blip.Position, blip.Color, blip.SpriteId,
                    blip.Alpha, false, true);
            }
        }

        public static void createPointBlipDb(string description, Position position, int color, int spriteId, int alpha, Islands island) {
            using(var db = new ChoiceVDb()) {
                var dbBlip = new configblip {
                    name = description,
                    position = position.ToJson(),
                    colorId = color,
                    sprite = spriteId,
                    island = (int)island,
                };

                db.configblips.Add(dbBlip);
                db.SaveChanges();

                var blip = new Blip(dbBlip.id, dbBlip.name, dbBlip.position.FromJson(), dbBlip.sprite, dbBlip.colorId, 255);
                AllBlips.Add(blip);

                foreach(var other in ChoiceVAPI.GetAllPlayersOnIsland(blip.Island)) {
                    createPointBlip(other, description, position, color, spriteId, alpha, blip.Id.ToString());
                }
            }
        }

        public static Blip createPointBlipForAll(string description, Position position, int color, int spriteId, int alpha, Islands island) {
            var blip = new Blip(BlipId++, description, position, spriteId, color, alpha, island);
            AllBlips.Add(blip);

            foreach(var other in ChoiceVAPI.GetAllPlayersOnIsland(blip.Island)) {
                createPointBlip(other, description, position, color, spriteId, alpha, blip.Id.ToString());
            }

            return blip;
        }

        public static string createPointBlip(IPlayer player, string description, Position position, int color, int spriteId, int alpha, bool flashes = false, bool shortRange = false, string uniqueId = null) {
            if(uniqueId == null) {
                uniqueId = (BlipId++).ToString();
            }

            ChoiceVAPI.createPointBlip(player, description, position, color, spriteId, alpha, flashes, shortRange, uniqueId);

            return uniqueId;
        }

        public static void createPointBlip(IPlayer player, string description, Position position, int color, int spriteId, int alpha, string uniqueId) {
            ChoiceVAPI.createPointBlip(player, description, position, color, spriteId, alpha, false, false, uniqueId);
        }

        public static void createTemporaryPointBlip(IPlayer player, string description, Position position, int color, int spriteId, int alpha, TimeSpan duration) { 
            var id = $"TEMPORARY-{TemporaryBlipId++}";
            createPointBlip(player, description, position, color, spriteId, alpha, id);

            InvokeController.AddTimedInvoke($"{player.getCharacterId()}-{id}", (i) => {
                destroyBlipByName(player, id);
            }, duration, false);
        }

        public static void createRouteBlip(IPlayer player, string description, Position position, int blipColor, int spriteId, int routeColor, int alpha, bool flashes = false, bool shortRange = false) {
            ChoiceVAPI.createRouteBlip(player, description, position, blipColor, spriteId, alpha, flashes, shortRange, routeColor);
        }

        public static void createAreaBlip(IPlayer player, Position position, float width, float height, int color, int alpha) {
            ChoiceVAPI.createAreaBlip(player, position, width, height, color, alpha);
        }

        public static void setWaypoint(IPlayer player, float x, float y) {
            player.emitClientEvent("SET_WAYPOINT", x, y);
        }

        public static string createRadiusBlip(IPlayer player, Position position, float radius, int color, int alpha, string uniqueId = null) {
            if(uniqueId == null) {
                uniqueId = (BlipId++).ToString();
            }

            ChoiceVAPI.createRadiusBlip(player, position, radius, color, alpha, uniqueId);

            return uniqueId;
        }

        public static void createWaypointBlip(IPlayer player, float x, float y) {
            ChoiceVAPI.createWaypointBlip(player, x, y);
        }

        public static void createWaypointBlip(IPlayer player, Position position) {
            ChoiceVAPI.createWaypointBlip(player, position.X, position.Y);
        }

        public static void destroyBlip(IPlayer player, Position position) {
            ChoiceVAPI.destroyBlip(player, position);
        }

        public static void destroyBlipByName(IPlayer player, string id) {
            ChoiceVAPI.destroyBlipByName(player, id);
        }

        public static void destroyBlipForAll(Blip blip) {
            foreach(var player in ChoiceVAPI.GetAllPlayersOnIsland(blip.Island)) {
                ChoiceVAPI.destroyBlipByName(player, blip.Id.ToString());
            }
        }
    }
}