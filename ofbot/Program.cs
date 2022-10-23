using Azure.Data.Tables;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfBot.TableStorage;
using OfBot.TableStorage.Models;

namespace OfBot
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
            {
                var botSettings = new BotSettings();
                hostContext.Configuration.Bind(nameof(BotSettings), botSettings);

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
            .Build();

            host.Run();
        }
    }
}