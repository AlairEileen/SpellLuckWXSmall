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
                    var account = mongo.GetMongoCollection<AccountModel>().Find(x=>x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault();
                    var list = account.OrderList.FindAll(x=>x.OrderStatus==waitType);
                    json = JsonConvert.SerializeObject(new BaseResponseModel<List<OrderModel>>() { JsonData = list, StatusCode = (int)ActionParams.code_ok });
                }
            }
            catch (Exception)
            {

                json = JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error });
            }
            return json;
        }
    }
}
