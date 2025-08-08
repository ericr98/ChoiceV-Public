import * as alt from 'alt-client'
import * as game from 'natives'; 

import * as chat from '/js/chat.mjs';

//Interior

alt.onServer("INTERIOR_RELOAD", () => {
	var pos = alt.Player.local.pos;
	var int = game.getInteriorAtCoords(pos.x, pos.y, pos.z);
	
	game.refreshInterior(int);
})

//VoiceChat

var hour = -1;
alt.onServer("SET_DATE_TIME_HOUR", (h) => {
    hour = h;
});

alt.everyTick(() => {
    var player = alt.Player.local;

    var date = new Date();
    game.setClockDate(date.getDate(), date.getMonth(), date.getFullYear());
	
    if(hour == -1) {
        game.setClockTime(toInt32(date.getHours()), toInt32(date.getMinutes()), toInt32(date.getSeconds()));
    } else {
        game.setClockTime(toInt32(hour), 0, 0);
    }
});

function toInt32(x) {
    var uint32 = ToUint32(x);
    if (uint32 >= Math.pow(2, 31)) {
        return uint32 - Math.pow(2, 32)
    } else {
        return uint32;
    }
}

function ToUint32(x) {
    return modulo(ToInteger(x), Math.pow(2, 32));
}

function modulo(a, b) {
    return a - Math.floor(a/b)*b;
}

function ToInteger(x) {
    x = Number(x);
    return x < 0 ? Math.ceil(x) : Math.floor(x);
}

//Animations
alt.on('keyup', (key) => {
	if(chat.opened) {
		return;
	}

	switch (key) {
		case 90: //Z
			playAnimation("amb@code_human_cower@female@base", "base", 10000000, 1);
			break;
		case 85: //U
			playAnimation("amb@world_human_picnic@male@idle_a", "idle_b", 1000000, 1);
			break;
		case 73: //I
			playAnimation("amb@prop_human_parking_meter@female@idle_a", "idle_b_female", 10000000, 1);
			break;
		case 72: //H
			playAnimation("random@mugging3", "handsup_standing_base", 10000000, 1);
			break;
		case 74: //J
			playAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", 1000000, 1);
			break;
		case 75: // K
			playAnimation("random@arrests@busted", "idle_c", 1000000, 1);
			break;
		case 76: //L
			playAnimation("amb@world_human_sunbathe@male@back@idle_a", "idle_a", 1000000, 1);
			break;
		case 78: //N
			playAnimation("amb@medic@standing@kneel@base", "base", 1000000, 1);
			break;
		case 77: //M
			playAnimation("missmic2leadinmic_2_intleadout", "ko_on_floor_idle", 1000000, 1);
			break;
		case 188: //,
			stopAnimation();
			break;
		case 189: //-
			alt.emitServer("TOGGLE_VOICE_RANGE");
			break;
		case 88: //X
			alt.emitServer("TOGGLE_MUTE_VOICE");
			break;
	}
});

function playAnimation(dict, name, duration, flag) {
	var player = alt.Player.local.scriptID;

    game.setPedCanRagdoll(player, true);
    game.requestAnimDict(dict);

    alt.setTimeout( () => {
        game.taskPlayAnim(player, dict, name, 8.0, 1.0, duration, flag, 0.5, false, false, false);
    }, 500);
}

function stopAnimation() {
    var player = alt.Player.local.scriptID;

    game.clearPedTasks(player);
}


//Connect
alt.on('connectionComplete', () => {
	alt.emitServer("SOCIAL_CLUB_REGISTER", game.scAccountInfoGetNickname());
	
	var miloList = ["prologue01", "prologue01c", "prologue01d", "prologue01e", "prologue01f", "prologue01g", "prologue01h", "prologue01i", "prologue01j", "prologue01k", "prologue01z", "prologue02", "prologue03", "prologue03b", "prologue03_grv_dug", "prologue_grv_torch", "prologue04", "prologue04b", "prologue04_cover", "des_protree_end", "des_protree_start", "prologue05", "prologue05b", "prologue06", "prologue06b", "prologue06_int", "prologue06_pannel", "plg_occl_00", "prologue_occl", "prologuerd", "prologuerdb" ]

	miloList.forEach((el) => {
	 alt.requestIpl(el);
	});
	
	alt.loadDefaultIpls();
	
	
    // //Max stats of you character
    alt.setStat("stamina", 100);
    alt.setStat("strength", 100);
    alt.setStat("lung_capacity", 100);
    alt.setStat("wheelie_ability", 100);
    alt.setStat("flying_ability", 100);
    alt.setStat("shooting_ability", 100);
    alt.setStat("stealth_ability", 100);

     alt.setInterval(() => {
    //     alt.setConfigFlag(429, true);
    //     alt.setConfigFlag(241, true);
    //     alt.setConfigFlag(184, true);
    //     alt.setConfigFlag(35, false);

        game.setPedConfigFlag(alt.Player.local.scriptID, 429, true); // disable start engine
        game.setPedConfigFlag(alt.Player.local.scriptID, 241, true); // disable stop engine
        game.setPedConfigFlag(alt.Player.local.scriptID, 184, true); // disable auto shuffle
        game.setPedConfigFlag(alt.Player.local.scriptID, 35, false); // disable auto helmet
        game.setPedConfigFlag(alt.Player.local.scriptID, 248, false); //Disable Vehicle Random Animations
    }, 100);

});

//Model

alt.on("syncedMetaChange", (entity, key, value) => {
	if(key == "PLAYER_INVISIBLE") {
		game.setEntityVisible(entity.scriptID, !value, 0);
		game.setEntityAlpha(entity.scriptID, value ? 0 : 255, 0);
	} else if( key == "PLAYER_NO_COLLISION") {
		game.setEntityCollision(entity.scriptID, !value, true);
	}
});

alt.on("gameEntityCreate", (entity) => {
	if(entity.hasSyncedMeta("PLAYER_INVISIBLE")) {
		var invis = entity.getSyncedMeta("PLAYER_INVISIBLE");
		game.setEntityVisible(entity.scriptID, !invis, 0);
		game.setEntityAlpha(entity.scriptID, invis ? 0 : 255, 0);
	}
});

alt.onServer("PLAYER_INVISIBLE", (invisible) => {
	game.setEntityVisible(alt.Player.local.scriptID, !invisible, 0);
	game.setEntityAlpha(alt.Player.local.scriptID, invisible ? 0 : 255, 0);
});

alt.onServer("CLEAR_PED_BLOOD", (target) => {
	game.clearPedBloodDamage(target.scriptID);
});

//Vehicle

alt.onServer("SET_PED_INTO_VEHICLE", (veh) => {
	alt.setTimeout(() => {
		game.setPedIntoVehicle(alt.Player.local.scriptID, veh.scriptID, -1);
	}, 500);
});

//Labels
alt.onServer('TEXT_LABEL_ON_PLAYER', (player, msg, time) => {
    if (player !== null) {
        let id = player.id;

        var interval = alt.setInterval(() => {
            if (!player || !player.valid) {
                alt.clearInterval(interval[id]);
                return;
            }
                drawText3d(
                    player,
                    `${msg}`,
                    0.35,
                    4,
                    194,
                    162,
                    218,
                    255,
                    true,
                    false
                );
        }, 0);

        alt.setTimeout(() => {
            alt.clearInterval(interval);
        }, time);
    }
});

function drawText3d(
    player,
    msg,
    scale,
    fontType,
    r,
    g,
    b,
    a,
    useOutline = true,
    useDropShadow = true,
    layer = 0
) {
    let hex = msg.match('{.*}');
    if (hex) {
        const rgb = hexToRgb(hex[0].replace('{', '').replace('}', ''));
        r = rgb[0];
        g = rgb[1];
        b = rgb[2];
        msg = msg.replace(hex[0], '');
    }
    const localPlayer = player;
    const playerPos = localPlayer.pos;
    const entity = localPlayer.vehicle ? localPlayer.vehicle.scriptID : localPlayer.scriptID;
    const vector = game.getEntityVelocity(entity);
    const frameTime = game.getFrameTime();
    game.setDrawOrigin(playerPos.x + (vector.x * frameTime), playerPos.y + (vector.y * frameTime), playerPos.z + (vector.z * frameTime) + 1, 0);
    game.beginTextCommandDisplayText('STRING');
    game.addTextComponentSubstringPlayerName(msg);
    game.setTextFont(fontType);
    game.setTextScale(1, scale);
    game.setTextWrap(0.0, 1.0);
    game.setTextCentre(true);
    game.setTextColour(r, g, b, a);

    if (useOutline) game.setTextOutline();

    if (useDropShadow) game.setTextDropShadow();

    game.endTextCommandDisplayText(0, 0, 0);
    game.clearDrawOrigin();
}

//CAMERA!

var spectateCam = false;
var spectateTarget = null;
alt.onServer('SPECTATE_CAM', (target) => {
	var coords = game.getEntityCoords(target.scriptID, true);
	createCam(coords.x, coords.y, coords.z, 0, 0, 0);
	offset = new Vector3(0, 0, 0);
	alt.setTimeout(() => {
		spectateCam = true;
		spectateTarget = target;
		game.attachCamToEntity(camera, target.scriptID, 0, 0, 1, true);
	}, 100);
});

alt.onServer('END_SPECTATE_CAM', () => {
	spectateCam = false;
	spectateTarget = null;

	destroyCam();
	
	game.clearFocus();
	game.clearHdArea();
});

class Vector3 {
    constructor(posX, posY, posZ) {
        this.x = posX;
        this.y = posY;
        this.z = posZ;
    }
}

export var camera = null;

export function createCam(posX, posY, posZ, rotX, rotY, rotZ) {
	destroyCam();
	alt.setTimeout(() => {
		camera = game.createCam("DEFAULT_SCRIPTED_CAMERA", true);
		game.setCamCoord(camera, posX, posY, posZ);
		game.setCamRot(camera, rotX, rotY, rotZ, 2);
		game.setCamFov(camera, 60.0);

		game.setCamActive(camera, true);
		game.renderScriptCams(true, false, 0, true, false, 0);
	}, 0);
};

export function destroyCam () {
	if(camera == null){
		return;
	}

	alt.setTimeout(() => {
		if (!game.doesCamExist(camera)) return;
		game.setCamActive(camera, false);
		game.destroyCam(camera, true);
		game.renderScriptCams(false, true, 0, true, true, 0);
		camera = null;
		game.setFollowPedCamViewMode(1);
	}, 0);
};

export function hideGui(toggle) {
     game.displayHud(!toggle);
     alt.toggleGameControls(!toggle);
     game.displayRadar(!toggle);
}

alt.on('connectionComplete', () => {
	game.destroyAllCams(true);
});


//FreeCam Mode
var freeCamMode = false;
var speed = 15;
var rotSpeed = 0.6;

var fowardPressed;
var backwardPressed;
var leftPressed;
var rightPressed;

var leftRollPressed;
var rightRollPressed;

var rotScaleToogle = false;
var camPos;
var offset;

var interPolCams = [];
var camDriving = false;

class Cam {
    constructor(posX, posY, posZ, rotX, rotY, rotZ, durationToNext) {
        this.x = posX;
        this.y = posY;
		this.z = posZ;
		this.rotX = rotX;
        this.rotY = rotY;
		this.rotZ = rotZ;
		this.duration = durationToNext; 
    }
}

alt.onServer('FREE_CAM', () => {	
	var player = alt.Player.local;
	var pos = game.getEntityCoords(player.scriptID, true);
	var rot = game.getGameplayCamRot(2);
	camPos = new Vector3(pos.x, pos.y, pos.z);
	offset = new Vector3(0, 0, 0);
	
	createCam(camPos.x, camPos.y, camPos.z, rot.x, rot.y, rot.z);
	freeCamMode = true;
});

alt.everyTick(() => {

	if(!freeCamMode && !spectateCam) {
		return;
	}

	drawInterpolCam();

	if(freeCamMode) {
		if(fowardPressed) { //W
			var multX = Math.sin(offset.z * Math.PI / 180);
			var multY = Math.cos(offset.z * Math.PI / 180);
			var multZ = Math.sin(offset.x * Math.PI / 180);

			camPos.x = camPos.x - (0.1 * speed * multX);
			camPos.y = camPos.y + (0.1 * speed * multY);
			camPos.z = camPos.z + (0.1 * speed * multZ);

			//camPos.y = camPos.y + speed;
		}

		if(backwardPressed) { //S
			var multX = Math.sin(offset.z * Math.PI / 180);
			var multY = Math.cos(offset.z * Math.PI / 180);
			var multZ = Math.sin(offset.x * Math.PI / 180);

			camPos.x = camPos.x + (0.1 * speed * multX);
			camPos.y = camPos.y - (0.1 * speed * multY);
			camPos.z = camPos.z - (0.1 * speed * multZ);

			//camPos.y = camPos.y - speed;
		}

		if(leftPressed) { //A
			var multX = Math.sin((offset.z + 90) * Math.PI / 180);
			var multY = Math.cos((offset.z + 90) * Math.PI / 180);
			var multZ = Math.sin(offset.y * Math.PI / 180);

			camPos.x = camPos.x - (0.1 * speed * multX);
			camPos.y = camPos.y + (0.1 * speed * multY);
			camPos.z = camPos.z + (0.1 * speed * multZ);

			//camPos.x = camPos.x - speed;
		}

		if(rightPressed) { //D
			var multX = Math.sin((offset.z + 90) * Math.PI / 180);
			var multY = Math.cos((offset.z + 90) * Math.PI / 180);
			var multZ = Math.sin(offset.y) * Math.PI / 180;

			camPos.x = camPos.x + (0.1 * speed * multX);
			camPos.y = camPos.y - (0.1 * speed * multY);
			camPos.z = camPos.z - (0.1 * speed * multZ);

			//camPos.x = camPos.x + speed;
		}

		if(leftRollPressed) {
			offset.y = offset.y - rotSpeed;
		}

		if(rightRollPressed) {
			offset.y = offset.y + rotSpeed;
		}
	}

	offset.x = offset.x - (game.getControlNormal(1, 2) * rotSpeed * 8.0);
	offset.z = offset.z - (game.getControlNormal(1, 1) * rotSpeed * 8.0);

	changeOffsetIfNeeded();


	if(freeCamMode) {
		game.setHdArea(camPos.x, camPos.y, camPos.z, 100);
		game.setFocusPosAndVel(camPos.x, camPos.y, camPos.z, 0.0, 0.0, 0.0);
		game.setCamCoord(camera, camPos.x, camPos.y, camPos.z);
		game.setEntityCoords(alt.Player.local.scriptID, camPos.x, camPos.y, camPos.z + 1, true, false, false, false);
	}

	if(spectateCam) {
		var coords = game.getEntityCoords(spectateTarget, true);

		game.setHdArea(coords.x, coords.y, coords.z, 100);
		game.setFocusPosAndVel(coords.x, coords.y, coords.z, 0.0, 0.0, 0.0);

		game.setEntityCoords(alt.Player.local.scriptID, coords.x, coords.y, coords.z, true, false, false, false);
	}

	game.setCamRot(camera, offset.x, offset.y, offset.z, 2);
});

function drawInterpolCam() {
	interPolCams.forEach((el) => {
		if(!camDriving)
			game.drawBox(el.x - 0.5, el.y - 0.5, el.z - 0.5, el.x + 0.5, el.y + 0.5, el.z + 0.5, 171, 101, 21, 255);
	});
}

function changeOffsetIfNeeded() {
	if (offset.x > 90.0) {
		offset.x = 90.0;
	} else if (offset.x < -90.0)  {
		offset.x = -90.0;
	}

	if (offset.y > 90.0) {
		offset.y = 90.0;
	} else if (offset.y < -90.0)  {
		offset.y = -90.0;
	}
	if (offset.z > 360.0) { 
		offset.z = offset.z - 360.0 ;
	} else if (offset.z < -360.0) {
		offset.z = offset.z + 360.0;
	}
}

function endFreeModeCam(teleportToPos) {
	game.clearFocus();
	game.clearHdArea();
	destroyCam();
	freeCamMode = false;
	speed = 3;
	rotSpeed = 1.5;

	if(teleportToPos) {
		game.setEntityCoords(alt.Player.local.scriptID, camPos.x, camPos.y, camPos.z, true, false, false, true);
	}

	alt.emitServer("END_FREECAM");
}

// var splineCam;
// function spline() {
// 	destroyCam();

// 	alt.setTimeout(() => {
// 		var c = interPolCams.pop();
// 		splineCam = game.createCam("DEFAULT_SPLINE_CAMERA", 0);
// 		game.setCamCoord(splineCam, c.x, c.y, c.z);
// 		game.setCamRot(splineCam, c.rotX, c.rotY, c.rotZ, 2);
// 		game.setCamFov(splineCam, 60);
		
// 		var c2 = interPolCams.pop();
		
// 		game.addCamSplineNode(splineCam, c2.x, c2.y, c2.z, c2.rotX, c2.rotY, c2.rotZ, 1000, 3, 2);

// 		game.setCamActive(splineCam, true);
// 		alt.everyTick(() => {
// 			game._0x0923DBF87DFF735E(c.x, c.y, c.z);
// 			game.setCamSplinePhase(splineCam, 0);
// 		});
// 		game.renderScriptCams(true, false, 0, false, false);
// 	}, 100);
// }

function camDrive(dur, easePos, easeRot) {
	destroyCam();
	var d = dur / interPolCams.length;
	interPolCams.reverse();
	camDriving = true;
	recCam(d, easePos, easeRot);
}

var stop;
function recCam(dur, easePos, easeRot) {

	var c2 = interPolCams.pop();
	var c = interPolCams[interPolCams.length - 1];
	if(c != null) {
		interpolateCamera(c.x, c.y, c.z, c.rotX, c.rotY, c.rotZ, 60, c2.x, c2.y, c2.z, c2.rotX, c2.rotY, c2.rotZ, 60, dur, easePos, easeRot);
 
		alt.setTimeout(() => {
			recCam(dur, easePos, easeRot);
		}, dur);
	} else {
		camDriving = false;
	}
}

var from;
var interpolCam;
function interpolateCamera(pos1X, pos1Y, pos1Z, rot1X, rot1Y, rot1Z, fov, pos2X, pos2Y, pos2Z, rot2X, rot2Y, rot2Z, fov2, duration, easePos, easeRot) {
    
    if (camera != null || interpolCam != null) {
        destroyCam();
    }

    game.setFocusPosAndVel(pos2X, pos2Y, pos2Z, pos1X, pos1Y, pos1Z);
    game.setHdArea(pos1X, pos1Y, pos1Z, 200);

	from = game.createCam("DEFAULT_SCRIPTED_CAMERA", true);
	game.setCamCoord(from, pos1X, pos1Y, pos1Z);
	game.setCamRot(from, rot1X, rot1Y, rot1Z, 2);
	game.setCamFov(from,fov);

	interpolCam = game.createCam("DEFAULT_SCRIPTED_CAMERA", true);
	game.setCamCoord(interpolCam, pos2X, pos2Y, pos2Z);
	game.setCamRot(interpolCam, rot2X, rot2Y, rot2Z, 2);
	game.setCamFov(interpolCam, fov2);

	game.setCamActiveWithInterp(from, interpolCam, duration, easePos, easeRot);
	alt.setTimeout(() => {
		game.renderScriptCams(true, false, 0, false, false, 0);
	}, 100);
}

alt.on('keydown', (key) => {
	if (!freeCamMode)
		return;

	switch (key) {
		case 87: //W Forward
			fowardPressed = true;
			break;
		case 83: //S 
			backwardPressed = true;
			break;
		case 65: //A
			leftPressed = true;
			break;
		case 68: //D
			rightPressed = true;
			break;
		case 81: //Q
			leftRollPressed = true;
			break;
		case 69: //E
			rightRollPressed = true;
			break;
		case 89: //Y Teleport Player to new Position and End Freecam
			endFreeModeCam(true);
			break;
		case 88: //X End FreeCam Jump back to Player
			endFreeModeCam(false);
			break;
		case 70: //F Change from RotationChange to SpeedChange mode
			rotScaleToogle = !rotScaleToogle;

			if (rotScaleToogle) {
				alt.log("Rotation-Scale-Mode");
			} else {
				alt.log("Position-Scale-Mode");
			}
			break;
		case 38: //Arrow Up Scale Up
			if (rotScaleToogle) {
				rotSpeed = rotSpeed + 0.01;
				alt.log("Rotation-Speed:" + rotSpeed);
			} else {
				speed = speed + 0.5;
				alt.log("Position-Speed:" + speed);
			}
			break;
		case 40: //Arrow down Scale Down
			if (rotScaleToogle) {
				rotSpeed = rotSpeed - 0.01;
				if (rotSpeed < 0) {
					rotSpeed = 0;
				}
				alt.log("Rotation-Speed:" + rotSpeed);
			} else {
				speed = speed - 0.5;
				if (speed < 0) {
					speed = 0;
				}
				alt.log("Position-Speed:" + speed);
			}
			break;
		//case 76: //L to Save Camera Position for Interpolate
			//var iCam = new Cam(camPos.x, camPos.y, camPos.z, offset.x, offset.y, offset.z, 10000);
			//interPolCams.push(iCam);
			//break;
		//case 79: //O Remove last Node
			//interPolCams.pop();
			//break;
	}
});

alt.on('keyup', (key) => {
	if(!freeCamMode)
	return;

	switch(key) {
		case 87: //W
			fowardPressed = false; 
			break;
			
		case 83: //S 
			backwardPressed = false;
			break;
		case 65: //A
			leftPressed = false; 
			break;
		case 68: //D
			rightPressed = false;
			break;
		case 81: //Q
			leftRollPressed = false; 
			break;
		case 69: //E
			rightRollPressed = false;
			break;
	}
});

//Fingerpointing

class Fingerpointing {
	constructor() {
		this.active = false;
		this.interval = null;
		this.cleanStart = false;
		this.debounceTime = 150;
		this.lastBlockDate = null;
		this.localPlayer = alt.Player.local;
		this.registerEvents();
	}

	registerEvents() {
		alt.on('keydown', (key) => {
			if (key !== 66) return;
			this.start();
		});

		alt.on('keyup', (key) => {
			if (key !== 66) return;
			this.stop();
		});
	}

	async start() {
		if (this.active) return;
		this.active = true;
		try {
			await this.requestAnimDictPromise('anim@mp_point');
			game.setPedCurrentWeaponVisible(
				this.localPlayer.scriptID,
				false,
				true,
				true,
				true
			);
			game.setPedConfigFlag(this.localPlayer.scriptID, 36, true);
			game.taskMoveNetworkByName(
				this.localPlayer.scriptID,
				'task_mp_pointing',
				0.5,
				false,
				'anim@mp_point',
				24
			);
			this.cleanStart = true;
			this.interval = alt.setInterval(this.process.bind(this), 0);
		} catch (e) {
			alt.log(e);
		}
	}

	stop() {
		if (!this.active) return;
		if (this.interval) {
			alt.clearInterval(this.interval);
		}
		this.interval = null;

		this.active = false;

		if (!this.cleanStart) return;
		this.cleanStart = false;
		game.requestTaskMoveNetworkStateTransition(
			this.localPlayer.scriptID,
			'Stop'
		);

		if (!game.isPedInjured(this.localPlayer.scriptID)) {
			game.clearPedSecondaryTask(this.localPlayer.scriptID);
		}
		if (!this.localPlayer.vehicle) {
			game.setPedCurrentWeaponVisible(
				this.localPlayer.scriptID,
				true,
				true,
				true,
				true
			);
		}
		game.setPedConfigFlag(this.localPlayer.scriptID, 36, false);
		game.clearPedSecondaryTask(this.localPlayer.scriptID);
	}

	getRelativePitch() {
		let camRot = game.getGameplayCamRot(2);
		return camRot.x - game.getEntityPitch(this.localPlayer.scriptID);
	}

	process() {
		if (!this.active) return;
		let camPitch = this.getRelativePitch();
		if (camPitch < -70.0) {
			camPitch = -70.0;
		} else if (camPitch > 42.0) {
			camPitch = 42.0;
		}
		camPitch = (camPitch + 70.0) / 112.0;

		let camHeading = game.getGameplayCamRelativeHeading();
		let cosCamHeading = Math.cos(camHeading);
		let sinCamHeading = Math.sin(camHeading);

		if (camHeading < -180.0) {
			camHeading = -180.0;
		} else if (camHeading > 180.0) {
			camHeading = 180.0;
		}
		camHeading = (camHeading + 180.0) / 360.0;

		let coords = game.getOffsetFromEntityInWorldCoords(
			this.localPlayer.scriptID,
			cosCamHeading * -0.2 - sinCamHeading * (0.4 * camHeading + 0.3),
			sinCamHeading * -0.2 + cosCamHeading * (0.4 * camHeading + 0.3),
			0.6
		);
		let ray = game.startShapeTestCapsule(
			coords.x,
			coords.y,
			coords.z - 0.2,
			coords.x,
			coords.y,
			coords.z + 0.2,
			1.0,
			95,
			this.localPlayer.scriptID,
			7
		);
		let [_, blocked, coords1, coords2, entity] = game.getShapeTestResult(
			ray,
			false,
			null,
			null,
			null
		);
		if (blocked && this.lastBlockDate === null) {
			this.lastBlockDate = new Date();
		}
		game.setTaskMoveNetworkSignalFloat(
			this.localPlayer.scriptID,
			'Pitch',
			camPitch
		);
		game.setTaskMoveNetworkSignalFloat(
			this.localPlayer.scriptID,
			'Heading',
			camHeading * -1.0 + 1.0
		);

		//this is a debounce for isBlocked network signal to avoid flickering of the peds arm on fast raycast changes
		if (this.isBlockingAllowed()) {
			game.setTaskMoveNetworkSignalBool(
				this.localPlayer.scriptID,
				'isBlocked',
				blocked
			);
		}

		game.setTaskMoveNetworkSignalBool(
			this.localPlayer.scriptID,
			'isFirstPerson',
			game.getCamViewModeForContext(game.getCamActiveViewModeContext()) === 4
		);
	}

	isBlockingAllowed() {
		const isAllowed = new Date() - this.lastBlockDate > this.debounceTime;
		if (isAllowed) {
			this.lastBlockDate = null;
		}
		return isAllowed;
	}

	requestAnimDictPromise(dict) {
		game.requestAnimDict(dict);
		return new Promise((resolve, reject) => {
			let tries = 0;
			let check = alt.setInterval(() => {
				tries++;
				if (game.hasAnimDictLoaded(dict)) {
					alt.clearInterval(check);
					resolve(true);
				} else if (tries > 30) {
					alt.clearInterval(check);
					reject('Anim request wait limit reached');
				}
			}, 50);
		});
	}
}

const FingerpointingInstance = new Fingerpointing();


//Notification
alt.onServer('notifications:show', show);
alt.onServer('notifications:showWithPicture', showWithPicture);

export function show(
	message,
	flashing = false,
	textColor = -1,
	bgColor = -1,
	flashColor = [0, 0, 0, 110]
) {
	game.beginTextCommandThefeedPost('STRING');

	if (textColor > -1) game.setColourOfNextTextComponent(textColor);
	if (bgColor > -1) game.thefeedSetBackgroundColorForNextPost(bgColor);
	if (flashing) {
		game.thefeedSetAnimpostfxColor(
			flashColor[0],
			flashColor[1],
			flashColor[2],
			flashColor[3]
		);
	}

	game.addTextComponentSubstringPlayerName(message);

	game.endTextCommandThefeedPostTicker(flashing, true);
}

export function showWithPicture(
	title,
	sender,
	message,
	notifPic,
	iconType = 0,
	flashing = false,
	textColor = -1,
	bgColor = -1,
	flashColor = [0, 0, 0, 50]
) {
	game.beginTextCommandThefeedPost('STRING');

	if (textColor > -1) game.setColourOfNextTextComponent(textColor);
	if (bgColor > -1) game.thefeedSetNextPostBackgroundColor(bgColor);
	if (flashing) {
		game.thefeedSetAnimpostfxColor(
			flashColor[0],
			flashColor[1],
			flashColor[2],
			flashColor[3]
		);
	}

	game.addTextComponentSubstringPlayerName(message);

	game.endTextCommandThefeedPostMessagetext(
		notifPic,
		notifPic,
		flashing,
		iconType,
		title,
		sender
	);

	game.endTextCommandThefeedPostTicker(flashing, true);
}

//Voice Range
alt.onServer("SHOW_VOICE_RANGE", (range, name) => {
	var rngStr = "";
	var pos = alt.Player.local.pos;
	createMarker(-1, 1, pos.x, pos.y, pos.z - 2, range * 2);
	
	alt.setTimeout(() => {
		deleteMarker(-1);
	}, 750);
	
	show("Sprachweite ist nun: " + name);
}); 

var markerList = [];
function createMarker(id, type, x, y, z, scale) {
    var marker = {id: id, type: type, x: x, y: y, z: z, scale: scale };
    markerList.push(marker);
}

function deleteMarker(id) {
    markerList = markerList.filter(marker => marker.id != id);
}

alt.everyTick(() => {
	for(var marker of markerList) {
		game.drawMarker(marker.type, marker.x, marker.y, marker.z, 0, 0, 0, 0, 0, 0, marker.scale, marker.scale, marker.scale, 17, 177, 165, 75, 0, 0, 1, 0, 0, 0, 0);
	}
});