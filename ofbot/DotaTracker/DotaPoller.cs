using Microsoft.Extensions.Logging;
using OfBot.Api.Dota;

namespace OfBot.DotaTracker
{
    public class DotaPoller
    {
        private readonly ILogger<DotaPoller> logger;
        private readonly AnnouncementService announcementService;
        private readonly DotaApiClient dotaApiClient;
        private readonly TrackedDotaPlayers trackedPlayers;
        private readonly BotSettings botSettings;
        
        public DotaPoller(
            ILogger<DotaPoller> logger,
            AnnouncementService announcementService,
            DotaApiClient dotaApi,
            TrackedDotaPlayers trackedPlayers,
            BotSettings botSettings
            )
        {
            this.logger = logger;
            this.announcementService = announcementService;
            this.dotaApiClient = dotaApi;
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
                await AnnounceRecentMatches();
            } while (await timer.WaitForNextTickAsync());
        }

        private async Task AnnounceRecentMatches()
        {
            foreach (var playerState in trackedPlayers.players)
            {
                var recentMatch = await dotaApiClient.GetMostRecentDotaMatch(playerState.player.AccountId);

                /* Latest match id is always initially null, initial update (from null to a valid match id) will
                   not trigger an announcement. An announcement about a new detected match is made when the latest
                   match id changes during runtime. */
                if (playerState.latestMatchId == null)
                {
                    playerState.latestMatchId = recentMatch.match_id;
                }
                else if (playerState.latestMatchId != recentMatch.match_id)
                {
                    // Recent match id has changed
                    playerState.latestMatchId = recentMatch.match_id;

                    // Update state of each included player in the match to avoid duplicate announcements for each game
                    var includedPlayers = trackedPlayers.players.Where(
                        trackedPlayer => recentMatch.players.Any(p => p.account_id.ToString() == trackedPlayer.player.AccountId));
                    foreach (var player in includedPlayers)
                    {
                        player.latestMatchId = recentMatch.match_id;
                    }

                    // Make announcement for all tracked players in the match
                    var playerNames = includedPlayers.Select(p => p.player.SteamName).ToList();
                    AnnounceMatch(recentMatch.match_id.ToString(), playerNames);
                }
            }
        }

        private async void AnnounceMatch(string matchId, List<string> players)
        {
            logger.LogInformation($"Announcing game ID {matchId}, players ({players?.Count}): {string.Join(",", players)}");

            var playerNames = String.Join(", ", players.SkipLast(1));
            playerNames += players.Count > 1 ? $" and {players.Last()}" : players.Last();
            var gameLink = $"\n<https://www.opendota.com/matches/{matchId}>\n<https://www.dotabuff.com/matches/{matchId}>";
            
            await announcementService.Announce(
                botSettings.DotaTrackerAnnouncementGuild,
                botSettings.DotaTrackerAnnouncementChannel,
                $"{playerNames} played a match of dota: {gameLink}"
            );
        }
    }
}