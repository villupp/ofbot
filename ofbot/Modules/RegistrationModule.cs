using Discord.Commands;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.Registration;
using OfBot.CommandHandlers.Registration.Models;
using OfBot.Common;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
        [Alias("dotaa", "dota", "cs", "jengi", "letsplay", "reg", "peliä", "matsi", "roster", "rosteri", "pubg", "lineup", "r", "reg", "game", "g")]
        public async Task StartRegistration([Summary("Optional button description.")] params string[] descParams)
        {
            var description = string.Join(" ", descParams);
            description = StringHelpers.RemoveDiscordMarkdown(description);

            logger.LogInformation($"Registration initiated by {Context.User.Username}: {description}");

            var registerButtonId = Guid.NewGuid();
            var commentButtonId = Guid.NewGuid();
            var unregisterButtonId = Guid.NewGuid();

            if (string.IsNullOrEmpty(description))
                description = $"{Context.User.Username}'s event";

            var session = await registrationHandler.CreateSession(registerButtonId, unregisterButtonId, commentButtonId, description, Context.User);
            var embed = RegistrationHandler.CreateLineupEmbed(session);
            var btnComponent = RegistrationHandler.CreateButtonComponent(session);

            var msg = await ReplyAsync(null, components: btnComponent, embed: embed);
            session.Message = msg;

            await Context.Message.DeleteAsync();
        }

        // Reposts new registration session
        // -bump 1 (with ID)
        // -bump (fetches most recent session by user)
        // Short alias:
        // -b 1
        // -b
        [Command("bump")]
        [Summary("Reposts a registration session.")]
        [Alias("b", "repost")]
        public async Task Repost([Summary("ID of the registration session to repost.")] int? sessionId = null)
        {
            logger.LogInformation($"Repost session ID {(!sessionId.HasValue ? "<not given>" : sessionId)} initiated");

            RegistrationSession session = null;

            if (sessionId == null)
            {
                session = registrationHandler.Sessions
                    .Where(s => s.CreatedBy.Username.ToLower() == Context.User.Username.ToLower())
                    .OrderByDescending(s => s.CreatedOn)
                    .FirstOrDefault();

                if (session != null)
                    logger.LogInformation($"Found recent session by {Context.User.Username}, ID {session.Id}");
            }
            else
                session = registrationHandler.Sessions.Where(s => s.Id == sessionId).FirstOrDefault();

            if (session == null)
            {
                var errMsg = $"Cannot find valid session {(sessionId.HasValue ? $" with ID {sessionId}" : "")}";
                logger.LogInformation(errMsg);
                await ReplyAsync(errMsg);
                return;
            }

            var embed = RegistrationHandler.CreateLineupEmbed(session);
            var btnComponent = RegistrationHandler.CreateButtonComponent(session);

            logger.LogInformation($"Deleting message ID {session.Message.Id}");

            await session.Message.DeleteAsync();

            logger.LogInformation($"Reposting session ID {session.Id}");

            var sessionMessage = await ReplyAsync(null, components: btnComponent, embed: embed);
            session.Message = sessionMessage;

            await Context.Message.DeleteAsync();
        }

        // Changes registration session description
        // -cd <description>
        // -changedescription <description>
        // -cd initial description
        // -changedescription new description
        [Command("changedescription")]
        [Summary("Changes description of user's most recent registration session.")]
        [Alias("cd", "description", "desc")]
        public async Task ChangeDescription([Summary("Session description.")] params string[] descParams)
        {
            var description = string.Join(" ", descParams);
            description = StringHelpers.RemoveDiscordMarkdown(description);

            logger.LogInformation($"Registration session description change to '{description}' initiated by {Context.User.Username}.");

            var session = registrationHandler.Sessions
                .Where(s => s.CreatedBy.Username.ToLower() == Context.User.Username.ToLower())
                .OrderByDescending(s => s.CreatedOn)
                .FirstOrDefault();

            if (session == null)
            {
                var errMsg = $"Cannot find session by {Context.User.Username}";
                logger.LogInformation(errMsg);
                await ReplyAsync(errMsg);
                return;
            }

            session.Description = description;

            logger.LogInformation($"Changed description of most recent session by {Context.User.Username}, ID {session.Id} to '{description}'. Reposting..");

            await Repost(session.Id);
        }
    }
}