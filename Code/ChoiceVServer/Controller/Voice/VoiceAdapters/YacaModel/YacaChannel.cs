using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Voice.YacaModel {
    public class YacaChannel {
        private int ChannelId;
        private YacaFilter ChannelType;
        public Dictionary<YacaPlayer, bool> Players;
        private string ShowChannelName = null;
       
        private object ActionsLock = new object();
        
        //TODO For Future: Make something like: IsLeaking (Meaning that surrounding players can hear into the channel (muffled))
        public YacaChannel(int channelId, YacaFilter filterType) {
            ChannelId = channelId;
            ChannelType = filterType;
            Players = new Dictionary<YacaPlayer, bool>();
        }

        public YacaChannel withShowOtherPlayersSending(string showName) {
            ShowChannelName = showName; 
            
            return this;
        }

        public void remove() {
            lock (ActionsLock) {
                foreach(var p in Players.Keys) {
                    p.VoicePlayer.emitClientEvent("YACA_UPDATE_COMM_DEVICE",
                        YacaAdapter.getValueForYacaFilter(ChannelType),
                        ChannelId,
                        "MUTE",
                        Players.Select(p => p.Key.ClientId).ToArray().ToJson(),
                        ShowChannelName,
                        true
                    );
                }

                Players = null;
            }
        }
        
        public int getPlayerCount() {
            return Players.Count;
        }
        
        public bool hasPlayer(YacaPlayer player) {
            return Players.ContainsKey(player);
        }
        
        public void addPlayer(YacaPlayer player, bool initialMute) {
            Players[player] = initialMute;
        }
        
        public void removePlayer(YacaPlayer player) {
            lock (ActionsLock) {
                Players.Remove(player);

                foreach(var p in Players.Keys) {
                    p.VoicePlayer.emitClientEvent("YACA_UPDATE_COMM_DEVICE",
                        YacaAdapter.getValueForYacaFilter(ChannelType),
                        ChannelId,
                        "MUTE",
                        new[] { player.ClientId }.ToJson(),
                        ShowChannelName,
                        true
                    );
                }
            }
        }
        
        public void mutePlayer(YacaPlayer player) {
            lock (ActionsLock) {
                if(Players.ContainsKey(player)) {
                    Players[player] = true;

                    foreach(var p in Players.Keys) {
                        p.VoicePlayer.emitClientEvent("YACA_UPDATE_COMM_DEVICE",
                            YacaAdapter.getValueForYacaFilter(ChannelType),
                            ChannelId,
                            "MUTE",
                            new[] { player.ClientId }.ToJson(),
                            ShowChannelName,
                            false
                        );

                        if(ShowChannelName != null) {
                            VoiceController.updateVoiceChannelHud(p.VoicePlayer, ShowChannelName, player.VoicePlayer != p.VoicePlayer, true);
                        }
                    }
                }
            }
        }

        private record PluginPlayer(int client_id, int mode);
        public void unmutePlayer(YacaPlayer player) {
            lock (ActionsLock) {
                if(Players.ContainsKey(player)) {
                    Players[player] = false;

                    foreach(var p in Players.Keys) {
                        p.VoicePlayer.emitClientEvent("YACA_UPDATE_COMM_DEVICE",
                            YacaAdapter.getValueForYacaFilter(ChannelType),
                            ChannelId,
                            "UNMUTE",
                            Players.Select(p => new PluginPlayer(p.Key.ClientId,
                                    p.Value ? YacaAdapter.getValueForYacaCommDeviceType(YacaCommDeviceType.Receiver) : YacaAdapter.getValueForYacaCommDeviceType(YacaCommDeviceType.Transceiver))).ToList()
                                .ToJson(),
                            ShowChannelName,
                            false
                        );

                        if(ShowChannelName != null) {
                            VoiceController.updateVoiceChannelHud(p.VoicePlayer, ShowChannelName, player.VoicePlayer != p.VoicePlayer, false);
                        }
                    }
                }
            }
        }
    }
}