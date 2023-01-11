namespace OfBot.Config
{
    public class BotSettings
    {
        public string BotToken { get; set; }
        public string StorageKey { get; set; }
        public string ApplicationInsightsKey { get; set; }
        public string SteamApiKey { get; set; }
        public string AnnouncementGuild { get; set; }
        public string AnnouncementChannel { get; set; }
        public int DotaTrackerPollingIntervalSeconds { get; set; } = 100000;
        public string PubgApiBaseUrl { get; set; }
        public string PubgApiKey { get; set; }
        public int PubgTrackerPollingIntervalSeconds { get; set; } = 100000;
    }
}