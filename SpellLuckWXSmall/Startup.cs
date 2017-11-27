using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tools.Logger;
using SpellLuckWXSmall.Interceptors;
using Microsoft.AspNetCore.Http;

namespace SpellLuckWXSmall
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddFile(this.Configuration.GetSection("FileLogging"));
            loggerFactory.AddConsole();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();
            app.UseSession();
           
            app.MapWhen(context =>
           context.Request.Path.StartsWithSegments("/GoodsListShow") ||
           context.Request.Path.StartsWithSegments("/GoodsPush") ||
           context.Request.Path.StartsWithSegments("/Settings") ||
           context.Request.Path.StartsWithSegments("/OrderListShow"), x =>
           {
               x.UseSampleMiddleware();
               x.UseMvc(routes =>
               {
                   routes.MapRoute(
                       name: "default",
                       template: "{controller}/{action=Index}/{id?}");
               });
           });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
            //app.UseSampleMiddleware();

        }
    }
}
