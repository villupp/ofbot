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

            await RespondAsync($"**Some useful commands:**\n`/git`, `/help`, `/customcommand`, `/registration`, `/pubgstats`, `/dotatracker`, `/pubgtracker` \n"
                , ephemeral: true
                );
        }
    }
}