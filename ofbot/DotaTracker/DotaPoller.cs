using Microsoft.Extensions.Logging;
using OfBot.Api.Dota;
using OfBot.TableStorage.Models;

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
            foreach (var playerState in trackedPlayers.trackingStates)
            {
                Match recentMatch;
                try
                {
                    recentMatch = await dotaApiClient.GetMostRecentDotaMatch(playerState.player.AccountId);
                }
                catch (Exception)
                {
                    continue;
                }
                /* Latest match id is always initially null, initial update (from null to a valid match id) will
                   not trigger an announcement. An announcement about a new detected match is made when the latest
                   match id changes during runtime. */
                if (playerState.latestMatchId == null)
                {
                    playerState.latestMatchId = recentMatch.match_id;
                }
                else if (playerState.latestMatchId != recentMatch.match_id)
                {
                    logger.LogInformation($"Detected new match for tracked player ID [{playerState.player.AccountId} {playerState.player.SteamName}]");
                    /*  Seems like sometimes match history does not update simultaneously for all tracked players.
                        When a new match gets detected with multiple tracked players, make sure recent match id is
                        greater than player latest match id.
                    */
                    if (recentMatch.match_id < playerState.latestMatchId) {
                        logger.LogInformation($"Detected match id [{recentMatch.match_id}] is older than player latest match id [{playerState.latestMatchId}], skipping");
                        continue;
                    }
                    
                    logger.LogInformation($"Updating recent match id to [{recentMatch.match_id}] from [{playerState.latestMatchId}]");

                    // Recent match id has changed
                    playerState.latestMatchId = recentMatch.match_id;

                    // Update state of each included player in the match to avoid duplicate announcements for each game
                    var includedPlayers = trackedPlayers.trackingStates.Where(
                        trackedPlayer => recentMatch.players.Any(p => p.account_id.ToString() == trackedPlayer.player.AccountId));
                    logger.LogInformation($"Updating latest match id of [{includedPlayers.ToList().Count}] included tracked players");
                    foreach (var state in includedPlayers)
                    {
                        // Skip if recent match id is not different than player state
                        if (state.latestMatchId == recentMatch.match_id) {
                            continue;
                        }
                        logger.LogInformation($"Updating latest match id of [{state.player.AccountId} {state.player.SteamName}] to [{recentMatch.match_id}] from [{state.latestMatchId}]");
                        state.latestMatchId = recentMatch.match_id;
                    }

                    // Make announcement for all tracked players in the match
                    var playerNames = includedPlayers.Select(p => p.player.SteamName).ToList();
                    logger.LogInformation($"Announcing game id [{recentMatch.match_id}], players [{playerNames?.Count}]: {string.Join(", ", playerNames)}");

                    var response = await dotaApiClient.GetMatchDetails(recentMatch.match_id);

                    // Update persona names of players included in the match (match response includes personaName)
                    logger.LogInformation($"Updating personaNames (steamNames) of included players");
                    await UpdatePersonaNames(includedPlayers.ToList(), response);

                    var playerIdList = includedPlayers.Select(p => Int64.Parse(p.player.AccountId)).ToList();
                    var matchDetails = new AnnouncedMatchDetails(response, playerIdList, playerNames);
                    await announcementService.Announce(
                        botSettings.DotaTrackerAnnouncementGuild,
                        botSettings.DotaTrackerAnnouncementChannel,
                        matchDetails.BuildEmbed()
                    );
                }
            }
        }

        private async Task UpdatePersonaNames(List<TrackingState<TrackedDotaPlayer>> states, GetMatchDetailsResponse recentMatch)
        {
            foreach (var state in states)
            {
                var player = recentMatch.result.players.FirstOrDefault(p => p.account_id.ToString() == state.player.AccountId);
                var updated = await trackedPlayers.UpdateSteamName(player.account_id, player.persona);
                if (updated)
                {
                    logger.LogInformation($"Tracked player [{state.player.AccountId}] steamName was updated to '{player.persona}'");
                }
            }
        }
    }
}