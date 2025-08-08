using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.SoundSystem {
    public class SoundEventZoneController : ChoiceVScript {
        public static List<SoundEventZone> SoundZones = new List<SoundEventZone>();

        public SoundEventZoneController() {
            EventController.addMenuEvent("TOGGLE_SOUND_ZONE_ACTIVE", onToggleActive);
            EventController.addMenuEvent("SET_SOUND_ZONE_SOURCE", onSetSource);
            EventController.addMenuEvent("SET_SOUND_ZONE_VOLUME", onSetVolume);
            EventController.addMenuEvent("SET_SOUND_ZONE_DISTANCE", onSetDistance);
        }

        public bool onToggleActive(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var zone = (SoundEventZone)data["ZONE"];

            zone.Active = !zone.Active;

            if(zone.Active) {
                zone.SoundEventId = SoundController.createPositionSoundEvent(zone.Position, zone.SoundSourceIdentifier, zone.SoundSource, zone.SoundMount, SoundEventZone.StandardVolume * zone.VolumeModifier, zone.Distance, true);
                player.sendNotification(Constants.NotifactionTypes.Success, "Zone aktiviert: " + zone.Name, "Soundzone aktiviert", Constants.NotifactionImages.Music, "SOUND_ZONE");
            } else {
                SoundController.removeSoundEvent(zone.SoundEventId);
                player.sendNotification(Constants.NotifactionTypes.Warning, "Zone deaktivert: " + zone.Name, "Soundzone deaktivert", Constants.NotifactionImages.Music, "SOUND_ZONE");
            }

            return true;
        }

        public bool onSetVolume(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var zone = (SoundEventZone)data["ZONE"];
            var evt = menuItemCefEvent as ListMenuItemEvent;

            zone.VolumeModifier = float.Parse(evt.currentElement.Replace("%", "")) / 100;

            if(zone.Active) {
                SoundController.changeSoundEventVolume(zone.SoundEventId, SoundEventZone.StandardVolume * zone.VolumeModifier);
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Zonenlautstärke auf {evt.currentElement} gesetzt", "Zonenlautstärke gesetzt", Constants.NotifactionImages.Music, "SOUND_ZONE");

            return true;
        }

        private bool onSetSource(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var zone = (SoundEventZone)data["ZONE"];
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var sourceEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var mountEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            zone.SoundSource = sourceEvt.input;
            zone.SoundMount = mountEvt.input;


            if(zone.Active) {
                SoundController.changeSoundEventSource(zone.SoundEventId, zone.SoundSourceIdentifier, zone.SoundSource, zone.SoundMount, true);
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Zonenquelle auf {sourceEvt.input}/{mountEvt.input} gesetzt", "Zonenquelle gesetzt", Constants.NotifactionImages.Music, "SOUND_ZONE");


            return true;
        }

        public bool onSetDistance(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var zone = (SoundEventZone)data["ZONE"];
            var evt = menuItemCefEvent as InputMenuItemEvent;

            zone.Distance = Math.Min(75, float.Parse(evt.input));

            if(zone.Active) {
                SoundController.changePositionSoundEventMaxDistance(zone.SoundEventId, zone.Distance);
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Zonenreichweite auf {zone.Distance} gesetzt", "Zonenreichweite gesetzt", Constants.NotifactionImages.Music, "SOUND_ZONE");

            return true;
        }
    }

    public class SoundEventZone {

        public static float StandardVolume = 0.1f;
        public string Name { get; internal set; }
        public CollisionShape InteractionShape { get; internal set; }

        public Position Position { get; internal set; }

        public bool Active { get; internal set; }
        public string SoundSourceIdentifier { get; internal set; }
        public string SoundSource { get; internal set; }
        public string SoundMount { get; internal set; }
        public float VolumeModifier { get; internal set; }
        public float Distance { get; internal set; }
        public int SoundEventId { get; internal set; }

        public SoundEventZone(string name, CollisionShape shape, Position position) {
            Name = name;
            InteractionShape = shape;
            Position = position;


            SoundSource = "";
            SoundMount = "radio.mp3";
            VolumeModifier = 1f;
            Distance = 15f;

            InteractionShape.OnCollisionShapeInteraction += onInteraction;
        }

        private bool onInteraction(IPlayer player) {
            var menu = new Menu(Name, "Was möchtest du tun?");

            if(!Active) {
                menu.addMenuItem(new ClickMenuItem("Anschalten", "Schalte die BoomBox ein", "", "TOGGLE_SOUND_ZONE_ACTIVE", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "ZONE", this } }));
            } else {
                menu.addMenuItem(new ClickMenuItem("Ausschalten", "Schalte die BoomBox aus", "", "TOGGLE_SOUND_ZONE_ACTIVE", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "ZONE", this } }));
            }

            var volumes = new List<string>();
            var shift = 0;
            for(int i = 0; i <= 100; i++) {
                if(i * 5 == Math.Round(VolumeModifier * 100)) {
                    shift = i;
                }
                volumes.Add(i * 5 + "%");
            }

            var elements = volumes.ShiftLeft(shift).ToArray();

            menu.addMenuItem(new ListMenuItem("Lautstärke einstellen", "Stelle die Lautstärke der Zone ein", elements, "SET_SOUND_ZONE_VOLUME", MenuItemStyle.normal, true, true)
                .withNoLoopOver("0%")
                .withData(new Dictionary<string, dynamic> { { "ZONE", this } }));


            menu.addMenuItem(new InputMenuItem("Reichweite einstellen", "Stelle die maximale Reichweite der Zone ein. < 75m!", "", "SET_SOUND_ZONE_DISTANCE")
               .withStartValue(Distance.ToString())
               .withData(new Dictionary<string, dynamic> { { "ZONE", this } }));


            if(player.getCharacterData().AdminMode) {
                var sourceMenu = new Menu("Quelle einstellen", "Was möchtest du tun?");

                sourceMenu.addMenuItem(new InputMenuItem("Quelle (Pfad) einstellen", "Stelle die Quelle des Soundevents ein.", "", "")
                    .withStartValue(SoundSource));

                sourceMenu.addMenuItem(new InputMenuItem("Quelle (Mount) einstellen", "Stelle die Quelle des Soundevents ein.", "", "")
                    .withStartValue(SoundMount));

                sourceMenu.addMenuItem(new MenuStatsMenuItem("Speichern", "Quelleneinstellungen speichern", "SET_SOUND_ZONE_SOURCE", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "ZONE", this } }));

                menu.addMenuItem(new MenuMenuItem(sourceMenu.Name, sourceMenu));
            } else {
                menu.addMenuItem(new StaticMenuItem("Quelle einstellen (nicht verfügbar)", "Nur für Admins verfügbar. Frage den Support um die Quelle anzupassen", "", MenuItemStyle.yellow));
            }

            player.showMenu(menu);

            return true;
        }
    }
}
