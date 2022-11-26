namespace OfBot
{
    public class BotSettings
    {
        public string BotToken { get; set; }
        public string StorageKey { get; set; }
        public string ApplicationInsightsKey { get; set; }
        public string SteamApiKey { get; set; }
        public string DotaTrackerAnnouncementGuild { get; set; }
        public string DotaTrackerAnnouncementChannel { get; set; }
        public int DotaTrackerPollingIntervalSeconds { get; set; }
    }
}