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
        /// <returns></returns>
        public string GetGoodsList(int pageIndex, int goodsPayType)
        {
            BaseResponseModel<List<GoodsModel>> responseModel = new BaseResponseModel<List<GoodsModel>>();
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                var filter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsPayType, goodsPayType);
                var GoodsModelList = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(filter).Skip(pageIndex * pageSize).Limit(pageSize).ToList();
                if (GoodsModelList == null || GoodsModelList.Count == 0)
                {
                    responseModel.StatusCode = (int)ActionParams.code_error_null;
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
            BaseResponseModel<ResponseGoodsDetail> responseModel = new BaseResponseModel<ResponseGoodsDetail>();
            var goods = new MongoDBTool().GetMongoCollection<GoodsModel>().Find(x => x.GoodsID.Equals(new ObjectId(goodsID))).FirstOrDefault();
            if (goods == null)
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
                "AssessmentList",
                "AssessmentContent",
                "CreateTime",
                "AssessAccount",
                "AccountName",
                "AccountAvatar"});


            responseModel.JsonData = new ResponseGoodsDetail() { GoodsInfo = new JackPotModel() { JackGoods = goods } };
            return JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
        }

        private string GetJackPotGoodsDetail(string jackPotID)
        {
            BaseResponseModel<ResponseGoodsDetail> responseModel = new BaseResponseModel<ResponseGoodsDetail>();
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
                    "WaitingJackPotList",
                    "JackGoods",
                "AssessmentList",
                "AssessmentContent",
                "CreateTime",
                "AssessAccount",
                "AccountName",
                "JackPotID",
                "WaitingAccount",
                "AccountAvatar"});

            responseModel.JsonData = new ResponseGoodsDetail() { GoodsInfo = jackPot };

            return JsonConvert.SerializeObject(responseModel);
        }
    }
}
