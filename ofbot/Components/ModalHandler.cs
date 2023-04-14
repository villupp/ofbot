using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.Registration;

namespace OfBot.Components
{
    public class ModalHandler
    {
        private ILogger logger;
        private RegistrationHandler registrationHandler;

        public ModalHandler(ILogger<ButtonHandler> logger, RegistrationHandler registrationHandler)
        {
            this.logger = logger;
            this.registrationHandler = registrationHandler;
        }

        public async Task OnModalSubmitted(SocketModal modal)
        {
            logger.LogInformation($"Modal ID '{modal.Data.CustomId}' was submitted by {modal.User.Username}");

            if (!Guid.TryParse(modal.Data.CustomId, out var modalCustomId))
                return;

            if (registrationHandler.Sessions.Any(rs => rs.CommentButtonId == modalCustomId))
                await registrationHandler.OnCommentModalSubmitted(modalCustomId, modal);
            else
                await modal.DeferAsync();
        }
    }
}