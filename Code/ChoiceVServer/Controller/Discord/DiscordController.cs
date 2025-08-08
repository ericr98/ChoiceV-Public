#nullable enable
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Discord {
    public class DiscordController : ChoiceVScript {
        private static bool IsOnline = false;
        public static DiscordClient Bot { get; private set; }

        public DiscordController() {
            if(Config.IsDiscordBotEnabled) {
                var thread = new Thread(() => {
                    runBot().GetAwaiter().GetResult();
                });

                thread.Start();
            }
        }

        public static async Task runBot() {
            var config = new DiscordConfiguration {
                Token = Config.DiscordBotToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true,
            };

            Bot = new DiscordClient(config);

            var slash = Bot.UseSlashCommands();
            slash.RegisterCommands<DiscordSlashCommands>(Config.DiscordServerId);

            await Bot.ConnectAsync();

            //Used to delete a specific command (when it is still shown in the Server->Intergrations->Bot->Commands Tab
            //await Bot.DeleteGlobalApplicationCommandAsync(1083020550269579285);

            IsOnline = true;
            await Task.Delay(-1);
        }

        public static async void sendMessageToUser(ulong userId, string message) {
            if(!Config.IsDiscordBotEnabled || !IsOnline) {
                return;
            }

            var guild = Bot.Guilds[Config.DiscordServerId];
            var member = await guild.GetMemberAsync(userId);
            var channel = await member.CreateDmChannelAsync();

            await Bot.SendMessageAsync(channel, message);

        }

        public class DiscordEmbedField {
            public string Name;
            public string Value;
            public bool Inline;

            public DiscordEmbedField(string name, string value, bool inline) {
                Name = name;
                Value = value;
                Inline = inline;
            }
        }

        public static async void sendEmbedToUser(ulong userId, string title, string description, string footer = null, List<DiscordEmbedField> fields = null) {
            if(!Config.IsDiscordBotEnabled || !IsOnline) {
                return;
            }

            var guild = Bot.Guilds[Config.DiscordServerId];
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
                await Bot.SendMessageAsync(channel, message);
            } catch(Exception) {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, "Discord message could not be sent!");
            }
        }

        public static async void sendEmbedInChannel(string title, string description, string footer = null, List<DiscordEmbedField> fields = null, IPlayer mentionPlayer = null) {
            if(!Config.IsDiscordBotEnabled || !IsOnline) {
                return;
            }
            
            var guild = Bot.Guilds[Config.DiscordServerId];
            var channel = guild.GetChannel(Config.DiscordLoggingChannelId);

            if(mentionPlayer != null) {
                var member = await guild.GetMemberAsync(mentionPlayer.getDiscordId());
                title += $": {member.Mention}";
            }

            DiscordEmbedBuilder message;
            try {
                message = new DiscordEmbedBuilder()
                    .WithColor(new DiscordColor(204, 138, 37))
                    .WithTitle(title)
                    .WithDescription(description)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(footer);
            } catch(Exception) {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, "Discord message could not be sent!");
                return;
            }
            
            if(fields != null) {
                foreach(var field in fields) {
                    message.AddField(field.Name, field.Value, field.Inline);
                }

            }
            
            try {
                await Bot.SendMessageAsync(channel, message);
            } catch(Exception ex) {
                Logger.logWarning(LogCategory.System, LogActionType.Blocked, "Discord message could not be sent!");
            }
        }

        public static async Task<int> getAdminLevelOfUser(IPlayer player) {
            var discordId = player.getDiscordId();
            
            return await getAdminLevelByDiscordId(discordId);
        }

        public static async Task<IReadOnlyCollection<DiscordMember>?> getAllGuildMembersAsync() {
            if(!Config.IsDiscordBotEnabled || !IsOnline) {
                return null;
            }

            var guild = Bot.Guilds[Config.DiscordServerId];
            return await guild.GetAllMembersAsync();
        }
        
        public static async Task<int> getAdminLevelByDiscordId(ulong discordId) {
            if (!Config.IsDiscordBotEnabled || !IsOnline) return 0;
            
            var guild = Bot.Guilds[Config.DiscordServerId];
            var member = await guild.GetMemberAsync(discordId);

            var regex = new Regex(@"\(([1-9]+)\) .*");
            var permissionRoles = member.Roles.Where(r => regex.IsMatch(r.Name));

            if (permissionRoles == null) return 0;
            
            var highest = 0;
            foreach(var role in permissionRoles) {
                var match = regex.Match(role.Name);
                highest = Math.Max(highest, int.Parse(match.Groups[1].Value));
            }

            return highest;
        }
    }
}
