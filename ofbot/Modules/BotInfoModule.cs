using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    // Create a module with the 'ofbot' prefix
    // -ofbot <command> <parameters>
    [Group("ofbot", "ofbot information.")]
    public class BotInfoModule : InteractionModuleBase
    {
        private readonly ILogger logger;

        public BotInfoModule(ILogger<BotInfoModule> logger)
        {
            this.logger = logger;
        }

        // /ofbot --> Hi!
        [SlashCommand("ofbot", "Says hello.", true)]
        public async Task Main()
        {
            await ReplyAsync($"Hi!");
        }

        // /ofbot git --> <github link>
        [SlashCommand("git", "Link to ofbot git version control.")]
        public async Task LinkGit()
        {
            var gitLink = $"https://github.com/villupp/ofbot";

            logger.LogInformation($"LinkGit called. Linking git {gitLink}");

            await ReplyAsync($"You can find my source code here: {gitLink}");
        }

        // /ofbot help --> <help text>
        [SlashCommand("help", "Provides general information about bot.")]
        public async Task Help()
        {
            logger.LogInformation($"Help called. Sending help message..");

            var commandPrefix = MessageHandler.COMMAND_PREFIX;
            var customCommandPrefix = MessageHandler.CUSTOM_COMMAND_PREFIX;
            await ReplyAsync($"Some useful commands: {commandPrefix}ofbot git, {commandPrefix}ofbot help, {commandPrefix}whois <user>.\n" +
                $"To create a registration/lineup session:\n" +
                $"{commandPrefix}reg <event description> or {commandPrefix}r <event description>\n" +
                $"Custom commands use {customCommandPrefix} prefix.\nTo set (add or update) a custom command:\n{customCommandPrefix}set <command name> <command content>.\n" +
                $"To remove custom command:\n{customCommandPrefix}remove <command name>.\n" +
                $"To search for custom commands:\n{customCommandPrefix}search <search input> or {customCommandPrefix}s <search input>"
                );
        }
    }
}