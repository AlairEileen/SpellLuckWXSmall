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
                    string avatarUrl = DownloadAvatar(wXAccount.AvatarUrl, wXAccount.OpenId);
                    accountCard = new AccountModel() { OpenID = wXAccount.OpenId, AccountName = wXAccount.NickName, Gender = gender, AccountAvatar = avatarUrl, CreateTime = DateTime.Now, LastChangeTime = DateTime.Now };
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
            jsonSerializerSettings.ContractResolver = new LimitPropsContractResolver(new string[] { "StatusCode", "JsonData", "AccountID" });
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
        public string ChangeAccountName(ObjectId accountID, string accountName)
        {
            BaseResponseModel<string> responseModel = new BaseResponseModel<string>();
            if (accountID==ObjectId.Empty||string.IsNullOrEmpty(accountName))
            {
                responseModel.StatusCode = (int)ActionParams.code_error_null;
                return JsonConvert.SerializeObject(responseModel);
            }
            responseModel.StatusCode = (int)ActionParams.code_ok;

            try
            {
                var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID);
                var update = Builders<AccountModel>.Update.Set(x => x.AccountName, accountName);
                new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(filter, update);
            }
            catch (Exception)
            {
            responseModel.StatusCode = (int)ActionParams.code_error;
            }
            return JsonConvert.SerializeObject(responseModel);
        }
    }
}
