using AltV.Net;
using ChoiceVServer;
using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ChoiceVWhitelist {
    public class Main : Resource {
        private static List<ChoiceVScript> LoadedTypes = new List<ChoiceVScript>();

        public override void OnStart() {
            foreach(Type type in Assembly.GetExecutingAssembly().GetTypes()) {
                if(type.IsSubclassOf(typeof(ChoiceVScript)) && !type.IsAbstract) {
                    Logger.logInfo("Loading class " + type.Name);
                    ChoiceVScript instance = (ChoiceVScript)Activator.CreateInstance(type);
                    Main.LoadedTypes.Add(instance);
                }
                if(type.IsSubclassOf(typeof(Resource)) && type != typeof(Main) && !type.IsAbstract)
                    throw new InvalidOperationException("Derivates of Script are not allowed! (" + type.Name + "). Use abstract class ChoiceVScript!");
            }



            foreach(string readAllLine in File.ReadAllLines("Admins.txt")) {
                Logger.logDebug("Admin added: " + readAllLine);
                ConnectionController.Admins.Add(readAllLine);
            }

            EventController.onMainReady();
        }

        public override void OnStop() {

        }
    }
}
