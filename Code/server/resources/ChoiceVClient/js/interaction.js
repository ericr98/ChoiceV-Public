import * as alt from 'alt'; 
import * as game from 'natives'; 
import * as math from 'js/math.js';

export var interactableObjects = new Set();

alt.onServer("SET_INTERACTABLE_OBJECTS", (hashes) => {
    hashes.forEach((el) => {
        interactableObjects.add(parseInt(el));
    })
});

alt.onServer("TOGGLE_OBJECT_INTERACTION", () => {
    initiateEntitySelection();
});

alt.onServer("CHECK_INTERACTION_OBJECT", () => {
    var pos = alt.Player.local.pos;

    var founds = [];
    for(var el of interactableObjects) {
        var obj = game.getClosestObjectOfType(pos.x, pos.y, pos.z, 3, el, false, false, false);
        if(obj != 0) {        
            var objPos = game.getEntityCoords(obj, true);

            founds.push({obj: obj, model: el, pos: objPos});
        }
    };

    var closest = null;
    var currentDist = 10000;
    for(var found of founds) {
        var dist = 0;
        if(closest != null) {
            dist = getDistance(found.pos.x, found.pos.y, closest.pos.x, closest.pos.y);
        }

        if(closest == null || dist < currentDist) {
            closest = found;
            currentDist = dist;
        }
    }

    if(closest != null) {
        var heading = game.getEntityHeading(closest.obj);
        var broken = game.hasObjectBeenBroken(closest.obj, 0);

        alt.emitServer("OBJECT.INTERACTION", closest.model, JSON.stringify(closest.pos), JSON.stringify({x:0, y:0, z:0}), heading, JSON.stringify(game.getEntityRotation(closest.obj, 0)), broken.toString(), true);
    }
});

var entitySelectionToggle = false;

export function initiateEntitySelection() {
    entitySelectionToggle = !entitySelectionToggle;
    game.setMouseCursorStyle(1);
}

var registerObjectMode = false;

alt.onServer("REGISTER_OBJECT_MODE", (toogle) => {
    registerObjectMode = toogle;
});

let startPosition = game.getPedBoneCoords(alt.Player.local.scriptID, 12844, 0.2, 0, 0);
let selectedHitData;
let lastSelectedEntity = null;
let selectedEntity = null;

var maxTick = 10;
var tickCount = 0;

alt.everyTick(() => {
    //Has to be checked every Frame, or the cursor blinks
    if(entitySelectionToggle) {
        game.setMouseCursorThisFrame();
        game.disableAllControlActions(0);
        game.disableAllControlActions(1);
    }
    
    if(game.isDisabledControlJustPressed(0, 25) && selectedEntity != null) {
        // //Is not entity pos, is pos the ray hit the object!
        // //var pos = selectedHitData[2];
        // var pos = game.getEntityCoords(selectedHitData[4], true);

        // alt.setTimeout(() => {
        //     entitySelectionToggle = false;
        // }, 200);

        // if(doorMode) {
        //     alt.emitServer("REGISTER_DOOR", pos.x, pos.y, pos.z, game.getEntityModel(selectedEntity), groupId); 
        // } else {
        //     alt.emitServer("INTERACT_WITH_DOOR", pos.x, pos.y, pos.z);
        // }
    }

    //Different hightlight method!
    if(selectedEntity != null) {
        var ppos = alt.Player.local.pos;
        var pos = game.getEntityCoords(selectedEntity, true);
        // var rot = math.degreesToRadians(game.getEntityHeading(selectedEntity));
        var [_, min, max] = game.getModelDimensions(game.getEntityModel(selectedEntity));

        var pos = game.getOffsetFromEntityInWorldCoords(selectedEntity, (min.x + max.x) / 2, (min.y + max.y) / 2, (min.z + max.z) / 2);
        var midX = pos.x;
        var midY = pos.y;
        var midZ = pos.z;

        // var midX = (pos.x + min.x + pos.x + max.x) / 2;
        // var midY = (pos.y + min.y + pos.y + max.y) / 2;
        // var midZ = (pos.z + min.z + pos.z + max.z) / 2;

        // if(Math.abs(midZ - ppos.z) > 2.5) {
        //     midZ = ppos.z + 0.75;
        // }

        game.setDrawOrigin(midX, midY, midZ, 0);
        game.requestStreamedTextureDict("helicopterhud", false);
        game.drawSprite("helicopterhud", "hud_corner", -0.01, -0.01, 0.022, 0.022, 0.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", 0.01, -0.01, 0.022, 0.022, 90.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", -0.01, 0.01, 0.022, 0.022, 270.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", 0.01, 0.01, 0.022, 0.022, 180.0, 204, 138, 37, 255, true, 1)
        game.clearDrawOrigin()
    }

    //Disabled, because of game.disableAllControlActions(0) and game.disableAllControlActions(1); Has to be in everyTick, beacause it wont work otherwise
    if (game.isDisabledControlJustPressed(0, 24) && selectedEntity != null) {
        var pos = game.getEntityCoords(selectedHitData[4], true);
        var player = alt.Player.local;
        var heading = game.getEntityHeading(selectedEntity);
        var type = game.getEntityType(selectedEntity);
        switch(type){
            case 0: //NO ENtity Or Interior
                var modelHash = game.getEntityModel(selectedEntity);
                var offset = selectedHitData[3];
                var broken = game.hasObjectBeenBroken(selectedEntity, 0);
                alt.emitServer("OBJECT.INTERACTION", modelHash, JSON.stringify(pos), JSON.stringify(offset), heading, JSON.stringify(game.getEntityRotation(selectedEntity, 0)), broken.toString(), false);
                registerObjectMode = false;
                break;
            case 1: //PED
                    var player = null;
                    alt.Player.all.forEach((el) => {
                        if(el.scriptID == selectedEntity) {
                            player = el;
                        }
                    });
                    
                    if(player != null) {
                        alt.emitServer("ENTITY.INTERACTION", player);
                    } else {
                        var modelHash = game.getEntityModel(selectedEntity);
                        alt.emitServer("PED.INTERACTION", modelHash, JSON.stringify(pos));
                    }
                    break;
                case 2: //Vehicle
                    var veh = null;
                    alt.Vehicle.all.forEach((el) => {
                        if(el.scriptID == selectedEntity) {
                            veh = el;
                        }
                    });
                    alt.emitServer("ENTITY.INTERACTION", veh);
                    break;
                case 3:
                    var modelHash = game.getEntityModel(selectedEntity);
                    var offset = selectedHitData[3];
                    var broken = game.hasObjectBeenBroken(selectedEntity, 0);
                    alt.emitServer("OBJECT.INTERACTION", modelHash, JSON.stringify(pos), JSON.stringify(offset), heading, JSON.stringify(game.getEntityRotation(selectedEntity, 0)), broken.toString(), false);

                    break;
            }

            entitySelectionToggle = false;
        }

    tickCount++;

    if(tickCount > maxTick) {
        tickCount = 0;

        //Cast Shape only every 5 ms, so the performance is not that bad
        onTick();
    }
});

function onTick() {
    if (lastSelectedEntity != null) {
        game.setEntityAlpha(lastSelectedEntity, 255, 0);
        lastSelectedEntity = null;
    }

    if (entitySelectionToggle) {
        var screen = game.getActualScreenResolution();
        let camPos = game.getGameplayCamCoord();

        //Disabled Control, because of game.disableAllControlActions!
        var x = game.getDisabledControlNormal(0, 239);
        var y = game.getDisabledControlNormal(0, 240);

        //startPosition = game.getPedBoneCoords(alt.Player.local.scriptID, 12844, 0.5, 0, 0);
        startPosition = alt.screenToWorld(screen[1] * x, screen[2] * y);

        var dir = { x: startPosition.x - camPos.x, y: startPosition.y - camPos.y, z: startPosition.z - camPos.z }
        var from = { x: startPosition.x + dir.x * 0.05, y: startPosition.y  + dir.y * 0.05, z: startPosition.z  + dir.z * 0.05 }
        let to = { x: startPosition.x + dir.x * 300, y: startPosition.y  + dir.y * 300, z: startPosition.z  + dir.z * 300 }

        //extended.Get3DFrom2D(screen[1] * x, screen[2] * y, (result) => {
        //let cursorPosition = result;
        //let ray = game.startShapeTestCapsule(startPosition.x, startPosition.y, startPosition.z, cursorPosition.x, cursorPosition.y, cursorPosition.z, 0.15, -1, alt.Player.local.scriptID, 7);
        let ray = game.startExpensiveSynchronousShapeTestLosProbe(from.x, from.y, from.z, to.x, to.y, to.z, -1, alt.Player.local.scriptID, 7);
        let hitData = game.getShapeTestResult(ray, null, null, null, null);

        //game.drawLine(from.x, from.y, from.z, to.x, to.y, to.z, 255, 255, 255, 255);
        // game.drawLine(startPosition.x, startPosition.y, startPosition.z, cursorPosition.x + 0.1, cursorPosition.y, cursorPosition.z, 255, 255, 255, 255); 
        // game.drawLine(startPosition.x, startPosition.y, startPosition.z, cursorPosition.x - 0.1, cursorPosition.y, cursorPosition.z, 255, 255, 255, 255); 
        // game.drawLine(startPosition.x, startPosition.y, startPosition.z, cursorPosition.x, cursorPosition.y + 0.1, cursorPosition.z, 255, 255, 255, 255); 
        // game.drawLine(startPosition.x, startPosition.y, startPosition.z, cursorPosition.x, cursorPosition.y - 0.1, cursorPosition.z, 255, 255, 255, 255); 

        if (hitData[1]) {
            if (selectedEntity == hitData[4]) {
                return;
            }

            var pos1 = game.getEntityCoords(hitData[4], true);
            var pos2 = alt.Player.local.pos;

            if (getDistance(pos1.x, pos1.y, pos2.x, pos2.y) <= 7.5) {
                var modelHash = game.getEntityModel(hitData[4]);
                var type = game.getEntityType(hitData[4]);
                if (type === 1 || type === 2 || registerObjectMode || interactableObjects.has(modelHash)) {
                    lastSelectedEntity = selectedEntity;
                    selectedEntity = hitData[4];
                    selectedHitData = hitData;
                    //game.setEntityAlpha(selectedEntity, 190, 0);
                    game.setMouseCursorStyle(4);
                    return;
                }
            }
        }

        selectedEntity = null;
            game.setMouseCursorStyle(1);
        //});
    } else {
        if(selectedEntity != null) {
            //game.setEntityAlpha(selectedEntity, 255, 0);
            game.setMouseCursorStyle(1);
            selectedEntity = null;
        }
    }
}

function getDistance(x1, y1, x2, y2) {
    var a = x1 - x2;
    var b = y1 - y2;

    return Math.sqrt(a*a + b*b); 
}

//Goes through ground(but who cares)
function screen2dToWorld3d(absoluteX, absoluteY) {
    const camPos = game.getGameplayCamCoords();
    const { x: rX, y: rY } = processCoordinates(absoluteX, absoluteY);
    const target = s2w(camPos, rX, rY);

    return target;

    // const dir = sub(target, camPos);
    // const from = add(camPos, mulNumber(dir, 0.5));
    // const to = add(camPos, mulNumber(dir, 300));

    // const ray = game.startShapeTestCapsule(from.X, from.Y, from.Z, to.X, to.Y, to.Z, 0.1, 1, null, 7);
    // var res = game.getShapeTestResult(ray, null, null, null, null);
    // game.drawLine(pos.x, pos.y, pos.z, from.x, from.y, from.z, 255, 255, 255, 255);
    // return res === undefined ? undefined : res[2];

}

function s2w(camPos, relX, relY) {
    const camRot = game.getGameplayCamRot(2);
    const camForward = rotationToDirection(camRot);

    const rotUp = add(camRot, new math.Vector3(10, 0, 0));
    const rotDown = add(camRot, new math.Vector3(-10, 0, 0));
    const rotLeft = add(camRot, new math.Vector3(0, 0, -10));
    const rotRight = add(camRot, new math.Vector3(0, 0, 10));

    const camRight = sub(rotationToDirection(rotRight), rotationToDirection(rotLeft));
    const camUp = sub(rotationToDirection(rotUp), rotationToDirection(rotDown));

    const rollRad = -degToRad(camRot.y);

    const camRightRoll = sub(mulNumber(camRight, Math.cos(rollRad)), mulNumber(camUp, Math.sin(rollRad)));
    const camUpRoll = add(mulNumber(camRight, Math.sin(rollRad)), mulNumber(camUp, Math.cos(rollRad)));

    const point3D = add(
        add(
            add(camPos, mulNumber(camForward, 10.0)),
            camRightRoll
        ),
        camUpRoll);

    const point2D = w2s(point3D);

    if (point2D === undefined) {
        return add(camPos, mulNumber(camForward, 10.0));
    }

    const point3DZero = add(camPos, mulNumber(camForward, 10.0));
    const point2DZero = w2s(point3DZero);

    if (point2DZero === undefined) {
        return add(camPos, mulNumber(camForward, 10.0));
    }


    const eps = 0.001;

    if (Math.abs(point2D.x - point2DZero.x) < eps || Math.abs(point2D.y - point2DZero.y) < eps) {
        return add(camPos, mulNumber(camForward, 10.0));
    }

    const scaleX = (relX - point2DZero.x) / (point2D.x - point2DZero.x);
    const scaleY = (relY - point2DZero.y) / (point2D.y - point2DZero.y);

    const point3Dret = add(
        add(
            add(camPos, mulNumber(camForward, 10.0)),
            mulNumber(camRightRoll, scaleX)
        ),
        mulNumber(camUpRoll, scaleY));


    return point3Dret;
}

function processCoordinates(x, y) {
    const screenX = game.getActualScreenResolution(null, null)[1];
    const screenY = game.getActualScreenResolution(null, null)[2];

    let relativeX = (1 - ((x / screenX) * 1.0) * 2);
    let relativeY = (1 - ((y / screenY) * 1.0) * 2);

    if (relativeX > 0.0) {
        relativeX = -relativeX;
    } else {
        relativeX = Math.abs(relativeX);
    }

    if (relativeY > 0.0) {
        relativeY = -relativeY;
    } else {
        relativeY = Math.abs(relativeY);
    }

    return { x: relativeX, y: relativeY };
}

function w2s(position) {
    const result = game.getScreenCoordFromWorldCoord(position.x, position.y, position.z, null, null);

    if (result === undefined) {
        return undefined;
    }

    //return new Vector3((result.x - 0.5) * 2, (result.y - 0.5) * 2, 0);
    return new math.Vector3((result[1] - 0.5) * 2, (result[2] - 0.5) * 2, 0);
}

function rotationToDirection(rotation) {
    const z = degToRad(rotation.z);
    const x = degToRad(rotation.x);
    const num = Math.abs(Math.cos(x));

    return new math.Vector3((-Math.sin(z) * num), (Math.cos(z) * num), Math.sin(x));
}

function degToRad(deg) {
    return deg * Math.PI / 180.0;
}

function add(vector1, vector2) {
    return new math.Vector3(vector1.x + vector2.x, vector1.y + vector2.y, vector1.z + vector2.z);
}

function sub(vector1, vector2) {
    return new math.Vector3(vector1.x - vector2.x, vector1.y - vector2.y, vector1.z - vector2.z);
}

function mulNumber(vector1, value) {
    return new math.Vector3(vector1.x * value, vector1.y * value, vector1.z * value);
}
