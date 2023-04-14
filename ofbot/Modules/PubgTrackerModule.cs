using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OfBot.PubgTracker;
using System.Text.RegularExpressions;

namespace OfBot.Modules
{
    [Group("pubgtracker", "")]
    public class PubgTrackerModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger logger;
        private readonly TrackedPubgPlayerManager trackedPlayerMngr;

        public PubgTrackerModule(
            ILogger<PubgTrackerModule> logger,
            TrackedPubgPlayerManager trackedPlayerMngr
            )
        {
            this.logger = logger;
            this.trackedPlayerMngr = trackedPlayerMngr;
        }

        // Track a new dota player
        [SlashCommand("track", "")]
        public async Task Track(
            string playername)
        {
            var initiatedBy = $"{Context.User.Username}#{Context.User.Discriminator}";
            logger.LogInformation($"PubgTracker track command initiated by {initiatedBy}. Player name: '{playername}'");

            try
            {
                var player = await trackedPlayerMngr.Add(playername, initiatedBy);
                await RespondAsync($"Tracking PUBG player {player.Name} [{player.Id}]");
            }
            catch (Exception e)
            {
                await RespondAsync(e.Message);
            }
        }

        // Remove an existing tracked player
        [SlashCommand("untrack", "")]
        public async Task Untrack(
            string playername)
        {
            logger.LogInformation($"PubgTracker untrack command initiated by {Context.User.Username}");
            try
            {
                var player = await trackedPlayerMngr.Remove(playername);
                await RespondAsync($"Removed tracked PUBG player {player.Name} [{player.Id}]");
            }
            catch (Exception e)
            {
                await RespondAsync($"Could not remove tracked PUBG player {playername}: " + e.Message);
            }
        }

        // Lists all tracked players
        [SlashCommand("list", "")]
        public async Task List()
        {
            logger.LogInformation($"PubgTracker tracked players list command initiated by {Context.User.Username}");

            try
            {
                var players = trackedPlayerMngr.trackedPlayers.Select(state => $"{state.Player.Name} [{state.Player.Id}] added by {state.Player.AddedBy}");
                if (players.ToList().Count > 0)
                {
                    await RespondAsync($"Currently tracked PUBG players:\n{string.Join("\n", players)}");
                }
                else
                {
                    await RespondAsync($"Currently tracked PUBG players:\n-");
                }
            }
            catch (Exception e)
            {
                await RespondAsync($"Could not get tracked players: " + e.Message);
            }
        }
    }
}