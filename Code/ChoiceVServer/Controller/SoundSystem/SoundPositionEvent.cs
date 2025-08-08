using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.SoundSystem {
    public class PositionSoundEvent : SoundEvent {
        public Position Position { get; set; }
        public float MaxDistance { get; set; }

        public PositionSoundEvent(int id, string sourceIdentifer, string soundPath, string mountPath, Position position, bool loop, float volume, float maxDistance) : base(id, sourceIdentifer, soundPath, mountPath, loop, volume) {
            Position = position;
            MaxDistance = maxDistance;
        }

        public PositionSoundEvent(int id, string soundPath, Position position, bool loop, float volume, float maxDistance) : base(id, "", soundPath, null, loop, volume) {
            Position = position;
            MaxDistance = maxDistance;
        } 

        internal override void onCreate(IPlayer player) {
            if(Mount != null) {
                player.emitCefEventNoBlock(new SoundDistanceEvent(Id, Position, player.Position, MaxDistance, Volume * SoundController.getPlayerSoundEventVolumeModifier(player), SourceIdentifier,
                    getRealUrl(player), Loop));
            } else {
                player.emitCefEventNoBlock(new SoundDistanceEvent(Id, Position, player.Position, MaxDistance, Volume * SoundController.getPlayerSoundEventVolumeModifier(player), SourceIdentifier,
                    Source, Loop));
            }

            player.emitClientEvent("CREATE_DISTANCE_SOUND_EVENT", Id, MaxDistance, Position);
        }

        internal override void setVolume(IPlayer player, float volume) {
            player.emitCefEventNoBlock(new SoundVolumeChangeDistanceEvent(Id, Volume * SoundController.getPlayerSoundEventVolumeModifier(player)));
        }

        internal override void updateSource(IPlayer player, bool withResume) {
            player.emitCefEventNoBlock(new SoundSourceChangeDistanceEvent(Id, SourceIdentifier, getRealUrl(player), withResume));
        }

        internal void setMaxDistance(IPlayer player, float newDistance) {
            player.emitCefEventNoBlock(new SoundDistanceChangeDistanceEvent(Id, newDistance));
        }

        internal override void resumeSound(IPlayer player) {
            player.emitCefEventNoBlock(new PauseResumeSourceChangeDistanceEvent(Id, true));
        }

        internal override void pauseSound(IPlayer player) {
            player.emitCefEventNoBlock(new PauseResumeSourceChangeDistanceEvent(Id, false));
        }

        internal override void onDestroy(IPlayer player) {
            player.emitCefEventNoBlock(new StopSoundDistanceEvent(Id));
            player.emitClientEvent("DELETE_DISTANCE_SOUND_EVENT", Id);
        }
    }
}
