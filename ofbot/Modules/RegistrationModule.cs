using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers;

namespace OfBot.Modules
{
    public class RegistrationModule : ModuleBase<SocketCommandContext>
    {
        private ILogger logger;
        private RegistrationHandler registrationHandler;

        public RegistrationModule(ILogger<RegistrationModule> logger, RegistrationHandler registrationHandler)
        {
            this.logger = logger;
            this.registrationHandler = registrationHandler;
        }

        // Starts new registration session
        // -registration <optional custom description>
        // -registration dotkkaa klo 16?
        [Command("registration")]
        [Summary("Initiates a registration message with registration buttons.")]
        [Alias("dotaa", "dota", "cs", "jengi", "letsplay", "reg", "peliä", "matsi", "roster", "rosteri", "pubg", "lineup", "r", "reg", "game", "game" , "g")]
        public async Task StartRegistration([Summary("Optional button description.")] params string[] descParams)
        {
            var description = string.Join(" ", descParams);

            logger.LogInformation($"Registration initiated by {Context.User.Username}: {description}");

            var registerButtonId = Guid.NewGuid();
            var commentButtonId = Guid.NewGuid();
            var unregisterButtonId = Guid.NewGuid();

            if (string.IsNullOrEmpty(description))
                description = $"{Context.User.Username}'s event";

            var session = registrationHandler.CreateSession(registerButtonId, unregisterButtonId, commentButtonId, description, Context.User.Username);
            var embed = RegistrationHandler.CreateLineupEmbed(session);
            var btnComponent = RegistrationHandler.CreateButtonComponent(session);

            var msg = await ReplyAsync(null, components: btnComponent, embed: embed);
            session.Message = msg;
        }
    }
}