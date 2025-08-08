using AltV.Net;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using System.Collections.Generic;

namespace ChoiceVWhitelist {

    public class ScenarioController : ChoiceVScript {
        public class Scenario {
            public int Dimension;
            public int TimeHour;
            public uint Weather;
            public List<IPlayer> Players;
            public List<IVehicle> Vehicles;


            public Scenario(IPlayer owner) {
                Dimension = DimensionCounter;
                DimensionCounter++;
                Players = new List<IPlayer> {
                    owner
                };

                addPlayer(owner);
                Vehicles = new List<IVehicle>();
            }

            public void addPlayer(IPlayer player) {
                Players.Add(player);
                player.setDimension(Dimension);

                //VoiceController.removePlayerFromGlobalVoice(player);
            }

            public void addVehicle(IVehicle vehicle) {
                Vehicles.Add(vehicle);
                vehicle.Dimension = Dimension;
            }

            public void removePlayer(IPlayer player) {
                if(!Players.Remove(player)) {
                    return;
                }
                player.setDimension(0);

                //VoiceController.addPlayerToGlobalVoice(player);
            }
        }

        public static List<Scenario> AllScenarios = new List<Scenario>();
        public static int DimensionCounter = 1;

        public static bool createScenario(IPlayer player) {
            if(!isPlayerInScenario(player)) {
                Scenario scenario = new Scenario(player);
                AllScenarios.Add(scenario);
                return true;
            }
            player.Emit("chatmessage", null, "Du bist schon in einem Scenario!");
            return false;
        }

        public static void deleteScenario(IPlayer player) {
            Scenario playerScenario = getPlayerScenario(player);
            if(playerScenario == null) {
                return;
            }

            AllScenarios.Remove(playerScenario);
            foreach(var veh in playerScenario.Vehicles) {
                veh.Destroy();
            }

            foreach(IPlayer p in playerScenario.Players.ToArray()) {
                playerScenario.removePlayer(p);
            }

            playerScenario.Players.Clear();
            playerScenario.Vehicles.Clear();
        }

        public static void addPlayerToScenario(IPlayer player, Scenario scenario) {
            if(isPlayerInScenario(player)) { 
                getPlayerScenario(player).removePlayer(player);
            }

            scenario.addPlayer(player);
        }

        public static Scenario getPlayerScenario(IPlayer player) {
            foreach(Scenario allScenario in AllScenarios) {
                if(allScenario.Players.Contains(player)) {
                    return allScenario;
                }
            }

            return null;
        }

        public static bool isPlayerInScenario(IPlayer player) {
            foreach(Scenario allScenario in AllScenarios) {
                if(allScenario.Players.Contains(player)) {
                    return true;
                }
            }
            return false;
        }
    }
}
