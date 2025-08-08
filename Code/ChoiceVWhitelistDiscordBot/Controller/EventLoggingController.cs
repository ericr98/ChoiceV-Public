using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DiscordBot.Controller {
    public class EventLoggingController : BotScript {
        public EventLoggingController() {
            EventController.MemberJoinDelegate += onMemberJoin;
            EventController.MemberLeaveDelegate += onMemberLeave;
            EventController.MemberUpdatedDelegate += onMemberUpdated;
        }

        private void onMemberJoin(GuildMemberAddEventArgs evt) {
            var channel = evt.Guild.GetChannel(Config.BotLoggingChannel);
            var message = new DiscordEmbedBuilder {
                Title = "Spieler beigetreten",
                Color = DiscordColor.Green,
                Description = $"User: {evt.Member.Mention}\nName: {evt.Member.DisplayName}#{evt.Member.Discriminator}",
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = $"Id: {evt.Member.Id}, Datum: {DateTime.Now.ToString("H:mm dd.MM.yyyy")}",
                },
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = evt.Member.GetAvatarUrl(DSharpPlus.ImageFormat.Png) },
            };

            channel.SendMessageAsync(message);
        }

        private void onMemberLeave(GuildMemberRemoveEventArgs evt) {
            var channel = evt.Guild.GetChannel(Config.BotLoggingChannel);
            var message = new DiscordEmbedBuilder {
                Title = "Spieler hat Server verlassen",
                Color = DiscordColor.Red,
                Description = $"Name: {evt.Member.DisplayName}#{evt.Member.Discriminator}",
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = $"Id: {evt.Member.Id}, Datum: {DateTime.Now.ToString("H:mm dd.MM.yyyy")}",
                },
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = evt.Member.GetAvatarUrl(DSharpPlus.ImageFormat.Png) },
            };

            channel.SendMessageAsync(message);
        }

        private void onMemberUpdated(GuildMemberUpdateEventArgs evt) {
            if(evt.RolesAfter != evt.RolesBefore) {
                var before = evt.RolesBefore.Except(evt.RolesAfter);
                if(before.Count() > 0) {
                    onMemberRoleRemoved(evt.Guild, evt.Member, before.First());
                    return;
                }

                var after = evt.RolesAfter.Except(evt.RolesBefore);
                if(after.Count() > 0) {
                    onMemberRoleAdded(evt.Guild, evt.Member, after.First());
                    return;
                }
            }
        }

        private void onMemberRoleAdded(DiscordGuild guild, DiscordMember member, DiscordRole role) {
            var channel = guild.GetChannel(Config.BotLoggingChannel);
            var message = new DiscordEmbedBuilder {
                Author = new DiscordEmbedBuilder.EmbedAuthor {
                    Name = $"{member.DisplayName}#{member.Discriminator}",
                    IconUrl = member.GetAvatarUrl(DSharpPlus.ImageFormat.Png),
                },
                Title = "Spielerrolle erhalten",
                Color = DiscordColor.Azure,
                Description = $"Rolle: **__{role.Name}__**\nName: {member.Mention}",
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = $"Id: {member.Id}, Datum: {DateTime.Now.ToString("H:mm dd.MM.yyyy")}",
                },
            };

            channel.SendMessageAsync(message);
        }

        private void onMemberRoleRemoved(DiscordGuild guild, DiscordMember member, DiscordRole role) {
            var channel = guild.GetChannel(Config.BotLoggingChannel);
            var message = new DiscordEmbedBuilder {
                Author = new DiscordEmbedBuilder.EmbedAuthor {
                    Name = $"{member.DisplayName}#{member.Discriminator}",
                    IconUrl = member.GetAvatarUrl(DSharpPlus.ImageFormat.Png),
                },
                Title = "Spielerrolle entfernt",
                Color = DiscordColor.Azure,
                Description = $"Rolle: **__{role.Name}__**\nName: {member.Mention}",
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = $"Id: {member.Id}, Datum: {DateTime.Now.ToString("H:mm dd.MM.yyyy")}",
                },
            };

            channel.SendMessageAsync(message);
        }

        public async static void log(DiscordMember logger, string title, DiscordColor color, string description) {
            var guild = await Program.Bot.GetGuildAsync(Config.DiscordServerId);
            var channel = guild.GetChannel(Config.BotLoggingChannel);

            var message = new DiscordEmbedBuilder {
                Author = new DiscordEmbedBuilder.EmbedAuthor {
                    Name = $"{logger.DisplayName}#{logger.Discriminator}",
                    IconUrl = logger.GetAvatarUrl(DSharpPlus.ImageFormat.Png),
                },
                Title = title,
                Color = color,
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = $"Id: {logger.Id}, Datum: {DateTime.Now.ToString("H:mm dd.MM.yyyy")}",
                },
            };

            await channel.SendMessageAsync(message);
        }

        public async static void log(string title, DiscordColor color, string description) {
            var guild = await Program.Bot.GetGuildAsync(Config.DiscordServerId);
            var channel = guild.GetChannel(Config.BotLoggingChannel);

            var message = new DiscordEmbedBuilder {
                Author = new DiscordEmbedBuilder.EmbedAuthor {
                    Name = $"Whitelist-Bot",
                },
                Title = title,
                Color = color,
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = $"Datum: {DateTime.Now.ToString("H:mm dd.MM.yyyy")}",
                },
            };

            await channel.SendMessageAsync(message);
        }
    }
}
