using Discord;
using Microsoft.Extensions.Logging;
using OfBot.Config;
using OfBot.PubgTracker.Api;
using OfBot.PubgTracker.Api.Models;

namespace OfBot.PubgTracker
{
    public class PubgPoller
    {
        private readonly ILogger<PubgPoller> logger;
        private readonly AnnouncementService announcementService;
        private readonly PubgApiClient pubgClient;
        private readonly TrackedPubgPlayerManager trackedPlayerMngr;
        private readonly BotSettings botSettings;

        public PubgPoller(
            ILogger<PubgPoller> logger,
            AnnouncementService announcementService,
            PubgApiClient pubgClient,
            TrackedPubgPlayerManager trackedPlayerMngr,
            BotSettings botSettings
            )
        {
            this.logger = logger;
            this.announcementService = announcementService;
            this.pubgClient = pubgClient;
            this.trackedPlayerMngr = trackedPlayerMngr;
            this.botSettings = botSettings;
        }

        public async Task Start()
        {
            logger.LogInformation("Initializing tracked players list");
            await trackedPlayerMngr.Refresh();

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(botSettings.PubgTrackerPollingIntervalSeconds));
            logger.LogInformation("PubgTracker polling service started");
            do
            {
                logger.LogDebug("Polling for recent PUBG matches of tracked players");
                await AnnounceRecentMatches();
            } while (await timer.WaitForNextTickAsync());
        }

        private async Task AnnounceRecentMatches()
        {
            var trackedPlayerNames = trackedPlayerMngr.trackedPlayers.Select(p => p.Player.Name).ToList();
            var players = await pubgClient.GetPlayers(trackedPlayerNames);
            var announceGames = new Dictionary<Game, List<Player>>();
            
            if (players?.Count == 0)
                return;

            foreach (var player in players)
            {
                Match recentMatch;
                var trackingState = trackedPlayerMngr.trackedPlayers.Where(ts => ts.Player.Name == player.Attributes.Name).FirstOrDefault();

                try
                {
                    recentMatch = player.Relationships?.Matches.Data?[0] ?? null;
                }
                catch (Exception)
                {
                    continue;
                }

                /* Latest match id is always initially null, initial update (from null to a valid match id) will
                   not trigger an announcement. An announcement about a new detected match is made when the latest
                   match id changes during runtime. */
                if (trackingState.LatestMatchId == null)
                {
                    trackingState.LatestMatchId = recentMatch.Id;
                }
                else if (trackingState.LatestMatchId != recentMatch.Id)
                {
                    logger.LogInformation($"Detected new match for tracked player '{trackingState.Player.Name}' [{trackingState.Player.Id}]");
                    logger.LogInformation($"Updating recent match id to [{recentMatch.Id}] from [{trackingState.LatestMatchId}]");

                    // Recent match id has changed
                    trackingState.LatestMatchId = recentMatch.Id;

                    // Update state of each included player in the match to avoid duplicate announcements for each game
                    //var includedPlayers = trackedPlayerMngr.trackingStates.Where(
                    //    trackedPlayer => recentMatch.players.Any(p => p.account_id.ToString() == trackedPlayer.player.AccountId));
                    //logger.LogInformation($"Updating latest match id of [{includedPlayers.ToList().Count}] included tracked players");
                    //foreach (var state in includedPlayers)
                    //{
                    //    // Skip if recent match id is not different than player state
                    //    if (state.latestMatchId == recentMatch.match_id)
                    //    {
                    //        continue;
                    //    }
                    //    logger.LogInformation($"Updating latest match id of [{state.player.AccountId} {state.player.SteamName}] to [{recentMatch.match_id}] from [{state.latestMatchId}]");
                    //    state.latestMatchId = recentMatch.match_id;
                    //}

                    //// Make announcement for all tracked players in the match
                    //var playerNames = includedPlayers.Select(p => p.player.SteamName).ToList();
                    //logger.LogInformation($"Announcing game id [{recentMatch.match_id}], players [{playerNames?.Count}]: {string.Join(", ", playerNames)}");

                    //var response = await dotaApiClient.GetMatchDetails(recentMatch.match_id);

                    //// Update persona names of players included in the match (match response includes personaName)
                    //logger.LogInformation($"Updating personaNames (steamNames) of included players");
                    //await UpdatePersonaNames(includedPlayers.ToList(), response);

                    //var playerIdList = includedPlayers.Select(p => Int64.Parse(p.player.AccountId)).ToList();
                    //var matchDetails = new AnnouncedMatchDetails(response, playerIdList, playerNames);
                    //await announcementService.Announce(
                    //    botSettings.AnnouncementGuild,
                    //    botSettings.AnnouncementChannel,
                    //    matchDetails.BuildEmbed()
                    //);
                }
            }
        }
    }
}