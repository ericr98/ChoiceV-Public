using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot {
    public static class Config {
        public static string BotToken { get; set; }
        public static string BotLogLevel { get; set; }

        public static string BotCommandsPrefix { get; set; }
        public static ulong DiscordServerId { get; set; }
        public static ulong DiscordSucceedRoleId { get; set; }
        public static string TeamspeakSucceedRoleIds { get; set; }

        public static ulong SupportInfoChannel { get; set; }
        public static ulong SupportRoleId { get; set; }

        public static string WhitelistChannelNamePrefix { get; set; }
        public static string WhitelistLogChannelNamePrefix { get; set; }

        public static ulong WhitelistTicketsCategory { get; set; }
        public static ulong WhitelistLogsCategory { get; set; }

        public static ulong WhitelistStartChannel { get; set; }

        public static ulong ServerEmojiId { get; set; }
        public static string ServerEmojiName { get; set; }


        //EventLoggingController
        public static ulong BotLoggingChannel { get; set; }

        public static string DatabaseIp { get; set; }
        public static string DatabasePort { get; set; }
        public static string DatabaseDatabase { get; set; }
        public static string DatabaseUser { get; set; }
        public static string DatabasePassword { get; set; }

        public static string TeamspeakAddress { get; set; }
        public static int TeamspeakPort { get; set; }
        public static string TeamspeakUser { get; set; }
        public static string TeamspeakPassword { get; set; }

        public static string GitUser { get; set; }
        public static string GitPassword { get; set; }

        public static string AltVServerPath { get; set; }
        public static string AltVWhitelistServerPath { get; set; }
        public static ulong ServerCommandInfoChannel { get; set; }

    }
}
