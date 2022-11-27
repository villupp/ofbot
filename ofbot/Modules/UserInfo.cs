using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace OfBot.Modules
{
    public class UserInfo : InteractionModuleBase
    {
        private ILogger logger;

        public UserInfo(ILogger<UserInfo> logger)
        {
            this.logger = logger;
        }

        // /userinfo --> foxbot#0282
        // /userinfo @Khionu --> Khionu#8708
        // /userinfo Khionu#8708 --> Khionu#8708
        // /userinfo Khionu --> Khionu#8708
        // /userinfo 96642168176807936 --> Khionu#8708
        [SlashCommand("userinfo", "Returns info about the current user, or the user parameter, if one passed.")]
        public async Task PostUserInfo(
            [Summary("The (optional) user to get info from.")]
            SocketUser user = null)
        {
            string userName = "";
            ulong userId = 0;
            string discriminator = "";
            DateTimeOffset createdAt = DateTimeOffset.MinValue;

            if (user != null) {
                userName = user.Username;
                userId = user.Id;
                discriminator = user.Discriminator;
                createdAt = user.CreatedAt;
            }
            else
            {
                userName = Context.Client.CurrentUser.Username;
                userId = Context.Client.CurrentUser.Id;
                discriminator = Context.Client.CurrentUser.Discriminator;
                createdAt = Context.Client.CurrentUser.CreatedAt;
            }

            logger.LogInformation($"Executing userinfo for user {userName} ID {userId}");
            
            await ReplyAsync($"{userName}#{discriminator}" +
                $" ID {userId}. Joined on {createdAt:yyyy-MM-dd HH:mm:ss}");
        }
    }
}