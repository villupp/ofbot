﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfBot.Components;
using System.Reflection;

namespace OfBot
{
    public sealed class BotService : IHostedService
    {
        private readonly ILogger logger;
        private readonly BotSettings botSettings;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly CommandService commandService;
        private readonly IServiceProvider serviceProvider;
        private readonly ButtonHandler buttonHandler;

        public BotService(
            ILogger<BotService> logger,
            IHostApplicationLifetime appLifetime,
            BotSettings botSettings,
            DiscordSocketClient discordSocketClient,
            CommandService commandService,
            IServiceProvider serviceProvider,
            ButtonHandler buttonHandler
            )
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            this.logger = logger;
            this.botSettings = botSettings;
            this.discordSocketClient = discordSocketClient;
            this.commandService = commandService;
            this.serviceProvider = serviceProvider;
            this.buttonHandler = buttonHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StartAsync");

            discordSocketClient.Log += WriteDiscordLogMessage;

            if (string.IsNullOrEmpty(botSettings.BotToken))
            {
                throw new Exception($"Bot token (BotSettings.BotToken) not set in configuration.");
            }

            await discordSocketClient.LoginAsync(TokenType.Bot, botSettings.BotToken);
            await discordSocketClient.StartAsync();

            discordSocketClient.Ready += () =>
            {
                logger.LogInformation("Bot is connected and ready");
                return Task.CompletedTask;
            };

            discordSocketClient.ButtonExecuted += buttonHandler.OnButtonExecuted;

            await InstallCommands();
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
            discordSocketClient.MessageReceived += messageHandler.Handle;

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