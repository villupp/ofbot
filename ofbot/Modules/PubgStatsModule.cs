using Discord.Commands;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.PubgStats;

namespace OfBot.Modules
{
    [Group("pubgstats")]
    [Alias("ps")]
    public class PubgStatsModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger logger;
        private PubgStatsHandler pubgStatsHandler;

        public PubgStatsModule(ILogger<PubgStatsModule> logger, PubgStatsHandler pubgStatsHandler)
        {
            this.logger = logger;
            this.pubgStatsHandler = pubgStatsHandler;
        }

        // Posts PUBG player stats for ongoing season
        // -pubgstats player <playerName>
        // -pubgstats p <playerName>
        // -pubgstats p villu --> stats info
        [Command("player")]
        [Summary("Prints PUBG player stats (ongoing season).")]
        [Alias("p")]
        public async Task CurrentSeasonStats([Summary("PUBG player name")] string playerName)
        {
            logger.LogInformation($"CurrentSeasonStats initiated by {Context.User.Username} for player '{playerName}'");

            if (string.IsNullOrEmpty(playerName))
            {
                await ReplyAsync($"Provide a player name. For example: `-pubgstats villu`");
                return;
            }

            var player = await pubgStatsHandler.GetPlayer(playerName);
            var season = await pubgStatsHandler.GetCurrentSeason();

            if (player == null)
            {
                logger.LogInformation($"Could not retrieve player. Stats not posted.");
                await ReplyAsync($"Player not found. Mind that player names are case-sensitive.");
                return;
            }

            if (season == null)
            {
                logger.LogInformation($"Could not retrieve current season. Stats not posted.");
                await ReplyAsync($"Ranked season not found. There might be an issue. Use `-pubgstats rs` to refresh season cache.");
                return;
            }

            var seasonStats = await pubgStatsHandler.GetRankedStats(player, season);
            var embed = pubgStatsHandler.CreateStatsEmded(player, season, seasonStats);

            await ReplyAsync(null, embed: embed);
        }

        // Refreshes season cache
        // -pubgstats refreshseasons
        // -pubgstats rs
        [Command("refreshseasons")]
        [Summary("Refreshes PUBG season cache.")]
        [Alias("rs")]
        public async Task RefreshSeasons()
        {
            logger.LogInformation($"Refresh PUBG seasons initiated by {Context.User.Username}");

            var success = await pubgStatsHandler.RefreshSeasonCache();

            if (success)
            {
                await ReplyAsync($"Season cache refreshed.");
                logger.LogInformation($"Season cache refresh successful");
            }
            else
            {
                await ReplyAsync($"Season cache refresh failed. See logs for details.");
                logger.LogInformation($"Season cache refresh failed");
            }
        }
    }
}