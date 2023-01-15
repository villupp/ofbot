using Discord.Commands;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers;

namespace OfBot.Modules
{
    public class PubgStatsModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger logger;

        public PubgStatsModule(ILogger<PubgStatsModule> logger)
        {
            this.logger = logger;
        }

        // Starts new registration session
        // -pubgstats <playerName>
        // -pubgstats villu --> stats info
        [Command("pubgstats")]
        [Summary("Prints PUBG player stats (most recent season).")]
        [Alias("ps")]
        public async Task CurrentSeasonStats([Summary("PUBG player name")] string playerName)
        {
            logger.LogInformation($"CurrentSeasonStats initiated by {Context.User.Username} for player '{playerName}'");

            if (string.IsNullOrEmpty(playerName))
            {
                await ReplyAsync($"Provide a player name. For example: `-pubgstats villu`");
                return;
            }

            //var embed = RegistrationHandler.CreateLineupEmbed(session);
            //var btnComponent = RegistrationHandler.CreateButtonComponent(session);

            //var msg = await ReplyAsync(null, components: btnComponent, embed: embed);
        }
    }
}