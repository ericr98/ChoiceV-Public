using DiscordBot.Controller.Whitelist.Steps;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Controller {
    public static class MessageController {
        public static DiscordColor CHOICEV_COLOR = new DiscordColor(204, 138, 37);

        public static async void sendEmbedToUser(this DiscordClient bot, ulong userId, string title, string description, string footer = null, List<DiscordEmbedField> fields = null) {
            var guild = bot.Guilds[Config.DiscordServerId];
            var member = await guild.GetMemberAsync(userId);
            var channel = await member.CreateDmChannelAsync();

            var message = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(204, 138, 37))
                .WithTitle(title)
                .WithDescription(description)
                .WithTimestamp(DateTime.Now)
                .WithFooter(footer);


            if(fields != null) {
                foreach(var field in fields) {
                    message.AddField(field.Name, field.Value, field.Inline);
                }
            }
            try {
                await bot.SendMessageAsync(channel, message);
            } catch(Exception e) {
                Console.WriteLine($"Error while sending message to userId: {userId}");
            }
        }

        public static async void sendEmbedInChannel(this DiscordClient bot, DiscordChannel channel, string title, string description, string footer = null, List<DiscordEmbedFieldAbstract> fields = null) {
            await bot.SendMessageAsync(channel, getStandardEmbed(title, description, footer, fields));
        }

        public static DiscordEmbedBuilder getStandardEmbed(string title, string description, string footer = null, List<DiscordEmbedFieldAbstract> fields = null) {
            return getStandardColorEmbed(CHOICEV_COLOR, title, description, footer, fields);
        }

        public static DiscordEmbedBuilder getStandardColorEmbed(DiscordColor color, string title, string description, string footer = null, List<DiscordEmbedFieldAbstract> fields = null) {
            var message = new DiscordEmbedBuilder()
                .WithColor(color)
                .WithTitle(title)
                .WithDescription(description)
                .WithTimestamp(DateTime.Now)
                .WithFooter(footer, "http://choicev-cef.net/src/whitelist_bot/ChoiceV.png")
                .WithAuthor("Whitelistverfahren", "https://www.choicev.net")
                .WithThumbnail("http://choicev-cef.net/src/whitelist_bot/ChoiceVLogo.png");


            if(fields != null) {
                foreach(var field in fields) {
                    message.AddField(field.Name, field.Value, field.Inline);
                }
            }

            return message;
        }

        public static List<DiscordEmbedFieldAbstract> concateMessageFields(this DiscordMessage message, List<DiscordEmbedFieldAbstract> newFields, int embedPos = 0) {
            var embed = message.Embeds[embedPos];

            var list = new List<DiscordEmbedFieldAbstract>();
            if(embed.Fields != null) {
                foreach(var field in embed.Fields) {
                    list.Add(new DiscordEmbedFieldAbstract(field.Name, field.Value, field.Inline));
                }
            }

            return list.Concat(newFields).ToList();
        }

        public static void sendChannelBlockMessage(this DiscordChannel channel, string reason) {
            Program.Bot.sendEmbedInChannel(channel,
                    "Ticket \"Blockiert\"",
                    $"Das Ticket wurde nun \"Blockiert\", das bedeutet dass ein Abbruch nicht mehr möglich ist. Alle entstandenen Daten werden bis aufs weitere gespeichert. Das Ticket wurde mit dem Grund: \"{reason}\" blockiert.",
                    "Ticket wurde blockiert");
        }

        public static void sendChannelUnBlockMessage(this DiscordChannel channel, string reason) {
            Program.Bot.sendEmbedInChannel(channel,
                    "Ticket \"Freigegeben\"",
                    $"Das Ticket wurde nun \"Freigegeben\", das bedeutet dass ein Abbruch wieder möglich ist. Das Ticket wurde mit dem Grund: \"{reason}\" freigegeben.",
                    "Ticket wurde blockiert");
        }
    }
}
