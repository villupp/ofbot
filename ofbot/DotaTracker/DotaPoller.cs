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
        private Dictionary<long, List<string>> announceGames; // long = gameid, List<string> = names of tracked players

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
            announceGames = new Dictionary<long, List<string>>();

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

                    if (!announceGames.ContainsKey(recentMatch.match_id))
                        announceGames.Add(recentMatch.match_id, new List<string>() { playerState.player.SteamName });
                    else
                        announceGames[recentMatch.match_id].Add(playerState.player.SteamName);
                }
            }

            foreach (var game in announceGames)
            {
                var playersStr = "";
                var gameStr = $"<https://www.opendota.com/matches/{game.Key}>";

                foreach (var player in game.Value)
                    playersStr += $"{player}, ";

                // remove last ", "
                playersStr = playersStr.Substring(playersStr.Length - 2, 2);

                await announcementService.Announce(
                    botSettings.DotaTrackerAnnouncementGuild,
                    botSettings.DotaTrackerAnnouncementChannel,
                    $"{playersStr} played a match of dota: {gameStr}"
                );
            }
        }
    }
}