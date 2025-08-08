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
    public class UndirectionalSoundEvent : SoundEvent {

        public UndirectionalSoundEvent(int id, string sourceIdentifer, string soundPath, string mountPath, bool loop, float volume) : base(id, sourceIdentifer, soundPath, mountPath, loop, volume) {
            
        }

        internal override void onCreate(IPlayer player) {
            player.emitCefEventNoBlock(new SoundNormalEvent(Id, Volume * SoundController.getPlayerSoundEventVolumeModifier(player), SourceIdentifier, getRealUrl(player)));
        }

        internal override void setVolume(IPlayer player, float volume) {
            player.emitCefEventNoBlock(new SoundVolumeChangeDistanceEvent(Id, Volume * SoundController.getPlayerSoundEventVolumeModifier(player)));
        }

        internal override void updateSource(IPlayer player, bool withResume) {
            player.emitCefEventNoBlock(new SoundSourceChangeDistanceEvent(Id, SourceIdentifier, getRealUrl(player), withResume));
        }

        internal override void resumeSound(IPlayer player) {
            player.emitCefEventNoBlock(new PauseResumeSourceChangeDistanceEvent(Id, true));
        }

        internal override void pauseSound(IPlayer player) {
            player.emitCefEventNoBlock(new PauseResumeSourceChangeDistanceEvent(Id, false));
        }

        internal override void onDestroy(IPlayer player) {
            player.emitCefEventNoBlock(new StopSoundDistanceEvent(Id));
        }
    }
}
