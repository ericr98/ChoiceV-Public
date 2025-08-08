using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller.Vehicles.AdditionalFeatures {
    public class VehicleRadioFeatureController : ChoiceVScript {
        public VehicleRadioFeatureController() {
            VehicleController.addSelfMenuElement(
                new ConditionalVehicleGeneratedSelfMenuElement(
                    "Fahrzeugradio",
                    vehicleRadioMenuGenerator,
                    veh => veh.VehicleClass != VehicleClassesDbIds.Cycles, //No bycicles
                    player => player.Seat == 1 || player.Seat == 2
                )
            );

            EventController.addMenuEvent("ACTIVATE_VEHICLE_RADIO", onActivateVehicleRadio);
            EventController.addMenuEvent("DEACTIVATE_VEHICLE_RADIO", onDeactivateVehicleRadio);

            EventController.addMenuEvent("SET_VEHICLE_RADIO_VOLUME", onVehicleRadioVolume);
            EventController.addMenuEvent("SET_VEHICLE_RADIO_STATION", onVehicleRadioStation);

            EventController.PlayerEnterVehicleDelegate += onPlayerEnterVehicle;
            EventController.PlayerExitVehicleDelegate += onPlayerExitVehicle;
        }

        private void onPlayerEnterVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if(vehicle.hasData("VEHICLE_RADIO")) {
                var vehicleRadio = vehicle.getData("VEHICLE_RADIO") as VehicleRadioStation;
                SoundController.addListenerToUndirectionalSoundEvent(vehicleRadio.SoundEventId, player);
            }
        }

        private void onPlayerExitVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if(vehicle.hasData("VEHICLE_RADIO")) {
                var vehicleRadio = vehicle.getData("VEHICLE_RADIO") as VehicleRadioStation;
                SoundController.removeListenerFromUndirectionalSoundEvent(vehicleRadio.SoundEventId, player);
            }
        }

        private class VehicleRadioStation {
            public int SoundEventId;
            public string RadioStation;
            public float Volume;
            public float VolumeModifier;

            public VehicleRadioStation(int soundEventId, string radioStation, float volume, float volumeModifier) {
                SoundEventId = soundEventId;
                RadioStation = radioStation;
                Volume = volume;
                VolumeModifier = volumeModifier;
            }
        }

        private Menu vehicleRadioMenuGenerator(ChoiceVVehicle vehicle, IPlayer player) {
            var menu = new Menu("Fahrzeugradio", "Wählen Sie einen Radiosender aus");

            if(vehicle.hasData("VEHICLE_RADIO")) {
                var vehicleRadio = vehicle.getData("VEHICLE_RADIO") as VehicleRadioStation;
                menu.addMenuItem(new ClickMenuItem("Radio ausschalten", "Schalte das Radio aus", "", "DEACTIVATE_VEHICLE_RADIO", MenuItemStyle.red)
                     .withData(new Dictionary<string, dynamic> { { "VEHICLE", vehicle } }));

                menu.addMenuItem(new ListMenuItem("Lautstärke einstellen", "Stelle die Lautstärke der Boombox ein", RadioController.getRadioVolumes(vehicleRadio.VolumeModifier), "SET_VEHICLE_RADIO_VOLUME", MenuItemStyle.normal, true, true)
                    .withNoLoopOver("0%")
                    .withData(new Dictionary<string, dynamic> { { "VEHICLE", vehicle } }));


                foreach(var radio in RadioController.getRadioStations()) {
                    menu.addMenuItem(new ClickMenuItem(radio.Name, $"Wähle {radio.Name}", vehicleRadio.RadioStation == radio.Name ? "Aktuell" : "", "SET_VEHICLE_RADIO_STATION")
                        .withData(new Dictionary<string, dynamic> { { "VEHICLE", vehicle }, {"RADIO", radio }}));
                }
            } else {
                menu.addMenuItem(new ClickMenuItem("Radio anschalten", "Schalte das Radio ein", "", "ACTIVATE_VEHICLE_RADIO", MenuItemStyle.green)
                 .withData(new Dictionary<string, dynamic> { { "VEHICLE", vehicle } }));
            }



            return menu;
        }

        private bool onActivateVehicleRadio(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["VEHICLE"];

            if(!vehicle.hasData("VEHICLE_RADIO")) {
                var radioStation = RadioController.getRadioStations().First();
                var soundEventId = SoundController.createUndirectionalSoundEvent(vehicle.Passengers.Select(p => p.Player), radioStation.Name, radioStation.Source, radioStation.Mount, radioStation.StandardVolume * 0.5f, true);

                var vehicleRadio = new VehicleRadioStation(soundEventId, radioStation.Name, radioStation.StandardVolume, 0.5f);
                vehicle.setData("VEHICLE_RADIO", vehicleRadio);

                player.sendNotification(Constants.NotifactionTypes.Success, $"Radio eingeschaltet. Es läuft: {radioStation.Name}", "Radio aktiviert", Constants.NotifactionImages.Music, "VEHICLE_RADIO");
            }

            return true;
        }

        private bool onDeactivateVehicleRadio(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["VEHICLE"];

            if(vehicle.hasData("VEHICLE_RADIO")) {
                var vehicleRadio = vehicle.getData("VEHICLE_RADIO") as VehicleRadioStation;
                SoundController.removeSoundEvent(vehicleRadio.SoundEventId);
                vehicle.resetData("VEHICLE_RADIO");

                player.sendNotification(Constants.NotifactionTypes.Info, $"Radio ausgeschaltet", "Radio deaktiviert", Constants.NotifactionImages.Music, "VEHICLE_RADIO");
            }

            return true;
        }

        private bool onVehicleRadioVolume(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as ListMenuItemEvent;
            var vehicle = (ChoiceVVehicle)data["VEHICLE"];
        
            if(vehicle.hasData("VEHICLE_RADIO")) {
                var vehicleRadio = vehicle.getData("VEHICLE_RADIO") as VehicleRadioStation;
                vehicleRadio.VolumeModifier = RadioController.getValueFromRadioVolume(evt.currentElement);
                SoundController.changeSoundEventVolume(vehicleRadio.SoundEventId, vehicleRadio.Volume * vehicleRadio.VolumeModifier);
                player.sendNotification(Constants.NotifactionTypes.Info, $"Lautstärke auf {evt.currentElement} gesetzt", "Lautstärke gesetzt", Constants.NotifactionImages.Music, "VEHICLE_RADIO");
            }

            return true;
        }

        private bool onVehicleRadioStation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vehicle = (ChoiceVVehicle)data["VEHICLE"];
            var radio = (RadioStation)data["RADIO"];

            if(vehicle.hasData("VEHICLE_RADIO")) {
                var vehicleRadio = vehicle.getData("VEHICLE_RADIO") as VehicleRadioStation;
                vehicleRadio.RadioStation = radio.Name;
                vehicleRadio.Volume = radio.StandardVolume;

                SoundController.pauseSoundEvent(vehicleRadio.SoundEventId);
                SoundController.playSoundAtCoords(player.Position, 6, SoundController.Sounds.RadioChange, 0.4f, "mp3");
                var anim = AnimationController.getAnimationByName("WORK_FRONT");

                AnimationController.animationTask(player, anim, () => {
                    SoundController.changeSoundEventSource(vehicleRadio.SoundEventId, radio.Name, radio.Source, radio.Mount, true);
                    player.sendNotification(Constants.NotifactionTypes.Info, $"Radio Sender auf {radio.Name} geändert", "Radio Sender geändert", Constants.NotifactionImages.Music, "VEHICLE_RADIO");
                }, null, true, null, TimeSpan.FromSeconds(1.5f));

            }

            return true;
        }
    }
}
