using System.Net.Http.Json;
using OfBot.Components.Api.Dota;

namespace OfBot.Components.Api.Dota
{
    public class DotaApi
    {
        private readonly BotSettings botSettings;
        private readonly HttpClient httpClient;

        public DotaApi(
            BotSettings botSettings,
            HttpClient httpClient)
        {
            this.botSettings = botSettings;
            this.httpClient = httpClient;
        }
        public async Task<GetMatchDetailsResponse> GetRecentDotaMatches(string accountId, int limit)
        {
            var endpoint = "/IDOTA2Match_570/GetMatchHistory/v1";
            string[] pathParams = new string[] {
                $"key={botSettings.SteamApiKey}",
                $"account_id={accountId}",
                $"matches_requested={limit}"
            };
            var pathParamsText = String.Join("&", pathParams);
            string path = $"{endpoint}?{pathParamsText}";
            var response = await httpClient.GetFromJsonAsync<GetMatchDetailsResponse>(path);
            return response;
        }
    }
}