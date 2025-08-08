using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Voice;
using ChoiceVServer.Controller.Voice.YacaModel;
using ChoiceVServer.EventSystem;
using System.Collections.Concurrent;
using System.Linq;

public enum YacaFilter {
    Radio,
    MegaPhone,
    Phone,
    PhoneSpeaker,
    Intercom,
    PhoneHistorical,
}

public enum YacaStereoMode {
    MonoLeft,
    MonoRight,
    Stereo,
}

public enum YacaCommDeviceType {
    Sender,
    Receiver,
    Transceiver,
}

public class YacaAdapter : IVoiceAdapter {
    private List<float> VoiceRanges;

    private const int MUFFLING_RANGE = 2;
    private const int UNMUTE_DELAY = 400;
    private static List<int> EXCLUDDED_CHANNELS = Config.TeamspeakExcluddedChannels.Split(',').Select(int.Parse).ToList();

    private static int ChannelIdCounter = 1;

    private static ConcurrentDictionary<int, YacaPlayer> Players = new();
    private static ConcurrentDictionary<string, YacaChannel> RadioChannels = new();
    private static ConcurrentDictionary<IPlayer, YacaChannel> PhoneChannels = new();

    //TODO: Make Button fo opening radio menu
    //TODO: Funkgerät leiser machen können
    
    public YacaAdapter(List<float> ranges) {
        VoiceRanges = ranges;

        EventController.addEvent("YACA_JOIN", onClientJoin);
        EventController.addEvent("YACA_OUTDATED_VERSION", onYacaOutdatedVersion);
        EventController.addEvent("YACA_DISCONNECTED", onYacaDisconnectd);
        EventController.addEvent("YACA_SOUND_STATE", onYacaSoundState);
    }

    private bool onYacaOutdatedVersion(IPlayer player, string eventname, object[] args) {
        player.Kick("Deine Voice Plugin Version (Yaca) ist veraltet! Bitte lade die vorgegebene Version herunter! Im Discord findest du den Link dazu!");
        return true;
    }

    private bool onYacaDisconnectd(IPlayer player, string eventname, object[] args) {
        player.Kick("Du wurdest vom Voicesystem (Yaca) getrennt!");
        return true;
    }

    private bool onYacaSoundState(IPlayer player, string eventname, object[] args) {
        var micMuted = bool.Parse(args[0].ToString());
        var micDisabled = bool.Parse(args[1].ToString());
        var speakerMuted = bool.Parse(args[2].ToString());
        var speakerDisabled = bool.Parse(args[3].ToString());
            
       VoiceController.displayVoiceSolutionMute(player, micMuted || micDisabled || speakerMuted, "teamspeak");
       
       return true;
    }

    private bool onClientJoin(IPlayer player, string eventname, object[] args) {
        var clientId = int.Parse(args[0].ToString());
        var ingameName = args[1].ToString();

        YacaPlayer newPlayer;
        if(!Players.ContainsKey(player.getCharacterId())) {
            newPlayer = new YacaPlayer(player, clientId, ingameName, VoiceRanges[0]);
            Players[player.getCharacterId()] = newPlayer;
        } else {
            newPlayer = Players[player.getCharacterId()];
            newPlayer.VoicePlayer = player;
            newPlayer.ClientId = clientId;
            newPlayer.IngameName = ingameName;
            
        }
        
        foreach(var p in ChoiceVAPI.GetAllPlayers()) {
            p.emitClientEvent("YACA_ADD_PLAYER", player.getCharacterId(), newPlayer.ClientId, newPlayer.Range, newPlayer.IsMuted, newPlayer.Dimension);
        } 
            
        foreach(var p in Players.Values) {
            player.emitClientEvent("YACA_ADD_PLAYER", p.VoicePlayer.getCharacterId(), p.ClientId, p.Range, p.IsMuted, p.Dimension);
        }

        return true;
    }

    public void onPlayerConnect(IPlayer player) {
        player.emitClientEvent("YACA_CONNECT", Config.TeamspeakUUID, ChoiceVAPI.randomString(30), Config.TeamspeakIngameChannelId, Config.TeamspeakDefaultChannel, Config.TeamspeakIngameChannelPassword, EXCLUDDED_CHANNELS, MUFFLING_RANGE, UNMUTE_DELAY);
    }

    public void onPlayerDisconnect(IPlayer player) {
        foreach(var channel in RadioChannels.Values) {
            if(channel.hasPlayer(Players[player.getCharacterId()])) {
                channel.removePlayer(Players[player.getCharacterId()]);
            }
        }
        
        foreach(var channel in PhoneChannels.Values) {
            if(channel.hasPlayer(Players[player.getCharacterId()])) {
                channel.removePlayer(Players[player.getCharacterId()]);
            }
        }
        
        if(Players.ContainsKey(player.getCharacterId())) {
            Players[player.getCharacterId()].removeFromPlayers();
            Players.Remove(player.getCharacterId());
            
            Players.Values.ForEach(p => p.VoicePlayer.emitClientEvent("YACA_REMOVE_PLAYER", player.getCharacterId()));
        }
    }

    public void joinRadioChannel(IPlayer player, string channelName, string displayName) {
        if(!RadioChannels.ContainsKey(channelName)) {
            RadioChannels[channelName] = new YacaChannel(ChannelIdCounter++, YacaFilter.Radio)
                .withShowOtherPlayersSending(displayName);
        }

        RadioChannels[channelName].addPlayer(Players[player.getCharacterId()], true);
    }

    public void leaveRadioChannel(IPlayer player, string channelName) {
        if(RadioChannels.ContainsKey(channelName)) {
            RadioChannels[channelName].removePlayer(Players[player.getCharacterId()]);
        }
        
        if(RadioChannels[channelName].getPlayerCount() == 0) {
            RadioChannels[channelName].remove();
            RadioChannels.Remove(channelName);
        }
    }

    public void muteInRadioChannel(IPlayer player, string channelName) {
        if(RadioChannels.ContainsKey(channelName)) {
            RadioChannels[channelName].mutePlayer(Players[player.getCharacterId()]);
        }
    }

    public void unmuteInRadioChannel(IPlayer player, string channelName) {
        if(RadioChannels.ContainsKey(channelName)) {
            RadioChannels[channelName].unmutePlayer(Players[player.getCharacterId()]);
        }
    }

    public void playerChangeDimension(IPlayer player, int fromDimension, int toDimension) {
        if(Players.ContainsKey(player.getCharacterId())) {
            Players[player.getCharacterId()].setDimension(toDimension);
        }
    }

    public void playerGlobalVoiceMute(IPlayer player) {
        if(Players.ContainsKey(player.getCharacterId())) {
            Players[player.getCharacterId()].setMute(true);
        }
    }

    public void playerGlobalVoiceUnmute(IPlayer player, float currentVoiceRange) {
        if(Players.ContainsKey(player.getCharacterId())) {
            Players[player.getCharacterId()].setMute(false);
            Players[player.getCharacterId()].setRange(currentVoiceRange);
        }
    }

    public void changePlayerSpatialVoiceRange(IPlayer player, float newRange) {
        if(Players.ContainsKey(player.getCharacterId())) {
            Players[player.getCharacterId()].setRange(newRange);
        }
    }

    public void startCall(IPlayer caller, IPlayer called) {
        if(!PhoneChannels.ContainsKey(caller) && !PhoneChannels.ContainsKey(called)) {
            PhoneChannels[caller] = new YacaChannel(ChannelIdCounter++, YacaFilter.Phone);
        }

        PhoneChannels[caller].addPlayer(Players[caller.getCharacterId()], false);
        PhoneChannels[caller].addPlayer(Players[called.getCharacterId()], false); 
        
        PhoneChannels[caller].unmutePlayer(Players[called.getCharacterId()]);
        PhoneChannels[caller].unmutePlayer(Players[caller.getCharacterId()]);
    }

    public void stopCall(IPlayer caller, IPlayer called) {
        if(PhoneChannels.TryGetValue(caller, out var channel)) {
            channel.remove();
            PhoneChannels.Remove(caller);
        }
    }


    public static string getValueForYacaFilter(YacaFilter filter) {
        switch(filter) {
            case YacaFilter.Radio: return "RADIO";
            case YacaFilter.MegaPhone: return "MEGAPHONE";
            case YacaFilter.Phone: return "PHONE";
            case YacaFilter.PhoneSpeaker: return "PHONE_SPEAKER";
            case YacaFilter.Intercom: return "INTERCOM";
            case YacaFilter.PhoneHistorical: return "PHONE_HISTORICAL";
            default: return "RADIO";
        }
    }

    public static string getValueForYacaStereoMode(YacaStereoMode mode) {
        switch(mode) {
            case YacaStereoMode.MonoLeft: return "MONO_LEFT";
            case YacaStereoMode.MonoRight: return "MONO_RIGHT";
            case YacaStereoMode.Stereo: return "STEREO";
            default: return "STEREO";
        }
    }

    public static int getValueForYacaCommDeviceType(YacaCommDeviceType type) {
        switch(type) {
            case YacaCommDeviceType.Sender: return 0;
            case YacaCommDeviceType.Receiver: return 1;
            case YacaCommDeviceType.Transceiver: return 2;
            default: return 0;
        }
    }
}