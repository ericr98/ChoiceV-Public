using DiscordBot.Base;
using DiscordBot.Controller.Whitelist.Steps;
using DiscordBot.Model.Database;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Controller.Whitelist {
    public enum LogLevel : int {
        Normal = 0,
        Good = 1,
        Bad = 2,
        Debug = 3,
    }

    public class WhitelistLoggingController : BotScript {
        public WhitelistLoggingController() {

        }

        public async static void addWhiteListLog(LogLevel level, ulong memberId, DiscordChannel channel, int step, string title, string message) {
            using(var db = new WhitelistDb()) {
                var result = await db.whitelist_procedures.Include(w => w.whitelist_procedures_logs).FirstOrDefaultAsync(w => w.channelId == channel.Id && w.userId == memberId && !w.blocked);

                if(result != null) {
                    result.whitelist_procedures_logs.Add(new whitelist_procedures_log {
                        step = step,
                        title = title,
                        message = message,
                        date = DateTime.Now,
                        level = (int)level,
                    });

                    db.SaveChanges();
                }
            }
        }

        private static DiscordColor getLogLevelColor(LogLevel level) {
            switch (level) {
                case LogLevel.Normal:
                    return new DiscordColor(204, 138, 37);
                case LogLevel.Good:
                    return new DiscordColor(87, 242, 135);
                case LogLevel.Bad:
                    return new DiscordColor(237, 66, 69);
                case LogLevel.Debug:
                    return new DiscordColor(88, 101, 242);
                default:
                    return new DiscordColor(0, 0, 0);
            }
        }

        private static string getLogLevelEmoji(LogLevel level) {
            switch(level) {
                case LogLevel.Normal:
                    return "";
                case LogLevel.Good:
                    return ":white_check_mark:";
                case LogLevel.Bad:
                    return ":x:";
                case LogLevel.Debug:
                    return "";
                default:
                    return "";
            }
        }

        public async static void openLoggingChannel(DiscordGuild guild, DiscordMember sender, int id, List<whitelist_procedures_log> entries) {
            var builder = new DiscordOverwriteBuilder(guild.EveryoneRole);
            builder.Deny(Permissions.All);

            var category = guild.GetChannel(Config.WhitelistLogsCategory);
            var channel = await guild.CreateChannelAsync($"{Config.WhitelistLogChannelNamePrefix}-{id}", ChannelType.Text, category, default, null, null, new List<DiscordOverwriteBuilder> { builder });
            await channel.AddOverwriteAsync(sender,
                Permissions.AccessChannels | Permissions.ReadMessageHistory | Permissions.SendMessages | Permissions.ManageChannels
            );
            var supportRole = guild.GetRole(Config.SupportRoleId);
            await channel.AddOverwriteAsync(supportRole, Permissions.UseApplicationCommands);

            foreach(var stepGroup in entries.GroupBy(e => e.step)) {
                var list = new List<DiscordEmbedFieldAbstract>();
                var first = stepGroup.First();

                var counter = 1;
                foreach(var entry in stepGroup) {
                    list.Add(new DiscordEmbedFieldAbstract($"{getLogLevelEmoji((LogLevel)entry.level)} {first.step + 1}.{counter} {entry.title}", $"{entry.message}\nGetätigt am {entry.date}", false));
                    counter++;
                }
                counter = 0;

                var chunks = list.ChunkBy(25);
                foreach(var chunk in chunks) {
                    var embed = MessageController.getStandardEmbed($"Schritt {first.step + 1}", "", $"Erste Aktion: {first.date}", chunk);

                    await Program.Bot.SendMessageAsync(channel, embed);
                }
            }
        }
    }
}
