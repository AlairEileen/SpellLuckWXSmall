using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Models
{
    public class BaseAccount
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountPhoneNumber { get; set; }
        public int Gender { get; set; }
        public string AccountAvatar { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        public DateTime CreateTime { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        public DateTime LastChangeTime { get; set; }
    }
}
