using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WXSmallAppCommon.Models;

namespace SpellLuckWXSmall.Models
{
    public class PayWaitingModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId PayWaitingID { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId AccountID { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId GoodsID { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId JackPotID { get; set; }
        public string JackPotKey { get; set; }
        public string GoodsColor { get; set; }
        public string GoodsRule { get; set; }
        public string WXOrderId { get; set; }
        public int JackPotPeopleNum { get; set; }
        public decimal JackPotPrice { get; set; }
        public WXPayModel WXPayData { get; set; }
        public bool isDisabled { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        public DateTime CreateTime { get; set; }
    }
}
