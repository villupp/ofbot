using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace OfBot
{
    public class MessageHandler
    {
        public const char COMMAND_PREFIX = '-';
        public const char CUSTOM_COMMAND_PREFIX = '!';

        private ILogger logger;
        private DiscordSocketClient discordSocketClient;
        private CommandService commandService;
        private CustomCommandService customCommandService;
        private IServiceProvider serviceProvider;

        public MessageHandler(ILogger<MessageHandler> logger,
            DiscordSocketClient discordSocketClient,
            CommandService commandService,
            CustomCommandService customCommandService,
            IServiceProvider serviceProvider
            )
        {
            this.logger = logger;
            this.discordSocketClient = discordSocketClient;
            this.commandService = commandService;
            this.customCommandService = customCommandService;
            this.serviceProvider = serviceProvider;
        }

        public async Task Handle(SocketMessage socketMessage)
        {
            // Don't process the command if it was a system message
            var message = socketMessage as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(discordSocketClient, message);

            if (message.HasMentionPrefix(discordSocketClient.CurrentUser, ref argPos) ||
                message.Author.IsBot)
            {
                // Dont handle mentions or bot messages
                return;
            }
            else if (message.HasCharPrefix(COMMAND_PREFIX, ref argPos))
            {
                logger.LogInformation($"Got command: '{message.Content}'");

                // Execute the command (implementation in Modules folder)
                await commandService.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: serviceProvider);
            }
            else if (message.Content.ToLower().StartsWith($"{CUSTOM_COMMAND_PREFIX}set ") ||
                message.Content.ToLower() == $"{CUSTOM_COMMAND_PREFIX}set")
            {
                // Set custom command
                // !set !<command> <content>
                // !set <command> <content>
                logger.LogInformation($"Got custom command set: '{message.Content}'");
                await customCommandService.Set(message, context);
            }
            else if (message.Content.ToLower().StartsWith($"{CUSTOM_COMMAND_PREFIX}remove ") ||
                message.Content.ToLower().StartsWith($"{CUSTOM_COMMAND_PREFIX}delete ") ||
                message.Content.ToLower() == $"{CUSTOM_COMMAND_PREFIX}remove" ||
                message.Content.ToLower() == $"{CUSTOM_COMMAND_PREFIX}delete")
            {
                // Remove custom command
                // !remove !<command>
                // !remove <command>
                logger.LogInformation($"Got custom command remove: '{message.Content}'");
                await customCommandService.Remove(message, context);
            }
            else if (message.Content.ToLower().StartsWith($"{CUSTOM_COMMAND_PREFIX}search ") ||
                message.Content.ToLower().StartsWith($"{CUSTOM_COMMAND_PREFIX}s ") ||
                message.Content.ToLower() == $"{CUSTOM_COMMAND_PREFIX}search" ||
                message.Content.ToLower() == $"{CUSTOM_COMMAND_PREFIX}s")
            {
                // Remove custom command
                // !search <command name>
                // !s <command name>
                logger.LogInformation($"Got custom command search: '{message.Content}'");
                await customCommandService.Search(message, context);
            }
            else if (message.HasCharPrefix(CUSTOM_COMMAND_PREFIX, ref argPos))
            {
                // Execute custom command
                // !<command>

                // Only execute if contains other than the prefix character
                var commandContent = message?.Content.Replace(CUSTOM_COMMAND_PREFIX.ToString(), "");
                if (commandContent.Length > 0)
                {
                    logger.LogInformation($"Executing custom command: '{message.Content}'");
                    await customCommandService.Execute(message, context);
                }
            }
            else
            {
                return;
            }
        }
    }
}