import alt from 'alt'; 
import game from 'natives';

import * as math from 'js/math.js';

//CAMERA!

alt.everyTick(() => {
	if(game.isCinematicCamInputActive()) {
		game.setCinematicModeActive(false);
	}
});

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
	}, 10);
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

alt.onServer('DESTROY_CAM', () => {
	destroyCam();
});



//FreeCam Mode
var freeCamMode = false;
var speed = -3;
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

//var disabledControls = [178, 21, 15, 14, 15, 32, 33, 34, 35, 22, 36, 44, 38];

alt.onServer('FREE_CAM_MODE', () => {	
	var player = alt.Player.local;
	var pos = game.getEntityCoords(player.scriptID, true);
	var rot = game.getGameplayCamRot(2);
	camPos = new math.Vector3(pos.x, pos.y, pos.z);
	offset = new math.Vector3(0, 0, 0);
	
	createCam(camPos.x, camPos.y, camPos.z, rot.x, rot.y, rot.z);
	freeCamMode = true;

	game.freezeEntityPosition(alt.Player.local.scriptID, true);
});

alt.everyTick(() => {
	if(!freeCamMode) {
		return;
	}

	drawInterpolCam();

	if(fowardPressed) { //W
		var multX = Math.sin(offset.z * Math.PI / 180);
		var multY = Math.cos(offset.z * Math.PI / 180);
		var multZ = Math.sin(offset.x * Math.PI / 180);

		camPos.x = camPos.x - (0.1 * getFuc(speed) * multX);
		camPos.y = camPos.y + (0.1 * getFuc(speed) * multY);
		camPos.z = camPos.z + (0.1 * getFuc(speed) * multZ);

		//camPos.y = camPos.y + speed;
	}

	if(backwardPressed) { //S
		var multX = Math.sin(offset.z * Math.PI / 180);
		var multY = Math.cos(offset.z * Math.PI / 180);
		var multZ = Math.sin(offset.x * Math.PI / 180);

		camPos.x = camPos.x + (0.1 * getFuc(speed) * multX);
		camPos.y = camPos.y - (0.1 * getFuc(speed) * multY);
		camPos.z = camPos.z - (0.1 * getFuc(speed) * multZ);

		//camPos.y = camPos.y - speed;
	}

	if(leftPressed) { //A
		var multX = Math.sin((offset.z + 90) * Math.PI / 180);
		var multY = Math.cos((offset.z + 90) * Math.PI / 180);
		var multZ = Math.sin(offset.y * Math.PI / 180);

		camPos.x = camPos.x - (0.1 * getFuc(speed) * multX);
		camPos.y = camPos.y + (0.1 * getFuc(speed) * multY);
		camPos.z = camPos.z + (0.1 * getFuc(speed) * multZ);

		//camPos.x = camPos.x - speed;
	}

	if(rightPressed) { //D
		var multX = Math.sin((offset.z + 90) * Math.PI / 180);
		var multY = Math.cos((offset.z + 90) * Math.PI / 180);
		var multZ = Math.sin(offset.y) * Math.PI / 180;

		camPos.x = camPos.x + (0.1 * getFuc(speed) * multX);
		camPos.y = camPos.y - (0.1 * getFuc(speed) * multY);
		camPos.z = camPos.z - (0.1 * getFuc(speed) * multZ);

		//camPos.x = camPos.x + speed;
	}

	if(leftRollPressed) {
		offset.y = offset.y - rotSpeed;
	}

	if(rightRollPressed) {
		offset.y = offset.y + rotSpeed;
	}

	offset.x = offset.x - (game.getControlNormal(1, 2) * rotSpeed * 8.0);
	offset.z = offset.z - (game.getControlNormal(1, 1) * rotSpeed * 8.0);

	changeOffsetIfNeeded();

	game.setHdArea(camPos.x, camPos.y, camPos.z, 100);
	game.setFocusPosAndVel(camPos.x, camPos.y, camPos.z, 0.0, 0.0, 0.0);

	if(camera != null) {
		game.setCamCoord(camera, camPos.x, camPos.y, camPos.z);
		game.setCamRot(camera, offset.x, offset.y, offset.z, 2);
	}
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
	speed = -3;
	rotSpeed = 0.6;


	if(teleportToPos) {
		game.setEntityCoords(alt.Player.local.scriptID, camPos.x, camPos.y, camPos.z, true, false, false, true);
	}

	game.freezeEntityPosition(alt.Player.local.scriptID, false);

}


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

function getFuc(x) {
    return Math.pow(Math.E, -x-1);
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
				rotSpeed = rotSpeed - 0.01;
				alt.log("Rotation-Speed:" + rotSpeed);
			} else {
				speed = speed - 0.3;
				alt.log("Position-Speed:" + getFuc(speed));
			}
			break;
		case 40: //Arrow down Scale Down
			if (rotScaleToogle) {
				rotSpeed = rotSpeed + 0.01;
				if (rotSpeed < 0) {
					rotSpeed = 0;
				}
				alt.log("Rotation-Speed:" + rotSpeed);
			} else {
				speed = speed + 0.3;
				alt.log("Position-Speed:" + getFuc(speed));
			}
			break;
		case 76: //L to Save Camera Position for Interpolate
			var iCam = new Cam(camPos.x, camPos.y, camPos.z, offset.x, offset.y, offset.z, 10000);
			interPolCams.push(iCam);
			break;
		case 79: //O Remove last Node
			interPolCams.pop();
			break;
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

alt.on('consoleCommand', (cmd, duration, easePos, easeRot) => {
    if(!freeCamMode)
		return;
	
	switch(cmd) {
		case "dur": 
			interPolCams[interPolCams.length-1].duration = parseInt(args[1], 10); 
			alt.log("Duration of last Node to Next has been set to " + args[1]);
			break;
		case "startCam": 
			camDrive(parseInt(duration), parseInt(easePos), parseInt(easeRot));
			break;
		case "showData": 
			alt.log("Position: " + JSON.stringify(camPos));
			alt.log("Rotation: " + JSON.stringify(offset));
			break;
	}
});

//Binoculars

var binFov = 60;
var binMinFov = 10;
var binMaxFov = 60;
var binOffset = [0, 0, 0];
var startHeading = 0;
var binRotSpeed = 0.3;
var scaleform;

var binTick = null;
var binNightVisionPos = false;
var binThermalVisionPos = false;
var binNightVisionToggle = false;
var binThermalVisionToggle = false;

alt.onServer("BINOCULARS_TOGGLE", (toggle, night, thermal) => {
	if(toggle) {
		startBinoculars(night, thermal);
	} else {
		stopBinoculars();
	}
});

function startBinoculars(night, thermal) {
	scaleform = game.requestScaleformMovie("BINOCULARS");

	stopBinoculars();

	alt.setTimeout(() => {
		binTick = alt.everyTick(() => {
			game.drawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);

			hideHud();

			if (game.isControlJustPressed(0, 241)) {
				binFov = Math.max(binFov - 7.5, binMinFov);
			} else if (game.isControlJustPressed(0, 242)) {
				binFov = Math.min(binFov + 7.5, binMaxFov);
			}
			game.setCamFov(camera, binFov);

			binOffset[0] = binOffset[0] - (game.getControlNormal(1, 2) * binRotSpeed * 8.0);
			binOffset[2] = binOffset[2] - (game.getControlNormal(1, 1) * binRotSpeed * 8.0);

			changeBinOffsetIfNeeded();

			game.setCamRot(camera, binOffset[0], binOffset[1], binOffset[2], 2);
		});

		binFov = binMaxFov;
		binNightVisionPos = night;
		binThermalVisionPos = thermal;
		createBinCam();
		
		game.beginScaleformMovieMethod(scaleform, "SET_CAM_LOGO");
		game.scaleformMovieMethodAddParamInt(0);
		game.endScaleformMovieMethod();
	}, 1000);
}

function stopBinoculars() {
	destroyCam();
	alt.toggleGameControls(true);
	game.setSeethrough(0);
	game.setNightvision(0);
	binNightVisionToggle = false;
	binThermalVisionToggle = false;
	
	if(binTick !== null) {
		alt.clearEveryTick(binTick);
		binTick = null;
	}
}

alt.on('keyup', (key) => {
	if(binTick === null) {
		return;
	}

	switch(key) {
		case 89: //Y
		if(binNightVisionPos) {
			binNightVisionToggle = !binNightVisionToggle;
			game.setNightvision(binNightVisionToggle);
		} 
		break;
		case 88: //X
		if(binThermalVisionPos) {
			binThermalVisionToggle = !binThermalVisionToggle;
			game.setSeethrough(binThermalVisionToggle);
		} 
		break;
	}
});

function createBinCam() {
    camera = game.createCam("DEFAULT_SCRIPTED_FLY_CAMERA", true);
    game.attachCamToEntity(camera, alt.Player.local.scriptID, 0.0, 0.0, 1.0, true);
	game.setCamRot(camera, 0, 0, game.getEntityHeading(alt.Player.local.scriptID), 2);
	binOffset[2] = game.getEntityHeading(alt.Player.local.scriptID);
	startHeading = game.getEntityHeading(alt.Player.local.scriptID);
    game.setCamActive(camera, true);
    game.setCamFov(camera, 70.0);
	game.renderScriptCams(true, false, 0, 1, 0, 0);
}

export function hideHud() {
	game.hideHelpTextThisFrame()
	game.hideHudAndRadarThisFrame()
	game.hideHudComponentThisFrame(1) // Wanted Stars
	game.hideHudComponentThisFrame(2) // Weapon icon
	game.hideHudComponentThisFrame(3) // Cash
	game.hideHudComponentThisFrame(4) // MP CASH
	game.hideHudComponentThisFrame(6)
	game.hideHudComponentThisFrame(7)
	game.hideHudComponentThisFrame(8)
	game.hideHudComponentThisFrame(9)
	game.hideHudComponentThisFrame(13) // Cash Change
	game.hideHudComponentThisFrame(11) // Floating Help Text
	game.hideHudComponentThisFrame(12) // more floating help text
	game.hideHudComponentThisFrame(15) // Subtitle Text
	game.hideHudComponentThisFrame(18) // Game Stream
	game.hideHudComponentThisFrame(19) // weapon wheel
}


function changeBinOffsetIfNeeded() {
	if (binOffset[0] > 30.0) {
		binOffset[0] = 30.0;
	} else if (binOffset[0] < -30.0)  {
		binOffset[0] = -30.0;
	}
}