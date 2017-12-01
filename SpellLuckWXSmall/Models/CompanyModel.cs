﻿using MongoDB.Bson;
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
        public List<CompanyAccountModel> CompanyAccountList { get; set; }
        public TimeOpenJack TimeOpenJack { get; set; }
    }

    public class TimeOpenJack
    {
        public int JackPotTimerHour { get; set; }
        public int JackPotTimerMinute { get; set; }
    }

    public class CompanyAccountModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId CompanyAccountID { get; set; }
        public string CompanyAccountName { get; set; }
        public string CompanyAccountPassword { get; set; }
        [BsonIgnore]
        public string CompanyAccountVerifyPassword { get; set; }
        public string CompanyAccountOlderPassword { get; set; }
        public string Token { get; set; }
        [BsonDateTimeOptions(Kind =DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
        [BsonDateTimeOptions(Kind =DateTimeKind.Local)]
        public DateTime LastLoginTime { get; set; }
    }

}
