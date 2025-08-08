using AltV.Net.Elements.Entities;
using Bogus.DataSets;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Web;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.SoundSystem {
    public class RadioStationTrackChangeEvent : IPlayerCefEvent {
        public string Event => "SET_SOURCE_IDENTIFIER";

        public string name;
        public string track;
        public string artist;

        public RadioStationTrackChangeEvent(string name, string track, string artist) {
            this.name = name;
            this.track = track;
            this.artist = artist;
        }
    }

    public class RadioStation {
        public string Name;
        public string Source;
        public string Mount;
        public float StandardVolume;

        public string CurrentTrack;
        public string CurrentArtist;

        public RadioStation(string name, string source, string mount, float standardVolume) {
            Name = name;
            Source = source;
            Mount = mount;
            StandardVolume = standardVolume;
        }

        public void setCurrentTrack(string track, string artist) {
            CurrentTrack = track;
            CurrentArtist = artist;

            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                if(player.getCharacterFullyLoaded()) {
                    player.emitCefEventNoBlock(new RadioStationTrackChangeEvent(Name, track, artist));
                }
            }
        }
    }

    public class RadioController : ChoiceVScript {
        private static List<RadioStation> RadioStations = new List<RadioStation> {
            new RadioStation("Dusty Trails FM", "https://azuracast.choicev-cef.net:8050", "radio.mp3", 0.15f),
            new RadioStation("Boombox Boulevard Radio", "https://azuracast.choicev-cef.net:8060", "radio.mp3", 0.15f),
            //new RadioStation("Radio Los Santos (GTA)", "https://azuracast.choicev-cef.net:8020", "radio.mp3", 0.15f),
            //new RadioStation("Los Santos Rock Radio (GTA)", "https://azuracast.choicev-cef.net:8030", "radio.mp3", 0.15f),
            //new RadioStation("Rebel Radio (GTA)", "https://azuracast.choicev-cef.net:8000", "radio.mp3", 0.15f),
            new RadioStation("East Los FM (GTA)", "https://azuracast.choicev-cef.net:8040", "radio.mp3", 0.15f),
        };

        public RadioController() {
            WebhookController.registerWebhookCallback(Config.RadioWebhookUser, Config.RadioWebhookPassword, onWebhook);

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnected;

            CharacterSettingsController.addListCharacterSettingBlueprint(
                "SOUND_EVENT_VOLUME_MODIFIER", "4", "Audio-Zonen Lautstärke","Die Lautstärke von Radios und Sound-Zonen (DJs, etc.)",
                new Dictionary<string, string> { { "0", "0%" }, { "1", "25%" }, { "2", "50%" }, { "3", "75%" }, { "4", "Standard" }, { "5", "125%" }, { "6", "150%" }, { "7", "175%" }, { "8", "200%" } },
                onChangeSoundEventPostion
             );
        }

        private void onChangeSoundEventPostion(IPlayer player, string settingName, string value) {
            SoundController.updateSoundEventsVolume(player);
        }

        private void onPlayerConnected(IPlayer player, character character) {
            foreach(var radioStation in RadioStations) {
                player.emitCefEventNoBlock(new RadioStationTrackChangeEvent(radioStation.Name, radioStation.CurrentTrack, radioStation.CurrentArtist));
            }
        }

        private class RadioWebhookRoot {
            public Station station { get; set; }
            public NowPlaying now_playing { get; set; }
        }

        private class NowPlaying {
            public Song song { get; set; }
        }

        private class Song {
            public string text { get; set; }
            public string artist { get; set; }
            public string title { get; set; }
            public string album { get; set; }
        }

        private class Station {
            public string name { get; set; }
        }


        private void onWebhook(string data) {
            var obj = data.FromJson<RadioWebhookRoot>();
            var station = getRadioStationByName(obj.station.name);

            station.setCurrentTrack(obj.now_playing.song.title, obj.now_playing.song.artist);
        }

        public static List<RadioStation> getRadioStations() {
            return RadioStations;
        }

        public static RadioStation getRadioStationByName(string name) {
            return RadioStations.FirstOrDefault(x => x.Name == name);
        }

        public static string[] getRadioVolumes(float currentModifier, int fiverSteps = 40) {
            var volumes = new List<string>();

            var shift = 0;
            for(int i = 0; i <= fiverSteps; i++) {
                if(i * 5 == Math.Round(currentModifier * 100)) {
                    shift = i;
                }
                volumes.Add(i * 5 + "%");
            }

            var elements = volumes.ShiftLeft(shift).ToArray();
            return elements;
        }

        public static float getValueFromRadioVolume(string volume) {
            return float.Parse(volume.Replace("%", "")) / 100;
        }
    }
}
