using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace SpellLuckWXSmall.Models
{
    public class AccountModel : BaseAccount
    {
        public string OpenID { get; set; }
        public OrderLocation OrderLocation { get; set; }
        public int WaitingJoin { get; set; }
        public int WaitingSend { get; set; }
        public int WaitingAssess { get; set; }
        public bool HasRedPocket { get; set; }
        public string ServicePhone { get; set; }
        public List<OrderModel> OrderList { get; set; }
    }

    public class OrderLocation
    {
        public string OrderLocationPhone { get; set; }
        public string OrderLocationPersonName { get; set; }
        public string[] ProvinceCityArea { get; set; }
        public string AddressDetail { get; set; }
    }
}
