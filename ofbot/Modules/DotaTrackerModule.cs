using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    [Group("dotatracker")]
    [Alias("dt", "dota")]
    public class DotaTrackerModule : ModuleBase<SocketCommandContext>
    {
        private ILogger logger;
        private TrackedDotaPlayers trackedPlayers;
        public DotaTrackerModule(
            ILogger<DotaTrackerModule> logger,
            TrackedDotaPlayers trackedPlayers
            )
        {
            this.logger = logger;
            this.trackedPlayers = trackedPlayers;
        }

        // Replies with help for dotatracker component
        // -dotatracker help
        [Command("help")]
        [Alias("")]
        [Summary("Help command for dotatracker component.")]
        public async Task Help()
        {
            logger.LogInformation($"Dotatracker help initiated by {Context.User.Username}");

            await Context.Channel.SendMessageAsync("Use `-dotatracker track <accountId>` to track a new player.\nUse `-dotatracker remove <accountId>` to remove a tracked player.");
        }

        // Track a new dota player
        // -dotatracker track 
        [Command("track")]
        [Alias("add")]
        [Summary("Track a new player.")]
        public async Task Track(
            [Remainder][Summary("Dota player account id")] string accountId)
        {
            logger.LogInformation($"Dotatracker track command initiated by {Context.User.Username}");
            try
            {
                await trackedPlayers.Add(accountId, Context.User.Username);
                await Context.Channel.SendMessageAsync($"Tracking dota player {accountId}");
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Could not track player {accountId}: " + e.Message);
            }
        }


        // Remove an existing tracked player
        // -dotatracker untrack 
        [Command("untrack")]
        [Alias("remove")]
        [Summary("Untrack an existing player.")]
        public async Task Untrack(
            [Remainder][Summary("Dota player account id")] string accountId)
        {
            logger.LogInformation($"Dotatracker untrack command initiated by {Context.User.Username}");
            try
            {
                await trackedPlayers.Remove(accountId);
                await Context.Channel.SendMessageAsync($"Removing tracked dota player {accountId}");
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Could remove tracked player {accountId}: " + e.Message);
            }
        }

        // Remove an existing tracked player
        // -dotatracker untrack 
        [Command("list")]
        [Alias("get")]
        [Summary("Get all tracked players.")]
        public async Task List()
        {
            logger.LogInformation($"Dotatracker tracked players list command initiated by {Context.User.Username}");
            try
            {
                var players = trackedPlayers.Get().Select(player => $"{player.AccountId} (Added by {player.AddedBy})");
                if (players.ToList().Count > 0)
                {
                    await Context.Channel.SendMessageAsync($"Currently tracked dota players:\n{String.Join("\n", players)}");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"Currently tracked dota players:\n-");
                }
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Could get tracked players: " + e.Message);
            }
        }
    }
}