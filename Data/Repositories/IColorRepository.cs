using Menova.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menova.Data.Repositories
{
    public interface IColorRepository : IRepository<Color>
    {
        Task<IEnumerable<Color>> GetActiveColorsAsync();
    }
} 