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
using System.Globalization;
using WXSmallAppCommon.Models;
using Tools.Json;

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
        /// <param name="jackPotPeopleNum">参团人数（3-6人开团可选项目）</param>
        /// <returns></returns>
        public string RequestCreateJackPot(string accountID, string goodsID, string jackPotID, string goodsColor, string goodsRule, string jackPotPassword, int jackPotPeopleNum)
        {
            if (string.IsNullOrEmpty(accountID) ||
                (string.IsNullOrEmpty(goodsID) && string.IsNullOrEmpty(jackPotID)) ||
                string.IsNullOrEmpty(goodsColor) || string.IsNullOrEmpty(goodsRule))
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
                    if (jackPot != null)
                    {
                        if (!jackPot.JackPotPassword.Equals(jackPotPassword) ||
                            jackPot.JackPotPeopleNum == jackPot.Participator.Count)
                        {
                            return new BaseResponseModel<string>()
                            {
                                StatusCode = (int)ActionParams.code_error_verify,
                                JsonData = "团满或密码有误"
                            }.ToJson();
                        }
                        if (jackPot.Participator.Exists(x => x.AccountID.Equals(account.AccountID)))
                        {
                            return new BaseResponseModel<string>()
                            {
                                StatusCode = (int)ActionParams.code_error_exists,
                                JsonData = "你已参此团"
                            }.ToJson();

                        }
                    }

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
                int peopleNum = 0;
                decimal price = 0;
                if (goods != null)
                {
                    switch (goods.GoodsPayType)
                    {
                        case 0:
                            peopleNum = 2;
                            price = GetJackPotPrice(goods.GoodsPrice, peopleNum);
                            break;
                        case 1:
                            peopleNum = jackPotPeopleNum;
                            price = GetJackPotPrice(goods.GoodsPrice, peopleNum);
                            break;
                        case 2:
                        default:
                            peopleNum = 0;
                            price = goods.GoodsPrice;
                            break;
                    }
                }
                else if (jackPot != null)
                {
                    peopleNum = jackPot.JackPotPeopleNum;
                    price = GetJackPotPrice(jackPot.JackGoods.GoodsPrice, peopleNum);
                }


                PayWaitingModel payWaitingModel = new PayWaitingModel()
                {
                    AccountID = account.AccountID,
                    GoodsID = goods != null ? goods.GoodsID : ObjectId.Empty,
                    JackPotID = jackPot != null ? jackPot.JackPotID : ObjectId.Empty,
                    GoodsColor = goodsColor,
                    JackPotKey = jackPotPassword,
                    JackPotPrice = price,
                    JackPotPeopleNum = peopleNum,
                    GoodsRule = goodsRule,
                    CreateTime = DateTime.Now
                };
                mongo.GetMongoCollection<PayWaitingModel>().InsertOne(payWaitingModel);
                JsApiPay jsApiPay = new JsApiPay();
                jsApiPay.openid = account.OpenID;
                jsApiPay.total_fee = payWaitingModel.JackPotPrice.ConvertToMoneyCent();
                var body = "test";
                var attach = payWaitingModel.PayWaitingID.ToString();
                var goods_tag = goods != null ? goods.GoodsTitle : jackPot.JackGoods.GoodsTitle;
                jsApiPay.GetUnifiedOrderResult(body, attach, goods_tag);
                var param = jsApiPay.GetJsApiParameters();
                if (!string.IsNullOrEmpty(param))
                {
                    payWaitingModel.WXPayData = JsonConvert.DeserializeObject<WXPayModel>(param);
                }

                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(
                    new string[] {
                    "StatusCode",
                    "JsonData",
                    "PayWaitingID" ,
                    "WXPayData",
                    "appId",
                    "nonceStr",
                "package",
                "paySign",
                "signType",
                "timeStamp"});


                json = JsonConvert.SerializeObject(new BaseResponseModel<PayWaitingModel>() { JsonData = payWaitingModel, StatusCode = (int)ActionParams.code_ok }, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                Console.WriteLine("请求支付时出错！" + ex.Message);
            }

            return json;
        }

        /// <summary>
        /// 获取团价
        /// </summary>
        /// <param name="goodsPrice"></param>
        /// <param name="peopleNum"></param>
        /// <returns></returns>
        public static decimal GetJackPotPrice(decimal goodsPrice, int peopleNum)
        {
            decimal bb = Math.Ceiling((decimal)goodsPrice.ConvertToMoneyCent() / (decimal)peopleNum) / 100;
            Console.WriteLine("bb:{0},goodsprice:{1}", bb, goodsPrice);
            return bb;
        }
        /// <summary>
        /// 根据人数获取团价
        /// </summary>
        /// <param name="goodsID">商品id</param>
        /// <param name="peopleNum">人数</param>
        /// <returns></returns>
        public string GetJackPotPrice(string goodsID, int peopleNum)
        {
            var responseModel = new BaseResponseModel<decimal>() { StatusCode = (int)ActionParams.code_ok };
            try
            {
                var goods = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(x => x.GoodsID.Equals(new ObjectId(goodsID))).FirstOrDefault();
                responseModel.JsonData = GetJackPotPrice(goods.GoodsPrice, peopleNum);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                throw;
            }
            return JsonConvert.SerializeObject(responseModel);
        }
        /// <summary>
        /// 查找待拼团列表
        /// </summary>
        /// <param name="accountID">账户Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public string GetWaitingJoinJackPotList(string accountID, int pageIndex)
        {

            if (string.IsNullOrEmpty(accountID))
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null }.ToJson();
            }
            string json = "";
            try
            {
                ///拼团商品
                var filter = Builders<JackPotModel>.Filter;
                var filterSum = filter.Eq(x => x.JackPotStatus, 0) & filter.Eq("Participator.AccountID", new ObjectId(accountID));
                var listJackPot = new MongoDBTool().GetMongoCollection<JackPotModel>().Find(filterSum).ToList();

                ///一分夺宝列表
                var waitingFilter = Builders<JackPotJoinWaitingModel>.Filter;
                var waitingFilterSum = waitingFilter.Eq(x => x.AccountID, new ObjectId(accountID)) & waitingFilter.Gt(x => x.ShareTimes, AppConstData.SharaMinAdd);
                var listWaiting = new MongoDBTool().GetMongoCollection<JackPotJoinWaitingModel>().Find(waitingFilterSum).ToList();
                if (listWaiting != null && listWaiting.Count != 0)
                {
                    var waitingJackPot = new List<JackPotModel>();
                    foreach (var item in listWaiting)
                    {
                        waitingJackPot.Add(new JackPotModel() { JackGoods = item.Goods });
                    }
                    listJackPot.AddRange(waitingJackPot);
                }
                listJackPot.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));
                var list = listJackPot.Skip(pageIndex * AppConstData.MobilePageSize).Take(AppConstData.MobilePageSize).ToList();

                json = new BaseResponseModel<List<JackPotModel>>() { StatusCode = (int)ActionParams.code_ok, JsonData = list }.ToJson();
            }
            catch (Exception)
            {
                json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                throw;
            }
            return json;
        }

        /// <summary>
        /// 添加转发一分夺宝次数
        /// </summary>
        /// <param name="jackPotJoinWaitingID">等待id</param>
        /// <param name="shareTimes">转发次数</param>
        /// <returns></returns>
        public string PutSharaTimes(string jackPotJoinWaitingID, int shareTimes)
        {
            if (string.IsNullOrEmpty(jackPotJoinWaitingID))
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null }.ToJson();
            }
            string json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_ok }.ToJson();
            try
            {
                var mongo = new MongoDBTool();
                var jackpotWait = mongo.GetMongoCollection<JackPotJoinWaitingModel>()
                    .Find(x => x.JackPotJoinWaitingID.Equals(new ObjectId(jackPotJoinWaitingID))).FirstOrDefault();
                jackpotWait.ShareTimes += shareTimes;
                if (jackpotWait.ShareTimes >= AppConstData.SharaMinAdd)
                {
                    var filter = Builders<JackPotModel>.Filter;
                    var filterSum = filter.Eq(x => x.JackGoods.GoodsID, jackpotWait.Goods.GoodsID) & filter.Eq(x => x.JackPotStatus, 0);
                    var jackPot = mongo.GetMongoCollection<JackPotModel>().Find(filterSum).FirstOrDefault();
                    var account = mongo.GetMongoCollection<AccountModel>().Find(x => x.AccountID.Equals(jackpotWait.AccountID)).FirstOrDefault();
                    var accountPot = new AccountPotModel()
                    {
                        AccountID = account.AccountID,
                        AccountName = account.AccountName,
                        AccountAvatar = account.AccountAvatar,
                        GoodsColor = jackpotWait.GoodsColor,
                        GoodsRule = jackpotWait.GoodsRule,
                        WXOrderId = jackpotWait.WXOrderId
                    };
                    if (jackPot == null)
                    {
                        mongo.GetMongoCollection<JackPotModel>().InsertOne(new JackPotModel()
                        {
                            CreateTime = DateTime.Now,
                            JackGoods = jackpotWait.Goods,
                            JackPotStatus = 0,
                            Participator = new List<AccountPotModel>() { accountPot },
                            PayWaitingID = jackpotWait.PayWaitingID
                        });
                    }
                    else
                    {
                        mongo.GetMongoCollection<JackPotModel>().UpdateOne(filterSum, Builders<JackPotModel>.Update.Push(x => x.Participator, accountPot));
                    }
                }
                else
                {
                    mongo.GetMongoCollection<JackPotJoinWaitingModel>().UpdateOne(Builders<JackPotJoinWaitingModel>.
                        Filter.Eq(x => x.JackPotJoinWaitingID, jackpotWait.JackPotJoinWaitingID),
                        Builders<JackPotJoinWaitingModel>.Update.Set(x => x.ShareTimes, jackpotWait.ShareTimes));
                }
            }
            catch (Exception)
            {
                json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                throw;
            }
            return json;
        }

        /// <summary>
        /// 获取拼团分享ID以及等待人列表
        /// </summary>
        /// <param name="payWaitingID">支付前的支付ID</param>
        /// <returns></returns>
        public string GetShareJackPotID(string payWaitingID)
        {
            if (string.IsNullOrEmpty(payWaitingID))
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null }.ToJson();
            }

            string json = "";
            try
            {
                ///获取拼团ID
                var mongo = new MongoDBTool();
                var jackpot = mongo.GetMongoCollection<JackPotModel>().Find(x => x.PayWaitingID.Equals(new ObjectId(payWaitingID))).FirstOrDefault();
                var response = new BaseResponseModel<JackPotModel>()
                {
                    StatusCode = jackpot == null ? (int)ActionParams.code_null : (int)ActionParams.code_ok,
                    JsonData = jackpot
                };
                json = response.ToJson();
            }
            catch (Exception)
            {
                json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                throw;
            }

            return json;
        }


    }
}
