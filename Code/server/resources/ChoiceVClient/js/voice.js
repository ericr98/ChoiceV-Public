import * as alt from 'alt';
import * as game from 'natives';

import {
    reactView
} from '/js/cef.js';

import { createMarker, deleteMarker, getMarker } from './marker.js';



//Voice

var voiceMarker = null;
var voiceTimeout = null;
alt.onServer("SHOW_VOICE_RANGE", (voiceRange) => {
    if (voiceMarker != null) {
        deleteMarker(voiceMarker);
        alt.clearTimeout(voiceTimeout);
    }

    var pos = alt.Player.local.pos;
    createMarker(-10, 1, pos.x, pos.y, pos.z - 2, 17, 177, 165, 50, voiceRange * 2, 1000);
    voiceMarker = -10;

    voiceTimeout = alt.setTimeout(() => {
        if (voiceMarker != null) {
            deleteMarker(voiceMarker);
            voiceMarker = null;
        }
    }, 1000);
});

alt.everyTick(() => {
    if (voiceMarker != null) {
        var pos = alt.Player.local.pos;
        var marker = getMarker(voiceMarker);
        marker.x = pos.x;
        marker.y = pos.y;
        marker.z = pos.z - 2;
    }
})

//Voice Filter

const walkieTalkieFilter = new alt.AudioFilter('walkietalkie')
walkieTalkieFilter.addBqfEffect(0, 1400, 0, 0.86, 0, 0, 1);
walkieTalkieFilter.addBqfEffect(1, 900, 0, 0.83, 0, 0, 2);
walkieTalkieFilter.addDistortionEffect(0, -2.95, -0.05, -0.08, 0.5, 3);

const megaphoneFilter = new alt.AudioFilter('megaphone');
megaphoneFilter.addBqfEffect(0, 2000, 0, 1, 0, 0, 1);
megaphoneFilter.addBqfEffect(1, 1000, 0, 0.86, 0, 0, 2);
megaphoneFilter.addDistortionEffect(0, -2.95, -0.05, -0.08, 0.25, 3);
megaphoneFilter.addCompressor2Effect(5, -15, 3, 10, 200, 4);
megaphoneFilter.audioCategory = 'altv_voice_megaphone';


//Yaca

var yacaSocket = null;
var yacaUpdateInterval = null;
var allPlayers = new Map();
var lastYacaHeartbeat = null;

class YacaPlayerData {
    constructor(clientId, range, isMuted, dimension) {
        this.clientId = clientId;
        this.range = range;
        this.isMuted = isMuted;
        this.dimension = dimension;
    }
}

var savedIngameName = null;
alt.onServer("YACA_CONNECT", (serverGuid, ingameName, ingameChannel, defaultChannel, ingameChannelPassword, excludedChannels, mufflingRange, unmuteDelay) => {
    if (yacaSocket !== null) {
        yacaSocket.close();
    }

    savedIngameName = ingameName;

    yacaSocket = new alt.WebSocketClient("ws://127.0.0.1:30125");

    yacaSocket.on("message", (msg) => {
        yacaHandleMessage(msg);
    });

    yacaSocket.on("open", () => {
        yacaSocket.send(JSON.stringify({
            base: { "request_type": "INIT" },
            server_guid: serverGuid,
            ingame_name: ingameName,
            ingame_channel: ingameChannel,
            default_channel: defaultChannel,
            ingame_channel_password: ingameChannelPassword,
            excluded_channels: excludedChannels,
            muffling_range: mufflingRange,
            build_type: 1, // 0 = Release, 1 = Debug
            unmute_delay: unmuteDelay,
            operation_mode: 1, // 0 = Normal, 1 = Whisper
        }));
    });

    yacaSocket.perMessageDeflate = true;
    yacaSocket.autoReconnect = true;
    yacaSocket.start();

    lastYacaHeartbeat = Date.now();
});

function yacaHandleMessage(msg) {
    let payload = JSON.parse(msg);

    if (payload.code === "OK") {
        if (payload.requestType === "JOIN") {
            alt.emitServer("YACA_JOIN", payload.message, savedIngameName);

            if (yacaUpdateInterval !== null) {
                alt.clearInterval(yacaUpdateInterval);
            }

            yacaUpdateInterval = alt.setInterval(yacaUpdate, 250);
        }
    } else if (payload.requestType === "INIT") {
        lastYacaHeartbeat = Date.now();
    } else if (payload.code === "TALK_STATE" || payload.code === "SOUND_STATE" || payload.code === "OTHER_TALK_STATE") {
        if (payload.code === "SOUND_STATE") {
            var data = JSON.parse(payload.message);
            alt.emitServer("YACA_SOUND_STATE", data.microphoneMuted, data.microphoneDisabled, data.soundMuted, data.soundDisabled);
        } else if (payload.code === "TALK_STATE" || payload.code === "OTHER_TALK_STATE") {
            yacaHandleTalkState(payload.code, payload.message);
        }
    } else if (payload.code === "MOVED_CHANNEL") {
        //TODO HANDLE CHANNEL MOVE
    } else if (payload.code === "HEARTBEAT") {
        lastYacaHeartbeat = Date.now();
    } else if (payload.code === "OUTDATED_VERSION") {
        alt.emitServer("YACA_OUTDATED_VERSION");
    }
}


function yacaUpdate() {
    var updatePlayers = [];
    var streamedPlayers = alt.Player.streamedIn;

    var localPlayer = alt.Player.local;
    var localVehicle = alt.Player.local.vehicle;
    var localData = allPlayers.get(localPlayer.getStreamSyncedMeta("CHARACTER_ID"));

    if (localData === undefined) return;

    if (localData.isMuted) showLipSync(localPlayer, false);

    if (lastYacaHeartbeat != null && Date.now() - lastYacaHeartbeat > 20000) {
        alt.emitServer("YACA_DISCONNECTED");
        return;
    }

    for (const player of streamedPlayers) {
        if (!player?.valid || player.getStreamSyncedMeta("CHARACTER_ID") === localPlayer.getStreamSyncedMeta("CHARACTER_ID")) continue;

        var playerData = allPlayers.get(player.getStreamSyncedMeta("CHARACTER_ID"));

        if (playerData === undefined) continue;

        //Only send players local chat in the same dimension
        if (playerData.dimension !== localData.dimension && !(player.dimension === 0 && localData.dimension < 0)) continue;

        let muffleIntensity = 0;
        if (game.getRoomKeyFromEntity(localPlayer) != game.getRoomKeyFromEntity(player) && !game.hasEntityClearLosToEntity(localPlayer, player, 17)) {
            muffleIntensity = 10; // 10 is the maximum intensity
        } else if (localVehicle != player.vehicle) {
            if (localVehicle?.valid && !hasVehicleOpening(localVehicle)) muffleIntensity += 3;
            if (player.vehicle?.valid && !hasVehicleOpening(player.vehicle)) muffleIntensity += 3;
        }

        updatePlayers.push({
            clientId: playerData.clientId,
            position: player.pos,
            direction: game.getEntityForwardVector(player),
            range: playerData.range,
            is_underwater: game.isPedSwimmingUnderWater(player),
            muffleIntensity: muffleIntensity,
            is_muted: playerData.isMuted,
        })
    }

    yacaSocket.send(JSON.stringify({
        base: { "request_type": "INGAME" },
        player: {
            player_direction: getYacaGameDirection(),
            player_position: localPlayer.pos,
            player_range: localData.range,
            player_is_underwater: game.isPedSwimmingUnderWater(localPlayer),
            player_is_muted: localData.isMuted,
            players_list: updatePlayers
        }
    }));
}

alt.onServer("YACA_ADD_PLAYER", (playerCharId, clientId, range, isMuted, dimension) => {
    allPlayers.set(playerCharId, new YacaPlayerData(clientId, range, isMuted, dimension));
});

alt.onServer("YACA_REMOVE_PLAYER", (playerCharId) => {
    allPlayers.delete(playerCharId);
});

alt.onServer("YACA_UPDATE_PLAYER", (playerCharId, key, value) => {
    var playerData = allPlayers.get(playerCharId);
    if (playerData === undefined) {
        alt.log(`Player ${playerCharId} not found in YACA_UPDATE_PLAYER for update ${key} with value ${value}`);
        return;
    }

    playerData[key] = value;
});


alt.onServer("YACA_UPDATE_COMM_DEVICE", (deviceType, channel, updateType, players, showChannelName, final) => {
    if (updateType == "MUTE") {
        var players = JSON.parse(players);

        if (!final) {
            var localData = allPlayers.get(alt.Player.local.getStreamSyncedMeta("CHARACTER_ID"));
            players = players.filter((el) => el !== localData.clientId);
        }

        yacaSocket.send(JSON.stringify({
            base: { "request_type": "INGAME" },
            comm_device_left: {
                comm_type: deviceType,
                client_ids: players,
                channel: channel,
            }
        }));
    } else if (updateType == "UNMUTE") {
        var players = JSON.parse(players);

        yacaSocket.send(JSON.stringify({
            base: { "request_type": "INGAME" },
            comm_device: {
                on: true,
                comm_type: deviceType,
                members: players,
                channel: channel,
            }
        }));
    }
});

alt.onServer("YACA_UPDATE_COMM_TYPE_SETTINGS", (deviceType, settingName, settingValue) => {
    var protocol = {
        comm_type: deviceType,
    };
    protocol[settingName] = settingValue;

    yacaSocket.send(JSON.stringify({
        base: { "request_type": "INGAME" },
        comm_device_settings: protocol
    }));
})

function yacaHandleTalkState(talkType, message) {
    var data = JSON.parse(message);

    if (talkType === "TALK_STATE") {
        var localData = allPlayers.get(alt.Player.local.getStreamSyncedMeta("CHARACTER_ID"));
        if (localData?.isMuted) return;

        showLipSync(alt.Player.local, data);
    } else if (talkType === "OTHER_TALK_STATE") {
        var remoteId = getCharIdByClientId(data.clientId);
        var otherPlayer = alt.Player.streamedIn.find((el) => el.getStreamSyncedMeta("CHARACTER_ID") === remoteId);

        if (!otherPlayer?.valid) return;

        showLipSync(otherPlayer, data.isTalking);
    }
}


//Utils

function getYacaGameDirection() {
    const rotVector = game.getGameplayCamRot(0);
    const num = rotVector.z * 0.0174532924;
    const num2 = rotVector.x * 0.0174532924;
    const num3 = Math.abs(Math.cos(num2));

    return new alt.Vector3(
        -Math.sin(num) * num3,
        Math.cos(num) * num3,
        game.getEntityForwardVector(alt.Player.local).z
    );
}

function hasVehicleOpening(vehicle) {
    if (!game.doesVehicleHaveRoof(vehicle)) return true;
    if (game.isVehicleAConvertible(vehicle, false) && game.getConvertibleRoofState(vehicle) !== 0) return true;
    if (!game.areAllVehicleWindowsIntact(vehicle)) return true;

    const doors = [];
    for (let i = 0; i < 6; i++) {
        if (i === 4 || !hasVehicleDoor(vehicle, i)) continue;
        doors.push(i);
    }

    if (doors.length === 0) return true;

    for (const door of doors) {
        if (game.getVehicleDoorAngleRatio(vehicle, door) > 0) return true;
        if (game.isVehicleDoorDamaged(vehicle, door)) return true;
    }

    for (let i = 0; i < 8 /* max windows */; i++) {
        if (hasVehicleWindow(vehicle, i) && !game.isVehicleWindowIntact(vehicle, i)) {
            return true;
        }
    }

    return false;
}

function hasVehicleWindow(vehicle, windowId) {
    switch (windowId) {
        case 0:
            return game.getEntityBoneIndexByName(vehicle, "window_lf") !== -1;
        case 1:
            return game.getEntityBoneIndexByName(vehicle, "window_rf") !== -1;
        case 2:
            return game.getEntityBoneIndexByName(vehicle, "window_lr") !== -1;
        case 3:
            return game.getEntityBoneIndexByName(vehicle, "window_rr") !== -1;
        default:
            return false;
    }
}

function hasVehicleDoor(vehicle, doorId) {
    switch (doorId) {
        case 0:
            return game.getEntityBoneIndexByName(vehicle, "door_dside_f") !== -1;
        case 1:
            return game.getEntityBoneIndexByName(vehicle, "door_pside_f") !== -1;
        case 2:
            return game.getEntityBoneIndexByName(vehicle, "door_dside_r") !== -1;
        case 3:
            return game.getEntityBoneIndexByName(vehicle, "door_pside_r") !== -1;
        case 4:
            return game.getEntityBoneIndexByName(vehicle, "bonnet") !== -1;
        case 5:
            return game.getEntityBoneIndexByName(vehicle, "boot") !== -1;
        default:
            return false;
    }
}

function showLipSync(player, isTalking) {
    var anim = { dict: "mic_chatter", name: "mp_facial" };

    if (!isTalking) {
        anim = { dict: "mood_normal_1", name: "facials@gen_male@variations@normal" };
    }

    game.playFacialAnim(player, anim.dict, anim.name);
}

function getCharIdByClientId(clientId) {
    for (const [key, value] of allPlayers.entries()) {
        if (value.clientId.toString() === clientId.toString()) return key;
    }

    return undefined;
}