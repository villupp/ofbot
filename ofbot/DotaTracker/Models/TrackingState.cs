namespace OfBot.DotaTracker.Models
{
    public class TrackingState<T>
    {
        public T player { get; set; }
        public long? latestMatchId { get; set; }
    }
}