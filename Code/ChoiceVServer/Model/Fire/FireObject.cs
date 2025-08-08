using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using AltV.Net;
using System.Linq;

namespace ChoiceVServer.Model.Fire {
    public class FireObject {
        // Public class variables
        public List<Fires> AllFires = new List<Fires>();
        public List<IPlayer> AllPlayers = new List<IPlayer>();
        public List<IPlayer> AllFirefighters = new List<IPlayer>();

        public DateTime systemTime = DateTime.Now;
        public DateTime startTime = DateTime.Now;
        public DateTime runTime = DateTime.Now;

        public DateTime lastGrown = DateTime.Now;
        public DateTime lastFire = DateTime.Now;
        public DateTime lastNotice = DateTime.Now;

        public string particleName = "";
        public string smokeName = "";
        public bool running = false;
        public bool test = false;
        public bool start = false;
        public bool firstStrike = true;
        public int explosionType = 0;
        public int fireCounter = 0;
        public int exploCounter = 0;
        public int percGrown = 0;

        public static string[] DayOfWeek = new string[] { "SO", "MO", "DI", "MI", "DO", "FR", "SA" };
        public static int fireId = 1;

        // Public class data
        public int Id = 0;
        public DateTime createDate = DateTime.Now;

        public string fireModel = "";
        public float fireSize = 2f;
        public int fireCount = 0;
        public int childDelay = 0;
        public int childSpread = 0;

        public string eventRunDate = "";
        public string eventRunDays = "";
        public string eventRunTime = "";
        public int eventRunPlayers = 0;
        public int eventRunDepartment = 0;

        public DateTime eventDate = DateTime.Now;
        public int eventTimespan = 0;
        public int eventRandomTime = 0;
        public int eventMaxRunTime = 0;
        public int eventActive = 0;

        public int explosionPossible = 0;
        public int explosionProbability = 0;

        public Position colPosition;
        public Rotation colRotation;

        public float colWidth = 9f;
        public float colHeight = 9f;

        public CollisionShape CollisionShapeFire = null;
        public CollisionShape CollisionShapeArea = null;

        public FireObject() { }

        public FireObject(float firesize, int firecount, int childdelay, int childspread, string eventrundate, string eventrundays, string eventruntime, int eventrunplayers, int eventrundepartment, DateTime eventdate, int eventtimespan, int eventrandomtime, int eventmaxruntime, int eventactive, int explosionpossible, int explosionprobability, Position position, Rotation rotation, float width, float height) {
            createDate = DateTime.Now;

            fireModel = "";
            fireSize = firesize;
            fireCount = firecount;

            childDelay = childdelay;
            childSpread = childspread;

            eventRunDate = eventrundate;
            eventRunDays = eventrundays;
            eventRunTime = eventruntime;
            eventRunPlayers = eventrunplayers;
            eventRunDepartment = eventrundepartment;

            eventDate = eventdate;
            eventTimespan = eventtimespan;
            eventRandomTime = eventrandomtime;
            eventMaxRunTime = eventmaxruntime;
            eventActive = eventactive;

            explosionPossible = explosionpossible;
            explosionProbability = explosionprobability;

            colPosition = position;
            colRotation = rotation;

            colWidth = width;
            colHeight = height;

            // Create interaction shape
            CollisionShapeFire = CollisionShape.Create(position, width, height, rotation.Yaw, true, false, true, "FIRE_OBJECT_SPOT");
            CollisionShapeFire.Owner = this;

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;

            EventController.addEvent("FIRE_EXTINGUISH", onExtinguishFire);
            EventController.addEvent("FIRE_EXTINGUISHED", onExtinguishedFire);
        }

        /// <summary>
        /// IMPORTANT! Call the base.initialize() after you created your Object! Otherwise the db will return an error
        /// </summary>
        /// <param name="register"></param>
        public virtual void initialize(bool register = true) {
            if(register) {
                FireController.registerFireObject(this);
            }
        }

        public virtual void onRemove() {
            try {
                // Remove all fires on all clients
                foreach(Fires fire in AllFires) {
                    foreach(IPlayer player in AllPlayers)
                        player.emitClientEvent("REMOVE_FIRE", fire.Id);
                }

                // Reset all lists
                AllFires.Clear();
                AllPlayers.Clear();
                AllFirefighters.Clear();

                // Remove from database
                FireController.unregisterFireObject(this);

                // Remove collisions
                if(CollisionShapeFire != null) {
                    CollisionShapeFire.Dispose();
                    CollisionShapeFire = null;
                }

                if(CollisionShapeArea != null) {
                    CollisionShapeArea.Dispose();
                    CollisionShapeArea = null;
                }

                EventController.PlayerDisconnectedDelegate -= onPlayerDisconnect;

                Logger.logDebug(LogCategory.System, LogActionType.Removed, $"Event {fireModel} fire ID:{Id} removed.");
            } catch(Exception e) {
                Logger.logException(e, "onRemove: Something went wrong");
            }
        }

        public virtual void onStop() {
            try {
                systemTime = DateTime.Now;
                Random rand = new Random();

                // Remove area collision
                if(CollisionShapeArea != null) {
                    CollisionShapeArea.Dispose();
                    CollisionShapeArea = null;
                }

                // Set data
                eventDate = systemTime.AddMinutes(eventTimespan + rand.Next(0, eventRandomTime));
                startTime = systemTime;
                runTime = systemTime;
                eventActive = 0;

                // Update database
                FireController.updateFireObject(this);

                // Set flags
                running = false;
                start = false;
                test = false;

                // Remove all fires on all clients
                foreach(Fires fire in AllFires) {
                    foreach(IPlayer player in AllPlayers)
                        player.emitClientEvent("REMOVE_FIRE", fire.Id);
                }

                // Clear fire- and firefighterlist
                AllFires.Clear();
                AllFirefighters.Clear();

                Logger.logDebug(LogCategory.System, LogActionType.Removed, $"Event {fireModel} fire ID:{Id} stopped (forced).");
            } catch(Exception e) {
                Logger.logException(e, "onStop: Something went wrong");
            }
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            try {
                // Remove player from player- and firefighterlist
                if(player != null && AllPlayers.Contains(player)) {
                    AllPlayers.Remove(player);

                    if(AllFirefighters.Contains(player))
                        AllFirefighters.Remove(player);

                    // Logger.logDebug($"Player ID:{player.Name} has left event {fireModel} fire ID:{Id} (disconnect).");
                }
            } catch(Exception e) {
                Logger.logException(e, "onPlayerDisconnect: Something went wrong");
            }
        }

        public virtual void onFireEnterShape(CollisionShape shape, IEntity entity) {
            try {
                IPlayer player = null;

                if(entity.Type == BaseObjectType.Vehicle) {
                    ChoiceVVehicle vehicle = (ChoiceVVehicle)entity;
                    player = vehicle.Driver;
                } else if(entity.Type == BaseObjectType.Player) {
                    player = (IPlayer)entity;
                }

                // Add player to playerlist and create fires on client
                if(player != null && !AllPlayers.Contains(player)) {
                    AllPlayers.Add(player);

                    foreach(Fires fire in AllFires)
                        player.emitClientEvent("CREATE_FIRE", fire.Id, fire.colPosition.X, fire.colPosition.Y, fire.colPosition.Z, particleName, smokeName, fire.Scale, true);

                    // Logger.logDebug($"Player ID:{player.Name} has entered event {fireModel} fire ID:{Id} (area).");
                }
            } catch(Exception e) {
                Logger.logException(e, "onFireEnterShape: Something went wrong");
            }
        }

        public virtual void onFireExitShape(CollisionShape shape, IEntity entity) {
            try {
                IPlayer player = null;

                if(entity.Type == BaseObjectType.Vehicle) {
                    ChoiceVVehicle vehicle = (ChoiceVVehicle)entity;
                    player = vehicle.Driver;
                } else if(entity.Type == BaseObjectType.Player) {
                    player = (IPlayer)entity;
                }

                // Remove player from player- and firefighterlist and remove fires on client
                if(player != null && AllPlayers.Contains(player)) {
                    AllPlayers.Remove(player);

                    if(AllFirefighters.Contains(player))
                        AllFirefighters.Remove(player);

                    foreach(Fires fire in AllFires)
                        player.emitClientEvent("REMOVE_FIRE", fire.Id);

                    // Logger.logDebug($"Player ID:{player.Name} has left event {fireModel} fire ID:{Id} (area).");
                }
            } catch(Exception e) {
                Logger.logException(e, "onFireExitShape: Something went wrong");
            }
        }

        public virtual void onInteraction(IPlayer player) {
            if(!running) {
                var menu = new Menu.Menu("Feuer", $"Feuerevent ID:{Id} ist nicht aktiv");
                var data = new Dictionary<string, dynamic> { { "fireevent", this } };

                menu.addMenuItem(new StaticMenuItem("Typ des Feuers:", "", fireModel));
                menu.addMenuItem(new StaticMenuItem("Maximale Größe der Feuer:", "", fireSize.ToString()));
                menu.addMenuItem(new StaticMenuItem("Maximale Anzahl der Feuer:", "", fireCount.ToString()));
                menu.addMenuItem(new StaticMenuItem("Verzögerung der Feuer:", "", childDelay + " min."));
                menu.addMenuItem(new StaticMenuItem("Ausbreitung der Feuer:", "", childSpread.ToString()));
                menu.addMenuItem(new StaticMenuItem("Explosionsgefahr:", "", (explosionPossible == 1 ? explosionProbability : 0) + " %"));

                if(eventRunDate.Length > 0) {
                    DateTime sta = Convert.ToDateTime("01.01." + systemTime.Year + " 00:00:00");
                    DateTime sto = Convert.ToDateTime("31.12." + systemTime.Year + " 23:59:59");

                    var date = eventRunDate.Split('-');

                    if(date.Length == 2) {
                        if(date[0].Length > 0)
                            sta = Convert.ToDateTime(date[0] + " 00:00:00");
                        if(date[1].Length > 0)
                            sto = Convert.ToDateTime(date[1] + " 23:59:59");
                    } else if(date.Length == 1) {
                        if(date[0].Length > 0)
                            sta = Convert.ToDateTime(date[0] + " 00:00:00");
                    }

                    menu.addMenuItem(new StaticMenuItem("Datum von/bis:", "", sta.ToString("dd.MM.yy") + "-" + sto.ToString("dd.MM.yy")));
                }

                if(eventRunDays.Length > 0) {
                    menu.addMenuItem(new StaticMenuItem("Wochentage:", "", eventRunDays.ToUpper()));
                }

                if(eventRunTime.Length > 0) {
                    DateTime sta = Convert.ToDateTime("00:00:00");
                    DateTime sto = Convert.ToDateTime("23:59:59");

                    var time = eventRunTime.Split('-');

                    if(time.Length == 2) {
                        if(time[0].Length > 0)
                            sta = Convert.ToDateTime(time[0]);
                        if(time[1].Length > 0)
                            sto = Convert.ToDateTime(time[1]);
                    } else if(time.Length == 1) {
                        if(time[0].Length > 0)
                            sta = Convert.ToDateTime(time[0]);
                    }

                    menu.addMenuItem(new StaticMenuItem("Uhrzeit von/bis:", "", sta.ToString("HH:mm") + "-" + sto.ToString("HH:mm")));
                }

                if(eventRunPlayers > 0)
                    menu.addMenuItem(new StaticMenuItem("Spieler Gesamt:", "", ChoiceVAPI.GetAllPlayers().Count + "/" + eventRunPlayers));

                if(eventRunDepartment > 0)
                    menu.addMenuItem(new StaticMenuItem("Spieler Feuerwehr:", "", ChoiceVAPI.GetAllPlayers().Count + "/" + eventRunDepartment));

                menu.addMenuItem(new ClickMenuItem("Jetzt starten", "", "", "START_FIREEVENT", MenuItemStyle.green).withData(data));
                menu.addMenuItem(new ClickMenuItem("Testen", "", "", "TEST_FIREEVENT", MenuItemStyle.green).withData(data));
                // menu.addMenuItem(new ClickMenuItem("Ändern", "", "", "CHANGE_FIREEVENT", MenuItemStyle.green).withData(data));
                menu.addMenuItem(new ClickMenuItem("Entfernen", "", "", "DESTROY_FIREEVENT", MenuItemStyle.red).withData(data));

                player.showMenu(menu);

            } else {
                var menu = new Menu.Menu("Feuer", $"Feuerevent ID:{Id} ist aktiv");
                var data = new Dictionary<string, dynamic> { { "fireevent", this } };

                menu.addMenuItem(new StaticMenuItem("Typ des Feuers:", "", fireModel));
                menu.addMenuItem(new StaticMenuItem("Status des Feuers:", "", percGrown + " %"));
                menu.addMenuItem(new StaticMenuItem("Maximale Größe der Feuer:", "", fireSize.ToString()));
                menu.addMenuItem(new StaticMenuItem("Maximale Anzahl der Feuer:", "", fireCount.ToString()));
                menu.addMenuItem(new StaticMenuItem("Verzögerung der Feuer:", "", childDelay + " min."));
                menu.addMenuItem(new StaticMenuItem("Ausbreitung der Feuer:", "", childSpread.ToString()));
                menu.addMenuItem(new StaticMenuItem("Explosionsgefahr:", "", (explosionPossible == 1 ? explosionProbability : 0) + " %"));

                menu.addMenuItem(new ClickMenuItem("Stoppen", "", "", "STOP_FIREEVENT", MenuItemStyle.red).withData(data));

                player.showMenu(menu);
            }
        }

        public virtual void onInterval() {
            try {
                systemTime = DateTime.Now;
                Random rand = new Random();

                // if not running check if event should be started now
                if(!running) {

                    // Don't check date, days and time if test or forced start
                    if(!start && !test) {

                        // Check for start- and enddate
                        if(eventRunDate.Length > 0) {
                            DateTime sta = Convert.ToDateTime("01.01." + systemTime.Year + " 00:00:00");
                            DateTime sto = Convert.ToDateTime("31.12." + systemTime.Year + " 23:59:59");

                            var date = eventRunDate.Split('-');

                            if(date.Length == 2) {
                                if(date[0].Length > 0)
                                    sta = Convert.ToDateTime(date[0] + " 00:00:00");
                                if(date[1].Length > 0)
                                    sto = Convert.ToDateTime(date[1] + " 23:59:59");
                            } else if(date.Length == 1) {
                                if(date[0].Length > 0)
                                    sta = Convert.ToDateTime(date[0] + " 00:00:00");
                            }

                            // Check if event should run at present date
                            if(DateTime.Compare(systemTime, sta) < 0 || DateTime.Compare(systemTime, sto) > 0) {
                                Logger.logTrace(LogCategory.System, LogActionType.Updated, $"Event {fireModel} fire ID:{Id} is not planned for now ({sta.ToString("dd.MM.yy")}-{sto.ToString("dd.MM.yy")}).");
                                return;
                            }
                        }

                        // Check for days of week
                        if(eventRunDays.Length > 0) {
                            string days = eventRunDays.ToUpper();

                            // Check if event should run on present day of week
                            if(!days.Contains(DayOfWeek[(int)systemTime.DayOfWeek])) {
                                Logger.logTrace(LogCategory.System, LogActionType.Updated, $"Event {fireModel}fire ID:{Id} is not planned for today ({days}).");
                                return;
                            }
                        }

                        // Check for time of day
                        if(eventRunTime.Length > 0) {
                            DateTime sta = Convert.ToDateTime("00:00:00");
                            DateTime sto = Convert.ToDateTime("23:59:59");

                            var time = eventRunTime.Split('-');

                            if(time.Length == 2) {
                                if(time[0].Length > 0)
                                    sta = Convert.ToDateTime(time[0]);
                                if(time[1].Length > 0)
                                    sto = Convert.ToDateTime(time[1]);
                            } else if(time.Length == 1) {
                                if(time[0].Length > 0)
                                    sta = Convert.ToDateTime(time[0]);
                            }

                            // Check if event should run at present time
                            if(DateTime.Compare(systemTime, sta) < 0 || DateTime.Compare(systemTime, sto) > 0) {
                                Logger.logTrace(LogCategory.System, LogActionType.Updated, $"Event {fireModel} fire ID:{Id} is not planned for now ({sta.ToString("HH:mm")}-{sto.ToString("HH:mm")}).");
                                return;
                            }
                        }

                        // Check online players
                        if(eventRunPlayers > 0) {
                            int players = ChoiceVAPI.GetAllPlayers().Count;

                            // Check if enough players are on server
                            if(players < eventRunPlayers) {
                                Logger.logDebug(LogCategory.System, LogActionType.Blocked, $"Not enough players on server to run event {fireModel} fire ID:{Id} ({players}/{eventRunPlayers}).");
                                return;
                            }
                        }

                        // Check online department players
                        if(eventRunDepartment > 0) {
                            int department = ChoiceVAPI.GetAllPlayers().Where(p => CompanyController.hasPlayerCompanyWithPredicate(p, c => c.CompanyType == Controller.Companies.CompanyType.Fire)).Count();

                            // Check if enough players are in department
                            if(department < eventRunDepartment) {
                                Logger.logDebug(LogCategory.System, LogActionType.Blocked, $"Not enough players in department to run event {fireModel} fire ID:{Id} ({department}/{eventRunDepartment}).");
                                return;
                            }
                        }
                    }

                    // Check for start of event
                    double seconds = ((start || test) ? 0f : eventDate.Subtract(systemTime).TotalSeconds);
                    if((seconds / 3600) >= 1f) {
                        Logger.logDebug(LogCategory.System, LogActionType.Updated, $"Event {fireModel} fire ID:{Id} is starting in {(int)(seconds / 3600)} hours.");
                    } else if((seconds / 60) >= 1f) {
                        Logger.logDebug(LogCategory.System, LogActionType.Updated, $"Event {fireModel} fire ID:{Id} is starting in {(int)(seconds / 60)} minutes.");
                    } else if(seconds >= 1f) {
                        Logger.logDebug(LogCategory.System, LogActionType.Updated, $"Event {fireModel} fire ID:{Id} is starting in {(int)seconds} seconds.");
                    } else {

                        // Create collision shape
                        CollisionShapeArea = CollisionShape.Create(colPosition, colWidth + 400, colHeight + 400, colRotation.Yaw, true, false, false, "FIRE_OBJECT_AREA");
                        CollisionShapeArea.HasNoHeight = true;
                        CollisionShapeArea.OnEntityEnterShape += onFireEnterShape;
                        CollisionShapeArea.OnEntityExitShape += onFireExitShape;
                        CollisionShapeArea.Owner = this;

                        // Check if players are already in area
                        foreach(IPlayer player in ChoiceVAPI.GetAllPlayers()) {
                            if(CollisionShapeArea.IsInShape(player.Position)) {
                                if(!AllPlayers.Contains(player)) {
                                    AllPlayers.Add(player);
                                }
                            }
                        }

                        // Set data
                        eventDate = systemTime.AddMinutes(eventTimespan + rand.Next(0, eventRandomTime));
                        startTime = systemTime;
                        runTime = systemTime;
                        eventActive = 1;

                        // Update database
                        FireController.updateFireObject(this);

                        // Set flags
                        running = true;
                        lastGrown = systemTime;
                        lastFire = systemTime;
                        fireCounter = 0;
                        exploCounter = 0;
                        percGrown = 0;
                        firstStrike = true;

                        // Clear fire- and firefighterlist
                        AllFires.Clear();
                        AllFirefighters.Clear();

                        Logger.logDebug(LogCategory.System, LogActionType.Created, $"Event {fireModel} fire ID:{Id} has started ({AllPlayers.Count} players in zone).");
                    }

                    // Event is running
                } else {

                    // Stop fires if all fires are extinguished
                    if(fireCounter > 0 && AllFires.Count == 0) {

                        // Remove area collision
                        if(CollisionShapeArea != null) {
                            CollisionShapeArea.Dispose();
                            CollisionShapeArea = null;
                        }

                        // Set data
                        eventDate = systemTime.AddMinutes(eventTimespan + rand.Next(0, eventRandomTime));
                        startTime = systemTime;
                        runTime = systemTime;
                        eventActive = 0;

                        // Update database
                        FireController.updateFireObject(this);

                        // Set flags
                        running = false;
                        start = false;
                        test = false;

                        // Clear fire- and firefighterlist
                        AllFires.Clear();
                        AllFirefighters.Clear();

                        Logger.logDebug(LogCategory.System, LogActionType.Removed, $"Event {fireModel} fire ID:{Id} stopped (extinguished).");
                        return;
                    }

                    // Stop fires if max burntime is reached or if test is running end fires after 5 minutes
                    if(DateTime.Compare(systemTime, startTime.AddMinutes(test ? 5 : eventMaxRunTime)) > 0) {

                        // Remove area collision
                        if(CollisionShapeArea != null) {
                            CollisionShapeArea.Dispose();
                            CollisionShapeArea = null;
                        }

                        // Set data
                        eventDate = systemTime.AddMinutes(eventTimespan + rand.Next(0, eventRandomTime));
                        startTime = systemTime;
                        runTime = systemTime;
                        eventActive = 0;

                        // Update database
                        FireController.updateFireObject(this);

                        // Set flags
                        running = false;
                        start = false;
                        test = false;

                        // Remove all fires on all clients
                        foreach(Fires fire in AllFires) {
                            foreach(IPlayer player in AllPlayers)
                                player.emitClientEvent("REMOVE_FIRE", fire.Id);
                        }

                        // Clear fire- and firefighterlist
                        AllFires.Clear();
                        AllFirefighters.Clear();

                        Logger.logDebug(LogCategory.System, LogActionType.Removed, $"Event {fireModel} fire ID:{Id} stopped (timeout).");
                        return;
                    }

                    // Slowly grow fires
                    if(fireCounter > 0 && DateTime.Compare(systemTime, lastGrown.AddSeconds(Constants.FIRE_GROW_SECONDS)) > 0) {
                        percGrown = 0;

                        foreach(Fires fire in AllFires) {
                            if(fire.Scale < fireSize) {
                                fire.Scale += (fireSize / 50);

                                lastGrown = systemTime;
                                percGrown += (int)((fire.Scale * 100) / fireSize);

                                // Only update fire on client if players are in range
                                foreach(IPlayer player in AllPlayers)
                                    player.emitClientEvent("UPDATE_FIRE", fire.Id, fire.Scale);

                            } else {
                                percGrown += 100;
                            }
                        }

                        percGrown = (percGrown / fireCount);
                    }

                    // Spawn fires as long firecount not reached
                    if(fireCounter < fireCount && DateTime.Compare(systemTime, lastFire.AddMinutes((test || fireCounter == 0) ? 0 : childDelay)) > 0) {
                        Position pos = colPosition;

                        // Random placement of child fires
                        if(fireCounter > 0) {
                            int spr = (childSpread * 100);

                            pos.X += (rand.Next(-spr, spr) / 100);
                            pos.Y += (rand.Next(-spr, spr) / 100);
                        }

                        // Get ground height or use corrected main level if in milo
                        var z = WorldController.getGroundHeightAt(pos.X, pos.Y);
                        if(z <= pos.Z + 5f)
                            pos.Z = z;
                        else
                            pos.Z -= 1f;

                        // Create new fire and add to firelist
                        Fires fire = new Fires(fireId++, Id, pos, colRotation, (test ? fireSize : fireSize / 10));
                        AllFires.Add(fire);

                        // Only create fire on client if players are in range
                        if(AllPlayers.Count > 0) {
                            foreach(IPlayer player in AllPlayers)
                                player.emitClientEvent("CREATE_FIRE", fire.Id, fire.colPosition.X, fire.colPosition.Y, fire.colPosition.Z, particleName, smokeName, fire.Scale, true);

                            Logger.logDebug(LogCategory.System, LogActionType.Created, $"Fire {fireCounter + 1}/{fireCount} for event {fireModel} fire ID:{Id} created.");

                            // Create explosion on client after probability check (only one explosion)
                            if(fireCounter > 0 && exploCounter == 0 && rand.Next(0, 100) <= explosionProbability) {
                                foreach(IPlayer player in AllPlayers)
                                    player.emitClientEvent("CREATE_EXPLOSION", 34, colPosition.X, colPosition.Y, colPosition.Z);

                                Logger.logDebug(LogCategory.System, LogActionType.Created, $"Explosion for event {fireModel} fire ID:{Id} created.");

                                exploCounter++;
                            }
                        }

                        // Update fire counter
                        lastFire = systemTime;
                        fireCounter++;
                    }

                    Logger.logDebug(LogCategory.System, LogActionType.Updated, $"Event {fireModel} fire ID:{Id} is in progress - {percGrown} % - {(int)systemTime.Subtract(runTime).TotalMinutes}/{eventMaxRunTime} min. ({AllPlayers.Count} players in zone).");
                }
            } catch(Exception e) {
                Logger.logException(e, "onInterval: Something went wrong");
            }
        }

        public virtual bool onExtinguishFire(IPlayer player, string eventName, object[] args) {
            try {
                if(running && AllFires.Count > 0 && args.Length > 2) {
                    int id = int.Parse(args[0].ToString());
                    string agent = args[2].ToString().ToLower();
                    Fires fire = AllFires.Find(r => r.Id == id);

                    if(fire != null) {
                        startTime = DateTime.Now;

                        if(!AllFirefighters.Contains(player))
                            AllFirefighters.Add(player);

                        // Fully grown all fires ands add explosion if extinguished with water
                        if(firstStrike && explosionType > 0 && agent == "water") {
                            firstStrike = false;

                            // Update all fires on all clients
                            foreach(Fires f in AllFires) {
                                foreach(IPlayer p in AllPlayers) {
                                    f.Scale = fireSize;
                                    p.emitClientEvent("UPDATE_FIRE", f.Id, f.Scale);
                                }
                            }

                            // Create explosion on all clients
                            foreach(IPlayer p in AllPlayers)
                                p.emitClientEvent("CREATE_EXPLOSION", explosionType, colPosition.X, colPosition.Y, colPosition.Z);

                            // Extinguish fire
                        } else {
                            fire.Scale = float.Parse(args[1].ToString());

                            // Update fire on all clients
                            foreach(IPlayer p in AllPlayers)
                                p.emitClientEvent("UPDATE_FIRE", fire.Id, fire.Scale);
                        }

                        if(DateTime.Compare(startTime, lastNotice.AddSeconds(3)) > 0) {
                            foreach(IPlayer p in AllFirefighters)
                                p.sendNotification(Constants.NotifactionTypes.Info, "Feuer wird gelöscht.", "Feuer löschen", Constants.NotifactionImages.Fire, "FIRE_EXTINGUISH");

                            lastNotice = DateTime.Now;
                        }
                    }
                }
            } catch(Exception e) {
                Logger.logException(e, "extinguishFire: Something went wrong");
            }

            return true;
        }

        public virtual bool onExtinguishedFire(IPlayer player, string eventName, object[] args) {
            try {
                if(running && AllFires.Count > 0 && args.Length > 0) {
                    int id = int.Parse(args[0].ToString());
                    Fires fire = AllFires.Find(r => r.Id == id);

                    if(fire != null) {
                        startTime = DateTime.Now;

                        // Remove fire from firelist
                        AllFires.Remove(fire);

                        // Remove fire on all clients
                        foreach(IPlayer p in AllPlayers)
                            p.emitClientEvent("REMOVE_FIRE", id);

                        foreach(IPlayer p in AllFirefighters) {
                            if(AllFires.Count > 0)
                                p.sendNotification(Constants.NotifactionTypes.Info, $"Ein Feuer wurde gelöscht. Noch {AllFires.Count} Feuer zu löschen!", "Ein Feuer gelöscht", Constants.NotifactionImages.Fire);
                            else
                                p.sendNotification(Constants.NotifactionTypes.Info, "Feuer wurde vollständig gelöscht.", "Alle Feuer gelöscht", Constants.NotifactionImages.Fire);
                        }
                    }
                }
            } catch(Exception e) {
                Logger.logException(e, "extinguishedFire: Something went wrong");
            }

            return true;
        }
    }
}
