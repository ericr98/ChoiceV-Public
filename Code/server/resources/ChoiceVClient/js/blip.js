/// <reference path="../types/alt-client.d.ts" />
/// <reference path="../types/alt-shared.d.ts" />
/// <reference path="../types/natives.d.ts" />
import * as alt from 'alt-client';
import * as game from 'natives';

import { log } from 'js/client.js';

var blips = new Map();
var totalBlips = 0;

var activeRouteBlipId = -1;
var activeRouteIntervalId = -1;

/**
 * @param {any[] | Map<any, any>} map
 * @param {string} searchValue
 */
function getByValue(map, searchValue) {
    for (let [key, value] of map.entries()) {
        if (value === searchValue)
            return key;
    }
}

/**
 * @param {number} x
 * @param {number} y
 * @param {number} z
 */
function destroyBlip(x, y, z) {
    //fixBlipPosition();
    for (var [value] of Object.entries(blips)) {
        //GETBLIPCOORDS DOESNT WORK! //https://github.com/altmp/altv-issues/issues/1365
        // @ts-ignore
        var pos = game.getBlipCoords(new Number(value));
        if (pos.x - 1 < x && pos.x + 1 > x && pos.y - 1 < y && pos.y + 1 > y && pos.z - 1 < z && pos.z + 1 > z) {
            log("Blip deleted: " + blip);
            game.removeBlip(value);
            blip.destroy();
            blips.delete(getByValue(blips, value))
        }
    }
}

/**
 * @param {string} uniqueID
 */
function destroyBlipByName(uniqueID) {
    log("destroyBlipByName: " + uniqueID)

    if (blips[uniqueID] !== undefined) {
        if(typeof(blips[uniqueID]) === 'number') {
            game.removeBlip(blips[uniqueID]);
        } else {
            blips[uniqueID].alpha = 0;
            blips[uniqueID].destroy();
        }
        blips.delete(uniqueID);
    }
}

/**
 * @param {any} x
 */
function getMapSize(x) {
    var len = 0;
    for (var count in x) {
        len++;
    }

    return len;
}

/**
 * @param {string} name
 * @param {number} x
 * @param {number} y
 * @param {number} z
 * @param {number} color
 * @param {number} sprite
 * @param {number} alpha
 * @param {boolean} flashes
 * @param {boolean} shortrange
 * @param {string} uniqueID
 */
function createPointBlip(name, x, y, z, color, sprite, alpha, flashes, shortrange, uniqueID) {
    if(blips[uniqueID]) {
        destroyBlipByName(uniqueID);
    }
    
    let blip = game.addBlipForCoord(x, y, z);
    game.setBlipSprite(blip, sprite);
    game.setBlipFlashes(blip, flashes);
    game.setBlipAsShortRange(blip, shortrange);
    game.beginTextCommandSetBlipName('STRING');
    game.addTextComponentSubstringPlayerName(name);
    game.endTextCommandSetBlipName(blip);
    game.setBlipColour(blip, color);
    game.setBlipAlpha(blip, alpha);

    if (uniqueID === undefined || uniqueID === null) {
        totalBlips += 1;
        uniqueID = `${totalBlips}`;
    }

    if (blips[uniqueID] !== undefined) {
        game.removeBlip(blips[uniqueID]);
    }

    blips[uniqueID] = blip;
}

/**
 * @param {string} name
 * @param {number} x
 * @param {number} y
 * @param {number} z
 * @param {alt.BlipColor} color
 * @param {alt.BlipSprite} sprite
 * @param {any} alpha
 * @param {boolean} flashes
 * @param {boolean} shortrange
 * @param {any} routecolor
 * @param {string} uniqueID
 */
function createRouteBlip(name, x, y, z, color, sprite, alpha, flashes, shortrange, routecolor, uniqueID) {
    if(activeRouteBlipId != -1 || activeRouteIntervalId != -1) {
        deleteRouteBlip();
    }

    const blipId = activeRouteBlipId = game.addBlipForCoord(x, y, z);
    game.setBlipColour(blipId, color);
    game.setBlipSprite(blipId, sprite);
    game.setBlipAlpha(blipId, alpha);
    game.setBlipFlashes(blipId, flashes);
    game.setBlipAsShortRange(blipId, shortrange);
    game.setBlipRouteColour(blipId, routecolor);
    game.setBlipDisplay(blipId, 2);
    game.setBlipRoute(blipId, true);
    game.beginTextCommandSetBlipName('STRING');
    game.addTextComponentSubstringPlayerName(name);
    game.endTextCommandSetBlipName(blipId);

    if(activeRouteIntervalId != -1) { deleteRouteBlip(); }

    activeRouteIntervalId = alt.setInterval(() => {
        const isWaypointActive = game.isWaypointActive();
        if (isWaypointActive) {
            deleteRouteBlip();
            return;
        }

        const playerPos = alt.Player.local.pos;
        const distance = game.getDistanceBetweenCoords(playerPos.x, playerPos.y, playerPos.z, x, y, z, true);
        if(distance <= 10) {
            deleteRouteBlip();
            return;
        }
    }, 1000)
}

function deleteRouteBlip() {
    game.setBlipRoute(activeRouteBlipId, false);
    game.removeBlip(activeRouteBlipId);
    alt.clearInterval(activeRouteIntervalId);

    activeRouteBlipId = -1;
    activeRouteIntervalId = -1;
}

/**
 * 
 * @param {number} x 
 * @param {number} y 
 */
function createWaypointBlip(x, y) {
    game.setNewWaypoint(x, y);
}

/**
 * @param {number} x
 * @param {number} y
 * @param {number} z
 * @param {number} width
 * @param {number} height
 * @param {alt.BlipColor} color
 * @param {number} alpha
 * @param {string} [uniqueID]
 */
function createAreaBlip(x, y, z, width, height, color, alpha, uniqueID) {
    var blip = new alt.AreaBlip(x, y, z, width, height);

    blip.color = color;
    blip.alpha = alpha;

    if (uniqueID === undefined || uniqueID === null) {
        totalBlips += 1;
        uniqueID = `${totalBlips}`;
    }

    if (blips[uniqueID] !== undefined) {
        blips[uniqueID].alpha = 0;
        blips[uniqueID].destroy();
    }

    blips[uniqueID] = blip;
}

/**
 * @param {number} x
 * @param {number} y
 * @param {number} z
 * @param {number} radius
 * @param {alt.BlipColor} color
 * @param {number} alpha
 * @param {string} [uniqueID]
 */
function createRadiusBlip(x, y, z, radius, color, alpha, uniqueID) {
    var blip = new alt.RadiusBlip(x, y, z, radius);
    blip.color = color;
    blip.alpha = alpha;


    // var blip = game.addBlipForRadius(x, y, z, radius);

    // game.setBlipColour(blip, color);
    // game.setBlipAlpha(blip, alpha);

    if (uniqueID === undefined || uniqueID === null) {
        totalBlips += 1;
        uniqueID = `${totalBlips}`;
    }

    if (blips[uniqueID] !== undefined) {
        blips[uniqueID].alpha = 0;
        blips[uniqueID].destroy();
    }

    blips[uniqueID] = blip;
}

alt.onServer("CREATE_POINT_BLIP", (name, x, y, z, color, sprite, alpha, flashes, shortrange, uniqueID) => {
    createPointBlip(name, x, y, z, color, sprite, alpha, flashes, shortrange, uniqueID);
});

alt.onServer("CREATE_ROUTE_BLIP", (name, x, y, z, color, sprite, alpha, flashes, shortrange, routecolor, uniqueID) => {
    createRouteBlip(name, x, y, z, color, sprite, alpha, flashes, shortrange, routecolor, uniqueID);
});

alt.onServer("CREATE_AREA_BLIP", (x, y, z, width, height, color, alpha) => {
    createAreaBlip(x, y, z, width, height, color, alpha);
});

alt.onServer("CREATE_RADIUS_BLIP", (x, y, z, radius, color, alpha, uniqueID) => {
    createRadiusBlip(x, y, z, radius, color, alpha, uniqueID);
});

alt.onServer("CREATE_WAYPOINT_BLIP", (x, y) => {
    createWaypointBlip(x, y);
});

alt.onServer("DESTROY_BLIP", (x, y, z) => {
    destroyBlip(x, y, z);
});

alt.onServer("DESTROY_BLIP_NAME", (uniqueID) => {
    destroyBlipByName(uniqueID);
});

alt.setInterval(() => {
    const blips = alt.Blip.all;
    blips.forEach(blip => {
        blip.pos = new alt.Vector3(blip.pos.x, blip.pos.y, blip.pos.z);
    });
}, 1000);