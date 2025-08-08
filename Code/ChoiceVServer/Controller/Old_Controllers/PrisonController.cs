//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using AltV.Net.Data;
//using ChoiceVServer.Companies;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Company;
//using ChoiceVServer.Model.FVSDatabase;
//using ChoiceVServer.Model.Menu;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Controller {
//    public class PrisonController : ChoiceVScript {
//        //Int is CharacterId, for fast access
//        private static readonly List<Inmate> AllInmates = new List<Inmate>();

//        private static bool settingsLoaded = false;
//        private static Position? PrisonExitPosition;
//        private static Position? PrisonEnterPosition;

//        public PrisonController() {
//            InvokeController.AddTimedInvoke("PrisonUpdater", updateInmates, PRISON_UPDATE_TIME, true);
//            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;
//            EventController.addCollisionShapeEvent("PRISON_COL_GUARD001PEDINTERACTION", onGuard001PedInteraction);
//            EventController.addCollisionShapeEvent("PRISON_COL_GUARD002PEDINTERACTION", onGuard002PedInteraction);
//            EventController.addMenuEvent("PRISON_MENU_RESTTIME", onShowRestTimeToPlayer);
//            EventController.addMenuEvent("PRISON_MENU_RELEASEME", onReleaseRequestByInmatePlayer);
//            EventController.addMenuEvent("PRISON_MENU_RELESECIV", onReleaseRequestByCivPlayer);
//            EventController.addMenuEvent("PRISON_MENU_REIMPRISON", onReimprisonRequestByInmatePlayer);
//            updateInmates(null);
//        }

//        public static void doSetPrisonExitPosition(IPlayer player) {
//            setPrisonPositionSetting(player, Constants.PRISON_SETTING_EXITPOSITION);
//        }

//        private static void setPrisonPositionSetting(IPlayer player, string setting) {
//            using(var db = new ChoiceVDb()) {
//                configprisonsettings prisonExitSetting = db.configprisonsettings.FirstOrDefault(s =>
//                    string.Equals(s.key, setting, StringComparison.InvariantCultureIgnoreCase));

//                if(prisonExitSetting == null) {
//                    prisonExitSetting = new configprisonsettings() { key = setting, value = player.getCharacterData().LastPosition.ToJson() };
//                    db.Add(prisonExitSetting);
//                } else {
//                    prisonExitSetting.value = player.getCharacterData().LastPosition.ToJson();
//                }

//                db.SaveChanges();
//            }
//        }

//        public static void doSetPrisonEnterPosition(IPlayer player) {
//            setPrisonPositionSetting(player, Constants.PRISON_SETTING_ENTERPOSITION);
//        }

//        private bool onReimprisonRequestByInmatePlayer(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            ensureSettings();
//            //TODO_FVS
//            //using (var db = new FVSDb())
//            //{

//            //    prisoners_notes newNote = new prisoners_notes() { creator_account_id = 0, jail_id = CharacterController.getCharIdToSocialSecurityNumber(player.getCharacterId())?? 0, note = "Der Gefangene wurde außerhalb der Gefängnisses angetroffen und durch Officer Gonzales zurückgeführt." };
//            //    db.Add(newNote);
//            //    db.SaveChanges();
//            //}

//            if(isGuardInDuty()) {
//                player.sendNotification(NotifactionTypes.Danger, "Es ist ein Beamter im Dienst. Diese Situation ist im RP zu behandeln.", "RP Situation");
//                return false;
//            }

//            int charId = player.getCharacterId();
//            if(AllInmates.Any(i => i.CharacterId == charId)) {
//                if(PrisonEnterPosition.HasValue) {
//                    player.SetPosition(PrisonEnterPosition.Value.X, PrisonEnterPosition.Value.Y, PrisonEnterPosition.Value.Z);
//                    player.sendNotification(NotifactionTypes.Danger, "Du wurdest in's Gefängnis zurück gebracht.", "In's Gefängnis");
//                    return true;
//                }
//                player.sendNotification(NotifactionTypes.Danger, "Der Teleport hat nicht geklappt. Bitte den Support kontaktieren.", "Teleport-Fehler");
//            }
//            return false;
//        }

//        private bool onGuard002PedInteraction(IPlayer player, CollisionShape collisionshape, Dictionary<string, object> data) {
//            ensureSettings();
//            if(isGuardInDuty()) {
//                player.sendNotification(NotifactionTypes.Danger, "Es ist ein Beamter im Dienst. Diese Situation ist im RP zu behandeln.", "RP Situation");
//                return false;
//            }

//            int charId = player.getCharacterId();
//            if(AllInmates.Any(i => i.CharacterId == charId)) {
//                Menu menu = new Menu("Gefängnis Los Santos", null);
//                menu.addMenuItem(new ClickMenuItem("Zurück in's Gefängnis", "NUR IM BUG-FALL NUTZEN! FUNKTION ERZEUGT SUPPORT-TICKET!", null, "PRISON_MENU_REIMPRISON", MenuItemStyle.red));
//                player.showMenu(menu);
//                return true;
//            } else {
//                player.sendNotification(NotifactionTypes.Info, "Schönen guten Tag.", "Keine Aktion");
//                return true;
//            }
//        }

//        private static void ensureSettings() {
//            if(!settingsLoaded) {
//                using(var db = new ChoiceVDb()) {
//                    List<configprisonsettings> configPrisonSettingList = db.configprisonsettings.ToList();
//                    configprisonsettings firstOrDefault = configPrisonSettingList.FirstOrDefault(s => string.Equals(s.key, Constants.PRISON_SETTING_EXITPOSITION, StringComparison.InvariantCultureIgnoreCase));
//                    if(firstOrDefault != null) {
//                        PrisonExitPosition = firstOrDefault.value.FromJson();
//                    }

//                    firstOrDefault = configPrisonSettingList.FirstOrDefault(s => string.Equals(s.key, Constants.PRISON_SETTING_ENTERPOSITION, StringComparison.InvariantCultureIgnoreCase));
//                    if(firstOrDefault != null) {
//                        PrisonEnterPosition = firstOrDefault.value.FromJson();
//                    }
//                }
//            }
//        }

//        private bool onReleaseRequestByCivPlayer(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            ensureSettings();
//            if(isGuardInDuty()) {
//                player.sendNotification(NotifactionTypes.Danger, "Es ist ein Beamter im Dienst. Diese Situation ist im RP zu behandeln.", "RP Situation");
//                return false;
//            }

//            int charId = player.getCharacterId();
//            if(AllInmates.Any(i => i.CharacterId == charId)) {
//                player.sendNotification(NotifactionTypes.Danger, "Diese Aktion ist für Nicht-Insassen vorbehalten.", "Aktionsfehler.");
//                return false;
//            }

//            if(PrisonExitPosition.HasValue) {
//                player.SetPosition(PrisonExitPosition.Value.X, PrisonExitPosition.Value.Y, PrisonExitPosition.Value.Z);
//                player.sendNotification(NotifactionTypes.Success, "Du wurdest zum Ausgang gebracht.", "Zum Ausgang gebracht.");
//                return true;
//            }

//            player.sendNotification(NotifactionTypes.Danger, "Gefängnis-Teleport-Ausgang nicht besetzt. Wende dich bitte an den Support.", "Fehler!");
//            return false;
//        }

//        private static bool isGuardInDuty() {
//            Company[] companies = CompanyController.AllCompanies.Where(c => c is CompanySheriff).ToArray();
//            foreach(Company company in companies) {
//                if(company.Employees.Any(e =>
//                    e.InDuty && PlayerController.AllOnlinePlayers.ContainsKey(e.CharacterId))) {
//                    return true;
//                }
//            }
//            return false;
//        }

//        private bool onReleaseRequestByInmatePlayer(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            ensureSettings();
//            if(isGuardInDuty()) {
//                player.sendNotification(NotifactionTypes.Danger, "Es ist ein Beamter im Dienst. Diese Situation ist im RP zu behandeln.", "RP Situation");
//                return false;
//            }

//            int charId = player.getCharacterId();
//            if(AllInmates.Any(i => i.CharacterId == charId)) {
//                Inmate inmate = AllInmates[charId];
//                if(inmate.AllowedToLeave) {
//                    if(PrisonExitPosition.HasValue) {
//                        releaseInmate(player);
//                        player.SetPosition(PrisonExitPosition.Value.X, PrisonExitPosition.Value.Y, PrisonExitPosition.Value.Z);
//                        player.sendNotification(NotifactionTypes.Success, "Du wurdest zum Ausgang geführt, deine Entlassung wurde in den Akten vermerkt.", "Entlassen.");
//                        return true;
//                    }
//                } else {
//                    onShowRestTimeToPlayer(player, itemevent, menuitemid, data, menuitemcefevent);
//                }
//            } else {
//                player.sendNotification(NotifactionTypes.Info, "Du bist nicht als Insasse geführt.", "Kein Insasse");
//            }

//            return true;
//        }

//        private bool onShowRestTimeToPlayer(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
//            int charId = player.getCharacterId();
//            if(AllInmates.Any(i => i.CharacterId == charId)) {
//                Inmate inmate = AllInmates[charId];
//                if(inmate.AllowedToLeave) {
//                    player.sendNotification(NotifactionTypes.Info, "Du hast deine Zeit abgesessen! Du kannst nun deine Freilassung beantragen.", "Zeit abgesessen!");
//                } else {
//                    string timeLeft;
//                    string timeLeftShort;
//                    if(inmate.ActiveTimeLeft.TotalHours >= 24) {
//                        timeLeft =
//                            $"{inmate.ActiveTimeLeft.Days} Tag(e), {inmate.ActiveTimeLeft.Hours} Stunde(n) und {inmate.ActiveTimeLeft.Minutes} Minute(n)";
//                        timeLeftShort = $"{inmate.ActiveTimeLeft.Days}, {inmate.ActiveTimeLeft.Hours}:{inmate.ActiveTimeLeft.Minutes}";
//                    } else if(inmate.ActiveTimeLeft.TotalMinutes >= 60) {
//                        timeLeft =
//                            $"{inmate.ActiveTimeLeft.Hours} Stunde(n) und {inmate.ActiveTimeLeft.Minutes} Minute(n)";
//                        timeLeftShort = $"{inmate.ActiveTimeLeft.Hours}:{inmate.ActiveTimeLeft.Minutes}";
//                    } else if(inmate.ActiveTimeLeft.TotalSeconds >= 60) {
//                        timeLeft =
//                            $"{inmate.ActiveTimeLeft.Minutes} Minute(n)";
//                        timeLeftShort = $"{inmate.ActiveTimeLeft.Minutes} min";
//                    } else {
//                        timeLeft = "wenige Momente";
//                        timeLeftShort = "gleich";
//                    }
//                    player.sendNotification(NotifactionTypes.Info, $"Deine Haftstrafe beträgt noch {timeLeft}.", $"Frei in: {timeLeftShort}");
//                }
//            } else {
//                player.sendNotification(NotifactionTypes.Info, "Du bist nicht als Insasse geführt.", "Kein Insasse");
//            }

//            return true;
//        }

//        private bool onGuard001PedInteraction(IPlayer player, CollisionShape collisionshape, Dictionary<string, object> data) {
//            if(isGuardInDuty()) {
//                player.sendNotification(NotifactionTypes.Info, "Nachdem jemand anderes gerade im Dienst ist, habe ich gerade Pause.", "RP Situation");
//                return false;
//            }

//            int characterId = player.getCharacterId();

//            Menu menu = new Menu("Gefängnis Los Santos", null);
//            if(AllInmates.Any(i => i.CharacterId == characterId)) {
//                menu.addMenuItem(new ClickMenuItem("Haft-Restzeit", "Zeigt deine Rest-Haftzeit an.", null, "PRISON_MENU_RESTTIME", MenuItemStyle.green));
//                if(AllInmates[characterId].AllowedToLeave) {
//                    menu.addMenuItem(new ClickMenuItem("Entlassen werden.", "Du entlässt dich selbst (keiner derzeit im RP).", null, "PRISON_MENU_RELEASEME", MenuItemStyle.green));
//                } else {
//                    menu.addMenuItem(new StaticMenuItem("Entlassen werden.", "Nicht verfügbar.", null));
//                }
//            } else {
//                menu.addMenuItem(new ClickMenuItem("Gefängnis verlassen.", "Du wirst zum Ausgang 'gebracht'.", null, "PRISON_MENU_RELESECIV", MenuItemStyle.green));
//            }
//            player.showMenu(menu);
//            return true;
//        }

//        private void updateInmates(IInvoke ivk) {
//            var players = ChoiceVAPI.GetAllPlayerDictionary();

//            //TODO_FVS
//            //using (var db = new FVSDb()) {
//            //    List<prisoners> prisonerList = db.prisoners.Where(p => p.state == 1).ToList();

//            //    AllInmates.RemoveAll(i => i.IsPrisoner = false);

//            //    foreach (prisoners prisoner in prisonerList) {
//            //        if (AllInmates.All(i => i.Id != prisoner.id)) {
//            //            int? charId = CharacterController.getCharIdToSocialSecurityNumber(prisoner.perso_id);
//            //            if (!charId.HasValue) {
//            //                Logger.logWarning($"Personalnummer bei Insassen-Eintrag #{prisoner.id} falsch gesetzt!");
//            //                continue;
//            //            }

//            //            Inmate newInmate = new Inmate() {
//            //                CellNumber = prisoner.prison_cell,
//            //                CharacterId = charId.Value,
//            //                DangerLevel = prisoner.danger_level,
//            //                IsPrisoner = true,
//            //                Id = prisoner.id,
//            //                JailedReason = prisoner.in_jail_for,
//            //                SentenceStart = BaseExtensions.FromUnixTime(prisoner.timestamp),
//            //                SentenceDone = prisoner.hafteinheiten_done,
//            //                SocialSecurityNumber = prisoner.perso_id,
//            //                TotalSentence = prisoner.hafteinheiten

//            //            };
//            //            AllInmates.Add(newInmate);
//            //        }

//            //        Inmate inmate = AllInmates.First(i => i.Id == prisoner.id);
//            //        inmate.CellNumber = prisoner.prison_cell;
//            //        inmate.DangerLevel = prisoner.danger_level;
//            //        inmate.IsPrisoner = true;
//            //        inmate.Id = prisoner.id;
//            //        inmate.JailedReason = prisoner.in_jail_for;
//            //        inmate.SentenceStart = BaseExtensions.FromUnixTime(prisoner.timestamp);
//            //        inmate.SocialSecurityNumber = prisoner.perso_id;
//            //        inmate.SentenceDone = prisoner.hafteinheiten_done;
//            //        inmate.TotalSentence = prisoner.hafteinheiten;
//            //    }


//            //    foreach (var inmate in AllInmates) {
//            //        var online = false;
//            //        IPlayer player = null;
//            //        if (players.ContainsKey(inmate.CharacterId)) {
//            //            player = players[inmate.CharacterId];
//            //            online = true;
//            //        }

//            //        if (online) {
//            //            inmate.SentenceDone += Constants.PRISON_SENTENCE_UNITS_PER_MINUTE;
//            //        }
//            //        else {
//            //            inmate.SentenceDone += Constants.PRISON_SENTENCE_UNITS_PER_MINUTE *
//            //                                   Constants.PASSIVE_TO_ACTIVE_SENTENCE_FACTOR;
//            //        }

//            //        try {
//            //            prisoners first = db.prisoners.First(p => p.id == inmate.Id);
//            //            first.hafteinheiten_done = inmate.SentenceDone;
//            //        }
//            //        catch (Exception e) {
//            //            Logger.logException(e);
//            //        }

//            //        if (inmate.AllowedToLeave && !inmate.PlayerGotMessaged) {

//            //            if (player != null) {
//            //                inmate.PlayerGotMessaged = true;
//            //                player.sendNotification(NotifactionTypes.Info,
//            //                    "Du hast deine Zeit abgesessen! Du kannst nun deine Freilassung beantragen.",
//            //                    "Zeit abgesessen!");
//            //            }
//            //        }
//            //    }

//            //    db.SaveChanges();
//            //}
//        }

//        //public static bool addInmate(IPlayer player, TimeSpan passiveTime, string crime, string description = "") {
//        //    var charId = player.getCharacterId();

//        //    if (AllInmates.ContainsKey(charId)) {
//        //        Logger.logWarning($"PrisonController: Someone tried to imprison a player, who should already be in Prison");
//        //        return false;
//        //    }

//        //    var inmate = new Inmate(charId, passiveTime, crime, description);

//        //    AllInmates.Add(charId, inmate);

//        //    return true;
//        //}

//        public static bool releaseInmate(IPlayer player) {
//            var charId = player.getCharacterId();

//            var inmate = AllInmates.FirstOrDefault(i => i.CharacterId == charId);

//            if(inmate != null) {
//                //TODO_FVS
//                //using (var db = new FVSDb()) {
//                //    prisoners firstOrDefault = db.prisoners.FirstOrDefault(p => p.id == inmate.Id);
//                //    if (firstOrDefault == null) {
//                //        return false;
//                //    }

//                //    firstOrDefault.released = Convert.ToInt32(DateTime.Now.ToUnixTime());
//                //    firstOrDefault.state = 0;

//                //    prisoners_notes newNote = new prisoners_notes() { creator_account_id = 0, jail_id = firstOrDefault.id, note = "Der Insasse wurde durch Officer Gonzales entlassen." };
//                //    db.Add(newNote);
//                //    db.SaveChanges();
//                //}
//                AllInmates.Remove(inmate);

//                return true;
//            } else {
//                return false;
//            }
//        }

//        private void onPlayerConnect(IPlayer player, characters character) {
//            if(AllInmates.All(i => i.CharacterId != player.getCharacterId())) {
//                return;
//            }

//            var inmate = AllInmates[player.getCharacterId()];
//            if(inmate.AllowedToLeave) {
//                player.sendNotification(NotifactionTypes.Info, "Du hast deine Zeit abgesessen! Du kannst nun deine Freilassung beantragen.", "Zeit abgesessen!");
//            }
//        }
//    }

//    public class Inmate {
//        public int Id { get; set; }
//        public int CharacterId { get; set; }
//        public long SocialSecurityNumber { get; set; }
//        public int TotalSentence { get; set; }
//        public DateTime SentenceStart { get; set; }
//        public string JailedReason { get; set; }
//        public int DangerLevel { get; set; }
//        public string CellNumber { get; set; }
//        public float SentenceDone { get; set; }
//        public bool IsPrisoner { get; set; }
//        public bool PlayerGotMessaged { get; set; }


//        public DateTime ReleaseDateTime =>
//            DateTime.Now +
//            ActiveTimeLeft;

//        public bool AllowedToLeave => SentenceDone >= TotalSentence;

//        public TimeSpan ActiveTimeLeft =>
//            TimeSpan.FromMinutes((TotalSentence - SentenceDone) * (1.0f / Constants.PRISON_SENTENCE_UNITS_PER_MINUTE));
//    }

//}
