import alt from 'alt'; 
import game from 'natives';

import * as playerEx from 'js/player.js';
import * as camera from 'js/camera.js';

import { setLock } from './client.js';
import { reactView } from './cef.js';
//CHARACTER CREATION

//FaceCamera:
//Pos -1127.39343 -2818.18018 40.4
//Rot 0 0 152

//Clothes:
//Pos -1126.734 -2817.112 40.5
//Rot 15 0 150

//Player:
//-1127.68347 -2818.866 39.70984

let baseAngle = 5;
let charCreatorTick = null;
alt.onServer('START_CHAR_CREATE', (style, otherStyle, hairStyleListJSON, hairOverlayJSON) => {
	if (charCreatorTick === null) {
		charCreatorTick = alt.everyTick(() => {
			game.invalidateIdleCam();
			game.clearPedTasksImmediately(alt.Player.local.scriptID);

			if (reactView != null) {
				reactView.unfocus();
			}
		});
	}

	game.setEntityCoords(game.playerPedId(), 38.835, -15.44, 1 - 1, 1, 0, 0, 1);
	game.setEntityRotation(game.playerPedId(), 0, 0, baseAngle, 1, true);

	camera.createCam(38.8555030034657, -15.943551864630663, 1.6874919809679192, 1.3228345960378647, 0, -0.18897637724876404);

	alt.toggleGameControls(false);

	//Refresh Interior at pos:
	alt.setTimeout(() => {
		camera.createCam(38.8555030034657, -15.943551864630663, 1.6874919809679192, 1.3228345960378647, 0, -0.18897637724876404);
	}, 2000);

	setLock(true);
	toggleCharCreator(style, otherStyle, hairStyleListJSON, hairOverlayJSON);
});

var female = false;
var hairStyleListM = [];
var hairStyleListF = [];
var hairOverlays = [];

function toggleCharCreator(style, otherStyle, hairStyleListJSON, hairOverlayJSON) {
	JSON.parse(hairStyleListJSON).forEach((el) => {
		if(el.isFemale) {
			hairStyleListF.push(el.gtaId);
		} else {
			hairStyleListM.push(el.gtaId);
		}
	});

	hairOverlays = JSON.parse(hairOverlayJSON);

	if(style != null) {
		var styleObj = JSON.parse(style);
		var temp = style;

		if(otherStyle != null) {
			var otherStyleObj = JSON.parse(otherStyle);
			if(overlayStr != null) {
				var overlayStr = otherStyleObj.overlayStr.split("#");
				var idx = -1;
				for(var i = 0; i < hairOverlays.length; i++) {
					if(hairOverlays[i].collection === overlayStr[1] && hairOverlays[i].hash === overlayStr[2]) {
						idx = i;
						break;
					}
				}
				otherStyleObj.overlayIdx = idx + 1;
				if(idx != -1) {
					styleObj["hairOverlay"] = idx;
				}
			} else {
				otherStyleObj.overlayIdx = 0;
			}
			otherStyle = JSON.stringify(otherStyleObj);
		}

		alt.setTimeout(() => {
			setPedCharacter(temp, alt.Player.local.scriptID);
		}, 1000);


		if(styleObj["gender"] == "F") {	
			alt.emitServer("SET_MODEL", 'mp_f_freemode_01');
			alt.emitServer("SET_NAKED", "F");
			styleObj["hairStyle"] = hairStyleListF.indexOf(styleObj["hairStyle"]);
			female = true;
		} else {
			alt.emitServer("SET_MODEL", 'mp_m_freemode_01');
			alt.emitServer("SET_NAKED", "M");
			styleObj["hairStyle"] = hairStyleListM.indexOf(styleObj["hairStyle"]);
			female = false;
		}

		style = JSON.stringify(styleObj);
	} else {
		alt.emitServer("SET_MODEL", 'mp_m_freemode_01');
		alt.emitServer("SET_NAKED", "M");
	}

	var webView = new alt.WebView('http://resource/cef/NEEDED/charcreator/charCreator.html');
	webView.focus();
	
	webView.emit("SET_DATA", style, otherStyle, hairStyleListF.length, hairStyleListM.length, hairOverlays.length);
	alt.showCursor(true);

	webView.on('UpdatePedData', (dataIn) => {
		var data = JSON.parse(dataIn);
		if(female) {
			data["hairStyle"] = hairStyleListF[data["hairStyle"]];
		} else {
			data["hairStyle"] = hairStyleListM[data["hairStyle"]];
		}
		setPedCharacter(JSON.stringify(data), alt.Player.local.scriptID);
	});

	webView.on('Rotate', (dataIn) => {
		game.setEntityRotation(game.playerPedId(), 0, 0, baseAngle + parseFloat(dataIn), 1, true);
		game.clearPedTasksImmediately(game.playerPedId());
	});

	webView.on('SetGender', (dataIn) => {
		if (dataIn === "female") {
			female = true;
			alt.emitServer("SET_MODEL", 'mp_f_freemode_01');
			alt.emitServer("SET_NAKED", "F");
		} else {
			female = false;
			alt.emitServer("SET_MODEL", 'mp_m_freemode_01');
			alt.emitServer("SET_NAKED", "M");
		}
		
		//Refresh Interior at pos:
		alt.setTimeout(() => {
			var int = game.getInteriorAtCoords(0, 0, 0);
			game.refreshInterior(int);
		}, 200);
	});

	webView.on('SetCamera', (dataIn) => {
		if (dataIn === "face") {
			//Gesichtskamera
			camera.createCam(38.8555030034657, -15.943551864630663, 1.6874919809679192, 1.3228345960378647, 0, -0.18897637724876404);
		} else {
			//Ganzkörperkamera
			camera.createCam(38.8555030034657, -16.993990373468073, 1.1529229640661731, -4.913386255502701, 0, 1.7952755838632584);
		}
	});

	webView.on('FinishPedCreation', (dataFace) => {
		var data = JSON.parse(dataFace);
		if(female) {
			data["hairStyle"] = hairStyleListF[data["hairStyle"]];
		} else {
			data["hairStyle"] = hairStyleListM[data["hairStyle"]];
		}

		if(data["hairOverlay"] > 0) {
			var overlay = hairOverlays[parseInt(data["hairOverlay"]) + 1];
			data["hairOverlayCollection"] = overlay.collection;
			data["hairOverlayHash"] = overlay.hash;
		} else {
			data["hairOverlayCollection"] = "";
			data["hairOverlayHash"] = "";
		}
		game.doScreenFadeOut(1000);
		alt.setTimeout(() => {
			alt.emitServer("FINISH_PED_CREATION", JSON.stringify(data), {}, female ? "F" : "M", game.scAccountInfoGetNickname());
			camera.destroyCam();
			webView.destroy();
			game.taskClearLookAt(alt.Player.local.scriptID);
			alt.toggleGameControls(true);
			if(charCreatorTick !== null) {
				alt.clearEveryTick(charCreatorTick);
				charCreatorTick = null;
			}
			alt.showCursor(false);
			setLock(false);
		}, 1500);
	});

	webView.on('CancelPedCreation', () => {
		alt.emitServer("CANCEL_PED_CREATION");

		game.doScreenFadeOut(1000);
		alt.setTimeout(() => {
			camera.destroyCam();
			webView.destroy();
			game.taskClearLookAt(alt.Player.local.scriptID);
			alt.toggleGameControls(true);
			if(charCreatorTick !== null) {
				alt.clearEveryTick(charCreatorTick);
				charCreatorTick = null;
			}
			alt.showCursor(false);
			setLock(false);
		}, 1500);
	});

	webView.on('SetMask', (value) => {
		game.setPedComponentVariation(alt.Player.local, 1, parseInt(value), 0, 2);
	});
}

alt.onServer('SET_CHARACTER_STYLE', (style) => {
	setPedCharacter(style, alt.Player.local.scriptID);
});

export function setPedCharacter(style, ped) {
	var data = JSON.parse(style);

	game.setPedHeadBlendData(ped, parseInt(data['faceFather']), parseInt(data['faceMother']), 0, parseInt(data['faceFather']), parseInt(data['faceMother']), 0, parseFloat(data['faceShape']), parseFloat(data['faceSkin']), 0, false);

	for (let l = 0; l < 13; l++) {
		if(l == 4 || l == 8) {
			game.setPedHeadOverlay(ped, l, parseInt(data['overlay_' + l]), parseFloat(data["overlay_" + l + "_opacity"]));
		} else {
			game.setPedHeadOverlay(ped, l, parseInt(data['overlay_' + l]), 1.0);
		}
	}

	game.setPedHeadOverlayTint(ped, 1, 1, parseInt(data['overlaycolor_1']), parseInt(data['overlayhighlight_1']));
	game.setPedHeadOverlayTint(ped, 2, 1, parseInt(data['overlaycolor_2']), parseInt(data['overlayhighlight_2']));
	game.setPedHeadOverlayTint(ped, 4, 2, parseInt(data['overlaycolor_4']), parseInt(data['overlayhighlight_4']));
	game.setPedHeadOverlayTint(ped, 5, 2, parseInt(data['overlaycolor_5']), parseInt(data['overlayhighlight_5']));
	game.setPedHeadOverlayTint(ped, 8, 2, parseInt(data['overlaycolor_8']), parseInt(data['overlayhighlight_8']));
	game.setPedHeadOverlayTint(ped, 10, 1, parseInt(data['overlaycolor_10']), parseInt(data['overlayhighlight_10']));

	let hairStyle = parseInt(data['hairStyle']);
	game.setPedComponentVariation(ped, 2, hairStyle, 0, 0);

	game.setPedHairTint(ped, parseInt(data['hairColor']), parseInt(data['hairHighlight']));
	game.setHeadBlendEyeColor(ped, parseInt(data['faceEyes']));

	for (var i = 0; i < 19; i++) {
		game.setPedMicroMorph(ped, i, parseFloat(data['faceFeature_' + i]));
	}

	if(data["hairOverlay"] != undefined) {
		game.clearPedDecorations(ped);
		if(data["hairOverlay"] > 0) {
			var el = hairOverlays[parseInt(data["hairOverlay"]) + 1];
			if(el != undefined) {
				game.addPedDecorationFromHashesInCorona(ped, alt.hash(el.collection), alt.hash(el.hash));
			}
		}
	}
}