using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace OfBot.Api.OpenDota
{
    public class OpenDotaApiClient
    {
        private readonly BotSettings botSettings;
        private readonly HttpClient httpClient;

        public OpenDotaApiClient(
            BotSettings botSettings,
            HttpClient httpClient
        )
        {
            this.botSettings = botSettings;
            this.httpClient = httpClient;
        }
        public async Task<GetPlayerResponse> GetPlayer(string accountId)
        {
            var endpoint = $"/api/players/{accountId}";
            var response = await httpClient.GetFromJsonAsync<GetPlayerResponse>(endpoint);
            return response;
        }
    }
}