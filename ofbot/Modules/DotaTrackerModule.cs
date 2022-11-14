using Discord.Commands;
using OfBot.DotaTracker;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace OfBot.Modules
{
    [Group("dotatracker")]
    [Alias("dt", "dota")]
    public class DotaTrackerModule : ModuleBase<SocketCommandContext>
    {
        private ILogger logger;
        private TrackedDotaPlayers playerStates;
        public DotaTrackerModule(
            ILogger<DotaTrackerModule> logger,
            TrackedDotaPlayers playerStates
            )
        {
            this.logger = logger;
            this.playerStates = playerStates;
        }

        // Replies with help for dotatracker component
        // -dotatracker help
        [Command("help")]
        [Alias("")]
        [Summary("Help command for dotatracker component.")]
        public async Task Help()
        {
            logger.LogInformation($"Dotatracker help initiated by {Context.User.Username}");

            await Context.Channel.SendMessageAsync(
                "Use `-dotatracker track <accountId>` to track a new player.\n" +
                "Use `-dotatracker remove <accountId>` to remove a tracked player.\n" +
                "Use `-dotatracker list` to list existing tracked players.");
        }

        // Track a new dota player
        // -dotatracker track 
        [Command("track")]
        [Alias("add")]
        [Summary("Track a new player.")]
        public async Task Track(
            [Remainder][Summary("Dota player account id")] string accountId)
        {
            var initiatedBy = $"{Context.User.Username}#{Context.User.Discriminator}";
            logger.LogInformation($"Dotatracker track command initiated by {initiatedBy}");
            if (!Regex.IsMatch(accountId, "^\\d+$"))
            {
                await Context.Channel.SendMessageAsync("Please provide a valid Dota account id");
            }
            else
            {
                try
                {
                    var player = await playerStates.Add(accountId, initiatedBy);
                    await Context.Channel.SendMessageAsync($"Tracking dota player {player.SteamName} [{accountId}]");
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync(e.Message);
                }
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
                var player = await playerStates.Remove(accountId);
                await Context.Channel.SendMessageAsync($"Removed tracked dota player {player.SteamName} [{accountId}]");
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Could not remove tracked player {accountId}: " + e.Message);
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
                var players = playerStates.players.Select(state => $"{state.player.SteamName} [{state.player.AccountId}] added by {state.player.AddedBy}");
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
                await Context.Channel.SendMessageAsync($"Could not get tracked players: " + e.Message);
            }
        }
    }
}