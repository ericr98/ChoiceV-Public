using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using Perfolizer.Exceptions;

namespace ChoiceVServer.Controller.Voice;

public class AltVVoiceAdapter : IVoiceAdapter {
    private readonly Dictionary<string, IVoiceChannel> Channels = [];
    private readonly Dictionary<float, IVoiceChannel> SpatialChannels = [];

    public AltVVoiceAdapter(List<float> ranges) {
        Channels = [];
        foreach(var range in ranges) {
            if(range == 0) continue;

            var channel = createVoiceChannel($"SPATIAL-{range}", true, range);

            if(channel != null) {
                SpatialChannels[range] = channel;
            }
        }
    }

    public void onPlayerConnect(IPlayer player) {
        foreach(var channel in SpatialChannels.Values) {
           channel.AddPlayer(player); 
           channel.MutePlayer(player);
        }
    }

    public void onPlayerDisconnect(IPlayer player) { 
        foreach(var channel in Channels) {
            channel.Value.RemovePlayer(player);

            if(channel.Value.PlayerCount == 0 && !SpatialChannels.ContainsValue(channel.Value)) {
                channel.Value.Destroy();
                Channels.Remove(channel.Key);
            }
        }
    }

    public void playerChangeDimension(IPlayer player, int fromDimension, int toDimension) {
        //can be ignored
    }

    public void playerGlobalVoiceMute(IPlayer player) {
        foreach(var channel in Channels) {
            channel.Value.MutePlayer(player);
        }
    }

    public void playerGlobalVoiceUnmute(IPlayer player, float currentVoiceRange) {
        var unmuteRadioList = player.hasData("UNMUTE_RADIO_CHANNELS") ? (List<string>)player.getData("UNMUTE_RADIO_CHANNELS") : new List<string>();
        foreach(var channel in Channels) {
            if(channel.Key.StartsWith($"SPATIAL")) {
                if(channel.Key.Split("-")[1] == currentVoiceRange.ToString()) {
                    channel.Value.UnmutePlayer(player);
                }
            } else if(channel.Key.StartsWith($"PHONE")) {
                if(channel.Value.HasPlayer(player)) {
                    channel.Value.UnmutePlayer(player);
                }
            } else if(channel.Key.StartsWith($"RADIO")) {
                if(unmuteRadioList.Contains(channel.Key)) {
                    channel.Value.UnmutePlayer(player);
                }
            }
        }
    }

    public void changePlayerSpatialVoiceRange(IPlayer player, float newRange) {
        foreach(var globalChannel in SpatialChannels) {
            globalChannel.Value.MutePlayer(player);

            if(globalChannel.Key == newRange) {
                globalChannel.Value.UnmutePlayer(player);
            }
        }
    }

    public void joinRadioChannel(IPlayer player, string channelName, string displayName) {
        var name = $"RADIO-{channelName}";
        if(!Channels.ContainsKey(name)) {
            var channel = Alt.CreateVoiceChannel(false, float.MaxValue);

            if(channel == null) {
                if(!Config.IsDevServer) Logger.logError("Failed to create voice channel");

                return;
            }

            Channels[name] = channel;
            Channels[name].Filter = ChoiceVAPI.Hash("walkietalkie");
        }

        Channels[name].AddPlayer(player);
        muteInRadioChannel(player, channelName);
    }

    public void leaveRadioChannel(IPlayer player, string channelName) {
        var name = $"RADIO-{channelName}";

       if(Channels.TryGetValue(name, out IVoiceChannel value)) {
            value.RemovePlayer(player);

            if(value.PlayerCount == 0) {
                value.Destroy(); 
                Channels.Remove(name);
            }
        }
    }

    public void muteInRadioChannel(IPlayer player, string channelName) {
        var name = $"RADIO-{channelName}";
        if(Channels.TryGetValue(name, out IVoiceChannel value)) {
            value.MutePlayer(player);

            if(player.hasData("UNMUTE_RADIO_CHANNELS")) {
                var list = (List<string>)player.getData("UNMUTE_RADIO_CHANNELS");
                list.RemoveAll(s => s.Equals(name));
                player.setData("UNMUTE_RADIO_CHANNELS", list);
            } else {
                player.setData("UNMUTE_RADIO_CHANNELS", new List<string>());
            }
        }
    } 

    public void unmuteInRadioChannel(IPlayer player, string channelName) {
        var name = $"RADIO-{channelName}";
        if(Channels.TryGetValue(name, out IVoiceChannel value)) {
            value.UnmutePlayer(player);

            if(player.hasData("UNMUTE_RADIO_CHANNELS")) {
                var list = (List<string>)player.getData("UNMUTE_RADIO_CHANNELS");
                list.Add(name);
                player.setData("UNMUTE_RADIO_CHANNELS", list);
            } else {
                player.setData("UNMUTE_RADIO_CHANNELS", new List<string>{name});
            }
        }
    }


    public void startCall(IPlayer caller, IPlayer called) {
        var voiceCall = createVoiceChannel($"PHONE-{caller.getCharacterId()}", false, float.MaxValue);
        voiceCall.AddPlayer(caller);
        voiceCall.AddPlayer(called);
    }

    public void stopCall(IPlayer caller, IPlayer called) {
        if(Channels.TryGetValue($"PHONE-{caller.getCharacterId()}", out IVoiceChannel value)) {
            value.RemovePlayer(caller);
            value.RemovePlayer(called);
            value.Destroy();
            Channels.Remove($"PHONE-{caller.getCharacterId()}");
        }
    }

    private IVoiceChannel createVoiceChannel(string name, bool global, float range) {
        var channel = Alt.CreateVoiceChannel(global, range);

        if(channel == null) {
            if(!Config.IsDevServer) Logger.logError("Failed to create voice channel");

            return null;
        }

        Channels[name] = channel;

        return channel;
    }
}