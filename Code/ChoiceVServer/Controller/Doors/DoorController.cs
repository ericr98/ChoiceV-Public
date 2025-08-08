using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class DoorController : ChoiceVScript {
        public static Dictionary<Position, Door> AllDoorsByPosition = new Dictionary<Position, Door>();
        private static List<Door> FreshlyRegisteredDoors = new List<Door>();

        public DoorController() {
            InteractionController.addObjectInteractionCallback(
                "INTERACT_DOOR",
                null,
                onInteractWithDoor
            );
            EventController.addMenuEvent("INTERACT_WITH_DOOR_MENU", onDoorMenuInteraction);

            EventController.PlayerPreSuccessfullConnectionDelegate += onPlayerConnect;

            Door.DoorAdditonalDimensionChange += onDoorAdditionalDimensionChange;

            EventController.addKeyEvent("DOOR_CONTROL", ConsoleKey.L, "Türen auf/abschließen", onLockUnlockDoorKeyPress);

            loadDoors();

            #region Support Stuff

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new ClickMenuItem("reg. Türen anzeigen/verstecken", "Zeige alle registrierte Türen an", "", "SUPPORT_TOOGLE_SHOW_DOOR"),
                    1,
                    SupportMenuCategories.TürSystem
                )
            );

            SupportController.addSupportMenuElement(
                new StaticSupportMenuElement(
                    () => new InputMenuItem("Tür-Registriermodus", "Gib die Türgruppe an. Lasse es leer um keine Gruppe zu setzen", "", "SUPPORT_TOOGLE_REGISTER_DOOR"),
                    3,
                    SupportMenuCategories.TürSystem
                )
            );

            EventController.addMenuEvent("SUPPORT_TOOGLE_SHOW_DOOR", onSupportToggleShowDoor);
            EventController.addMenuEvent("SUPPORT_TOOGLE_REGISTER_DOOR", onSupportToggleRegisterDoor);
            EventController.addMenuEvent("SUPPORT_CHANGE_DOOR_GROUP", onSupportChangeDoorGroup);
            EventController.addMenuEvent("SUPPORT_CHANGE_DOOR_COMBO", onSupportChangeDoorCombo);

            EventController.addMenuEvent("SUPPORT_GENERATE_DOOR_KEY", onSupportGenerateDoorKey);
            EventController.addMenuEvent("SUPPORT_ADD_DOOR_TO_NEW_COMBO_GROUP", onSupportAddDoorToNewComboGroup);
            EventController.addMenuEvent("SUPPORT_REMOVE_DOOR_COMBO_GROUP", onSupportRemoveDoorComboGroup);
            EventController.addMenuEvent("SUPPORT_SELECT_DOOR_COMBO_GROUP", onSupportSelectDoorComboGroup);
            EventController.addMenuEvent("SUPPORT_CHANGE_DOOR_LOCK_INDEX", onSupportChangeDoorLockIndex);

            #endregion
        }

        private bool onLockUnlockDoorKeyPress(IPlayer player, ConsoleKey key, string eventName) {
            var keys = player.getInventory().getItems<DoorKey>(k => true);
            foreach(var doorEntry in AllDoorsByPosition) {
                var door = doorEntry.Value;
                if(door.Position.Distance(player.Position) < 3 && door.canPlayerOpen(player, keys)) {
                    if(door.isLocked(player.Dimension)) {
                        door.unlockForDimension(player.Dimension);
                        player.sendNotification(Constants.NotifactionTypes.Info, "Du hast eine Tür aufgeschlossen", "Tür aufgeschlossen", Constants.NotifactionImages.Door);
                    } else {
                        door.lockForDimension(player.Dimension);
                        player.sendNotification(Constants.NotifactionTypes.Info, "Du hast eine Tür abgeschlossen", "Tür abgeschlossen", Constants.NotifactionImages.Door);
                    }
                    return true;
                }
            }

            return false;
        }

        #region Support Stuff

        private bool onSupportToggleShowDoor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.hasData("SUPPORT_DOOR_SHOWN_MODE")) {
                player.resetData("SUPPORT_DOOR_SHOWN_MODE");
                player.emitClientEvent("TOOGLE_DOOR_INFO", false);

                player.sendNotification(Constants.NotifactionTypes.Info, "Die registrierten Türen werden nun nicht mehr hervorgehoben", "");
            } else {
                player.emitClientEvent("TOGGLE_DOOR_SHOWN", true);

                var ids = new List<int>();
                var labels = new List<string>();

                foreach(var door in AllDoorsByPosition.Values) {
                    ids.Add(door.Id);

                    labels.Add($"Id: {door.Id}\nGruppe: {door.GroupName}\nCombos: {door.getComboDoorAmount()}\nSchloss-Index: {door.LockIndex}");
                }

                player.emitClientEvent("SET_DOOR_LABELS", ids.ToArray(), labels.ToArray());
                player.emitClientEvent("TOOGLE_DOOR_INFO", true);
                player.setData("SUPPORT_DOOR_SHOWN_MODE", true);

                player.sendNotification(Constants.NotifactionTypes.Info, "Die registrierten Türen werden nun hervorgehoben", "");

                Logger.logTrace(LogCategory.Support, LogActionType.Viewed, player, "Player started the admin-door mode");
            }

            return true;
        }


        private bool onSupportToggleRegisterDoor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            SupportController.setCurrentSupportFastAction(player, () => { onSupportToggleRegisterDoor(player, null, 0, null, menuItemCefEvent); });
            var inputEvt = menuItemCefEvent as InputMenuItemEvent;

            startDoorRegistrationWithCallback(player, inputEvt.input, (door) => {
                SupportController.setCurrentSupportFastAction(player, () => {
                    onSupportToggleRegisterDoor(player, itemEvent, menuItemId, data, menuItemCefEvent);
                });
                player.sendNotification(Constants.NotifactionTypes.Info, "Tür erfolgreich registriert", "");

                //Toggle twice to refresh
                onSupportToggleShowDoor(player, null, 0, null, null);
                onSupportToggleShowDoor(player, null, 0, null, null);
            });

            return true;
        }

        private bool onSupportChangeDoorGroup(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var door = (Door)data["Door"];
            var evt = menuItemCefEvent as InputMenuItemEvent;

            using(var db = new ChoiceVDb()) {
                var dbDoor = db.configdoors.Find(door.Id);

                if(dbDoor != null) {
                    dbDoor.doorGroup = evt.input;
                    db.SaveChanges();
                } else {
                    player.sendBlockNotification("Die Tür wurde nicht gefunden?", "");
                    return false;
                }
            }
            var prevGroup = door.GroupName;
            door.GroupName = evt.input;

            player.sendNotification(Constants.NotifactionTypes.Info, $"Die Türgruppe von Tür {door.Id} wurde von {prevGroup} zu {evt.input} geändert", "");

            Logger.logDebug(LogCategory.Support, LogActionType.Updated, player, $"Player changed door {door.Id}s id from {prevGroup} to {evt.input}");

            //Toggle twice to refresh
            onSupportToggleShowDoor(player, null, 0, null, null);
            onSupportToggleShowDoor(player, null, 0, null, null);

            return true;
        }

        private bool onSupportChangeDoorCombo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var door = (Door)data["Door"];

            var menu = new Menu("Kombogruppe wählen", "Wähle eine Kombogruppe");
            using(var db = new ChoiceVDb()) {
                var dbDoor = db.configdoors.Include(d => d.comboGroup).FirstOrDefault(d => d.id == door.Id);
                if(dbDoor.comboGroup != null) {
                    menu.addMenuItem(new StaticMenuItem("Aktuelle Kombo", $"Die aktuelle Kombo der Tür ist {dbDoor.comboGroup.description}", dbDoor.comboGroup.description));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Aktuell keine Kombo gesetzt", $"Aktuell keine Kombo gesetzt", "", MenuItemStyle.yellow));
                }
                menu.addMenuItem(new InputMenuItem("Neue Gruppe", "Erstelle eine neue Gruppe", "", "SUPPORT_ADD_DOOR_TO_NEW_COMBO_GROUP", MenuItemStyle.green)
                   .withData(new Dictionary<string, dynamic> { { "Door", door } }));

                menu.addMenuItem(new ClickMenuItem("Keine Gruppe", "Entferne die Kombogruppe von der Tür", "", "SUPPORT_REMOVE_DOOR_COMBO_GROUP", MenuItemStyle.yellow)
                    .withData(new Dictionary<string, dynamic> { { "Door", door } }));

                foreach(var group in db.configdoorcombos.Include(c => c.configdoors)) {
                    menu.addMenuItem(new ClickMenuItem(group.description, $"Füge die Tür der Gruppe {group.description} hinzu", $"Aktuell: {group.configdoors.Count}", "SUPPORT_SELECT_DOOR_COMBO_GROUP")
                        .withData(new Dictionary<string, dynamic> { { "Door", door }, { "Group", group } }));
                }
            }

            player.showMenu(menu);

            return true;
        }

        private bool onSupportAddDoorToNewComboGroup(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var door = (Door)data["Door"];
            var evt = menuItemCefEvent as InputMenuItemEvent;

            using(var db = new ChoiceVDb()) {
                var newGroup = new configdoorcombo {
                    description = evt.input,
                };
                db.configdoorcombos.Add(newGroup);
                db.SaveChanges();

                var dbDoor = db.configdoors.Find(door.Id);
                if(dbDoor != null) {
                    dbDoor.comboGroup = newGroup;
                    db.SaveChanges();
                } else {
                    player.sendBlockNotification("Die Tür wurde nicht gefunden?", "");
                    return false;
                }
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Erfolgreich die Tür {door.Id} zu einer neuen Gruppe mit Namen {evt.input} hinzugefügt", "");

            Logger.logDebug(LogCategory.Support, LogActionType.Updated, player, $"Player added door {door.Id}s to newly created combo group {evt.input}");

            //Toggle twice to refresh
            onSupportToggleShowDoor(player, null, 0, null, null);
            onSupportToggleShowDoor(player, null, 0, null, null);

            return true;
        }

        private bool onSupportRemoveDoorComboGroup(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var door = (Door)data["Door"];

            door.removeComboDoor(door);

            using(var db = new ChoiceVDb()) {
                var dbDoor = db.configdoors.Find(door.Id);

                if(dbDoor != null) {
                    dbDoor.comboGroupId = null;
                    db.SaveChanges();
                } else {
                    player.sendBlockNotification("Die Tür wurde nicht gefunden?", "");
                    return false;
                }
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Erfolgreich die Tür {door.Id} aus einer Kombogruppe entfernt", "");

            Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"Player removed door {door.Id}s from combo groups");

            //Toggle twice to refresh
            onSupportToggleShowDoor(player, null, 0, null, null);
            onSupportToggleShowDoor(player, null, 0, null, null);

            return true;
        }

        private bool onSupportSelectDoorComboGroup(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var door = (Door)data["Door"];
            var combo = (configdoorcombo)data["Group"];

            foreach(var dbDoor in combo.configdoors) {
                var otherDoor = AllDoorsByPosition.Values.FirstOrDefault(d => d.Id == dbDoor.id);

                door.addComboDoor(otherDoor);
                otherDoor.addComboDoor(door);
            }

            using(var db = new ChoiceVDb()) {
                var dbDoor = db.configdoors.Find(door.Id);

                if(dbDoor != null) {
                    dbDoor.comboGroupId = combo.id;
                    db.SaveChanges();
                } else {
                    player.sendBlockNotification("Die Tür wurde nicht gefunden?", "");
                    return false;
                }
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Erfolgreich die Tür {door.Id} zu der Kombogruppe {combo.description} hinzugefügt", "");

            Logger.logDebug(LogCategory.Support, LogActionType.Updated, player, $"Player added door {door.Id}s to combo group {combo.description}");

            //Toggle twice to refresh
            onSupportToggleShowDoor(player, null, 0, null, null);
            onSupportToggleShowDoor(player, null, 0, null, null);

            return true;
        }

        private bool onSupportGenerateDoorKey(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var door = (Door)data["Door"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var descEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var groupEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var lockIdxEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var lockIdx = lockIdxEvt.input != "" && lockIdxEvt.input != null ? int.Parse(lockIdxEvt.input) : door.LockIndex;

            var amountEvt = evt.elements[3].FromJson<InputMenuItemEvent>();


            var amount = 1;
            if(amountEvt.input != "" && amountEvt.input != null) {
                amount = int.Parse(amountEvt.input);
            }

            while(amount > 0) {
                amount--;

                var cfg = InventoryController.getConfigItemForType<DoorKey>();
                DoorKey key;

                if(groupEvt.input == null || groupEvt.input == "") {
                    key = new DoorKey(cfg, lockIdx, player.Dimension, "", door.Id, descEvt.input);
                } else {
                    key = new DoorKey(cfg, lockIdx, player.Dimension, groupEvt.input, -1, $"{descEvt.input}, Schloss-Idx: ({lockIdx})");
                }

                player.getInventory().addItem(key, true);
                Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"Player created Doorkey {key.Id} with group {groupEvt.input} at door {door.Id}"); ;
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Dir wurden {amountEvt.input} Türschlüssel mit Beschreibung \"{descEvt.input}\" hinzuzgefügt", "");

            return true;
        }

        private bool onSupportChangeDoorLockIndex(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var door = (Door)data["Door"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            door.changeLock(int.Parse(evt.input));

            player.sendNotification(Constants.NotifactionTypes.Info, $"Du hast den Schloss-Idx von Tür {door.Id} auf {evt.input} gesetzt", "");
            Logger.logDebug(LogCategory.Support, LogActionType.Updated, player, $"Player changed lockindex of door {door.Id} to {evt.input}");

            return true;
        }

        #endregion

        public static List<Door> getDoorsByIds(IEnumerable<int> doorIds) {
            return AllDoorsByPosition.Values.Where(d => doorIds.Contains(d.Id)).ToList();
        }

        public static List<Door> getDoorsByGroups(IEnumerable<string> doorGroups) {
            if(doorGroups == null) {
                return new List<Door>();
            }

            return AllDoorsByPosition.Values.Where(d => doorGroups.Contains(d.GroupName)).ToList();
        }

        public static List<string> getAllDoorGroups() {
            var list = new List<string>();
            using(var db = new ChoiceVDb()) {
                list = db.configdoors.Select(d => d.doorGroup).Distinct().ToList();
            }

            return list;
        }

        private void onDoorAdditionalDimensionChange(Door door, DoorDimension dimension, bool add) {
            using(var db = new ChoiceVDb()) {
                if(add) {
                    var newDbDim = new configdoorsadditonaldimension {
                        doorId = door.Id,
                        dimension = dimension.Dimension,
                        lockIndex = dimension.LockIndex,
                    };

                    db.configdoorsadditonaldimensions.Add(newDbDim);
                } else {
                    var dbDim = db.configdoorsadditonaldimensions.Find(door.Id, dimension.Dimension);

                    if(dbDim != null) {
                        db.configdoorsadditonaldimensions.Remove(dbDim);
                    }
                }

                db.SaveChanges();
            }
        }

        private void onInteractWithDoor(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
            try {
                var dicPos = new Position((float)Math.Round(objectPosition.X, 2), (float)Math.Round(objectPosition.Y, 2), (float)Math.Round(objectPosition.Z, 2));
                Door door = null;
                if(AllDoorsByPosition.TryGetValue(dicPos, out var value)) {
                    door = value;
                } else {
                    var minDistance = float.MaxValue;
                    foreach(var d in AllDoorsByPosition.Values.Where(d => d.Position.Distance(dicPos) < 1)) {
                        var dist = d.Position.Distance(dicPos);
                        if(door == null || dist < minDistance) {
                            minDistance = dist;
                            door = d;
                        }
                    }

                    if(door != null) {
                        using(var db = new ChoiceVDb()) {
                            var dbDoor = db.configdoors.Find(door.Id);
                            if(dbDoor != null) {
                                dbDoor.position = dicPos.ToJson();
                                db.SaveChanges();
                            }
                        }

                        var oldPos = door.Position;
                        door.Position = dicPos;
                        AllDoorsByPosition.RemoveWhere(dic => dic.Value == door);
                        AllDoorsByPosition.Add(dicPos, door);
                        foreach(var pl in ChoiceVAPI.GetAllPlayers()) {
                            pl.emitClientEvent("CHANGE_DOOR_POSITION", oldPos.X, oldPos.Y, oldPos.Z, dicPos.X, dicPos.Y, dicPos.Z);
                        }
                    }
                }

                if(door != null) {
                    Logger.logTrace(LogCategory.Player, LogActionType.Event, player, $"Player interacted with door {door.Id}");

                    if(player.getCharacterData().AdminMode) {
                        var adminMenu = new Menu($"Tür {door.Id}", "Was möchstest du tun?");

                        var keyGenMenu = new Menu("Schlüssel erstellen", "Gib die Daten ein");
                        keyGenMenu.addMenuItem(new InputMenuItem("Beschreibung", "Gib die Beschreibung des Schlüssels an", "", ""));
                        keyGenMenu.addMenuItem(new InputMenuItem("Türgruppe", "Gib die Türgruppe für die Erstellung an. Wenn keine angegeben wird, wird der Schlüssel nur für die ausgewählte Tür erstellt", $"{door.GroupName}", ""));
                        keyGenMenu.addMenuItem(new InputMenuItem("Schlossindex", "Gib den Schlossindex für den Schlüssel an. Wenn leer dann wird der der ausgewählten Tür gewählt", $"{door.LockIndex}", ""));
                        keyGenMenu.addMenuItem(new InputMenuItem("Anzahl", "Gib die Anzahl der zu erzeugenden Schlüssel an. Leer bedeutet 1", "", ""));
                        keyGenMenu.addMenuItem(new MenuStatsMenuItem(keyGenMenu.Name, "Erzeuge den/die Schlüssel wie angegeben", "SUPPORT_GENERATE_DOOR_KEY", MenuItemStyle.green)
                            .withData(new Dictionary<string, dynamic> { { "Door", door } })
                            .needsConfirmation("Schlüssel erstellen?", "Schlüssel wirklich erstellen?"));
                        adminMenu.addMenuItem(new MenuMenuItem(keyGenMenu.Name, keyGenMenu));

                        adminMenu.addMenuItem(new InputMenuItem("Türgruppe ändern", "Ändere die Gruppe der Tür", "", "SUPPORT_CHANGE_DOOR_GROUP")
                            .withStartValue(door.GroupName)
                            .withData(new Dictionary<string, dynamic> { { "Door", door } })
                            .needsConfirmation("Türgruppe ändern?", "Türgruppe wirklich ändern?"));

                        adminMenu.addMenuItem(new ClickMenuItem("Türkombo ändern", "Änder die kombinierte Türgruppe.", "", "SUPPORT_CHANGE_DOOR_COMBO")
                            .withData(new Dictionary<string, dynamic> { { "Door", door } }));

                        adminMenu.addMenuItem(new InputMenuItem("Schlossindex anpassen", "Passe den Schlossindex an. Aktuelle Schlüssel funktionieren dadurch nicht mehr. Normal sollte die Zahl eins hochgezählt werden!", $"Akt.: {door.LockIndex}", InputMenuItemTypes.number, "SUPPORT_CHANGE_DOOR_LOCK_INDEX")
                            .withData(new Dictionary<string, dynamic> { { "Door", door } })
                            .needsConfirmation("Schlossindex anpassen?", "Schlossindex wirklich anpassen?"));
                        player.showMenu(adminMenu);
                        return;
                    }

                    //Check if player has the right key
                    var allowedToOpen = door.canPlayerOpen(player);

                    var hardOpenMode = player.getCharacterData().AdminMode || CompanyController.hasPlayerPermission(player, "OPEN_DOORS_FORCEFULLY");

                    if(!allowedToOpen && !hardOpenMode) {
                        player.sendBlockNotification("Du hast keinen Schlüssel für diese Tür! Vielleicht wurde das Schloss gewechselt?", "Keinen Schlüssel!");
                        return;
                    }

                    var idx = door.getLockIndexForDimension(player.Dimension);
                    var doorMenu = new Menu("Tür Interaktion", $"Schließe ab oder auf. Schloss-Idx: {idx}");

                    if(allowedToOpen) {
                        if(door.isLocked(player.Dimension)) {
                            doorMenu.addMenuItem(
                                new ClickMenuItem("Aufschließen", "Schließ die Tür auf", "", "INTERACT_WITH_DOOR_MENU")
                                    .withData(new Dictionary<string, dynamic>() { { "type", "open" }, { "door", door }, { "dimension", player.Dimension } }));
                        } else {
                            doorMenu.addMenuItem(
                            new ClickMenuItem("Zuschließen", "Schließ die Tür zu", "", "INTERACT_WITH_DOOR_MENU").withData(
                                new Dictionary<string, dynamic>() { { "type", "close" }, { "door", door }, { "dimension", player.Dimension } }));
                        }
                    }

                    menu.addMenuItem(new MenuMenuItem(doorMenu.Name, doorMenu));
                } else {
                    Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onDoorInteraction: a player tried to interact with door, that wasnt registered: pos: {dicPos.ToString()}");
                }
            } catch(Exception e) {
                Logger.logException(e);
            }
        }

        private bool onDoorMenuInteraction(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            try {
                var type = data["type"];
                var door = (Door)data["door"];
                var dimension = (int)data["dimension"];

                if(type == "open") {
                    if(dimension == Constants.GlobalDimension) {
                        door.unlockDoor();
                    } else {
                        door.unlockForDimension(dimension);
                    }
                } else {
                    if(dimension == Constants.GlobalDimension) {
                        door.lockDoor();
                    } else {
                        door.lockForDimension(dimension);
                    }
                }
                return true;
            } catch(Exception e) {
                Logger.logException(e);
                return false;
            }
        }

        private static void loadDoors() {
            var doorIdsString = "export var doorIds = [";
            var doorHashString = "export var doorHash = [";
            var doorPosXString = "export var doorPosX = [";
            var doorPosYString = "export var doorPosY = [";
            var doorPosZString = "export var doorPosZ = [";

            var count = 0;
            var doorHashes = new List<string>();
            using(var db = new ChoiceVDb()) {
                foreach(var dbDoor in db.configdoors.Include(d => d.configdoorsadditonaldimensions).Include(c => c.comboGroup).ThenInclude(g => g.configdoors)) {
                    count++;

                    var pos = dbDoor.position.FromJson();
                    var x = pos.X.ToString().Replace(',', '.');
                    var y = pos.Y.ToString().Replace(',', '.');
                    var z = pos.Z.ToString().Replace(',', '.');

                    doorIdsString = doorIdsString + dbDoor.id + ", ";
                    doorHashString = doorHashString + "\"" + dbDoor.modelHash + "\"" + ", ";
                    doorPosXString = doorPosXString + x + ", ";
                    doorPosYString = doorPosYString + y + ", ";
                    doorPosZString = doorPosZString + z + ", ";

                    var p = dbDoor.position.FromJson();
                    var dicPos = new Position((float)Math.Round(p.X, 2), (float)Math.Round(p.Y, 2), (float)Math.Round(p.Z, 2));

                    doorHashes.Add(dbDoor.modelHash);

                    var door = new Door(dbDoor.id, dbDoor.position.FromJson(), dbDoor.locked == 1, dbDoor.modelHash, dbDoor.doorGroup == null ? "" : dbDoor.doorGroup, dbDoor.lockIndex);
                    foreach(var addDim in dbDoor.configdoorsadditonaldimensions) {
                        door.addAdditionalDimension(new DoorDimension(addDim.dimension, addDim.lockIndex, addDim.locked == 1), false);
                    }

                    if(dbDoor.comboGroup != null) {
                        foreach(var dbComboDoor in dbDoor.comboGroup.configdoors) {
                            var comboDoor = AllDoorsByPosition.Values.FirstOrDefault(d => d.Id == dbComboDoor.id);

                            if(comboDoor != null) {
                                door.addComboDoor(comboDoor);
                                comboDoor.addComboDoor(door);
                            } else {
                                //TODO LOG ERROR
                            }
                        }
                    }

                    AllDoorsByPosition.Add(dicPos, door);

                    Logger.logTrace(LogCategory.ServerStartup, LogActionType.Created, $"door was loaded: {dbDoor.id}");
                }
            }

            InteractionController.addInteractableObjects(doorHashes.Distinct().ToList(), "INTERACT_DOOR");

            if(count != 0) {
                doorIdsString = doorIdsString.Remove(doorIdsString.Length - 1);
                doorHashString = doorHashString.Remove(doorHashString.Length - 1);
                doorPosXString = doorPosXString.Remove(doorPosXString.Length - 1);
                doorPosYString = doorPosYString.Remove(doorPosYString.Length - 1);
                doorPosZString = doorPosZString.Remove(doorPosZString.Length - 1);
            }

            doorIdsString += "];";
            doorHashString += "];";
            doorPosXString += "];";
            doorPosYString += "];";
            doorPosZString += "];";

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                System.IO.File.WriteAllLines("resources\\ChoiceVClient\\js\\doorlist.js", new string[] { });
                System.IO.File.WriteAllLines("resources\\ChoiceVClient\\js\\doorlist.js", new string[] { doorIdsString, doorHashString, doorPosXString, doorPosYString, doorPosZString });
            } else {
                System.IO.File.WriteAllLines("resources/ChoiceVClient/js/doorlist.js", new string[] { });
                System.IO.File.WriteAllLines("resources/ChoiceVClient/js/doorlist.js", new string[] { doorIdsString, doorHashString, doorPosXString, doorPosYString, doorPosZString });
            }

            Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, "DoorController: {" + count + "} Doors have been loaded!");
        }

        private static void onPlayerConnect(IPlayer player, character character) {
            foreach(var door in AllDoorsByPosition.Values.Where(d => d.isLocked(player.Dimension))) {
                door.upateForPlayer(player);

                Logger.logTrace(LogCategory.Player, LogActionType.Created, $"onPlayerConnect: door was loaded: {door.Id}");
            }

            foreach(var door in FreshlyRegisteredDoors) {
                player.emitClientEvent("ADD_DOOR_TO_SYSTEM", door.Id, door.Position.X, door.Position.Y, door.Position.Z, door.ModelHash, door.LockedInBaseDimension);
            }
        }

        public delegate void DoorCreatorDelegate(Door door);
        public static void startDoorRegistrationWithCallback(IPlayer player, string groupInput, DoorCreatorDelegate callback) {
            InteractionController.activateAllObjectInteractionMode(player, (hash, position) => {
                if(groupInput == "NO_GROUP") {
                    groupInput = null;
                }

                var didAlreadyExist = false;
                var door = registerDoor(position, hash, groupInput, ref didAlreadyExist);
                if(!didAlreadyExist) {
                    if(player.hasData("REGISTER_DOOR_MODE")) {
                        var callback = (DoorCreatorDelegate)player.getData("REGISTER_DOOR_MODE");
                        player.resetData("REGISTER_DOOR_MODE");
                        callback.Invoke(door);
                    }
                    ChoiceVAPI.SendChatMessageToPlayer(player, $"Tür wurde registriert: door: {door.ToJson()}");
                } else {
                    ChoiceVAPI.SendChatMessageToPlayer(player, $"Diese Tür existiert schon!");
                }

                InteractionController.addInteractableObjects(new List<string> { hash }, "INTERACT_DOOR");

                callback.Invoke(door);
            });

            player.sendNotification(Constants.NotifactionTypes.Info, "Tür-Registrier-Modus aktiviert. Wähle eine Tür aus.", "");
        }

        private static Door registerDoor(Position position, string modelHash, string groupName, ref bool didAlreadyExist, int lockIndex = 1) {
            using(var db = new ChoiceVDb()) {
                position = position.Round();
                var already = AllDoorsByPosition.Values.FirstOrDefault(d => d.Position.Distance(position) < 0.1f);
                if(already != null) {
                    didAlreadyExist = true;
                    return already;
                }

                var dbDoor = new configdoor {
                    locked = 0,
                    modelHash = modelHash,
                    position = position.ToJson(),
                    doorGroup = groupName,
                    lockIndex = lockIndex,
                };

                db.configdoors.Add(dbDoor);

                db.SaveChanges();


                var door = new Door(dbDoor.id, position, false, modelHash, groupName, lockIndex);
                AllDoorsByPosition.Add(position, door);

                FreshlyRegisteredDoors.Add(door);
                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    player.emitClientEvent("ADD_DOOR_TO_SYSTEM", door.Id, position.X, position.Y, position.Z, modelHash, 0);
                }

                if(AllDoorsByPosition.Values.FirstOrDefault(door => door.ModelHash == modelHash) == null) {
                    InteractionController.addInteractableObjects(new List<string> { modelHash });
                }

                return door;
            }
        }
    }
}
