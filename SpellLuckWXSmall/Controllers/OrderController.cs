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
        public string GetWaitingOrderList(string accountID, int orderStatus, int pageIndex)
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
                        orderList = orderList.FindAll(x => x.OrderStatus == orderStatus).Skip(pageIndex * AppConstData.MobilePageSize).Take(AppConstData.MobilePageSize).ToList();
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

    }
}
