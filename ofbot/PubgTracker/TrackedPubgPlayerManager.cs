using OfBot.DotaTracker.Models;
using OfBot.Api.Pubg;
using OfBot.Api.Models;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;

namespace OfBot.Api
{
    public class TrackedPubgPlayerManager
    {
        public List<TrackingState> trackedPlayers { get; set; }
        private TableStorageService<TrackedPubgPlayer> tableService;
        private PubgApiClient pubgClient;

        public TrackedPubgPlayerManager(
            TableStorageService<TrackedPubgPlayer> tableService,
            PubgApiClient pubgClient
            )
        {
            trackedPlayers = new List<TrackingState>();
            this.tableService = tableService;
            this.pubgClient = pubgClient;
        }

        public async Task<TrackedPubgPlayer> Add(string playerName, string addedBy)
        {
            var trackedPlayer = await GetTrackedPlayer(playerName);
            if (trackedPlayer != null)
                throw new Exception($"PUBG player '{playerName}' is already tracked");

            var pubgPlayer = await pubgClient.GetPlayer(playerName);
            if (pubgPlayer == null)
                throw new Exception($"Could not validate player '{playerName}'");

            trackedPlayer = new TrackedPubgPlayer()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = "",
                Name = pubgPlayer.Attributes.Name,
                AddedBy = addedBy,
                Id = pubgPlayer.Id
            };

            await tableService.Add(trackedPlayer);
            this.trackedPlayers.Add(new TrackingState()
            {
                LatestMatchId = null,
                Player = trackedPlayer
            });

            return trackedPlayer;
        }

        public async Task<TrackedPubgPlayer> Remove(string playerName)
        {
            var player = await GetTrackedPlayer(playerName);

            if (player == null)
                throw new Exception($"Could not find tracked PUBG player with name '{playerName}'");

            await tableService.Delete(player);
            trackedPlayers.Remove(trackedPlayers.FirstOrDefault(state => state.Player.Name == playerName));
            return player;
        }

        private async Task<TrackedPubgPlayer> GetTrackedPlayer(string playerName)
        {
            var existingPlayers = await tableService.Get(
                trackedPubgPlayer => trackedPubgPlayer.Name == playerName);

            if (existingPlayers.Count > 0)
                return existingPlayers[0];

            return null;
        }

        public async Task Refresh()
        {
            var players = await tableService.Get(trackedPubgPlayer => true);
            trackedPlayers.Clear();

            foreach (var player in players)
            {
                trackedPlayers.Add(new TrackingState
                {
                    Player = player,
                    LatestMatchId = null
                });
            }

            trackedPlayers.Sort(
                (x, y) => Nullable.Compare(x.Player.Timestamp, y.Player.Timestamp));
        }
    }
}