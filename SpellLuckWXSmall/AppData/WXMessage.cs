using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WXSmallAppCommon.WXInteractions;
using WXSmallAppCommon.WXTool;

namespace SpellLuckWXSmall.AppData
{
    public class WXMessageOrderModel
    {
        [JsonProperty("touser")]
        public string OpenID { get; set; }
        [JsonProperty("template_id")]
        public string TemplateID { get { return AppConstData.MODEL_MESSAGE_ID; } }
        [JsonProperty("form_id")]
        public string FormID { get; set; }
        [JsonProperty("data")]
        public WXMessageOrderModelData Data { get; set; }
        [JsonProperty("emphasis_keyword")]
        public string EmphasisKeyword { get { return "keyword4.DATA"; } }
    }

    public class WXMessageOrderModelData
    {
        [JsonProperty("keyword1")]
        public KeyWord GoodsTitle { get; set; }
        [JsonProperty("keyword2")]
        public KeyWord OrderTotal { get; set; }
        [JsonProperty("keyword3")]
        public KeyWord OrderCreateTime { get; set; }
        [JsonProperty("keyword4")]
        public KeyWord OrderNumber { get; set; }
        [JsonProperty("keyword5")]
        public KeyWord ServicePhone { get; set; }
    }

    public class KeyWord
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class WXOrderMessageSender
    {
        public static void Send(WXMessageOrderModel wXMessageOrderModel)
        {
            string jsonData = JsonConvert.SerializeObject(wXMessageOrderModel);
            string accessToken = new JsApiPay().GetAccessToken();
            string url = $"https://api.weixin.qq.com/cgi-bin/message/wxopen/template/send?access_token={accessToken}";
            Request_WebClient(url, jsonData, Encoding.UTF8);
        }
        public static string Request_WebClient(string uri, string paramStr, Encoding encoding)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            string result = string.Empty;

            WebClient wc = new WebClient();

            // 采取POST方式必须加的Header  
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] postData = encoding.GetBytes(paramStr);

            //if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            //{
            //    wc.Credentials = GetCredentialCache(uri, username, password);
            //    wc.Headers.Add("Authorization", GetAuthorization(username, password));
            //}

            byte[] responseData = wc.UploadData(uri, "POST", postData); // 得到返回字符流  
            return encoding.GetString(responseData);// 解码                    
        }
    }
}
