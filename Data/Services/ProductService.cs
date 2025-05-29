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
                Pagination = new Menova.Models.ViewModels.PaginationInfo
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
            // Thực hiện truy vấn từ cơ sở dữ liệu để lấy sản phẩm bán chạy nhất
            // Lấy tất cả đơn hàng và chi tiết đơn hàng
            var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync();
            
            // Nhóm các chi tiết đơn hàng theo sản phẩm và tính tổng số lượng bán và doanh thu
            var productSummaries = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSales = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.UnitPrice * od.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(count)
                .ToList();

            // Lấy thông tin chi tiết của các sản phẩm
            var result = new List<ProductSalesSummary>();
            foreach (var summary in productSummaries)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(summary.ProductId);
                if (product != null)
                {
                    result.Add(new ProductSalesSummary
                    {
                        Product = product,
                        TotalSales = summary.TotalSales,
                        Revenue = summary.Revenue
                    });
                }
            }

            // Nếu không có dữ liệu bán hàng hoặc số lượng không đủ, bổ sung thêm sản phẩm nổi bật
            if (result.Count < count)
            {
                var existingProductIds = result.Select(p => p.Product.ProductId).ToList();
                var featuredProducts = (await _unitOfWork.Products.GetFeaturedProductsAsync(count))
                    .Where(p => !existingProductIds.Contains(p.ProductId))
                    .Take(count - result.Count);

                foreach (var product in featuredProducts)
                {
                    result.Add(new ProductSalesSummary
                    {
                        Product = product,
                        TotalSales = 0,
                        Revenue = 0
                    });
                }
            }

            return result;
        }

        public async Task<List<Product>> GetLowStockProductsAsync(int count)
        {
            // Lấy tất cả sản phẩm đang active với variants và thông tin chi tiết
            var products = await _unitOfWork.Products.GetAllWithIncludeAsync(p => p.ProductVariants);
            
            // Lọc sản phẩm active và tính toán hàng tồn kho có vấn đề
            return products
                .Where(p => p.IsActive) // Chỉ xem xét sản phẩm đang active
                .Where(p => {
                    // Kiểm tra nếu sản phẩm có variants
                    if (p.ProductVariants == null || !p.ProductVariants.Any())
                        return false;
                    
                    // Chỉ xét các variant đang active
                    var activeVariants = p.ProductVariants.Where(v => v.IsActive).ToList();
                    
                    if (!activeVariants.Any())
                        return false;
                    
                    // Tiêu chí cảnh báo hết hàng:
                    // 1. Nếu có ít nhất 30% các biến thể có số lượng < 5
                    // 2. Hoặc nếu tổng số lượng / số biến thể < 3 (trung bình mỗi biến thể < 3)
                    int lowStockVariants = activeVariants.Count(v => v.StockQuantity < 5);
                    double percentLowStock = (double)lowStockVariants / activeVariants.Count;
                    
                    int totalStock = activeVariants.Sum(v => v.StockQuantity);
                    double averageStock = (double)totalStock / activeVariants.Count;
                    
                    return percentLowStock > 0.3 || averageStock < 3;
                })
                .OrderBy(p => p.ProductVariants.Where(v => v.IsActive).Average(v => v.StockQuantity))
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
        
        // Phương thức mới chỉ trả về sản phẩm đang active
        public async Task<IEnumerable<Product>> GetAllActiveProductsAsync()
        {
            try
            {
                // Lấy tất cả sản phẩm kèm theo các thông tin liên quan
                var allProducts = await _unitOfWork.Products.GetAllWithIncludeAsync(p => p.Category, p => p.ProductVariants, p => p.Images);
                
                // Chỉ trả về các sản phẩm có trạng thái IsActive = true
                return allProducts.Where(p => p.IsActive);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading active products: {ex.Message}");
                
                // Quay lại cách lấy cơ bản nếu có lỗi
                var products = await _unitOfWork.Products.GetAllAsync();
                
                // Lọc chỉ lấy sản phẩm active
                products = products.Where(p => p.IsActive).ToList();
                
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
