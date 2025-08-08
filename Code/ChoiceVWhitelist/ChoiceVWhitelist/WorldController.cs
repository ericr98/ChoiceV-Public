using AltV.Net;
using AltV.Net.Elements.Entities;
using ChoiceVServer;
using ChoiceVServer.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVWhitelist {
    public class WorldController : ChoiceVScript {
        public static int ServerHour = 12;
        public static uint Weather = 1;

        public WorldController() {
            InvokeController.AddTimedInvoke("TIME_SETTER", setTimeForAll, TimeSpan.FromSeconds(1), true);

            //EventController.PlayerDamageDelegate += onPlayerDead;
        }

        //private void onPlayerDead(IPlayer player, IEntity killer, uint weapon, ushort damage) {
        //    player.Spawn(player.Position, 5000);
        //}

        private void setTimeForAll(IInvoke obj) {
            foreach(var player in Alt.GetAllPlayers().ToList().Reverse<IPlayer>()) {
                ScenarioController.Scenario playerScenario = ScenarioController.getPlayerScenario(player);
                if(playerScenario != null) {
                    player.emitClientEvent("SET_DATE_TIME_HOUR", playerScenario.TimeHour);
                    player.SetWeather(playerScenario.Weather);
                } else {
                    player.emitClientEvent("SET_DATE_TIME_HOUR", ServerHour);
                    player.SetWeather(Weather);
                }
            }
        }
    }
}
