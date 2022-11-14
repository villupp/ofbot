using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

namespace OfBot.Components.Api
{
    public class DotaApi
    {
        private readonly ILogger<DotaApi> logger = null!;
        private readonly BotSettings botSettings;
        private readonly HttpClient httpClient;

        public DotaApi(
            ILogger<DotaApi> logger,
            BotSettings botSettings,
            HttpClient httpClient)
        {
            this.logger = logger;
            this.botSettings = botSettings;
            this.httpClient = httpClient;
        }
        public async Task<MatchListResponse> GetRecentDotaMatches(string accountId, int limit)
        {
            var endpoint = "/IDOTA2Match_570/GetMatchHistory/v1";
            string[] pathParams = new string[] {
                $"key={botSettings.SteamApiKey}",
                $"account_id={accountId}",
                $"matches_requested={limit}"
            };
            var pathParamsText = String.Join("&", pathParams);
            string path = $"{endpoint}?{pathParamsText}";
            var response = await httpClient.GetFromJsonAsync<MatchListResponse>(path);
            return response;
        }
    }
}