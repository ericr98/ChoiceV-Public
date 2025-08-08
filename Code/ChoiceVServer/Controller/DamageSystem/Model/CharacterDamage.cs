using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.DamageSystem.Model {
    public class CharacterDamage {
        public Dictionary<CharacterBodyPart, List<Injury>> AllInjuriesByPart = new Dictionary<CharacterBodyPart, List<Injury>>();
        public List<Injury> AllInjuries { get => AllInjuriesByPart.SelectMany(ibp => ibp.Value).ToList(); }

        public CharacterDamage(List<characterinjury> allInjuries) {
            foreach (CharacterBodyPart part in Enum.GetValues(typeof(CharacterBodyPart))) {
                AllInjuriesByPart[part] = new List<Injury>();
            }

            foreach (var inj in allInjuries) {
                addInjurySimple(inj.id, (CharacterBodyPart)inj.bodyPart, (DamageType)inj.damageType, inj.damage, inj.wastedPain, inj.seed, inj.createDate, inj.isHealed == 1, inj.isMakeShiftTreated == 1, inj.configInjuryNavigation);
            }
        }

        private void addInjurySimple(int id, CharacterBodyPart bodyPart, DamageType type, float damage, float wastedPain, int seed, DateTime createDate, bool isHealing = false, bool isMakeshiftTreated = false, configinjury diagnosedInjury = null) {
            var newInj = new Injury(id, bodyPart, type, damage, wastedPain, seed, createDate, isHealing, isMakeshiftTreated, diagnosedInjury);
            var list = AllInjuriesByPart[bodyPart];
            list.Add(newInj);
        }

        private void healInjurySimple(Injury inj) {
            var list = AllInjuriesByPart[inj.BodyPart];
            if (list.Contains(inj)) {
                list.Remove(inj);
            }
        }

        public float getPainLevel() {
            var count = 0f;

            foreach (var injury in AllInjuries) {
                count += injury.Damage;
            }

            return count;
        }

        /// <summary>
        /// Returns if the Damage Data contains any Shot or Stab injuries
        /// </summary>
        /// <param name="notTreated">if set to true, only non healing/makeshifttreated injuries will be counted</param>
        /// <returns></returns>
        public bool hasShotOrStingInjury(bool notTreated) {
            return AllInjuries.Any(i => (i.Type == DamageType.Shot || i.Type == DamageType.Sting) && !(!notTreated ^ (i.IsHealing || i.IsMakeShiftTreated)));
        }

        public bool hasVerySevereInjury() {
            return AllInjuries.Any(i => !i.IsHealing && i.getSevernessLevel() >= 5);
        }

        private int getSevernessLevelWithCombinedPain(float painLevel) {
            if (painLevel > 100) {
                painLevel = 100;
            }

            return (int)Math.Ceiling(painLevel.Map(0, 100, 0, 6));
        }

        public float getSevernessLevel(CharacterBodyPart bodyPart, DamageType type) {
            var count = 0f;
            var list = AllInjuriesByPart[bodyPart];
            foreach (var inj in list.Where(i => i.Type == type).ToList()) {
                count += inj.Damage;
            }

            return count;
        }

        public void addInjury(IPlayer player, DamageType type, int bone, int damage) {
            addInjury(player, type, BoneToCharacterBodyPart[bone], damage);
        }

        public void addInjury(IPlayer player, DamageType type, CharacterBodyPart bodyPart, int damage) {
            if (damage > 100) {
                damage = 100;
            }

            var rand = new Random();
            var list = AllInjuriesByPart[bodyPart];

            // Dull injuries are not that bad
            if (type == DamageType.Dull) {
                damage = rand.Next(damage / 3, damage / 2);
            }

            var injuriesWithType = list.Where(i => i.Type == type && !i.IsHealing && !i.IsTreated && i.DiagnosedInjury == null).ToList();
            if (((damage <= 5 && rand.NextDouble() >= 0.9) || rand.NextDouble() <= 0.25) && injuriesWithType.Count != 0) {
                var index = rand.Next(0, injuriesWithType.Count - 1);
                var inj = injuriesWithType[index];
                inj.Damage += damage;
                if (inj.Damage >= 100) {
                    inj.Damage = 100;
                }

                using (var db = new ChoiceVDb()) {
                    var row = db.characterinjuries.Find(inj.Id);
                    if (row != null) {
                        row.damage = inj.Damage;

                        db.SaveChanges();
                    }
                }
                return;
            }

            var seed = new Random().Next(100000, 999999);
            using (var db = new ChoiceVDb()) {
                var inj = new characterinjury {
                    charId = player.getCharacterId(),
                    bodyPart = (int)bodyPart,
                    damage = damage,
                    wastedPain = 0,
                    damageType = (int)type,
                    seed = seed,
                    createDate = DateTime.Now,
                    retreatPossible = DateTime.MinValue,
                };

                db.characterinjuries.Add(inj);

                db.SaveChanges();

                addInjurySimple(inj.id, bodyPart, type, damage, 0, seed, DateTime.Now);
                checkPainEffects(player, null);
            }
        }

        public void healInjury(IPlayer player, Injury injury) {
            healInjurySimple(injury);
            checkPainEffects(player, null);

            using (var db = new ChoiceVDb()) {
                var row = db.characterinjuries.Find(injury.Id);
                if (row != null) {
                    db.characterinjuries.Remove(row);
                } else {
                    Logger.logError($"healInjury: Injury not found!",
                          $"Fehler bei Verletzungsheilung: Verletzung {injury.BodyPart} {injury.Type} {injury.Damage} wurde nicht gefunden", player);
                }

                db.SaveChanges();
            }
        }

        public Injury findInjuryById(int id) {
            return AllInjuries.FirstOrDefault(i => i.Id == id);
        }

        public List<Injury> getInjuriesOfPart(CharacterBodyPart part) {
            return AllInjuriesByPart[part];
        }


        public record class PainInfo(float PainLevel, float WastedPainLevel, float SevernessLevel, CharacterBodyPart MainlyDamagedPart);

        public PainInfo getPainInfo() {
            var retBod = CharacterBodyPart.None;

            var max = 0f;
            var retPain = 0f;
            var wastedPain = 0f;

            foreach (var pair in AllInjuriesByPart) {
                var painLevel = 0f;
                foreach (var inj in pair.Value) {
                    painLevel += inj.Damage;
                    retPain += inj.Damage;
                    wastedPain += inj.WastedPainLevel;
                }

                if (painLevel > max) {
                    retBod = pair.Key;
                }
            }

            return new PainInfo(retPain, wastedPain, getSevernessLevelWithCombinedPain(retPain), retBod);
        }

        public float getWastedPain() {
            var wastedPain = 0f;

            foreach (var pair in AllInjuriesByPart) {
                foreach (var inj in pair.Value) {
                    wastedPain += inj.WastedPainLevel;
                }
            }

            return wastedPain;
        }

        public void checkPainEffects(IPlayer player, int? medPainLevel) {
            var painInfo = getPainInfo();
            var rand = new Random();

            if (!player.hasState(PlayerStates.Dead)) {
                if (medPainLevel == null) {
                    medPainLevel = player.getMedicatedPainLevel();
                }

                if (painInfo.SevernessLevel > 2 && painInfo.SevernessLevel > medPainLevel) {
                    player.setAdditionalTimeCycle("DAMAGE", "damage");

                    player.emitClientEvent("SET_PLAYER_INJURED", false, 1);
                    player.emitClientEvent("SET_PLAYER_INJURED", false, 2);
                    player.emitClientEvent("STOP_PULSE_EFFECT");

                    if (painInfo.SevernessLevel <= 4) {
                        if (painInfo.WastedPainLevel < 75) {
                            player.setTimeCycle("DAMAGE", "ufo", Math.Min(painInfo.PainLevel, 60).Map(30, 60, 0.4f, 0.5f));
                        } else {
                            player.setTimeCycle("DAMAGE", "blackout", painInfo.WastedPainLevel.Map(75, 100, 0.3f, 1));
                        }

                        if (rand.Next() > 0.1 * painInfo.SevernessLevel) {
                            EffectController.playScreenEffect(player, new ScreenEffect("BeastLaunch", TimeSpan.FromSeconds(0.5), false));
                        } else if (rand.Next() > 0.1 * painInfo.SevernessLevel) {
                            EffectController.playScreenEffect(player, new ScreenEffect("CamPushInTrevor", TimeSpan.FromSeconds(4), false));
                        }
                    } else {
                        if (painInfo.MainlyDamagedPart == CharacterBodyPart.Torso || painInfo.MainlyDamagedPart == CharacterBodyPart.LeftArm || painInfo.MainlyDamagedPart == CharacterBodyPart.RightArm) {
                            player.emitClientEvent("SET_PLAYER_INJURED", true, 2);
                        }

                        player.emitClientEvent("SET_PLAYER_INJURED", true, 1);

                        if (painInfo.WastedPainLevel < 75) {
                            player.emitClientEvent("START_PULSE_EFFECT", 3000 * (7 - painInfo.SevernessLevel), 2 * (7 - painInfo.SevernessLevel));
                        } else {
                            player.setTimeCycle("DAMAGE", "blackout", painInfo.WastedPainLevel.Map(75, 100, 0.3f, 1));
                        }
                    }
                } else {
                    player.stopTimeCycle("DAMAGE");
                    player.stopAdditionalTimeCycle("DAMAGE");

                    player.emitClientEvent("STOP_PULSE_EFFECT");
                    player.emitClientEvent("SET_PLAYER_INJURED", false, 1);
                    player.emitClientEvent("SET_PLAYER_INJURED", false, 2);
                }
            } else {
                player.setAdditionalTimeCycle("DAMAGE", "damage");

                if (painInfo.WastedPainLevel >= 100) {
                    player.setTimeCycle("DAMAGE", "MP_death_grade_blend01", 1);
                    player.emitClientEvent("START_PULSE_EFFECT", 2000, 100000);
                } else {
                    //Only temporary knockdown
                    player.setTimeCycle("DAMAGE", "MP_death_grade_blend01", 1);
                }
            }
        }

        public void saveDamagesToDb(ChoiceVDb db) {
            foreach (var inj in AllInjuries) {
                var dbInj = db.characterinjuries.Find(inj.Id);

                if (dbInj != null) {
                    dbInj.damage = inj.Damage;
                    dbInj.wastedPain = inj.WastedPainLevel;
                    dbInj.isHealed = inj.IsHealing ? 1 : 0;
                    dbInj.isMakeShiftTreated = inj.IsMakeShiftTreated ? 1 : 0;
                    //dbInj.isTreated = inj.IsTreated ? 1 : 0;
                }
            }
        }

        public bool canTriggerPermadeath() {
            var damgCounter = 0f;

            var allInjs = AllInjuries;
            var lastInj = allInjs.Aggregate((i1, i2) => i1.CreateDate > i2.CreateDate ? i1 : i2);

            if (lastInj != null && (lastInj.Type == DamageType.Shot || lastInj.Type == DamageType.Sting)) {
                return true;
            }

            foreach (var inj in allInjs) {
                if (!inj.IsHealing && (inj.Type == DamageType.Shot || inj.Type == DamageType.Sting)) {
                    damgCounter += inj.Damage;
                }
            }

            return damgCounter > 60;
        }
    }
}
