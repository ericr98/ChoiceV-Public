import * as alt from 'alt'; 
import * as game from 'natives';
import * as math from 'js/math.js';

alt.everyTick(() => {
    if (drawing) {
        if (colShape !== null) {
            colShape.draw();
        }
        colArray.forEach(function (item, index) { item.draw(); });
    }
});

var colArray = [];
var colShape = null;
var vhMode = false;
var fineMode = false;
var fineRes = 1.0;
var clientPrefix = "SPOT_";
var updateEvent = null;
var drawing = false;


alt.onServer('SPOT_START', (x, y, z, zDiv, width, height, id, angle, prefix, type, event) => {
    colShape = null;
    colShape = new twoDcolShape(x, y, z, zDiv, width, height, id, angle);
    colShape.type = type;
    colShape.event = event;

    clientPrefix = prefix || "SPOT_";
    if (drawing === false) {
        drawing = true;
    }
});

alt.onServer('SPOT_ADD', (x, y, z, zDiv, width, height, id, angle) => {
    colShape = new twoDcolShape(x, y, z, zDiv, width, height, id, angle);
    colShape.r = 171;
    colShape.g = 101;
    colShape.b = 21;

    colArray.push(colShape);
    colShape = null;
    
    if (drawing === false) {
        drawing = true;
    }
});

alt.onServer('SPOT_END', () => {
    colArray = [];
    colShape = null;

    if (drawing === true) {
        drawing = false;
    }
});

alt.on('keyup', (key) => {
    if (colShape === null) {
        return;
    }

    if (key === 96) {
        alt.emitServer(clientPrefix + "SAVE", colShape.x, colShape.y, colShape.width, colShape.height, colShape.angle, "" + colShape.id, "" + colShape.type, "" + colShape.event);
        colShape.r = 171;
        colShape.g = 101;
        colShape.b = 21;

        colArray.push(colShape);

        var type = colShape.type;
        var event = colShape.event;
        colShape = new twoDcolShape(colShape.x + 5, colShape.y + 5, colShape.z, 3, colShape.width, colShape.height, 0, 0);
        colShape.type = type;
        colShape.event = event;

    } else if (key === 110) {
        colArray = [];
        colShape = null;
        if (drawing === true) {
            drawing = false;
        }
    } else if (key === 111) {
        fineRes += 0.2;
        alt.log("Resolution: " + getFuc(fineRes));
    } else if (key === 106) {
        fineRes -= 0.2;
        alt.log("Resolution: " + getFuc(fineRes));
    } else if (key === 104) {
        colShape.y += getFuc(fineRes);
        colShape.calculate();
    } else if (key === 102) {
        colShape.x += getFuc(fineRes);
        colShape.calculate();
    } else if (key === 98) {
        colShape.y -= getFuc(fineRes);
        colShape.calculate();
    } else if (key === 100) {
        colShape.x -= getFuc(fineRes);
        colShape.calculate();
    } else if (key === 105) {
        colShape.width += getFuc(fineRes);
        colShape.calculate();
    } else if (key === 103) {
        colShape.width -= getFuc(fineRes);
        colShape.calculate();
    } else if (key === 99) {
        colShape.height += getFuc(fineRes);
        colShape.calculate();
    } else if (key === 97) {
        colShape.height -= getFuc(fineRes);
        colShape.calculate();
    } else if (key === 107) {
        colShape.angle -= getFuc(fineRes);
        colShape.angle = Math.abs(colShape.angle % 360);
        colShape.calculate();
    } else if (key === 109) {
        colShape.angle += getFuc(fineRes);
        colShape.angle = Math.abs(colShape.angle % 360);
        colShape.calculate();
    }
});

function getFuc(x) {
    return Math.pow(Math.E, -x-1);
}

class twoDcolShape {
    constructor(_x, _y, _z, _zdiv, _width, _height, _id = 0, _angle = 0) {
        this.x = _x;
        this.y = _y;
        this.z = _z;
        this.zdiv = _zdiv;
        this.width = _width;
        this.height = _height;
        this.id = _id;
        this.angle = (360 + _angle) % 360;
        this.calculate();

        this.type;
        this.event;

        this.r = 255;
        this.g = 255;
        this.b = 255;
    }

    calculate() {
        this.p = this.z + this.zdiv;
        this.m = this.z - this.zdiv;

        //horizontal lines
        this.line_1s = new math.Vector3(this.x, this.y, this.m);
        this.line_1e = new math.Vector3(this.x, this.y, this.p);
        this.line_2s = new math.Vector3(this.x + this.width, this.y, this.m);
        this.line_2e = new math.Vector3(this.x + this.width, this.y, this.p);
        this.line_3s = new math.Vector3(this.x, this.y + this.height, this.m);
        this.line_3e = new math.Vector3(this.x, this.y + this.height, this.p);
        this.line_4s = new math.Vector3(this.x + this.width, this.y + this.height, this.m);
        this.line_4e = new math.Vector3(this.x + this.width, this.y + this.height, this.p);

        this.rotate(this.line_2s, this.angle, -this.width / 2, this.height / 2);
        this.rotate(this.line_2e, this.angle, -this.width / 2, this.height / 2);
        this.rotate(this.line_3s, this.angle, this.width / 2, this.height / 2);
        this.rotate(this.line_3e, this.angle, this.width / 2, this.height / 2);
        this.rotate(this.line_1s, this.angle, -this.width / 2, -this.height / 2);
        this.rotate(this.line_1e, this.angle, -this.width / 2, -this.height / 2);
        this.rotate(this.line_4s, this.angle, this.width / 2, -this.height / 2);
        this.rotate(this.line_4e, this.angle, this.width / 2, -this.height / 2);

        // alt.log("LOG", "A " + this.line_1s.X + " " + this.line_1s.Y);
        // alt.log("LOG", "B " + this.line_2s.X + " " + this.line_2s.Y);
        // alt.log("LOG", "C " + this.line_3s.X + " " + this.line_3s.Y);
        // alt.log("LOG", "D " + this.line_4s.X + " " + this.line_4s.Y);
        this.bool = true;
    }

    rotate(v, R, Xos, Yos) {
        v.x = this.x + (Xos * Math.cos(math.degreesToRadians(R))) - (Yos * Math.sin(math.degreesToRadians(R)));
        v.y = this.y + (Xos * Math.sin(math.degreesToRadians(R))) + (Yos * Math.cos(math.degreesToRadians(R)));
    }

    draw() {
        if (this.bool) {
            game.drawLine(this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_2s.x, this.line_2s.y, this.line_2s.z, this.line_2e.x, this.line_2e.y, this.line_2e.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_3s.x, this.line_3s.y, this.line_3s.z, this.line_3e.x, this.line_3e.y, this.line_3e.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_4e.x, this.line_4e.y, this.line_4e.z, this.r, this.g, this.b, 255);

            // 1 to 2
            game.drawPoly(this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_2e.x, this.line_2e.y, this.line_2e.z, this.line_2s.x, this.line_2s.y, this.line_2s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_2e.x, this.line_2e.y, this.line_2e.z, this.r, this.g, this.b, 50);

            game.drawPoly(this.line_2s.x, this.line_2s.y, this.line_2s.z, this.line_2e.x, this.line_2e.y, this.line_2e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_2e.x, this.line_2e.y, this.line_2e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.r, this.g, this.b, 50);

            // 2 to 3
            game.drawPoly(this.line_2s.x, this.line_2s.y, this.line_2s.z, this.line_3e.x, this.line_3e.y, this.line_3e.z, this.line_3s.x, this.line_3s.y, this.line_3s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_2e.x, this.line_2e.y, this.line_2e.z, this.line_2s.x, this.line_2s.y, this.line_2s.z, this.line_3e.x, this.line_3e.y, this.line_3e.z, this.r, this.g, this.b, 50);

            game.drawPoly(this.line_3s.x, this.line_3s.y, this.line_3s.z, this.line_3e.x, this.line_3e.y, this.line_3e.z, this.line_2s.x, this.line_2s.y, this.line_2s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_3e.x, this.line_3e.y, this.line_3e.z, this.line_2s.x, this.line_2s.y, this.line_2s.z, this.line_2e.x, this.line_2e.y, this.line_2e.z, this.r, this.g, this.b, 50);

            //3 to 4
            game.drawPoly(this.line_3s.x, this.line_3s.y, this.line_3s.z, this.line_4e.x, this.line_4e.y, this.line_4e.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_3e.x, this.line_3e.y, this.line_3e.z, this.line_3s.x, this.line_3s.y, this.line_3s.z, this.line_4e.x, this.line_4e.y, this.line_4e.z, this.r, this.g, this.b, 50);

            game.drawPoly(this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_4e.x, this.line_4e.y, this.line_4e.z, this.line_3s.x, this.line_3s.y, this.line_3s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_4e.x, this.line_4e.y, this.line_4e.z, this.line_3s.x, this.line_3s.y, this.line_3s.z, this.line_3e.x, this.line_3e.y, this.line_3e.z, this.r, this.g, this.b, 50);

            // 4 to 1
            game.drawPoly(this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_4e.x, this.line_4e.y, this.line_4e.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.r, this.g, this.b, 50);

            game.drawPoly(this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.r, this.g, this.b, 50);
            game.drawPoly(this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_4e.x, this.line_4e.y, this.line_4e.z, this.r, this.g, this.b, 50);


            //vertical
            game.drawLine(this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_2s.x, this.line_2s.y, this.line_2s.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_2s.x, this.line_2s.y, this.line_2s.z, this.line_3s.x, this.line_3s.y, this.line_3s.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_3s.x, this.line_3s.y, this.line_3s.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.r, this.g, this.b, 255);

            game.drawLine(this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_2e.x, this.line_2e.y, this.line_2e.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_2e.x, this.line_2e.y, this.line_2e.z, this.line_3e.x, this.line_3e.y, this.line_3e.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_3e.x, this.line_3e.y, this.line_3e.z, this.line_4e.x, this.line_4e.y, this.line_4e.z, this.r, this.g, this.b, 255);
            game.drawLine(this.line_4e.x, this.line_4e.y, this.line_4e.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.r, this.g, this.b, 255);

            // 4 to 1
            // game.drawPoly(this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.r, this.g, this.b, 50);
            // game.drawPoly(this.line_4e.x, this.line_4e.y, this.line_4e.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.r, this.g, this.b, 50);

            // game.drawPoly(this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.r, this.g, this.b, 50);
            // game.drawPoly(this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_4s.x, this.line_4s.y, this.line_4s.z, this.line_4e.x, this.line_4e.y, this.line_4e.z, this.r, this.g, this.b, 50);
                    
        // game.drawPoly(this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_2e.x, this.line_2e.y, this.line_2e.z, this.line_2s.x, this.line_2s.y, this.line_2s.z);
            // game.drawPoly(this.line_1e.x, this.line_1e.y, this.line_1e.z, this.line_1s.x, this.line_1s.y, this.line_1s.z, this.line_2s.z, this.line_2e.x, this.line_2e.y, this.line_2e.z);

            //API.drawLine(this.line_1s, this.line_1e, 255, this.id === 0 ? 255 : 0, this.id === 0 ? 0 : 255, 255);
            //API.drawLine(this.line_2s, this.line_2e, 255, this.id === 0 ? 255 : 0, this.id === 0 ? 0 : 255, 0);
            //API.drawLine(this.line_3s, this.line_3e, 255, this.id === 0 ? 255 : 0, this.id === 0 ? 0 : 255, 0);
            //API.drawLine(this.line_4s, this.line_4e, 255, this.id === 0 ? 255 : 0, this.id === 0 ? 0 : 255, 0);
        }
    }
}