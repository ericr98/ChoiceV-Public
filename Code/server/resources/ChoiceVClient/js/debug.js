/// <reference path="../types/alt-client.d.ts" />
/// <reference path="../types/alt-shared.d.ts" />
/// <reference path="../types/natives.d.ts" />
import alt from 'alt-client'; 
import game from 'natives';

import { Vector3 } from './math.js';

//Grid
var gridList = [];

class Grid {
    constructor(x, y, width, height) {
        this.a = new Vector3(x, y, -1);
        this.b = new Vector3(x + width, y, -1);
        this.c = new Vector3(x, y + height, -1);
        this.d = new Vector3(x + width, y + height, -1);
    }

    draw() {
        game.drawLine(this.a.x, this.a.y, -5000, this.a.x, this.a.y, 5000, 165, 91, 34, 255);
        game.drawLine(this.b.x, this.b.y, -5000, this.b.x, this.b.y, 5000, 165, 91, 34, 255);
        game.drawLine(this.c.x, this.c.y, -5000, this.c.x, this.c.y, 5000, 165, 91, 34, 255);
        game.drawLine(this.d.x, this.d.y, -5000, this.d.x, this.d.y, 5000, 165, 91, 34, 255);

        game.drawBox(this.a.x, this.a.y, -5000, this.b.x, this.b.y, 5000, 165, 91, 34, 25);
        game.drawBox(this.b.x, this.b.y, -5000, this.c.x, this.c.y, 5000, 165, 91, 34, 25);
        game.drawBox(this.c.x, this.c.y, -5000, this.d.x, this.d.y, 5000, 165, 91, 34, 25);
        game.drawBox(this.d.x, this.d.y, -5000, this.a.x, this.a.y, 5000, 165, 91, 34, 25);
    }
}


var gridTick = null;
alt.onServer(("SHOW_GRID"), (x, y, width, height) => {
    var grid = new Grid(x, y, width, height);
    gridList.push(grid);

    gridTick = alt.everyTick(() => {
        gridList.forEach((el) => {
            el.draw();
        });
    }); 
});

alt.onServer(("STOP_GRIDS"), () => {
    gridList = [];
    
    alt.clearEveryTick(gridTick);
    gridTick = null;
});

alt.onServer('TP_TO_WAYPOINT', () => {
    var blip = game.getFirstBlipInfoId(8);
    if (game.doesBlipExist(blip)) {
        var coords = game.getBlipInfoIdCoord(blip);
        game.setEntityCoords(alt.Player.local.scriptID, coords.x, coords.y, 0, true, false, false, true);
        alt.setTimeout(() => {
            var j = 0;
            var z = 0;
            while (j <= 60 && z == 0) {
                z = game.getGroundZFor3dCoord(coords.x, coords.y, j * 25, false, true)[1];
                j++;
            }

            if (z != 0) {
                game.setEntityCoords(alt.Player.local.scriptID, coords.x, coords.y, z, true, false, false, true);
            }
        }, 500);
    }
});


// Show Positions

var positions = [];
var tick = null;

alt.onServer("SHOW_POSITIONS", (newPos, r, g, b) => {
    if (!r) r = 123;
    if (!g) g = 10;
    if (!b) b = 52;
    newPos = newPos.map((pos) => {
        return { pos: new Vector3(pos.x, pos.y, pos.z), r: r, g: g, b: b };
    });

    positions = positions.concat(newPos);

    if (tick === null) {
        tick = alt.everyTick(() => {
            positions.forEach((entry) => {
                const pos = entry.pos;
                game.drawBox(pos.x - 0.15, pos.y - 0.15, pos.z - 0.15, pos.x + 0.15, pos.y + 0.15, pos.z + 0.15, entry.r, entry.g, entry.b, 255);
                game.drawLine(pos.x, pos.y, -5000, pos.x, pos.y, 5000, entry.r, entry.g, entry.b, 255);
            });
        });
    }
});

alt.onServer("REMOVE_SHOW_POSITION", (toRemove) => {
    positions = positions.filter((pos) => {
        return pos.distanceTo(toRemove) > 0.1;
    });
});

alt.onServer("STOP_SHOW_POSITIONS", () => {
    positions = [];
    alt.clearEveryTick(tick);
    tick = null;
});

alt.onServer("TAKE_HEAP_SNAPSHOT", async (chunkSize) => {
    var s = await alt.Profiler.takeHeapSnapshot();
    var chunks = [];
    for(var i = 0; i < s.length; i += chunkSize) {
        chunks.push(s.substring(i, i + chunkSize));
    }

    alt.log("Heap snapshot size: " + s.length + " characters with " + chunks.length + " chunks");
    for(var i = 0; i < chunks.length; i++) {
        alt.emitServer("SENT_HEAP_SNAPSHOT_PART", i, chunks[i], i == chunks.length - 1);
    }
});
