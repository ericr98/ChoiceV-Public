/// <reference path="../types/alt-client.d.ts" />
/// <reference path="../types/alt-shared.d.ts" />
/// <reference path="../types/natives.d.ts" />
import * as alt from 'alt-client';
import * as game from 'natives';
import * as math from 'js/math.js';
import * as island from 'js/island.js';

import {
    useFireTruckWater,
    useFireTruckFoam
} from '/js/fire.js';

import {
    reactView
} from '/js/cef.js';

import {
    entitySpawned
} from 'js/entity.js';

import { log } from 'js/client.js';


var specialFlags = {
    IsRappelHelicopter: 1,
    IsEmergencyCarryVehicle: 2,
    IsFlatBed: 4,
    IsFileSystemOpenVehicle: 8,
    IsFireTruckWater: 16,
    IsFireTruckFoam: 32,
}

let toggleSpeedlimiter = false;

alt.onServer("SET_NUMBERPLATE", (vehicle, text) => {
    game.setVehicleNumberPlateText(vehicle.scriptID, text);
});

alt.onServer("VEHICLE_REPAIRED", (vehicle) => {
    game.setVehicleFixed(vehicle);
});

alt.onServer("VEHICLE_DESTROYED", (vehicle) => {
    for(var i = 0; i < 6; i++) {
        game.setVehicleDoorBroken(vehicle.scriptID, i, true);
    }
});

alt.onServer("ENTER_VEHICLE", (vehicle, seat) => {
    var mult = vehicle.getSyncedMeta("ENGINE_MULT");
    if(mult == null) {
        mult = 1;
    }

    enterVehicle(alt.Player.local, vehicle, seat);

    if (vehicle.model != "1938952078" && vehicle.model != "3016781352" && vehicle.model != "4265336708") { // Firetruck with water        
        //Disables all vehicle weapons
        for(var i = 0; i < 10; i++) {
            game.setVehicleWeaponRestrictedAmmo(vehicle, i, 0);
        }
    }
});

function enterVehicle(player, vehicle, seat) {
    var vcl = game.getVehicleClass(vehicle.scriptID);
    var roll = game.getEntityRoll(vehicle.scriptID);
    
    if (player.vehicle != null)
        return false;

    if ((roll > 75 || roll < -75) && vcl != 8 && vcl != 13)
        return false;


    if (game.isVehicleSeatFree(vehicle.scriptID, seat, false)) {
        var sea = game.getVehicleModelNumberOfSeats(vehicle.model);

        if ((vcl == 8 || vcl == 14) && sea == 2 && seat == 0)
            game.taskEnterVehicle(player.scriptID, vehicle.scriptID, -1, seat, 2, 1, "", 0);
            //game.setPedIntoVehicle(player.scriptID, vehicle.scriptID, seat);
        else
            game.taskEnterVehicle(player.scriptID, vehicle.scriptID, -1, seat, 2, 1, "", 0);

        return true;
    }

    return false;
}

alt.onServer("OPEN_VEHICLE_DOORS", () => {
	for(var i = 0; i <= 7; i++) {
        game.setVehicleDoorOpen(alt.Player.local.vehicle.scriptID, i, true, true);
    }
});

alt.onServer("SET_ENGINE_MULT", (mult) => {
	alt.Player.local.vehicle.handling.driveBiasRear = mult;
});

alt.onServer("SET_ENGINE_ACC", (modelName, mult) => {
    let handling = alt.HandlingData.getForModel(alt.hash(modelName));
    handling.driveBiasRear = mult;
});

alt.onServer("DETACH_VEHICLE_TRAILER", (vehicle) => {
    game.detachVehicleFromTrailer(vehicle);

    var towedVehicle = game.getEntityAttachedToTowTruck(vehicle);
    if(towedVehicle != 0) {
        game.detachVehicleFromTowTruck(vehicle, towedVehicle);
    }
});

alt.on('gameEntityCreate', (entity) => {
    if(entity.seatCount) {
        game.setVehicleDamageScale(entity.scriptID, 0.75);
    }

    if(entity.hasStreamSyncedMeta("VEHICLE_SIREN_SOUND")) {
        alt.setTimeout(() => {
            let sirenSound = entity.getStreamSyncedMeta("VEHICLE_SIREN_SOUND");
            game.setVehicleHasMutedSirens(entity.scriptID, !sirenSound);
        }, 50);
    }
});


alt.on('streamSyncedMetaChange', (entity, key, newValue, oldValue) => {
    if (entity instanceof alt.Vehicle) {
        var model = game.getEntityModel(entity); 
        if(key.startsWith('HANDLING_')) {
            var handlingData = alt.HandlingData.getForHandlingName(model);
            var split = key.split('_');
            var property = split[1];

            entity.handling[property] = handlingData[property] * newValue;
            log("VEHICLE", "Set " + property + " to " + entity.handling[property]);       
        }

        if(key === 'ENGINE_MULT') {
            var handlingData = alt.HandlingData.getForHandlingName(model);

            var accChange = entity.getStreamSyncedMeta("HANDLING_acceleration");
            if(accChange == null) {
                accChange = 1;
            }
            entity.handling.driveBiasRear = handlingData.driveBiasRear * accChange * newValue;
            log("VEHILCE", "Set acceleration to " + entity.handling.driveBiasRear);  

            var initialDriveMaxFlatVelChange = entity.getStreamSyncedMeta("HANDLING_initialDriveMaxFlatVel");
            if(initialDriveMaxFlatVelChange == null) {
                initialDriveMaxFlatVelChange = 1;
            }
            entity.handling.initialDriveMaxFlatVel = handlingData.initialDriveMaxFlatVel * initialDriveMaxFlatVelChange * newValue;
            log("VEHICLE", "Set initialDriveMaxFlatVel to " + entity.handling.initialDriveMaxFlatVel);  
        }

        if(key === 'VEHICLE_SIREN_SOUND') {
            alt.setTimeout(() => {
                let sirenSound = entity.getStreamSyncedMeta("VEHICLE_SIREN_SOUND");
                game.setVehicleHasMutedSirens(entity.scriptID, !sirenSound);
            }, 50);
        }
    }
});

alt.onServer("ATTACH_VEHICLE_TO_VEHICLE", (transporter, transported, bone, x, y, z, vertexIndex) => { 
    attachVehicleToVehicle(transporter, transported, bone, x, y, z, vertexIndex);
});

function attachVehicleToVehicle(transporter, transported, bone, x, y, z, vertexIndex) {
    game.attachEntityToEntity(transported, transporter, bone, x, y, z, 0.0, 0.0, 0.0, true, false, true, false, vertexIndex, true, 1);
}

alt.onServer("DETACH_VEHICLE", (vehicle) => {
    game.detachEntity(vehicle, false, false);
});

alt.onServer("VEHICLE_DOOR_TOGGLE", (veh, op, door) => {
    if(op) {
        game.setVehicleDoorOpen(veh.scriptID, door, false, false);
    } else {
        game.setVehicleDoorShut(veh.scriptID, door, false);
    }
});

alt.onServer("MAKE_VEHICLE_DISPLAY", (vehicle) => {
    var int = alt.setInterval(() => {
        try {
            var heading = game.getEntityHeading(vehicle.scriptID);
            game.setEntityHeading(vehicle.scriptID, heading + 0.25);
        }catch (e) {
            alt.clearInterval(int);
        }      
    }, 10);
});

alt.onServer("SET_PED_INTO_VEHICLE", (veh, seatIndex) => {
	alt.setTimeout(() => {
		game.setPedIntoVehicle(alt.Player.local.scriptID, veh.scriptID, seatIndex);
	}, 500);
});

alt.onServer("GET_VEHICLE_MODS_REQUEST", (id, vehicle, vehicleModKitsJson) => {
    const vehicleModKits = JSON.parse(vehicleModKitsJson);

    const result = [];
    for (let i = 0; i < vehicleModKits.length; i++) {
        const vehicleModelKit = vehicleModKits[i];
        vehicleModelKit.Mods = [];

        for (let modIndex = -1; modIndex < vehicleModelKit.ModsCount; modIndex++) {
            const modTextLabel = game.getModTextLabel(vehicle.scriptID, vehicleModelKit.ModTypeIndex, modIndex);
            var labelText = game.getFilenameForAudioConversation(modTextLabel);

            //Index -1 is always the default variation for the current ModType.
            if (modIndex === -1 && !labelText) {
                labelText = "Serie";
            }

            vehicleModelKit.Mods.push({ ModIndex: modIndex, ModDisplayName: labelText });
        }

        result.push(vehicleModelKit);
    }

    log("VEHICLE", "Vehicle Mods: " + JSON.stringify(result));
    alt.emitServer("ON_ANSWER_CALLBACK", id, vehicle, JSON.stringify(result));
});

alt.onServer("TEST_VEHICLE_INFO", (id, vehicle) => {
    var pos = vehicle.pos;
    var cl = game.getVehicleClass(vehicle.scriptID);
    var hash = game.getEntityModel(vehicle.scriptID);
    var [_, pos1, pos2] = game.getModelDimensions(hash);
    var arr = [];
    for(var i = 0; i < 100; i++) {
        if(game.doesExtraExist(vehicle.scriptID, i)) {
            arr.push(i);
        }
    }

    //Is WRONG for some vehicles. (e.g. bison)
    var seatAmount = game.getVehicleModelNumberOfSeats(hash);
    //var seatAmount = 9999;
    var relativeSeatList = [];
    var bon = ['seat_dside_f', 'seat_pside_f', 'seat_dside_r', 'seat_pside_r', 'seat_dside_r1', 'seat_pside_r1', 'seat_dside_r2', 'seat_pside_r2', 'seat_dside_r3', 'seat_pside_r3', 'seat_dside_r4', 'seat_pside_r4', 'seat_dside_r5', 'seat_pside_r5', 'seat_dside_r6', 'seat_pside_r6', 'seat_dside_r7', 'seat_pside_r7'];            
    
    var bonBike = ['seat_f', 'seat_r'];
    if(game.getEntityBoneIndexByName(vehicle.scriptID, 'seat_f') != -1) {
        bon = bonBike;
    }

    for (var i = 0; i < bon.length && relativeSeatList.length < seatAmount; i++) {
        var bix = game.getEntityBoneIndexByName(vehicle.scriptID, bon[i]);

        if (bix != -1) {
            var g = game.getWorldPositionOfEntityBone(vehicle.scriptID, bix);
            relativeSeatList.push(new math.Vector2(g.x, g.y))
        }
    };

    var windowCount = getVehicleWindowCount(vehicle);
    var doorCount = getVehicleDoorCount(vehicle);
    var tyreCount = getVehicleTyreCount(vehicle);
    var displayName = game.getFilenameForAudioConversation(game.getDisplayNameFromVehicleModel(game.getEntityModel(vehicle.scriptID)));

    alt.emitServer("ON_ANSWER_CALLBACK", id, cl, JSON.stringify(pos1), JSON.stringify(pos2), JSON.stringify(arr), seatAmount, JSON.stringify(relativeSeatList), windowCount, doorCount, tyreCount, displayName, vehicle);   
});

alt.onServer("TEST_VEHICLES_DAMAGE", (id, vehicle, windowCount) => {
    var windowList = [];

    for(var i = 0; i < windowCount; i++) {
        windowList.push(!game.isVehicleWindowIntact(vehicle.scriptID, i));
    }

    alt.emitServer("ON_ANSWER_CALLBACK", id, JSON.stringify(windowList), vehicle);   
});

function getVehicleWindowCount(veh) {
	var list = ["window_lf1", "window_lf2", "window_lf3", "window_rf1", "window_rf2",
	"window_rf3", "window_lr1", "window_lr2", "window_lr3", "window_rr1", "window_rr2", 
	"window_rr3", "windscreen", "windscreen_r", "window_lf", "window_rf", "window_lr", 
	"window_rr", "window_lm", "window_rm"];

	var windowCount = 0;
	list.forEach((el, id) => {
		var bix = game.getEntityBoneIndexByName(veh.scriptID, el);
		if(bix != -1) {
			windowCount++;
		}
	});

	return windowCount;
}

function getVehicleTyreCount(veh) {
	var list = ["wheel_lf", "wheel_rf", "wheel_lm1", "wheel_rm1", "wheel_lm2", "wheel_rm2",
	"wheel_lm3", "wheel_rm3", "wheel_lr", "wheel_rr"];

	var tyreCount = 0;
	list.forEach((el, id) => {
		var bix = game.getEntityBoneIndexByName(veh.scriptID, el);
		if(bix != -1) {
			tyreCount++;
		}
	});

	return tyreCount;
}

function getVehicleDoorCount(veh) {

	return game.getNumberOfVehicleDoors(veh.scriptID);
}

alt.on("gameEntityCreate", (entity) => {
    entitySpawned(entity).then(() => {
        if(entity.hasStreamSyncedMeta("DESTROYED_DOORS")) {
            alt.setTimeout(() => {
                var value = entity.getStreamSyncedMeta("DESTROYED_DOORS");
                if(value != "") {
                    value.split(",").forEach((el) => {
                        var id = parseInt(el);
                        log("VEHICLE", "Break vehicle door: " + entity.id + ", doorId: " + id);
                        game.setVehicleDoorBroken(entity.scriptID, id, true);
                    });
                }
            }, 500);
        }

        if(entity.hasStreamSyncedMeta("DESTROYED_TYRES")) {
            entitySpawned(entity).then(() => {
                var value = entity.getStreamSyncedMeta("DESTROYED_TYRES");
                if(value != "") {
                    value.split(",").forEach((el) => {
                        var id = parseInt(el);
                        log("VEHICLE", "Break tyre: " + entity.id + ", tyreId: " + id);
                        game.setVehicleTyreBurst(entity.scriptID, id, true, 1000.0);
                    });
                }
            })
        }
    
        if(entity.hasStreamSyncedMeta("INVINCIBLE_VEHICLE_WINDOWS")) {
            //game.setDontProcessVehicleGlass(entity.scriptID, true);
        }
    
        if(entity.hasStreamSyncedMeta("INVINCIBLE_VEHICLE_LIGHTS")) {
            //game.setVehicleHasUnbreakableLights(entity.scriptID, true);
        }
    
        if (entity.hasStreamSyncedMeta("DESTROYED_WINDOWS")) {
            var value = entity.getStreamSyncedMeta("DESTROYED_WINDOWS");
            if (value != "") {
                var damaged = [];
                value.split(",").forEach((el) => {
                    damaged.push(parseInt(el));
                });
    
                for (var i = 0; i < 10; i++) {
                    if (damaged.includes(i)) {
                        log("VEHICLE", "Smash Window: " + i);
                        game.smashVehicleWindow(entity.scriptID, i);
                        game.removeVehicleWindow(entity.scriptID, i);
                    } else {
                        log("VEHICLE", "Fix Window: " + i);
                        game.fixVehicleWindow(entity.scriptID, i);
                    }
                }
            }
        }
    
        if(entity.hasStreamSyncedMeta("VEHICLE_DAMAGE_PARTS")) {
            applyDeformationDamage(entity, entity.getStreamSyncedMeta("VEHICLE_DAMAGE_PARTS"));
        }
    });
});

var seatbelt = false;

alt.on("syncedMetaChange", (entity, key, value, oldValue) => {
    if(key == "VEHICLE_DAMAGE") {
        applyDamage(entity, value);
    } else if(key == "VEHICLE_SEATBELT") {
        if(entity == alt.Player.local) {
            seatbelt = value;
        }
    }
});

alt.on("streamSyncedMetaChange", (entity, key, value, oldValue) => {
    if(key == "DESTROYED_DOORS") {
        if(value != "") {
            entitySpawned(entity).then(() => {
                value.split(",").forEach((el) => {
                    var id = parseInt(el);
                    log("VEHICLE", "Break vehicle door: " + entity.id);
                    game.setVehicleDoorBroken(entity.scriptID, id, true);
                });
            })
        }
    //setVehicleFixed DOESNT WORK WITH DOORS!
    } else if(key == "DESTROYED_TYRES") {
        if(entity.hasStreamSyncedMeta("DESTROYED_TYRES")) {
            entitySpawned(entity).then(() => {
                value.split(",").forEach((el) => {
                    var id = parseInt(el);
                    log("VEHICLE", "Break tyre: " + entity.id);
                    game.setVehicleTyreBurst(entity.scriptID, id, true, 1000.0);
                });
            })
        }
    } else if(key == "SET_VEHICLE_FIXED") {  
        if(value != undefined) {
            game.setVehicleFixed(entity.scriptID);
        }
    } else if(key == "INVINCIBLE_VEHICLE_WINDOWS") {
        //game.setDontProcessVehicleGlass(entity.scriptID, true);
    } else if(key == "INVINCIBLE_VEHICLE_LIGHTS") {
        //game.setVehicleHasUnbreakableLights(entity.scriptID, true);
    } else if(key == "DESTROYED_WINDOWS") {
        if (value != "") {
            var damaged = [];
            value.split(",").forEach((el) => {
                damaged.push(parseInt(el));
            });

            for (var i = 0; i < 10; i++) {
                if (damaged.includes(i)) {
                    log("VEHICLE", "Smash Window: " + i);
                    game.smashVehicleWindow(entity.scriptID, i);
                    game.removeVehicleWindow(entity.scriptID, i);
                } else {
                    log("VEHICLE", "Fix Window: " + i);
                    game.fixVehicleWindow(entity.scriptID, i);
                }
            }
        }
    } else if(key == "VEHICLE_DAMAGE_PARTS") {
        //applyDeformationDamage(entity, value);
    } else if(key == "VEHICLE_SEATBELT") {
        if(entity == alt.Player.local) {
            seatbelt = value;
        }
    }
});

function applyDeformationDamage(veh, data) { 
    var obj = JSON.parse(data);

    //game.setVehicleDeformationFixed(veh.scriptID);


    //alt.setTimeout(() => {
        var hash = game.getEntityModel(veh.scriptID);
        var [_, pos1, pos2] = game.getModelDimensions(hash);
    
        obj.forEach((el) => {
            console.log("Applying part damage: " + JSON.stringify(el));
            applyPartDamage(veh, el, pos1, pos2);
        });
}


function applyPartDamage(vehicle, part, pos1, pos2) {
    if(part.DamageLevel == 0) {
        return;
    }
    
    var height = pos1.z - Math.abs(pos1.z - pos2.z); //pos2.z - Math.abs(pos1.z - pos2.z);

    var width = Math.abs(pos1.x - pos2.x);
    var length = Math.abs(pos1.y - pos2.y);

    switch(part.PartId) {
        case 0:
            for (var i = 0; i < part.DamageLevel; i++)
                game.setVehicleDamage(vehicle.scriptID, pos2.x, pos2.y, height, 200, 40, true);
            break;
        case 1:
            for (var i = 0; i < part.DamageLevel; i++)
                game.setVehicleDamage(vehicle.scriptID, pos2.x - width, pos2.y, height, 200, 40, true);
            break;
        case 2:
            for (var i = 0; i < part.DamageLevel; i++)
                game.setVehicleDamage(vehicle.scriptID, pos2.x, pos2.y - length / 2, height, 350, 20, true);
            break;
        case 3:
            for (var i = 0; i < part.DamageLevel; i++)
                game.setVehicleDamage(vehicle.scriptID, pos2.x - width, pos2.y -  length / 2, height, 350, 20, true);
            break;
        case 4:
            for (var i = 0; i < part.DamageLevel; i++)
                game.setVehicleDamage(vehicle.scriptID, pos2.x, pos2.y - length, height, 200, 40, true);
            break;
        case 5:
            for (var i = 0; i < part.DamageLevel; i++)
                game.setVehicleDamage(vehicle.scriptID, pos2.x - width, pos2.y - length, height, 200, 40, true);
            break;
    }
}



var seatsToggle;
var seatsList = null;
var seatHeight = null;
alt.onServer("PLAYER_SHOW_SEATS", (toggle, seats, height) => {
    seatsToggle = toggle;
    height = height;
    if(toggle) {
        alt.setTimeout(() => {
            if(seatsToggle) {
                seatsList = seats;
            }
        }, 1000);
    } else {
        seatsList = null;
    }
});


var count = 0;
var shortTick = 15;

var lastVehicleLightState = -1;
var lastSeatbelt = false;

var mapList = [58, 157, 158, 160, 164, 165, 159, 161, 162, 163];
alt.everyTick(() => {
    var vehicle = alt.Player.local.vehicle;

    if(seatsList != null) {
        seatsList.forEach((seat, idx) => {
            game.setDrawOrigin(seat.x, seat.y, seat.z + (seatHeight / 2), 0);
            game.beginTextCommandDisplayText('STRING');
            game.addTextComponentSubstringPlayerName("Sitz: " + idx);
            game.setTextFont(4);
            game.setTextScale(1, 0.75);
            game.setTextWrap(0.0, 1.0);
            game.setTextCentre(true);
            if(idx != 0 && game.isDisabledControlPressed(0, mapList[idx])) {
                game.setTextColour(204, 138, 37, 255);
                alt.emitServer("VEHICLE_ENTER_PASSENGER", false, game.getGameplayCamRot(2), game.isDisabledControlPressed(0, 157), game.isDisabledControlPressed(0, 158), game.isDisabledControlPressed(0, 160), game.isDisabledControlPressed(0, 164), game.isDisabledControlPressed(0, 165), game.isDisabledControlPressed(0, 159), game.isDisabledControlPressed(0, 161),  game.isDisabledControlPressed(0, 162), game.isDisabledControlPressed(0, 163));
                seatsList = null;
            } else {
                game.setTextColour(194, 162, 218, 255);
            }
            game.setTextOutline();
            game.setTextDropShadow();
        
            game.endTextCommandDisplayText(0, 0, 0);
            game.clearDrawOrigin();
        });
    }

    if (alt.Player.local.vehicle != null && seatsList != null) {
        seatsList = null;
    }
    
    game.disableControlAction(0, 23, true);


    // Passenger
    game.disableControlAction(0, 58, true); //

    if (game.isDisabledControlPressed(0, 58)) {  
        game.disableControlAction(0, 157, true); // 1
        game.disableControlAction(0, 158, true); // 2
        game.disableControlAction(0, 160, true); // 3
        game.disableControlAction(0, 164, true); // 4
        game.disableControlAction(0, 165, true); // 5
        game.disableControlAction(0, 159, true); // 6
        game.disableControlAction(0, 161, true); // 7
        game.disableControlAction(0, 162, true); // 8
        game.disableControlAction(0, 163, true); // 9
    }

    game.setPedConfigFlag(alt.Player.local.scriptID, 32, !seatbelt);
    if(seatbelt) {
        game.disableControlAction(0, 75, true); // disable Vehicle Exit
    }

    count++;
    if(shortTick < count) {
        count = 0;
        var veh = alt.Player.local.vehicle;
        if(veh != null && reactView != null) {
            var vehClass = game.getVehicleClass(veh.scriptID);

            var data = {
                Event: "UPDATE_CAR_SPEED_CLIENT",
                //1.1 Makes Tacho show 10% more as in reality
                speed: game.getEntitySpeed(veh.scriptID) * 3.6 * 1.1,
            }

            reactView.emit('CEF_EVENT', data);

            var lightStateBool = game.getVehicleLightsState(veh.scriptID);

            var lightState = 0;
            if(lightStateBool[2]) {
                lightState = 1;
            } else if(lightStateBool[1]){
                lightState = 2;
            }

            if(lastVehicleLightState != lightState) {
                var data = {
                    Event: "UPDATE_CAR_ICON",
                    name: "light",
                    value: lightState,
                }
    
                reactView.emit('CEF_EVENT', data);
            }

            var roll = game.getEntityRoll(veh.scriptID);
            if ((roll > 75 || roll < -75) && vehClass != 8 && vehClass != 15 && vehClass != 16) {
                game.disableControlAction(2, 59, true); // disable left/right
                game.disableControlAction(2, 60, true); // disable up/down
            }
        }

        if(lastSeatbelt != seatbelt) {
            var data = {
                Event: "UPDATE_CAR_ICON",
                name: "belt",
                value: seatbelt ? 1 : 0,
            }

            reactView.emit('CEF_EVENT', data);

            lastSeatbelt = seatbelt;
        }

        if(alt.Player.local.vehicle != null) {
            game.setUserRadioControlEnabled(false);

            if(game.getPlayerRadioStationName() != "OFF") {
                game.setVehRadioStation(alt.Player.local.vehicle.scriptID, "OFF");
            }
        }
    }

    // If Firetruck and right mouse button pressed, call fire class function
    if(vehicle != null) {
        // Vehicle cannot be rolled over
        var roll = game.getEntityRoll(vehicle.scriptID);
        if ((roll > 75 || roll < -75) && vehClass != 8 && vehClass != 13) {
            game.disableControlAction(2, 59, true); // disable left/right
            game.disableControlAction(2, 60, true); // disable up/down
        }
        
        if (game.isControlPressed(0, 68) && game.getPedInVehicleSeat(vehicle.scriptID, -1, true) == alt.Player.local.scriptID) {
            var specialFlag = vehicle.getStreamSyncedMeta("SPECIAL_FLAG");

            if ((specialFlag & specialFlags.IsFireTruckWater) == specialFlags.IsFireTruckWater) { // Firetruck with water
                useFireTruckWater(vehicle);
            } else if ((specialFlag & specialFlags.IsFireTruckFoam) == specialFlags.IsFireTruckFoam) // TODO Firetruck with foam
                useFireTruckFoam(vehicle);
        }
    }
});

alt.onServer("REQUEST_VEHICLE_ENTER", (type, isPressed) => {
    if(type == 1) {
        alt.emitServer("VEHICLE_ENTER_DRIVER", game.getGameplayCamRot(2));
    } else if(type == 2) {
        alt.emitServer("VEHICLE_ENTER_PASSENGER", isPressed, game.getGameplayCamRot(2), game.isDisabledControlPressed(0, 157), game.isDisabledControlPressed(0, 158), game.isDisabledControlPressed(0, 160), game.isDisabledControlPressed(0, 164), game.isDisabledControlPressed(0, 165), game.isDisabledControlPressed(0, 159), game.isDisabledControlPressed(0, 161),  game.isDisabledControlPressed(0, 162), game.isDisabledControlPressed(0, 163));
    }
});

var passengerEveryTick = null;

alt.onServer("VEHICLE_SHOW_PASSENGER", (vehicle, seatID) => {
    if(passengerEveryTick != null) {
        alt.clearEveryTick(passengerEveryTick);
        passengerEveryTick = null;
    }

    var ped = game.getPedInVehicleSeat(vehicle, seatID - 1, true);
    var counter = 0;
    passengerEveryTick = alt.everyTick(() => {
        var [_, min, max] = game.getModelDimensions(game.getEntityModel(ped));

        var pos = game.getOffsetFromEntityInWorldCoords(ped, (min.x + max.x) / 2, (min.y + max.y) / 2, (min.z + max.z) / 2);
        var midX = pos.x;
        var midY = pos.y;
        var midZ = pos.z + 1;

        game.setDrawOrigin(midX, midY, midZ, 0);
        game.requestStreamedTextureDict("helicopterhud", false);
        game.drawSprite("helicopterhud", "hud_corner", -0.01, -0.01, 0.022, 0.022, 0.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", 0.01, -0.01, 0.022, 0.022, 90.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", -0.01, 0.01, 0.022, 0.022, 270.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", 0.01, 0.01, 0.022, 0.022, 180.0, 204, 138, 37, 255, true, 1)
        game.clearDrawOrigin()

        counter++;
        if(counter > 150) {
            alt.clearEveryTick(passengerEveryTick);
            passengerEveryTick = null;
        }
    });
});


var seatEveryTick = null;

alt.onServer("VEHICLE_SHOW_SEAT", (seatPos) => {
    if(seatEveryTick != null) {
        alt.clearEveryTick(seatEveryTick);
        seatEveryTick = null;
    }

    var counter = 0;
    seatEveryTick = alt.everyTick(() => {
        game.setDrawOrigin(seatPos.x, seatPos.y, seatPos.z, 0);
        game.requestStreamedTextureDict("helicopterhud", false);
        game.drawSprite("helicopterhud", "hud_corner", -0.01, -0.01, 0.022, 0.022, 0.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", 0.01, -0.01, 0.022, 0.022, 90.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", -0.01, 0.01, 0.022, 0.022, 270.0, 204, 138, 37, 255, true, 1)
        game.drawSprite("helicopterhud", "hud_corner", 0.01, 0.01, 0.022, 0.022, 180.0, 204, 138, 37, 255, true, 1)
        game.clearDrawOrigin()

        counter++;
        if(counter > 150) {
            alt.clearEveryTick(seatEveryTick);
            seatEveryTick = null;
        }
    });
});

alt.onServer('RAPPEL_FROM_HELICOPTER', (player) => {
    game.taskRappelFromHeli(player.scriptID, 1);
});

alt.onServer('BURST_TYRE', (vehicle, wheelId) => {
    if (!game.isVehicleTyreBurst(vehicle.scriptID, wheelId, true))
        game.setVehicleTyreBurst(vehicle.scriptID, wheelId, true, 1000.0);
});

alt.onServer("SET_TOGGLE_SPEEDLIMITER", (vehicle) => {
    if (vehicle != null) {
        let speedlimiter = game.getEntitySpeed(vehicle.scriptID) * 3.6 * 1.1;
        let currentSpeed = Math.round(speedlimiter / 10) * 10
        if (!toggleSpeedlimiter) {
            if(currentSpeed >= 30) {
                game.setVehicleMaxSpeed(vehicle.scriptID, currentSpeed / 3.6 / 1.1);
                toggleSpeedlimiter = !toggleSpeedlimiter;
                alt.emitServer("NOTIFICATION_SPEEDLIMITER", "Speedlimiter auf "+ currentSpeed + "KMH gestellt!");
            }
            else {
                alt.emitServer("NOTIFICATION_SPEEDLIMITER", "Du musst mindestens 30KMH fahren!")
            }
        }
        else {
            game.setVehicleMaxSpeed(vehicle.scriptID, 500);
            toggleSpeedlimiter = !toggleSpeedlimiter;
            alt.emitServer("NOTIFICATION_SPEEDLIMITER", "Speedlimiter ausgeschalted!");
        }
    }
});

alt.onServer("SET_SPEEDLIMTER", (vehicle, speed) => {
    let speedlimiter = speed / 3.6 / 1.1
    if (vehicle != null) {
        let currentSpeed = game.getEntitySpeed(vehicle.scriptID)
        if (currentSpeed < speedlimiter * 1.05) {
            game.setVehicleMaxSpeed(vehicle.scriptID, speedlimiter);
            toggleSpeedlimiter = true;
            if (speed < 500)
                alt.emitServer("NOTIFICATION_SPEEDLIMITER", "Speedlimiter auf " + speed + "KMH gestellt!");
        }
        else {
            alt.emitServer("NOTIFICATION_SPEEDLIMITER", "Du bist zu Schnell!");
        }
        if (speed > 200) {
            toggleSpeedlimiter = false;
            alt.emitServer("NOTIFICATION_SPEEDLIMITER", "Speedlimiter ausgeschalted!");
        }
    }
});

alt.onServer('VEHICLE_LOCKSTATE_INDICATOR', (vehicle) => {
    let state = true;
        let interval = alt.setInterval(() => {
            if (state) {
                game.setVehicleLights(vehicle.scriptID, 2);
            } else {
                game.setVehicleLights(vehicle.scriptID, 1);
            }
            state = !state;
        }, 250);

        setTimeout(() => {
            clearInterval(interval);
            game.setVehicleLights(vehicle.scriptID, 0);
        }, 1000);
})


var vehObjs = new Map();
var vehUpds = new Map();
var vehPos = new Map();
var vehPosOld = new Map

var vehRot = new Map();
var vehRotOld = new Map();

var maxCount = 5;
var counter = 0;

alt.onServer("VEHICLE_DELETE", (vehicle) => {
	if(vehObjs.has(vehicle.id)) {
		var vehObj = vehObjs.get(vehicle.id);
		game.deleteObject(vehObj);
		game.deleteEntity(vehObj);
		vehObjs.delete(vehicle.id);
		vehUpds.delete(vehicle.id);
		vehPos.delete(vehicle.id);
		vehPosOld.delete(vehicle.id);
		vehRot.delete(vehicle.id);
		vehRotOld.delete(vehicle.id);
	}
});

alt.everyTick(() => {
    //Disable player Headlight Key
    game.disableControlAction(0, 74, true);

    counter++;
    if(counter > maxCount) {
        counter = 0;
        alt.Vehicle.all.forEach((el) => {
            if(el.scriptID == 0 && el.hasSyncedMeta("p") && el.getSyncedMeta("i") == island.island) {          
				if(vehObjs.has(el.id)) {
					var vehObj = vehObjs.get(el.id);
					
					var currentPos = vehPos.get(el.id);
					var oldPos = vehPosOld.get(el.id);
					
					var currentRot = vehRot.get(el.id);
					var oldRot = vehRotOld.get(el.id);
					
					var pos = el.getSyncedMeta("p");
					var rot = el.getSyncedMeta("r");
					
					if(currentPos.x == pos.x && currentPos.y == pos.y && currentPos.z == pos.z) {
						var delta = new Date().getTime() - vehUpds.get(el.id);
						var per = delta / 1000;
						
						if(per > 1) {
							per = 1;
						}
						
						var vec = {x: currentPos.x - oldPos.x, y: currentPos.y - oldPos.y, z: currentPos.z - oldPos.z};
						game.setEntityCoords(vehObj, oldPos.x + vec.x * per, oldPos.y + vec.y * per, oldPos.z + vec.z * per, 1, 0, 0, 1);

                        var quFrom = math.toQuaternion(oldRot.x, oldRot.y, oldRot.z);
                        var quTo = math.toQuaternion(currentRot.x, currentRot.y, currentRot.z)

                        var setQu = math.slerpQuaternion(quFrom, quTo, per);
                        
						var preRot = math.toEuler(setQu);
                        var setRot = {x: toDegree(preRot.x), y: toDegree(preRot.y), z: toDegree(preRot.z)};
                        
						game.setEntityRotation(vehObj, setRot.x, setRot.y, setRot.z, 2, true);	
					} else {					
						game.setEntityCoords(vehObj, currentPos.x, currentPos.y, currentPos.z, 1, 0, 0, 1);
						
						vehPos.set(el.id, pos);
						vehPosOld.set(el.id, currentPos);
						
						vehRot.set(el.id, rot);
						
						vehRotOld.set(el.id, currentRot);
						
						vehUpds.set(el.id, new Date().getTime());
					}
					
				} else {
					var vehObj;
					if(el.model == 0x2C75F0DD) {
						vehObj = game.createObjectNoOffset(alt.hash("custom_lod_buzzard2"), el.pos.x, el.pos.y, el.pos.z, true, true, false);
					} else {				
						vehObj = game.createObjectNoOffset(alt.hash("custom_lod_luxor"), el.pos.x, el.pos.y, el.pos.z, true, true, false);
					}

					vehObjs.set(el.id, vehObj);
                    vehPos.set(el.id, el.pos);
                    vehRot.set(el.id, el.rot);

                    alt.setTimeout(() => {
                        game.setEntityAsMissionEntity(vehObj, true, true);
                        game.setEntityInvincible(vehObj, true);
                        game.setEntityCollision(vehObj, false, false);
                        game.setEntityLodDist(vehObj, 100000);
                    }, 250);
                }
            } else {
				if(vehObjs.has(el.id)) {
					var vehObj = vehObjs.get(el.id);
					game.deleteObject(vehObj);
					game.deleteEntity(vehObj);
					vehObjs.delete(el.id);
					vehPos.delete(el.id);
					vehPosOld.delete(el.id);
					vehRot.delete(el.id);
					vehRotOld.delete(el.id);
				}
			}
        });
    }
})

function toDegree(radians) {
  var pi = Math.PI;
  return radians * (180/pi);
}