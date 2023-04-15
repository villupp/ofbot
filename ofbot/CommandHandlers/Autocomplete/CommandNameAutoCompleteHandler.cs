using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OfBot.TableStorage.Repositories;

namespace OfBot.CommandHandlers.Autocomplete
{
    public class CommandNameHandler : AutocompleteHandler
    {
        private CommandRepository commandRepository;

        public CommandNameHandler(
            CommandRepository commandRepository
            )
        {
            this.commandRepository = commandRepository;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var interaction = (SocketAutocompleteInteraction)autocompleteInteraction;
            var commandNameOpt = interaction.Data.Options.Where(opt => opt.Name == "commandname").FirstOrDefault();

            if (commandNameOpt == null)
                return AutocompletionResult.FromSuccess();

            var allCommands = await commandRepository.Get();
            var foundCommands = allCommands.Where(c => c.Name.Contains(commandNameOpt.Value.ToString())).OrderBy(c => c.Name).ToList();
            var results = foundCommands.Select(cmd => new AutocompleteResult() { Name = cmd.Name, Value = cmd.Name }).ToArray();

            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}