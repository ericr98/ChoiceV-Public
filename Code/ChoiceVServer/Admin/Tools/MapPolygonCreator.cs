using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Admin.Tools.CollisionShapeCreator;

namespace ChoiceVServer.Admin.Tools {
    public class MapPolygonCreator : ChoiceVScript {
        public MapPolygonCreator() {
            EventController.addEvent("AREA_FINISHED", onAreaFinished);
        }

        public delegate void PolygonShapeCreatorDelegate(List<Vector2> polygon);

        private static Dictionary<int, PolygonShapeCreatorDelegate> Callbacks = new Dictionary<int, PolygonShapeCreatorDelegate>();
        public static void startPolygonCreationWithCallback(IPlayer player, PolygonShapeCreatorDelegate callback) {
            Callbacks[player.getCharacterId()] = callback;

            player.emitClientEvent("AREA_START");
            player.sendNotification(Constants.NotifactionTypes.Info, "Füge mit Numpad+ eine Ecke hinzu. Mit Numpad- wird die letzte Ecke entfernt. Mit Numpad0 wird die Area gespeichert, Numpad, bricht ab.", "");

            player.addState(Constants.PlayerStates.SupportTaskBlock);
        }

        private bool onAreaFinished(IPlayer player, string eventName, object[] args) {
            player.removeState(Constants.PlayerStates.SupportTaskBlock);
            try {
                var code = args[0].ToString();
                if(code == "CANCEL") {
                    return false;
                }

                var pols = args[1].ToString().FromJson<List<Vector2>>();
                
                var callback = Callbacks[player.getCharacterId()];
                callback.Invoke(pols);
            } catch(Exception e) {
                ChoiceVAPI.SendChatMessageToPlayer(player, e.ToString());
            }

            return true;
        }
    }
}
