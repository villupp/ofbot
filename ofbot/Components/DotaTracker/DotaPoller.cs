using Microsoft.Extensions.Logging;

namespace OfBot
{
    public class DotaPoller
    {
        private readonly ILogger<DotaPoller> logger;
        private readonly AnnouncementService announcementService;
        private readonly SteamApi steamApi;
        
        public DotaPoller(
            ILogger<DotaPoller> logger,
            AnnouncementService announcementService,
            SteamApi steamApi
            )
        {
            this.logger = logger;
            this.announcementService = announcementService;
            this.steamApi = steamApi;
        }
        public async void Start()
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
            logger.LogInformation("DotaTracker polling service started");
            await announcementService.Announce("bottesting", "general", "Poller started");
            while (await timer.WaitForNextTickAsync())
            {
                logger.LogDebug("Polling for dota matches of tracked players");
                await announcementService.Announce("bottesting", "general", "Polling");
                steamApi.GetRecentDotaMatches("41231571", 1);
            }
        }
    }
}