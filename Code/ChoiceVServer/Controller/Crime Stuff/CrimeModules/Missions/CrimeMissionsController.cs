using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Crime_Stuff {
     public delegate List<MenuItem> AdminCreateTriggerDelegate(IPlayer player, ref CrimeAction action, ref string codeItem);

    public class CrimeMissionsController : ChoiceVScript {
        private static Dictionary<string, AdminCreateTriggerDelegate> TaskCreator = [];
        private static Dictionary<int, CrimeNetworkMissionTrigger> AllTriggers = [];
        private static TimeSpan TOKEN_GIVER_INTERVAL = TimeSpan.FromMinutes(7.67);

        public CrimeMissionsController() {
            EventController.MainReadyDelegate += loadTrigger;
            
            InvokeController.AddTimedInvoke("CrimeTokenGiver", updateMissionTokens, TOKEN_GIVER_INTERVAL, true);

            EventController.addMenuEvent("CRIME_SELECT_RANDOM_MISSION", onCrimeSelectRandomMission);

            EventController.addMenuEvent("CRIME_SELECT_GET_NEW_MISSIONS", onCrimeGetNewSelectMissions);
            EventController.addMenuEvent("CRIME_SELECT_SELECT_OPTION", onCrimeSelectSelectOption);

            EventController.addMenuEvent("CRIME_SELECT_CRIME_SPREE_MISSION", onCrimeSelectCrimeSpreeMission);

            EventController.addMenuEvent("CRIME_FINISH_MISSION", onCrimeFinishMission);
            EventController.addMenuEvent("CRIME_FINISH_CRIME_SPREE_MISSION", onCrimeFinishCrimeSpreeMission);

            EventController.addMenuEvent("CRIME_SHOW_MISSION_AREA", onCrimeShowMissionArea);

            #region Create Stuff

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    generateMissionMenu,
                    4,
                    SupportMenuCategories.Crime,
                    "Crime Aufträge"
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_CRIME_MISSION", onSupportCreateCrimeMission);
            EventController.addMenuEvent("SUPPORT_CREATE_CRIME_MISSION_SUBMIT", onSupportCreateCrimeMissionSubmit);
            EventController.addMenuEvent("SUPPORT_ACCEPT_CRIME_MISSION", onSupportAcceptCrimeMission);

            PedController.addNPCModuleGenerator("Crime Auftrag Modul", onSupportAddCrimeMissionModule, onSupportAddCrimeMissionModuleCallback);

            #endregion
        }

        private void updateMissionTokens(IInvoke obj) {
            using(var db = new ChoiceVDb()) {
                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    if(player.getCharacterFullyLoaded() && player.getCharacterData().CharacterFlag.HasFlag(CharacterFlag.CrimeFlag)) {
                        var tokens = (CrimeMissionTokens)player.getData("CRIME_MISSION_TOKENS");

                        var mult = getPlayTimeModifier(player);
                        tokens.RandomToken += (float)(TOKEN_GIVER_INTERVAL.TotalMilliseconds / TimeSpan.FromHours(1).TotalMilliseconds) * mult;
                        tokens.RandomToken = Math.Min(tokens.RandomToken, 10);

                        tokens.SelectToken += (float)(TOKEN_GIVER_INTERVAL.TotalMilliseconds / TimeSpan.FromHours(1).TotalMilliseconds) * 0.33f * mult;
                        tokens.SelectToken = Math.Min(tokens.SelectToken, 5);

                        tokens.CrimeSpreeToken += (float)(TOKEN_GIVER_INTERVAL.TotalMilliseconds / TimeSpan.FromHours(1).TotalMilliseconds) * 0.125f * mult;
                        tokens.CrimeSpreeToken = Math.Min(tokens.CrimeSpreeToken, 3);

                        var charToken = db.charactercrimemissiontokens.Find(player.getCharacterId());

                        if(charToken != null) {
                            charToken.selectToken = tokens.SelectToken;
                            charToken.randomToken = tokens.RandomToken;
                            charToken.crimeSpreeToken = tokens.CrimeSpreeToken;
                        } else {
                            var newToken = new charactercrimemissiontoken {
                                charId = player.getCharacterId(),
                                selectToken = tokens.SelectToken,
                                randomToken = tokens.RandomToken,
                                crimeSpreeToken = tokens.CrimeSpreeToken,
                            };

                            db.charactercrimemissiontokens.Add(newToken);
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private float getPlayTimeModifier(IPlayer player) {
            var playTime = TimeSpan.FromMilliseconds(player.getCharacterData().CurrentPlayTime);

            if(playTime <= TimeSpan.FromHours(2)) {
                return 1;
            } else if(playTime <= TimeSpan.FromHours(4)) {
                return 0.5f;
            } else {
                return 0.25f;
            }
        }

        private static void loadTrigger() {
            AllTriggers = new Dictionary<int, CrimeNetworkMissionTrigger>();

            using(var db = new ChoiceVDb()) {
                foreach(var dbTrigger in db.configcrimemissiontriggers) {
                    var type = Type.GetType("ChoiceVServer.Controller.Crime_Stuff." + dbTrigger.codeItem, false);

                    var crimeNetworkPillar = CrimeNetworkController.getPillarById(dbTrigger.pillarId);
                    var trigger = (CrimeNetworkMissionTrigger)Activator.CreateInstance(type, dbTrigger.id, (CrimeAction)dbTrigger.type, crimeNetworkPillar, dbTrigger.amount, TimeSpan.FromMilliseconds(dbTrigger.time), dbTrigger.position.FromJson(), dbTrigger.radius);
                    AllTriggers[dbTrigger.id] = trigger;
                    trigger.setSettings(dbTrigger.settings.FromJson<Dictionary<string, string>>());
                    trigger.setReward(dbTrigger.cashReward, dbTrigger.reputationReward, dbTrigger.favorReward);
                }
            }
        }

        public static CrimeNetworkMissionTrigger getMissionTrigger(int id) {
            if(!AllTriggers.ContainsKey(id)) {
                return null;
            }

            return AllTriggers[id];
        }

        public static void onPlayerCrimeAction(IPlayer player, CrimeAction action, float amount, Dictionary<string, dynamic> data) {
            if(player.hasData("CRIME_MISSIONS")) {
                var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");

                if(currentMissions?.CrimeSpreeMission != null) {
                    for(var i = 0; i < currentMissions.CrimeSpreeMission.Count; i++) {
                        var trigger = getMissionTrigger(currentMissions.CrimeSpreeMission[i]);
                        if(trigger.onTriggerProgress(player, "Verbrechensserie-Auftrag", action, amount, data, currentMissions.CrimeSpreeProgress[i])) {
                            currentMissions.updateProgressDb(player);
                            return;
                        }
                    }
                }

                if(currentMissions?.SelectSelection != null && currentMissions.SelectSelection.Count == 1) {
                    var trigger = getMissionTrigger(currentMissions.SelectSelection.First());
                    if(trigger.onTriggerProgress(player, "Wahlauftrag", action, amount, data, currentMissions.SelectProgress)) {
                        if(trigger.TimeConstraint != TimeSpan.Zero) {
                            if(currentMissions.SelectStart + trigger.TimeConstraint <= DateTime.Now) {
                                currentMissions.SelectProgress.Status = CrimeMissionProgressStatus.CompletedLate;
                            } 
                        }
                        currentMissions.updateProgressDb(player);
                        return;
                    }
                }

                if(currentMissions?.RandomMission != null) {
                    var trigger = getMissionTrigger(currentMissions.RandomMission ?? -1);
                    if(trigger.onTriggerProgress(player, "Zufallsauftrag", action, amount, data, currentMissions.RandomProgress)) {
                        if(trigger.TimeConstraint != TimeSpan.Zero) {
                            if(currentMissions.RandomStart + trigger.TimeConstraint <= DateTime.Now) {
                                currentMissions.RandomProgress.Status = CrimeMissionProgressStatus.CompletedLate;
                            } 
                        }
                        currentMissions.updateProgressDb(player);
                        return;
                    }
                }
            }
        }

        public static List<CrimeNetworkMissionTrigger> getAllMissions() {
            return AllTriggers.Values.ToList();
        }

        private bool onCrimeSelectRandomMission(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var tokens = (CrimeMissionTokens)player.getData("CRIME_MISSION_TOKENS");

            lock(tokens) {
                var module = (CrimeMissionModule)data["Module"];
                var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");
                var rep = player.getCrimeReputation();

                if(tokens.RandomToken >= 1) {
                    tokens.RandomToken--;
                    tokens.updateDb(player);

                    var viableMissions = AllTriggers.Values.Where(t => module.getMissionViable(rep, t.Pillar, t.getPillarReputationName())).ToList();
                    var rnd = viableMissions[new Random().Next(0, viableMissions.Count)];

                    currentMissions.RandomMission = rnd.Id;
                    currentMissions.RandomStart = DateTime.Now;
                    currentMissions.RandomProgress = new CrimeMissionProgress();

                    currentMissions.updateDb(player);

                    rnd.sendPlayerSelectNotification(player);

                    module.reopenMenu(player);
                }
            }
            return true;
        }

        private bool onCrimeGetNewSelectMissions(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var tokens = (CrimeMissionTokens)player.getData("CRIME_MISSION_TOKENS");

            lock(tokens) {
                var evt = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;
                var crimeType = CrimeNetworkController.getCrimeActionFromName(evt.currentElement);
                var module = (CrimeMissionModule)data["Module"];
                var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");
                var rep = player.getCrimeReputation();

                if(tokens.SelectToken >= 1) {
                    tokens.SelectToken--;
                    tokens.updateDb(player);

                    var viableMissions = AllTriggers.Values.Where(t => t.Type == crimeType && module.getMissionViable(rep, t.Pillar, t.getPillarReputationName())).ToList();

                    var rnd = new Random();
                    currentMissions.SelectSelection = viableMissions.OrderBy(x => rnd.Next()).Take(3).Select(s => s.Id).ToList();

                    currentMissions.updateDb(player);
                    player.sendNotification(Constants.NotifactionTypes.Success, "Es wurde dir eine Auswahl an verfügbaren Aufträgen gegeben. Wähle einen davon aus", "Wahlaufträge erhalten", Constants.NotifactionImages.Thief);

                    module.reopenMenu(player);
                }
            }
            return true;
        }

        private bool onCrimeSelectSelectOption(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var module = (CrimeMissionModule)data["Module"];
            var mission = (CrimeNetworkMissionTrigger)data["Mission"];

            var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");
            currentMissions.SelectSelection = new List<int> { mission.Id };
            currentMissions.SelectProgress = new CrimeMissionProgress();
            currentMissions.SelectStart = DateTime.Now;

            currentMissions.updateDb(player);

            mission.sendPlayerSelectNotification(player);

            module.reopenMenu(player);

            return true;
        }

        private bool onCrimeSelectCrimeSpreeMission(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var tokens = (CrimeMissionTokens)player.getData("CRIME_MISSION_TOKENS");

            lock(tokens) {
                var module = (CrimeMissionModule)data["Module"];
                var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");
                var rep = player.getCrimeReputation();

                if(tokens.CrimeSpreeToken >= 1) {
                    tokens.CrimeSpreeToken--;
                    tokens.updateDb(player);

                    var viableMissions = AllTriggers.Values.Where(t => module.getMissionViable(rep, t.Pillar, t.getPillarReputationName())).ToList();

                    var rnd = new Random();
                    currentMissions.CrimeSpreeMission = viableMissions.OrderBy(x => rnd.Next()).Take(3).Select(s => s.Id).ToList();

                    currentMissions.CrimeSpreeStart = DateTime.Now;
                    currentMissions.CrimeSpreeProgress = [new(), new(), new()];

                    currentMissions.updateDb(player);

                    player.sendNotification(Constants.NotifactionTypes.Success, "Verbrechensserie erhalten. Siehe dir die zu Bearbeitenden Missionen an!", "Verbrechensserie erhalten", Constants.NotifactionImages.Thief);

                    module.reopenMenu(player);
                }
            }
            return true;
        }

        private bool onCrimeFinishMission(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var which = (string)data["Which"];
            var module = (CrimeMissionModule)data["Module"];
            var mission = (CrimeNetworkMissionTrigger)data["Mission"];
            var type = (string)data["Type"];

            var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");
            var reputation = player.getCrimeReputation();

            var mult = 1f;
            switch(which) {
                case "RANDOM":
                    if(currentMissions.RandomProgress.Status == CrimeMissionProgressStatus.CompletedLate) {
                        mult = 0.60f;
                    }

                    currentMissions.RandomMission = null;
                    currentMissions.RandomStart = DateTime.MinValue;
                    currentMissions.RandomProgress = new();
                    currentMissions.updateDb(player);
                    break;
                case "SELECT":
                    if(currentMissions.SelectProgress.Status == CrimeMissionProgressStatus.CompletedLate) {
                        mult = 0.60f;
                    }

                    currentMissions.SelectSelection = null;
                    currentMissions.SelectStart = DateTime.MinValue;
                    currentMissions.SelectProgress = new();
                    currentMissions.updateDb(player);
                    break;
            }

            var cash = 0m;
            var reputationGain = 0f;
            var favors = 0f;

            switch(type) {
                case "CASH":
                    cash = Math.Round(mission.CashReward * (decimal)mult, 2);
                    reputationGain = mission.ReputationReward * 0.5f * mult;
                    favors = mission.FavorReward * 0.2f * mult;
                    break;
                case "REPUTATION":
                    cash = Math.Round(mission.CashReward * 0.5m * (decimal)mult, 2);
                    reputationGain = mission.ReputationReward * mult;
                    favors = mission.FavorReward * 0.2f * mult;
                    break;
                case "FAVORS":
                    cash = Math.Round(mission.CashReward * 0.2m * (decimal)mult, 2);
                    reputationGain = mission.ReputationReward * 0.2f * mult;
                    favors = mission.FavorReward * mult;
                    break;
            }

            player.addCash(cash);

            reputation.giveReputation(module.Pillar, reputationGain);
            reputation.giveFavors(module.Pillar, favors);

            reputation.updateDb(player, module.Pillar);

            var malusStr = "";
            if(mult != 1) {
                malusStr = "Aufgrund einer nicht eingehaltenen Einschränkung, hast du eine geringere Belohnung erhalten!";
            }

            player.sendNotification(Constants.NotifactionTypes.Success, $"Auftrag erfolgreich abgeschlossen. Du hast ${cash} erhalten. {malusStr}", "Auftrag abgeschlossen", Constants.NotifactionImages.Thief);

            return true;
        }

        private bool onCrimeFinishCrimeSpreeMission(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var module = (CrimeMissionModule)data["Module"];
            var type = (string)data["Type"];
            var mult = (float)data["Mult"];

            var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");
            var reputation = player.getCrimeReputation();

            var missions = new List<CrimeNetworkMissionTrigger>();
            currentMissions.CrimeSpreeMission.ForEach(m => missions.Add(getMissionTrigger(m)));

            currentMissions.CrimeSpreeMission = null;
            currentMissions.CrimeSpreeProgress = null;
            currentMissions.CrimeSpreeStart = DateTime.MinValue;

            var cash = 0m;
            var reputationGain = 0f;
            var favors = 0f;

            foreach(var mission in missions) {
                cash += mission.CashReward;
                reputationGain += mission.ReputationReward;
                favors += mission.FavorReward;
            }

            switch(type) {
                case "CASH":
                    cash = Math.Round(cash * (decimal)mult, 2);
                    reputationGain = reputationGain * 0.5f * mult;
                    favors = favors * 0.2f * mult;
                    break;
                case "REPUTATION":
                    cash = Math.Round(cash * 0.5m * (decimal)mult, 2);
                    reputationGain = reputationGain * mult;
                    favors = favors * 0.2f * mult;
                    break;
                case "FAVORS":
                    cash = Math.Round(cash * 0.2m * (decimal)mult, 2);
                    reputationGain = reputationGain * 0.2f * mult;
                    favors = favors * mult;
                    break;
            }

            player.addCash(cash);

            reputation.giveReputation(module.Pillar, reputationGain);
            reputation.giveFavors(module.Pillar, favors);

            reputation.updateDb(player, module.Pillar);

            var malusStr = "";
            if(mult != 1.25) {
                malusStr = "Aufgrund einer nicht eingehaltenen Einschränkung, hast du eine geringere Belohnung erhalten!";
            }

            player.sendNotification(Constants.NotifactionTypes.Success, $"Verbrechensserie erfolgreich abgeschlossen. Du hast ${cash} erhalten. {malusStr}", "Verbrechensserie abgeschlossen", Constants.NotifactionImages.Thief);

            return true;
        }

        private bool onCrimeShowMissionArea(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var mission = (CrimeNetworkMissionTrigger)data["Mission"];

            var centerBlip = BlipController.createPointBlip(player, $"Auftragsareal: {mission.getName()}", mission.Position, 3, 433, 200);
            var radiusBlip = BlipController.createRadiusBlip(player, mission.Position, mission.Radius, 3, 100);

            InvokeController.AddTimedInvoke("Blip-Remover", (i) => {
                BlipController.destroyBlipByName(player, centerBlip);
                BlipController.destroyBlipByName(player, radiusBlip);
            }, TimeSpan.FromMinutes(1), false);

            return true;
        }

        #region Create Stuff

        public static void addJobTaskCreator(string name, AdminCreateTriggerDelegate creator) {
            TaskCreator.Add(name, creator);
        }

        private Menu generateMissionMenu() {
            var menu = new Menu("Crime Missionen erstellen", "Erstelle Crime Missionen");

            var alreadyMenu = new Menu("Liste", "Was möchtest du tun?");

            foreach(var trigger in AllTriggers.Values) {
                var triggerMenu = new VirtualMenu($"{trigger.Id} {trigger.Type}", () => {
                    var men = new Menu($"{trigger.Id} {trigger.Type}", "Was möchtest du tun?");

                    men.addMenuItem(new StaticMenuItem("Säule", "Die Säule des Triggers.", trigger.Pillar.Name));

                    men.addMenuItem(new StaticMenuItem("Anzahl", "Die Anzahl des Triggers.", $"{trigger.Amount}"));
                    var timeStr = "Unbegrenzt";
                    if(trigger.TimeConstraint != TimeSpan.Zero) {
                        timeStr = $"{trigger.TimeConstraint.TotalMinutes}min";
                    }
                    men.addMenuItem(new StaticMenuItem("Zeitbeschränkung", "Die Zeitbeschränkung.", timeStr));

                    var posStr = "Keine";
                    if(trigger.Position != Position.Zero) {
                        posStr = $"Radius: {trigger.Radius}";
                    }
                    men.addMenuItem(new StaticMenuItem("Ortsbeschränkung", $"Orstbeschränkung: {trigger.Position.ToString}", posStr));

                    foreach(var item in trigger.getCreateListMenuItems()) {
                        men.addMenuItem(item);
                    }

                    men.addMenuItem(new ClickMenuItem("Auftrag annehmen", "Nimm den Auftrag an", "", "SUPPORT_ACCEPT_CRIME_MISSION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Mission", trigger } }));
                    men.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Mission", "", "SUPPORT_DELETE_CRIME_MISSION", MenuItemStyle.red));
                    return men;
                });

                alreadyMenu.addMenuItem(new MenuMenuItem(triggerMenu.Name, triggerMenu));
            }
            menu.addMenuItem(new MenuMenuItem(alreadyMenu.Name, alreadyMenu));

            menu.addMenuItem(new ListMenuItem("Erstellen", "Erstelle eine Mission", TaskCreator.Keys.ToArray(), "SUPPORT_CREATE_CRIME_MISSION", MenuItemStyle.green));

            return menu;
        }

        private bool onSupportCreateCrimeMission(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItem.ListMenuItemEvent;

            var menu = new Menu("Mission-Erstellung", "Erstell die Mission");

            menu.addMenuItem(new ListMenuItem("Säule", "Wähle die Säule aus", CrimeNetworkController.getAllPillars().Select(p => p.Name).ToArray(), ""));
            menu.addMenuItem(new InputMenuItem("Anzahl", "", "", ""));
            menu.addMenuItem(new InputMenuItem("Zeibeschränkung in min", "leer für keine", "", ""));
            menu.addMenuItem(new CheckBoxMenuItem("Aktuelle Position", "Nimm die aktuelle Position als Mittelpunkt des Radius", false, ""));
            menu.addMenuItem(new InputMenuItem("Radius", "Der Radius der Position. Leerlassen falls irrelevant", "", ""));
            menu.addMenuItem(new InputMenuItem("Cashbelohnung", "", "", ""));
            menu.addMenuItem(new InputMenuItem("Reputationsbelohnung", "", "", ""));
            menu.addMenuItem(new InputMenuItem("Gefallenbelohnung", "", "", ""));

            var callback = TaskCreator[evt.currentElement];

            var codeItem = "";
            var crimeType = CrimeAction.None;
            foreach(var item in callback.Invoke(player, ref crimeType, ref codeItem)) {
                menu.addMenuItem(item);
            }

            menu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Missiontrigger", "SUPPORT_CREATE_CRIME_MISSION_SUBMIT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "CodeItem", codeItem }, { "Action", crimeType } }).needsConfirmation("Mission erstellen?", "Mission wirklich erstellen?"));
            player.showMenu(menu);
            return true;
        }

        private bool onSupportCreateCrimeMissionSubmit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var codeItem = (string)data["CodeItem"];
            var action = (CrimeAction)data["Action"];

            var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

            var pillarEvt = evt.elements[0].FromJson<ListMenuItem.ListMenuItemEvent>();
            var amountEvt = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>();
            var timeEvt = evt.elements[2].FromJson<InputMenuItem.InputMenuItemEvent>();
            var positionEvt = evt.elements[3].FromJson<CheckBoxMenuItem.CheckBoxMenuItemEvent>();
            var radiusEvt = evt.elements[4].FromJson<InputMenuItem.InputMenuItemEvent>();
            var cashEvt = evt.elements[5].FromJson<InputMenuItem.InputMenuItemEvent>();
            var reputationEvt = evt.elements[6].FromJson<InputMenuItem.InputMenuItemEvent>();
            var favorsEvt = evt.elements[7].FromJson<InputMenuItem.InputMenuItemEvent>();

            var pillar = CrimeNetworkController.getAllPillars().FirstOrDefault(p => p.Name == pillarEvt.currentElement);
            var amount = float.Parse(amountEvt.input);
            var time = timeEvt.input == null || timeEvt.input == "" ? TimeSpan.Zero : TimeSpan.FromMinutes(int.Parse(timeEvt.input));
            var position = positionEvt.check ? player.Position : Position.Zero;
            var radius = float.Parse(radiusEvt.input == null || radiusEvt.input == "" ? "0" : radiusEvt.input);

            var cash = decimal.Parse(cashEvt.input);
            var reputation = float.Parse(reputationEvt.input);
            var favors = float.Parse(favorsEvt.input);

            var type = Type.GetType("ChoiceVServer.Controller.Crime_Stuff." + codeItem, false);
            var prot = (CrimeNetworkMissionTrigger)Activator.CreateInstance(type, -1, action, pillar, amount, time, position, radius);

            evt.elements = evt.elements.Skip(8).ToArray();

            var settings = prot.getSettingsFromMenuStats(evt);

            using(var db = new ChoiceVDb()) {
                var newTrigger = new configcrimemissiontrigger {
                    pillarId = pillar.Id,
                    type = (int)action,
                    codeItem = codeItem,

                    amount = amount,
                    time = (int)time.TotalMilliseconds,
                    position = position.ToJson(),
                    radius = radius,

                    cashReward = cash,
                    reputationReward = reputation,
                    favorReward = favors,

                    settings = settings.ToJson(),
                };

                db.configcrimemissiontriggers.Add(newTrigger);
                db.SaveChanges();

                player.sendNotification(Constants.NotifactionTypes.Success, "Trigger erstellt!", "");
            }

            loadTrigger();

            return true;
        }

        private bool onSupportAcceptCrimeMission(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var mission = (CrimeNetworkMissionTrigger)data["Mission"];
            
            var currentMissions = (PlayerCrimeMissions)player.getData("CRIME_MISSIONS");
            
            currentMissions.RandomMission = mission.Id;
            currentMissions.RandomStart = DateTime.Now;
            currentMissions.RandomProgress = new CrimeMissionProgress();
            
            currentMissions.updateDb(player);
                
            mission.sendPlayerSelectNotification(player);

            return true;
        }
        
        private List<MenuItem> onSupportAddCrimeMissionModule(ref Type codeType) {
            codeType = typeof(CrimeMissionModule);

            var list = new List<MenuItem>();

            var pillars = CrimeNetworkController.getAllPillars().Select(p => p.Name).ToArray();
            list.Add(new ListMenuItem("Säule", "Wähle die Crime Säule aus, zu dem das Modul gehört!", pillars, ""));

            return list;
        }

        private void onSupportAddCrimeMissionModuleCallback(IPlayer player, MenuStatsMenuItem.MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var pillarEvt = evt.elements[0].FromJson<ListMenuItem.ListMenuItemEvent>();

            var pillar = CrimeNetworkController.getAllPillars().FirstOrDefault(p => p.Name == pillarEvt.currentElement);

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "Mission_CrimePillar", pillar.Id } });
        }

        #endregion
    }


    
}