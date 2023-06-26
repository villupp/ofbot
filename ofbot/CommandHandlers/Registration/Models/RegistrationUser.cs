using Discord.WebSocket;

namespace OfBot.CommandHandlers.Registration.Models
{
    public class RegistrationUser : IEquatable<RegistrationUser>
    {
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string DisplayName { get; set; }
        public string Name => Nickname ?? DisplayName ?? Username;
        public string Comment { get; set; }

        public RegistrationUser(SocketGuildUser user, string comment = null)
        {
            Username = user.Username;
            Nickname = user.Nickname;
            DisplayName = user.DisplayName;
            Comment = comment;
        }

        public bool Equals(RegistrationUser other)
        {
            return Username == other.Username;
        }
        
        public override bool Equals(Object obj) {
            return obj is RegistrationUser user && Equals(user);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Username);
        }
    }
}