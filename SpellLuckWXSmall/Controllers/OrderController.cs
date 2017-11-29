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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class OrderController : Controller
    {

        /// <summary>
        /// 查询订单列表
        /// </summary>
        /// <param name="waitType">0:待确认发货，1：待评价，2：待加入</param>
        /// <returns></returns>
        public string GetOrderList(string accountID, int waitType)
        {
            string json = "";
            if (string.IsNullOrEmpty(accountID))
            {
                json = JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null });
            }
            try
            {

                var mongo = new MongoDBTool();
                if (waitType == 2)
                {
                    var filter = Builders<JackPotModel>.Filter;
                    var filterSum = filter.Eq("Participator.AccountID", new ObjectId(accountID)) & filter.Eq(x => x.JackPotStatus, 0);
                    var list = mongo.GetMongoCollection<JackPotModel>().Find(filterSum).ToList();
                    json = JsonConvert.SerializeObject(new BaseResponseModel<List<JackPotModel>>() { JsonData = list, StatusCode = (int)ActionParams.code_ok });
                }
                else
                {
                    var account = mongo.GetMongoCollection<AccountModel>().Find(x => x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault();
                    var list = account.OrderList.FindAll(x => x.OrderStatus == waitType);
                    json = JsonConvert.SerializeObject(new BaseResponseModel<List<OrderModel>>() { JsonData = list, StatusCode = (int)ActionParams.code_ok });
                }
            }
            catch (Exception)
            {

                json = JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error });
            }
            return json;
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
                        orderList = orderList.FindAll(x => x.OrderStatus == orderStatus);
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
    }
}
