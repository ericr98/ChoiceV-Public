import { PluginCommand } from "./PluginCommand.js";
import { Command } from "../../Enum/SaltyChat/Command.js";
import { PlayerState } from "./PlayerState.js";
import { SaltyVoice } from "../../app.js";
export class VoiceClient {
    player;
    teamSpeakName;
    voiceRange;
    isAlive;
    lastPosition;
    distanceCulled;
    constructor(player, teamSpeakName, voiceRange, isAlive, lastPosition) {
        this.player = player;
        this.teamSpeakName = teamSpeakName;
        this.voiceRange = voiceRange;
        this.isAlive = isAlive;
        this.lastPosition = lastPosition;
    }
    SendPlayerStateUpdate() {
        SaltyVoice.GetInstance().executeCommand(new PluginCommand(Command.playerStateUpdate, new PlayerState(this.teamSpeakName, this.lastPosition, this.voiceRange, this.isAlive)));
    }
}
