/// <reference path="../types/alt-client.d.ts" />
/// <reference path="../types/alt-shared.d.ts" />
/// <reference path="../types/natives.d.ts" />
import * as alt from 'alt-client'; 
import * as game from 'natives';
import * as NativeUI from "./NativeUI/NativeUi.js";

import { setLock, log } from './client.js';
import { hideHud } from './camera.js';

import { toInt32, degreesToRadians, forwardVectorFromRotation } from './math.js';
import { loadAnimDic, loadClipSet } from './entity.js';

import { interactableObjects } from './interaction.js';

var slowWalk = false;
var noAttack = false;
var noLook = false;

alt.on('connectionComplete', () => {
    // //Max stats of you character
    alt.setStat("stamina", 100);
    alt.setStat("strength", 100);
    alt.setStat("lung_capacity", 100);
    alt.setStat("wheelie_ability", 100);
    alt.setStat("flying_ability", 100);
    alt.setStat("shooting_ability", 100);
    alt.setStat("stealth_ability", 100);

     alt.setInterval(() => {
        game.setPedConfigFlag(alt.Player.local.scriptID, 429, true); // disable start engine
        game.setPedConfigFlag(alt.Player.local.scriptID, 241, true); // disable stop engine
        game.setPedConfigFlag(alt.Player.local.scriptID, 184, true); // disable auto shuffle
        game.setPedConfigFlag(alt.Player.local.scriptID, 35, false); // disable auto helmet
        game.setPedConfigFlag(alt.Player.local.scriptID, 248, false); //Disable Vehicle Random Animations
    }, 100);

    alt.everyTick(() => {
        game.extendWorldBoundaryForPlayer(-100000, -100000, 1000);
        game.extendWorldBoundaryForPlayer(100000, 100000, 1000);
    });
});


alt.onServer("SET_WAYPOINT", (x, y) => {
    game.setNewWaypoint(x, y);
});

alt.onServer('KICK_MESSAGE', (reason) => {
    alt.log("Du wurdest gekickt wegen: " + reason);
});

alt.onServer("TASK_CLIMB_LADDER", () => {
    game.taskClimbLadder(alt.Player.local.scriptID, 1);
});

//Movement
var movementDisabled = false;
alt.onServer("DISABLE_MOVEMENT", (toggle) => {
    movementDisabled = toggle;
});

alt.everyTick(() => {
    if(!movementDisabled) return;

    game.disableControlAction(0, 32, true) // move (w)
    game.disableControlAction(0, 34, true) // move (a)
    game.disableControlAction(0, 33, true) // move (s)
    game.disableControlAction(0, 35, true) //move (d)
});

const WaitUntil = (cb, ...args)=>{
    return new Promise((resolve, _)=>{
        const et = alt.everyTick(()=>{
            if (!cb(...args)) return;
            alt.clearEveryTick(et);
            resolve();
        });
    });
};

//Cutscenes
alt.onServer('PLAYER_INTRO_CUTSCENE', async (gender) => {
    setLock(true);

    let isGenderFemale = gender == "F";
    if(isGenderFemale) {   
        game.requestCutsceneWithPlaybackList("mp_intro_concat", 103, 8);
    } else {
        game.requestCutsceneWithPlaybackList("mp_intro_concat", 31, 8);
    }

    await WaitUntil(game.hasThisCutsceneLoaded, "mp_intro_concat");

    let hideHudInterval = alt.everyTick(() => hideHud());

    game.freezeEntityPosition(alt.Player.local, true);
    
    if(isGenderFemale) {
        generatePed("MP_Female_Character", "MP_Male_Character", alt.Player.local);
    } else {
        generatePed("MP_Male_Character", "MP_Female_Character", alt.Player.local);
    }

    //TODO Maybe think about adding the other peds https://github.com/Doublox/CutScene/blob/main/cutscene/cutscene.lua
    for(let i = 1; i <= 7; i++) {
        game.registerEntityForCutscene(0, "MP_Plane_Passenger_" + i, 3, alt.hash("mp_f_freemode_01"), 0);
        game.registerEntityForCutscene(0, "MP_Plane_Passenger_" + i, 3, alt.hash("mp_m_freemode_01"), 0);
    }

    game.setEntityCoords(alt.Player.local, -1161, -1625, -4.3, true, true, true, false);

    let view = new alt.WebView('http://resource/cef/NEEDED/welcome/welcome.html');
    view.isVisible = false;

    game.startCutscene(0);
    await WaitUntil(game.isCutsceneActive);

    let stopLoop = false;
    while(!stopLoop) {
        await new Promise(resolve => alt.setTimeout(resolve, 100));

        if (game.getCutsceneTime() >= 23000) {
            view.isVisible = true;
        }

        if (game.getCutsceneSectionPlaying() == 3) {
            alt.emitServer('PLAYER_INTRO_CUTSCENE_FINISHED');
            stopLoop = true;

            alt.setTimeout(() => {
                game.stopCutsceneImmediately();
            }, 1000)
        }
    }
    
    alt.clearInterval(hideHudInterval);
    view.destroy();
    setLock(false);
});

function generatePed(modelString, modelString2, playerId) {
    game.registerEntityForCutscene(0, modelString, 3, game.getEntityModel(playerId), 0)
    game.registerEntityForCutscene(playerId, modelString, 0, 0, 0)
    game.setCutsceneEntityStreamingFlags(modelString, 0, 1) 

    game.registerEntityForCutscene(0, modelString2, 3, 0, 64)

    //game.networkSetEntityOnlyExistsForParticipants(ped, true)
}

//Fade
alt.onServer('FADE_SCREEN', (out, duration) => {
	if(out) {
        game.doScreenFadeOut(duration);
    } else {
        game.doScreenFadeIn(duration);
    }
});

alt.onServer("CLEAN_PED", () => {
    game.clearPedBloodDamage(alt.Player.local.scriptID);
});

alt.onServer("SET_PLAYER_HEADING", (rot) => {
    var player = game.playerPedId();
    game.setEntityHeading(player, rot);
});

//Hands up
alt.onServer('WEAPON_PULLOUT_DISABLED', (toggle) => {
    noAttack = toggle;
    slowWalk = toggle;
});

//Raycast
alt.onServer('TEST_FOR_GROUND_MATERIAL', (id, posX, posY, posZ) => {
    //var startPosition = game.getPedBoneCoords(player, 12844, 0.2, 0, 0);
    var ray = game.startExpensiveSynchronousShapeTestLosProbe(posX, posY, posZ + 1, posX, posY, posZ - 20, 1, alt.Player.local.scriptID, 7);
    var hitData = game.getShapeTestResultIncludingMaterial(ray, null, null, null, null, null);

    alt.emitServer("ON_ANSWER_CALLBACK", id, hitData[0], hitData[1], hitData[2], hitData[3], hitData[4]);
});

//Region of player
alt.onServer('TEST_FOR_REGION', (id, posX, posY, posZ) => {
    var region = game.getNameOfZone(posX, posY, posZ);
    alt.emitServer('ON_ANSWER_CALLBACK', id, region);
});


alt.onServer('TEST_FOR_MILO_NAMES', (id, posX, posY, posZ, milonames) => {
    var found = false;
    var miloFound = "";
    
    milonames.forEach((el) => {
        var int = game.getInteriorAtCoordsWithType(posX, posY, posZ, el);
        found = (int != 0);
        miloFound = el;
    });

    alt.emitServer("ON_ANSWER_CALLBACK", id, found, miloFound);
});

alt.onServer('TEST_FOR_GROUND_Z', (id, posX, posY, posZ) => {
    var player = game.playerPedId();
    var pos = game.getEntityCoords(player, true);
    // @ts-ignore
    var data = game.getGroundZFor3dCoord(posX, posY, posZ, true, false);

    // @ts-ignore
    let ray = game.startExpensiveSynchronousShapeTestLosProbe(pos.x, pos.y, pos.z, posX, posY, parseFloat(data[1]), 16, alt.Player.local.scriptID, 7);
    let hitData = game.getShapeTestResult(ray, null, null, null, null);

    var vec = game.getEntityForwardVector(player);
    let wallRay = game.startExpensiveSynchronousShapeTestLosProbe(pos.x, pos.y, pos.z, pos.x + vec.x, pos.y + vec.y, pos.z, 1, alt.Player.local.scriptID, 7);
    let wallData = game.getShapeTestResult(wallRay, null, null, null, null);

    var inFrontOfWall = wallData[1];
    var probablyInObject = hitData[1];

    alt.emitServer("ON_ANSWER_CALLBACK", id, data[1], inFrontOfWall, probablyInObject);
});

alt.onServer('TEST_FOR_WALL', (id, posX, posY, posZ, front) => {
    var player = alt.Player.local;
    var vec = game.getEntityForwardVector(player.scriptID);
    //var pos = player.pos;
    var mult = 0.6;
    
    if(!front) {
        mult = mult * (-1);
    }
    
    let ray = game.startExpensiveSynchronousShapeTestLosProbe(posX, posY, posZ, posX + (vec.x * mult * 1.2), posY + (vec.y * mult * 1.2), posZ, 1, alt.Player.local.scriptID, 7);
    let hitData = game.getShapeTestResult(ray, null, null, null, null);

    alt.emitServer("ON_ANSWER_CALLBACK", id, hitData[1]);
});

alt.onServer('TEST_DAMAGED_BONE', (id) => {
    var player = alt.Player.local;
    var bone = game.getPedLastDamageBone(player.scriptID);

    alt.emitServer("ON_ANSWER_CALLBACK", id, bone[1]);
});

alt.onServer('TEST_FOR_NEARBY_OBJECTS', (id, posX, posY, posZ, radius, objsList) => {
    var didFind = false;

    objsList.forEach((modelHash) => {
        var found = game.getClosestObjectOfType(posX, posY, posZ, radius, modelHash, false, false, false);
        if(found != 0) {
            var objPos = game.getEntityCoords(found, true);
            var objHeading = game.getEntityHeading(found);
            alt.emitServer("ON_ANSWER_CALLBACK", id, modelHash, JSON.stringify(objPos), objHeading);
            didFind = true;
            return;
        }
    });

    if(!didFind) {
        alt.emitServer("ON_ANSWER_CALLBACK", id, null, null, null);
    }
});

alt.onServer('TEST_FOR_POSITION_IN_FRONT', (id, mult) => {
    var player = alt.Player.local;
    var vec = game.getEntityForwardVector(player.scriptID);

    var startPosition = game.getPedBoneCoords(alt.Player.local.scriptID, 12844, 0.5, 0, 0);
    let ray = game.startExpensiveSynchronousShapeTestLosProbe(startPosition.x, startPosition.y, startPosition.z, startPosition.x + vec.x * mult, startPosition.y + vec.y * mult, startPosition.z - 3, -1, alt.Player.local.scriptID, 7);       
    let hitData = game.getShapeTestResult(ray, null, null, null, null);
    var hitPos = hitData[2];

    alt.emitServer("ON_ANSWER_CALLBACK", id, JSON.stringify(hitPos));
});

alt.onServer('TEST_FOR_CAMERA_HEADING', (id) => {
    var player = alt.Player.local;

    //The order is no order but a "type" of display
    // On 0 the rotation is normal
    // On 1 the rotation is parted in 2 equal half circles with have the same 0 - (-180) degrees 
    var heading = game.getGameplayCamRot(0).z;
    alt.emitServer("ON_ANSWER_CALLBACK", id, heading);
});

alt.onServer('TEST_TEXTURE_VARIATION', (id, componentId, drawableId, clothOrAccessoire) => {
    var player = alt.Player.local;

    var textures;
    if(clothOrAccessoire) {
        textures = game.getNumberOfPedTextureVariations(player.scriptID, componentId, drawableId);
    } else {
        textures = game.getNumberOfPedPropTextureVariations(player.scriptID, componentId, drawableId);
    }
    
    alt.emitServer("ON_ANSWER_CALLBACK", id, textures);
});

alt.onServer('TEST_LOCALIZED_NAME', (id, gxt, data) => {
    alt.emitServer("ON_ANSWER_CALLBACK", id, game.getFilenameForAudioConversation(gxt), data);
});

alt.onServer("TEST_FOR_WATER", (id, pos) => {
    var [isThereWater, waterX] = game.testVerticalProbeAgainstAllWater(pos.x, pos.y, pos.z + 1, 1)

    if(isThereWater == 1) {
        var [isWater, waterHeight] = game.getWaterHeight(pos.x, pos.y, pos.z);
        var [worked, groundHeight] = game.getGroundZFor3dCoord(pos.x, pos.y, pos.z, false, false);

        alt.emitServer("ON_ANSWER_CALLBACK", id, true, waterHeight, groundHeight);
    } else {
        alt.emitServer("ON_ANSWER_CALLBACK", id, false, null, null);
    }
});

alt.onServer("TEST_FOR_WAYPOINT", (id) => {
    var waypoint = game.getFirstBlipInfoId(8);

    var coords = null;
    if (game.doesBlipExist(waypoint)) {
        coords = game.getBlipInfoIdCoord(waypoint);
    }

    alt.emitServer("ON_ANSWER_CALLBACK", id, coords);
});

alt.onServer('TEST_FOR_OBJECTS_IN_FRONT', (id) => {
    var player = alt.Player.local;
    var posX = player.pos.x;
    var posY = player.pos.y;
    var posZ = player.pos.z;


    var heading = game.getGameplayCamRot(2).z;
    var yaw = heading * Math.PI / 180.0;
    var vec = new alt.Vector2(Math.cos(yaw + Math.PI / 2), Math.sin(yaw + Math.PI / 2));
    vec = vec.normalize();
    var orthVect = new alt.Vector2(-vec.y, vec.x);

    //var vec = game.getEntityForwardVector(player.scriptID);
    var mult = 4;
    
    var objectModels = [];
    var matrix = [[0, 0, 0], [0, 0.5, 0], [0, 1, 0], [0, 1, 0.75], [0, 0.5, 0.75], [0, 1, 0.75], [0, 1, -0.75], [0, 0.5, -0.75], [0, 1, -0.75]];

    matrix.forEach((el) => {
        let ray = game.startExpensiveSynchronousShapeTestLosProbe(posX, posY, posZ + el[0], 
                                    posX + (vec.x * mult) + (orthVect.x * el[2]), posY + (vec.y * mult) + (orthVect.y * el[2]), posZ - el[1], 
                                    -1, alt.Player.local.scriptID, 7);
        let hitData = game.getShapeTestResult(ray, null, null, null, null);

        if (hitData[1] != 0) {
            if (game.doesEntityExist(hitData[4]) && game.doesEntityHaveDrawable(hitData[4])) {
                var modelHash = game.getEntityModel(hitData[4]);

                if (interactableObjects.has(modelHash)) {
                    objectModels.push({
                        "modelHash": modelHash,
                        "position": game.getEntityCoords(hitData[4], true),
                        "offset": hitData[3],
                        "heading": game.getEntityHeading(hitData[4]),
                        "isBroken": game.hasObjectBeenBroken(hitData[4], 0),
                    });
                }
            }
        }
    });

    alt.emitServer("ON_ANSWER_CALLBACK", id, JSON.stringify(objectModels));
});


//Screen Effect

alt.onServer('START_SCREEN_EFFECT', (effectName, duration, looped) => {
    game.animpostfxPlay(effectName, duration, looped);
});

alt.onServer('STOP_SCREEN_EFFECT', () => {
    game.animpostfxStopAll();
});

//Scenario

alt.onServer('PLAY_SCENARIO_AT_POS', (scenarioName, posX, posY, posZ, heading, duration, sitting, teleport) => {
    var player = alt.Player.local;

    game.taskStartScenarioAtPosition(player.scriptID, scenarioName, posX, posY, posZ, heading, duration, sitting, teleport);    
});

alt.onServer("PLAY_SCENARIO", (scenarioName) => {
    var player = alt.Player.local;
    game.taskStartScenarioInPlace(player.scriptID, scenarioName, 0, true);
})

alt.onServer('STOP_SCENARIO', () => {
    var player = alt.Player.local;
    game.clearPedTasks(player.scriptID);
});

//Put on Stretcher

alt.onServer('PUT_ON_CARRY', (pusher, patient) => {
    var player = alt.Player.local;
    if(player == patient) {
        alt.setTimeout(() => {
            game.setPedCanRagdoll(alt.Player.local.scriptID, true);
            game.attachEntityToEntity(player.scriptID, pusher.scriptID, game.getEntityBoneIndexByName(pusher.scriptID, "IK_R_Hand"), -0.25, 0.6, 1.0, 0.0, 0.0, -5.0, false, false, true, true, 2, true);
        }, 100);
    }
});

alt.onServer('PUT_OFF_STRETCHER', (pusher, patient) => {
    var player = alt.Player.local;
    if(player == patient) {
        game.detachEntity(player.scriptID, true, false);
        game.setEntityCollision(player.scriptID, true, true);
        alt.setTimeout(() => {
            game.setEntityCollision(player.scriptID, true, true);
        }, 100);
        alt.setTimeout(() => {
            game.setEntityCollision(player.scriptID, true, true);
        }, 1000);
    }
});

//Dragging

alt.onServer('START_PLAYER_DRAGGING', (dragger, dragged) => {
    var player = alt.Player.local;
    if(player == dragger) {
        game.requestAnimDict("combat@drag_ped@");
        alt.setTimeout(() => {
            game.taskPlayAnim(player.scriptID, "combat@drag_ped@", "injured_drag_plyr", 8.0, -8, -1, 33, 0, false, false, false);
        }, 1000);
    } else if(player == dragged) {
        game.requestAnimDict("combat@drag_ped@");
        alt.setTimeout(() => {
            game.setPedCanRagdoll(alt.Player.local.scriptID, true);
            game.taskPlayAnim(player.scriptID, "combat@drag_ped@", "injured_drag_ped", 8.0, -8, -1, 33, 0, false, false, false);
            game.attachEntityToEntity(player.scriptID, dragger.scriptID, 11816, 0.0, 0.48, 0.0, 0.0, 0.0, 0.0, false, false, false, false, 2, true);
        }, 1000);
    }
});


alt.onServer('STOP_PLAYER_DRAGGING', (dragger, dragged) => {
    var player = alt.Player.local;
    if(player == dragger) {
        //TODO: function clearPedTasks(ped: number | alt.Player): void (+1 overload) Expected 1 arguments, but got 0.ts(2554)natives.d.ts(19382, 35): An argument for 'ped' was not provided. 
        game.clearPedTasks();
    } else if(player == dragged) {
        //TODO: function clearPedTasks(ped: number | alt.Player): void (+1 overload) Expected 1 arguments, but got 0.ts(2554)natives.d.ts(19382, 35): An argument for 'ped' was not provided. 
        game.clearPedTasks();
        game.detachEntity(dragger.scriptID, true, true);
    }
});

//Animations

var anims = [];
var forceAnim = null;

alt.onServer('PLAY_ANIM', (dict, name, duration, flag, facingRotation, isBackgroundAnim, isForced, time) => {
    var player = game.playerPedId();

    alt.log("Door Arm Animation Disabled");
    game.setPedConfigFlag(player, 104, false); // Disable Door Arm Animation

    //Set Facing Rotation
    if(facingRotation != -1) {
        game.setEntityHeading(player, facingRotation);
    }
    
    game.setPedCanRagdoll(player, true);
    loadAnimDic(dict).then(() => {
        anims.push({dict: dict, name: name, flag: flag, duration: toInt32(duration), startTime: Date.now(), isBackgroundAnim: isBackgroundAnim, isForced: isForced});
        if(isForced) {
            game.clearPedTasksImmediately(player);
            forceAnim = {dict: dict, name: name, flag: flag, time: time};
        }
    
        alt.setTimeout( () => {
            game.taskPlayAnim(player, dict, name, 8.0, 1.0, toInt32(duration), toInt32(flag), time, false, false, false);
        }, 50);
    });
});

//var doorTimeout = null;
alt.setInterval(() => {
    if(forceAnim != null && !game.isEntityPlayingAnim(alt.Player.local.scriptID, forceAnim.dict, forceAnim.name, forceAnim.flag)) {  
        game.taskPlayAnim(alt.Player.local.scriptID, forceAnim.dict, forceAnim.name, 8.0, 1.0, -1, forceAnim.flag, forceAnim.time, false, false, false);
    }

    var newAnims = [];
    anims.forEach((el) => { 
        if(Date.now() - el.startTime < el.duration) {
            newAnims.push(el); 
        }
    });

    anims = newAnims;

    if (anims.length == 0 && forceAnim == null && !game.getPedConfigFlag(alt.Player.local.scriptID, 104, false)) {
        alt.log("Door Arm Animation Enabled");
        game.setPedConfigFlag(alt.Player.local.scriptID, 104, true); // Enable Door Arm Animation
    }

    //if(game.isPedOpeningDoor(alt.Player.local.scriptID)) {
    //    if(doorTimeout == null) {
    //        doorTimeout = alt.setTimeout(() => {
    //            if(!game.isPedOpeningDoor(alt.Player.local.scriptID)) {
    //                anims.forEach((el) => {
    //                    game.taskPlayAnim(alt.Player.local.scriptID, el.dict, el.name, 8.0, 1.0, el.duration - (Date.now() - el.startTime), el.flag, 0, false, false, false);

    //                    if(el.duration - (Date.now() - el.startTime) > 750) {
    //                        alt.setTimeout(() => {
    //                            game.taskPlayAnim(alt.Player.local.scriptID, el.dict, el.name, 8.0, 1.0, el.duration - (Date.now() - el.startTime) - 500, el.flag, 0, false, false, false);
    //                        }, 500);
    //                    }
    //                });
    //            }
    //            doorTimeout = null;
    //        }, 100);
    //    }
    //}
}, 1000);

var facialAnimInterval = null;
alt.onServer("PLAY_FACIAL_ANIM", (dict, name, duration) => {
    if(facialAnimInterval != null) {
        alt.clearInterval(facialAnimInterval);
    }

    loadAnimDic(dict).then(() => {
        game.playFacialAnim(alt.Player.local.scriptID, name, dict);

        if (duration > 1000) {
            facialAnimInterval = alt.setInterval(() => {
                game.playFacialAnim(alt.Player.local.scriptID, name, dict);
            }, 1000);
        }

        if(duration != -1) {
            alt.setTimeout(() => {
                if(facialAnimInterval != null) {
                    alt.clearInterval(facialAnimInterval);
                    facialAnimInterval = null;
                }
            
                game.playFacialAnim(alt.Player.local.scriptID, 'mood_normal_1', 'facials@gen_male@variations@normal');
            }, duration);
        }
    });
});

alt.onServer('STOP_ANIM', () => {
    var player = alt.Player.local.scriptID;

    var backgroundAnims = [];
    anims.forEach((el) => {
        if(!el.isBackgroundAnim) {
            //game.taskPlayAnim(player, el.dict, el.name, 8.0, 1.0, 10, el.flag, 0, false, false, false);
            // alt.setTimeout(() => {
            game.stopAnimTask(player, el.dict, el.name, 2);
            // }, 500);
            //game.taskPlayAnim(player, el.dict, el.name, 8.0, 1.0, 10, el.flag, 0, false, false, false);
        } else {
            backgroundAnims.push(el);
        }
    });

    if(forceAnim != null) {
        var temp = forceAnim;
        forceAnim = null;
        
        alt.setTimeout( () => {
            game.taskPlayAnim(player, temp.dict, temp.name, 8.0, 1.0, 1, temp.flag, 0, false, false, false);
        }, 3);
    }

    backgroundAnims.forEach((el) => {
        game.taskPlayAnim(player, el.dict, el.name, 8.0, 1.0, el.duration - (Date.now() - el.startTime), el.flag, 1, false, false, false);
    });

    anims = backgroundAnims;


    if(facialAnimInterval != null) {
        alt.clearInterval(facialAnimInterval);
        facialAnimInterval = null;

        game.playFacialAnim(player, 'mood_normal_1', 'facials@gen_male@variations@normal');
    }
});

alt.onServer('STOP_BACKGROUND_ANIM', () => {
    var player = alt.Player.local.scriptID;

    var foregroundAnims = [];
    anims.forEach((el) => {
        if(el.isBackgroundAnim) {
            game.stopAnimTask(player, el.dict, el.name, el.flag);
            //game.taskPlayAnim(player, el.dict, el.name, 8.0, 1.0, 1, el.flag, 0, false, false, false);
        } else {
            foregroundAnims.push(el);
        }
    });

    anims = foregroundAnims;
});

var overLayList = [];

alt.onServer("SET_PLAYER_DECORATION", (element) => {
    overLayList.push(element);
    
    setPlayerDecorations(alt.Player.local.scriptID, overLayList);
});

alt.onServer("RESET_PLAYER_DECORATION_TYPE", (type) => {
    overLayList = overLayList.filter((el) => {
        var subSplit = el.split("#");
        return subSplit[0] != type;
    });

    setPlayerDecorations(alt.Player.local.scriptID, overLayList);
});

export function redoOverlayTick() {
    //setPlayerDecorations(alt.Player.local.scriptID, overLayList);
}

function setPlayerDecorations(scriptID, overlayList) {
    game.clearPedDecorations(scriptID);

    overlayList.forEach((el) => {
        var subSplit = el.split("#");
        if (subSplit[0] == "hair_overlay") {
            game.addPedDecorationFromHashesInCorona(scriptID, alt.hash(subSplit[1]), alt.hash(subSplit[2]));
        } else {
            game.addPedDecorationFromHashes(scriptID, alt.hash(subSplit[1]), alt.hash(subSplit[2]));
        }
    });
}


export function moveMouth(player, isTalking) {
    if(isTalking) {
        game.playFacialAnim(player.scriptID, "mic_chatter", "mp_facial");
    } else {
        game.playFacialAnim(player.scriptID, "mood_normal_1", "facials@gen_male@base");
    }
}

alt.onServer('GET_HEADING', () => {
    var player = game.playerPedId();
    alt.log(game.getEntityHeading(player));
});

alt.onServer('SIT_DOWN_WITHOUT_CHAIR', () => {
    var player = alt.Player.local;
    var pos = alt.Player.local.pos;
    game.taskStartScenarioAtPosition(player.scriptID, "PROP_HUMAN_SEAT_BENCH", pos.x, pos.y, pos.z - 0.5, -90, 0, true, true);
});


alt.onServer("INITIATE_CROUCH", () => {
    initiateCrouch();
});

var crouchToggle = false;
game.requestClipSet("MOVE_M@TOUGH_GUY@");
game.requestClipSet("move_ped_crouched");
game.requestClipSet("move_ped_crouched_strafing");

export function initiateCrouch() {
    game.requestClipSet("MOVE_M@TOUGH_GUY@");
    game.requestClipSet("move_ped_crouched");
    game.requestClipSet("move_ped_crouched_strafing");

    var player = alt.Player.local;

    if(!player.vehicle) {
        if(!crouchToggle){
            game.setPedUsingActionMode(player.scriptID, false, -1, 'DEFAULT_ACTION');
            game.setPedMovementClipset(player.scriptID, "move_ped_crouched", 0.55);
            game.setPedStrafeClipset(player.scriptID, "move_ped_crouched_strafing");

            alt.emitServer("PLAYER_CROUCH_TOGGLE", true);
            crouchToggle = true; 
        }else{
            game.setPedUsingActionMode(player.scriptID, false, -1, 'DEFAULT_ACTION');
            
            game.setPedMovementClipset(player.scriptID, "MOVE_M@TOUGH_GUY@", 0.25);
            alt.setTimeout(() => {
                game.resetPedMovementClipset(player.scriptID, 0);
                game.resetPedStrafeClipset(player.scriptID, 0);
                alt.emitServer("PLAYER_CROUCH_TOGGLE", false);
                crouchToggle = false; 
            }, 200);
        }
    }
}

alt.setInterval(() => {
    var player = alt.Player.local;

    if(crouchToggle){
        game.setPedMovementClipset(player.scriptID, "move_ped_crouched", 0.55);
        game.setPedStrafeClipset(player.scriptID, "move_ped_crouched_strafing");

        game.disableControlAction(0, 22, true); //Jump
        game.setPedUsingActionMode(player.scriptID, false, -1, 'DEFAULT_ACTION');

        if(game.isAimCamActive()){
            game.setPedUsingActionMode(player.scriptID, false, -1, 'DEFAULT_ACTION');
        }
    } else {
        game.resetPedMovementClipset(player.scriptID, 0);
        game.resetPedStrafeClipset(player.scriptID, 0);
    }
}, 100) 



//Walking Style

var clipSet = null;
var holdingShift = false;
alt.on("keydown", (key) => {
	if(key == 16) {
		clipSet = runningClipSet;
        
        if(runningClipSet == null) {
            game.resetPedMovementClipset(alt.Player.local.scriptID, 0); 
        }

        holdingShift = true;
	}
});

alt.on("keyup", (key) => {
	if(key == 16) {
		clipSet = walkingClipSet;

        if(walkingClipSet == null) {
            game.resetPedMovementClipset(alt.Player.local.scriptID, 0); 
        }

        holdingShift = false;
	}
});

alt.everyTick(() => {
    if(!crouchToggle) {
        if(clipSet != null) {
	        game.setPedMovementClipset(alt.Player.local.scriptID, clipSet, 1);
        }
    }
});

var walkingClipSet = null;
var runningClipSet = null;
alt.onServer('SET_WALKING_ANIMATION', (type, name) => {
    var player = alt.Player.local;

    if(name != null) {
        loadClipSet(name).then(() => {
            if(type == "walk") {
                walkingClipSet = name;
                if(!holdingShift) {
                    clipSet = name;
                }
            } else {
                runningClipSet = name;
                if(holdingShift) {
                    clipSet = name;
                }
            }
        });
    } else {
        if(type == "walk") {
            walkingClipSet = null;
        } else {
            runningClipSet = null;
        }

        game.resetPedMovementClipset(player.scriptID, 0);     
    }
});


var hour = -1;
alt.onServer('SET_DATE_TIME_HOUR', (h) => {
    hour = h;
});

alt.everyTick(() => {
    var player = alt.Player.local;

    //Disabling STRG so it is customCrouch!
    game.disableControlAction(1, 36, true);

    if(crouchToggle && game.isAimCamActive()) {
        //WASD
        game.disableControlAction(0, 30, true);
        game.disableControlAction(0, 31, true);
        game.disableControlAction(0, 32, true);
        game.disableControlAction(0, 33, true);
        game.disableControlAction(0, 34, true);
        game.disableControlAction(0, 35, true);

        game.disableControlAction(0, 266, true);
        game.disableControlAction(0, 267, true);
        game.disableControlAction(0, 268, true);
        game.disableControlAction(0, 269, true);

        //Space
        game.disableControlAction(0, 22, true);
        game.disableControlAction(0, 55, true);

        //Sprint
        game.disableControlAction(0, 21, true);
    }

    if(hour == -1) {
        var date = new Date();
        game.setClockTime(toInt32(date.getHours()), toInt32(date.getMinutes()),toInt32( date.getSeconds()));
        game.setClockDate(date.getDate(), date.getMonth(), date.getFullYear());
    } else {
        game.setClockTime(toInt32(hour), 0, 0);
    }
});

// //Clothing

alt.onServer("SHOW_CLOTHES", (slot, drawable, texture, dlc) => {
    if(dlc == null) {
        game.setPedComponentVariation(alt.Player.local, slot, drawable, texture, 0);
    } else {
        alt.setPedDlcClothes(alt.Player.local, alt.hash(dlc), slot, drawable, texture, 0)
    }
});


alt.onServer('REMOVE_PLAYER_DECORATION_BY_TYPE', (player, type) => {
    deleteIfNot(allOverlayList, (key, value) => {
        return value.type != type;
    });
    
    game.clearPedDecorations(player.scriptID);

    allOverlayList.forEach((el) => {
        if(el.type == "hair_overlay") {
            game.addPedDecorationFromHashesInCorona(player.scriptID, alt.hash(el.collection), alt.hash(el.hash));
        } else {
            game.addPedDecorationFromHashes(player.scriptID, alt.hash(el.collection), alt.hash(el.hash));
        }
    });
});

function deleteIfNot(map, pred) {
    for (let [k, v] of map) {
        if (!pred(k, v)) {
            map.delete(k);
        }
    }
    return map;
}  

// //Weather

var currentlyChanging = false;
alt.onServer("SET_WEATHER_TRANISTION", (oldW, newW, mix) => {
    currentlyChanging = true;
    var per = 0;
    var int = alt.setInterval(() => {
        game.setCurrWeatherState(alt.hash(oldW), alt.hash(newW), per);
        log("WEATHER", "Set Weather Transitation: From: " + oldW + ", New: " + newW + ", Percent: " + per)
        if(per <= mix) {
            per = per + 0.01;
        } else {
            alt.clearInterval(int);
            currentlyChanging = false;
        }
    }, 250);
});

alt.onServer("SET_WEATHER_MIX", (oldW, newW, mix) => {
    if(oldW == newW) {
        holdWeather(newW, newW, 1);
    } else {
        holdWeather(oldW, newW, mix);
    }
});

var oldInt;
function holdWeather(oldW, newW, mix) {
    if(oldInt != null){
        alt.clearInterval(oldInt);
    }

    game.setCurrWeatherState(alt.hash(oldW), alt.hash(newW), mix);
    oldInt = alt.setInterval(() => {
        alt.setWeatherSyncActive(false);
        if(!currentlyChanging){
            game.setCurrWeatherState(alt.hash(oldW), alt.hash(newW), mix);
        }
    }, 100);
}


//DamageSystem

var noAttackToggle = false;
alt.onServer('TOOGLE_CAN_ATTACK', () => {
    noAttackToggle = !noAttackToggle;
    if(noAttackToggle) {      
        game.setPedConfigFlag(game.playerPedId(), 122, true);
    } else {   
        game.setPedConfigFlag(game.playerPedId(), 122, false);
    }
});

alt.setInterval(() => {
    if(noAttackToggle) {      
        game.setPedConfigFlag(game.playerPedId(), 122, true);
    }
}, 5000);

var ragdollInt;
alt.onServer('SET_RAGDOLL', () => {
    if(ragdollInt != null) {
        ragdollInt = alt.setInterval(() => {
            game.setPedToRagdoll(alt.Player.local.scriptID, 1000, 1000, 0, false, false, false);
        }, 1);
    }
});

alt.onServer('STOP_RAGDOLL', () => {
    alt.clearInterval(ragdollInt);
});

alt.onServer('SET_PLAYER_INJURED', (toggle, state) => {
    switch(state) {
        case 1:
            toggleLegInjury(toggle, true);
            break;
        case 2:
            toggleNoAttack(toggle)
            break; 
        case 3:
            toggleLegInjury(toggle,false);
            break;
    }
});

function toggleLegInjury(toggle, setSlow) {
    if(setSlow) {
        slowWalk = toggle;
    }

    if(toggle) {
        //game.setEntityMaxSpeed(game.playerPedId(), 2);

        //Player limp is injured
        game.setPedConfigFlag(game.playerPedId(), 166, true);
        game.setPedConfigFlag(game.playerPedId(), 166, true);
        
    } else {
        //game.setEntityMaxSpeed(game.playerPedId(), -1);


        //Player limp is not injured
        game.setPedConfigFlag(game.playerPedId(), 166, false);
        game.setPedConfigFlag(game.playerPedId(), 166, false);
    }
}

alt.onServer('TOOGLE_NO_ATTACK', (toggle) => {
    toggleNoAttack(toggle);
});

function toggleNoAttack(toggle) {
    noAttack = toggle;
    if(toggle) {
        //Player cannot attack
        game.setPedConfigFlag(game.playerPedId(), 122, true);

        //Switch to hand
        game.setCurrentPedWeapon(game.playerPedId(), 2725352035, true);
    } else {
        //Player can attack
        game.setPedConfigFlag(game.playerPedId(), 122, false);
    }
}



alt.everyTick(( ) => {
    if(noLook) {
        //No Looking up and down
        game.disableControlAction(1, 1, true);
        game.disableControlAction(1, 2, true);
    }

    if(slowWalk) {
        //Sprinting and jumping
        game.disableControlAction(0, 21, true);
        game.disableControlAction(0, 22, true);
    }

    if(noAttack || noAttackToggle) {
        //Player cannot shoot
        game.disableControlAction(0, 45, true);
        game.disableControlAction(0, 47, true);
        game.disableControlAction(0, 58, true);
        game.disableControlAction(0, 24, true);
        game.disableControlAction(0, 25, true);
        game.disablePlayerFiring(game.playerPedId(), true);
    }

    if(noAttack || noAttackToggle) {
        //Player cannot switch weapon
        game.disableControlAction(0, 12, true);
        game.disableControlAction(0, 13, true);
        game.disableControlAction(0, 14, true);
        game.disableControlAction(0, 15, true);
        game.disableControlAction(0, 16, true);
        game.disableControlAction(0, 17, true);
        game.disableControlAction(0, 37, true);
        game.disableControlAction(0, 99, true);
        game.disableControlAction(0, 100, true);
        game.disableControlAction(0, 115, true);
        game.disableControlAction(0, 116, true);
        game.disableControlAction(0, 117, true);
        game.disableControlAction(0, 118, true);
        game.disableControlAction(0, 140, true);
        game.disableControlAction(0, 141, true);
        game.disableControlAction(0, 142, true);
        game.disableControlAction(0, 157, true);
        game.disableControlAction(0, 158, true);
        game.disableControlAction(0, 159, true);
        game.disableControlAction(0, 160, true);
        game.disableControlAction(0, 161, true);
        game.disableControlAction(0, 162, true);
        game.disableControlAction(0, 163, true);
        game.disableControlAction(0, 164, true);
        game.disableControlAction(0, 165, true);
        game.disableControlAction(0, 166, true);
        game.disableControlAction(0, 167, true);
        game.disableControlAction(0, 168, true);
        game.disableControlAction(0, 169, true);
        game.disableControlAction(0, 257, true);
        game.disableControlAction(0, 261, true);
        game.disableControlAction(0, 262, true);
        game.disableControlAction(0, 263, true);
        game.disableControlAction(0, 264, true);
    }
});


//TimeCycle

var currentTimeCycles = new Map();
alt.onServer('SET_TIMECYCLE', (id, cycle, strength) => {   
    if(currentTimeCycles[id] != null) {
        currentTimeCycles[id].cycle = cycle;
        currentTimeCycles[id].strength = strength;
    } else {
        currentTimeCycles.set(id, {cycle: cycle, strength: strength});
    }

    timeCycleInterval();
});

var currentAdditionalTimeCycles = new Map();
alt.onServer('SET_ADDITIONAL_TIMECYCLE', (id, cycle) => {    
    if(currentAdditionalTimeCycles[id] != null) {
        currentAdditionalTimeCycles[id].currentAdditionalTimeCycles = cycle;
    } else {
        currentAdditionalTimeCycles.set(id, {cycle: cycle});
    }

    timeCycleInterval();
});

alt.onServer('REMOVE_TIMECYCLE', (id) => {
    currentTimeCycles.delete(id);

    timeCycleInterval();
});

alt.onServer('REMOVE_ADDITIONAL_TIMECYCLE', (id) => {
    currentAdditionalTimeCycles.delete(id);

    timeCycleInterval();
});

alt.setInterval(() => {
    timeCycleInterval();
}, 5000);

function timeCycleInterval() {
    if(currentTimeCycles.size > 0) {
        var first = [...currentTimeCycles][0][1];
        game.setTimecycleModifier(first.cycle);
        game.setTimecycleModifierStrength(first.strength);
    } else {
        game.clearTimecycleModifier();
    }

    if(currentAdditionalTimeCycles.size > 0) {
        var first = [...currentAdditionalTimeCycles][0][1];
        game.setExtraTcmodifier(first.cycle);
    } else {
        game.clearExtraTcmodifier();
    }
}


var hungerTick = null;
alt.onServer("TOGGLE_HUNGER_MODE", (toggle) => {
    if (toggle) {
        if (hungerTick != null) {
            alt.clearInterval(hungerTick);
            hungerTick = null;
        }

        hungerTick = alt.setInterval(() => {
            var rand = Math.random();
            if (rand > 0.5) {
                game.shakeGameplayCam("SMALL_EXPLOSION_SHAKE", 0.1);
            } else {
                game.shakeGameplayCam("DEATH_FAIL_IN_EFFECT_SHAKE", 0.25);
            }
        }, Math.random() * 30000 + 15000);
    } else {
        if (hungerTick != null) {
            alt.clearInterval(hungerTick);
        }
    }
});

var pulseEffectInt = null;
alt.onServer('START_PULSE_EFFECT', (intTime, stepToBlink) => {
    var count = 0;

    pulseEffectInt = alt.setInterval(() => {
        game.shakeGameplayCam("VIBRATE_SHAKE", 0.075);
        
        count++;
        if(count > stepToBlink) {
            count = 0;
            game.animpostfxPlay("MP_intro_logo", 1000, false);
        }
    }, intTime);
});

alt.onServer('STOP_PULSE_EFFECT', () => {
    if(pulseEffectInt != null) {
        alt.clearInterval(pulseEffectInt);
        pulseEffectInt = null;
    }
});


var zoneName;
var streetName;

function getStreetInfo() {
if (native.isRadarEnabled() && !native.isRadarHidden()) {
        isMetric = native.getProfileSetting(227) == 1;
        minimap = getMinimapAnchor();

        const position = alt.Player.local.pos;
        let getStreet = native.getStreetNameAtCoord(position.x, position.y, position.z, 0, 0);
        zoneName = native.getLabelText(native.getNameOfZone(position.x, position.y, position.z));
        streetName = native.getStreetNameFromHashKey(getStreet.streetName);
        if (getStreet.crossingRoad && getStreet.crossingRoad != getStreet.streetName) streetName += ` / ${native.getStreetNameFromHashKey(getStreet.crossingRoad)}`;
    } else {
        streetName = null;
        zoneName = null;
    }
}

//Rockstar Editor
alt.onServer('CLIP', (check) => {
    if (check){
        game.startReplayRecording(1);
    }
    else {
        game.stopReplayRecording();
    }
});

alt.onServer('EDITOR', (state) => {
    if (state) {
        game.activateRockstarEditor(0);
        game.setPlayerRockstarEditorDisabled(false);
        let interval = alt.setInterval(() => {
            if (game.isScreenFadedOut()) {
                game.doScreenFadeIn(1000);
                alt.clearInterval(interval);
            }
        }, 1000);
    }
    else {
        game.setPlayerRockstarEditorDisabled(true);
    }
});

alt.onServer('TOGGLE_FIRE_PROOF', (toggle) => {
    game.setEntityProofs(alt.Player.local.scriptID, false, toggle, false, false, false, false, false, false);
});

alt.onServer("TOGGLE_INFINITE_AIR", (toggle) => {
    game.setPedConfigFlag(alt.Player.local.scriptID, 3, !toggle);
});

alt.onServer("TOGGLE_SCUBA_MODE", (toggle) => {
    game.setEnableScuba(alt.Player.local.scriptID, toggle);
})

//FINGER POINTER

alt.onServer('TOOGLE_FINGER_POINT', () => {
    if(FingerpointingInstance.active) {
        FingerpointingInstance.stop();
    } else {
        FingerpointingInstance.start();
    }
});

class Fingerpointing {
	constructor() {
		this.active = false;
		this.interval = null;
		this.cleanStart = false;
		this.debounceTime = 150;
		this.lastBlockDate = null;
		this.gameplayCam = game.createCameraWithParams(alt.hash('gameplay'), 0, 0, 0, 0, 0, 0, 1, false, 0);
		this.localPlayer = alt.Player.local;
	}

	async start() {
		if (this.active) return;
		this.active = true;
		try {
			await this.requestAnimDictPromise('anim@mp_point');
			game.setPedCurrentWeaponVisible(
				this.localPlayer.scriptID,
				false,
				true,
				true,
				true
			);
			game.setPedConfigFlag(this.localPlayer.scriptID, 36, true);
			game.taskMoveNetworkByName(
				this.localPlayer.scriptID,
				'task_mp_pointing',
				0.5,
				false,
				'anim@mp_point',
				24
			);
			this.cleanStart = true;
			this.interval = alt.setInterval(this.process.bind(this), 0);
		} catch (e) {
            log("FINGERPOINTING", "Error: " + e);
		}
	}

	stop() {
		if (!this.active) return;
		if (this.interval) {
			alt.clearInterval(this.interval);
		}
		this.interval = null;

		this.active = false;

		if (!this.cleanStart) return;
		this.cleanStart = false;
		game.requestTaskMoveNetworkStateTransition(
			this.localPlayer.scriptID,
			'Stop'
		);

		if (!game.isPedInjured(this.localPlayer.scriptID)) {
			game.clearPedSecondaryTask(this.localPlayer.scriptID);
		}
		if (!game.isPedInAnyVehicle(this.localPlayer.scriptID, true)) {
			game.setPedCurrentWeaponVisible(
				this.localPlayer.scriptID,
				true,
				true,
				true,
				true
			);
		}
		game.setPedConfigFlag(this.localPlayer.scriptID, 36, false);
		game.clearPedSecondaryTask(this.localPlayer.scriptID);
	}

	getRelativePitch() {
		let camRot = game.getGameplayCamRot(2);
		return camRot.x - game.getEntityPitch(this.localPlayer.scriptID);
	}

	process() {
		if (!this.active) return;
		let camPitch = this.getRelativePitch();
		if (camPitch < -70.0) {
			camPitch = -70.0;
		} else if (camPitch > 42.0) {
			camPitch = 42.0;
		}
		camPitch = (camPitch + 70.0) / 112.0;

		let camHeading = game.getGameplayCamRelativeHeading();
		let cosCamHeading = Math.cos(camHeading);
		let sinCamHeading = Math.sin(camHeading);

		if (camHeading < -180.0) {
			camHeading = -180.0;
		} else if (camHeading > 180.0) {
			camHeading = 180.0;
		}
		camHeading = (camHeading + 180.0) / 360.0;

		let coords = game.getOffsetFromEntityInWorldCoords(
			this.localPlayer.scriptID,
			cosCamHeading * -0.2 - sinCamHeading * (0.4 * camHeading + 0.3),
			sinCamHeading * -0.2 + cosCamHeading * (0.4 * camHeading + 0.3),
			0.6
		);
		let ray = game.startShapeTestCapsule(
			coords.x,
			coords.y,
			coords.z - 0.2,
			coords.x,
			coords.y,
			coords.z + 0.2,
			1.0,
			95,
			this.localPlayer.scriptID,
			7
		);
		let [_, blocked, coords1, coords2, entity] = game.getShapeTestResult(
			ray,
			false,
			null,
			null,
			null
		);
        
		if (blocked && this.lastBlockDate === null) {
			this.lastBlockDate = new Date();
		}
		game.setTaskMoveNetworkSignalFloat(
			this.localPlayer.scriptID,
			'Pitch',
			camPitch
		);
		game.setTaskMoveNetworkSignalFloat(
			this.localPlayer.scriptID,
			'Heading',
			camHeading * -1.0 + 1.0
		);

		//this is a debounce for isBlocked network signal to avoid flickering of the peds arm on fast raycast changes
		if (this.isBlockingAllowed()) {
			game.setTaskMoveNetworkSignalBool(
				this.localPlayer.scriptID,
				'isBlocked',
				blocked
			);
		}

		game.setTaskMoveNetworkSignalBool(
			this.localPlayer.scriptID,
			'isFirstPerson',
			game.getCamViewModeForContext(game.getCamActiveViewModeContext()) === 4
		);
	}

	isBlockingAllowed() {
		const isAllowed = new Date() - this.lastBlockDate > this.debounceTime;
		if (isAllowed) {
			this.lastBlockDate = null;
		}
		return isAllowed;
	}

	requestAnimDictPromise(dict) {
		game.requestAnimDict(dict);
		return new Promise((resolve, reject) => {
			let tries = 0;
			let check = alt.setInterval(() => {
				tries++;
				if (game.hasAnimDictLoaded(dict)) {
					alt.clearInterval(check);
					resolve(true);
				} else if (tries > 30) {
					alt.clearInterval(check);
					reject('Anim request wait limit reached');
				}
			}, 50);
		});
	}
}

export const FingerpointingInstance = new Fingerpointing();



//Minimap

alt.onServer("CHANGE_MINIMAP_SIZE", (size) => {
    game.displayRadar(true);

    if(size == "OFF") {
        game.displayRadar(false);
    } else if( size == "NORMAL") {
        game.setBigmapActive(false, false);
    } else if(size == "BIGGER") {
        game.setBigmapActive(true, false);
    } else {
        game.setBigmapActive(true, true);
    }
});

//https://gist.github.com/xxshady/21c2c838d5c7d266b8975884feadeb6e
const SYNC_MS_DELAY = 500
const SYNC_MS_DELAY_REMOTE = 100
const MAX_HEADING_DIST = 70.0
const BACKWARDS_HEADING_DIST = 145.0
const TASK_LOOK_AT_COORD_DURATION = 450
const LOOKING_AT_POS_OFFSET = 5.0

let lastSendedLookingAtPosString
const sendServer = (lookingAtPos) => {
  let posString = ""

  if (lookingAtPos) {
    posString = lookingAtPos
      .toArray()
      .map(v => v.toFixed(1))
      .join("|")
  }

  if (lastSendedLookingAtPosString === posString) return

  alt.emitServer(
    "SYNC_PLAYER_LOOK_AT",
    posString,
  )
}

let prevPlayerHeading = -1
let prevCamRot = alt.Vector3.zero

alt.setInterval(() => {
  let camRot = game.getGameplayCamRot(2)
  const playerHeading = alt.Player.local.rot.toDegrees().z

  if (
    (
      prevCamRot.x.toFixed(1) === camRot.x.toFixed(1) &&
      prevCamRot.y.toFixed(1) === camRot.y.toFixed(1) &&
      prevCamRot.z.toFixed(1) === camRot.z.toFixed(1)
    ) &&
    (playerHeading.toFixed(1) === prevPlayerHeading.toFixed(1))
  ) return

  prevCamRot = camRot
  prevPlayerHeading = playerHeading

  const camHeading = camRot.z

  // if player is looking backwards block player head heading at MAX_HEADING_DIST
  const headingDist = distanceDegrees(camHeading, playerHeading)

  if (headingDist > BACKWARDS_HEADING_DIST) {
    sendServer(null)
    return
  }

  if (headingDist > MAX_HEADING_DIST) {
    // why i wrote something like this? don't ask me
    const a = playerHeading + MAX_HEADING_DIST
    const b = playerHeading - MAX_HEADING_DIST
    const aDist = distanceDegrees(camHeading, a)
    const bDist = distanceDegrees(camHeading, b)
    const closest = aDist < bDist ? a : b
    camRot = new alt.Vector3(camRot.x, camRot.y, closest)
  }

  const direction = rotationToDirectionDegrees(camRot)
  const lookingAtPos = direction.mul(LOOKING_AT_POS_OFFSET).add(alt.Player.local.pos)

  sendServer(lookingAtPos)
}, SYNC_MS_DELAY)

alt.setInterval(() => {
  for (const player of alt.Player.streamedIn) {
    const posString = player.getStreamSyncedMeta("SYNC_PLAYER_LOOK_AT");
    if (!posString) continue

    const pos = posString
      .split("|")
      .map(v => +v)

      game.taskLookAtCoord(player, ...pos, TASK_LOOK_AT_COORD_DURATION, 0, 0)
  }
}, SYNC_MS_DELAY_REMOTE)

function rotationToDirectionDegrees(rotation) {
  const z = rotation.z * (Math.PI / 180.0)
  const x = rotation.x * (Math.PI / 180.0)
  const num = Math.abs(Math.cos(x))

  return new alt.Vector3(
    (-Math.sin(z) * num),
    (Math.cos(z) * num),
    Math.sin(x),
  )
}

function distanceDegrees(a, b) {
  let dist = Math.abs(a % 360 - b % 360)
  dist = Math.min(dist, 360 - dist)

  return dist
}


//Death Message

alt.onServer('SHOW_WASTED_SCREEN', (title, message, showEffect) => {
    showWastedScreen(title, message, showEffect);
});

alt.onServer('SHOW_BLACK_WHITE_SCREEN', () => {
    showBlackAndWhiteScreen();
});

function showBlackAndWhiteScreen() {
    game.animpostfxPlay("DeathFailMPIn", 0, true);
}

alt.onServer('STOP_WASTED_SCREEN', () => {
    stopWastedScreen();
});

var running = false;
var titleMsg;
var messageMsg;

function showWastedScreen(title, message, doEffects) {
    running = true;

    if (doEffects) {
        game.playSoundFrontend(-1, "Bed", "WastedSounds", true);
        game.animpostfxPlay("DeathFailMPIn", 0, true);
        game.setCamDeathFailEffectState(1);
    }

    titleMsg = title;
    messageMsg = message;

    alt.setTimeout(() => {
        NativeUI.MidsizedMessage.ShowCondensedShardMessage(
            title, 
            message, 
            NativeUI.HudColor.HUD_COLOUR_BLACK, 
            true,
        20000)
    }, 1250)
}

function stopWastedScreen(){
    game.animpostfxStop("DeathFailMPIn");
    game.setCamDeathFailEffectState(0);
}