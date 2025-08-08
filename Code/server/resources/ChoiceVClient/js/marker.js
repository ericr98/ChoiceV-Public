import alt from 'alt'; 
import game from 'natives';

import { log } from 'js/client.js';

var markerList = [];

class Marker {
    constructor(id, type, x, y, z, r, g, b, alpha, scale, showDistance) {
        this.id = id;
        this.type = type;
        this.x = x; 
        this.y = y;
        this.z = z;

        this.r = r;
        this.g = g;
        this.b = b;
        this.alpha = alpha;

        this.scale = scale;

        this.showDistance = showDistance;

        this.show = false;
    }

    check(x, y) {
        if(getDistance(this.x, this.y, x, y) < this.showDistance && !this.show) {
            log("MARKER", `Showing marker ${this.id}`);
            this.show = true;

            this.marker = new alt.Marker(this.type, new alt.Vector3(this.x, this.y, this.z), new alt.RGBA(this.r, this.g, this.b, this.alpha));
            this.marker.scale = new alt.Vector3(this.scale, this.scale, this.scale);
        }
        
        if(getDistance(this.x, this.y, x, y) > this.showDistance && this.show) {
            log("MARKER", `Hiding marker ${this.id}`);
            this.show = false;

            if(this.marker) {
                this.marker.destroy();
                this.marker = null;
            }
        }
    }

    destroy() {
        if(this.marker) {
            this.marker.destroy();
        }
    }
}

function getDistance(x1, y1, x2, y2) {
    var a = x1 - x2;
    var b = y1 - y2;

    return Math.sqrt(a*a + b*b); 
}

export function onTick() {
    var pos = alt.Player.local.pos;

    markerList.forEach((el) => {
        el.check(pos.x, pos.y);
    });
}

alt.onServer('CREATE_MARKER', (id, type, x, y, z, r, g, b, alpha, scale, showDistance) => {
    log("MARKER", `Creating marker ${id} at ${x}, ${y}, ${z} with type ${type} and color ${r}, ${g}, ${b}, ${alpha} and scale ${scale} and show distance ${showDistance}`);
    createMarker(id, type, x, y, z, r, g, b, alpha, scale, showDistance);
});

alt.onServer('DELETE_MARKER', (id) => {
    deleteMarker(id);
});


export function createMarker(id, type, x, y, z, r, g, b, alpha, scale, showDistance) {
    var marker = new Marker(id, type, x, y, z, r, g, b, alpha, scale, showDistance);
    markerList.push(marker);

    return marker;
}

export function deleteMarker(id) {
    var marker = getMarker(id);
    marker.destroy();

    markerList = markerList.filter(marker => marker.id != id);
}

export function getMarker(id) {
    return markerList.filter(marker => marker.id == id)[0];
}


var labels = [];

alt.onServer("SHOW_TEXT_LABEL", (text, pos, textsize) => {
    var label = new alt.TextLabel(text, "", textsize, 1, pos, new alt.Vector3(0, 0, 0), new alt.RGBA(255, 255, 255, 255), 1, new alt.RGBA(0, 0, 0, 255), false);
    label.faceCamera = true;
    labels.push(label);
});

alt.onServer("REMOVE_ALL_TEXT_LABELS", () => {
    labels.forEach((el) => {
        el.destroy();
    });

    labels = [];
})
