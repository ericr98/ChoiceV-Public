using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.Controller {
    public class CamController : ChoiceVScript {
        public class Cam {
            public int Id { get; private set; }
            public int GridId { get; private set; }
            public Position Position { get; private set; }
            public string Model { get; private set; }
            public float Heading { get; private set; }

            public Cam(int id, int gridId, Position position, string model, float heading) {
                Id = id;
                GridId = gridId;
                Position = position;
                Model = model;
                Heading = heading;
            }
        }

        private static Dictionary<string, string> CamModelToName = new Dictionary<string, string> {
            { "prop_cs_cctv", "Direktionale Kamera" },
            { "p_cctv_s", "Direktionale Kamera" },
            { "prop_cctv_cam_01a", "Direktionale Kamera" },
            { "prop_cctv_cam_01b", "Direktionale Kamera" },
            { "prop_cctv_cam_02a", "Direktionale Kamera" },
            { "prop_cctv_cam_03a", "Direktionale Kamera" },
            { "prop_cctv_cam_04a", "360° Kamera" },
            { "prop_cctv_cam_04b", "360° Kamera" },
            { "prop_cctv_cam_04c", "360° Kamera" },
            { "prop_cctv_cam_05a", "Direktionale Kamera" },
            { "prop_cctv_cam_06a", "Direktionale Kamera" },
            { "prop_cctv_cam_07a", "Direktionale Kamera" },
            { "prop_cctv_pole_01a", "Direktionale Mastkamera" },
            { "prop_cctv_pole_02", "360° Dopple-Kameramast" },
            { "prop_cctv_pole_03", "Direktionale Doppel-Mastkamera" },
            { "prop_cctv_pole_04", "Direktionale Mastkamera" },

            { "ba_prop_battle_cctv_cam_01a", "Direktionale Kamera" },
            { "ba_prop_battle_cctv_cam_01b", "Direktionale Kamera" },

            { "xm_prop_x17_cctv_01a", "Direktionale Militär-Kamera" },
            { "xm_prop_x17_server_farm_cctv_01", "Direktionale Kamera" },

            { "hei_prop_bank_cctv_01", "Direktionale Kamera" },
            { "hei_prop_bank_cctv_02", "360° Kamera" },

            { "ch_prop_ch_cctv_cam_01a", "Direktionale Kamera" },
            { "ch_prop_ch_cctv_cam_02a", "Direktionale Kamera" },
            { "tr_prop_tr_cctv_cam_01a", "Direktionale Kamera" },
        };

        private static List<Cam> AllCams;

        private record CharacterCameraActioCheck(int Id, string ShortDescription, string Description);

        private static Dictionary<int, List<CharacterCameraActioCheck>> CharacterCameraActionCallbacks;

        private static int CallbackCounter = 0;

        //TODO HMMM? Maus ist beim BEschriften der Protokolle da

        public CamController() {
            CharacterCameraActionCallbacks = new Dictionary<int, List<CharacterCameraActioCheck>>();
            loadCams();
            removeOldLogs();
            InvokeController.AddTimedInvoke("Remove-Old-Logs", (i) => removeOldLogs(), TimeSpan.FromDays(1), true);

            EventController.addEvent("CAM_ENTER_RANGE", onPlayerEnterCamRange);
            EventController.addEvent("CAM_EXIT_RANGE", onPlayerExitCamRange);
            EventController.addEvent("ANSWER_IF_CAM_VISIBLE", onAnswerIfCamVisible);

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnected;

            EventController.addMenuEvent("SELECT_CAMERA", onSelectCamera);
            EventController.addMenuEvent("RECEIVE_CAM_PROTOCOL", onReceiverCamProtocol);

            EventController.addMenuEvent("SHOW_NPC_OF_CAM_PERSON", onShowNpcOfCamPerson);
            EventController.addMenuEvent("ON_SELECT_CAM_LOG", onSelectCamLog);

            #region Admin Stuff

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ClickMenuItem("CCTV Modus de/aktivieren", "Aktiviere den Modus um CCTV Kameras zu sehen und hinzuzufügen", "", "ADMIN_TOGGLE_CCTV_MODE"),
                    1,
                    SupportMenuCategories.Misc
                )
            );
            EventController.addMenuEvent("ADMIN_TOGGLE_CCTV_MODE", onAdminToggleCCTVMode);

            EventController.addEvent("FOUND_NEW_CAM", onFoundNewCam);

            EventController.addMenuEvent("CREATE_PROTOCOL_ITEM", onCreateProtocolItem);
            EventController.addMenuEvent("CAM_PROTOCOL_NAME_ITEM", onCamProtocolNameItem);

            #endregion
        }

        private void onPlayerDisconnected(IPlayer player, string reason) {
            var charId = player.getCharacterId();
            if(CharacterCameraActionCallbacks.ContainsKey(charId)) {
                CharacterCameraActionCallbacks.Remove(charId);
            }
        }

        private void loadCams() {
            AllCams = new List<Cam>();

            using(var db = new ChoiceVDb()) {
                foreach(var cam in db.configcams) {
                    var pos = cam.position.FromJson<Position>();
                    AllCams.Add(new Cam(cam.id, WorldController.getWorldGrid(pos).Id, pos, cam.model, cam.heading));
                }
            }

            var camdIdsString = "export var camIds = [";
            var camGridsString = "export var camGrids = [";
            var camModelsString = "export var camModels = [";
            var camPosXsString = "export var camPosXs = [";
            var camPosYsString = "export var camPosYs = [";
            var camPosZsString = "export var camPosZs = [";
            var camHeadingsString = "export var camHeadings = [";

            var count = 0;
            using(var db = new ChoiceVDb()) {
                foreach(var cam in AllCams) {
                    count++;
                    var x = cam.Position.X.ToString().Replace(',', '.');
                    var y = cam.Position.Y.ToString().Replace(',', '.');
                    var z = cam.Position.Z.ToString().Replace(',', '.');

                    camdIdsString = camdIdsString + cam.Id + ", ";
                    camGridsString = camGridsString + cam.GridId + ", ";
                    camModelsString = camModelsString + "\"" + cam.Model + "\"" + ", ";
                    camPosXsString = camPosXsString + x + ", ";
                    camPosYsString = camPosYsString + y + ", ";
                    camPosZsString = camPosZsString + z + ", ";
                    camHeadingsString = camHeadingsString + cam.Heading + ", ";

                    Logger.logTrace(LogCategory.ServerStartup, LogActionType.Created, $"cam was loaded: {cam.Id}");
                }
            }

            if(count != 0) {
                camdIdsString = camdIdsString.Remove(camdIdsString.Length - 1);
                camGridsString = camGridsString.Remove(camGridsString.Length - 1);
                camModelsString = camModelsString.Remove(camModelsString.Length - 1);
                camPosXsString = camPosXsString.Remove(camPosXsString.Length - 1);
                camPosYsString = camPosYsString.Remove(camPosYsString.Length - 1);
                camPosZsString = camPosZsString.Remove(camPosZsString.Length - 1);
                camHeadingsString = camHeadingsString.Remove(camHeadingsString.Length - 1);
            }

            camdIdsString += "];";
            camGridsString += "];";
            camModelsString += "];";
            camPosXsString += "];";
            camPosYsString += "];";
            camPosZsString += "];";
            camHeadingsString += "];";

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                System.IO.File.WriteAllLines("resources\\ChoiceVClient\\js\\camsList.js", new string[] { });
                System.IO.File.WriteAllLines("resources\\ChoiceVClient\\js\\camsList.js", new string[] { camdIdsString, camGridsString, camModelsString, camPosXsString, camPosYsString, camPosZsString, camHeadingsString });
            } else {
                System.IO.File.WriteAllLines("resources/ChoiceVClient/js/camsList.js", new string[] { });
                System.IO.File.WriteAllLines("resources/ChoiceVClient/js/camsList.js", new string[] { camdIdsString, camGridsString, camModelsString, camPosXsString, camPosYsString, camPosZsString, camHeadingsString });
            }

            Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, "CamController: {" + count + "} Cams have been loaded!");
        }

        private void removeOldLogs() {
            using(var db = new ChoiceVDb()) {
                var maxDate = DateTime.Now - TimeSpan.FromDays(4);
                var oldLogs = db.camlogs.Where(c => c.save == 0 && c.createTime < maxDate).ToList();

                Logger.logTrace(LogCategory.ServerStartup, LogActionType.Removed, $"Deleting {oldLogs.Count()} old logs");

                db.RemoveRange(oldLogs);

                var maxOldDate = DateTime.Now - TimeSpan.FromDays(60);
                var veryOldLogs = db.camlogs.Where(c => c.save == 1 && c.createTime > maxOldDate).ToList();
                Logger.logTrace(LogCategory.ServerStartup, LogActionType.Removed, $"Deleting {oldLogs.Count()} very old logs");

                db.RemoveRange(veryOldLogs);

                db.SaveChanges();
            }
        }

        #region Item

        private bool onCreateProtocolItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var startTime = (DateTime)data["StartTime"];
            var endTime = startTime + TimeSpan.FromMinutes(30);
            var cam = (Cam)data["Cam"];

            using(var db = new ChoiceVDb()) {
                var logs = db.camlogs.Where(c => c.camId == cam.Id && c.createTime > startTime && c.createTime < endTime).ToList();

                logs.ForEach(l => l.save = 1);

                db.SaveChanges();

                var item = new CamProtocol(cam.Id, startTime);
                player.getInventory().addItem(item, true);
                item.Description = $"{startTime.ToString("T")}";
                item.updateDescription();
                player.sendNotification(Constants.NotifactionTypes.Success, "Aufzeichnung erfolgreich erstellt! Sie hält für 60 Tage", "Aufzeichnung erstellt");
            }

            return true;
        }

        private bool onCamProtocolNameItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (CamProtocol)data["Item"];

            var evt = menuItemCefEvent as InputMenuItemEvent;
            item.Description = evt.input;
            item.updateDescription();

            player.sendNotification(Constants.NotifactionTypes.Success, $"Aufzeichnung erfolgreich beschriftet! Sie trägt nun den Namen: {item.Description}", "Aufzeichnung umbennant");

            return true;
        }

        #endregion

        public static Menu getCamProtocolMenu(IPlayer player) {
            var menu = new Menu("Kameraprotokolle anzeigen", "Welche Kamera möchtest du anzeigen?");

            foreach(var cam in AllCams.Where(c => c.Position.Distance(player.Position) < 30)) {
                var name = CamModelToName[cam.Model];
                var distance = Math.Round(cam.Position.Distance(player.Position), 2);

                var data = new Dictionary<string, dynamic> {
                    { "Cam", cam }
                };

                menu.addMenuItem(new ClickMenuItem(name, $"Wähle die {name} Kamera mit einer Entfernung von {distance} zu dir", $"{distance}", "SELECT_CAMERA", MenuItemStyle.normal, true).withData(data).needsConfirmation("Kameraprotokoll anzeigen?", "Wirklich anzeigen?"));
            }

            return menu;
        }

        private bool onSelectCamera(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var cam = (Cam)data["Cam"];

            if(menuItemCefEvent.action == "changed") {
                player.emitClientEvent("CAM_SHOW_FOR_FIVE_SECONDS", cam.GridId, cam.Id);
            } else {
                var menu = new Menu("Zeitraum auswählen", "Wähle einen 30min Zeitraum aus");

                var menData = new Dictionary<string, dynamic> {
                    { "Cam", cam }
                };

                menu.addMenuItem(new ClickMenuItem("Letzten 30min ausgeben", "Gib dir die letzten 30min aus", "", "RECEIVE_CAM_PROTOCOL").withData(menData).needsConfirmation("Letzten 30min ausgeben?", "Wirklich ausgeben?"));
                menu.addMenuItem(new InputMenuItem("Startpunkt", "Gib den Startpunkt der 30min an. Das Format ist TAG.MONAT STUNDE:MINUTE", "z.B. 01.07 14:52", "RECEIVE_CAM_PROTOCOL").withData(menData).needsConfirmation("Diese 30min ausgeben?", "Wirklich ausgeben?"));

                player.showMenu(menu);
            }

            return true;
        }


        private record CamPerson(int Number, int CharId, string ClothesStr);
        private bool onReceiverCamProtocol(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var cam = (Cam)data["Cam"];
            var startTime = DateTime.Now - TimeSpan.FromMinutes(30);

            if(menuItemCefEvent is InputMenuItemEvent) {
                var input = (menuItemCefEvent as InputMenuItemEvent).input;

                try {
                    var split = input.Split(' ');
                    var daySplit = split[0].Split('.');
                    var timeSplit = split[1].Split(':');

                    startTime = new DateTime(DateTime.Now.Year, int.Parse(daySplit[1]), int.Parse(daySplit[0]), int.Parse(timeSplit[0]), int.Parse(timeSplit[1]), 0);
                } catch(Exception) {
                    player.sendBlockNotification("Eingabe fehlerhaft!", "", Constants.NotifactionImages.System);
                    return true;
                }
            }

            var endTime = startTime + TimeSpan.FromMinutes(30);
            using(var db = new ChoiceVDb()) {
                var logs = db.camlogs.Where(c => c.camId == cam.Id && c.dimension == player.Dimension && c.createTime > startTime && c.createTime < endTime).ToList();

                var menu = getCameraProtocolMenu(player, logs);

                var protData = new Dictionary<string, dynamic> {
                    { "StartTime", startTime },
                    { "Cam", cam }
                };

                menu.addMenuItem(new ClickMenuItem("Protkoll aufzeichnen", "Zeichne das Protkoll auf um es später noch einmal betrachten zu können", "", "CREATE_PROTOCOL_ITEM").withData(protData).needsConfirmation("Ausdruck erstellen?", "Ausdruck wirklich erstellen?"));

                player.showMenu(menu, false);
            }

            return true;
        }

        public static Menu getCameraProtocolMenu(IPlayer player, List<camlog> camLogs) {
            var menu = new Menu("Kameraprotokoll", "Siehe dir die Informationen an");

            //var logs = db.camlogs.Where(c => c.camId == cam.Id && c.createTime > startTime && c.createTime < endTime).ToList();

            var personsLogs = camLogs.GroupBy(l => new { l.charId, l.clothes });

            var allPersons = new List<CamPerson>();
            var count = 0;
            foreach(var personLog in personsLogs) {
                count++;
                var first = personLog.First();
                allPersons.Add(new CamPerson(count, first.charId, first.clothes));

                var personMenu = new Menu($"Person {count}", $"Siehe dir die Aktionen von Person {count}", onClosePersonMenu);

                var virtPersDesc = new VirtualMenu("Personenbeschreibung", () => {
                    var descMenu = new Menu("Personenbeschreibung", "Die Person sah folgend aus", onClosePersonMenu, true);

                    var split = first.clothes.Split('#');

                    var gender = "Weiblich";
                    if(split[0] == "M") {
                        gender = "Männlich";
                    }

                    var cl = ClothingPlayer.FromJson(split[1]);
                    descMenu.addMenuItem(new StaticMenuItem("Geschlecht", $"Die Person war {gender}", gender));

                    var cfgMask = ClothingController.getConfigClothing(1, cl.Mask.Drawable, "U", cl.Mask.Dlc);
                    descMenu.addMenuItem(new StaticMenuItem("Maske", $"Die Maske der Person war: {cfgMask.name}", cfgMask.name));

                    var cfgTop = ClothingController.getConfigClothing(11, cl.Top.Drawable, split[0], cl.Top.Dlc);
                    descMenu.addMenuItem(new StaticMenuItem("Oberteil", $"Das Oberteil der Person war {cfgTop.name}", cfgTop.name));

                    var cfgShirt = ClothingController.getConfigClothing(8, cl.Shirt.Drawable, split[0], cl.Shirt.Dlc);
                    descMenu.addMenuItem(new StaticMenuItem("Unterteil", $"Das Unterteil der Person war {cfgShirt.name}", cfgShirt.name));

                    var cfgLegs = ClothingController.getConfigClothing(4, cl.Legs.Drawable, split[0], cl.Legs.Dlc);
                    descMenu.addMenuItem(new StaticMenuItem("Hose", $"Die Hose der Person war {cfgLegs.name}", cfgLegs.name));

                    var cfgFeet = ClothingController.getConfigClothing(6, cl.Feet.Drawable, split[0], cl.Feet.Dlc);
                    descMenu.addMenuItem(new StaticMenuItem("Hose", $"Die Schuhe der Person waren {cfgFeet.name}", cfgFeet.name));

                    descMenu.addMenuItem(new HoverMenuItem("Schaubild anzeigen", "Zeige dir ein Schaubild der Person an", "", "SHOW_NPC_OF_CAM_PERSON").withData(new Dictionary<string, dynamic> { { "Clothes", personLog.First().clothes } }));

                    return descMenu;
                });
                personMenu.addMenuItem(new MenuMenuItem("Personenbeschreibung", virtPersDesc));


                var virtActMenu = new VirtualMenu("Aktionen", () => {
                    var actMenu = new Menu("Personenbeschreibung", "Die Person sah folgend aus");

                    foreach(var log in personLog) {
                        if(log.position.FromJson<Position>().Distance(player.Position) > 60) {
                            actMenu.addMenuItem(new StaticMenuItem($"{log.createTime.ToString("T")}", $"Die Aktion: \"{log.description}\" wurde ca. {log.distance} entfernt von der Kamera ausgeführt", log.shortDesc));
                        } else {
                            actMenu.addMenuItem(new HoverMenuItem($"{log.createTime.ToString("T")}", $"Die Aktion: \"{log.description}\" wurde ca. {log.distance} entfernt von der Kamera ausgeführt", log.shortDesc, "ON_SELECT_CAM_LOG", MenuItemStyle.lightlyGreen).withData(new Dictionary<string, dynamic> { { "Log", log } }));
                        }
                    }

                    return actMenu;
                });
                personMenu.addMenuItem(new MenuMenuItem("Aktionen", virtActMenu));

                menu.addMenuItem(new MenuMenuItem(personMenu.Name, personMenu));
            }

            var virtAllAcMenu = new VirtualMenu("Alle Aktionen", () => {
                var allAcMenu = new Menu("Alle Aktionen", "Liste alle Aktionen");

                foreach(var log in camLogs) {
                    var p = allPersons.FirstOrDefault(p => p.CharId == log.charId && p.ClothesStr == log.clothes);

                    if(log.position.FromJson<Position>().Distance(player.Position) > 60) {
                        allAcMenu.addMenuItem(new StaticMenuItem($"{log.createTime.ToString("T")}", $"Die Aktion: \"{log.description}\" wurde ca. {log.distance} entfernt von der Kamera ausgeführt", $"P{p.Number}: {log.shortDesc}"));
                    } else {
                        allAcMenu.addMenuItem(new HoverMenuItem($"{log.createTime.ToString("T")}", $"Die Aktion: \"{log.description}\" wurde ca. {log.distance} entfernt von der Kamera ausgeführt", $"P{p.Number}: {log.shortDesc}", "ON_SELECT_CAM_LOG", MenuItemStyle.lightlyGreen).withData(new Dictionary<string, dynamic> { { "Log", log } }));
                    }
                }

                return allAcMenu;
            });
            menu.addMenuItem(new MenuMenuItem("Alle Aktionen", virtAllAcMenu));

            return menu;
        }

        private bool onSelectCamLog(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var log = (camlog)data["Log"];

            var marker = MarkerController.createMarker(0, log.position.FromJson(), new Rgba(137, 34, 18, 200), 0.3f, 100, new List<IPlayer> { player });

            InvokeController.AddTimedInvoke("Marker_Remover", (i) => {
                MarkerController.removeMarker(marker);
            }, TimeSpan.FromSeconds(5), false);

            return true;
        }


        private static void onClosePersonMenu(IPlayer player) {
            if(player.hasData("NPC_CAM_SHOWPED")) {
                var ped = (ChoiceVPed)player.getData("NPC_CAM_SHOWPED");

                PedController.destroyPed(ped);
            }
        }

        private bool onShowNpcOfCamPerson(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.hasData("NPC_CAM_SHOWPED")) {
                var alreadyPed = (ChoiceVPed)player.getData("NPC_CAM_SHOWPED");
                PedController.destroyPed(alreadyPed);
            }

            var clothesStr = (string)data["Clothes"];
            var split = clothesStr.Split('#');

            var model = "mp_f_freemode_01";
            if(split[0] == "M") {
                model = "mp_m_freemode_01";
            }

            var forwardVector = player.getForwardVector();
            var pedPos = new Position(player.Position.X + forwardVector.X * 2, player.Position.Y + forwardVector.Y, player.Position.Z - 1);
            var heading = player.getRotationTowardsPosition(pedPos);

            var ped = PedController.createPed(player, "", model, pedPos, heading.Yaw - 180);
            var cl = ClothingPlayer.FromJson(split[1]);

            //TODO ADD alt.setPedDlcClothes (clientside) to ped, so it can also show dlc clothes!
            PedController.setPedPlayerClothing(ped, cl);
            ped.setAlpha(210);

            player.setData("NPC_CAM_SHOWPED", ped);

            return true;
        }


        private bool onPlayerEnterCamRange(IPlayer player, string eventName, object[] args) {
            var camId = int.Parse(args[0].ToString());
            var position = new Position(float.Parse(args[1].ToString()), float.Parse(args[2].ToString()), float.Parse(args[3].ToString()));

            if(player.IsInVehicle) {
                var vehicle = (ChoiceVVehicle)player.Vehicle;

                var posStr = "Fahrer";
                if(((ChoiceVVehicle)player.Vehicle).PassengerList.FirstOrDefault(p => p.Value == player).Key != 0) {
                    posStr = "Mitfahrer";
                }

                addCamLog(player, camId, position, player.Dimension, "Bereich befahren", $"Person hat den Kamerasichtbereich in Fahrzeug ({vehicle.DbModel.ModelName}:{vehicle.NumberplateText}) als {posStr} betreten", DateTime.Now, player.Position);
            } else {
                var weaponShortStr = "";
                var weaponStr = ".";
                var weapon = player.CurrentWeapon;
                if(weapon != WeaponController.WeaponHandHash) {
                    weaponShortStr = "(W)";
                    var cfg = WeaponController.getConfigWeapon(player.CurrentWeapon);
                    weaponStr = $" und hat ein/eine {cfg} ausgerüstet.";
                }
                addCamLog(player, camId, position, player.Dimension, $"Bereich betreten {weaponShortStr}", $"Person hat den Kamerasichtbereich betreten{weaponStr}", DateTime.Now, player.Position);
            }

            return true;
        }

        private bool onPlayerExitCamRange(IPlayer player, string eventName, object[] args) {
            var camId = int.Parse(args[0].ToString());
            var position = new Position(float.Parse(args[1].ToString()), float.Parse(args[2].ToString()), float.Parse(args[3].ToString()));
            var playerPos = new Position(float.Parse(args[4].ToString()), float.Parse(args[5].ToString()), float.Parse(args[6].ToString()));

            if(player.IsInVehicle) {
                var vehicle = (ChoiceVVehicle)player.Vehicle;

                var posStr = "Fahrer";
                if(((ChoiceVVehicle)player.Vehicle).PassengerList.FirstOrDefault(p => p.Value == player).Key != 0) {
                    posStr = "Mitfahrer";
                }

                addCamLog(player, camId, position, player.Dimension, "Bereich abgefahren", $"Person hat den Kamerasichtbereich in Fahrzeug ({vehicle.DbModel.ModelName}:{vehicle.NumberplateText}) als {posStr} verlassen", DateTime.Now - TimeSpan.FromSeconds(2), playerPos);
            } else {
                addCamLog(player, camId, position, player.Dimension, "Bereich verlassen", "Person hat den Kamerasichtbereich verlassen", DateTime.Now - TimeSpan.FromSeconds(2), playerPos);
            }

            return true;
        }

        public static void checkIfCamSawAction(IPlayer player, string shortDescription, string description) {
            var count = CallbackCounter++;
            if(CharacterCameraActionCallbacks.ContainsKey(player.getCharacterId())) {
                var list = CharacterCameraActionCallbacks[player.getCharacterId()];
                list.Add(new CharacterCameraActioCheck(count, shortDescription, description));
            } else {
                var newList = new List<CharacterCameraActioCheck>();
                newList.Add(new CharacterCameraActioCheck(count, shortDescription, description));
                CharacterCameraActionCallbacks[player.getCharacterId()] = newList;
            }

            player.emitClientEvent("REQUEST_IF_CAM_VISIBLE", count);
        }

        private record AnswerCam(int id, float posX, float posY, float posZ);
        private bool onAnswerIfCamVisible(IPlayer player, string eventName, object[] args) {
            var requestId = int.Parse(args[0].ToString());
            var list = args[1].ToString().FromJson<List<AnswerCam>>();

            if(CharacterCameraActionCallbacks.ContainsKey(player.getCharacterId())) {
                var checks = CharacterCameraActionCallbacks[player.getCharacterId()];

                var check = checks.FirstOrDefault(c => c.Id == requestId);

                if(check != null && list != null && list.Count > 0) {
                    foreach(var cam in list) {
                        addCamLog(player, cam.id, new Position(cam.posX, cam.posY, cam.posZ), player.Dimension, check.ShortDescription, check.Description, DateTime.Now, player.Position);
                    }
                }
            }

            return true;
        }

        private static void addCamLog(IPlayer player, int camId, Position position, int dimension, string shortDescription, string description, DateTime createDate, Position playerPos) {
            var clothesStr = $"{player.getCharacterData().Gender}#{player.getClothing().ToJson()}";

            using(var db = new ChoiceVDb()) {
                var camlogs = new camlog {
                    camId = camId,
                    charId = player.getCharacterId(),
                    distance = (int)position.Distance(player.Position),
                    position = playerPos.ToJson(),
                    dimension = dimension,
                    shortDesc = shortDescription,
                    description = description,
                    clothes = clothesStr,
                    createTime = createDate,
                };

                db.camlogs.Add(camlogs);

                db.SaveChanges();
            }
        }


        #region AdminStuff

        private bool onAdminToggleCCTVMode(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.emitClientEvent("TOGGLE_CAM_FIND_MODE");

            return true;
        }


        private bool onFoundNewCam(IPlayer player, string eventName, object[] args) {
            lock(AllCams) {
                var model = args[0].ToString();
                var position = (Position)args[1];
                var heading = float.Parse(args[2].ToString());

                var already = AllCams.FirstOrDefault(c => c.Model == model && c.Position.Distance(position) < 1 && Math.Abs(heading - c.Heading) < 5);

                if(already == null) {
                    using(var db = new ChoiceVDb()) {
                        var newDbCam = new configcam {
                            model = model,
                            position = position.ToJson(),
                            heading = heading
                        };

                        db.configcams.Add(newDbCam);

                        db.SaveChanges();

                        AllCams.Add(new Cam(newDbCam.id, WorldController.getWorldGrid(position).Id, position, model, heading));

                        ChoiceVAPI.emitClientEventToAll("ADD_CAM", newDbCam.id, WorldController.getWorldGrid(position).Id, model, position, heading);
                    }
                }

                return true;
            }
        }

        #endregion
    }
}
