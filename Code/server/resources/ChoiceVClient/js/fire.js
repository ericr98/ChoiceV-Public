import * as alt from 'alt';
import * as game from 'natives';
import * as math from 'js/math.js';

import { log } from 'js/client.js';

var fireExtinguishDistance = 6; // Distance for Fire-Extinguisher
var fireFireTruckDistance = 30; // Distance for Fire-Trucks
var explosionDamage = 20; // Explosion damage in percent
var allFires = []; // Array of all active fires
var locked = false; // Used to fix extinguish rate to 500ms

class Fire {
    constructor(id, x, y, z, particle, smoke, scale, gasoline) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.particle = particle;
        this.smoke = smoke;
        this.scale = scale;
        this.gasoline = gasoline;
        this.layer1 = null;
        this.layer2 = null;
        this.layer3 = null;
        this.loaded = false;

        if (this.scale > 5) this.scale = 5;

        this.spawn();
    }

    spawn() {
        this.loaded = true;

        var spread = parseInt(5 + (this.scale * 5));
        if (spread > 25) spread = 25;

        if (this.particle != null && this.particle.length > 0) {
            game.requestNamedPtfxAsset('core');
            game.useParticleFxAsset('core');
            this.layer1 = game.startParticleFxLoopedAtCoord(this.particle, this.x, this.y, this.z, 0, 0, 0, this.scale, 0, 0, 0, false);
        }

        if (this.smoke != null && this.smoke.length > 0) {
            game.requestNamedPtfxAsset('core');
            game.useParticleFxAsset('core');
            this.layer2 = game.startParticleFxLoopedAtCoord(this.smoke, this.x, this.y, this.z - 0.5, 0, 0, 0, this.scale * 0.5, 0, 0, 0, false);
        }

        this.layer3 = game.startScriptFire(this.x, this.y, this.z, spread, this.gasoline);

        log("FIRE", "Fire spawned  id: " + this.id + "  scale: " + this.scale + ", spread: " + spread);
    }

    update(scale) {
        this.scale = scale;

        game.stopParticleFxLooped(this.layer1, 0);
        game.stopParticleFxLooped(this.layer2, 0);
        game.removeScriptFire(this.layer3);

        if (this.scale > 0) {
            var spread = parseInt(5 + (this.scale * 5));
            if (spread > 25) spread = 25;

            if (this.particle != null && this.particle.length > 0) {
                game.requestNamedPtfxAsset('core');
                game.useParticleFxAsset('core');
                this.layer1 = game.startParticleFxLoopedAtCoord(this.particle, this.x, this.y, this.z, 0, 0, 0, this.scale, 0, 0, 0, false);
            }

            if (this.smoke != null && this.smoke.length > 0) {
                game.requestNamedPtfxAsset('core');
                game.useParticleFxAsset('core');
                this.layer2 = game.startParticleFxLoopedAtCoord(this.smoke, this.x, this.y, this.z - 0.5, 0, 0, 0, this.scale* 0.5, 0, 0, 0, false);
            }

            this.layer3 = game.startScriptFire(this.x, this.y, this.z, spread, this.gasoline);

            log("FIRE", "Fire updated  id: " + this.id + "  scale: " + this.scale + ", spread: " + spread);
        } else {
            alt.emitServer("FIRE_EXTINGUISHED", this.id);

            log("FIRE", "Fire extinguished  id: " + this.id + "  scale: " + this.scale);
        }
    }

    remove() {
        game.stopParticleFxLooped(this.layer1, 0);
        game.stopParticleFxLooped(this.layer2, 0);
        game.removeScriptFire(this.layer3);

        for (var i = 0; i < allFires.length; i++) {
            if (allFires[i].id == this.id)
                allFires.splice(i, 1);
        }

        this.loaded = false;

        log("FIRE", "Fire removed  id: " + this.id + "  scale: " + this.scale);
    }
}

export function useFireExtinguisher() {
    if (locked) return;
    locked = true;

    var pos = alt.Player.local.pos;

    allFires.forEach((el) => {
        var dis = math.checkDistance(el.x, el.y, pos.x, pos.y);

        if (dis <= fireExtinguishDistance) {
            var pif = math.positionInFront(pos, game.getGameplayCamRot(2), dis);

            if (math.checkPointInCircle(pif.x, pif.y, el.x, el.y, 2)) {
                var scale = (el.scale - 0.04);

                alt.emitServer("FIRE_EXTINGUISH", el.id, scale, "powder");
                log("FIRE", "Fire extinguish  id: " + el.id + "  scale: " + scale);
            }
        }
    });

    alt.setTimeout(() => {
        locked = false;
    }, 500);
}

export function useFireTruckWater(vehicle) {
    if (locked) return;
    locked = true;

    var pos = vehicle.pos;

    allFires.forEach((el) => {
        var dis = math.checkDistance(el.x, el.y, pos.x, pos.y);

        if (dis <= fireFireTruckDistance) {
            var pif = math.positionInFront(pos, game.getGameplayCamRot(2), dis);

            if (math.checkPointInCircle(pif.x, pif.y, el.x, el.y, 2)) {
                var scale = (el.scale - 0.04);

                alt.emitServer("FIRE_EXTINGUISH", el.id, scale, "water");
                log("FIRE", "Fire extinguish  id: " + el.id + "  scale: " + scale);
            }
        }
    });

    alt.setTimeout(() => {
        locked = false;
    }, 500);
}

export function useFireTruckFoam(vehicle) {
    if (locked) return;
    locked = true;

    var pos = vehicle.pos;

    allFires.forEach((el) => {
        var dis = math.checkDistance(el.x, el.y, pos.x, pos.y);

        if (dis <= fireFireTruckDistance) {
            var pif = math.positionInFront(pos, game.getGameplayCamRot(2), dis);

            if (math.checkPointInCircle(pif.x, pif.y, el.x, el.y, 2)) {
                var scale = (el.scale - 0.04);

                alt.emitServer("FIRE_EXTINGUISH", el.id, scale, "foam");
                log("FIRE", "Fire extinguish  id: " + el.id + "  scale: " + scale);
            }
        }
    });

    alt.setTimeout(() => {
        locked = false;
    }, 500);
}

alt.onServer('CREATE_FIRE', (id, x, y, z, particle, smoke, scale, gasoline) => {
    allFires.forEach((el) => {
        if (el.id == id) return;
    });

    var fire = new Fire(id, x, y, z, particle, smoke, scale, gasoline);
    allFires.push(fire);
});

alt.onServer('UPDATE_FIRE', (id, scale) => {
    allFires.forEach((el) => {
        if (el.id == id) {
            el.update(scale);
            return;
        }
    });
});

alt.onServer('REMOVE_FIRE', (id) => {
    allFires.forEach((el) => {
        if (el.id == id) {
            el.remove(id);
            return;
        }
    });
});

alt.onServer('CREATE_EXPLOSION', (type, x, y, z) => {
    game.addExplosion(x, y, z, type, explosionDamage, true, true, true, false);
});