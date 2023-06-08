using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Interactions;
using Discord;

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
                    GatewayIntents =    GatewayIntents.AllUnprivileged,
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

            client.MessageReceived += async (SocketMessage message) =>
            {
                // bot & dm messages are ignored
                if(message.Author.IsBot || message.Channel.ToString().First<char>() == '@') { return; }


                message.Channel.SendMessageAsync("MESSAGE SENT");
            };

            await client.LoginAsync(TokenType.Bot, "MTExMzkyNzU1NjY1MjA4MTIyMg.GtE3ZF.f67-pnn6ZFBMKI5nKFaDPU6SQ69vPRxCcBe9bg");
            await client.StartAsync();
            await Task.Delay(-1);

        }
    }
}