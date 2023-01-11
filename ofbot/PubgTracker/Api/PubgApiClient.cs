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

        public async Task<List<Player>> GetPlayers(List<string> playerNames)
        {
            var playerNamesStr = string.Join(',', playerNames);

            logger.LogInformation($"Getting players: '{playerNamesStr}'");

            var reqUri = $"players?filter[playerNames]={}";
            var httpResponse = await httpClient.GetAsync(reqUri);

            if (!httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                LogHttpFailure(httpResponse);

                throw new HttpRequestException();
            }

            var playerRes = await httpResponse.Content.ReadFromJsonAsync<PlayerResponse>();

            logger.LogInformation($"Got {playerRes?.Players?.Count} players from PUBG API");

            return playerRes.Players;
        }

        public async Task<Player> GetPlayer(string playerName)
        {
            var players = await GetPlayers(new List<string>() { playerName });

            if (players != null && players?.Count > 0)
                return players[0];
            else
                return null;
        }
    }
}