using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Interactions;
using Discord;

namespace LevelSystemDiscordBot.Modules
{
    public class LevelsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("level", "Displays the user's level.")]
        public async Task HandleLevel(SocketGuildUser user)
        {
            await RespondAsync($"**`{user.Username}`** level is **`UNKNOWN`**");
        }
    }
}
