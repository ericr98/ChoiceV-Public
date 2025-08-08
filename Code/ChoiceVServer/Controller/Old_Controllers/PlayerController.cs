//using System;
//using System.Collections.Generic;
//using System.Linq;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;

//namespace ChoiceVServer.Controller {
//    public class PlayerController : ChoiceVScript {

//        public static Dictionary<int, IPlayer> AllOnlinePlayers { get; private set; }

//        public PlayerController()
//        {
//            AllOnlinePlayers = new Dictionary<int, IPlayer>();

//            EventController.PlayerSuccessfullConnectionDelegate += loadCharacter;
//            EventController.PlayerDisconnectedDelegate += disconnectCharacter;
//            InvokeController.AddTimedInvoke("CheckForDisconnectedPlayers", checkForDisconnectedPlayers, TimeSpan.FromMinutes(1), true);
//        }

//        private void checkForDisconnectedPlayers(IInvoke obj)
//        {
//            AllOnlinePlayers.RemoveWhere(p=> !p.Value.IsConnected);
//        }

//        private void disconnectCharacter(IPlayer player, string reason)
//        {
//            if (AllOnlinePlayers.Values.Contains(player))
//            {
//                AllOnlinePlayers.RemoveWhere(p=> p.Value == player);
//            }
//        }

//        public static IPlayer getPlayerToCharId(int charId)
//        {
//            if (AllOnlinePlayers.ContainsKey(charId))
//            {
//                return AllOnlinePlayers[charId];
//            }

//            return null;
//        }

//        private void loadCharacter(IPlayer player, characters character)
//        {
//            if (AllOnlinePlayers.ContainsKey(character.id))
//            {
//                AllOnlinePlayers.Remove(character.id);
//            }
//            AllOnlinePlayers.Add(character.id, player);
//        }
//    }
//}
