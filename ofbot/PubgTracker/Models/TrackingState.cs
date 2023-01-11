using OfBot.TableStorage.Models;

namespace OfBot.PubgTracker.Models
{
    public class TrackingState
    {
        public TrackedPubgPlayer Player { get; set; }
        public Guid? LatestMatchId { get; set; }
    }
}