using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using OfBot.PubgTracker;

namespace OfBot.Modules
{
    [Group("pubgtracker")]
    [Alias("pt")]
    public class PubgTrackerModule : ModuleBase<SocketCommandContext>
    {
        private ILogger logger;
        private TrackedPubgPlayerManager trackedPlayerMngr;

        public PubgTrackerModule(
            ILogger<PubgTrackerModule> logger,
            TrackedPubgPlayerManager trackedPlayerMngr
            )
        {
            this.logger = logger;
            this.trackedPlayerMngr = trackedPlayerMngr;
        }

        // Replies with help for dotatracker component
        // -pubgtracker help
        [Command("help")]
        [Alias("h", "?", "info")]
        [Summary("Help command for pubgtracker component.")]
        public async Task Help()
        {
            logger.LogInformation($"Pubgtracker help initiated by {Context.User.Username}");

            await Context.Channel.SendMessageAsync(
                "Use `-pubgtracker track <playername>` to track a new player.\n" +
                "Use `-pubgtracker remove <playername>` to remove a tracked player.\n" +
                "Use `-pubgtracker list` to list existing tracked players.");
        }

        // Track a new dota player
        // -pubgtracker track 
        [Command("track")]
        [Alias("add")]
        [Summary("Track a new player.")]
        public async Task Track(
            [Remainder][Summary("Pubg player name")] string playerName)
        {
            var initiatedBy = $"{Context.User.Username}#{Context.User.Discriminator}";
            logger.LogInformation($"Dotatracker track command initiated by {initiatedBy}. Player name: '{playerName}'");

            
                try
                {
                    var player = await trackedPlayerMngr.Add(playerName, initiatedBy);
                    await Context.Channel.SendMessageAsync($"Tracking dota player {player.SteamName} [{playerName}]");
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync(e.Message);
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
                var player = await trackedPlayerMngr.Remove(accountId);
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
                var players = trackedPlayerMngr.trackingStates.Select(state => $"{state.player.SteamName} [{state.player.AccountId}] added by {state.player.AddedBy}");
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