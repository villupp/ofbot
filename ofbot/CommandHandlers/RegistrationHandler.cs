using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.Models;

namespace OfBot.CommandHandlers
{
    public class RegistrationHandler
    {
        private ILogger logger;

        public List<RegistrationSession> Sessions { get; set; }

        public RegistrationHandler(ILogger<RegistrationHandler> logger)
        {
            this.logger = logger;

            Sessions = new List<RegistrationSession>();
        }

        private async Task AddUserToSession(Guid registerButtonId, string userName, ISocketMessageChannel channel)
        {
            var session = GetSession(registerButtonId);

            if (!session.Users.Contains(userName))
            {
                session.Users.Add(userName);

                await channel.SendMessageAsync($"{userName} is in!\n{CreateLineupString(session)}");
            }
            else
            {
                await channel.SendMessageAsync($"{userName} is already in!\n{CreateLineupString(session)}");
            }
        }

        private async Task RemoveUserFromSession(Guid unregisterButtonId, string userName, ISocketMessageChannel channel)
        {
            var session = GetSession(unregisterButtonId);

            if (session.Users.Contains(userName))
            {
                session.Users.Remove(userName);

                await channel.SendMessageAsync($"{userName} changed their mind :(\n{CreateLineupString(session)}");
            }
        }

        private string CreateLineupString(RegistrationSession session)
        {
            if (session.Users.Count == 0)
                return $"No users in lineup for '{session.Description}'";
            
            return $"Lineup ({session.Users.Count}) for '{session.Description}': {string.Join(", ", session.Users)}";
        }

        public async Task OnRegister(Guid registerButtonId, SocketMessageComponent component)
        {
            var userName = component.User.Username;
            await AddUserToSession(registerButtonId, userName, component.Channel);
        }

        public async Task OnUnregister(Guid unregisterButtonId, SocketMessageComponent component)
        {
            var userName = component.User.Username;
            await RemoveUserFromSession(unregisterButtonId, userName, component.Channel);
        }

        public void CreateSession(Guid registerButtonId, Guid unregisterButtonId, string description, string initialUserName)
        {
            logger.LogInformation($"Creating new registration session with register button ID {registerButtonId}, description: '{description}'");

            var session = new RegistrationSession()
            {
                RegisterButtonId = registerButtonId,
                UnregisterButtonId = unregisterButtonId,
                Description = description,
                Users = new List<string>() { initialUserName }
            };

            // Only keep max 10 session in memory
            if (Sessions.Count > 10)
                Sessions.RemoveAt(0);

            Sessions.Add(session);
        }

        private RegistrationSession GetSession(Guid buttonId)
        {
            return Sessions.Where(rs => rs.RegisterButtonId == buttonId || rs.UnregisterButtonId == buttonId).FirstOrDefault();
        }
    }
}