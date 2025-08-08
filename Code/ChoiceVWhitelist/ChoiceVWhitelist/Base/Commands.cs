using System;
using System.Collections.Generic;
using System.Text;
using ChoiceVServer.Base;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using System.Reflection;
using System.Threading;
using AltV.Net.Data;
using ChoiceVWhitelist;

namespace ChoiceVServer.Admin.Commands {
    public class CommandAttribute : Attribute {
        public string Command;
        public string Help;
        public CommandAttribute(string command, string help) {
            Command = command;
            Help = help;
        }
    }

    class Commands : ChoiceVScript {
        private record Command(MethodInfo Method, string Help);
        private Dictionary<string, Command> AllCommands = new Dictionary<string, Command>();

        public Commands() {
            EventController.addEvent("chatmessage", handleChatMessage);

            var methods = GetType().GetMethods();
            foreach(var method in methods.Where(ifo => ifo.CustomAttributes.Any(att => att.AttributeType == typeof(CommandAttribute)))) {
                var cmd = method.GetCustomAttribute<CommandAttribute>();
                AllCommands.Add(cmd.Command.ToLower(), new Command(method, cmd.Help));

                Logger.logDebug($"Command: {cmd.Command.ToLower()} registered");
            }

            EventController.addEvent("END_FREECAM", onEndFreeCam);
        }

        private bool handleChatMessage(IPlayer player, string eventName, object[] args) {
            var input = args[0].ToString();
            Logger.logDebug($"Command thrown by: {player.Name}: {input}");

            if(input.StartsWith('/')) {
                string[] commandWithArgs = input.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                var key = commandWithArgs[0].Substring(1).ToLower();
                if(AllCommands.ContainsKey(key)) {
                    try {
                        AllCommands[key].Method.Invoke(null, new object[] { player, commandWithArgs.Skip(1).ToArray() });
                    } catch(Exception) {
                        player.emitClientEvent("chatmessage", null, "{FF0000}" + AllCommands[key].Help);
                    }
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Command not found!");
                }
            } else {
                player.emitClientEvent("chatmessage", null, "{FF0000} Thats not a command!");
            }

            return true;
        }

        [Command("createVehicle", "/createvehicle [model] | z.B. /createvehicle adder")]
        public static void createVehicle(IPlayer player, string[] args) {
            IVehicle vehicle = Alt.CreateVehicle(args[0], player.Position, player.Rotation);
            if(ScenarioController.isPlayerInScenario(player)) {
                ScenarioController.getPlayerScenario(player).addVehicle(vehicle);
            }
            vehicle.EngineOn = true;
            vehicle.ManualEngineControl = false;
            player.Emit("SET_PED_INTO_VEHICLE", vehicle);
        }

        [Command("deleteVehicle", "/deleteVehicle")]
        public static void deleteVehicle(IPlayer player, string[] args) {
            if(!player.IsInVehicle) {
                return;
            }
            player.Vehicle.Destroy();
        }

        [Command("deleteAllVehicles", "/deleteAllVehicles")]
        public static void deleteAllVehicles(IPlayer player, string[] args) {
            var scenario = ScenarioController.getPlayerScenario(player);
            foreach(var veh in Alt.GetAllVehicles().Reverse<IVehicle>()) {
                if(scenario != null) {
                    if(scenario.Vehicles.Contains(veh)) {
                       veh.Destroy();
                    }
                } else {
                    if(veh.Dimension == 0) {
                        veh.Destroy();
                    }
                }
            }
        }

        [Command("setVehColors", "/setvehcolor [primär] [sekundär] | z.B. /setvehcolor 123 19")]
        public static void setVehColors(IPlayer player, string[] args) {
            if(!player.IsInVehicle) {
                return;
            }
            IVehicle vehicle = player.Vehicle;
            vehicle.PrimaryColor = ((byte)int.Parse(args[0]));
            vehicle.SecondaryColor = ((byte)int.Parse(args[1]));
        }

        [Command("showSc", "/showSc [Zeit in Sekunden] | z.B. /showSc 30")]
        public static void showSc(IPlayer player, string[] args) {
            var time = int.Parse(args[0]);
            foreach(var p in Alt.GetAllPlayers()) {
                if(p.Position.Distance(player.Position) < 50) {
                    player.emitClientEvent("TEXT_LABEL_ON_PLAYER", p, (string)p.getData("SOCIALCLUB"), time * 1000);
                }
            }
        }

        [Command("giveWeapon", "/giveWeapon [Socialclub : optional] [Waffe] [Munition] | z.B. /giveWeapon WEAPON_PISTOL 12 o. /giveWeapon Devknecht WEAPON_PISTOL 12")]
        public static void giveWeapon(IPlayer player, string[] args) {
            if(args.Length > 2) {
                var sc = args[0];

                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.GiveWeapon(Alt.Hash(args[1]), int.Parse(args[2]), false);
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                player.GiveWeapon(Alt.Hash(args[0]), int.Parse(args[1]), false);
            }
        }

        [Command("clearWeapons", "/clearWeapons [Socialclub : optional]")]
        public static void clearWeapons(IPlayer player, string[] args) {
            if(args.Length > 0) {
                var sc = args[0];

                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.RemoveAllWeapons(true);
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                player.RemoveAllWeapons(true);
            }
        }

        [Command("tpTo", "/tpToPlayer [Socialclub]")]
        public static void tpToPlayer(IPlayer player, string[] args) {
            var sc = args[0];
            var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
            if(target != null) {
                player.Position = target.Position;
            } else {
                player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
            }
        }

        [Command("tpToMe", "/tpPlayerToMe [socialclub] | z.B. /tpPlayerToMe DevKnecht ")]
        public static void tpPlayerToMe(IPlayer player, string[] args) {
            if(args.Length <= 0) {
                var scenario = ScenarioController.getPlayerScenario(player);
                if(scenario != null) {
                    foreach(var target in scenario.Players) {
                        target.Position = player.Position;
                    }
                }
            } else {
                var sc = args[0];
                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.Position = player.Position;
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            }
        }

        [Command("kick", "/kick [Socialclub] [Grund] | z.B. /kick Devknecht Trolling")]
        public static void kick(IPlayer player, string[] args) {
            var sc = args[0];
            var reason = args[1];

            var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
            if(target != null) {
                target.Kick(reason);
                ConnectionController.AllowedSocialclubs.Remove(sc);
            } else {
                player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
            }
        }

        [Command("setModel", "/setmodel [SocialClub : optional!] [modelName] [Kopfvariante] [Oberkörpervariante] | z.B. /setmodel csb_grove_str_dlr 0 0 o. /setModel Devknecht csb_grove_str_dlr 0 0")]
        public static void setModel(IPlayer player, string[] args) {
            if(args.Length >= 4) {
                var sc = args[0];
                var model = args[1];

                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.Model = Alt.Hash(model);
                    target.Emit("INTERIOR_RELOAD");
                    target.SetClothes(0, ushort.Parse(args[2]), 0, 0);
                    target.SetClothes(3, ushort.Parse(args[3]), 0, 0);
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                var model = args[0];
                player.Model = Alt.Hash(model);

                try {
                    player.SetClothes(0, ushort.Parse(args[1]), 0, 0);
                    player.SetClothes(3, ushort.Parse(args[2]), 0, 0);
                } catch(Exception) { }

                player.Emit("INTERIOR_RELOAD");
            }
        }

        [Command("addPlayer", "/addPlayer [socialclub] | z.B. /addPlayer DevKnecht")]
        public static void addPlayer(IPlayer player, string[] args) {
            ConnectionController.AllowedSocialclubs.Add(args[0]);
            player.Emit("chatmessage", null, $"Du hast {args[0]} freigeschalten. Es befinden sich {ConnectionController.AllowedSocialclubs.Count} Spieler auf der Liste!");
        }

        [Command("removePlayer", "/removePlayer [socialclub] | /removePlayer DevKnecht")]
        public static void removePlayer(IPlayer player, string[] args) {
            var sc = args[0];
            var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
            if(target != null) {
                target.Kick("");
                ConnectionController.AllowedSocialclubs.Remove(sc);
            }

            player.Emit("chatmessage", null, $"Du hast {args[0]} entfernt. Es befinden sich {ConnectionController.AllowedSocialclubs.Count} Spieler auf der Liste!");
        }

        [Command("showOnline", "/showOnline")]
        public static void showOnline(IPlayer player, string[] args) {
            string str1 = (string)Alt.GetAllPlayers().ToList().First().getData("SOCIALCLUB");
            foreach(var p in Alt.GetAllPlayers().Skip(1)) {
                var str2 = (string)p.getData("SOCIALCLUB");
                str1 = str1 + ", " + str2;
            }
            player.Emit("chatmessage", null, "Aktuell online: " + str1);
        }

        [Command("showList", "/showList")]
        public static void showList(IPlayer player, string[] args) {
            if(ConnectionController.AllowedSocialclubs.Count <= 0) {
                player.Emit("chatmessage", null, "Die Whitelist ist leer!");
            } else {
                string str1 = ConnectionController.AllowedSocialclubs.First();
                foreach(string str2 in ConnectionController.AllowedSocialclubs.Skip(1)) {
                    str1 = str1 + ", " + str2;
                }
                player.Emit("chatmessage", null, "Die Whitelist: " + str1);
            }
        }

        [Command("clearList", "/clearList")]
        public static void clearList(IPlayer player, string[] args) {
            var sc = args[0];
            foreach(string allowedSocialclub in ConnectionController.AllowedSocialclubs) {
                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.Kick("Whitelist gecleared!");
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            }
        }

        [Command("respawn", "/respawn [Socialclub : optional] | z.B. /respawn o. /respawn DevKnecht2")]
        public static void respawn(IPlayer player, string[] args) {
            if(args.Length > 0) {
                var sc = args[0];
                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.Spawn(target.Position);
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                player.Spawn(player.Position);
            }
        }

        [Command("clearBlood", "/clearBlood [socialclub : optional] | z.B. /clearBlood o. /clearBlood DevKnecht")]
        public static void clearBlood(IPlayer player, string[] args) {
            if(args.Length > 0) {
                var sc = args[0];
                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    foreach(var p in Alt.GetAllPlayers().ToList()) {
                        p.emitClientEvent("CLEAR_PED_BLOOD", target);
                    }
 
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                foreach(var p in Alt.GetAllPlayers().ToList()) {
                    p.emitClientEvent("CLEAR_PED_BLOOD", player);
                }
            }
        }

        [Command("addToScenario", "/addToScenario [socialclub] | z.B. /addToScenario DevKnecht")]
        public static void addToScenario(IPlayer player, string[] args) {
            if(ScenarioController.isPlayerInScenario(player)) {
                var sc = args[0];

                var playerScenario = ScenarioController.getPlayerScenario(player);
                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    ScenarioController.addPlayerToScenario(target, playerScenario);
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                player.Emit("chatmessage", "Du bist in keinem Szenario!");
            }
        }

        [Command("setTime", "/setTime [Stunde] | z.B. /setTime 12")]
        public static void setTime(IPlayer player, string[] args) {
            if(ScenarioController.isPlayerInScenario(player)) {
                ScenarioController.getPlayerScenario(player).TimeHour = int.Parse(args[0]);
            } else {
                WorldController.ServerHour = int.Parse(args[0]);
            }
        }

        [Command("setHealth", "/setHealth [socialclub : optional] [Leben 0 - 100] | z.B. /setHealth 100 o. /setHealth DevKnecht 100 ")]
        public static void setHealth(IPlayer player, string[] args) {
            if(args.Length > 1) {
                var sc = args[0];
                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.Health = (ushort)(ushort.Parse(args[1]) + 100);
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                player.Health = ushort.Parse(args[0]);
            }
        }

        [Command("setWeather", "/setWeather [Wetter] | z.B. /setWeather 1 | (0, 1: Sonnig), (2: Wolkig), (3: Smog), (4: Nebel), (6: Regen), (7: Gewitter), (10: Schnee), (11: Blizzard), (13: Weihnachten)")]
        public static void setWeather(IPlayer player, string[] args) {
            if(ScenarioController.isPlayerInScenario(player)) {
                ScenarioController.getPlayerScenario(player).Weather = uint.Parse(args[0]);
            } else {
                WorldController.Weather = uint.Parse(args[0]);
            }
        }

        [Command("freeCam", "/freeCam")]
        public static void freeCam(IPlayer player, string[] args) {
            player.Emit("FREE_CAM");
            player.Frozen = true;
        }

        private bool onEndFreeCam(IPlayer player, string eventName, object[] args) {
            player.Frozen = false;

            return true;
        }

        [Command("getPos", "/getPos")]
        public static void getPos(IPlayer player, string[] args) {
            player.Emit("chatmessage", null, string.Format("Position: {0} {1} {2}", (object)Math.Round((double)player.Position.X, 1), (object)Math.Round((double)player.Position.Y, 1), (object)Math.Round((double)player.Position.Z, 1)));
        }

        [Command("setPos", "/setPos [Socialclub: optional!] [X] [Y] [Z] | z.B. /setPos 0 0 72 o. /setPos Devknecht 0 0 72")]
        public static void setPos(IPlayer player, string[] args) {
            if(args.Length >= 4) {
                var sc = args[0];
                var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == sc);
                if(target != null) {
                    target.Position = new Position(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                    target.setDimension(player.Dimension);
                } else {
                    player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
                }
            } else {
                player.Position = new Position(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
            }
        }

        [Command("invis", "/invis ")]
        public static void invis(IPlayer player, string[] args) {
            player.Streamed = !player.Streamed;
            player.emitClientEvent("PLAYER_INVISIBLE", !player.Streamed);


            //if(!player.HasSyncedMetaData("PLAYER_INVISIBLE")) {
            //    player.SetSyncedMetaData("PLAYER_INVISIBLE", false);
            //}
            //player.GetSyncedMetaData<bool>("PLAYER_INVISIBLE", out var flag);
            //player.SetSyncedMetaData("PLAYER_INVISIBLE", !flag);
        }

        [Command("startScenario", "/startScenario")]
        public static void startScenario(IPlayer player, string[] args) {
            ScenarioController.createScenario(player);
        }


        [Command("stopScenario", "/stopScenario")]
        public static void stopScenario(IPlayer player, string[] args) {
            ScenarioController.deleteScenario(player);
        }


        [Command("spectate", "/spectate [Socialclub] | z.B. /spectate DevKnecht")]
        public static void spectate(IPlayer player, string[] args) {
            var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == args[0]);
            //player.SetSyncedMetaData("PLAYER_INVISIBLE", true);
            player.emitClientEvent("SPECTATE_CAM", target);

            player.Collision = false;
        }

        [Command("stopSpectate", "/stopSpectate")]
        public static void stopSpectate(IPlayer player, string[] args) {
            player.emitClientEvent("END_SPECTATE_CAM");

            player.Collision = true;
        }

        [Command("message", "/message [Socialclub] [Rest ist Nachricht]")]
        public static void message(IPlayer player, string[] args) {
            var target = Alt.GetAllPlayers().ToList().FirstOrDefault(p => p.getData("SOCIALCLUB") == args[0]);

            if(target != null) {
                var str = "";
                args.Skip(1).ForEach(arg => str += " " + arg);

                target.Emit("notifications:show", str, true, -1, 6);
                player.emitClientEvent("chatmessage", null, "{00FF00} Nachricht wurde gesendet!");
            } else {
                player.emitClientEvent("chatmessage", null, "{FF0000} Spieler nicht gefunden!");
            }
        }

        [Command("messageAll", "/messageAll [Rest ist Nachricht]")]
        public static void messageAll(IPlayer player, string[] args) {
            var scenario = ScenarioController.getPlayerScenario(player);
            if(scenario != null) {
                var str = "";
                args.Skip(1).ForEach(arg => str += " " + arg);

                foreach(var target in scenario.Players) {
                    target.Emit("notifications:show", str, true, -1, 6);
                }
                player.emitClientEvent("chatmessage", null, "{00FF00} Nachricht wurde gesendet!");
            }
        }
    }
}