using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.Models;

namespace OfBot.CommandHandlers
{
    public class RegistrationHandler
    {
        private const string COMMENT_TEXT_ID = "comment-text";
        private ILogger logger;

        public List<RegistrationSession> Sessions { get; set; }

        public RegistrationHandler(ILogger<RegistrationHandler> logger)
        {
            this.logger = logger;

            Sessions = new List<RegistrationSession>();
        }

        public string CreateLineupString(RegistrationSession session)
        {
            var lineupStr = $"{session.Description}\n";

            if (session.InUsers.Count == 0)
                lineupStr += $"No users in lineup.";
            else
            {
                lineupStr += $"Lineup ({session.InUsers.Count}): ";

                for (var i = 0; i < session.InUsers.Count; i++)
                {
                    var user = session.InUsers[i];
                    lineupStr += $"{user.Username}";

                    if (!string.IsNullOrEmpty(user.Comment))
                        lineupStr += $" ({user.Comment})";

                    if (i + 1 != session.InUsers.Count)
                        lineupStr += ", ";
                }
            }

            if (session.OutUsers.Count > 0)
                lineupStr += $"\nOut: {string.Join(", ", session.OutUsers)}.";

            return lineupStr;
        }

        public async Task OnRegister(Guid registerButtonId, SocketMessageComponent component)
        {
            var userName = component.User.Username;
            var session = GetSession(registerButtonId);

            var existingInUser = session.InUsers.Where(u => u.Username.ToLower() == userName.ToLower()).FirstOrDefault();

            if (existingInUser == null)
                session.InUsers.Add(new RegistrationUser()
                {
                    Username = userName,
                });
            else
                existingInUser.Comment = null;

            if (session.OutUsers.Contains(userName))
                session.OutUsers.Remove(userName);

            await component.UpdateAsync(mp => { mp.Content = CreateLineupString(session); });
        }

        public async Task OnUnregister(Guid unregisterButtonId, SocketMessageComponent component)
        {
            var userName = component.User.Username;
            var session = GetSession(unregisterButtonId);

            var inUser = session.InUsers.Where(u => u.Username.ToLower() == userName.ToLower()).FirstOrDefault();

            if (inUser != null)
                session.InUsers.Remove(inUser);

            if (!session.OutUsers.Contains(userName))
                session.OutUsers.Add(userName);

            await component.UpdateAsync(mp => { mp.Content = CreateLineupString(session); });
        }

        public async Task OnRegisterWithComment(Guid commentButtonId, SocketMessageComponent component)
        {
            var mb = new ModalBuilder()
                .WithTitle("I'm in, but..")
                .WithCustomId(commentButtonId.ToString())
                .AddTextInput("Comment", COMMENT_TEXT_ID, TextInputStyle.Short, "", 1, 25, true);

            await component.RespondWithModalAsync(mb.Build());
        }

        public async Task OnCommentModalSubmitted(Guid modalId, SocketModal modal)
        {
            var userName = modal.User.Username;
            var commentButtonId = modalId;
            var session = GetSession(commentButtonId);

            var components = modal.Data.Components.ToList();
            var comment = components.First(x => x.CustomId == COMMENT_TEXT_ID).Value;

            var existingInUser = session.InUsers.Where(u => u.Username.ToLower() == userName.ToLower()).FirstOrDefault();

            if (existingInUser == null)
                session.InUsers.Add(new RegistrationUser()
                {
                    Username = userName,
                    Comment = comment
                });
            else if (existingInUser.Comment != comment)
                existingInUser.Comment = comment;

            if (session.OutUsers.Contains(userName))
                session.OutUsers.Remove(userName);

            await session.Message.ModifyAsync(mp => { mp.Content = CreateLineupString(session); });

            await modal.DeferAsync(true);
        }

        public RegistrationSession CreateSession(Guid registerButtonId, Guid unregisterButtonId, Guid commentButtonId, string description, string initialUserName)
        {
            logger.LogInformation($"Creating new registration session with" +
                $" register button ID {registerButtonId}" +
                $", unregister button ID {unregisterButtonId}" +
                $", comment button ID {commentButtonId}" +
                $", description: '{description}'");

            var session = new RegistrationSession()
            {
                RegisterButtonId = registerButtonId,
                UnregisterButtonId = unregisterButtonId,
                CommentButtonId = commentButtonId,
                Description = description
            };

            session.InUsers.Add(new RegistrationUser() { Username = initialUserName });

            // Only keep max 10 session in memory
            if (Sessions.Count > 10)
                Sessions.RemoveAt(0);

            Sessions.Add(session);

            return session;
        }

        private RegistrationSession GetSession(Guid buttonId)
        {
            return Sessions.Where(rs =>
                rs.RegisterButtonId == buttonId
                || rs.UnregisterButtonId == buttonId
                || rs.CommentButtonId == buttonId)
                .FirstOrDefault();
        }
    }
}