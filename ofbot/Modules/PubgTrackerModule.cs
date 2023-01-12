using Discord.Commands;
using Microsoft.Extensions.Logging;
using OfBot.Api;
using OfBot.Api.Pubg;
using System.Text.RegularExpressions;

namespace OfBot.Modules
{
    [Group("pubgtracker")]
    [Alias("pt")]
    public class PubgTrackerModule : ModuleBase<SocketCommandContext>
    {
        private ILogger logger;
        private TrackedPubgPlayerManager trackedPlayerMngr;
        private PubgApiClient pubgClient;

        public PubgTrackerModule(
            ILogger<PubgTrackerModule> logger,
            TrackedPubgPlayerManager trackedPlayerMngr,
            PubgApiClient pubgClient
            )
        {
            this.logger = logger;
            this.trackedPlayerMngr = trackedPlayerMngr;
            this.pubgClient = pubgClient;
        }

        // Replies with help for pubgtracker component
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
            logger.LogInformation($"PubgTracker track command initiated by {initiatedBy}. Player name: '{playerName}'");

            try
            {
                var player = await trackedPlayerMngr.Add(playerName, initiatedBy);
                await Context.Channel.SendMessageAsync($"Tracking PUBG player {player.Name} [{player.Id}]");
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(e.Message);
            }
        }

        // Remove an existing tracked player
        // -pubgtracker untrack
        [Command("untrack")]
        [Alias("remove", "r")]
        [Summary("Untrack a tracked player.")]
        public async Task Untrack(
            [Remainder][Summary("PUBG player name")] string playerName)
        {
            logger.LogInformation($"PubgTracker untrack command initiated by {Context.User.Username}");
            try
            {
                var player = await trackedPlayerMngr.Remove(playerName);
                await Context.Channel.SendMessageAsync($"Removed tracked PUBG player {player.Name} [{player.Id}]");
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Could not remove tracked PUBG player {playerName}: " + e.Message);
            }
        }

        [Command("list")]
        [Alias("l")]
        [Summary("Get all tracked players.")]
        public async Task List()
        {
            logger.LogInformation($"PubgTracker tracked players list command initiated by {Context.User.Username}");

            try
            {
                var players = trackedPlayerMngr.trackedPlayers.Select(state => $"{state.Player.Name} [{state.Player.Id}] added by {state.Player.AddedBy}");
                if (players.ToList().Count > 0)
                {
                    await Context.Channel.SendMessageAsync($"Currently tracked PUBG players:\n{string.Join("\n", players)}");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"Currently tracked PUBG players:\n-");
                }
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync($"Could not get tracked players: " + e.Message);
            }
        }
    }
}