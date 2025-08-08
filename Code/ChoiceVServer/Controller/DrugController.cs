using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
    //Wenn Abhängig und Schwellenwert überschritten: +1
    public abstract class Drug {
        public readonly string Identifier;
        public readonly string Name;
        public readonly float LevelPerDose;
        public readonly float RestiancePerDose;
        public readonly float DependencyPerDose;

        public readonly float LevelDropoffPerHour;
        public readonly float ResistanceDropoffPerHour;
        public readonly float DependencyDropoffPerHour;

        public Drug(string identifier, string name, float levelPerDose, float resitancePerDose, float dependencyPerDose, float levelDropoff, float resDropoff, float depDropoff) {
            Identifier = identifier;
            Name = name;

            LevelPerDose = levelPerDose;
            RestiancePerDose = resitancePerDose;
            DependencyPerDose = dependencyPerDose;

            LevelDropoffPerHour = levelDropoff;
            ResistanceDropoffPerHour = resDropoff;
            DependencyDropoffPerHour = depDropoff;
        }

        public void addDose(IPlayer player) {
            if(player.getCharacterData().DrugLevels.ContainsKey(Identifier)) {
                var drug = player.getCharacterData().DrugLevels[Identifier];

                var hourDifference = (DateTime.Now - drug.LastCheck).TotalHours;

                if(hourDifference > 1) {
                    hourDifference = 1;
                }

                drug.CurrentLevel += (float)(LevelPerDose * Math.Pow(Math.E, -drug.CurrentResitance) - LevelDropoffPerHour * hourDifference); //Caluclate new level depending on resistance
                drug.CurrentResitance += (float)(RestiancePerDose - ResistanceDropoffPerHour * hourDifference);
                drug.CurrentDependency += (float)(DependencyPerDose - DependencyDropoffPerHour * hourDifference);

            } else {
                var drug = new PlayerDrugLevel(Identifier, LevelPerDose, RestiancePerDose, DependencyPerDose, DateTime.Now);
                player.getCharacterData().DrugLevels.Add(Identifier, drug);
            }

            DrugController.updatePlayerDrugs(player);
        }

        public void update(IPlayer player) {
            foreach(var drug in player.getCharacterData().DrugLevels.Values) {
                if(drug.Identifier == Identifier) {
                    var hourDifference = (DateTime.Now - drug.LastCheck).TotalHours;

                    if(hourDifference > 1) {
                        hourDifference = 1;
                    }

                    drug.CurrentLevel -= (float)(LevelDropoffPerHour * hourDifference);
                    drug.CurrentResitance -= (float)(ResistanceDropoffPerHour * hourDifference);
                    drug.CurrentDependency -= (float)(DependencyDropoffPerHour * hourDifference);

                    drug.CurrentLevel = Math.Max(drug.CurrentLevel, 0);
                    drug.CurrentResitance = Math.Max(drug.CurrentResitance, 0);
                    drug.CurrentDependency = Math.Max(drug.CurrentDependency, 0);

                    drug.LastCheck = DateTime.Now;
                }
            }
        }

        public abstract void checkForEffect(IPlayer player);
    }

    public class PlayerDrugLevel {
        public string Identifier;
        public float CurrentLevel;
        public float CurrentResitance;
        public float CurrentDependency;
        public DateTime LastCheck;

        public PlayerDrugLevel(string identifier, float currentLevel, float currentResistance, float currentDependancy, DateTime lastCheck) {
            Identifier = identifier;
            CurrentLevel = currentLevel;
            CurrentResitance = currentResistance;
            CurrentDependency = currentDependancy;
            LastCheck = lastCheck;
        }
    }

    public class DrugController : ChoiceVScript {
        private static Dictionary<string, Drug> AllDrugs = new();

        //TODO Save data to database (periodiclly and on playerdisconnect)

        public DrugController() {
            EventController.PlayerPreSuccessfullConnectionDelegate += onPlayerConnect;

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnected;

            InvokeController.AddTimedInvoke("Drug-Updater", updateDrugs, TimeSpan.FromMinutes(1), true);
        }

        private void onPlayerDisconnected(IPlayer player, string reason) {
            updatePlayerDrugs(player);
        }

        private void updateDrugs(IInvoke obj) {
            using(var db = new ChoiceVDb()) {
                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    if(player.getCharacterFullyLoaded()) {
                        var charId = player.getCharacterId();
                        var dbDrugs = db.characterdrugs.Where(d => d.charId == charId).ToList();

                        foreach(var drug in player.getCharacterData().DrugLevels.Values) {
                            var cfgDrug = AllDrugs[drug.Identifier];

                            cfgDrug.update(player);
                            cfgDrug.checkForEffect(player);

                            var already = dbDrugs.FirstOrDefault(dd => dd.identifier == drug.Identifier);

                            if(already != null) {
                                if(drug.CurrentLevel == 0 && drug.CurrentResitance == 0 && drug.CurrentDependency == 0) {
                                    db.characterdrugs.Remove(already);
                                } else {
                                    already.currentLevel = drug.CurrentLevel;
                                    already.currentResistance = drug.CurrentResitance;
                                    already.currentDependency = drug.CurrentDependency;
                                    already.lastCheck = drug.LastCheck;
                                }
                            } else {
                                var dbDrug = new characterdrug {
                                    charId = charId,
                                    identifier = drug.Identifier,
                                    currentLevel = drug.CurrentLevel,
                                    currentResistance = drug.CurrentResitance,
                                    currentDependency = drug.CurrentDependency,
                                    lastCheck = drug.LastCheck,
                                };

                                db.characterdrugs.Add(dbDrug);
                            }
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        public static void updatePlayerDrugs(IPlayer player) {
            var charId = player.getCharacterId();

            if(!player.getCharacterFullyLoaded()) {
                return;
            }

            using(var db = new ChoiceVDb()) {
                var dbDrugs = db.characterdrugs.Where(d => d.charId == charId).ToList();

                foreach(var drug in player.getCharacterData().DrugLevels.Values) {
                    var already = dbDrugs.FirstOrDefault(dd => dd.identifier == drug.Identifier);

                    if(already != null) {
                        if(drug.CurrentLevel == 0 && drug.CurrentResitance == 0 && drug.CurrentDependency == 0) {
                            db.characterdrugs.Remove(already);
                        } else {
                            already.currentLevel = drug.CurrentLevel;
                            already.currentResistance = drug.CurrentResitance;
                            already.currentDependency = drug.CurrentDependency;
                            already.lastCheck = drug.LastCheck;
                        }
                    } else {
                        var dbDrug = new characterdrug {
                            charId = charId,
                            identifier = drug.Identifier,
                            currentLevel = drug.CurrentLevel,
                            currentResistance = drug.CurrentResitance,
                            currentDependency = drug.CurrentDependency,
                            lastCheck = drug.LastCheck,
                        };

                        db.characterdrugs.Add(dbDrug);
                    }
                }

                db.SaveChanges();
            }
        }

        private void onPlayerConnect(IPlayer player, character character) {
            var data = player.getCharacterData();

            foreach(var dbLevel in character.characterdrugs) {
                data.DrugLevels.Add(dbLevel.identifier, new PlayerDrugLevel(dbLevel.identifier, dbLevel.currentLevel, dbLevel.currentResistance, dbLevel.currentDependency, dbLevel.lastCheck));
            }
        }

        public static void addDrug(Drug drug) {
            AllDrugs.Add(drug.Identifier, drug);
        }

        public static Drug getDrugByIdentifier(string identifier) {
            if(AllDrugs.ContainsKey(identifier)) {
                return AllDrugs[identifier];
            } else {
                return null;
            }
        }

        public static List<T> getDrugsByPredicate<T>(Predicate<T> predicate) {
            return AllDrugs.Values.Where(d => d is T t && predicate(t)).Cast<T>().ToList();
        }
    }
}
