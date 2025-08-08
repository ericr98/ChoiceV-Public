import React from 'react';

export default class OutputController  {
    constructor() {

    }

    initWebSocket(ws, identifier, loginToken) {
        this.ws = ws;
        this.identifier = identifier;
        this.loginToken = loginToken;
    }

    sendToServer(event, data, releaseMovement, movementBlockedIdentifier) {
        var dataObject = {
            Id: this.identifier,
            LoginToken: this.loginToken,
            Event: event,
            Data: JSON.stringify(data),
            ReleaseMovement: releaseMovement,
            MovementBlockedIdentifier: movementBlockedIdentifier,
        }

        if(this.ws !== undefined) {
            this.ws.send(JSON.stringify(dataObject));
        } else {
            console.log("Tried to send evt without websocket: evt: " + event + "data: ");
            console.log(JSON.stringify(data));
        }
    }
}