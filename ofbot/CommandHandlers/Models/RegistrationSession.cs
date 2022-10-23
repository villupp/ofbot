namespace OfBot.CommandHandlers.Models
{
    public class RegistrationSession
    {
        public List<string> Users { get; set; }

        public Guid RegisterButtonId { get; set; }

        public Guid UnregisterButtonId { get; set; }

        public string Description { get; set; }
    }
}