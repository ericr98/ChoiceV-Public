using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using BenchmarkDotNet.Disassemblers;
using Bogus.DataSets;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Collectables;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public enum CollectableAreaTypes {
        Mushroom,
        Artefact,
        AllCactus,
        CactusHighland,
        CactusLowland,
        CactusWater,
        AllFrogs,
        AllFlowers,
        FlowerMountain,
    }

    public delegate void CollectableRemoveDelegate(Collectable collectable);

    public class CollectableController : ChoiceVScript {
        public static Dictionary<int, CollectableArea> AllAreas = new Dictionary<int, CollectableArea>();

        public CollectableController() {
            if(Config.IsStressTestActive) {
                return;
            }

            loadAreas();

            InvokeController.AddTimedInvoke("AreaUpdater", updateAreas, TimeSpan.FromSeconds(120), true);

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Misc,
                    "Sammel-Areale",
                    supportCollectableGenerator
                )
            );
            EventController.addMenuEvent("SUPPORT_SHOW_AREA", onSupportShowArea);
            EventController.addMenuEvent("SUPPORT_DELETE_AREA", onSupportDeleteArea);
            EventController.addMenuEvent("SUPPORT_TRIGGER_AREA", onSupportTriggerArea);
            EventController.addMenuEvent("SUPPORT_CREATE_NEW_AREA", onSupportCreateArea);
        }

        private Menu supportCollectableGenerator(IPlayer player) {
            SupportController.setCurrentSupportFastAction(player, () => { player.showMenu(supportCollectableGenerator(player)); });

            var menu = new Menu("Sammel-Areale", "Was möchtest du tun?", true);
            var subMenu = new Menu("Alle Areale", "Wähle eins aus");

            foreach (var area in AllAreas.Values) {
                var areaMenu = new Menu(area.Name, "Was möchtest du tun?");

                areaMenu.addMenuItem(new StaticMenuItem("Typ", $"Der Typ des Areals ist {area.Type}", $"{area.Type}"));
                areaMenu.addMenuItem(new StaticMenuItem("Maximale Objekte", $"Die maximale Anzahl an Objekten die spawnen können ist {area.MaxCollectables}", $"{area.MaxCollectables}"));
                areaMenu.addMenuItem(new StaticMenuItem("Spawn-Verzögerung", $"Neue Objekte spawnen alle {area.SpawnDelay.TotalMinutes}min", $"{area.SpawnDelay.TotalMinutes}min"));

                areaMenu.addMenuItem(new CheckBoxMenuItem("Areal anzeigen?", "Zeige das Areal auf der Karte an", player.hasData($"SUPPORT_AREAL_{area.Id}"), "SUPPORT_SHOW_AREA", MenuItemStyle.normal, false, false)
                    .withData(new Dictionary<string, dynamic> { { "Area", area } }));

                areaMenu.addMenuItem(new InputMenuItem("Spawns triggern", "Triggere das Spawnen von der angegeben Anzahl von Objekten", "", InputMenuItemTypes.number, "SUPPORT_TRIGGER_AREA").withNoCloseOnEnter()
                    .withData(new Dictionary<string, dynamic> { { "Area", area } }));

                areaMenu.addMenuItem(new ClickMenuItem("Areal löschen", "Lösche das Areal", "", "SUPPORT_DELETE_AREA", MenuItemStyle.red)
                    .needsConfirmation($"{area.Name} löschen?", "Wirklich löschen")
                    .withData(new Dictionary<string, dynamic> { { "Area", area } }));

                subMenu.addMenuItem(new MenuMenuItem(areaMenu.Name, areaMenu));
            }
            menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));

            var createMenu = new Menu("Neues Areal", "Erstelle ein neues Areal", false);

            createMenu.addMenuItem(new InputMenuItem("Name", "Wie soll das Areal heißen? Sollte das Areal klar identifiezieren", "", ""));
            var list = Enum.GetValues<CollectableAreaTypes>().Select(t => t.ToString()).ToArray();
            createMenu.addMenuItem(new ListMenuItem("Typ", "Wähle den Typ des Areals aus", list, ""));
            createMenu.addMenuItem(new InputMenuItem("Spawnzeit (min)", "Gib die Zeit ein die das Spawnen eines Objektes benötigt. Die Zeit ist in Minuten.", "z.B.: 5", InputMenuItemTypes.number, ""));
            createMenu.addMenuItem(new InputMenuItem("Maximale Objektzahl", "Gib die maximale Anzahl an spawnbaren Objekten an", "z.B.: 5", InputMenuItemTypes.number, ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Areal setzen", "Setze nun das Areal mit den angegeben Daten", "SUPPORT_CREATE_NEW_AREA", MenuItemStyle.green));
            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));
            return menu;
        }

        private bool onSupportShowArea(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var area = data["Area"] as CollectableArea;

            if (player.hasData($"SUPPORT_AREAL_{area.Id}")) {
                player.resetData($"SUPPORT_AREAL_{area.Id}");
                player.emitClientEvent("AREA_END");
            } else {
                player.setData($"SUPPORT_AREAL_{area.Id}", true);

                foreach (var point in area.Polygon) {
                    player.emitClientEvent("AREA_ADD", point.X, point.Y);
                }
            }

            return true;
        }

        private bool onSupportTriggerArea(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var area = data["Area"] as CollectableArea;

            var evt = menuItemCefEvent as InputMenuItemEvent;
            var amount = int.Parse(evt.input);

            for (var i = 0; i < amount; i++) {
                area.update(true);
            }

            player.sendNotification(NotifactionTypes.Info, $"Es wurden {amount} Objekte gespawnt.", "Spawns getriggert");

            return true;
        }

        private bool onSupportDeleteArea(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var area = data["Area"] as CollectableArea;

            using (var db = new ChoiceVDb()) {
                var dbArea = db.configcollectableareas.FirstOrDefault(a => a.id == area.Id);
                if (dbArea != null) {
                    db.configcollectableareas.Remove(dbArea);
                }
            }

            player.sendNotification(NotifactionTypes.Warning, "Area gelöscht", "Das Areal wurde erfolgreich gelöscht.");

            return true;
        }

        private bool onSupportCreateArea(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var typeEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
            var timeEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var maxEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            var name = nameEvt.input;
            var type = Enum.Parse<CollectableAreaTypes>(typeEvt.currentElement);
            var time = TimeSpan.FromMinutes(int.Parse(timeEvt.input));
            var max = int.Parse(maxEvt.input);

            MapPolygonCreator.startPolygonCreationWithCallback(player, (pols) => {
                using (var db = new ChoiceVDb()) {
                    var dbArea = new configcollectablearea {
                        maxCollectables = max,
                        name = name,
                        spawnDelay = (int)time.TotalMilliseconds,
                        type = type + "",
                    };

                    db.configcollectableareas.Add(dbArea);
                    db.SaveChanges();


                    foreach (var pol in pols) {
                        var dbPol = new configcollectableareapoint {
                            areaId = dbArea.id,
                            x = pol.X,
                            y = pol.Y,
                        };

                        db.configcollectableareapoints.Add(dbPol);
                    }

                    db.SaveChanges();

                    var area = new CollectableArea(dbArea.id, dbArea.name, (CollectableAreaTypes)Enum.Parse(typeof(CollectableAreaTypes), dbArea.type), dbArea.maxCollectables, TimeSpan.FromMilliseconds(dbArea.spawnDelay));
                    AllAreas.Add(area.Id, area);

                    area.Polygon = pols;

                    player.sendNotification(NotifactionTypes.Success, "Area erstellt", "Das Areal wurde erfolgreich erstellt.");
                }
            });

            return true;
        }


        private void loadAreas() {
            using (var db = new ChoiceVDb()) {

                foreach (var row in db.configcollectableareas) {
                    var area = new CollectableArea(row.id, row.name, (CollectableAreaTypes)Enum.Parse(typeof(CollectableAreaTypes), row.type), row.maxCollectables, TimeSpan.FromMilliseconds(row.spawnDelay));
                    AllAreas.Add(area.Id, area);
                }

                foreach (var row in db.configcollectableareapoints) {
                    var p = new Vector2(row.x, row.y);
                    var area = AllAreas[row.areaId];
                    area.Polygon.Add(p);
                }
            }
        }

        private void updateAreas(IInvoke ivk) {
            foreach (var area in AllAreas.Values) {
                area.update();
            }
        }
    }

    public class CollectableArea {
        public int Id;
        public string Name;
        public CollectableAreaTypes Type;
        public List<Collectable> AllCollectables = new List<Collectable>();
        public int MaxCollectables;
        public List<Vector2> Polygon = new List<Vector2>();

        public TimeSpan SpawnDelay;
        public DateTime NextCollectableSpawn;

        public CollectableArea(int id, string name, CollectableAreaTypes type, int maxCollectables, TimeSpan spawnDelay) {
            Id = id;
            Name = name;
            Type = type;
            MaxCollectables = maxCollectables;
            SpawnDelay = spawnDelay;

            NextCollectableSpawn = DateTime.Now;

        }

        public void update(bool overrideTime = false) {
            if (NextCollectableSpawn < DateTime.Now || overrideTime) {
                //Renew the oldest Mushroom
                if (AllCollectables.Count == MaxCollectables) {
                    var reNew = AllCollectables.First();
                    reNew.destroy();
                }

                var point = PolygonMethods.getEnclosedPoint(Polygon.ToArray());
                if (point == Vector2.Zero) {
                    return;
                }

                var spawn = new Position(Convert.ToSingle(point.X), Convert.ToSingle(point.Y), WorldController.getGroundHeightAt(Convert.ToSingle(point.X), Convert.ToSingle(point.Y)));
                Collectable collectable = null;
                switch (Type) {
                    case CollectableAreaTypes.Mushroom:
                        collectable = new CollectableMushroom(Type, spawn);
                        break;
                    case CollectableAreaTypes.Artefact:
                        collectable = new CollectableArtefact(Type, spawn);
                        break;
                    case CollectableAreaTypes.AllCactus:
                    case CollectableAreaTypes.CactusLowland:
                    case CollectableAreaTypes.CactusHighland:
                    case CollectableAreaTypes.CactusWater:
                        collectable = new CollectableCactus(Type, spawn);
                        break;
                    case CollectableAreaTypes.AllFlowers:
                    case CollectableAreaTypes.FlowerMountain:
                        collectable = new CollectableFlower(Type, spawn);
                        break;
                    case CollectableAreaTypes.AllFrogs:
                        collectable = new CollectableFrog(Type, spawn);
                        break;
                }

                if (collectable != null) {
                    collectable.spawn();
                    collectable.CollectableRemoveDelegate += onCollectableCollect;
                    AllCollectables.Add(collectable);
                }

                NextCollectableSpawn = DateTime.Now + SpawnDelay;
            }
        }

        private void onCollectableCollect(Collectable obj) {
            AllCollectables.Remove(obj);
        }
    }
}
