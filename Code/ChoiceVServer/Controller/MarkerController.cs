using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public class MarkerController : ChoiceVScript {
        public class ChoiceVMarker {
            public int Id;
            public int Type;
            public Position Position;
            public Rgba Rgba;
            public float Scale;
            public float ShowDistance;

            public List<IPlayer> ShowTo;

            public ChoiceVMarker(int id, int type, Position position, Rgba rgba, float scale, float showDistance, List<IPlayer> showTo = null) {
                Id = id;
                Type = type;
                Position = position;
                Rgba = rgba;
                Scale = scale;
                ShowDistance = showDistance;

                if(showTo == null) {
                    ShowTo = new List<IPlayer>();
                } else {
                    ShowTo = showTo;

                    foreach(var player in ShowTo) {
                        player.emitClientEvent("CREATE_MARKER", Id, Type, Position.X, Position.Y, Position.Z, (int)Rgba.R, (int)Rgba.G, (int)Rgba.B, (int)Rgba.A, Scale, ShowDistance);
                    }
                }
            }

            public void showToPlayer(IPlayer player) {
                ShowTo.Add(player);
                player.emitClientEvent("CREATE_MARKER", Id, Type, Position.X, Position.Y, Position.Z, (int)Rgba.R, (int)Rgba.G, (int)Rgba.B, (int)Rgba.A, Scale, ShowDistance);
            }

            public void noLongerShowToPlayer(IPlayer player) {
                ShowTo.Remove(player);
                player.emitClientEvent("DELETE_MARKER", Id);
            }

            public void deleteMarker() {
                foreach(var player in ShowTo) {
                    player.emitClientEvent("DELETE_MARKER", Id);
                }
            }
        }

        private static int MarkerCounter = 0;
        private static List<ChoiceVMarker> CurrentMarkers = new List<ChoiceVMarker>();

        public static ChoiceVMarker createMarker(int type, Position position, Rgba rgba, float scale, float showDistance, List<IPlayer> showTo = null) {
            var marker = new ChoiceVMarker(MarkerCounter++, type, position, rgba, scale, showDistance, showTo);

            CurrentMarkers.Add(marker);

            return marker;
        }

        public static void removeMarker(ChoiceVMarker marker) {
            if(CurrentMarkers.Contains(marker)) {
                CurrentMarkers.Remove(marker);
                marker.deleteMarker();
            } else {
                Logger.logError($"Marker that was tried to be removed does not exist!",
                        $"Fehler bei Markern: Es wurde versucht einen nicht existierenden Markern {marker.Id}");
            }
        }
    }
}
