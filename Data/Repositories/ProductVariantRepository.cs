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
        
        public async Task<bool> CheckStockAvailabilityAsync(int variantId, int requestedQuantity)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant == null)
            {
                return false;
            }
            
            return variant.StockQuantity >= requestedQuantity;
        }
        
        public async Task<bool> UpdateStockQuantityAsync(int variantId, int quantityToReduce)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant == null)
            {
                return false;
            }
            
            if (variant.StockQuantity < quantityToReduce)
            {
                return false;
            }
            
            variant.StockQuantity -= quantityToReduce;
            
            // Nếu hết hàng, có thể cân nhắc đánh dấu biến thể không còn hoạt động
            if (variant.StockQuantity == 0)
            {
                variant.StockQuantity = 0;
                // Tùy chọn: đánh dấu không còn hoạt động
                // variant.IsActive = false;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 