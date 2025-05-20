using Menova.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menova.Data.Repositories
{
    public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ProductVariant> GetVariantWithDetailsAsync(int id)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.Size)
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.ProductVariantId == id);
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Include(v => v.Size)
                .Include(v => v.Color)
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.Size.Name)
                .ThenBy(v => v.Color.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductVariant>> GetActiveVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Include(v => v.Size)
                .Include(v => v.Color)
                .Where(v => v.ProductId == productId && v.IsActive)
                .OrderBy(v => v.Size.Name)
                .ThenBy(v => v.Color.Name)
                .ToListAsync();
        }
    }
} 