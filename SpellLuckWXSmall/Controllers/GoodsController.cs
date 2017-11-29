using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpellLuckWXSmall.Models;
using MongoDB.Driver;
using Tools.DB;
using Tools.ResponseModels;
using Tools;
using Newtonsoft.Json;
using Tools.Json;
using MongoDB.Bson;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class GoodsController : Controller
    {
        const int pageSize = 5;
        /// <summary>
        /// 获取分页商品列表
        /// </summary>
        /// <param name="pageIndex">页码0-...</param>
        /// <param name="goodsPayType">商品交易类型：0.2人拼团，1.2-6人拼团，2.1分夺宝</param>
        /// <param name="searchParam">搜索关键字</param>
        /// <returns></returns>
        public string GetGoodsList(int pageIndex, int goodsPayType, string searchParam)
        {
            BaseResponseModel<List<GoodsModel>> responseModel = new BaseResponseModel<List<GoodsModel>>();
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                var filter = Builders<GoodsModel>.Filter;
                var filterSum = filter.Eq(x => x.GoodsPayType, goodsPayType);
                var filterSum2 = filter.Eq(x => x.GoodsPayType, goodsPayType) & filter.Regex(x => x.GoodsTitle, $"/{searchParam}/");
                var find = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(string.IsNullOrEmpty(searchParam) ? filterSum : filterSum2).Skip(pageIndex * pageSize);
                var count = find.Count();
                var pageSum = (count / pageSize) + (count % pageSize == 0 ? 0 : 1);
                List<GoodsModel> GoodsModelList = null;
                if (pageSum >= pageIndex + 1)
                {
                    GoodsModelList = find.Skip(pageIndex * pageSize).Limit(pageSize).ToList();
                    if (GoodsModelList == null || GoodsModelList.Count == 0)
                    {
                        responseModel.StatusCode = (int)ActionParams.code_error_null;
                    }
                }
                else
                {
                    responseModel.StatusCode = (int)ActionParams.code_null;
                }

                responseModel.JsonData = GoodsModelList;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
            }
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(new string[] {
                "StatusCode",
                "JsonData",
                "GoodsID",
                "GoodsTitle",
                "GoodsPrice",
                "GoodsSales",
                "GoodsListImage",
                "FileUrlData" });
            return JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
        }

      
        #region 获取商品信息
        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="goodsID">商品id</param>
        /// <param name="jackPotID">拼团id</param>
        /// <returns></returns>
        public string GetGoodsDetail(string goodsID, string jackPotID)
        {
            string json = null;
            if ((string.IsNullOrEmpty(goodsID) && string.IsNullOrEmpty(jackPotID)) || (!string.IsNullOrEmpty(goodsID) && !string.IsNullOrEmpty(jackPotID)))
            {
                return JsonConvert.SerializeObject(new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_error_null });
            }

            if (!string.IsNullOrEmpty(goodsID))
            {
                json = GetSimpleGoodsDetail(goodsID);
            }
            else if (!string.IsNullOrEmpty(jackPotID))
            {
                json = GetJackPotGoodsDetail(jackPotID);

            }
            return json;
        }

        private string GetSimpleGoodsDetail(string goodsID)
        {
            string json = "";
            BaseResponseModel<ResponseGoodsDetail> responseModel = new BaseResponseModel<ResponseGoodsDetail>();
            var goods = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(x => x.GoodsID.Equals(new ObjectId(goodsID))).FirstOrDefault();
            if (goods == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_null;
            }
            if (goods.GoodsPayType == 0)
            {
                var filter = Builders<JackPotModel>.Filter;
                var filterSum = filter.Eq(x => x.JackPotStatus, 0) & filter.Eq(x => x.JackGoods.GoodsID, goods.GoodsID);
                var jackpotList = new MongoDBTool().GetMongoCollection<JackPotModel>().Find(filterSum).ToList();
                var waitingJackPots = GetWaitingJackPots(jackpotList);
                responseModel.StatusCode = (int)ActionParams.code_ok;
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(
                    new string[] {
                    "StatusCode",
                    "JsonData",
                    "GoodsInfo",
                    "JackGoods",
                "GoodsID",
                "GoodsTitle",
                "GoodsColor",
                "GoodsRule",
                "GoodsDetail",
                "GoodsPrice",
                "GoodsOldPrice",
                "GoodsPayType",
                "GoodsMainImages",
                "GoodsSales",
                "GoodsOtherImages",
                "FileUrlData",
                "GoodsPeopleNum",
                "AssessmentList",
                "WaitingJackPotList",
                "JackPotID",
                "WaitingAccount",
                "AccountID",
                "WaitingJackPotList",
                "AssessmentContent",
                "AssessAccount",
                "AccountName",
                "AssessTime",
                "AccountAvatar"});
                responseModel.JsonData = new ResponseGoodsDetail() { GoodsInfo = new JackPotModel() { JackGoods = goods }, WaitingJackPotList = waitingJackPots };
                json = JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
            }
            else
            {
                responseModel.StatusCode = (int)ActionParams.code_ok;
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(
                    new string[] {
                    "StatusCode",
                    "JsonData",
                    "GoodsInfo",
                    "JackGoods",
                "GoodsID",
                "GoodsTitle",
                "GoodsColor",
                "GoodsRule",
                "GoodsDetail",
                "GoodsPrice",
                "GoodsOldPrice",
                "GoodsPayType",
                "GoodsMainImages",
                "GoodsSales",
                "GoodsOtherImages",
                "FileUrlData",
                "GoodsPeopleNum",
                "AssessmentList",
                "AssessmentContent",
                "AssessAccount",
                "AccountName",
                "AssessTime",
                "AccountAvatar"});
                responseModel.JsonData = new ResponseGoodsDetail() { GoodsInfo = new JackPotModel() { JackGoods = goods } };
                json = JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
            }
            return json;
        }

        private List<WaitingJackPot> GetWaitingJackPots(List<JackPotModel> jackpotList)
        {
            List<WaitingJackPot> list = new List<WaitingJackPot>();
            if (jackpotList != null)
            {
                foreach (var item in jackpotList)
                {
                    list.Add(new WaitingJackPot() { WaitingAccount = item.Participator[0], JackPotID = item.JackPotID });
                }
            }
            return list;
        }

        private string GetJackPotGoodsDetail(string jackPotID)
        {
            BaseResponseModel<ResponseGoodsDetail> responseModel = new BaseResponseModel<ResponseGoodsDetail>() { StatusCode = (int)ActionParams.code_ok };
            var jackPot = new MongoDBTool().GetMongoCollection<JackPotModel>().Find(x => x.JackPotID.Equals(new ObjectId(jackPotID))).FirstOrDefault();
            if (jackPot == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_null;
            }
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(
            new string[] {
                    "StatusCode",
                    "JsonData",
                    "GoodsInfo",
                    "JackGoods",
                "GoodsID",
                "GoodsTitle",
                "GoodsDetail",
                "GoodsPrice",
                "GoodsOldPrice",
                 "GoodsColor",
                "GoodsRule",
                "GoodsPayType",
                "GoodsMainImages",
                "GoodsSales",
                "GoodsOtherImages",
                "FileUrlData",
                "GoodsPeopleNum",
                "AssessmentList",
                "AssessmentContent",
                "AssessAccount",
                "AccountName",
                "AssessTime",
                "AccountAvatar",
            "WaitingJackPotList",
            "JackPotID",
            "WaitingAccount"});

            responseModel.JsonData = new ResponseGoodsDetail() { GoodsInfo = jackPot };

            return JsonConvert.SerializeObject(responseModel);
        }

        #endregion
        /// <summary>
        /// 添加评价
        /// </summary>
        /// <param name="accountID">账户ID</param>
        /// <param name="orderID">订单ID</param>
        /// <param name="assessmentContent">评价内容</param>
        /// <returns></returns>
        public string SaveAssessment(string accountID, string orderID, string assessmentContent)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>() { StatusCode = (int)ActionParams.code_ok };

            var mongo = new MongoDBTool();
            var account = mongo.GetMongoCollection<AccountModel>().Find(x => x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault();
            if (account == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return responseModel.ToJson();
            }
            var order = account.OrderList.Find(x => x.OrderID.Equals(new ObjectId(orderID)));
            if (order == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return responseModel.ToJson();

            }
            AssessmentModel assessmentModel = new AssessmentModel()
            {
                AssessmentID = ObjectId.GenerateNewId(),
                OrderID = order.OrderID,
                AssessmentContent = assessmentContent,
                AssessTime = DateTime.Now,
                AssessAccount = new AccountPotModel()
                {
                    AccountID = account.AccountID,
                    AccountAvatar = account.AccountAvatar,
                    AccountName = account.AccountName,
                    WXOrderId = order.WXOrderId,
                    GoodsColor = order.GoodsInfo.GoodsColor,
                    GoodsRule = order.GoodsInfo.GoodsRule
                }
            };
            var filter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsID, order.GoodsInfo.GoodsID);
            var update = Builders<GoodsModel>.Update.Push(x => x.AssessmentList, assessmentModel);
            var goods = mongo.GetMongoCollection<GoodsModel>().Find(filter).FirstOrDefault();
            var orderFilter = Builders<AccountModel>.Filter;
            var orderFilterSum = orderFilter.Eq(x => x.AccountID, account.AccountID) & orderFilter.Eq("OrderList.OrderID", order.OrderID);
            mongo.GetMongoCollection<AccountModel>().UpdateOne(orderFilterSum, Builders<AccountModel>.Update.Set("OrderList.$.OrderStatus", 3));
            if (goods.AssessmentList == null)
            {
                goods.AssessmentList = new List<AssessmentModel>();
                goods.AssessmentList.Add(assessmentModel);
                mongo.GetMongoCollection<GoodsModel>().UpdateOne(filter, Builders<GoodsModel>.Update.Set(x => x.AssessmentList, goods.AssessmentList));

            }
            else
            {
                mongo.GetMongoCollection<GoodsModel>().UpdateOne(filter, update);
            }
            return responseModel.ToJson();
        }

    }
}
