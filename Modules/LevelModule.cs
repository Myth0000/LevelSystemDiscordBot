using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Interactions;
using Discord;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Reflection.Emit;

namespace LevelSystemDiscordBot.Modules
{
    public class LevelModule : InteractionModuleBase<SocketInteractionContext>
    {
        private MongoClient mongoClient = new(Settings.MongoDbConnectionString);


        [SlashCommand("level", "Displays the user's level.")]
        public async Task HandleLevel(SocketUser user = null)
        {
            if (user == null) { user = Context.User; }

            await DeferAsync();

            var collection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");
            var filter = Builders<BsonDocument>.Filter.Eq("UserId", user.Id);
            var userLevelData = collection.Find<BsonDocument>(filter).FirstOrDefault();
            UserLevel userLevel;

            if(userLevelData == null)
            {
                userLevel = new UserLevel(user.Id, 0);
            }
            else
            {
                userLevel = BsonSerializer.Deserialize<UserLevel>(userLevelData);
            }
            

            Embed embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder().WithName(user.Username))
                .WithImageUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .AddField(new EmbedFieldBuilder().WithName($"**Level {userLevel.Level}**").WithValue($"{userLevel.Exp}/{userLevel.MaxExp} experience\n{userLevel.TotalExp} total experience"))
                .WithCurrentTimestamp()
                .Build();

            await FollowupAsync(embed: embed);
        }



        [SlashCommand("leaderboard", "Shows the the users with the highest levels")]
        public async Task HandleLeaderboard()
        {
            await DeferAsync();

            var collection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");
            var sortLevel = Builders<BsonDocument>.Sort.Descending("TotalExp");
            List<UserLevel> userLevels = collection.Find<BsonDocument>(new BsonDocument()).Sort(sortLevel).ToList().Select(_userLevel => BsonSerializer.Deserialize<UserLevel>(_userLevel)).ToList();
            string usersEmbedContent = "```";

            int index = 1;
            foreach(var userLevel in userLevels)
            {
                SocketGuildUser user = Context.Guild.GetUser(userLevel.UserId);
                string userLevelText = $"{index}. {user.Username}";

                // keeps adding space until there are 35 characters in the text
                for(int textLength = userLevelText.Length ; textLength < 22; textLength++) { userLevelText += " "; }
                userLevelText += $" {userLevel.Level}\n";

                usersEmbedContent += userLevelText;
                index++;
            }

            if(userLevels.Count <= 0)
            {
                Embed _embed = new EmbedBuilder()
                .WithAuthor("Levels Leaderboard").WithDescription($"Leaderboard is empty.")
                .WithCurrentTimestamp()
                .Build();

                await FollowupAsync(embed: _embed);
                return;
            }

            Embed embed = new EmbedBuilder()
                .WithAuthor("Levels Leaderboard").WithDescription($"Top {userLevels.Count()} highest leveled users.")
                .AddField(new EmbedFieldBuilder().WithName("**User                                                  Level**")
                .WithValue(usersEmbedContent + "```")) // 32 spaces + 4 + 5 = 41 chars
                .WithCurrentTimestamp()
                .Build();

            await FollowupAsync(embed: embed);
        }
























        [SlashCommand("add", "Adds item to the database")]
        public async Task HandleAdd(SocketUser user, int level)
        {
            var userLevel = new UserLevel(user.Id, level).ToBsonDocument<UserLevel>();
            var levelCollection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");

            levelCollection.InsertOne(userLevel);
        }




















        [SlashCommand("dbview", "shows all items in the database")]
        public async Task HandleDbview()
        {
            await DeferAsync();

            var levelCollection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");
            string message = "";

            var data = levelCollection.Find(new BsonDocument()).ToList();

            foreach(var level in data)
            {
                UserLevel userLevel = BsonSerializer.Deserialize<UserLevel>(level);

                try
                {
                    message += $"{Context.Guild.GetUser(userLevel.UserId).Mention} **`{userLevel.Level} | {userLevel.Exp}/{userLevel.MaxExp}`**\n\n";
                }
                catch(Exception ex)
                {
                    message += $"{userLevel.UserId} **`{userLevel.Level} | {userLevel.Exp}/{userLevel.MaxExp}`**\n\n";
                }
                
            }

            if(message.Length <= 0)
            {
                await FollowupAsync("NONE");
                return;
            }

            await FollowupAsync(message);
        }
    }
}
