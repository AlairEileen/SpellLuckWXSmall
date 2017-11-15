using MongoDB.Driver;
using SpellLuckWXSmall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools.DB;

namespace SpellLuckWXSmall.AppData
{
    public class OrderData
    {

    }

    public class OrderAutoSend
    {

        Timer orderTimer;
        public OrderAutoSend()
        {
            orderTimer = new Timer(CheckOrder, null, 30, AppConstData.CheckOrderAutoSend*1000 * 60);
        }

        private void CheckOrder(object state)
        {
            MongoDBTool mongo = new MongoDBTool();
            var filter = Builders<AccountModel>.Filter;
            var yesterday = DateTime.Now.AddDays(-1);
            ///case1
            var filterSum = filter.Eq("OrderList.OrderStatus", 0) & filter.Lte("OrderList.CreateTime", yesterday);
            //var update = Builders<AccountModel>.Update.Set("OrderList.OrderStatus", 1);
            //mongo.GetMongoCollection<AccountModel>().UpdateMany(filterSum, update);
            /// case2
            var collection = mongo.GetMongoCollection<AccountModel>();
            var accounts = collection.Find(filterSum).ToList();
            if (accounts == null)
            {
                return;
            }
            foreach (var account in accounts)
            {
                foreach (var order in account.OrderList)
                {
                    if (order.CreateTime < yesterday && order.OrderStatus == 0)
                    {
                        order.OrderStatus = 1;
                    }
                }
                collection.UpdateOne(x => x.AccountID.Equals(account.AccountID), Builders<AccountModel>.Update.Set(x => x.OrderList, account.OrderList));
            }
        }
    }
}
