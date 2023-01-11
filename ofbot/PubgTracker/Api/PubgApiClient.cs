using Microsoft.Extensions.Logging;
using OfBot.Common;
using OfBot.Config;
using OfBot.PubgTracker.Api.Models;
using System.Net.Http.Json;

namespace OfBot.PubgTracker.Api
{
    public class PubgApiClient : ApiClient
    {
        public PubgApiClient(ILogger<ApiClient> logger, BotSettings botSettings, HttpClient httpClient) : base(logger, botSettings, httpClient)
        {
        }

        public async Task<Player> GetPlayer(string playerName)
        {
            var reqUri = $"players?filter[playerNames]={playerName}";
            var httpResponse = await httpClient.GetAsync(reqUri);

            if (!httpResponse.IsSuccessStatusCode)
            {
                LogHttpFailure(httpResponse);

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw new HttpRequestException();
            }

            var playerRes = await httpResponse.Content.ReadFromJsonAsync<PlayerResponse>();
            var resdbg = await httpResponse.Content.ReadAsStringAsync();
            if (playerRes != null && playerRes?.Players?.Count > 0)
                return playerRes.Players[0];
            else return null;
        }
    }
}
