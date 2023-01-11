namespace OfBot.Common
{
    public class TrackingState<T>
    {
        public T player { get; set; }
        public long? latestMatchId { get; set; }
    }
}