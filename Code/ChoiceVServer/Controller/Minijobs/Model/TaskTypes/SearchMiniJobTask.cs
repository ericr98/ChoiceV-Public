using AltV.Net.Data;
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
using System.Numerics;
using static ChoiceVServer.Controller.MiniJobController;
using static ChoiceVServer.Controller.Minijobs.Model.Minijob;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Minijobs.Model {
    public class SearchMiniJobTaskController : ChoiceVScript {
        public SearchMiniJobTaskController() {
            EventController.addMenuEvent("ON_SEARCH_MINIJOB_ACCEPT", onSearchMinijobAccept);
            EventController.addMenuEvent("ON_SEARCH_MINIJOB_SHOW_LOCATION", onSearchMinijobShowLocation);

            #region Creation

            addMinijobTaskCreator("Suche-Minijob", startSearchMinijobCreation);

            EventController.addMenuEvent("ADMIN_SEARCH_MINIJOBTASK_CREATION", onAdminCreateSearchMinijob);

            EventController.addMenuEvent("ADMIN_SEARCH_MINIJOB_CREATE_OPTION", onAdminSearchDeliverOptionCreate);
            EventController.addMenuEvent("ADMIN_SEARCH_MINIJOB_DELETE_OPTION", onAdminSearchDeliverOptionDelete);

            #endregion
        }

        private bool onSearchMinijobAccept(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (SearchMinijobTask)data["Task"];
            var instance = (MinijobInstance)data["Instance"];

            task.onFinish(instance);

            return true;
        }

        private bool onSearchMinijobShowLocation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (SearchMinijobTask)data["Task"];

            task.createWayPointForPlayer(player);
            return true;
        }

        #region Creation

        protected static void startSearchMinijobCreation(IPlayer player, Action<string, Dictionary<string, string>> finishCallback) {
            var menu = new Menu("SearchMinijobTask", "Trage die Infos ein");
            menu.addMenuItem(new InputMenuItem("Name", "Trage den Namen des Treffpunktes ein (z.B. NPC Name)", "", ""));
            menu.addMenuItem(new InputMenuItem("Beschreibung", "Trage ein was an diesem Punkt als Beschreibung steht (Konversation vom NPC)", "", ""));
            menu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_SEARCH_MINIJOBTASK_CREATION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Callback", finishCallback } }));

            player.showMenu(menu);
        }

        private bool onAdminCreateSearchMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var callback = (Action<string, Dictionary<string, string>>)data["Callback"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var settings = new Dictionary<string, string>();
            settings.Add("Name", nameEvt.input);
            settings.Add("Description", descriptionEvt.input);
            settings.Add("AllOptions", new List<SearchMinijobTask>().ToJson());

            callback.Invoke("SearchMinijobTask", settings);

            return true;
        }

        private bool onAdminSearchDeliverOptionCreate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (SearchMinijobTask)data["Task"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var clueEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var findEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var propNameEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var propPositionEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
            var propQuaternioneEvt = evt.elements[4].FromJson<InputMenuItemEvent>();

            Object obj = null;
            var propName = "";
            Position pos = Position.Zero;
            Rotation rot = Rotation.Zero;

            if(propNameEvt.input != null && propNameEvt.input != "") {
                propName = propNameEvt.input;
                var posSplit = propPositionEvt.input.Split(',');
                pos = new Position(float.Parse(posSplit[0]), float.Parse(posSplit[1]), float.Parse(posSplit[2]));

                var quatSplit = propQuaternioneEvt.input.Split(',');
                rot = new Quaternion(float.Parse(quatSplit[0]), float.Parse(quatSplit[1]), float.Parse(quatSplit[2]), float.Parse(quatSplit[3])).ToEulerAngles();

                obj = ObjectController.createObject(propNameEvt.input, pos, ((DegreeRotation)rot));

                InvokeController.AddTimedInvoke("Search-Task-Creator-Object-Remover", (i) => {
                    if(obj != null) {
                        ObjectController.deleteObject(obj);
                        obj = null;
                    }
                }, TimeSpan.FromSeconds(30), false);
            }

            player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun die Kollision des Search", "");
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                var colShape = CollisionShape.Create(p, w, h, r, true, false, true);

                var option = new SearchMinijobTaskOption(colShape.toShortSave(), propName, pos, rot, clueEvt.input, findEvt.input);

                task.AllOptions.Add(option);

                using(var db = new ChoiceVDb()) {
                    var already = db.configminijobstaskssettings.FirstOrDefault(s => s.taskId == task.Id && s.name == "AllOptions");
                    already.value = task.AllOptions.ToJson();

                    db.SaveChanges();
                }

                player.sendNotification(Constants.NotifactionTypes.Success, "Task erfolgreich erstellt", "");

                if(obj != null) {
                    ObjectController.deleteObject(obj);
                }
                colShape.Dispose();
            }, 3, 3, pos);

            return true;
        }

        private bool onAdminSearchDeliverOptionDelete(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (SearchMinijobTask)data["Task"];
            var option = (SearchMinijobTaskOption)data["Option"];

            task.AllOptions.Remove(option);


            using(var db = new ChoiceVDb()) {
                var already = db.configminijobstaskssettings.FirstOrDefault(s => s.taskId == task.Id && s.name == "AllOptions");
                already.value = task.AllOptions.ToJson();

                db.SaveChanges();
            }

            player.sendNotification(Constants.NotifactionTypes.Warning, "Task erfolgreich gelöscht", "");

            return true;
        }

        #endregion
    }

    public record SearchMinijobTaskOption(string CollStr, string PropName, Position PropPosition, Rotation PropRotation, string Clue, string FindInfo);

    public class SearchMinijobTask : MinijobTask {
        private class InstanceInfo {
            public MinijobInstance Instance;
            public DateTime StartTime;
            public CollisionShape ClueCol;
            public Object Object;
            public SearchMinijobTaskOption Selected;
            public bool Found = false;
            public IInvoke HintInvoke = null;

            public InstanceInfo(MinijobInstance instance, DateTime startTime, CollisionShape clueCol, Object obj, SearchMinijobTaskOption selected, bool found, IInvoke hintInvoke) {
                Instance = instance;
                StartTime = startTime;
                ClueCol = clueCol;
                Object = obj;
                Selected = selected;
                Found = found;
                HintInvoke = hintInvoke;
            }
        }

        private string Name;
        private string Description;

        public List<SearchMinijobTaskOption> AllOptions;

        private List<InstanceInfo> AllInstanceInfos;

        public SearchMinijobTask(Minijob master, int id, int order, string spotStr, Dictionary<string, string> settings) : base(master, id, order, spotStr, settings) {
            Name = Settings["Name"];
            Description = Settings["Description"];

            AllOptions = Settings["AllOptions"].FromJson<List<SearchMinijobTaskOption>>();

            AllOptions = new();
            //Rotation yaws need to be signed (* (-1)), because of GTA, idk..
            foreach(var option in Settings["AllOptions"].FromJson<List<SearchMinijobTaskOption>>()) {
                var rotation = option.PropRotation;
                rotation.Yaw = rotation.Yaw * (-1);

                AllOptions.Add(new SearchMinijobTaskOption(option.CollStr, option.PropName, option.PropPosition, rotation, option.Clue, option.FindInfo));
            }


            AllInstanceInfos = new();
        }

        public override void onInteractionStep(IPlayer player, MinijobInstance instance) {
            var info = AllInstanceInfos.FirstOrDefault(i => i.Instance == instance);
            if(info != null) {
                var menu = new Menu(Name, "Was möchstest du tun?");
                menu.addMenuItem(new StaticMenuItem("Information", Description, ""));
                menu.addMenuItem(new StaticMenuItem("Hinweis", info.Selected.Clue, ""));
                menu.addMenuItem(new StaticMenuItem("Task-Bearbeitung", $"Das Ziel ist einen bestimmten Ort auf der GTA Karte zu finden und mit diesem zu interagieren. Der Ort ist oftmals etwas versteckt. Benutze die Information und den Hinweis um logisch auf die Position zu schließen. Hat z.B. jemand etwas verloren, versuche, falls möglich, seine Schritte zurückzuverfolgen. Falls du nach einer gewissen Zeit den Ort nicht findest gibt es die Möglichkeit ihn sich hier anzeigen zu lassen.", "Siehe Beschreibung"));

                if(DateTime.Now - info.StartTime > TimeSpan.FromMinutes(7)) {
                    menu.addMenuItem(new ClickMenuItem("Ort als Wegpunkt setzen", $"Lasse dir den Ort auf der Karte als Wegpunkt anzeigen", "", "ON_SEARCH_MINIJOB_SHOW_LOCATION", MenuItemStyle.yellow).withData(new Dictionary<string, dynamic> { { "Task", this } }));
                }

                if(info.Found) {
                    menu.addMenuItem(new ClickMenuItem("Fortfahren", "Fahre fort", "", "ON_SEARCH_MINIJOB_ACCEPT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Instance", instance } }));
                }

                player.showMenu(menu);
            }
        }

        protected override void onShowInfoStep(MinijobInstance instance, IPlayer player) { }


        protected override void prepareStep() { }

        protected override void resetStep() { }

        public void createWayPointForPlayer(IPlayer player) {
            var info = AllInstanceInfos.FirstOrDefault(i => i.Instance.ConnectedPlayersIds.Contains(player.getCharacterId()));
            if(info != null) {
                BlipController.createWaypointBlip(player, info.ClueCol.Position);
                player.sendNotification(Constants.NotifactionTypes.Info, $"Der Ort ist nun auf der Karte markiert", "Ort markiert", Constants.NotifactionImages.MiniJob);
            }
        }

        public override void start(MinijobInstance instance) {
            var rand = new Random();

            var selected = AllOptions[rand.Next(0, AllOptions.Count)];

            var clueCol = CollisionShape.Create(selected.CollStr);
            clueCol.OnCollisionShapeInteraction += (player) => onClueInteraction(player, clueCol);

            Object obj = null;
            if(selected.PropName != "") {
                obj = ObjectController.createObject(selected.PropName, selected.PropPosition, selected.PropRotation);
            }

            var hintInvoke = InvokeController.AddTimedInvoke("Hint_Activater", (i) => {
                instance.ConnectedPlayers.ForEach(p => p.sendNotification(Constants.NotifactionTypes.Info, "Es ist nun möglich sich die Position des Ortes anzeigen zu lassen. Kehre dazu zurück!", "Hinweis möglich!", Constants.NotifactionImages.MiniJob));
            }, TimeSpan.FromMinutes(7), false);

            AllInstanceInfos.Add(new InstanceInfo(instance, DateTime.Now, clueCol, obj, selected, false, hintInvoke));
        }

        protected override void finishStep(MinijobInstance instance) {
            var info = AllInstanceInfos.Find(i => i.Instance == instance);

            if(info != null) {
                if(info.HintInvoke != null) {
                    info.HintInvoke.EndSchedule();
                }

                if(info.ClueCol != null) {
                    info.ClueCol.Dispose();
                    info.ClueCol = null;
                }

                if(info.Object != null) {
                    ObjectController.deleteObject(info.Object);
                }

                AllInstanceInfos.Remove(info);
            }
        }

        protected override void finishForPlayerStep(MinijobInstance instance, IPlayer player) { }

        public override bool isMultiViable() {
            return true;
        }

        public override void preprepare() { }


        private bool onClueInteraction(IPlayer player, CollisionShape shape) {
            Master.ifInJobInteract(player, (instance) => {
                var info = AllInstanceInfos.Find(i => i.ClueCol == shape && i.Instance.ConnectedPlayersIds.Contains(player.getCharacterId()));
                if(info != null) {
                    var anim = AnimationController.getAnimationByName("KNEEL_DOWN");

                    AnimationController.animationTask(player, anim, () => {
                        if(!info.Found) {
                            instance.ConnectedPlayers.ForEach(p => p.sendNotification(Constants.NotifactionTypes.Success, info.Selected.FindInfo, $"Objekt gefunden", Constants.NotifactionImages.MiniJob));

                            info.Found = true;
                            if(info.Object != null) {
                                ObjectController.deleteObject(info.Object);
                            }

                            info.ClueCol.Dispose();
                            info.ClueCol = null;
                        }
                    });
                } else {
                    player.sendBlockNotification("Dieser Ort ist für einen anderen Minijob gedacht. Dein gesuchter Ort befindet sich woanders!", "Gesuchter Ort woanders", Constants.NotifactionImages.MiniJob);
                }
            });

            return true;
        }

        #region Admin Stuff

        public override List<MenuItem> getAdminMenuElements() {
            var list = new List<MenuItem>();

            var alreadyMenu = new Menu("Liste", "Was möchtest du tun?");
            for(var i = 0; i < AllOptions.Count; i++) {
                var op = AllOptions[i];
                var optionMenu = new Menu($"Option: {i}", "Was möchtest du tun?");
                optionMenu.addMenuItem(new StaticMenuItem("Hinweis", op.Clue, ""));
                optionMenu.addMenuItem(new StaticMenuItem("Fundinfo", op.FindInfo, ""));
                optionMenu.addMenuItem(new StaticMenuItem("PropName", op.PropName, ""));
                optionMenu.addMenuItem(new StaticMenuItem("PropRotation", op.PropPosition.ToJson(), ""));
                optionMenu.addMenuItem(new StaticMenuItem("PropRotation", op.PropRotation.ToJson(), ""));

                optionMenu.addMenuItem(new ClickMenuItem("Löschen", $"Lösche die Option", "", "ADMIN_SEARCH_MINIJOB_DELETE_OPTION", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Option", op }, { "Task", this } }).needsConfirmation("Option löschen?", "Option wirklich löschen?"));
                alreadyMenu.addMenuItem(new MenuMenuItem(optionMenu.Name, optionMenu));
            }

            list.Add(new MenuMenuItem(alreadyMenu.Name, alreadyMenu));

            var createMenu = new Menu("Option erstellen", "Erstelle eine neue Suche");
            createMenu.addMenuItem(new InputMenuItem("Hinweis", "Der Hinweis der für diese Option gegeben wird", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Find-Information", "Die Information die kommt, wenn der Ort gefunden ist", "", ""));

            createMenu.addMenuItem(new InputMenuItem("Propname", "Der GTA Name des Props. Vorher mit /createObject [name] 0 0 0 testen ob das Props funktioniert", "prop_fire_hydrant_2", ""));
            createMenu.addMenuItem(new InputMenuItem("Prop Position", "Die Gta Position des Props. Das Format ist dasselbe wie in Codewalker, [x, y, z] (ohne [])", "-412.45, -15.05, 45.78", ""));
            createMenu.addMenuItem(new InputMenuItem("Prop Rotation", "Die Rotation des Props. Im Codewalker 'Quaternion' System. [x, y, z, w], (ohne [])", "0, 0, 0.0876, 0.9961", ""));

            createMenu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_SEARCH_MINIJOB_CREATE_OPTION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this } }).needsConfirmation("Erstellung fortfahren?", "Erstellung wirklich fortfahren?"));

            list.Add(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            return list;
        }


        #endregion
    }
}
