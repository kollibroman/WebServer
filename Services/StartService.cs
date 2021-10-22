using System.Text;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace WebServer.Services
{
    public class StartService : IHostedService
    {
        private readonly ILogger<StartService> _logger;
        private readonly IConfiguration _config;
        TcpListener server = null;

        public StartService(ILogger<StartService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            
            IPAddress localaddr = IPAddress.Parse(_config.GetValue<string>("ip"));
            server = new TcpListener(localaddr, _config.GetValue<int>("port"));
            server.Start();
            await StartListener();
        }

        public async Task StartListener()
        {
            try
            {
                while (true)
                {
                    _logger.LogInformation("Waiting for connection...");
                    TcpClient client = await server.AcceptTcpClientAsync();
                    _logger.LogInformation("Connected!");

                    Thread t = new Thread(new ParameterizedThreadStart(HandleDevice));
                    t.Start(client);
                }
            }
            catch (SocketException ex)
            {
                _logger.LogInformation("SocketException: {0}", ex);
                server.Stop();
            }
        }

        public void HandleDevice(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();
            string imei = string.Empty;

            string data = null;
            Byte[] bytes= new Byte[256];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    _logger.LogInformation("{1} received: {0}", data, Thread.CurrentThread.ManagedThreadId);

                    string str = "My device";
                    Byte[] reply = Encoding.ASCII.GetBytes(str);
                    stream.WriteAsync(reply, 0, reply.Length);
                    _logger.LogInformation("{1}: Sent {0}", str, Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch (System.Exception ex)
            {
                 _logger.LogInformation("Exception: {0}", ex.Message);
                 client.Close();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}