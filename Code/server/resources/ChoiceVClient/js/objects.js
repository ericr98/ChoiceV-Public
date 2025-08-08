/// <reference path="../types/alt.d.ts" />
/// <reference path="../types/altv.d.ts" />

import alt from 'alt';
import game, { disableAimCamThisUpdate } from 'natives';
import * as math from 'js/math.js';
import { loadModel } from 'js/entity.js';

import { log } from 'js/client.js';

var objectLoadDistance = 200;
var objects = [];

export function onTick() {
    var pos = alt.Player.local.pos;

    objects.forEach((el) => {
        el.check(pos.x, pos.y);
    });
}

// Objects

class Object {
    constructor(id, model, posX, posY, posZ, lodDist, col, pitch, roll, yaw, placeOnGroundProperly, placeOnGroundZOffset) {
        this.id = id;
        this.model = model;
        this.x = posX;
        this.y = posY;
        this.z = posZ;
        this.lodDist = lodDist;
        this.col = col;
        this.pitch = pitch;
        this.roll = roll;
        this.yaw = yaw;
        this.loaded = false;
        this.isLoading = false;

        this.attach = false;
        this.rotation = false;
        this.heading = false;

        this.sameDimension = true;

        this.placeOnGroundProperly = placeOnGroundProperly;
        this.placeOnGroundZOffset = placeOnGroundZOffset;

        log("OBJECT", `Created: id: ${this.id}, model: ${this.model}, position: x: ${this.x}, y: ${this.y}, z: ${this.z}, rotation: pitch: ${this.pitch}, roll: ${this.roll}, yaw: ${this.yaw}, placeOnGroundProperly: ${this.placeOnGroundProperly}`);
    }

    check(x, y) {
        if ((getDistance(this.x, this.y, x, y) < objectLoadDistance && getDistance(this.x, this.y, x, y) < this.lodDist) || this.lodDist == -1 && this.sameDimension) {
            this.spawn(this.model, this.x, this.y, this.z, this.lodDist, this.col, this.pitch, this.roll, this.yaw);
        } else {
            //Only remove 
            if(this.loaded) {
                log("OBJECT", `Renderout: id: ${this.id}, model: ${this.model}, gtaId: ${this.obj}`);
                this.remove();
                this.loaded = false;
            }
        }

        if (this.loaded) {
            if (this.attach && !this.hasAttach) {
                this.attachToPlayer(this.aPlayer, this.aBone, this.aOffX, this.aOffY, this.aOffZ, this.aRotX, this.aRotY, this.aRotZ, this.aVertexOrder);
                this.hasAttach = true;
            }

            if (this.rotation && !this.hasRotation) {
                this.setRotation(this.rPitch, this.rRoll, this.rYaw);
                this.hasRotation = true;
            }

            if (this.heading && !this.hasHeading) {
                this.setHeading(this.hHeading);
                this.hasHeading = true;
            }
        }
    }

    spawn(model, posX, posY, posZ, lodDist, col, pitch, roll, yaw) {
        if (!this.loaded && !this.isLoading) {
            this.isLoading = true;

            if (!game.hasModelLoaded(alt.hash(model))) {
                loadModel(alt.hash(model)).then(() => {
                    this.spawnObjectNoLoad(model, posX, posY, posZ, lodDist, col, pitch, roll, yaw);
                });
            } else {
                this.spawnObjectNoLoad(model, posX, posY, posZ, lodDist, col, pitch, roll, yaw);
            }
        }
    }

    spawnObjectNoLoad(model, posX, posY, posZ, lodDist, col, pitch, roll, yaw) {
        var zCoord = posZ;
        if(this.placeOnGroundProperly) {
            const ground = game.getGroundZFor3dCoord(posX, posY, posZ + 0.1, false, false);
        
            if(ground[0]) {
                zCoord = ground[1] + this.placeOnGroundZOffset;
            } else {
                const ground2 = game.getGroundZFor3dCoord(posX, posY, posZ + 1, false, false);
        
                if(ground2[0]) {
                    zCoord = ground2[1] + this.placeOnGroundZOffset;
                } else {
                    log("OBJECT", `Object with id: ${this.id}, model: ${this.model} should be placed on ground properly, but no z-coord could be found`)
                }
            }
        }
        
        var obj = game.createObjectNoOffset(alt.hash(model), posX, posY, zCoord, true, true, false);
        this.obj = obj;
        game.setEntityCollision(obj, col, false);
        game.setEntityInvincible(obj, true);
        game.freezeEntityPosition(obj, true);
        
        alt.setTimeout(() => {
            if (lodDist != -1) {
                game.setEntityLodDist(obj, lodDist);
            }

            game.setEntityAsMissionEntity(obj, false, false);
            game.setEntityRotation(obj, pitch, roll, yaw, 2, true);
            game.setEntityCollision(obj, col, false);
            game.setEntityInvincible(obj, true);
            game.freezeEntityPosition(obj, true);

            log("OBJECT", `Spawned: id: ${this.id}, model: ${this.model}, gtaId: ${this.obj}`);
     
            this.isLoading = false;
            this.loaded = true;
        }, 100);
    }

    remove() {
        log("OBJECT", `Deleted: id: ${this.id}, model: ${this.model}, gtaId: ${this.obj}`);
        if (this.obj != null) {
            game.detachEntity(this.obj, false, false);

            game.setEntityAsNoLongerNeeded(this.obj);
            game.setEntityCoords(this.obj, 0, 0, 0, 1, 0, 0, 1);
            game.setEntityVisible(this.obj, false, false);
            

            game.deleteObject(this.obj);
            game.deleteEntity(this.obj);
        }

        this.hasAttach = false;
        this.hasHeading = false;
        this.hasRotation = false;
    }

    attachToPlayer(player, bone, posX, posY, posZ, rotX, rotY, rotZ, attachVertexOrder) {
        this.attach = true;
        this.aPlayer = player;
        this.aBone = bone;
        this.aOffX = posX;
        this.aOffY = posY;
        this.aOffZ = posZ;
        
        this.aRotX = rotX;
        this.aRotY = rotY;
        this.aRotZ = rotZ;

        this.aVertexOrder = attachVertexOrder;

        if (this.loaded && !this.isLoading) {
            if(!this.hasAttach) {
                alt.setTimeout(() => {
                    var boneIndex = game.getPedBoneIndex(player.scriptID, bone);
                    log("OBJECT", `Attached: id: ${this.id}, model: ${this.model}, gtaId: ${this.obj} rotation: pitch: ${rotX}, roll: ${rotY}, yaw: ${rotZ}`); 
                    game.attachEntityToEntity(this.obj, player.scriptID, boneIndex, posX, posY, posZ, rotX, rotY, rotZ, 1, 0, 0, 0, attachVertexOrder, 1, 1);
                }, 500);

                this.hasAttach = true;
            }
        }
    }

    reattachToPlayer(player, bone, posX, posY, posZ, rotX, rotY, rotZ) {
        if (this.loaded && !this.isLoading) {
            if (game.isEntityAttached(this.obj)) {
                game.detachEntity(this.obj, false, false);
            }

            var boneIndex = game.getPedBoneIndex(player.scriptID, bone);
            game.attachEntityToEntity(this.obj, player.scriptID, boneIndex, posX, posY, posZ, rotX, rotY, rotZ, 1, 1, 0, 0, this.aVertexOrder, 1, 1);
        }
    }

    setRotation(pitch, roll, yaw) {
        if (this.loaded) {
            game.setEntityRotation(this.obj, pitch, roll, yaw, 2, true);
        }

        this.heading = false;
        this.rotation = true;
        this.hasRotation = false;

        this.rPitch = pitch;
        this.rRoll = roll;
        this.rYaw = yaw;
    }

    setHeading(heading) {
        if (this.loaded) {
            game.setEntityHeading(this.obj, heading);
        }

        this.rotation = false;
        this.heading = true;
        this.hasHeading = false;

        this.hHeading = heading;
    }

    updatePosition(x, y, z) {
        this.x = x;
        this.y = y;
        this.z = z;

        log("OBJECT", `Updated object position: id: ${this.id}, model: ${this.model}, gtaId: ${this.obj} to x: ${this.x}, y: ${this.y}, z: ${this.z}`); 
    }
}

function getDistance(x1, y1, x2, y2) {
    var a = x1 - x2;
    var b = y1 - y2;

    return Math.sqrt(a * a + b * b);
}

alt.onServer('CREATE_OBJECT', (id, model, posX, posY, posZ, lodDist, col, pitch, roll, yaw, placeOnGroundProperly, placeOnGroundZOffset) => {
    var obj = new Object(id, model, posX, posY, posZ, lodDist, col, pitch, roll, yaw, placeOnGroundProperly, placeOnGroundZOffset);
    objects.push(obj);

    var pos = alt.Player.local.pos;
    obj.check(pos.x, pos.y);
});

alt.onServer('DELETE_OBJECT', (id) => {
    tryDeleteObject(id, 0);
});

function tryDeleteObject(id, tries) {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null) {
        obj.remove();
        for (var i = 0; i < objects.length; i++) {
            if (objects[i].id === id) {
                objects.splice(i, 1);
                return;
            }
        }
    } else {
        if(tries < 4) {
            alt.setTimeout(() => {
                tryDeleteObject(id, tries + 1);
            }, 1000);
        } else {
            alt.logError("OBJECT: " + id + " could not be found, when trying to delete!");
        }
    }
}

alt.onServer('ATTACH_OBJECT_TO_PLAYER', (id, player, bone, posX, posY, posZ, rotX, rotY, rotZ, attachVertexOrder) => {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null) {
        obj.attachToPlayer(player, bone, posX, posY, posZ, rotX, rotY, rotZ, attachVertexOrder);
    }
});

alt.onServer('REATTACH_OBJECT_TO_PLAYER', (id, player, bone, posX, posY, posZ, rotX, rotY, rotZ) => {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null) {
        obj.reattachToPlayer(player, bone, posX, posY, posZ, rotX, rotY, rotZ);
    }
});

alt.onServer('CHANGE_OBJECT_SAME_DIMENSION', (id, sameDimension) => {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null) {
        obj.sameDimension = sameDimension;
    }
});

alt.onServer('UPDATE_OBJECT_POSITION', (id, x, y, z) => {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null) {
        obj.updatePosition(x, y, z);
    }
});

alt.onServer('ROTATE_OBJECT', (id, pitch, roll, yaw) => {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null) {
        obj.setRotation(pitch, roll, yaw);
    }
});

alt.onServer('HEADING_OBJECT', (id, heading) => {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null) {
        obj.setHeading(heading);
    }
});

alt.onServer('LOG_OBJECTS', () => {
    alt.log(JSON.stringify(objects));
});

// Moving Objects

export class MoveObject {
    constructor(obj, id, toX, toY, toZ, speedX, speedY, speedZ, collision) {
        this.obj = obj;
        this.id = id;
        this.toX = toX;
        this.toY = toY;
        this.toZ = toZ;
        this.speedX = speedX;
        this.speedY = speedY;
        this.speedZ = speedZ;
        this.collision = collision;
    }
}

var moveObjs = [];

alt.onServer('MOVE_OBJECT', (id, toX, toY, toZ, speedX, speedY, speedZ, collision) => {
    var obj = objects.filter((el) => {
        return id == el.id;
    })[0];

    game.freezeEntityPosition(obj.obj, false);
    log("OBJECT", toX + " " + toY + " " + toZ + " " + speedX + " " + speedY + " " + speedZ + " " + collision);

    moveObjs.push(new MoveObject(obj.obj, id, toX, toY, toZ, speedX, speedY, speedZ, collision));
});

alt.setInterval(() => {
    moveObjs.forEach(function (obj) {
        if (game.slideObject(obj.obj, obj.toX, obj.toY, obj.toZ, obj.speedX, obj.speedY, obj.speedZ, obj.collision)) {
            moveObjs.splice(moveObjs.indexOf(obj), 1);
            alt.emitServer('OBJECT_MOVED', obj.id, obj.obj);
        }
    });
}, 0);


var placerEveryTick = null;
var placerObj = null;
var placerHeading = null;
var placerPos = null;
alt.onServer('START_OBJECT_PLACE_MODE', (model, headingStartOffset, objectZOffset) => {
    if(placerEveryTick != null) {
        alt.clearEveryTick(placerEveryTick);
    }

    stopObjectPlacerMode();

    loadModel(alt.hash(model)).then(() => {
        var pos = alt.Player.local.pos;
        placerObj = game.createObjectNoOffset(alt.hash(model), pos.x, pos.y, pos.z, true, true, false);
        
        game.setEntityCollision(placerObj, false, false);
        game.setEntityInvincible(placerObj, true);
        game.freezeEntityPosition(placerObj, true);
        game.setEntityAlpha(placerObj, 120, false);

        var dimension = game.getModelDimensions(alt.hash(model));
        var zOffset = dimension[1].z;
        
        var heading = alt.Player.local.rot.z + headingStartOffset;
        const distance = 2; // distance from ped
    
        placerEveryTick = alt.everyTick(() => {
            //Disable all mouse wheel related controls
            game.disableControlAction(0, 14, true);
            game.disableControlAction(0, 15, true);
            game.disableControlAction(0, 16, true);
            game.disableControlAction(0, 17, true);
            game.disableControlAction(0, 27, true);
            game.disableControlAction(0, 50, true);
            game.disableControlAction(0, 99, true);

            if (game.isDisabledControlPressed(0, 241)) {
                heading = heading + 3 % 360;
            }
        
            if (game.isDisabledControlPressed(0, 242)) {
                heading = heading - 3 % 360;
            }
        
            const pos = alt.Player.local.pos;
            const rot = game.getGameplayCamRot(2).toRadians(); // natives return rotation in degrees
            const forward = rotationToDirection(rot);
            const forwardPos = pos.add(forward.mul(distance));  
        
            var resultPos = null
            var ground = game.getGroundZFor3dCoord(forwardPos.x, forwardPos.y, pos.z + 0.5, false, false);
        
            let ray = game.startExpensiveSynchronousShapeTestLosProbe(pos.x, pos.y, pos.z + 1.5, forwardPos.x, forwardPos.y, ground[1] + (dimension[2].z - dimension[1].z), 17, alt.Player.local.scriptID, 7);
            let hitData = game.getShapeTestResult(ray, null, null, null, null);

            if(!hitData[1] && ground[0] && Math.abs(ground[1] - zOffset - pos.z) < 2) {
                resultPos = new alt.Vector3(
                    forwardPos.x,
                    forwardPos.y,
                    ground[1] - zOffset + objectZOffset
                );
            }

            if(resultPos != null) {        
                game.setEntityAlpha(placerObj, 120, false);   
                game.setEntityCoords(placerObj, resultPos.x, resultPos.y, resultPos.z, 1, 0, 0, 1);
                game.setEntityHeading(placerObj, heading);

				log("OBJECT", "Coords: " + JSON.stringify(resultPos));

                placerHeading = heading;
                placerPos = resultPos;
    
            } else {
                game.setEntityAlpha(placerObj, 0, false);
                if(!hitData[1]) {
                    game.drawBox(
                        resultPos.x - 0.15, resultPos.y - 0.15, resultPos.z + 0.5 - 0.15,
                        resultPos.x + 0.15, resultPos.y + 0.15, resultPos.z + 0.5 + 0.15,
                        200, 12, 34, 150 
                    );
                } else {
                    resultPos = hitData[2];
                    game.drawBox(
                        resultPos.x - 0.15, resultPos.y - 0.15, resultPos.z + 0.5 - 0.15,
                        resultPos.x + 0.15, resultPos.y + 0.15, resultPos.z + 0.5 + 0.15,
                        200, 12, 34, 150 
                    );
                }
            }
        });
    });

});

function rotationToDirection(rot) {
    return new alt.Vector3(
        -Math.sin(rot.z) * Math.abs(Math.cos(rot.x)),
        Math.cos(rot.z) * Math.abs(Math.cos(rot.x)),
        Math.sin(rot.x),
    );
}

alt.onServer('STOP_OBJECT_PLACE_MODE', (callbackId) => {
    stopObjectPlacerMode();

    if(callbackId != -1) {
        alt.emitServer("ON_ANSWER_CALLBACK", callbackId, placerPos, placerHeading);
    }
});

function stopObjectPlacerMode() {
    if(placerEveryTick != null) {
        alt.clearEveryTick(placerEveryTick);
        placerEveryTick = null;
    }

    if(placerObj != null) {
        game.deleteObject(placerObj);
        game.deleteEntity(placerObj);
        placerObj = null;
    }
}

// Placer

var vhMode = false;
var rotMode = false;
var clientEventPrefix = "OP_";
var PosA = new math.Vector3();
var PosB = new math.Vector3();
var PosC = new math.Vector3();
var PosD = new math.Vector3();
var zVector = new math.Vector3(0, 0, 500);;
var QTZ = null;
var localMarkers = new Array();
var update = false;

alt.onServer("OP_START", (arg) => {
    vhMode = true;
    clientEventPrefix = arg || "OP_";
    return;
});

alt.onServer("OP_STOP", () => {
    PosA = null;
    update = false;
    return;
});

alt.onServer("OP_DRAW", (posA, posB, posC, posD) => {
    PosA = posA;
    PosB = posB;
    PosC = posC;
    PosD = posD;
    update = true;
    return;
});

alt.everyTick(() => {
    if (!update) return;

    if (PosA !== null) {
        game.drawLine(PosA.x, PosA.y, PosA.z, PosA.x + zVector.x, PosA.y + zVector.y, PosA.z + zVector.z, 255, 0, 255, 255);
        game.drawLine(PosB.x, PosB.y, PosB.z, PosB.x + zVector.x, PosB.y + zVector.y, PosB.z + zVector.z, 255, 0, 255, 0);
        game.drawLine(PosC.x, PosC.y, PosC.z, PosC.x + zVector.x, PosC.y + zVector.y, PosC.z + zVector.z, 255, 0, 0, 255);
        game.drawLine(PosD.x, PosD.y, PosD.z, PosD.x + zVector.x, PosD.y + zVector.y, PosD.z + zVector.z, 255, 255, 0, 0);

        // API.drawLine(PosA, PosA.Add(zVector), 255, 0, 255, 255);
        // API.drawLine(PosB, PosB.Add(zVector), 255, 0, 255, 0);
        // API.drawLine(PosC, PosC.Add(zVector), 255, 0, 0, 255);
        // API.drawLine(PosD, PosD.Add(zVector), 255, 255, 0, 0);
    }
});

alt.on('keyup', (key) => {
    if (vhMode) {
        var cmd = "";

        if (!rotMode) {
            switch (key) {
                case 111:
                    cmd = "R+"; break;
                case 106:
                    cmd = "R-"; break;
                case 37:
                    cmd = "Y+"; break;
                case 34:
                    cmd = "X+"; break;
                case 39:
                    cmd = "Y-"; break;
                case 35:
                    cmd = "X-"; break;
                case 38:
                    cmd = "Z+"; break;
                case 40:
                    cmd = "Z-"; break;
                case 12:
                    rotMode = true;
                    alt.log("~g~Rotation mode on.");
                    alt.emitServer(clientEventPrefix + "CMD", "ROT");
                    return;
                case 107:
                    alt.emitServer(clientEventPrefix + "CMD", "TOGGLE");
                    return;
                case 46:
                    alt.emitServer(clientEventPrefix + "ABORT"); vhMode = false; return;
                case 45:
                    alt.emitServer(clientEventPrefix + "SAVE"); vhMode = false; return;
            }
        } else {
            switch (key) {
                case 111:
                    cmd = "R+"; break;
                case 106:
                    cmd = "R-"; break;
                case 37:
                    cmd = "1Y+"; break;
                case 34:
                    cmd = "1X+"; break;
                case 39:
                    cmd = "1Y-"; break;
                case 35:
                    cmd = "1X-"; break;
                case 38:
                    cmd = "1Z+"; break;
                case 40:
                    cmd = "1Z-"; break;
                case 12:
                    rotMode = false;
                    alt.log("~g~Rotation mode OFF.");
                    alt.emitServer(clientEventPrefix + "CMD", "POS");
                    return;
                case 107:
                    alt.emitServer(clientEventPrefix + "CMD", "TOGGLE");
                    return;
                case 46:
                    alt.emitServer(clientEventPrefix + "ABORT"); vhMode = false; return;
                case 45:
                    alt.emitServer(clientEventPrefix + "SAVE"); return;
            }
        }

        if (cmd !== "")
            alt.emitServer(clientEventPrefix + "CMD", cmd);
        return;
    }
});
