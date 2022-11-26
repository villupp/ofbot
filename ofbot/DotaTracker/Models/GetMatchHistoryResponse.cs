namespace OfBot.Api.Dota
{
    public class GetMatchHistoryResponse {
        public MatchSearchResult result { get; set; }
    }
    public class MatchSearchResult {
        public long status { get; set; }
        public long num_results { get; set; }
        public Match[] matches { get; set; }
    }
    public class Match {
        public long match_id { get; set; }
        public long start_time { get; set; }
        public long lobby_type { get; set; }
        public Player[] players { get; set; }
    }
    public class Player {
        public long account_id { get; set; }
    }
}