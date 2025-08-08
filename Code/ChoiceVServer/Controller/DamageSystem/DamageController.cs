using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.Controller.Discord;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.DamageSystem {
    public delegate void BackendPlayerDeathDelegate(IPlayer player);
    public delegate void BackendPlayerReviveDelegate(IPlayer player);

    ///The weight should be a number from 0 to 1, that represents the amount of "pressure" on the arm
    ///0.1 is only a light pressure: Opening a hard to open can
    ///0.5 is a medium pressure: Hammering in a nail
    ///1: is a high pressure: using a jackhammer
    public delegate void HurtfulArmActionDelegate(IPlayer player, float weight, string actionName);

    public class DamageController : ChoiceVScript {
        public static HurtfulArmActionDelegate HurtfulArmActionDelegate;

        public Dictionary<int, int> PlayerTearGas = new Dictionary<int, int>();
        public Dictionary<int, DeathTypes> PlayerStates = new Dictionary<int, DeathTypes>();

        public static BackendPlayerDeathDelegate BackendPlayerDeathDelegate;
        public static BackendPlayerReviveDelegate BackendPlayerReviveDelegate;

        public static TimeSpan DAMAGE_TICK_TIME = TimeSpan.FromMinutes(1);

        public DamageController() {
            EventController.PlayerDamageDelegate += onPlayerDamage;
            EventController.PlayerDeadDelegate += onPlayerDead;
            EventController.WeaponDamageDelegate += onWeaponDamage;
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            EventController.addOnPlayerMoveCallback(onPlayerMove);

            InvokeController.AddTimedInvoke("DamageCheck", checkInjuriesForUpdate, DAMAGE_TICK_TIME, true);

            CharacterController.addPlayerConnectDataSetCallback("PLAYER_DEAD", onPlayerDead);
            CharacterController.addPlayerConnectDataSetCallback("PLAYER_PERMA_DEAD", onPlayerPermaDead);

            InventoryController.ItemUsedDelegate += onItemUsed;
            HurtfulArmActionDelegate += onPenalizeArmAction;

            EventController.addMenuEvent("DISPATCH_SEND_FROM_DEAD", onSendDispatchFromDead);
        }

        public static void addPlayerInjury(IPlayer player, DamageType injuryType, CharacterBodyPart bodyPart, int damage) {
            player.getCharacterData().CharacterDamage.addInjury(player, injuryType, bodyPart, damage);
        }

        #region Penalties for not properly rp-ing with injury

        private void onPlayerMove(object sender, IPlayer player, Position moveToPosition, float distance) {
            if (player.Exists() && player.getCharacterFullyLoaded() && !player.IsInVehicle && player.MoveSpeed > 1.5) {
                var damg = player.getCharacterData().CharacterDamage;
                var addDmg = 0f;
                foreach (var inj in damg.AllInjuries) {
                    var addDamg = inj.checkMoving(distance);
                    inj.Damage += addDamg;
                    addDmg += addDamg;
                }

                if (addDmg > 0) {
                    if (player.hasData("RUNNING_PENALTY_NOTIFICATION")) {
                        var lastNot = player.getData("RUNNING_PENALTY_NOTIFICATION");
                        if (DateTime.Now - lastNot > TimeSpan.FromMinutes(1)) {
                            sendLegMessage(player);
                        }
                    } else {
                        sendLegMessage(player);
                    }
                }
            }
        }

        private void sendLegMessage(IPlayer player) {
            var medLevel = player.getMedicatedPainLevel();
            var painInfo = player.getCharacterData().CharacterDamage.getPainInfo();
            if (painInfo.SevernessLevel > medLevel) {
                player.sendBlockNotification("Es ist schmerzhaft, mit dem verletzen Bein bzw. Torso zu laufen", "Schmerzen im Bein", NotifactionImages.Bone);

            } else {
                player.sendBlockNotification("Dein Bein bzw. Torso fühlt sich trotz Schmerzmittel komisch an beim Laufen", "Schmerzen im Bein", NotifactionImages.Bone);
            }

            player.setData("RUNNING_PENALTY_NOTIFICATION", DateTime.Now);
        }

        private void onItemUsed(IPlayer player, Item item) {
            if (item is ToolItem) {
                HurtfulArmActionDelegate?.Invoke(player, Math.Min(item.Weight / 10, 1), "Benutzen des Werkzeuges");
            }
        }

        private void onPenalizeArmAction(IPlayer player, float weight, string actionName) {
            var damg = player.getCharacterData().CharacterDamage;
            var addDmg = 0f;
            foreach (var inj in damg.AllInjuries) {
                var addDamg = inj.checkArmPressure(weight);
                inj.Damage += addDamg;
                addDmg += addDamg;
            }

            if (addDmg > 0) {
                var showMessage = false;

                if (player.hasData("ARM_PENALTY_NOTIFICATION")) {
                    var lastNot = player.getData("ARM_PENALTY_NOTIFICATION");
                    if (DateTime.Now - lastNot > TimeSpan.FromSeconds(10)) {
                        showMessage = true;
                    }
                } else {
                    showMessage = true;
                }

                if (showMessage) {
                    var medLevel = player.getMedicatedPainLevel();
                    var painInfo = player.getCharacterData().CharacterDamage.getPainInfo();
                    if (painInfo.SevernessLevel > medLevel) {
                        player.sendBlockNotification($"Es war schmerzhaft das \"{actionName}\" mit deinem verletzten Arm bzw. Torso auszuführen", "Schmerzen im Arm", NotifactionImages.Bone);

                    } else {
                        player.sendBlockNotification($"Dein Arm bzw. Torso hat sich beim {actionName} nicht gut angefühlt!", "Schmerzen im Arm", NotifactionImages.Bone);
                    }

                    player.setData("ARM_PENALTY_NOTIFICATION", DateTime.Now);
                }
            }
        }

        #endregion

        private void checkInjuriesForUpdate(IInvoke obj) {
            var rand = new Random();

            var meds = DrugController.getDrugsByPredicate<MedicationDrug>(d => true);

            using (var db = new ChoiceVDb()) {
                foreach (var player in ChoiceVAPI.GetAllPlayers().Reverse<IPlayer>()) {
                    if (player.Exists() && player.getCharacterFullyLoaded()) {
                        var data = player.getCharacterData();
                        var damg = data.CharacterDamage;

                        var medPainLevel = PainMedicationController.getMedicatedPainLevel(meds, player);

                        var wastedPain = 0f;

                        if (!player.hasState(Constants.PlayerStates.Dead)) {
                            foreach (var inj in damg.AllInjuries) {
                                wastedPain += inj.updateRecovery(DAMAGE_TICK_TIME, medPainLevel, player.hasState(Constants.PlayerStates.InHospital));

                                if (inj.Damage <= 0) {
                                    damg.healInjury(player, inj);
                                }
                            }

                            if (wastedPain >= 100) {
                                killPlayer(player, true, "Deine Verletzungen waren nicht mehr auszuhalten und du bist in Ohnmacht gefallen!", "In Ohnmacht gefallen");
                            }
                        } else {
                            var timeOfDeath = ((string)player.getData("DEATH_MOMENT")).FromJson<DateTime>();

                            if(timeOfDeath + TimeSpan.FromMinutes(15) < DateTime.Now) {
                                player.showMenu(MenuController.getConfirmationMenu("Dispatch senden", "Dispatch an die Leitstelle senden?", "DISPATCH_SEND_FROM_DEAD"));
                            }

                            var wastedPainBefore = damg.getWastedPain();
                            if (wastedPainBefore < 100) {
                                foreach (var inj in damg.AllInjuries) {
                                    wastedPain += inj.updateRecoveryWhileDead(DAMAGE_TICK_TIME);
                                }

                                wastedPain = Math.Min(wastedPain, 99);
                            }
                        }

                        damg.checkPainEffects(player, medPainLevel);

                        var diff = player.getHealth() - (100 - wastedPain);
                        if (Math.Abs(diff) > 2) {
                            diff = 2 * Math.Sign(diff);
                        }

                        if (!player.hasState(Constants.PlayerStates.InHospital) || player.hasState(Constants.PlayerStates.InHospital) && diff > 0) {
                            if (player.getHealth() - diff > 0 || player.getHealth() - diff <= 0 && !player.hasState(Constants.PlayerStates.Dead)) {
                                player.setHealth(player.getHealth() - diff, diff > 0);
                            }
                        }

                        if (wastedPain <= 25) {
                            if (!player.hasData("DEATH_MOMENT") || ((string)player.getData("DEATH_MOMENT")).FromJson<DateTime>() + TimeSpan.FromMinutes(10) < DateTime.Now) {
                                if(!data.PermadeathActivated) {
                                    revivePlayer(player);
                                }
                            }
                        }

                        damg.saveDamagesToDb(db);
                    }
                }

                db.SaveChanges();
            }
        }

        private void onPlayerDamage(IPlayer player, IEntity killer, uint weapon, ushort damage) {
            if (player.hasData("IGNORE_NEXT_DAMAGE") || player.hasState(Constants.PlayerStates.InTerminal)) {
                player.resetData("IGNORE_NEXT_DAMAGE");
                return;
            }

            if (!Config.IsDamageSystemEnabled || player.hasState(Constants.PlayerStates.Dead) || !player.getCharacterFullyLoaded()) {
                return;
            }

            var charId = player.getCharacterId();
            var type = DamageType.Dull;

            if (WeaponIdToDamageType.ContainsKey(weapon)) {
                type = WeaponIdToDamageType[weapon];
            }

            if (type == DamageType.NoInjury) {
                return;
            }

            EvidenceController.createBloodEvidence(player);

            CallbackController.getPlayerLastDamagedBone(player, (p, bone) => {
                //Drowning and Gas
                if (bone == 0) {
                    return;
                }

                var data = player.getCharacterData();
                var temp = damage;
                if (player.Health == 0) {
                    damage -= 100;
                    if (damage <= 0) {
                        damage = temp;
                    }
                }

                var armorWest = player.getInventory().getItem<ArmorWest>(i => i.IsEquipped);
                if(armorWest != null) {
                    
                }

                data.CharacterDamage.addInjury(p, type, bone, damage);
            });
        }

        private WeaponDamageResponse onWeaponDamage(IPlayer player, IEntity target, uint weapon, ushort damage, Position shotOffset, BodyPart bodyPart) {
            if (target is IPlayer t && t.hasState(Constants.PlayerStates.Dead)) {
                return new WeaponDamageResponse(false, 0);
            }

            return new WeaponDamageResponse(true, damage);
        }

        private void onPlayerConnect(IPlayer player, character character) {
            var damage = player.getCharacterData().CharacterDamage;

            if (damage.getWastedPain() >= 100) {
                killPlayer(player, false, "Durch den starken Schmerz bist du Ohnmächtig geworden!", "Ohnmächtig geworden!");
            } else {
                var meds = DrugController.getDrugsByPredicate<MedicationDrug>(d => true);
                var medPainLevel = PainMedicationController.getMedicatedPainLevel(meds, player);

                damage.checkPainEffects(player, medPainLevel);
            }
        }

        public static void killPlayer(IPlayer player, bool doAnimation, string message, string shortMessage, bool permaDeathPossible = false) {
            var data = player.getCharacterData();
            var damg = data.CharacterDamage;

            if (data.PermadeathActivated && (permaDeathPossible || damg.canTriggerPermadeath())) {
                player.addState(Constants.PlayerStates.PermaDeath);

                message = "Du befindest dich in einem komatösen Zustand. Begib dich nach deiner Behandlung in den Support! (Permadeath)";
                shortMessage = "Permadeath aktiviert";

                player.setPermanentData("PLAYER_PERMA_DEAD", true.ToString());
            } else {
                player.setPermanentData("PLAYER_DEAD", true.ToString());
            }

            player.addState(Constants.PlayerStates.Dead);

            player.setPermanentData("DEATH_MOMENT", DateTime.Now.ToJson());

            player.sendNotification(NotifactionTypes.Danger, message, shortMessage, NotifactionImages.Bone);
            player.emitClientEvent("DISABLE_MOVEMENT", true);

            player.toggleInfiniteAir(true, false);
            player.toggleFireProof(true, false);

            damg.checkPainEffects(player, null);

            if (!player.IsInVehicle) {
                if (doAnimation) {
                    player.playAnimation("anim@scripted@data_leak@fixf_fin_ig2_johnnyguns_wounded@", "enter", 5000, 0, null);
                    InvokeController.AddTimedInvoke("Animation-Setter", (i) => {
                        player.forceAnimation("dead", "dead_b", -1, 1);
                    }, TimeSpan.FromSeconds(5), false);
                } else {
                    player.forceAnimation("dead", "dead_b", -1, 1);
                }
            } else {
                player.playAnimation("veh@mower@base", "die", -1, 18, null, 1);
            }

            player.setHealth(33, player.getHealth() > 33);

            BackendPlayerDeathDelegate?.Invoke(player);
        }

        public static void onPlayerDead(IPlayer player, IEntity killer, uint weapon) {
            if (Config.IsDamageSystemEnabled && !player.hasState(Constants.PlayerStates.InTerminal) && !Config.IsStressTestActive) {
                var tempPos = player.Position;
                player.emitClientEvent("SHOW_BLACK_WHITE_SCREEN");
                InvokeController.AddTimedInvoke("Player-Dead-kill", (i) => {
                    if (player.Position == tempPos) {
                        player.Spawn(player.Position);

                        if(new Random().Next(0, 1000) == 1) {
                            player.emitClientEvent("SHOW_WASTED_SCREEN", "~r~ Errungenschaft freigeschalten: \"Hast dich wohl wegkegeln lassen\"", "Du kannst nach 15min einen Dispatch senden", true);
                        } else {
                            player.emitClientEvent("SHOW_WASTED_SCREEN", "~r~ Du bist ohnmächtig", "Du kannst nach 15min einen Dispatch senden", true);
                        }

                        checkPermaDeathMessage(player, killer, weapon);
                        killPlayer(player, false, "Durch den starken Schmerz bist du Ohnmächtig geworden!", "Ohnmächtig geworden!", true);
                    } else {
                        onPlayerDead(player, killer, weapon);
                    }
                }, TimeSpan.FromMilliseconds(2000), false);
            } else {
                player.Spawn(player.Position, 1000);

                var damg = player.getCharacterData().CharacterDamage;
                foreach (var inj in damg.AllInjuries) {
                    damg.healInjury(player, inj);
                }
            }
        }

        private static void checkPermaDeathMessage(IPlayer player, IEntity killer, uint weapon) {
            var damageType = DamageType.Dull;

            if(WeaponIdToDamageType.ContainsKey(weapon)) {
                damageType = WeaponIdToDamageType[weapon];
            }

            if(killer is IPlayer && damageType == DamageType.Shot) {
                var currentDates = ((string)player.getData("PERMA_DEATH_DATES")).FromJson<List<DateTime>>() ?? new List<DateTime>();
                var filteredDates = currentDates.Where(d => d.Date >= DateTime.Now - TimeSpan.FromDays(7)).ToList();
                filteredDates.Add(DateTime.Now);
                player.setPermanentData("PERMA_DEATH_DATES", filteredDates.ToJson());

                if(filteredDates.Count >= 4) {
                    var fieldList = new List<DiscordController.DiscordEmbedField>();
                    filteredDates.ForEach(d => fieldList.Add(new DiscordController.DiscordEmbedField("Todesdatum:", d.ToString(), false)));

                    DiscordController.sendEmbedInChannel(
                        "Permadeath-Kandidat", 
                        $"Spieler {player.Name} hat in den letzten 7 Tagen 4 mal den Tod durch Schusswaffen erlitten. Bitte um Überprüfung",
                        null,
                        fieldList,
                        player
                    );

                }
            }
        }

        private void onPlayerDead(IPlayer player, character character, characterdatum data) {
            killPlayer(player, false, "Durch den starken Schmerz bist du Ohnmächtig geworden!", "Ohnmächtig geworden!");
        }

        private void onPlayerPermaDead(IPlayer player, character character, characterdatum data) {
            killPlayer(player, false, "Du bist in einem komatösen Zustand. Spiele das RP durch und melde dich im Support!", "Permadeath aktiviert", true);
        }

        public static void revivePlayer(IPlayer player) {
            if (player.hasState(Constants.PlayerStates.Dead) && !player.hasState(Constants.PlayerStates.PermaDeath)) {
                player.resetPermantData("PLAYER_DEAD");
                player.resetPermantData("PLAYER_PERMA_DEAD");
                player.resetData("DEATH_MOMENT");
                player.emitClientEvent("DISABLE_MOVEMENT", false);
                player.sendNotification(NotifactionTypes.Success, "Du spürst wieder Kraft in deinen Knochen!", "Wiederbelebt worden", NotifactionImages.Bone);
                player.sendNotification(NotifactionTypes.Success, "Du kannst die aktuelle Animation jederzeit beenden!", "Wiederbelebt worden", NotifactionImages.Bone);
                BackendPlayerReviveDelegate?.Invoke(player);
                player.removeState(Constants.PlayerStates.Dead);

                //If player has rebreather etc. on the data will be set
                if (!player.isInfiniteAirToggled()) {
                    player.toggleInfiniteAir(false, false);
                }

                //If player has Firesuit on etc. on the data will be set
                if (!player.isFireProofToggled()) {
                    player.toggleFireProof(false, false);
                }

                player.stopTimeCycle("DAMAGE");
                player.stopAdditionalTimeCycle("DAMAGE");

                player.getCharacterData().CharacterDamage.checkPainEffects(player, null);
                player.emitClientEvent("STOP_WASTED_SCREEN");
            }
        }      


        private bool onSendDispatchFromDead(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            ControlCenterController.createDispatch(DispatchType.NpcCallDispatch, "Verletze Person", "Es wurde eine verletzte Person gefunden", player.Position, true, false);
            
            player.sendNotification(NotifactionTypes.Success, "Dispatch wurde gesendet", "Dispatch gesendet", NotifactionImages.Bone);
            return true;
        }
    }
}
