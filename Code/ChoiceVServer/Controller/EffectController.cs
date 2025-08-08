using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using System;

namespace ChoiceVServer.Controller {
    public class EffectController : ChoiceVScript {
        public static void playScreenEffect(IPlayer player, ScreenEffect effect) {
            player.emitClientEvent(Constants.PlayerStartScreenEffect, effect.EffectName, effect.Duration.TotalMilliseconds, effect.Looped);

            InvokeController.AddTimedInvoke("Screen-Effect-Wearoff:" + player.getCharacterId(), (ivk) => stopScreenEffects(player), effect.Duration, false);
        }

        public static void stopScreenEffects(IPlayer player) {
            for(int i = 0; i <= 3; i++) {
                player.emitClientEvent(Constants.PlayerStopScreenEffect);
            }
        }
    }

    public class ScreenEffect {
        public string EffectName;
        public TimeSpan Duration;
        public bool Looped;

        public ScreenEffect(string effectName, TimeSpan duration, bool looped) {
            EffectName = effectName;
            Duration = duration;
            Looped = looped;
        }
    }
}
