using AltV.Net.Elements.Entities;
using BenchmarkDotNet.Running;
using ChoiceVServer.Controller.Discord;
using System;
using System.Numerics;

namespace ChoiceVServer.Base {
    public enum LogCategory {
        ServerStartup = 0,
        System = 1,
        Player = 2,
        Vehicle = 3,
        Support = 4,
    }

    public enum LogActionType {
        Event = 0,
        Created = 1,
        Removed = 2,
        Updated = 3,
        Viewed = 4,
        Blocked = 5,
    }

    static class Logger {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static string getDoerFromCategory(LogCategory category) {
            switch(category) {
                case LogCategory.ServerStartup:
                    return "STARTUP";
                case LogCategory.System:
                    return "SYSTEM";
                default:
                    return "UNKNOWN";
            }
        }

        private static string getActionNameFromType(LogActionType type) {
            switch(type) {
                case LogActionType.Event:
                    return "EVENT";
                case LogActionType.Created:
                    return "CREATED";
                case LogActionType.Removed:
                    return "DELETED";
                case LogActionType.Updated:
                    return "UPDATED";
                case LogActionType.Viewed:
                    return "VIEWED";
                case LogActionType.Blocked:
                    return "BLOCKED";
                default:
                    return "UNKNOWN";
            }
        }

        public static void logTrace(LogCategory category, LogActionType type, string text, string doer = null) {
            if(doer == null) {
                doer = getDoerFromCategory(category);
            }

            logger.Trace($"[{doer}|{getActionNameFromType(type)}] {text}");
        }

        public static void logTrace(LogCategory category, LogActionType type, IPlayer player, string text) {
            if(category == LogCategory.Support) {
                logTrace(category, type, text, $"SUPPORT|{player.getCharacterId()}-{player.getAccountId()}");
            } else {
                logTrace(category, type, text, $"PLAYER|{player.getCharacterId()}-{player.getAccountId()}");
            }
        }

        public static void logDebug(LogCategory category, LogActionType type, string text, string doer = null) {
            if(doer == null) {
                doer = getDoerFromCategory(category);
            }

            logger.Debug($"[{doer}|{getActionNameFromType(type)}] {text}");
        }

        public static void logDebug(LogCategory category, LogActionType type, IPlayer player, string text) {
            if(category == LogCategory.Support) {
                logDebug(category, type, text, $"SUPPORT|{player.getCharacterId()}-{player.getAccountId()}");
            } else {
                logDebug(category, type, text, $"PLAYER|{player.getCharacterId()}-{player.getAccountId()}");
            }
        }

        public static void logDebug(LogCategory category, LogActionType type, ChoiceVVehicle vehicle, string text) {
            logDebug(category, type, text, $"VEHICLE {vehicle.VehicleId}");
        }

        public static void logInfo(LogCategory category, LogActionType type, string text, string doer = null) {
            if(doer == null) {
                doer = getDoerFromCategory(category);
            }

            logger.Info($"[{doer}|{getActionNameFromType(type)}] {text}");
        }

        public static void logInfo(LogCategory category, LogActionType type, IPlayer player, string text) {
            if(category == LogCategory.Support) {
                logInfo(category, type, text, $"SUPPORT|{player.getCharacterId()}-{player.getAccountId()}");
            } else {
                logInfo(category, type, text, $"PLAYER|{player.getCharacterId()}-{player.getAccountId()}");
            }
        }

        public static void logWarning(LogCategory category, LogActionType type, string text, string doer = null) {
            if(doer == null) {
                doer = getDoerFromCategory(category);
            }

            logger.Warn($"[{doer}|{getActionNameFromType(type)}] {text}");
        }

        public static void logWarning(LogCategory category, LogActionType type, IPlayer player, string text) {
            if(category == LogCategory.Support) {
                logWarning(category, type, text, $"SUPPORT|{player.getCharacterId()}-{player.getAccountId()}");
            } else {
                logWarning(category, type, text, $"PLAYER|{player.getCharacterId()}-{player.getAccountId()}");
            }
        }

        public static void logWarning(LogCategory category, LogActionType type, ChoiceVVehicle vehicle, string text) {
            if(category == LogCategory.Support) {
                logWarning(category, type, text, $"SUPPORT|{vehicle.VehicleId}");
            } else {
                logWarning(category, type, text, $"VEHICLE|{vehicle.VehicleId}");
            }
        }

        public static void logError(string text, string description = null, IPlayer player = null) {
            logger.Error(text);

            //DiscordController.sendEmbedInChannel("Fehler geworfen", text + description, null, null, player);
        }

        public static void logFatal(string text) {
            logger.Fatal(text);
        }

        public static void logFatal(IPlayer player, string text) {
            logFatal($"Character with ID: {player.getCharacterId()} has done: " + text);
        }

        public static void logException(IPlayer player, Exception exception, string message = null) {
            logException(exception, $"Character with ID: {message}");
        }

        public static void logException(Exception exception, string message = null) {
            DiscordController.sendEmbedInChannel("Exception geworfen", $"{exception}", null, null);

            if(message == null) {
                logger.Error(exception);
                return;
            }

            logger.Error(exception, message + "\n " + exception.ToString());
        }


        //public static void logTrace(string text) {
        //    //if (Config.IsDevServer) {
        //    ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [TRACE] {text}", ConsoleColor.Cyan);
        //    //}
        //}

        //public static void logDebug(string text) {
        //    //if (Config.IsDevServer) {
        //    ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [DEBUG] {text}", ConsoleColor.Blue);
        //    //}

        //}

        //public static void logInfo(string text) {
        //    //ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [INFO] {text}", ConsoleColor.White);
        //    Alt.Server.LogInfo($"[{DateTime.Now.TimeOfDay}] [INFO] {text}");
        //}

        //public static void logWarning(string text) {
        //    //ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [WARN] {text}", ConsoleColor.Magenta);
        //    Alt.Server.LogWarning($"[{DateTime.Now.TimeOfDay}] [WARN] {text}");
        //}

        //public static void logError(string text) {
        //    //ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [ERROR] {text}", ConsoleColor.Red);
        //    Alt.Server.LogError($"[{DateTime.Now.TimeOfDay}] [ERROR] {text}");
        //}

        //public static void logFatal(string text) {
        //    //ChoiceVAPI.Log($"\n[{DateTime.Now.TimeOfDay}] [FATAL] {text}\n", ConsoleColor.Red);
        //    Alt.Server.LogError($"\n[{DateTime.Now.TimeOfDay}] [FATAL] {text}\n");
        //}

        //public static void logException(Exception exception) {
        //    string stackInfoMethodName = "Could not gather Stack-Data";
        //    try {
        //        StackTrace stackTrace = new StackTrace();
        //        stackInfoMethodName = stackTrace.GetFrame(1)?.GetMethod()?.Name;

        //    } catch (Exception e) {
        //        //Ignore this Crash!
        //    }
        //    logFatal($"[{stackInfoMethodName}] {exception.ToString()}");
        //}

    }
}
