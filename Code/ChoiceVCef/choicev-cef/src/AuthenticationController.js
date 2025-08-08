export default class AuthenticationController  {
    constructor(input) {
        this.requestId = 0;
        this.callbacks = {};

        this.onDiscordToken = this.onDiscordToken.bind(this);
        input.registerEvent("ANSWER_DISCORD_TOKEN", this.onDiscordToken);   
    }

    onDiscordToken(data) {
        var callbackId = data.callbackId;
        var token = data.token;

        if(callbackId in this.callbacks) {
            this.callbacks[callbackId](token);
            delete this.callbacks[callbackId];
        }
    }

    requestDisordToken(callback) {
        if("alt" in window) {
            var callbackId = this.requestId++;
            this.callbacks[callbackId] = callback;
            window.alt.emit("REQUEST_DISCORD_TOKEN", callbackId);
        }
    }
}