using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using BenchmarkDotNet.Diagnosers;
using Bogus;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public delegate List<MenuItem> NPCModuleGeneratorDelegate(ref Type codeType);
    public delegate void NPCModuleGeneratorCallbackDelegate(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback);

    public record ModuleCreateUnion(NPCModuleGeneratorDelegate Generator, NPCModuleGeneratorCallbackDelegate Callback);

    class PedController : ChoiceVScript {
        public static int NonDbPedsId = -2;
        private static List<ChoiceVPed> AllPeds = new List<ChoiceVPed>();

        public static TimeSpan MODULE_TICK_TIME = TimeSpan.FromMinutes(5);

        private static Dictionary<string, ModuleCreateUnion> ModuleGenerators = new();

        public PedController() {
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;
            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            EventController.PedInteractionDelegate += onPedInteractionDelegate;

            loadAllPeds();
            EventController.MainAfterReadyDelegate += onLoadPedModules;

            EventController.addEvent("PED_BEING_THREATEND", onPedBeingThreatend);


            InvokeController.AddTimedInvoke("Ped-Module-Tick", _ => {
                AllPeds.ForEach(p => p.onTick(MODULE_TICK_TIME));
            }, MODULE_TICK_TIME, true);

            InvokeController.AddTimedInvoke("Ped-Module-Short-Tick", _ => {
                AllPeds.ForEach(p => p.onShortTick());
            }, TimeSpan.FromSeconds(5), true);

            addNPCModuleGenerator("Nicht-bedrohbar Modul", noThreatenModuleGenerator, noThreatenModuleCallback);
            addNPCModuleGenerator("Keine-Interaktion Modul", noInteractionModuleGenerator, noInteractionModuleCallback);
            addNPCModuleGenerator("Statischer-Text Modul", staticTextModuleGenerator, staticTextModuleCallback);
            addNPCModuleGenerator("Collisionshape Änderungs Modul", collisionShapeRelocateModuleGenerator, collisionShapeRelocateModuleCallback);

            SupportController.AdminModeToggleDelegate += onAdminModeToggle;

            #region Support

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    getPedCreationMenu,
                    3,
                    SupportMenuCategories.Misc,
                    "NPC-Erstellung"
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_PED", onSupportCreatePed);
            EventController.addMenuEvent("SUPPORT_DELETE_NPC", onSupportDeletePed);
            EventController.addMenuEvent("SUPPORT_ON_PED_CREATE_NEW_MODULE", onSupportCreateNewModule);
            EventController.addMenuEvent("SUPPORT_ON_DELETE_NPC_MODULE", onSupportDeleteNpcModule);

            #endregion
        }

        private void loadAllPeds() {
            using(var db = new ChoiceVDb()) {
                foreach(var row in db.configpeds.Include(p => p.pedsdata).Include(p => p.configpedsmodules)) {
                    var ped = createPed(row);

                    var ev = ped.CollisionShape != null ? ped.CollisionShape.EventName : "null";
                    Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"ConfigPed loaded: model: {ped}");
                }
            }

            Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, "PedController: {" + AllPeds.Count + "} Peds loaded");
        }

        private void onLoadPedModules() {
            using(var db = new ChoiceVDb()) {
                foreach(var ped in AllPeds) {
                    var row = db.configpeds.Include(p => p.configpedsmodules).FirstOrDefault(p => p.id == ped.Id);
                    if(row != null) {
                        foreach(var dbModule in row.configpedsmodules) {
                            var type = Type.GetType(dbModule.codeItem, false);
                            var module = (NPCModule)Activator.CreateInstance(type, ped, dbModule.settings.FromJson<Dictionary<string, dynamic>>());
                            module.DbId = dbModule.id;
                            ped.addModule(module);
                        }
                    }
                }
            }
        }

        private void onPlayerConnect(IPlayer player, character character) {
            foreach(var ped in AllPeds.Where(p => p.Seer == player || p.Seer == null).Reverse()) {
                ped.spawn(player);
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            foreach(var ped in AllPeds.Reverse<ChoiceVPed>().Where(p => p.Seer == player)) {
                destroyPed(ped);
            }
        }

        private void onPedInteractionDelegate(IPlayer player, string modelhash, Position position) {
            var peds = AllPeds.Where(p => ChoiceVAPI.Hash(p.Model).ToString() == modelhash);
            ChoiceVPed ped = null;
            var currentDist = float.MaxValue;
            foreach(var p in peds) {
                var newDist = p.Position.Distance(position);
                if(newDist < currentDist) {
                    currentDist = newDist;
                    ped = p;
                }
            }
            if(ped != null) {
                if(player.getCharacterData().AdminMode && player.getAdminLevel() >= 3) {
                    var menu = new Menu($"{ped.Name} (Admin)", "Was möchtest du tun?");

                    var moduleMenu = new Menu("Module anzeigen", "Alle aktuellen Module");
                    foreach(var module in ped.getModules()) {
                        moduleMenu.addMenuItem(module.getAdminMenuStaticRepresentative(player));
                    }
                    menu.addMenuItem(new MenuMenuItem(moduleMenu.Name, moduleMenu));


                    var newModuleMenu = new Menu("Modul hinzufügen", "Welches Modul?");
                    foreach(var module in ModuleGenerators) {
                        var virtMenu = new VirtualMenu(module.Key, () => {
                            var men = new Menu(module.Key, "Gib die Daten ein");

                            Type codeType = null;
                            foreach(var item in module.Value.Generator.Invoke(ref codeType)) {
                                men.addMenuItem(item);
                            }

                            men.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das angegebene Modul", "SUPPORT_ON_PED_CREATE_NEW_MODULE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "CodeItem", codeType.FullName }, { "Name", module.Key }, { "Ped", ped } }).needsConfirmation("Modul erstellen?", "Modul wirklich erstellen?"));
                            return men;
                        });

                        newModuleMenu.addMenuItem(new MenuMenuItem(virtMenu.Name, virtMenu));
                    }
                    menu.addMenuItem(new MenuMenuItem(newModuleMenu.Name, newModuleMenu));

                    var moduleRemove = new Menu("Module entfernen", "Entferne aktuelle Module");
                    foreach(var module in ped.getModules()) {
                        var rep = module.getAdminMenuStaticRepresentative(player);
                        moduleRemove.addMenuItem(new ClickMenuItem(rep.Name, rep.Description, "löschen", "SUPPORT_ON_DELETE_NPC_MODULE", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Ped", ped }, { "Module", module } }).needsConfirmation("Module löschen?", $"{rep.Name} wirklich löschen?"));
                    }
                    menu.addMenuItem(new MenuMenuItem(moduleRemove.Name, moduleRemove, MenuItemStyle.red));

                    menu.addMenuItem(new ClickMenuItem("NPC löschen", "Lösche diesen NPC", "", "SUPPORT_DELETE_NPC", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Ped", ped } }).needsConfirmation("NPC löschen?", "NPC wirklich löschen?"));

                    player.showMenu(menu);
                } else {
                    if(ped.CollisionShape != null) {
                        ped.CollisionShape.Interaction(player);
                    }
                }
            }
        }

        public static ChoiceVPed findPed(Predicate<ChoiceVPed> predicate) {
            return AllPeds.Find(predicate);
        }

        public static List<ChoiceVPed> findPeds(Predicate<ChoiceVPed> predicate) {
            return AllPeds.Where(p => predicate(p)).ToList();
        }

        public static ChoiceVPed createPedDb(string name, string model, Position position, float heading, int headVariation = 0, int torosVariation = 0, Dictionary<string, dynamic> data = null) {
            var scenario = data != null && data.ContainsKey("Scenario") ? data["Scenario"] : null;
            var animDict = data != null && data.ContainsKey("AnimDict") ? data["AnimDict"] : null;
            var animName = data != null && data.ContainsKey("AnimName") ? data["AnimName"] : null;
            var animFlag = data != null && data.ContainsKey("AnimFlag") ? data["AnimFlag"] : null;
            var animPercent = data != null && data.ContainsKey("AnimPercent") ? data["AnimPercent"] : null;

            using(var db = new ChoiceVDb()) {
                var ped = new configped {
                    name = name,
                    model = model,
                    x = position.X,
                    y = position.Y,
                    z = position.Z,
                    heading = heading,
                    headComponent = headVariation,
                    torsoComponent = torosVariation,
                    animDict = animDict == "" ? null : animDict,
                    animName = animName,
                    animFlag = animFlag,
                    animPercent = animPercent,

                    scenario = scenario == "" ? null : scenario,
                };

                db.configpeds.Add(ped);
                db.SaveChanges();

                return createPed(ped);
            }
        }

        public static ChoiceVPed createPed(string name, string model, Position position, float heading, int headVariation = 0, int torsoVariation = 0, Dictionary<string, dynamic> data = null, int id = -1) {
            var ped = new ChoiceVPed(id, name, model, position, heading, null, headVariation, torsoVariation, data);
            AllPeds.Add(ped);

            var players = ChoiceVAPI.GetAllPlayers();
            foreach(var player in players) {
                ped.spawn(player);
            }

            return ped;
        }

        public static ChoiceVPed createPed(configped dbPed) {
            var ped = createPed(dbPed.name, dbPed.model, new Position(dbPed.x, dbPed.y, dbPed.z), dbPed.heading, dbPed.headComponent, dbPed.torsoComponent, dbPed.pedsdata.ToDictionary(d => d.name, d => d.value.FromJson<dynamic>()), dbPed.id);
            ped.Id = dbPed.id;

            if(dbPed.scenario != null) {
                ped.setScenario(dbPed.scenario);
            }

            if(dbPed.animDict != null) {
                ped.setStandardAnimation(dbPed.animDict, dbPed.animName, dbPed.animFlag ?? 1, -1, dbPed.animPercent ?? 0);
            }

            return ped;
        }

        public static ChoiceVPed createPed(IPlayer player, string name, string model, Position position, float heading, int headVariation = 0, int torsoVariation = 0) {
            var ped = new ChoiceVPed(-1, name, model, position, heading, player, headVariation, torsoVariation);
            AllPeds.Add(ped);

            ped.spawn(player);

            return ped;
        }

        public static void addPedModuleDb(ChoiceVPed ped, NPCModule module) {
            if(ped.Id < 0) {
                throw new Exception("Ped is not a db ped");
            }

            var settings = module.getSettings();
            using(var db = new ChoiceVDb()) {
                var newModule = new configpedsmodule {
                    pedId = ped.Id,
                    codeItem = module.GetType().FullName,
                    settings = settings.ToJson()
                };

                db.configpedsmodules.Add(newModule);
                db.SaveChanges();

                module.DbId = newModule.id;
                ped.addModule(module);
            }
        }

        private void onAdminModeToggle(IPlayer player, bool toggle) {
            AllPeds.ForEach(p => p.setAdminVisible(player, toggle));
        }
        public static void setPedStyle(ChoiceVPed ped, characterstyle style) {
            if(ped.Seer != null) {
                ped.Seer.emitClientEvent("SET_PED_STYLE", ped.Id, style.ToJson());
            } else {
                ChoiceVAPI.emitClientEventToAll("SET_PED_STYLE", ped.Id, style.ToJson());
            }
        }

        public static void setPedPlayerClothing(ChoiceVPed ped, ClothingPlayer clothing) {
            var toList = new List<IPlayer>();
            if(ped.Seer != null) {
                toList.Add(ped.Seer);
            } else {
                toList = ChoiceVAPI.GetAllPlayers();
            }

            foreach(var player in toList) {
                foreach(var slot in ClothingPlayer.ClothingSlots) {
                    var component = clothing.GetSlot(slot, false);
                    player.emitClientEvent("SET_PED_CLOTHES", ped.Id, slot, component.Drawable, component.Texture, component.Dlc);
                }

                foreach(var slot in ClothingPlayer.AccessoireSlots) {
                    var component = clothing.GetSlot(slot, true);
                    player.emitClientEvent("SET_PED_ACCESSOIRE", ped.Id, slot, component.Drawable, component.Texture, component.Dlc);
                }
            }
        }

        public static void destroyPed(ChoiceVPed ped) {
            if(ped.Seer != null && ped.Seer.Exists()) {
                ped.Seer.emitClientEvent(Constants.PlayerDestroyStaticPed, ped.Id);
            } else {
                ChoiceVAPI.emitClientEventToAll(Constants.PlayerDestroyStaticPed, ped.Id);
            }
           
            using(var db = new ChoiceVDb()) {
                var find = db.configpeds.FirstOrDefault(p => p.id == ped.Id);

                if(find != null) {
                    db.configpeds.Remove(find);
                    db.SaveChanges();
                }
            }

            ped.remove();
            AllPeds.Remove(ped);
        }

        internal static void onPedDataValueChanged(ChoiceVPed ped, string key, dynamic value) {
            using(var db = new ChoiceVDb()) {
                var data = db.pedsdata.Find(ped.Id, key);
                if(data != null) {
                    data.value = ((object)value).ToJson();
                } else {
                    var datum = new pedsdatum {
                        pedId = ped.Id,
                        name = key,
                        value = ((object)value).ToJson()
                    };

                    db.pedsdata.Add(datum);
                }
                db.SaveChanges();
            }
        }

        internal static bool onPedDataValueRemoved(ChoiceVPed ped, string key) {
            var worked = false;
            using(var db = new ChoiceVDb()) {
                var data = db.pedsdata.Find(ped.Id, key);
                if(data != null) {
                    db.pedsdata.Remove(data);
                    worked = true;
                }
                db.SaveChanges();

            }
            return worked;
        }

        private bool onPedBeingThreatend(IPlayer player, string eventName, object[] args) {
            var pedId = int.Parse(args[0].ToString());

            var ped = AllPeds.FirstOrDefault(p => p.Id == pedId);

            if(ped != null && !ped.hasModule<NPCCannotBeThreatendModule>(m => true)) {
                if(!ped.IsBeingThreatend) {
                    var weapon = WeaponController.getConfigWeapon(player.CurrentWeapon);
                    CamController.checkIfCamSawAction(player, "Person bedroht", $"Die Person hat eine andere Person mit einer/einem {weapon?.displayName} bedroht.");

                    ped.playAnimation("random@mugging3", "handsup_standing_base", 1, 999_999_999, 0, 1);
                }
                ped.LastThreatendDate = DateTime.Now;
            }

            return true;
        }

        public static void addNPCModuleGenerator(string name, NPCModuleGeneratorDelegate generator, NPCModuleGeneratorCallbackDelegate callback) {
            ModuleGenerators.Add(name, new ModuleCreateUnion(generator, callback));
        }

        public static void updatePedModuleSettings(NPCModule module, string key, dynamic newValue) {
            using(var db = new ChoiceVDb()) {
                var dbModule = db.configpedsmodules.Find(module.DbId);

                if(dbModule != null) {
                    var settings = dbModule.settings.FromJson<Dictionary<string, dynamic>>();
                    settings[key] = newValue;

                    dbModule.settings = settings.ToJson();
                    db.SaveChanges();
                } else {
                    Logger.logError($"Could not find module in db {module.DbId} with key {key}");
                }
            }
        } 
        
        private List<MenuItem> noThreatenModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCCannotBeThreatendModule);

            return new List<MenuItem>();
        }

        private void noThreatenModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            creationFinishedCallback.Invoke(new Dictionary<string, dynamic>());
        }

        private List<MenuItem> noInteractionModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCNoInteractModule);

            return new List<MenuItem>();
        }

        private void noInteractionModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            creationFinishedCallback.Invoke(new Dictionary<string, dynamic>());
        }

        private List<MenuItem> staticTextModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCSimpleTextModule);

            return new List<MenuItem> {
                new InputMenuItem("Text", "Der Text der den NPC sagt.", "", "")
            };
        }

        private void staticTextModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            var textEvt = evt.elements[0].FromJson<InputMenuItemEvent>();

            creationFinishedCallback.Invoke(new Dictionary<string, dynamic>() { { "Text", textEvt.input } });
        }

        private List<MenuItem> collisionShapeRelocateModuleGenerator(ref Type codeType) {
            codeType = typeof(NPCRelocateCollisionModule);

            return new List<MenuItem>();
        }

        private void collisionShapeRelocateModuleCallback(IPlayer player, MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                creationFinishedCallback.Invoke(
                    new Dictionary<string, dynamic> {
                        { "ColshapeStr", CollisionShape.getShortSaveForData(p, w, h, r, true, false, true) }
                    });
            });
        }

        #region Support Stuff

        private Menu getPedCreationMenu() {
            var menu = new Menu("NPC-Erstellung", "Erstelle einen NPC");

            menu.addMenuItem(new InputMenuItem("Name", "Der Name des NPCs. Leerlassen um zufälligen Namen zu erzeugen", "", ""));
            menu.addMenuItem(new InputMenuItem("Model", "Der Gta Model Name des Peds", "", ""));

            menu.addMenuItem(new InputMenuItem("Kopf/Hautfarbe", "Setze die Kopfform i.V.m der Hautfarbe des NPCs. Es ist normalerweise eine Zahl von 0 - n", "", ""));
            menu.addMenuItem(new InputMenuItem("Torso/Oberteil", "Setze den Torso bzw. das Oberteil des Peds.", "", ""));

            var animMenu = new Menu("Animation/Szenarios", "Gib die Daten ein");
            animMenu.addMenuItem(new InputMenuItem("Szenario", "Das Szenario welches das Ped abspielt. Kann leer gelassen werden!", "", ""));
            animMenu.addMenuItem(new InputMenuItem("Animations-Dict", "Die Animation welches das Ped abspielt. Kann leer gelassen werden!", "", ""));
            animMenu.addMenuItem(new InputMenuItem("Animations-Name", "Die Animation welches das Ped abspielt. Kann leer gelassen werden!", "", ""));
            animMenu.addMenuItem(new InputMenuItem("Animations-Flag", "Die Animation welches das Ped abspielt. Kann leer gelassen werden!", "", ""));
            animMenu.addMenuItem(new InputMenuItem("Animations-Prozent", "Der Prozentwert ab welchem die Animation startet. Muss zwischen 0 und 100 sein", "", ""));
            animMenu.addMenuItem(new StaticMenuItem("Zurück", "Benutze dieses Menü Item zum zurück zu kommen", ""));

            menu.addMenuItem(new MenuMenuItem(animMenu.Name, animMenu));

            menu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle das Beschriebene Ped.", "SUPPORT_CREATE_PED", MenuItemStyle.green, true).needsConfirmation("Ped so erstellen?", "Ped wirklich erstellen?"));
            return menu;
        }

        private bool onSupportCreatePed(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            string name = null;
            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            if(nameEvt.input != null && nameEvt.input != "") {
                name = nameEvt.input;
            }
            var model = evt.elements[1].FromJson<InputMenuItemEvent>().input;
            var head = int.Parse(evt.elements[2].FromJson<InputMenuItemEvent>().input);
            var torso = int.Parse(evt.elements[3].FromJson<InputMenuItemEvent>().input);

            string scenario = null;
            string animDic = null;
            string animName = null;
            int? animFlag = null;
            float? animPercent = null;


            if(evt.elements[4] != "null") {
                try {
                    scenario = evt.elements[4].FromJson<InputMenuItemEvent>().input;
                    animDic = evt.elements[5].FromJson<InputMenuItemEvent>().input;
                    animName = evt.elements[6].FromJson<InputMenuItemEvent>().input;
                    animFlag = int.Parse(evt.elements[7].FromJson<InputMenuItemEvent>().input);
                    animPercent = float.Parse(evt.elements[8].FromJson<InputMenuItemEvent>().input);
                } catch(Exception) { }
            }

            if(evt.action == "changed") {
                var forward = player.getForwardVector();
                var ped = createPed(player, "", model, new Position(player.Position.X + forward.X * 2, player.Position.Y + forward.Y * 2, player.Position.Z - 1), 180 - ((DegreeRotation)player.Rotation).Yaw, head, torso);
                if(scenario != null) {
                    ped.setScenario(scenario);
                }

                if(animDic != null) {
                    ped.playAnimation(animDic, animName, animFlag ?? 1, 1000000, animPercent ?? 0);
                }

                player.setData("SUPPORT_PED_POS", player.Position);
                player.setData("SUPPORT_PED_ROT", ((DegreeRotation)player.Rotation).Yaw);
                InvokeController.AddTimedInvoke("", (i) => {
                    destroyPed(ped);
                }, TimeSpan.FromSeconds(10), false);
            } else if(evt.action == "enter") {
                var pos = (Position)player.getData("SUPPORT_PED_POS"); 
                pos.Z -= 1;
                var yaw = (float)player.getData("SUPPORT_PED_ROT");

                var ped = createPedDb(name, model, pos, yaw, head, torso, new Dictionary<string, dynamic> {
                    { "Scenario", scenario },
                    { "AnimDict", animDic },
                    { "AnimName", animName },
                    { "AnimFlag", animFlag },
                    { "AnimPercent", animPercent }});

                player.sendNotification(Constants.NotifactionTypes.Success, "Ped wurde erstellt", "");
            }
            return true;
        }

        private bool onSupportDeletePed(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var ped = (ChoiceVPed)data["Ped"];

            ChoiceVAPI.GetAllPlayers().ForEach(pl => ped.delete(pl));
            AllPeds.Remove(ped);

            using(var db = new ChoiceVDb()) {
                var find = db.configpeds.FirstOrDefault(p => p.id == ped.Id);

                if(find != null) {
                    db.configpeds.Remove(find);
                    db.SaveChanges();
                }
            }

            player.sendNotification(Constants.NotifactionTypes.Warning, "Ped erfolgreich gelöscht!", "Ped gelöscht!");

            return true;
        }

        private bool onSupportCreateNewModule(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var name = (string)data["Name"];
            var codeItem = (string)data["CodeItem"];
            var ped = (ChoiceVPed)data["Ped"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            ModuleGenerators[name].Callback.Invoke(player, evt, (settings) => {
                using(var db = new ChoiceVDb()) {
                    var newModule = new configpedsmodule {
                        pedId = ped.Id,
                        codeItem = codeItem,
                        settings = settings.ToJson(),
                    };

                    db.configpedsmodules.Add(newModule);
                    db.SaveChanges();

                    var type = Type.GetType(codeItem, false);
                    var module = (NPCModule)Activator.CreateInstance(type, ped, settings);
                    module.DbId = newModule.id;
                    ped.addModule(module);
                    player.sendNotification(Constants.NotifactionTypes.Success, "Modul erfolgreich erstellen", "");
                }

            });
            return true;
        }

        private bool onSupportDeleteNpcModule(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var ped = (ChoiceVPed)data["Ped"];
            var module = (NPCModule)data["Module"];

            deletePedModule(ped, module);

            player.sendNotification(Constants.NotifactionTypes.Warning, "Modul erfolgreich entfernt!", "Modul entfernt");

            return true;
        }

        public static void deletePedModule(ChoiceVPed ped, NPCModule module) {
            ped.removeModule(module);

            if(module.DbId != null) {
                using(var db = new ChoiceVDb()) {
                    var mod = db.configpedsmodules.Find(module.DbId);

                    if(mod != null) {
                        db.configpedsmodules.Remove(mod);

                        db.SaveChanges();
                    }
                }
            }
        }

        #endregion
    }
}
