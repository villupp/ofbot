using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    public class EchoModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger logger;

        public EchoModule(ILogger<EchoModule> logger)
        {
            this.logger = logger;
        }

        // !say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task Say([Remainder][Summary("The text to echo")] string echo)
        {
            logger.LogInformation($"Replying: '{echo}'");
            return ReplyAsync(echo);
        }
    }
}