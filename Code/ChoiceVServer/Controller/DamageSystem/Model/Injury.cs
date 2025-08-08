using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.DamageSystem.Model {
    public class Injury {
        public int Id { get; private set; }
        public float Damage { get; set; }
        public CharacterBodyPart BodyPart { get; private set; }
        public DamageType Type { get; private set; }
        public int Seed { get; private set; }
        public DateTime CreateDate { get; private set; }

        public bool IsTreated { get; set; }
        public bool IsHealing { get; set; }

        [JsonIgnore]
        public configinjury DiagnosedInjury { get; private set; }
        public List<int> OperatedOrder { get; set; }


        public bool IsMakeShiftTreated;

        public float WastedPainLevel;

        public Injury(int id, CharacterBodyPart bodyPart, DamageType type, float damage, float wastedPain, int seed, DateTime createDate, bool isHealing = false, bool isMakeShiftTreated = false, configinjury diagnosedInjury = null) {
            Id = id;
            BodyPart = bodyPart;
            Damage = damage;
            WastedPainLevel = wastedPain;

            Type = type;
            Seed = seed;

            CreateDate = createDate;

            IsHealing = isHealing;
            IsMakeShiftTreated = isMakeShiftTreated;
            DiagnosedInjury = diagnosedInjury;
            OperatedOrder = new List<int>();
        }

        public int getSevernessLevel() {
            return Math.Min((int)Math.Round(((float)Damage).Map(0, 70, 1, 6)), 6);
        }

        public int getShowSevernessLevel() {
            if(IsTreated) {
                return -1;
            } else if(IsHealing) {
                return 0;
            } else {
                return Math.Min((int)Math.Round(((float)Damage).Map(0, 70, 1, 6)), 6);
            }
        }

        public void getMessage(ref string type, ref string strength) {
            switch(Type) {
                case DamageType.Dull:
                    type = "stumpfe Verletzung";
                    break;
                case DamageType.Shot:
                    type = "Schussverletzung";
                    break;
                case DamageType.Sting:
                    type = "Stichverletzung";
                    break;
                case DamageType.Inflammation:
                    type = "Entzündung";
                    break;
                case DamageType.Burning:
                    type = "Verbrennung";
                    break;
            }

            switch(getSevernessLevel()) {
                case 0:
                    strength = "unbeachtliche";
                    break;
                case 1:
                    strength = "geringe";
                    break;
                case 2:
                    strength = "leichte";
                    break;
                case 3:
                    strength = "mittlere";
                    break;
                case 4:
                    strength = "schwere";
                    break;
                case 5:
                    strength = "sehr schwere";
                    break;
                case 6:
                    strength = "gefährliche";
                    break;
            }
        }

        public string diagnoseInjury(bool specialEquip, out string shortString) {
            if(DiagnosedInjury == null) {
                if(specialEquip || Config.IsDevServer) {
                    var diag = generateInjury();
                    if(diag != null) {
                        shortString = $"{DiagnosedInjury.name}";
                        return $"Die Person hat einen/eine {DiagnosedInjury.name}";
                    } else {
                        Logger.logError($"diagnoseInjury: no viable injury found for: bodyPart: {BodyPart}, damageType: {Type}",
                          $"Fehler bei Verletzungsdiagnose: Verletzung {BodyPart} {Type} {Damage} wurde nicht gefunden");

                        shortString = "Fehler aufgetreten!";
                        return "FEHLER AUFGETRETEN! Melde dich bitte beim Dev Team! Code: PainWorm";
                    }
                } else {
                    shortString = $"Falsches Equipment!";
                    return "Es wird besseres Equipment für weitere Analysen benötigt!";
                }
            } else {
                shortString = $"{DiagnosedInjury.name}";
                return $"Die Person hat einen/eine {DiagnosedInjury.name}";
            }
        }

        public configinjury generateInjury() {
            var painLevel = getSevernessLevel();
            using(var db = new ChoiceVDb()) {
                var dbInj = db.characterinjuries.Find(Id);
                if(dbInj != null) {
                    var viable = db.configinjuries
                        .Include(i => i.treatmentCategoryNavigation)
                        .ThenInclude(i => i.configinjurytreatmentssteps)
                        .Where(i => (getCategoriesForBodyPartStrings(BodyPart).Contains(i.bodyPart) || i.bodyPart == BodyPart.ToString()) && i.damageType == Type.ToString() && i.minSeverness <= painLevel && i.maxSeverness >= painLevel).ToList();

                    if(viable.Count > 0) {
                        var idx = new Random().Next(0, viable.Count - 1);
                        var diag = viable[idx];
                        DiagnosedInjury = diag;
                        dbInj.configInjury = diag.id;
                        db.SaveChanges();
                        return diag;
                    }
                }
            }

            return null;
        }

        public bool checkForRightTreatment() {
            var rightItemOrder = DiagnosedInjury.treatmentCategoryNavigation.configinjurytreatmentssteps.Select(i => i.itemId).ToList();
            if(OperatedOrder.containsSequence(rightItemOrder) && !IsHealing) {
                //var newVal = (int)Math.Round(Damage * 0.5);
                //if(newVal <= 30) {
                //    Damage = newVal;
                //} else {
                //    Damage = 30;
                //}

                IsHealing = true;
                WastedPainLevel = 0;

                using(var db = new ChoiceVDb()) {
                    var inj = db.characterinjuries.Find(Id);
                    if(inj != null) {
                        inj.isHealed = 1;
                        db.SaveChanges();
                    } else {
                        Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"checkForRightTreatment: Injury in not found");
                    }
                }
                return true;
            } else {
                return false;
            }
        }

        private bool isNaturallyHealing(int painLevel) {
            return (Type == DamageType.Dull && painLevel <= 3) || (Type == DamageType.Inflammation && painLevel <= 2) || Type == DamageType.Burning || (Type == DamageType.Sting && painLevel <= 2);
        }

        public bool needsToolDiagnose() {
            return getSevernessLevel() >= 4;
        }

        /// <summary>
        /// Checks the healing Process
        /// </summary>
        /// <returns>Returns current Wastedpainlevel</returns>
        public float updateRecovery(TimeSpan tickTime, int medPainLevel, bool canOnlyGetBetter) {
            var painLevel = getSevernessLevel();

            var naturallyHealing = isNaturallyHealing(painLevel);
            var tickDamage = 0f;

            //Every 10 minutes 1 damage is removed / added tick
            var baseDamageTick = getBaseTickDamage(tickTime);

            //Natural healing modifier of 1 / Painlevel
            if(naturallyHealing) {
                tickDamage = -baseDamageTick / painLevel;

                //If the wound is makeshift or real treated, it heals even faster
                if(IsMakeShiftTreated || IsTreated) {
                    tickDamage = -baseDamageTick / painLevel;
                }
            }

            if(IsHealing) {
                if(painLevel == 1) {
                    tickDamage -= baseDamageTick;
                    //Painlevel 2,3,4
                } else if(painLevel <= 3) {
                    //Painlevel 2, 3 and 4 injuries only 1 / 5 of the speed
                    tickDamage -= baseDamageTick / 5;

                    //Painlevel 5,6,7
                } else {
                    //Painlevel 2, 3 and 4 injuries only 1 / 2.5 of the speed
                    tickDamage -= baseDamageTick / 2.5f;
                }

                //If pain medication is sufficient, wasted pain does not rise!
            } else if(!naturallyHealing) {
                //If the injury is makeshift treated, the damage stuff halfs.
                //Additionally the WastedPainLevel rises significantly slower

                if(!canOnlyGetBetter || tickDamage > 0) {
                    if(IsMakeShiftTreated) {
                        tickDamage += baseDamageTick / 2;
                        if(medPainLevel < painLevel) {
                            WastedPainLevel += painLevel * baseDamageTick / 2;
                        }
                    } else {
                        tickDamage += baseDamageTick;
                        if(medPainLevel < painLevel) {
                            WastedPainLevel += painLevel * baseDamageTick;
                        }
                    }
                }
            }

            if(IsHealing || naturallyHealing) {
                WastedPainLevel -= painLevel * baseDamageTick;
                if(WastedPainLevel < 0) {
                    WastedPainLevel = 0;
                }
            }

            if(!canOnlyGetBetter || tickDamage > 0) {
                Damage += tickDamage;
            }

            return WastedPainLevel;
        }

        private float getBaseTickDamage(TimeSpan tickTime) {
            return (float)(6d / tickTime.TotalSeconds);
        }

        public float updateRecoveryWhileDead(TimeSpan tickTime) {
            var painLevel = getSevernessLevel();
            var baseDamageTick = getBaseTickDamage(tickTime);

            WastedPainLevel -= (7 - painLevel) * baseDamageTick / 3;
            if(WastedPainLevel < 0) {
                WastedPainLevel = 0;
            }

            return WastedPainLevel;
        }

        public float checkMoving(float moveDistance) {
            if(BodyPart is CharacterBodyPart.LeftLeg or CharacterBodyPart.RightLeg or CharacterBodyPart.Torso && getSevernessLevel() > 2) {
                //Torso injuries not that bad
                return moveDistance / 100f / (BodyPart == CharacterBodyPart.Torso ? 3 : 1);
            } else {
                return 0;
            }
        }

        public float checkArmPressure(float pressure) {
            if(BodyPart is CharacterBodyPart.LeftArm or CharacterBodyPart.RightArm or CharacterBodyPart.Torso && getSevernessLevel() > 2) {
                return pressure / (BodyPart == CharacterBodyPart.Torso ? 3 : 1);
            } else {
                return 0;
            }
        }
    }
}
