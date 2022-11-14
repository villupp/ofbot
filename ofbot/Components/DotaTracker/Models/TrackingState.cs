
namespace OfBot.Components.DotaTracker
{
    public class TrackingState<T> {
        public T player { get; set; }
        public Int64? latestMatchId { get; set; }
    }
}