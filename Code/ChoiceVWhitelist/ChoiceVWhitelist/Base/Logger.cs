using AltV.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChoiceVServer.Base {
    static class Logger {
        public static void logTrace(string text) {
            //if (Config.IsDevServer) {
            //Alt.Core.LogInfo($"[{DateTime.Now.TimeOfDay}] [TRACE] {text}");
            //}
        }

        public static void logDebug(string text) {
            //if (Config.IsDevServer) {
            Alt.Core.LogInfo($"[{DateTime.Now.TimeOfDay}] [DEBUG] {text}");
            //}

        }

        public static void logInfo(string text) {
            //ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [INFO] {text}", ConsoleColor.White);
            Alt.Core.LogInfo($"[{DateTime.Now.TimeOfDay}] [INFO] {text}");
        }

        public static void logWarning(string text) {
            //ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [WARN] {text}", ConsoleColor.Magenta);
            Alt.Core.LogWarning($"[{DateTime.Now.TimeOfDay}] [WARN] {text}");
        }

        public static void logError(string text) {
            //ChoiceVAPI.Log($"[{DateTime.Now.TimeOfDay}] [ERROR] {text}", ConsoleColor.Red);
            Alt.Core.LogError($"[{DateTime.Now.TimeOfDay}] [ERROR] {text}");
        }

        public static void logFatal(string text) {
            //ChoiceVAPI.Log($"\n[{DateTime.Now.TimeOfDay}] [FATAL] {text}\n", ConsoleColor.Red);
            Alt.Core.LogError($"\n[{DateTime.Now.TimeOfDay}] [FATAL] {text}\n");
        }

    }
}
