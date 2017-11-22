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
        public GoodsModel Goods { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId AccountID { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId PayWaitingID { get; set; }
        public int ShareTimes { get; set; }
        public string GoodsColor { get; set; }
        public string GoodsRule { get; set; }
        public string WXOrderId { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        public DateTime CreateTime { get; set; }
    }
}
