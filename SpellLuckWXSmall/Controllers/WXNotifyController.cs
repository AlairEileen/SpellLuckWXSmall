using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WXSmallAppCommon.WXTool;
using SpellLuckWXSmall.Models;
using Newtonsoft.Json;
using Tools.DB;
using MongoDB.Driver;
using MongoDB.Bson;
using SpellLuckWXSmall.AppData;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpellLuckWXSmall.Controllers
{
    public class WXNotifyController : Controller
    {
        JackPotData jackPotData = new JackPotData();
      
        /// <summary>
        /// 微信支付回掉
        /// </summary>
        /// <returns></returns>
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
     
        /// <summary>
        /// 微信支付成功返回数据
        /// </summary>
        /// <param name="data"></param>
        private void OnPaySuccess(WxPayData data)
        {
            try
            {
                var attach = (string)data.GetValue("attach");
                var wxOrderId = (string)data.GetValue("transaction_id") ;
                if (string.IsNullOrEmpty(attach))
                {
                    return;
                }
                if (string.IsNullOrEmpty(wxOrderId))
                {
                    Console.WriteLine("####微信订单号为空");
                }
                var payWaitingModel=new MongoDBTool().GetMongoCollection<PayWaitingModel>().Find(x=>x.PayWaitingID.Equals(new ObjectId(attach))).FirstOrDefault();
                if (payWaitingModel!=null)
                {
                    payWaitingModel.WXOrderId = wxOrderId;
                    jackPotData.StartJack(payWaitingModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("腾讯支付成功返回处理出错" + ex.Message);
            }
        }
    }
}
