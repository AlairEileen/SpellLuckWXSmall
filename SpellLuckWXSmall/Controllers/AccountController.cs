using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SpellLuckWXSmall.Models;
using System;
using System.IO;
using System.Net;
using Tools;
using Tools.DB;
using Tools.Json;
using Tools.ResponseModels;
using Tools.Strings;
using WXSmallAppCommon.WXInteractions;

namespace SpellLuckWXSmall.Controllers
{
    public class AccountController : Controller
    {

        /// <summary>
        /// 请求登录
        /// </summary>
        /// <param name="code"></param>
        /// <param name="iv"></param>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        [HttpGet]
        public string RequestLogin(string code, string iv, string encryptedData)
        {
            BaseResponseModel<AccountModel> responseModel = new BaseResponseModel<AccountModel>();

            AccountModel accountCard = null;
            WXSmallAppCommon.Models.WXAccountInfo wXAccount = WXSmallAppCommon.WXInteractions.WXLoginAction.ProcessRequest(code, iv, encryptedData);
            if (wXAccount.OpenId != null)
            {
                var filter = Builders<AccountModel>.Filter.And(Builders<AccountModel>.Filter.Eq(x => x.OpenID, wXAccount.OpenId));
                var collection = new MongoDBTool().GetMongoCollection<AccountModel>();
                var update = Builders<AccountModel>.Update.Set(x => x.LastChangeTime, DateTime.Now);
                accountCard = collection.FindOneAndUpdate<AccountModel>(filter, update);

                if (accountCard == null)
                {
                    int gender = wXAccount.Gender == 1 ? 1 : wXAccount.Gender == 2 ? 2 : 3;
                    //string avatarUrl = DownloadAvatar(wXAccount.AvatarUrl, wXAccount.OpenId);
                    string avatarUrl = wXAccount.AvatarUrl;
                    accountCard = new AccountModel() { OpenID = wXAccount.OpenId, HasRedPocket = false, AccountName = wXAccount.NickName, Gender = gender, AccountAvatar = avatarUrl, CreateTime = DateTime.Now, LastChangeTime = DateTime.Now };
                    collection.InsertOne(accountCard);
                }
            }
            int stautsCode = (int)(ActionParams.code_error);
            if (accountCard != null)
            {
                responseModel.JsonData = accountCard;
                stautsCode = (int)(ActionParams.code_ok);
            }
            responseModel.StatusCode = stautsCode;
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

            string[] param = new string[] { "StatusCode", "JsonData", "AccountID", "HasRedPocket" };


            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(param);
            string jsonString = JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
            Console.WriteLine("json#####UserInfo:" + jsonString);
            return jsonString;
        }

        /// <summary>
        /// 下载微信头像
        /// </summary>
        /// <param name="avatarUrl">微信头像地址</param>
        /// <param name="openId">openid</param>
        /// <returns>图片下载后的路径</returns>
        private string DownloadAvatar(string avatarUrl, string openId)
        {
            WebClient webClient = new WebClient();
            string saveDBName = $@"{ConstantProperty.AvatarDir}{openId}.jpg";
            string saveFileName = $@"{ConstantProperty.BaseDir}{saveDBName}";
            string path = $@"{ConstantProperty.BaseDir}{ConstantProperty.AvatarDir}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            webClient.DownloadFile(avatarUrl, saveFileName);
            return saveDBName;
        }

        /// <summary>
        /// 修改昵称
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public string ChangeAccountName(string accountID, string accountName)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(accountName))
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;

            try
            {
                var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                var update = Builders<AccountModel>.Update.Set(x => x.AccountName, accountName);
                new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(filter, update);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
            }
            return JsonConvert.SerializeObject(responseModel);
        }

        /// <summary>
        /// 获取账户信息
        /// </summary>
        /// <param name="accountID">账户Id</param>
        /// <returns></returns>
        public string GetAccountInfo(string accountID)
        {
            BaseResponseModel<AccountModel> responseModel = new BaseResponseModel<AccountModel>();
            if (accountID == null)
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;
            try
            {
                var mongo = new MongoDBTool();
                var account = mongo.GetMongoCollection<AccountModel>().Find(x => x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault();
                if (account == null)
                {
                    responseModel.StatusCode = (int)ActionParams.code_null;
                }

                //查询三代
                ///查询待拼单

                var jackFilter = Builders<JackPotModel>.Filter;
                var jackFilterSum = jackFilter.Eq("Participator.AccountID", account.AccountID) & jackFilter.Eq(x => x.JackPotStatus, 0);
                var jackList = mongo.GetMongoCollection<JackPotModel>().Find(jackFilterSum).ToList();
                if (jackList != null)
                {
                    account.WaitingJoin = jackList.Count;
                }

                if (account.OrderList != null)
                {
                    account.WaitingSend = account.OrderList.FindAll(x => x.OrderStatus == 0||x.OrderStatus==1).Count;
                    account.WaitingAssess = account.OrderList.FindAll(x => x.OrderStatus == 2).Count;
                }

                var company = mongo.GetMongoCollection<CompanyModel>().Find(Builders<CompanyModel>.Filter.Empty).FirstOrDefault();
                if (company != null)
                {
                    account.ServicePhone = company.ServicePhone;
                }

                responseModel.JsonData = account;
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;

                throw;
            }
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(
                new string[] {
                    "StatusCode",
                    "JsonData",
                    "AccountName" ,
                    "AccountAvatar",
                    "OrderList",
                    "ServicePhone",
                "WaitingJoin",
                "WaitingSend",
                "WaitingAssess",
                "OrderLocation",
                "OrderLocationPhone",
                "OrderLocationPersonName",
                "ProvinceCityArea",
                "AddressDetail"});

            return JsonConvert.SerializeObject(responseModel, jsonSerializerSettings);
        }

        /// <summary>
        /// 设置收货地址
        /// </summary>
        /// <param name="accountID">账户id</param>
        /// <param name="provinceArray">省市区数组["北京","北京","朝阳"]</param>
        /// <param name="addressDetail">详细地址</param>
        /// <param name="orderLocationPhone">收件联系方式</param>
        /// <param name="orderLocationPersonName">收件人姓名</param>
        /// <returns></returns>
        public string SetAddress(string accountID, string provinceArray, string addressDetail, string orderLocationPhone, string orderLocationPersonName)
        {
            BaseResponseModel<AccountModel> responseModel = new BaseResponseModel<AccountModel>();
            if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(provinceArray) || string.IsNullOrEmpty(addressDetail))
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;

            try
            {
                string[] pca = JsonConvert.DeserializeObject<string[]>(provinceArray);
                var address = new OrderLocation() { ProvinceCityArea = pca, AddressDetail = addressDetail, OrderLocationPersonName = orderLocationPersonName, OrderLocationPhone = orderLocationPhone };
                var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, new ObjectId(accountID));
                var update = Builders<AccountModel>.Update.Set(x => x.OrderLocation, address);
                new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(filter, update);
            }
            catch (Exception)
            {
                responseModel.StatusCode = (int)ActionParams.code_error;
                throw;
            }
            return JsonConvert.SerializeObject(responseModel);
        }


        public string GetRedPacket(string accountID)
        {
            string json = "";
            var collection = new MongoDBTool().GetMongoCollection<AccountModel>();
            var account = collection.Find(x => x.AccountID.Equals(new ObjectId(accountID))).FirstOrDefault();
            if (!account.HasRedPocket)
            {
                int redPacket = 20;
                Refund.Run("", new RandomNumber().GetRandom1(), redPacket, redPacket);
            }
            return json;
        }
    }
}
