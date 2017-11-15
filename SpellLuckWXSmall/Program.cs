using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpellLuckWXSmall.AppData;

namespace SpellLuckWXSmall
{
    public class Program
    {
        static JackPotTimer jackPotTimer;
        static OrderAutoSend orderAutoSend;
        public static void Main(string[] args)
        {
            jackPotTimer = new JackPotTimer();
            orderAutoSend = new OrderAutoSend();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
