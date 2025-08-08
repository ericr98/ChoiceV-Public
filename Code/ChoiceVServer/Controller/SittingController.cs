using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public delegate void PlayerSitOnChairDelegate(IPlayer player, bool sitDown);

    public class SittingController : ChoiceVScript {
        public record SittingPosition(string Name, float ZOffset, float ForwardOffset, float SideOffset, float RotationOffset);
        public class SittingChair {
            public string ModelHash;
            public bool IsBed;

            public List<SittingPosition> AllPositions;

            public SittingChair(string modelHash, bool isBed, string name, float zOffset, float xOffset, float yOffset, float rotationOffset) {
                AllPositions = new List<SittingPosition> {
                    new(name, zOffset, xOffset, yOffset, rotationOffset)
                };

                ModelHash = modelHash;
                IsBed = isBed;
            }
        }

        private class SittingInfo {
            public Position Position;
            public string ModelHash;

            public SittingInfo(Position position, string modelHash) {
                Position = position;
                ModelHash = modelHash;
            }
        }

        private static Dictionary<string, PlayerSitOnChairDelegate> SittingDownCallbacks = new Dictionary<string, PlayerSitOnChairDelegate>();

        public static List<SittingChair> ConfigChairs = new List<SittingChair>();

        public static Dictionary<int, Position> UsedSeats = new Dictionary<int, Position>();

        private static readonly Dictionary<string, string> SittingAnimations = new Dictionary<string, string> {
            { "1", "Standard" }, { "2", "Zurückhaltend" }, { "3", "Beine überkreuzt" }, { "4", "Genervt gelehnt" },
            { "5", "Nach vorne gelehnt" }, { "6", "Melancholisch" }, { "7", "Lässig" }, { "8", "Gelehnt u. wippend" },
            { "9", "Locker" }, { "10", "Genervt" }, { "11", "Zuhörend (überkreuzt)" } };


        public SittingController() {
            EventController.addKeyEvent("STAND_UP", ConsoleKey.NumPad0, "Aufstehen", onStandUp, true);
            EventController.MainReadyDelegate += load;

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;
            EventController.PlayerDeadDelegate += onPlayerDead;

            CharacterSettingsController.addListCharacterSettingBlueprint(
                "CHAIR_SITTING_ANIMATION", "1", "Standard Sitzstil", "Wähle die Animation aus welche du beim Hinsetzen auf einen Stuhl abspielst",
                SittingAnimations
            , onPlayerSittingAnimationChange);

            EventController.addMenuEvent("ON_PLAYER_SELECT_CHAIR_POSITION", onPlayerSelectChairPosition);

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    "Sitzstil ändern",
                    getSittingAnimationMenu,
                    p => p.hasData("SITTING_DOWN")
                )
            );
            EventController.addMenuEvent("ON_SELECT_SITTING_STYLE", onSelectSittingStyle);

            #region Support Stuff

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ClickMenuItem("Sitz-Editor-Modus", "In diesem Modus können neue Stuhltypen registriert und vorhandene editiert werden", "", "SUPPORT_ACTIVATE_CHAIR_REGISTER_MODE"),
                    3,
                    SupportMenuCategories.Registrieren
                )
             );
            EventController.addMenuEvent("SUPPORT_ACTIVATE_CHAIR_REGISTER_MODE", onSupportActivateChairRegisterMode);
            EventController.addMenuEvent("SUPPORT_ON_DELETE_CONFIGCHAIR", onSupportOnDeleteConfigChair);
            EventController.addMenuEvent("SUPPORT_ON_EDIT_CONFIGCHAIR", onSupportEditConfigChair);
            EventController.addMenuEvent("SUPPORT_ON_CREATE_CONFIGCHAIR", onSupportCreateConfigChair);

            #endregion
        }

        #region Support Stuff

        private bool onSupportActivateChairRegisterMode(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            SupportController.setCurrentSupportFastAction(player, () => onSupportActivateChairRegisterMode(player, null, 0, null, null));

            player.sendNotification(Constants.NotifactionTypes.Info, "Sitz-Editor-Modus aktiviert. Wähle ein Objekt aus, auf welches sich gesetzt werden soll.", "");

            InteractionController.activateAllObjectInteractionMode(player, (hash, position) => {
                using(var db = new ChoiceVDb()) {
                    var already = db.configchairs.Where(c => c.modelHash == hash).OrderBy(c => c.seatName).ToList();

                    var menu = new Menu("Sitz-Editor-Menü", "Erstelle Sitze oder editiere sie");
                    menu.addMenuItem(new StaticMenuItem("Information", "Es werden alle Sitze aufgezeigt die auf dem Objekt möglich sind. Um das Objekt \"unbesitzbar\" zu machen, müssen alle Sitze entfernt werden", ""));

                    foreach(var seat in already) {
                        var subMenu = new Menu($"{seat.seatName}", "Was möchtest du tun?");
                        subMenu.addMenuItem(new InputMenuItem("Name", "Ändere den Namen des Sitzes. Sitze sind immer aus der Sicht von vorne zu bennennen, und zusätzlich sollten sie nummeriert werden von links nach rechts. Bei zwei Sitzen: 1: Links, 2: Rechts, bei drei Sitzen: 1: Links, 2: Mitte, 3: Rechts, bei vier Sitzen: 1: Links Außen, 2: Links Innen, ...", $"Aktuell: {seat.seatName}", "").withStartValue(seat.seatName.ToString()));
                        subMenu.addMenuItem(new InputMenuItem("Z-Offset", "Das Z-Offset bestimmt wie hoch der Spieler auf dem Stuhl sitzt. Normal sollte die Zahl um die 0.5 sein.", $"Aktuell: {seat.zOffset}", "").withStartValue(seat.zOffset.ToString()));
                        subMenu.addMenuItem(new InputMenuItem("X-Offset", "Das X-Offset bestimmt wie weit vorne oder hinten der Spieler auf dem Stuhl sitzt. (AUFPASSEN! Sitzt der Spieler zu weit hinten, kann die Kamera komisch in den Stuhl buggen). Je nach Stuhl sind hier so ca. 0 - 0.2 einzutragen", $"Aktuell: {seat.xOffset}", "").withStartValue(seat.xOffset.ToString()));
                        subMenu.addMenuItem(new InputMenuItem("Y-Offset", "Das Y-Offset gibt an wie weit links oder rechts auf der Sitzgelegenheit der Spieler sitzt. Bei Stühlen ist dies meistens null, bei Bänken (mit mehreren Plätzen) wird dieses Offset aber bestimmen wie weit links/rechts der Spieler auf der z.B. Couch sitzt. Da die Objekte meist symmetrisch sind, bedeutet das oftmals: 1: Links mit Y-Offset 0.5, => 2: Rechts mit Y-Offset -0.5", $"Aktuell: {seat.yOffset}", "").withStartValue(seat.yOffset.ToString()));
                        subMenu.addMenuItem(new InputMenuItem("Rotations-Offset", "Das Rotations-Offset muss genutzt werden, wenn der Spieler falsch herum auf dem Stuhl sitzt. Es ist in Grad ° (z.B. 90) anzugeben. Auch kann es benutzt werden wenn es sich um eine Eck-Couch handelt.", $"Aktuell: {seat.rotOffset}", "").withStartValue(seat.rotOffset.ToString()));
                        subMenu.addMenuItem(new InputMenuItem("Name des Stuhls", "Falls bekannt hier bitte den GTA-Namen des Stuhls eingeben. Wenn dieser nicht bekannt, dann bitte eine ganz kurze Beschreibung des Stuhls: z.B. Barhocker (rund)", $"Aktuell: {seat.comment}", "").withStartValue(seat.comment));
                        subMenu.addMenuItem(new CheckBoxMenuItem("Ist Bett", "Ist der Sitz ein Bett?", seat.isBed == 1, ""));

                        subMenu.addMenuItem(new MenuStatsMenuItem("Editieren", "Editiere die angegebenen Felder. Falls ein Feld nicht editiert werden soll, sollte es freigelassen werden!", "SUPPORT_ON_EDIT_CONFIGCHAIR", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "ConfigChair", seat }, { "ModelHash", hash } }));
                        subMenu.addMenuItem(new ClickMenuItem("Löschen", "Lösche den angegebenen Sitz", "", "SUPPORT_ON_DELETE_CONFIGCHAIR", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "ConfigChair", seat } }).needsConfirmation("Sitz löschen?", "Sitz wirklich löschen?"));

                        menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                    }

                    var createMenu = new Menu($"Sitz erstellen", "Was möchtest du tun?");
                    createMenu.addMenuItem(new InputMenuItem("Name", "Ändere den Namen des Sitzes. Sitze sind immer aus der Sicht von vorne zu bennennen, und zusätzlich sollten sie nummeriert werden von links nach rechts. Bei zwei Sitzen: 1: Links, 2: Rechts, bei drei Sitzen: 1: Links, 2: Mitte, 3: Rechts, bei vier Sitzen: 1: Links Außen, 2: Links Innen, ...", $"", ""));
                    createMenu.addMenuItem(new InputMenuItem("Z-Offset", "Das Z-Offset bestimmt wie hoch der Spieler auf dem Stuhl sitzt. Normal sollte die Zahl um die 0.5 sein.", $"Standard: 0.5", ""));
                    createMenu.addMenuItem(new InputMenuItem("X-Offset", "Das X-Offset bestimmt wie weit vorne oder hinten der Spieler auf dem Stuhl sitzt. (AUFPASSEN! Sitzt der Spieler zu weit hinten, kann die Kamera komisch in den Stuhl buggen). Je nach Stuhl sind hier so ca. 0 - 0.2 einzutragen", $"Standard: 0", ""));
                    createMenu.addMenuItem(new InputMenuItem("Y-Offset", "Das Y-Offset gibt an wie weit links oder rechts auf der Sitzgelegenheit der Spieler sitzt. Bei Stühlen ist dies meistens null, bei Bänken (mit mehreren Plätzen) wird dieses Offset aber bestimmen wie weit links/rechts der Spieler auf der z.B. Couch sitzt. Da die Objekte meist symmetrisch sind, bedeutet das oftmals: 1: Links mit Y-Offset 0.5, => 2: Rechts mit Y-Offset -0.5", $"Standard: 0", ""));
                    createMenu.addMenuItem(new InputMenuItem("Rotations-Offset", "Das Rotations-Offset muss genutzt werden, wenn der Spieler falsch herum auf dem Stuhl sitzt. Es ist in Grad ° (z.B. 90) anzugeben. Auch kann es benutzt werden wenn es sich um eine Eck-Couch handelt.", $"Standard: 0", ""));
                    createMenu.addMenuItem(new InputMenuItem("Name des Stuhls", "Falls bekannt hier bitte den GTA-Namen des Stuhls eingeben. Wenn dieser nicht bekannt, dann bitte eine ganz kurze Beschreibung des Stuhls: z.B. Barhocker (rund)", $"", ""));
                    createMenu.addMenuItem(new CheckBoxMenuItem("Ist Bett", "Ist der Sitz ein Bett?", false, ""));

                    createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Editiere die angegebenen Felder. Falls ein Feld nicht editiert werden soll, sollte es freigelassen werden!", "SUPPORT_ON_CREATE_CONFIGCHAIR", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "ModelHash", hash } }));

                    menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

                    player.showMenu(menu);
                }
            });

            return true;
        }

        private bool onSupportEditConfigChair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var seat = (configchair)data["ConfigChair"];
            var form = getDataFromForm(menuItemCefEvent as MenuStatsMenuItemEvent);

            var str = "";
            using(var db = new ChoiceVDb()) {
                var find = db.configchairs.FirstOrDefault(c => c.modelHash == seat.modelHash && c.seatName == seat.seatName);

                if(form["name"] != "") {
                    str += "Name, ";
                    find.seatName = form["name"];
                }

                if(form["zOffset"] != -10000) {
                    str += "Z-Offset, ";
                    find.zOffset = form["zOffset"];
                }

                if(form["xOffset"] != -10000) {
                    str += "X-Offset, ";
                    find.xOffset = form["xOffset"];
                }

                if(form["yOffset"] != -10000) {
                    str += "Y-Offset, ";
                    find.yOffset = form["yOffset"];
                }

                if(form["rotOffset"] != -10000) {
                    str += "Rotations-Offset, ";
                    find.rotOffset = form["rotOffset"];
                }

                if(form["comment"] != "") {
                    str += "Kommentar, ";
                    find.comment = form["comment"];
                }

                if(str.Length > 2) {
                    str = str.Substring(0, str.Length - 2);
                }

                find.isBed = form["isBed"] ? 1 : 0;

                db.SaveChanges();

                load();
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Sitzplatz erfolgreich editiert. Es wurden {str} angepasst", "");

            return true;
        }

        private bool onSupportOnDeleteConfigChair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var seat = (configchair)data["ConfigChair"];

            using(var db = new ChoiceVDb()) {
                var find = db.configchairs.FirstOrDefault(c => c.modelHash == seat.modelHash && c.seatName == seat.seatName);

                db.configchairs.Remove(find);

                load();
                db.SaveChanges();
            }

            player.sendNotification(Constants.NotifactionTypes.Warning, "Sitzgelegenheit erfolgreich gelöscht", "");

            return true;
        }

        private bool onSupportCreateConfigChair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var form = getDataFromForm(menuItemCefEvent as MenuStatsMenuItemEvent);

            using(var db = new ChoiceVDb()) {
                var newSeat = new configchair {
                    modelHash = (string)data["ModelHash"],
                    seatName = form["name"],
                    zOffset = form["zOffset"],
                    xOffset = form["xOffset"],
                    yOffset = form["yOffset"],
                    rotOffset = form["rotOffset"],
                    comment = form["comment"],
                    isBed = form["isBed"] ? 1 : 0
                };

                db.configchairs.Add(newSeat);
                db.SaveChanges();

                load();
                player.sendNotification(Constants.NotifactionTypes.Success, "Sitzgelegenheit erfolgreich erstellt", "");

            }

            return true;
        }

        private Dictionary<string, dynamic> getDataFromForm(MenuStatsMenuItemEvent evt) {
            var dic = new Dictionary<string, dynamic> { };

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            dic.Add("name", nameEvt.input ?? "");

            var zOffsetEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            dic.Add("zOffset", float.Parse(zOffsetEvt.input ?? "-10000"));

            var xOffsetEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            dic.Add("xOffset", float.Parse(xOffsetEvt.input ?? "-10000"));

            var yOffsetEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
            dic.Add("yOffset", float.Parse(yOffsetEvt.input ?? "-10000"));

            var rotOffsetEvt = evt.elements[4].FromJson<InputMenuItemEvent>();
            dic.Add("rotOffset", float.Parse(rotOffsetEvt.input ?? "-10000"));

            var commentEvt = evt.elements[5].FromJson<InputMenuItemEvent>();
            dic.Add("comment", commentEvt.input ?? "");

            var isBedEvt = evt.elements[6].FromJson<CheckBoxMenuItemEvent>();
            dic.Add("isBed", isBedEvt.check);

            return dic;
        }

        #endregion

        private void onPlayerDisconnect(IPlayer player, string reason) {
            var charId = player.getCharacterId();
            if(UsedSeats.ContainsKey(charId)) {
                UsedSeats.Remove(charId);
            }
        }

        private void onPlayerDead(IPlayer player, IEntity killer, uint weapon) {
            var charId = player.getCharacterId();
            if(UsedSeats.ContainsKey(charId)) {
                UsedSeats.Remove(charId);
            }
        }

        public static void load() {
            ConfigChairs.Clear();
            using(var db = new ChoiceVDb()) {
                foreach(var chair in db.configchairs) {
                    var first = ConfigChairs.FirstOrDefault(c => c.ModelHash == chair.modelHash);
                    if(first == null) {
                        ConfigChairs.Add(new SittingChair(chair.modelHash, chair.isBed == 1, chair.seatName, chair.zOffset, chair.xOffset, chair.yOffset, chair.rotOffset));
                    } else {
                        first.AllPositions.Add(new SittingPosition(chair.seatName, chair.zOffset, chair.xOffset, chair.yOffset, chair.rotOffset));
                    }

                    if(chair.comment?.ToLower().Contains("toilet") ?? false) {
                        addChairSittingCallback(chair.modelHash, onPlayerSitOnToilet);
                    }
                }
            }

            InteractionController.addObjectInteractionCallback("INTERACT_CHAIR", "Stuhl Interaktion", onChairInteraction, true);

            InteractionController.addInteractableObjects(ConfigChairs.Select(c => c.ModelHash).ToList(), "INTERACT_CHAIR");

            EventController.addMenuEvent("PLAYER_SIT_ON_CHAIR", onPlayerSitOnChair);
        }

        private static bool onPlayerSitOnChair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var objectPosition = (Position)data["ObjectPos"];
            var chair = (SittingChair)data["SittingChair"];
            var sitPos = (SittingPosition)data["SittingPosition"];
            var objectHeading = (float)data["Heading"];
            var modelHash = (string)data["ModelHash"];

            onChairSitting(player, objectPosition, chair.ModelHash, chair.IsBed, objectHeading, sitPos.ZOffset, sitPos.ForwardOffset, sitPos.SideOffset, sitPos.RotationOffset, new DegreeRotation(0, 0, objectHeading));
            return true;
        }

        private static void onPlayerSitOnToilet(IPlayer player, bool sitDown) {
            if(sitDown) {
                var cloth = player.getInventory().getItem<ClothingItem>(c => c.ComponentId == 4 && c.IsEquipped);

                if(cloth != null) {
                    cloth.unequip(player);
                    player.setData("TOILET_PANTS", cloth.Id);
                }
            } else {
                if(player.hasData("TOILET_PANTS")) {
                    var id = (int)player.getData("TOILET_PANTS");
                    var cloth = player.getInventory().getItem<ClothingItem>(c => c.Id == id);

                    if(cloth != null) {
                        cloth.equip(player);
                        player.resetData("TOILET_PANTS");
                    }
                }
            }
        }

        private static void onChairInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            var chair = SittingController.ConfigChairs.FirstOrDefault(s => s.ModelHash == modelName);

            if(chair.AllPositions.Count <= 1) {
                var sitPos = chair.AllPositions[0];

                menu.addMenuItem(new ClickMenuItem("Hinsetzen", "Setze dich auf den Stuhl", "", "PLAYER_SIT_ON_CHAIR").withData(new Dictionary<string, dynamic> {
                    { "ObjectPos", objectPosition },
                    { "ModelHash", modelName },
                    { "Heading", objectHeading },
                    { "Rotation", new DegreeRotation(0, 0, objectHeading) },
                    { "SittingChair", chair },
                    { "SittingPosition", sitPos }
                }));
            } else {
                var menuSitting = new Menu("Sitzposition wählen", "Wo möchtest du dich hinsetzen?");
                foreach(var sitPos in chair.AllPositions) {
                    var vec = ChoiceVAPI.getForwardVector(ChoiceVAPI.degreesToRadians(objectHeading + sitPos.RotationOffset), true);

                    var rightVec = Vector2.Normalize(new Vector2(
                        (float)Math.Cos((Math.PI / 2)) * vec.X - (float)Math.Sin((Math.PI / 2)) * vec.Y,
                        (float)Math.Sin((Math.PI / 2)) * vec.X + (float)Math.Cos((Math.PI / 2)) * vec.Y
                    ));

                    //If Female char is wearing heels, then remove like 0.1 from zOffset
                    var pos = new Position(objectPosition.X + sitPos.ForwardOffset * vec.X + sitPos.SideOffset * rightVec.X, objectPosition.Y + sitPos.ForwardOffset * vec.Y + sitPos.SideOffset * rightVec.Y, objectPosition.Z + sitPos.ZOffset);

                    if(!UsedSeats.Any(s => s.Value.Distance(pos) <= 0.25)) {
                        var data = new Dictionary<string, dynamic> {
                            { "ObjectPos", objectPosition },
                            { "ModelHash", modelName },
                            { "Heading", objectHeading },
                            { "Rotation", new DegreeRotation(0, 0, objectHeading) },
                            { "SittingChair", chair },
                            { "SittingPosition", sitPos }
                        };
                        menuSitting.addMenuItem(new ClickMenuItem(sitPos.Name, "Wähle diese Position", "", "ON_PLAYER_SELECT_CHAIR_POSITION").withData(data));
                    }
                }
                menu.addMenuItem(new MenuMenuItem(menuSitting.Name, menuSitting));
            }
        }

        private bool onPlayerSelectChairPosition(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var objectPos = (Position)data["ObjectPos"];
            var modelHash = (string)data["ModelHash"];
            var heading = (float)data["Heading"];
            var rotation = (DegreeRotation)data["Rotation"];
            var chair = (SittingChair)data["SittingChair"];
            var sitPos = (SittingPosition)data["SittingPosition"];

            onChairSitting(player, objectPos, modelHash, chair.IsBed, heading, sitPos.ZOffset, sitPos.ForwardOffset, sitPos.SideOffset, sitPos.RotationOffset, rotation);

            return true;
        }

        public static void addChairSittingCallback(string modelHash, PlayerSitOnChairDelegate callback) {
            SittingDownCallbacks[modelHash] = callback;
        }

        public static void onChairSitting(IPlayer player, Position objectPos, string modelHash, bool isBed, float heading, float zOffset, float forwardOffset, float sideOffset, float rotationOffset, DegreeRotation rotation) {
            //if(rotation.Roll > 45 || rotation.Roll < -45 || rotation.Pitch > 45 || rotation.Pitch < -45) {
            //    player.sendBlockNotification("Der Stuhl steht zu schief um sich auf ihn zu setzen!", "Stuhl zu schief!");
            //    return;
            //}

            if(CarryController.isPersonCarrier(player)) {
                var sitter = CarryController.getCarriedPlayer(player);
                CarryController.removeCarryState(player, sitter);
                player = sitter;
            }

            var vec = ChoiceVAPI.getForwardVector(ChoiceVAPI.degreesToRadians(heading + rotationOffset), true);

            var rightVec = Vector2.Normalize(new Vector2(
                (float)Math.Cos((Math.PI / 2)) * vec.X - (float)Math.Sin((Math.PI / 2)) * vec.Y,
                (float)Math.Sin((Math.PI / 2)) * vec.X + (float)Math.Cos((Math.PI / 2)) * vec.Y
            ));

            //TODO If Female char is wearing heels, then remove like 0.1 from zOffset
            //Maybe with Callback: CPED_CONFIG_FLAG_HasHighHeels as CallbackController
            var pos = new Position(objectPos.X + forwardOffset * vec.X + sideOffset * rightVec.X, objectPos.Y + forwardOffset * vec.Y + sideOffset * rightVec.Y, objectPos.Z + zOffset);
            var rot = new Rotation(0, 0, heading + 180 + rotationOffset);

            var charId = player.getCharacterId();
            var usedChair = UsedSeats.FirstOrDefault(s => s.Value.Distance(pos) <= 0.25);
            if(!object.Equals(usedChair, default(KeyValuePair<int, Position>))) {
                var user = ChoiceVAPI.FindPlayerByCharId(usedChair.Key);
                if(user != null) {
                    if(user.Position.Distance(usedChair.Value) >= 1.5 || charId == usedChair.Key) {
                        UsedSeats.Remove(usedChair.Key);
                    } else {
                        return;
                    }
                } else {
                    UsedSeats.Remove(usedChair.Key);
                }
            }
            player.setData("SITTING_DOWN", new SittingInfo(player.Position, modelHash));

            AnimationController.playScenarioAtPosition(player, "PROP_HUMAN_SEAT_BENCH", pos, rot.Yaw, 10000000, false, true);

            //ANIM WITH FLAG 33 DOES NOT CANCEL SCENARIO!!!!
            Animation anim;
            if(isBed) {
                anim = new Animation("anim@gangops@morgue@table@", "ko_front", TimeSpan.FromSeconds(1000000), 33, 0);
                player.addState(Constants.PlayerStates.LayingDown);
            } else {
                var idx = player.getCharSetting("CHAIR_SITTING_ANIMATION");
                anim = AnimationController.getAnimationByName("CHAIR_SITTING_" + idx);
            }

            anim.Flag = 33;
            player.playBackgroundAnimation(anim, new Rotation(0, 0, heading + 180));

            player.setData("SITTING_DOWN", new SittingInfo(player.Position, modelHash));

            if(SittingDownCallbacks.ContainsKey(modelHash)) {
                SittingDownCallbacks[modelHash]?.Invoke(player, true);
            }

            UsedSeats[player.getCharacterId()] = pos;
        }

        public static bool isPlayerSittingOnChair(IPlayer player) {
            if(player.hasData("SITTING_DOWN")) {
                var info = (SittingInfo)player.getData("SITTING_DOWN");
                return UsedSeats.Any(s => s.Value.Distance(player.Position) <= 1);
            } else {
                return false;
            }
        }

        public static bool isPlayerSittingOnChair(IPlayer player, string modelHash) {
            if(player.hasData("SITTING_DOWN")) {
                var info = (SittingInfo)player.getData("SITTING_DOWN");
                return info.ModelHash == modelHash && UsedSeats.Any(s => s.Value.Distance(player.Position) <= 1);
            } else {
                return false;
            }
        }

        private bool onStandUp(IPlayer player, ConsoleKey key, string eventName) {
            if(!player.getBusy(new List<Constants.PlayerStates> { Constants.PlayerStates.LayingDown })) {
                if(player.hasData("SITTING_DOWN")) {
                    AnimationController.stopBackgroundAnimation(player);
                    AnimationController.stopScenario(player);
                    player.removeState(Constants.PlayerStates.LayingDown);

                    if(UsedSeats.Any(s => s.Value.Distance(player.Position) <= 2)) {
                        var info = (SittingInfo)player.getData("SITTING_DOWN");
                        player.Position = info.Position;
                        UsedSeats.RemoveWhere(s => s.Key == player.getCharacterId());

                        if(SittingDownCallbacks.ContainsKey(info.ModelHash)) {
                            SittingDownCallbacks[info.ModelHash]?.Invoke(player, false);
                        }
                    }
                    player.resetData("SITTING_DOWN");
                    return true;
                }
            }

            return false;
        }

        public static void removeFromSitting(IPlayer player) {
            if(player.hasData("SITTING_DOWN")) {
                if(UsedSeats.Any(s => s.Value.Distance(player.Position) <= 1)) {
                    var info = (SittingInfo)player.getData("SITTING_DOWN");
                    player.Position = info.Position;
                    UsedSeats.RemoveWhere(s => s.Key == player.getCharacterId());

                    player.removeState(Constants.PlayerStates.LayingDown);
                }

                player.resetData("SITTING_DOWN");
            }
        }

        private void onPlayerSittingAnimationChange(IPlayer player, string settingName, string value) {
            if(player.hasData("SITTING_DOWN")) {
                var idx = player.getCharSetting("CHAIR_SITTING_ANIMATION");
                var anim = AnimationController.getAnimationByName("CHAIR_SITTING_" + idx);
                anim.Flag = 33;
                player.playBackgroundAnimation(anim, null);
            }
        }

        private Menu getSittingAnimationMenu(IPlayer player) {
            var menu = new Menu("Sitzanimation wählen", "Wähle deine Sitzanimation aus");

            foreach(var el in SittingAnimations) {
                menu.addMenuItem(new ClickMenuItem(el.Value, $"Wähle den Sitzstil: {el.Value}", "", "ON_SELECT_SITTING_STYLE").withNotCloseOnAction().withData(new Dictionary<string, dynamic> { { "Id", el.Key } }));
            }

            return menu;
        }

        private bool onSelectSittingStyle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var id = int.Parse((string)data["Id"]);

            if(player.hasData("SITTING_DOWN")) {
                var anim = AnimationController.getAnimationByName("CHAIR_SITTING_" + id);
                anim.Flag = 33;
                player.playBackgroundAnimation(anim, null);
            }

            return true;
        }
    }
}
