import alt from 'alt'; 
import game from 'natives';

//import * as miloList from '/js/milolist.js';
import * as doorList from '/js/doorlist.js';

import { log } from '/js/client.js';
import * as math from '/js/math.js';

var miloLoadDistance = 400;
var milos = [];

var doorLoadDistance = 100;
var doors = [];

var lastPos = alt.Player.local.pos;

export function onTick() {
    var pos = alt.Player.local.pos;
    if(getDistance(pos.x, pos.y, lastPos.x, lastPos.y) > 1) {
        lastPos = pos;

        milos.forEach((el) => {
            el.check(pos.x, pos.y);
        });

        doors.forEach((el) => {
            el.check(pos.x, pos.y);
        });
    }
}

alt.onServer("SET_IPLS", (iplsList) => {
    JSON.parse(iplsList).forEach((el) => {
        milos.push(new Milo(el.name, el.pos.X, el.pos.Y, el.shown));
    });
});

alt.onServer("UPDATE_IPL", (ipl) => {
    var obj = JSON.parse(ipl);
    var milo = milos.filter((el) => el.name == obj.name)[0];

    if(milo != undefined) {
        milo.shown = obj.shown;
        milo.loaded = !obj.shown;
        var pos = alt.Player.local.pos;
    
        milo.check(pos.x, pos.y);
    } else {
        milos.push(new Milo(obj.name, obj.pos.X, obj.pos.Y, obj.shown));
    }
});

alt.loadDefaultIpls();

alt.on('connectionComplete', () => {
    for(var i = 0; i < doorList.doorHash.length; i++) {
        var d = new Door(doorList.doorIds[i], doorList.doorPosX[i], doorList.doorPosY[i], doorList.doorPosZ[i], math.ToUint32(doorList.doorHash[i]), 0);
        doors.push(d);
        d.check();
    }
});


function getDistance(x1, y1, x2, y2) {
    var a = x1 - x2;
    var b = y1 - y2;

    return Math.sqrt(a*a + b*b); 
}

//MILOS

class Milo {
    constructor(name, x, y, shown) {
        this.name = name;
        this.x = x; 
        this.y = y;
        this.z = 0;

        this.shown = shown;
        this.loaded = shown;

        log("MLO", "Milo added: " + this.name + " " + this.x + " " + this.y + " " + this.shown)
        if(!this.shown) {
            alt.removeIpl(this.name);
        } else {
            alt.requestIpl(this.name);
        }

        this.entitySets = [];
    }

    check(x, y) {
        if(!this.shown) {
            if(this.loaded) {
                alt.removeIpl(this.name);
                this.loaded = false;
            }

            return;
        }
        
        var dist = getDistance(this.x, this.y, x, y);
        
        if(dist < miloLoadDistance && !this.loaded) {
            alt.requestIpl(this.name);
            this.loaded = true;

            log("MLO", "IPL Loaded: " + this.name);
        }

        if(dist < miloLoadDistance / 2 && this.loaded) {
            var interior = game.getInteriorAtCoords(this.x, this.y, this.z);
            this.entitySets.forEach((set) => {
                if(set.shown && !set.isLoaded) {
                    game.activateInteriorEntitySet(interior, set.set);
                    set.isLoaded = true;
                    game.refreshInterior(interior);
                }

                if(!set.shown && set.isLoaded) {
                    game.deactivateInteriorEntitySet(interior, set.set);
                    set.isLoaded = false;
                    game.refreshInterior(interior);
                }

                log("MLO", "Set Entity Set: " + set.set + " in interior: " + interior + " was set to: " + set.shown);
            });
        }
        
        if(dist > miloLoadDistance && this.loaded) {
            alt.removeIpl(this.name);
            this.loaded = false;

           log("MLO", "IPL Unloaded: " + this.name);
        }
    }

    setEntitySetState(x, y, z, entitySet, toggle) {
        this.x = x;
        this.y = y;
        this.z = z;

        var find = this.entitySets.find((el) => { return el.set == entitySet; });
        if(find != null) {
            find.shown = toggle;
        } else {
            this.entitySets.push({set: entitySet, shown: toggle, isLoaded: this.loaded && toggle});
        }


        if(this.loaded) {
            var interior = game.getInteriorAtCoords(this.x, this.y, this.z);
            log("MLO", "Set Entity Set: " + entitySet + " in interior: " + interior + " was toggled to: " + toggle);
            if(toggle) {
                game.activateInteriorEntitySet(interior, entitySet);
            } else {
                game.deactivateInteriorEntitySet(interior, entitySet);
            }
            game.refreshInterior(interior);
        }
    }
}


alt.onServer("SET_ENTITY_SET_STATE", (x, y, z, gtaName, entitySet, toogle) => {
    var mlo = milos.find((el) => el.name == gtaName);

    mlo.setEntitySetState(x, y, z, entitySet, toogle)
});

alt.onServer("ADD_DOOR_TO_SYSTEM", (id, posX, posY, posZ, hash, locked) => {
    var door = new Door(id, posX, posY, posZ, hash, locked == 1);
    doors.push(door);
    door.check();
});

alt.onServer("CHANGE_DOOR_POSITION", (oPosX, oPosY, oPosZ, nPosX, nPosY, nPosZ) => {
    var door = doors.filter((el) => {
        return el.x.toFixed(2) == oPosX.toFixed(2) && el.y.toFixed(2) == oPosY.toFixed(2) && el.z.toFixed(2) == oPosZ.toFixed(2);
    })[0];
    door.x = nPosX;
    door.y = nPosY;
    door.z = nPosZ;

    log("DOOR", "Door has fixed position: " + JSON.stringify(door));
});

alt.onServer("STRETCHER_DOOR_MODE", (toggle) => {
    openDoorsFullyOpen = toggle;
});

//DOOR

var doorAdminMode = false;
class Door {
    constructor(id, posX, posY, posZ, hash, locked) {
        this.id = id;
        this.x = posX;
        this.y = posY;
        this.z = posZ;
        this.hash = parseInt(hash);
        this.locked = locked;
        this.loaded = 0;
        this.reload = 0;
        this.obj = 0;
    }

    check(x, y) {
        if(getDistance(this.x, this.y, x, y) < doorLoadDistance) {
            if(this.loaded && this.reload < Date.now()) {
                game.doorSystemSetOpenRatio(this.hash, 0, true, true);
                game.setLockedUnstreamedInDoorOfType(this.hash, this.x, this.y, this.z, this.locked ? 1 : 0, 0, 0, 0);

                game.doorSystemSetDoorState(this.hash, this.locked ? 1 : 0, true, true);
                log("DOOR", "door loaded and state set: x:" + this.x + " y:" + this.y + " z:" + this.z + ", locked: " + this.locked);

                if (doorAdminMode) {
                    this.obj = game.getClosestObjectOfType(this.x, this.y, this.z, 0.1, this.hash, 0, 0, 0);
                    if (this.obj != 0) {
                        game.setEntityAlpha(this.obj, 120, 0);
                    } else {
                        log("DOOR", `Door with id ${this.id} not found!`);
                    }
                } else if (this.obj != 0) {
                    game.setEntityAlpha(this.obj, 255, 0);
                    this.obj = 0;
                }
                
                this.reload = Date.now() + 5000;
            }

            this.loaded = true;
        } else {
            this.loaded = false;
        }

    }
}

alt.onServer('CHANGE_DOOR_STATE', (posX, posY, posZ, locked) => {
    var door = doors.filter((el) => {
        return el.x.toFixed(2) == posX.toFixed(2) && el.y.toFixed(2) == posY.toFixed(2) && el.z.toFixed(2) == posZ.toFixed(2);
    })[0];

    door.locked = locked;
    door.reload = 0;

    var pos = alt.Player.local.pos;
    door.check(pos.x, pos.y);
});


var labelMapping = {};

alt.onServer("SET_DOOR_LABELS", (doorIds, labels) => {
    for (let i = 0; i < doorIds.length; i++) {
        labelMapping[doorIds[i]] = labels[i];
    }
})

var showDoorsLabels = [];
alt.onServer("TOOGLE_DOOR_INFO", (toggle) => {
    doorAdminMode = toggle;
    var playerPos = alt.Player.local.pos;
    if(toggle) {
        if(showDoorsLabels != []) {
            showDoorsLabels.forEach((el) => {
                el.destroy();
            });
            showDoorsLabels = [];
        }

        doors.forEach((door) => {
            var [_, min, max] = game.getModelDimensions(door.hash);
            door.check(playerPos.x, playerPos.y);
            
            var pos = game.getOffsetFromEntityInWorldCoords(door.obj, (min.x + max.x) / 2, (min.y + max.y) / 2, (min.z + max.z) / 2);

            var label = new alt.TextLabel(labelMapping[door.id], "", 25, 1, pos,  new alt.Vector3(0, 0, 0), new alt.RGBA(255, 255, 255, 255), 1, new alt.RGBA(0, 0, 0, 255), true, 100);
            label.faceCamera = true;
            showDoorsLabels.push(label);
        });
    } else {
        doors.forEach((door) => {
            door.check();
        });
        if(showDoorsLabels != []) {
            showDoorsLabels.forEach((el) => {
                el.destroy();
            });

            showDoorsLabels = [];
        }
    }
});
