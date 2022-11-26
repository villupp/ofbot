using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace OfBot.Api.OpenDota
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
                this.LogHttpFailure(httpResponse); ;
                throw new HttpRequestException();
            }
        }
    }
}