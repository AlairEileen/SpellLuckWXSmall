using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellLuckWXSmall
{
    public class AppConstData
    {
        #region 开奖时间
        /// <summary>
        /// 开奖时间——时
        /// </summary>
        public const int JackPotTimerHour = 17;
        /// <summary>
        /// 开奖时间——分
        /// </summary>
        public const int JackPotTimerMinute = 0;
        #endregion
        /// <summary>
        /// 团购超时退款小时
        /// </summary>
        public const int OverTimeGroupJack = 24;
        /// <summary>
        /// 检测下单后自动发货分钟
        /// </summary>
        public const int CheckOrderAutoSend = 6*60;
        /// <summary>
        /// 手机分页大小
        /// </summary>
        public const int MobilePageSize = 5;
        /// <summary>
        /// 分享次数，达到后才能获得获奖机会
        /// </summary>
        public const int SharaMinAdd = 3;
        /// <summary>
        /// 中奖推送信息模板
        /// </summary>
        public const string MODEL_MESSAGE_ID = "SshDn_vyQWbTKx0bN6QZ4-8n0_ZknPJxgBPY1JMe7do";

    }
}
