using Microsoft.Extensions.Logging;
using OfBot.Config;
using System.Net.Http.Headers;

namespace OfBot.Api.Pubg
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly BotSettings settings;
        private readonly ILogger<AuthenticationHandler> logger;

        public AuthenticationHandler(BotSettings settings, ILogger<AuthenticationHandler> logger)
        {
            this.logger = logger;
            this.settings = settings;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.PubgApiKey);
            request.Headers.Add("Accept", "application/vnd.api+json");

            return await base.SendAsync(request, cancellationToken);
        }
    }
}