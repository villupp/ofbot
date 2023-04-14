namespace OfBot.Common
{
    public class StringHelpers
    {
        public static string RemoveDiscordMarkdown(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;

            s = s.Replace(@"`", "");
            s = s.Replace(@"*", "");
            s = s.Replace(@"_", "");
            s = s.Replace(@"~", "");
            s = s.Replace(@"|", "");
            s = s.Trim();

            return s;
        }
    }
}