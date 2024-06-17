using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace CS_OGFN_Backend
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "CS OGFN Backend";
            try 
            {
                StartTcpServer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            // split http server and tcp server
            try
            {
                await StartHttpServer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        public static bool IsPortAvailable(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners();
            return endPoints.All(endPoint => endPoint.Port != port);
        }

        public static void StartTcpServer()
        {
            const int port = 3551;
            if (!IsPortAvailable(port))
            {
                Console.WriteLine($"Port {port} is already in use.");
                return;
            }

            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine("Tcp Server started on port " + port);
        }

        public static async Task StartHttpServer()
        {
            const int port = 80;
            if (!IsPortAvailable(port))
            {
                Console.WriteLine($"Port {port} is already in use.");
                return;
            }

            HttpListener server = new HttpListener();
            server.Prefixes.Add($"http://*:{port}/");
            server.Start();
            Console.WriteLine("Http Server started on port " + port);

            await ProcessRequests(server);
        }

        private static async Task ProcessRequests(HttpListener server)
        {
            try
            {
                while (true)
                {
                    HttpListenerContext context = await server.GetContextAsync();
                    await HandleRequest(context);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        private static async Task HandleRequest(HttpListenerContext context)
        {
            string htmlContent = GenerateHtmlContent();
            byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);

            HttpListenerResponse response = context.Response;
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)HttpStatusCode.OK;

            using (System.IO.Stream output = response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private static string GenerateHtmlContent()
        {
            return @"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Backend Dashboard</title>
            </head>
            <body>
                <h1>Welcome to the Backend Dashboard</h1>
                <p>This is a simple HTML response from your backend server.</p>
            </body>
            </html>";
        }
    }
}
