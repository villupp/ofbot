using OfBot.TableStorage.Models;

namespace OfBot.Api.Models
{
    public class TrackingState
    {
        public TrackedPubgPlayer Player { get; set; }
        public Guid? LatestMatchId { get; set; }
    }
}