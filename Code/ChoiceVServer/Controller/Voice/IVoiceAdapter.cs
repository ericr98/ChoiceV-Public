using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.LockerSystem.Model;

namespace ChoiceVServer.Controller.Voice;

public interface IVoiceAdapter {
    public void onPlayerConnect(IPlayer player);
    public void onPlayerDisconnect(IPlayer player);

    //Global Voice
    public void playerGlobalVoiceMute(IPlayer player);
    public void playerGlobalVoiceUnmute(IPlayer player, float currentVoiceRange);
    public void changePlayerSpatialVoiceRange(IPlayer player, float newRange);

    public void playerChangeDimension(IPlayer player, int fromDimension, int toDimension);

    //Radio
    public void joinRadioChannel(IPlayer player, string channelName, string displayName);
    public void leaveRadioChannel(IPlayer player, string channelName);
    public void muteInRadioChannel(IPlayer player, string channelName);
    public void unmuteInRadioChannel(IPlayer player, string channelName);

    //Phone Calls
    public void startCall(IPlayer caller, IPlayer called);
    public void stopCall(IPlayer caller, IPlayer called);
}