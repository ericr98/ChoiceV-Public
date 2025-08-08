using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using System;
using System.Linq;

namespace ChoiceVServer.Controller.Voice.YacaModel {
    public class YacaPlayer {
        public IPlayer VoicePlayer;
        public int ClientId;
        public string IngameName;
        public float Range { get; private set; }
        public bool IsMuted { get; private set; }
        public int Dimension { get; private set; }
        
        public YacaPlayer(IPlayer voicePlayer, int clientId, string ingameName, float range) {
            VoicePlayer = voicePlayer;
            ClientId = clientId;
            IngameName = ingameName;
            Range = range;
            IsMuted = false;
        }

        public void removeFromPlayers() {
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("YACA_REMOVE_PLAYER", VoicePlayer.getCharacterId());
            }
        }

        public void setRange(float range) {
            Range = range;
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("YACA_UPDATE_PLAYER", VoicePlayer.getCharacterId(), "range", range);
            }
        }
        
        public void setMute(bool isMuted) {
            IsMuted = isMuted;
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("YACA_UPDATE_PLAYER", VoicePlayer.getCharacterId(), "isMuted", isMuted);
            }
        }
        
        public void setDimension(int dimension) {
            Dimension = dimension;
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("YACA_UPDATE_PLAYER", VoicePlayer.getCharacterId(), "dimension", dimension);
            }
        }
    }
}