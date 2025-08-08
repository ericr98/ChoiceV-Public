using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class PlayerCrimeMissions {
        public List<int> SelectSelection;
        public int? RandomMission;
        public List<int> CrimeSpreeMission;

        public CrimeMissionProgress SelectProgress;
        public CrimeMissionProgress RandomProgress;
        public List<CrimeMissionProgress> CrimeSpreeProgress;

        public DateTime SelectStart;
        public DateTime RandomStart;
        public DateTime CrimeSpreeStart;

        public PlayerCrimeMissions(charactercrimemissiontrigger db) {
            SelectSelection = db.selectSelection?.FromJson<List<int>>();
            RandomMission = db.randomMission;
            CrimeSpreeMission = db.crimeSpreeMissions?.FromJson<List<int>>();

            SelectProgress = db.selectProgress.FromJson<CrimeMissionProgress>();
            RandomProgress = db.randomProgress.FromJson<CrimeMissionProgress>();
            CrimeSpreeProgress = db.crimeSpreeProgress.FromJson<List<CrimeMissionProgress>>();

            SelectStart = db.selectStart;
            RandomStart = db.randomStart;
            CrimeSpreeStart = db.crimeSpreeStart;
        }

        public PlayerCrimeMissions() {
            SelectSelection = null;
            RandomMission = null;
            CrimeSpreeMission = null;

            SelectProgress = new();
            RandomProgress = new();
            CrimeSpreeProgress = [];

            SelectStart = DateTime.MinValue;
            RandomStart = DateTime.MinValue;
            CrimeSpreeStart = DateTime.MinValue;
        }

        public PlayerCrimeMissions(List<int> selectSelection, int? randomMission, List<int> crimeSpreeMission, CrimeMissionProgress selectProgress, CrimeMissionProgress randomProgress, List<CrimeMissionProgress> crimeSpreeProgress, DateTime selectStart, DateTime randomStart, DateTime crimeSpreeStart) {
            SelectSelection = selectSelection;
            RandomMission = randomMission;
            CrimeSpreeMission = crimeSpreeMission;

            SelectProgress = selectProgress;
            RandomProgress = randomProgress;
            CrimeSpreeProgress = crimeSpreeProgress;

            SelectStart = selectStart;
            RandomStart = randomStart;
            CrimeSpreeStart = crimeSpreeStart;
        }

        public void updateDb(IPlayer player) {
            using(var db = new ChoiceVDb()) {
                var dbTrigger = db.charactercrimemissiontriggers.Find(player.getCharacterId());

                if(dbTrigger != null) {
                    dbTrigger.selectSelection = SelectSelection.ToJson();
                    dbTrigger.randomMission = RandomMission;
                    dbTrigger.crimeSpreeMissions = CrimeSpreeMission.ToJson();

                    dbTrigger.selectProgress = SelectProgress.ToJson();
                    dbTrigger.randomProgress = RandomProgress.ToJson();
                    dbTrigger.crimeSpreeProgress = CrimeSpreeProgress.ToJson();

                    dbTrigger.selectStart = SelectStart;
                    dbTrigger.randomStart = RandomStart;
                    dbTrigger.crimeSpreeStart = CrimeSpreeStart;
                    db.SaveChanges();
                } else {
                    createNewDbTrigger(player);
                }
            }
        }

        public void updateSelectionDb(IPlayer player) {
            using(var db = new ChoiceVDb()) {
                var dbTrigger = db.charactercrimemissiontriggers.Find(player.getCharacterId());

                if(dbTrigger != null) {
                    dbTrigger.selectSelection = SelectSelection.ToJson();
                    dbTrigger.randomMission = RandomMission;
                    dbTrigger.crimeSpreeMissions = CrimeSpreeMission.ToJson();

                    db.SaveChanges();
                } else {
                    createNewDbTrigger(player);
                }
            }
        }

        public void updateProgressDb(IPlayer player) {
            using(var db = new ChoiceVDb()) {
                var dbTrigger = db.charactercrimemissiontriggers.Find(player.getCharacterId());

                if(dbTrigger != null) {
                    dbTrigger.selectProgress = SelectProgress.ToJson();
                    dbTrigger.randomProgress = RandomProgress.ToJson();
                    dbTrigger.crimeSpreeProgress = CrimeSpreeProgress.ToJson();

                    db.SaveChanges();
                } else {
                    createNewDbTrigger(player);
                }
            }
        }

        public void updateStartDb(IPlayer player) {
            using(var db = new ChoiceVDb()) {
                var dbTrigger = db.charactercrimemissiontriggers.Find(player.getCharacterId());

                if(dbTrigger != null) {
                    dbTrigger.selectStart = SelectStart;
                    dbTrigger.randomStart = RandomStart;
                    dbTrigger.crimeSpreeStart = CrimeSpreeStart;

                    db.SaveChanges();
                } else {
                    createNewDbTrigger(player);
                }
            }
        }

        private void createNewDbTrigger(IPlayer player) {
            using(var db = new ChoiceVDb()) {
                var newTrigger = new charactercrimemissiontrigger {
                    charId = player.getCharacterId(),

                    selectSelection = SelectSelection.ToJson(),
                    randomMission = RandomMission,
                    crimeSpreeMissions = CrimeSpreeMission.ToJson(),

                    selectProgress = SelectProgress.ToJson(),
                    randomProgress = RandomProgress.ToJson(),
                    crimeSpreeProgress = CrimeSpreeProgress.ToJson(),

                    selectStart = SelectStart,
                    randomStart = RandomStart,
                    crimeSpreeStart = CrimeSpreeStart,
                };

                db.charactercrimemissiontriggers.Add(newTrigger);
                db.SaveChanges();
            }
        }
    }
}