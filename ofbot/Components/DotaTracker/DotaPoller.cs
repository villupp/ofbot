using Microsoft.Extensions.Logging;

namespace OfBot
{
    public class DotaPoller
    {
        private readonly ILogger<DotaPoller> logger;
        private readonly AnnouncementService announcementService;
        private readonly SteamApi steamApi;
        private readonly TrackedDotaPlayers trackedPlayers;

        public DotaPoller(
            ILogger<DotaPoller> logger,
            AnnouncementService announcementService,
            SteamApi steamApi,
            TrackedDotaPlayers trackedPlayers
            )
        {
            this.logger = logger;
            this.announcementService = announcementService;
            this.steamApi = steamApi;
            this.trackedPlayers = trackedPlayers;
        }
        public async Task Start()
        {
            logger.LogInformation("Initializing tracked players list");
            await trackedPlayers.Refresh();

            var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            logger.LogInformation("DotaTracker polling service started");
            await announcementService.Announce("bottesting", "general", "Poller started");
            while (await timer.WaitForNextTickAsync())
            {
                logger.LogDebug("Polling for dota matches of tracked players");
                await announcementService.Announce("bottesting", "general", "Polling");
                await steamApi.GetRecentDotaMatches("41231571", 1);
            }
        }
    }
}