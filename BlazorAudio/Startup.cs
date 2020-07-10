using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorAudio.Services;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Microsoft.Extensions.Logging;

namespace BlazorAudio
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<ISoundNotifier, SoundNotifier>();
            if (string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED"),
                "true", StringComparison.OrdinalIgnoreCase))
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.KnownProxies.Add(IPAddress.Parse("192.168.0.5"));
                    options.KnownProxies.Add(IPAddress.Parse("192.168.0.1"));
                });
            }
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> _logger)
        {
                app.UseForwardedHeaders();

            app.Use(async (context, next) =>
            {
                // Request method, scheme, and path
                _logger.LogInformation("Request Method: {Method}", context.Request.Method);
                _logger.LogInformation("Request Scheme: {Scheme}", context.Request.Scheme);
                _logger.LogInformation("Request Path: {Path}", context.Request.Path);

                // Headers
                foreach (var header in context.Request.Headers)
                {
                    _logger.LogInformation("Header: {Key}: {Value}", header.Key, header.Value);
                }

                // Connection: RemoteIp
                _logger.LogInformation("Request RemoteIp: {RemoteIpAddress}",
                    context.Connection.RemoteIpAddress);

                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseForwardedHeaders();
                app.UseHsts();
            }

            app.Use((ctx, next) => { Console.WriteLine($"Remote Ip {ctx.Connection.RemoteIpAddress}");
                Console.WriteLine($"Local Ip {ctx.Connection.LocalIpAddress}");

                Console.WriteLine($"Remote Ipv4 {ctx.Connection.RemoteIpAddress.MapToIPv4()}");

                return next.Invoke();
            });


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
