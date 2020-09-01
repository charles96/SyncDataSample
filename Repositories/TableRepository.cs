using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SyncDataSample.Models;

namespace SyncDataSample.Repositories
{
    public class TableRepository : ITableRepository
    {
        readonly ILogger<TableRepository> _logger;

        public TableRepository(IServiceScopeFactory serviceScopeFactory) 
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _logger = scope.ServiceProvider.GetRequiredService<ILogger<TableRepository>>();
            }
        }

        public async virtual Task<IEnumerable<TableDTO>> GetTableDTOAsync()
        {
            _logger.LogInformation("[] ");

            IEnumerable<TableDTO> ret = null;

            using (var connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Source\SyncDataSample\Repositories\Database.mdf;Integrated Security=True"))
            {
                ret = await connection.QueryAsync<TableDTO>(@"select id, value from test;");
            }

            return ret;
        }
    }
}
