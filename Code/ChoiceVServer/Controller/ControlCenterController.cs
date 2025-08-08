using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public enum DispatchType {
        NpcSilentDispatch = 0,
        NpcCallDispatch = 1,
        AutomatedDispatch = 2,
    }

    public enum TransmitterColor {
        White,
        Blue,
        Beige,
        Red,
        Green,
        Violet,
        Yellow
    }

    public class ControlCenterController : ChoiceVScript {
        public static Dictionary<TransmitterColor, string> TransmittorColorToString = new Dictionary<TransmitterColor, string> {
            { TransmitterColor.Blue, "b" },
            { TransmitterColor.Beige, "be" },
            { TransmitterColor.Red, "r" },
            { TransmitterColor.White, "w" },
            { TransmitterColor.Green, "g" },
            { TransmitterColor.Violet, "v" },
            { TransmitterColor.Yellow, "y" },
        };

        public class MapMarkerTransmitter {
            public int ItemId;
            public string DisplayName;
            public IEntity Entity;
            public TransmitterColor Color;

            public MapMarkerTransmitter(int itemId, string displayName, IEntity entity, TransmitterColor color) {
                ItemId = itemId;
                DisplayName = displayName;
                Entity = entity;
                Color = color;
            }
        }

        //Mobile Leitstelle

        //Dispatches nur in Collshape!
        //Streifenzuordnung auch nur in Colshape!
        //Einheit kann sich blockiert setzen
        //Bug mit multiple refresh
        //Handy auch Anrufe an ZweitSim anzeigen!


        //Funken im SCP möglich!

        //Dispatch von Handy auch an Leitstelle schicken

        private record Marker(int id, int type, string name, Vector2 pos, string iconName);

        public CollisionShape ControlCenter;
        private static List<MapMarkerTransmitter> AllMapMarkerTransmitters = new List<MapMarkerTransmitter>();

        private class ControlCenterRecipient {
            public IPlayer Player;
            public DateTime LastHeartBeat;

            public ControlCenterRecipient(IPlayer player) {
                Player = player;
                LastHeartBeat = DateTime.Now;
            }
        }

        private static Dictionary<int, ControlCenterRecipient> ControlCenterRecipients = new();
        private static int RECIPIENT_HEARTHBEAT_CHECK_COUNTER = 30;

        public ControlCenterController() {
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            EventController.addEvent("REQUEST_UPDATE_CONTROL_MAP", onRequestControlMap);
            EventController.addEvent("SENT_DISPATCH_TO_PATROL", onSentDispatchToPatrol);

            InvokeController.AddTimedInvoke("UpdateControlCenter", updateControlCenter, TimeSpan.FromSeconds(1), true);
            InvokeController.AddTimedInvoke("CreateRandomDispatch", createRandomDispatch, TimeSpan.FromMinutes(3), true);

            //Hardcoding this is fucking ugly, but dont want to manage another config entry or something
            ControlCenter = CollisionShape.Create(new Position(311.86813f, -292.57584f, 60.14868f), 21.326105f, 22.804327f, 68.00629f, true, false, false);

            ControlCenter.Owner = "Control-Center";
            ControlCenter.OnEntityEnterShape += onEnterShape;
            ControlCenter.OnEntityExitShape += onExitShape;

            EventController.PlayerEnterVehicleDelegate += onPlayerEnterVehicle;
            EventController.PlayerExitVehicleDelegate += onPlayerExitVehicle;

            EventController.addMenuEvent("CONTROL_CENTER_TRANSMITTER_TOOGLE", onControlCenterTransmitterToggle);
            EventController.addMenuEvent("CONTROL_CENTER_TRANSMITTER_SET_DATA", onControlCenterTransmitterSetData);

            VehicleController.addSelfMenuElement(
                new ConditionalVehicleGeneratedSelfMenuElement(
                    "Patrolliendaten setzen",
                    patrolDataGenerator,
                    v => v.NumberplateText != "",
                    p => CompanyController.findCompanies(c => (c.CompanyType == CompanyType.Police || c.CompanyType == CompanyType.Sheriff || c.CompanyType == CompanyType.Fire) && c.findEmployee(p.getCharacterId()) != null).Count > 0
                )
            );
            EventController.addMenuEvent("CONTROL_CENTER_SAVE_PATROL_DATA", onControlCenterSavePatrolData);

            if(Config.IsStressTestActive) {
                EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnected;
            }
        }

        private void onPlayerConnected(IPlayer player, character character) {
            if(Config.IsStressTestActive) {
                player.setPermanentData("HAS_CONTROL_CENTER_ACCESS", "true");
            }
        }

        private Menu patrolDataGenerator(ChoiceVVehicle vehicle, IPlayer player) {
            var menu = new Menu("Patrolliendaten setzen", "Was möchtest du tun?", false);

            using(var fsDb = new ChoiceVFsDb()) {
                var patrol = fsDb.executive_patrols.FirstOrDefault(p => p.numberPlate == vehicle.NumberplateText);
                var statuses = fsDb.executive_control_center_statuses.Select(s => s.name).ToList();

                if(patrol != null) {
                    var pos = statuses.FindIndex(a => a == patrol.status);
                    if(pos == -1) {
                        pos = 0;
                    }
                    statuses = statuses.ShiftLeft(pos);

                    menu.addMenuItem(new ListMenuItem("Status ändern", "Ändere den Status deiner Patrollie selbständig", statuses.ToArray(), ""));
                    var split = patrol.info.Split("\n");
                    menu.addMenuItem(new InputMenuItem("Info: Erste Zeile", "Die erste Zeile der Infoanzeige im Leitstellenblatt", "", "").withStartValue(split[0]));
                    var second = new InputMenuItem("Info: Zweite Zeile", "Die zweite Zeile der Infoanzeige im Leitstellenblatt", "", "");
                    if(split.Length > 1) {
                        second.withStartValue(split[1]);
                    }
                    menu.addMenuItem(second);
                    menu.addMenuItem(new MenuStatsMenuItem("Angaben speichern", "Speichere die eingegeben Daten. Sie sind danach für die Leitstelle sichtbar!", "CONTROL_CENTER_SAVE_PATROL_DATA", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Patrol", patrol } }));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Keine Patrollie mit passendem Kennzeichen!", "Es gibt keine Patrollie mit passendem Kennzeichen", "", MenuItemStyle.yellow));
                }
            }

            return menu;
        }

        private bool onControlCenterSavePatrolData(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var patrol = (executive_patrol)data["Patrol"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var statusEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var firstLineEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var secondLineEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            var input = "";
            if(firstLineEvt.input != null) {
                input += firstLineEvt.input;
            }

            if(secondLineEvt.input != null) {
                input += $"\n{secondLineEvt.input}";
            }

            using(var fsDb = new ChoiceVFsDb()) {
                var dbPatrol = fsDb.executive_patrols.Find(patrol.id, patrol.tab);

                dbPatrol.status = statusEvt.currentElement;
                dbPatrol.info = input;

                fsDb.SaveChanges();

                player.sendNotification(Constants.NotifactionTypes.Info, "Patrolliendaten erfolgreich geändert!", "Patrolliendaten angepasst", Constants.NotifactionImages.Police);
            }

            return true;
        }

        private void onPlayerEnterVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if(VehicleController.hasVehicleSpecialFlag(vehicle, SpecialVehicleModelFlag.IsControlCenterVehicle)) {
                player.setPermanentData("HAS_CONTROL_CENTER_ACCESS", "true");
            }
        }

        private void onPlayerExitVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if(VehicleController.hasVehicleSpecialFlag(vehicle, SpecialVehicleModelFlag.IsControlCenterVehicle)) {
                player.resetPermantData("HAS_CONTROL_CENTER_ACCESS");
            }
        }

        private bool onControlCenterTransmitterToggle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (ControlCenterTransmitter)data["Item"];

            if(item.Data.hasKey("Activated") && item.Activated) {
                item.Activated = false;

                removeMapMarkerTransmittor(m => m.ItemId == item.Id);
                player.sendNotification(Constants.NotifactionTypes.Info, "Der Transmitter wurde deaktiviert!", "Tranmitter deaktivert", Constants.NotifactionImages.System);
            } else {
                item.Activated = true;
                addMapMarkerTransmittor(item.Id ?? -1, item.DisplayName, player, item.Color);

                player.sendNotification(Constants.NotifactionTypes.Info, "Der Transmitter wurde aktiviert!", "Tranmitter aktivert", Constants.NotifactionImages.System);
            }

            return true;
        }

        private bool onControlCenterTransmitterSetData(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (ControlCenterTransmitter)data["Item"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var colorEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
            var color = Enum.GetValues<TransmitterColor>().FirstOrDefault(c => ControlCenterController.getNameForTransmitterColor(c) == colorEvt.currentElement);

            item.DisplayName = nameEvt.input.ToString();
            item.Color = color;

            var already = AllMapMarkerTransmitters.FirstOrDefault(m => m.ItemId == item.Id);

            if(already != null) {
                AllMapMarkerTransmitters.Remove(already);
                AllMapMarkerTransmitters.Add(new MapMarkerTransmitter(already.ItemId, item.DisplayName, already.Entity, item.Color));
            }

            item.Description = $"Transmitter: {item.DisplayName} - {getNameForTransmitterColor(item.Color)}";
            item.updateDescription();

            player.sendNotification(Constants.NotifactionTypes.Info, $"Die Position des Transmitters wird nun als {item.DisplayName} in {colorEvt.currentElement} angezeigt!", "Tranmitter-Daten gesetzt", Constants.NotifactionImages.System);

            return true;
        }

        public static MapMarkerTransmitter addMapMarkerTransmittor(int itemId, string displayName, IEntity entity, TransmitterColor color) {
            var already = AllMapMarkerTransmitters.FirstOrDefault(m => m.ItemId == itemId);
            if(already == null) {
                var element = new MapMarkerTransmitter(itemId, displayName, entity, color);
                AllMapMarkerTransmitters.Add(element);

                return element;
            } else {
                return already;
            }
        }

        public static void removeMapMarkerTransmittor(Predicate<MapMarkerTransmitter> predicate) {
            var element = AllMapMarkerTransmitters.FirstOrDefault(t => predicate(t));
            if(element != null) {
                AllMapMarkerTransmitters.Remove(element);
            }
        }

        public static bool hasMapMarkerBeenRegistered(Predicate<MapMarkerTransmitter> predicate) {
            return AllMapMarkerTransmitters.Any(m => predicate(m));
        }

        public static string getNameForTransmitterColor(TransmitterColor color) {
            switch(color) {
                case TransmitterColor.Blue:
                    return "Blau";
                case TransmitterColor.Beige:
                    return "Beige";
                case TransmitterColor.Red:
                    return "Rot";
                case TransmitterColor.White:
                    return "Weiß";
                case TransmitterColor.Green:
                    return "Grün";
                case TransmitterColor.Violet:
                    return "Violett";
                case TransmitterColor.Yellow:
                    return "Gelb";
                default:
                    return "Kein Name!";
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            ControlCenterRecipients.Remove(player.getCharacterId());

            if(player.hasData("HAS_CONTROL_CENTER_ACCESS")) {
                player.resetPermantData("HAS_CONTROL_CENTER_ACCESS");
            }
        }

        private void onEnterShape(CollisionShape shape, IEntity entity) {
            if(entity is IPlayer player) {
                player.setPermanentData("HAS_CONTROL_CENTER_ACCESS", "true");
            }
        }

        private void onExitShape(CollisionShape shape, IEntity entity) {
            if(entity is IPlayer player) {
                player.resetPermantData("HAS_CONTROL_CENTER_ACCESS");
            }
        }

        private static bool hasPlayerAccessToControlCenter(IPlayer player) {
            return player.hasData("HAS_CONTROL_CENTER_ACCESS") || player.getCharacterData().AdminMode || Config.IsStressTestActive;
        }

        public static void createDispatch(DispatchType type, string title, string info, Position position, bool realDispatch = true, bool doNotUseForFakes = false) {
            using(var db = new ChoiceVDb()) {
                var newDispatch = new dispatch {
                    type = getDispatchTypeName(type),
                    title = title,
                    info = info,
                    positionX = position.X.ToString(),
                    positionY = position.Y.ToString(),
                    createDate = DateTime.Now,
                    editType = 0,
                    isReal = realDispatch ? 1 : 0,
                    serverCreated = 1,
                    doNotUseForFakes = doNotUseForFakes ? 1 : 0
                };

                db.dispatches.Add(newDispatch);
                db.SaveChanges();
            }

            foreach(var recipient in ControlCenterRecipients.Values) {
                SoundController.playSound(recipient.Player, SoundController.Sounds.Dispatch, 1, "mp3");
            }
        }

        private static string getDispatchTypeName(DispatchType type) {
            switch(type) {
                case DispatchType.NpcSilentDispatch:
                    return "Stiller Alarm (Person)";
                case DispatchType.NpcCallDispatch:
                    return "Alarm (Person)";
                case DispatchType.AutomatedDispatch:
                    return "Stiller Alarm (Maschine)";
                default:
                    return "Fehler!";
            }
        }

        public static DispatchType getDispatchTypeFromName(string name) {
            switch(name) {
                case "Stiller Alarm (Person)":
                    return DispatchType.NpcSilentDispatch;
                case "Alarm (Person)":
                    return DispatchType.NpcCallDispatch;
                case "Stiller Alarm (Maschine)":
                    return DispatchType.AutomatedDispatch;
                default:
                    return DispatchType.AutomatedDispatch;
            }
        }

        private bool onRequestControlMap(IPlayer player, string eventName, object[] args) {
            lock(ControlCenterRecipients) {
                if(hasPlayerAccessToControlCenter(player)) {
                    var charId = player.getCharacterId();
                    if(ControlCenterRecipients.ContainsKey(charId)) {
                        ControlCenterRecipients[charId].LastHeartBeat = DateTime.Now;
                    } else {
                        ControlCenterRecipients.Add(charId, new ControlCenterRecipient(player));
                    }
                }

                return true;
            }
        }

        private static int HeartBeatCounter = 0;
        private void updateControlCenter(IInvoke invoke) {
            HeartBeatCounter++;
            if(HeartBeatCounter >= RECIPIENT_HEARTHBEAT_CHECK_COUNTER) {
                HeartBeatCounter = 0;

                var removeList = new List<int>();
                foreach(var recipient in ControlCenterRecipients) {
                    if(recipient.Value.LastHeartBeat + TimeSpan.FromSeconds(5) < DateTime.Now) {
                        removeList.Add(recipient.Key);
                    }
                }

                foreach(var remove in removeList) {
                    ControlCenterRecipients.Remove(remove);
                }
            }

            if(ControlCenterRecipients.Count > 0) {
                var playerDict = new Dictionary<int, string>();
                var vehicleDict = new Dictionary<int, string>();

                foreach(var transmittor in AllMapMarkerTransmitters) {
                    switch(transmittor.Entity.Type) {
                        case BaseObjectType.Player:
                            var pl = (IPlayer)transmittor.Entity;
                            if(!pl.IsInVehicle || !vehicleDict.ContainsKey(((ChoiceVVehicle)pl.Vehicle).VehicleId)) {
                                if(!playerDict.ContainsKey(pl.getCharacterId())) {
                                    playerDict.Add(pl.getCharacterId(), new Marker(transmittor.ItemId, 1, transmittor.DisplayName, new Vector2(transmittor.Entity.Position.X, transmittor.Entity.Position.Y), TransmittorColorToString[transmittor.Color]).ToJson());
                                }
                            }
                            break;
                        case BaseObjectType.Vehicle:
                            var vehicle = (ChoiceVVehicle)transmittor.Entity;
                            if(!vehicleDict.ContainsKey(vehicle.VehicleId)) {
                                if(vehicle.DbModel.classId != 15) {
                                    vehicleDict.Add(vehicle.VehicleId, new Marker(transmittor.ItemId, 2, transmittor.DisplayName, new Vector2(transmittor.Entity.Position.X, transmittor.Entity.Position.Y), TransmittorColorToString[transmittor.Color]).ToJson());
                                } else {
                                    vehicleDict.Add(vehicle.VehicleId, new Marker(transmittor.ItemId, 3, transmittor.DisplayName, new Vector2(transmittor.Entity.Position.X, transmittor.Entity.Position.Y), TransmittorColorToString[transmittor.Color]).ToJson());
                                }
                            }

                            foreach(var passenger in vehicle.PassengerList.Values) {
                                playerDict.Remove(passenger.getCharacterId());
                            }
                            break;
                    }
                }

                var finalList = playerDict.Values.Concat(vehicleDict.Values).ToList();

                foreach(var recipient in ControlCenterRecipients.Values) {
                    recipient.Player.emitClientEvent("ANSWER_UPDATE_CONTROL_MAP", finalList);
                }
            }
        }

        private static Random Random = new Random();
        private void createRandomDispatch(IInvoke invoke) {
            if(Random.NextDouble() > 0.1) {
                return;
            }

            if(!ControlCenterRecipients.Any()) {
                return;
            }

            using(var db = new ChoiceVDb()) {
                if(db.dispatches.Any()) {
                    var dispatches = db.dispatches.Where(d => d.serverCreated == 1 && d.editType == 3 && d.doNotUseForFakes == 0).ToList();
                    if(dispatches.Count > 0) {
                        var randomOld = dispatches[new Random().Next(dispatches.Count)];

                        createDispatch(getDispatchTypeFromName(randomOld.type), randomOld.title, randomOld.info, new Position(float.Parse(randomOld.positionX), float.Parse(randomOld.positionY), 0), false, true);
                    }
                }
            }
        }

        private bool onSentDispatchToPatrol(IPlayer player, string eventName, object[] args) {
            if(!hasPlayerAccessToControlCenter(player)) { return false; }

            var dispatchId = int.Parse(args[0].ToString());
            var patrolId = int.Parse(args[1].ToString().Remove(0, 9));
            var patrolPrefix = args[1].ToString().Substring(0, 2);
            var patrolTab = "lspd";

            switch(patrolPrefix) {
                case "SD":
                    patrolTab = "lssd";
                    break;
                case "FD":
                    patrolTab = "lsfd";
                    break;
            }

            using(var db = new ChoiceVDb()) {
                using(var fsDb = new ChoiceVFsDb()) {
                    var patrol = fsDb.executive_patrols.Include(p => p.executive_patrol_members).FirstOrDefault(p => p.tab == patrolTab && p.id == patrolId);
                    var dispatch = db.dispatches.Find(dispatchId);

                    if(patrol != null && dispatch != null) {
                        var employeeIds = patrol.executive_patrol_members.Select(m => m.employeeId);
                        var charIds = db.companyemployees.Where(e => employeeIds.Contains(e.id)).Select(e => e.charId).ToList();

                        var players = ChoiceVAPI.GetAllPlayers().Where(p => charIds.Contains(p.getCharacterId()));

                        var pos = new Position(float.Parse(dispatch.positionX), float.Parse(dispatch.positionY), 1);

                        foreach(var p in players) {
                            BlipController.createPointBlip(p, $"D-{dispatch.id} {dispatch.title}", pos, 29, 60, 255, "Dispatch-" + dispatch.id);
                            SoundController.playSound(p, SoundController.Sounds.Dispatch, 1, "mp3");
                        }

                        InvokeController.AddTimedInvoke($"Dispatch-{dispatch.id}-Remover", (i) => {
                            foreach(var p in players) {
                                if(p != null && p.Exists()) {
                                    BlipController.destroyBlipByName(p, "Dispatch-" + dispatch.id);
                                }
                            }
                        }, TimeSpan.FromMinutes(5), false);
                    } else {
                        player.sendBlockNotification("Es ist ein Fehler aufgetreten. Bitte versuche es erneut!", "Fehler");
                    }
                }
            }

            return true;
        }
    }
}
