using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace OfBot
{
    public class AnnouncementService
    {
        private readonly ILogger logger;
        private readonly DiscordSocketClient discordSocketClient;

        public AnnouncementService(
            ILogger<AnnouncementService> logger,
            DiscordSocketClient discordSocketClient
            )
        {
            this.logger = logger;
            this.discordSocketClient = discordSocketClient;
        }

        public async Task Announce(string guildName, string channelName, string messageContent)
        {
            logger.LogInformation($"Sending message to channel '{guildName}/{channelName}' with content '{messageContent}'");
            await GetChannel(guildName, channelName).SendMessageAsync(messageContent);
        }

        public async Task Announce(string guildName, string channelName, Embed embed) {
            logger.LogInformation($"Sending message to channel '{guildName}/{channelName}' with custom embed content'");
            await GetChannel(guildName, channelName).SendMessageAsync(embed: embed);
        }

        private IMessageChannel GetChannel(string guildName, string channelName)
        {
            var guild = discordSocketClient.Guilds.FirstOrDefault(
                g => g.Name == guildName
            );
            var channel = guild.Channels.FirstOrDefault(
                c => c.Name == channelName
            ) as IMessageChannel;
            return channel;
        }
    }
}