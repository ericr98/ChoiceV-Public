using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Controller.Whitelist.Steps {
    public class InfoWhitelistStep : WhitelistStep {
        private List<DiscordEmbedFieldAbstract> Fields;
        public InfoWhitelistStep(string title, string startText, TimeSpan infoDelay, List<DiscordEmbedFieldAbstract> fields = null) : base(title, startText, infoDelay) {
            Fields = fields;
        }

        public override void onEndStep(DiscordChannel channel, DiscordMember member) { }

        public override Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) { return Task.FromResult(false); }

        public async override void onStartStep(DiscordChannel channel, DiscordMember member) {
            await Task.Delay((int)InfoDelay.TotalMilliseconds);

            var message = getEmbedMessageBuilder(Title, Text, null, Fields);
            await channel.SendMessageAsync(message);
            await Task.Delay((int)InfoDelay.TotalMilliseconds);

            log(LogLevel.Normal, member, channel, "Info gesendet", $"Info mit Inhalt: {Text}");

            finishStep(channel, member);
        }
    }
}
