using Discord;
using Microsoft.Extensions.Logging;
using OfBot.Api.Pubg;
using OfBot.Api.Pubg.Models;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;

namespace OfBot.CommandHandlers.PubgStats
{
    public class PubgStatsHandler
    {
        private const string SEASONID_PREFIX_PC = "division.bro.official.pc-2018-";

        private ILogger logger;
        private PubgApiClient pubgClient;
        private TableStorageService<PubgSeason> seasonTableService;
        private TableStorageService<PubgPlayer> playerTableService;

        public List<PubgSeason> Seasons { get; set; }

        public PubgStatsHandler(ILogger<PubgStatsHandler> logger,
            PubgApiClient pubgClient,
            TableStorageService<PubgSeason> seasonTableService,
            TableStorageService<PubgPlayer> playerTableService
            )
        {
            this.logger = logger;
            this.pubgClient = pubgClient;
            this.seasonTableService = seasonTableService;
            this.playerTableService = playerTableService;

            PopulateSeasons().Wait();
        }

        public Embed CreateStatsEmded(PubgPlayer player, PubgSeason season, RankedStats rankedStats)
        {
            var statsStr = "";
            var seasonNumber = season.Id.Replace(SEASONID_PREFIX_PC, "");

            if (rankedStats?.Attributes?.Stats?.SquadFpp == null)
            {
                return new EmbedBuilder()
                 .WithTitle($"PUBG ranked season {seasonNumber} squad FPP stats for player {player.Name}")
                 .WithDescription($"No stats found :(")
                 .WithColor(Color.DarkGrey)
                 .Build();
            }
            var stats = rankedStats.Attributes.Stats.SquadFpp;
            var kdr = 0.00m;
            var kdrDisplay = "N/A";

            if (stats.Deaths > 0)
            {
                kdr = stats.Kills / (decimal)stats.Deaths;
                kdrDisplay = string.Format("{0:0.0#}", kdr);
            }

            statsStr += $"Rank: **{stats.CurrentTier?.Tier} {GetSubTierRomanNumeral(stats.CurrentTier?.SubTier)}** " +
                $"(best: **{stats.BestTier?.Tier} {GetSubTierRomanNumeral(stats.BestTier?.SubTier)}**)";
            statsStr += $"\nMatches: **{stats.RoundsPlayed}** Wins: **{stats.Wins}** (**{string.Format("{0:0.0#}", stats.WinRatio * 100)}%**)";
            statsStr += $"\nAverage rank: **{string.Format("{0:0.0#}", stats.AvgRank)}** Top 10: **{string.Format("{0:0.0#}", stats.Top10Ratio * 100)}%**";
            statsStr += $"\nKDR: **{kdrDisplay}** KDA: **{string.Format("{0:0.0#}", stats.Kda)}** Avg dmg: **{string.Format("{0:0}", stats.DamageDealt/stats.RoundsPlayed)}**";

            var embedBuilder = new EmbedBuilder()
                 .WithTitle($"PUBG ranked season {seasonNumber} squad FPP stats for player {player.Name}")
                 .WithDescription(statsStr)
                 .WithColor(Color.Blue)
                 ;

            return embedBuilder.Build();
        }

        private static string GetSubTierRomanNumeral(string subTier)
        {
            return subTier switch
            {
                "1" => "V",
                "2" => "IV",
                "3" => "III",
                "4" => "II",
                "5" => "I",
                _ => "",
            };
        }

        private async Task PopulateSeasons()
        {
            Seasons = await seasonTableService.Get();
        }

        public async Task<bool> RefreshSeasonCache()
        {
            var seasons = await pubgClient.GetSeasons();

            if (seasons == null || seasons.Count == 0)
                return false;

            if (!(await seasonTableService.DeleteAll()))
            {
                logger.LogError($"RefreshSeasonCache: table clear failed.");
                return false;
            }

            foreach (var season in seasons)
            {
                await seasonTableService.Add(new PubgSeason
                {
                    PartitionKey = "",
                    RowKey = Guid.NewGuid().ToString(),
                    Id = season.Id,
                    IsCurrentSeason = season.Attributes.IsCurrentSeason,
                    IsOffSeason = season.Attributes.IsOffseason
                });
            }

            await PopulateSeasons();
            return true;
        }

        public async Task<PubgPlayer> GetPlayer(string playerName)
        {
            var players = await playerTableService.Get(p => p.Name == playerName);

            if (players != null && players.Count > 0)
                return players[0];

            // If not found --> retrieve from API
            var player = await pubgClient.GetPlayer(playerName);

            if (player?.Attributes == null)
                return null;

            var pubgPlayer = new PubgPlayer()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = "",
                Id = player.Id,
                Name = player.Attributes.Name
            };

            await playerTableService.Add(pubgPlayer);
            return pubgPlayer;
        }

        public async Task<PubgSeason> GetCurrentSeason()
        {
            if (Seasons == null || Seasons.Count == 0)
                await PopulateSeasons();

            return Seasons.Where(s => s.IsCurrentSeason && s.Id.StartsWith(SEASONID_PREFIX_PC)).FirstOrDefault();
        }

        public async Task<RankedStats> GetRankedStats(PubgPlayer player, PubgSeason season)
        {
            logger.LogInformation($"GetRankedStats '{player.Name}', season '{season.Id}'");

            var stats = await pubgClient.GetRankedStats(player.Id, season.Id);
            return stats;
        }
    }
}