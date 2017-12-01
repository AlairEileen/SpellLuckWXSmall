using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tools.DB;
using SpellLuckWXSmall.Models;
using MongoDB.Driver;

namespace SpellLuckWXSmall.Pages
{
    public class PayListModel : PageModel
    {
        MongoDBTool mongo = new MongoDBTool();

        public List<JackPotModel> AllJackPotList { get; set; }

        public void OnGet()
        {
            GetAllJackPot();
        }

        private void GetAllJackPot()
        {
            var listJackPot = mongo.GetMongoCollection<JackPotModel>().Find(Builders<JackPotModel>.Filter.In(x => x.JackPotStatus, new int[] { 0, 1 })).ToList();
            var listWaiting = mongo.GetMongoCollection<JackPotJoinWaitingModel>().Find(Builders<JackPotJoinWaitingModel>.Filter.Lt(x => x.ShareTimes, AppConstData.SharaMinAdd)).ToList();
            if (listWaiting != null && listWaiting.Count != 0)
            {
                var waitingJackPot = new List<JackPotModel>();
                foreach (var item in listWaiting)
                {

                    var account = mongo.GetMongoCollection<AccountModel>().Find(x => x.AccountID.Equals(item.AccountID)).FirstOrDefault();
                    waitingJackPot.Add(new JackPotModel()
                    {
                        JackGoods = item.Goods,
                        Participator = new List<AccountPotModel>()
                        {
                         new AccountPotModel()
                         {
                             AccountID=account.AccountID,
                             AccountAvatar=account.AccountAvatar,
                             AccountName=account.AccountName,
                             GoodsColor=item.GoodsColor,
                             GoodsRule=item.GoodsRule,
                             PayWaitingID=item.PayWaitingID,
                             WXOrderId=item.WXOrderId
                         }

                        }
                    });
                }
                listJackPot.AddRange(waitingJackPot);
            }
            listJackPot.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));
            AllJackPotList = listJackPot;
        }
    }
}