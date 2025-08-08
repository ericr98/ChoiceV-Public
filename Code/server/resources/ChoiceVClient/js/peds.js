import * as alt from 'alt'; 
import * as game from 'natives';


import { setPedCharacter } from './charCreator.js';
import { loadModel, loadAnimDic, entityExists } from './entity.js';

import { toInt32 } from './math.js';
import { log } from 'js/client.js';

//Create Friendly Ped Relationship Group
// var hash = game.addRelationshipGroup("pedFriendlySpawn");
// game.setRelationshipBetweenGroups(1, hash[0], 0x6F0783F5);
// game.setRelationshipBetweenGroups(1, 0x6F0783F5, hash[0]);

var peds = [];

export function onTick() {
    var pos = alt.Player.local.pos;

    peds.forEach((el) => {
        el.check(pos.x, pos.y);
    });
}


class Ped {
    constructor(id, model, x, y, z, heading, headVariation, torsoVariation, scenario, dict, name, flag, duration, percent, isVisible) {
        this.id = id;
        this.model = model;
        this.x = x;
        this.y = y;
        this.z = z;
        this.heading = heading;

        this.headVariation = headVariation;
        this.torsoVariation = torsoVariation;

        this.scenario = scenario;
        this.animDict = dict;
        this.animName = name;
        this.animFlag = flag;
        this.animDuration = duration;
        this.animPercent = percent;

        this.animStart = Date.now();

        this.isVisible = isVisible;
        this.isAdminVisible = false;

        this.ped = null;

        this.spawned = false;

        this.alpha = 255;
    }

    check(x, y) {
        if(this.isVisible || this.isAdminVisible) {
            if(getDistance(this.x, this.y, x, y) < 200) {
                if(!this.spawned) {
                    this.spawn();
                }
            } else {
                if(this.spawned) {
                    this.despawn();
                }
            }
        } else {
            if(this.spawned) {
                this.despawn();
            }
        }
    }

    spawn() {
        this.spawned = true;
        var hash = alt.hash(this.model);
        loadModel(hash).then((h) => {
            //var ped = game.createPed(4, hash, x, y, z, 60, true, false);
            this.ped = game.createPed(1, hash, this.x, this.y, this.z, this.heading, false, false);

            let i;
            for (i = 0; i < 11; i++) {
                game.setPedComponentVariation(this.ped, i, 0, 0, 2);
            }

            game.setPedComponentVariation(this.ped, 0, this.headVariation, 0, 2);
            game.setPedComponentVariation(this.ped, 3, this.torsoVariation, 0, 2);

            game.freezeEntityPosition(this.ped, true);
            
            entityExists(this.ped).then((p) => {
                game.setEntityInvincible(this.ped, true);
    
                game.setEntityAsMissionEntity(this.ped, true, false); // make sure its not despawned by game engine
                game.setBlockingOfNonTemporaryEvents(this.ped, true); // make sure ped doesnt flee etc only do what its told
                game.setPedCanBeTargetted(this.ped, false);
                game.setPedCanBeKnockedOffVehicle(this.ped, 1);
                game.setPedCanBeDraggedOut(this.ped, false);
                game.setPedSuffersCriticalHits(this.ped, false);
                game.setPedDropsWeaponsWhenDead(this.ped, false);
                game.setPedDiesInstantlyInWater(this.ped, false);
                game.setPedCanRagdoll(this.ped, false);
                game.setPedDiesWhenInjured(this.ped, false);
                game.taskSetBlockingOfNonTemporaryEvents(this.ped, true);
                game.setPedFleeAttributes(this.ped, 0, false);
                game.setPedConfigFlag(this.ped, 32, false); // ped cannot fly thru windscreen
                game.setPedConfigFlag(this.ped, 281, true); // ped no writhe
                game.setPedGetOutUpsideDownVehicle(this.ped, false);
                game.setPedCanEvasiveDive(this.ped, false);
                game.setEntityAlpha(this.ped, this.alpha, false);

                this.applyAnims();
                this.setAdminVisibleAlpha();

                log("PED", "Ped spawned: id: " + this.id + ", model: " + this.model + ", gtaId: " + this.ped);
            });
        });
    }

    applyAnims() {
        if(this.ped != null) {
            if(this.animDict != null && this.animDict != "") {
                var duration = 999_999_999;
				if(this.animDuration != -1) {
					duration = toInt32(this.animDuration);
				}
                duration -= Date.now() - this.animStart;

                if(duration > 0) {
                    loadAnimDic(this.animDict).then(() => {
                        game.taskPlayAnim(this.ped, this.animDict, this.animName, 8.0, 1.0, duration, toInt32(this.animFlag), Number(this.animPercent), false, false, false);
                    });
                } else {
                    this.animDict = null;
                }
            } else if(this.scenario != null && this.scenario != "") {
                game.taskStartScenarioInPlace(this.ped, this.scenario, 0, false);
            }
        }
    }

    despawn() {    
        this.spawned = false; 
        game.deletePed(this.ped);
    }

    setAdminVisibleAlpha() {
        if(this.spawned) {
            if(!this.isVisible && this.isAdminVisible) {
                game.setEntityAlpha(this.ped, 100, false);
            } else {
                this.applyAlpha();
            }
        }
    }

    applyAlpha() {
        if(this.spawned && this.ped != null) {
            game.setEntityAlpha(this.ped, this.alpha, false);
        }
    }
}

function getDistance(x1, y1, x2, y2) {
    var a = x1 - x2;
    var b = y1 - y2;

    return Math.sqrt(a*a + b*b); 
}

alt.onServer("SPAWN_STATIC_PED", (id, model, x, y, z, heading, headVariation, torsoVariation, scenario, dict, name, flag, duration, percent, isVisible) => {
    var ped = new Ped(id, model, x, y, z, heading, headVariation, torsoVariation, scenario, dict, name, flag, duration, percent, isVisible);
    peds.push(ped);

    var pos = alt.Player.local.pos;
    ped.check(pos.x, pos.y);

});

alt.onServer("SET_STATIC_PED_DATA", (pedId, dataName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) => {
    setPedData(0, pedId, dataName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
});

function setPedData(counter, pedId, dataName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
    log("PED", "Setting ped data: " + pedId + ", " + dataName + ", " + arg1 + ", " + arg2 + ", " + arg3 + ", " + arg4 + ", " + arg5 + ", " + arg6 + ", " + arg7 + ", " + arg8);
    var ped = peds.find((el) => el.id == pedId);

    if(ped != null) {
        switch(dataName) {
            case "SCENARIO":
                ped.scenario = arg1;

                ped.applyAnims();
                break;
            case "ANIMATION":
                ped.animDict = arg1;
                ped.animName = arg2;
                ped.animFlag = arg3;
                ped.animDuration = arg4;
                ped.animPercent = arg5;

                ped.animStart = Date.now();
                ped.applyAnims();
                break;
            case "STOP_ANIMATION":       
                ped.animDict = null;
                ped.animName = null;
                ped.animFlag = null;
                ped.animDuration = null;
                ped.animPercent = null;

                game.clearPedTasks(ped.ped);
                ped.applyAnims();
                break;
            case "VISIBLE":       
                ped.isVisible = arg1;
                if(arg1) {
                    var pos = alt.Player.local.pos;
                    ped.check(pos.x, pos.y);
                } else {
                    ped.despawn();
                }
                break;
            case "ADMIN_VISIBLE":       
                ped.isAdminVisible = arg1;

                var pos = alt.Player.local.pos;
                ped.check(pos.x, pos.y);

                if(ped.spawned) {
                    ped.setAdminVisibleAlpha();
                }
                break;
            case "ALPHA":
                ped.alpha = arg1;
                ped.applyAlpha();
                break;
        }
    } else {
        if(counter < 5) {
            alt.setTimeout(() => {
                setPedData(counter+1, pedId, dataName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
            }, 1000);
        }
    }
}

alt.onServer("DESTROY_STATIC_PED", (id) => {
    var obj = peds.filter((el) => {
        return id == el.id;
    })[0];
    
    obj.despawn();
    peds = peds.filter((el) => {
        return id != el.id;
    });
});

alt.onServer("SET_PED_STYLE", (id, style) => {
    setPedStyle(id, style);
});

function setPedStyle(id, style, tries = 0, currentDelay = 10) {
    if(tries > 10) {
        return;
    }

    var obj = peds.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null && obj.ped != null) {
        setPedCharacter(style, obj.ped);
    } else {
        alt.setTimeout(() => {
            setPedStyle(id, style, tries+1, currentDelay*2);
        }, currentDelay);
    }
}

alt.onServer('SET_PED_CLOTHES', (id, slot, drawable, texture, dlc) => {
	setPedClothes(id, slot, drawable, texture, 0, dlc);
});

function setPedClothes(id, slot, drawable, texture, palette, dlc, tries = 0, currentDelay = 10) {
    if(tries > 5) {
        return;
    }

    if(palette == undefined) {
        palette = 0;
    }

    var obj = peds.filter((el) => {
        return id == el.id;
    })[0];

    if (obj != null && obj.ped != null) {
        if(dlc == null) {
            game.setPedComponentVariation(obj.ped, slot, drawable, texture, palette);
        } else {
            alt.setPedDlcClothes(obj.ped, alt.hash(dlc), slot, drawable, texture, palette)
        }
    } else {
        alt.setTimeout(() => {
            setPedClothes(id, slot, drawable, texture, palette, dlc, tries+1, currentDelay*2);
        }, currentDelay);
    }
}

alt.onServer('SET_PED_ACCESSOIRE', (id, slot, drawable, texture, dlc) => {
	setPedAccessoire(id, slot, drawable, texture, dlc);
});

export function setPedAccessoire(id, slot, drawable, texture, dlc, tries = 0, currentDelay = 10) {
    if(tries > 5) {
        return;
    }

    var obj = peds.filter((el) => {
        return id == el.id;
    })[0];

    if(obj != null && obj.ped != null) {
        if(drawable == -1) {
            game.clearPedProp(obj.ped, slot, 0);
        } else {
            if(dlc == null) {
                game.setPedPropIndex(obj.ped, slot, drawable, texture, true, 0);
            } else {
                alt.setPedDlcProp(obj.ped, alt.hash(dlc), slot, drawable, texture);
            }
        }
    } else {
        alt.setTimeout(() => {
            setPedAccessoire(id, slot, drawable, texture, dlc, tries+1, currentDelay*2);
        }, currentDelay);
    }
}

var sharpWeapons = [
    alt.hash("weapon_dagger"),
    alt.hash("weapon_bottle"),
    alt.hash("weapon_hatchet"), 
    alt.hash("weapon_knife"),
    alt.hash("weapon_machete"),
    alt.hash("weapon_switchblade"),
    alt.hash("weapon_battleaxe"),
    alt.hash("weapon_stone_hatchet"),
];

export function onLongerTick() {
    var player = alt.Player.local;
    if(game.isAimCamActive()) {
        var [found, entity] = game.getEntityPlayerIsFreeAimingAt(player.scriptID);

        if(found) {
            for(var el of peds) {
                if(el.ped == entity && getDistance(el.x, el.y, player.pos.x, player.pos.y) < 10) {
                    alt.emitServer("PED_BEING_THREATEND", el.id);
                    return;
                }
            }
        }
    }

    var currentWeapon = game.getSelectedPedWeapon(player);
    if(sharpWeapons.includes(currentWeapon)) {
        var nearPed = peds.find((el) => getDistance(el.x, el.y, player.pos.x, player.pos.y) < 1 && Math.abs(el.z - player.pos.z) < 2);
        if(nearPed != null) {            
            alt.emitServer("PED_BEING_THREATEND", nearPed.id);
            return;
        }
    }

}