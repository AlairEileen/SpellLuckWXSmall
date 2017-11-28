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
            var listWaiting = mongo.GetMongoCollection<JackPotJoinWaitingModel>().Find(Builders<JackPotJoinWaitingModel>.Filter.Gt(x => x.ShareTimes, AppConstData.SharaMinAdd)).ToList();
            if (listWaiting != null && listWaiting.Count != 0)
            {
                var waitingJackPot = new List<JackPotModel>();
                foreach (var item in listWaiting)
                {
                    waitingJackPot.Add(new JackPotModel() { JackGoods = item.Goods });
                }
                listJackPot.AddRange(waitingJackPot);
            }
            listJackPot.Sort((x, y) => -x.CreateTime.CompareTo(y.CreateTime));
            AllJackPotList = listJackPot;
        }
    }
}