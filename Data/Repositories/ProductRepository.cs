using Menova.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Menova.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetAllWithIncludeAsync(params Expression<Func<Product, object>>[] includeProperties)
        {
            IQueryable<Product> query = _context.Products;
            
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count)
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<int> GetProductsCountByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .CountAsync();
        }

        public async Task<Product> GetProductWithDetailsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId && p.ProductId != productId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int page, int pageSize)
        {
            return await _context.Products
                .Where(p => p.IsActive &&
                           (p.Name.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm) ||
                            p.Category.Name.Contains(searchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public async Task<int> GetSearchProductsCountAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => p.IsActive &&
                           (p.Name.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm) ||
                            p.Category.Name.Contains(searchTerm)))
                .CountAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithSortingAsync(int? categoryId, string sortOrder, int page, int pageSize)
        {
            var query = _context.Products.Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            switch (sortOrder)
            {
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "name_asc":
                    query = query.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(p => p.Name);
                    break;
                default:
                    query = query.OrderByDescending(p => p.CreatedAt); // Default: newest first
                    break;
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();
        }

        public IQueryable<Product> GetQueryableProductsWithDetails()
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color);
        }
    }
}
