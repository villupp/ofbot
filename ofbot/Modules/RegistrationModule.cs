using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.Registration;
using OfBot.CommandHandlers.Registration.Models;
using OfBot.Common;

namespace OfBot.Modules
{
    [Group("registration", "")]
    public class RegistrationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger logger;
        private readonly RegistrationHandler registrationHandler;

        public RegistrationModule(ILogger<RegistrationModule> logger, RegistrationHandler registrationHandler)
        {
            this.logger = logger;
            this.registrationHandler = registrationHandler;
        }

        // Starts new registration session
        [SlashCommand("create", "")]
        public async Task StartRegistration(string description = null)
        {
            description = StringHelpers.RemoveDiscordMarkdown(description);

            logger.LogInformation($"Registration initiated by {Context.User.Username}: '{description}'");

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

            await RespondAsync("Created.", ephemeral: true);
        }

        // Reposts new registration session
        [SlashCommand("bump", "")]
        public async Task Repost(int? sessionid = null)
        {
            logger.LogInformation($"Repost session ID {(!sessionid.HasValue ? "<not given>" : sessionid)} initiated");

            RegistrationSession session = null;

            if (sessionid == null)
            {
                session = registrationHandler.Sessions
                    .Where(s => s.CreatedBy.Username.ToLower() == Context.User.Username.ToLower())
                    .OrderByDescending(s => s.CreatedOn)
                    .FirstOrDefault();

                if (session != null)
                    logger.LogInformation($"Found recent session by {Context.User.Username}, ID {session.Id}");
            }
            else
                session = registrationHandler.Sessions.Where(s => s.Id == sessionid).FirstOrDefault();

            if (session == null)
            {
                var errMsg = $"Cannot find valid session {(sessionid.HasValue ? $" with ID {sessionid}" : "")}";
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

            await RespondAsync("Bumped.", ephemeral: true);
        }

        // Changes registration session description
        [SlashCommand("changedescription", "")]
        public async Task ChangeDescription(string description)
        {
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

            await RespondAsync("Description changed.", ephemeral: true);
        }
    }
}