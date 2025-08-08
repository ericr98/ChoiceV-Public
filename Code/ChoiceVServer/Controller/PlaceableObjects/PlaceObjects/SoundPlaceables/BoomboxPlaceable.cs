using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Bogus.DataSets;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static System.Net.WebRequestMethods;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class BoomBoxController : ChoiceVScript {
        public BoomBoxController() {
            EventController.addMenuEvent("SELECT_BOOMBOX_SOURCE", onSetRadioStation);
            EventController.addMenuEvent("TOGGLE_BOOMBOX_ACTIVE", onToggleActive);
            EventController.addMenuEvent("SELECT_BOOMBOX_VOLUME", onSelectBoomboxVolume);
        }

        private bool onSetRadioStation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var placeable = (BoomboxPlaceable)data["PLACEABLE"];
            var radioStation = (string)data["RADIO_STATION"];

            if(placeable != null) {
                placeable.setRadioStation(player, radioStation);
            }

            return true;
        }

        private bool onToggleActive(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var placeable = (BoomboxPlaceable)data["PLACEABLE"];

            if(placeable != null) {
                placeable.toggleActive(player);
            }

            return true;
        }

        private bool onSelectBoomboxVolume(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var placeable = (BoomboxPlaceable)data["PLACEABLE"];
            var evt = menuItemCefEvent as ListMenuItemEvent;

            if(placeable != null) {
                placeable.changeVolume(evt.currentElement);
            }

            return true;
        }
    }

    public class BoomboxPlaceable : SoundPlaceable {
        protected string RadioStation { get => Data.hasKey("RadioStation") ? (string)Data["RadioStation"] : ""; set { Data["RadioStation"] = value; } }

        public BoomboxPlaceable(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) { }

        public BoomboxPlaceable(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(model, placeableItem, player, playerPosition, playerRotation) {
            Volume = 0f;
            VolumeModifier = 1;
            Distance = 15f;
            Loop = true;

            ModelName = model;
        }

        public override void initialize(bool register = true) {
            Object = ObjectController.createObject(ModelName, Position, Rotation, 100, false);

            base.initialize(register);
        }

        public override void onRemove() {
            base.onRemove();

            if(Active) {
                deactivate();
            }
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var menu = new Menu("Boom Box", "Was möchtest du tun?");

            if(Data.hasKey("RadioStation")) {
                if(!Active) {
                    menu.addMenuItem(new ClickMenuItem("Anschalten", "Schalte die BoomBox ein", "", "TOGGLE_BOOMBOX_ACTIVE", MenuItemStyle.green)
                        .withData(new Dictionary<string, dynamic> { { "PLACEABLE", this } }));
                } else {
                    menu.addMenuItem(new ClickMenuItem("Ausschalten", "Schalte die BoomBox aus", "", "TOGGLE_BOOMBOX_ACTIVE", MenuItemStyle.red)
                        .withData(new Dictionary<string, dynamic> { { "PLACEABLE", this } }));
                }
            } else {
                menu.addMenuItem(new StaticMenuItem("Erst Musikwiedergabe wählen", "Es muss zuerst eine Musikwiedergabe gewählt werden.", ""));
            }


            if(Active) {
                menu.addMenuItem(new ListMenuItem("Lautstärke einstellen", "Stelle die Lautstärke der Boombox ein", RadioController.getRadioVolumes(VolumeModifier), "SELECT_BOOMBOX_VOLUME", MenuItemStyle.normal, true, true)
                    .withNoLoopOver("0%")
                    .withData(new Dictionary<string, dynamic> { { "PLACEABLE", this } }));
            }

            var senderMenu = new Menu("Radio-Sender", "Wähle einen Sender");

            foreach(var station in RadioController.getRadioStations()) {
                senderMenu.addMenuItem(new ClickMenuItem(station.Name, $"Wähle {station.Name} als Sender", RadioStation == station.Name ? "Aktiv" : "" , "SELECT_BOOMBOX_SOURCE", MenuItemStyle.normal)
                                       .withData(new Dictionary<string, dynamic> { { "PLACEABLE", this }, { "RADIO_STATION", station.Name } }));
            }

            menu.addMenuItem(new MenuMenuItem(senderMenu.Name, senderMenu));

            menu.addMenuItem(new ClickMenuItem("Aufheben", "Lege die Boombox wieder in dein Inventar", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> {
                {"placeable", this }
            }));

            return menu;
        }

        public void toggleActive(IPlayer player) {
            var anim = player.getInteractAnimation(Position);
            AnimationController.animationTask(player, anim, () => {
                if(!Active) {
                    activate();
                } else {
                    deactivate();
                }
            }, null, true, null, TimeSpan.FromSeconds(1.5f));
        }

        public void changeVolume(string currentElement) {
            VolumeModifier = RadioController.getValueFromRadioVolume(currentElement);
            Distance = Math.Max(15f * VolumeModifier, 7.5f);

            if(Active) {
                SoundController.changeSoundEventVolume(SoundEventId, Volume * VolumeModifier);
                SoundController.changePositionSoundEventMaxDistance(SoundEventId, Distance);
            }
        }

        public void setRadioStation(IPlayer player, string radioStation) {
            RadioStation = radioStation;
            var element = RadioController.getRadioStationByName(radioStation);
            SoundSource = element.Source;
            SoundMount = element.Mount;
            SoundSourceIdentifier = element.Name;

            Volume = element.StandardVolume;
            if(Active) {
                SoundController.pauseSoundEvent(SoundEventId);
                SoundController.playSoundAtCoords(Position, Distance, SoundController.Sounds.RadioChange, 0.4f, "mp3");
            }

            var anim = player.getInteractAnimation(Position);
            AnimationController.animationTask(player, anim, () => {
                if(Active) {
                    SoundController.changeSoundEventSource(SoundEventId, SoundSourceIdentifier, SoundSource, SoundMount, true);
                }

                player.sendNotification(Constants.NotifactionTypes.Info, $"Radio Sender auf {RadioStation} geändert.", "Radio geändert", Constants.NotifactionImages.Music, "BOOMBOX");
                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"Player updated Radiostation to {RadioStation} on Boombox {Id}");
            }, null, true, null, TimeSpan.FromSeconds(1.5f));
        }
    }
}
