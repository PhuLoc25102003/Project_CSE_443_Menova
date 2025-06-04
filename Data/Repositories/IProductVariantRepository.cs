using Menova.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menova.Data.Repositories
{
    public interface IProductVariantRepository : IRepository<ProductVariant>
    {
        Task<ProductVariant> GetVariantWithDetailsAsync(int id);
        Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(int productId);
        Task<IEnumerable<ProductVariant>> GetActiveVariantsByProductIdAsync(int productId);
        Task<bool> CheckStockAvailabilityAsync(int variantId, int requestedQuantity);
        Task<bool> UpdateStockQuantityAsync(int variantId, int quantityToReduce);
    }
} 