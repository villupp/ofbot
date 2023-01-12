using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfBot.Api.Dota;
using OfBot.Api.OpenDota;
using OfBot.CommandHandlers;
using OfBot.Components;
using OfBot.Config;
using OfBot.DotaTracker;
using OfBot.Api;
using OfBot.Api.Pubg;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;
using System.Net;

namespace OfBot
{
    public class Program
    {
        private static BotSettings botSettings;

        private static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
            {
                var settings = new BotSettings();
                hostContext.Configuration.Bind(nameof(BotSettings), settings);
                botSettings = settings;

                var discordSocketConfig = new DiscordSocketConfig()
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                };

                var commandServiceConfig = new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false
                };

                services.AddAzureClients(builder =>
                {
                    builder.AddTableServiceClient(botSettings.StorageKey);
                });

                services.AddHostedService<BotService>();

                services.AddSingleton(serviceProvider => serviceProvider);
                services.AddSingleton(botSettings);
                services.AddSingleton(discordSocketConfig);
                services.AddSingleton(commandServiceConfig);
                services.AddSingleton<DiscordSocketClient>();
                services.AddSingleton<CommandService>();
                services.AddSingleton<CustomCommandService>();
                services.AddSingleton<MessageHandler>();
                services.AddSingleton<TableStorageService<Command>>();
                services.AddSingleton<RegistrationHandler>();
                services.AddSingleton<ButtonHandler>();
                services.AddSingleton<ModalHandler>();
                services.AddSingleton<AnnouncementService>();
                services.AddSingleton<AuthenticationHandler>();

                // DotaTracker
                services.AddSingleton<TableStorageService<TrackedDotaPlayer>>();
                services.AddSingleton<DotaPoller>();
                services.AddSingleton<TrackedDotaPlayers>();
                services.AddHttpClient<DotaApiClient>(client => { client.BaseAddress = new Uri("https://api.steampowered.com"); });
                services.AddHttpClient<OpenDotaApiClient>(client => { client.BaseAddress = new Uri("https://api.opendota.com"); });

                // PubgTracker
                services.AddSingleton<TableStorageService<TrackedPubgPlayer>>();
                services.AddSingleton<PubgPoller>();
                services.AddSingleton<TrackedPubgPlayerManager>();
                services.AddHttpClient<PubgApiClient>(client => { client.BaseAddress = new Uri(botSettings.PubgApiBaseUrl); })
                    .AddHttpMessageHandler<AuthenticationHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                    {
                        DefaultProxyCredentials = CredentialCache.DefaultCredentials
                    })
                    ;
            })
                .ConfigureLogging((context, builder) =>
                {
                    string instrumentationKey = botSettings.ApplicationInsightsKey;
                    if (!string.IsNullOrEmpty(instrumentationKey))
                        builder.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = instrumentationKey);
                })
            .Build();

            host.Run();
        }
    }
}