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
using System.Threading.Channels;
using System.Runtime.InteropServices;
using Discord.Interactions;
using MongoDB.Bson.Serialization.Attributes;

namespace LevelSystemDiscordBot
{
    public static class LevelSystem
    {
        private static MongoClient mongoClient = new(Settings.MongoDbConnectionString);

        /// <summary>
        /// Runs when an user sends a message
        /// </summary>
        public async static Task MessageReceived(SocketMessage message, [Optional] Func<bool> onExpGain, [Optional] Func<bool> onLevelUp)
        {
            var channel = message.Channel;
            var userId = message.Author.Id;

            // bot & dm messages are ignored
            if (message.Author.IsBot || channel.ToString().First<char>() == '@') { return; }

            GiveExp(userId);

            try { onExpGain(); } catch { }

            if (LevelUpIfPossible(userId))
            {
                try { onLevelUp(); } catch { }

                UserLevel userLevel = GetUserLevel(userId);
                await channel.SendMessageAsync($"**`{channel.GetUserAsync(userId).Result.Username}`** has leveled up to level {userLevel.Level}");
            }

        }


        public static void GiveExp(ulong userId, [Optional]int amount)
        {
            var collection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");
            var update = Builders<BsonDocument>.Update;
            var expIncrement = amount == 0 ? new Random().Next(1, 6) : amount;

            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserId", userId);
                UserLevel userLevel = BsonSerializer.Deserialize<UserLevel>(collection.Find(filter).FirstOrDefault());

                var updateExp = update.Set("Exp", userLevel.Exp + expIncrement);
                collection.UpdateOne(filter, updateExp);

                var updateTotalExp = update.Set("TotalExp", userLevel.TotalExp + expIncrement);
                collection.UpdateOne(filter, updateTotalExp);
            }
            catch (ArgumentNullException error) // user does not exist in the database exception
            {
                var newUserLevel = new UserLevel(userId, 0);

                newUserLevel.Exp += expIncrement;
                newUserLevel.TotalExp += expIncrement;

                collection.InsertOne(newUserLevel.ToBsonDocument());
            }



        }

        public static UserLevel AddUserToDatabase(ulong userId)
        {
            var collection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");
            var userLevel = new UserLevel(userId, 0);

            collection.InsertOne(userLevel.ToBsonDocument<UserLevel>());
            return userLevel;
        }

        private static bool LevelUpIfPossible(ulong userId)
        {
            var collection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");
            var update = Builders<BsonDocument>.Update;
            var filter = Builders<BsonDocument>.Filter.Eq("UserId", userId);
            UserLevel userLevel = GetUserLevel(userId);

            // level up
            if (userLevel.Exp >= userLevel.MaxExp)
            {
                var updateLevel = update.Set("Level", ++userLevel.Level);
                var updateExp = update.Set("Exp", userLevel.Exp - userLevel.MaxExp);
                var updateMaxExp = update.Set("MaxExp", Math.Floor(userLevel.MaxExp * 1.1));

                collection.UpdateOne(filter, updateLevel);
                collection.UpdateOne(filter, updateExp);
                collection.UpdateOne(filter, updateMaxExp);
                return true;
            }
            return false;
        }

        private static UserLevel GetUserLevel(ulong userId)
        {
            var collection = mongoClient.GetDatabase("LevelDatabase").GetCollection<BsonDocument>("LevelCollection");

            var filter = Builders<BsonDocument>.Filter.Eq("UserId", userId);
            var userLevelData = collection.Find<BsonDocument>(filter).FirstOrDefault();

            if (userLevelData != null)
            {
                return BsonSerializer.Deserialize<UserLevel>(userLevelData);
            }
            else
            {
                throw new Exception($"LevelCollection does not contain a document with the UserId {userId}");
            }
        }

    }





    [BsonIgnoreExtraElements]
    public class UserLevel
    {
        public ulong UserId { get; set; }
        public int Level { get; set; } = 0;
        public int Exp { get; set; } = 0;
        public int TotalExp { get; set; } = 0;
        public int MaxExp { get; set; } = 10;
        public bool HasDefaultProperties { get; set; } = true;

        public UserLevel(ulong userId, int level)
        {
            UserId = userId;
            Level = level;
        }

        public void Update()
        {

        }
    }
}
