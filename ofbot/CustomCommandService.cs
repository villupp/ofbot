using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;

namespace OfBot
{
    public class CustomCommandService
    {
        private ILogger logger;
        private TableStorageService<Command> commandTableService;

        public CustomCommandService(ILogger<CustomCommandService> logger,
            TableStorageService<Command> commandTableService
            )
        {
            this.logger = logger;
            this.commandTableService = commandTableService;
        }

        public async Task Execute(SocketUserMessage message, SocketCommandContext context, int argPos)
        {
            var commandName = ParseCustomCommandName(message.Content);

            logger.LogInformation($"Executing command '{commandName}'");

            var existingCommands = await commandTableService.Get(command => command.Name == commandName);

            if (existingCommands.Count > 0)
            {
                var existingCommand = existingCommands[0];
                var content = existingCommand.Content;
                logger.LogInformation($"Responding to custom command '{commandName}': '{content}'");
                await context.Channel.SendMessageAsync(content);
            }
            else
            {
                var msg = $"Could not find command '{commandName}'.";
                logger.LogInformation(msg);
                await context.Channel.SendMessageAsync(msg);
            }
        }

        public async Task<bool> Set(SocketUserMessage message, SocketCommandContext context)
        {
            // Setting command syntax:
            // !set <command name with or without prefix, e.g. !> <command content>
            // prefix ! might be something else (set in constant)
            // For example:
            // !set !commandname command content can be anything
            // OR:
            // !set commandname command content can be anything
            var messageParts = message.Content.Split(' ');
            var isSuccess = false;

            if (messageParts.Length < 3)
            {
                logger.LogInformation($"Invalid custom command set: '{message.Content}'");
                return false;
            }

            var commandName = ParseCustomCommandName(messageParts[1]);
            var commandContent = string.Join(' ', messageParts.Skip(2).ToArray());

            logger.LogInformation($"Setting command '{commandName}' to '{commandContent}'");

            var existingCommands = await commandTableService.Get(command => command.Name == commandName);

            if (existingCommands.Count > 0)
            {
                // Update existing command
                var existingCommand = existingCommands[0];

                logger.LogInformation($"Updating existing command PartitionKey: {existingCommand.PartitionKey}, RowKey: {existingCommand.RowKey}");

                isSuccess = await commandTableService.Update(new Command
                {
                    RowKey = existingCommand.RowKey,
                    PartitionKey = existingCommand.PartitionKey,
                    Name = commandName,
                    Content = commandContent,
                    ETag = existingCommand.ETag
                });
            }
            else
            {
                logger.LogInformation($"Creating new command '{commandName}'");

                // Create new command
                isSuccess = await commandTableService.Add(new Command()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = "",
                    Name = commandName,
                    Content = commandContent
                });
            }

            var msg = "";

            if (isSuccess)
                msg = $"Command '{commandName}' set successfully.";
            else
                msg = $"Could not set command '{commandName}' See logs for details.";

            logger.LogInformation(msg);
            await context.Channel.SendMessageAsync(msg);

            return isSuccess;
        }

        private string ParseCustomCommandName(string rawCommand)
        {
            var commandName = rawCommand.Trim().ToLower();

            if (commandName.StartsWith(MessageHandler.CUSTOM_COMMAND_PREFIX))
                commandName = commandName[1..];

            return commandName;
        }
    }
}