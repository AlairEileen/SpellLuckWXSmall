using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellLuckWXSmall.Models
{
    public class ResponseGoodsDetail
    {
        public JackPotModel GoodsInfo { get; set; }
        public List<WaitingJackPot> WaitingJackPotList { get; set; }
    }

    public class WaitingJackPot
    {
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId JackPotID { get; set; }
        public AccountPotModel WaitingAccount { get; set; }
    }
}
