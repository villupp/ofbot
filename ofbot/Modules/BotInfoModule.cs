using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    public class BotInfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        private ILogger logger;

        public BotInfoModule(ILogger<BotInfoModule> logger)
        {
            this.logger = logger;
        }

        [SlashCommand("git", "")]
        public async Task LinkGit()
        {
            var gitLink = $"https://github.com/villupp/ofbot";
            logger.LogInformation($"LinkGit called. Linking git {gitLink}");
            await RespondAsync($"You can find my source code here: {gitLink}",
                null, false, true);
        }

        [SlashCommand("help", "")]
        public async Task Help()
        {
            logger.LogInformation($"Help called. Sending help message..");

            var commandPrefix = MessageHandler.COMMAND_PREFIX;
            var customCommandPrefix = MessageHandler.CUSTOM_COMMAND_PREFIX;
            await RespondAsync($"**Some useful commands:** `/git`, `/help`\n" +
                $"**To create a registration/lineup session:**\n" +
                $"`{commandPrefix}reg <event description>` or `{commandPrefix}r <event description>`\n" +
                $"**To bump your session use:\n**" +
                $"`{commandPrefix}bump <event ID>` or `{commandPrefix}b <event ID>`\n" +
                $"`{commandPrefix}b` without a session ID bumps your most recent session\n" +
                $"**To change description of your most recent registration session use:**\n" +
                $"`{commandPrefix}changedescription <new description>` or `{commandPrefix}cd <new description>`\n" +
                $"**PUBG ranked squad FPP stats can be queried by player name with the following command:**\n" +
                $"`{commandPrefix}pubgstats player <player name>` or `{commandPrefix}ps p <player name>`. Note that player names are case-sensitive.\n" +
                $"**Use `{commandPrefix}pubgstats refreshseasons` or `{commandPrefix}ps rs` to refresh season data cache. This has to be done when the season changes" +
                $" to get stats for the latest season. This command might take a while to complete.**\n" +
                $"**Custom commands use {customCommandPrefix} prefix.**\n" +
                $"**To set (add or update) a custom command:**\n`{customCommandPrefix}set <command name> <command content>`\n" +
                $"**To remove custom command:**\n`{customCommandPrefix}remove <command name>`\n" +
                $"**To search for custom commands:**\n`{customCommandPrefix}search <search input>` or `{customCommandPrefix}s <search input>`"
                , null, false, true
                );
        }
    }
}