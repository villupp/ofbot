using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers;

namespace OfBot.Components
{
    public class ButtonHandler
    {
        private ILogger logger;
        private RegistrationHandler registrationHandler;

        public ButtonHandler(ILogger<ButtonHandler> logger, RegistrationHandler registrationHandler)
        {
            this.logger = logger;
            this.registrationHandler = registrationHandler;
        }

        public async Task OnButtonExecuted(SocketMessageComponent component)
        {
            logger.LogInformation($"Button ID '{component.Data.CustomId}' was clicked by {component.User.Username}");

            if (!Guid.TryParse(component.Data.CustomId, out var buttonCustomId))
                return;

            if (registrationHandler.Sessions.Any(rs => rs.RegisterButtonId == buttonCustomId))
                await registrationHandler.OnRegister(buttonCustomId, component);
            else if (registrationHandler.Sessions.Any(rs => rs.UnregisterButtonId == buttonCustomId))
                await registrationHandler.OnUnregister(buttonCustomId, component);
            else
                await component.DeferAsync();
        }
    }
}