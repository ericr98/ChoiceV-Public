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
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Minijobs.Model {
    public class CollectMiniJobTaskController : ChoiceVScript {
        public CollectMiniJobTaskController() {
            EventController.addMenuEvent("ON_COLLECT_MINIJOB_ACCEPT", onCollectMinijobAccept);
            EventController.addMenuEvent("ON_COLLECT_MINIJOB_SHOW_LOCATION", onCollectMinijobShowLocation);

            #region Creation

            addMinijobTaskCreator("Sammel-Minijob", startCollectMinijobCreation);

            EventController.addMenuEvent("ADMIN_COLLECT_MINIJOBTASK_CREATION", onAdminCreateCollectMinijob);
            EventController.addMenuEvent("ADMIN_COLLECT_MINIJOB_CREATE_COLLECTABLE", onAdminCollectMinijobCreateCollectable);

            EventController.addMenuEvent("ADMIN_COLLECT_MINIJOB_DELETE_COLLECTABLE", onAdminCollectMinijobDeleteCollectable);

            #endregion
        }

        private bool onCollectMinijobAccept(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CollectMinijobTask)data["Task"];
            var instance = (MinijobInstance)data["Instance"];

            task.onFinish(instance);

            return true;
        }

        private bool onCollectMinijobShowLocation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CollectMinijobTask)data["Task"];
            var collectables = task.CurrentCollectables.Where(c => !c.Disposed && !c.IsShownOnMap).ToList();
            if(collectables.Count != 0) {
                foreach(var collectable in collectables) {
                    collectable.IsShownOnMap = true;
                    BlipController.createPointBlip(player, collectable.Name, collectable.Shape.Position, task.Order % 85, 682, 255, $"COLLECT_MINIJOB_TASK_{collectable.Id}");
                }
                player.sendNotification(Constants.NotifactionTypes.Info, $"Die übrigen Orte wurden auf der Karte markiert", "Ort markiert", Constants.NotifactionImages.MiniJob);
            }

            return true;
        }

        #region Creation

        protected static void startCollectMinijobCreation(IPlayer player, Action<string, Dictionary<string, string>> finishCallback) {
            var menu = new Menu("CollectMinijobTask", "Trage die Infos ein");
            menu.addMenuItem(new InputMenuItem("Name", "Trage den Namen des Treffpunktes ein (z.B. NPC Name)", "", ""));
            menu.addMenuItem(new InputMenuItem("Beschreibung", "Trage ein was an diesem Punkt als Beschreibung steht (Konversation vom NPC)", "", ""));
            menu.addMenuItem(new InputMenuItem("Ausgewählte Collectables", "Wieviele Collectables von den vorhandenen ausgewählt werden", "", ""));
            menu.addMenuItem(new InputMenuItem("Benötigte Collectables", "Wieviele Collectables von den ausgewählten eingesammelt werden müssen. Muss kleiner/gleich wie \"Ausgewählte Collectables\" sein", "", ""));

            menu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_COLLECT_MINIJOBTASK_CREATION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Callback", finishCallback } }));

            player.showMenu(menu);
        }

        private bool onAdminCreateCollectMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var callback = (Action<string, Dictionary<string, string>>)data["Callback"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var selectedEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var requiredEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            var settings = new Dictionary<string, string>();
            settings.Add("Name", nameEvt.input);
            settings.Add("Description", descriptionEvt.input);
            settings.Add("SelectedCollects", int.Parse(selectedEvt.input).ToString());
            settings.Add("RequiredCollects", int.Parse(requiredEvt.input).ToString());
            settings.Add("AllCollectables", new List<CollectMinijobCollectableBlueprint>().ToJson());

            callback.Invoke("CollectMinijobTask", settings);

            return true;
        }

        private bool onAdminCollectMinijobCreateCollectable(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CollectMinijobTask)data["Task"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var propNameEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var propPositionEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var propRotationEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
            var showOnMapEvt = evt.elements[4].FromJson<CheckBoxMenuItemEvent>();
            var destroyedEvt = evt.elements[5].FromJson<CheckBoxMenuItemEvent>();
            var animEvt = evt.elements[6].FromJson<InputMenuItemEvent>();
            var spawnLastEvt = evt.elements[7].FromJson<CheckBoxMenuItemEvent>();

            Object obj = null;
            var propName = "";
            Position pos = Position.Zero;
            Rotation rot = Rotation.Zero;

            if(propNameEvt.input != null && propNameEvt.input != "") {
                propName = propNameEvt.input;
                var posSplit = propPositionEvt.input.Split(',');
                pos = new Position(float.Parse(posSplit[0]), float.Parse(posSplit[1]), float.Parse(posSplit[2]));

                var quatSplit = propRotationEvt.input.Split(',');
                rot = new Quaternion(float.Parse(quatSplit[0]), float.Parse(quatSplit[1]), float.Parse(quatSplit[2]), float.Parse(quatSplit[3])).ToEulerAngles();

                obj = ObjectController.createObject(propNameEvt.input, pos, ((DegreeRotation)rot));

                InvokeController.AddTimedInvoke("Collect-Task-Creator-Object-Remover", (i) => {
                    if(obj != null) {
                        ObjectController.deleteObject(obj);
                        obj = null;
                    }
                }, TimeSpan.FromSeconds(30), false);
            }

            player.sendNotification(Constants.NotifactionTypes.Info, "Setze nun die Kollision des Collectables", "");
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                var colShape = CollisionShape.Create(p, w, h, r, true, false, true);

                var option = new CollectMinijobCollectableBlueprint(nameEvt.input, colShape.toShortSave(), propName, pos, rot, animEvt.input, showOnMapEvt.check, destroyedEvt.check, spawnLastEvt.check);

                task.AllCollectables.Add(option);

                using(var db = new ChoiceVDb()) {
                    var already = db.configminijobstaskssettings.FirstOrDefault(s => s.taskId == task.Id && s.name == "AllCollectables");
                    already.value = task.AllCollectables.ToJson();

                    db.SaveChanges();
                }

                player.sendNotification(Constants.NotifactionTypes.Success, "Collectable erfolgreich erstellt", "");

                if(obj != null) {
                    ObjectController.deleteObject(obj);
                    obj = null;
                }

                colShape.Dispose();
            }, 3, 3, pos);

            return true;
        }

        private bool onAdminCollectMinijobDeleteCollectable(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (CollectMinijobTask)data["Task"];
            var collectable = (CollectMinijobCollectableBlueprint)data["Collectable"];

            task.AllCollectables.Remove(collectable);

            using(var db = new ChoiceVDb()) {
                var already = db.configminijobstaskssettings.FirstOrDefault(s => s.taskId == task.Id && s.name == "AllCollectables");
                already.value = task.AllCollectables.ToJson();

                db.SaveChanges();
            }

            player.sendNotification(Constants.NotifactionTypes.Warning, "Collectable erfolgreich gelöscht!", "");

            return true;
        }


        #endregion
    }

    public record CollectMinijobCollectableBlueprint(string Name, string ColShapeStr, string PropName, Position PropPosition, Rotation PropRotation, string AnimIdentifier, bool IsShownOnMap, bool DestroyedWhenUsed, bool SpawnObjectOnInteract);
    //Option einbauen, umkehrt zu handeln. Nicht Item spawnt und man muss es wegmachen, sondern Ort ist auf jeden Fall markiert und man muss Item aufstellen
    //Generell einbauen, dass wenn man fertig ist, eine custom Notizifaction kommt
    public class CollectMinijobTask : MinijobTask {
        public class CollectMinijobCollectable : IDisposable {
            public int Id;
            public string Name;
            private CollectMinijobTask Master;
            private string ColShapeStr;
            public CollisionShape Shape;
            public Object Object;
            private string AnimIdentifier;
            public bool IsShownOnMap;
            private bool DestroyedWhenUsed;
            public bool SpawnObjectOnInteract;

            private CollectMinijobCollectableBlueprint Blueprint;

            public bool Disposed;

            public bool Activated;

            public CollectMinijobCollectable(CollectMinijobCollectableBlueprint blueprint, int id, string name, CollectMinijobTask master, Object obj, string colShapeStr, string animIdentifier, bool isShownOnMap, bool destroyedWhenUsed, bool spawnObjectOnInteract) {
                Blueprint = blueprint;
                Id = id;
                Name = name;
                Master = master;
                ColShapeStr = colShapeStr;
                Object = obj;
                AnimIdentifier = animIdentifier;
                IsShownOnMap = isShownOnMap;

                DestroyedWhenUsed = destroyedWhenUsed;
                SpawnObjectOnInteract = spawnObjectOnInteract;
            }

            public void activate() {
                if(Shape == null) {
                    Shape = CollisionShape.Create(ColShapeStr);
                    Shape.OnCollisionShapeInteraction += onInteract;
                }
            }

            private bool onInteract(IPlayer player) {
                return Master.Master.ifInJobInteract(player, (instance) => {
                    if(AnimIdentifier != null && AnimIdentifier != "") {
                        var anim = AnimationController.getAnimationByName(AnimIdentifier);
                        AnimationController.animationTask(player, anim, () => {
                            if(!Disposed) {
                                Master.onCollect(player, this);
                                if(!SpawnObjectOnInteract) {
                                    if(DestroyedWhenUsed && Object != null) {
                                        ObjectController.deleteObject(Object);
                                    }
                                } else {
                                    Object = ObjectController.createObject(Blueprint.PropName, Blueprint.PropPosition, Blueprint.PropRotation);
                                }
                            }
                        });
                    } else {
                        Master.onCollect(player, this);
                        if(!Disposed) {
                            if(!SpawnObjectOnInteract) {
                                if(DestroyedWhenUsed && Object != null) {
                                    ObjectController.deleteObject(Object);
                                }
                            } else {
                                Object = ObjectController.createObject(Blueprint.PropName, Blueprint.PropPosition, Blueprint.PropRotation);
                            }
                        }
                    }
                });
            }

            public void Dispose() {
                if(Shape != null) {
                    Shape.Dispose();
                    Shape = null;
                }

                Master = null;

                Disposed = true;
            }
        }

        //Config
        private string Name;
        private string Description;
        private int RequiredCollects;
        private int SelectedCollects;

        public List<CollectMinijobCollectableBlueprint> AllCollectables;

        //Changeable
        public List<CollectMinijobCollectable> CurrentCollectables;
        private int CurrentCount;

        private IInvoke HintInvoke;
        private DateTime StartTime;

        public CollectMinijobTask(Minijob master, int id, int order, string spotStr, Dictionary<string, string> settings) : base(master, id, order, spotStr, settings) {
            Name = Settings["Name"];
            Description = Settings["Description"];
            RequiredCollects = int.Parse(Settings["RequiredCollects"]);
            SelectedCollects = int.Parse(Settings["SelectedCollects"]);

            AllCollectables = new();
            //Rotation yaws need to be signed (* (-1)), because of GTA, idk..
            foreach(var collectable in Settings["AllCollectables"].FromJson<List<CollectMinijobCollectableBlueprint>>()) {
                var rotation = collectable.PropRotation;
                rotation.Yaw = rotation.Yaw * (-1);

                AllCollectables.Add(new CollectMinijobCollectableBlueprint(collectable.Name, collectable.ColShapeStr, collectable.PropName, collectable.PropPosition, rotation, collectable.AnimIdentifier, collectable.IsShownOnMap, collectable.DestroyedWhenUsed, collectable.SpawnObjectOnInteract));
            }

            CurrentCollectables = [];
        }

        public override void onInteractionStep(IPlayer player, MinijobInstance instance) {
            var menu = new Menu(Name, "Was möchstest du tun?");
            menu.addMenuItem(new StaticMenuItem("Information", Description, ""));
            menu.addMenuItem(new StaticMenuItem("Task-Bearbeitung", $"Gehe an verschiedene Orte auf der GTA Karte und interagiere mit ihnen. Im Feld \"Information\" findest du Hinweise auf was gesucht ist. Oftmals ist an der Stelle durch ein Objekt/Prop markiert, welches nicht direkt ins GTA Bild passt. Ggf. sind die Orte (oder einer von mehreren Orten) auf der Karte markiert, dies ist jedoch nicht immer der Fall. Nach einer gewissen Zeit ist es möglich sich die Position der Orte hier anzeigen zu lassen,", "Siehe Beschreibung"));

            if(DateTime.Now - StartTime > TimeSpan.FromMinutes(7)) {
                menu.addMenuItem(new ClickMenuItem("Ort als Wegpunkt setzen", $"Lasse dir einen der Orte auf der Karte als Wegpunkt anzeigen", "", "ON_COLLECT_MINIJOB_SHOW_LOCATION", MenuItemStyle.yellow).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Instance", instance } }));
            }

            if(CurrentCount >= RequiredCollects) {
                menu.addMenuItem(new ClickMenuItem("Fortfahren", "Fahre fort", "", "ON_COLLECT_MINIJOB_ACCEPT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Instance", instance } }));
            }

            player.showMenu(menu);
        }

        public void onCollect(IPlayer player, CollectMinijobCollectable collectable) {
            var instance = Master.getPlayerInstance(player);

            if(instance != null && collectable != null && !collectable.Disposed) {
                CurrentCount++;

                if(CurrentCount < RequiredCollects) {
                    player.sendNotification(Constants.NotifactionTypes.Info, $"{collectable.Name} erfolgreich abgearbeitet. {CurrentCount}/{RequiredCollects} abgeschlossen!", $"{CurrentCount}/{RequiredCollects} abgeschlossen", Constants.NotifactionImages.MiniJob);
                } else {
                    instance.ConnectedPlayers.ForEach(p => p.sendNotification(Constants.NotifactionTypes.Info, $"Alles abgearbeitet. Kehre zurück!", "Alles abgearbeitet", Constants.NotifactionImages.MiniJob));
                }
            }

            if(collectable.IsShownOnMap) {
                foreach(var p in instance.ConnectedPlayers) {
                    BlipController.destroyBlipByName(p, $"COLLECT_MINIJOB_TASK_{collectable.Id}");
                }
            }

            collectable.Dispose();
        }

        protected override void onShowInfoStep(MinijobInstance instance, IPlayer player) {
            foreach(var collectable in CurrentCollectables) {
                if(!collectable.Disposed && collectable.IsShownOnMap) {
                    collectable.activate();
                    BlipController.createPointBlip(player, collectable.Name, collectable.Shape.Position, Order % 85, 682, 255, $"COLLECT_MINIJOB_TASK_{collectable.Id}");
                }
            }

            if(CurrentCollectables.Any(c => !c.IsShownOnMap)) {
                player.sendNotification(Constants.NotifactionTypes.Info, "Nicht alle Sammelpunkte werden auf der Karte angezeigt. Es werden ggf. einige als Hilfestellung angezeigt. Den Rest wirst du suchen müssen.", "Nicht Sammelobjekte angezeigt!");
            }

            if(HintInvoke == null) {
                HintInvoke = InvokeController.AddTimedInvoke("Hint_Activater", (i) => {
                    instance.ConnectedPlayers.ForEach(p => p.sendNotification(Constants.NotifactionTypes.Info, "Es ist nun möglich sich die Position der Orte auf der Karte anzeigen zu lassen!", "Hinweis möglich!", Constants.NotifactionImages.MiniJob));
                    HintInvoke = null;
                }, TimeSpan.FromMinutes(7), false);
            }
        }

        public override void start(MinijobInstance instance) { }

        protected override void prepareStep() {
            //Would be needed to saved per Instance, but the Task is not MultiViable!
            StartTime = DateTime.Now;

            foreach(var collectable in CurrentCollectables) {
                collectable.activate();
            }
        }

        protected override void resetStep() {
            CurrentCount = 0;

            if(HintInvoke != null) {
                HintInvoke.EndSchedule();
                HintInvoke = null;
            }
        }

        protected override void finishStep(MinijobInstance instance) {
            foreach(var collect in CurrentCollectables) {
                foreach(var player in instance.ConnectedPlayers) {
                    if(collect.IsShownOnMap) {
                        BlipController.destroyBlipByName(player, $"COLLECT_MINIJOB_TASK_{collect.Id}");
                    }
                }
                collect.Dispose();
            }
        }

        protected override void finishForPlayerStep(MinijobInstance instance, IPlayer player) {
            foreach(var collect in CurrentCollectables) {
                if(collect.IsShownOnMap) {
                    BlipController.destroyBlipByName(player, $"COLLECT_MINIJOB_TASK_{collect.Id}");
                }
            }
        }

        public override void preprepare() {
            CurrentCollectables = new();

            var selected = AllCollectables.GetRandomElements(SelectedCollects);
            for(var i = 0; i < selected.Count; i++) {
                var collectable = selected[i];

                Object obj = null;
                if(!collectable.SpawnObjectOnInteract && collectable.PropName != null && collectable.PropName != "") {
                    obj = ObjectController.createObject(collectable.PropName, collectable.PropPosition, collectable.PropRotation);
                }

                var collect = new CollectMinijobCollectable(collectable, i, collectable.Name, this, obj, collectable.ColShapeStr, collectable.AnimIdentifier, collectable.IsShownOnMap, collectable.DestroyedWhenUsed, collectable.SpawnObjectOnInteract);

                CurrentCollectables.Add(collect);
            }
        }

        public override void postreset() {
            foreach(var collect in CurrentCollectables) {
                if(collect.Object != null) {
                    ObjectController.deleteObject(collect.Object);
                }
            }

            CurrentCollectables = new();
        }

        public override bool isMultiViable() {
            return false;
        }

        #region Create Stuff

        public override List<MenuItem> getAdminMenuElements() {
            var list = new List<MenuItem>();

            list.Add(new StaticMenuItem("Ausgewählte Collectables", "Wieviele Collectables von den vorhandenen ausgewählt werden", $"{SelectedCollects}"));
            list.Add(new StaticMenuItem("Benötigte Collectables", "Wieviele Collectables von den ausgewählten eingesammelt werden müssen. Muss kleiner/gleich wie \"Ausgewählte Collectables\" sein", $"{RequiredCollects}"));

            var alreadyList = new Menu("Collectables", "Was möchtest du tun?");

            for(var i = 0; i < AllCollectables.Count; i++) {
                var collectable = AllCollectables[i];

                var collectMenu = new Menu($"{i}: {collectable.Name}", "Was möchstest du tun?");

                collectMenu.addMenuItem(new StaticMenuItem("Name", $"Der Name des Collectables ist: {collectable.Name}", collectable.Name));
                collectMenu.addMenuItem(new StaticMenuItem("PropName", $"Der PropName des Collectables ist: {collectable.PropName}", ""));
                collectMenu.addMenuItem(new StaticMenuItem("PropPosition", $"Der PropPosition des Collectables ist: {collectable.PropPosition.ToJson()}", ""));
                collectMenu.addMenuItem(new StaticMenuItem("CollisionShape", $"Der CollisionShape des Collectables ist: {collectable.ColShapeStr}", ""));
                collectMenu.addMenuItem(new StaticMenuItem("Anim-Name", $"Der Identifier der Animation des Collectables ist: {collectable.AnimIdentifier}", collectable.AnimIdentifier));
                var shownStr = collectable.IsShownOnMap ? "" : "NICHT";
                collectMenu.addMenuItem(new StaticMenuItem("Karte angezeigt", $"Das Collectable wird {shownStr} auf der Karte angezeigt", shownStr));
                var destroyStr = collectable.DestroyedWhenUsed ? "" : "NICHT";
                collectMenu.addMenuItem(new StaticMenuItem("Objekt entfernt", $"Nach dem interagieren wird das Objekt {destroyStr} entfernt.", destroyStr));

                collectMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche das Collectable", "", "ADMIN_COLLECT_MINIJOB_DELETE_COLLECTABLE", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Collectable", collectable } }).needsConfirmation($"{i}: {collectable.Name} löschen", "Wirklich löschen?"));

                alreadyList.addMenuItem(new MenuMenuItem(collectMenu.Name, collectMenu));
            }
            list.Add(new MenuMenuItem(alreadyList.Name, alreadyList));

            var createMenu = new Menu("Collectable erstellen", "Gib die Daten ein");
            createMenu.addMenuItem(new InputMenuItem("Name", "Der Name des Collectables. Muss nicht individuell sein (z.B. Unkraut)", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Propname", "Der GTA Name des Props. Vorher mit /createObject [name] 0 0 0 testen ob das Props funktioniert. Kann auch leer sein", "prop_fire_hydrant_2", ""));
            createMenu.addMenuItem(new InputMenuItem("Prop Position", "Die Gta Position des Props. Das Format ist dasselbe wie in Codewalker, [x, y, z] (ohne [])", "-412.45, -15.05, 45.78", ""));
            createMenu.addMenuItem(new InputMenuItem("Prop Rotation", "Die Rotation des Props. Im Codewalker 'Quaternion' System. [x, y, z, w], (ohne [])", "0, 0, 0.0876, 0.9961", ""));
            createMenu.addMenuItem(new CheckBoxMenuItem("Auf Karte angezeigt", "Gib an ob das Collectable auf der Karte angezeigt wird", false, ""));
            createMenu.addMenuItem(new CheckBoxMenuItem("Objekt entfernen", "Gib an ob das Object, falls es exitiert, nach dem interagieren entfernt wird", true, ""));
            createMenu.addMenuItem(new InputMenuItem("Animname", "Der Identifier der Animation die abgespielt wird. Kann auch leer sein", "", ""));
            createMenu.addMenuItem(new CheckBoxMenuItem("Objekt danach spawnen", "Spawne das Objekt erst nachdem mit dem Ort interagiert worden ist", false, ""));

            createMenu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Erstelle das Collectable", "ADMIN_COLLECT_MINIJOB_CREATE_COLLECTABLE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this } }));

            list.Add(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));
            return list;
        }

        #endregion
    }
}
