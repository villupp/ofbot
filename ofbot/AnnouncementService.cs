using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace OfBot
{
    public class AnnouncementService
    {
        private readonly ILogger logger;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly IServiceProvider serviceProvider;

        public AnnouncementService(
            ILogger<AnnouncementService> logger,
            DiscordSocketClient discordSocketClient,
            IServiceProvider serviceProvider
            )
        {
            this.logger = logger;
            this.discordSocketClient = discordSocketClient;
            this.serviceProvider = serviceProvider;
        }

        public async Task Announce(String guildName, String channelName, String messageContent)
        {
            logger.LogInformation($"Sending message to channel '{guildName}/{channelName}' with content '{messageContent}'");
            var guild = discordSocketClient.Guilds.FirstOrDefault(
                g => g.Name == guildName
                );
            var channel = guild.Channels.FirstOrDefault(
                c => c.Name == channelName
                )  as IMessageChannel;
            await channel.SendMessageAsync(messageContent);
        }
    }
}