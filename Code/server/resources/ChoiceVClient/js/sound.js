import * as alt from 'alt-client';
import * as game from 'natives';

import { getDistance } from './math.js';

import {
    reactView
} from '/js/cef.js';

import { createMarker, deleteMarker, getMarker } from './marker.js';

const radio = alt.AudioCategory.getForName("radio");
const category = alt.AudioCategory.getForName("video_editor_weapons_guns_bullet_impacts"); 

category.volume = 30;
category.distanceRolloffScale = radio.distanceRolloffScale;
category.plateauRolloffScale = radio.plateauRolloffScale;
category.occlusionDamping = radio.occlusionDamping;
category.environmentalFilterDamping = radio.environmentalFilterDamping;
category.sourceReverbDamping = radio.sourceReverbDamping;
category.distanceReverbDamping = radio.distanceReverbDamping;
category.interiorReverbDamping = radio.interiorReverbDamping;
category.environmentalLoudness = radio.environmentalLoudness;
category.underwaterWetLevel = radio.underwaterWetLevel;
category.stonedWetLevel = radio.stonedWetLevel;
category.pitch = radio.pitch;
category.lowPassFilterCutoff = radio.lowPassFilterCutoff;
category.highPassFilterCutoff = radio.highPassFilterCutoff;


var loopedAudios = [];
alt.onServer('PLAY_SOUND_AT_POS', (source, position, volume, identifier, looped) => {
	const output = new alt.AudioOutputWorld(position, alt.hash("video_editor_weapons_guns_bullet_impacts"));
    const audio = new alt.Audio(source, volume);

    if(looped) {
        audio.looped = looped;
        loopedAudios.push([identifier, audio]);
    }

    audio.on("streamEnded", () => {
        alt.setTimeout(() => {
            audio.destroy();
        }, 1000);
    });

    audio.addOutput(output);
    audio.play();
});

alt.onServer('STOP_SOUND', (identifier) => {
    loopedAudios.forEach((audio) => {
        if(audio[0] == identifier) {
            if(audio[1].isValid)
            audio[1].pause();
            audio[1].destroy();
        }
    });

    loopedAudios = loopedAudios.filter((audio) => {
        return audio[0] != identifier;
    });
});

alt.onServer('PLAY_SOUND', (source, volume) => {
	const output = new alt.AudioOutputAttached(alt.Player.local, alt.hash("video_editor_weapons_guns_bullet_impacts"));
    const audio = new alt.Audio(source, volume);

    audio.on("streamEnded", () => {
        alt.setTimeout(() => {
            audio.destroy();
        }, 1000);
    });

    audio.addOutput(output);
    audio.play();
});


var distanceSounds = [];

alt.onServer('CREATE_DISTANCE_SOUND_EVENT', (soundId, distance, position) => {
    distanceSounds.push({ 
        soundId: soundId,
        position: position,
        distance: distance
    });
});

alt.onServer('DELETE_DISTANCE_SOUND_EVENT', (id) => {
    distanceSounds = distanceSounds.filter((sound) => {
        return sound.id != id;
    });
});

var audioOrigin = "CAMERA";
alt.onServer("SET_AUDIO_ORIGIN", (type) => {
    audioOrigin = type;
});

var counter = 0;
alt.everyTick(() => {
    if(reactView != null)  {
        var forwardVec = null;

        if(audioOrigin == "CAMERA") {
            var heading = game.getGameplayCamRot(0).z;
            var yaw = heading * Math.PI / 180.0;
            forwardVec = { x: Math.cos(yaw + Math.PI / 2), y: Math.sin(yaw + Math.PI / 2) };
        } else {
            forwardVec = game.getEntityForwardVector(alt.Player.local);
        }

        var data = {
            Event: "UPDATE_PLAYER_SOUND_POSITION",
            position: alt.Player.local.pos,
            forwardVec: forwardVec
        }
    
        reactView.emit('CEF_EVENT', data);
    }

    counter++;
    if(counter > 50) {
        counter = 0;
        distanceSounds.forEach((sound) => {
            if(getDistance(sound.position.x, sound.position.y, alt.Player.local.pos.x, alt.Player.local.pos.y) <= sound.distance * 1.1) {
                var data = {
                    Event: "UPDATE_PLAYER_SOUND_MUFFLED",
                    soundId: sound.soundId,
                    isMuffled: !isPosVisible(sound.position, alt.Player.local.pos)
                }

                reactView.emit('CEF_EVENT', data);
            }
        });
    }
});


function isPosVisible(from, to) {
    var zOffsets = [
        [0, 0, 0],
        [0, 0, 1],
        [0, 1, 0],
        [0, 1, 1],
        [1, 0, 0],
        [1, 0, 1],
        [1, 1, 0],
        [1, 1, 1],
    ];

    var can = false;
    zOffsets.forEach((zOffset) => {
        var fromPos = { x: from.x + zOffset[0] * 1.5, y: from.y + zOffset[1] * 1.5, z: from.z + zOffset[2] * 1.5}
        var toPos = { x: to.x + zOffset[0], y: to.y + zOffset[1], z: to.z + zOffset[2] }

        let ray = game.startExpensiveSynchronousShapeTestLosProbe(
            fromPos.x, fromPos.y, fromPos.z, toPos.x, toPos.y, toPos.z,
            1,
            alt.Player.local.scriptID, 7);

        let hitData = game.getShapeTestResult(ray, null, null, null, null);
        if(!hitData[1]) {
            can = true;
            return;
        }
    });

    return can;
}