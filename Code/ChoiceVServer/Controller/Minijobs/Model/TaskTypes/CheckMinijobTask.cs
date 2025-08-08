using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.MiniJobController;
using static ChoiceVServer.Controller.Minijobs.Model.Minijob;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Minijobs.Model {
    public class CheckMiniJobTaskController : ChoiceVScript {
        public CheckMiniJobTaskController() {
            EventController.addMenuEvent("ON_CHECK_MINIJOB_ACCEPT", onCheckMinijobAccept);
            EventController.addMenuEvent("ON_CHECK_MINIJOB_SEND_INFO", onCheckMinijobSendInfo);

            #region Creation

            addMinijobTaskCreator("Check-Minijob", startCheckMinijobCreation);

            EventController.addMenuEvent("ADMIN_CHECK_MINIJOBTASK_CREATION", onAdminCreateCheckMinijob);

            EventController.addMenuEvent("ADMIN_DELIVER_MINIJOB_CREATE_SPOT", onAdminCreateCheckSpot);
            EventController.addMenuEvent("ADMIN_DELIVER_MINIJOB_CREATE_INFO", onAdminCreateCheckInfo);

            EventController.addMenuEvent("ADMIN_DELIVER_MINIJOB_DELETE_SPOT", onAdminDeleteCheckOption);
            EventController.addMenuEvent("ADMIN_DELIVER_MINIJOB_DELETE_INFO", onAdminDeleteCheckInfo);

            #endregion
        }

        private bool onCheckMinijobAccept(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CheckMinijobTask)data["Task"];
            var instance = (MinijobInstance)data["Instance"];

            task.onFinish(instance);

            return true;
        }

        private bool onCheckMinijobSendInfo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CheckMinijobTask)data["Task"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var inputs = new List<string>();

            foreach(var element in evt.elements) {
                var inputEvt = element.FromJson<InputMenuItemEvent>();
                inputs.Add(inputEvt.input);
            }

            var solved = task.onSendInfo(inputs);
            if(solved == 0) {
                player.sendNotification(Constants.NotifactionTypes.Warning, $"Keine neue Information abegeben! Zu oft wiederholtes falsches eingeben (z.B. Raten) führt zu einem temporären Blocken der Eingabe!", $"{solved} abgegeben", Constants.NotifactionImages.MiniJob);
            } else if(solved == -1) {
                player.sendNotification(Constants.NotifactionTypes.Danger, $"Zu oft falsch eingegeben! Du bist für 90sek für neue Inputs geblockt!", $"Eingabe blockiert", Constants.NotifactionImages.MiniJob);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Info, $"{solved} neue Informationen abegeben!", $"{solved} abgegeben", Constants.NotifactionImages.MiniJob);
            }

            return true;
        }


        #region Creation

        protected static void startCheckMinijobCreation(IPlayer player, Action<string, Dictionary<string, string>> finishCallback) {
            var menu = new Menu("CheckMinijobTask", "Trage die Infos ein");
            menu.addMenuItem(new InputMenuItem("Name", "Trage den Namen des Treffpunktes ein (z.B. NPC Name)", "", ""));
            menu.addMenuItem(new InputMenuItem("Beschreibung", "Trage ein was an diesem Punkt als Beschreibung steht (Konversation vom NPC)", "", ""));
            menu.addMenuItem(new InputMenuItem("Ausg. Spot Anzahl", "Die Anzahl an Spots die zufällig aus den verfügbaren ausgewählt wird.", "", ""));
            menu.addMenuItem(new InputMenuItem("Ausg. Infos Anzahl", "Die Anzahl an Infos, die herausgefunden werden müssen, die zufällig aus den verfügbaren ausgewählt wird.", "", ""));
            menu.addMenuItem(new CheckBoxMenuItem("Nur ausg. Spots anzeigen", "Gibt an, ob alle erstellten Spots erstellt werden sollen, oder nur die, von denen die Info benötigt wird.", false, ""));

            menu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_CHECK_MINIJOBTASK_CREATION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Callback", finishCallback } }));

            player.showMenu(menu);
        }

        private bool onAdminCreateCheckMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var callback = (Action<string, Dictionary<string, string>>)data["Callback"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var amountEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var amount2Evt = evt.elements[3].FromJson<InputMenuItemEvent>();
            var onlySelectedCreated = evt.elements[4].FromJson<CheckBoxMenuItemEvent>();

            var settings = new Dictionary<string, string>();
            settings.Add("Name", nameEvt.input);
            settings.Add("Description", descriptionEvt.input);
            settings.Add("AmountSpotsSelected", int.Parse(amountEvt.input).ToString());
            settings.Add("AmountInfosSelected", int.Parse(amount2Evt.input).ToString());
            settings.Add("AllBlueprints", new List<CheckMinijobSpotBlueprint>().ToJson());
            settings.Add("OnlySelectedSpotsAreShown", onlySelectedCreated.check ? "TRUE" : "FALSE");

            callback.Invoke("CheckMinijobTask", settings);

            return true;
        }

        private bool onAdminCreateCheckSpot(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CheckMinijobTask)data["Task"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var amountEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var amount = int.Parse(amountEvt.input);

            player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun die Kollision des Spots", "");
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                var bluePrint = new CheckMinijobSpotBlueprint(CollisionShape.Create(p, w, h, r, true, false, true).toShortSave(), nameEvt.input, amount, new());

                task.AllBlueprints.Add(bluePrint);

                task.updateDbSettings();
            });

            return true;
        }

        private bool onAdminCreateCheckInfo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CheckMinijobTask)data["Task"];
            var spot = (CheckMinijobSpotBlueprint)data["Spot"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var listEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var list = new List<string>();

            foreach(var s in listEvt.input.Split(',').ToList()) {
                var info = s;
                if(s.StartsWith(' ')) {
                    info = s.Substring(1);
                }

                list.Add(info);
            }

            spot.AllInfos.Add(new CheckMinijobSpotInfoBlueprint(nameEvt.input, list));

            task.updateDbSettings();

            return true;
        }


        private bool onAdminDeleteCheckOption(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CheckMinijobTask)data["Task"];
            var bluePrint = (CheckMinijobSpotBlueprint)data["Blueprint"];

            task.AllBlueprints.Remove(bluePrint);

            task.updateDbSettings();

            return true;
        }

        private bool onAdminDeleteCheckInfo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CheckMinijobTask)data["Task"];
            var bluePrint = (CheckMinijobSpotBlueprint)data["Blueprint"];
            var info = (CheckMinijobSpotInfoBlueprint)data["Info"];

            var tB = task.AllBlueprints.FirstOrDefault(b => b == bluePrint);
            tB.AllInfos.Remove(info);

            task.updateDbSettings();

            return true;
        }


        #endregion
    }

    public record CheckMinijobSpotInfoBlueprint(string Description, List<string> Options);
    public record CheckMinijobSpotBlueprint(string ColShapeStr, string Name, int InfosSelectAmount, List<CheckMinijobSpotInfoBlueprint> AllInfos);

    public record CheckMinijobInfo(string MasterName, string Description, string Option);

    public class CheckMinijobTask : MinijobTask {
        public class CheckMinijobSpot : IDisposable {
            private CheckMinijobTask Master;
            private string Name;
            public CollisionShape Shape;
            public List<CheckMinijobInfo> SelectedOptions;

            private string ColString;

            public bool Activated { get; private set; }

            public CheckMinijobSpot(CheckMinijobTask master, string name, string colString, List<CheckMinijobInfo> selectedOptions) {
                Master = master;
                Name = name;
                ColString = colString;
                SelectedOptions = selectedOptions;
            }

            public void activate() {
                Shape = CollisionShape.Create(ColString);
                Shape.OnCollisionShapeInteraction += onSpotInteract;
                Activated = true;
            }

            private bool onSpotInteract(IPlayer player) {
                return Master.Master.ifInJobInteract(player, (instance) => {
                    var menu = new Menu(Name, "Siehe die Informationen unten");

                    foreach(var option in SelectedOptions) {
                        menu.addMenuItem(new StaticMenuItem(option.Description, $"Die Info {option.Description} hat den Wert: {option.Option}", option.Option));
                    }

                    player.showMenu(menu);
                });
            }

            public void Dispose() {
                if(Shape != null) {
                    Shape.Dispose();
                }
                Shape = null;
            }
        }

        //Config
        private string Name;
        private string Description;
        public List<CheckMinijobSpotBlueprint> AllBlueprints;
        private int AmountSpotsSelected;
        private int AmountInfosSelected;

        private bool OnlySelectedSpotsAreShown = false;

        //Changeable
        private List<CheckMinijobSpot> AllCreatedSpots;
        private List<CheckMinijobInfo> RequiredInfos;
        private List<CheckMinijobInfo> SolvedInfos;
        private DateTime BlockTimeAvailable;
        private int FalseInputCounter;

        public CheckMinijobTask(Minijob master, int id, int order, string spotStr, Dictionary<string, string> settings) : base(master, id, order, spotStr, settings) {
            Name = Settings["Name"];
            Description = Settings["Description"];
            AmountSpotsSelected = int.Parse(Settings["AmountSpotsSelected"]);
            AmountInfosSelected = int.Parse(Settings["AmountInfosSelected"]);
            AllBlueprints = Settings["AllBlueprints"].FromJson<List<CheckMinijobSpotBlueprint>>();

            if(Settings.ContainsKey("OnlySelectedSpotsAreShown")) {
                OnlySelectedSpotsAreShown = Settings["OnlySelectedSpotsAreShown"] == "TRUE";
            }

            AllCreatedSpots = new();
            RequiredInfos = new();
            SolvedInfos = new();
            FalseInputCounter = 0;
            BlockTimeAvailable = DateTime.MinValue;
        }

        public override void onInteractionStep(IPlayer player, MinijobInstance instance) {
            var menu = new Menu(Name, "Was möchstest du tun?");
            menu.addMenuItem(new StaticMenuItem("Information", Description, ""));

            menu.addMenuItem(new StaticMenuItem("Task-Bearbeitung", $"Die Aufgabe dieses Tasks ist es die richtigen Informationen zu beschaffen. Im Untermenü \"Benötigte Daten\" sind alle Informationen aufgelistet die gesucht sind. Auf deiner Karte sind alle möglichen Orte mit Informationen eingezeichnet. Merke dir, welche Informationen benötigt werden, gehe zu den verschiedenen Orten und lies sie ab. Es kann sein, dass überflüssige Informationen oder Orte existieren. Trage sie dann unter \"Benötigte Daten\" ein und drücke Abgeben. Du musst nicht alle Informationen gleichzeitig abgeben. Achte darauf, dass du die Information genau so einträgst wie sie am Ort steht. Das enthält Groß/Kleinschreibung, Rechtschreibung und z.B. Einheiten (wie Ghz oder Meter)", "Siehe Beschreibung"));

            if(BlockTimeAvailable == DateTime.MinValue || BlockTimeAvailable < DateTime.Now) {
                var infoMenu = new Menu("Benötigte Daten", "Gib die richtigen genauen (!) Daten ein.");
                foreach(var requiredInfo in RequiredInfos) {
                    var solved = SolvedInfos.FirstOrDefault(s => s.Description == requiredInfo.Description && s.MasterName == requiredInfo.MasterName);

                    if(solved != null) {
                        infoMenu.addMenuItem(new InputMenuItem($"{requiredInfo.MasterName}: {requiredInfo.Description}", $"Die korrekte Information: {solved.Option} wurde bereits gegeben", solved.Option, "", MenuItemStyle.green));
                    } else {
                        infoMenu.addMenuItem(new InputMenuItem($"{requiredInfo.MasterName}: {requiredInfo.Description}", $"Finde die korrekte Information: {requiredInfo.Description} von/vom {requiredInfo.MasterName}", "", ""));
                    }
                }
                infoMenu.addMenuItem(new MenuStatsMenuItem("Abgeben", $"Gib die eingetragenen Daten ein. Es müssen nicht alle gleichzeitig ausgefüllt werden!", "ON_CHECK_MINIJOB_SEND_INFO", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Instance", instance } }));
                menu.addMenuItem(new MenuMenuItem(infoMenu.Name, infoMenu));
            } else {
                menu.addMenuItem(new StaticMenuItem("Eingabe blockiert!", "Du kannst aktuell keine Eingabe tätigen, da zu viel falsche Informationen abgegeben wurden! Die Eingabe ist nach der oben gezeigten Uhrzeit wieder möglich!", $"möglich ab: {BlockTimeAvailable.TimeOfDay}", MenuItemStyle.red));
            }

            if(RequiredInfos.Count == SolvedInfos.Count) {
                menu.addMenuItem(new ClickMenuItem("Fortfahren", "Fahre fort", "", "ON_CHECK_MINIJOB_ACCEPT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Instance", instance } }));
            }


            player.showMenu(menu);
        }

        protected override void onShowInfoStep(MinijobInstance instance, IPlayer player) {
            if(instance != null) {
                foreach(var spot in AllCreatedSpots) {
                    if(spot.Activated) {
                        BlipController.createPointBlip(player, "Minijob-Ort", spot.Shape.Position, Order % 85, 682, 255, $"MINIJOB_CHECK_BLIP_{spot.Shape.Id}");
                    }
                }
            }
        }

        public override void start(MinijobInstance instance) { }

        public override void onFinish(MinijobInstance instance) {
            base.onFinish(instance);
        }

        protected override void prepareStep() {
            var rand = new Random();

            foreach(var select in AllBlueprints) {
                var selectInfos = select.AllInfos.GetRandomElements(select.InfosSelectAmount);
                var list = new List<CheckMinijobInfo>();
                foreach(var selectInfo in selectInfos) {
                    list.Add(new CheckMinijobInfo(select.Name, selectInfo.Description, selectInfo.Options[rand.Next(selectInfo.Options.Count)]));
                }

                var spot = new CheckMinijobSpot(this, select.Name, select.ColShapeStr, list);
                AllCreatedSpots.Add(spot);

                if(!OnlySelectedSpotsAreShown) {
                    spot.activate();
                }
            }

            RequiredInfos = AllCreatedSpots.SelectMany(c => c.SelectedOptions).GetRandomElements(AmountInfosSelected).OrderBy(x => x.MasterName.ExtractNumber()).ThenBy(x => x.Description).ToList();

            if(OnlySelectedSpotsAreShown) {
                foreach(var spot in AllCreatedSpots) {
                    if(RequiredInfos.Any(i => spot.SelectedOptions.Contains(i))) {
                        spot.activate();
                    }
                }
            }
        }

        protected override void resetStep() {
            foreach(var spot in AllCreatedSpots) {
                spot.Dispose();
            }

            AllCreatedSpots = new();
            RequiredInfos = new();
            SolvedInfos = new();
        }

        public int onSendInfo(List<string> inputs) {
            var pref = SolvedInfos.Count;

            for(var i = 0; i < inputs.Count - 1; i++) {
                var input = inputs[i];
                var option = RequiredInfos[i];

                if(input != null && input != "") {
                    var solved = SolvedInfos.FirstOrDefault(s => s.Description.ToLower().Equals(option.Description.ToLower()) && s.MasterName == option.MasterName);

                    if(solved == null) {
                        if(input == option.Option) {
                            SolvedInfos.Add(option);
                        }
                    }
                }
            }

            if(SolvedInfos.Count - pref == 0) {
                FalseInputCounter++;

                if(FalseInputCounter >= 3) {
                    BlockTimeAvailable = DateTime.Now + TimeSpan.FromSeconds(90);
                    return -1;
                } else {
                    return 0;
                }
            } else {
                BlockTimeAvailable = DateTime.MinValue;
                return SolvedInfos.Count - pref;
            }
        }

        public override void preprepare() { }

        protected override void finishStep(MinijobInstance instance) {
            foreach(var spot in AllCreatedSpots) {
                foreach(var pl in instance.ConnectedPlayers) {
                    if(spot.Activated) {
                        BlipController.destroyBlipByName(pl, $"MINIJOB_CHECK_BLIP_{spot.Shape.Id}");
                    }
                }
            }
        }

        protected override void finishForPlayerStep(MinijobInstance instance, IPlayer player) {
            foreach(var spot in AllCreatedSpots) {
                if(spot.Activated) {
                    BlipController.destroyBlipByName(player, $"MINIJOB_CHECK_BLIP_{spot.Shape.Id}");
                }
            }
        }

        public override bool isMultiViable() {
            return false;
        }


        #region Admin Stuff

        public override List<MenuItem> getAdminMenuElements() {
            var list = new List<MenuItem>();

            list.Add(new StaticMenuItem("Ausg. Spot Anzahl", "Die Anzahl an Spots die zufällig aus den verfügbaren ausgewählt wird.", $"{AmountSpotsSelected}"));
            list.Add(new StaticMenuItem("Ausg. Infos Anzahl", "Die Anzahl an Infos, die herausgefunden werden müssen, die zufällig aus den verfügbaren ausgewählt wird.", $"{AmountInfosSelected}"));
            list.Add(new StaticMenuItem("Nur ausg. Spots anzeigen", "Gibt an, ob alle erstellten Spots erstellt werden sollen, oder nur die, von denen die Info benötigt wird.", $"{OnlySelectedSpotsAreShown}"));

            //Delete
            var alreadyMenu = new Menu("Spots", "Was möchtest du tun?");
            foreach(var spot in AllBlueprints) {
                var optionMenu = new Menu(spot.Name, "Was möchtest du tun?");

                optionMenu.addMenuItem(new StaticMenuItem("Name", $"Der Name des Spots ist: {spot.Name}", spot.Name));
                var amount = spot.AllInfos.Count;
                optionMenu.addMenuItem(new StaticMenuItem("Anzahl. Optionen", $"Es werden stets {spot.InfosSelectAmount} von möglichen {amount} ausgewählt", $"{spot.InfosSelectAmount}/{amount}"));
                optionMenu.addMenuItem(new StaticMenuItem("Collisionshape", $"Ist definiert durch den String: {spot.ColShapeStr}", "Siehe Beschreibung"));

                var infosMenu = new Menu("Informationen", "Was möchtest du tun?");
                foreach(var info in spot.AllInfos) {
                    var infoMenu = new Menu(info.Description, "Was möchtest du tun?");

                    infoMenu.addMenuItem(new StaticMenuItem("Beschreibung", $"Die Beschreibung der Info ist: {info.Description}", info.Description));
                    infoMenu.addMenuItem(new ListMenuItem("Mögliche Infos", $"Die Liste aller Optionen enthält: {info.Options.ToJson()}", info.Options.ToArray(), ""));

                    infoMenu.addMenuItem(new ClickMenuItem("Info löschen", "Lösche diese Info aus dem Spot", "", "ADMIN_DELIVER_MINIJOB_DELETE_INFO", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Blueprint", spot }, { "Info", info } }).needsConfirmation($"{spot.Name} löschen?", "Spot wirklich löschen?"));

                    infosMenu.addMenuItem(new MenuMenuItem(infoMenu.Name, infoMenu));
                }

                optionMenu.addMenuItem(new MenuMenuItem(infosMenu.Name, infosMenu));

                var infoCreateMenu = new Menu("Info erstellen", "Gib die Daten ein");
                infoCreateMenu.addMenuItem(new InputMenuItem("Beschreibung", "Die Beschreibung des Datums", "", ""));
                infoCreateMenu.addMenuItem(new InputMenuItem("Optionenliste", "Die Liste aller möglichen Optionen. Getrennt durch Kommata ',', z.B. [5Ghz, 5.5Ghz, 6Ghz] (ohne [])", "", ""));
                infoCreateMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das Info", "ADMIN_DELIVER_MINIJOB_CREATE_INFO", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Spot", spot } }).needsConfirmation("Information erstellen?", "Info wirklich erstellen?"));

                optionMenu.addMenuItem(new MenuMenuItem(infoCreateMenu.Name, infoCreateMenu, MenuItemStyle.green));

                optionMenu.addMenuItem(new ClickMenuItem("Spot löschen", "Lösche diesen Spot und alle seine Informationen", "", "ADMIN_DELIVER_MINIJOB_DELETE_SPOT", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Blueprint", spot } }).needsConfirmation($"{spot.Name} löschen?", "Spot wirklich löschen?"));
                alreadyMenu.addMenuItem(new MenuMenuItem(optionMenu.Name, optionMenu));
            }
            list.Add(new MenuMenuItem(alreadyMenu.Name, alreadyMenu));


            //Create
            var spotCreateMenu = new Menu("Spot erstellen", "Gib die Infos ein");
            spotCreateMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Spots. z.B. Ventil 3", "", ""));
            spotCreateMenu.addMenuItem(new InputMenuItem("Anzahl Infos", "Wieviele der später eingestellten Infos ausgewählt werden", "", ""));
            spotCreateMenu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_DELIVER_MINIJOB_CREATE_SPOT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this } }));

            list.Add(new MenuMenuItem(spotCreateMenu.Name, spotCreateMenu, MenuItemStyle.green));

            return list;
        }

        public void updateDbSettings() {
            using(var db = new ChoiceVDb()) {
                var already = db.configminijobstaskssettings.FirstOrDefault(s => s.taskId == Id && s.name == "AllBlueprints");
                already.value = AllBlueprints.ToJson();

                db.SaveChanges();
            }
        }

        #endregion
    }
}
