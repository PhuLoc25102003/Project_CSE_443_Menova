using Menova.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menova.Data.Repositories
{
    public class ColorRepository : Repository<Color>, IColorRepository
    {
        public ColorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Color>> GetActiveColorsAsync()
        {
            return await _context.Colors.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        }
    }
} 