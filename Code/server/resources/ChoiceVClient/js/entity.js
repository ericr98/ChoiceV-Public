import * as alt from 'alt'; 
import * as game from 'natives';

export function loadModel(modelHash) {
    return new Promise((resolve, reject) => {
        if (!game.isModelValid(modelHash)) return resolve(false);

        if (game.hasModelLoaded(modelHash)) return resolve(modelHash);

        let requestModel = null;
        let interval = alt.setInterval(() => {
            if(requestModel == null) {
                game.requestModel(modelHash);
                requestModel = modelHash;
            }

            if (game.hasModelLoaded(modelHash)) {
                alt.clearInterval(interval);
                requestModel = null;

                return resolve(modelHash);
            }
        }, 10);
    });
}

export function loadAnimDic(animDict) {
    return new Promise((resolve, reject) => {
        if (!game.doesAnimDictExist(animDict)) return resolve(false);

        if (game.hasAnimDictLoaded(animDict)) return resolve(animDict);

        let requestDict = null;
        let interval = alt.setInterval(() => {
            if(requestDict == null) {
                game.requestAnimDict(animDict);
                requestDict = animDict;
            }

            if (game.hasAnimDictLoaded(animDict)) {
                alt.clearInterval(interval);
                requestDict = null;

                return resolve(animDict);
            }
        }, 10);
    });
}

export function loadClipSet(clipSet) {
    return new Promise((resolve, reject) => {

        if (game.hasClipSetLoaded(clipSet)) return resolve(clipSet);

        let requestDict = null;
        let interval = alt.setInterval(() => {
            if(requestDict == null) {
                game.requestClipSet(clipSet);
                requestDict = clipSet;
            }

            if (game.hasClipSetLoaded(clipSet)) {
                alt.clearInterval(interval);
                requestDict = null;

                return resolve(clipSet);
            }
        }, 10);
    });
}

export function entityExists(entity) {
    return new Promise((resolve, reject) => {
        if (game.doesEntityExist(entity)) return resolve(entity);

        let interval = alt.setInterval(() => {
            if (game.doesEntityExist(entity)) {
                alt.clearInterval(interval);

                return resolve(entity);
            }
        }, 10);
    });
}

export function entitySpawned(entity) {
    return new Promise((resolve, reject) => {
        var counter = 0;
        if (game.doesEntityExist(entity) && entity.spawned) return resolve(entity);

        let interval = alt.setInterval(() => {
            if (counter >= 200 || (game.doesEntityExist(entity) && entity.spawned)) {
                alt.clearInterval(interval);

                return resolve(entity);
            }
            counter++;
        }, 10);
    });
}