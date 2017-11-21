﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpellLuckWXSmall.Models;
using Tools.DB;
using MongoDB.Driver;

namespace SpellLuckWXSmall.Pages
{
    public class IndexModel : PageModel
    {

        public List<OrderModel> OrderList { get; set; }
        public List<AccountModel> AccountList { get; set; }

        [BindProperty]
        public string TrackingCompany { get; set; }
        [BindProperty]
        public string TrackingNumber { get; set; }
        [BindProperty]
        public string OrderId { get; set; }
        [BindProperty]
        public string SearchParam { get; set; }
        [BindProperty]
        public int OrderStatus { get; set; }


        public void OnGet()
        {
            GetWaitingSendOrder();
        }

        private void GetWaitingSendOrder()
        {
            var accountWaitingSend = new MongoDBTool().GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Empty).ToList();
            OrderList = ConvertToOrderList(accountWaitingSend, o =>
            {
                if (o.OrderStatus == 1 && string.IsNullOrEmpty(o.TrackingNumber))
                {
                    return true;
                }
                return false;
            });
        }
        private void GetWaitingSendOkOrder()
        {
            var accountWaitingSend = new MongoDBTool().GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Empty).ToList();
            OrderList = ConvertToOrderList(accountWaitingSend, o =>
            {
                if (o.OrderStatus == 0)
                {
                    return true;
                }
                return false;
            });
        }
        private void GetWaitingAssessOrder()
        {
            var accountWaitingSend = new MongoDBTool().GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Empty).ToList();
            OrderList = ConvertToOrderList(accountWaitingSend, o =>
            {
                if (o.OrderStatus == 1 && !string.IsNullOrEmpty(o.TrackingNumber))
                {
                    return true;
                }
                return false;
            });
        }
        private List<OrderModel> ConvertToOrderList(List<AccountModel> accountWaitingSend, Func<OrderModel, bool> func)
        {
            List<OrderModel> list = new List<OrderModel>();
            foreach (var item in accountWaitingSend)
            {
                if (item.OrderList == null || item.OrderList.Count == 0)
                {
                    continue;
                }
                foreach (var order in item.OrderList)
                {
                    if (func(order))
                    {
                        list.Add(order);
                    }
                }
                AccountList.Add(item);
            }
            return list;
        }

        public IActionResult OnPostSendGoods()
        {
            Console.WriteLine(OrderId + TrackingNumber + TrackingCompany);
            return Page();
        }

        public IActionResult OnPostChangeOrderStatus()
        {
            switch (OrderStatus)
            {
                case 0:
                    OnGet();
                    break;
                case 1:
                    GetWaitingSendOkOrder();
                    break;
                case 2:
                    GetWaitingAssessOrder();
                    break;
                default:
                    OnGet();
                    break;
            }
            return Page();
        }
    }
}
