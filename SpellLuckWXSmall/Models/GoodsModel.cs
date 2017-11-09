using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Range(1, 100)]
        [DataType(DataType.Currency)]
        public decimal GoodsPrice { get; set; }
        [Range(1, 100)]
        [DataType(DataType.Currency)]
        public decimal GoodsOldPrice { get; set; }
        public int GoodsPayType { get; set; }
        public long GoodsSales { get; set; }
        public FileModel<string[]> GoodsListImage { get; set; }
        public List<FileModel<string[]>> GoodsMainImages { get; set; }
        public List<FileModel<string[]>> GoodsOtherImages { get; set; }
        public long GoodsPeopleNum { get; set; }
        public List<Assessment> AssessmentList { get; set; }
        public List<string> GoodsColor { get; set; }
        public List<string> GoodsRule { get; set; }
    }

    public class Assessment
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId AssessmentID { get; set; }
        public ObjectId OrderID { get; set; }
        public string AssessmentContent { get; set; }
        public DateTime AssessTime { get; set; }
        public AccountPotModel AssessAccount { get; set; }
    }
}
