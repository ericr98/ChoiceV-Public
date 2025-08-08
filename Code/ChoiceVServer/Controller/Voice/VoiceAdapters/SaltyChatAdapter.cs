using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.Voice;

public class SaltyChatAdapter : IVoiceAdapter {

    public SaltyChatAdapter(List<float> ranges) {

    }

    public void onPlayerConnect(IPlayer player) {
        Alt.Emit("SaltyChat:EnablePlayer", player);
    }

    public void onPlayerDisconnect(IPlayer player) { }

    public void joinRadioChannel(IPlayer player, string channelName, string displayName) {
        Alt.Emit("SaltyChat:JoinRadioChannel", player, channelName, true);
    }

    public void leaveRadioChannel(IPlayer player, string channelName) {
        Alt.Emit("SaltyChat:JoinRadioChannel", player, channelName, false);
    }

    public void muteInRadioChannel(IPlayer player, string channelName) {
        //TODO Send to client that this player is muted in this channel
    }

    public void unmuteInRadioChannel(IPlayer player, string channelName) {
        //TODO Send to client that this player is unmuted in this channel
    }


    public void playerChangeDimension(IPlayer player, int fromDimension, int toDimension) {
        //Maybe needed?
    }

    public void playerGlobalVoiceMute(IPlayer player) {
        Alt.Emit("SaltyChat:SetPlayerAlive", player, false);   
    }

    public void playerGlobalVoiceUnmute(IPlayer player, float range) {
        Alt.Emit("SaltyChat:SetPlayerAlive", player, true);   
    }

    public void startCall(IPlayer caller, IPlayer called) {
        Alt.Emit("SaltyChat:StartCall", caller, called);
    }

    public void stopCall(IPlayer caller, IPlayer called) {
        Alt.Emit("SaltyChat:EndCall", caller, called);
    }

    public void playerGlobalVoiceUnmute(IPlayer player) {
        throw new System.NotImplementedException();
    }

    public void changePlayerSpatialVoiceRange(IPlayer player, float newRange) {
        throw new System.NotImplementedException();
    }
}