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
            var gitLink = $"https://github.com/villupp/ofbot";

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
            await Context.Channel.SendMessageAsync($"Some useful commands: `{commandPrefix}ofbot git`, `{commandPrefix}ofbot help`, `{commandPrefix}whois <user>`\n" +
                $"To create a registration/lineup session:\n" +
                $"`{commandPrefix}reg <event description>` or `{commandPrefix}r <event description>`\n" +
                $"To bump your session use:\n" +
                $"`{commandPrefix}bump <event ID>` or `{commandPrefix}b <event ID>`\n" +
                $"`{commandPrefix}b` without a session ID bumps your most recent session\n" +
                $"To change description of your most recent registration session use:\n" +
                $"`{commandPrefix}changedescription <new description>` or `{commandPrefix}cd <new description>`\n" +
                $"PUBG ranked squad FPP stats can be queried by player name with the following command:\n" +
                $"`{commandPrefix}pubgstats player <player name>` or `{commandPrefix}ps p <player name>`. Note that player names are case-sensitive.\n" +
                $"Use `{commandPrefix}pubgstats refreshseasons` or `{commandPrefix}ps rs` to refresh season data cache. This has to be done when the season changes" +
                $" to get stats for the latest season. This command might take a while to complete.\n" +
                $"Custom commands use {customCommandPrefix} prefix.\nTo set (add or update) a custom command:\n`{customCommandPrefix}set <command name> <command content>`\n" +
                $"To remove custom command:\n`{customCommandPrefix}remove <command name>`\n" +
                $"To search for custom commands:\n`{customCommandPrefix}search <search input>` or `{customCommandPrefix}s <search input>`"
                );
        }
    }
}