namespace OfBot.Api.Dota
{
    public class GetMatchDetailsResponse {
        public MatchSearchResult result { get; set; }
    }
    public class MatchSearchResult {
        public Int64 status { get; set; }
        public Int64 num_results { get; set; }
        public Match[] matches { get; set; }
    }
    public class Match {
        public Int64 match_id { get; set; }
        public Int64 start_time { get; set; }
        public Int64 lobby_type { get; set; }
        public Player[] players { get; set; }
    }
    public class Player {
        public Int64 account_id { get; set; }
    }
}