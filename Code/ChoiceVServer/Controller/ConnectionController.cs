using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.Controller.Discord;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller {
    [Flags]
    public enum AccountFlag {
        ExceptFrom2Fa = 1,
        StressTestAccount = 2,
        LiteModeActivated = 4,
    }

    class ConnectionController : ChoiceVScript {
        public static int DimensionCounter = 2;

        public ConnectionController() {
            EventController.PlayerConnectedDelegate += onPlayerConnected;

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerSuccessfullConnect;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            EventController.addMenuEvent("PLAYER_SELECT_CHAR", onPlayerSelectCharacter);
            EventController.addMenuEvent("PLAYER_SELECT_NEW_CHAR", onPlayerSelectNewCharacter);
            EventController.addMenuEvent("PLAYER_DISMISS_NEW_CHAR", onPlayerDismissNewChar);

            using(var db = new ChoiceVDb()) {
                foreach(var dbChar in db.characters) {
                    dbChar.loginToken = null;
                }

                db.SaveChanges();
            }

            EventController.addEvent("ANSWER_DISCORD_LOGIN", onPlayerAnswerDiscordEvent);
        }

        private void onPlayerSuccessfullConnect(IPlayer player, character character) {
            var charId = player.getCharacterId();
            var token = Guid.NewGuid().ToString();

            using(var db = new ChoiceVDb()) {
                var dbChar = db.characters.Find(charId);

                if(dbChar != null) {
                    dbChar.loginToken = token;
                    dbChar.lastLogin = DateTime.Now;

                    db.SaveChanges();
                }

                player.setData("ENCRYPT_KEY", token);
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            var charId = player.getCharacterId();

            using(var db = new ChoiceVDb()) {
                var dbChar = db.characters.Find(charId);

                if(dbChar != null) {
                    dbChar.loginToken = null;

                    db.SaveChanges();
                }
            }
        }

        //TODO
        //Zugang auf Server nur mit Admin Level > 1 oder wenn er bestimmte Gruppe hat
        //Zähle solange Wartungsmodus
        private void onPlayerConnected(IPlayer player, string reason) {
            using(var db = new ChoiceVDb()) {
                var account = db.accounts.FirstOrDefault(p => p.socialclubName.Equals(player.SocialClubName));
                if(account == null) {
                    if(Config.IsSkipPasswordLoginEnabled) {
                        account = db.accounts.FirstOrDefault(p => p.name == player.Name);

                        if(account == null) {
                            ChoiceVAPI.KickPlayer(player, "NO_USER", "Dein Socialclubname existiert nicht!", "Dein Socialclubname existiert nicht!");
                            return;
                        }
                    } else {
                        ChoiceVAPI.KickPlayer(player, "NO_USER", "Dein Socialclubname existiert nicht!", "Dein Socialclubname existiert nicht!");
                        return;
                    }
                } else if(account.state == 2) {
                    ChoiceVAPI.KickPlayer(player, "BANNED", "Du bist gebannt!", "Du bist gebannt!");
                    return;
                } else if(account.state == -1) {
                    ChoiceVAPI.KickPlayer(player, "NO_WHITELIST", "Du hast keine Whitelist!", "Du hast keine Whitelist!");
                    return;
                }

                if(((AccountFlag)account.flag).HasFlag(AccountFlag.StressTestAccount) && !Config.IsStressTestActive) {
                    player.Kick("Der Server ist gerade nicht zugänglich. Bitte versuche es später erneut!");
                    return;
                }

                if (Config.IsMaintanceMode) {
                    if(Config.IsDiscordBotEnabled) {
                        var adminLevelTask = DiscordController.getAdminLevelByDiscordId(ulong.Parse(account.discordId));
                        adminLevelTask.Wait();
                        if (adminLevelTask.Result < 1) {
                            player.Kick("Der Server ist im Wartungsmodus!");
                            return;
                        }
                    } else if(!Config.IsDevServer) {
                        player.Kick("Konfiguration des Servers nicht korrekt!");
                    }
                }
            }

            Logger.logInfo(LogCategory.System, LogActionType.Event, $"Player with Socialclub: {player.SocialClubName} connected.");
            player.setCharacterFullyLoaded(false);

            if(Config.IsDiscordBotEnabled) {
                player.emitClientEvent("REQUEST_DISCORD_LOGIN", Config.PublicKeyFileName);
            } else if(Config.IsSkipPasswordLoginEnabled) {
                account account = null;
                using(var db = new ChoiceVDb()) {
                    account = db.accounts.FirstOrDefault(p => p.name == player.Name);

                    account.loginToken = Guid.NewGuid().ToString();
                    db.SaveChanges();
                }
                
                loadPlayerAccount(player, account, player.SocialClubName, 0);
            }
        }


        private bool onPlayerAnswerDiscordEvent(IPlayer player, string eventName, object[] args) {
            try {
                var discordToken = SecurityController.decryptMessage(args[0].ToString());

                if(discordToken == "COULDNT_GET_PUBLIC_KEY") {
                    ChoiceVAPI.KickPlayer(player, "PublicSwan", "Versuche den Login erneut, bei weiterem Fehler melde dich im Support!", "Public Key konnte nicht abgerufen werden (Möglicherweise Serverfehler)");
                    return false;
                } else if(discordToken == "CLOUDNT_GET_AUTH_TOKEN") {
                    ChoiceVAPI.KickPlayer(player, "Doorbell", "Discord Auth konnte nicht durchgeführt werden! Melde dich mit deinem richtigen Discord Account an!", "Discord Auth fehlgeschlagen");
                    return false;
                }

                var licenseHash = args[2].ToString();

                var request = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/@me");
                request.Method = "Get";
                request.ContentLength = 0;
                request.Headers.Add("Authorization", "Bearer " + discordToken);
                request.ContentType = "application/x-www-form-urlencoded";

                string apiResponse = "";
                using(HttpWebResponse response1 = request.GetResponse() as HttpWebResponse) {
                    StreamReader reader1 = new StreamReader(response1.GetResponseStream());
                    apiResponse = reader1.ReadToEnd();
                }

                dynamic parse = JsonConvert.DeserializeObject(apiResponse);
                var discordId = ulong.Parse((string)parse.id);
                var mfaActivated = bool.Parse((string)parse.mfa_enabled);

                account account = null;
                using(var db = new ChoiceVDb()) {
                    account = db.accounts.FirstOrDefault(p => p.discordId == discordId.ToString() && p.socialclubName == player.SocialClubName);
                    account.loginToken = Guid.NewGuid().ToString();
                    db.SaveChanges();
                }

                //Kicking Player because no Account is found in the Database
                if(account == null) {
                    ChoiceVAPI.KickPlayer(player, "Homefront", "Es wurde kein Account zu diesem Socialclub in Verbindung mit dem aktuellen Discord Account gefunden!", "Kein Account gefunden", discordId);
                    return false;
                } else {
                    var flag = (AccountFlag)account.flag;
                    if(!mfaActivated && !flag.HasFlag(AccountFlag.ExceptFrom2Fa)) {
                        ChoiceVAPI.KickPlayer(player, "FactoryLock", "Du hast auf Discord keine 2 Faktor Authentifizierung eingerichtet! Falls du damit Probleme hast begib dich in den Support.", "Keine 2 Faktor Authentifizierung eingerichtet!", discordId);
                        return false;
                    }

                    if(!Config.IsStressTestActive && flag.HasFlag(AccountFlag.StressTestAccount)) {
                        ChoiceVAPI.KickPlayer(player, "NoStresstest", "Dein Account ist nur für einen Stresstest freigeschaltet!", "Aktuell keine Whitelist!", discordId);
                        return false;
                    }

                    switch(account.state) {
                        case 1:
                            loadPlayerAccount(player, account, player.SocialClubName, discordId);
                            break;
                        //Ban
                        case 2:
                            ChoiceVAPI.KickPlayer(player, "DoorSlam", $"Du bist gebannt! Melde dich bei Unklarheit im Support! Der angegebene Grund ist: {account.stateReason}", $"Spieler gebannt mit Grund: {account.stateReason}");
                            break;
                        default:
                            ChoiceVAPI.KickPlayer(player, "GhostWear", "Etwas stimmt mit deinem Account nicht, begib dich in den Support!", "Falscher State beim Account des Spielers");
                            break;
                    }
                }
            } catch(Exception e) {
                Logger.logException(e);
                ChoiceVAPI.KickPlayer(player, "IntensiveFrog", "Beim Login ist etwas schiefgelaufen", $"Beim Login ist etwas schiefgelaufen: {e}", 0);
            }

            return true;
        }
        

        private void loadPlayerAccount(IPlayer player, account account, string socialclubName, ulong discordId) {
            account.failedLogins = 0;
            account.lastLogin = DateTime.Now;

            Logger.logInfo(LogCategory.System, LogActionType.Event, $"Player with SocialClub {socialclubName} logged in.");

            player.setData(DATA_ACCOUNT_ID, account.id);
            player.setData("SOCIALCLUB", socialclubName);
            player.setData("ACCOUNT_TOKEN", account.loginToken);
            player.setData("ACCOUNT_FLAG", (AccountFlag)account.flag);

            if(discordId != 0) {
                player.setData("DISCORD_ID", discordId);
            }

            WebController.initPlayerWebSocketConnection(player);

            using(var db = new ChoiceVDb()) {
                var dbAccount = db.accounts.Find(account.id);

                if(dbAccount != null) {
                    dbAccount.lastLogin = DateTime.Now;
                    dbAccount.lastIp = player.Ip;

                    db.SaveChanges();
                }
            }

            startCharSelection(player, account.charAmount, account.id);
        }

        //private void onDiscordAuthHeartbeat() {
        //    foreach(var player in ChoiceVAPI.GetAllPlayers()) {
        //        if(player.getCharacterFullyLoaded()) {
        //            if(player.hasData("DISCORD_HEARTHBEAT_FLAG")) {
        //                addPlayerDiscordHeartbeatStrike(player);
        //            } else {
        //                player.setData("DISCORD_HEARTHBEAT_FLAG", DateTime.Now);
        //            }

        //            player.emitClientEvent("REQUEST_DISCORD_LOGIN", "");
        //        }
        //    }
        //}

        //private void addPlayerDiscordHeartbeatStrike(IPlayer player) {
        //    var strikes = 1;
        //    if(player.hasData("DISCORD_HEARTHBEAT_STRIKE")) {
        //        strikes = (int)player.getData("DISCORD_HEARTHBEAT_STRIKE");
        //        strikes++;

        //        if(strikes >= 3) {
        //            player.ban("Spielt nicht auf eigenem Account! (Discord gehijacked)");
        //            return;
        //        }
        //    }
        //    player.setData("DISCORD_HEARTHBEAT_STRIKE", strikes);
        //}

        //private bool onPlayerRegisterSubmit(IPlayer player, string eventname, object[] args) {
        //    using(var db = new ChoiceVDb()) {
        //        var name = player.Name;
        //        var hashedPassword = args[0].ToString();
        //        var salt = args[1].ToString();
        //        var socialclubName = args[2].ToString();
        //        var licenseHash = args[3].ToString();

        //        var account = db.accounts.FirstOrDefault(p => p.socialclubName == socialclubName);
        //        if(account == null) {
        //            ChoiceVAPI.KickPlayer(player, NO_ACCOUNT_KICK_MESSAGE);
        //            return false;
        //        }

        //        account.password = hashedPassword;
        //        account.salt = salt;

        //        account.stateReason = "";
        //        account.licenseHash = licenseHash;
        //        account.lastLogin = DateTime.Now.ToString();
        //        db.SaveChanges();

        //        Logger.logInfo($"Player with SocialClub {socialclubName} registered.");

        //        db.SaveChanges();
        //        player.setData(DATA_ACCOUNT_ID, account.id);


        //        if(account.state == 3) {
        //            account.state = 1;
        //            db.SaveChanges();

        //            WebController.initPlayerWebSocketConnection(player);

        //            startCharSelection(player, account.charAmount, account.id);
        //            return true;
        //        }

        //        player.emitClientEvent(PlayerLoginScreenDeactivate, false);
        //        openCharCreator(player, false);
        //        player.fadeScreen(false, 1000);
        //        return true;
        //    }
        //}

        //private bool onPlayerLoginSubmit(IPlayer player, string eventname, object[] args) {
        //    using(var db = new ChoiceVDb()) {
        //        var name = player.Name;
        //        var socialclubName = args[1].ToString();
        //        var licenseHash = args[2].ToString();

        //        //var account = db.accounts.FirstOrDefault(p => p.socialclubName == socialclubName && p.name == name);
        //        var account = db.accounts.FirstOrDefault(p => p.socialclubName == socialclubName);

        //        if(account == null) {
        //            ChoiceVAPI.KickPlayer(player, "Old_Homefront", "Es wurde kein Account zu diesme Socialclub in Verbindung mit dem aktuellen Discord Account gefunden!", "Kein Account gefunden (Alter Login)");

        //            return true;
        //        } else {
        //            //if(account.state != 4 && account.licenseHash != licenseHash) {
        //            //    player.Kick("KickCode: PirateWorm. Komme in den Support!");
        //            //    return true;
        //            //} else if(account.state == 4) {
        //            //    account.licenseHash = licenseHash;
        //            //    account.state = 1;
        //            //    db.SaveChanges();
        //            //}

        //            var salt = account.salt;

        //            var hashedPassword = args[0].ToString();
        //            var hashedAccountPassword = account.password;

        //            if(!hashedPassword.Equals(hashedAccountPassword)) {
        //                account.failedLogins = account.failedLogins + 1;
        //                if(account.failedLogins >= AllowedWrongLogins) {
        //                    ChoiceVAPI.KickPlayer(player, "Old_Doorkeeper", "Das Passwort wurde falsch angegeben!", "Spieler hat ein falsches Passwort angegeben (Alter Login)");
        //                    account.state = 2;
        //                    account.stateReason = TO_MANY_WRONG_LOGINS_MESSAGE;

        //                    db.SaveChanges();
        //                    Logger.logWarning($"Player with SocialClub {socialclubName} was blocked because the logged in too often with a wrong password!");
        //                    return true;
        //                }

        //                db.SaveChanges();
        //                Logger.logWarning($"Player with SocialClub {socialclubName} tried to log in with a wrong password!");

        //                player.emitClientEvent("LOGIN_WRONG_CREDENTIALS");
        //                return true;
        //            }

        //            account.failedLogins = 0;
        //            account.lastLogin = DateTime.Now.ToString();
        //            db.SaveChanges();

        //            Logger.logInfo($"Player with SocialClub {socialclubName} logged in.");

        //            player.setData(DATA_ACCOUNT_ID, account.id);
        //            player.setData("SOCIALCLUB", socialclubName);

        //            WebController.initPlayerWebSocketConnection(player);

        //            startCharSelection(player, account.charAmount, account.id);
        //        }
        //    }

        //    return true;
        //}

        private void startCharSelection(IPlayer player, int charAmount, int accountId) {
            player.fadeScreen(false, 900);
            if(charAmount <= 1) {
                using(var db = new ChoiceVDb()) {
                    var character = db.characters.FirstOrDefault(c => c.accountId == accountId);
                    if(character != null) {
                        finishPlayerCharSelection(player, character);
                    } else {
                        openCharCreator(player, false);
                    }
                }
            } else {
                openCharSelectMenu(player, charAmount, accountId);
            }
        }

        private void openCharSelectMenu(IPlayer player, int charAmount, int accountId) {
            player.emitClientEvent("CREATE_CHAR_SELECT_SCREEN");
            var menu = new Menu("Charakterauswahl", "Wähle deinen Charakter aus");

            using(var db = new ChoiceVDb()) {
                var dbChars = db.characters.Where(c => c.accountId == accountId).ToList();
                foreach(var dbChar in dbChars) {
                    var data = new Dictionary<string, dynamic> {
                            { "AccountId", accountId },
                            { "CharId", dbChar.id }
                        };

                    menu.addMenuItem(new ClickMenuItem($"{dbChar.firstname} {dbChar.lastname}", $"Wähle {dbChar.firstname} {dbChar.lastname} aus", ">", "PLAYER_SELECT_CHAR", MenuItemStyle.normal, true).withData(data));
                }

                if(dbChars.Count < charAmount) {
                    menu.addMenuItem(new ClickMenuItem($"Neuen Charakter erstellen", "Erstelle einen völlig neuen Charakter!", ">", "PLAYER_SELECT_NEW_CHAR", MenuItemStyle.green)
                        .needsConfirmation($"Neuen Charakter erstellen?", "Wirklich neuen Charakter erstellen?", "PLAYER_DISMISS_NEW_CHAR")
                        .withData(new Dictionary<string, dynamic> { { "CharAmount", charAmount}, { "AccountId", accountId } }));
                }
            }

            player.showMenu(menu);
        }

        private void finishPlayerCharSelection(IPlayer player, character character) {
            if(character != null) {
                player.setData(DATA_CHARACTER_ID, character.id);
                player.SetStreamSyncedMetaData(DATA_CHARACTER_ID, character.id);

                EventController.onPlayerSuccessfullConnection(player);
            }
        }

        private class HairStyleRepresentative {
            public int gtaId;
            public bool isFemale;
        }

        private class HairOverlayRepresentative {
            public string collection;
            public string hash;
        }

        private class CharCreatorInfo {
            public string title;
            public string firstName;
            public string middleNames;
            public string lastName;
            public string birthday;
            public string sscPrefix;
            public string overlayStr;

            public CharCreatorInfo(string title, string firstName, string middleNames, string lastName, string birthday, string sscPrefix, string overlayStr) {
                this.title = title;
                this.firstName = firstName;
                this.middleNames = middleNames;
                this.lastName = lastName;
                this.birthday = birthday;
                this.sscPrefix = sscPrefix;
                this.overlayStr = overlayStr;
            }
        }

        public static void openCharCreator(IPlayer player, bool withExisting) {
            player.addState(PlayerStates.InCharCreator);
            player.emitClientEvent("UNFOCUS_CEF");
            DimensionCounter++;
            player.changeDimension(DimensionCounter);
            player.setData("IS_IN_CHAR_CREATOR", true);

            var overlayL = player.getOverlayStringForType("hair_overlay");

            if(withExisting) {
                var data = player.getCharacterData();
                var style = player.getCharacterData().Style;
                player.emitClientEvent(Constants.PlayerStartCharacterCreation,
                    style.ToJsonWithIgnore(new JsonIgnoreContractResolver(new string[] { "_char" })),
                    new CharCreatorInfo(data.Title, data.FirstName, data.MiddleNames, data.LastName, data.DateOfBirth.ToString("dd.MM.yyyy"), data.SocialSecurityNumber != "-1" ? data.SocialSecurityNumber.Substring(0, 3) : "843", overlayL.Count > 0 ? overlayL.First() : null).ToJson(),
                    FaceFeatureController.AllHairStyles.OrderBy(h => h.level).Select(h => new HairStyleRepresentative { gtaId = h.gtaId, isFemale = h.gender == "F" }).ToJson(),
                    FaceFeatureController.AllHairOverlays.OrderBy(o => o.displayName).Select(o => new HairOverlayRepresentative { collection = o.collection, hash = o.hash }).ToJson()
                );
            } else {
                player.emitClientEvent(Constants.PlayerStartCharacterCreation,
                    null,
                    null,
                    FaceFeatureController.AllHairStyles.OrderBy(h => h.level).Select(h => new HairStyleRepresentative { gtaId = h.gtaId, isFemale = h.gender == "F" }).ToJson(),
                    FaceFeatureController.AllHairOverlays.OrderBy(o => o.displayName).Select(o => new HairOverlayRepresentative { collection = o.collection, hash = o.hash }).ToJson()
                );
            }
        }

        private bool onPlayerSelectNewCharacter(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            openCharCreator(player, false);

            return true;
        }

        private bool onPlayerDismissNewChar(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var accountId = (int)data["AccountId"];
            var charAmount = (int)data["CharAmount"];

            startCharSelection(player, charAmount, accountId);

            return true;
        }

        private bool onPlayerSelectCharacter(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var charId = (int)data["CharId"];
            using(var db = new ChoiceVDb()) {
                var dbChar = db.characters.Include(c => c.characterstyle).Include(c => c.characterclothing).FirstOrDefault(c => c.id == charId);

                if(dbChar != null) {
                    if(menuItemCefEvent.action == "changed") {
                        spawnCharPed(player, dbChar);
                    } else {
                        player.fadeScreen(true, 900);
                        InvokeController.AddTimedInvoke("onPlayerSelectCharacter", (i) => {
                            player.emitClientEvent("STOP_CHAR_SELECT_SCREEN");
                            if(CharSelectorPed.ContainsKey(dbChar.accountId)) {
                                PedController.destroyPed(CharSelectorPed[dbChar.accountId]);
                            }

                            finishPlayerCharSelection(player, dbChar);
                        }, TimeSpan.FromSeconds(1), false);
                    }
                }
            }

            return true;
        }

        private static Dictionary<int, ChoiceVPed> CharSelectorPed = new Dictionary<int, ChoiceVPed>();
        private static void spawnCharPed(IPlayer player, character character) {
            lock(CharSelectorPed) {
                using(var db = new ChoiceVDb()) {
                    if(CharSelectorPed.ContainsKey(character.accountId)) {
                        var oldPed = CharSelectorPed[character.accountId];
                        if(oldPed.Data["CharId"] == character.id) {
                            return;
                        }

                        PedController.destroyPed(oldPed);
                    }

                    var model = character.gender == "M" ? "mp_m_freemode_01" : "mp_f_freemode_01";
                    if(character.model != null) {
                        model = character.model;
                    }
                    
                    var ped = PedController.createPed(player, "", model, new Position(-765.73f, 79.015f, 55.23f - 1f), ChoiceVAPI.radiansToDegrees(2.573f));
                    ped.Data["CharId"] = character.id;
                    PedController.setPedStyle(ped, character.characterstyle);
                    PedController.setPedPlayerClothing(ped, ClothingController.getClothingFromDb(character.characterclothing));

                    CharSelectorPed[character.accountId] = ped;
                }
            }
        }
    }
}
