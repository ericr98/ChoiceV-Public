using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class SmallFeatures {
        public SmallFeatures() {
            //Crouch
            EventController.addEvent("PLAYER_CROUCH_TOGGLE", onPlayerCrouchToggle);


            //Action Mode
            EventController.addKeyEvent("GTA_ACTION_MODE", ConsoleKey.O, "GTA-Action Mode deaktivieren", (p, k, e) => {
                p.emitClientEvent("DEACTIVATE_ACTION_MODE");
                return true;
            }, false, false);

            //Sync Player Look At
            EventController.addEvent("SYNC_PLAYER_LOOK_AT", onSyncPlayerLookAt);


            //Player Notes
            CharacterController.addSelfMenuElement(
                new UnconditionalPlayerSelfMenuElement(
                    "Notizen",
                   characterNotesGenerator
                )
            );

            EventController.addMenuEvent("CREATE_CHARACTER_NOTE", onCreateCharacterNote);
            EventController.addMenuEvent("REMOVE_CHARACTER_NOTE", onRemoveCharacterNote);

            //Finger Pointing
            EventController.addKeyToggleEvent("POINT", ConsoleKey.B, "Mit Finger zeigen", onFingerPoint);

            //Block attack
            CharacterSettingsController.addFlagCharacterSettingBlueprint(
                "START_WITHOUT_WEAPONS", true, "Waffen stand. deaktiviert", "Deine Waffen sind bei Login deaktiviert und müssen erst aktiviert werden"
            );
            EventController.addKeyEvent("BLOCK_ATTACK", ConsoleKey.OemMinus, "Angriff blockieren", onBlockAttack);

            //Ladder 
            EventController.addKeyEvent("LADDER", ConsoleKey.L, "Leiter runterklettern", (p, _, _) => {
                if(p.IsInVehicle) {
                    return false;
                }

                p.emitClientEvent("TASK_CLIMB_LADDER");
                return false;
            });

            //Ragdoll
            EventController.addKeyEvent("RAGDOLL", ConsoleKey.Oem7, "Ragdoll aktivieren", onActivcateRagdoll, true);


            //Crouching (STRG)
            EventController.addKeyEvent("INITIATE_CROUCH", (ConsoleKey)17, "Ducken", (p, _, _) => {
                p.emitClientEvent("INITIATE_CROUCH");
                return true;
            });
        }

        #region Player Notes

        private record CharNote(string name, string shortNote, string longNote);
        private Menu characterNotesGenerator(IPlayer player) {
            var menu = new Menu("Kurznotizen", "Schreibe dir selber persönliche Notizen", false);

            var formMenu = new Menu("Notiz erstellen", "Notizen werden alphabetisch sortiert!");
            formMenu.addMenuItem(new InputMenuItem("Name", "Der Name der Notiz", "", ""));
            formMenu.addMenuItem(new InputMenuItem("Kurzinfo", "Die Kurzinfo der Notiz", "", ""));
            formMenu.addMenuItem(new InputMenuItem("Langinfo", "Die länger Info für diese Notiz", "", ""));
            formMenu.addMenuItem(new MenuStatsMenuItem("Notiz erstellen", "Erstelle die Notiz", "CREATE_CHARACTER_NOTE", MenuItemStyle.green).needsConfirmation("Notiz erstellen?", "Wirklich erstellen?"));
            menu.addMenuItem(new MenuMenuItem(formMenu.Name, formMenu, MenuItemStyle.green));


            var notes = ((string)player.getData("CHAR_NOTES")).FromJson<List<CharNote>>();

            if(notes == null) {
                notes = new List<CharNote>();
            }

            foreach(var note in notes) {
                menu.addMenuItem(new ClickMenuItem(note.name, note.longNote, note.shortNote, "REMOVE_CHARACTER_NOTE").needsConfirmation("Notiz löschen?", "Notiz wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Note", note } }));
            }

            return menu;
        }


        private bool onCreateCharacterNote(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var name = evt.elements[0].FromJson<InputMenuItemEvent>().input;
            var shortInfo = evt.elements[1].FromJson<InputMenuItemEvent>().input;
            var longInfo = evt.elements[2].FromJson<InputMenuItemEvent>().input;

            var notes = ((string)player.getData("CHAR_NOTES")).FromJson<List<CharNote>>();

            if(notes == null) {
                notes = new List<CharNote>();
            }

            notes.Add(new CharNote(name, shortInfo, longInfo));

            player.setPermanentData("CHAR_NOTES", notes.ToJson());

            player.sendNotification(NotifactionTypes.Success, "Notiz erfolgreich erstellt!", "Notiz erstellt!");

            Logger.logDebug(LogCategory.Player, LogActionType.Created, player, $"Wrote Char Note with name: \"{name}\" and shortInfo: \"{shortInfo}\" and text \"{longInfo}\"");

            return true;
        }

        private bool onRemoveCharacterNote(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var note = (CharNote)data["Note"];

            var notes = ((string)player.getData("CHAR_NOTES")).FromJson<List<CharNote>>();

            if(notes == null) {
                notes = new List<CharNote>();
            }

            notes.RemoveAll(x => x.name == note.name && x.shortNote == x.shortNote);

            player.setPermanentData("CHAR_NOTES", notes.ToJson());

            player.sendNotification(NotifactionTypes.Warning, "Notiz erfolgreich gelöscht!", "Notiz gelöscht!");

            Logger.logDebug(LogCategory.Player, LogActionType.Removed, player, $"Removed CharNote with name {note.name}");

            return true;
        }

        #endregion

        #region Crouch

        private bool onPlayerCrouchToggle(IPlayer player, string eventName, object[] args) {
            if(player.Exists() && player.getCharacterFullyLoaded()) {
                var toggle = (bool)args[0];

                var data = player.getCharacterData();
                data.IsCrouching = toggle;
            }

            return true;
        }

        #endregion

        #region Look At

        private bool onSyncPlayerLookAt(IPlayer player, string eventName, object[] args) {
            player.SetStreamSyncedMetaData("SYNC_PLAYER_LOOK_AT", args[0].ToString());

            return true;
        }

        #endregion

        #region Finger Pointing

        private bool onFingerPoint(IPlayer player, ConsoleKey key, bool isPressed, string eventName) {
            player.emitClientEvent("TOOGLE_FINGER_POINT");
            return true;
        }

        #endregion

        #region Block attack

        private bool onBlockAttack(IPlayer player, ConsoleKey key, string eventName) {
            if(player.hasData("TOOGLE_CAN_ATTACK")) {
                var already = (bool)player.getData("TOOGLE_CAN_ATTACK");
                var newA = !already;
                if(newA) {
                    player.sendNotification(NotifactionTypes.Info, "Du hast Angriffe deaktiviert. Nahkampf und Schusswaffen können nicht benutzt werden!", "Angriff deaktiviert", NotifactionImages.System);
                } else {
                    player.sendNotification(NotifactionTypes.Info, "Du hast Angriffe wieder aktiviert. Nahkampf und Schusswaffen können wieder benutzt werden!", "aktivert deaktiviert", NotifactionImages.System);
                }

                player.setData("TOOGLE_CAN_ATTACK", newA);
            } else {
                player.setData("TOOGLE_CAN_ATTACK", true);
                player.sendNotification(NotifactionTypes.Info, "Du hast Angriffe deaktiviert. Nahkampf und Schusswaffen können nicht benutzt werden!", "Angriff deaktiviert", NotifactionImages.System);
            }

            player.emitClientEvent("TOOGLE_CAN_ATTACK");
            return true;
        }


        #endregion

        #region Ragdoll

        private bool onActivcateRagdoll(IPlayer player, ConsoleKey key, string eventName) {
            if(!player.hasData("CURRENTLY_RAGDOLLING")) {
                if(!player.getBusy()) {
                    player.setPedRagdoll();
                    player.setData("CURRENTLY_RAGDOLLING", true);
                }
            } else {
                player.stopPedRagdoll();
                player.resetData("CURRENTLY_RAGDOLLING");
            }

            return true;
        }

        #endregion
    }
}
