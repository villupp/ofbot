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
        private Dictionary<long, List<string>> announceGamesAndPlayers; // long = gameid, List<string> = names of tracked players

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
                logger.LogDebug("Polling for recent dota matches of tracked players");
                await AnnounceRecentMatches();
            } while (await timer.WaitForNextTickAsync());
        }

        private async Task AnnounceRecentMatches()
        {
            announceGamesAndPlayers = new Dictionary<long, List<string>>();

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
                    // Recent match id has changed -> add player and game to announced games
                    playerState.latestMatchId = recentMatch.match_id;

                    if (!announceGamesAndPlayers.ContainsKey(recentMatch.match_id))
                        announceGamesAndPlayers.Add(recentMatch.match_id, new List<string>());
                    
                    announceGamesAndPlayers[recentMatch.match_id].Add(playerState.player.SteamName);
                }
            }

            if (announceGamesAndPlayers?.Count == 0)
                return;

            logger.LogInformation($"Announcing {announceGamesAndPlayers?.Count} games");
            
            foreach (var gameAndPlayers in announceGamesAndPlayers)
            {
                var gameId = gameAndPlayers.Key;
                var players = gameAndPlayers.Value;

                logger.LogInformation($"Announcing game ID {gameId}, players ({players?.Count}): {string.Join(',', players)}");

                var playersStr = "";
                var gameLinkStr = $"<https://www.opendota.com/matches/{gameId}> <https://www.dotabuff.com/matches/{gameId}>";

                foreach (var playerName in players)
                    playersStr += $"{playerName}, ";

                // remove last ", "
                playersStr = playersStr.Substring(0, playersStr.Length - 2);

                await announcementService.Announce(
                    botSettings.DotaTrackerAnnouncementGuild,
                    botSettings.DotaTrackerAnnouncementChannel,
                    $"{playersStr} played a match of dota: {gameLinkStr}"
                );
            }
        }
    }
}