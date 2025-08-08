import alt from 'alt'; 
import game from 'natives';

import { setLock } from './client.js';
var isFishing = false;

const FISHING_FORWARD = 20;

alt.onServer("TEST_FISHING_AVAILABE", () => {
    var pos = alt.Player.local.pos;
    var forward = game.getEntityForwardVector(alt.Player.local.scriptID);

    for(var i = 0; i <= FISHING_FORWARD; i++) {
        var fx = pos.x + forward.x * i;
        var fy = pos.y + forward.y * i;
        var fz0 = -10000;
        var fz1 = 10000;

        var res = game.testProbeAgainstWater(fx, fy, fz0, fx, fy, fz1);
        if(res[0]) {
            var ray = game.startExpensiveSynchronousShapeTestLosProbe(pos.x, pos.y, pos.z + 1, res[1].x, res[1].y, res[1].z, 1, alt.Player.local.scriptID, 7);
            let hitData = game.getShapeTestResult(ray, null, null, null, null);

            if(hitData[1]) {
                alt.emitServer("ANSWER_FISHING_AVAILABE", false);
                return;
            }
        }
    }

    alt.emitServer("ANSWER_FISHING_AVAILABE", true);
});


//Check if facing water
// function () {
//     const headPosition = natives.getPedBoneCoord(this, 31086, 0, 0, 0)
//     const offsetPosition = natives.getOffsetFromEntityInWorldCoords(this, 0, 50, -25)
//     const hit = natives.testProbeAgainstWater(headPosition.x, headPosition.y, headPosition.z, offsetPosition.x, offsetPosition.y, offsetPosition.z)
//     return hit[0]
// }

alt.onServer("keyUp", (key) => {
    if(!isFishing) {
        return;
    }
});

const CATCH_INTERVAL_BASE = 750;
const MAX_FISH_LEVEL = 5;
alt.onServer("START_FISHING", (timeout, level) => {
    //First wait for fish to bite
    //Reel fish in
    //   If fish is active dont reel, if active reel.
    //   when you reel or dont if you should/shouldnt stress is added
    //   level determines how much stress it needs to break free

    game.requestNamedPtfxAsset('core');
    isFishing = true;
    setLock(true);
    recursiveFishing(timeout, level, level);
});

function recursiveFishing(timeout, level, counter) {
    if(counter == 0) {
        //TODO SEND TRUE TO SERVER!
    }

    var pos = alt.Player.local.pos;
    var forward = game.getEntityForwardVector(alt.Player.local.scriptID);

    var fx = pos.x + forward.x * FISHING_FORWARD;
    var fy = pos.y + forward.y * FISHING_FORWARD;
    var fz0 = -10000;
    var fz1 = 10000;

    var floatPos = game.testProbeAgainstWater(fx, fy, fz0, fx, fy, fz1);
    if(!floatPos[0]) {
        isFishing = false;
        alt.emitServer("STOP_FISHING", false);
        return;
    }

    alt.setTimeout(() => {
        var countMax = CATCH_INTERVAL_BASE * (1 - (level / (MAX_FISH_LEVEL + 1)));
        var count = 0;
        var particleCount = 0;

        var int = alt.setInterval(() => {
            count++;
            particleCount++;

            //Check if fish is hooked
            if(game.isDisabledControlJustPressed(0, 24)) {
                //alt.clearInterval(int);
                //alt.emitServer("STOP_FISHING", true);
                //return;
                //hookFish(level);
                level++;
            }
            
            //Didnt hook fish
            if(count >= countMax) {
                alt.clearInterval(int);
                alt.emitServer("STOP_FISHING", false);
                isFishing = false;
                return;
            }

            if(particleCount >= 50) {
                particleCount = 0;
                game.useParticleFxAsset('core');
                game.startParticleFxNonLoopedAtCoord("ent_sht_water_tower", floatPos[1].x, floatPos[1].y, floatPos[1].z - scaleToRange(level, 1, 5, 0.5, 1.2), 0, 0, 0, scaleToRange(level, 1, 5, 2, 4), 0, 0, 0);
            }
            
        }, 1);
    }, timeout);
}

alt.everyTick(() => {
    if(isFishing) {
        game.disableAllControlActions(0);
        game.disableAllControlActions(1);
    }
});

function scaleToRange(value, fromMin, fromMax, toMin, toMax) {
    return ((toMax - toMin) * (value - fromMin) / (fromMax - fromMin)) + toMin;
}

//Fish needs to be reeled In
function hookFish(level) {
    // Versch. "Level"
    // Jedes Level Fisch geht in eine Richtung, Spieler muss in andere Richtung angel bewegen
    // Das Wiederholt sich bis zu 3mal pro level

    var layerCount = 0;
    var layerMax = level;

    var int = alt.setInterval(() => { 

    });

    isFishing = false;
}

function fishFight(startPos, forward, level) {
    var sig = (Math.random() * 2) - 1;
    var stepMax = getRandomInt(7, 20);

    var pX = startPos.x;
    var pY = startPos.y;

    var count = 0;
    alt.everyTick(() => {
        count++;
        if(count >= 25) {
            count = 0;
            game.useParticleFxAsset('core');
    
            pX += -forward.y * sig;
            pY += forward.x * sig;
    
            if(stepCount <= stepMax) {
                stepCount += 1;
            }
            
            game.startParticleFxNonLoopedAtCoord("ent_sht_water_tower", pX, pY, pos.z - 0.5, 0, 0, 0, 3, 0, 0, 0);
        }
    });   
}

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min)) + min;
  }