using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FleckWebsocket.Models;
using System.Net;
using Fleck;
using System.Security.Cryptography.X509Certificates;

namespace FleckWebsocket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult test()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult server() {
            //websocket
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
            string ipadd = "ws://panshubeinewfleck.azurewebsites.net";
            //ipadd = "ws://" + ipAddress + ":80";
            //Fleck.WebSocketServer server = new Fleck.WebSocketServer("wss://" + ipAddress + ":8088");
            Fleck.WebSocketServer server = new Fleck.WebSocketServer(ipadd);
            string path = System.Environment.CurrentDirectory;
            //bool isexist = System.IO.File.Exists(path + "//panshubei.club.pfx");
            //server.Certificate = new X509Certificate2(path+"//panshubei.club.pfx", "panshubei");
            try
            {
                server.Start(socket =>
                {
                    Trace.WriteLine("FleckSocket=>server.start");
                    socket.OnOpen = () =>
                    {
                        Trace.WriteLine("FleckSocket=>server.open");
                        sockets.Add(socket);
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
            return View();
        }
    }
}
