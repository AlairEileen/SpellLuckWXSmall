using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace SpellLuckWXSmall.Models
{
    public class OrderModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId OrderID { get; set; }

        /// <summary>
        /// 订单号码
        /// </summary>
        public string OrderNumber { get; set; }

        public OrderGoodsInfo GoodsInfo { get; set; }

        public decimal OrderPrice { get; set; }
        [JsonConverter(typeof(Tools.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// -2:确认退款,-1：未中奖，0:待确认发货与退款，1：确认发货，，2：待评价，3：结束
        /// </summary>
        public int OrderStatus { get; set; }

        public string WXOrderId { get; set; }

        /// <summary>
        /// 运单号
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// 快递公司
        /// </summary>
        public string TrackingCompany { get; set; }
        public bool isRefound { get; set; }
        public bool hasRefoundByCompany { get; set; }

        [JsonIgnore]
        [BsonIgnore]
        public string OrderIDText { get { return OrderID.ToString(); } }

        public string OrderStatusText
        {
            get
            {
                switch ((OrderStatusType)OrderStatus)
                {
                    case OrderStatusType.WaitCompanyRefund:
                        return "待商家退款";
                    case OrderStatusType.NoGetJack:
                        return "未中奖";
                    case OrderStatusType.WaitAgreeSendGoods:
                        return "待确认发货";
                    case OrderStatusType.WaitCompanySendGoods:
                        return "待商家发货";
                    case OrderStatusType.WaitAssess:
                        return "待评价";
                    case OrderStatusType.FinishOrder:
                        if (isRefound)
                        {
                            return "完成——已退款";
                        }
                        return "完成";
                    default:
                        return "未知";
                }
            }
        }
        public List<AccountPotModel> Participator { get; set; }
        public AccountPotModel LuckAccount { get; set; }
        public int JackPotPeopleNum { get; set; }

    }
    public enum OrderStatusType
    {
        /// <summary>
        /// 待商家退款
        /// </summary>
        WaitCompanyRefund = -2,
        /// <summary>
        /// 未中奖
        /// </summary>
        NoGetJack = -1,
        /// <summary>
        /// 待确认发货
        /// </summary>
        WaitAgreeSendGoods = 0,
        /// <summary>
        /// 待商家发货
        /// </summary>
        WaitCompanySendGoods = 1,
        /// <summary>
        /// 待评价
        /// </summary>
        WaitAssess = 2,
        /// <summary>
        /// 完成
        /// </summary>
        FinishOrder = 3

    }
    public class OrderGoodsInfo
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Json.ObjectIdConverter))]
        public ObjectId GoodsID { get; set; }
        public string GoodsTitle { get; set; }
        public decimal GoodsPrice { get; set; }
        /// <summary>
        /// 0:2人拼团
        /// 1:多人拼团
        /// 2:1分夺宝
        /// </summary>
        public int GoodsPayType { get; set; }
        public FileModel<string[]> GoodsListImage { get; set; }
        public long GoodsPeopleNum { get; set; }
        public string GoodsColor { get; set; }
        public string GoodsRule { get; set; }
    }
}
