namespace System {
    static class Config {
        public static int ServerVersion { get; set; }
        public static string ServerName { get; set; }
        public static int DevServerFlag { get; set; }

        public static int MaintananceMode { get; set; }
        public static bool IsMaintanceMode { get => MaintananceMode == 1; }

        public static int TwoFactorAuthentication { get; set; }

        public static bool IsDevServer { get => DevServerFlag == 1; }
        public static bool IsTwoFactorAuthentication { get => TwoFactorAuthentication == 1; }

        //ChoiceVDb
        public static string DatabaseIp { get; set; }
        public static string DatabasePort { get; set; }
        public static string DatabaseDatabase { get; set; }
        public static string DatabaseUser { get; set; }
        public static string DatabasePassword { get; set; }

        //FVSDb
        public static string FVSDatabaseIp { get; set; }
        public static string FVSDatabasePort { get; set; }
        public static string FVSDatabaseDatabase { get; set; }
        public static string FVSDatabaseUser { get; set; }
        public static string FVSDatabasePassword { get; set; }

        public static string CefIp { get; set; }
        public static string WebSocketIp { get; set; }
        public static string FsIp { get; set; }

        public static int EnableDamageSystem { get; set; }
        public static bool IsDamageSystemEnabled { get => EnableDamageSystem == 1; }
        public static string VoiceSystem { get; set; }
        public static bool IsVoiceEnabled { get =>  string.IsNullOrEmpty(VoiceSystem) || VoiceSystem == "NONE"; }
        public static int EnableHunger { get; set; }
        public static bool IsEnableHunger { get => EnableHunger == 1; }

        public static int EnableDiscordBot { get; set; }
        public static bool IsDiscordBotEnabled { get => EnableDiscordBot == 1; }
        public static string DiscordBotToken { get; set; }
        public static ulong DiscordServerId { get; set; }
        public static ulong DiscordCommandChannelId { get; set; }
        public static ulong DiscordLoggingChannelId { get; set; }

        public static string FtpAddress { get; set; }
        public static string FtpUser { get; set; }
        public static string FtpPassword { get; set; }
        public static string PublicKeyFileName { get; set; }

        public static string RadioProxyIp { get; set; }
        public static string RadioWebhookUser { get; set; }
        public static string RadioWebhookPassword { get; set; }

        public static string WebhookIp { get; set; }

        public static string CefResourcesAddress { get; set; }
        public static string CefResourcesUser { get; set; }
        public static string CefResourcesPassword { get; set; }
        
        //CDN
        public static int UploadResourcesToCDN { get; set; }
        public static bool IsUploadResourcesToCDN { get => UploadResourcesToCDN == 1; }
        public static string CDNFtpAddress { get; set; }
        public static string CDNFtpUser { get; set; }
        public static string CDNFtpPassword { get; set; }
        public static string CDNConnectServerIp { get; set; }


        public static string SocialMediaHost { get; set; }
        public static string SocialMediaJWTProvider { get; set; }
        
        public static string SocialMediaUser { get; set; }
        public static string SocialMediaPassword { get; set; }

        public static int RandomlySpawnedVehicles { get; set; }

        public static int SkipPasswordLogin { get; set; }
        public static bool IsSkipPasswordLoginEnabled { get => SkipPasswordLogin == 1; }

        public static int StressTestActive { get; set; }
        public static bool IsStressTestActive { get => StressTestActive == 1; }
        
        public static string ExternApiUrl { get; set; }
        public static string ExternApiNeededUsername { get; set; }
        public static string ExternApiNeededPassword { get; set; }
        
        //Teamspeak
        public static string TeamspeakUUID { get; set; }
        public static int TeamspeakIngameChannelId { get; set; }
        public static string TeamspeakIngameChannelPassword { get; set; }
        public static int TeamspeakDefaultChannel { get; set; }
        public static string TeamspeakExcluddedChannels { get; set; }
    }
}