using Discord.Commands;
using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    public class EchoModule : InteractionModuleBase
    {
        private readonly ILogger logger;

        public EchoModule(ILogger<EchoModule> logger)
        {
            this.logger = logger;
        }

        // /say hello world -> hello world
        [SlashCommand("say", "Echoes a message.")]
        public Task Say([Remainder] string echo)
        {
            logger.LogInformation($"Echo called. Replying: '{echo}'");
            return ReplyAsync(echo);
        }
    }
}