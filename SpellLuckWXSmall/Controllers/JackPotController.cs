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
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class JackPotController : Controller
    {
        /// <summary>
        /// 请求参奖
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="goodsID"></param>
        /// <param name="jackPotID"></param>
        /// <param name="goodsColor"></param>
        /// <param name="goodsRule"></param>
        /// <returns></returns>
        public string RequestCreateJackPot(string accountID, string goodsID, string jackPotID,string goodsColor,string goodsRule)
        {
            string param = "";
            try
            {
                var mongo = new MongoDBTool();
                var accountFilter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                var goodsFilter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsID, new ObjectId(goodsID));
                var jackPotFilter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotID, new ObjectId(jackPotID));
                var account = mongo.GetMongoCollection<AccountModel>().Find(accountFilter).FirstOrDefault();
                var goods = mongo.GetMongoCollection<GoodsModel>().Find(goodsFilter).FirstOrDefault();
                var jackPot = mongo.GetMongoCollection<JackPotModel>().Find(jackPotFilter).FirstOrDefault();
                //OrderModel orderModel = new OrderModel() {
                //    OrderID = ObjectId.GenerateNewId(),
                //    OrderStatus = 0,
                //    OrderPrice = goods.GoodsPrice,
                //    CreateTime = DateTime.Now,
                //    GoodsInfo = new OrderGoodsInfo()
                //    {
                //        GoodsPrice = goods.GoodsPrice,
                //        GoodsID = goods.GoodsID,
                //        GoodsListImage = goods.GoodsListImage,
                //        GoodsPayType = goods.GoodsPayType,
                //        GoodsPeopleNum = goods.GoodsPeopleNum,
                //        GoodsTitle=goods.GoodsTitle
                //    }

                //};
                PayWaitingModel payWaitingModel = new PayWaitingModel()
                {
                    AccountID = account.AccountID,
                    GoodsID = goods != null ? goods.GoodsID : ObjectId.Empty,
                    JackPotID = jackPot != null ? jackPot.JackPotID : ObjectId.Empty,
                    GoodsColor=goodsColor,
                    GoodsRule=goodsRule
                };
                mongo.GetMongoCollection<PayWaitingModel>().InsertOne(payWaitingModel);
                JsApiPay jsApiPay = new JsApiPay();
                jsApiPay.openid = account.OpenID;
                jsApiPay.total_fee = goods.GoodsPrice;
                var body = "test";
                var attach = JsonConvert.SerializeObject(payWaitingModel);
                var goods_tag = goods.GoodsTitle;
                jsApiPay.GetUnifiedOrderResult(body, attach, goods_tag);
                param = jsApiPay.GetJsApiParameters();
            }
            catch (Exception ex)
            {

                Console.WriteLine("请求支付时出错！" + ex.Message);
            }
            return param;
        }
    }
}
