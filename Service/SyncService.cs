using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SyncDataSample.Engine;
using SyncDataSample.Repositories;

namespace SyncDataSample.Service
{
    public class SyncService : BackgroundService
    {
        protected readonly IConfiguration _configuration = null;
        readonly ILogger<SyncService> _logger;
        readonly CacheManager _cacheManager = null;
        readonly TableCacheRepository _tableCacheRepository;


        public SyncService(IServiceScopeFactory serviceScopeFactory, CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _cacheManager.OnReceivedClientMessage += _cacheManager_OnReceivedClientMessage;

            using (var scope = serviceScopeFactory.CreateScope())
            {
                _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                _logger = scope.ServiceProvider.GetRequiredService<ILogger<SyncService>>();
                _tableCacheRepository = scope.ServiceProvider.GetRequiredService<TableCacheRepository>();

            }
        }

        private void _cacheManager_OnReceivedClientMessage(Models.ClientEventArgs args)
        {
            _logger.LogInformation($"[] {args.Body}");

            _tableCacheRepository.Remove("Table-1");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[]");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("SyncService");
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
            _logger.LogInformation("[]");

            await _cacheManager.ClientListeningAsync(stoppingToken);
        }
    }
}
