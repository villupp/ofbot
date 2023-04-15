using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OfBot.Modules;
using OfBot.TableStorage.Models;
using System.Linq.Expressions;

namespace OfBot.TableStorage.Repositories
{
    public class CommandRepository
    {
        private const string ALL_COMMANDS_CACHE_KEY = "ALL_CMDS_CACHE_KEY";

        private readonly ILogger logger;
        private TableStorageService<Command> commandTableService;
        private readonly IMemoryCache memoryCache;

        private static readonly SemaphoreSlim lockSemaphore = new(1);

        public CommandRepository(
            ILogger<CustomCommandModule> logger,
            TableStorageService<Command> commandTableService,
            IMemoryCache memoryCache
            )
        {
            this.logger = logger;
            this.commandTableService = commandTableService;
            this.memoryCache = memoryCache;
        }

        public async Task<List<Command>> Get(Expression<Func<Command, bool>> query)
        {
            var commands = await commandTableService.Get(query);

            return commands;
        }

        public async Task<List<Command>> Get(bool useCache = true)
        {
            if (!useCache) return await commandTableService.Get();

            if (memoryCache.TryGetValue(ALL_COMMANDS_CACHE_KEY, out List<Command> cachedCommands))
            {
                logger.LogDebug($"Got {cachedCommands.Count} commands from cache");
                return cachedCommands;
            }
            else
            {
                logger.LogDebug($"Commands not in cache. Updating.");
                var allCommands = await commandTableService.Get();
                await UpdateCache(allCommands);
                return allCommands;
            }
        }

        private async Task UpdateCache(List<Command> allCommands)
        {
            logger.LogDebug($"Update command cache with {allCommands.Count} items");

            await lockSemaphore.WaitAsync();

            try
            {
                memoryCache.Set(ALL_COMMANDS_CACHE_KEY, allCommands, DateTimeOffset.Now + TimeSpan.FromSeconds(60));
            }
            catch (Exception ex)
            {
                logger.LogWarning($"CommandRepository.UpdateCache failed: {ex}");
            }
            finally
            {
                lockSemaphore.Release();
            }
        }
    }
}