using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpellLuckWXSmall.Interceptors
{
    public class SampleMiddleware
    {
        private readonly RequestDelegate _next;

        public SampleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            byte[] data = context.Session.Get("CompanyAccountName");
            if (data==null||string.IsNullOrEmpty(System.Text.Encoding.UTF8.GetString(data)))
            {
                context.Response.Redirect("/Index");
                return;
            }
           
            await _next.Invoke(context);    
        }

    }
    public static partial class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSampleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SampleMiddleware>();
        }
    }


}
