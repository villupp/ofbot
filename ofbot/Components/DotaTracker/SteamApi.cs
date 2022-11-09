using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace OfBot
{
    public class SteamApi
    {
        private readonly ILogger<SteamApi> logger = null!;
        private readonly BotSettings botSettings;
        private readonly HttpClient httpClient;

        public SteamApi(
            ILogger<SteamApi> logger,
            BotSettings botSettings,
            HttpClient httpClient)
        {
            this.logger = logger;
            this.botSettings = botSettings;
            this.httpClient = httpClient;
        }
        public async void GetRecentDotaMatches(string accountId, int limit)
        {
            
            var endpoint = "/IDOTA2Match_570/GetMatchHistory/v1";
            string[] pathParams = new string[] {
                $"key={botSettings.SteamApiKey}",
                $"account_id={accountId}",
                $"matches_requested={limit}"
            };
            var pathParamsString = String.Join("&", pathParams);
            string path = $"{endpoint}?{pathParamsString}";
            var result = await httpClient.GetStringAsync(path);
            logger.LogInformation(result);
        }
    }
}