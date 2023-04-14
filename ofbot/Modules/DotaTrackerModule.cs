using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OfBot.DotaTracker;
using System.Text.RegularExpressions;

namespace OfBot.Modules
{
    [Group("dotatracker", "")]
    public class DotaTrackerModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger logger;
        private readonly TrackedDotaPlayers playerStates;

        public DotaTrackerModule(
            ILogger<DotaTrackerModule> logger,
            TrackedDotaPlayers playerStates
            )
        {
            this.logger = logger;
            this.playerStates = playerStates;
        }

        // Track a new dota player
        [SlashCommand("track", "")]
        public async Task Track(
            string accountid)
        {
            var initiatedBy = $"{Context.User.Username}#{Context.User.Discriminator}";
            logger.LogInformation($"Dotatracker track command initiated by {initiatedBy}");
            if (!Regex.IsMatch(accountid, "^\\d+$"))
            {
                await RespondAsync("Please provide a valid Dota account id", null, false, true);
            }
            else
            {
                try
                {
                    var player = await playerStates.Add(accountid, initiatedBy);
                    await RespondAsync($"Tracking dota player {player.SteamName} [{accountid}]");
                }
                catch (Exception e)
                {
                    await RespondAsync($"Could not track player {accountid}: {e.Message}");
                }
            }
        }

        // Remove an existing tracked player
        [SlashCommand("untrack", "")]
        public async Task Untrack(
            string accountid)
        {
            logger.LogInformation($"Dotatracker untrack command initiated by {Context.User.Username}");
            try
            {
                var player = await playerStates.Remove(accountid);
                await RespondAsync($"Removed tracked dota player {player.SteamName} [{accountid}]");
            }
            catch (Exception e)
            {
                await RespondAsync($"Could not remove tracked player {accountid}: " + e.Message);
            }
        }

        // Remove an existing tracked player
        [SlashCommand("list", "")]
        public async Task List()
        {
            logger.LogInformation($"Dotatracker tracked players list command initiated by {Context.User.Username}");
            try
            {
                var players = playerStates.trackingStates.Select(state => $"{state.player.SteamName} [{state.player.AccountId}] added by {state.player.AddedBy}");
                if (players.ToList().Count > 0)
                {
                    await RespondAsync($"Currently tracked dota players:\n{string.Join("\n", players)}");
                }
                else
                {
                    await RespondAsync($"Currently tracked dota players:\n-");
                }
            }
            catch (Exception e)
            {
                await RespondAsync($"Could not get tracked players: " + e.Message);
            }
        }
    }
}