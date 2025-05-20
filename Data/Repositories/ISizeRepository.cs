using Menova.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menova.Data.Repositories
{
    public interface ISizeRepository : IRepository<Size>
    {
        Task<IEnumerable<Size>> GetActiveSizesAsync();
    }
} 