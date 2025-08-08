using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.Web;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Controller.Phone.PhoneController;

namespace ChoiceVServer.Controller.Phone {
    public class SocialMediaController : ChoiceVScript {
        public SocialMediaController() {
            EventController.addCefEvent("REQUEST_SOCIAL_MEDIA_ACCOUNTS", onRequestSocialMediaAccounts);
            EventController.addCefEvent("REQUEST_SOCIAL_MEDIA_CREDENTIALS", onRequestSocialMediaCredentials);
            EventController.addCefEvent("REQUEST_SOCIAL_MEDIA_ACCOUNT_CREATION", onRequestSocialMediaAccountCreation);

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            CompanyController.addExternalCompanySelfMenuRegister(
                "SOCIAL_MEDIA",
                socialMediaAccountGenerator,
                socialMediaAccountCallback,
                "SOCIAL_MEDIA_ACCESS");
        }

        private static MenuElement socialMediaAccountGenerator(Company company, IPlayer player) {
            var menu = new Menu("Social Media Aktionen", "Was möchtest du tun?");
            if(company.getSetting("SOCIAL_MEDIA_USERNAME") == null) {
                menu.addMenuItem(new ClickMenuItem("Account erstellen", "Erstelle einen neuen Social Media Account", "", "ACCOUNT_CREATION", MenuItemStyle.green)
                    .needsConfirmation("Social Media Account erstellen?", "Account wirklich erstellen?"));
            } else {
                menu.addMenuItem(new ClickMenuItem("Account ausloggen", "Lösche alle aktuellen Sessions für das Social Media. Empfehlenswert, wenn ein Mitarbeiter mit Social Media Zugriff diesen Zugriff verloren hat (Entlassung, Degradierung, etc.)", "", "ACCOUNT_LOGOUT", MenuItemStyle.yellow));
            }

            return menu;
        }

        private void socialMediaAccountCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "ACCOUNT_CREATION") {
                var userName = company.ShortName;
                if(userName.Length < 4) {
                    userName += "_SOCIAL";
                }

                var success = createSocialMediaAccount(SocialMediaOwnerType.Company, company.Id, userName, userName, "");
                if(success.success) {
                    company.setSetting("SOCIAL_MEDIA_USERNAME", userName);
                    player.sendNotification(Constants.NotifactionTypes.Success, "Social Media Account erstellt!", "Account erstellt!", Constants.NotifactionImages.Company);
                } else {
                    player.sendBlockNotification($"Social Media Account Erstellung fehlgeschlagen! Begib dich in den Support: {success.message}", "Account nicht erstellt.!", Constants.NotifactionImages.Company);
                }
            } else if(subEvent == "ACCOUNT_LOGOUT") {
                var success = deleteAllSessionsForUser(company.getSetting("SOCIAL_MEDIA_USERNAME"));

                if(success) {
                    player.sendNotification(Constants.NotifactionTypes.Success, "Social Media Account ausgeloggt!", "Account ausgeloggt!", Constants.NotifactionImages.Company);
                } else {
                    player.sendBlockNotification("Social Media Account Logout fehlgeschlagen! Begib dich in den Support!", "Account nicht ausgeloggt.!", Constants.NotifactionImages.Company);
                }
            }
        }

        private void onPlayerConnect(IPlayer player, character character) {
            onRequestSocialMediaAccounts(player, null);
        }

        public enum SocialMediaOwnerType {
            User = 0,
            Company = 1,
        }

        public static AccountCreationAnswer createSocialMediaAccount(SocialMediaOwnerType ownerType, int ownerId, string userName, string firstName, string lastName, string title = "", string phoneNumber = "") {
            var accountCreationAnswer = createAccountViaRest(userName, firstName, lastName, title, phoneNumber);
            
            if(accountCreationAnswer.success) {
                using(var db = new ChoiceVDb()) {
                    var newSocial = new socialmediaaccount {
                        ownerType = (int)ownerType,
                        ownerId = ownerId,
                        guid = accountCreationAnswer.guid,
                        userName = userName,
                    };

                    db.socialmediaaccounts.Add(newSocial);
                    db.SaveChanges();
                }
            }

            return accountCreationAnswer;
        }

        internal class SocialMediaAccountCreationRequest {
            public string userName;
            public string firstName;
            public string lastName;
            public string title;
            public string phoneNumber;
        }

        internal class SocialMediaAccountCreation : PhoneAnswerEvent {
            public bool success;
            public string message;

            public SocialMediaAccountCreation(bool success, string message) : base("ANSWER_SOCIAL_MEDIA_ACCOUNT_CREATION") {
                this.success = success;
                this.message = message;
            }
        }

        private void onRequestSocialMediaAccountCreation(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var requestEvt = evt.Data.FromJson<SocialMediaAccountCreationRequest>();

            if(requestEvt.userName.Length < 6 || requestEvt.userName.Length > 20) {
                return;
            }
            var success = createSocialMediaAccount(SocialMediaOwnerType.User, player.getCharacterId(), requestEvt.userName, requestEvt.firstName, requestEvt.lastName);

            player.emitCefEventNoBlock(new SocialMediaAccountCreation(success.success, success.success ? "Accounterstellung erfolgreich!" : success.message));
        }

        #region RequestSocialMediaAccountList

        internal record SocialMediaAccountInfo(int type, string userName, string firstName, string lastName);
        internal class SocialMediaAccountList : PhoneAnswerEvent {

            public List<SocialMediaAccountInfo> accounts;

            public SocialMediaAccountList(List<SocialMediaAccountInfo> accounts): base("ANSWER_SOCIAL_MEDIA_ACCOUNTS_REQUEST") {
                this.accounts = accounts;
            }
        }

        private void onRequestSocialMediaAccounts(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            using(var db = new ChoiceVDb()) {
                var companies = CompanyController.getCompanies(player);
                
                var idList = new List<int> { player.getCharacterId() };
                if(companies != null) {
                    foreach(var company in companies) {
                        if(CompanyController.hasPlayerPermission(player, company, "SOCIAL_MEDIA_ACCESS")) {
                            idList.Add(company.Id);
                        }
                    }
                }

                var charId = player.getCharacterId();

                var socialMediaAccounts = db.socialmediaaccounts.Where(s => 
                    s.ownerType == (int)SocialMediaOwnerType.Company && idList.Contains(s.ownerId) ||
                    s.ownerType == (int)SocialMediaOwnerType.User && s.ownerId == charId).ToList();

                var list = new List<SocialMediaAccountInfo>();

                foreach(var account in socialMediaAccounts) {
                    var userInfo = getUserInfoViaRest(account.userName);

                    if(userInfo.success) {
                        list.Add(new SocialMediaAccountInfo(account.ownerType, account.userName, userInfo.firstName, userInfo.lastName));
                    } else {
                        Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"SocialMediaController.onRequestSocialMediaAccounts: Could not get user info for {account.userName}");
                    }
                }

                player.emitCefEventNoBlock(new SocialMediaAccountList(list));
            }
        }



        #endregion

        #region RequestSocialMediaAccountCredentials

        internal class SocialMediaCredentialsRequest {
            public string userName;
        }

        internal class SocialMediaCredentialsPhone : PhoneAnswerEvent {
            public string urlStart;
            public string guid;
            public string token;
            public int userId;
            public string schema;

            public SocialMediaCredentialsPhone(string urlStart, string guid, string token, int userId, string schema) : base("ANSWER_SOCIAL_MEDIA_CREDENTIALS_REQUEST") {
                this.urlStart = urlStart;
                this.guid = guid;
                this.token = token;
                this.userId = userId;
                this.schema = schema;
            }
        }

        private void onRequestSocialMediaCredentials(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var requestEvt = evt.Data.FromJson<SocialMediaCredentialsRequest>();

            using(var db = new ChoiceVDb()) {
                var permitted = false;

                var socialMediaAccount = db.socialmediaaccounts.FirstOrDefault(s => s.userName == requestEvt.userName);

                if(socialMediaAccount != null) {
                    if(socialMediaAccount.ownerType == (int)SocialMediaOwnerType.User) {
                        if(player.getCharacterId() == socialMediaAccount.ownerId) {
                            permitted = true;
                        }
                    } else if (socialMediaAccount.ownerType == (int)SocialMediaOwnerType.Company) {
                        var company = CompanyController.getCompanyById(socialMediaAccount.ownerId);

                        if(company != null) {
                            if(CompanyController.hasPlayerPermission(player, company, "SOCIAL_MEDIA_ACCESS")) {
                                socialMediaAccount.currentLoginAttempt = $"{player.getCharacterId()}#{DateTime.Now}";
                                db.SaveChanges();
                                permitted = true;
                            }
                        }
                    }
                }

                if(permitted) {
                    player.emitCefEventNoBlock(new SocialMediaCredentialsPhone(Config.SocialMediaJWTProvider, socialMediaAccount.guid, player.getLoginToken(), player.getCharacterId(), Config.DatabaseDatabase));
                }
            }
        }

        #endregion


        #region Humhub REST API

        #region Template

        public class UserRequestAccount {
            public int visibility = 0;
            public int status = 1;
            public List<string> tagsField = new List<string>();
            public string language = "de";
            public string authclient = "local";
            public string authclient_id = null;
            public string username { get; set; }
            public string email { get; set; }

            public UserRequestAccount(string username) {
                this.username = username;
                this.email = $"{username}@none.none";
            }
        }

        public class UserRequestPassword {
            public string newPassword { get; set; }
            public bool mustChangePassword = false;

            public UserRequestPassword(string newPassword) {
                this.newPassword = newPassword;
            }
        }

        public class UserRequestProfile {
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string title { get; set; }
            public string gender = null;
            public string street = null;
            public string zip = null;
            public string city = null;
            public string country = null;
            public string state = null;
            public int birthday_hide_year = 0;
            public string birthday = null;
            public string about = null;
            public string phone_private = null;
            public string phone_work = null;
            public string mobile = null;
            public string fax = null;
            public string im_skype = null;
            public string im_xmpp = null;
            public string url = null;
            public string url_facebook = null;
            public string url_linkedin = null;
            public string url_xing = null;
            public string url_youtube = null;
            public string url_vimeo = null;
            public string url_flickr = null;
            public string url_myspace = null;
            public string url_twitter = null;

            public UserRequestProfile(string firstname, string lastname, string title, string mobile) {
                this.firstname = firstname;
                this.lastname = lastname;
                this.title = title;
                this.mobile = mobile;
            }
        }

        public class UserRequestRoot {
            public UserRequestAccount account { get; set; }
            public UserRequestProfile profile { get; set; }
            public UserRequestPassword password { get; set; }

            public UserRequestRoot(string username, string firstname, string lastname, string title, string phoneNumber, string newPassword) {
                account = new UserRequestAccount(username);
                profile = new UserRequestProfile(firstname, lastname, title, phoneNumber);
                password = new UserRequestPassword(newPassword);
            }
        }

        #endregion

        public record AccountCreationAnswer(bool success, string message, string guid);
        private static AccountCreationAnswer createAccountViaRest(string userName, string firstName, string lastName, string title = "", string phoneNumber = null) {
            var responseJson = WebRESTCallController.makeWebRESTCall(
                $"{Config.SocialMediaHost}/api/v1/user", 
                "POST",
                new UserRequestRoot(userName, firstName, lastName, title, phoneNumber, Guid.NewGuid().ToString()).ToJson(),
                "application/json", 
                Config.SocialMediaUser, 
                Config.SocialMediaPassword);

            var response = responseJson.FromJson<ExpandoObject>();
            var responseDictionary = (IDictionary<string, object>)response;

            if(responseDictionary.ContainsKey("code")) {
                return new AccountCreationAnswer(false, responseDictionary["message"].ToString(), null);
            } else {
                return new AccountCreationAnswer(true, "Account created successfully", responseDictionary["guid"].ToString());
            }
        }


        private record UserInfoAnswer(bool success, int id, string firstName, string lastName);
        private static UserInfoAnswer getUserInfoViaRest(string userName) {
            var responseJson = WebRESTCallController.makeWebRESTCall(
                $"{Config.SocialMediaHost}/api/v1/user/get-by-username?username={userName}",
                "GET",
                "",
                "application/json",
                Config.SocialMediaUser,
                Config.SocialMediaPassword);

            var response = responseJson.FromJson<ExpandoObject>();
            var responseDictionary = (IDictionary<string, object>)response;

            if(responseDictionary.ContainsKey("code")) {
                return new UserInfoAnswer(false, -1, "", "");
            } else {
                var profile = (IDictionary<string, object>)responseDictionary["profile"];
                var firstName = "";
                if(profile.ContainsKey("firstname") && profile["firstname"] != null) {
                    firstName = profile["firstname"].ToString();
                }
                var lastName = "";
                if(profile.ContainsKey("lastname") && profile["lastname"] != null) {
                    lastName = profile["lastname"].ToString();
                }
                return new UserInfoAnswer(true, int.Parse(responseDictionary["id"].ToString()), firstName, lastName);
            }
        }

        private static bool deleteAllSessionsForUser(string userName) {
            var info = getUserInfoViaRest(userName);

            if(info.success) {
                WebRESTCallController.makeWebRESTCall(
                $"{Config.SocialMediaHost}/api/v1/user/session/all/{info.id}",
                "DEL",
                "",
                "application/json",
                Config.SocialMediaUser,
                Config.SocialMediaPassword);

                return true;
            } else {
                return false;
            }
        }

        #endregion
    }

}
