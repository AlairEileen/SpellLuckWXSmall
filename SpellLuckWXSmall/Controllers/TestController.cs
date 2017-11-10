using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WXSmallAppCommon;

namespace SpellLuckWXSmall.Controllers
{
    public class TestController:Controller
    {
        public string TestCertExists()
        {
            return new Test().CertFileExists();
        }
    }
}
