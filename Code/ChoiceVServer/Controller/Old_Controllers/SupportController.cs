//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ChoiceVServer.Controller {
//    class SupportController : ChoiceVScript {
//        public SupportController() {
//            EventController.addMenuEvent("Support_SetInvis", onPlayerInvisible); 
//            EventController.addMenuEvent("Support_SetInvinc", onPlayerInvincible);
//            EventController.addMenuEvent("Support_Message_Everyone", onMessageEveryone);
//            EventController.addMenuEvent("Support_PlayerInteraction", onSupportPlayerInteraction);

//            EventController.addMenuEvent("Support_SendMessage", onPlayerMessage);
//            EventController.addMenuEvent("Support_TeleportToPlayer", onPlayerTeleport);
//            EventController.addMenuEvent("Support_TeleportPlayerToMe", onPlayerTeleport);
//            EventController.addMenuEvent("Support_OpenPlayerInv", onPlayerOpenInv);
//            EventController.addMenuEvent("Support_KickPlayer", onPlayerKick);
//            EventController.addMenuEvent("Support_BanPlayer", onPlayerBan);

//            EventController.addMenuEvent("Playersupport_ShowPlayers", OnPlayershowNearPlayer);
//            EventController.addMenuEvent("Playersupport_Start_Recording", OnRecording);
//            EventController.addMenuEvent("Playersupport_Stop_Recording", OnRecording);
//            EventController.addMenuEvent("Playersupport_CreateTicket", onPlayerCreateTicket);

//            CharacterController.addSelfMenuElement(new UnconditionalPlayerSelfMenuElement(getSupportMenu()));
//        }

//        private bool onMessageEveryone(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            try {
//                InputMenuItem.InputMenuItemEvent menuItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//                foreach (var target in ChoiceVAPI.GetAllPlayers()) {
//                    target.sendNotification(Constants.NotifactionTypes.Warning, menuItem.input, "Servernachricht");
//                }
//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//            return true;
//        }

//        private bool OnPlayershowNearPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            System.Text.StringBuilder idStringBuilder = new System.Text.StringBuilder();
//            foreach (var target in getNearPlayerByRange(player, 300)) {
//                idStringBuilder.Append($"{target.getAccountId()}#");
//            }
//            player.sendNotification(Constants.NotifactionTypes.Warning, idStringBuilder.ToString(), "");
//            return true;
//        }

//        private bool OnRecording(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (itemEvent == "Playersupport_Start_Recording") {
//                player.emitClientEvent("CLIP", true);
//            } else if (itemEvent == "Playersupport_Stop_Recording") {
//                player.emitClientEvent("CLIP", false);
//            }
//            return true;
//        }

//        private bool onPlayerCreateTicket(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            if (player.hasData("LastSupportTicket")) {
//                var lastDateJson = (string)player.getData("LastSupportTicket");
//                var lastDate = lastDateJson.FromJson<DateTime>();
//                if (DateTime.Now < lastDate.AddMinutes(20)) {
//                    player.sendBlockNotification("Du kannst nicht schon wieder ein Ticket erstellen!", "");
//                    return true;
//                }
//            }
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            var description = "Null";
//            if (inputItem.input != "" && inputItem.input != null) {
//                description = inputItem.input;
//            }
//            System.Text.StringBuilder idStringBuilder = new System.Text.StringBuilder();
//            System.Text.StringBuilder socialClubStringBuilder = new System.Text.StringBuilder();
//            foreach (var target in getNearPlayerByRange(player, 300)) {
//                idStringBuilder.Append($"{target.getAccountId()}#");
//                socialClubStringBuilder.Append($"{target.getData("SOCIALCLUB")}#");
//            }
//            using (var db = new ChoiceVDb()) {
//                var ticket = new supporttickets {
//                    playerId = player.getAccountId(),
//                    playersocialClub = player.getData("SOCIALCLUB"),
//                    nearPlayerIds = idStringBuilder.ToString(),
//                    nearPlayerSocialClubs = socialClubStringBuilder.ToString(),
//                    description = description,
//                    date = DateTime.Now,
//                    position = player.Position.ToJson(),
//                };
//                db.supporttickets.Add(ticket);
//                db.SaveChanges();
//            }
//            player.sendNotification(Constants.NotifactionTypes.Success, "Das Supportticket wurde erstellt. Bitte melde dich im Support um den ganzen Fall abzuschließen!", "");
//            return true;
//        }
//        #region player
//        public static Menu getSupportMenu() {
//            var menu = new Menu("Support Menu", "Was möchtest du machen?");
//            menu.addMenuItem(new ClickMenuItem("Spieler in der Nähe", "", "", "Playersupport_ShowPlayers"));
//            menu.addMenuItem(new ClickMenuItem("Gta-Aufnahme starten", "", "", "Playersupport_Start_Recording"));
//            menu.addMenuItem(new ClickMenuItem("Gta-Aufnahme stoppen", "", "", "Playersupport_Stop_Recording"));
//            menu.addMenuItem(new InputMenuItem("Support Ticket erstellen", "Erstellt ein Supportticket", "Kurze Info", "Playersupport_CreateTicket"));
//            return menu;
//        }
//        #endregion

//        #region Support
//        private bool onPlayerMessage(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var target = (IPlayer)data["Playerhandle"];
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            if (inputItem.input != "" && inputItem != null) {
//                target.sendNotification(Constants.NotifactionTypes.Warning, inputItem.input, "Support Nachricht");
//            }
//            return true;
//        }

//        private bool onPlayerTeleport(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var target = (IPlayer)data["Playerhandle"];
//            if (itemEvent == "Support_TeleportToPlayer") {
//                player.Position = target.Position;
//            }
//            if (itemEvent == "Support_TeleportPlayerToMe") {
//                target.Position = player.Position;
//            };
//            return true;
//        }

//        private bool onPlayerOpenInv(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var target = (IPlayer)data["Playerhandle"];
//            InventoryController.showMoveInventory(player, player.getInventory(), target.getInventory());
//            player.sendNotification(Constants.NotifactionTypes.Info, $"Inventar von {target.getCharacterName()}", "");
//            return true;
//        }

//        private bool onPlayerKick(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var target = (IPlayer)data["Playerhandle"];
//            target.sendNotification(Constants.NotifactionTypes.Warning, "Du wurdest gekickt. Melde dich im Support!", "");
//            InvokeController.AddTimedInvoke("KickInvoke", (ivk) => {
//                target.Kick("Du wurdest gekickt. Melde dich im Support!");
//            }, TimeSpan.FromSeconds(2), false);
//            return true;
//        }

//        private bool onPlayerBan(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var target = (IPlayer)data["Playerhandle"];
//            InputMenuItem.InputMenuItemEvent inputItem = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
//            target.sendNotification(Constants.NotifactionTypes.Warning, "Du wurdest gebannt. Melde dich im Support!", "");
//            InvokeController.AddTimedInvoke("KickInvoke", (ivk) => {
//                target.ban(inputItem.input);
//            }, TimeSpan.FromSeconds(2), false);
//            return true;
//        }

//        private bool onPlayerInvisible(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var check = data["Check"];
//            if (!player.HasSyncedMetaData("PLAYER_INVISIBLE")) {
//                player.SetSyncedMetaData("PLAYER_INVISIBLE", false);
//            }
//            player.GetSyncedMetaData<bool>("PLAYER_INVISIBLE", out var flag);
//            player.SetSyncedMetaData("PLAYER_INVISIBLE", !flag);
//            if (check) {
//                player.resetData("Support_Player_Invis");
//            } else {
//                player.setData("Support_Player_Invis", true);
//            }
//            return true;
//        }
//        private bool onPlayerInvincible(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var check = data["Check"];
//            if (check) {
//                player.emitClientEvent("InvincibleState", false);
//                player.resetData("Support_Player_Invinc");
//                player.sendNotification(Constants.NotifactionTypes.Info, "Du bist ab jetzt sterblich", "");
//            } else {
//                player.emitClientEvent("InvincibleState", true);
//                player.setData("Support_Player_Invinc", true);
//                player.sendNotification(Constants.NotifactionTypes.Info, "Du bist ab jetzt Unsterblich", "");
//            }
//            return true;
//        }
//        private bool onSupportPlayerInteraction(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var target = (IPlayer)data["Playerhandle"];
//            var menu = new Menu($"{target.getCharacterName()}", "");

//            menu.addMenuItem(new InputMenuItem("Spieler benachrichtigen", "Sendet dem Spieler einen Text", "Text", "Support_SendMessage").withData(new Dictionary<string, dynamic> { { "Playerhandle", target } }));
//            menu.addMenuItem(new ClickMenuItem("Zu Spieler teleportieren", "Teleportiert dich zum Spieler", "", "Support_TeleportToPlayer").withData(new Dictionary<string, dynamic> { { "Playerhandle", target } }));
//            menu.addMenuItem(new ClickMenuItem("Spieler zu mir teleportieren", "Teleportiert dich zum Spieler", "", "Support_TeleportPlayerToMe").withData(new Dictionary<string, dynamic> { { "Playerhandle", target } }));
//            menu.addMenuItem(new ClickMenuItem("Spieler Inventar öffnen", "Zeigt das Spieler Inventar", "", "Support_OpenPlayerInv").withData(new Dictionary<string, dynamic> { { "Playerhandle", target } }));
//            menu.addMenuItem(new ClickMenuItem("Spieler kicken", "Kickt den Spieler", "", "Support_KickPlayer").needsConfirmation("Spieler sicher Kicken?", "Kickt den Spieler vom Server").withData(new Dictionary<string, dynamic> { { "Playerhandle", target } }));
//            menu.addMenuItem(new InputMenuItem("Spieler Bannen", "Bannt den Spieler", "", "Support_BanPlayer").needsConfirmation("Spieler sicher Bannen?", "Bannt den Spieler vom Server").withData(new Dictionary<string, dynamic> { { "Playerhandle", target } }));

//            player.showMenu(menu);
//            return true;
//        }

//        public static void SupportMenu(IPlayer player) {
//            var adminLevel = player.getAdminLevel();
//            var menu = new Menu("SupportMenu", "Was möchtest du machen?");
//            var playerMenu = new Menu("Alle Spieler", "Links Char name. Rechts SocialClub");
//            var playerList = ChoiceVAPI.GetAllPlayers().OrderBy(x => x.getCharacterName()).ToList();
//            foreach (var target in playerList) {
//                playerMenu.addMenuItem(new ClickMenuItem($"{target.getCharacterName()}", "", $"{target.getData("SOCIALCLUB")}", "Support_PlayerInteraction").withData(new Dictionary<string, dynamic> { {"Playerhandle", target } }));
//            }

//            if (adminLevel >= 1) {
//                menu.addMenuItem(new CheckBoxMenuItem("Unsichtbar", "Macht dich unsichtbar", player.hasData("Support_Player_Invis"), "Support_SetInvis").withData(new Dictionary<string, dynamic> { { "Check", player.hasData("Support_Player_Invis") } }));
//                menu.addMenuItem(new CheckBoxMenuItem("Unsterblich", "Macht dich unsterblich", player.hasData("Support_Player_Invinc"), "Support_SetInvinc").withData(new Dictionary<string, dynamic> { { "Check", player.hasData("Support_Player_Invinc") } }));
//                menu.addMenuItem(new InputMenuItem("Nachricht an alle Spieler", "Sendet eine Nachricht an alle Spieler", "", "Support_Message_Everyone"));
//                menu.addMenuItem(new MenuMenuItem("Aktive Spieler", playerMenu));

//            }

//            if (adminLevel >= 2) {

//            }

//            if (adminLevel >= 3) {

//            }

//            player.showMenu(menu);
//        }

//        #endregion

//        #region Helper
//        public static List<IPlayer> getNearPlayerByRange(IPlayer player, int Range) {
//            List<IPlayer> playerList = new List<IPlayer>();
//            foreach (var target in ChoiceVAPI.GetAllPlayers()) {
//                if (player.Position.Distance(target.Position) <= Range) {
//                    playerList.Add(target);
//                }
//            }
//            return playerList;
//        }
//        #endregion
//    }
//}
