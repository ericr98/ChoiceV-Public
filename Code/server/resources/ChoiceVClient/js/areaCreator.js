import alt from 'alt'; 
import game from 'natives';

import * as math from 'js/math.js';

var drawing = false;

var area = [];
var points = [];

var drawingTick = null;
function startDrawing() {
    if(drawingTick !== null) return;

    drawingTick = alt.everyTick(() => {
        area.forEach(function (item, index) { item.draw(area[(index + 1) % area.length]); });
        points.forEach(function (item, index) { item.draw(); });
    });
}


function stopDrawing() {
    if (drawingTick !== null) {
        alt.clearEveryTick(drawingTick);
        drawingTick = null;
    }
}

alt.onServer('SHOW_POINT', (x, y) => {    
    var point = new Vector2(x, y);
    point.r = 123;
    point.g = 76;
    point.b = 121;

    points.push(point);
    startDrawing();
});

alt.onServer('HIDE_POINTS', () => {
    points = [];

    stopDrawing();
});


alt.onServer('AREA_START', () => {
    var pos = alt.Player.local.pos;
    var edge = new Vector2(pos.x, pos.y);

    area.push(edge);
   
    startDrawing();
});

alt.onServer('AREA_ADD', (x, y) => {
    var edge = new Vector2(x, y);
    edge.r = 166;
    edge.g = 93;
    edge.b = 30;

    startDrawing();

    area.forEach((el) => {
        el.r = 171;
        el.g = 101;
        el.b = 21;
    });
    area.push(edge);
});

alt.onServer('AREA_END', () => {
    area = [];
    stopDrawing();
});

alt.on('keydown', (key) => {
    if (!drawing) {
        return;
    }
        
    if(key == 107) { //NumPad+ for adding
        var point = alt.Player.local.pos;

        var edge = new Vector2(point.x, point.y);
        edge.r = 166;
        edge.g = 93;
        edge.b = 30;
    
        area.forEach((el) => {
            el.r = 255;
            el.g = 255;
            el.b = 255;
        });
        area.push(edge);
    } else if(key == 109) { //NumPad- to remove last Edge
        area.pop();
    } else if(key == 96) { //NumPad0 to Save

        alt.emitServer("AREA_FINISHED", "FINISH", JSON.stringify(area));

        area = [];
        stopDrawing();
    } else if(key == 110) {
        alt.emitServer("AREA_FINISHED", "CANCEL", null);

        area = [];
        stopDrawing();
    }
});

class Vector2 {
    constructor(_x, _y) {
        this.x = _x;
        this.y = _y;

        this.r = 255;
        this.g = 255;
        this.b = 255;

        this.calculate();

        this.bool = true;
    }

    calculate() {
        this.p = 5000;
        this.m = -5000;
        this.line_1s = new math.Vector3(this.x, this.y, this.m);
        this.line_1e = new math.Vector3(this.x, this.y, this.p);
    }


    draw(lastPoint) {
        if (this.bool) {
            game.drawLine(this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.r, this.g, this.b, 255);

            if(lastPoint != null) {
                game.drawPoly(this.line_1s.x, this.line_1s.y, this.line_1s.z, lastPoint.line_1e.x, lastPoint.line_1e.y, lastPoint.line_1e.z, lastPoint.line_1s.x, lastPoint.line_1s.y, lastPoint.line_1s.z, this.r, this.g, this.b, 50);
                game.drawPoly(this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, lastPoint.line_1e.x, lastPoint.line_1e.y, lastPoint.line_1e.z, this.r, this.g, this.b, 50);
    
                game.drawPoly(lastPoint.line_1s.x, lastPoint.line_1s.y, lastPoint.line_1s.z, lastPoint.line_1e.x, lastPoint.line_1e.y, lastPoint.line_1e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.r, this.g, this.b, 50);
                game.drawPoly(lastPoint.line_1e.x, lastPoint.line_1e.y, lastPoint.line_1e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.r, this.g, this.b, 50);
            }
        }
    }
}