import alt from 'alt'; 
import game from 'natives';

import { setLock } from './client.js';
import { opened } from './chat.js';
import { DISCORD_APP_ID } from './connection.js';


export var otherCefOpened = false;
export var editorOpen = false;
export var cursorShown = false;

export function showCursor() {
    if(!alt.isCursorVisible()) {
        alt.showCursor(true);   
    }
    cursorShown = true;
}

export function hideCursor() {
    if(alt.isCursorVisible()) {
        alt.showCursor(false);  
    } 
    cursorShown = false;
}

//http://cef-choicev.net/cef/
export var reactView = null;
var viewOpening = false;
alt.onServer('INIT_WEBSOCKET', (id, url, cefUrl, timeout, loginToken) => { 
	initCef(id, url, cefUrl, timeout, loginToken);
});

function initCef(id, url, cefUrl, timeout, loginToken) {
    if(viewOpening) {
        alt.setTimeout(() => {
            viewOpening = false;
        }, 10000);
        return;
    }

    viewOpening = true;
    if(reactView != null) {
        reactView.emit('INIT_WEBSOCKET', id, url, loginToken);
        reactView.focus();
        return;
    }
    
    alt.setTimeout(() => {
        reactView = new alt.WebView(cefUrl);
        reactView.on("HAS_LOADED", () => {
            if(reactView != null) {
                reactView.emit('INIT_WEBSOCKET', id, url, loginToken);
                reactView.focus();
            } else {
                initCef(id, url, cefUrl, timeout);
            }
        });

        reactView.on("REQUEST_DISCORD_TOKEN", async (callbackId) => {
            var token = await alt.Discord.requestOAuth2Token(DISCORD_APP_ID);

            var data = {
                Event: "ANSWER_DISCORD_TOKEN",
                callbackId: callbackId,
                token: token,
            }

            reactView.emit('CEF_EVENT', data);
        });
    }, 1000);
}

alt.on('disconnect', () => {
	if(reactView != null) {
		reactView.destroy();
		reactView = null;
	}
});

var otherCef = null;
alt.onServer('OTHER_CEF', (url, eventName, eventData) => { 
    onOpenOtherCef(url, eventName, eventData);
});

var savedUrl = null;
async function onOpenOtherCef(url, eventName, eventData) {
    savedUrl = url;

    var token = await alt.Discord.requestOAuth2Token(DISCORD_APP_ID);
    url = url + "&discordToken=" + token;
    
    if(otherCef == null) { 
        otherCef = new alt.WebView(url);
    } else {
        otherCef.isVisible = true;
    }

    otherCefOpened = true;

    alt.setTimeout(() => {
        otherCef.focus();
        if(eventName != undefined && eventName != null) {
            otherCef.emit(eventName, eventData);
        }

        otherCef.on("OTHER_CEF_CLOSED", () => {
            otherCef.unfocus();
            otherCef.isVisible = false;
            otherCefOpened = false;

            alt.emitServer("OTHER_CEF_CLOSED");
        });

        otherCef.on("REQUEST_UPDATE_CONTROL_MAP", () => {
            alt.emitServer("REQUEST_UPDATE_CONTROL_MAP");
        });

        otherCef.on("SENT_DISPATCH_TO_PATROL", (dispatchId, patrolId) => {
            alt.emitServer("SENT_DISPATCH_TO_PATROL", dispatchId, patrolId);
        });

        otherCef.on("FIRE_EMPLOYEE", (systemId, charId) => {
            alt.emitServer("FIRE_EMPLOYEE", systemId, charId);
        });

        otherCef.on("FS_RANK_UPDATE", (systemId, rankId) => {
            alt.emitServer("FS_RANK_UPDATE", systemId, rankId);
        });

        otherCef.on("FS_EMPLOYEE_UPDATE", (systemId, charId) => {
            alt.emitServer("FS_EMPLOYEE_UPDATE", systemId, charId);
        });

        otherCef.on("SESSION_TOKEN_REMOVED", () => {
            otherCef.destroy();
            alt.setTimeout(() => {
                otherCef = null;
                onOpenOtherCef(savedUrl);
            }, 500);
        });
    }, 500);
}

alt.onServer("ANSWER_UPDATE_CONTROL_MAP", (markers) => {
    if(otherCef != null) {
        otherCef.emit("ANSWER_UPDATE_CONTROL_MAP", markers);
    }
});

alt.on('keyup', (key) => {
    if(key == 122) {
        if(otherCef != null) {
            otherCef.focus();
            otherCef.destroy();
            otherCef = null;

            alt.toggleGameControls(true);
            setLock(false);
            hideCursor();
            blockMovementCef = false;

            otherCefOpened = false;
            otherCef.focus();
        }
    }
});

var blockMovementCefList = [];

var blockMovementCef = false;
var blockAllCef = false;

var blockAllSmartphoneApp = false;
var blockLookSmartphoneApp = false;
var blockShootSmartphoneApp = false;


alt.onServer('SET_CEF_BLOCK_MOVEMENT', (identifer, block) => { 
    if(block) {
        if(blockMovementCefList.find(x => x == identifer) == undefined) {
            blockMovementCefList.push(identifer);
        }

        setLock(true);
        showCursor();
        blockAllCef = true;
        blockMovementCef = true;
    } else {
        var index = blockMovementCefList.indexOf(identifer);
        if(index > -1) {
            blockMovementCefList.splice(index, 1);
        }

        if(blockMovementCefList.length == 0) {
            setLock(false);
            hideCursor();
            blockAllCef = false;
            blockMovementCef = false;
        }
    }
});


alt.onServer('RESET_CEF_BLOCK_MOVEMENT', () => { 
    blockMovementCefList = [];

    setLock(false);
    hideCursor();
    blockAllCef = false;
    blockMovementCef = false;

    reactView.focus();
});

alt.onServer('FOCUS_ON_CEF', () => {
    if(otherCefOpened) {
        otherCef.focus();
    } else {
        if(!opened && !editorOpen) {
            reactView.focus();
        }
    }
});

alt.onServer('UNFOCUS_CEF', () => {
    reactView.unfocus();
});

alt.onServer("EDITOR_OPENED", (toggle) => {
    editorOpen = toggle;
    if(toggle) {
        reactView.unfocus();
    }
});

alt.everyTick(( ) => {
    if(blockAllCef || blockAllSmartphoneApp) {
        for(var i = 0; i <= 360; i++) {
            game.disableControlAction(0, i, true);
        }
    }

    if(blockLookSmartphoneApp) {
        //No Looking up and down
        game.disableControlAction(1, 1, true);
        game.disableControlAction(1, 2, true);
    }

    if(blockShootSmartphoneApp) {      
        //Player cannot aim
        game.disableControlAction(0, 25, true);
        
        //Player cannot shoot
        game.disableControlAction(0, 47, true);
        game.disableControlAction(0, 58, true);
        game.disableControlAction(0, 24, true);
        game.disableControlAction(0, 25, true);
        
        game.disableControlAction(0, 140, true);
        game.disableControlAction(0, 141, true);
        game.disableControlAction(0, 142, true);
        game.disableControlAction(0, 143, true);
        game.disableControlAction(0, 257, true);
        game.disableControlAction(0, 263, true);
        game.disableControlAction(0, 264, true);

        game.disablePlayerFiring(game.playerPedId(), true);

        game.disableControlAction(0, 12, 1);
        game.disableControlAction(0, 13, 1);
        game.disableControlAction(0, 14, 1);
        game.disableControlAction(0, 15, 1);
        game.disableControlAction(0, 16, 1);
        game.disableControlAction(0, 17, 1);
        game.disableControlAction(0, 37, 1);
        game.disableControlAction(0, 99, 1);
        game.disableControlAction(0, 100, 1);
        game.disableControlAction(0, 115, 1);
        game.disableControlAction(0, 116, 1);
        game.disableControlAction(0, 117, 1);
        game.disableControlAction(0, 118, 1);
        game.disableControlAction(0, 157, 1);
        game.disableControlAction(0, 158, 1);
        game.disableControlAction(0, 159, 1);
        game.disableControlAction(0, 160, 1);
        game.disableControlAction(0, 161, 1);
        game.disableControlAction(0, 162, 1);
        game.disableControlAction(0, 163, 1);
        game.disableControlAction(0, 164, 1);
        game.disableControlAction(0, 165, 1);
        game.disableControlAction(0, 166, 1);
        game.disableControlAction(0, 167, 1);
        game.disableControlAction(0, 168, 1);
        game.disableControlAction(0, 169, 1);
        game.disableControlAction(0, 261, 1);
        game.disableControlAction(0, 262, 1);
    }

    if(blockMovementCef) {
        //Sprinting and jumping
        game.disableControlAction(0, 21, 1);
        game.disableControlAction(0, 22, 1);

        //Player cannot shoot
        game.disableControlAction(0, 47, 1);
        game.disableControlAction(0, 58, 1);
        game.disableControlAction(0, 24, 1);
        game.disableControlAction(0, 25, 1);
        game.disablePlayerFiring(game.playerPedId(), 1);

        //Player cannot switch weapon
        game.disableControlAction(0, 12, 1);
        game.disableControlAction(0, 13, 1);
        game.disableControlAction(0, 14, 1);
        game.disableControlAction(0, 15, 1);
        game.disableControlAction(0, 16, 1);
        game.disableControlAction(0, 17, 1);
        game.disableControlAction(0, 37, 1);
        game.disableControlAction(0, 99, 1);
        game.disableControlAction(0, 100, 1);
        game.disableControlAction(0, 115, 1);
        game.disableControlAction(0, 116, 1);
        game.disableControlAction(0, 117, 1);
        game.disableControlAction(0, 118, 1);
        game.disableControlAction(0, 157, 1);
        game.disableControlAction(0, 158, 1);
        game.disableControlAction(0, 159, 1);
        game.disableControlAction(0, 160, 1);
        game.disableControlAction(0, 161, 1);
        game.disableControlAction(0, 162, 1);
        game.disableControlAction(0, 163, 1);
        game.disableControlAction(0, 164, 1);
        game.disableControlAction(0, 165, 1);
        game.disableControlAction(0, 166, 1);
        game.disableControlAction(0, 167, 1);
        game.disableControlAction(0, 168, 1);
        game.disableControlAction(0, 169, 1);
        game.disableControlAction(0, 261, 1);
        game.disableControlAction(0, 262, 1);
    }
});


//Smartphone
alt.onServer('TOGGLE_SMARTPHONE', (toggle) => {
    setLock(toggle);
    
    blockShootSmartphoneApp = toggle;
    blockLookSmartphoneApp = toggle;

    alt.showCursor(toggle);
});

alt.onServer('TOGGLE_NO_MOVE_APP', (toggle) => {
    blockAllSmartphoneApp = toggle;
});