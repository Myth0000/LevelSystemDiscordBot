using Discord.WebSocket;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace LevelSystemDiscordBot
{
    public static class LevelSystem
    {
        /// <summary>
        /// Runs when an user sends a message
        /// </summary>
        public async static Task MessageReceived(SocketMessage message)
        {
            var channel = message.Channel;

            // bot & dm messages are ignored
            if (message.Author.IsBot || channel.ToString().First<char>() == '@') { return; }

            MongoClient mongoClient = new(Settings.MongoDbConnectionString);
            var collection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");
            var filter = Builders<BsonDocument>.Filter.Eq("UserId", message.Author.Id);
            var update = Builders<BsonDocument>.Update;
            UserLevel userLevel = BsonSerializer.Deserialize<UserLevel>(collection.Find(filter).FirstOrDefault());
            var expIncrement = new Random().Next(1, 6);

            userLevel.Exp += expIncrement;

            var updateTotalExp = update.Set("TotalExp", userLevel.TotalExp + expIncrement);
            collection.UpdateOne(filter, updateTotalExp);

            // level up
            if(userLevel.Exp >= userLevel.MaxExp)
            {
                var updateLevel = update.Set("Level", ++userLevel.Level);
                var updateExp = update.Set("Exp", userLevel.Exp - userLevel.MaxExp);
                var updateMaxExp = update.Set("MaxExp", Math.Floor(userLevel.MaxExp * 1.1));

                collection.UpdateOne(filter, updateLevel);
                collection.UpdateOne(filter, updateExp);
                collection.UpdateOne(filter, updateMaxExp);

                await channel.SendMessageAsync($"**`{channel.GetUserAsync(userLevel.UserId).Result.Username}`** has leveled up to level {userLevel.Level}");
            }
            else
            {
                var updateExp = update.Set("Exp", userLevel.Exp);
                collection.UpdateOne(filter, updateExp);
            }

        }


    }
}
