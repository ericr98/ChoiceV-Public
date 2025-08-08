using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.SoundSystem.SoundController;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;
using ListMenuItem = ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    public class DoorCompanyFunctionality : CompanyFunctionality {
        private static readonly List<(Sounds, string)> AVAILABLE_DOOR_SOUNDS = new List<(Sounds, string)> { 
            (Sounds.None, ""), 
            (Sounds.DoorBuzz, "mp3"), 
            (Sounds.DoorLock, "wav") };

        private Dictionary<int, Door> NormalDoors;
        private Dictionary<int, Door> SpecialDoors;

        private record RemoteDoorGroup(string Group, string DisplayName, Sounds OpenSound, Sounds CloseSound);
        private List<RemoteDoorGroup> RemoteDoors;

        public DoorCompanyFunctionality() { }

        public DoorCompanyFunctionality(Company company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "DOOR_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Türgruppen zuweisen", "Weise Türgruppen zu, welche dann mit einer Permission geöffnet werden können");
        }

        public override List<string> getSinglePermissionsGranted() {
            return ["DOOR_ACCESS", "SPECIAL_DOOR_ACCESS"];
        }

        public override void onLoad() {
            NormalDoors = new Dictionary<int, Door>();
            var normalDoorSettings = Company.getSettings("NORMAL_DOOR_GROUPS");
            var normalDoors = DoorController.getDoorsByGroups(normalDoorSettings.Select(s => s.settingsValue));
            foreach(var normalDoor in normalDoors) {
                NormalDoors.Add(normalDoor.Id, normalDoor);
                normalDoor.setConnectedCompanyFunctionality(this);
            }

            SpecialDoors = new Dictionary<int, Door>();
            var specialDoorSettings = Company.getSettings("SPECIAL_DOOR_GROUPS");
            var specialDoors = DoorController.getDoorsByGroups(specialDoorSettings.Select(s => s.settingsValue));
            foreach(var specialDoor in specialDoors) {
                SpecialDoors.Add(specialDoor.Id, specialDoor);
                specialDoor.setConnectedCompanyFunctionality(this);
            }

            RemoteDoors = new();
            var remoteDoorSettings = Company.getSettings("REMOTE_DOORS");
            foreach(var remoteDoor in remoteDoorSettings) {
                var split = remoteDoor.settingsValue.Split("#");
                var group = split[0];
                var name = split[1];
                var openSound = Enum.Parse<Sounds>(split[2]);
                var closeSound = Enum.Parse<Sounds>(split[3]);

                RemoteDoors.Add(new RemoteDoorGroup(group, name, openSound, closeSound));
            }

            Company.registerCompanySelfElement(
                "REMOTE_DOOR_CONTROL",
                getRemoteDoorControls,
                onRemoteDoorControls
            );


            Company.registerCompanyAdminElement(
                "REGISTER_DOORS",
                registerDoorsGenerator,
                registerDoorsCallback
            );

            Company.registerCompanyAdminElement(
                "REGISTER_REMOTE_DOORS",
                registerDoorsRemoteGenerator,
                registerDoorsRemoteCallback
            );
        }

        private MenuElement getRemoteDoorControls(Company company, IPlayer player) {
            if(RemoteDoors.Count == 0) {
                return null;
            }

            var allDoors = NormalDoors.Concat(SpecialDoors);
            var menu = new Menu("Remote-Türsteuerung", "Was möchtest du tun?");
            foreach(var remoteDoor in RemoteDoors) {
                var anyOpen = allDoors.Any(d => d.Value.GroupName == remoteDoor.Group && !d.Value.LockedInBaseDimension);

                var stateList = new List<string> { "Offen (mind. 1)", "Geschlossen" };

                if(!anyOpen) {
                    stateList.Reverse();
                }

                if(SpecialDoors.Any(d => d.Value.GroupName == remoteDoor.Group) && !CompanyController.hasPlayerPermission(player, Company, "SPECIAL_DOOR_ACCESS")) {
                    menu.addMenuItem(new StaticMenuItem(remoteDoor.DisplayName, $"Du hast keine Berechtigung für die Türgruppe {remoteDoor.DisplayName}", "(Keine Berechtigung)", MenuItemStyle.yellow));
                } else {
                    menu.addMenuItem(new ListMenuItem(remoteDoor.DisplayName, $"Öffne/Schließe die Türgruppe {remoteDoor.DisplayName}", stateList.ToArray(), "REMOTE_DOOR_TOGGLE", MenuItemStyle.normal, true, true)
                        .withData(new Dictionary<string, dynamic> { { "RemoteDoorGroup", remoteDoor } }));
                }
            }

            return menu;
        }

        private void onRemoteDoorControls(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "REMOTE_DOOR_TOGGLE") {
                var evt = menuItemCefEvent as ListMenuItemEvent;
                var remoteDoor = (RemoteDoorGroup)data["RemoteDoorGroup"];

                var doors = NormalDoors.Concat(SpecialDoors).Where(d => d.Value.GroupName == remoteDoor.Group).Select(d => d.Value);
                
                var newState = evt.currentElement == "Offen (mind. 1)";

                var alreadyCombos = new List<Door>();
                foreach(var door in doors) {
                    if(newState != door.LockedInBaseDimension) {
                        continue;
                    }

                    if(door.getComboDoorAmount() > 0) {
                        var allCombos = door.getComboDoors();

                        if(alreadyCombos.Any(d => allCombos.Contains(d))) {
                            continue;
                        } else {
                            alreadyCombos.Add(door);
                        }
                    }

                    if(newState) {
                        door.unlockDoor();
                        if(remoteDoor.OpenSound != Sounds.None) {
                            var format = AVAILABLE_DOOR_SOUNDS.First(s => s.Item1 == remoteDoor.OpenSound).Item2;
                            SoundController.playSoundAtCoords(door.Position, 10, remoteDoor.OpenSound, 0.75f, format);
                        }
                    } else {
                        door.lockDoor();

                        if(remoteDoor.CloseSound != Sounds.None) {
                            var format = AVAILABLE_DOOR_SOUNDS.First(s => s.Item1 == remoteDoor.CloseSound).Item2;
                            SoundController.playSoundAtCoords(door.Position, 10, remoteDoor.CloseSound, 0.75f, format);
                        }
                    }
                    
                    player.sendNotification(Constants.NotifactionTypes.Success, $"Türgruppe {remoteDoor.DisplayName} wurde erfolgreich geändert", "");
                }
            }
        }

        public override void onRemove() {
            var doors = NormalDoors.Concat(SpecialDoors);
            doors.ForEach(d => d.Value.setConnectedCompanyFunctionality(null));

            Company.deleteSetting("DOOR_ADD_COUNTER");
            Company.deleteSettings("NORMAL_DOOR_GROUPS");
            Company.deleteSettings("SPECIAL_DOOR_GROUPS");
        }

        private MenuElement registerDoorsGenerator(IPlayer player) {
            var menu = new Menu("Firmentüren hinzufügen", "Was möchtest du tun?");

            var addMenu = new Menu("Türgruppe hinzufügen", "Gib die Daten ein", false);
            addMenu.addMenuItem(new InputMenuItem("Gruppenname", "Gib den Namen der Türgruppe ein", "", "").withOptions(DoorController.getAllDoorGroups().ToArray()));
            addMenu.addMenuItem(new ListMenuItem("Türart", "Gib die Art der Tür an", new string[] { "Normal", "Special" }, ""));
            addMenu.addMenuItem(new MenuStatsMenuItem("Türgruppe hinzufügen", "Füge die Türgruppe wie angegeben hinzu", "ADD_DOOR_GROUP", MenuItemStyle.green));
            menu.addMenuItem(new MenuMenuItem(addMenu.Name, addMenu));

            var listMenu = new Menu("Türgruppenliste", "Wähle welche du löschen möchstest");

            var normalDoorSettings = Company.getSettings("NORMAL_DOOR_GROUPS");
            foreach(var normalDoorGroup in normalDoorSettings) {
                listMenu.addMenuItem(new ClickMenuItem(normalDoorGroup.settingsValue, $"Lösche die Türgruppe {normalDoorGroup.settingsValue}. Es handelt sich um eine normale Türgruppe", "Normal", "REMOVE_DOOR_GROUP", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "DoorGroup", normalDoorGroup.settingsValue }, { "Type", "NORMAL" } }).needsConfirmation($"{normalDoorGroup.settingsValue} löschen?", "Türgruppe wirklich löschen?"));
            }

            var specialDoorSettings = Company.getSettings("SPECIAL_DOOR_GROUPS");
            foreach(var specialDoorGroup in specialDoorSettings) {
                listMenu.addMenuItem(new ClickMenuItem(specialDoorGroup.settingsValue, $"Lösche die Türgruppe {specialDoorGroup.settingsValue}. Es handelt sich um eine normale Türgruppe", "Special", "REMOVE_DOOR_GROUP", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "DoorGroup", specialDoorGroup.settingsValue }, { "Type", "SPECIAL" } }).needsConfirmation($"{specialDoorGroup.settingsValue} löschen?", "Türgruppe wirklich löschen?"));
            }

            menu.addMenuItem(new MenuMenuItem(listMenu.Name, listMenu));

            return menu;
        }

        private void registerDoorsCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "ADD_DOOR_GROUP") {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
                var groupEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var typeEvt = evt.elements[1].FromJson<ListMenuItemEvent>();

                var group = typeEvt.currentElement == "Normal" ? "NORMAL_DOOR_GROUPS" : "SPECIAL_DOOR_GROUPS";

                Company.setSetting($"DOOR_GROUP_{groupEvt.input}", groupEvt.input, group);

                var newDoors = DoorController.getDoorsByGroups(new List<string> { groupEvt.input });
                foreach(var newDoor in newDoors) {
                    if(typeEvt.currentElement == "Normal") {
                        NormalDoors.Add(newDoor.Id, newDoor);
                    } else {
                        SpecialDoors.Add(newDoor.Id, newDoor);
                    }

                    newDoor.setConnectedCompanyFunctionality(this);
                }

                player.sendNotification(Constants.NotifactionTypes.Info, $"Türgruppe {groupEvt.input} wurde erfolgreich als {group} gespeichert", "");
            } else if(subEvent == "REMOVE_DOOR_GROUP") {
                var group = (string)data["DoorGroup"];
                var type = (string)data["Type"];

                if(type == "NORMAL") {
                    Company.deleteSetting($"DOOR_GROUP_{group}", "NORMAL_DOOR_GROUPS");
                    foreach (var door in NormalDoors.Where(door => door.Value.GroupName == group)) {
                        door.Value.removeConnectedCompanyFunctionality(this);
                        NormalDoors.Remove(door.Key);
                    }
                } else {
                    Company.deleteSetting($"DOOR_GROUP_{group}", "SPECIAL_DOOR_GROUPS");
                    foreach (var door in SpecialDoors.Where(door => door.Value.GroupName == group)) {
                        door.Value.removeConnectedCompanyFunctionality(this);
                        SpecialDoors.Remove(door.Key);
                    }
                }
                player.sendNotification(Constants.NotifactionTypes.Warning, $"Die Türgruppe {group} wurde erfolgreich entfernt", "");
            }
        }

        private MenuElement registerDoorsRemoteGenerator(IPlayer player) {
            var menu = new Menu("Remote-Türgruppen hinzufügen", "Was möchtest du tun?", false);

            var createMenu = new Menu("Remote-Türgruppen hinzufügen", "Gib die Daten ein"); 

            var normalDoorSettings = Company.getSettings("NORMAL_DOOR_GROUPS").Select(s => s.settingsValue).ToArray();
            createMenu.addMenuItem(new InputMenuItem("Gruppenname", "Gib den Namen der Türgruppe ein", "", "").withOptions(normalDoorSettings));

            createMenu.addMenuItem(new InputMenuItem("Anzeigename", "Gib den Anzeigenamen der Türgruppe ein", "", ""));

            var sounds = AVAILABLE_DOOR_SOUNDS.Select((s, _) => s.Item1.ToString()).ToArray();
            createMenu.addMenuItem(new ListMenuItem("Öffnungssound wählen", "Tür öffnen/schließen Sound wählen", sounds, ""));
            createMenu.addMenuItem(new ListMenuItem("Schließsound wählen", "Tür öffnen/schließen Sound wählen", sounds, ""));

            createMenu.addMenuItem(new MenuStatsMenuItem("Remote-Tür hinzufügen", "Füge die Remote-Tür wie angegeben hinzu", "ADD_REMOTE_DOOR", MenuItemStyle.green)
                .needsConfirmation("Remote-Tür hinzufügen?", "Remote-Tür wirklich hinzufügen?"));

            menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            var settings = Company.getSettings("REMOTE_DOORS");

            foreach(var setting in settings) {
                var split = setting.settingsValue.Split("#");
                var group = split[0];
                var name = split[1];
                var sound = Enum.Parse<Sounds>(split[2]);
                
                menu.addMenuItem(new ClickMenuItem($"{group} entfernen", $"Die Gruppe mit Anzeigenamen: {name} und Sound: {sound} entfernen", "", "REMOVE_REMOTE_DOOR", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Group", group } })
                    .needsConfirmation($"{group} entfernen?", "Gruppe wirklich entfernen?"));
            }

            return menu;
        }

        private void registerDoorsRemoteCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "ADD_REMOTE_DOOR") {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

                var groupEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var nameEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
                var openSoundEvt = evt.elements[2].FromJson<ListMenuItemEvent>();
                var closeSoundEvt = evt.elements[3].FromJson<ListMenuItemEvent>();

                var openSound = (Sounds)Enum.Parse(typeof(Sounds), openSoundEvt.currentElement);
                var closeSound = (Sounds)Enum.Parse(typeof(Sounds), closeSoundEvt.currentElement);

                Company.setSetting($"REMOTE_DOOR_GROUP_{groupEvt.input}", $"{groupEvt.input}#{nameEvt.input}#{openSound}#{closeSound}", "REMOTE_DOORS");
                
                RemoteDoors.Add(new RemoteDoorGroup(groupEvt.input, nameEvt.input, openSound, closeSound));
                player.sendNotification(Constants.NotifactionTypes.Success, $"Remote-Türgruppe {groupEvt.input} wurde erfolgreich hinzugefügt", "");
            } else if(subEvent == "REMOVE_REMOTE_DOOR") {
                var group = (string)data["Group"];

                Company.deleteSetting($"REMOTE_DOOR_GROUP_{group}", "REMOTE_DOORS");
                RemoteDoors.RemoveAll(r => r.Group == group);

                player.sendNotification(Constants.NotifactionTypes.Warning, $"Remote-Türgruppe {group} wurde erfolgreich entfernt", "");
            }
        }


        public bool canPlayerOpenDoor(IPlayer player, Door door) {
            if(NormalDoors.ContainsKey(door.Id)) {
                return CompanyController.hasPlayerPermission(player, Company, "DOOR_ACCESS");
            } else if(SpecialDoors.ContainsKey(door.Id)) {
                return CompanyController.hasPlayerPermission(player, Company, "SPECIAL_DOOR_ACCESS");
            } else {
                return false;
            }
        }
    }
}
