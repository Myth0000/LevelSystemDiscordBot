using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Discord.WebSocket;
using Discord.Interactions;

namespace LevelSystemDiscordBot
{
    public class InteractionHandler
    {
        private readonly IServiceProvider Services;
        private readonly InteractionService Interactions;
        private readonly DiscordSocketClient Client;
        public InteractionHandler(DiscordSocketClient client, InteractionService interactions, IServiceProvider services)
        {
            this.Services = services;
            this.Interactions = interactions;
            this.Client = client;
        }


        public async Task InitializeAsync()
        {
            try
            {
                await Interactions.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
                Client.InteractionCreated += HandleInteraction;
            } catch (Exception ex) { Console.WriteLine(ex); }
            
        }

        public async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                SocketInteractionContext context = new(Client, interaction);
                await Interactions.ExecuteCommandAsync(context, Services);
            } catch(Exception ex) { Console.WriteLine(ex.ToString()); }
        }
    }
}
