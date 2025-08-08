import * as alt from 'alt'; 
import * as game from 'natives';

//TODO
// game.setArtificialLightsState(false);
// when san andreas to close

export var island = 0;

alt.on("connectionComplete", () => {
    let blip = game.addBlipForCoord(5943.5679611650485, -6272.114833599767,2); // a invisible blip to make the map clickable at the island
    game.setBlipSprite(blip, 407);
    game.setBlipScale(blip, 0);
    game.setBlipAsShortRange(blip, false);
});

alt.everyTick(() => {
    if(island == 1) {
        game.setRadarAsExteriorThisFrame();
        game.setRadarAsInteriorThisFrame(alt.hash("h4_fake_islandx"), 4700.0, -5145.0, 0, 0);
    }
});

alt.onServer("ARRIVE_AT_CAYO_PERICO", (withWaypoint) => {
    island = 1;

    game.setIslandEnabled('HeistIsland', true);
    game.setScenarioGroupEnabled('Heist_Island_Peds', true);
    game.setAudioFlag("PlayerOnDLCHeist4Island", true);
    game.setAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", true, true);
    game.setAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", false, true);

    if(withWaypoint) {
        game.setNewWaypoint(4470, -4500);
    }
});

alt.onServer("LEAVE_CAYO_PERICO", (withWaypoint) => {
    island = 0;
    
    game.setIslandEnabled('HeistIsland', false);
    game.setScenarioGroupEnabled("Heist_Island_Peds", false);
    game.setAudioFlag("PlayerOnDLCHeist4Island", false);
    game.setAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", false, false);
    game.setAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", false, false);

    if(withWaypoint) {
        game.setNewWaypoint(0, 0);
    }
});