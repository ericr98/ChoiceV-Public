using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Web;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ChoiceVServer.Controller.SoundSystem {
    public class SoundController : ChoiceVScript {
        public enum Sounds {
            None,

            ElevatorMusic, //10sec
            ElevatorBell, //3sec
            BagRustle, //3sec
            CarLock, //2sec
            CarUnlock, //2sec
            LighterSound, //2sec
            LockerBreak,
            Indicator,
            WindowOpen,
            WindowClose,
            SeatbeltOn, //3sec
            SeatbeltOff, //3sec
            Bell,
            ScissorsCut, //2sec
            TrimmerCut, //6sec
            ManualShaving, //5sec
            TattooMachine, //5sec
            Spray, //3sec
            ShipHorn, //5sec
            ClothTear, //mp3 2sec,

            BurnerStarting, //mp3 3sec
            FizzlingWater, //mp3 3sec
            CuttingMetal, //mp3 6sec
            ScannerSound, //mp3 3sec
            LiquidPour, //mp3 1sec
            SolidPour, //mp3 1sec

            SandStep, //mp3 1sec

            HeadlightSwitch, //mp3 1sec
            EngineOnSpecial, //wav 1sec
            EngineOn, //mp3 1sec
            BikeEngineOn, //mp3 1sec

            Airport1,
            Airport2,
            Airport3,
            Airport4,
            Airport5,
            Airport6,
            Airport7,
            Airport8,
            Airport9,
            Airport10,
            Terminal10min,
            Terminal5min,
            Terminal2min,

            Dispatch, //mp3 1sec

            Quack1, //mp3 1sec
            Quack2, //mp3 1sec
            Quack3, //mp3 1sec
            QuackRare, //mp3 1sec
            WaterSplash, //mp3 1sec

            RadioChange, //mp3 1sec
            DoorBuzz, //mp3 1sec
            DoorLock, //wav 1sec
            
            MachineHum, //mp3 1:09min
            
            KnifeChopping, //mp3 1sec
            OilFrying, //mp3 25sec
            BoilingWater, //mp3 51min
            GasOven, //mp3 52sec
            
            
            HuskyTalk1, //mp3 1sec
            HuskyTalk2, //mp3 1sec
            HuskyTalk3, //mp3 1sec
            HuskyTalk4, //mp3 1sec
            HuskyTalk5, //mp3 1sec
        }

        private static readonly Dictionary<string, (string Name, Sounds Sound, string Format, float Volume)> SOUND_IDENTIFIERS = new Dictionary<string, (string Name, Sounds Sound, string Format, float Volume)> {
            {"OIL_FRYING", ("Öl Fritteuse", Sounds.OilFrying, "mp3", 0.35f) },
            {"WATER_BOILING", ("Kochendes Wasser", Sounds.BoilingWater, "mp3", 0.5f) },
            {"GAS_OVEN", ("Gas Ofen", Sounds.GasOven, "mp3", 0.3f) },
            
            {"HUSKY_TALK_1", ("Husky Reden 1", Sounds.HuskyTalk1, "mp3", 0.75f)},
            {"HUSKY_TALK_2", ("Husky Reden 2", Sounds.HuskyTalk2, "mp3", 0.75f)},
            {"HUSKY_TALK_3", ("Husky Reden 3", Sounds.HuskyTalk3, "mp3", 0.75f)},
            {"HUSKY_TALK_4", ("Husky Reden 4", Sounds.HuskyTalk4, "mp3", 0.75f)},
            {"HUSKY_TALK_5", ("Husky Reden 5", Sounds.HuskyTalk5, "mp3", 0.75f)},
        };

        private static uint SoundEventId = 0;
        private static List<SoundEvent> AllSyncedSoundEvents = new List<SoundEvent>();
        private static List<SoundEvent> AllSelfManagedSoundEvents = new List<SoundEvent>();

        public SoundController() {
            CharacterSettingsController.addListCharacterSettingBlueprint(
                "SPATIAL_SOUND_ORIGIN", "CAMERA", "3D-Sound-Orientation", "Gibt die Orientierung für die Bestimmung des Spatial-Sounds an",
                new Dictionary<string, string> { { "PLAYER", "Charakterrotation" }, { "CAMERA", "Kameraausrichtung" } },
                onChangeOrigin
            );

            CharacterSettingsController.addListCharacterSettingBlueprint(
                "SOUND_VOLUME_MODIFIER", "4", "Sound Lautstärke", "Gib einen Modifikator für die Lautstärke der Custom Sounds an",
                new Dictionary<string, string> { { "0", "0%" }, { "1", "25%" }, { "2", "50%" }, { "3", "75%" }, { "4", "Standard" }, { "5", "125%" }, { "6", "150%" }, { "7", "175%" }, { "8", "200%" } }
            );

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;
        }

        private void onPlayerConnect(IPlayer player, character character) {
            player.emitClientEvent("SET_AUDIO_ORIGIN", player.getCharSetting("SPATIAL_SOUND_ORIGIN"));

            AllSyncedSoundEvents.ForEach(e => e.onCreate(player));
        }
        
        private void onChangeOrigin(IPlayer player, string settingName, string value) {
            player.emitClientEvent("SET_AUDIO_ORIGIN", value);
        }
        
        public static List<string> getAllSoundNames() {
            return SOUND_IDENTIFIERS.Values.Select(v => v.Name).ToList();
        }
        
        public static string getSoundIdentifier(string name) {
            return SOUND_IDENTIFIERS.FirstOrDefault(v => v.Value.Name == name).Key;
        }

        public static string playSoundAtCoords(Position position, float distance, string soundIdentifier, bool loop = false, Predicate<IPlayer> playPredicate = null) {
            if(SOUND_IDENTIFIERS.TryGetValue(soundIdentifier, out var sound)) {
                return playSoundAtCoords(position, distance, sound.Sound, sound.Volume, sound.Format, loop, playPredicate);
            } else {
                Logger.logError("Sound with identifier " + soundIdentifier + " not found!", "Der Sound mit dem Identifier " + soundIdentifier + " wurde nicht gefunden!");
                return null;
            }
        }
        
        public static string playSoundAtCoords(Position position, float distance, Sounds sound, float volume = 1f, string format = "ogg", bool loop = false, Predicate<IPlayer> playPredicate = null) {
            var evt = new PositionSoundEvent((int)SoundEventId++, $"http://www.choicev-cef.net/src/cef/sounds/{sound}.{format}", position, loop, volume, distance);
            
            foreach (var player in ChoiceVAPI.GetAllPlayers()) {
                evt.onCreate(player);
            }

            if(loop) {
                AllSyncedSoundEvents.Add(evt);
            }

            return evt.Id.ToString();
        }

        public static void stopSound(string identifier) {
            var id = int.Parse(identifier);
            var evt = AllSyncedSoundEvents.FirstOrDefault(e => e.Id == id);
            if(evt != null) {
                AllSyncedSoundEvents.Remove(evt);

                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    evt.onDestroy(player);
                }
            }
        }

        public static float getPlayerSoundVolumeModifier(IPlayer player) {
            return float.Parse(CharacterSettingsController.getCharacterSettingValue(player, "SOUND_VOLUME_MODIFIER")).Map(0, 8, 0, 2);
        }

        public static float getPlayerSoundEventVolumeModifier(IPlayer player) {
            return float.Parse(CharacterSettingsController.getCharacterSettingValue(player, "SOUND_EVENT_VOLUME_MODIFIER")).Map(0, 8, 0, 2);
        }

        public static void playSoundInVehicle(ChoiceVVehicle vehicle, Sounds sound, float volume = 1f, string format = "ogg", bool loop = false) {
            foreach (var passenger in vehicle.PassengerList.Values) {
                playSound(passenger, sound, volume, format, loop);
            }
        }

        public static void playSound(IPlayer player, Sounds sound, float volume = 1f, string format = "ogg", bool loop = false) {
            player.emitClientEvent("PLAY_SOUND",
                    $"http://choicev-cef.net/cef/sounds/{sound}.{format}",
                    volume * 25 * getPlayerSoundVolumeModifier(player));
        }

        public static int createPositionSoundEvent(Position position, string sourceIdentifier, string soundPath, string mountPath, float volume, float maxDistance, bool loop) {
            var evt = new PositionSoundEvent((int)SoundEventId++, sourceIdentifier, soundPath, mountPath, position, loop, volume, maxDistance);

            foreach (var player in ChoiceVAPI.GetAllPlayers()) {
                evt.onCreate(player);
            }

            AllSyncedSoundEvents.Add(evt);

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"Position-Soundevent with Id {evt.Id}, Position: {evt.Position} and Path: {evt.Source} was created");

            return evt.Id;
        }

        public static int createUndirectionalSoundEvent(IEnumerable<IPlayer> listeners, string sourceIdentifiers, string soundPath, string mountPath, float volume, bool loop) {
            var evt = new UndirectionalSoundEvent((int)SoundEventId++, sourceIdentifiers, soundPath, mountPath, loop, volume);

            foreach (var player in listeners) {
                evt.onCreate(player);
            }

            AllSelfManagedSoundEvents.Add(evt);

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"Undirectional-Soundevent with Id {evt.Id} and Path: {evt.Source} was created");

            return evt.Id;
        }

        public static void addListenerToUndirectionalSoundEvent(int id, IPlayer listener) {
            var evt = getSoundEvent(id);

            if(evt != null) {
                evt.onCreate(listener);
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!", listener);
            }
        }

        public static void removeListenerFromUndirectionalSoundEvent(int id, IPlayer listener) {
            var evt = getSoundEvent(id);

            if(evt != null) {
                evt.onDestroy(listener);
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!", listener);
            }
        }

        public static void removeSoundEvent(int id) {
            var evt = getSoundEvent(id);
            if (evt != null) {
                AllSyncedSoundEvents.Remove(evt);

                foreach (var player in ChoiceVAPI.GetAllPlayers()) {
                    evt.onDestroy(player);
                }
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!");
            }
        }

        public static void changeSoundEventSource(int id, string sourceIdentifier, string newSource, string newMount, bool withResume) {
            var evt = getSoundEvent(id);
            if (evt != null) {
                evt.Source = newSource;
                evt.Mount = newMount;
                evt.SourceIdentifier = sourceIdentifier;

                foreach (var player in ChoiceVAPI.GetAllPlayers()) {
                    evt.updateSource(player, withResume);
                }
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!");
            }
        }

        public static void changeSoundEventVolume(int id, float newVolume) {
            var evt = getSoundEvent(id);
            if(evt != null) {
                evt.Volume = newVolume;

                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    evt.setVolume(player, newVolume);
                }
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!");
            }
        }

        public static void updateSoundEventsVolume(IPlayer player) {
            foreach(var evt in AllSyncedSoundEvents) {
                evt.setVolume(player, evt.Volume);
            }
        }

        public static void changePositionSoundEventMaxDistance(int id, float newDistance) {
            var evt = getSoundEvent(id);
            if(evt != null && evt is PositionSoundEvent posEvt) {
                posEvt.MaxDistance = newDistance;

                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    posEvt.setMaxDistance(player, newDistance);
                }
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!");
            }
        }

        public static void resumeSoundEvent(int id) {
            var evt = getSoundEvent(id);
            if(evt != null) {
                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    evt.resumeSound(player);
                }
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!");
            }
        }

        public static void pauseSoundEvent(int id) {
            var evt = getSoundEvent(id);
            if (evt != null) {
                foreach (var player in ChoiceVAPI.GetAllPlayers()) {
                    evt.pauseSound(player);
                }
            } else {
                Logger.logError("SoundEvent with id " + id + " not found!", "Das SoundEvent mit der ID " + id + " wurde nicht gefunden!");
            }
        }

        private static SoundEvent getSoundEvent(int id) {
            return AllSyncedSoundEvents.Concat(AllSelfManagedSoundEvents).FirstOrDefault(e => e.Id == id);
        }
    }
}
