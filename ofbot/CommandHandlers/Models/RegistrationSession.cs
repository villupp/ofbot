namespace OfBot.CommandHandlers.Models
{
    public class RegistrationSession
    {
        public RegistrationSession()
        {
            InUsers = new List<string>();
            OutUsers = new List<string>();
        }

        public List<string> InUsers { get; set; }

        public List<string> OutUsers { get; set; }

        public Guid RegisterButtonId { get; set; }

        public Guid UnregisterButtonId { get; set; }

        public string Description { get; set; }
    }
}