using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;
using NLog.Extensions.Logging;
using SyncDataSample.Service;

namespace SyncDataSample
{
    class Program
    {
        static async Task  Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                await host.RunAsync();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(System.IO.Directory.GetCurrentDirectory());
                    configHost.AddEnvironmentVariables(prefix: "NETCORE_");
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile($"appsettings_{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<HostOptions>((options) => options.ShutdownTimeout = TimeSpan.FromMinutes(10));
                    services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                        loggingBuilder.AddNLog();
                    });

                    #region Dependency Injection

                    var configuration = hostContext.Configuration;

                    services.AddSingleton<HttpListener>();
                    services.AddSingleton<CacheManager>();
                    services.AddHostedService<SyncService>();
                    services.AddHostedService<MainService>();
                    
                    // *** HostedService - ProcessService
                    //services.AddHostedService<ProcessService>();
                    //services.AddMemoryCache().BuildServiceProvider();

                    #endregion Dependency Injection
                })
                .UseNLog();

    }
}
