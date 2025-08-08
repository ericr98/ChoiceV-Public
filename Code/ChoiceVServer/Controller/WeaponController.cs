using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {
    public class WeaponController : ChoiceVScript {
        public static uint WeaponHandHash = ChoiceVAPI.Hash("weapon_unarmed");

        public static Dictionary<string, configweapon> AllConfigWeapons = [];
        public static List<WeaponDamage> weaponList = [];

        public WeaponController() {
            EventController.addEvent("WEAPON_SHOOT", onWeaponShoot);
            EventController.addMenuEvent("WEAPON_DISASSEMBLE", onWeaponDisassemble);

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnected;
            EventController.PlayerWeaponChangeDelegate += onPlayerChangeWeapon;

            //Add Clothingcheck for Armor!

            fillList();

            //EventController.StartProjectileDelegate += onStartProjectile;

            loadConfigWeapons();
        }

        public static void loadConfigWeapons() {
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.configweapons) {
                    AllConfigWeapons[row.weaponName] = row;
                }
            }
        }

        public static void equipWeaponToPlayer(IPlayer player, Weapon weapon) {
            var equipedWeapons = player.getInventory().getItems<Weapon>(i => i.IsEquipped);

            if(equipedWeapons == null || !equipedWeapons.Contains(weapon)) {
                if(equipedWeapons == null || equipedWeapons.FirstOrDefault(i => (i as Weapon).WeaponType == weapon.WeaponType) == null) {
                    var ammunation = player.getInventory().getItem<WeaponAmmunation>(i => i.WeaponType == weapon.WeaponType && i.additionalEquipCheck(weapon));
                    var count = 0;
                    if(ammunation != null) {
                        count = ammunation.StackAmount ?? 1;
                        player.getInventory().removeItem(ammunation, count);
                    }

                    if(weapon.AmountOfAmmunation > 0) {
                        count = weapon.AmountOfAmmunation ?? 0;
                    }

                    weapon.AmountOfAmmunation = count;

                    ChoiceVAPI.giveWeaponToPlayer(player, weapon.WeaponName, count, false);
                    foreach(var component in weapon.WeaponParts) {
                        if(component.GTAComponent != null) {
                            ChoiceVAPI.giveWeaponComponentToPlayer(player, weapon.WeaponName, component.GTAComponent);
                        }
                    }
                } else {
                    player.sendBlockNotification("Du hast schon eine Waffe diesen Types ausgerüstet!", "Waffenslot voll!");
                }
            } else {
                player.sendBlockNotification("Diese Waffe ist schon ausgerüstet!", "Waffe blockiert!");
            }
        }

        public static void unequipWeaponToPlayer(IPlayer player, Weapon weapon) {
            if (weapon != null) {
                var hash = ChoiceVAPI.Hash(weapon.WeaponName);
                var ammoCount = player.GetWeaponAmmo(hash);
                ChoiceVAPI.removeWeaponFromPlayer(player, weapon.WeaponName);

                foreach (var component in weapon.WeaponParts) {
                    if (component.GTAComponent != null) {
                        ChoiceVAPI.removeWeaponComponentFromPlayer(player, weapon.WeaponName, component.GTAComponent);
                    }
                }

                if(ammoCount > weapon.AmountOfAmmunation) {
                    player.ban("Weapon-Cheat: Munition gecheatet!");
                }

                if (ammoCount > 0) {
                    var amCfg = InventoryController.getConfigItemForType<WeaponAmmunation>(a => a.additionalInfo == weapon.WeaponType.ToString());
                    if (amCfg != null) {
                        var ammunation = new WeaponAmmunation(amCfg, ammoCount, -1);
                        player.getInventory().addItem(ammunation, true);
                    }
                }

                weapon.AmountOfAmmunation = 0;
            } else {
                Logger.logError("onWeaponUnequip: player had Weapon equipped which was not in his inventory",
                            $"Fehler Waffensystem: Spieler hatte Waffe ausgerüstet die er nicht im Inventar hatte: Id: {weapon.Id}", player);
                ChoiceVAPI.KickPlayer(player, "ShootingStar", "Es gab ein Fehler beim Waffensystem! Melde dich unverzüglich im Support!", "Fehler im Waffensystem. Muniton konnte geschossen werden, obwohl keine Waffe im Inventar ausgerüstet war: Möglicher Cheatverdacht!");
            }
        }

        private void onPlayerConnected(IPlayer player, character character) {
            ChoiceVAPI.setPlayerWeaponsDamageMult(player, AllConfigWeapons.Values.Select(i => i.weaponName).ToArray(), AllConfigWeapons.Values.Select(i => i.damageMult).ToArray());
        }

        public static void showLongWeaponOnPlayer(IPlayer player, LongWeapon weapon) {
            if(!player.hasData("LONG_WEAPON_DISPLAY")) {
                player.setData("LONG_WEAPON_DISPLAY", weapon);
                checkForDisplay(player, null, null);
            } else {
                Logger.logError($"showLongWeaponOnPlayer: player had Weapon in Inventory he should had: charId: {player.getCharacterId()}, itemId: {weapon.Id}",
                        $"Fehler Waffensystem: Spieler hatte Waffe nicht angzeigt obwohl er sie im Inventar hatte. Waffen-Item-Id: {weapon.Id}", player);
            }
        }

        public static void noLongerShowLongWeaponOnPlayer(IPlayer player, LongWeapon weapon) {
            if(player.hasData("LONG_WEAPON_DISPLAY")) {
                player.resetData("LONG_WEAPON_DISPLAY");
                var obj = player.getData("LONG_WEAPON_DISPLAY_OBJECT");
                if(obj != null) {
                    ObjectController.deleteObject((Object)obj);
                }
            } else {
                Logger.logError($"showLongWeaponOnPlayer: player had Weapon on display, not in his inventory: charId: {player.getCharacterId()}, itemId: {weapon.Id}",
                        $"Fehler Waffensystem: Spieler hatte Waffe nicht angzeigt obwohl er sie im Inventar hatte. Waffen-Item-Id: {weapon.Id}", player);
            }
        }

        private bool onWeaponDisassemble(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (Weapon)data["Item"];
            player.getInventory().removeItem(item);

            foreach(var part in item.WeaponParts) {
                WeaponPartItem itemPart;
                var cf = InventoryController.getConfigItem(i => i.additionalInfo == part.WeaponPartType.ToString());
                itemPart = new WeaponPartItem(cf, part);
                player.getInventory().addItem(itemPart);
            }

            return true;
        }

        public static void assembleWeapon(IPlayer player, WeaponPartItem body) {
            var partItems = player.getInventory().getItems<WeaponPartItem>(i => true);
            if(body.WeaponPart.WeaponName != null && partItems != null) {
                var cfWeapon = getConfigWeapon(body.WeaponPart.WeaponName);
                var weaponType = (WeaponType)Enum.Parse(typeof(WeaponType), cfWeapon.weaponType);
                var parts = WeaponTypeToWeaponParts[weaponType];
                var UsedParts = new List<WeaponPartItem>();

                foreach(var part in parts) {
                    var item = partItems.FirstOrDefault(i => i.WeaponPart.WeaponPartType == part);
                    if(item != null) {
                        UsedParts.Add(item);
                    } else {
                        UsedParts = null;
                        break;
                    }
                }

                if(UsedParts != null) {
                    AnimationController.animationTask(player, AnimationController.getAnimationByName("HACK_ATM"), () => {
                        foreach(var item in UsedParts) {
                            player.getInventory().removeItem(item);
                        }

                        var cf = InventoryController.getConfigItem(c => c.additionalInfo == body.WeaponPart.WeaponName);

                        if(weaponType == WeaponType.Rifle || weaponType == WeaponType.Shotgun || weaponType == WeaponType.Smg) {
                            var weapon = new LongWeapon(cf, UsedParts.Select(u => u.WeaponPart).ToList());
                            player.getInventory().addItem(weapon);
                        } else {
                            var weapon = new Weapon(cf, UsedParts.Select(u => u.WeaponPart).ToList());
                            player.getInventory().addItem(weapon);
                        }
                    });
                } else {
                    player.sendBlockNotification("Du hast nicht die passenden Teile dabei!", "Nicht genug Teile");
                }
            } else {
                player.sendBlockNotification("Du hast nicht die passenden Teile dabei!", "Nicht genug Teile");
            }
        }

        private static void checkForDisplay(IPlayer player, uint? currentWeapon, uint? lastWeapon) {
            if(player.hasData("LONG_WEAPON_DISPLAY")) {
                var weapon = (LongWeapon)player.getData("LONG_WEAPON_DISPLAY");

                if(ChoiceVAPI.Hash(weapon.WeaponName) == currentWeapon) {
                    var obj = (Object)player.getData("LONG_WEAPON_DISPLAY_OBJECT");
                    if(obj != null) {
                        ObjectController.deleteObject(obj);
                    } else {
                        Logger.logError($"onWeaponChange: Tried to remove displayed longweapon without object: charId: {player.getCharacterId()}, weaponId: {weapon.Id}", 
                            $"Fehler Waffensystem: Spieler hatte Waffe angzeigt aber kein Objekt konnte gefunden werden. Waffen-Item-Id: {weapon.Id}", player);
                    }
                } else if(ChoiceVAPI.Hash(weapon.WeaponName) == lastWeapon || (currentWeapon == null && lastWeapon == null)) {
                    var cfWeapon = AllConfigWeapons[weapon.WeaponName];
                    //{"X":0.075,"Y":-0.15,"Z":-0.02}
                    var obj = ObjectController.createObject(cfWeapon.attachObjectModel, player, cfWeapon.attachObjectPosition.FromJson(), cfWeapon.attachObjectRotation.FromJson<Rotation>(), cfWeapon.attachObjectBone ?? 0, -1, false); ;
                    player.setData("LONG_WEAPON_DISPLAY_OBJECT", obj);
                }
            }
        }

        private bool onWeaponShoot(IPlayer player, string eventName, object[] args) {
            var weapon = uint.Parse(args[0].ToString());

            if(weapon == (uint)AltV.Net.Enums.WeaponModel.FireExtinguisher) {
                return true;
            }

            var weaponItem = player.getInventory().getItem<Weapon>(w => ChoiceVAPI.Hash(w.WeaponName) == weapon);
            if(weaponItem != null) {
                EvidenceController.createWeaponEvidence(player, weaponItem);
                DamageController.HurtfulArmActionDelegate?.Invoke(player, 0.25f, "Abfeuern der Waffe");
                weaponItem.TempAmmoSpent++;
            } else {
                if(player.getAdminLevel() <= 0) {
                    player.ban("Weapon-Cheat");

                    Logger.logError($"onWeaponShoot: Weapon shoot was not in the player inventory: weapon: {weapon}, player: {player.getCharacterId()}",
                        $"Fehler Waffensystem: Spieler hat mit einer Waffe geschossen, obwohl er sie nicht im Inventar hat! Waffen-GTA-Hash: {weapon}", player);

                    return false;
                }
            }

            return true;
        }

        private void fillList() {
            using(var db = new ChoiceVDb()) {
                foreach(var weapon in db.configweapondamages) {
                    var newWeapon = new WeaponDamage {
                        weaponUint = uint.Parse(weapon.weaponint),
                        damage = weapon.damage,
                        weaponName = weapon.weapon,
                    };
                    weaponList.Add(newWeapon);
                }
            }
        }

        public static configweapon getConfigWeapon(string weaponName) {
            if(AllConfigWeapons.ContainsKey(weaponName)) {
                return AllConfigWeapons[weaponName];
            } else {
                return null;
            }
        }

        public static configweapon getConfigWeapon(uint weaponHash) {
            return AllConfigWeapons.FirstOrDefault(w => ChoiceVAPI.Hash(w.Key) == weaponHash).Value;
        }

        private void onPlayerChangeWeapon(IPlayer player, uint oldWeapon, uint newWeapon) {
            if(newWeapon == WeaponHandHash) {
                var cfg = getConfigWeapon(oldWeapon);
                if(cfg == null) return;
                CamController.checkIfCamSawAction(player, "Waffe abgerüstet", $"Die Person die Waffe {cfg.displayName} weggesteckt.");
            } else {
                if(oldWeapon == WeaponHandHash) {
                    var cfg = getConfigWeapon(newWeapon);
                    if(cfg == null) return;
                    CamController.checkIfCamSawAction(player, "Waffe ausgerüstet", $"Die Person die Waffe {cfg.displayName} herausgeholt.");
                } else {
                    var cfgOld = getConfigWeapon(oldWeapon);
                    var cfgNew = getConfigWeapon(newWeapon);
                    if(cfgOld == null || cfgNew == null) return;
                    CamController.checkIfCamSawAction(player, "Waffe gewechselt", $"Die Person die Waffe von {cfgOld.displayName} zu {cfgNew.displayName} gewechselt.");
                }
            }

            if(player.hasData("LONG_WEAPON_DISPLAY")) {
                checkForDisplay(player, newWeapon, oldWeapon);
            }

            //CamController.checkIfCamSawAction(player, "")
        }
    }

    public class WeaponDamage {
        public uint weaponUint { get; set; }
        public float damage { get; set; }
        public string weaponName { get; set; }

    }
}
