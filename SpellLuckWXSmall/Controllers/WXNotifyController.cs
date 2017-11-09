using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WXSmallAppCommon.WXTool;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class WXNotifyController : Controller
    {
        public string OnWXPayBack()
        {
            var body = Request.Body;
            var bodyString = body.ToString();
            Log.Info(this.GetType().ToString(), "Receive data from WeChat : " + bodyString);
            string ret = "";
            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(bodyString);
                OnPaySuccess(data);
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                ret = res.ToXml();
            }

            Log.Info(this.GetType().ToString(), "Check sign success");
            return ret;
        }

        private void OnPaySuccess(WxPayData data)
        {
            throw new NotImplementedException();
        }
    }
}
