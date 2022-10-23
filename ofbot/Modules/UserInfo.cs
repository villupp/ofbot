using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    public class UserInfo : ModuleBase<SocketCommandContext>
    {
        private ILogger logger;

        public UserInfo(ILogger<UserInfo> logger)
        {
            this.logger = logger;
        }

        // !userinfo --> foxbot#0282
        // !userinfo @Khionu --> Khionu#8708
        // !userinfo Khionu#8708 --> Khionu#8708
        // -userinfo Khionu --> Khionu#8708
        // -userinfo 96642168176807936 --> Khionu#8708
        // -whois 96642168176807936 --> Khionu#8708
        [Command("userinfo")]
        [Summary
        ("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task PostUserInfo(
            [Summary("The (optional) user to get info from.")]
        SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            logger.LogInformation($"Executing userinfo for user {userInfo.Username} ID {userInfo.Id}");

            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}" +
                $" ID {userInfo.Id} is {userInfo.Status}. Joined on {userInfo.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
    }
}