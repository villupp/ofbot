using Microsoft.Extensions.Logging;
using OfBot.Components.Api.Dota;

namespace OfBot.Components.DotaTracker
{
    public class DotaPoller
    {
        private readonly ILogger<DotaPoller> logger;
        private readonly AnnouncementService announcementService;
        private readonly DotaApi dotaApi;
        private readonly TrackedDotaPlayers trackedPlayers;
        private readonly BotSettings botSettings;

        public DotaPoller(
            ILogger<DotaPoller> logger,
            AnnouncementService announcementService,
            DotaApi dotaApi,
            TrackedDotaPlayers trackedPlayers,
            BotSettings botSettings
            )
        {
            this.logger = logger;
            this.announcementService = announcementService;
            this.dotaApi = dotaApi;
            this.trackedPlayers = trackedPlayers;
            this.botSettings = botSettings;
        }
        public async Task Start()
        {
            logger.LogInformation("Initializing tracked players list");
            await trackedPlayers.Refresh();

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(botSettings.DotaTrackerPollingIntervalSeconds));
            logger.LogInformation("DotaTracker polling service started");
            do
            {
                logger.LogInformation("Polling for recent dota matches of tracked players");
                await PollRecentMatches();
            } while (await timer.WaitForNextTickAsync());

        }

        private async Task PollRecentMatches()
        {
            foreach (var playerState in trackedPlayers.players)
            {
                try
                {
                    var response = await dotaApi.GetRecentDotaMatches(playerState.player.AccountId, 1);
                    // Make sure response is valid
                    if (
                        response != null &&
                        response.result.status == 1 &&
                        response.result != null &&
                        response.result.num_results > 0
                    )
                    {
                        /* Latest match id is always initially null, initial update (from null to a valid match id) will
                           not trigger an announcement. An announcement about a new detected match is made when the latest
                           match id changes during runtime. */
                        var recentMatchId = response.result.matches[0].match_id;
                        if (playerState.latestMatchId == null)
                        {
                            playerState.latestMatchId = recentMatchId;
                        }
                        else if (playerState.latestMatchId != recentMatchId)
                        {
                            // Recent match id has changed -> announce
                            playerState.latestMatchId = recentMatchId;
                            await announcementService.Announce(
                                botSettings.DotaTrackerAnnouncementGuild,
                                botSettings.DotaTrackerAnnouncementChannel,
                                $"{playerState.player.SteamName} played a match of dota\n<https://www.opendota.com/matches/{recentMatchId}>"
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogInformation($"Failed getting recent matches for account {playerState.player.AccountId}: {e.Message}");
                }

            }
        }
    }
}