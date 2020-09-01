using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SyncDataSample.Repositories;

namespace SyncDataSample.Service
{
    public class MainService : BackgroundService
    {

        int maxTaskCount = 10;
        int maxTaskDelay = 1;
        int threadDelay = 1;
        readonly ILogger<MainService> _logger;
        readonly TableCacheRepository _tableCacheRepository;

        public MainService(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _logger = scope.ServiceProvider.GetRequiredService<ILogger<MainService>>();
                var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                _tableCacheRepository = scope.ServiceProvider.GetRequiredService<TableCacheRepository>();
            }

            int minWorkThread = 0;
            int minIoThread = 0;

            ThreadPool.GetMinThreads(out minWorkThread, out minIoThread);
            ThreadPool.SetMinThreads(20, minIoThread);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[]");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("MainService");
            Console.ResetColor();

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[]");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mainThread = Task.Run(async () =>
            {
                List<Task> tasks = new List<Task>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    var data = Guid.NewGuid();

                    if (data != null)
                    {
                        tasks.Add(Task.Run(async () => await ProcessAsync(data.ToString())));
                    }

                    if (tasks.Count >= maxTaskCount)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();

                        await Task.Delay(TimeSpan.FromMilliseconds(maxTaskDelay));
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(threadDelay));
                }

                await Task.WhenAll(tasks);

            });

            await Task.WhenAny(mainThread);
        }

        private async Task ProcessAsync(string data)
        {
            _logger.LogTrace($"[] {data}");
            await _tableCacheRepository.GetTableDTOAsync();
            await Task.Delay(1);
        }
    }
}
