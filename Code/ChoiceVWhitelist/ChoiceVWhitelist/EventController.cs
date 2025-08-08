using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Native;
using ChoiceVServer.Base;

namespace ChoiceVServer {
    public delegate bool EventControllerDelegate(IPlayer player, string eventName, object[] args);
    public delegate bool EventControllerKeyDelegate(IPlayer player, ConsoleKey key, string eventName);


    public delegate void TickDelegate();
    public delegate void LongTickDelegate();

    #region ChoiceV Custom Delegates

    public delegate void MainReadyDelegate();
    public delegate void MainShutdownDelegate();

    public delegate void PlayerMovedDelegate(object sender, IPlayer player, Position moveToPosition);
    public delegate void VehicleMovedDelegate(object sender, IVehicle vehicle, Position moveFromPos, Position moveToPosition, float distance);

    public delegate void CollisionShapeInteractionDelegate(IPlayer player);

    public delegate void ObjectInteractionDelegate(IPlayer player, string modelhash, Position objectPosition, Position playerOffset, float objectHeading, DegreeRotation rotation);

    public delegate void CefEventDelegate(IPlayer player, PlayerWebSocketConnectionDataElement evt);
    #endregion

    #region API Delegates
    public delegate void PlayerConnectedDelegate(IPlayer player, string reason);
    public delegate void PlayerDisconnectedDelegate(IPlayer player, string reason);
    public delegate void PlayerEnterVehicleDelegate(IPlayer player, IVehicle vehicle, byte seatId);
    public delegate void PlayerExitVehicleDelegate(IPlayer player, IVehicle vehicle, byte seatId);
    public delegate void PlayerDeadDelegate(IPlayer player, IEntity killer, uint weapon);
    public delegate void PlayerDamageDelegate(IPlayer player, IEntity killer, uint weapon, ushort damage);
    public delegate void ConsoleCommandDelegate(string text, string[] args);

    public delegate void CheckpointDelegate(ICheckpoint checkpoint, IEntity entity, bool state);
    public delegate WeaponDamageResponse WeaponDamageDelegate(IPlayer player, IEntity target, uint weapon, ushort damage, Position shotOffset, BodyPart bodyPart);
    #endregion

    public class EventController : ChoiceVScript {
        /// <summary>
        /// Is triggered every 200 milliseconds
        /// </summary>
        public static TickDelegate TickDelegate;
        public static LongTickDelegate LongTickDelegate;

        #region API Delegate Variables
        public static PlayerConnectedDelegate PlayerConnectedDelegate;
        public static PlayerDisconnectedDelegate PlayerDisconnectedDelegate;
        public static PlayerEnterVehicleDelegate PlayerEnterVehicleDelegate;
        public static PlayerExitVehicleDelegate PlayerExitVehicleDelegate;
        public static PlayerDeadDelegate PlayerDeadDelegate;
        public static PlayerDamageDelegate PlayerDamageDelegate;
        public static ConsoleCommandDelegate ConsoleCommandDelegate;

        public static CheckpointDelegate CheckpointDelegate;

        public static WeaponDamageDelegate WeaponDamageDelegate;
        #endregion

        #region ChoiceV Custom Delegate Variables

        public static MainReadyDelegate MainReadyDelegate;
        public static MainShutdownDelegate MainShutdownDelegate;

        #endregion

        private static ConcurrentDictionary<string, List<EventControllerCallback>> RegisteredEvents = new ConcurrentDictionary<string, List<EventControllerCallback>>();
        private static ConcurrentDictionary<ConsoleKey, List<EventControllerKeyDelegate>> RegisteredKeyEvents = new ConcurrentDictionary<ConsoleKey, List<EventControllerKeyDelegate>>();

        private static ConcurrentDictionary<string, List<CefEventDelegate>> RegisteredCefEvents = new ConcurrentDictionary<string, List<CefEventDelegate>>();

        private static ConcurrentDictionary<IntPtr, object> PlayerLocks = new ConcurrentDictionary<IntPtr, object>();

        public EventController() {
            Alt.OnPlayerEvent += onPlayerEventTrigger;
            Alt.OnPlayerConnect += onPlayerConnected;
            Alt.OnPlayerDisconnect += onPlayerDisonnected;
            Alt.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Alt.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;
            Alt.OnConsoleCommand += OnConsoleCommand;
            Alt.OnPlayerDead += OnPlayerDead;
            Alt.OnPlayerDamage += OnPlayerDamage;

            Alt.OnWeaponDamage += OnWeaponDamage;
        }

        //TODO Maybe Find way to Call (IPlayer player, string eventName, object[] args)
        public static void onPlayerEventTrigger(IPlayer player, string eventName, object[] args) {
            try {
                if(!PlayerLocks.ContainsKey(player.NativePointer)) {
                    player.Kick("Not found!");
                    return;
                }

                var playerLock = PlayerLocks[player.NativePointer];
                lock(playerLock) {
                    //Standart EventControllerDelegate Event
                    var toCallEvents = RegisteredEvents.FirstOrDefault(p => p.Key.Equals(eventName)).Value;
                    if(toCallEvents == null) {
                        Logger.logError($"Event: {eventName} triggered by {player.Name} not found!");
                        return;
                    }

                    string argsToString = "";
                    foreach(var item in args) {
                        if(item != null)
                            argsToString = argsToString + " " + item.ToString();
                    }

                    Logger.logDebug($"Event: {eventName} triggered by {player.Name}. With Data: {argsToString}");

                    foreach(var callback in toCallEvents) {
                        callback.invokeDelegate(player, eventName, args);
                    }
                }
            } catch(Exception e) {
                Logger.logError(e.ToString());
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

        ///// <summary>
        ///// Method for adding a event to an event for a specific Checkpoint. It is triggered when an Entity leaves or walks in a Checkpoint
        ///// </summary>
        ///// <param name="checkpoint">The Checkpoint the event should be registered to</param>
        ///// <param name="eventDelegate">Callback for the registered IPlayerEvent</param>
        //public void addCheckpointEvent(Checkpoint checkpoint, CheckpointEventDelegate eventDelegate) {
        //    List<CheckpointEventDelegate> eventList = null;
        //    if (RegisteredCheckpointEvents.ContainsKey(checkpoint.NativePointer))
        //        eventList = RegisteredCheckpointEvents[checkpoint.NativePointer];

        //    if (eventList == null)
        //        eventList = new List<CheckpointEventDelegate>();

        //    eventList.Add(new CheckpointEventDelegate(eventDelegate));
        //    RegisteredCheckpointEvents[checkpoint.NativePointer] = eventList;
        //}

        #region API "Event Extensions"

        //Player
        public static void onPlayerConnected(IPlayer player, string reason) {
            PlayerLocks.TryAdd(player.NativePointer, new object());

            try {
                PlayerConnectedDelegate?.Invoke(player, reason);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }
        }

        public static void onPlayerDisonnected(IPlayer player, string reason) {
            object ignored;
            PlayerLocks.Remove(player.NativePointer, out ignored);

            try {
                PlayerDisconnectedDelegate?.Invoke(player, reason);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }

            BaseObjectData.RemoveBaseObject(player.NativePointer);
        }

        public static void OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seatId) {
            try {
                PlayerEnterVehicleDelegate?.Invoke(player, vehicle, seatId);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }
        }

        public static void OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seatId) {
            try {
                PlayerExitVehicleDelegate?.Invoke(player, vehicle, seatId);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }
        }

        private void OnConsoleCommand(string name, string[] args) {
            try {
                ConsoleCommandDelegate?.Invoke(name, args);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }
        }

        private void OnPlayerDead(IPlayer player, IEntity killer, uint weapon) {
            try {
                PlayerDeadDelegate?.Invoke(player, killer, weapon);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }
        }

        private void OnPlayerDamage(IPlayer player, IEntity attacker, uint weapon, ushort damage, ushort armor) {
            try {
                PlayerDamageDelegate?.Invoke(player, attacker, weapon, damage);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }
        }

        private void OnCheckpoint(ICheckpoint checkpoint, IEntity entity, bool state) {
            try {
                CheckpointDelegate?.Invoke(checkpoint, entity, state);
            } catch(Exception e) {
                Logger.logError(e.ToString());
            }
        }

        private WeaponDamageResponse OnWeaponDamage(IPlayer player, IEntity target, uint weapon, ushort damage, Position shotOffset, BodyPart bodyPart) {
            try {
                if(WeaponDamageDelegate != null) {
                    return WeaponDamageDelegate.Invoke(player, target, weapon, damage, shotOffset, bodyPart);
                } else {
                    return true;
                }
            } catch(Exception e) {
                Logger.logError(e.ToString());
                return false;
            }
        }

        #endregion

        #region ChoiceV CustomEvents

        public static void onMainReady() {
            if(MainReadyDelegate != null) {
                MainReadyDelegate.Invoke();
            }
        }

        public static void onMainShutdown() {
            if(MainShutdownDelegate != null) {
                MainShutdownDelegate.Invoke();
            }
        }

        public static void onTick() {
            if(TickDelegate != null) {
                TickDelegate.Invoke();
            }
        }

        public static void onLongTick() {
            if(LongTickDelegate != null) {
                LongTickDelegate.Invoke();
            }
        }

        public static void onCollisionShapeInteraction(IPlayer player, string eventName, object[] args) {
            onPlayerEventTrigger(player, eventName, args);
        }

        #endregion
    }

    class EventControllerCallback {
        public EventControllerDelegate EventDelegate;

        public EventControllerCallback(EventControllerDelegate eventDelegate) {
            EventDelegate = eventDelegate;
        }

        public bool invokeDelegate(IPlayer player, string eventName, object[] args) {
            return EventDelegate.Invoke(player, eventName, args);
        }
    }

    public class PlayerWebSocketConnectionDataElement {
        public int Id;
        public string Event;
        public string Data;
        public bool BlockedMovement;
    }
}