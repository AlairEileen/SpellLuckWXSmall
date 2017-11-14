using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellLuckWXSmall.Models
{
    public class CompanyModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string ServicePhone { get; set; }
    }
}
