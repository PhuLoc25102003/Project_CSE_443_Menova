using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int page, int pageSize);
        Task<int> GetProductsCountByCategoryAsync(int categoryId);
        Task<Product> GetProductWithDetailsAsync(int id);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int page, int pageSize);
        Task<int> GetSearchProductsCountAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsWithSortingAsync(int? categoryId, string sortOrder, int page, int pageSize);
    }
}
