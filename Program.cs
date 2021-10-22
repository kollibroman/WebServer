using System.IO;
using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebServer.Services;
using System.Threading.Tasks;

namespace WebServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var host = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    
                })
                .ConfigureLogging(x =>
                {
                    x.AddSerilog(Log.Logger);
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<StartService>();
                })
                .UseConsoleLifetime()
                .UseSerilog();

                var _host = host.Build();

                using(_host)
                {
                 await _host.RunAsync();
                }
        }
    }
}
