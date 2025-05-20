using Menova.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menova.Data.Repositories
{
    public class SizeRepository : Repository<Size>, ISizeRepository
    {
        public SizeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Size>> GetActiveSizesAsync()
        {
            return await _context.Sizes.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        }
    }
} 