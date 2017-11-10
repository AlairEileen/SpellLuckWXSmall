using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellLuckWXSmall.Models
{
    public class JackPotJoinWaitingModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId JackPotJoinWaitingID { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId GoodsID { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId AccountID { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId PayWaitingID { get; set; }
        public int SharaTimes { get => sharaTimes; set => sharaTimes = value; }

        private int sharaTimes = 3;
        public int currentSharaTimes { get; set; }
    }
}
