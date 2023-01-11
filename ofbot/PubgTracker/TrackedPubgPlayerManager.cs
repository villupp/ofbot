using OfBot.Common;
using OfBot.PubgTracker.Api;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;

namespace OfBot.PubgTracker
{
    public class TrackedPubgPlayerManager
    {
        public List<TrackingState<TrackedPubgPlayer>> trackingStates { get; set; }
        private TableStorageService<TrackedPubgPlayer> tableService;
        private PubgApiClient pubgClient;

        public TrackedPubgPlayerManager(
            TableStorageService<TrackedPubgPlayer> tableService,
            PubgApiClient pubgClient
            )
        {
            trackingStates = new List<TrackingState<TrackedPubgPlayer>>();
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
            trackingStates.Add(new TrackingState<TrackedPubgPlayer>()
            {
                latestMatchId = null,
                player = trackedPlayer
            });

            return trackedPlayer;
        }

        public async Task<TrackedPubgPlayer> Remove(string playerName)
        {
            var player = await GetTrackedPlayer(playerName);

            if (player == null)
                throw new Exception($"Could not find tracked PUBG player with name '{playerName}'");

            await tableService.Delete(player);
            trackingStates.Remove(trackingStates.FirstOrDefault(state => state.player.Name == playerName));
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
            trackingStates.Clear();

            foreach (var player in players)
            {
                trackingStates.Add(new TrackingState<TrackedPubgPlayer>
                {
                    player = player,
                    latestMatchId = null
                });
            }

            trackingStates.Sort(
                (x, y) => Nullable.Compare(x.player.Timestamp, y.player.Timestamp));
        }
    }
}