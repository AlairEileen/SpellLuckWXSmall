using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellLuckWXSmall.Models
{
    public class TestModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId TestID { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind =DateTimeKind.Local)]
        public DateTime CDate { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind =DateTimeKind.Local)]
        public DateTime MDate { get; set; }
    }
}
