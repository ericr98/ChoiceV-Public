using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class CrimeMissionModule : CrimeNetworkModule {
        public CrimeMissionModule(ChoiceVPed ped, CrimeNetworkPillar pillar) : base(ped, pillar, "Aufträge", "MISSIONS") {

        }

        public CrimeMissionModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, CrimeNetworkController.getPillarById((int)settings["Mission_CrimePillar"]), "Aufträge", "MISSIONS") {

        }

        public override List<MenuItem> getCrimeMenuItems(IPlayer player) {
            if (!Active) {
                return [];
            }

            var tokens = (CrimeMissionTokens)player.getData("CRIME_MISSION_TOKENS");
            var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");

            var rep = player.getCrimeReputation();

            var menu = new Menu($"{Pillar.Name} Aufträge", "Was möchtest du tun?");

            //Random Missions
            if (currentMissions.RandomMission != null && currentMissions.RandomProgress.Status is CrimeMissionProgressStatus.Completed or CrimeMissionProgressStatus.CompletedLate) {
                var finishMissionPillar = CrimeMissionsController.getMissionTrigger(currentMissions.RandomMission ?? -1)?.Pillar;
                
                if(finishMissionPillar == null || finishMissionPillar == Pillar) {
                    var finishMenu = getMissionRewardMenu(currentMissions.RandomMission ?? -1, currentMissions, rep, "Zufälligen Auftrag abgeben", "RANDOM");
                    menu.addMenuItem(new MenuMenuItem(finishMenu.Name, finishMenu, MenuItemStyle.green));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Aktueller Auftrag auf falscher Säule!", "Der aktuell aktive Auftrag ist von einer anderen Säule. Du kannst ihn hier nur ersetzen!", "", MenuItemStyle.yellow));
                } 
            } else {
                var rndMenu = new Menu("Zufälliger Auftrag", "Bearbeite einen zufälligen Auftrag");
                var randomMissionPillar = CrimeMissionsController.getMissionTrigger(currentMissions.RandomMission ?? -1)?.Pillar;

                if (randomMissionPillar == null || randomMissionPillar == Pillar) {
                    if (currentMissions.RandomMission != null) {
                        var currentMission = CrimeMissionsController.getMissionTrigger(currentMissions.RandomMission ?? -1);
                        if(currentMission != null) {
                            var currentMenu = currentMission.getMenuRepresentative(player, currentMissions.RandomStart, currentMissions.RandomProgress);
                            rndMenu.addMenuItem(new MenuMenuItem(currentMenu.Name, currentMenu));
                        }
                    }
                } else {
                    rndMenu.addMenuItem(new StaticMenuItem("Aktueller Auftrag auf falscher Säule!", "Der aktuell aktive Auftrag ist von einer anderen Säule. Du kannst ihn hier nur ersetzen!", "", MenuItemStyle.yellow));
                }

                if (tokens.RandomToken >= 1) {
                    rndMenu.addMenuItem(new ClickMenuItem("Neuen Auftrag erhalten", "Ersetze deinen aktuellen Auftrag mit einem neuen.", "", "CRIME_SELECT_RANDOM_MISSION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Module", this } }).needsConfirmation("Neuen Auftrag anfordern?", "Wirklich anfordern?"));
                } else {
                    rndMenu.addMenuItem(new StaticMenuItem("Neuer Auftrag nicht verfügbar", "Du kannst aktuell keine neuen zufälligen Aufträge erhalten. Komme später wieder!", "", MenuItemStyle.yellow));
                }
                menu.addMenuItem(new MenuMenuItem(rndMenu.Name, rndMenu));
            }


            //Select Missions
            if (rep.isReputationHighEnough(Pillar, "SELECT_MISSION")) {
                if (currentMissions.SelectSelection != null && currentMissions.SelectProgress.Status is CrimeMissionProgressStatus.Completed or CrimeMissionProgressStatus.CompletedLate) {
                    var finishMissionPillar = CrimeMissionsController.getMissionTrigger(currentMissions.SelectSelection.First())?.Pillar;
                    if(finishMissionPillar == null || finishMissionPillar == Pillar) {
                        var finishMenu = getMissionRewardMenu(currentMissions.SelectSelection.First(), currentMissions, rep, "Wahlauftrag abgeben", "SELECT");
                        menu.addMenuItem(new MenuMenuItem(finishMenu.Name, finishMenu, MenuItemStyle.green));
                    } else {
                        menu.addMenuItem(new StaticMenuItem("Aktueller Auftrag auf falscher Säule!", "Der aktuell aktive Auftrag ist von einer anderen Säule. Du kannst ihn hier nur ersetzen!", "", MenuItemStyle.yellow));
                    }
                } else {
                    var selectMenu = new Menu("Wahlauftrag", "Wähle einen Auftrag aus");

                    var selectMissionPillar = CrimeMissionsController.getMissionTrigger(currentMissions.SelectSelection?.FirstOrDefault() ?? -1)?.Pillar;
                    if (selectMissionPillar == null || selectMissionPillar == Pillar) {
                        if (currentMissions.SelectSelection != null) {
                            if (currentMissions.SelectSelection.Count == 1) {
                                var currentMission = CrimeMissionsController.getMissionTrigger(currentMissions.SelectSelection.First());
                                var currentMenu = currentMission.getMenuRepresentative(player, currentMissions.SelectStart, currentMissions.SelectProgress);
                                selectMenu.addMenuItem(new MenuMenuItem(currentMenu.Name, currentMenu));
                            } else {
                                var subMenu = new Menu("Auftrag wählen", "Wähle einen der beschriebenen Aufträge");

                                var count = 0;
                                foreach (var option in currentMissions.SelectSelection) {
                                    count++;
                                    var missionOption = CrimeMissionsController.getMissionTrigger(option);
                                    var optionMenu = missionOption.getMenuRepresentative(player, currentMissions.SelectStart, currentMissions.SelectProgress);
                                    optionMenu.addMenuItem(new ClickMenuItem("Diesen Auftrag auswählen", "Wähle den selektierten Auftrag aus", "", "CRIME_SELECT_SELECT_OPTION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Module", this }, { "Mission", missionOption } }).needsConfirmation("Diesen Auftrag wählen?", "Den selektierten Auftrag wählen?"));
                                    subMenu.addMenuItem(new MenuMenuItem($"Option: {count}", optionMenu));
                                }
                                selectMenu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                            }
                        } else {
                            selectMenu.addMenuItem(new StaticMenuItem("Aktueller Auftrag auf falscher Säule!", "Der aktuell aktive Auftrag ist von einer anderen Säule. Du kannst ihn hier nur ersetzen!", "", MenuItemStyle.yellow));
                        }
                    }

                    if (tokens.SelectToken >= 1) {
                        var viableMissions = CrimeMissionsController.getAllMissions().Where(t => getMissionViable(rep, t.Pillar, t.getPillarReputationName())).ToList();
                        var options = viableMissions.Select(c => CrimeNetworkController.getNameForCrimeAction(c.Type)).ToArray();

                        selectMenu.addMenuItem(new ListMenuItem("Neuen Auftrag erhalten", "Ersetze deine aktuellen Aufträge mit einem Set aus neuen. Wähle danach einen aus.", options, "CRIME_SELECT_GET_NEW_MISSIONS", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Module", this } }).needsConfirmation("Neue Auftrag anfordern?", "Aufträge von dem Typen anfordern?"));
                    } else {
                        selectMenu.addMenuItem(new StaticMenuItem("Neuer Auftrag nicht verfügbar", "Du kannst aktuell keine neuen Wahlaufträge erhalten. Komme später wieder!", "", MenuItemStyle.yellow));
                    }

                    menu.addMenuItem(new MenuMenuItem(selectMenu.Name, selectMenu));
                }
            }


            //Crime Spree Missions
            if(rep.isReputationHighEnough(Pillar, "CRIME_SPREE_MISSIONS")) {
                if(currentMissions.CrimeSpreeMission != null && currentMissions.CrimeSpreeMission.Count > 0 && currentMissions.CrimeSpreeProgress.All(p => p.Status is CrimeMissionProgressStatus.Completed or CrimeMissionProgressStatus.CompletedLate)) {
                    var finishMissionPillar = CrimeMissionsController.getMissionTrigger(currentMissions.CrimeSpreeMission.FirstOrDefault())?.Pillar;

                    if(finishMissionPillar == null || finishMissionPillar == Pillar) {
                        var finishMenu = new Menu($"Verbrechensserie", "Wähle die Belohnungsoption");
                        var restTime = currentMissions.CrimeSpreeStart + TimeSpan.FromHours(0.75 * currentMissions.CrimeSpreeMission.Count) - DateTime.Now;

                        var mult = 1.25m;
                        if(restTime.TotalMilliseconds < 0) {
                            mult = 0.75m;
                        }

                        var cash = 0m;
                        currentMissions.CrimeSpreeMission.ForEach(c => cash += CrimeMissionsController.getMissionTrigger(c).CashReward);

                        cash = Math.Round(cash * mult, 2);
                        finishMenu.addMenuItem(new ClickMenuItem("Cash bevorzugen", $"Die Belohnung auf Cash fokussieren. Du erhälst ${cash}, einige Reputationspunkte, und extrem wenige Gefallen", "", "CRIME_FINISH_CRIME_SPREE_MISSION").withData(new Dictionary<string, dynamic> { { "Type", "CASH" }, { "Module", this }, { "Mult", mult } }).needsConfirmation("Cashbelohnung wählen?", "Wirklich Cashbelohnung wählen?"));
                        finishMenu.addMenuItem(new ClickMenuItem("Reputation bevorzugen", $"Die Belohnung auf die Steigerung deiner Reputation fokussieren. Du erhälst ${Math.Round(cash * 0.5m, 2)}, viele Reputationspunkte, und extrem wenige Gefallen", "", "CRIME_FINISH_CRIME_SPREE_MISSION").withData(new Dictionary<string, dynamic> { { "Type", "REPUTATION" }, { "Module", this }, { "Mult", mult } }).needsConfirmation("Reputationsbelohnung wählen?", "Wirklich Reputationsbelohnung wählen?"));

                        if(rep.isReputationHighEnough(Pillar, "MISSIONS_FAVORS_REWARD")) {
                            finishMenu.addMenuItem(new ClickMenuItem("Gefallen bevorzugen", $"Die Belohnung auf den Gefallensaufbau fokussieren. Du erhälst ${Math.Round(cash * 0.2m, 2)}, wenige Reputationspunkte, und einige Gefallen", "", "CRIME_FINISH_CRIME_SPREE_MISSION").withData(new Dictionary<string, dynamic> { { "Type", "FAVORS" }, { "Module", this }, { "Mult", mult } }).needsConfirmation("Gefallensbelohnung wählen?", "Wirklich Gefallensbelohnung wählen?"));
                        }

                        menu.addMenuItem(new MenuMenuItem(finishMenu.Name, finishMenu, MenuItemStyle.green));
                    } else {
                        menu.addMenuItem(new StaticMenuItem("Aktuelle Verbrechensserie auf falscher Säule!", "Die aktuelle Verbrechensserie ist von einer anderen Säule. Du kannst sie hier nur ersetzen!", "", MenuItemStyle.yellow));
                    }
                } else {
                    var crimeSpreeMenu = new Menu("Verbrechensserie", "Bearbeite eine Verbrechensserie");

                    var crimeSpreeMissionPillar = CrimeMissionsController.getMissionTrigger(currentMissions.CrimeSpreeMission?.FirstOrDefault() ?? -1)?.Pillar;
                    if (crimeSpreeMissionPillar == null || crimeSpreeMissionPillar == Pillar) {
                        if (currentMissions.CrimeSpreeMission != null && currentMissions.CrimeSpreeMission.Count > 0) {
                            var restTime = Math.Round((currentMissions.CrimeSpreeStart + TimeSpan.FromHours(0.75 * currentMissions.CrimeSpreeMission.Count) - DateTime.Now).TotalMinutes);

                            if (restTime > 0) {
                                crimeSpreeMenu.addMenuItem(new StaticMenuItem("Übrige Zeit", $"Du hast noch {restTime}min Zeit um alle aufgelisteten Missionen zu bearbeiten. Bearbeitung aller Missionen in der Zeit gibt einen Bonus auf die Belohnung", $"{restTime}min"));
                            } else {
                                crimeSpreeMenu.addMenuItem(new StaticMenuItem("Zeit abgelaufen", $"Die Zeit für die Bearbeitung der Verbrechensserie ist abgelaufen. Der Auftrag kann trotzdem noch bearbeitet und abgegeben werden, bringt jedoch einen geringeren Gewinn ein", "", MenuItemStyle.yellow));
                            }

                            var crimeSpreeMissionsMenu = new Menu("Missionen", "Die Missionen der Verbrechensserie");
                            for (var i = 0; i < currentMissions.CrimeSpreeMission.Count; i++) {
                                var missionId = currentMissions.CrimeSpreeMission[i];
                                var mission = CrimeMissionsController.getMissionTrigger(missionId);
                                var currentMenu = mission.getMenuRepresentative(player, currentMissions.CrimeSpreeStart, currentMissions.CrimeSpreeProgress[i]);
                                crimeSpreeMissionsMenu.addMenuItem(new MenuMenuItem(currentMenu.Name, currentMenu));
                            }
                            crimeSpreeMenu.addMenuItem(new MenuMenuItem(crimeSpreeMissionsMenu.Name, crimeSpreeMissionsMenu));
                        }
                    } else {
                        crimeSpreeMenu.addMenuItem(new StaticMenuItem("Aktuelle Verbrechensserie auf falscher Säule!", "Die aktuelle Verbrechensserie ist von einer anderen Säule. Du kannst sie hier nur ersetzen!", "", MenuItemStyle.yellow));
                    }

                    if (tokens.CrimeSpreeToken >= 1) {
                        crimeSpreeMenu.addMenuItem(new ClickMenuItem("Neue Verbrechensserie erhalten", "Ersetze deine aktuelle Verbrechensserie mit einer neuen.", "", "CRIME_SELECT_CRIME_SPREE_MISSION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Module", this } }).needsConfirmation("Neuen Auftrag anfordern?", "Wirklich anfordern?"));
                    } else {
                        crimeSpreeMenu.addMenuItem(new StaticMenuItem("Neue Verbrechensserie nicht verfügbar", "Du kannst aktuell keine neue Verbrechensserie erhalten. Komme später wieder!", "", MenuItemStyle.yellow));
                    }
                    menu.addMenuItem(new MenuMenuItem(crimeSpreeMenu.Name, crimeSpreeMenu));
                }
            }

            return new List<MenuItem> { new MenuMenuItem(menu.Name, menu) };
        }

        private Menu getMissionRewardMenu(int missionId, PlayerCrimeMissions currentMissions, PlayerCrimeReputation rep, string name, string which) {
            var mission = CrimeMissionsController.getMissionTrigger(missionId);
            var finishMenu = new Menu($"{name}", "Wähle die Belohnungsoption");

            var mult = 1m;
            switch(which) {
               case "SELECT":
                    if(currentMissions.SelectProgress.Status == CrimeMissionProgressStatus.CompletedLate) {
                        mult = 0.66m;
                    }
                    break;
                case "RANDOM":
                    if(currentMissions.RandomProgress.Status == CrimeMissionProgressStatus.CompletedLate) {
                        mult = 0.66m;
                    }
                    break; 
            }

            var cash = Math.Round(mission.CashReward * mult, 2);
            finishMenu.addMenuItem(new ClickMenuItem("Cash bevorzugen", $"Die Belohnung auf Cash fokussieren. Du erhälst ${cash}, einige Reputationspunkte, und extrem wenige Gefallen", "", "CRIME_FINISH_MISSION").withData(new Dictionary<string, dynamic> { { "Type", "CASH" }, { "Which", which }, { "Mission", mission }, { "Module", this } }).needsConfirmation("Cashbelohnung wählen?", "Wirklich Cashbelohnung wählen?"));
            finishMenu.addMenuItem(new ClickMenuItem("Reputation bevorzugen", $"Die Belohnung auf die Steigerung deiner Reputation fokussieren. Du erhälst ${Math.Round(cash * 0.5m, 2)}, viele Reputationspunkte, und extrem wenige Gefallen", "", "CRIME_FINISH_MISSION").withData(new Dictionary<string, dynamic> { { "Type", "REPUTATION" }, { "Which", which }, { "Mission", mission }, { "Module", this } }).needsConfirmation("Reputationsbelohnung wählen?", "Wirklich Reputationsbelohnung wählen?"));

            if(rep.isReputationHighEnough(Pillar, "MISSIONS_FAVORS_REWARD")) {
                finishMenu.addMenuItem(new ClickMenuItem("Gefallen bevorzugen", $"Die Belohnung auf den Gefallensaufbau fokussieren. Du erhälst ${Math.Round(cash * 0.2m, 2)}, wenige Reputationspunkte, und einige Gefallen", "", "CRIME_FINISH_MISSION").withData(new Dictionary<string, dynamic> { { "Type", "FAVORS" }, { "Which", which }, { "Mission", mission }, { "Module", this } }).needsConfirmation("Gefallensbelohnung wählen?", "Wirklich Gefallensbelohnung wählen?"));
            }

            return finishMenu;
        }

        public bool getMissionViable(PlayerCrimeReputation reputation, CrimeNetworkPillar missionPillar, string identifier) {
            return Pillar == missionPillar && reputation.isReputationHighEnough(Pillar, identifier);
        }

        public void reopenMenu(IPlayer player) {
            Ped.onInteract(player);
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Crime-Aufträge Modul", $"Fügt die Funktionalität hinzu Auftrag der {Pillar.Name} Crime Säule anzunehmen", "");
        }

        public override void onRemove() { }
    }

   
}
