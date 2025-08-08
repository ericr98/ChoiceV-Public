using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Fire;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

// Database & class description
//
// fireModel                Modelname (Forest, Apartment, Farm, Oil, ...)
// fireSize                 Max. size of fire und childfires (2-3 is standard)
// fireCount                Total fires count
//
// childDelay               Delaytime in minutes for child fires
// childSpread              Spread out of child fires (3-6 is standard)
//
// eventRunDate             Datespawn where event is triggered (Format: 01.04.20-15.04.20 or empty)
// eventRunDays             Days of week where event is triggered (Format: Mo,Di,Mi,Do,Fr,Sa,So or empty)
// eventRunTime             Timespawn where event is triggered (Format: 00:00-23:59 or empty)
// eventRunPlayers          Min. amount of players in server to trigger event
// eventRunDepartment       Min. amount of players in fire department to trigger event
//
// eventDate                Generated start date & time where event starts
// eventTimespan            Time in minutes where event starts again
// eventRandomTime          Random time in minutes (will be added to eventTimespan.)
// eventMaxRunTime          Amount of minutes where event will end if not extinguished
//
// eventActive              Generated if event is running (0 or 1). Can be used by other plugins
//
// explosionPossible        Is explosion possible (0 or 1)
// explosionProbability     Probability of a explosion (0 to 100)
//
// position                 Position of main fire
// rotation                 Roatation of main fire (not used by now)
// colHeight                Height (y) of event marker
// colWidth                 Width (x) of event marker
//
// codeItem                 Classname for activator (One of the subclasses name: FireForest, FireOil, FireGas, ...)
//
//
// Database records can be changed by hand or from other plugins and will be read from controller every interval (typical 30sec.) if event is not running
// Every subclass has a different fire and smoke type
// If you try to extuingish oilfire with water there will be an explosion and fire grows to full size (use fire extinguisher or foam)

namespace ChoiceVServer.Controller {
    public class FireController : ChoiceVScript {
        // Private class variables
        private int lastObject = 0;
        private bool tick = true;

        // Public class variables
        public static List<FireObject> AllFireObjects = new List<FireObject>();

        public FireController() {
            EventController.addCollisionShapeEvent("FIRE_OBJECT_SPOT", onFireObjectInteract);

            EventController.addMenuEvent("START_FIREEVENT", onStartFireObject);
            EventController.addMenuEvent("TEST_FIREEVENT", onTestFireObject);
            EventController.addMenuEvent("CHANGE_FIREEVENT", onChangeFireObject);
            EventController.addMenuEvent("STOP_FIREEVENT", onStopFireObject);
            EventController.addMenuEvent("DESTROY_FIREEVENT", onDestroyFireObject);

            loadFireObject();

            InvokeController.AddTimedInvoke("Fire-Interval", (ivk) => onFireInterval(), TimeSpan.FromSeconds(FIRE_INTERVAL_SECONDS), true);
        }

        private void loadFireObject() {
            try {
                using(var db = new ChoiceVDb()) {
                    foreach(var row in db.fireobjects) {
                        Type type = Type.GetType("ChoiceVServer.Model.Fire." + row.codeItem, false);

                        FireObject fire = Activator.CreateInstance(type, row.fireSize, row.fireCount, row.childDelay, row.childSpread, row.eventRunDate, row.eventRunDays, row.eventRunTime, row.eventRunPlayers, row.eventRunDepartment, row.eventDate, row.eventTimespan, row.eventRandomTime, row.eventMaxRunTime, row.eventActive, row.explosionPossible, row.explosionProbability, row.position.FromJson<Position>(), row.rotation.FromJson<Rotation>(), row.colWidth, row.colHeight) as FireObject;

                        fire.Id = row.id;
                        fire.eventActive = 0;

                        fire.initialize(false);

                        AllFireObjects.Add(fire);
                    }

                    db.Dispose();
                }
            } catch(Exception e) {
                Logger.logException(e, "loadFireObject: Something went wrong");
            }
        }

        public static void registerFireObject(FireObject fire) {
            try {
                using(var db = new ChoiceVDb()) {
                    var row = new fireobject {
                        createDate = fire.createDate,

                        fireModel = fire.fireModel,
                        fireSize = fire.fireSize,
                        fireCount = fire.fireCount,
                        childDelay = fire.childDelay,
                        childSpread = fire.childSpread,

                        eventRunDate = fire.eventRunDate,
                        eventRunDays = fire.eventRunDays,
                        eventRunTime = fire.eventRunTime,
                        eventRunPlayers = fire.eventRunPlayers,
                        eventRunDepartment = fire.eventRunDepartment,

                        eventDate = fire.eventDate,
                        eventTimespan = fire.eventTimespan,
                        eventRandomTime = fire.eventRandomTime,
                        eventMaxRunTime = fire.eventMaxRunTime,
                        eventActive = fire.eventActive,

                        explosionPossible = fire.explosionPossible,
                        explosionProbability = fire.explosionProbability,

                        position = fire.colPosition.ToJson(),
                        rotation = fire.colRotation.ToJson(),
                        colHeight = fire.colHeight,
                        colWidth = fire.colWidth,

                        codeItem = fire.GetType().Name,
                    };

                    db.fireobjects.Add(row);
                    db.SaveChanges();

                    fire.Id = row.id;

                    db.Dispose();
                }

                AllFireObjects.Add(fire);

            } catch(Exception e) {
                Logger.logException(e, "registerFireObject: Something went wrong");
            }
        }

        public static void unregisterFireObject(FireObject fire) {
            try {
                using(var db = new ChoiceVDb()) {
                    var row = db.fireobjects.FirstOrDefault(r => r.id == fire.Id);
                    if(row != null) {
                        db.fireobjects.Remove(row);
                        db.SaveChanges();
                    }

                    db.Dispose();
                }

                if(AllFireObjects.Contains(fire))
                    AllFireObjects.Remove(fire);

            } catch(Exception e) {
                Logger.logException(e, "unregisterFireObject: Something went wrong");
            }
        }

        public static void updateFireObject(FireObject fire) {
            try {
                using(var db = new ChoiceVDb()) {
                    var row = db.fireobjects.FirstOrDefault(r => r.id == fire.Id);
                    if(row != null) {
                        row.fireSize = fire.fireSize;
                        row.fireCount = fire.fireCount;
                        row.childDelay = fire.childDelay;
                        row.childSpread = fire.childSpread;

                        row.eventRunDate = fire.eventRunDate;
                        row.eventRunDays = fire.eventRunDays;
                        row.eventRunTime = fire.eventRunTime;
                        row.eventRunPlayers = fire.eventRunPlayers;
                        row.eventRunDepartment = fire.eventRunDepartment;

                        row.eventDate = fire.eventDate;
                        row.eventTimespan = fire.eventTimespan;
                        row.eventRandomTime = fire.eventRandomTime;
                        row.eventMaxRunTime = fire.eventMaxRunTime;
                        row.eventActive = fire.eventActive;

                        row.explosionPossible = fire.explosionPossible;
                        row.explosionProbability = fire.explosionProbability;

                        row.position = fire.colPosition.ToJson();
                        row.rotation = fire.colRotation.ToJson();
                        row.colHeight = fire.colHeight;
                        row.colWidth = fire.colWidth;

                        db.SaveChanges();
                    }

                    db.Dispose();
                }
            } catch(Exception e) {
                Logger.logException(e, "updateFireObject: Something went wrong");
            }
        }

        private void onFireInterval() {
            try {
                int count = AllFireObjects.Count();

                if(count > 0) {
                    if(tick) {

                        // Block update for all running events
                        foreach(FireObject fire in AllFireObjects)
                            if(fire.running || fire.test) fire.onInterval();

                        tick = false;

                    } else {
                        if(lastObject >= count) lastObject = 0;

                        FireObject fire = AllFireObjects[lastObject++];

                        // Single update for non running events
                        if(fire != null && !fire.running && !fire.test) {
                            using(var db = new ChoiceVDb()) {
                                var row = db.fireobjects.FirstOrDefault(r => r.id == fire.Id);

                                // Update fire class from database
                                if(row != null) {
                                    fire.fireSize = row.fireSize.Value;
                                    fire.fireCount = row.fireCount.Value;
                                    fire.childDelay = row.childDelay.Value;
                                    fire.childSpread = row.childSpread.Value;

                                    fire.eventRunDate = row.eventRunDate;
                                    fire.eventRunDays = row.eventRunDays;
                                    fire.eventRunTime = row.eventRunTime;
                                    fire.eventRunPlayers = row.eventRunPlayers.Value;
                                    fire.eventRunDepartment = row.eventRunDepartment.Value;

                                    fire.eventDate = row.eventDate;
                                    fire.eventTimespan = row.eventTimespan.Value;
                                    fire.eventRandomTime = row.eventRandomTime.Value;
                                    fire.eventMaxRunTime = row.eventMaxRunTime.Value;
                                    fire.eventActive = row.eventActive.Value;

                                    fire.explosionPossible = row.explosionPossible.Value;
                                    fire.explosionProbability = row.explosionProbability.Value;

                                    fire.colPosition = row.position.FromJson<Position>();
                                    fire.colRotation = row.rotation.FromJson<Rotation>();
                                    fire.colHeight = row.colHeight.Value;
                                    fire.colWidth = row.colWidth.Value;

                                    fire.onInterval();

                                    // If record is deleted then remove fire class
                                } else {
                                    fire.onRemove();
                                }

                                db.Dispose();
                            }
                        }

                        // Check for new database entries
                        using(var db = new ChoiceVDb()) {
                            foreach(var row in db.fireobjects) {
                                fire = AllFireObjects.Find(r => r.Id == row.id);

                                if(fire == null) {
                                    Type type = Type.GetType("ChoiceVServer.Model.Fire." + row.codeItem, false);
                                    fire = Activator.CreateInstance(type, row.fireSize, row.fireCount, row.childDelay, row.childSpread, row.eventDate, row.eventTimespan, row.eventRandomTime, row.eventMaxRunTime, row.eventActive, row.explosionPossible, row.explosionProbability, row.position.FromJson<Position>(), row.rotation.FromJson<Rotation>(), row.colWidth, row.colHeight) as FireObject;

                                    fire.Id = row.id;
                                    fire.eventActive = 0;

                                    fire.initialize(false);

                                    AllFireObjects.Add(fire);
                                }
                            }

                            db.Dispose();
                        }

                        tick = true;
                    }
                }
            } catch(Exception e) {
                Logger.logException(e, "onFireInterval: Something went wrong: ");
            }
        }

        private bool onFireObjectInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
            var plac = AllFireObjects.FirstOrDefault(p => p.CollisionShapeFire.Id == collisionShape.Id);

            if(plac != null && player.getCharacterData().AdminMode)
                plac.onInteraction(player);

            return true;
        }

        private bool onStartFireObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("fireevent")) {
                var obj = (FireObject)data["fireevent"];

                obj.start = true;

                player.sendNotification(NotifactionTypes.Success, "Feuerevent wird gestartet.", "", NotifactionImages.Fire);
                return true;
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"onTestFireObject: Fire object not found!");
            }

            return false;
        }

        private bool onTestFireObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("fireevent")) {
                var obj = (FireObject)data["fireevent"];

                obj.test = true;

                player.sendNotification(NotifactionTypes.Success, "Feuerevent wird getestet und stoppt automatisch nach 5 Minuten.", "", NotifactionImages.Fire);
                return true;
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"onTestFireObject: Fire object not found!");
            }

            return false;
        }

        private bool onChangeFireObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("fireevent")) {
                var obj = (FireObject)data["fireevent"];

                return true;
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"onChangeFireObject: Fire object not found!");
            }

            return false;
        }

        private bool onStopFireObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("fireevent")) {
                var obj = (FireObject)data["fireevent"];

                obj.onStop();

                player.sendNotification(NotifactionTypes.Success, "Feuerevent wurde gestoppt und alle Feuer gelöscht.", "", NotifactionImages.Fire);
                return true;
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"onChangeFireObject: Fire object not found!");
            }

            return false;
        }

        private bool onDestroyFireObject(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("fireevent")) {
                var obj = (FireObject)data["fireevent"];

                obj.onRemove();

                player.sendNotification(NotifactionTypes.Success, "Feuerevent wurde gelöscht.", "", NotifactionImages.Fire);
                return true;
            } else {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"onDestroyFireObject: Fire object not found!");
            }

            return false;
        }
    }
}
