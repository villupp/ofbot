using OfBot.TableStorage.Models;
using OfBot.TableStorage;
using OfBot.Components.Api;

namespace OfBot.Components.DotaTracker
{
    public class TrackedDotaPlayers
    {
        public List<TrackingState<TrackedDotaPlayer>> players { get; set; }
        private TableStorageService<TrackedDotaPlayer> tableService;
        private DotaApi dotaApi;

        public TrackedDotaPlayers(
            TableStorageService<TrackedDotaPlayer> commandTableService,
            DotaApi dotaApi
        )
        {
            this.players = new List<TrackingState<TrackedDotaPlayer>>();
            this.tableService = commandTableService;
            this.dotaApi = dotaApi;
        }
        public async Task Add(string accountId, string addedBy)
        {
            var player = await this.Exists(accountId);
            if (player != null)
            {
                throw new Exception($"Dota player {accountId} is already tracked");
            }
            var validationState = await Validate(accountId);
            if (validationState.isValid == false) {
                throw new Exception($"Could not track player: {validationState.errorMessage}");
            } 
            var trackedPlayer = new TrackedDotaPlayer()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = "",
                AccountId = accountId,
                AddedBy = addedBy
            };
            await tableService.Add(trackedPlayer);
            this.players.Add(new TrackingState<TrackedDotaPlayer>()
            {
                latestMatchId = null,
                player = trackedPlayer
            });
        }
        public async Task Remove(string accountId)
        {
            var player = await this.Exists(accountId);
            if (player == null)
            {
                throw new Exception($"Could not find tracked dota player with id {accountId}");
            }
            await this.tableService.Delete(player);
            this.players.Remove(this.players.FirstOrDefault(state => state.player.AccountId == accountId));
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
            var players = await tableService.Get(trackedDotaPlayer => true);
            this.players.Clear();
            foreach (var player in players)
            {
                this.players.Add(new TrackingState<TrackedDotaPlayer>
                {
                    player = player,
                    latestMatchId = null
                });
            }
            this.players.Sort(
                (x, y) => Nullable.Compare(x.player.Timestamp, y.player.Timestamp));
        }

        private async Task<(bool isValid, string errorMessage)> Validate(string accountId) {
            var response = await dotaApi.GetRecentDotaMatches(accountId, 1);
            if (response == null) {
                return (false, $"Couldn't validate account id {accountId}, Steam API might be down");
            } else if (response.result.status != 1) {
                return (false, $"Account id {accountId} match history is not exposed");
            }
            return (true, null);
        }
    }
}