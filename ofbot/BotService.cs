using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfBot.Components;
using OfBot.DotaTracker;
using System.Reflection;

namespace OfBot
{
    public sealed class BotService : IHostedService
    {
        private readonly ILogger logger;
        private readonly BotSettings botSettings;
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly IServiceProvider serviceProvider;
        private readonly ButtonHandler buttonHandler;
        private readonly DotaPoller dotaPoller;

        private static readonly Dictionary<string, string> allGlobalCommands = new()
        {
            {"ping", "Get a pong."},
            {"reg", "Create a registration session."},
            {"ofbot", "Create a registration session."},
        };

        public BotService(
            ILogger<BotService> logger,
            IHostApplicationLifetime appLifetime,
            BotSettings botSettings,
            DiscordSocketClient discordSocketClient,
            CommandService commandService,
            IServiceProvider serviceProvider,
            ButtonHandler buttonHandler,
            DotaPoller dotaPoller
            )
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            this.logger = logger;
            this.botSettings = botSettings;
            this.discordClient = discordSocketClient;
            this.commandService = commandService;
            this.serviceProvider = serviceProvider;
            this.buttonHandler = buttonHandler;
            this.dotaPoller = dotaPoller;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StartAsync");

            discordClient.Log += WriteDiscordLogMessage;
            discordClient.Ready += OnClientReady;
            discordClient.ButtonExecuted += buttonHandler.OnButtonExecuted;

            if (string.IsNullOrEmpty(botSettings.BotToken))
            {
                throw new Exception($"Bot token (BotSettings.BotToken) not set in configuration.");
            }

            await discordClient.LoginAsync(TokenType.Bot, botSettings.BotToken);
            await discordClient.StartAsync();

            await InstallCommands();
        }

        public async Task OnClientReady()
        {
            logger.LogInformation("Bot is connected and ready");

            try
            {
                logger.LogInformation("Installing global commands");
                await InstallGlobalCommands();
                logger.LogInformation("Starting dota tracker polling service");
                await dotaPoller.Start();
            }
            catch (HttpException ex)
            {
                logger.LogError($"Error while creating global commands: {ex}");
            }
        }

        public async Task InstallGlobalCommands()
        {
            var existingGlobalCommands = await discordClient.GetGlobalApplicationCommandsAsync();

            // Add missing global commands
            foreach (var command in allGlobalCommands)
            {
                if (!existingGlobalCommands.Any(c => c.Name.ToLower() == command.Key))
                {
                    var globalCommand = new SlashCommandBuilder();
                    globalCommand.WithName(command.Key);
                    globalCommand.WithDescription(command.Value);

                    await discordClient.CreateGlobalApplicationCommandAsync(globalCommand.Build());
                }
            }

            // Remove unused global commands
            foreach (var existingCommand in existingGlobalCommands)
            {
                if (!allGlobalCommands.ContainsKey(existingCommand.Name.ToLower()))
                {
                    await existingCommand.DeleteAsync();
                }
            }
        }

        private async Task WriteDiscordLogMessage(LogMessage msg)
        {
            await Task.CompletedTask;
            logger.LogInformation($"Discord client: {msg.ToString()}");
        }

        public async Task InstallCommands()
        {
            logger.LogInformation($"Installing commands");

            var messageHandler = serviceProvider.GetService<MessageHandler>();
            discordClient.MessageReceived += messageHandler.Handle;

            await commandService.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: serviceProvider);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StopAsync has been called.");

            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            logger.LogInformation("OnStarted has been called.");
        }

        private void OnStopping()
        {
            logger.LogInformation("OnStopping has been called.");
        }

        private void OnStopped()
        {
            logger.LogInformation("OnStopped has been called.");
        }
    }
}