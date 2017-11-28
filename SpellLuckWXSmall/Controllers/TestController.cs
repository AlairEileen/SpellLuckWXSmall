using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using SpellLuckWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.DB;
using WXSmallAppCommon;
using WXSmallAppCommon.WXInteractions;

namespace SpellLuckWXSmall.Controllers
{
    public class TestController : Controller
    {
        public string TestCertExists()
        {
            return new Test().CertFileExists();
        }

        public string TestSendRedPack()
        {
            WxRedPack wxRedPack = new WxRedPack();
            var result = wxRedPack.SendRedPack(
                   new SendRedPackModel()
                   {
                       re_openid = "oNd4E0Sn_wWIs035fA5_5RclQxZo",
                       remark = "备注",
                       act_name = "活动",
                       SenderName = "发送者名称",
                       total_amount = 1,
                       total_num = 1,
                       wishing = "祝福语"
                   }
                );
            return result;
        }

        public string SaveTest(TestModel testModel)
        {
            new MongoDBTool().GetMongoCollection<TestModel>().InsertOne(new TestModel() { CDate = DateTime.Now.ToUniversalTime(), MDate = DateTime.Now });
            return "ok";
        }
        public string GetTestList()
        {
            return JsonConvert.SerializeObject(new MongoDBTool().GetMongoCollection<TestModel>().Find(Builders<TestModel>.Filter.Empty).ToList());
        }
    }
}
