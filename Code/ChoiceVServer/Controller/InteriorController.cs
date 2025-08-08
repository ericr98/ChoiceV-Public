using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.Interior;
using static ChoiceVServer.Controller.Interior.InteriorEntitySet;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    public abstract class MapObject {
        public int Id;
        public string Identifier;
        public string DisplayName;
        public Position Position;

        public Islands Island;

        public Blip Blip;

        public int BlipType;
        public int BlipColor;

        public MapObject(int id, string identifier, string displayName, Position position, Islands island, int blipType, int blipColor) {
            Id = id;
            Identifier = identifier;
            DisplayName = displayName;
            Position = position;
            Island = island;
            BlipType = blipType;
            BlipColor = blipColor;
        }
    }

    public class Interior : MapObject {
        public class InteriorEntitySetSpot {
            public Interior Main;
            private string DisplayName;
            private CollisionShape InteriorInteractSpot;
            public List<InteriorEntitySet> Sets { get; private set; }

            public InteriorEntitySetSpot(Interior main, string displayName, CollisionShape interactSpot) {
                Main = main;
                DisplayName = displayName;
                Sets = new();

                InteriorInteractSpot = interactSpot;

                InteriorInteractSpot.OnCollisionShapeInteraction += onInteriorInteract;
            }

            private bool onInteriorInteract(IPlayer player) {
                var menu = new Menu($"{DisplayName}-Interaktion", "Wähle eine Option?");

                foreach(var set in Sets) {
                    var list = set.Options.Select(o => o.DisplayName).ToList().ShiftLeft(set.Options.IndexOf(set.CurrentOption)).ToArray();
                    menu.addMenuItem(new ListMenuItem(set.DisplayName, "Ändere die Einrichtung", list, "ON_INTERIOR_CHANGE_ENTITY_SET", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "EntitySet", set } }).needsConfirmation("Dieses Set aktivieren?", "Set wirklich aktivieren?"));
                }
                player.showMenu(menu);

                return true;
            }

            public void addEntitySpot(InteriorEntitySet set) {
                Sets.Add(set);
            }

            public InteriorEntitySet getSetWithName(string name) {
                return Sets.FirstOrDefault(s => s.DisplayName == name);
            }
        }

        public class InteriorEntitySet {
            public record InteriorEntitySetOption(int DbId, string DisplayName, string GtaName);

            public InteriorEntitySetSpot Spot;
            public string DisplayName;
            public List<InteriorEntitySetOption> Options;
            public InteriorEntitySetOption CurrentOption;

            public InteriorEntitySet(string displayName, InteriorEntitySetSpot spot) {
                DisplayName = displayName;
                Options = new List<InteriorEntitySetOption>();
                Spot = spot;
            }

            public void addEntitySetOption(InteriorEntitySetOption option, bool isLoaded) {
                Options.Add(option);
                if(isLoaded) {
                    CurrentOption = option;
                }
            }
        }

        public string GtaName { get; private set; }
        public List<InteriorEntitySetSpot> EntitySetSpotList { get; private set; }

        public Interior(int id, string identifier, string gtaName, string displayName, Position position, Islands island, int blipType, int blipColor) : base(id, identifier, displayName, position, island, blipType, blipColor) {
            GtaName = gtaName;

            EntitySetSpotList = new List<InteriorEntitySetSpot>();

            if(blipType != -1) {
                Blip = BlipController.createPointBlipForAll(displayName, position, blipColor, blipType, 255, island);
            }
        }

        public void addEntitySet(InteriorEntitySetSpot set) {
            EntitySetSpotList.Add(set);
        }
    }

    public class YmapIPL : MapObject {
        public bool IsLoaded;
        public bool IsStandardLoadedIn;

        public string IplName;

        public YmapIPL(int id, string identifier, string displayName, string iplName, bool isLoaded, bool isStandardLoadedIn, Position position, Islands island, int blipType, int blipColor) : base(id, identifier, displayName, position, island, blipType, blipColor) {
            IplName = iplName;
            IsLoaded = isLoaded;
            IsStandardLoadedIn = isStandardLoadedIn;

            if(IsLoaded && blipType != -1) {
                Blip = BlipController.createPointBlipForAll(displayName, position, blipColor, blipType, 255, island);
            }
        }

        public void setIplLoaded(bool isLoaded) {
            IsLoaded = isLoaded;

            InteriorController.toogleIpl(IplName, isLoaded);

            if(IsLoaded) {
                if(Blip == null) {
                    Blip = BlipController.createPointBlipForAll(DisplayName, new Position(Position.X, Position.Y, 0f), BlipColor, BlipType, 255, Island);
                }
            } else {
                if(Blip != null) {
                    BlipController.destroyBlipForAll(Blip);
                    Blip = null;
                }
            }
        }
    }

    public class ClientIpls {
        public string name;
        public Vector2 pos;
        public bool shown;

        [JsonIgnore]
        public bool standardLoadedIn;

        public ClientIpls(string name, Vector2 pos, bool shown, bool standardLoadedIn) {
            this.name = name;
            this.pos = pos;
            this.shown = shown;
            this.standardLoadedIn = standardLoadedIn;
        }
    }

    public class InteriorController : ChoiceVScript {
        private static List<MapObject> AllMapObjects;
        internal static Dictionary<string, ClientIpls> ClientIpls;

        public InteriorController() {
            AllMapObjects = new List<MapObject>();
            ClientIpls = new Dictionary<string, ClientIpls>();

            //return;
            using(var db = new ChoiceVDb()) {
                foreach(var dbIpl in db.configipls
                                          .Include(i => i.configinteriorentitysetspots)
                                            .ThenInclude(es => es.configinteriorentitysets)) {

                    if(dbIpl.iplType == "INTERIOR") {
                        var interior = new Interior(dbIpl.id, dbIpl.identifier, dbIpl.gtaName, dbIpl.displayName, dbIpl.position.FromJson(), (Islands)dbIpl.island, dbIpl.blipType ?? -1, dbIpl.blipColor ?? -1);
                        AllMapObjects.Add(interior);

                        ClientIpls.Add(interior.GtaName, new ClientIpls(interior.GtaName, new Vector2(interior.Position.X, interior.Position.Y), true, true));

                        foreach(var dbSpot in dbIpl.configinteriorentitysetspots) {
                            CollisionShape shape = null;
                            if(dbSpot.interactSpotString != null) {
                                shape = CollisionShape.Create(dbSpot.interactSpotString);
                            }

                            var spot = new InteriorEntitySetSpot(interior, dbSpot.displayName, shape);
                            interior.addEntitySet(spot);
                            foreach(var dbSetOption in dbSpot.configinteriorentitysets) {
                                var already = spot.getSetWithName(dbSetOption.setName);
                                if(already != null) {
                                    already.addEntitySetOption(new InteriorEntitySetOption(dbSetOption.id, dbSetOption.displayName, dbSetOption.gtaName), dbSetOption.isLoaded == 1);
                                } else {
                                    var newSet = new InteriorEntitySet(dbSetOption.setName, spot);
                                    newSet.addEntitySetOption(new InteriorEntitySetOption(dbSetOption.id, dbSetOption.displayName, dbSetOption.gtaName), dbSetOption.isLoaded == 1);
                                    spot.addEntitySpot(newSet);
                                }
                            }
                        }
                    } else {
                        var ymap = new YmapIPL(dbIpl.id, dbIpl.identifier, dbIpl.displayName, dbIpl.gtaName, dbIpl.isLoaded == 1, dbIpl.standardLoadedIn == 1, dbIpl.position.FromJson(), (Islands)dbIpl.island, dbIpl.blipType ?? -1, dbIpl.blipColor ?? -1);
                        AllMapObjects.Add(ymap);
                        ClientIpls.Add(ymap.IplName, new ClientIpls(ymap.IplName, new Vector2(ymap.Position.X, ymap.Position.Y), ymap.IsLoaded, ymap.IsStandardLoadedIn));
                    }
                }
            }

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            EventController.addMenuEvent("ON_INTERIOR_CHANGE_ENTITY_SET", onInteriorChangeEntitySet);

            //loadMilos();
        }

        private bool onInteriorChangeEntitySet(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;

            var entitySet = (InteriorEntitySet)data["EntitySet"];
            var interior = entitySet.Spot.Main;
            var selectedOption = entitySet.Options.FirstOrDefault(i => i.DisplayName == evt.currentElement);

            using(var db = new ChoiceVDb()) {
                var dbCurrent = db.configinteriorentitysets.Find(entitySet.CurrentOption.DbId);
                var dbNew = db.configinteriorentitysets.Find(selectedOption.DbId);

                dbCurrent.isLoaded = 0;
                dbNew.isLoaded = 1;
            }

            entitySet.CurrentOption = selectedOption;

            player.sendNotification(NotifactionTypes.Info, $"Du hast die Option: {evt.currentElement} für {entitySet.DisplayName} aktiviert!", $"{evt.currentElement} aktiviert", NotifactionImages.Shop);

            foreach(var target in ChoiceVAPI.GetAllPlayers()) {
                foreach(var option in entitySet.Options) {
                    if(option.GtaName != "NONE" && option.GtaName != "") {
                        target.emitClientEvent("SET_ENTITY_SET_STATE", interior.Position.X, interior.Position.Y, interior.Position.Z, interior.GtaName, option.GtaName, entitySet.CurrentOption == option);
                    }
                }
            }

            return true;
        }

        private void onPlayerConnect(IPlayer player, character character) {
            player.emitClientEvent("SET_IPLS", ClientIpls.Values.ToList().ToJson());

            foreach(var mapObject in AllMapObjects) {
                if(mapObject is Interior interior) {
                    foreach(var spot in interior.EntitySetSpotList) {
                        foreach(var set in spot.Sets) {
                            foreach(var option in set.Options) {
                                player.emitClientEvent("SET_ENTITY_SET_STATE", interior.Position.X, interior.Position.Y, interior.Position.Z, interior.GtaName, option.GtaName, set.CurrentOption == option);
                            }
                        }
                    }
                }
            }
        }

        internal static void toogleIpl(string iplName, bool show) {
            var clIpl = ClientIpls[iplName];
            clIpl.shown = show;

            ChoiceVAPI.emitClientEventToAll("UPDATE_IPL", clIpl.ToJson());
        }

        public static void toogleIplByIdentifier(string identifier, bool show) {
            var ipl = AllMapObjects.FirstOrDefault(i => i is YmapIPL && i.Identifier == identifier);
            if(ipl != null) {
                var ymap = (ipl as YmapIPL);
                if(ymap.IsLoaded != show) {
                    ymap.setIplLoaded(show);

                    var clIpl = ClientIpls[ymap.IplName];
                    clIpl.shown = show;

                    ChoiceVAPI.emitClientEventToAll("UPDATE_IPL", clIpl.ToJson());
                }
            }
        }

        public static T getMapObjectByIdentifer<T>(string identifier) {
            var mapObject = AllMapObjects.FirstOrDefault(i => i.GetType() == typeof(T) && i.Identifier == identifier);

            T returnItem;
            TypeMethods.TryCast<T>(mapObject, out returnItem);

            return returnItem;
        }
    }
}
