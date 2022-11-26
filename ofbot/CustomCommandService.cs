using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;
using System.Text.RegularExpressions;

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

        public async Task Execute(SocketUserMessage message, SocketCommandContext context)
        {
            var commandName = ParseCustomCommandName(message.Content);

            logger.LogInformation($"Executing command '{commandName}'");

            var existingCommands = await commandTableService.Get(command => command.Name == commandName);

            if (existingCommands.Count > 0)
            {
                var existingCommand = existingCommands[0];
                var content = existingCommand.Content;
                logger.LogInformation($"Responding to custom command '{commandName}' (RowKey {existingCommand.RowKey}): '{content}'");
                await context.Channel.SendMessageAsync(content);
            }
            else
            {
                var msg = $"Could not find command '{commandName}'.";
                logger.LogInformation(msg);
                await context.Channel.SendMessageAsync(msg);
            }
        }

        public async Task<bool> Remove(SocketUserMessage message, SocketCommandContext context)
        {
            var isSuccess = false;

            var messageParts = message.Content.Split(' ');

            if (messageParts.Length < 2)
            {
                await context.Channel.SendMessageAsync($"Invalid remove. Use '{MessageHandler.CUSTOM_COMMAND_PREFIX}remove <command name>'");
                return false;
            }

            var commandName = ParseCustomCommandName(messageParts[1]);

            logger.LogInformation($"Removing command '{commandName}'");

            var existingCommands = await commandTableService.Get(command => command.Name == commandName);

            if (existingCommands.Count > 0)
            {
                var existingCommand = existingCommands[0];

                logger.LogInformation($"Existing command '{commandName}' found, RowKey {existingCommand.RowKey}");

                isSuccess = await commandTableService.Delete(existingCommand);

                if (isSuccess)
                {
                    var msg = $"Custom command '{commandName}' removed successfully.";
                    logger.LogInformation($"{msg} RowKey {existingCommand.RowKey}.");
                    await context.Channel.SendMessageAsync(msg);
                }
                else
                {
                    var msg = $"Custom command '{commandName}' removal failed. See logs for details.";
                    logger.LogInformation($"{msg} RowKey {existingCommand.RowKey}.");
                    await context.Channel.SendMessageAsync(msg);
                }
            }
            else
            {
                var msg = $"Could not find command '{commandName}'.";
                logger.LogInformation(msg);
                await context.Channel.SendMessageAsync(msg);
            }

            return isSuccess;
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
                await context.Channel.SendMessageAsync($"Invalid set. Use '{MessageHandler.CUSTOM_COMMAND_PREFIX}set <command name> <command content>'");
                return false;
            }

            var commandName = ParseCustomCommandName(messageParts[1]);
            
            if (!Regex.IsMatch(commandName, "^[a-zöäå0-9]*$", RegexOptions.IgnoreCase))
            {
                await context.Channel.SendMessageAsync($"Invalid command name. Command name may only contain letters and numbers.");
                return false;
            }
            
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

        public async Task Search(SocketUserMessage message, SocketCommandContext context)
        {
            // Setting command syntax:
            // !search <search string>
            // !search sami  --> list of commands containing "sami"
            var messageParts = message.Content.Split(' ');

            if (messageParts.Length < 2)
            {
                await context.Channel.SendMessageAsync($"Invalid search. Use '{MessageHandler.CUSTOM_COMMAND_PREFIX}search <search input>'");
                return;
            }

            var searchStr = string.Join(' ', messageParts.Skip(1).ToArray()).ToLower().TrimStart(MessageHandler.CUSTOM_COMMAND_PREFIX).Trim();

            if (searchStr.Length < 2)
            {
                await context.Channel.SendMessageAsync($"Be more specific (min. 2 characters).");
                return;
            }

            logger.LogInformation($"Searching for custom commands with search string '{searchStr}'");

            var allCommands = await commandTableService.Get();
            var foundCommands = allCommands.Where(c => c.Name.Contains(searchStr)).ToList();

            logger.LogInformation($"Found {foundCommands.Count} custom commands with search string '{searchStr}'");

            var resStr = string.Empty;

            if (foundCommands.Count == 0)
            {
                await context.Channel.SendMessageAsync($"Could not find any custom commands.");
                return;
            }

            if (foundCommands.Count > 50)
                resStr = $"Found {foundCommands.Count} commands. Showing 50 first.\n";

            var commandNames = foundCommands.Select(c => c.Name).ToList();
            commandNames.Sort();

            await context.Channel.SendMessageAsync($"Available custom commands:\n{string.Join(", ", commandNames)}");
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