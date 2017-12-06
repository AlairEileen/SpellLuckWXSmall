using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using SpellLuckWXSmall.AppData;
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
            var collection = new MongoDBTool().GetMongoCollection<TestModel>();
            collection.InsertOne(new TestModel() { CDate = DateTime.Now.ToUniversalTime(), MDate = DateTime.Now, TestType = TestType.error });
            var list = collection.Find(Builders<TestModel>.Filter.Empty).ToList();

            foreach (var item in list)
            {
                switch (item.TestType)
                {
                    case TestType.ok:
                        break;
                    case TestType.error:
                        break;
                    case TestType.success:
                        break;
                    default:
                        break;
                }
            }
            return JsonConvert.SerializeObject(list);
        }

        public string ConvertJackOrder()
        {
            var mongo = new MongoDBTool();
            var accountCollection = mongo.GetMongoCollection<AccountModel>();
            var accountList = accountCollection.Find(Builders<AccountModel>.Filter.Exists("OrderList.Participator", false)).ToList();
            List<JackPotModel> jackPotList = new List<JackPotModel>();
            var jackCollection = mongo.GetMongoCollection<JackPotModel>();

            foreach (var item in accountList)
            {
                foreach (var order in item.OrderList)
                {
                    if (order.Participator == null)
                    {
                        jackPotList.Add(jackCollection.Find(Builders<JackPotModel>.Filter.Eq("Participator.WXOrderId", order.WXOrderId)).FirstOrDefault());
                    }
                }
            }
            jackPotList = jackPotList.Distinct().ToList();


            return "success";
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


        public string SendMessage()
        {
            WXMessageOrderModel wXMessageOrderModel = new WXMessageOrderModel()
            {
                OpenID = "oNd4E0Sn_wWIs035fA5_5RclQxZo",
                FormID = "prepay_id=wx20171206143523d6e69376140860386017",
                Data = new WXMessageOrderModelData()
                {
                    GoodsTitle = new KeyWord()
                    {
                        Value = "标题"
                    },
                    OrderCreateTime = new KeyWord()
                    {
                        Value = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss")
                    },
                    OrderNumber = new KeyWord()
                    {
                        Value = "423432jfsdjfo"
                    },
                    OrderTotal = new KeyWord()
                    {
                        Value = "10元"
                    },
                    ServicePhone = new KeyWord()
                    {
                        Value = "432423432423"
                    }

                }
            };
            WXOrderMessageSender.Send(wXMessageOrderModel);
            return "success";
        }
    }
}
