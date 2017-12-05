using MongoDB.Bson;
using MongoDB.Driver;
using SpellLuckWXSmall.Controllers;
using SpellLuckWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools;
using Tools.DB;
using Tools.Strings;
using WXSmallAppCommon.WXInteractions;
using WXSmallAppCommon.WXTool;

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

                var currentGoods = goods == null ? jackPot.JackGoods : goods;
                switch (currentGoods.GoodsPayType)
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
                jackPotJoinWaiting = new JackPotJoinWaitingModel()
                {
                    Goods = goods,
                    AccountID = account.AccountID,
                    PayWaitingID = payWaitingModel.PayWaitingID,
                    GoodsRule = payWaitingModel.GoodsRule,
                    GoodsColor = payWaitingModel.GoodsColor,
                    WXOrderId = payWaitingModel.WXOrderId,
                    CreateTime = DateTime.Now
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
            if (jackPot.JackPotPeopleNum <= jackPot.Participator.Count)//奖池已关闭
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
                PayWaitingID = payWaitingModel.PayWaitingID,
                AccountName = account.AccountName
            });
            if (jackPot.JackPotPeopleNum == jackPot.Participator.Count)
            {
                //ObjectId accountID = JackTool.CalcJackAccount(jackPot.Participator);
                //JackTool.CreateOrder(jackPot, accountID);
                JackTool.CalcCreateOrder(jackPot, jackPot.Participator);
                jackPot.JackPotStatus = 2;
                var filter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotID, jackPot.JackPotID);
                var update = Builders<JackPotModel>.Update.Set(x => x.Participator, jackPot.Participator).Set(x => x.JackPotStatus, jackPot.JackPotStatus);
                mongo.GetMongoCollection<JackPotModel>().UpdateOne(filter, update);
                mongo.GetMongoCollection<PayWaitingModel>().UpdateOne(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID), Builders<PayWaitingModel>.Update.Set(x => x.isDisabled, true));

            }
            else if (jackPot.JackPotPeopleNum >= jackPot.Participator.Count)
            {
                var filter = Builders<JackPotModel>.Filter.Eq(x => x.JackPotID, jackPot.JackPotID);
                var update = Builders<JackPotModel>.Update.Set(x => x.Participator, jackPot.Participator);
                mongo.GetMongoCollection<JackPotModel>().UpdateOne(filter, update);
                mongo.GetMongoCollection<PayWaitingModel>().UpdateOne(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID), Builders<PayWaitingModel>.Update.Set(x => x.isDisabled, true));
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
            Refund.Run(payWaitingModel.WXOrderId, "", goods.GoodsPrice.ConvertToMoneyCent(), goods.GoodsPrice.ConvertToMoneyCent());
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
                var jackPot = mongo.GetMongoCollection<JackPotModel>().Find(Builders<JackPotModel>.Filter.Eq("Participator.PayWaitingID", payWaitingModel.PayWaitingID));
                if (jackPot == null || jackPot.Count() == 0)
                {
                    JackPotModel jackPotModel = new JackPotModel()
                    {
                        JackGoods = goods,
                        CreateTime = DateTime.Now,
                        JackPotPassword = payWaitingModel.JackPotKey,
                        JackPotStatus = 0,
                        JackPotPrice = JackPotController.GetJackPotPrice(goods.GoodsPrice, payWaitingModel.JackPotPeopleNum),
                        JackPotPeopleNum = payWaitingModel.JackPotPeopleNum,
                        Participator = new List<AccountPotModel>() { new AccountPotModel() {
                    AccountAvatar=account.AccountAvatar,
                    AccountID=account.AccountID,
                    PayWaitingID = payWaitingModel.PayWaitingID,
                    GoodsColor = payWaitingModel.GoodsColor,
                    GoodsRule = payWaitingModel.GoodsRule,
                    WXOrderId=payWaitingModel.WXOrderId,
                    AccountName=account.AccountName
                } }
                    };
                    mongo.GetMongoCollection<JackPotModel>().InsertOne(jackPotModel);
                    mongo.GetMongoCollection<PayWaitingModel>().UpdateOne(x => x.PayWaitingID.Equals(payWaitingModel.PayWaitingID), Builders<PayWaitingModel>.Update.Set(x => x.isDisabled, true));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("创建奖池出错" + ex.Message);
            }
        }

        /// <summary>
        /// 获取带拼团列表
        /// </summary>
        /// <param name="accountID">用户ID</param>
        /// <returns></returns>
        public List<JackPotModel> GetAllWaitJackPot(string accountID)
        {
            ///拼团商品
            var filter = Builders<JackPotModel>.Filter;
            var filterSum = filter.Eq(x => x.JackPotStatus, 0) & filter.Eq("Participator.AccountID", new ObjectId(accountID));
            var listJackPot = new MongoDBTool().GetMongoCollection<JackPotModel>().Find(filterSum).ToList();
            foreach (var item in listJackPot)
            {
                if (item.JackGoods.GoodsPayType == 2)
                {
                    item.Description = "待开奖-一分夺宝";
                }
                else
                {
                    if (item.JackPotStatus == 3)
                    {
                        item.Description = "已退款";
                    }
                    else
                    {
                        item.Description = "待拼团";
                    }
                }
            }
            ///一分夺宝列表
            var waitingFilter = Builders<JackPotJoinWaitingModel>.Filter;
            var waitingFilterSum = waitingFilter.Eq(x => x.AccountID, new ObjectId(accountID)) & waitingFilter.Gt(x => x.ShareTimes, AppConstData.SharaMinAdd);
            var listWaiting = new MongoDBTool().GetMongoCollection<JackPotJoinWaitingModel>().Find(waitingFilterSum).ToList();
            if (listWaiting != null && listWaiting.Count != 0)
            {
                var waitingJackPot = new List<JackPotModel>();
                foreach (var item in listWaiting)
                {
                    waitingJackPot.Add(new JackPotModel() { JackGoods = item.Goods, Description = "待分享-一分夺宝" });
                }
                listJackPot.AddRange(waitingJackPot);
            }
            listJackPot.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));

            return listJackPot;
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
        internal static void CreateOrder(JackPotModel jackPot, ObjectId accountID, int orderStatus = 0)
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
                OrderStatus = orderStatus,
                OrderPrice = jackPot.JackPotPrice,
                CreateTime = DateTime.Now,
                OrderNumber = new RandomNumber().GetRandom1(),
                WXOrderId = account.WXOrderId,
                GoodsInfo = new OrderGoodsInfo()
                {
                    GoodsPrice = jackPot.JackGoods.GoodsPrice,
                    GoodsID = jackPot.JackGoods.GoodsID,
                    GoodsListImage = jackPot.JackGoods.GoodsListImage,
                    GoodsPayType = jackPot.JackGoods.GoodsPayType,
                    GoodsPeopleNum = jackPot.JackPotPeopleNum,
                    GoodsTitle = jackPot.JackGoods.GoodsTitle,
                    GoodsRule = account.GoodsRule,
                    GoodsColor = account.GoodsColor,
                },
                Participator = jackPot.Participator

            };
            ///商品销量增加
            var goodsCollection = new MongoDBTool().GetMongoCollection<GoodsModel>();
            var goods = goodsCollection.Find(x => x.GoodsID.Equals(orderModel.GoodsInfo.GoodsID)).FirstOrDefault();
            goods.GoodsSales++;
            goodsCollection.UpdateOne(x => x.GoodsID.Equals(goods.GoodsID), Builders<GoodsModel>.Update.Set(x => x.GoodsSales, goods.GoodsSales));
            ///保存订单
            var collection = new MongoDBTool().GetMongoCollection<AccountModel>();
            var filter = Builders<AccountModel>.Filter.Eq(x => x.AccountID, accountID);
            var accountCurrent = collection.Find(filter).FirstOrDefault();
            if (accountCurrent.OrderList == null)
            {
                accountCurrent.OrderList = new List<OrderModel>();
            }
            accountCurrent.OrderList.Add(orderModel);
            var update = Builders<AccountModel>.Update.Set(x => x.OrderList, accountCurrent.OrderList);
            collection.UpdateOne(filter, update);
        }

        internal static void CalcCreateOrder(JackPotModel jackPot, List<AccountPotModel> participator)
        {
            ObjectId objectId = CalcJackAccount(participator);
            var hasJackPotAccount = jackPot.Participator.Find(x => x.AccountID.Equals(objectId));
            hasJackPotAccount.HasJack = true;
            for (int i = 0; i < participator.Count; i++)
            {
                if (participator[i].AccountID.Equals(objectId))
                {
                    CreateOrder(jackPot, participator[i].AccountID);
                }
                else
                {
                    CreateOrder(jackPot, participator[i].AccountID, (int)OrderStatusType.NoGetJack);
                }
            }
            var filter = Builders<JackPotModel>.Filter;
            var filterSum = filter.Eq(x => x.JackPotID, jackPot.JackPotID) & filter.Eq("Participator.AccountID", objectId);
            new MongoDBTool().GetMongoCollection<JackPotModel>().UpdateOne(filterSum, Builders<JackPotModel>.Update.Set("Participator.$.HasJack", true));
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
            var company = new MongoDBTool().GetMongoCollection<CompanyModel>().Find(Builders<CompanyModel>.Filter.Empty).FirstOrDefault();
            if (company == null || company.TimeOpenJack == null)
            {
                return;
            }
            int currentHour = company.TimeOpenJack.JackPotTimerHour;
            int currentMinute = company.TimeOpenJack.JackPotTimerMinute;
            var date = DateTime.Now;
            int hour = date.Hour;
            int minute = date.Minute;
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
                if ((DateTime.Now - item.CreateTime).TotalHours > AppConstData.OverTimeGroupJack)
                {
                    if (item.JackPotPeopleNum > item.Participator.Count)
                    {
                        Log.Info("已经检测到退款项目", item.JackPotID.ToString());
                        GoRefund(item, mongo);
                    }
                }

            }

        }

        /// <summary>
        /// 请求退款
        /// </summary>
        /// <param name="item"></param>
        private void GoRefund(JackPotModel item, MongoDBTool mongo)
        {
            for (int i = 0; i < item.Participator.Count; i++)
            {
                if (item.Participator[i].IsRefund)
                {
                    continue;
                }
                Refund.Run(item.Participator[i].WXOrderId, "", item.JackGoods.GoodsPrice.ConvertToMoneyCent(), item.JackGoods.GoodsPrice.ConvertToMoneyCent());
                Log.Info("已经退款项目", item.Participator[i].WXOrderId);
                mongo.GetMongoCollection<JackPotModel>().UpdateOne(Builders<JackPotModel>.Filter.Eq("Participator.WXOrderId", item.Participator[i].WXOrderId), Builders<JackPotModel>.Update.Set("Participator.$.IsRefund", true));
            }
            mongo.GetMongoCollection<JackPotModel>().UpdateOne(x => x.JackPotID.Equals(item.JackPotID), Builders<JackPotModel>.Update.Set(x => x.JackPotStatus, 3));
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
                if ((DateTime.Now - item.CreateTime).TotalHours > AppConstData.OverTimeGroupJack)
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
            //ObjectId luckAccount = JackTool.CalcJackAccount(item.Participator);
            //JackTool.CreateOrder(item, luckAccount);
            JackTool.CalcCreateOrder(item, item.Participator);
        }

    }
}
