using MongoDB.Bson;
using MongoDB.Driver;
using SpellLuckWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools.DB;
using WXSmallAppCommon.WXInteractions;

namespace SpellLuckWXSmall.AppData
{
    public class JackPotData
    {
        /// <summary>
        /// 开始玩
        /// </summary>
        /// <param name="payWaitingModel"></param>
        public void StartJack(PayWaitingModel payWaitingModel)
        {
            if (payWaitingModel != null)
            {
                var mongo = new MongoDBTool();
                var accountFilter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, payWaitingModel.AccountID);
                var account = mongo.GetMongoCollection<AccountModel>().Find(accountFilter).FirstOrDefault();
                GoodsModel goods = null;
                if (!payWaitingModel.GoodsID.Equals(ObjectId.Empty))
                {
                    var goodsFilter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsID, payWaitingModel.GoodsID);
                    goods = mongo.GetMongoCollection<GoodsModel>().Find(goodsFilter).FirstOrDefault();
                }
                JackPotModel jackPot = null;
                if (!payWaitingModel.JackPotID.Equals(ObjectId.Empty))
                {
                    var jackPotFilter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotID, payWaitingModel.JackPotID);
                    jackPot = mongo.GetMongoCollection<JackPotModel>().Find(jackPotFilter).FirstOrDefault();
                }
                switch (goods.GoodsPayType)
                {
                    case 0://2人拼团
                    case 1://多人拼团
                        if (goods != null)
                        {
                            CreateJackPot(mongo, account, goods, payWaitingModel);
                        }
                        else if (jackPot != null)
                        {
                            JoinJackPot(mongo, account, jackPot, payWaitingModel);
                        }
                        break;
                    case 2://1分夺宝
                        JoinJackPotSharaWaiting(mongo, account, goods, payWaitingModel);
                        break;
                }

            }
            else
            {
                Console.WriteLine("腾讯支付成功返回数据为空");

            }
        }

        /// <summary>
        /// 加入或者创建一分夺宝奖池待分享奖池
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="account"></param>
        /// <param name="goods"></param>
        /// <param name="payWaitingModel"></param>
        private void JoinJackPotSharaWaiting(MongoDBTool mongo, AccountModel account, GoodsModel goods, PayWaitingModel payWaitingModel)
        {
            var jackPotJoinWaiting = mongo.GetMongoCollection<JackPotJoinWaitingModel>().Find(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID)).FirstOrDefault();
            if (jackPotJoinWaiting == null)
            {
                JackPotJoinWaitingModel jackPotJoinWaitingModel = new JackPotJoinWaitingModel()
                {
                    GoodsID = goods.GoodsID,
                    AccountID = account.AccountID,
                    PayWaitingID = payWaitingModel.PayWaitingID,
                    GoodsRule = payWaitingModel.GoodsRule,
                    GoodsColor = payWaitingModel.GoodsColor,
                    WXOrderId = payWaitingModel.WXOrderId
                };
                mongo.GetMongoCollection<JackPotJoinWaitingModel>().InsertOne(jackPotJoinWaiting);
            }
        }

        /// <summary>
        /// 加入奖池
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="account"></param>
        /// <param name="jackPot"></param>
        /// <param name="payWaitingModel"></param>
        private void JoinJackPot(MongoDBTool mongo, AccountModel account, JackPotModel jackPot, PayWaitingModel payWaitingModel)
        {
            if (jackPot.JackGoods.GoodsPeopleNum <= jackPot.Participator.Count)//奖池已关闭
            {
                OpenedJack(mongo, account, payWaitingModel);
                return;
            }
            if (jackPot.Participator.Exists(x => x.AccountID.Equals(account.AccountID)))//已加入
            {
                return;
            }
            jackPot.Participator.Add(new AccountPotModel()
            {
                AccountAvatar = account.AccountAvatar,
                AccountID = account.AccountID,
                GoodsColor = payWaitingModel.GoodsColor,
                GoodsRule = payWaitingModel.GoodsRule,
                WXOrderId = payWaitingModel.WXOrderId,
                AccountName = account.AccountName
            });
            if (jackPot.JackGoods.GoodsPeopleNum == jackPot.Participator.Count)
            {
                ObjectId accountID = JackTool.CalcJackAccount(jackPot.Participator);
                JackTool.CreateOrder(jackPot, accountID);
                jackPot.JackPotStatus = 2;
                var filter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotID, jackPot.JackPotID);
                var update = Builders<JackPotModel>.Update.Set(x => x.Participator, jackPot.Participator).Set(x => x.JackPotStatus, jackPot.JackPotStatus);
                mongo.GetMongoCollection<JackPotModel>().UpdateOne(filter, update);
                mongo.GetMongoCollection<PayWaitingModel>().DeleteOne(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID));

            }
            else if (jackPot.JackGoods.GoodsPeopleNum >= jackPot.Participator.Count)
            {
                var filter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotID, jackPot.JackPotID);
                var update = Builders<JackPotModel>.Update.Set(x => x.Participator, jackPot.Participator);
                mongo.GetMongoCollection<JackPotModel>().UpdateOne(filter, update);
                mongo.GetMongoCollection<PayWaitingModel>().DeleteOne(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID));
            }

        }

        /// <summary>
        /// 该奖池已经开奖，选择重新开团或者退款 或者推送
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="account"></param>
        /// <param name="payWaitingModel"></param>
        private void OpenedJack(MongoDBTool mongo, AccountModel account, PayWaitingModel payWaitingModel)
        {
            var filter = Builders<GoodsModel>.Filter.Eq(x => x.GoodsID, payWaitingModel.GoodsID);
            var goods = mongo.GetMongoCollection<GoodsModel>().Find(filter).FirstOrDefault();
            if (goods == null)
            {
                Console.WriteLine("商品不存在");
                return;
            }
            Refund.Run(payWaitingModel.WXOrderId, "", goods.GoodsPrice, goods.GoodsPrice);
        }

        /// <summary>
        /// 创建奖池
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="account"></param>
        /// <param name="goods"></param>
        /// <param name="payWaitingModel"></param>
        private void CreateJackPot(MongoDBTool mongo, AccountModel account, GoodsModel goods, PayWaitingModel payWaitingModel)
        {
            try
            {
                var jackPot = mongo.GetMongoCollection<JackPotModel>().Find(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID));
                if (jackPot == null || jackPot.Count() == 0)
                {
                    JackPotModel jackPotModel = new JackPotModel()
                    {
                        JackGoods = goods,
                        CreateTime = DateTime.Now,
                        JackPotPassword = payWaitingModel.JackPotKey,
                        JackPotStatus = 0,
                        Participator = new List<AccountPotModel>() { new AccountPotModel() {
                    AccountAvatar=account.AccountAvatar,
                    AccountID=account.AccountID,
                    GoodsColor = payWaitingModel.GoodsColor,
                    GoodsRule = payWaitingModel.GoodsRule,
                    WXOrderId=payWaitingModel.WXOrderId,
                    AccountName=account.AccountName
                } }
                    };
                    mongo.GetMongoCollection<JackPotModel>().InsertOne(jackPotModel);
                    mongo.GetMongoCollection<PayWaitingModel>().DeleteOne(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("创建奖池出错" + ex.Message);
            }
        }



    }


    /// <summary>
    /// 奖计算工具与颁发工具
    /// </summary>
    internal class JackTool
    {
        /// <summary>
        /// 颁发奖励
        /// </summary>
        /// <param name="jackPot"></param>
        /// <param name="accountID"></param>
        internal static void CreateOrder(JackPotModel jackPot, ObjectId accountID)
        {
            var account = jackPot.Participator.Find(x => x.AccountID.Equals(accountID));
            if (account == null)
            {
                Console.WriteLine("订单用户不存在！");
                return;
            }
            OrderModel orderModel = new OrderModel()
            {
                OrderID = ObjectId.GenerateNewId(),
                OrderStatus = 0,
                OrderPrice = jackPot.JackGoods.GoodsPrice,
                CreateTime = DateTime.Now,
                WXOrderId = account.WXOrderId,
                GoodsInfo = new OrderGoodsInfo()
                {
                    GoodsPrice = jackPot.JackGoods.GoodsPrice,
                    GoodsID = jackPot.JackGoods.GoodsID,
                    GoodsListImage = jackPot.JackGoods.GoodsListImage,
                    GoodsPayType = jackPot.JackGoods.GoodsPayType,
                    GoodsPeopleNum = jackPot.JackGoods.GoodsPeopleNum,
                    GoodsTitle = jackPot.JackGoods.GoodsTitle,
                    GoodsRule = account.GoodsRule,
                    GoodsColor = account.GoodsColor,
                }
            };
            var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID);
            var update = Builders<AccountModel>.Update.Push(x => x.OrderList, orderModel);
            new MongoDBTool().GetMongoCollection<AccountModel>().UpdateOne(filter, update);
        }

        /// <summary>
        /// 计算获奖者
        /// </summary>
        /// <param name="participator"></param>
        /// <returns></returns>
        internal static ObjectId CalcJackAccount(List<AccountPotModel> participator)
        {
            Random random = new Random();
            int luckIndex = random.Next(0, participator.Count);
            return participator[luckIndex].AccountID;
        }
    }


    /// <summary>
    /// 开奖定时器
    /// </summary>
    public class JackPotTimer
    {
        Timer jackPotTimer;
        int hour = AppConstData.JackPotTimerHour;
        int minute = AppConstData.JackPotTimerMinute;
        public JackPotTimer()
        {
            jackPotTimer = new Timer(CheckJack, null, 0, 1000 * 60);
        }

        /// <summary>
        /// 检测时间
        /// </summary>
        /// <param name="state"></param>
        private void CheckJack(object state)
        {
            var currentDate = DateTime.Now;
            int currentHour = currentDate.Hour;
            int currentMinute = currentDate.Minute;
            if (hour == currentHour && minute == currentMinute)
            {
                DoCheckJack();
            }
        }

        /// <summary>
        /// 检测开奖
        /// </summary>
        private void DoCheckJack()
        {
            var mongo = new MongoDBTool();
            var collection = mongo.GetMongoCollection<JackPotModel>();
            CheckGroupJack(mongo, collection);
            CheckJackPot(mongo, collection);
        }

        /// <summary>
        /// 检测团购
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="collection"></param>
        private void CheckGroupJack(MongoDBTool mongo, IMongoCollection<JackPotModel> collection)
        {
            var filter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotStatus, 0);
            var list = collection.Find(filter).ToList();
            foreach (var item in list)
            {
                if ((DateTime.Now - item.CreateTime).Hours > AppConstData.OverTimeGroupJack)
                {
                    if (item.JackGoods.GoodsPeopleNum > item.Participator.Count)
                    {
                        GoRefund(item);
                    }
                }

            }

        }

        /// <summary>
        /// 请求退款
        /// </summary>
        /// <param name="item"></param>
        private void GoRefund(JackPotModel item)
        {
            for (int i = 0; i < item.Participator.Count; i++)
            {
                Refund.Run(item.Participator[i].WXOrderId, "", item.JackGoods.GoodsPrice, item.JackGoods.GoodsPrice);
            }
        }




        /// <summary>
        /// 检测1分夺宝
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="collection"></param>
        private void CheckJackPot(MongoDBTool mongo, IMongoCollection<JackPotModel> collection)
        {
            var filter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotStatus, 1);
            var list = collection.Find(filter).ToList();
            foreach (var item in list)
            {
                if ((DateTime.Now - item.CreateTime).Hours > AppConstData.OverTimeGroupJack)
                {
                    OpenJackPot(item);
                }

            }
        }

        /// <summary>
        /// 一分夺宝开奖
        /// </summary>
        /// <param name="item"></param>
        private void OpenJackPot(JackPotModel item)
        {
            ObjectId luckAccount = JackTool.CalcJackAccount(item.Participator);
            JackTool.CreateOrder(item, luckAccount);
        }

    }
}
