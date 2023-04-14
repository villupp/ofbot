using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    public class EchoModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger logger;

        public EchoModule(ILogger<EchoModule> logger)
        {
            this.logger = logger;
        }

        // !say hello world -> hello world
        [SlashCommand("say", "")]
        public async Task Say(string message)
        {
            logger.LogInformation($"Say called. Replying: '{message}'");
            await RespondAsync(message);
        }
    }
}