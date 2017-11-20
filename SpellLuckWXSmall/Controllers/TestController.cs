using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WXSmallAppCommon;
using WXSmallAppCommon.WXInteractions;

namespace SpellLuckWXSmall.Controllers
{
    public class TestController : Controller
    {
        public string TestCertExists()
        {
            return new Test().CertFileExists();
        }

        public string TestSendRedPack()
        {
            WxRedPack wxRedPack = new WxRedPack();
            var result = wxRedPack.SendRedPack(
                   new SendRedPackModel()
                   {
                       re_openid = "oNd4E0Sn_wWIs035fA5_5RclQxZo",
                       remark = "备注",
                       act_name = "活动",
                       SenderName = "发送者名称",
                       total_amount = 1,
                       total_num = 1,
                       wishing = "祝福语"
                   }
                );
            return result;
        }
    }
}
