using Microsoft.Extensions.Logging;
using OfBot.Api.OpenDota;
using OfBot.Config;
using System.Net.Http.Json;

namespace OfBot.Api.Dota
{
    public class OpenDotaApiClient : ApiClient
    {
        public OpenDotaApiClient(ILogger<ApiClient> logger, BotSettings botSettings, HttpClient httpClient) : base(logger, botSettings, httpClient)
        {
        }

        public async Task<GetPlayerResponse> GetPlayer(string accountId)
        {
            var path = $"/api/players/{accountId}";
            var httpResponse = await httpClient.GetAsync(path);
            if (httpResponse.IsSuccessStatusCode)
            {
                var getPlayerResponse = await httpResponse.Content.ReadFromJsonAsync<GetPlayerResponse>();
                return getPlayerResponse;
            }
            else
            {
                LogHttpFailure(httpResponse); ;
                throw new HttpRequestException();
            }
        }
    }
}