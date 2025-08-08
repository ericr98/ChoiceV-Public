using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Vehicles;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChoiceVServer.EventSystem {
    public delegate bool EventControllerDelegate(IPlayer player, string eventName, object[] args);
    public delegate bool EventControllerKeyDelegate(IPlayer player, ConsoleKey key, string eventName);
    public delegate bool EventControllerKeyToggleDelegate(IPlayer player, ConsoleKey key, bool isPressed, string eventName);
    public delegate bool EventControllerInteractionDelegate(IPlayer player, IEntity target, uint model, BaseObjectType BaseObjectType, Position position, Rotation rotation);
    public delegate bool EventControllerInteriorInteractionDelegate(IPlayer player, string milo);
    public delegate bool MenuEventDelegate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent);
    public delegate bool CheckpointEventDelegate(IEntity entity, ICheckpoint checkpoint, bool state);
    public delegate bool CollisionShapeEventDelegate(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data);

    public delegate void TickDelegate();
    public delegate void LongTickDelegate();

    public delegate bool KeyPressOverrideDelegate(IPlayer player, ConsoleKey key);
    

    #region ChoiceV Custom Delegates

    public delegate void MainReadyDelegate();
    public delegate void MainShutdownDelegate();

    public delegate void PlayerSuccessfullConnectionDelegate(IPlayer player, character character);
    public delegate void PlayerPreSuccessfullConnectionDelegate(IPlayer player, character character);
    public delegate void PlayerPastSuccessfullConnectionDelegate(IPlayer player, character character);

    public delegate void PlayerDisconnectSlowDelegate(IPlayer player);

    public delegate void PlayerMovedDelegate(object sender, IPlayer player, Position moveToPosition, float distance);
    public delegate void PlayerChangeWorldGridDelegate(object sender, IPlayer player, WorldGrid previousGrid, WorldGrid currentGrid);
    public delegate void VehicleMovedDelegate(object sender, ChoiceVVehicle vehicle, Position moveFromPos, Position moveToPosition, float distance);

    public delegate void CollisionShapeDelegate(CollisionShape shape, IEntity entity);
    public delegate bool CollisionShapeInteractionDelegate(IPlayer player);

    public delegate void ObjectInteractionDelegate(IPlayer player, string modelhash, Position objectPosition, Position playerOffset, float objectHeading, DegreeRotation rotation, bool isBroken, bool notDirectInteract);
    public delegate void PedInteractionDelegate(IPlayer player, string modelhash, Position position);

    public delegate void CefEventDelegate(IPlayer player, PlayerWebSocketConnectionDataElement evt);

    public delegate void PlayerChangeDimensionDelegate(IPlayer player, int oldDimension, int newDimension);

    public delegate void VehicleHealthChangeDelegate(ChoiceVVehicle vehicle, uint oldHealth, uint newHealth);

    public delegate void OnCefCloseDelegate(IPlayer player);
    
    #endregion

    #region API Delegates
    public delegate void PlayerConnectedDelegate(IPlayer player, string reason);
    public delegate void PlayerDisconnectedDelegate(IPlayer player, string reason);
    public delegate void PlayerEnterVehicleDelegate(IPlayer player, ChoiceVVehicle vehicle, byte seatId);
    public delegate void PlayerChangeVehicleSeatDelegate(IPlayer player, ChoiceVVehicle vehicle, byte oldSeatId, byte newSeatId);
    public delegate void PlayerExitVehicleDelegate(IPlayer player, ChoiceVVehicle vehicle, byte seatId);
    public delegate void PlayerDeadDelegate(IPlayer player, IEntity killer, uint weapon);
    public delegate void PlayerDamageDelegate(IPlayer player, IEntity killer, uint weapon, ushort damage);
    public delegate void ConsoleCommandDelegate(string text, string[] args);

    public delegate void CheckpointDelegate(ICheckpoint checkpoint, IEntity entity, bool state);
    public delegate WeaponDamageResponse WeaponDamageDelegate(IPlayer player, IEntity target, uint weapon, ushort damage, Position shotOffset, BodyPart bodyPart);
    public delegate bool WeaponChangeDelegate(IPlayer player, uint oldWeapon, uint newWeapon);

    public delegate string PlayerBeforeConnectDelegate(PlayerConnectionInfo connectionInfo, string reason);
    public delegate bool StartProjectileDelegate(IPlayer player, Position startPosition, Position direction, uint ammoHash, uint weaponHash);

    public delegate void NetworkOwnerChangeDelegate(IEntity entity, IPlayer oldOwner, IPlayer newOwner);

    public delegate void VehicleDamageDelegate(IVehicle target, IEntity attacker, uint bodyHealthDamage, uint additionalBodyHealthDamage, uint engineHealthDamage, uint petrolTankDamage, uint weaponHash);
    public delegate void VehicleDestroyDelegate(IVehicle vehicle);

    public delegate bool ExplosionDelegate(IPlayer player, AltV.Net.Data.ExplosionType explosionType, Position position, uint explosionFx, IEntity targetEntity);

    public delegate void PlayerWeaponChangeDelegate(IPlayer player, uint oldWeapons, uint newWeapon);

    #endregion

    class EventController : ChoiceVScript {
        /// <summary>
        /// Is triggered every 50 milliseconds
        /// </summary>
        public static TickDelegate TickDelegate;

        /// <summary>
        /// Is triggered every 200 milliseconds
        /// </summary>
        public static LongTickDelegate LongTickDelegate;

        #region API Delegate Variables
        public static PlayerConnectedDelegate PlayerConnectedDelegate;
        public static PlayerDisconnectedDelegate PlayerDisconnectedDelegate;
        public static PlayerEnterVehicleDelegate PlayerEnterVehicleDelegate;
        public static PlayerChangeVehicleSeatDelegate PlayerChangeVehicleSeatDelegate;
        public static PlayerExitVehicleDelegate PlayerExitVehicleDelegate;
        public static PlayerDeadDelegate PlayerDeadDelegate;
        public static PlayerDamageDelegate PlayerDamageDelegate;
        public static ConsoleCommandDelegate ConsoleCommandDelegate;

        public static CheckpointDelegate CheckpointDelegate;

        public static WeaponDamageDelegate WeaponDamageDelegate;
        public static WeaponChangeDelegate WeaponChangeDelegate;

        public static StartProjectileDelegate StartProjectileDelegate;

        public static NetworkOwnerChangeDelegate NetworkOwnerChangeDelegate;

        public static VehicleDamageDelegate VehicleDamageDelegate;
        public static VehicleDestroyDelegate VehicleDestroyDelegate;

        public static ExplosionDelegate ExplosionDelegate;

        public static PlayerWeaponChangeDelegate PlayerWeaponChangeDelegate;

        #endregion

        #region ChoiceV Custom Delegate Variables

        public static MainReadyDelegate MainReadyDelegate;
        public static MainReadyDelegate MainAfterReadyDelegate;
        public static MainShutdownDelegate MainShutdownDelegate;

        public static PlayerSuccessfullConnectionDelegate PlayerSuccessfullConnectionDelegate;

        /// <summary>
        /// Is triggered before Successfull Conection! No CharacterData set yet
        /// </summary>
        public static PlayerPreSuccessfullConnectionDelegate PlayerPreSuccessfullConnectionDelegate;

        /// <summary>
        /// Is triggered after Successfull Conection! No CharacterData set yet
        /// </summary>
        public static PlayerPastSuccessfullConnectionDelegate PlayerPastSuccessfullConnectionDelegate;

        public static PlayerDisconnectSlowDelegate PlayerDisconnectSlowDelegate;

        //public static PlayerMovedDelegate PlayerMovedDelegate;
        /// <summary>
        /// WARNING! Try to use addVehicleMoveCallback for faster Code!
        /// Only make minimal Calculation in this Delegate Callback
        /// </summary>
        public static VehicleMovedDelegate VehicleMovedDelegate;

        public static ObjectInteractionDelegate ObjectInteractionDelegate;
        public static PedInteractionDelegate PedInteractionDelegate;

        public static PlayerChangeWorldGridDelegate PlayerChangeWorldGridDelegate;

        public static KeyPressOverrideDelegate KeyPressOverrideDelegate;

        public static PlayerChangeDimensionDelegate PlayerChangeDimensionDelegate;

        public static VehicleHealthChangeDelegate VehicleHealthChangeDelegate;

        public static OnCefCloseDelegate OnCefCloseDelegate;
        
        #endregion

        private static readonly ConcurrentDictionary<string, List<EventControllerCallback>> RegisteredEvents = new ConcurrentDictionary<string, List<EventControllerCallback>>();
        public static readonly ConcurrentDictionary<ConsoleKey, List<PlayerKeyEventCallback>> RegisteredKeyEvents = new ConcurrentDictionary<ConsoleKey, List<PlayerKeyEventCallback>>();
        private static readonly ConcurrentDictionary<string, PlayerKeyEventCallback> RegisterdKeysByIdentifer = new();

        public static readonly ConcurrentDictionary<ConsoleKey, List<PlayerKeyToggleEventCallback>> RegisteredKeyToggleEvents = new ConcurrentDictionary<ConsoleKey, List<PlayerKeyToggleEventCallback>>();
        private static readonly ConcurrentDictionary<string, PlayerKeyToggleEventCallback> RegisterdKeyTogglesByIdentifer = new();

        private static readonly ConcurrentDictionary<BaseObjectType, List<EventControllerInteractionDelegate>> RegisteredInteractionEvents = new ConcurrentDictionary<BaseObjectType, List<EventControllerInteractionDelegate>>();
        private static readonly ConcurrentDictionary<string, List<EventControllerInteriorInteractionDelegate>> RegisteredInteriorInteractionEvents = new ConcurrentDictionary<string, List<EventControllerInteriorInteractionDelegate>>();

        private static readonly ConcurrentDictionary<string, List<MenuEventDelegate>> RegisteredMenuEvents = new ConcurrentDictionary<string, List<MenuEventDelegate>>();
        private static readonly ConcurrentDictionary<IntPtr, List<CheckpointEventDelegate>> RegisteredCheckpointEvents = new ConcurrentDictionary<IntPtr, List<CheckpointEventDelegate>>();

        private static readonly ConcurrentDictionary<string, List<CollisionShapeEventDelegate>> RegisteredCollisionShapeEvents = new ConcurrentDictionary<string, List<CollisionShapeEventDelegate>>();

        private static readonly ConcurrentDictionary<string, List<CefEventDelegate>> RegisteredCefEvents = new ConcurrentDictionary<string, List<CefEventDelegate>>();

        //Uses WorldGrid Id
        private static readonly ConcurrentDictionary<int, List<PlayerMovedDelegate>> RegisteredPlayerMoveEvents = new ConcurrentDictionary<int, List<PlayerMovedDelegate>>();
        private static readonly ConcurrentDictionary<int, List<VehicleMovedDelegate>> RegisteredVehicleMoveEvents = new ConcurrentDictionary<int, List<VehicleMovedDelegate>>();


        public static ConcurrentDictionary<IntPtr, object> PlayerLocks = new ConcurrentDictionary<IntPtr, object>();

        public EventController() {
            if(Constants.TRY_THREAD_BASED) {
                Alt.OnPlayerEvent += onPlayerEventTriggerEvaluate;
            } else {
                Alt.OnPlayerEvent += onPlayerEventTrigger;
            }

            Alt.OnPlayerConnect += onPlayerConnected;
            Alt.OnPlayerDisconnect += onPlayerDisonnected;
            Alt.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Alt.OnPlayerChangeVehicleSeat += OnPlayerChangedSeat;
            Alt.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;
            Alt.OnConsoleCommand += OnConsoleCommand;
            Alt.OnPlayerDead += OnPlayerDead;
            Alt.OnPlayerDamage += OnPlayerDamage;

            Alt.OnStartProjectile += onStartProjectile;

            Alt.OnNetworkOwnerChange += onNetworkOwnerChange;

            //Alt.OnCheckpoint += OnCheckpoint;
            Alt.OnWeaponDamage += OnWeaponDamage;
            
            Alt.OnVehicleDamage += onVehicleDamage;
            Alt.OnVehicleDestroy += onVehicleDestroy;
            //Alt.OnPlayerWeaponChange += OnWeaponChange;
            Alt.OnExplosion += onExplosion;

            Alt.OnPlayerWeaponChange += onPlayerWeaponChange;

            Alt.OnClientRequestObject += onRequestObject; //For Parachute
            Alt.OnClientDeleteObject += onRequestDelete; //For Parachute
            //Alt.OnRequestSyncScene += OnRequestSyncScene; //For Scenarios (or not have to debug) Scenarios arent synced
        }

        private bool onRequestObject(IPlayer target, uint model, Position position) {
            return true;
        }

        private bool onRequestDelete(IPlayer target) {
            return true;
        }

        //For MultiThread
        private void onPlayerEventTriggerEvaluate(IPlayer player, string eventName, object[] args) {
            try {
                if(!PlayerLocks.ContainsKey(player.NativePointer)) {
                    ChoiceVAPI.KickPlayer(player, "Partytime", "Es ist ein unbekannter Fehler aufgetreten! Melde dich unverzüglich im Support!", "Es wurde ein Event geschickt, obwohl der Spieler nicht auf dem Server ist! Hackingverdacht!");
                    return;
                }

                var playerLock = PlayerLocks[player.NativePointer];
                lock(playerLock) {
                    var thread = new Thread(() => {
                        onPlayerEventTrigger(player, eventName, args);
                    });

                    thread.Start();
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }   

        private static void onPlayerEventTrigger(IPlayer player, string eventName, object[] args) {
            try {
                if(player.getCharacterFullyLoaded()) {
                    //KeyToggle Event
                    if(eventName.StartsWith("KEY_TOGGLE_")) {
                        var keyString = eventName.Remove(0, 11);
                        var keyInt = int.Parse(keyString);
                        var key = (ConsoleKey)keyInt;
                        var mappingByIdentifier = player.getCharacterData().ChangeMappingsByIdentifier;

                        var callList = new List<PlayerKeyToggleEventCallback>();
                        if(RegisteredKeyToggleEvents.ContainsKey(key)) {
                            callList = callList.Concat(RegisteredKeyToggleEvents[key].Where(e => !mappingByIdentifier.ContainsKey(e.Identifier))).ToList();
                        }

                        var mapping = player.getCharacterData().ChangeKeyMappings;
                        if(mapping.ContainsKey(key)) {
                            var identifierList = mapping[key];
                            foreach(var identifier in identifierList) {
                                if(RegisterdKeyTogglesByIdentifer.ContainsKey(identifier)) {
                                    callList.Add(RegisterdKeyTogglesByIdentifer[identifier]);
                                }
                            }
                        }

                        var isPressed = bool.Parse(args[0].ToString());

                        if(callList.Count == 0) {
                            return;
                        }

                        foreach(var callback in callList) {
                            if(callback.IgnoresBusy || !player.getBusy()) {
                                callback.Callback?.Invoke(player, key, isPressed, eventName);
                            }
                        }

                        Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"KeyToggleEvent: {eventName} triggered by {player.Name} for key {key}");
                        return;
                    }

                    //Key Event
                    if(eventName.StartsWith("KEY_")) {
                        var keyString = eventName.Remove(0, 4);
                        var keyInt = int.Parse(keyString);
                        var key = (ConsoleKey)keyInt;
                        var mappingByIdentifier = player.getCharacterData().ChangeMappingsByIdentifier;

                        if(KeyPressOverrideDelegate != null) {
                            foreach(KeyPressOverrideDelegate f in KeyPressOverrideDelegate.GetInvocationList()) {
                                if(f(player, key)) {
                                    return;
                                }
                            }
                        }

                        var callList = new List<PlayerKeyEventCallback>();
                        if (RegisteredKeyEvents.ContainsKey(key)) {
                            callList = callList.Concat(RegisteredKeyEvents[key].Where(e => !mappingByIdentifier.ContainsKey(e.Identifier))).ToList();
                        }

                        var mapping = player.getCharacterData().ChangeKeyMappings;
                        if (mapping.ContainsKey(key)) {
                            var identifierList = mapping[key];
                            foreach (var identifier in identifierList) {
                                if(RegisterdKeysByIdentifer.ContainsKey(identifier)) {
                                    callList.Add(RegisterdKeysByIdentifer[identifier]);
                                }
                            }
                        }

                        if (callList.Count == 0) {
                            return;
                        }

                        foreach (var callback in callList) {
                            if (callback.IgnoresBusy || !player.getBusy()) {
                                if(callback.Callback.Invoke(player, key, eventName)) {
                                    break;
                                }
                            }
                        }
                        Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"KeyEvent: {eventName} triggered by {player.Name} for key {key}");
                        return;
                    }

                    //Interaction event
                    if(eventName == "ENTITY.INTERACTION") {
                        //TODO MAY NOT WORK!
                        var entity = (IEntity)args[0];
                        uint model = 0;
                        var baseObjectType = entity.Type;
                        var position = entity.Position;
                        var rotation = entity.Rotation;

                        if((baseObjectType == BaseObjectType.Player && position.Distance(player.Position) > Constants.MAX_ENTITY_INTERACTION_RANGE)
                              || ((entity is ChoiceVVehicle veh) && position.Distance(player.Position) > veh.getSize() + Constants.MAX_ENTITY_INTERACTION_RANGE)) {
                            player.sendBlockNotification("Du bist zu weit entfernt!", "Zu weit weg!");
                            return;
                        }

                        var toCallInteractionEvents = RegisteredInteractionEvents.FirstOrDefault(ev => ev.Key.Equals(baseObjectType)).Value;

                        if(toCallInteractionEvents == null) {
                            Logger.logError($"InteractionEvent: {eventName} triggered by {player.Name} not found! This BaseObjectType seems to be not registered",
                                $"Fehler im Interaktionssysten: Interaktionsevent mit Namen nicht {eventName} gefunden.", player);
                            return;
                        }

                        foreach(var callback in toCallInteractionEvents) {
                            callback.Invoke(player, entity, model, baseObjectType, position, rotation);
                        }

                        Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"InteractionEvent: {eventName}");
                        return;
                    }

                    if(eventName == "PED.INTERACTION") {
                        var modelHash = args[0].ToString();
                        var position = ((string)args[1]).FromJson<Position>();

                        PedInteractionDelegate?.Invoke(player, modelHash, position);
                        return;
                    }

                    //Object Interaction Event
                    if(eventName == "OBJECT.INTERACTION") {
                        var model = args[0].ToString();
                        var pos = args[1].ToString().FromJson<Position>();
                        var offset = args[2].ToString().FromJson<Position>();
                        var heading = float.Parse(args[3].ToString() ?? string.Empty);
                        var rotationPos = args[4].ToString().FromJson<Position>();
                        var rotation = new DegreeRotation(rotationPos.X, rotationPos.Y, rotationPos.Z);
                        var isBroken = bool.Parse(args[5].ToString() ?? string.Empty);
                        var notDirectInteract = bool.Parse(args[6].ToString() ?? string.Empty);

                        if(ObjectInteractionDelegate != null) {
                            ObjectInteractionDelegate?.Invoke(player, model, pos, offset, heading, rotation, isBroken, notDirectInteract);
                        }

                        Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"InteractionEvent: {eventName}. With Data: {args.ToJson()}");
                        return;
                    }

                }

                //Standart EventControllerDelegate Event
                var toCallEvents = RegisteredEvents.FirstOrDefault(p => p.Key.Equals(eventName)).Value;
                if(toCallEvents == null) {
                    Logger.logError($"Event: {eventName} triggered by {player.Name} not found!",
                                $"Fehler im Eventsystem: Event mit Namen nicht {eventName} gefunden.", player);
                    return;
                }

                if(eventName != Constants.OnPlayerSubmitLogin || eventName != Constants.OnPlayerSubmitRegistration) {
                    var argsToString = "";
                    foreach(var item in args) {
                        if(item != null)
                            argsToString = argsToString + " " + item.ToString();
                    }

                    Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"Event: {eventName}. With Data: {argsToString}");
                }

                foreach(var callback in toCallEvents) {
                    callback.invokeDelegate(player, eventName, args);
                }

            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        /// <summary>
        /// DO NOT USE! ONLY FOR MenuController
        /// </summary>
        public static void triggerMenuEvent(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var toCallEvents = RegisteredMenuEvents.FirstOrDefault(p => p.Key.Equals(itemEvent)).Value;

            if(toCallEvents == null) {
                Logger.logError($"MenuEventTrigger: {itemEvent} triggered by {player.Name} not found!",
                    $"Fehler im Menüsystem: Menüevent mit Namen {itemEvent} nicht gefunden.", player);
                return;
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"MenuEventTrigger: {itemEvent}");

            if(data.ContainsKey("PreviousCefEvent")) {
                menuItemCefEvent = data["PreviousCefEvent"];
            }

            foreach(var callback in toCallEvents) {
                callback.Invoke(player, itemEvent, menuItemId, data, menuItemCefEvent);
            }
        }

        /// <summary>
        /// DO NOT USE! ONLY FOR CheckpointController
        /// </summary>
        public static void triggerCheckpointEvent(IEntity entity, ICheckpoint checkpoint, bool state) {
            var toCallEvents = RegisteredCheckpointEvents.FirstOrDefault(p => p.Key.Equals(checkpoint.NativePointer)).Value;

            if(toCallEvents == null) {
                //Logger.logError($"CheckpointEventTriggerd: {checkpoint.NativePointer} at {checkpoint.Position.ToString()} triggered by {entity.Model} not found!");
                return;
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Event, $"CheckpointEventTriggered: {checkpoint.NativePointer} at {checkpoint.Position.ToString()} triggered for {entity.Model}.");

            foreach(var callback in toCallEvents) {
                callback.Invoke(entity, checkpoint, state);
            }
        }

        /// <summary>
        /// DO NOT USE! ONLY FOR CheckpointController
        /// </summary>
        public static void triggerInteriorInteractionEvent(IPlayer player, string miloName) {
            var toCallEvents = RegisteredInteriorInteractionEvents.FirstOrDefault(p => p.Key.Equals(miloName)).Value;

            if(toCallEvents == null) {
                //Logger.logError($"CheckpointEventTriggerd: {checkpoint.NativePointer} at {checkpoint.Position.ToString()} triggered by {entity.Model} not found!");
                return;
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"InteriorInteractionEven: {miloName} at {player.Position}");

            foreach(var callback in toCallEvents) {
                callback.Invoke(player, miloName);
            }
        }

        /// <summary>
        /// DO NOT USE! ONLY FOR CheckpointController
        /// </summary>
        public static void triggerCollisionShapeEvent(IPlayer player, CollisionShape colShape, Dictionary<string, dynamic> data) {
            var toCallEvents = RegisteredCollisionShapeEvents.FirstOrDefault(p => p.Key == colShape.EventName).Value;

            if(toCallEvents == null) {
                Logger.logError($"CollisionShapeEvent: {colShape.EventName} at {colShape.Position} triggered by {player.getCharacterName()} not found!",
                                    $"Fehler im Eventsystem: Ein Collision Shape Event konnte nicht gefunden werden: {colShape.EventName}.", player);
                return;
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"CollisionShapeEvent triggered: {colShape} at {player.Position}");

            foreach(var callback in toCallEvents) {
                callback.Invoke(player, colShape, colShape.InteractData);
            }
        }

        /// <summary>
        /// DO NOT USE! ONLY FOR WebController
        /// </summary>
        public static void triggerCefEvent(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var toCallEvents = RegisteredCefEvents.FirstOrDefault(p => p.Key == evt.Event).Value;

            if(toCallEvents == null) {
                Logger.logError($"CefEvent: {evt.Event} triggered by {player.getCharacterName()} not found!",
                                    $"Fehler im Eventsystem: Ein Collision CEF Event konnte nicht gefunden werden: {evt.Event}.", player);
                return;
            }

            Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"CefEvent triggered: {evt.Event} at {player.Position}");

            foreach(var callback in toCallEvents) {
                callback.Invoke(player, evt);
            }
        }

        /// <summary>
        /// Method for adding events during runtime. The registered method will be called, when a IPlayer triggers an RemoteEvent with that eventName
        /// </summary>
        /// <param name="eventName">For identification. Determinates with callback to invoke</param>
        /// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        public static void addEvent(string eventName, EventControllerDelegate eventDelegate) {
            //Catch wrongly registered Events
            if(eventName.StartsWith("KEY_")) {
                throw new InvalidOperationException("Use addKeyEvent for Keyboard events!");
            } else if(eventName == "ENTITY.INTERACTION") {
                throw new InvalidOperationException("Use addInteractionEvent for Entity Interaction events!");
            }

            List<EventControllerCallback> eventList = null;
            if(RegisteredEvents.ContainsKey(eventName))
                eventList = RegisteredEvents[eventName];

            if(eventList == null)
                eventList = new List<EventControllerCallback>();

            eventList.Add(new EventControllerCallback(eventDelegate));
            RegisteredEvents[eventName] = eventList;
        }

        /// <summary>
        /// Method for adding a key events during runtime. The registered method will be called, when a IPlayer presses a certain Key and releases it
        /// </summary>
        /// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        /// <param name="showName">The name for the key mapping</param>
        /// <param name="key">The key to register</param>
        /// <param name="identifier">The identifier for the key mapping</param>
        public static void addKeyToggleEvent(string identifier, ConsoleKey key, string showName, EventControllerKeyToggleDelegate eventDelegate, bool ignoresBusy = false) {
            List<PlayerKeyToggleEventCallback> eventList = null;

            if(RegisteredKeyToggleEvents.ContainsKey(key))
                eventList = RegisteredKeyToggleEvents[key];

            if(eventList == null)
                eventList = new List<PlayerKeyToggleEventCallback>();

            var el = new PlayerKeyToggleEventCallback(identifier, showName, eventDelegate, ignoresBusy);
            eventList.Add(el);
            RegisterdKeyTogglesByIdentifer[identifier] = el;
            RegisteredKeyToggleEvents[key] = eventList;
        }

        /// <summary>
        /// Method for adding a key events during runtime. The registered method will be called, when a IPlayer presses a certain Key
        /// </summary>
        /// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        /// <param name="showName">The name for the key mapping</param>
        /// <param name="key">The key to register</param>
        /// <param name="identifier">The identifier for the key mapping</param>
        public static void addKeyEvent(string identifier, ConsoleKey key, string showName, EventControllerKeyDelegate eventDelegate, bool ignoresBusy = false, bool ignoresKeyLock = false) {
            List<PlayerKeyEventCallback> eventList = null;

            if(RegisteredKeyEvents.ContainsKey(key))
                eventList = RegisteredKeyEvents[key];

            if(eventList == null)
                eventList = new List<PlayerKeyEventCallback>();

            var el = new PlayerKeyEventCallback(identifier, showName, eventDelegate, ignoresBusy, ignoresKeyLock);
            eventList.Add(el);
            RegisteredKeyEvents[key] = eventList;
            RegisterdKeysByIdentifer[identifier] = el;
        }

        /// <summary>
        /// Method for adding an interaction event. The registered method will be called, when a specific BaseObjectType is selected by player.
        /// </summary>
        /// <param name="key">The BaseObjectType your Event should be registered to</param>
        /// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        public static void addInteractionEvent(BaseObjectType key, EventControllerInteractionDelegate eventDelegate) {
            List<EventControllerInteractionDelegate> eventList = null;

            if(RegisteredInteractionEvents.ContainsKey(key))
                eventList = RegisteredInteractionEvents[key];

            if(eventList == null)
                eventList = new List<EventControllerInteractionDelegate>();

            eventList.Add(eventDelegate);
            RegisteredInteractionEvents[key] = eventList;
        }

        /// <summary>
        /// Method for adding an interior interaction event. The registered method will be called, when a specific interior is selected by player.
        /// </summary>
        /// <param name="miloName">The name for the milo</param>
        /// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        public static void addInteriorInteractionEvent(string miloName, EventControllerInteriorInteractionDelegate eventDelegate) {
            List<EventControllerInteriorInteractionDelegate> eventList = null;

            if(RegisteredInteriorInteractionEvents.ContainsKey(miloName))
                eventList = RegisteredInteriorInteractionEvents[miloName];

            if(eventList == null)
                eventList = new List<EventControllerInteriorInteractionDelegate>();

            eventList.Add(eventDelegate);
            RegisteredInteriorInteractionEvents[miloName] = eventList;
        }

        /// <summary>
        /// Method for adding a MenuEvent triggered by a specific Item in a Menu
        /// </summary>
        /// <param name="eventName">For identification. Determinates with callback to invoke</param>
        /// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        public static void addMenuEvent(string eventName, MenuEventDelegate eventDelegate) {
            List<MenuEventDelegate> eventList = null;
            if(RegisteredMenuEvents.ContainsKey(eventName))
                eventList = RegisteredMenuEvents[eventName];

            if(eventList == null)
                eventList = new List<MenuEventDelegate>();

            eventList.Add(new MenuEventDelegate(eventDelegate));
            RegisteredMenuEvents[eventName] = eventList;
        }

        /// <summary>
        /// Method for adding a event to an event for a specific CollisionShape Interaction. It is triggered when a player Interacts with an CollisionShape;
        /// </summary>
        /// <param name="eventName">For identification. Determinates with callback to invoke</param>
        /// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        public static void addCollisionShapeEvent(string eventName, CollisionShapeEventDelegate eventDelegate) {
            List<CollisionShapeEventDelegate> eventList = null;

            if(RegisteredCollisionShapeEvents.ContainsKey(eventName))
                eventList = RegisteredCollisionShapeEvents[eventName];

            if(eventList == null)
                eventList = new List<CollisionShapeEventDelegate>();

            eventList.Add(new CollisionShapeEventDelegate(eventDelegate));

            RegisteredCollisionShapeEvents[eventName] = eventList;
        }

        /// <summary>
        /// Method for adding a event to an event for a specific Checkpoint. It is triggered when an Entity leaves or walks in a Checkpoint
        /// </summary>
        public static void addCefEvent(string name, CefEventDelegate callback) {
            List<CefEventDelegate> eventList = null;
            if(RegisteredCefEvents.ContainsKey(name))
                eventList = RegisteredCefEvents[name];

            if(eventList == null)
                eventList = new List<CefEventDelegate>();

            eventList.Add(callback);
            RegisteredCefEvents[name] = eventList;
        }

        /// <summary>
        /// Method for adding a event to an callBack called, when a player moves. Will always trigger
        /// </summary>
        public static void addOnPlayerMoveCallback(PlayerMovedDelegate callback) {
            addOnPlayerMoveCallback(null, callback);
        }

        /// <summary>
        /// Method for adding a event to an callBack called, when a player moves. Put null for grid to always trigger
        /// </summary>
        public static void addOnPlayerMoveCallback(WorldGrid grid, PlayerMovedDelegate callback) {
            var gridId = -1;

            if(grid != null) {
                gridId = grid.Id;
            }

            List<PlayerMovedDelegate> eventList = null;
            if(RegisteredPlayerMoveEvents.ContainsKey(gridId))
                eventList = RegisteredPlayerMoveEvents[gridId];

            if(eventList == null)
                eventList = new List<PlayerMovedDelegate>();

            eventList.Add(new PlayerMovedDelegate(callback));
            RegisteredPlayerMoveEvents[gridId] = eventList;
        }

        /// <summary>
        /// Method for adding an event to an callBack called, when a vehicle moves
        /// </summary>
        public static void addOnVehicleMoveCallback(WorldGrid grid, VehicleMovedDelegate callback) {
            if(grid == null) {
                return;
            }

            List<VehicleMovedDelegate> eventList = null;
            if(RegisteredVehicleMoveEvents.ContainsKey(grid.Id))
                eventList = RegisteredVehicleMoveEvents[grid.Id];

            if(eventList == null)
                eventList = new List<VehicleMovedDelegate>();

            eventList.Add(new VehicleMovedDelegate(callback));
            RegisteredVehicleMoveEvents[grid.Id] = eventList;
        }

        /// <summary>
        /// Method for removing a event to an callBack called, when a player moves
        /// </summary>
        public static void removeOnPlayerMoveCallback(WorldGrid grid, PlayerMovedDelegate callback) {
            if(RegisteredPlayerMoveEvents.ContainsKey(grid.Id)) {
                var eventList = RegisteredPlayerMoveEvents[grid.Id];
                if(eventList.Remove(callback)) {
                    return;
                }
            } else {
                Logger.logError($"removeOnPlayerMoveCallback: tried to remove onPlayerMoveCallback that didnt exist! grid: {grid.Id}, callback: {callback}",
                    $"Fehler im Eventsystem: Fahrzeugcallback wurde entfernt hat aber nicht existiert. Grid: {grid.Id}");
            }
        }

        /// <summary>
        /// Method for adding a event to an callBack called, when a vehicle moves
        /// </summary>
        public static void removeOnVehicleMoveCallback(WorldGrid grid, VehicleMovedDelegate callback) {
            if(RegisteredVehicleMoveEvents.ContainsKey(grid.Id)) {
                var eventList = RegisteredVehicleMoveEvents[grid.Id];
                if(eventList.Remove(callback)) {
                    return;
                }
            } else {
                Logger.logError($"removeOnVehicleMoveCallback: tried to remove removeOnVehicleMoveCallback that didnt exist! grid: {grid.Id}, callback: {callback}",
                    $"Fehler im Eventsystem: Fahrzeugcallback wurde entfernt hat aber nicht existiert. Grid: {grid.Id}");
            }
        }

        #region API "Event Extensions"

        //Player
        public static void onPlayerConnected(IPlayer player, string reason) {
            PlayerLocks.TryAdd(player.NativePointer, new object());

            try {
                PlayerConnectedDelegate?.Invoke(player, reason);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        public static void onPlayerDisonnected(IPlayer player, string reason) {
            object ignored;
            PlayerLocks.Remove(player.NativePointer, out ignored);

            try {
                PlayerDisconnectedDelegate?.Invoke(player, reason);
                PlayerDisconnectSlowDelegate?.Invoke(player);
            } catch(Exception e) {
                Logger.logException(e);
            }

            BaseObjectData.RemoveBaseObject(player.NativePointer);
        }

        public static void OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seatId) {
            try {
                PlayerEnterVehicleDelegate?.Invoke(player, (ChoiceVVehicle)vehicle, seatId);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }
        public static void OnPlayerChangedSeat(IVehicle vehicle, IPlayer player, byte oldSeatId, byte newSeatId) {
            try {
                PlayerChangeVehicleSeatDelegate?.Invoke(player, (ChoiceVVehicle)vehicle, oldSeatId, newSeatId);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        public static void OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seatId) {
            try {
                PlayerExitVehicleDelegate?.Invoke(player, (ChoiceVVehicle)vehicle, seatId);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private void OnConsoleCommand(string name, string[] args) {
            try {
                ConsoleCommandDelegate?.Invoke(name, args);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private void OnPlayerDead(IPlayer player, IEntity killer, uint weapon) {
            try {
                PlayerDeadDelegate?.Invoke(player, killer, weapon);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private void OnPlayerDamage(IPlayer player, IEntity attacker, uint weapon, ushort damage, ushort armor) {
            try {
                PlayerDamageDelegate?.Invoke(player, attacker, weapon, damage);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private void OnCheckpoint(ICheckpoint checkpoint, IEntity entity, bool state) {
            try {
                CheckpointDelegate?.Invoke(checkpoint, entity, state);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private WeaponDamageResponse OnWeaponDamage(IPlayer player, IEntity target, uint weapon, ushort damage, Position shotOffset, BodyPart bodyPart) {
            try {
                return WeaponDamageDelegate?.Invoke(player, target, weapon, damage, shotOffset, bodyPart) ?? true;
            } catch(Exception e) {
                Logger.logException(e);
                return true;
            }
        }

        private bool OnWeaponChange(IPlayer player, uint oldWeapon, uint newWeapon) {
            try {
                return WeaponChangeDelegate?.Invoke(player, oldWeapon, newWeapon) ?? true;
            } catch(Exception e) {
                Logger.logException(e);
                return false;
            }
        }

        private bool onStartProjectile(IPlayer player, Position startPosition, Position direction, uint ammoHash, uint weaponHash) {
            try {
                return StartProjectileDelegate?.Invoke(player, startPosition, direction, ammoHash, weaponHash) ?? true;
            } catch(Exception e) {
                Logger.logException(e);
                return false;
            }
        }

        private void onNetworkOwnerChange(IEntity target, IPlayer oldNetOwner, IPlayer newNetOwner) {
            try {
                NetworkOwnerChangeDelegate?.Invoke(target, oldNetOwner, newNetOwner);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private void onVehicleDamage(IVehicle target, IEntity attacker, uint bodyHealthDamage, uint additionalBodyHealthDamage, uint engineHealthDamage, uint petrolTankDamage, uint weaponHash) {
            try {
                VehicleDamageDelegate?.Invoke(target, attacker, bodyHealthDamage, additionalBodyHealthDamage, engineHealthDamage, petrolTankDamage, weaponHash);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private void onVehicleDestroy(IVehicle vehicle) {
            try {
                VehicleDestroyDelegate?.Invoke(vehicle);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private bool onExplosion(IPlayer player, AltV.Net.Data.ExplosionType explosionType, Position position, uint explosionFx, IEntity targetEntity) {
            try {
                return ExplosionDelegate?.Invoke(player, explosionType, position, explosionFx, targetEntity) ?? true;
            } catch(Exception e) {
                Logger.logException(e);
                return true;
            }
        }

        private void onPlayerWeaponChange(IPlayer player, uint oldWeapon, uint newWeapon) {
            try {
                PlayerWeaponChangeDelegate?.Invoke(player, oldWeapon, newWeapon);
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        #endregion

        #region ChoiceV CustomEvents

        public static void onMainReady() {
            if(MainReadyDelegate != null) {
                MainReadyDelegate.Invoke();
            }

            if(MainAfterReadyDelegate != null) {
                MainAfterReadyDelegate.Invoke();
            }
        }

        public static void onMainShutdown() {
            if(MainShutdownDelegate != null) {
                MainShutdownDelegate.Invoke();


                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    PlayerDisconnectedDelegate.Invoke(player, "Server-Shutdown");
                }
            }
        }

        public static void onTick() {
            try {
                if(TickDelegate != null) {
                    TickDelegate.Invoke();
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        public static void onLongTick() {
            try {
                if(LongTickDelegate != null) {
                    LongTickDelegate.Invoke();
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        public static void onPlayerMoved(object sender, IPlayer player, Position oldPos, WorldGrid previousWorldGrid, Position newPos, WorldGrid[] currentGrids) {
            var thread = new Thread(() => {
                var distance = newPos.Distance(oldPos);

                try {
                    //Trigger -1 events (which will always be called)
                    if(RegisteredPlayerMoveEvents.ContainsKey(-1)) {
                        foreach(var callBack in RegisteredPlayerMoveEvents[-1]) {
                            callBack.Invoke(sender, player, newPos, distance);
                        }
                    }

                    if(previousWorldGrid != null && RegisteredPlayerMoveEvents.ContainsKey(previousWorldGrid.Id)) {
                        foreach(var callBack in RegisteredPlayerMoveEvents[previousWorldGrid.Id]) {
                            callBack.Invoke(sender, player, newPos, distance);
                        }
                    }

                    foreach(var currentGrid in currentGrids) {
                        if(currentGrid != previousWorldGrid) {
                            if(RegisteredPlayerMoveEvents.ContainsKey(currentGrid.Id)) {
                                foreach(var callBack in RegisteredPlayerMoveEvents[currentGrid.Id]) {
                                    callBack.Invoke(sender, player, newPos, distance);
                                }
                            }
                        }
                    }
                } catch(Exception e) {
                    Logger.logException(e);
                }
            });

            thread.Start();
        }

        public static void onPlayerChangeWorldGrid(object sender, IPlayer player, WorldGrid previousGrid, WorldGrid currentGrid) {
            if(PlayerChangeWorldGridDelegate != null) {
                PlayerChangeWorldGridDelegate.Invoke(sender, player, previousGrid, currentGrid);
            }
        }

        /// <summary>
        /// Careful when calling this before deletion! Await the finishing of this function before deleting the vehicle
        /// </summary>
        public static Task onVehicleMoved(object sender, ChoiceVVehicle vehicle, Position previousPos, Position newPos, WorldGrid[] currentGrids, float distance) {
            return Task.Run(() => {
                //Invoke Standard Event
                VehicleMovedDelegate.Invoke(sender, vehicle, previousPos, newPos, distance);
                foreach(var currentGrid in currentGrids) {
                    if(RegisteredVehicleMoveEvents.ContainsKey(currentGrid.Id) && vehicle.Exists()) {
                        foreach(var callBack in RegisteredVehicleMoveEvents[currentGrid.Id]) {
                            callBack.Invoke(sender, (ChoiceVVehicle)vehicle, newPos, previousPos, distance);
                        }
                    }
                }
            });
        }

        public static void onCollisionShapeInteraction(IPlayer player, string eventName, object[] args) {
            onPlayerEventTrigger(player, eventName, args);
        }

        //IMPORTANT Add Here all includes for Successfull Connection player Clothing stuff
        public static void onPlayerSuccessfullConnection(IPlayer player) {
            try {
                character character;
                using(var db = new ChoiceVDb()) {
                    character = db.characters
                        .Include(c => c.account)
                            .ThenInclude(a => a.accountkeymappings)
                        .Include(c => c.characterclothing)
                        .Include(c => c.characterstyle)
                        .Include(c => c.characterinjuries)
                            .ThenInclude(i => i.configInjuryNavigation)
                                .ThenInclude(i => i.treatmentCategoryNavigation)
                                    .ThenInclude(t => t.configinjurytreatmentssteps)
                        .Include(c => c.characterdata)
                        .Include(c => c.tattoos)
                        .Include(c => c.charactersettings)
                        .Include(c => c.characterdrugs)
                        .Include(c => c.charactercrimereputations)
                        .Include(c => c.charactercrimemissiontoken)
                        .Include(c => c.charactercrimemissiontrigger)
                        .Include(c => c.charactersetanimations)
                    .FirstOrDefault(c => c.id == player.getCharacterId());
                }

                PlayerPreSuccessfullConnectionDelegate?.Invoke(player, character);
                PlayerSuccessfullConnectionDelegate?.Invoke(player, character);
                PlayerPastSuccessfullConnectionDelegate?.Invoke(player, character);
                player.setCharacterFullyLoaded(true);
                player.fadeScreen(false, 900);
            } catch(Exception e) {
                Logger.logError($"Error in loading Character with: {player.Name}",
                                $"Fehler im Charakter-Laden: Es gab einen Fehler beim Charakter laden.", player);
                Logger.logException(e);
                ChoiceVAPI.KickPlayer(player, "DollhouseMadness", "Es ist ein Fehler beim Laden des Charakters aufgetreten. Melde dich bitte im Support!", $"Fehler beim Laden: {e}");

            }
        }

        public static void onPlayerChangeDimension(IPlayer player, int oldDimension, int newDimension) {
            PlayerChangeDimensionDelegate?.Invoke(player, oldDimension, newDimension);
        }


        #endregion

        #region Util Functions

        public static string[] getMiloInteractionNames() {
            return RegisteredInteriorInteractionEvents.Keys.ToArray();
        }

        #endregion
    }

    internal class EventControllerCallback {
        private readonly EventControllerDelegate EventDelegate;

        public EventControllerCallback(EventControllerDelegate eventDelegate) {
            EventDelegate = eventDelegate;
        }

        public bool invokeDelegate(IPlayer player, string eventName, object[] args) {
            return EventDelegate.Invoke(player, eventName, args);
        }
    }
        
    public class PlayerWebSocketConnectionDataElement {
        public int Id;
        public string LoginToken;
        public string Event;
        public string Data;
        public bool ReleaseMovement;
        public string MovementBlockedIdentifier;
    }

    public class PlayerKeyEventCallback {
        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public EventControllerKeyDelegate Callback { get; private set; }
        public bool IgnoresBusy { get; private set; }
        public bool IgnoresKeyLock { get; private set; }


        public PlayerKeyEventCallback(string identifier, string name, EventControllerKeyDelegate callback, bool ignoresBusy, bool ignoresKeyLock) {
            Identifier = identifier;
            Name = name;
            Callback = callback;
            IgnoresBusy = ignoresBusy;
            IgnoresKeyLock = ignoresKeyLock;
        }
    }

    public class PlayerKeyToggleEventCallback {
        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public EventControllerKeyToggleDelegate Callback { get; private set; }
        public bool IgnoresBusy { get; private set; }
        
        public PlayerKeyToggleEventCallback(string identifier, string name, EventControllerKeyToggleDelegate callback, bool ignoresBusy = false) {
            Identifier = identifier;
            Name = name;
            Callback = callback;
            IgnoresBusy = ignoresBusy;
        }
    }
}
