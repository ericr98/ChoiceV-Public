using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using DiscordBot.Controller.Whitelist.Steps;
using DiscordBot.Controller.Whitelist;

namespace DiscordBot.Controller {
    public class SupportController : BotScript {
        public SupportController(){
            EventController.ComponentInteractionDelegate += onComponentInteraction;
        }

        private async void onComponentInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
            if(e.Id == "join_ticket") {              
                var info = e.Message.Embeds[0].Footer.Text.Split('#');

                var member = await e.Guild.GetMemberAsync(e.User.Id);
                var channelId = ulong.Parse(info[1]);
                var channel = e.Guild.GetChannel(channelId);

                if(member != null && channel != null) {
                    if(await makeSupportRankCheck(e.Guild, e.User)) {
                        await channel.AddOverwriteAsync(member,
                            Permissions.AccessChannels | Permissions.ReadMessageHistory | Permissions.SendMessages
                        );
                        Program.Bot.sendEmbedInChannel(channel, "Supporter beigetreten", $"Es ist ein Supporter dem Ticket beigetreten. Der Discord-Tag des Supporters ist: {member.Mention}.");
                    }

                    var embed = e.Message.Embeds[0];
                    var list = e.Message.concateMessageFields(new List<DiscordEmbedFieldAbstract> { new DiscordEmbedFieldAbstract($"{member.DisplayName}#{member.Discriminator} beigetreten", $"{member.Mention} ist um {DateTime.Now} dem Ticket beigetreten", true) });
                    
                    var newMessage = getJoinRequestMessageBuilder(embed.Title, embed.Description, embed.Footer.Text, list);
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(newMessage));
                } else {
                    Program.Bot.sendEmbedToUser(e.User.Id, "Ticket/User existiert nicht mehr!", "Der Ticket oder der User des Tickets existieren nicht mehr auf dem Discord!");
                }
            }
        }

        public async static Task<bool> removeSelfFromTicket(DiscordMember sender, DiscordChannel channel) {
            if(await makeSupportRankCheck(channel.Guild, sender) && WhitelistProcedureManager.interactionPrecheck(channel)) {
                await channel.DeleteOverwriteAsync(sender, "Selbst aus Ticket entfernt");
                Program.Bot.sendEmbedInChannel(channel, "Supporter entfernt", $"Der Support {sender.Mention} hat sich aus dem Ticket entfernt!");
                return true;
            } else {
                return false;
            }
        }

        private async static Task<bool> makeSupportRankCheck(DiscordGuild guild, DiscordUser user) {
            var member = await guild.GetMemberAsync(user.Id);

            if(member.Roles.Any(r => r.Id == Config.SupportRoleId)) {
                return true;
            } else {
                var supportChannel = await Program.Bot.GetChannelAsync(Config.SupportInfoChannel);
                Program.Bot.sendEmbedToUser(user.Id, "Keine Berechtigung!", "Dir fehlt die Whitelist-Support Discord-Rolle um diese Aktion auszuführen!");

                return false;
            }
        }

        public static async void sendJoinRequestToSupportChannel(DiscordMember sender, DiscordChannel channel, string title, string text) {
            var builder = getJoinRequestMessageBuilder(title, text, $"{sender.Id}#{channel.Id}");
            var supportChannel = await Program.Bot.GetChannelAsync(Config.SupportInfoChannel);

            await Program.Bot.SendMessageAsync(supportChannel, builder);
        }

        private static DiscordMessageBuilder getJoinRequestMessageBuilder(string title, string text, string footer, List<DiscordEmbedFieldAbstract> fields = null, bool deativateButton = false) {
            var message = MessageController.getStandardEmbed(title, text, footer, fields);

            var builder = new DiscordMessageBuilder()
            .AddEmbed(message.Build())
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "join_ticket", "Beitreten", deativateButton));

            return builder;
        }

        public async static void deactivateJoinRequestsForTicket(ulong userId, ulong channelId, string reason) {
            var checkString = $"{userId}#{channelId}";

            var channel = await Program.Bot.GetChannelAsync(Config.SupportInfoChannel);
            var messages = (await channel.GetMessagesAsync()).Where(m => m.Embeds != null && m.Embeds.Count > 0 && m.Embeds[0].Footer.Text == checkString).ToList();
        
            foreach(var message in messages) {
                var embed = message.Embeds[0];
                var list = message.concateMessageFields(new List<DiscordEmbedFieldAbstract> { new DiscordEmbedFieldAbstract("Das Ticket wurde geschlossen", $"Das Ticket wurde mit dem Grund: {reason} geschlossen", false) });

                var builder = getJoinRequestMessageBuilder(embed.Title, embed.Description, embed.Footer.Text, list, true);
                
                await message.ModifyAsync(builder);
            }
        }
    }
}
