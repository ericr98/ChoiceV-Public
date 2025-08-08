using DiscordBot.Model.Database;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot.Controller.Whitelist.Steps {
    public class DiscordEmbedFieldAbstract {
        public string Name;
        public string Value;
        public bool Inline;
        public DiscordEmbedFieldAbstract(string name, string value, bool inline) {
            Name = name;
            Value = value;
            Inline = inline;
        }
    }

    public abstract class WhitelistStep {
        private static int UniqueIdCounter;
        protected int UniqueId;

        protected WhitelistProcedureManager Main;
        public int Order { get; private set; }
        protected string Title;
        protected string Text;
        protected TimeSpan InfoDelay;

        public bool AllowsExternalInteraction { get; private set; }

        public WhitelistStep(string title, string text, TimeSpan infoDelay, bool allowsExternalInteraction = false) {
            UniqueId = UniqueIdCounter++;

            Main = null;
            Order = -1;
            Title = title;
            Text = text;
            InfoDelay = infoDelay;
            AllowsExternalInteraction = allowsExternalInteraction;
        }

        public void setInfo(WhitelistProcedureManager main, int order) {
            Main = main;
            Order = order;
        }

        protected void finishStep(DiscordChannel channel, DiscordMember member) {
            Main.finishStep(this, channel, member);
            log(LogLevel.Normal, member, channel, "Schritt abgeschlossen", $"Schritt erfolgreich abgeschlossen");
        }

        public abstract void onStartStep(DiscordChannel channel, DiscordMember member);
        public abstract void onEndStep(DiscordChannel channel, DiscordMember member);
        public abstract Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data);
        public virtual bool getHaltedBlocked(List<whitelist_procedure_datum> data) { return false; }

        protected virtual DiscordMessageBuilder getEmbedColorMessageBuilder(DiscordColor color, string title, string text, string footer = null, List<DiscordEmbedFieldAbstract> fields = null, string imageUrl = null) {
            var embed = MessageController.getStandardColorEmbed(color, title, text, footer ?? $"Verfahrenstufe: {Order + 1}/{WhitelistProcedureManager.StepCount}", fields);

            if(imageUrl != null) {
                embed.WithImageUrl(imageUrl);
            }
            var builder = new DiscordMessageBuilder()
            .AddEmbed(embed.Build());

            return builder;
        }

        protected virtual DiscordMessageBuilder getEmbedMessageBuilder(string title, string text, string footer = null, List<DiscordEmbedFieldAbstract> fields = null, string imageUrl = null) {
            var embed = MessageController.getStandardEmbed(title, text, footer ?? $"Verfahrenstufe: {Order + 1}/{WhitelistProcedureManager.StepCount}", fields);

            if(imageUrl != null) {
                embed.WithImageUrl(imageUrl);
            }
            var builder = new DiscordMessageBuilder()
            .AddEmbed(embed.Build());

            return builder;
        }

        protected void log(LogLevel level, DiscordMember member, DiscordChannel channel, string title, string message) {
            WhitelistLoggingController.addWhiteListLog(level, member.Id, channel, Order, title, message);
        }
    }
}
