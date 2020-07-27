using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Fleck;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleckWebsocket
{
    public class Startup
    {
        private readonly ILogger _logger;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            string ipAddress = string.Empty;
            var host = System.Net.Dns.GetHostEntry(Dns.GetHostName());

            List<IWebSocketConnection> sockets = new List<IWebSocketConnection>();

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = ip.ToString();
                    break;
                }
            }
            //ipadd = "wss://" + ipAddress + ":80";
            string ipadd = "wss://panshubeinewfleck.azurewebsites.net:443";
            ipadd = "wss://" + ipAddress + ":443";
            Fleck.WebSocketServer server = new Fleck.WebSocketServer(ipadd);
            string path = System.Environment.CurrentDirectory;
            bool isexist = System.IO.File.Exists(path + "//panshubeinewfleck.pfx");
            if (isexist)
            {
                // if use `wss` ,need  Certificate
                server.Certificate = new X509Certificate2(path + "//panshubeinewfleck.pfx", "panshubei");
            }
            // if use `wss` ,need  Certificate
            //server.Certificate=new X509Certificate2("panshubeinewfleck.pfx","panshubei");


            try
            {
                server.Start(socket =>
                {

                    Trace.WriteLine("FleckSocket=>server.start");
                    socket.OnOpen = () =>
                    {
                        try
                        {
                            Trace.WriteLine("FleckSocket=>server.open");
                            sockets.Add(socket);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("FleckSocket=>server.open err" + ex);
                            throw;
                        }

                    };
                    socket.OnClose = () =>
                    {
                        Trace.WriteLine("FleckSocket=>server.close");
                        sockets.Remove(socket);
                    };
                    socket.OnMessage = message =>
                    {
                        Trace.WriteLine("FleckSocket=>server.message and the message is : " + message);
                        sockets.ToList().ForEach(s => s.Send(" client says: " + message));
                    };

                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine("FleckSocket=>err" + ex.ToString());
                throw;
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
