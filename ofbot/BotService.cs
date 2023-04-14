using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfBot.Components;
using OfBot.Config;
using OfBot.DotaTracker;
using OfBot.PubgTracker;
using System.Reflection;

namespace OfBot
{
    public sealed class BotService : IHostedService
    {
        private readonly ILogger logger;
        private readonly BotSettings botSettings;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly CommandService commandService;
        private readonly InteractionService interactionService;
        private readonly IServiceProvider serviceProvider;
        private readonly ButtonHandler buttonHandler;
        private readonly ModalHandler modalHandler;
        private readonly DotaPoller dotaPoller;
        private readonly PubgPoller pubgPoller;

        public BotService(
            ILogger<BotService> logger,
            IHostApplicationLifetime appLifetime,
            BotSettings botSettings,
            DiscordSocketClient discordSocketClient,
            CommandService commandService,
            InteractionService interactionService,
            IServiceProvider serviceProvider,
            ButtonHandler buttonHandler,
            ModalHandler modalHandler,
            DotaPoller dotaPoller,
            PubgPoller pubgPoller
            )
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            this.logger = logger;
            this.botSettings = botSettings;
            this.discordSocketClient = discordSocketClient;
            this.commandService = commandService;
            this.interactionService = interactionService;
            this.serviceProvider = serviceProvider;
            this.buttonHandler = buttonHandler;
            this.modalHandler = modalHandler;
            this.dotaPoller = dotaPoller;
            this.pubgPoller = pubgPoller;
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

            discordSocketClient.Ready += async () =>
            {
                logger.LogInformation("Bot is connected and ready");

                if (botSettings.DotaTrackerIsEnabled)
                {
                    logger.LogInformation("Starting dota tracker polling service");
                    _ = dotaPoller.Start().ConfigureAwait(false);
                }
                if (botSettings.PubgTrackerIsEnabled)
                {
                    logger.LogInformation("Starting PUBG tracker polling service");
                    _ = pubgPoller.Start().ConfigureAwait(false);
                }

                var slashCommands = new List<SlashCommandBuilder>
                {
                    new SlashCommandBuilder()
                        .WithName("say")
                        .WithDescription("Make me say something.")
                        .AddOption("message", ApplicationCommandOptionType.String, "Message", isRequired: true),
                    new SlashCommandBuilder()
                        .WithName("git")
                        .WithDescription("Link to ofbot git version control."),
                    new SlashCommandBuilder()
                        .WithName("help")
                        .WithDescription("Provides general information about bot."),
                    new SlashCommandBuilder()
                        .WithName("dotatracker")
                        .WithDescription("DOTA tracker related commands.")
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("track")
                            .WithDescription("Track a new player.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("accountid", ApplicationCommandOptionType.String, "DOTA player account ID", isRequired: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("untrack")
                            .WithDescription("Untrack a tracked player.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("accountid", ApplicationCommandOptionType.String, "DOTA player account ID", isRequired: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("list")
                            .WithDescription("List all tracked players.")
                            .WithType(ApplicationCommandOptionType.SubCommand)),
                    new SlashCommandBuilder()
                        .WithName("pubgtracker")
                        .WithDescription("PUBG tracker related commands.")
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("track")
                            .WithDescription("Track a new player.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("playername", ApplicationCommandOptionType.String, "PUBG player name (case sensitive)", isRequired: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("untrack")
                            .WithDescription("Untrack a tracked player.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("playername", ApplicationCommandOptionType.String, "PUBG player name (case sensitive)", isRequired: true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("list")
                            .WithDescription("List all tracked players.")
                            .WithType(ApplicationCommandOptionType.SubCommand)),
                    new SlashCommandBuilder()
                        .WithName("pubgstats")
                        .WithDescription("PUBG stats related commands.")
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("player")
                            .WithDescription("Get player stats for current ranked season (squad FPP).")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("playername", ApplicationCommandOptionType.String, "PUBG player name (case sensitive)", isRequired: true)
                            .AddOption("ispublic", ApplicationCommandOptionType.Boolean, "Announce stats in public response"))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("refreshseasons")
                            .WithDescription("Refreshes season cache. Might take some time to complete.")
                            .WithType(ApplicationCommandOptionType.SubCommand)),
                    new SlashCommandBuilder()
                        .WithName("registration")
                        .WithDescription("Create a new registration session.")
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("create")
                            .WithDescription("Creates a new registration session.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("description", ApplicationCommandOptionType.String, "Session description"))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("bump")
                            .WithDescription("Reposts a registration session.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("sessionid", ApplicationCommandOptionType.String, "Session ID"))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("changedescription")
                            .WithDescription("Changes description of your most recent registration session.")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("description", ApplicationCommandOptionType.String, "Session description", isRequired: true))
                };

                try
                {
                    foreach (var slashCmd in slashCommands)
                        await discordSocketClient.Rest.CreateGuildCommand(slashCmd.Build(), botSettings.PrimaryGuildId);
                }
                catch (HttpException ex)
                {
                    logger.LogError($"Error while creating command: {ex}");
                }
            };

            discordSocketClient.ButtonExecuted += buttonHandler.OnButtonExecuted;
            discordSocketClient.ModalSubmitted += modalHandler.OnModalSubmitted;

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
            discordSocketClient.SlashCommandExecuted += HandleSlashCommand;

            await commandService.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: serviceProvider);

            await interactionService.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: serviceProvider);
        }

        private async Task HandleSlashCommand(SocketSlashCommand slashCommand)
        {
            logger.LogInformation($"slash command {slashCommand.CommandName} executed");

            var context = new SocketInteractionContext(discordSocketClient, slashCommand);

            await interactionService.ExecuteCommandAsync(
                context: context,
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