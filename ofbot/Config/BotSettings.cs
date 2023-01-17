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
        public string PubgApiBaseUrl { get; set; }
        public string PubgApiKey { get; set; }
        public string PubgStatsRankImageTemplateUrl { get; set; } = "";
        public bool DotaTrackerIsEnabled { get; set; } = false;
        public bool PubgTrackerIsEnabled { get; set; } = false;
        public int DotaTrackerPollingIntervalSeconds { get; set; } = 100000;
        public int PubgTrackerPollingIntervalSeconds { get; set; } = 100000;
        public int PubgTrackerPlacementThreshold { get; set; } = 1;
        public string PubgTrackerThumbnailUrlErangel { get; set; } = "";
        public string PubgTrackerThumbnailUrlMiramar { get; set; } = "";
        public string PubgTrackerThumbnailUrlTaego { get; set; } = "";
        
    }
}