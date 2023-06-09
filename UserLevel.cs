using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LevelSystemDiscordBot
{
    [BsonIgnoreExtraElements]
    public class UserLevel
    {
        public ulong UserId { get; set; }
        public int Level { get; set; } = 0;
        public int Exp { get; set; } = 0;
        public int TotalExp { get; set; } = 0;
        public int MaxExp { get; set; } = 10;

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
