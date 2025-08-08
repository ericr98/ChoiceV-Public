using AltV.Net;
using AltV.Net.Elements.Entities;
using ChoiceVServer;
using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChoiceVWhitelist {
    public class VoiceController : ChoiceVScript {

        //Maybe Host Voice Server External
        //https://wiki.altv.mp/wiki/Scripting:CDN_Links#Voice_Server

        public class VoiceDimension {
            private static List<IVoiceChannel> Channels;
             
            public VoiceDimension() {
                Channels = new List<IVoiceChannel>();
                foreach(var range in VoiceRanges) {
                    Channels.Add(Alt.CreateVoiceChannel(true, range));
                }
            }

            public void addPlayer(IPlayer player) {
                foreach(var channel in Channels) {
                    channel.AddPlayer(player);
                    channel.MutePlayer(player);
                }

                var currentRange = (int)player.getData("VOICE_RANGE");

                var selected = Channels.FirstOrDefault(c => c.MaxDistance == currentRange);
                selected.UnmutePlayer(player);
            }

            public void removePlayer(IPlayer player) {
                foreach(var channel in Channels) {
                    channel.RemovePlayer(player);
                }
            }

            public void changeVolume(IPlayer player, int newVolume) {
                foreach(var channel in Channels) {
                    channel.MutePlayer(player);
                }

                var selected = Channels.FirstOrDefault(c => c.MaxDistance == newVolume);
                selected.UnmutePlayer(player);
            }

            public void setPlayerMute(IPlayer player, bool mute) {
                foreach(var channel in Channels) {
                    channel.MutePlayer(player);
                }

                if(!mute) {
                    var selected = Channels.FirstOrDefault(c => c.MaxDistance == (int)player.getData("VOICE_RANGE"));
                    selected.UnmutePlayer(player);
                }
            }

            public void delete() {
                foreach(var channel in Channels) {
                    channel.Destroy();
                }
            }
        }

        public static Dictionary<int, VoiceDimension> VoiceDimensions { get; set; }

        private static int StartRange = 6;
        private static List<int> VoiceRanges = new List<int> { 3, 6, 15 };
        private static List<string> VoiceRangeNames = new List<string> { "Nah", "Normal", "Fern" };

        public VoiceController() {
            EventController.PlayerConnectedDelegate += onPlayerConnect;

            VoiceDimensions = new();
            VoiceDimensions[0] = new VoiceDimension();

            EventController.addEvent("TOGGLE_VOICE_RANGE", onToggleVoiceRange);
            EventController.addEvent("TOGGLE_MUTE_VOICE", onToggleMuteVoice);
        }

        private bool onToggleVoiceRange(IPlayer player, string eventName, object[] args) {
            var current = (int)player.getData("VOICE_RANGE");

            var idx = VoiceRanges.IndexOf(current);
            idx = (idx + 1) % VoiceRanges.Count;
        
            var newRange = VoiceRanges[idx];
            player.setData("VOICE_RANGE", newRange);

            VoiceDimensions[0].changeVolume(player, newRange);

            player.emitClientEvent("SHOW_VOICE_RANGE", newRange, VoiceRangeNames[idx]);
            return true;
        }

        private bool onToggleMuteVoice(IPlayer player, string eventName, object[] args) {
            var current = (bool)player.getData("IS_MUTE");
            player.setData("IS_MUTE", !current);

            VoiceDimensions[0].setPlayerMute(player, !current);

            player.emitClientEvent("notifications:show", !current ? "Du bist nun gemuted!" : "Du bist nun entmuted!", false, -1, !current ? 12 : 20); 
            return true;
        }

        private void onPlayerConnect(IPlayer player, string reason) {
            //Alt.Emit("SaltyChat:EnablePlayer", player);

            player.setData("VOICE_RANGE", StartRange);
            player.setData("IS_MUTE", false);

            VoiceDimensions[0].addPlayer(player);
        }

        public static void addPlayerToGlobalVoice(IPlayer player) {
            VoiceDimensions[0].addPlayer(player);
        }

        public static void removePlayerFromGlobalVoice(IPlayer player) {
            VoiceDimensions[0].removePlayer(player);
        }

        public VoiceDimension createNewVoiceDimension(int dimension) {
            var voiceDim = new VoiceDimension();
            VoiceDimensions.Add(dimension, voiceDim);

            return voiceDim;
        }

        public void deleteVoiceDimension(int dimension) {
            var dm = VoiceDimensions.Get(dimension);

            if(dm != null) {
                dm.delete();
            }
        }
    }
}
