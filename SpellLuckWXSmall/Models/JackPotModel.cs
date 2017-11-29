﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellLuckWXSmall.Models
{
    public class JackPotModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId JackPotID { get; set; }
        public List<AccountPotModel> Participator { get; set; }
        public GoodsModel JackGoods { get; set; }
      
        /// <summary>
        /// 0:等待加入，1：等待开奖，2：已开奖
        /// </summary>
        public int JackPotStatus { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind =DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
        public string JackPotPassword { get; set; }
        public int JackPotPeopleNum { get; set; }
        public decimal JackPotPrice { get; set; }
        public string Description { get; set; }
    }

    public class AccountPotModel
    {

        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountAvatar { get; set; }
        public string WXOrderId { get; set; }
        public string GoodsColor { get; set; }
        public string GoodsRule { get; set; }
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId PayWaitingID { get; set; }
    }
}
