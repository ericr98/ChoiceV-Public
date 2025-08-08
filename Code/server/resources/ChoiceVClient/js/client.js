/// <reference path="../types/alt.d.ts" />
/// <reference path="../types/altv.d.ts" />

import * as alt from 'alt-client'
import * as game from 'natives'; 

import * as interaction from '/js/interaction.js';
import * as math from '/js/math.js';
import * as spotCreator from '/js/spotCreator.js';
import * as cef from '/js/cef.js';
import * as charCreator from '/js/charCreator.js';
import * as playerEx from '/js/player.js';
import * as camera from '/js/camera.js';
import * as connection from '/js/connection.js';
import * as weapons from '/js/weapons.js';
import * as chat from '/js/chat.js';
import * as object from '/js/objects.js';
import * as entity from '/js/entity.js';
import * as areaCreator from '/js/areaCreator.js';
import * as peds from '/js/peds.js';
import * as syncer from '/js/syncer.js';
import * as blip from '/js/blip.js';
import * as marker from '/js/marker.js';
import * as debug from '/js/debug.js';
import * as vehicle from '/js/vehicle.js';
import * as fire from '/js/fire.js';
import * as fishing from '/js/fishing.js';
import * as island from '/js/island.js';
import * as cams from '/js/cams.js';
import * as jsencrypt from '/js/jsencrypt/JSEncrypt.js';
import * as sound from '/js/sound.js';
import * as voice from '/js/voice.js';


var clientSideLogging = false;
alt.onServer("TOGGLE_CLIENT_LOGGING", (toggle) => {
    clientSideLogging = toggle;
});

export function log(category, msg) {
    if(clientSideLogging) { 
        var date = new Date();
        var time = date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds();

        alt.log(`[${category}] ${msg}`);	
        alt.emitServer("CLIENT_LOG", `[${time}] [${category}] ${msg}`);
    }
}

game.replaceHudColourWithRgba(143, 204, 138, 37, 255);
game.replaceHudColourWithRgba(116, 204, 138, 37, 255);

function callServerKeyEvent(key) {
	alt.emitServer("KEY_" + key);
}

function callServerKeyToggleEvent(key, isPressed) {
	alt.emitServer("KEY_TOGGLE_" + key, isPressed);
}

export var currentGrids = [];

alt.onServer("CHANGE_GRID", (newGrids) => {
    currentGrids = newGrids;
});

//TICK COUNTER

alt.on('connectionComplete', () => {
    alt.setMsPerGameMinute(1000 * 60);
});

setInterval(() => {
    onTick();
}, 500);

setInterval(() => {
    onLongerTick();
}, 1500);

setInterval(() => {
    onVeryLongTick();
}, 3000);

function onTick() {
    syncer.onTick();
    peds.onTick();
    object.onTick();
    marker.onTick();

    var player = alt.Player.local;
    game.setPedConfigFlag(player.scriptID, 184, 1);
}

function onLongerTick() {
    peds.onLongerTick(); 
}

function onVeryLongTick() {    
    playerEx.redoOverlayTick();

    var pos = alt.Player.local.pos;

    var ray = game.startExpensiveSynchronousShapeTestLosProbe(pos.x, pos.y, pos.z + 1, pos.x, pos.y, pos.z - 20, 1, alt.Player.local.scriptID, 7);
    var hitData = game.getShapeTestResultIncludingMaterial(ray, null, null, null, null, null);

    alt.emitServer("UPDATE_GROUND_MATERIAL", hitData[4]);

    game.invalidateCinematicVehicleIdleMode();
    game.invalidateIdleCam();
}

export function setLock(toggle) {
    lock = toggle;
}

export function isLock() {
    return lock;
}

var keySendList = [
    // 72,  // H (Hands Up)
    // 77,  // M (Opens self)
    // 89,  // Y (Engine On)
    // 33,  // PageUp (SmartphoneUp)
    // 34,  // PagDown (SmartphoneDown)
    // 85,  // U Vehicle indoor interaction
    // 73,  // I Open inventory
    // 74,  // J Open/close vehicle window
    // 75,  // Trunk
    // 69,  // E Key Interaction
    // 90,  // Z Special Key
    // 79,  // O For sitting down
    // 192, // Ö Buckle up
    // 163, // # Speed Limiter
	// 116, // F5 SupportMenu
    // 107, // + Voice Range
    // 186, // Ü for Radio Menu
    // 37,  // ArrowLeft for left Indicator
    // 39,  // ArrowRight for right Indicator
    // 40,  // ArrowDown for all Indicators
    // 96, // NumPad 0
    // 66, //B (Keine ahnung)
    // 76,  // L Key to unlock/lock vehicle
];
var keyToogleSendList = [
    // 187, //Volume Up
    // 226, //Activate Radio
];

var keyIgnoredLockList = [
     //34, // PagDown (SmartphoneDown)
]

var animKeyList = []

alt.onServer("SET_KEY_LIST", (keySendListJSON, keyToggleSendListJSON, keyIgnoresBusyListJSON) => {
    keySendList = JSON.parse(keySendListJSON);
    keyToogleSendList = JSON.parse(keyToggleSendListJSON);
    keyIgnoredLockList = JSON.parse(keyIgnoresBusyListJSON);
});

alt.onServer("SET_ANIMATION_KEYS", (animKeyListJSON) => {
    animKeyList = JSON.parse(animKeyListJSON);
});

alt.onServer("ALLOW_ALL_KEYS_ONCE", () => {
    allowAllKeys = true;

    lastKeyPress = Date.now();
});

var lock = false;

var allowAllKeys = false;
var lastKeyPress = null;

//KEYS
alt.on('keydown', (key) => {
    if (lock && !keyIgnoredLockList.includes(key)) {
        return;
    }

    if (chat.opened) {
        return;
    }

    if(keyToogleSendList.includes(key)) {
        callServerKeyToggleEvent(key, true);
    }
});

alt.on('keyup', (key) => {
    if(lastKeyPress != null && Date.now() - lastKeyPress < 350) {
        return;
    }

    if(allowAllKeys) {
        allowAllKeys = false;
        callServerKeyEvent(key);
        return;
    }

    if (cef.otherCefOpened || cef.editorOpen) {
        return;
    }

    if (lock && !keyIgnoredLockList.includes(key)) {
        return;
    }

    if (chat.opened) {
        return;
    }

    if (key === 0x71) {  // F2 Key shows curosr
        if (cef.cursorShown){
            cef.hideCursor();
            alt.toggleGameControls(true);
        } else {
            cef.showCursor();
            alt.toggleGameControls(false);
        }
    
    }
    
    if (keySendList.includes(key) || keyIgnoredLockList.includes(key)) {
        callServerKeyEvent(key);
        lastKeyPress = Date.now();
    }
    
    if(keyToogleSendList.includes(key)) {
        callServerKeyToggleEvent(key, false);
        lastKeyPress = Date.now();
    }

    if(animKeyList.includes(key)) {
        alt.emitServer("ANIMATION_KEY_PRESSED", key);
    }
});

var labelIntervals = [];
//Labels
alt.onServer('TEXT_LABEL_ON_PLAYER', (player, msg, time) => {
    if (player !== null) {
        let id = player.id;

        labelIntervals[player.id] = alt.setInterval(() => {
            if (!player || !player.valid) {
                alt.clearInterval(labelIntervals[id]);
                return;
            }
            drawText3d(
                player,
                `${msg}`,
                0.35,
                4,
                194,
                162,
                218,
                255,
                true,
                false
            );
        }, 0);

        if(time != -1) {
            alt.setTimeout(() => {
                alt.clearInterval(labelIntervals[player.id]);
            }, time);
        }
    }
});

alt.onServer("STOP_TEXT_LABELS", () => {
    labelIntervals.forEach((el) => {
        alt.clearInterval(el);
    });
});

export function drawText3d(
    player,
    msg,
    scale,
    fontType,
    r,
    g,
    b,
    a,
    useOutline = true,
    useDropShadow = true,
    layer = 0
) {
    let hex = msg.match('{.*}');
    if (hex) {
        const rgb = hexToRgb(hex[0].replace('{', '').replace('}', ''));
        r = rgb[0];
        g = rgb[1];
        b = rgb[2];
        msg = msg.replace(hex[0], '');
    }
    const localPlayer = player;
    const playerPos = localPlayer.pos;
    const entity = localPlayer.vehicle ? localPlayer.vehicle.scriptID : localPlayer.scriptID;
    const vector = game.getEntityVelocity(entity);
    const frameTime = game.getFrameTime();
    game.setDrawOrigin(playerPos.x + (vector.x * frameTime), playerPos.y + (vector.y * frameTime), playerPos.z + (vector.z * frameTime) + 1, 0);
    game.beginTextCommandDisplayText('STRING');
    game.addTextComponentSubstringPlayerName(msg);
    game.setTextFont(fontType);
    game.setTextScale(1, scale);
    game.setTextWrap(0.0, 1.0);
    game.setTextCentre(true);
    game.setTextColour(r, g, b, a);

    if (useOutline) game.setTextOutline();

    if (useDropShadow) game.setTextDropShadow();

    game.endTextCommandDisplayText(0, 0, 0);
    game.clearDrawOrigin();
}

//IPLS

alt.onServer('REQUEST_IPL', (ipl) => {
    alt.requestIpl(ipl);
});

alt.onServer('REMOVE_IPL', (ipl) => {
    alt.removeIpl(ipl);
});

alt.onServer('FREEZE', () => {
    game.freezeEntityPosition(alt.Player.local.scriptID, true);
});

//TESTING

alt.onServer('SET_HAIR_COLOR', (color) => {
    var player = game.playerPedId();
    game.setPedHairTint(player, color, color);
});

alt.on('consoleCommand', (cmd, arg) => {
    alt.log(cmd, arg);
});

alt.onServer('SP', () => {
    game.loadSpDlcMaps();
});

alt.onServer('MP', () => {
    game.loadMpDlcMaps();
});

// Start Disable Environment Sounds
alt.everyTick(() => {
    game.startAudioScene('CHARACTER_CHANGE_IN_SKY_SCENE');
    game.startAudioScene('FBI_HEIST_H5_MUTE_AMBIENCE_SCENE'); // Used to stop police sound in town
    game.startAudioScene('DLC_MPHEIST_TRANSITION_TO_APT_FADE_IN_RADIO_SCENE');
    game.clearAmbientZoneState('AZ_COUNTRYSIDE_PRISON_01_ANNOUNCER_GENERAL', true); // Turn off prison sound
    game.clearAmbientZoneState('AZ_COUNTRYSIDE_PRISON_01_ANNOUNCER_WARNING', true); // Turn off prison sound
    game.clearAmbientZoneState('AZ_COUNTRYSIDE_PRISON_01_ANNOUNCER_ALARM', true); // Turn off prison sound
    game.clearAmbientZoneState('AZ_DISTANT_SASQUATCH', false);
});

alt.on('enteredVehicle', (vehicle) => {
    game.setAudioFlag('DisableFlightMusic', true);
});

alt.on('leftVehicle', (vehicle) => {
    game.setAudioFlag('DisableFlightMusic', true);
});
// Stop Disable Environment Sounds
