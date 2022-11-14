using OfBot.TableStorage.Models;
using OfBot.TableStorage;
using OfBot.Api.OpenDota;
using OfBot.Api.Dota;

namespace OfBot.DotaTracker
{
    public class TrackedDotaPlayers
    {
        public List<TrackingState<TrackedDotaPlayer>> players { get; set; }
        private TableStorageService<TrackedDotaPlayer> tableService;
        private DotaApiClient dotaApi;
        private OpenDotaApiClient openDotaApi;

        public TrackedDotaPlayers(
            TableStorageService<TrackedDotaPlayer> commandTableService,
            DotaApiClient dotaApi,
            OpenDotaApiClient openDotaApi
        )
        {
            this.players = new List<TrackingState<TrackedDotaPlayer>>();
            this.tableService = commandTableService;
            this.dotaApi = dotaApi;
            this.openDotaApi = openDotaApi;
        }
        public async Task<TrackedDotaPlayer> Add(string accountId, string addedBy)
        {
            var player = await this.Exists(accountId);
            if (player != null)
            {
                throw new Exception($"Dota player {accountId} is already tracked");
            }
            var validationState = await Validate(accountId);
            if (validationState.steamName == null)
            {
                throw new Exception(validationState.errorMessage);
            }

            var trackedPlayer = new TrackedDotaPlayer()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = "",
                AccountId = accountId,
                AddedBy = addedBy,
                SteamName = validationState.steamName
            };
            await tableService.Add(trackedPlayer);
            this.players.Add(new TrackingState<TrackedDotaPlayer>()
            {
                latestMatchId = null,
                player = trackedPlayer
            });
            return trackedPlayer; 
        }
        public async Task<TrackedDotaPlayer> Remove(string accountId)
        {
            var player = await this.Exists(accountId);
            if (player == null)
            {
                throw new Exception($"Could not find tracked dota player with id {accountId}");
            }
            await this.tableService.Delete(player);
            this.players.Remove(this.players.FirstOrDefault(state => state.player.AccountId == accountId));
            return player;
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

        private async Task<(string steamName, string errorMessage)> Validate(string accountId)
        {
            var response = await dotaApi.GetRecentDotaMatches(accountId, 1);
            if (response == null)
            {
                return (null, $"Couldn't validate account id {accountId}, Steam API might be down");
            }
            else if (response.result.status != 1)
            {
                return (null, $"Account id {accountId} match history is not exposed");
            }
            // Get player information from OpenDota Api
            var openDotaPlayer = await openDotaApi.GetPlayer(accountId);
            if (openDotaPlayer == null || openDotaPlayer.profile == null || openDotaPlayer.profile.personaname == null)
            {
                return (null, $"Couldn't validate account id {accountId}, OpenDota API might be down");
            }
            return (openDotaPlayer.profile.personaname, null);
        }
    }
}