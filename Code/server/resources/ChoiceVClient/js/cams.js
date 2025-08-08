/// <reference path="../types/alt.d.ts" />
/// <reference path="../types/altv.d.ts" />

import * as alt from 'alt-client'
import * as game from 'natives'; 

import { currentGrids, log } from '/js/client.js';
import { rotatePointInRect, degreesToRadians } from '/js/math.js';

import * as camsList from '/js/camsList.js';

var configCams = [
	{ model: "prop_cs_cctv", headings: [270], distance: 30, zOffset: -0.1, xyOffset: null},
	{ model: "p_cctv_s", headings: [270], distance: 30, zOffset: -0.1, xyOffset: null},
	{ model: "prop_cctv_cam_01a", headings: [300], distance: 30, zOffset: 0.2, xyOffset: {x: -0.6, y: 0.1}},
	{ model: "prop_cctv_cam_01b", headings: [240], distance: 30, zOffset: 0.2, xyOffset:  {x: -0.6, y: -0.1}},
	{ model: "prop_cctv_cam_02a", headings: [300], distance: 30, zOffset: 0, xyOffset: null},
	{ model: "prop_cctv_cam_03a", headings: [225], distance: 30, zOffset: 0.3, xyOffset: {x: -0.3, y: -0.3}},
	{ model: "prop_cctv_cam_04a", headings: null, distance: 30, zOffset: 0, xyOffset: null},
	{ model: "prop_cctv_cam_04b", headings: null, distance: 30, zOffset: 0.8, xyOffset: {x: -0.6, y: 0}},
	{ model: "prop_cctv_cam_04c", headings: null, distance: 30, zOffset: 0, xyOffset: null},
	{ model: "prop_cctv_cam_05a", headings: [270], distance: 30, zOffset: -0.2, xyOffset: null},
	{ model: "prop_cctv_cam_06a", headings: [265], distance: 30, zOffset: -0.2, xyOffset: null},
	{ model: "prop_cctv_cam_07a", headings: null, distance: 30, zOffset: 0, xyOffset: null},
	{ model: "prop_cctv_pole_01a", headings: [200], distance: 60, zOffset: 7, xyOffset: null},
	{ model: "prop_cctv_pole_02", headings: null, distance: 60, zOffset: 6, xyOffset: null},
	{ model: "prop_cctv_pole_03", headings: [30, 210], distance: 60, zOffset: 6, xyOffset: null},
	{ model: "prop_cctv_pole_04", headings: [210], distance: 60, zOffset: 4.5, xyOffset: {x: 0, y: -0.4}},
	
	{ model: "ba_prop_battle_cctv_cam_01a", headings: [0], distance: 10, zOffset: 0.25, xyOffset: {x: -0.4, y: 0}},
	{ model: "ba_prop_battle_cctv_cam_01b", headings: [175], distance: 30, zOffset: 0.25, xyOffset: {x: -0.4, y: -0.3}},
	
	{ model: "xm_prop_x17_cctv_01a", headings: [270], distance: 40, zOffset: 0, xyOffset: null},
	{ model: "xm_prop_x17_server_farm_cctv_01", headings: [270], distance: 20, zOffset: -0.2, xyOffset: null},
	{ model: "hei_prop_bank_cctv_01", headings: [270], distance: 20, zOffset: -0.2, xyOffset: null},
	{ model: "hei_prop_bank_cctv_02", headings: null, distance: 20, zOffset: -0.1, xyOffset: null},
	
	{ model: "ch_prop_ch_cctv_cam_01a", headings: [270], distance: 20, zOffset: 0.1, xyOffset: null},
	{ model: "ch_prop_ch_cctv_cam_02a", headings: [270], distance: 20, zOffset: 0.1, xyOffset: null},
	{ model: "tr_prop_tr_cctv_cam_01a", headings: [270], distance: 20, zOffset: 0.1, xyOffset: null},
]

var camList = []; 
var camMap = new Map();

alt.on('connectionComplete', () => {
    for(var i = 0; i < camsList.camIds.length; i++) {
        var c = new Cam(camsList.camIds[i], camsList.camGrids[i], camsList.camModels[i], {x: camsList.camPosXs[i], y: camsList.camPosYs[i], z: camsList.camPosZs[i]}, camsList.camHeadings[i])
        camList.push(c);
        
        if(camMap.has(c.gridId)) {
            var list = camMap.get(c.gridId);
            list.push(c);
        } else {
            camMap.set(c.gridId, [c]);
        }
    }
});

class Cam {
    constructor(id, gridId, model, pos, heading) {
        this.id = id;
        this.gridId = gridId;
        this.model = model;
        this.pos = pos;
        this.heading = heading;

        this.configCam = configCams.find((el) => { return el.model == model});

        this.localPlayerIsVisible = false;
        this.currentlyInExitCheck = false;
    }

    checkIfVisible(pos) {
        var seeing = false;
        var dist = getDistance(pos.x, pos.y, 0, this.pos.x, this.pos.y, 0)
		if(dist < this.configCam.distance && dist > Math.abs(this.pos.z - pos.z)) {
			if(this.configCam.headings != null) {
				var objHeading = this.heading;
				this.configCam.headings.forEach((heading) => {	
					var camVec = {	x: Math.cos(degrees_to_radians(heading + objHeading)),
									y: Math.sin(degrees_to_radians(heading + objHeading)),
									z: 0 }
									
					var camToPlayerVec = {	x: pos.x - this.pos.x,
											y: pos.y - this.pos.y,
											z: 0 }
				
					var angle = (Math.acos(dot(camVec, camToPlayerVec) / (mag(camVec) * mag(camToPlayerVec))) * 180 / Math.PI);
					
					if(Math.abs(angle) < 35) {
						seeing = true;
					} 
				});
			} else {
                if(this.pos.z > pos.z) {
                    seeing = true;
                }
			}
		}
		
		if(seeing) {
            var offset = { x: 0, y: 0 };
            if(this.configCam.xyOffset != null) {
                offset = rotatePointInRect(this.configCam.xyOffset.y, this.configCam.xyOffset.x, 0, 0, degreesToRadians(this.heading));
            }

            //var probe = game.startExpensiveSynchronousShapeTestLosProbe(pos.x, pos.y, pos.z + 0.5, this.pos.x + offset.x, this.pos.y + offset.y, this.pos.z + this.configCam.zOffset, 19, alt.Player.local.scriptID, 7);
            var probe = game.startShapeTestCapsule(pos.x, pos.y, pos.z + 0.5, this.pos.x + offset.x, this.pos.y + offset.y, this.pos.z + this.configCam.zOffset, 0, 19, alt.Player.local.scriptID, 4);
            
			var result = game.getShapeTestResult(probe);
			var hitPos = result[2];
           
			var selectedEntity = result[4];
            if(!game.doesEntityHaveDrawable(selectedEntity) || !game.doesEntityExist(selectedEntity)) {
                return false;
            }
            
			var modelHash = "";
			try {	
				modelHash = game.getEntityModel(selectedEntity);
			} catch(e) {
                log("Failed with type: " + game.getEntityType(selectedEntity));
			}
			
			if(modelHash == alt.hash(this.configCam.model) || getDistance(this.pos.x, this.pos.y, this.pos.z + this.configCam.zOffset, hitPos.x, hitPos.y, hitPos.z) < 0.1) {			
				return true;
			} else {	
				return false;
			}
		} else {
			return false;
		}
    }

    //Has to be called everyTick
    debugShow() {
        var offset = { x: 0, y: 0 };
        if(this.configCam.xyOffset != null) {
            offset = rotatePointInRect(this.configCam.xyOffset.y, this.configCam.xyOffset.x, 0, 0, degreesToRadians(this.heading));
        }

		if(this.configCam.headings != null) {
			var objHeading = this.heading;
			this.configCam.headings.forEach((heading) => {	
				game.drawLine(this.pos.x + offset.x, this.pos.y + offset.y, this.pos.z + this.configCam.zOffset, this.pos.x + offset.x +  Math.cos(degrees_to_radians(heading + objHeading)) * this.configCam.distance, this.pos.y + offset.y + Math.sin(degrees_to_radians(heading + objHeading)) * this.configCam.distance, this.pos.z + this.configCam.zOffset, 78, 156, 211, 255);
			});
		}

		game.drawLine(this.pos.x + offset.x, this.pos.y + offset.y, -5000, this.pos.x + offset.x, this.pos.y + offset.y, 5000, 123, 10, 52, 255);

        if(this.localPlayerIsVisible) {
            var pos = alt.Player.local.pos;
            game.drawLine(this.pos.x + offset.x, this.pos.y + offset.y, this.pos.z + this.configCam.zOffset, pos.x, pos.y, pos.z, 65, 255, 12, 255);
        }
    }

    showPosition() {
        var offset = { x: 0, y: 0, z: this.configCam.zOffset };
        if(this.configCam.xyOffset != null) {
            offset = rotatePointInRect(this.configCam.xyOffset.y, this.configCam.xyOffset.x, 0, 0, degreesToRadians(this.heading));
        }

        game.drawBox(this.pos.x + offset.x - 0.15, this.pos.y + offset.y - 0.15, this.pos.z + this.configCam.zOffset - 0.15, this.pos.x + offset.x + 0.15, this.pos.y + offset.y + 0.15, this.pos.z + this.configCam.zOffset + 0.15, 123, 10, 52, 255);    
		game.drawLine(this.pos.x + offset.x, this.pos.y + offset.y, -5000, this.pos.x + offset.x, this.pos.y + offset.y, 5000, 123, 10, 52, 255);
    }
}

var debugInterval = null;
var debugTick = null;

var found = [];
alt.onServer("TOGGLE_CAM_FIND_MODE", () => {
    if(debugInterval == null) {
        debugInterval = alt.setInterval(() => {
            var pos = alt.Player.local.pos;
            
            configCams.forEach((cam) => {
                var camModel = cam.model;
                var obj = game.getClosestObjectOfType(pos.x, pos.y, pos.z, 20.0, alt.hash(camModel), false, true, true);
                if(obj == 0) {
                    var obj = game.getClosestObjectOfType(pos.x, pos.y, pos.z, 100.0, alt.hash(camModel), false, true, true);
                    if(obj == 0) {
                        var obj = game.getClosestObjectOfType(pos.x, pos.y, pos.z, 1000.0, alt.hash(camModel), false, true, true);
                    }
                }

                if(obj != 0) {
                    var coords = game.getEntityCoords(obj, false);
                    var already = camList.concat(found).filter((el) => {
                        return el.model == camModel && getDistance(el.pos.x, el.pos.y, el.pos.z, coords.x, coords.y, coords.z) < 1;
                    })[0];

                    if(already == undefined && !game.hasObjectBeenBroken(obj, 0)) {
                        var heading = game.getEntityHeading(obj);
                        found.push({ model: camModel, pos: coords, heading: heading });
                        log("Found " + camModel + ", at: " + JSON.stringify(coords));

                        alt.emitServer("FOUND_NEW_CAM", camModel, coords, heading);
                    }
                }
            });  
        }, 750);

        debugTick = alt.everyTick(() => {
            currentGrids.forEach((el) => {
                if(camMap.has(el)) {
                    var cams = camMap.get(el);

                    cams.forEach((cam) => {
                        cam.debugShow();
                    });
                }   
            });
        });
    } else {
        alt.clearInterval(debugInterval);
        debugInterval = null;
        alt.clearEveryTick(debugTick);
        debugTick = null;
    }
});

//Constant check
var safePos = null;
alt.setInterval(() => {
    currentGrids.forEach((el) => {
        if(camMap.has(el)) {
            var cams = camMap.get(el);

            if(cams != undefined) {
                cams.forEach((cam) => {
                    if(!(cam instanceof Cam)) { 
                        log(typeof cam);
                    }
                    if(cam.checkIfVisible(alt.Player.local.pos)) {
                        if(!cam.localPlayerIsVisible) {
                            cam.localPlayerIsVisible = true;
                            if(!cam.currentlyInExitCheck) {
                                alt.emitServer("CAM_ENTER_RANGE", cam.id, cam.pos.x, cam.pos.y, cam.pos.z);
                            }
                        }
                    } else {
                        if(cam.localPlayerIsVisible) {
                            if(!cam.currentlyInExitCheck) {
                                safePos = alt.Player.local.pos;
                                cam.currentlyInExitCheck = true;
                                alt.setTimeout(() => {
                                    if(!cam.localPlayerIsVisible) {
                                        alt.emitServer("CAM_EXIT_RANGE", cam.id, cam.pos.x, cam.pos.y, cam.pos.z, safePos.x, safePos.y, safePos.z);
                                    }
                                    cam.currentlyInExitCheck = false;
                                }, 2000);
                            }

                            cam.localPlayerIsVisible = false;
                        }
                    }
                });
            }
        }
    });
}, 750);

alt.onServer("ADD_CAM", (id, gridId, model, pos, heading) => {
    var c = new Cam(id, gridId, model, pos, heading);
    camList.push(c);
    
    if(camMap.has(c.gridId)) {
        var list = camMap.get(c.gridId);
        list.push(c);
    } else {
        camMap.set(c.gridId, [c]);
    }
});

alt.onServer("REQUEST_IF_CAM_VISIBLE", (requestId) => {
    var list = [];
    currentGrids.forEach((el) => {
        if(camMap.has(el)) {
            var cams = camMap.get(el);

            cams.forEach((cam) => {
            if(cam.localPlayerIsVisible) {
                list.push({id: cam.id, posX: cam.pos.x, posY: cam.pos.y, posZ: cam.pos.z});
            }
            });
        }
    });

    alt.emitServer("ANSWER_IF_CAM_VISIBLE", requestId, JSON.stringify(list));
});

alt.onServer("CAM_SHOW_FOR_FIVE_SECONDS", (gridId, id) => {
    var cam = camMap.get(gridId).find((el) => { return el.id == id });

    var tick = alt.everyTick(() => {
        cam.showPosition();
    });

    alt.setTimeout(() => {
        alt.clearEveryTick(tick);
    }, 3000);
});

//Math Functions
function getDistance(x1, y1, z1, x2, y2, z2) {
    var a = x1 - x2;
    var b = y1 - y2;
	var c = z1 - z2;

    return Math.sqrt(a*a + b*b + c*c); 
}

function degrees_to_radians(degrees) {
  var pi = Math.PI;
  return degrees * (pi/180);
}

const dot = (p1, p2) => p1.x * p2.x + p1.y * p2.y + p1.z * p2.z;
const mag = (p) => Math.hypot(p.x, p.y, p.z);