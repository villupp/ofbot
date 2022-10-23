using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    // Create a module with the 'ofbot' prefix
    // -ofbot <command> <parameters>
    [Group("ofbot")]
    public class BotInfoModule : ModuleBase<SocketCommandContext>
    {
        private ILogger logger;

        public BotInfoModule(ILogger<BotInfoModule> logger)
        {
            this.logger = logger;
        }

        // -ofbot git --> <github link>
        [Command("git")]
        [Summary("Link to ofbot git version control.")]
        public async Task LinkGit()
        {
            var gitLink = $"https://github.com/villupp/ofbot/tree/main/ofbot/TableStorage";

            logger.LogInformation($"LinkGit called. Linking git {gitLink}");

            await Context.Channel.SendMessageAsync($"You can find my source code here: {gitLink}");
        }

        // -ofbot help --> <help text>
        // -ofbot ? --> <help text>
        [Command("help")]
        [Summary("Provides general information about bot.")]
        [Alias("how", "h", "?")]
        public async Task Help()
        {
            logger.LogInformation($"Help called. Sending help message..");

            var commandPrefix = MessageHandler.COMMAND_PREFIX;
            var customCommandPrefix = MessageHandler.CUSTOM_COMMAND_PREFIX;
            await Context.Channel.SendMessageAsync($"Some useful commands: {commandPrefix}ofbot git, {commandPrefix}ofbot help, {commandPrefix} whois <user>.\n" +
                $"Custom commands use {customCommandPrefix} prefix.\nTo set (add or update) a custom command:\n{customCommandPrefix}set <command name> <command content>.\n" +
                $"To remove custom command:\n{customCommandPrefix}remove <command name>.");
        }
    }
}