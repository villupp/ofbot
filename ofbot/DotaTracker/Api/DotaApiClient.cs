using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using OfBot.Api;

namespace OfBot.Api.Dota
{
    public class DotaApiClient : ApiClient
    {
        public DotaApiClient(ILogger<ApiClient> logger, BotSettings botSettings, HttpClient httpClient) : base(logger, botSettings, httpClient)
        {
        }

        public async Task<GetMatchHistoryResponse> GetRecentDotaMatches(string accountId, int limit)
        {
            var endpoint = "/IDOTA2Match_570/GetMatchHistory/v1";
            string[] pathParams = new string[]
            {
                $"key={botSettings.SteamApiKey}",
                $"account_id={accountId}",
                $"matches_requested={limit}"
            };
            var pathParamsText = String.Join("&", pathParams);
            string path = $"{endpoint}?{pathParamsText}";
            var httpResponse = await httpClient.GetAsync(path);
            if (httpResponse.IsSuccessStatusCode)
            {
                var getMatchHistoryResponse = await httpResponse.Content.ReadFromJsonAsync<GetMatchHistoryResponse>();
                return getMatchHistoryResponse;
            }
            else
            {
                this.LogHttpFailure(httpResponse);;
                throw new HttpRequestException();
            }
        }

        public async Task<GetMatchDetailsResponse> GetMatchDetails(long matchId)
        {
            var endpoint = "/IDOTA2Match_570/GetMatchDetails/v1";
            string[] pathParams = new string[] {
                $"key={botSettings.SteamApiKey}",
                $"match_id={matchId}",
                "include_persona_names=1"
            };
            var pathParamsText = String.Join("&", pathParams);
            string path = $"{endpoint}?{pathParamsText}";
            var httpResponse = await httpClient.GetAsync(path); 
            if (httpResponse.IsSuccessStatusCode)
            {
                var getMatchDetailsResponse = await httpResponse.Content.ReadFromJsonAsync<GetMatchDetailsResponse>();
                return getMatchDetailsResponse;
            }
            else
            {
                LogHttpFailure(httpResponse);
                throw new HttpRequestException();
            }
        }

        public async Task<Match> GetMostRecentDotaMatch(string accountId)
        {
            var res = await GetRecentDotaMatches(accountId, 1);
            return res.result.matches.FirstOrDefault();
        }
    }
}