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

        public string CreateLineupString(RegistrationSession session)
        {
            var lineupStr = string.Empty;

            if (session.InUsers.Count == 0)
                lineupStr = $"{session.Description}\nNo users in lineup.";
            else 
                lineupStr = $"{session.Description}\nLineup ({session.InUsers.Count}): {string.Join(", ", session.InUsers)}";

            if (session.OutUsers.Count > 0)
                lineupStr += $"\nOut: {string.Join(", ", session.OutUsers)}.";

            return lineupStr;
        }

        public async Task OnRegister(Guid registerButtonId, SocketMessageComponent component)
        {
            var userName = component.User.Username;
            var session = GetSession(registerButtonId);

            if (!session.InUsers.Contains(userName))
                session.InUsers.Add(userName);

            if (session.OutUsers.Contains(userName))
                session.OutUsers.Remove(userName);

            await component.UpdateAsync(mp => { mp.Content = CreateLineupString(session); });
        }

        public async Task OnUnregister(Guid unregisterButtonId, SocketMessageComponent component)
        {
            var userName = component.User.Username;
            var session = GetSession(unregisterButtonId);

            if (session.InUsers.Contains(userName))
                session.InUsers.Remove(userName);
            
            if (!session.OutUsers.Contains(userName))
                session.OutUsers.Add(userName);

            await component.UpdateAsync(mp => { mp.Content = CreateLineupString(session); });
        }

        public RegistrationSession CreateSession(Guid registerButtonId, Guid unregisterButtonId, string description, string initialUserName)
        {
            logger.LogInformation($"Creating new registration session with register button ID {registerButtonId}, description: '{description}'");

            var session = new RegistrationSession()
            {
                RegisterButtonId = registerButtonId,
                UnregisterButtonId = unregisterButtonId,
                Description = description
            };

            session.InUsers.Add(initialUserName);

            // Only keep max 10 session in memory
            if (Sessions.Count > 10)
                Sessions.RemoveAt(0);

            Sessions.Add(session);

            return session;
        }

        private RegistrationSession GetSession(Guid buttonId)
        {
            return Sessions.Where(rs => rs.RegisterButtonId == buttonId || rs.UnregisterButtonId == buttonId).FirstOrDefault();
        }
    }
}