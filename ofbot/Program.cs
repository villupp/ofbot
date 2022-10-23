using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;
using Microsoft.Extensions.Logging;

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
                    // Add a cloud table client
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
            })
                .ConfigureLogging((context, builder) => {
                    string instrumentationKey = botSettings.ApplicationInsightsKey;
                    if (!string.IsNullOrEmpty(instrumentationKey))
                        builder.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = instrumentationKey);
                })
            .Build();

            host.Run();
        }
    }
}