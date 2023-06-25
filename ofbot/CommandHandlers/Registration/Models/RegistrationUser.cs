namespace OfBot.CommandHandlers.Registration.Models
{
    public class RegistrationUser : IEquatable<RegistrationUser>
    {
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Comment { get; set; }

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