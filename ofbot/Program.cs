using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfBot.Api.Dota;
using OfBot.Api.Pubg;
using OfBot.CommandHandlers.PubgStats;
using OfBot.CommandHandlers.Registration;
using OfBot.Components;
using OfBot.Config;
using OfBot.DotaTracker;
using OfBot.PubgTracker;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;
using OfBot.TableStorage.Repositories;
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
                    GatewayIntents = GatewayIntents.AllUnprivileged
                };

                var commandServiceConfig = new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false
                };

                var interactionServiceConfig = new InteractionServiceConfig()
                {
                };

                services.AddAzureClients(builder =>
                {
                    builder.AddTableServiceClient(botSettings.StorageKey);
                });

                services.AddMemoryCache();

                services.AddHostedService<BotService>();

                services.AddSingleton(serviceProvider => serviceProvider);
                services.AddSingleton(botSettings);
                services.AddSingleton(discordSocketConfig);
                services.AddSingleton(commandServiceConfig);
                services.AddSingleton(interactionServiceConfig);
                services.AddSingleton<DiscordSocketClient>();
                services.AddSingleton<CommandService>();
                services.AddSingleton<InteractionService>();
                services.AddSingleton<TableStorageService<Command>>();
                services.AddSingleton<TableStorageService<PubgSeason>>();
                services.AddSingleton<TableStorageService<PubgPlayer>>();
                services.AddSingleton<RegistrationHandler>();
                services.AddSingleton<PubgStatsHandler>();
                services.AddSingleton<ButtonHandler>();
                services.AddSingleton<ModalHandler>();
                services.AddSingleton<AnnouncementService>();
                services.AddSingleton<AuthenticationHandler>();
                services.AddSingleton<CommandRepository>();

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
                    builder.ClearProviders();
                    builder.AddConsole();
                    if (!string.IsNullOrEmpty(botSettings.AppInsightsConnectionString))
                        builder.AddApplicationInsights(
                            configureTelemetryConfiguration: (config) => config.ConnectionString = botSettings.AppInsightsConnectionString,
                            configureApplicationInsightsLoggerOptions: (config) => { });
                })
            .Build();

            host.Run();
        }
    }
}