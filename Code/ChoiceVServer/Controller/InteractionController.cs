using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public delegate void KeyInteractionCallDelegate(IPlayer player);
    public delegate void ObjectInteractionDelegate(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu);

    public class ObjectInteractionCallback {
        public string Identifier;
        public ObjectInteractionDelegate Callback;
        public bool AllowOnBusy;

        public ObjectInteractionCallback(string identifier, string name, ObjectInteractionDelegate callback, bool allowOnBusy) {
            Identifier = identifier;
            Callback = callback;
            AllowOnBusy = allowOnBusy;
        }
    }

    public class InteractionController : ChoiceVScript {
        private static List<string> InteractableObjectList = new List<string>();
        private static List<ConditionalKeyInteractionCallback> AllKeyInteractionCallbacks = new List<ConditionalKeyInteractionCallback>();
        public static Dictionary<string, ObjectInteractionCallback> AllObjectInteractionCallbacks = new Dictionary<string, ObjectInteractionCallback>();
  
        public static Dictionary<string, configobject> AllConfigObjects { get; private set; }
        public static Dictionary<string, List<string>> AllServerGeneratedObjects { get; private set; }

        private InteractionObject InteractionObject = new InteractionObject();
        public static List<InteractionMenuElement> PlayerInteractionMenuElements = new List<InteractionMenuElement>();
        public static List<InteractionMenuElement> VehicleInteractionMenuElements = new List<InteractionMenuElement>();

        public InteractionController() {
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;
            EventController.addKeyEvent("INTERACTION", ConsoleKey.E, "Interaktion", onInteraction);
            EventController.addKeyEvent("OBJECT_INTERACT", ConsoleKey.N, "Objekt-Interaktion", onObjectInteraction, true);

            EventController.ObjectInteractionDelegate += onObjectInteraction;
            EventController.addInteractionEvent(BaseObjectType.Player, onPlayerInteraction);
            EventController.addInteractionEvent(BaseObjectType.Vehicle, onVehicleInteraction);

            loadConfigObjects();
            InteractionObject.loadInteractionObject();

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Registrieren,
                    "Config-Objects Menü",
                    objMenuGenerator
                )
            );
            EventController.addMenuEvent("SUPPORT_CREATE_CONFIG_OBJECT", onSupportCreateConfigObject);
            EventController.addMenuEvent("SUPPORT_DELETE_CONFIG_OBJECT", onSupportDeleteConfigObject);
            EventController.addMenuEvent("SUPPORT_ENTER_OBJECT_REGISTER_MODE", onSupportEnterObjectRegisterMode);
        }

        //EntityInteraction
        public static bool onPlayerInteraction(IPlayer player, IEntity target, uint model, BaseObjectType BaseObjectType, Position position, Rotation rotation) {
            if(player.Position.Distance(target.Position) > Constants.MAX_PLAYER_INTERACTION_RANGE) {
                player.sendBlockNotification("Du bist zu weit weg!", "Zu weit weg!", Constants.NotifactionImages.System);
                return false;
            }

            if(player.IsInVehicle) {
                player.sendBlockNotification("Du kannst das nicht tun, während du im Fahrzeug bist!", "Fahrzeug blockiert", Constants.NotifactionImages.System);
                return false;
            }

            var menu = new Menu("Spieler Interaktion", "Was möchtest du tun?");

            foreach(var element in PlayerInteractionMenuElements) {
                if(element.checkShow(player, target, player.getBusy())) {
                    var el = element.getMenuElement(player, target);
                    if(el != null) {
                        if(el is MenuItem) {
                            var item = el as MenuItem;
                            if(item.Data != null) {
                                item.Data["InteractionTargetBaseType"] = BaseObjectType;
                                item.Data["InteractionTargetId"] = (target as IPlayer).getCharacterId();
                                item.Data["InteractionTarget"] = (target as IPlayer);
                            } else {
                                var data = new Dictionary<string, dynamic> {
                                    { "InteractionTargetBaseType", BaseObjectType },
                                    { "InteractionTargetId", (target as IPlayer).getCharacterId()},
                                    { "InteractionTarget", target as IPlayer},
                                };

                                item.Data = data;
                            }
                            menu.addMenuItem(el as MenuItem);
                        } else {
                            var virtualMenu = el as VirtualMenu;

                            //Make MenuItem only using generator, to reduce cpu power
                            menu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu));
                        }
                    }
                }
            }

            player.showMenu(menu);
            return true;
        }

        /// <summary>
        /// Register Objects, that can be selected in the clientside
        /// </summary>
        public static void addInteractableObjects(List<string> hashes, string identifier = null) {
            InteractableObjectList = InteractableObjectList.Concat(hashes).Distinct().ToList();
            if(identifier != null) {
                if(AllServerGeneratedObjects == null) {
                    AllServerGeneratedObjects = new();
                }

                foreach(var hash in hashes) {
                    if(!AllServerGeneratedObjects.ContainsKey(hash)) {
                        AllServerGeneratedObjects.Add(hash, new List<string>() { identifier});
                    } else if (!AllServerGeneratedObjects[hash].Contains(identifier)) { 
                        AllServerGeneratedObjects[hash].Add(identifier);
                    }
                }
            }

            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("SET_INTERACTABLE_OBJECTS", InteractableObjectList);
            }
        }

        private void onPlayerConnect(IPlayer player, character character) {
            player.emitClientEvent("SET_INTERACTABLE_OBJECTS", InteractableObjectList);
        }

        /// <summary>
        /// Registers a callback that is triggered when the player is clicking E (if no collision callbacks match)
        /// </summary>
        public static void addKeyInteractionCallback(ConditionalKeyInteractionCallback conditionalCallback) {
            AllKeyInteractionCallbacks.Add(conditionalCallback);
        }

        /// <summary>
        /// Registers a callback that is triggered when the player is interacting with an object that has an identifier (configobjects table)
        /// </summary>
        /// <param name="name">specify a name for adding the object with support menu. put null or "" to ignore</param>
        public static void addObjectInteractionCallback(string identifier, string name, ObjectInteractionDelegate callback, bool allowOnBusy = false) {
            AllObjectInteractionCallbacks[identifier] = new ObjectInteractionCallback(identifier, name, callback, allowOnBusy);
        }

        private bool onVehicleInteraction(IPlayer player, IEntity target, uint model, BaseObjectType BaseObjectType, Position position, Rotation rotation) {
            if(target is ChoiceVVehicle) {
                openVehicleInteractionMenu(player, target as ChoiceVVehicle);
            } else {
                player.sendBlockNotification("Irgendetwas ist schiefgelaufen!", "Fehler verursacht");
            }

            return true;
        }

        public static void openVehicleInteractionMenu(IPlayer player, ChoiceVVehicle vehicle) {
            var menu = new Menu("Fahrzeug Interaktion", "Was möchtest du tun?");
            menu.addMenuItem(new StaticMenuItem("Chassisnummer", $"Die Chassisnummer des Fahrzeuges ist: {vehicle.ChassisNumber}", $"{vehicle.ChassisNumber}"));
            menu.addMenuItem(new StaticMenuItem("Kraftstoffart", $"Die Kraftstoffart des Fahrzeuges is {vehicle.FuelType}", $"{vehicle.FuelType}"));
        
            foreach(var element in VehicleInteractionMenuElements) {
                if(element.checkShow(player, vehicle, player.getBusy())) {
                    var el = element.getMenuElement(player, vehicle);
                    if(el != null) {
                        if(el is MenuItem item) {
                            if(item.Data != null) {
                                item.Data["InteractionTarget"] = (vehicle as ChoiceVVehicle);
                                item.Data["InteractionTargetId"] = (vehicle as ChoiceVVehicle).VehicleId;
                                item.Data["InteractionTarget"] = (vehicle as ChoiceVVehicle);
                            } else {
                                var data = new Dictionary<string, dynamic> {
                                    { "InteractionTarget", (vehicle as ChoiceVVehicle)},
                                    { "InteractionTargetId", (vehicle as ChoiceVVehicle).VehicleId},
                                    { "InteractionTarget", (vehicle as ChoiceVVehicle)},
                                };
                                item.Data = data;
                            }

                            menu.addMenuItem(item);
                        } else {
                            var virtualMenu = el as VirtualMenu;

                            //Make MenuItem only using generator, to reduce cpu power
                            menu.addMenuItem(new MenuMenuItem(virtualMenu.Name, virtualMenu, virtualMenu.Style));
                        }
                    }
                }
            }

            player.showMenu(menu);
        }

        /// <summary>
        /// Add a Element to the Player to player InteractionElement
        /// </summary>
        public static void addPlayerInteractionElement(InteractionMenuElement element, List<CharacterType> showForTypes = null) {
            element.ShowForTypes = showForTypes ?? [CharacterType.Player];
            
            PlayerInteractionMenuElements.Add(element);
        }

        /// <summary>
        /// Add a Element to the Player to player InteractionElement
        /// </summary>
        public static void addVehicleInteractionElement(InteractionMenuElement element, List<CharacterType> showForTypes = null) {
            element.ShowForTypes = showForTypes ?? [CharacterType.Player];
            
            VehicleInteractionMenuElements.Add(element);
        }

        //Object Interaction
        private void loadConfigObjects(bool updatePlayers = true) {
            AllConfigObjects = new Dictionary<string, configobject>();
            using(var db = new ChoiceVDb()) {
                var objs = db.configobjects;

                foreach(var obj in objs) {
                    string input;
                    if(obj.modelHash != null) {
                        input = obj.modelHash;
                    } else {
                        input = ChoiceVAPI.Hash(obj.modelName).ToString();
                    }

                    AllConfigObjects.Add(input, obj);
                }
            }

            if(updatePlayers) {
                addInteractableObjects(AllConfigObjects.Keys.ToList());
            }
        }

        private bool onInteraction(IPlayer player, ConsoleKey key, string eventName) {
            //Test for Interior Interaction
            CallbackController.getPlayerInInterior(player, EventController.getMiloInteractionNames(), (p, b, m) => {
                if(b) EventController.triggerInteriorInteractionEvent(p, m);
            });

            var data = player.getCharacterData();
            var colShapes = player.getCurrentCollisionShapes().Where(c => c.Interactable && c.InteractableTypes.Contains(data.CharacterType)).ToList();
            if(colShapes.Count != 0) {
                colShapes.Reverse();
                foreach(var colShape in colShapes.OrderBy(c => c.Size)) {
                    if (colShape.Interactable && !player.getPlayerInteracting() && (!player.getBusy() || colShape.InteractableOnBusy)) {
                        Logger.logDebug(LogCategory.Player, LogActionType.Event, player, $"onInteraction: player ColShape interaction. id: {colShape.Id}, position: {colShape.Position.ToJson()}, event: {colShape.EventName}");

                        player.setPlayerInteracting(true);
                        if (colShape.Animation != null) {
                            AnimationController.animationTask(player, colShape.Animation, () => executeInteraction(player, colShape));
                        } else {
                            if(executeInteraction(player, colShape)) {
                                break;
                            }
                        }
                    }
                }
            } else {
                CallbackController.getObjectsInFront(player, (p, objects) => {
                    if(objects.Count == 0) {
                        return;
                    }
                    
                    var mostOften = objects.GroupBy(o => o.modelHash).MaxBy(g => g.Count()).FirstOrDefault();
                    
                    if(mostOften != null) {
                        onObjectInteraction(p, mostOften.modelHash.ToString(), mostOften.position, mostOften.offset, mostOften.heading, new DegreeRotation(0, 0, 0), mostOften.isBroken, false);
                    }
                });
            }

            if(colShapes.Count() > 0) {
                return true;
            }

            foreach(var conditionalKeyInteraction in AllKeyInteractionCallbacks) {
                if(conditionalKeyInteraction.check(player)) {
                    conditionalKeyInteraction.Callback?.Invoke(player);
                }
            }

            //Fix or deactivate
            //player.emitClientEvent("CHECK_INTERACTION_OBJECT");

            return true;
        }

        private bool onObjectInteraction(IPlayer player, ConsoleKey key, string eventName) {
            player.emitClientEvent("TOGGLE_OBJECT_INTERACTION");

            return true;
        }

        private bool executeInteraction(IPlayer player, CollisionShape colShape) {
            player.setPlayerInteracting(false);
            return colShape.Interaction(player);
        }

        private void onObjectInteraction(IPlayer player, string modelHash, Position objectPosition, Position playerOffset, float objectHeading, DegreeRotation rotation, bool isBroken, bool isNotDirectInteraction) {
            if(player.hasData("ALL_OBJECT_INTERACTION_CALLBACK")) {
                var callback = (Action<string, Position>)player.getData("ALL_OBJECT_INTERACTION_CALLBACK");

                player.resetData("ALL_OBJECT_INTERACTION_CALLBACK");
                callback.Invoke(modelHash, objectPosition);
            } else {
                if(Vector3.Distance(objectPosition, player.Position) > Constants.MAX_OBJECT_INTERACT_RANGE) {
                    if(!isNotDirectInteraction) {
                        player.sendBlockNotification("Du bist außer Reichweite!", "Außer Reichweite!");
                    }
                    return;
                } else {
                    var menu = new Menu("Interaktionsmenü", "Was möchtest du tun?");

                    var skip = false;
                    if(AllConfigObjects.ContainsKey(modelHash)) {
                        if(InteractionObject.evaluateInteraction(player, AllConfigObjects[modelHash], objectPosition, objectHeading, isBroken, ref menu)) {
                            skip = true;
                        }
                    }

                    if(!skip && AllServerGeneratedObjects.ContainsKey(modelHash)) {
                        InteractionObject.evaluateGeneratedInteraction(player, AllServerGeneratedObjects[modelHash], modelHash, objectPosition, objectHeading, isBroken, ref menu);
                    }

                    if(menu.getMenuItemCount() > 0) {
                        if(menu.getMenuItemCount() == 1 && menu.getMenuItemByIndex(0) is ClickMenuItem) {
                            MenuController.fakeClickMenuEvent(player, menu.getMenuItemByIndex(0) as ClickMenuItem);
                        } else {
                            player.showMenu(menu);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Gives the opportunity for the player to select any object, not just the one that can already be interacted with
        /// </summary>
        /// <param name="player"></param>
        /// <param name="callback">string is the hash of the object interacted with. Position is the position of the object</param>
        public static void activateAllObjectInteractionMode(IPlayer player, Action<string, Position> callback) {
            player.setData("ALL_OBJECT_INTERACTION_CALLBACK", callback);
            player.emitClientEvent("REGISTER_OBJECT_MODE", true);
        }


        #region Support

        private Menu objMenuGenerator(IPlayer player) {
            var menu = new Menu("Config-Objects Menü", "Was möchtest du tun?");

            menu.addMenuItem(new ClickMenuItem("Object registrieren/löschen", "Wähle ein Objekt aus und registriere es damit", "", "SUPPORT_ENTER_OBJECT_REGISTER_MODE", MenuItemStyle.green));

            var createMenu = new Menu("Object erstellen", "Erstelle ein neues Object");
            createMenu.addMenuItem(new InputMenuItem("ModelName (gewünscht)", "Gib den ModelNamen an. (Es wird theoretisch nur Name oder Hash benötigt!", "", ""));
            createMenu.addMenuItem(new InputMenuItem("ModelHash (optional)", "Gib den ModelHash an. (Es wird theoretisch nur Name oder Hash benötigt!", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Funktion/Identifikator", "Gibt die Funktion des Objektes im Code an", "z.B. OPEN_TRASHCAN", ""));
            createMenu.addMenuItem(new InputMenuItem("Info", "Beschreibe das Objekt kurz", "z.B. Mülleimer", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Object erstellen", "Erstelle das angegebene Object", "SUPPORT_CREATE_CONFIG_OBJECT", MenuItemStyle.green).needsConfirmation("Wirklich erstellen?", "Object wirklich erstellen?"));
            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            foreach(var obj in AllConfigObjects.Values.OrderBy(o => o.codeFunctionOrIdentifier).ToList()) {
                var subMenu = new Menu(obj.modelName != null ? obj.modelName : obj.modelHash, "Was möchtest du tun?");
                subMenu.addMenuItem(new StaticMenuItem("Funktion/Identifikator", $"Die Funktion des Items ist {obj.codeFunctionOrIdentifier}", $"{obj.codeFunctionOrIdentifier}"));
                subMenu.addMenuItem(new StaticMenuItem("Info", $"Die Info des Items ist {obj.info}", $"{obj.info}"));
                subMenu.addMenuItem(new ClickMenuItem("Object löschen", "Löscht das aktuelle Object", "", "SUPPORT_DELETE_CONFIG_OBJECT", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Id", obj.id } }).needsConfirmation("Wirklich löschen?", "Object wirklich löschen?"));
                menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
            }

            return menu;
        }

        private bool onSupportEnterObjectRegisterMode(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            SupportController.setCurrentSupportFastAction(player, () => onSupportEnterObjectRegisterMode(player, null, 0, null, null));

            player.sendNotification(Constants.NotifactionTypes.Info, "Objekt-Editor-Modus aktiviert. Wähle ein Objekt aus", "");

            activateAllObjectInteractionMode(player, (hash, position) => {
                if(AllConfigObjects.ContainsKey(hash)) {
                    var obj = AllConfigObjects[hash];

                    var menu = new Menu("Bereits registriert", "Was möchtest du tun?");
                    menu.addMenuItem(new StaticMenuItem("Funktion/Identifikator", obj.codeFunctionOrIdentifier, ""));
                    menu.addMenuItem(new StaticMenuItem("Info", obj.info, ""));
                    menu.addMenuItem(new ClickMenuItem("Object löschen", "Löscht das aktuelle Object", "", "SUPPORT_DELETE_CONFIG_OBJECT", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Id", obj.id } }).needsConfirmation("Wirklich löschen?", "Object wirklich löschen?"));
                    player.showMenu(menu);
                } else {
                    var createMenu = new Menu("Object erstellen", "Erstelle ein neues Object");
                    createMenu.addMenuItem(new InputMenuItem("Funktion/Identifikator", "Gibt die Funktion des Objektes im Code an", "z.B. OPEN_TRASHCAN", ""));
                    createMenu.addMenuItem(new InputMenuItem("Info", "Beschreibe das Objekt kurz", "z.B. Mülleimer", ""));
                    createMenu.addMenuItem(new MenuStatsMenuItem("Object erstellen", "Erstelle das angegebene Object", "SUPPORT_CREATE_CONFIG_OBJECT", MenuItemStyle.green).needsConfirmation("Wirklich erstellen?", "Object wirklich erstellen?").withData(new Dictionary<string, dynamic> { { "ModelHash", hash } }));

                    player.showMenu(createMenu);
                }

            });

            return true;
        }

        private bool onSupportCreateConfigObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            try {
                var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;
                string modelName = null;
                string modelHash = null;

                var function = "";
                var info = "";

                if(evt.elements.Length >= 4) {
                    var modelNameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                    var modelHashEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
                    var functionEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
                    var infoEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

                    modelHash = modelHashEvt.input;
                    modelHash = modelHashEvt.input;

                    function = functionEvt.input;
                    info = infoEvt.input;
                } else {
                    modelHash = (string)data["ModelHash"];
                    var functionEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                    var infoEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

                    function = functionEvt.input;
                    info = infoEvt.input;
                }

                if(AllConfigObjects.ContainsKey(modelHash)) {
                    player.sendBlockNotification("Das Objekt gibt es schon!", "");
                    return true;
                }


                using(var db = new ChoiceVDb()) {
                    var newObj = new configobject {
                        modelName = modelName,
                        modelHash = modelHash,
                        codeFunctionOrIdentifier = function,
                        info = info,
                    };

                    db.configobjects.Add(newObj);
                    db.SaveChanges();
                    loadConfigObjects(false);
                    addInteractableObjects(new List<string> { modelName != null ? ChoiceVAPI.Hash(modelName).ToString() : modelHash });
                    foreach(var p in ChoiceVAPI.GetAllPlayers()) {
                        p.emitClientEvent("SET_INTERACTABLE_OBJECTS", InteractableObjectList);
                    }
                }
                player.sendNotification(Constants.NotifactionTypes.Success, "Das Object wurde erfolgreich erstellt!", "");
            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe was im falschen Format!", "");
            }

            return true;
        }

        private bool onSupportDeleteConfigObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var id = data["Id"];
            using(var db = new ChoiceVDb()) {
                var obj = db.configobjects.Find(id);
                if(obj != null) {
                    db.configobjects.Remove(obj);
                    db.SaveChanges();
                    player.sendNotification(Constants.NotifactionTypes.Warning, "Das Object wurde erfolgreich gelöscht!", "");
                    loadConfigObjects(false);
                } else {
                    player.sendBlockNotification("Etwas ist schief gelaufen!", "");
                }
            }

            return true;
        }

        #endregion
    }

    public class InteractionObject {
        public static Dictionary<Position, DateTime> ParkingClockResetTimes = new Dictionary<Position, DateTime>();

        public static void loadInteractionObject() {
            EventController.addMenuEvent("BREAK_PARKING_CLOCK", breakParkingClock);

        }

        public bool evaluateInteraction(IPlayer player, configobject configObject, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            if(InteractionController.AllObjectInteractionCallbacks.ContainsKey(configObject.codeFunctionOrIdentifier)) {
                var callback = InteractionController.AllObjectInteractionCallbacks[configObject.codeFunctionOrIdentifier];
                if(callback.AllowOnBusy || !player.getBusy()) {
                    callback.Callback.Invoke(player, configObject.modelName, configObject.info, objectPosition, objectHeading, isBroken, ref menu);
                    return true;
                } else {
                    player.sendBlockNotification("Du bist gerade beschäftigt!", "Beschäftigt");
                }
            } else {
                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(configObject.codeFunctionOrIdentifier);
                if(theMethod != null) {
                    theMethod.Invoke(this, new object[] { player, configObject.modelHash, configObject.info, objectPosition, objectHeading });
                } else {
                    Logger.logError($"Object Interaction Method could not be found! {configObject.codeFunctionOrIdentifier}",
                        $"Fehler beim Objekt interagieren: Die angegebene Methode konnte nicht gefunden werden. Es besteht keine Interaktionsanbindung für das Objekt trotz angebunden: {configObject.codeFunctionOrIdentifier}", player);
                }
            }

            return false;
        }

        public bool evaluateGeneratedInteraction(IPlayer player, List<string> identifiers, string modelHash, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            foreach(var identifier in identifiers) {
                if(InteractionController.AllObjectInteractionCallbacks.ContainsKey(identifier)) {
                    var callback = InteractionController.AllObjectInteractionCallbacks[identifier];
                    if(callback.AllowOnBusy || !player.getBusy()) {
                        callback.Callback.Invoke(player, modelHash, "GENERATED", objectPosition, objectHeading, isBroken, ref menu);
                        return true;
                    } else {
                        player.sendBlockNotification("Du bist gerade beschäftigt!", "Beschäftigt");
                    }
                }
            }
            return false;
        }


        public static void onPlantInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading) {
            ChoiceVAPI.SendChatMessageToPlayer(player, "PLANT!");
        }

        public static void onATMInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading) {
            try {
                //ATMController.onATMInteraction(player, objectPosition, int.Parse(info));
            } catch(Exception e) {
                Logger.logException(e);
            }
        }



        public static void onParkClockInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading) {
            if(!(player.getInventory().getItem<ToolItem>(i => i.Flag == SpecialToolFlag.Crowbar) != null)) {
                player.sendNotification(Constants.NotifactionTypes.Danger, "Dir fehlt das Werkzeug um die Parkuhr aufzubrechen!", "Werkzeug fehlt!", Constants.NotifactionImages.Thief);
                return;
            }

            if(ParkingClockResetTimes.ContainsKey(objectPosition.Round())) {
                if(ParkingClockResetTimes[objectPosition.Round()] + TimeSpan.FromHours(3) >= DateTime.Now) {
                    player.sendNotification(Constants.NotifactionTypes.Danger, "Die Parkuhr sieht kaputt aus. Jemand hat sie kürzlich aufgebrochen.", "Parkuhr kaputt!", Constants.NotifactionImages.Thief);
                    return;
                }
            }

            var menu = MenuController.getConfirmationMenu("Parkuhr aufbrechen", "Parkuhr wirklich aufbrechen?", "BREAK_PARKING_CLOCK", new Dictionary<string, dynamic> { { "ObjectPosition", objectPosition } });
            player.showMenu(menu);

        }

        private static bool breakParkingClock(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var objectPosition = (Position)data["ObjectPosition"];


            player.getRotationTowardsPosition(objectPosition, true);
            var anim = AnimationController.getAnimationByName("SMASH_CROWBAR");
            anim.Duration = TimeSpan.FromSeconds(7.5f);
            AnimationController.animationTask(player, anim, () => {
                var rand = new Random();
                var amount = rand.Next(0, 6);

                player.getCharacterData().Cash += amount;
                player.sendNotification(Constants.NotifactionTypes.Info, "Die aufgebrochene Parkuhr enthielt $" + amount + " in Münzen", "$" + amount + " erhalten", Constants.NotifactionImages.Thief);

                ParkingClockResetTimes[objectPosition.Round()] = DateTime.Now;

                //7.5% Chance The Cops are alerted
                if(rand.NextDouble() <= 0.075) {
                    ControlCenterController.createDispatch(DispatchType.NpcCallDispatch, "Parkuhren aufgebrochen", "Eine Person wurde gesichtet wie sie Parkuhren aufbricht", player.Position);
                }
                CrimeNetworkController.OnPlayerCrimeActionDelegate.Invoke(player, CrimeAction.ParkingClockBreaking, 1, null);

            }, null, true);
            return true;
        }
    }

}
