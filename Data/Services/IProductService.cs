using Menova.Models.ViewModels;
using Menova.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Menova.Areas.Admin.Models;

namespace Menova.Data.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int page, int pageSize);
        Task<ProductViewModel> GetProductDetailsAsync(int id);
        Task<CategoryViewModel> GetProductsListAsync(int? categoryId, string sortOrder, int page, int pageSize);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int page, int pageSize);
        Task<int> GetSearchProductsCountAsync(string searchTerm);
        
        // Thêm các phương thức mới cho Dashboard
        Task<int> GetTotalProductCountAsync();
        Task<List<ProductSalesSummary>> GetTopSellingProductsAsync(int count);
        Task<List<Product>> GetLowStockProductsAsync(int count);
        
        // Thêm các phương thức cho Admin Product Management
        Task<Product> GetProductWithDetailsAsync(int id);
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> SoftDeleteProductAsync(int id);
        Task<bool> ToggleProductStatusAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        Task<IEnumerable<Product>> GetFilteredProductsAsync(string searchString, int? categoryId, string sortOrder, int page, int pageSize);
        Task<IEnumerable<Product>> GetAllProductsIncludingInactiveAsync();
        Task<IEnumerable<Product>> GetAllActiveProductsAsync();
    }
}
