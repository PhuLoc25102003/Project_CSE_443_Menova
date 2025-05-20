using Menova.Models.ViewModels;
using Menova.Models;
using Menova.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menova.Data.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count)
        {
            return await _unitOfWork.Products.GetFeaturedProductsAsync(count);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            return await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId, page, pageSize);
        }

        public async Task<ProductViewModel> GetProductDetailsAsync(int id)
        {
            var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return null;
            }

            // Get available sizes and colors
            var availableSizes = product.ProductVariants
                .Where(v => v.IsActive && v.StockQuantity > 0)
                .Select(v => v.Size)
                .Distinct()
                .ToList();

            var availableColors = product.ProductVariants
                .Where(v => v.IsActive && v.StockQuantity > 0)
                .Select(v => v.Color)
                .Distinct()
                .ToList();

            // Get related products (same category)
            var relatedProducts = await _unitOfWork.Products.GetRelatedProductsAsync(product.ProductId, product.CategoryId, 4);

            var viewModel = new ProductViewModel
            {
                Product = product,
                AvailableSizes = availableSizes,
                AvailableColors = availableColors,
                Variants = product.ProductVariants.Where(v => v.IsActive).ToList(),
                RelatedProducts = (List<Product>) relatedProducts,
                Reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList()
            };

            return viewModel;
        }

        public async Task<CategoryViewModel> GetProductsListAsync(int? categoryId, string sortOrder, int page, int pageSize)
        {
            var products = await _unitOfWork.Products.GetProductsWithSortingAsync(categoryId, sortOrder, page, pageSize);

            var totalItems = 0;
            Category currentCategory = null;
            List<Category> subCategories = null;

            if (categoryId.HasValue)
            {
                totalItems = await _unitOfWork.Products.GetProductsCountByCategoryAsync(categoryId.Value);
                currentCategory = await _unitOfWork.Categories.GetByIdAsync(categoryId.Value);
                subCategories = (await _unitOfWork.Categories.GetSubCategoriesByParentIdAsync(categoryId.Value)).ToList();
            }
            else
            {
                totalItems = (await _unitOfWork.Products.FindAsync(p => p.IsActive)).Count();
            }

            var viewModel = new CategoryViewModel
            {
                CurrentCategory = currentCategory,
                SubCategories = subCategories,
                Products = products.ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                }
            };

            return viewModel;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int page, int pageSize)
        {
            return await _unitOfWork.Products.SearchProductsAsync(searchTerm, page, pageSize);
        }

        public async Task<int> GetSearchProductsCountAsync(string searchTerm)
        {
            return await _unitOfWork.Products.GetSearchProductsCountAsync(searchTerm);
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return products.Count();
        }

        public async Task<List<ProductSalesSummary>> GetTopSellingProductsAsync(int count)
        {
            // Giả lập dữ liệu - trong thực tế sẽ lấy từ OrderDetails
            var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);

            return products.Select(p => new ProductSalesSummary
            {
                Product = p,
                TotalSales = new Random().Next(10, 100), // Giả lập
                Revenue = p.Price * new Random().Next(10, 100) // Giả lập
            }).ToList();
        }

        public async Task<List<Product>> GetLowStockProductsAsync(int count)
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return products
                .Where(p => p.ProductVariants != null && p.ProductVariants.Sum(v => v.StockQuantity) < 10)
                .Take(count)
                .ToList();
        }

        public async Task<Product> GetProductWithDetailsAsync(int id)
        {
            return await _unitOfWork.Products.GetProductWithDetailsAsync(id);
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product != null)
            {
                _unitOfWork.Products.Remove(product);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> SoftDeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.Now;
                _unitOfWork.Products.Update(product);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ToggleProductStatusAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.Now;
                _unitOfWork.Products.Update(product);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }
        
        public async Task<IEnumerable<Product>> GetAllProductsIncludingInactiveAsync()
        {
            try
            {
                // Cố gắng lấy sản phẩm với danh mục được tải sẵn
                return await _unitOfWork.Products.GetAllWithIncludeAsync(p => p.Category, p => p.ProductVariants, p => p.Images);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading products with include: {ex.Message}");
                
                // Quay lại cách lấy cơ bản nếu có lỗi
                var products = await _unitOfWork.Products.GetAllAsync();
                
                // Tải thủ công danh mục cho mỗi sản phẩm
                foreach (var product in products)
                {
                    try
                    {
                        if (product.Category == null && product.CategoryId > 0)
                        {
                            product.Category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Error loading category for product {product.ProductId}: {innerEx.Message}");
                    }
                }
                
                return products;
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product != null;
        }

        public async Task<IEnumerable<Product>> GetFilteredProductsAsync(string searchString, int? categoryId, string sortOrder, int page, int pageSize)
        {
            var products = await _unitOfWork.Products.GetAllAsync();

            // Lọc theo search string
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // Lọc theo category
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // Sắp xếp
            products = sortOrder switch
            {
                "name_asc" => products.OrderBy(p => p.Name),
                "name_desc" => products.OrderByDescending(p => p.Name),
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                _ => products.OrderByDescending(p => p.CreatedAt),
            };

            // Phân trang
            return products.Skip((page - 1) * pageSize).Take(pageSize);
        }

    }

}
