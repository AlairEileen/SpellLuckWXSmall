using System;
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
            GetAllOrder();
        }

        private void GetAllOrder()
        {
            var collection = new MongoDBTool().GetMongoCollection<AccountModel>();
            var waitFilter = Builders<AccountModel>.Filter;
            string noTacking = null;
            var waitFilterSum = waitFilter.Eq("OrderList.$.OrderStatus", 1) & waitFilter.Eq("OrderList.$.TrackingNumber", noTacking);
            var accountWaitingSend = collection.Find(Builders<AccountModel>.Filter.Empty).ToList();
            OrderList = ConvertToOrderList(accountWaitingSend);
        }

        private List<OrderModel> ConvertToOrderList(List<AccountModel> accountWaitingSend)
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
                    if (order.OrderStatus == 1 && order.TrackingNumber == null)
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

        public IActionResult OnGetChangeOrderStatus()
        {
            Console.WriteLine(OrderStatus);
            return Page();
        }
    }
}
