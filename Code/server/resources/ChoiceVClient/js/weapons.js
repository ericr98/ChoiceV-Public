import alt from 'alt';
import game from 'natives';
import {
	useFireExtinguisher
} from '/js/fire.js';

alt.onServer('GIVE_WEAPON_COMPONENT', (weapon, component) => {
	let player = alt.Player.local.scriptID;
	game.giveWeaponComponentToPed(player, alt.hash(weapon), alt.hash(component));

});

alt.onServer('REMOVE_WEAPON_COMPONENT', (weapon, component) => {
	let player = alt.Player.local.scriptID;
	game.removeWeaponComponentFromPed(player, alt.hash(weapon), alt.hash(component));
});

alt.onServer('GIVE_AMMO', (weapon, ammo) => {
	let player = alt.Player.local.scriptID;
	game.addAmmoToPed(player, alt.hash(weapon), ammo);
});

alt.onServer('SET_AMMO', (weapon, ammo) => {
	let player = alt.Player.local.scriptID;
	game.setPedAmmo(player, alt.hash(weapon), ammo);
});

alt.onServer('INITIALIZE_WEAPON_UNEQUIP', (weapon, id) => {
	let player = alt.Player.local.scriptID;
	var ammo = game.getAmmoInPedWeapon(player, alt.hash(weapon));
	//game.setPedAmmo(player, alt.hash(weapon), 0);

	game.removeWeaponFromPed(player, alt.hash(weapon));
	alt.emitServer("SUBMIT_WEAPON_UNEQUIP", id, ammo);
});

alt.onServer('INITIALIZE_ARMOUR_UNEQUIP', () => {
	let player = alt.Player.local.scriptID;
	var armour = game.getPedArmour(player);
	game.setPedArmour(player, 0);

	alt.emitServer("SUBMIT_ARMOUR_UNEQUIP", armour);
});

alt.onServer('SET_WEAPON_DAMAGE_MULT', (weaponName, mult) => {
	var data = alt.WeaponData.getForHash(alt.hash(weaponName));
	data.playerDamageModifier = mult;
});

var weaponNames = [];
var mults = [];

alt.onServer('SET_WEAPONS_DAMAGE_MULT', (weaponNamesL, multsL) => {
	weaponNames = weaponNamesL;
	mults = multsL;
});

alt.onServer("DEACTIVATE_ACTION_MODE", () => {
	if(game.isPedUsingActionMode(alt.Player.local.scriptID)) {
		game.setPedUsingActionMode(alt.Player.local.scriptID, false, -1, 0);
	}
});

var lastWeapon;
var lastAmmo;
var ticks = 10;
var cTicks = 0;

var meleeList = [
	alt.hash("weapon_dagger"),
	alt.hash("weapon_bat"),
	alt.hash("weapon_bottle"),
	alt.hash("weapon_crowbar"),
	alt.hash("weapon_unarmed"),
	alt.hash("weapon_flashlight"),
	alt.hash("weapon_golfclub"),
	alt.hash("weapon_hammer"),
	alt.hash("weapon_hatchet"),
	alt.hash("weapon_knuckle"),
	alt.hash("weapon_knife"),
	alt.hash("weapon_machete"),
	alt.hash("weapon_switchblade"),
	alt.hash("weapon_nightstick"),
	alt.hash("weapon_wrench"),
	alt.hash("weapon_battleaxe"),
	alt.hash("weapon_poolcue"),
	alt.hash("weapon_stone_hatchet")
]

const recoils = new Map();
recoils.set(453432689, 0.3); // PISTOL
recoils.set(3219281620, 0.3); // PISTOL MK2
recoils.set(1593441988, 0.2); // COMBAT PISTOL
recoils.set(584646201, 0.1); // AP PISTOL
recoils.set(2578377531, 0.6); // PISTOL .50
recoils.set(3218215474, 0.2); // SNS PISTOL
recoils.set(3523564046, 0.5); // HEAVY PISTOL
recoils.set(137902532, 0.4); // VINTAGE PISTOL
recoils.set(1198879012, 0.9); // FLARE GUN
recoils.set(3696079510, 0.9); // MARKSMAN PISTOL
recoils.set(3249783761, 0.6); // REVOLVER
recoils.set(3415619887, 0.6); // REVOLVER MK2
recoils.set(2441047180, 0.6); // NAVY REVOLVER
recoils.set(727643628, 0.2); // CERAMIC PISTOL
recoils.set(1470379660, 0.1); // GADGET PISTOL


recoils.set(324215364, 0.2) // MICRO SMG
recoils.set(736523883, 0.25) // SMG
recoils.set(2024373456, 0.1) // SMG MK2
recoils.set(4024951519, 0.1) // ASSAULT SMG
recoils.set(171789620, 0.2) // COMBAT PDW
recoils.set(3675956304, 0.3) // MACHINE PISTOL
recoils.set(3173288789, 0.1) // MINI SMG

recoils.set(3220176749, 0.2) // ASSAULT RIFLE
recoils.set(961495388, 0.2) // ASSAULT RIFLE MK2
recoils.set(2210333304, 0.1) // CARBINE RIFLE
recoils.set(4208062921, 0.1) // CARBINE RIFLE MK2
recoils.set(2937143193, 0.1) // ADVANCED RIFLE
recoils.set(3231910285, 0.2) // SPECIAL CARBINE
recoils.set(2526821735, 0.15) // SPECIAL CARBINE MK2
recoils.set(2132975508, 0.2) // BULLPUP RIFLE
recoils.set(2228681469, 0.15) // BULLPUP RIFLE MK2
recoils.set(2636060646, 0.15) // MILITARY RIFLE
recoils.set(3347935668, 0.15) // HEAVY RIFLE
recoils.set(3520460075, 0.2) // TACTICAL RIFLE

recoils.set(2634544996, 0.1) // MG
recoils.set(2144741730, 0.1) // COMBAT MG
recoils.set(3686625920, 0.1) // COMBAT MG MK2
recoils.set(1627465347, 0.1) // GUSENBERG

recoils.set(487013001, 0.4) // PUMP SHOTGUN
recoils.set(1432025498, 0.35) // PUMP SHOTGUN MK2
recoils.set(2017895192, 0.7) // SAWNOFF SHOTGUN
recoils.set(3800352039, 0.4) // ASSAULT SHOTGUN
recoils.set(2640438543, 0.2) // BULLPUP SHOTGUN
recoils.set(984333226, 0.2) // HEAVY SHOTGUN
recoils.set(4019527611, 0.7) // DOUBLE BARREL SHOTGUN
recoils.set(317205821, 0.2) // AUTO SHOTGUN
recoils.set(94989220, 0.4) // COMBAT SHOTGUN

recoils.set(911657153, 0.1) // STUN GUN

recoils.set(100416529, 0.5) // SNIPER RIFLE
recoils.set(205991906, 0.7) // HEAVY SNIPER
recoils.set(177293209, 0.6) // HEAVY SNIPER MK2
recoils.set(856002082, 1.2) // REMOTE SNIPER
recoils.set(2828843422, 0.7) // MUSKET
recoils.set(3342088282, 0.3) // MARKSMAN RIFLE
recoils.set(1785463520, 0.25) // MARKSMAN RIFLE MK
recoils.set(1649403952, 0.3) // COMPACT RIFLE
recoils.set(1853742572, 0.6) // COMPACT RIFLE

recoils.set(2726580491, 1.0) // GRENADE LAUNCHER
recoils.set(1305664598, 1.0) // GRENADE LAUNCHER SMOKE
recoils.set(2982836145, 0.0) // RPG
recoils.set(1752584910, 0.0) // STINGER
recoils.set(1119849093, 0.01) // MINIGUN
recoils.set(1672152130, 0) // HOMING LAUNCHER
recoils.set(1834241177, 2.4) // RAILGUN
recoils.set(125959754, 0.5) // COMPACT LAUNCHER
	

alt.everyTick(() => {
	cTicks++;
	var player = alt.Player.local.scriptID;
	var currentWeapon = game.getSelectedPedWeapon(player);

	game.setPedSuffersCriticalHits(player, false);

	if(!game.isPlayerFreeAiming(game.playerId()) && !meleeList.includes(game.getSelectedPedWeapon(player))) {
		game.disablePlayerFiring(alt.Player.local.scriptID, 1);
	}

	//Disable one hit weapon hit
	if (game.isPedArmed(player, 6)) {
		game.disableControlAction(0, 140, true); // INPUT_MELEE_ATTACK_LIGHT (R button - B on controller)
		game.disableControlAction(0, 141, true); // INPUT_MELEE_ATTACK_HEAVY (Q button - A on controller)
		game.disableControlAction(0, 142, true); // INPUT_MELEE_ATTACK_ALTERNATE (LMB - RT on controller)
	}

	if(game.isPedShooting(player)) {
		if(!game.isPedDoingDriveby(player) && recoils.get(currentWeapon) != 0) {
			var count = 0
			while (count < recoils.get(currentWeapon)) {
				var p = game.getGameplayCamRelativePitch();
				if (recoils.get(currentWeapon) > 0.1) {
					game.setGameplayCamRelativePitch(p + 0.6, 1.2);
					count = count + 0.6
				} else {
					game.setGameplayCamRelativePitch(p + 0.016, 0.333);
					count = count + 0.1
				}
			}
		}
	}

	if (cTicks >= ticks)
		cTicks = 0;
	else
		return;

	if(alt.Player.local.vehicle != null) {

	}

	if(currentWeapon == 2725352035) {
		return;
	}

	for (var i = 0; i < weaponNames.length; i++) {
		var data = alt.WeaponData.getForHash(alt.hash(weaponNames[i]));
		data.playerDamageModifier = mults[i];
	}

	var ammo = 0;

	if (lastWeapon !== undefined) {
		ammo = game.getAmmoInPedWeapon(player, lastWeapon);
	}
	
	if (game.isPedShooting(player)){
		var weaponDamage = game.getWeaponDamage(currentWeapon, 0);

		// If Fireextinguisher call Fire class function
		if (currentWeapon == 101631238) {
			useFireExtinguisher();
		}
	}

	if (lastWeapon == undefined) {
		lastWeapon = currentWeapon;
	}

	if (lastAmmo == undefined) {
		lastAmmo = ammo;
	}

	if (lastWeapon != currentWeapon) {
		lastWeapon = currentWeapon;
	} else {
		//if (ammo < lastAmmo) {
		//	var weaponDamage = game.getWeaponDamage(currentWeapon, 0);
		//	alt.emitServer('WEAPON_SHOOT', currentWeapon, weaponDamage);
		//	var weaponAmmoType = game.getPedAmmoTypeFromWeapon(alt.Player.local.scriptID, currentWeapon);
		//	var ammo = game.getPedAmmoByType(alt.Player.local.scriptID, weaponAmmoType);
		//	if (ammo >= 9999 || ammo == -1) {
		//		alt.emitServer('ANTICHEAT_BAN');
		//	}
		//}
	}

	lastAmmo = ammo;
});


var selectedWeapon = "WEAPON_UNARMED";
var wasAiming = false;

alt.setInterval(() => {
	selectedWeapon = game.getSelectedPedWeapon(alt.Player.local.scriptID);
}, 1000);

alt.setInterval(() => {
	if(selectedWeapon != "WEAPON_UNARMED") {
		if(game.isPlayerFreeAiming(game.playerId()) || game.isPedShooting(game.playerId())) {
			wasAiming = true;
			game.setFollowPedCamViewMode(4);
			game.setFollowVehicleCamViewMode(4);
			game.setCamViewModeForContext(2, 4);
        } else if(wasAiming) {
			wasAiming = false;
			game.setFollowPedCamViewMode(0);
			game.setFollowVehicleCamViewMode(0);
			game.setCamViewModeForContext(2, 0);
        }
	}
}, 1);