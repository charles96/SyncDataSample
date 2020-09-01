using System.Collections.Generic;
using System.Threading.Tasks;
using SyncDataSample.Models;

namespace SyncDataSample.Repositories
{
    public interface ITableRepository
    {
        Task<IEnumerable<TableDTO>> GetTableDTOAsync();
    }
}
