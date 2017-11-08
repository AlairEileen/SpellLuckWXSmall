using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace SpellLuckWXSmall.Models
{
    public class GoodsModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId GoodsID { get; set; }
        public string GoodsTitle { get; set; }
        public string GoodsDetail { get; set; }
        public int GoodsPrice { get; set; }
        public int GoodsPayType { get; set; }
        public long GoodsSales { get; set; }
        public FileModel<string[]> GoodsListImage { get; set; }
        public List<FileModel<string[]>> GoodsMainImages { get; set; }
        public List<FileModel<string[]>> GoodsOtherImages { get; set; }
        public long GoodsPeopleNum { get; set; }
    }
}
