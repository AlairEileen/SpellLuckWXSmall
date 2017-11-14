﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace SpellLuckWXSmall.Models
{
    public class OrderModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId OrderID { get; set; }
        public OrderGoodsInfo GoodsInfo { get; set; }
        public decimal OrderPrice { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 0:待确认发货，1：待评价，2：结束
        /// </summary>
        public int OrderStatus { get; set; }
        public string WXOrderId { get; set; }
    }

    public class OrderGoodsInfo
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId GoodsID { get; set; }
        public string GoodsTitle { get; set; }
        public decimal GoodsPrice { get; set; }
        public int GoodsPayType { get; set; }
        public FileModel<string[]> GoodsListImage { get; set; }
        public long GoodsPeopleNum { get; set; }
        public string GoodsColor { get; set; }
        public string GoodsRule { get; set; }
    }
}
