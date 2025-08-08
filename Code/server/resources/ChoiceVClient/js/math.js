import * as alt from 'alt';

export class Vector3 {
    constructor(posX, posY, posZ) {
        this.x = posX;
        this.y = posY;
        this.z = posZ;
    }
}

export class Vector2 {
    constructor(posX, posY) {
        this.x = posX;
        this.y = posY;
    }
}

export function radiansToDegrees(radians) {
    return (radians * (180 / 3.14159));
}

export function degreesToRadians(degrees) {
    return (degrees * (3.14159 / 180));
}

export function checkDistance(x1, y1, x2, y2) {
    var a = x1 - x2;
    var b = y1 - y2;

    return Math.sqrt(a * a + b * b);
}

export function checkPointInCircle(a, b, x, y, r) {
    var p = (a - x) * (a - x) + (b - y) * (b - y);

    return (p < (r *= r));
}

function getAreaTriangle(x1, y1, x2, y2, x3, y3) {
    return Math.abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);
}

export function checkPointInRect(x1, y1, x2, y2, x3, y3, x4, y4, x, y) {
    var A1 = getAreaTriangle(x1, y1, x2, y2, x3, y3) + getAreaTriangle(x1, y1, x4, y4, x3, y3);
    var A2 = getAreaTriangle(x, y, x1, y1, x2, y2);
    var A3 = getAreaTriangle(x, y, x2, y2, x3, y3);
    var A4 = getAreaTriangle(x, y, x3, y3, x4, y4);
    var A5 = getAreaTriangle(x, y, x1, y1, x4, y4);

    var A = (Math.round(A1 * 1000) / 1000);
    var B = (Math.round((A2 + A3 + A4 + A5) * 1000) / 1000);

    return (A == B);
}

export function rotatePointInRect(pointX, pointY, originX, originY, angle) {
    return {
        x: Math.cos(angle) * (pointX - originX) - Math.sin(angle) * (pointY - originY) + originX,
        y: Math.sin(angle) * (pointX - originX) + Math.cos(angle) * (pointY - originY) + originY
    };
}

export function forwardVectorFromRotation(rotation) {
    var z = degreesToRadians(rotation.z);
    var x = degreesToRadians(rotation.x);

    return new alt.Vector3(-Math.sin(z) * Math.abs(Math.cos(x)), Math.cos(z) * Math.abs(Math.cos(x)), Math.sin(x));
}

export function positionInFront(position, rotation, distance) {
    var fove = forwardVectorFromRotation(rotation);
    var scal = new alt.Vector3(fove.x * distance, fove.y * distance, fove.z * distance);

    return new alt.Vector3(position.x + scal.x, position.y + scal.y, position.z + scal.z);
}

export function toInt32(x) {
    var uint32 = ToUint32(x);
    if (uint32 >= Math.pow(2, 31)) {
        return uint32 - Math.pow(2, 32)
    } else {
        return uint32;
    }
}

export function ToUint32(x) {
    return modulo(ToInteger(x), Math.pow(2, 32));
}

function modulo(a, b) {
    return a - Math.floor(a / b) * b;
}

function ToInteger(x) {
    x = Number(x);
    return x < 0 ? Math.ceil(x) : Math.floor(x);
}

export class Quaternion {
    constructor(w, x, y, z) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}

export function toQuaternion(x, y, z) {
    var c1 = Math.cos(x / 2);
    var c2 = Math.cos(y / 2);
    var c3 = Math.cos(z / 2);
    var s1 = Math.sin(x / 2);
    var s2 = Math.sin(y / 2);
    var s3 = Math.sin(z / 2);

    var xQ = s1 * c2 * c3 + c1 * s2 * s3;
    var yQ = c1 * s2 * c3 - s1 * c2 * s3;
    var zQ = c1 * c2 * s3 + s1 * s2 * c3;
    var wQ = c1 * c2 * c3 - s1 * s2 * s3;

    return new Quaternion(wQ, xQ, yQ, zQ);
}

const copysign = (x, y) => Math.sign(x) === Math.sign(y) ? x : -x;

export function toEuler(q) {
    var angles = {};

    var sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
    var cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
    angles.x = Math.atan2(sinr_cosp, cosr_cosp);

    // pitch (y-axis rotation)
    var sinp = 2 * (q.w * q.y - q.z * q.x);
    if (Math.abs(sinp) >= 1)
        angles.y = copysign(M_PI / 2, sinp); // use 90 degrees if out of range
    else
        angles.y = Math.asin(sinp);

    // yaw (z-axis rotation)
    var siny_cosp = 2 * (q.w * q.z + q.x * q.y);
    var cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
    angles.z = Math.atan2(siny_cosp, cosy_cosp);

    return angles;
}

export function normalizeQuaternion(q1) {
    var w = q1.w;
    var x = q1.x;
    var y = q1.y;
    var z = q1.z;

    var norm = Math.sqrt(w * w + x * x + y * y + z * z);

    if (norm < 1e-16) {
        return new Quaternion(0, 0, 0, 0);
    }

    norm = 1 / norm;

    return new Quaternion(w * norm, x * norm, y * norm, z * norm);
}

export function slerpQuaternion(q1, q2, pct) {
    var w1 = q1.w;
    var x1 = q1.x;
    var y1 = q1.y;
    var z1 = q1.z;

    var w2 = q2.w;
    var x2 = q2.x;
    var y2 = q2.y;
    var z2 = q2.z;

    var cosTheta0 = w1 * w2 + x1 * x2 + y1 * y2 + z1 * z2;

    if (cosTheta0 < 0) {
        w1 = -w1;
        x1 = -x1;
        y1 = -y1;
        z1 = -z1;
        cosTheta0 = -cosTheta0;
    }

    if (cosTheta0 > 0.9995) { // DOT_THRESHOLD
        return normalizeQuaternion(new Quaternion(
            w1 + pct * (w2 - w1),
            x1 + pct * (x2 - x1),
            y1 + pct * (y2 - y1),
            z1 + pct * (z2 - z1)));
    }

    var Theta0 = Math.acos(cosTheta0);
    var sinTheta0 = Math.sin(Theta0);


    var Theta = Theta0 * pct;
    var sinTheta = Math.sin(Theta);
    var cosTheta = Math.cos(Theta);

    var s0 = cosTheta - cosTheta0 * sinTheta / sinTheta0;
    var s1 = sinTheta / sinTheta0;

    return new Quaternion(
        s0 * w1 + s1 * w2,
        s0 * x1 + s1 * x2,
        s0 * y1 + s1 * y2,
        s0 * z1 + s1 * z2);
}

export function getDistance(x1, y1, x2, y2) {
    var a = x1 - x2;
    var b = y1 - y2;

    return Math.sqrt(a*a + b*b); 
}
