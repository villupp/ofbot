namespace OfBot.Api.Dota
{
    public class GetMatchDetailsResponse {
        public MatchDetailsResult result { get; set; }
    }
    public class MatchDetailsResult {
        public Int64 match_id { get; set; }
        public bool radiant_win { get; set; }
        public int duration { get; set; }
        public int radiant_score { get; set; }
        public int dire_score { get; set; }
        public MatchDetailsPlayer[] players { get; set; }
    }
    public class MatchDetailsPlayer {
        public Int64 account_id { get; set; }
        public int player_slot { get; set; }
        public string persona { get; set; }
    }
}