using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.Registration.Models;
using OfBot.Common;
using System.Data;

namespace OfBot.CommandHandlers.Registration
{
    public class RegistrationHandler
    {
        private const string COMMENT_TEXT_ID = "comment-text";

        private ILogger logger;

        private int sessionId = 0;

        public List<RegistrationSession> Sessions { get; set; }

        public RegistrationHandler(ILogger<RegistrationHandler> logger)
        {
            this.logger = logger;

            Sessions = new List<RegistrationSession>();
        }

        public static Embed CreateLineupEmbed(RegistrationSession session)
        {
            var lineupStr = "";
            var outStr = "";

            if (session.InUsers.Count == 0)
                lineupStr += $"No users in lineup.";
            else
            {
                lineupStr += $"Lineup ({session.InUsers.Count}): ";

                for (var i = 0; i < session.InUsers.Count; i++)
                {
                    var user = session.InUsers[i];
                    lineupStr += $"**{user.DisplayName}**";

                    if (!string.IsNullOrEmpty(user.Comment))
                        lineupStr += $" ({user.Comment})";

                    if (i + 1 != session.InUsers.Count)
                        lineupStr += ", ";
                }
            }

            if (session.OutUsers.Count > 0)
            {
                var str = session.OutUsers.Select(
                    user => user.DisplayName);
                outStr = $"Out: {string.Join(", ", str)}\n";
            }

            var author = session.CreatedBy as SocketGuildUser;
            var avatarUrl = author.GetDisplayAvatarUrl();
            if (avatarUrl == null) 
                avatarUrl = author.GetDefaultAvatarUrl();
            var authorField = new EmbedAuthorBuilder()
                .WithName(author.DisplayName)
                .WithIconUrl(avatarUrl);
            var embedBuilder = new EmbedBuilder()
                .WithAuthor(authorField)
                .WithTitle($"{session.Description}")
                .WithDescription(lineupStr)
                .WithColor(Color.Blue)
                .WithFooter($"{outStr}ID: {session.Id}")
                .WithTimestamp(session.CreatedOn);

            return embedBuilder.Build();
        }

        public static MessageComponent CreateButtonComponent(RegistrationSession session)
        {
            var btnCompBuilder = new ComponentBuilder()
                .WithButton("I'm in!", session.RegisterButtonId.ToString(), ButtonStyle.Success)
                .WithButton("I'm in, but..", session.CommentButtonId.ToString(), ButtonStyle.Primary)
                .WithButton("I'm out..", session.UnregisterButtonId.ToString(), ButtonStyle.Secondary);

            return btnCompBuilder.Build();
        }

        public async Task OnRegister(Guid registerButtonId, SocketMessageComponent component)
        {
            var session = GetSession(registerButtonId);
            var user = GetInUser(session, component.User.Username);
            if (user == null)
            {
                user = BuildRegistrationUser(component.User);
                session.InUsers.Add(user);
            }
            else
            {
                user.Comment = null;
            }

            session.OutUsers.Remove(user).ToString();

            await component.UpdateAsync(mp =>
            {
                mp.Embed = CreateLineupEmbed(session);
                mp.Components = CreateButtonComponent(session);
            });
        }

        public async Task OnUnregister(Guid unregisterButtonId, SocketMessageComponent component)
        {
            var session = GetSession(unregisterButtonId);
            var user = GetInUser(session, component.User.Username);
            if (user == null) {
                user = BuildRegistrationUser(component.User);
            }

            session.InUsers.Remove(user);

            if (!session.OutUsers.Contains(user))
                session.OutUsers.Add(user);

            await component.UpdateAsync(mp =>
            {
                mp.Embed = CreateLineupEmbed(session);
                mp.Components = CreateButtonComponent(session);
            });
        }

        public async Task OnRegisterWithComment(Guid commentButtonId, SocketMessageComponent component)
        {
            string existingComment = null;
            var session = GetSession(commentButtonId);

            if (session == null)
            {
                await component.RespondAsync($"Could not find session..");
                return;
            }

            var inUser = GetInUser(session, component.User.Username);

            if (inUser != null && !string.IsNullOrEmpty(inUser.Comment))
                existingComment = inUser.Comment;

            var mb = new ModalBuilder()
                .WithTitle("I'm in, but..")
                .WithCustomId(commentButtonId.ToString())
                .AddTextInput("Comment", COMMENT_TEXT_ID, TextInputStyle.Short, "", 1, 25, true, existingComment);

            await component.RespondWithModalAsync(mb.Build());
        }

        public async Task OnCommentModalSubmitted(Guid modalId, SocketModal modal)
        {
            var commentButtonId = modalId;
            var session = GetSession(commentButtonId);

            var components = modal.Data.Components.ToList();
            var comment = components.First(x => x.CustomId == COMMENT_TEXT_ID).Value;
            comment = StringHelpers.RemoveDiscordMarkdown(comment);
            comment = comment.Replace(@"(", "");
            comment = comment.Replace(@")", "");

            var user = GetInUser(session, modal.User.Username);
            if (user == null)
            {
                user = BuildRegistrationUser(modal.User, comment);
                session.InUsers.Add(user);
            }
            else if (user.Comment != comment)
                user.Comment = comment;

            session.OutUsers.Remove(user);

            await session.Message.ModifyAsync(mp =>
            {
                mp.Embed = CreateLineupEmbed(session);
                mp.Components = CreateButtonComponent(session);
            });

            await modal.DeferAsync(true);
        }

        public async Task<RegistrationSession> CreateSession(Guid registerButtonId, Guid unregisterButtonId, Guid commentButtonId, string description, SocketUser createdBy)
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
                Description = description,
                Id = ++sessionId,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow
            };

            session.InUsers.Add(BuildRegistrationUser(createdBy));

            // Only keep max 10 session in memory
            if (Sessions.Count > 10)
            {
                await DeleteSessionMessage(Sessions[0]);
                Sessions.RemoveAt(0);
            }

            Sessions.Add(session);

            return session;
        }

        private async Task DeleteSessionMessage(RegistrationSession session)
        {
            logger.LogInformation($"DeleteMessage from session ID {session?.Id}");

            try
            {
                if (session?.Message != null)
                {
                    var message = session.Message;
                    logger.LogInformation($"Deleting message ID {message?.Id}");
                    await message.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"DeleteMessage failed for session message ID {session?.Message?.Id}: {ex.Message}");
            }
        }

        private RegistrationSession GetSession(Guid buttonId)
        {
            return Sessions.Where(rs =>
                rs.RegisterButtonId == buttonId
                || rs.UnregisterButtonId == buttonId
                || rs.CommentButtonId == buttonId)
                .FirstOrDefault();
        }


        private RegistrationUser GetInUser(RegistrationSession session, string username)
        {
            return session.InUsers.Where(u => u.Username.ToLower() == username.ToLower()).FirstOrDefault();
        }

        private RegistrationUser BuildRegistrationUser(SocketUser socketUser, string comment)
        {
            var user = socketUser as SocketGuildUser;
            return new RegistrationUser(user.Username, user.DisplayName, comment);
        }
        private RegistrationUser BuildRegistrationUser(SocketUser socketUser)
        {
            var user = socketUser as SocketGuildUser;
            return new RegistrationUser(user.Username, user.DisplayName);
        }
    }
}