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
            await RespondAsync($"**Some useful commands:** `/git`, `/help`, `/registration`, `/pubgstats`, `/dotatracker`, `/pubgtracker` \n" +
                $"**Custom commands use {customCommandPrefix} prefix.**\n" +
                $"**To set (add or update) a custom command:**\n`{customCommandPrefix}set <command name> <command content>`\n" +
                $"**To remove custom command:**\n`{customCommandPrefix}remove <command name>`\n" +
                $"**To search for custom commands:**\n`{customCommandPrefix}search <search input>` or `{customCommandPrefix}s <search input>`"
                , null, false, true
                );
        }
    }
}