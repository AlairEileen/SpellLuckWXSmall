using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tools.DB;
using SpellLuckWXSmall.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using Tools.ResponseModels;
using Tools;
using Tools.Json;
using WXSmallAppCommon.WXInteractions;
using SpellLuckWXSmall.AppData;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class OrderController : Controller
    {

        ///// <summary>
        ///// 查询订单列表
        ///// </summary>
        ///// <param name="waitType">0:待确认发货，1：待评价，2：待加入</param>
        ///// <returns></returns>
        //public string GetOrderList(string accountID, int waitType)
        //{
        //    string json = "";
        //    if (string.IsNullOrEmpty(accountID))
        //    {
        //        json = JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null });
        //    }
        //    try
        //    {

        //        var mongo = new MongoDBTool();
        //        if (waitType == 2)
        //        {
        //            var filter = Builders<JackPotModel>.Filter;
        //            var filterSum = filter.Eq("Participator.AccountID", new ObjectId(accountID)) & filter.Eq(x => x.JackPotStatus, 0);
        //            var list = mongo.GetMongoCollection<JackPotModel>().Find(filterSum).ToList();
        //            json = JsonConvert.SerializeObject(new BaseResponseModel<List<JackPotModel>>() { JsonData = list, StatusCode = (int)ActionParams.code_ok });
        //        }
        //        else
        //        {
        //            var account = mongo.GetMongoCollection<AccountModel>().Find(x => x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault();
        //            var list = account.OrderList.FindAll(x => x.OrderStatus == waitType);
        //            json = JsonConvert.SerializeObject(new BaseResponseModel<List<OrderModel>>() { JsonData = list, StatusCode = (int)ActionParams.code_ok });
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        json = JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error });
        //    }
        //    return json;
        //}
        JackPotData jpd = new JackPotData();
        private IHostingEnvironment hostingEnvironment;

        public OrderController(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }

        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <returns></returns>
        public string GetOrderList(string accountID)
        {
            try
            {
                var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                var account = new MongoDBTool().GetMongoCollection<AccountModel>().Find(filter).FirstOrDefault();
                var jackpotList = jpd.GetAllWaitJackPot(accountID);
                if (account.OrderList != null)
                {
                    foreach (var item in account.OrderList)
                    {
                        jackpotList.Add(new JackPotModel()
                        {
                            CreateTime = item.CreateTime,
                            Description = item.OrderStatusText,
                            JackGoods = new GoodsModel()
                            {
                                GoodsID = item.GoodsInfo.GoodsID,
                                GoodsColor = new List<string>() { item.GoodsInfo.GoodsColor },
                                GoodsRule = new List<string>() { item.GoodsInfo.GoodsRule },
                                GoodsListImage = item.GoodsInfo.GoodsListImage,
                                GoodsPayType = item.GoodsInfo.GoodsPayType,
                                GoodsPrice = item.GoodsInfo.GoodsPrice,
                                GoodsTitle = item.GoodsInfo.GoodsTitle
                            }
                        });
                    }
                }
                jackpotList.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));
                return new BaseResponseModel<List<JackPotModel>>() { StatusCode = (int)ActionParams.code_ok, JsonData = jackpotList }.ToJson(); ;
            }
            catch (Exception)
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                throw;
            }
        }

        private string GetOrderStatusText(int orderStatus, bool isRefound, bool hasRefoundByCompany)
        {

            switch (orderStatus)
            {
                case -1:
                    return "未获奖";
                case 0:
                    if (isRefound && !hasRefoundByCompany)
                    {
                        return "等待商家退款";
                    }
                    else if (isRefound && hasRefoundByCompany)
                    {
                        return "退款完成";
                    }
                    return "待确认发货";
                case 1:
                    return "待商家发货";
                case 2:
                    if (isRefound && !hasRefoundByCompany)
                    {
                        return "等待商家退款";
                    }
                    return "待评价";
                case 3:
                    if (isRefound && hasRefoundByCompany)
                    {
                        return "退款完成";
                    }
                    return "完成";
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 获取待发货、待评价列表
        /// </summary>
        /// <param name="accountID">账户Id</param>
        /// <param name="orderStatus">0：待发货，1：待评价</param>
        /// <param name="pageIndex">页码（从0开始）</param>
        /// <returns></returns>
        public string GetWaitingOrderList(string accountID, int orderStatus)
        {
            if (string.IsNullOrEmpty(accountID))
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null }.ToJson();
            }
            string json = "";
            try
            {
                var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                //var filterSum = filter.Eq(x => x.JackPotStatus, 0) & filter.Eq("Participator.AccountID", new ObjectId(accountID));
                var account = new MongoDBTool().GetMongoCollection<AccountModel>().Find(filter).FirstOrDefault();
                if (account == null)
                {
                    json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_null }.ToJson();
                    return json;
                }
                if (account.OrderList != null)
                {

                    var orderList = account.OrderList.OrderByDescending(x => x.CreateTime).ToList();
                    if (orderList != null)
                    {
                        if (orderStatus == 0)
                        {
                            orderList = orderList.FindAll(x => x.OrderStatus == (int)OrderStatusType.WaitAgreeSendGoods);
                        }
                        else
                        {
                            orderList = orderList.FindAll(x => x.OrderStatus == (int)OrderStatusType.WaitAssess);
                        }
                    }
                    json = new BaseResponseModel<List<OrderModel>>() { StatusCode = (int)ActionParams.code_ok, JsonData = orderList }.ToJson();
                }
                else
                {
                    json = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_null }.ToJson();
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
        /// 后台 获取订单信息和个人信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string GetOrderAndPersonInfo(string orderId)
        {
            var filter = Builders<AccountModel>.Filter;
            var filterSum = filter.Eq("OrderList.OrderID", new ObjectId(orderId));
            var account = new MongoDBTool().GetMongoCollection<AccountModel>().Find(filterSum).FirstOrDefault();

            if (account != null)
            {
                var order = account.OrderList.Find(x => x.OrderID.Equals(new ObjectId(orderId)));
                return new BaseResponseModel2<OrderLocation, OrderModel>() { StatusCode = (int)ActionParams.code_ok, JsonData1 = account.OrderLocation, JsonData2 = order }.ToJson();
            }

            return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
        }

        /// <summary>
        /// 获取获奖者ID
        /// </summary>
        /// <param name="accountID">账号id</param>
        /// <param name="wXOrderId">微信订单号</param>
        /// <returns></returns>
        public string GetWinnerWithAccount(string accountID, string wXOrderId)
        {
            var responseModel = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_ok };
            string json = responseModel.ToJson();
            try
            {
                var mongo = new MongoDBTool();
                var filter = Builders<AccountModel>.Filter;
                var filterSum = filter.Eq("OrderList.WXOrderId", wXOrderId) & filter.Eq("OrderList.OrderStatus", 0);
                var account = mongo.GetMongoCollection<AccountModel>().Find(filterSum).FirstOrDefault();
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(
                    new string[] {
                    "StatusCode",
                    "JsonData1",
                    "JsonData2" ,
                    "AccountAvatar",
                    "AccountName",
                    "AccountID"});
                var response = JsonConvert.SerializeObject(new BaseResponseModel2<bool, AccountModel>() { StatusCode = (int)ActionParams.code_ok, JsonData1 = account.AccountID.Equals(new ObjectId(accountID)), JsonData2 = account }, jsonSerializerSettings);
                return response;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                json = responseModel.ToJson();
            }
            return json;
        }

        /// <summary>
        /// 后台 发货
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="trackingCompany">快递公司</param>
        /// <param name="trackingNumber">运单号</param>
        /// <returns></returns>
        public string SendOrderByTrackingCompany(string orderID, string trackingCompany, string trackingNumber)
        {
            try
            {
                var filter = Builders<AccountModel>.Filter;
                var filterSum = filter.Eq("OrderList.OrderID", new ObjectId(orderID));
                var update = Builders<AccountModel>.Update
                    .Set("OrderList.$.TrackingNumber", trackingNumber)
                    .Set("OrderList.$.TrackingCompany", trackingCompany)
                    .Set("OrderList.$.OrderStatus", (int)OrderStatusType.WaitAssess);
                new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(filterSum, update);
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_ok }.ToJson();
            }
            catch (Exception)
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();

                throw;
            }
        }

        /// <summary>
        /// 用户确认发货
        /// </summary>
        /// <param name="orderID">订单号</param>
        /// <returns></returns>
        public string AgreeSendGoods(string orderID)
        {
            try
            {
                new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(Builders<AccountModel>.Filter.Eq("OrderList.OrderID", new ObjectId(orderID)), Builders<AccountModel>.Update.Set("OrderList.$.OrderStatus", (int)OrderStatusType.WaitCompanySendGoods));
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_ok }.ToJson();
            }
            catch (Exception)
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();

                throw;
            }
        }

        /// <summary>
        /// 用户同意退款
        /// </summary>
        /// <param name="accountID">账户id</param>
        /// <param name="orderID">订单号</param>
        /// <returns></returns>
        public string AgreeRefund(string accountID, string orderID)
        {
            try
            {
                var collection = new MongoDBTool().GetMongoCollection<AccountModel>();
                var account = collection.Find(Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID))).FirstOrDefault();
                var order = account.OrderList.Find(x => x.OrderID.Equals(new ObjectId(orderID)));
                Refund.Run(order.WXOrderId, order.OrderNumber, order.OrderPrice.ConvertToMoneyCent(), order.OrderPrice.ConvertToMoneyCent());
                collection.UpdateOne(Builders<AccountModel>.Filter.Eq("OrderList.OrderID", new ObjectId(orderID)), Builders<AccountModel>.Update.Set("OrderList.$.isRefound", true).Set("OrderList.$.OrderStatus", (int)OrderStatusType.WaitCompanyRefund));
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_ok }.ToJson();

            }
            catch (Exception)
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                throw;
            }
        }

        /// <summary>
        /// 后台 获取退款信息
        /// </summary>
        /// <param name="orderID">订单ID</param>
        /// <returns></returns>
        public string GetRefundOrderByCompany(string orderID)
        {
            try
            {
                var account = new MongoDBTool().GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Eq("OrderList.OrderID", new ObjectId(orderID))).FirstOrDefault();
                var order = account.OrderList.Find(x => x.OrderID.Equals(new ObjectId(orderID)));
                return new BaseResponseModel3<OrderModel, string, decimal>()
                {
                    StatusCode = (int)ActionParams.code_ok,
                    JsonData = order,
                    JsonData1 = "目前商户号只能退给用户支付订单的最大金额，剩余部分请商户联系用户主动（通过微信或者支付宝等）打给用户",
                    JsonData2 = order.GoodsInfo.GoodsPrice - order.OrderPrice
                }.ToJson();
            }
            catch (Exception)
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();
                throw;
            }
        }

        /// <summary>
        /// 后台 商户确认退款
        /// </summary>
        /// <param name="orderID">订单ID</param>
        /// <returns></returns>
        public string AgreeRefundOrderByCompany(string orderID)
        {

            try
            {
                var account = new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(
                    Builders<AccountModel>.Filter.Eq("OrderList.OrderID", new ObjectId(orderID)), Builders<AccountModel>.Update.Set("OrderList.OrderStatus", (int)OrderStatusType.WaitAssess)
                    );
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_ok }.ToJson();
            }
            catch (Exception)
            {
                return new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error }.ToJson();

                throw;
            }


        }
    }
}
