using OfBot.TableStorage.Models;
using OfBot.TableStorage;
using Microsoft.Extensions.Logging;

namespace OfBot
{
    public class TrackedDotaPlayers
    {

        private List<TrackedDotaPlayer> trackedPlayers { get; set; }
        private TableStorageService<TrackedDotaPlayer> tableService;

        public TrackedDotaPlayers(
            TableStorageService<TrackedDotaPlayer> commandTableService
        )
        {
            this.trackedPlayers = new List<TrackedDotaPlayer>();
            this.tableService = commandTableService;
        }
        public async Task Add(string accountId, string addedBy)
        {
            var player = await this.Exists(accountId);
            if (player != null)
            {
                throw new Exception($"Dota player {accountId} is already tracked");
            }
            await tableService.Add(new TrackedDotaPlayer()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = "",
                AccountId = accountId,
                AddedBy = addedBy
            });
            await this.Refresh();
        }
        public async Task Remove(string accountId)
        {
            var player = await this.Exists(accountId);
            if (player == null)
            {
                throw new Exception($"Could not find tracked dota player with id {accountId}");
            }
            await this.tableService.Delete(player);
            await this.Refresh();
        }
        private async Task<TrackedDotaPlayer> Exists(string accountId)
        {
            var existingPlayers = await tableService.Get(
                trackedDotaPlayer => trackedDotaPlayer.AccountId == accountId);
            if (existingPlayers.Count > 0)
            {
                return existingPlayers[0];
            }
            return null;
        }
        public async Task Refresh()
        {
            this.trackedPlayers = await tableService.Get(trackedDotaPlayer => true);
            this.trackedPlayers.Sort(
                (x,y) => Nullable.Compare(x.Timestamp, y.Timestamp));
        }
        public List<TrackedDotaPlayer> Get()
        {
            return this.trackedPlayers;
        }
    }
}