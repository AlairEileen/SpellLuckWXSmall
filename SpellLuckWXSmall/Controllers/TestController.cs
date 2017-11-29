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
#if DEBUG
        public string TestReFund(string orderId, int money)
        {
            string status = Refund.Run(orderId, "", money, money);
            return status;
        }

        public string ReFundAllMoney()
        {
            var jackPots = new MongoDBTool().GetMongoCollection<JackPotModel>().Find(Builders<JackPotModel>.Filter.Empty).ToList();
            var waits = new MongoDBTool().GetMongoCollection<PayWaitingModel>().Find(Builders<PayWaitingModel>.Filter.Empty).ToList();

            if (jackPots != null)
            {
                foreach (var item in jackPots)
                {
                    foreach (var per in item.Participator)
                    {
                        if (!string.IsNullOrEmpty(per.WXOrderId))
                        {
                            Refund.Run(per.WXOrderId, "", 1, 1);

                        }
                    }
                }
            }
            if (waits != null)
            {
                foreach (var item in waits)
                {

                    if (!string.IsNullOrEmpty(item.WXOrderId))
                    {
                        Refund.Run(item.WXOrderId, "", 1, 1);

                    }

                }
            }
            return "success";
        }
#endif
    }
}
