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
    public class OrderListShowModel : PageModel
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
        public string OrderNumber { get; set; }
        [BindProperty]
        public string SearchParam { get; set; }
        [BindProperty]
        public int OrderStatus { get; set; }

        #region ·ÖÒ³Ïà¹Ø
        private int pageSize = 5;
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        private int maxPageShow = 10;
        public int MaxPageShow { get => maxPageShow; }
        public int PageSize { get => pageSize; }
        #endregion

        public void OnGet()
        {
            ChangeOrderStatus();
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
                if (AccountList == null)
                {
                    AccountList = new List<AccountModel>();
                }
                AccountList.Add(item);
            }
            list.Sort((x, y) => DateTime.Compare(x.CreateTime, y.CreateTime));

            //return list.Skip(PageIndex * PageSize).Take(PageSize).ToList();
            return list;
        }

        public IActionResult OnPostSendGoods()
        {
            Console.WriteLine(OrderId + TrackingNumber + TrackingCompany);
            return Page();
        }

        public IActionResult OnPostChangeOrderStatus()
        {
            PageIndex = 0;
            if (OrderStatus == -3)
            {
                GetAllOrders();
                return Page();
            }
            ChangeOrderStatus();

            return Page();
        }

        private void ChangeOrderStatus()
        {
            var accountWaitingSend = new MongoDBTool().GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Empty).ToList();
            OrderList = ConvertToOrderList(accountWaitingSend, o =>
            {
                if (o.OrderStatus == OrderStatus)
                {
                    return true;
                }
                return false;
            });
        }

        private void GetAllOrders()
        {
            var accountWaitingSend = new MongoDBTool().GetMongoCollection<AccountModel>().Find(Builders<AccountModel>.Filter.Empty).ToList();
            OrderList = ConvertToOrderList(accountWaitingSend, o => true);
            OrderStatus = -3;
        }

        public IActionResult OnPostSearchByOrderNumber()
        {
            var filter = Builders<AccountModel>.Filter;
            var filterSum = filter.Eq("OrderList.OrderNumber", OrderNumber);
            var accountWaitingSend = new MongoDBTool().GetMongoCollection<AccountModel>().Find(filterSum).ToList();
            List<OrderModel> list = new List<OrderModel>();
            foreach (var item in accountWaitingSend)
            {
                if (item.OrderList == null || item.OrderList.Count == 0)
                {
                    continue;
                }
                foreach (var order in item.OrderList)
                {
                    list.Add(order);
                }
                if (AccountList == null)
                {
                    AccountList = new List<AccountModel>();
                }
                AccountList.Add(item);
            }
            OrderList = list;

            return Page();
        }

     
    }
}