using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.Controller.Voice;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public class VoiceController : ChoiceVScript {
        private static List<float> VoiceRanges = [1.5f, 4f, 9f, 25];
        public static IVoiceAdapter VoiceManager;

        public VoiceController() {
            if(Config.IsVoiceEnabled) return;

            if(Config.VoiceSystem == "ALTV") {
                VoiceManager = new AltVVoiceAdapter(VoiceRanges);
            } else if(Config.VoiceSystem == "YACA") {
                VoiceManager = new YacaAdapter(VoiceRanges);
            }

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnected;

            EventController.addKeyEvent("CHANGE_VOLUME", ConsoleKey.OemPlus, "Sprechlautstärke ändern", onVoiceVolumeChange, true);
            EventController.addKeyEvent("PUSH_TO_MUTE", ConsoleKey.Oem2, "Push-to-mute", onPressPushToMute, true);

            DamageController.BackendPlayerDeathDelegate += (p) => globalMutePlayer(p, "DEATH");
            DamageController.BackendPlayerReviveDelegate += (p) => globalUnmutePlayer(p, "DEATH");

            EventController.PlayerChangeDimensionDelegate += onPlayerChangeDimension;
        }

        private static void onPlayerChangeDimension(IPlayer player, int oldDimension, int newDimension) {
            VoiceManager.playerChangeDimension(player, oldDimension, newDimension);
        }

        //Radio
        public static void joinRadioChannel(IPlayer player, string channelName, string displayName) {
            VoiceManager.joinRadioChannel(player, channelName, displayName);
        }

        public static void leaveRadioChannel(IPlayer player, string channelName) { 
            VoiceManager.leaveRadioChannel(player, channelName);
        }

        public static void muteInRadioChannel(IPlayer player, string channelName) {
            VoiceManager.muteInRadioChannel(player, channelName);
        }

        public static void unmuteInRadioChannel(IPlayer player, string channelName) {
            VoiceManager.unmuteInRadioChannel(player, channelName);
        }

        //Phone Calls
        public static void startCall(IPlayer caller, IPlayer called) {
            VoiceManager.startCall(caller, called);
        }

        public static void stopCall(IPlayer caller, IPlayer called) {
            VoiceManager.stopCall(caller, called);
        }

        //Normal Voice Range Talk

        private void onPlayerConnected(IPlayer player, character character) {
            VoiceManager.onPlayerConnect(player); 

            var cefEvt = new VoiceRangeCefEvent(1);
            player.emitCefEventNoBlock(cefEvt);
            VoiceManager.changePlayerSpatialVoiceRange(player, VoiceRanges[0]);
        }


        public static void globalMutePlayer(IPlayer player, string source) {
            var list = new List<string>();
            if (player.hasData("GLOBAL_MUTE_LIST")) {
                list = player.getData("GLOBAL_MUTE_LIST");
            } else {
                player.setData("GLOBAL_MUTE_LIST", list);
            }

            list.Add(source);
            VoiceManager.playerGlobalVoiceMute(player);
        }

        public static void globalUnmutePlayer(IPlayer player, string source) {
            if(source == null) {
                VoiceManager.playerGlobalVoiceUnmute(player, getCurrentVoiceRange(player));
                return;
            }
            
            var list = new List<string>();
            if (player.hasData("GLOBAL_MUTE_LIST")) {
                list = player.getData("GLOBAL_MUTE_LIST");
            }

            list.RemoveAll(s => s.Equals(source));
            if (list.Count == 0) {
                VoiceManager.playerGlobalVoiceUnmute(player, getCurrentVoiceRange(player));

                player.resetData("GLOBAL_MUTE_LIST");
            }
        }

        public static void setVoiceRange(IPlayer player, float newRange) {
            VoiceManager.changePlayerSpatialVoiceRange(player, newRange);
        }

        private static float getCurrentVoiceRange(IPlayer player) {
            if(!player.hasData("VOICE_RANGE")) {
                player.setData("VOICE_RANGE", 1);
            }

            return VoiceRanges[player.getData("VOICE_RANGE")];
        }

        private static void setCurrentVoiceRange(IPlayer player, float range) {
            player.setData("VOICE_RANGE", VoiceRanges.IndexOf(range));
            setVoiceRange(player, range);
        }

        public class VoiceRangeCefEvent : IPlayerCefEvent {
            public string Event { get; private set; }
            public int Range;
            public VoiceRangeCefEvent(int rangeId) {
                Event = "CREATE_VOICERANGE";
                Range = rangeId;
            }
        }

        private bool onVoiceVolumeChange(IPlayer player, ConsoleKey key, string eventName) {
            if(player.hasData("PUSH_TO_MUTE")) {
                return true;
            }

            var voiceRange = getCurrentVoiceRange(player);
            var idx = VoiceRanges.IndexOf(voiceRange);
            idx = (idx + 1) % VoiceRanges.Count;

            player.emitCefEventNoBlock(new VoiceRangeCefEvent(idx + 1));
            player.emitClientEvent("SHOW_VOICE_RANGE", VoiceRanges[idx % VoiceRanges.Count]);

            setCurrentVoiceRange(player, VoiceRanges[idx % VoiceRanges.Count]);
            return true;
        }
        
        private bool onPressPushToMute(IPlayer player, ConsoleKey key, string eventName) {
            if(player.hasData("PUSH_TO_MUTE")) {
                player.resetData("PUSH_TO_MUTE");

                var currentVoiceRange = getCurrentVoiceRange(player);
                player.emitCefEventNoBlock(new VoiceRangeCefEvent(VoiceRanges.IndexOf(currentVoiceRange) + 1));
                globalUnmutePlayer(player, "PUSH_TO_MUTE");
            } else {
                player.setData("PUSH_TO_MUTE", true);

                player.emitCefEventNoBlock(new VoiceRangeCefEvent(0));
                globalMutePlayer(player, "PUSH_TO_MUTE");
            }

            return true;
        }
        
        public class VoiceCefVoiceSolutionMuteEvent : IPlayerCefEvent {
            public string Event { get; private set; }
            public bool State;
            public string Icon;
            
            public VoiceCefVoiceSolutionMuteEvent(bool state, string icon) {
                Event = "VOICE_SOLUTION_MUTE";
                State = state;
                Icon = icon;
            }
        }
        
        public static void displayVoiceSolutionMute(IPlayer player, bool state, string iconName) {
           player.emitCefEventNoBlock(new VoiceCefVoiceSolutionMuteEvent(state, iconName)); 
        }
                    
        
        
        public class VoiceDisplayChannelsEvent : IPlayerCefEvent {
            public string Event { get; private set; }
            public string channel;
            public bool isReceiving;
            public bool isRemoving;
            
            public VoiceDisplayChannelsEvent(string channel, bool isReceiving, bool isRemoving) {
                Event = "UPDATE_CHANNEL_HUD";
                this.channel = channel;
                this.isReceiving = isReceiving;
                this.isRemoving = isRemoving;
            }
        }

        public static void updateVoiceChannelHud(IPlayer player, string channel, bool isReceiving, bool isRemoving) {
            player.emitCefEventNoBlock(new VoiceDisplayChannelsEvent(channel, isReceiving, isRemoving));
        }
    }
}