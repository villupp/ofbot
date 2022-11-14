using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace OfBot.Components.Api.OpenDota
{
    public class OpenDotaApi
    {
        private readonly BotSettings botSettings;
        private readonly HttpClient httpClient;

        public OpenDotaApi(
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