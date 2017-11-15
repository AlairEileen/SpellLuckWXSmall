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
using Tools;
using Tools.ResponseModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class JackPotController : Controller
    {
        /// <summary>
        /// 请求参奖
        /// </summary>
        /// <param name="accountID">账户Id</param>
        /// <param name="goodsID">商品id（与jackPotID只能有一个，有goodsID为一分夺宝或者开团）</param>
        /// <param name="jackPotID">奖池Id（与goodsId只能有一个，有jackPotID为拼团）</param>
        /// <param name="goodsColor">商品颜色</param>
        /// <param name="goodsRule">商品尺寸</param>
        /// <param name="jackPotPassword">参团密码（3-6人开团可选项目）</param>
        /// <returns></returns>
        public string RequestCreateJackPot(string accountID, string goodsID, string jackPotID, string goodsColor, string goodsRule, string jackPotPassword)
        {
            if (string.IsNullOrEmpty(accountID) || (string.IsNullOrEmpty(goodsID) && string.IsNullOrEmpty(jackPotID)) || string.IsNullOrEmpty(goodsColor) || string.IsNullOrEmpty(goodsRule))
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null }.ToJson();
            }
            string json = "";
            try
            {
                var mongo = new MongoDBTool();
                var accountFilter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                var account = mongo.GetMongoCollection<AccountModel>().Find(accountFilter).FirstOrDefault();

                JackPotModel jackPot = null;
                GoodsModel goods = null;
                if (!string.IsNullOrEmpty(jackPotID))
                {
                    var jackPotFilter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotID, new ObjectId(jackPotID));
                    jackPot = mongo.GetMongoCollection<JackPotModel>().Find(jackPotFilter).FirstOrDefault();

                }
                else if (!string.IsNullOrEmpty(goodsID))
                {
                    var goodsFilter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsID, new ObjectId(goodsID));
                    goods = mongo.GetMongoCollection<GoodsModel>().Find(goodsFilter).FirstOrDefault();
                }

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
                    GoodsColor = goodsColor,
                    JackPotKey = jackPotPassword,
                    GoodsRule = goodsRule

                };
                mongo.GetMongoCollection<PayWaitingModel>().InsertOne(payWaitingModel);
                JsApiPay jsApiPay = new JsApiPay();
                jsApiPay.openid = account.OpenID;
                jsApiPay.total_fee = goods != null ? goods.GoodsPrice.ConvertToMoneyCent() : jackPot.JackGoods.GoodsPrice.ConvertToMoneyCent();
                var body = "test";
                var attach = JsonConvert.SerializeObject(payWaitingModel.PayWaitingID);
                var goods_tag = goods != null ? goods.GoodsTitle : jackPot.JackGoods.GoodsTitle;
                jsApiPay.GetUnifiedOrderResult(body, attach, goods_tag);
                var param = jsApiPay.GetJsApiParameters();
                json = param;
            }
            catch (Exception ex)
            {
                json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                Console.WriteLine("请求支付时出错！" + ex.Message);
            }

            return json;
        }
    }
}
