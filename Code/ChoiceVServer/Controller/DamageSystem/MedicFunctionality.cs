using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Admin.Tools;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.InteractionController;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.Controller.DamageSystem {
    public class MedicController : ChoiceVScript {
        public MedicController() {
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;
        }


        private void onPlayerConnect(IPlayer player, character character) {
            var charId = player.getCharacterId();

            var medicCompanies = CompanyController.findCompanies(c => c.hasFunctionality<MedicFunctionality>() && c.getFunctionality<MedicFunctionality>().hasPlayerRegistered(charId));

            foreach (var medicCompany in medicCompanies) {
                var functionality = medicCompany.getFunctionality<MedicFunctionality>();
                if (functionality.isInHospital(character.position.FromJson())) {
                    functionality.onPlayerConnect(player);

                    return;
                }
            }
        }
    }


    //Person meldet sich beim Register Ped an
    //Dadurch wird sie dann "getrackt"
    //Medics können über das N Menü auch Personen tracken
    //Solange getrackt kriegen Medics Infos über die Person.
    //  Person verlässt das MD: Patient NAME hat das Gebäude verlassen
    //  Person betritt das MD: Patient NAME hat das Gebäude betreten
    //  Person loggt sich aus: Patient NAME benötigt in Zimmer ZIMMER Ruhe.
    //  Person loggt ein: Patient NAME ist in Zimmer ZIMMER aufgewacht

    public class MedicFunctionality : CompanyFunctionality {
        public class RegisteredPerson {
            public int CharId;
            public string DisplayName;
            public string CreatorName;

            public string CurrentState;

            public DateTime CreateDate;
            public DateTime StateChangeDate;

            public RegisteredPerson(int charId, string displayName, string creatorName, DateTime createDate) {
                CharId = charId;
                DisplayName = displayName;
                CreatorName = creatorName;
                CreateDate = createDate;
                CurrentState = "Im Krankenhaus";
                StateChangeDate = createDate;
            }

            public void changeState(string newState) {
                CurrentState = newState;
                StateChangeDate = DateTime.Now;
            }

        }

        private List<CollisionShape> DiagnoseSpots;
        private int DiagnoseSpotCounter;

        public List<CollisionShape> OutlineSpots; //Multiple, so that the rough shape can be approximated
        private int OutlineSpotCounter;

        public ChoiceVPed RegisterRed;

        public MedicFunctionality() { }

        public MedicFunctionality(Company company) : base(company) { }

        public override string getIdentifier() {
            return "MEDIC_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Medizische Funktionen", "Fügt die Funktionen der Mediziner hinzu. z.B. Diagnosepunkte, das Definieren von Krankenhäusern");
        }

        public override List<string> getSinglePermissionsGranted() {
            return new List<string> { "SHOW_PATIENT_NOTIFICATIONS", "REGISTER_NEW_PATIENT" };
        }

        public override void onLoad() {
            DiagnoseSpots = new List<CollisionShape>();

            DiagnoseSpotCounter = int.Parse(Company.getSetting("DIAGNOSE_SPOT_COUNTER") ?? "0");

            var diagnoseSpots = Company.getSettings("DIAGNOSE_SPOTS");
            foreach (var spot in diagnoseSpots) {
                var col = CollisionShape.Create(spot.settingsValue);
                col.Owner = "DIAGNOSE_SPOT";

                DiagnoseSpots.Add(col);
            }

            OutlineSpotCounter = int.Parse(Company.getSetting("OUTLINE_SPOT_COUNTER") ?? "0");

            OutlineSpots = new List<CollisionShape>();
            var outlines = Company.getSettings("OUTLINE_SPOTS");
            foreach (var outline in outlines) {
                var col = CollisionShape.Create(outline.settingsValue);

                col.OnEntityEnterShape = onEnterOutline;
                col.OnEntityExitShape = onExitOutline;

                OutlineSpots.Add(col);
            }

            foreach (var el in Company.Data.Items.Reverse()) {
                if (el.Key.StartsWith("RegPer")) {
                    var pat = ((string)Company.Data[el.Key]).FromJson<RegisteredPerson>();

                    if (DateTime.Now - pat.CreateDate > TimeSpan.FromDays(3)) {
                        using (var db = new ChoiceVDb()) {
                            var dbChar = db.characters.Find(pat.CharId);

                            if (dbChar != null && !OutlineSpots.Any(o => o.IsInShape(dbChar.position.FromJson()))) {
                                unregisterPlayer(pat.CharId);
                            }
                        }
                    }
                }
            }

            //FOR FUTURE: Registering Patients
            //Company.registerCompanyInteractElement(
            //    "REGISTER_NEW_PATIENT",
            //    registerPatientMenuGenerator,
            //    onRegisterPatientCallback,
            //    "REGISTER_NEW_PATIENT"
            //);

            //Company.registerCompanySelfElement(
            //    "PATIENT_LIST",
            //    patientListMenuGenerator,
            //    patientListCallback
            //);

            Company.registerCompanySelfElement(
                "PATIENT_FROM_FAR",
                loadPatientFromFarGenerator,
                loadPatientFromFarCallback
            );

            #region Admin Stuff

            Company.registerCompanyAdminElement(
                "SET_SPOTS",
                adminSpotGenerator,
                adminSpotCallback
            );

            #endregion
        }

        public override void onRemove() {
            Company.deleteSetting("DIAGNOSE_SPOT_COUNTER");
            Company.deleteSettings("DIAGNOSE_SPOTS");
            Company.deleteSetting("OUTLINE_SPOT_COUNTER");
            Company.deleteSettings("OUTLINE_SPOTS");

            foreach (var el in Company.Data.Items.Reverse()) {
                if (el.Key.StartsWith("RegPer")) {
                    Company.Data.remove(el.Key);
                }
            }

            Company.unregisterCompanyElement("REGISTER_NEW_PATIENT");
            Company.unregisterCompanyElement("PATIENT_LIST");
            Company.unregisterCompanyElement("PATIENT_FROM_FAR");
            Company.unregisterCompanyElement("SET_SPOTS");
        }

        private MenuElement loadPatientFromFarGenerator(Company company, IPlayer player) {
            if (player.IsInVehicle && ChoiceVAPI.FindNearbyPlayer(player.Position, 50, t => !t.IsInVehicle && t.hasState(Constants.PlayerStates.Dead) && !t.hasState(Constants.PlayerStates.BeingCarried)) != null) {
                return new ClickMenuItem("Verletzte Person einladen (Ferne)", "Lade eine verletzte Person aus der Ferne ein. (Die nächste zum Fahrzeug befindliche verletze Person). Nur zu benutzen, wenn keine andere Möglichkeit besteht!", "", "LOAD_DEAD_PERSON_IN_VEHICLE", MenuItemStyle.yellow);
            } else {
                return null;
            }
        }

        private void loadPatientFromFarCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = ChoiceVAPI.FindNearbyPlayer(player.Position, 50, t => !t.IsInVehicle && t.hasState(Constants.PlayerStates.Dead) && !t.hasState(Constants.PlayerStates.BeingCarried));

            if (target != null && player.IsInVehicle) {
                CarryController.setPlayerInCarryVehicle(player, target, (ChoiceVVehicle)player.Vehicle, 3);
                //TODO Message to Support Discord LOG
                player.sendNotification(Constants.NotifactionTypes.Warning, "Person wurde in dein Fahrzeug gesetzt.", "Person eingeladen", Constants.NotifactionImages.Bone);
            } else {
                player.sendBlockNotification("Keine Person oder freie Rücksitze gefunden", "Aktion blockiert", Constants.NotifactionImages.Bone);
            }
        }

        private MenuElement patientListMenuGenerator(Company company, IPlayer player) {
            var patients = Company.Data.Items.Where(k => k.Key.StartsWith("RegPer")).Select(k => ((string)k.Value).FromJson<RegisteredPerson>()).ToList();

            var menu = new Menu("Patientenliste", "Wähle einen Patienten aus");

            foreach (var patient in patients) {
                //var subMenu = new VirtualMenu(patient.DisplayName, () => {
                var subMenu = new Menu(patient.DisplayName, "Was möchtest du tun?");

                subMenu.addMenuItem(new StaticMenuItem("Ersteller", $"Der Patient wurde von {patient.CreatorName} registriert", patient.CreatorName));
                subMenu.addMenuItem(new StaticMenuItem("Erstellzeit", $"Der Patient wurde {patient.CreateDate.ToString("g")} registriert", patient.CreateDate.ToString("g")));
                subMenu.addMenuItem(new StaticMenuItem("Status", $"Der Patient hat aktuell den Status: {patient.CurrentState}", patient.CurrentState));
                subMenu.addMenuItem(new StaticMenuItem("Statuszeit", $"Der Status des Patienten hat sich das letzte mal {patient.StateChangeDate.ToString("g")} geändert", patient.StateChangeDate.ToString("g")));

                var data = new Dictionary<string, dynamic> { { "Patient", patient } };
                subMenu.addMenuItem(new ClickMenuItem("Patient entlassen", "Entferne den Patienten aus dem System", "", "DELETE_PATIENT", MenuItemStyle.red).withData(data).needsConfirmation("Patient entlassen?", "Patient wirklich entlassen?"));
                //TODO Abgemeldet

                menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                //return subMenu;
                //});
            }

            return menu;
        }

        private void patientListCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            switch (subEvent) {
                case "DELETE_PATIENT":
                    var patient = (RegisteredPerson)data["Patient"];

                    unregisterPlayer(patient.CharId);

                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Patient {patient.DisplayName} erfolgreich gelöscht", "Patient gelöscht");
                    break;
            }
        }

        public void registerPlayer(IPlayer player, string displayName, string creatorName) {
            Company.Data[$"RegPer_{player.getCharacterId()}"] = new RegisteredPerson(player.getCharacterId(), displayName, creatorName, DateTime.Now).ToJson();
        }

        public void unregisterPlayer(int charId) {
            Company.Data.remove($"RegPer_{charId}");
        }

        public void updateRegisteredPlayer(RegisteredPerson patient) {
            Company.Data[$"RegPer_{patient.CharId}"] = patient.ToJson();
        }

        public bool hasPlayerRegistered(int charId) {
            return Company.Data.hasKey($"RegPer_{charId}");
        }

        public RegisteredPerson getRegisteredPatient(int charId) {
            return ((string)Company.Data[$"RegPer_{charId}"]).FromJson<RegisteredPerson>();
        }

        public bool isInHospital(Position position) {
            return OutlineSpots.Any(c => c.IsInShape(position));
        }

        public void sendMessageToStaff(string logMessage, string shortMessage) {
            foreach (var p in ChoiceVAPI.GetAllPlayers().Where(p => CompanyController.isInDuty(p, Company) && isInHospital(p.Position) && CompanyController.hasPlayerPermission(p, Company, "SHOW_PATIENT_NOTIFICATIONS")).ToList()) {
                p.sendNotification(Constants.NotifactionTypes.Info, logMessage, shortMessage, Constants.NotifactionImages.Hotel);
            }
        }

        private void onEnterOutline(CollisionShape shape, IEntity entity) {
            if (entity is IPlayer player) {
                player.addState(Constants.PlayerStates.InHospital);

                var charId = player.getCharacterId();
                if (hasPlayerRegistered(charId)) {
                    var patient = getRegisteredPatient(charId);
                    if (patient.CurrentState == "Krankenhaus verlassen") {
                        sendMessageToStaff($"Patient: {patient.DisplayName} hat das Krankenhaus betreten!", "Patient betritt MD");

                        patient.changeState("Krankenhaus betreten");
                        updateRegisteredPlayer(patient);
                    }
                }
            }
        }

        private void onExitOutline(CollisionShape shape, IEntity entity) {
            if (entity is IPlayer player) {

                if (!isInHospital(entity.Position)) {
                    player.removeState(Constants.PlayerStates.InHospital);
                    if (hasPlayerRegistered(player.getCharacterId())) {
                        player.sendNotification(Constants.NotifactionTypes.Warning, "Du verlässt das Krankenhaus, obwohl du noch angemeldet bist. Kameras werden dich dabei beobachten!", "Krankenhaus wird verlassen", Constants.NotifactionImages.Hotel);

                        InvokeController.AddTimedInvoke($"RegPer_{player.getCharacterId()}", (i) => {
                            if (!isInHospital(player.Position)) {
                                var patient = getRegisteredPatient(player.getCharacterId());

                                if (!player.Exists()) {
                                    sendMessageToStaff($"Patient: {patient.DisplayName} ruht sich aus!", "Patient ruht aus");

                                    patient.changeState("Ruht sich aus");

                                    updateRegisteredPlayer(patient);
                                } else {
                                    sendMessageToStaff($"Patient: {patient.DisplayName} hat das Krankenhaus verlassen!", "Patient verließ MD");

                                    patient.changeState("Krankenhaus verlassen");

                                    updateRegisteredPlayer(patient);

                                }
                            }
                        }, TimeSpan.FromSeconds(15), false);
                    }
                }
            }
        }

        public void onPlayerConnect(IPlayer player) {
            var charId = player.getCharacterId();

            var patient = getRegisteredPatient(charId);

            sendMessageToStaff($"{patient.DisplayName} ist im Krankenhaus aufgewacht!", $"Patient aufgewacht");

            patient.changeState("Aufgewacht");
            updateRegisteredPlayer(patient);
        }

        public MenuElement getRegisterMenu(IPlayer player, IPlayer interactPlayer) {
            var charId = player.getCharacterId();

            if (isInHospital(player.Position)) {
                var data = new Dictionary<string, dynamic> { { "InteractionTarget", interactPlayer } };

                if (!hasPlayerRegistered(charId)) {
                    return new InputMenuItem("Patient registrieren", "Trage die Person ins Patienten-System ein. Sie wird daraufhin getrackt", "Name", "REGISTER_PERSON", MenuItemStyle.green).withData(data).needsConfirmation("Patient eintragen?", "Patient wirklich eintragen?"); ;
                } else {
                    var patient = getRegisteredPatient(charId);
                    return new ClickMenuItem("Patient entlassen", $"Entferne den Patient aus dem System. Er wurde von {patient.CreatorName} eingetragen.", $"{patient.DisplayName}", "UNREGISTER_PERSON").withData(data).needsConfirmation("Patient entfernen?", "Patient wirklich entfernen?");
                }
            } else {
                return new StaticMenuItem("Patient nicht registrierbar", "Die Person muss sich im Krankenhaus befinden um registriert werden zu können", "", MenuItemStyle.yellow);
            }
        }

        private MenuElement registerPatientMenuGenerator(IPlayer player, IPlayer target) {
            return getRegisterMenu(player, target);
        }

        private void onRegisterPatientCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["InteractionTarget"];

            if (target != null) {
                switch (subEvent) {
                    case "REGISTER_PERSON":
                        var inputEvt = menuItemCefEvent as InputMenuItemEvent;
                        registerPlayer(target, inputEvt.input, player.getCharacterName());
                        player.sendNotification(Constants.NotifactionTypes.Success, $"Patient {inputEvt.input} erfolgreich registriert.", "Patient registriert", Constants.NotifactionImages.Hotel);
                        break;
                    case "UNREGISTER_PERSON":
                        unregisterPlayer(target.getCharacterId());
                        player.sendNotification(Constants.NotifactionTypes.Warning, $"Patient erfolgreich entlassen.", "Patient entlassen", Constants.NotifactionImages.Hotel);
                        break;
                }
            }
        }

        #region Admin Stuff

        private MenuElement adminSpotGenerator(IPlayer player) {
            var menu = new Menu("Collisionshapes setzen", "Was möchtest du tun?");

            var diagnoseMenu = new Menu("Diagnosespots setzen", "Was möchstest du tun?");
            diagnoseMenu.addMenuItem(new ClickMenuItem("Neuen Spot setzen", "Setze einen neuen Spot zum diagnostizieren von Verletzungen", "", "CREATE_DIAGNOSE_SPOT", MenuItemStyle.green));

            foreach (var diagnoseSpot in DiagnoseSpots) {
                var data = new Dictionary<string, dynamic> { { "DiagnoseSpot", diagnoseSpot } };
                diagnoseMenu.addMenuItem(new ClickMenuItem("Position", $"Daten des Shapes sind Position: {diagnoseSpot.Position}, Width: {diagnoseSpot.Width}, Height: {diagnoseSpot.Height}", $"{diagnoseSpot.Position}", "DELETE_DIAGNOSE_SPOT").withData(data).needsConfirmation("Spot löschen?", "Wirklich löschen?"));
            }

            menu.addMenuItem(new MenuMenuItem(diagnoseMenu.Name, diagnoseMenu));


            var outlineMenu = new Menu("Krankenhaus setzen", "Setze die Umgebung des Krankenhauses");

            outlineMenu.addMenuItem(new ClickMenuItem("Neuen Spot setzen", "Setze einen neuen Spot zum Krankenhaus hinzufügen", "", "CREATE_OUTLINE_SPOT", MenuItemStyle.green));

            foreach (var outlineSpot in OutlineSpots) {
                var data = new Dictionary<string, dynamic> { { "OutlineSpot", outlineSpot } };
                outlineMenu.addMenuItem(new ClickMenuItem("Position", $"Daten des Shapes sind Position: {outlineSpot.Position}, Width: {outlineSpot.Width}, Height: {outlineSpot.Height}", $"{outlineSpot.Position}", "DELETE_OUTLINE_SPOT").withData(data).needsConfirmation("Spot löschen?", "Wirklich löschen?"));
            }

            menu.addMenuItem(new MenuMenuItem(outlineMenu.Name, outlineMenu));

            return menu;
        }

        private void adminSpotCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            switch (subEvent) {
                case "CREATE_DIAGNOSE_SPOT":
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                        var colShape = CollisionShape.Create(p, w, h, r, true, false);

                        colShape.Owner = "DIAGNOSE_SPOT";

                        DiagnoseSpots.Add(colShape);

                        DiagnoseSpotCounter++;
                        Company.setSetting("DIAGNOSE_SPOT_COUNTER", OutlineSpotCounter.ToString());

                        Company.setSetting($"DIAGNOSE_SPOT_{DiagnoseSpotCounter + 1}", colShape.toShortSave(), "DIAGNOSE_SPOTS");
                        player.sendNotification(Constants.NotifactionTypes.Success, "Diagnosespot erfolgreich gesetzt", "Spot gesetzt");
                    });
                    break;
                case "CREATE_OUTLINE_SPOT":
                    CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (p, w, h, r) => {
                        p.Z = 0;

                        var colShape = CollisionShape.Create(p, w, h, r, true, false);

                        colShape.OnEntityEnterShape = onEnterOutline;
                        colShape.OnEntityExitShape = onExitOutline;

                        OutlineSpots.Add(colShape);

                        OutlineSpotCounter++;
                        Company.setSetting("OUTLINE_SPOT_COUNTER", OutlineSpotCounter.ToString());
                        Company.setSetting($"OUTLINE_SPOT_{OutlineSpotCounter}", colShape.toShortSave(), "OUTLINE_SPOTS");
                        player.sendNotification(Constants.NotifactionTypes.Success, "Outlinespots erfolgreich gesetzt", "Spot gesetzt");
                    });
                    break;
            }
        }

        #endregion

    }
}
