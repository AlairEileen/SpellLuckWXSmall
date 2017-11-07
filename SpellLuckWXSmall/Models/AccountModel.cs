using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace SpellLuckWXSmall.Models
{
    public class AccountModel:BaseAccount
    {
        public string OpenID { get; set; }
        public OrderLocation OrderLocation { get; set; }
    }

    public class OrderLocation
    {
        public string Province { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string AddressDetail { get; set; }
    }
}
