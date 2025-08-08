namespace ChoiceVServer.Controller {
    //public class EmergencyCallElement {
    //    public Constants.EmergencyCallType CallType { get; set; }
    //    public Constants.EmergencyCallSeverity Severity { get; set; }
    //    public DateTime Time { get; set; }
    //    public Position Position { get; set; }
    //    public int CharId { get; set; }
    //    public string CharName { get; set; }
    //    public bool IsAnonymous { get; set; }
    //    public string Message { get; set; }
    //    public bool Ended { get; set; }

    //    public bool IsPersisted { get; set; }
    //    public long Id { get; set; }

    //    public EmergencyCallElement(emergencycallelements element) {
    //        CallType = (Constants.EmergencyCallType)element.calltype;
    //        Severity = (Constants.EmergencyCallSeverity)element.severity;
    //        Time = element.time;
    //        Position = element.position.FromJson();
    //        CharId = element.charid;
    //        CharName = element.charname;
    //        IsAnonymous = element.isanonymous != 0;
    //        Message = element.message;
    //        Ended = element.ended != 0;
    //        Id = element.id;
    //    }

    //    public EmergencyCallElement() { }
    //}

    //class EmergencyCallCenterElementEvent {
    //    public EmergencyCallCenterElementEvent(configemergencycallcenterevents row) {
    //        Name = row.name;
    //        Description = row.description;
    //        RightInfo = row.rightinfo;
    //        Event = row._event;
    //    }

    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    public string RightInfo { get; set; }
    //    public string Event { get; set; }
    //}

    //public class TrackedVehicle {
    //    public ChoiceVVehicle vehicle { get; set; }
    //    public Constants.EmergencyCallType callType { get; set; }
    //    public Position? oldBlipPos { get; set; }
    //    public string DisplayName { get; set; }
    //}

    //public class EmergencyControlCentreController : ChoiceVScript {
    //    public static readonly List<EmergencyCallElement> ActiveEmergencyCallElements = new List<EmergencyCallElement>();
    //    public static readonly Dictionary<Company, List<Constants.EmergencyCallType>> EmergencyCallTypeToCompaniesList = new Dictionary<Company, List<Constants.EmergencyCallType>>();
    //    public static readonly IDictionary<Constants.EmergencyCallType, List<IPlayer>> EmergencyCallCenterWorkerList = new Dictionary<Constants.EmergencyCallType, List<IPlayer>>();
    //    public static readonly IDictionary<Constants.EmergencyCallType, List<IPlayer>> EmergencyBlipReceiverList = new ConcurrentDictionary<Constants.EmergencyCallType, List<IPlayer>>();
    //    public static readonly List<TrackedVehicle> AllTrackedVehicles = new List<TrackedVehicle>();
    //    public static readonly List<IPlayer> ActiveTrackingPlayer = new List<IPlayer>();

    //    private static int _blipAlpha = 255;

    //    private static readonly List<int> PersistedCallCenterWorkerList = new List<int>();
    //    private static readonly List<EmergencyCallCenterElementEvent> EmergencyCallCenterElementEvents = new List<EmergencyCallCenterElementEvent>();

    //    private static readonly TimeSpan SpamTimeSlot = TimeSpan.FromSeconds(10);
    //    private static readonly TimeSpan EmergencyCallEndTimeSlot = TimeSpan.FromMinutes(15);

    //    public EmergencyControlCentreController() {
    //        try {
    //            EmergencyBlipReceiverList.Clear();
    //            foreach (Constants.EmergencyCallType emergencyCallType in (Constants.EmergencyCallType[])
    //                Enum.GetValues(typeof(Constants.EmergencyCallType))) {
    //                EmergencyBlipReceiverList.Add(emergencyCallType, new List<IPlayer>());
    //            }

    //            EmergencyCallCenterWorkerList.Clear();
    //            foreach (Constants.EmergencyCallType emergencyCallType in (Constants.EmergencyCallType[])
    //                Enum.GetValues(typeof(Constants.EmergencyCallType))) {
    //                EmergencyCallCenterWorkerList.Add(emergencyCallType, new List<IPlayer>());
    //            }

    //            InvokeController.AddTimedInvoke("EmergencyControlCentreController_Persisting", persistEmergencyCalls, TimeSpan.FromSeconds(1), true);
    //            InvokeController.AddTimedInvoke("EmergencyControlCentreController_CheckTimeOut", checkEmergencyCalls, TimeSpan.FromSeconds(10), true);
    //            InvokeController.AddTimedInvoke("EmergencyControlCentreController_TrackVehicles", updateVehicleTracking, TimeSpan.FromSeconds(10), true);

    //            EventController.addMenuEvent("EMERGENCY_CALL_MENUEVENT_SET", onSetEmergencyCallsMenu);
    //            EventController.addCefEvent("EMERGENCY_CALL_CEF_SET", onSetEmergencyCallsCef);
    //            EventController.addCollisionShapeEvent("EMERGENCY_CALL_COLSHAPE_SET", onSetEmergencyCallsCollisionShape);
    //            EventController.addEvent("EMERGENCY_CALL_EVENT_SET", onSetEmergencyCallsEvent);
    //            EventController.addMenuEvent("EMERGENCY_CALL_MENUEVENT_GETLAST", onGetLastEmergencyCallsMenu);
    //            EventController.addCefEvent("EMERGENCY_CALL_CEF_GETLAST", onGetLastEmergencyCallsCef);
    //            EventController.addCollisionShapeEvent("EMERGENCY_CALL_COLSHAPE_GETLAST", onGetLastEmergencyCallsCollisionShape);
    //            EventController.addEvent("EMERGENCY_CALL_EVENT_GETLAST", onGetLastEmergencyCallsEvent);

    //            EventController.addMenuEvent("EMERGENCY_CALL_MENUEVENT_SET_ANONYMOUS", onSetEmergencyCallsMenuAnonymous);
    //            EventController.addCefEvent("EMERGENCY_CALL_CEF_SET_ANONYMOUS", onSetEmergencyCallsCefAnonymous);
    //            EventController.addCollisionShapeEvent("EMERGENCY_CALL_COLSHAPE_SET_ANONYMOUS", onSetEmergencyCallsCollisionShapeAnonymous);
    //            EventController.addEvent("EMERGENCY_CALL_EVENT_SET_ANONYMOUS", onSetEmergencyCallsEventAnonymous);

    //            EventController.addMenuEvent("EMERGENCY_CALL_CENTER_REGISTER_FOR_CENTER", onSetEmergencyCallsCenterRegisterForCenter);
    //            EventController.addMenuEvent("EMERGENCY_CALL_CENTER_DEREGISTER_FOR_CENTER", onSetEmergencyCallsCenterDeregisterForCenter);

    //            EventController.addMenuEvent("EMERGENCY_CALL_CENTER_MENU_SHOW_MENU", onEmergencyCallCenterMenuShowMenu);
    //            EventController.addCollisionShapeEvent("EMERGENCY_CALL_COLSHAPE_SHOW_MENU", onEmergencyCallCenterColShapeShowMenu);

    //            EventController.addMenuEvent("EMERGENCY_CALL_CENTER_MENU_SHOW_MANAGE_MENU", onEmergencyCallCenterMenuShowManageMenu);
    //            EventController.addCollisionShapeEvent("EMERGENCY_CALL_COLSHAPE_SHOW_MANAGE_MENU", onEmergencyCallCenterColShapeShowManageMenu);

    //            EventController.addMenuEvent("EMERGENCY_CALL_CENTER_CLOSE_CALL", onEmergencyCallCenterCloseCall);
    //            EventController.addMenuEvent("ECC_TOGGLE_SHOW_VEHICLE_TRACKING", onShowVehicleTracking);

    //            EventController.addMenuEvent("EMERGENCY_VEHICLE_SET_NAME", onSetDisplayName);

    //            EventController.addKeyEvent(ConsoleKey.F5, onEmergencyCallCenterKeyShowManageMenu);

    //            //registerPlayerForCallCentre

    //            EventController.PlayerSuccessfullConnectionDelegate += loadCharacter;
    //            EventController.PlayerDisconnectedDelegate += unloadCharacter;
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }

    //    private bool onSetDisplayName(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        var vehicle = (ChoiceVVehicle)data["Vehicle"];
    //        InputMenuItem.InputMenuItemEvent inputItem = menuitemcefevent as InputMenuItem.InputMenuItemEvent;
    //        if (inputItem == null) { return false; }
    //        if (string.IsNullOrEmpty(inputItem.input)) {
    //            player.sendNotification(Constants.NotifactionTypes.Warning, "Der Input ist nicht korrekt", "Falscher Input");
    //            return true;
    //        }
    //        vehicle.setData("VehicleDisplayName", inputItem.input);
    //        return true;
    //    }

    //    private bool onShowVehicleTracking(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        lock (ActiveTrackingPlayer) {
    //            if (ActiveTrackingPlayer.Contains(player)) {
    //                ActiveTrackingPlayer.Remove(player);
    //                foreach (TrackedVehicle allTrackedVehicle in AllTrackedVehicles) {
    //                    destroyTrackingBlip(player, allTrackedVehicle);
    //                }
    //                player.sendNotification(Constants.NotifactionTypes.Success,"Fahrzeugtracking ausgeschaltet.", "Fahrzeugtracking aus.");
    //            } else {
    //                ActiveTrackingPlayer.Add(player);
    //                foreach (TrackedVehicle allTrackedVehicle in AllTrackedVehicles) {
    //                    createTrackingBlip(player, allTrackedVehicle);
    //                }
    //                player.sendNotification(Constants.NotifactionTypes.Success, "Fahrzeugtracking eingeschaltet.", "Fahrzeugtracking ein.");
    //            }
    //        }

    //        return true;
    //    }

    //    private static Position generateVehicleBlipPosition(ChoiceVVehicle vehicle) {
    //        return new Position(Convert.ToSingle(Math.Round(vehicle.Position.X, 2)), Convert.ToSingle(Math.Round(vehicle.Position.Y, 2)), Convert.ToSingle(Math.Round(vehicle.Position.Z, 2)));
    //    }

    //    private static void createTrackingBlip(IPlayer player, TrackedVehicle trackedVehicle) {
    //        ensureDisplayName(trackedVehicle);
    //        if (trackedVehicle.oldBlipPos != null)
    //            BlipController.createPointBlip(player, trackedVehicle.DisplayName, trackedVehicle.oldBlipPos.Value, 1, getTrackingSpriteToCallType(trackedVehicle.callType), 255, trackedVehicle.DisplayName);
    //    }

    //    private static int getTrackingSpriteToCallType(Constants.EmergencyCallType trackedVehicleCallType, ChoiceVVehicle vehicle = null)
    //    {
    //        int vehicleClass = vehicle.getClass();
    //        if (vehicleClass == 15) {
    //            return (int)Constants.Blips.Helicopter;
    //        }
    //        if (vehicleClass == 16) {
    //            return (int)Constants.Blips.Airplane;
    //        }
    //        if (vehicleClass == 19) {
    //            return (int)Constants.Blips.AirplaneJet;
    //        }

    //        switch (trackedVehicleCallType)
    //        {
    //            case Constants.EmergencyCallType.Police:
    //                return (int)Constants.Blips.PoliceCar;
    //            case Constants.EmergencyCallType.Medic:
    //                return (int)Constants.Blips.Ambulance;
    //            case Constants.EmergencyCallType.Fire:
    //                return (int)Constants.Blips.Firetruck;
    //            case Constants.EmergencyCallType.Towing:
    //                return (int)Constants.Blips.TowTruck;
    //            case Constants.EmergencyCallType.FBI:
    //                return (int)Constants.Blips.PoliceCar;
    //            case Constants.EmergencyCallType.Purge:
    //                return (int)Constants.Blips.PurgeCar;
    //            default:
    //                return (int)Constants.Blips.A;
    //        }
    //    }

    //    private static void destroyTrackingBlip(IPlayer player, TrackedVehicle trackedVehicle) {
    //        if (trackedVehicle.oldBlipPos != null) BlipController.destroyBlipByName(player, trackedVehicle.DisplayName);
    //    }

    //    private void updateVehicleTracking(IInvoke obj) {
    //        try {

    //            lock (ActiveTrackingPlayer) {
    //                ActiveTrackingPlayer.RemoveAll(e => !e.IsConnected);
    //                IDictionary<uint, Constants.EmergencyCallType> vehicleModels = VehicleController.AllVehicleModels.Where(m => m.EmergencyType != -1).ToDictionary(m => m.Model, m => (Constants.EmergencyCallType)m.EmergencyType);
    //                List<ChoiceVVehicle> vehicles = ChoiceVAPI.GetAllVehicles().Where(v => vehicleModels.ContainsKey(v.Model)).ToList();


    //                foreach (IPlayer player in ActiveTrackingPlayer) {
    //                    foreach (TrackedVehicle allTrackedVehicle in AllTrackedVehicles) {
    //                        destroyTrackingBlip(player, allTrackedVehicle);
    //                    }
    //                }

    //                AllTrackedVehicles.Clear();
    //                AllTrackedVehicles.AddRange(vehicles.Select(v => new TrackedVehicle() { callType = vehicleModels[v.Model], oldBlipPos = generateVehicleBlipPosition(v), DisplayName = null, vehicle = v }));

    //                foreach (IPlayer player in ActiveTrackingPlayer) {
    //                    foreach (TrackedVehicle allTrackedVehicle in AllTrackedVehicles) {
    //                        createTrackingBlip(player, allTrackedVehicle);
    //                    }
    //                }
    //            }
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }

    //    private static void ensureDisplayName(TrackedVehicle allTrackedVehicle) {
    //        try {
    //            if (string.IsNullOrEmpty(allTrackedVehicle.DisplayName)) {
    //                if (allTrackedVehicle.vehicle.hasData("VehicleDisplayName")) {
    //                    allTrackedVehicle.DisplayName = allTrackedVehicle.vehicle.getData("VehicleDisplayName");
    //                } else {
    //                    allTrackedVehicle.DisplayName = allTrackedVehicle.vehicle.NumberplateText;
    //                }
    //            }
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }


    //    private bool onEmergencyCallCenterCloseCall(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        if (!data.ContainsKey("CallElement")) {
    //            return false;
    //        }

    //        EmergencyCallElement emergencyCallElement = ActiveEmergencyCallElements.FirstOrDefault(e => e.Id == (long)data["CallElement"]);

    //        if (emergencyCallElement == null || emergencyCallElement.Ended) {
    //            return false;
    //        }

    //        emergencyCallElement.Ended = true;
    //        emergencyCallElement.IsPersisted = false;

    //        return true;
    //    }

    //    private bool onEmergencyCallCenterKeyShowManageMenu(IPlayer player, ConsoleKey key, string eventname) {
    //        doShowCallCenterMenu(player);
    //        return true;
    //    }

    //    private bool onSetEmergencyCallsCenterDeregisterForCenter(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        if (deregisterPlayerForCallCentre(player)) {
    //            player.sendNotification(Constants.NotifactionTypes.Success,
    //                "Du wurdest von der Leitstelle abgemeldet.", "Abgemeldet");
    //            return true;
    //        }
    //        player.sendNotification(Constants.NotifactionTypes.Warning,
    //            "Abmeldung von der Leitstelle nicht möglich.", "Abmeldung fehlgeschlagen");
    //        return false;
    //    }

    //    private bool onEmergencyCallCenterColShapeShowManageMenu(IPlayer player, CollisionShape collisionshape, Dictionary<string, object> data) {
    //        doShowCallCenterMenu(player);
    //        return true;
    //    }

    //    private bool onEmergencyCallCenterMenuShowManageMenu(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        doShowCallCenterMenu(player);
    //        return true;
    //    }

    //    private bool onEmergencyCallCenterColShapeShowMenu(IPlayer player, CollisionShape collisionshape, Dictionary<string, object> data) {
    //        doShowManagementMenu(player);
    //        return true;
    //    }

    //    private bool onEmergencyCallCenterMenuShowMenu(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        doShowManagementMenu(player);
    //        return true;
    //    }

    //    public static void doShowManagementMenu(IPlayer player) {
    //        int characterId = player.getCharacterId();
    //        Menu menu = new Menu("Los Santos Leitstelle", "Leitstellensystem V. 1.0");
    //        if (PersistedCallCenterWorkerList.Contains(characterId)) {
    //            menu.addMenuItem(new ClickMenuItem("An Leitstelle abmelden.", "Melde dich von der Leitstelle ab (Und gebe das Leitstellentelefon ab.)", string.Empty, "EMERGENCY_CALL_CENTER_DEREGISTER_FOR_CENTER", MenuItemStyle.green));
    //        } else if (getCallTypesForPlayer(player).Any()) {
    //            menu.addMenuItem(new ClickMenuItem("An Leitstelle anmelden.", "Melde dich an der Leitstelle an und bekomme das Leitstellentelefon.", string.Empty, "EMERGENCY_CALL_CENTER_REGISTER_FOR_CENTER", MenuItemStyle.green));
    //        } else {
    //            player.sendNotification(Constants.NotifactionTypes.Warning, "Du bist nicht berechtigt dich bei der Leitstelle anzumelden.", "Nicht berechtigt.");
    //        }
    //        player.showMenu(menu);
    //    }

    //    public static void doShowCallCenterMenu(IPlayer player) {
    //        int characterId = player.getCharacterId();
    //        if (PersistedCallCenterWorkerList.Contains(characterId)) {
    //            Menu menu = new Menu("Los Santos Leitstelle", "Die letzten 20 Notrufe der letzten 10 Minuten.");
    //            persistEmergencyCalls(null);
    //            List<EmergencyCallElement> emergencyCallElements = ActiveEmergencyCallElements.OrderBy(a => a.Time).Take(20).ToList();
    //            foreach (EmergencyCallElement emergencyCallElement in emergencyCallElements) {
    //                string callElementText = $"{getEmergencyHeaderBySeverity(emergencyCallElement.Severity)} {getBlipMessageToEmergencyCallElement(emergencyCallElement)}";
    //                menu.addMenuItem(new MenuMenuItem(callElementText,
    //                    createOptionMenuToEmergencyCallElement(emergencyCallElement, callElementText),
    //                    MenuItemStyle.green));
    //            }
    //            menu.addMenuItem(new ClickMenuItem("Fahrzeugtracking", "Schaltet das tracking von Dienstfahrzeugen ein oder aus", string.Empty, "ECC_TOGGLE_SHOW_VEHICLE_TRACKING", MenuItemStyle.green));

    //            if (!emergencyCallElements.Any()) {
    //                player.sendNotification(Constants.NotifactionTypes.Info, "Aktuell gibt es keine Notrufe zur Anzeige.", "Keine Notrufe.");
    //            }
    //            player.showMenu(menu);
    //        } else {
    //            if (getCallTypesForPlayer(player).Any()) {
    //                player.sendNotification(Constants.NotifactionTypes.Warning, "Du bist nicht an der Leitstelle angemeldet.", "Nicht angemeldet.");
    //            }
    //        }
    //    }

    //    private static string getEmergencyHeaderBySeverity(Constants.EmergencyCallSeverity callSeverity) {
    //        switch (callSeverity) {
    //            case Constants.EmergencyCallSeverity.Automatic:
    //                return "[AUT]";
    //            case Constants.EmergencyCallSeverity.Low:
    //                return "[---]";
    //            case Constants.EmergencyCallSeverity.Medium:
    //                return "[!!-]";
    //            case Constants.EmergencyCallSeverity.High:
    //                return "[!!!]";
    //            case Constants.EmergencyCallSeverity.Critical:
    //                return "<<!!!>>";
    //            default:
    //                return "[---]";
    //        }
    //    }

    //    private static Menu createOptionMenuToEmergencyCallElement(EmergencyCallElement emergencyCallElement, string callElementText) {
    //        Menu menu = new Menu(callElementText, emergencyCallElement.Message);
    //        menu.addMenuItem(new ClickMenuItem("Ticket schließen", "Beendet das Notrufticket.", string.Empty, "EMERGENCY_CALL_CENTER_CLOSE_CALL").withData(getEmergencyTicketData(emergencyCallElement)));
    //        foreach (EmergencyCallCenterElementEvent emergencyCallCenterElementEvent in EmergencyCallCenterElementEvents) {
    //            menu.addMenuItem(new ClickMenuItem(emergencyCallCenterElementEvent.Name, emergencyCallCenterElementEvent.Description, emergencyCallCenterElementEvent.RightInfo, emergencyCallCenterElementEvent.Event, MenuItemStyle.green));
    //        }
    //        return menu;
    //    }

    //    private static Dictionary<string, dynamic> getEmergencyTicketData(EmergencyCallElement emergencyCallElement) {
    //        return new Dictionary<string, dynamic>() { { "CallElement", emergencyCallElement.Id } };
    //    }

    //    private bool onSetEmergencyCallsCenterRegisterForCenter(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        if (registerPlayerForCallCentre(player)) {
    //            player.sendNotification(Constants.NotifactionTypes.Success,
    //                "Du bist in der Leitstelle angemeldet.", "Angemeldet");
    //            return true;
    //        }
    //        player.sendNotification(Constants.NotifactionTypes.Warning,
    //            "Anmeldung an Leitstelle nicht möglich.", "Anmeldung fehlgeschlagen");
    //        return false;
    //    }

    //    private bool onSetEmergencyCallsCenterRegisterForBlips(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        if (registerPlayerForBlips(player)) {
    //            player.sendNotification(Constants.NotifactionTypes.Success,
    //                "Dir werden nun Notrufe auf der Karte angezeigt.", "Notruf-Anzeige An");
    //            return true;
    //        }
    //        player.sendNotification(Constants.NotifactionTypes.Warning,
    //            "Notrufanzeige auf Karte nicht möglich.", "Notruf-Anzeige fehlgeschlagen");
    //        return false;
    //    }

    //    public static void initEmergencyCallsFromDB() {
    //        using (var db = new ChoiceVDb()) {
    //            EmergencyCallTypeToCompaniesList.Clear();
    //            ActiveEmergencyCallElements.Clear();
    //            PersistedCallCenterWorkerList.Clear();
    //            EmergencyCallCenterElementEvents.Clear();

    //            List<emergencycontrolcentreworkers> emergencyControlCentreWorkers = db.emergencycontrolcentreworkers.ToList();
    //            PersistedCallCenterWorkerList.AddRange(emergencyControlCentreWorkers.Select(e => e.charid).ToList());

    //            List<emergencycallelements> emergencyCallElements = db.emergencycallelements.Where(e => e.ended == 0).ToList();
    //            ActiveEmergencyCallElements.AddRange(emergencyCallElements.Select(e => new EmergencyCallElement(e)).ToList());

    //            List<configemergencycalltypetocompanies> configEmergencyCallTypeToCompanies = db.configemergencycalltypetocompanies.ToList();
    //            foreach (configemergencycalltypetocompanies element in configEmergencyCallTypeToCompanies) {
    //                if (EmergencyCallTypeToCompaniesList.Keys.All(c => c.Id != element.companyid)) {
    //                    EmergencyCallTypeToCompaniesList.Add(CompanyController.AllCompanies.First(a => a.Id == element.companyid), new List<Constants.EmergencyCallType>());
    //                }
    //                EmergencyCallTypeToCompaniesList.First(c => c.Key.Id == element.companyid).Value.Add((Constants.EmergencyCallType)element.calltype);
    //            }

    //            List<configemergencycallcenterevents> rows = db.configemergencycallcenterevents.ToList();

    //            EmergencyCallCenterElementEvents.AddRange(rows.Select(r => new EmergencyCallCenterElementEvent(r)).ToList());

    //            //configemergencycallcentersetting alphaSetting = db.configemergencycallcentersetting.FirstOrDefault(s => s.name == "ALPHA");
    //            //if (alphaSetting != null) {
    //            //    _blipAlpha = int.Parse(alphaSetting.value);
    //            //}
    //        }
    //    }

    //    private void checkEmergencyCalls(IInvoke obj) {
    //        List<EmergencyCallElement> emergencyCallElements = ActiveEmergencyCallElements.Where(e => e.Time + EmergencyCallEndTimeSlot < DateTime.Now).ToList();

    //        if (emergencyCallElements.Any()) {
    //            foreach (var callElement in emergencyCallElements) {
    //                callElement.Ended = true;
    //                callElement.IsPersisted = false;
    //            }
    //        }
    //    }

    //    private static void persistEmergencyCalls(IInvoke obj) {
    //        if (ActiveEmergencyCallElements.Any(e => !e.IsPersisted)) {
    //            List<EmergencyCallElement> emergencyCallElements = ActiveEmergencyCallElements.Where(e => !e.IsPersisted).ToList();
    //            using (var db = new ChoiceVDb()) {
    //                foreach (EmergencyCallElement emergencyCallElement in emergencyCallElements) {
    //                    if (emergencyCallElement.Id == -1) {
    //                        emergencycallelements callElement = new emergencycallelements {
    //                            calltype = (int)emergencyCallElement.CallType,
    //                            position = emergencyCallElement.Position.ToJson(),
    //                            charid = emergencyCallElement.CharId,
    //                            charname = emergencyCallElement.CharName,
    //                            message = emergencyCallElement.Message,
    //                            severity = (int)emergencyCallElement.Severity,
    //                            time = emergencyCallElement.Time,
    //                            isanonymous = emergencyCallElement.IsAnonymous ? 1 : 0,
    //                            ended = emergencyCallElement.Ended ? 1 : 0
    //                        };
    //                        db.Add(callElement);
    //                        db.SaveChanges();
    //                        emergencyCallElement.Id = callElement.id;
    //                        emergencyCallElement.IsPersisted = true;
    //                    }

    //                    emergencycallelements existingCallElement = db.emergencycallelements.FirstOrDefault(e => e.id == emergencyCallElement.Id);
    //                    if (existingCallElement != null) {
    //                        existingCallElement.ended = emergencyCallElement.Ended ? 1 : 0;
    //                        existingCallElement.position = emergencyCallElement.Position.ToJson();
    //                        existingCallElement.message = emergencyCallElement.Message;
    //                        existingCallElement.severity = (int)emergencyCallElement.Severity;
    //                        existingCallElement.charid = emergencyCallElement.CharId;
    //                        existingCallElement.time = emergencyCallElement.Time;
    //                        existingCallElement.charname = emergencyCallElement.CharName;
    //                        existingCallElement.isanonymous = emergencyCallElement.IsAnonymous ? 1 : 0;
    //                        existingCallElement.calltype = (int)emergencyCallElement.CallType;
    //                        emergencyCallElement.IsPersisted = true;
    //                    }

    //                    if (emergencyCallElement.Ended) {
    //                        doSyncEmergencyCallElement(emergencyCallElement);
    //                        ActiveEmergencyCallElements.Remove(emergencyCallElement);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private void loadCharacter(IPlayer player, characters character) {
    //        int characterId = player.getCharacterId();
    //        if (PersistedCallCenterWorkerList.Contains(characterId)) {
    //            deregisterPlayerForCallCentre(player);
    //        }
    //    }

    //    public static void addCenterPhoneToPlayer(IPlayer player) {
    //        Inventory inventory = player.getInventory();
    //        var sim = InventoryController.AllConfigItems.FirstOrDefault(c => c.codeItem == typeof(SIMCard).Name);
    //        SIMCard simCard = new SIMCard(sim, 911);
    //        simCard.Description = "SIM-Karte der Notruf-Leitstelle";
    //        simCard.saveDescription();
    //        inventory.addItem(simCard);

    //        player.sendNotification(Constants.NotifactionTypes.Success,
    //            "Du hast die SIM-Karte für die Leitstelle bekommen. Bitte diese nun ausrüsten.",
    //            "Leitstellensim erhalten.");
    //    }

    //    private static bool removeCenterPhoneFromPlayer(IPlayer player) {
    //        try {
    //            Inventory inventory = player.getInventory();
    //            List<Smartphone> smartphones = inventory.getAllItems().OfType<Smartphone>().ToList();

    //            bool removedCard = false;
    //            foreach (Smartphone smartphone in smartphones) {
    //                List<SIMCard> cards = smartphone.SIMCardInventory.getAllItems().OfType<SIMCard>().Where(s => s.Number == 911).ToList();
    //                foreach (SIMCard card in cards) {
    //                    if (!smartphone.SIMCardInventory.moveItem(inventory, card)) {
    //                        player.sendNotification(Constants.NotifactionTypes.Warning,
    //                            $"Nicht genügend Platz im Inventar für {card.Name} {card.Number}.", "Abmeldung fehlgeschlagen.");
    //                        return false;
    //                    }
    //                    smartphone.updateDescription();
    //                    inventory.removeItem(card);
    //                    removedCard = true;
    //                }
    //            }
    //            List<SIMCard> cards2 = inventory.getAllItems().OfType<SIMCard>().Where(s => s.Number == 911).ToList();
    //            foreach (SIMCard card in cards2) {
    //                inventory.removeItem(card);
    //                removedCard = true;
    //            }

    //            if (removedCard) {

    //                return true;
    //            }

    //            player.sendNotification(Constants.NotifactionTypes.Warning,
    //                "Um dich von der Leitstelle abmelden zu können musst du die Leitstellen-Sim im Inventar haben.",
    //                "Abmeldung fehlgeschlagen.");
    //            return false;

    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    //private bool onCallCenterForceEndCall(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, object> data, MenuItemCefEvent menuItemCefEvent) {
    //    //    try {
    //    //        int characterId = player.getCharacterId();
    //    //        if (PersistedCallCenterWorkerList.Contains(characterId)) {
    //    //            deregisterPlayerForCallCentre(player);
    //    //        }

    //    //        return true;
    //    //    } catch (Exception e) {
    //    //        Logger.logException(e);
    //    //        return false;
    //    //    }
    //    //}

    //    private void unloadCharacter(IPlayer player, string reason) {
    //        try {
    //            int characterId = player.getCharacterId();
    //            if (PersistedCallCenterWorkerList.Contains(characterId)) {
    //                deregisterPlayerForCallCentre(player);
    //            }
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }

    //    private static void syncBlipsToPlayer(IPlayer player, Constants.EmergencyCallType emergencyCallType) {
    //        try {
    //            List<EmergencyCallElement> emergencyCallElements = ActiveEmergencyCallElements
    //                .Where(e => e.CallType == emergencyCallType && !e.Ended).ToList();
    //            foreach (EmergencyCallElement emergencyCallElement in emergencyCallElements) {
    //                string blipMessage = getBlipMessageToEmergencyCallElement(emergencyCallElement);
    //                int sprite = getSpriteToEmergencyCallType(emergencyCallElement.CallType);
    //                BlipController.createPointBlip(player, blipMessage, emergencyCallElement.Position, getBlipColorBySeverity(emergencyCallElement.Severity), sprite, _blipAlpha, emergencyCallElement.Id.ToString());
    //            }
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }

    //    private static void removeAllPlayerBlips(IPlayer player) {
    //        try {
    //            List<Constants.EmergencyCallType> emergencyCallTypes = EmergencyBlipReceiverList
    //                .Where(l => l.Value.Contains(player)).Select(l => l.Key).ToList();
    //            List<EmergencyCallElement> emergencyCallElements = ActiveEmergencyCallElements
    //                .Where(e => emergencyCallTypes.Contains(e.CallType) && !e.Ended).ToList();
    //            foreach (EmergencyCallElement activeEmergencyCallElement in emergencyCallElements) {
    //                BlipController.destroyBlipByName(player, activeEmergencyCallElement.Id.ToString());
    //            }
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }

    //    public static bool deregisterPlayerForCallCentre(IPlayer player, bool overwriteRemove = false) {
    //        try {
    //            if (!removeCenterPhoneFromPlayer(player) && !overwriteRemove) {
    //                return false;
    //            }

    //            foreach (Constants.EmergencyCallType callType in EmergencyCallCenterWorkerList.Keys) {
    //                if (EmergencyCallCenterWorkerList[callType].Contains(player)) {
    //                    EmergencyCallCenterWorkerList[callType].Remove(player);
    //                }
    //            }

    //            int characterId = player.getCharacterId();
    //            using (var db = new ChoiceVDb()) {
    //                emergencycontrolcentreworkers firstOrDefault =
    //                    db.emergencycontrolcentreworkers.FirstOrDefault(e => e.charid == characterId);
    //                if (firstOrDefault != null) {
    //                    db.Remove(firstOrDefault);
    //                    db.SaveChanges();
    //                }
    //            }

    //            if (PersistedCallCenterWorkerList.Contains(characterId)) {
    //                PersistedCallCenterWorkerList.Remove(characterId);
    //            }

    //            return true;
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    public static bool registerPlayerForCallCentre(IPlayer player) {
    //        try {
    //            int characterId = player.getCharacterId();

    //            if (PersistedCallCenterWorkerList.Contains(characterId)) {
    //                player.sendNotification(Constants.NotifactionTypes.Info, "Du bist bereits an der Leitstelle angemeldet.", "Bereits angemeldet.");
    //                return false;
    //            }

    //            bool retval = false;
    //            List<Constants.EmergencyCallType> callTypes = getCallTypesForPlayer(player);
    //            foreach (Constants.EmergencyCallType callType in callTypes) {
    //                if (!EmergencyCallCenterWorkerList[callType].Contains(player)) {
    //                    EmergencyCallCenterWorkerList[callType].Add(player);
    //                }
    //            }

    //            if (callTypes.Any()) {
    //                addCenterPhoneToPlayer(player);
    //            }

    //            using (var db = new ChoiceVDb()) {
    //                if (!db.emergencycontrolcentreworkers.Any(e => e.charid == characterId)) {
    //                    emergencycontrolcentreworkers worker = new emergencycontrolcentreworkers { charid = characterId };
    //                    db.Add(worker);
    //                    db.SaveChanges();
    //                }
    //            }

    //            if (!PersistedCallCenterWorkerList.Contains(characterId)) {
    //                PersistedCallCenterWorkerList.Add(characterId);
    //                retval = true;
    //            }

    //            return retval;
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    public static void deregisterPlayerForBlips(IPlayer player) {
    //        try {
    //            removeAllPlayerBlips(player);

    //            foreach (List<IPlayer> players in EmergencyBlipReceiverList.Values) {
    //                if (players.Contains(player)) {
    //                    players.Remove(player);
    //                }
    //            }

    //            registerPlayerForBlips(player);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }

    //    public static bool registerPlayerForBlips(IPlayer player) {
    //        try {
    //            bool retval = false;
    //            List<Constants.EmergencyCallType> callTypes = getCallTypesForPlayer(player);
    //            foreach (Constants.EmergencyCallType emergencyCallType in callTypes) {
    //                if (!EmergencyBlipReceiverList[emergencyCallType].Contains(player)) {
    //                    retval = true;
    //                    EmergencyBlipReceiverList[emergencyCallType].Add(player);
    //                    syncBlipsToPlayer(player, emergencyCallType);
    //                }
    //            }

    //            return retval;
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private static List<Constants.EmergencyCallType> getCallTypesForPlayer(IPlayer player) {
    //        try {

    //            if (player.getCharacterData().AdminMode) {
    //                return EmergencyBlipReceiverList.Keys.ToList();
    //            }

    //            int characterId = player.getCharacterId();
    //            List<Constants.EmergencyCallType> retval = new List<Constants.EmergencyCallType>();
    //            foreach (Company company in EmergencyCallTypeToCompaniesList.Keys) {
    //                if (company.hasEmployee(characterId) && company.findEmployee(characterId).InDuty) {
    //                    retval.AddRange(EmergencyCallTypeToCompaniesList[company]);
    //                }
    //            }

    //            return retval.Distinct().ToList();
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return new List<Constants.EmergencyCallType>();
    //        }
    //    }

    //    private bool onSetEmergencyCallsEventAnonymous(IPlayer player, string eventname, object[] args) {
    //        try {
    //            return addNewEmergencyCallWithArgs(player, args, true);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private static bool addNewEmergencyCallWithArgs(IPlayer player, object[] args, bool anonymous = false) {
    //        try {
    //            if (args.Length < 1) {
    //                player.sendNotification(Constants.NotifactionTypes.Danger, "Notruf konnte nicht gesendet werden.",
    //                    "Notruf nicht gesendet.");
    //                return false;
    //            }

    //            Constants.EmergencyCallType emergencyCallType = Enum.Parse<Constants.EmergencyCallType>(
    //                args[0].ToString() ??
    //                throw new
    //                    InvalidOperationException(
    //                        "EmergencyCallType (0)"));

    //            string message = null;
    //            Position? position = null;
    //            bool isAnonymous = anonymous;
    //            Constants.EmergencyCallSeverity severity = Constants.EmergencyCallSeverity.High;


    //            if (args.Length >= 2) {
    //                severity = Enum.Parse<Constants.EmergencyCallSeverity>(args[1].ToString() ??
    //                                                                       throw new
    //                                                                           InvalidOperationException(
    //                                                                               "EmergencyCallSeverity (1)"));
    //            }

    //            if (args.Length >= 3) {
    //                message = args[2].ToString();
    //            }

    //            if (args.Length >= 4) {
    //                if (args[3] is Position) {
    //                    position = (Position)args[3];
    //                } else {
    //                    position = args[3].ToString().FromJson();
    //                }
    //            }

    //            if (args.Length >= 5) {
    //                if (args[4] is bool) {
    //                    isAnonymous = (bool)args[4];
    //                } else {
    //                    isAnonymous = bool.Parse(args[4].ToString()!);
    //                }
    //            }

    //            addNewEmergencyCall(player, emergencyCallType, severity, message, position, isAnonymous);

    //            return true;
    //        } catch (Exception e) {
    //            player.sendNotification(Constants.NotifactionTypes.Danger, "Notruf konnte nicht gesendet werden.",
    //                "Notruf nicht gesendet.", Constants.NotifactionImages.System);
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private bool onSetEmergencyCallsCollisionShapeAnonymous(IPlayer player, CollisionShape collisionShape, Dictionary<string, object> data) {
    //        try {
    //            return addNewEmergencyCallWithData(player, data, true);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private static bool addNewEmergencyCallWithData(IPlayer player, Dictionary<string, object> data, bool anonymous = false) {
    //        try {
    //            Constants.EmergencyCallType emergencyCallType = (Constants.EmergencyCallType)data["EmergencyCallType"];
    //            string message = null;
    //            Position? position = null;
    //            bool isAnonymous = anonymous;
    //            Constants.EmergencyCallSeverity severity = Constants.EmergencyCallSeverity.High;

    //            if (data.ContainsKey("Severity")) {
    //                severity = (Constants.EmergencyCallSeverity)data["Severity"];
    //            }

    //            if (data.ContainsKey("Message")) {
    //                message = (string)data["Message"];
    //            }

    //            if (data.ContainsKey("Position")) {
    //                position = (Position)data["Position"];
    //            }

    //            if (data.ContainsKey("IsAnonymous")) {
    //                isAnonymous = (bool)data["IsAnonymous"];
    //            }

    //            addNewEmergencyCall(player, emergencyCallType, severity, message, position, isAnonymous);

    //            return true;
    //        } catch (Exception e) {
    //            player.sendNotification(Constants.NotifactionTypes.Danger, "Notruf konnte nicht gesendet werden.",
    //                "Notruf nicht gesendet.", Constants.NotifactionImages.System);
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private void onSetEmergencyCallsCefAnonymous(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
    //        try {
    //            addNewEmergencyCall(player, Constants.EmergencyCallType.Police, Constants.EmergencyCallSeverity.High,
    //                null, null, true);
    //        } catch (Exception e) {
    //            player.sendNotification(Constants.NotifactionTypes.Danger, "Notruf konnte nicht gesendet werden.",
    //                "Notruf nicht gesendet.", Constants.NotifactionImages.System);
    //            Logger.logException(e);
    //        }
    //    }

    //    private bool onSetEmergencyCallsMenuAnonymous(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        try {
    //            return addNewEmergencyCallWithData(player, data, true);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private bool onGetLastEmergencyCallsEvent(IPlayer player, string eventname, object[] args) {
    //        try {
    //            return addNewEmergencyCallWithArgs(player, args);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private bool onGetLastEmergencyCallsCollisionShape(IPlayer player, CollisionShape collisionshape, Dictionary<string, object> data) {
    //        try {
    //            return addNewEmergencyCallWithData(player, data);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private void onGetLastEmergencyCallsCef(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
    //        try {
    //            addNewEmergencyCall(player, Constants.EmergencyCallType.Police);
    //        } catch (Exception e) {
    //            player.sendNotification(Constants.NotifactionTypes.Danger, "Notruf konnte nicht gesendet werden.",
    //                "Notruf nicht gesendet.", Constants.NotifactionImages.System);
    //            Logger.logException(e);
    //        }
    //    }

    //    private bool onGetLastEmergencyCallsMenu(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        try {
    //            return addNewEmergencyCallWithData(player, data);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private bool onSetEmergencyCallsEvent(IPlayer player, string eventname, object[] args) {
    //        try {
    //            return addNewEmergencyCallWithArgs(player, args);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private bool onSetEmergencyCallsCollisionShape(IPlayer player, CollisionShape collisionshape, Dictionary<string, object> data) {
    //        try {
    //            return addNewEmergencyCallWithData(player, data);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    private void onSetEmergencyCallsCef(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
    //        try {
    //            addNewEmergencyCall(player, Constants.EmergencyCallType.Police);
    //        } catch (Exception e) {
    //            player.sendNotification(Constants.NotifactionTypes.Danger, "Notruf konnte nicht gesendet werden.",
    //                "Notruf nicht gesendet.", Constants.NotifactionImages.System);
    //            Logger.logException(e);
    //        }
    //    }

    //    private bool onSetEmergencyCallsMenu(IPlayer player, string itemevent, int menuitemid, Dictionary<string, object> data, MenuItemCefEvent menuitemcefevent) {
    //        try {
    //            return addNewEmergencyCallWithData(player, data);
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //            return false;
    //        }
    //    }

    //    public static bool addNewEmergencyCall(IPlayer player, Constants.EmergencyCallType callType, Constants.EmergencyCallSeverity severity = Constants.EmergencyCallSeverity.High, string message = null, Position? position = null, bool isAnonymous = false) {
    //        try {
    //            int characterId = player.getCharacterId();
    //            if (player == null && position == null) {
    //                return false;
    //            }

    //            if (ActiveEmergencyCallElements.Count(d =>
    //                d.CharId == characterId && d.Time >= DateTime.Now - SpamTimeSlot) > 3) {
    //                player.sendNotification(Constants.NotifactionTypes.Warning,
    //                    "Zu viele Dispatches in den letzten 10 Sekunden.", "Dispatch nicht gesendet.");
    //                return false;
    //            }

    //            EmergencyCallElement emergencyCallElement = new EmergencyCallElement {
    //                Id = -1,
    //                CallType = callType,
    //                CharId = characterId,
    //                CharName = $"{player.getCharacterData().LastName}, {player.getCharacterData().FirstName}",
    //                Ended = false,
    //                IsAnonymous = isAnonymous,
    //                IsPersisted = false,
    //                Time = DateTime.Now,
    //                Message = message,
    //                Severity = severity,
    //                Position = position ?? player.getCharacterData().LastPosition
    //            };

    //            ActiveEmergencyCallElements.Add(emergencyCallElement);

    //            doSyncEmergencyCallElement(emergencyCallElement);

    //            return true;
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }

    //        return false;
    //    }

    //    private static void doSyncEmergencyCallElement(EmergencyCallElement emergencyCallElement) {
    //        try {
    //            if (emergencyCallElement.Ended) {
    //                foreach (IPlayer player in EmergencyBlipReceiverList[emergencyCallElement.CallType]) {
    //                    if (player.IsConnected) {
    //                        BlipController.destroyBlipByName(player, emergencyCallElement.Id.ToString());
    //                    }
    //                }
    //                return;
    //            }

    //            string blipMessage = getBlipMessageToEmergencyCallElement(emergencyCallElement);
    //            int sprite = getSpriteToEmergencyCallType(emergencyCallElement.CallType);
    //            foreach (IPlayer player in EmergencyBlipReceiverList[emergencyCallElement.CallType]) {
    //                if (player.IsConnected) {
    //                    BlipController.createPointBlip(player, blipMessage, emergencyCallElement.Position,
    //                        getBlipColorBySeverity(emergencyCallElement.Severity), sprite, _blipAlpha, emergencyCallElement.Id.ToString());
    //                }
    //            }

    //            StringBuilder notificationText = new StringBuilder();
    //            if (emergencyCallElement.IsAnonymous) {
    //                notificationText.AppendLine($"Meldung von Unbekannt:");
    //            } else {
    //                notificationText.AppendLine($"Meldung von Bürger: {emergencyCallElement.CharName}");
    //            }

    //            notificationText.AppendLine(blipMessage);

    //            if (!string.IsNullOrEmpty(emergencyCallElement.Message)) {
    //                notificationText.Append(emergencyCallElement.Message);
    //            }

    //            getNotificationDataToEmergencyCallElement(emergencyCallElement,
    //                out Constants.NotifactionTypes notificationType, out Constants.NotifactionImages notificationImage);

    //            foreach (IPlayer player in EmergencyCallCenterWorkerList[emergencyCallElement.CallType]) {
    //                if (player.IsConnected) {
    //                    player.sendNotification(notificationType, notificationText.ToString(), "Neuer Dispatch",
    //                        notificationImage);
    //                    if (emergencyCallElement.Severity == Constants.EmergencyCallSeverity.High ||
    //                        emergencyCallElement.Severity == Constants.EmergencyCallSeverity.Critical) {
    //                        SoundController.playSound(player, SoundController.Sounds.Bell);
    //                    }
    //                }
    //            }
    //        } catch (Exception e) {
    //            Logger.logException(e);
    //        }
    //    }

    //    private static void getNotificationDataToEmergencyCallElement(EmergencyCallElement emergencyCallElement, out Constants.NotifactionTypes notificationType, out Constants.NotifactionImages notificationImage) {
    //        switch (emergencyCallElement.Severity) {
    //            case Constants.EmergencyCallSeverity.Automatic:
    //                notificationType = Constants.NotifactionTypes.Warning;
    //                break;
    //            case Constants.EmergencyCallSeverity.Low:
    //                notificationType = Constants.NotifactionTypes.Info;
    //                break;
    //            case Constants.EmergencyCallSeverity.Medium:
    //                notificationType = Constants.NotifactionTypes.Warning;
    //                break;
    //            case Constants.EmergencyCallSeverity.High:
    //                notificationType = Constants.NotifactionTypes.Danger;
    //                break;
    //            case Constants.EmergencyCallSeverity.Critical:
    //                notificationType = Constants.NotifactionTypes.Danger;
    //                break;
    //            default:
    //                notificationType = Constants.NotifactionTypes.Warning;
    //                break;
    //        }

    //        switch (emergencyCallElement.CallType) {
    //            case Constants.EmergencyCallType.Police:
    //                notificationImage = Constants.NotifactionImages.Police;
    //                break;
    //            case Constants.EmergencyCallType.Medic:
    //                notificationImage = Constants.NotifactionImages.Bone;
    //                break;
    //            case Constants.EmergencyCallType.Fire:
    //                notificationImage = Constants.NotifactionImages.Fire;
    //                break;
    //            case Constants.EmergencyCallType.Towing:
    //                notificationImage = Constants.NotifactionImages.MiniJob;
    //                break;
    //            case Constants.EmergencyCallType.FBI:
    //                notificationImage = Constants.NotifactionImages.Police;
    //                break;
    //            case Constants.EmergencyCallType.Purge:
    //                notificationImage = Constants.NotifactionImages.Police;
    //                break;
    //            case Constants.EmergencyCallType.System:
    //                notificationImage = Constants.NotifactionImages.System;
    //                break;
    //            default:
    //                notificationImage = Constants.NotifactionImages.System;
    //                break;
    //        }
    //    }

    //    private static int getBlipColorBySeverity(Constants.EmergencyCallSeverity emergencyCallSeverity) {
    //        switch (emergencyCallSeverity) {
    //            case Constants.EmergencyCallSeverity.Automatic:
    //                return (int)Constants.BlipColors.Automatic;
    //            case Constants.EmergencyCallSeverity.Low:
    //                return (int)Constants.BlipColors.Low;
    //            case Constants.EmergencyCallSeverity.Medium:
    //                return (int)Constants.BlipColors.Medium;
    //            case Constants.EmergencyCallSeverity.High:
    //                return (int)Constants.BlipColors.High;
    //            case Constants.EmergencyCallSeverity.Critical:
    //                return (int)Constants.BlipColors.Critical;
    //            default:
    //                return (int)Constants.BlipColors.Low;
    //        }
    //    }

    //    public static int getSpriteToEmergencyCallType(Constants.EmergencyCallType callType) {
    //        switch (callType) {
    //            case Constants.EmergencyCallType.Police:
    //                return (int)Constants.Blips.Police;
    //            case Constants.EmergencyCallType.Medic:
    //                return (int)Constants.Blips.Medic;
    //            case Constants.EmergencyCallType.Fire:
    //                return (int)Constants.Blips.Fire;
    //            case Constants.EmergencyCallType.Towing:
    //                return (int)Constants.Blips.Towing;
    //            case Constants.EmergencyCallType.FBI:
    //                return (int)Constants.Blips.FBI;
    //            case Constants.EmergencyCallType.Purge:
    //                return (int)Constants.Blips.Purge;
    //            default:
    //                return (int)Constants.Blips.Marker;
    //        }
    //    }

    //    private static string getBlipMessageToEmergencyCallElement(EmergencyCallElement emergencyCallElement) {
    //        StringBuilder blipMessage = new StringBuilder();
    //        blipMessage.Append($"[{emergencyCallElement.Time:T}]");
    //        switch (emergencyCallElement.CallType) {
    //            case Constants.EmergencyCallType.Police:
    //                blipMessage.Append("[PD]");
    //                break;
    //            case Constants.EmergencyCallType.Medic:
    //                blipMessage.Append("[PM]");
    //                break;
    //            case Constants.EmergencyCallType.Fire:
    //                blipMessage.Append("[FD]");
    //                break;
    //            case Constants.EmergencyCallType.Towing:
    //                blipMessage.Append("[TC]");
    //                break;
    //            case Constants.EmergencyCallType.FBI:
    //                blipMessage.Append("[FBI]");
    //                break;
    //            case Constants.EmergencyCallType.Purge:
    //                blipMessage.Append("[PU]");
    //                break;
    //            case Constants.EmergencyCallType.System:
    //                blipMessage.Append("[AA]");
    //                break;
    //            default:
    //                blipMessage.Append("[??]");
    //                break;
    //        }

    //        return blipMessage.ToString();
    //    }
    //}
}