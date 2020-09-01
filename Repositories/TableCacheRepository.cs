using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SyncDataSample.Models;

namespace SyncDataSample.Repositories
{
    public class TableCacheRepository : TableRepository
    {
        readonly ILogger<TableCacheRepository> _logger;
        readonly IMemoryCache _memoryCache = null;
        readonly MemoryCacheEntryOptions _memoryCacheEntryOptions = null;
        object _lockObj = new object();
        bool __lockWasTaken = false;

        public TableCacheRepository(IMemoryCache memoryCache, IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory)
        {
            _memoryCache = memoryCache;
            _memoryCacheEntryOptions = new MemoryCacheEntryOptions()  { };

            using (var scope = serviceScopeFactory.CreateScope())
            {
                _logger = scope.ServiceProvider.GetRequiredService<ILogger<TableCacheRepository>>();
            }
        }

        public override async Task<IEnumerable<TableDTO>> GetTableDTOAsync()
        {
            string cacheKey = $"Table-1";

            if (!_memoryCache.TryGetValue<IEnumerable<TableDTO>>(cacheKey, out IEnumerable<TableDTO> result))
            {
                _logger.LogInformation("[] Get Data from DB");
                //System.Threading.Monitor.Enter(_lockObj, ref __lockWasTaken);
                result = await base.GetTableDTOAsync();
                if (result != null) _memoryCache.Set(cacheKey, result, _memoryCacheEntryOptions);
                //if (__lockWasTaken) System.Threading.Monitor.Exit(_lockObj);
            }

            return result;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
