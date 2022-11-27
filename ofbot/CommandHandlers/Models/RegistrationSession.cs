using Discord;

namespace OfBot.CommandHandlers.Models
{
    public class RegistrationSession
    {
        public RegistrationSession()
        {
            InUsers = new List<RegistrationUser>();
            OutUsers = new List<string>();
        }

        public List<RegistrationUser> InUsers { get; set; }

        public List<string> OutUsers { get; set; }

        public Guid RegisterButtonId { get; set; }

        public Guid UnregisterButtonId { get; set; }

        public Guid CommentButtonId { get; set; }

        public string Description { get; set; }

        public IUserMessage Message { get; set; }
    }
}