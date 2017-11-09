using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WXSmallAppCommon.WXInteractions;
using Tools.DB;
using SpellLuckWXSmall.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using WXSmallAppCommon.WXTool;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class JackPotController : Controller
    {

        public string RequestCreateJackPot(string accountID, string goodsID)
        {

            var mongo = new MongoDBTool();
            var accountFilter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
            var goodsFilter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsID, new ObjectId(goodsID));
            var account = mongo.GetMongoCollection<AccountModel>().Find(accountFilter).FirstOrDefault();
            var goods = mongo.GetMongoCollection<GoodsModel>().Find(goodsFilter).FirstOrDefault();
            JsApiPay jsApiPay = new JsApiPay();
            jsApiPay.openid = account.OpenID;
            jsApiPay.total_fee = goods.GoodsPrice;
            var body = "test";
            var attach = "test";
            var goods_tag = goods.GoodsTitle;
            jsApiPay.GetUnifiedOrderResult(body, attach, goods_tag);
            return jsApiPay.GetJsApiParameters();
        }
    }
}
