using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Minijobs.Model;
using ChoiceVServer.Controller.Minijobs.Modules;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class JobAnnouncement {
        public int Id;
        public int OwnerId;

        public string Title;
        public string ShortDescription;
        public string Message;
        public string PhoneNumber;

        public DateTime ExpireDate;

        public JobAnnouncement(int id, int ownerId, string title, string shortDescription, string message, string phoneNumber, DateTime expireDate) {
            Id = id;
            OwnerId = ownerId;
            Title = title;
            ShortDescription = shortDescription;
            Message = message;
            PhoneNumber = phoneNumber;
            ExpireDate = expireDate;
        }
    }

    public class AnnouncementController : ChoiceVScript {
        public static List<JobAnnouncement> AllAnnouncements;
        private static CollisionShape CollisionShape;

        private static decimal ANNOUNCEMENT_COST = 100;

        public AnnouncementController() {
            AllAnnouncements = new List<JobAnnouncement>();

            EventController.addMenuEvent("CREATE_ANNOUNCEMENT", onAnnoucementCreate);
            EventController.addMenuEvent("REMOVE_ANNOUNCEMENT", onAnnoucementRemove);

            loadAllAnnouncements();

        }

        #region Announcement Stuff

        private static void loadAllAnnouncements() {
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.jobannouncements) {
                    if(row.expireDate > DateTime.Now) {
                        AllAnnouncements.Add(new JobAnnouncement(row.id, row.ownerId, row.title, row.shortDescription, row.message, row.phoneNumber, row.expireDate));
                    } else {
                        db.jobannouncements.Remove(row);
                    }
                }

                db.SaveChanges();
            }
        }

        public static void onCollisionShapeInteract(IPlayer player) {
            var menu = new Menu("Job-Ausschreibungen", "Siehe Ausschreibungen oder verfasse sie");

            var createMenu = new Menu("Ausschreibung erstellen", $"Dies kostet ${ANNOUNCEMENT_COST}!");
            createMenu.addMenuItem(new InputMenuItem("Titel", "Füge einen Titel hinzu. Nicht mehr als 20 Zeichen", "Titel", "DONTCARE"));
            createMenu.addMenuItem(new InputMenuItem("Kurze Beschreibung", "Beschreibe die Aktivität kurz. Nicht mehr als 35 Zeichen", "Kurz-Beschreibung", "DONTCARE"));
            createMenu.addMenuItem(new InputMenuItem("Nachricht", "Füge der Ausschreibung eine Nachricht hinzu.", "Nachricht", "DONTCARE"));
            createMenu.addMenuItem(new InputMenuItem("Telefonnummer", "Füge der Ausschreibung eine Telefonnummer hinzu", "Telefonnummer", "DONTCARE"));
            createMenu.addMenuItem(new MenuStatsMenuItem("Abschicken", "Schicke die oben eingetragenen Informationen an den Server", "CREATE_ANNOUNCEMENT", MenuItemStyle.green));

            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            foreach(var announcement in AllAnnouncements) {
                var annMenu = new Menu(announcement.Title, announcement.ShortDescription);
                annMenu.addMenuItem(new StaticMenuItem("Nachricht", announcement.Message, ""));
                annMenu.addMenuItem(new StaticMenuItem("Telefonnummer", announcement.PhoneNumber, ""));
                if(player.getCharacterId() == announcement.OwnerId) {
                    var data = new Dictionary<string, dynamic> { { "Announcement", announcement } };
                    var item = new ClickMenuItem("Ausschreibung löschen", "Lösche die Ausschreibung. Geld gibt es nicht zurück", "", "REMOVE_ANNOUNCEMENT", MenuItemStyle.red);
                    item.withData(data);
                    item.needsConfirmation("Ausschreibung entfernen?", "Ausschreibung wirklich entfernen?");
                    annMenu.addMenuItem(item);
                }

                menu.addMenuItem(new MenuMenuItem(annMenu.Name, annMenu));
            }

            player.showMenu(menu);
        }

        private bool onAnnoucementCreate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var titleEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var shortDescEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var messageEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var phoneEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            if(player.removeCash(ANNOUNCEMENT_COST)) {
                var expireDate = DateTime.Now + TimeSpan.FromHours(48);
                using(var db = new ChoiceVDb()) {
                    var dbAnn = new jobannouncement {
                        ownerId = player.getCharacterId(),
                        title = titleEvt.input,
                        shortDescription = shortDescEvt.input,
                        message = messageEvt.input,
                        phoneNumber = phoneEvt.input,
                        expireDate = DateTime.Now + TimeSpan.FromHours(48),
                    };

                    db.Add(dbAnn);
                    db.SaveChanges();
                    AllAnnouncements.Add(new JobAnnouncement(dbAnn.id, player.getCharacterId(), titleEvt.input, shortDescEvt.input, messageEvt.input, phoneEvt.input, expireDate));
                }
                player.sendNotification(Constants.NotifactionTypes.Success, "Deine Ausschreibung wurde erfolgreich erstellt", "Ausschreibung erstellt", Constants.NotifactionImages.MiniJob);
            } else {
                player.sendBlockNotification($"Nicht genug Bargeld. Du benötigst ${ANNOUNCEMENT_COST} Bargeld", $"${ANNOUNCEMENT_COST} benötigt", Constants.NotifactionImages.MiniJob);
            }

            return true;
        }

        private bool onAnnoucementRemove(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var announcement = (JobAnnouncement)data["Announcement"];
            if(AllAnnouncements.Remove(announcement)) {
                using(var db = new ChoiceVDb()) {
                    var dbAnn = db.jobannouncements.Find(announcement.Id);
                    db.Add(dbAnn);
                    db.SaveChanges();
                }
                player.sendNotification(Constants.NotifactionTypes.Info, "Deine Ausschreibung wurde entfernt!", "Ausschreibung entfernt", Constants.NotifactionImages.MiniJob);
            } else {
                player.sendBlockNotification("Die Ausschreibung konnte nicht entfernt werden. Vielleicht wurde sie schon entfernt.", "Auscchreibung nicht entfernt", Constants.NotifactionImages.MiniJob);
            }

            return true;
        }

        #endregion
    }

    //TODO Keine Minijobs wenn im Dienst

    public class MiniJobController : ChoiceVScript {
        private static Dictionary<string, Action<IPlayer, Action<string, Dictionary<string, string>>>> TaskCreator = new();

        public static List<Minijob> Minijobs = new();

        public static Dictionary<MiniJobTypes, int> MinijobCountPerType = new();

        public enum MiniJobTypes : int {
            CityAdmin = 0,
            Prison = 1,
            Placeholder2 = 2,
            Placeholder3 = 3,
            Placeholder4 = 4,
            Placeholder5 = 5,
            Placeholder6 = 6,
        }

        public MiniJobController() {
            EventController.MainReadyDelegate += loadMinijobs;

            #region Admin Stuff

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Misc,
                    "Minijobs",
                    generateMinjobMenu
                )
            );

            EventController.addMenuEvent("ADMIN_CREATE_MINIJOB", onAdminCreateMinijob);
            EventController.addMenuEvent("ADMIN_DELETE_TASK", onAdminDeleteTask);
            EventController.addMenuEvent("ADMIN_CREATE_TASK", onAdminCreateTask);
            EventController.addMenuEvent("ADMIN_ON_CHANGE_TASK_ORDER", onAdminChangeTaskOrder);

            EventController.addMenuEvent("ADMIN_CREATE_MINIJOB_REWARD", onAdminCreateMinijobReward);
            EventController.addMenuEvent("ADMIN_DELETE_MINIJOB_REWARD", onAdminDeleteMinijobReward);

            #endregion

            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    playerMinijobInteract,
                    s => s is IPlayer player && player.hasData("CURRENT_MINIJOB"),
                    t => t is IPlayer player && !player.hasData("CURRENT_MINIJOB")
                )
            );

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    (p) => new ClickMenuItem("Minijob abbrechen", "Brich den Minijob ab. Falls du die einzige noch bearbeitende Person bist wird der Minijob zurückgesetzt", "", "ON_PLAYER_CANCEL_MINIJOB", MenuItemStyle.yellow).needsConfirmation("Job verlassen?", "Möchtest du aus dem Job ausscheiden?"),
                    s => s.hasData("CURRENT_MINIJOB")
                )
            );
            EventController.addMenuEvent("ON_PLAYER_CANCEL_MINIJOB", onPlayerCancelMinijob);

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    "Job-Belohnung wählen",
                    minijobRewardGetter,
                    sender => sender.hasData("MINIJOB_REWARD_OPTIONS")
                )
            );
            EventController.addMenuEvent("MINIJOB_SELECT_REWARD", onMinijobSelectSelectReward);

            EventController.addMenuEvent("MINIJOB_TAKE_JOB", onMinijobTakeJob);
            EventController.addMenuEvent("MINIJOB_ADD_PERSON", onMinijobAddPerson);

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;
            CharacterController.addPlayerConnectDataSetCallback("ADD_TO_MINIJOB", onConnectAddToMinijob);

            InvokeController.AddTimedInvoke("Minijob-Ender", updateMinijobs, TimeSpan.FromMinutes(3), true);

            PedController.addNPCModuleGenerator("Minijobauswahl-Modul", onMinijobModuleGenerator, onMinijobModuleCallback);
            PedController.addNPCModuleGenerator("Nur-mit-Minijob-sichtbar-Modul", onMinijobVisibleModuleGenerator, onMinijobVisibleModuleCallback);
        }

        #region Module

        private List<MenuItem> onMinijobModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCMinijobDistributerModule);

            var list = new List<MenuItem>();

            list.Add(new ListMenuItem("Typ auswählen", "Wähle welcher Typ Minijobs hier zur Verfügung stehen soll", Enum.GetValues<MiniJobTypes>().Select(t => t.ToString()).ToArray(), ""));
            list.Add(new InputMenuItem("Anzahl auswählen", "Wähle aus viele Jobs hier gleichzeitig zur Verfügung stehen sollen", "", ""));

            return list;
        }

        private void onMinijobModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var listEvt = evt.elements[0].FromJson<ListMenuItemEvent>();
            var inputEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var type = Enum.Parse<MiniJobTypes>(listEvt.currentElement);
            var amount = int.Parse(inputEvt.input);

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "Type", type }, { "AmountOfJobs", amount } });
        }


        private List<MenuItem> onMinijobVisibleModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCOnlyVisibleInMinijobModule);

            var list = new List<MenuItem>();

            list.Add(new ListMenuItem("Minijob", "Wähle bei welchem Minijob der NPC sichtbar sein soll", Minijobs.Select(m => m.Name).ToArray(), ""));

            return list;
        }

        private void onMinijobVisibleModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var minijobEvt = evt.elements[0].FromJson<ListMenuItemEvent>();

            var minijobs = Minijobs.Where(m => m.Name == minijobEvt.currentElement).ToList();

            Minijob job;
            if(minijobs.Count > 0) {
                job = minijobs.MinBy((m) => Math.Abs(m.Id - minijobEvt.currentIndex));
            } else {
                job = minijobs.First();
            }

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic> { { "MinijobId", job.Id } });
        }

        #endregion

        private void updateMinijobs(IInvoke obj) {
            foreach(var minijob in Minijobs) {
                minijob.update();
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            if(player.hasData("CURRENT_MINIJOB")) {
                var minijob = (Minijob)player.getData("CURRENT_MINIJOB");
                var instance = minijob.getPlayerInstance(player);
                player.setPermanentData("ADD_TO_MINIJOB", $"{minijob.Id}#{minijob.getPlayerCurrentTask(player)}#{DateTime.Now.ToJson()}#{instance.ConnectedPlayersIds.ToJson()}");
                minijob.removePlayerForNow(player);
            }
        }

        private bool onPlayerCancelMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.hasData("CURRENT_MINIJOB")) {
                var minijob = (Minijob)player.getData("CURRENT_MINIJOB");
                minijob.removePlayerFromMinijob(player);

                player.setPermanentData("MINIJOB_BLOCKED", (DateTime.Now + TimeSpan.FromMinutes(30)).ToJson());

                player.sendNotification(Constants.NotifactionTypes.Warning, "Job abgebrochen! Du hast eine 30min Sperre für weitere Minijobs erhalten.", "Job abgebrochen!", Constants.NotifactionImages.MiniJob);
            }

            return true;
        }

        private void onConnectAddToMinijob(IPlayer player, character character, characterdatum data) {
            var split = data.value.Split("#");
            var id = int.Parse(split[0]);
            var currentTask = int.Parse(split[1]);
            var date = split[2].FromJson<DateTime>();
            var connectedIds = split[3].FromJson<List<int>>();

            var job = Minijobs.FirstOrDefault(m => m.Id == id);
            if(job != null) {
                if(DateTime.Now - date < TimeSpan.FromHours(1)) {
                    if(job.Ongoing) {
                        foreach(var otherPlayerId in connectedIds) {
                            if(job.isPlayerByIdDoingMinijob(otherPlayerId)) {
                                var instance = job.getPlayerByIdInstance(otherPlayerId);
                                player.sendNotification(Constants.NotifactionTypes.Info, "Du wurdest wieder zum Job hinzugefügt!", "Job wiederaufgenommen", Constants.NotifactionImages.MiniJob);
                                job.addPlayerToMinijob(instance, player);

                                return;
                            }
                        }
                        player.sendNotification(Constants.NotifactionTypes.Warning, "Der Minijob, den du angefangen hattest wird leider von jemand anderem ausgeführt!", "Job nicht wiederaufgenommen", Constants.NotifactionImages.MiniJob);
                    } else {
                        player.sendNotification(Constants.NotifactionTypes.Info, "Du wurdest wieder zum Job hinzugefügt!", "Job wiederaufgenommen", Constants.NotifactionImages.MiniJob);
                        job.startMiniJob(player, currentTask);
                    }
                }
            } else {
                player.sendBlockNotification("Der Job der angefangen wurde existiert nicht mehr!", "Job existiert nicht mehr!");
            }
        }

        public static Menu getMinijobMenu(MiniJobTypes type, IPlayer otherPlayer = null) {
            var minijobs = Minijobs.Where(m => m.Type == type && (!m.Ongoing || m.isMultiViable())).ToList();

            var menu = new Menu("Minijobauswahl", "Was möchtest du tun?");

            foreach(var minijob in minijobs) {
                var menuItem = minijob.getRepresentative(otherPlayer);

                menu.addMenuItem(new MenuMenuItem(menuItem.Name, menuItem));
            }

            return menu;
        }

        public static void openMinijobMenu(IPlayer player, MiniJobTypes type, IPlayer otherPlayer = null) {
            if(checkPlayerMinijobBlocked(player) > DateTime.Now) {
                player.sendNotification(Constants.NotifactionTypes.Warning, "Du bist noch blockiert Minijobs zu spielen!", "Blockiert", Constants.NotifactionImages.MiniJob);
                return;
            }

            player.showMenu(getMinijobMenu(type, otherPlayer));
        }

        public static bool onMinijobTakeJob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var minijob = (Minijob)data["Minijob"];
            IPlayer originalPlayer = null;
            if(data.ContainsKey("Player")) {
                originalPlayer = (IPlayer)data["Player"];
            }

            if(!minijob.Ongoing || minijob.isMultiViable()) {
                minijob.startMiniJob(player);

                if(data.ContainsKey("GivingModule")) {
                    var givingModule = (NPCMinijobDistributerModule)data["GivingModule"];
                    givingModule.AvailableMinijobs.Remove(minijob);
                }
            } else {
                var host = (IPlayer)data["Player"];
                if(player != null && player.Exists()) {
                    var instance = minijob.getPlayerInstance(host);
                    if(instance != null) {
                        minijob.addPlayerToMinijob(instance, player);

                        originalPlayer?.sendNotification(Constants.NotifactionTypes.Info, "Die Person hat das Minijob-Angebot angenommen!", "JoPersonb hinzugefügt", Constants.NotifactionImages.MiniJob);
                    }
                } else {
                    player.sendBlockNotification("Etwas ist schiefgelaufen!", "Fehler");
                }
            }
            return true;
        }

        private bool onMinijobAddPerson(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["InteractionTarget"];

            var minijob = (Minijob)player.getData("CURRENT_MINIJOB");

            target.showMenu(minijob.getRepresentative(player));

            return true;
        }

        private MenuItem playerMinijobInteract(IEntity player, IEntity target) {
            var minijob = (Minijob)player.getData("CURRENT_MINIJOB");
            var targetPlayer = target as IPlayer;
            if(minijob.canPlayerDoMinijob(targetPlayer) && checkPlayerMinijobBlocked(targetPlayer) < DateTime.Now) {
                return new ClickMenuItem("Person zu Job hinzufügen", "Füge die Person zu deinem aktuellen Job hinzu. Dies teilt ggf. die Belohnung am Ende auf!", "", "MINIJOB_ADD_PERSON")
                    .needsConfirmation("Person hinzufügen?", "Person wirklich hinzufügen?");
            } else {
                return new StaticMenuItem("Hinzufügen zu Job nicht möglich", $"Die Person kann nicht zum aktuellen Job hinzugefügt werden", "", MenuItemStyle.yellow);
            }
        }

        private Menu minijobRewardGetter(IPlayer player) {
            var menu = new Menu("Job-Belohnung wählen", "Welche Belohnung möchtest du?");

            var options = ((string)player.getData("MINIJOB_REWARD_OPTIONS")).FromJson<List<RewardOption>>();
            var count = int.Parse((string)player.getData("MINIJOB_REWARD_COUNT"));

            for(var i = 0; i < options.Count; i++) {
                var cashReward = options[i].CashReward / count;

                var optionMenu = new Menu($"Belohnung {i + 1}", "Wähle diese Belohnung");
                optionMenu.addMenuItem(new StaticMenuItem("Cash-Belohnung", $"In dieser Option befinden sich ${cashReward}", $"${cashReward}"));
                for(var j = 0; j < options[i].Items.Count; j++) {

                    var cfg = InventoryController.getConfigById(options[i].Items[j]);
                    var amount = options[i].ItemsAmount[j];

                    optionMenu.addMenuItem(new StaticMenuItem(cfg.name, $"In dieser Option befinden sich {amount}x {cfg.name}", $"{amount}x"));
                }

                var data = new Dictionary<string, dynamic> { { "Reward", options[i] }, { "Count", count } };
                optionMenu.addMenuItem(new ClickMenuItem("Belohnung wählen", "Wähle die oben aufgeführten Belohnungen", "", "MINIJOB_SELECT_REWARD", MenuItemStyle.green).withData(data).needsConfirmation("Belohnung wählen?", "Wirklich wählen?"));

                menu.addMenuItem(new MenuMenuItem(optionMenu.Name, optionMenu));
            }

            return menu;
        }

        private bool onMinijobSelectSelectReward(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var reward = (RewardOption)data["Reward"];
            var count = (int)data["Count"];

            player.resetPermantData("MINIJOB_REWARD_OPTIONS");
            player.resetPermantData("MINIJOB_REWARD_COUNT");

            givePlayerReward(player, reward, count);

            return true;
        }

        internal static void givePlayerReward(IPlayer player, RewardOption option, int playerCount) {
            var str = "Du hast ";
            if(option.CashReward > 0) {
                var mult = 1m;
                if(playerCount > 1) {
                    mult = 1.33m;
                }
                player.addCash(option.CashReward * mult / playerCount);
                str += $"${option.CashReward * mult / playerCount}";
            }

            for(var i = 0; i < option.Items.Count; i++) {
                var cfg = InventoryController.getConfigById(option.Items[i]);
                var items = InventoryController.createItems(cfg, option.ItemsAmount[i]);
                foreach(var item in items) {
                    player.getInventory().addItem(item, true);
                }
                str += $", {option.ItemsAmount[i]}x {cfg.name}";
            }

            str += " erhalten!";
            player.sendNotification(Constants.NotifactionTypes.Success, str, "Belohnung erhalten");
        }

        private void loadMinijobs() {
            Minijobs = new();

            using(var db = new ChoiceVDb()) {
                foreach(var job in db.configminijobs.Include(m => m.configminijobstasks).ThenInclude(t => t.configminijobstaskssettings)) {
                    var minijob = new Minijob(job.id, (MiniJobTypes)job.type, job.name, job.description, job.requirements, job.information, job.workTimeHour, job.maxUses, job.rewardJson.FromJson<List<RewardOption>>() ?? new List<RewardOption>(), job.blockMultiUse == 1);

                    foreach(var dbTask in job.configminijobstasks) {
                        var settingsDic = dbTask.configminijobstaskssettings.ToList().ToDictionary(s => s.name, s => s.value);

                        var type = Type.GetType($"ChoiceVServer.Controller.Minijobs.Model.{dbTask.codeItem}", true);
                        var minijobTask = (MinijobTask)Activator.CreateInstance(type, minijob, dbTask.id, dbTask.order, dbTask.colShape, settingsDic);
                        minijob.addMinijobTask(minijobTask);
                    }

                    Minijobs.Add(minijob);

                    if(MinijobCountPerType.ContainsKey(minijob.Type)) {
                        MinijobCountPerType[minijob.Type] += 1;
                    } else {
                        MinijobCountPerType[minijob.Type] = 1;
                    }
                }
            }
        }

        public static DateTime checkPlayerMinijobBlocked(IPlayer player) {
            if(player.hasData("MINIJOB_BLOCKED")) {
                var blockedDate = ((string)player.getData("MINIJOB_BLOCKED")).FromJson<DateTime>();

                if(blockedDate <= DateTime.Now) {
                    player.resetPermantData("MINIJOB_BLOCKED");
                    return DateTime.MinValue;
                }

                return blockedDate;
            }

            return DateTime.MinValue;
        }

        #region Admin Stuff

        public static void addMinijobTaskCreator(string name, Action<IPlayer, Action<string, Dictionary<string, string>>> creator) {
            TaskCreator.Add(name, creator);
        }

        private Menu generateMinjobMenu(IPlayer player) {
            var menu = new Menu("Minjob-Erstellung", "Erstelle Minijobs");

            //Already Jobs
            var alreadyMenu = new Menu("Job-Liste", "Aktuell bestehende Minijobs");
            foreach(var minijob in Minijobs) {
                var jobMenu = new VirtualMenu($"{minijob.Id} {minijob.Name}", () => createMinijobMenu(minijob, player));
                alreadyMenu.addMenuItem(new MenuMenuItem(jobMenu.Name, jobMenu));
            }

            menu.addMenuItem(new MenuMenuItem(alreadyMenu.Name, alreadyMenu));

            var createMenu = new Menu("Minijoberstellung", "Gib die Daten ein");

            createMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Minijobs", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Beschreibung", "Beschreibe den Minijob kurz", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Vorraussetzungen", "Beschreibung der Vorraussetzungen. Benötigte Items, und z.B. (Crime Ranghöhe)", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Informationen", "Informationen zum Job. Ungefähre Dauer und empfohlene Spieler", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Max. Bearbeitungszeit (h)", "Gib an wieviele Stunden die Person zum bearbeiten hat.", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Max. Bearbeitungen", "Gib an wie oft der Minijob abgeschlossen werden kann. -1 Steht für unendlich viele Bearbeitungen", "-1", ""));
            var typesList = Enum.GetValues<MiniJobTypes>().Select(t => t.ToString()).ToArray();
            createMenu.addMenuItem(new ListMenuItem("Typ", "Der Typ des Minijobs. Bestimmt ggf. das Vorkommen", typesList, ""));
            createMenu.addMenuItem(new CheckBoxMenuItem("Multi-Use erlauben", "Erlaubt es, wenn möglich (!) mehreren Spielern den Minijob simulatan zu erledigen.", false, ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle den Minijob", "ADMIN_CREATE_MINIJOB", MenuItemStyle.green).needsConfirmation("Erstellen?", "Minijob wirklich erstellen?"));

            menu.addMenuItem(new MenuMenuItem("Minijob erstellen", createMenu, MenuItemStyle.green));
            return menu;
        }

        private static Menu createMinijobMenu(Minijob minijob, IPlayer player) {
            if(player != null) {
                MiniJobController.setSupportFastAction(player, minijob);
            }

            var jobMenu = new Menu($"{minijob.Id} {minijob.Name}", "Was möchstest du tun?");
            jobMenu.addMenuItem(new StaticMenuItem("Beschreibung", minijob.Description, ""));
            jobMenu.addMenuItem(new StaticMenuItem("Informationen", $"Maximale Bearbeitungszeit: {minijob.WorkTime.TotalHours}h. {minijob.Information}", ""));
            jobMenu.addMenuItem(new StaticMenuItem("Vorraussetzungen", minijob.Requirement, ""));
            //Already Tasks
            var tasksMenu = new Menu("Task-Liste", "Aktuell bestehende Tasks");
            foreach(var task in minijob.AllTasks) {
                var data = new Dictionary<string, dynamic> { { "Minijob", minijob }, { "Task", task } };

                var name = task.GetType().Name;
                var taskMenu = new Menu($"{task.Order}: {name}", "Was möchstest du tun?");

                taskMenu.addMenuItem(new InputMenuItem("Reihenfolge", "Die Reihenfolge des Tasks", "", "ADMIN_ON_CHANGE_TASK_ORDER"));

                foreach(var item in task.getAdminMenuElements()) {
                    taskMenu.addMenuItem(item);
                }

                taskMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche den Task", "", "ADMIN_DELETE_TASK", MenuItemStyle.red).withData(data).needsConfirmation("Task löschen?", "Task wirklich löschen?"));

                tasksMenu.addMenuItem(new MenuMenuItem(taskMenu.Name, taskMenu));
            }

            jobMenu.addMenuItem(new MenuMenuItem(tasksMenu.Name, tasksMenu));

            var names = TaskCreator.Keys.ToArray();
            jobMenu.addMenuItem(new ListMenuItem("Task erstellen", "Erstelle einen neuen Task", names, "ADMIN_CREATE_TASK", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Minijob", minijob } }));


            var rewardsMenu = new Menu("Belohnungen", "Wähle eine Belohnung");
            for(var i = 0; i < minijob.RewardOptions.Count; i++) {
                var reward = minijob.RewardOptions[i];
                var data = new Dictionary<string, dynamic> { { "Minijob", minijob }, { "Reward", reward } };

                var rewardMenu = new Menu($"Belohnung {i}", "Was möchtest du tun?");
                rewardMenu.addMenuItem(new StaticMenuItem("Cash", $"Die Belohnung enthält ${reward.CashReward} Cashbelohnung", $"${reward.CashReward}"));
                for(var j = 0; j < reward.Items.Count; j++) {
                    var item = InventoryController.getConfigById(reward.Items[j]);

                    rewardMenu.addMenuItem(new StaticMenuItem($"{item.name}", $"Die Belohnung enthält {reward.ItemsAmount[j]}x {item.name} als Itembelohnung", $"{reward.ItemsAmount[j]}x"));
                }
                rewardMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche die Belohnung", "", "ADMIN_DELETE_MINIJOB_REWARD", MenuItemStyle.red).withData(data).needsConfirmation("Belohnung löschen?", "Wirklich löschen?"));

                rewardsMenu.addMenuItem(new MenuMenuItem(rewardMenu.Name, rewardMenu));
            }
            jobMenu.addMenuItem(new MenuMenuItem(rewardsMenu.Name, rewardsMenu));

            var rewardCreateMenu = new Menu("Belohnung erstellen", "Gib die Daten ein");
            rewardCreateMenu.addMenuItem(new InputMenuItem("Cashbelohnung", "Den Cash der Belohnung. Kann 0 sein", "", ""));
            rewardCreateMenu.addMenuItem(new InputMenuItem("Items", "Die Itembelohnung. Sie sollte im Format: ItemId#Menge,ItemId#Menge,.. sein. z.B. [1#5,78#4]", "", ""));
            rewardCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die angegebene Belohnung", "ADMIN_CREATE_MINIJOB_REWARD", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Minijob", minijob } }).needsConfirmation("Belohnung erstellen?", "Belohnung so erstellen?"));

            jobMenu.addMenuItem(new MenuMenuItem(rewardCreateMenu.Name, rewardCreateMenu, MenuItemStyle.green));

            return jobMenu;
        }

        private bool onAdminCreateMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var requirementEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var informationEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
            var workTimeEvt = evt.elements[4].FromJson<InputMenuItemEvent>();
            var maxUses = evt.elements[5].FromJson<InputMenuItemEvent>();
            var typesEvt = evt.elements[6].FromJson<ListMenuItemEvent>();
            var multiUseEvt = evt.elements[7].FromJson<CheckBoxMenuItem>();

            using(var db = new ChoiceVDb()) {
                var newJob = new configminijob {
                    name = nameEvt.input,
                    description = descriptionEvt.input ?? "",
                    requirements = requirementEvt.input ?? "",
                    information = informationEvt.input ?? "",
                    workTimeHour = float.Parse(workTimeEvt.input),
                    type = (int)Enum.Parse<MiniJobTypes>(typesEvt.currentElement),
                    maxUses = maxUses.input == "" || maxUses.input == null ? -1 : int.Parse(maxUses.input),
                    blockMultiUse = multiUseEvt.Checked ? 0 : 1,
                };

                db.configminijobs.Add(newJob);
                db.SaveChanges();

                setSupportFastAction(player, Minijobs.FirstOrDefault(m => m.Id == newJob.id));
            }

            player.sendNotification(Constants.NotifactionTypes.Success, "Minijob erfolgreich erstellt!", "");

            loadMinijobs();

            return true;
        }

        public static void setSupportFastAction(IPlayer player, Minijob minijob) {
            SupportController.setCurrentSupportFastAction(player, () => {
                player.showMenu(createMinijobMenu(minijob, player));
            });
        }

        private bool onAdminDeleteTask(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var minijob = (Minijob)data["Minijob"];
            var task = (MinijobTask)data["Task"];

            minijob.AllTasks.Remove(task);

            using(var db = new ChoiceVDb()) {
                var dbTask = db.configminijobstasks.FirstOrDefault(m => m.minijobId == minijob.Id && m.id == task.Id);

                if(dbTask != null) {
                    db.configminijobstasks.Remove(dbTask);
                }
            }

            return true;
        }

        private bool onAdminChangeTaskOrder(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            return true;
        }

        private bool onAdminCreateTask(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var miniJob = (Minijob)data["Minijob"];

            var evt = menuItemCefEvent as ListMenuItemEvent;

            var callback = TaskCreator[evt.currentElement];

            player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun die Kollision des Tasks", "");
            callback.Invoke(player, (codeItem, settings) => {
                CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                    var colShape = CollisionShape.Create(p, w, h, r, true, false, true);

                    using(var db = new ChoiceVDb()) {
                        var newTask = new configminijobstask {
                            minijobId = miniJob.Id,
                            codeItem = codeItem,
                            colShape = colShape.toShortSave(),
                            order = miniJob.AllTasks.Count(),
                        };

                        db.configminijobstasks.Add(newTask);
                        db.SaveChanges();

                        foreach(var setting in settings) {
                            var newSetting = new configminijobstaskssetting {
                                taskId = newTask.id,
                                name = setting.Key,
                                value = setting.Value
                            };

                            db.configminijobstaskssettings.Add(newSetting);
                        }

                        db.SaveChanges();
                    }

                    player.sendNotification(Constants.NotifactionTypes.Success, "Minijobtask erfolgreich erstellt", "");
                    loadMinijobs();
                });
            });

            return true;
        }

        private bool onAdminCreateMinijobReward(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var minijob = (Minijob)data["Minijob"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var cashEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var itemsEvt = evt.elements[1].FromJson<InputMenuItemEvent>();


            var cfgList = new List<int>();
            var amountList = new List<int>();

            if(itemsEvt.input != null && itemsEvt.input != "") {
                var itemsSplit = itemsEvt.input.Split(",");
                foreach(var split in itemsSplit) {
                    var subSplit = split.Split("#");

                    cfgList.Add(InventoryController.getConfigById(int.Parse(subSplit[0])).configItemId);
                    amountList.Add(int.Parse(subSplit[1]));
                }
            }

            var option = new RewardOption(decimal.Parse(cashEvt.input), cfgList, amountList, null, 0);

            minijob.RewardOptions.Add(option);

            using(var db = new ChoiceVDb()) {
                var dbJob = db.configminijobs.FirstOrDefault(m => m.id == minijob.Id);

                if(dbJob != null) {
                    dbJob.rewardJson = minijob.RewardOptions.ToJson();

                    db.SaveChanges();
                }
            }

            player.sendNotification(Constants.NotifactionTypes.Success, "Belohnung erfolgreich erstellt!", "Belohnung erstellt");

            return true;
        }

        private bool onAdminDeleteMinijobReward(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var minijob = (Minijob)data["Minijob"];
            var reward = (RewardOption)data["Reward"];

            minijob.RewardOptions.Remove(reward);

            using(var db = new ChoiceVDb()) {
                var dbJob = db.configminijobs.FirstOrDefault(m => m.id == minijob.Id);

                if(dbJob != null) {
                    dbJob.rewardJson = minijob.RewardOptions.ToJson();

                    db.SaveChanges();
                }
            }

            return true;
        }

        #endregion
    }
}