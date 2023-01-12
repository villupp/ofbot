using Discord;
using Microsoft.Extensions.Logging;
using OfBot.Api.Pubg;
using OfBot.Api.Pubg.Models;
using OfBot.Config;

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
                try
                {
                    logger.LogDebug("Polling for recent PUBG matches of tracked players");
                    await AnnounceRecentMatches();
                }
                catch (Exception ex)
                {
                    logger.LogError($"PubgPoller iteration failed: {ex}");
                }
            } while (await timer.WaitForNextTickAsync());
        }

        private async Task AnnounceRecentMatches()
        {
            var trackedPlayerNames = trackedPlayerMngr.trackedPlayers.Select(p => p.Player.Name).ToList();
            var trackedPlayers = await pubgClient.GetPlayers(trackedPlayerNames);
            var recentMatchIds = new List<Guid>();
            var announceMatches = new List<MatchResponse>();

            if (trackedPlayers?.Count == 0)
                return;

            // Iterate all tracked players and search for recent matches
            foreach (var player in trackedPlayers)
            {
                PlayerMatch recentMatch;
                var trackingState = trackedPlayerMngr.trackedPlayers.Where(ts => ts.Player.Name == player.Attributes.Name).FirstOrDefault();

                recentMatch = player.Relationships?.Matches.Data?[0] ?? null;

                /* Latest match id is always initially null, initial update (from null to a valid match id) will
                   not trigger an announcement. An announcement about a new detected match is made when the latest
                   match id changes during runtime. */
                if (trackingState.LatestMatchId == null)
                    trackingState.LatestMatchId = recentMatch.Id;
                else if (trackingState.LatestMatchId != recentMatch.Id)
                {
                    logger.LogInformation($"Detected new match for tracked player '{trackingState.Player.Name}' [{trackingState.Player.Id}]");
                    logger.LogInformation($"Updating recent match id to [{recentMatch.Id}] from [{trackingState.LatestMatchId}]");

                    // Recent match id has changed
                    trackingState.LatestMatchId = recentMatch.Id;

                    if (!recentMatchIds.Contains(recentMatch.Id))
                        recentMatchIds.Add(recentMatch.Id);
                }
            }

            // Iterate all recent matches one by one and add to announced matches if valid
            foreach (var matchId in recentMatchIds)
            {
                var matchRes = await pubgClient.GetMatch(matchId);

                if (matchRes?.Match?.Attributes == null)
                {
                    logger.LogInformation($"Could not retrieve match details for match ID {matchId}. Not accouncing.");
                    continue;
                }

                if (matchRes.Match.Attributes.IsCustomMatch
                    || matchRes.Match.Attributes.MatchType != "competitive")
                {
                    logger.LogInformation($"Match ID {matchId} is custom or not competitive. Not announcing.");
                    continue;
                }

                if (!announceMatches.Any(m => m.Match.Id == matchId))
                    announceMatches.Add(matchRes);
            }

            // Announce valid matches
            foreach (var matchRes in announceMatches)
            {
                var announceRosters = new Dictionary<int, List<MatchPlayer>>();

                var trackedMatchPlayers = matchRes.Players.Where(p =>
                    p?.Attributes?.Stats != null
                    && trackedPlayerNames.Contains(p.Attributes.Stats.Name))
                    .ToList();
                var trackedWinPlaces = trackedMatchPlayers.Select(p => p.Attributes.Stats.WinPlace).Distinct();

                foreach (var winPlace in trackedWinPlaces)
                    announceRosters.Add(winPlace, trackedMatchPlayers
                        .Where(p => p.Attributes.Stats.WinPlace == winPlace)
                        .OrderBy(p => p.Attributes.Stats.Name)
                        .ToList());

                foreach (var roster in announceRosters)
                    await AnnounceMatchRoster(matchRes, roster.Value);
            }
        }

        private async Task AnnounceMatchRoster(MatchResponse matchRes, List<MatchPlayer> players)
        {
            var color = Color.DarkRed;
            var winPlace = players[0].Attributes.Stats.WinPlace;

            if (players?.Count == 0)
                return;

            if (winPlace == 1) color = Color.Gold;
            else if (winPlace < 10) color = Color.DarkGreen;

            var playerNames = players.Select(p => p.Attributes.Stats.Name);
            var playerStatsStr = "";
            var playerNamesStr = "";

            foreach (var player in players)
            {
                var playerName = player.Attributes.Stats.Name;

                if (players.Last().Attributes.Stats.Name == playerName)
                    playerNamesStr += " and ";
                else if (players.First().Attributes.Stats.Name != playerName)
                    playerNamesStr += ", ";

                playerNamesStr += $"{playerName}";
            }

            foreach (var player in players)
            {
                var playerName = player.Attributes.Stats.Name;
                var playerStats = player.Attributes.Stats;

                playerStatsStr += $"[{playerName}](https://pubg.op.gg/user/{playerName}) " +
                    $"K: {playerStats.Kills}\tA: {playerStats.Assists}\tDMG: {string.Format("{0:0}", playerStats.DamageDealt)}\n";
            }
            var chicken = "";

            if (winPlace == 1)
                chicken = "WINNER WINNER CHICKEN DINNER!\n";

            var embed = new EmbedBuilder();
            embed.WithColor(color)
            .WithTitle($"{chicken}{playerNamesStr} finished #{winPlace} in a match of PUBG!")
            .WithDescription(
                $"{playerStatsStr}");

            await announcementService.Announce(
                botSettings.AnnouncementGuild,
                botSettings.AnnouncementChannel,
                embed.Build()
            );
        }
    }
}