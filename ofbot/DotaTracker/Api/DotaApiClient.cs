using System.Net.Http.Json;

namespace OfBot.Api.Dota
{
    public class DotaApiClient
    {
        private readonly BotSettings botSettings;
        private readonly HttpClient httpClient;

        public DotaApiClient(
            BotSettings botSettings,
            HttpClient httpClient)
        {
            this.botSettings = botSettings;
            this.httpClient = httpClient;
        }
        public async Task<GetMatchHistoryResponse> GetRecentDotaMatches(string accountId, int limit)
        {
            var endpoint = "/IDOTA2Match_570/GetMatchHistory/v1";
            string[] pathParams = new string[] {
                $"key={botSettings.SteamApiKey}",
                $"account_id={accountId}",
                $"matches_requested={limit}"
            };
            var pathParamsText = String.Join("&", pathParams);
            string path = $"{endpoint}?{pathParamsText}";
            var response = await httpClient.GetFromJsonAsync<GetMatchHistoryResponse>(path);
            return response;
        }

        public async Task<GetMatchDetailsResponse> GetMatchDetails(Int64 matchId)
        {
            var endpoint = "/IDOTA2Match_570/GetMatchDetails/v1";
            string[] pathParams = new string[] {
                $"key={botSettings.SteamApiKey}",
                $"match_id={matchId}"
            };
            var pathParamsText = String.Join("&", pathParams);
            string path = $"{endpoint}?{pathParamsText}";
            var response = await httpClient.GetFromJsonAsync<GetMatchDetailsResponse>(path);
            return response;
        }

        public async Task<Match> GetMostRecentDotaMatch(string accountId)
        {
            var res = await GetRecentDotaMatches(accountId, 1);

            return res?.result?.matches?.FirstOrDefault();
        }
    }
}