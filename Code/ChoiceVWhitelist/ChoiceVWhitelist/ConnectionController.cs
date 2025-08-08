using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer;
using ChoiceVServer.Base;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ChoiceVWhitelist {
    public class ConnectionController : ChoiceVScript {
        public static List<string> Admins = new List<string>();
        public static List<string> AllowedSocialclubs = new List<string>();

        public ConnectionController() {
            EventController.PlayerConnectedDelegate += onPlayerConnected;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnected;
            EventController.addEvent("SOCIAL_CLUB_REGISTER", onSocialClubRegister);
        }

        private bool onSocialClubRegister(IPlayer player, string eventName, object[] args) {
            string str = args[0].ToString();
            if(!ConnectionController.AllowedSocialclubs.Contains(str) && !ConnectionController.Admins.Contains(str)) {
                player.Kick("Du stehst nicht auf der Whitelist!");
                foreach(var p in Alt.GetAllPlayers().ToList()) {
                    if(ConnectionController.Admins.Contains(p.getData("SOCIALCLUB"))) {
                        p.emitClientEvent("chatmessage", null, $"{str} wurde gekickt, da er nicht auf der Whitelist stand!");
                    }
                }
                return false;
            } else {
                player.setData("SOCIALCLUB", str);
                return true;
            }
        }

        private void onPlayerConnected(IPlayer player, string reason) {
            player.Spawn(new Position(3246f, -4681f, 115f), 0U);
            player.Model = Alt.Hash("u_m_y_proldriver_01");
            Alt.Emit("PlayerLoggedIn", player, player.Name);
            foreach(var p in Alt.GetAllPlayers().ToList()) {
                player.Emit("DIMENSION_CHANGE", p.Dimension, p.Id);
            }

            player.SetWeather(1);

            Alt.Emit("SaltyChat:SetPlayerAlive", player);
        }

        private void onPlayerDisconnected(IPlayer player, string reason) {
            if(!ScenarioController.isPlayerInScenario(player))
                return;
            ScenarioController.getPlayerScenario(player).removePlayer(player);
        }
    }
}
