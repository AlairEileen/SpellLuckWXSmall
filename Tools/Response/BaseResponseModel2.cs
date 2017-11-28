using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tools.ResponseModels
{
    public class BaseResponseModel2<T,P>
    {
        public int StatusCode { get; set; }
        public T JsonData1 { get; set; }
        public P JsonData2 { get; set; }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
