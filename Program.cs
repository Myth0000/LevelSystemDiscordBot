using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Interactions;
using Discord;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace LevelSystemDiscordBot
{
    class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => services
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()
                {
                    GatewayIntents =    GatewayIntents.All,
                    AlwaysDownloadUsers = true,
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()).Build();

            await RunAsync(host);
        }


        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
            var interactions = serviceProvider.GetRequiredService<InteractionService>();
            await serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();

            client.Log += async (LogMessage message) => Console.WriteLine(message);
            interactions.Log += async (LogMessage message) => Console.WriteLine(message);

            client.Ready += async () =>
            {
                Console.WriteLine("Bot Is Ready");
                await interactions.RegisterCommandsToGuildAsync(1113079064228536372);
            };

            client.MessageReceived += async (SocketMessage message) => LevelSystem.MessageReceived(message);

            await client.LoginAsync(TokenType.Bot, File.ReadAllText("TOKEN.txt"));
            await client.StartAsync();
            await Task.Delay(-1);

        }
    }
}