using Discord.Interactions;
using Microsoft.Extensions.Logging;
using OfBot.CommandHandlers.Autocomplete;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;
using System.Text.RegularExpressions;

namespace OfBot.Modules
{
    [Group("customcommand", "")]
    public class CustomCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger logger;
        private TableStorageService<Command> commandTableService;

        public CustomCommandModule(
            ILogger<CustomCommandModule> logger,
            TableStorageService<Command> commandTableService
            )
        {
            this.logger = logger;
            this.commandTableService = commandTableService;
        }

        [SlashCommand("execute", "")]
        public async Task Execute(
            [Autocomplete(typeof(CommandNameHandler))] string commandname, bool isprivate = false)
        {
            var commandName = ParseCustomCommandName(commandname);

            logger.LogInformation($"Executing command '{commandName}'");

            var existingCommands = await commandTableService.Get(command => command.Name == commandName);

            if (existingCommands.Count > 0)
            {
                var existingCommand = existingCommands[0];
                var content = existingCommand.Content;
                logger.LogInformation($"Responding to custom command '{commandName}' (RowKey {existingCommand.RowKey}): '{content}'");

                if (isprivate)
                    await RespondAsync(content, ephemeral: true);
                else
                    await RespondAsync(content);
            }
            else
            {
                var msg = $"Could not find command '{commandName}'.";
                logger.LogInformation(msg);
                await RespondAsync(msg, ephemeral: true);
            }
        }

        [SlashCommand("remove", "")]
        public async Task Remove([Autocomplete(typeof(CommandNameHandler))] string commandname)
        {
            var commandName = ParseCustomCommandName(commandname);

            logger.LogInformation($"Removing command '{commandName}'");

            var existingCommands = await commandTableService.Get(command => command.Name == commandName);

            if (existingCommands.Count > 0)
            {
                var existingCommand = existingCommands[0];

                logger.LogInformation($"Existing command '{commandName}' found, RowKey {existingCommand.RowKey}");

                var isSuccess = await commandTableService.Delete(existingCommand);

                if (isSuccess)
                {
                    var msg = $"Custom command '{commandName}' removed successfully.";
                    logger.LogInformation($"{msg} RowKey {existingCommand.RowKey}.");
                    await RespondAsync(msg);
                }
                else
                {
                    var msg = $"Custom command '{commandName}' removal failed. See logs for details.";
                    logger.LogInformation($"{msg} RowKey {existingCommand.RowKey}.");
                    await RespondAsync(msg);
                }
            }
            else
            {
                var msg = $"Could not find command '{commandName}'.";
                logger.LogInformation(msg);
                await RespondAsync(msg, ephemeral: true);
            }
        }

        [SlashCommand("set", "")]
        public async Task Set(string commandname, string commandcontent)
        {
            var commandName = ParseCustomCommandName(commandname);

            if (!Regex.IsMatch(commandName, "^[a-zöäå0-9]*$", RegexOptions.IgnoreCase))
                await RespondAsync($"Invalid command name. Command name may only contain letters and numbers.", ephemeral: true);

            logger.LogInformation($"Setting command '{commandName}' to '{commandcontent}'");

            var isSuccess = false;
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
                    Content = commandcontent,
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
                    Content = commandcontent
                });
            }

            var msg = "";

            if (isSuccess)
                msg = $"Command '{commandName}' set successfully.";
            else
                msg = $"Could not set command '{commandName}' See logs for details.";

            logger.LogInformation(msg);
            await RespondAsync(msg);
        }

        private static string ParseCustomCommandName(string rawCommand)
        {
            var commandName = rawCommand.Trim().ToLower();
            return commandName;
        }
    }
}