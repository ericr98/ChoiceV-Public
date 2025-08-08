using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.SoundSystem {
    public abstract class SoundEvent {
        public int Id { get; set; }
        public string Source { get; set; }
        public string Mount { get; set; }
        public string SourceIdentifier { get; set; }

        public bool Loop { get; set; }

        public float Volume { get; set; }

        public SoundEvent(int id, string sourceIdentifer, string soundPath, string mountPath, bool loop, float volume) {
            Id = id;
            Source = soundPath;
            Mount = mountPath;
            SourceIdentifier = sourceIdentifer;
            Loop = loop;
            Volume = volume;
        }

        internal abstract void onCreate(IPlayer player);
        internal abstract void onDestroy(IPlayer player);

        internal abstract void setVolume(IPlayer player, float volume);
        internal abstract void updateSource(IPlayer player, bool withResume);

        internal abstract void resumeSound(IPlayer player);
        internal abstract void pauseSound(IPlayer player);

        protected string getRealUrl(IPlayer player) {
            return $"{Config.RadioProxyIp}/{Mount}?target={Source}&userId={player.getCharacterId()}&token={player.getLoginToken()}&schema={Config.DatabaseDatabase}";
        }
    }
}
