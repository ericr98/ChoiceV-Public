using AltV.Net.Data;
using ChoiceVServer.EventSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.SoundSystem {

    #region PositionSoundEvent
    public class SoundDistanceEvent : IPlayerCefEvent {
        public string Event => "PLAY_DISTANCE_SOUND";

        public int soundId;
        public Position soundPos;
        public Position playerPos;
        public float maxDistance;
        public float volume;
        public string sourceIdentifier;
        public string source;
        public bool loop;

        public SoundDistanceEvent(int soundId, Position soundPos, Position playerPos, float maxDistance, float volume, string sourceIdentifier, string source, bool loop) {
            this.soundId = soundId;
            this.soundPos = soundPos;
            this.playerPos = playerPos;
            this.maxDistance = maxDistance;
            this.volume = volume;
            this.sourceIdentifier = sourceIdentifier;
            this.source = source;
            this.loop = loop;
        }
    }

    #endregion

    #region UndrectionalSoundEvent

    public class SoundNormalEvent : IPlayerCefEvent {
        public string Event => "PLAY_NORMAL_SOUND";

        public int soundId;
        public float volume;
        public string sourceIdentifier;
        public string source;

        public SoundNormalEvent(int soundId, float volume, string sourceIdentifier, string source) {
            this.soundId = soundId;
            this.volume = volume;
            this.sourceIdentifier = sourceIdentifier;
            this.source = source;

        }
    }

    #endregion

    public class StopSoundDistanceEvent : IPlayerCefEvent {
        public string Event => "STOP_DISTANCE_SOUND";

        public int soundId;

        public StopSoundDistanceEvent(int soundId) {
            this.soundId = soundId;
        }
    }

    public class SoundVolumeChangeDistanceEvent : IPlayerCefEvent {
        public string Event => "UPDATE_SOUND_VOLUME";

        public int soundId;
        public float volume;

        public SoundVolumeChangeDistanceEvent(int soundId, float volume) {
            this.soundId = soundId;
            this.volume = volume;
        }
    }

    public class SoundSourceChangeDistanceEvent : IPlayerCefEvent {
        public string Event => "UPDATE_SOUND_SOURCE";

        public int soundId;
        public string source;
        public bool withResume;
        public string sourceIdentifier;

        public SoundSourceChangeDistanceEvent(int soundId, string sourceIdentifier, string newSource, bool withResume) {
            this.soundId = soundId;
            this.sourceIdentifier = sourceIdentifier;
            this.source = newSource;
            this.withResume = withResume;
        }
    }

    public class SoundDistanceChangeDistanceEvent : IPlayerCefEvent {
        public string Event => "UPDATE_SOUND_DISTANCE";

        public int soundId;
        public float newDistance;

        public SoundDistanceChangeDistanceEvent(int soundId, float newDistance) {
            this.soundId = soundId;
            this.newDistance = newDistance;
        }
    }

    public class PauseResumeSourceChangeDistanceEvent : IPlayerCefEvent {
        public string Event => "RESUME_PAUSE_SOUND";

        public int soundId;
        public bool resume;

        public PauseResumeSourceChangeDistanceEvent(int soundId, bool resume) {
            this.soundId = soundId;
            this.resume = resume;
        }
    }
}
