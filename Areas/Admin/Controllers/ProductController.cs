using Menova.Data;
using Menova.Data.Services;
using Menova.Models;
using Menova.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IUnitOfWork unitOfWork)
        {
            _productService = productService;
            _categoryService = categoryService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, int page = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentSort"] = sortOrder;

            try
            {
                // Get all products including inactive ones with Category loaded
                var products = await _productService.GetAllProductsIncludingInactiveAsync();
                
                // Ensure Category is loaded (this should be done in the service instead ideally)
                foreach (var product in products)
                {
                     Console.WriteLine(product.Name +"---------------------------------------------");
                    if (product.Category == null && product.CategoryId > 0)
                    {
                        product.Category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
                    }
                }
                
                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    products = products.Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
                }

                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId.Value);
                }

                // Apply sorting
                products = sortOrder switch
                {
                    "name_asc" => products.OrderBy(p => p.Name),
                    "name_desc" => products.OrderByDescending(p => p.Name),
                    "price_asc" => products.OrderBy(p => p.Price),
                    "price_desc" => products.OrderByDescending(p => p.Price),
                    "date_asc" => products.OrderBy(p => p.CreatedAt),
                    "date_desc" => products.OrderByDescending(p => p.CreatedAt),
                    _ => products.OrderByDescending(p => p.CreatedAt), // Default sort
                };
               
                 foreach (var product in products)
                {
                     Console.WriteLine(product.Name);
                }
                // Apply pagination
                var pagedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.TotalItems = products.Count();
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = (int)Math.Ceiling((double)products.Count() / pageSize);

                // Get categories for filter dropdown
                ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

                return View(pagedProducts);
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"Error in Index action: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi tải danh sách sản phẩm: {ex.Message}";
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                // Return an empty list to prevent crashing
                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            // Bỏ qua validation cho Category
            ModelState.Remove("Category");
            
            // Lấy danh sách categories để hiển thị dropdown
            ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

            // Sử dụng ViewModel thay vì Product
            return View(new ProductCreateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateEditViewModel viewModel)
        {
            // Debug model validation errors
            if (!ModelState.IsValid)
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error: {error}");
                }

                // Store validation errors in TempData to display them to the user
                TempData["ValidationErrors"] = errors;
                
                // If model state is invalid, redisplay form with categories
                ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();
                return View(viewModel);
            }
            
            try
            {
                // Chuyển đổi ViewModel sang Product model
                var product = viewModel.ToProduct();
                
                // Đảm bảo ngày tạo được thiết lập
                product.CreatedAt = DateTime.Now;
                
                await _productService.AddProductAsync(product);
                TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception when adding product: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi thêm sản phẩm: {ex.Message}";
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    TempData["ErrorMessage"] += $" Chi tiết lỗi: {ex.InnerException.Message}";
                }
                
                // If we got here, something failed, redisplay form
                ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Bỏ qua validation cho Category
            ModelState.Remove("Category");
            
            var product = await _productService.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Chuyển đổi Product sang ViewModel
            var viewModel = ProductCreateEditViewModel.FromProduct(product);

            // Lấy danh sách categories để hiển thị dropdown
            ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateEditViewModel viewModel)
        {
            if (id != viewModel.ProductId)
            {
                return NotFound();
            }

            // Debug model validation errors
            if (!ModelState.IsValid)
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
                
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error: {error}");
                }

                // Store validation errors in TempData to display them to the user
                TempData["ValidationErrors"] = errors;
                
                // Nếu model state không hợp lệ, cần lấy lại danh sách categories
                ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();
                return View(viewModel);
            }
            
            try
            {
                // Get the existing product from the database
                var existingProduct = await _productService.GetProductWithDetailsAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                
                // Update only the fields that should be updated
                existingProduct.Name = viewModel.Name;
                existingProduct.Description = viewModel.Description;
                existingProduct.Price = viewModel.Price;
                existingProduct.DiscountPrice = viewModel.DiscountPrice > 0 ? viewModel.DiscountPrice : viewModel.Price;
                existingProduct.SKU = viewModel.SKU;
                existingProduct.CategoryId = viewModel.CategoryId;
                existingProduct.ImageUrl = viewModel.ImageUrl;
                existingProduct.IsActive = viewModel.IsActive;
                existingProduct.UpdatedAt = DateTime.Now;
                
                // Update the product in the database
                await _productService.UpdateProductAsync(existingProduct);
                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật sản phẩm: {ex.Message}";
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    TempData["ErrorMessage"] += $" Chi tiết lỗi: {ex.InnerException.Message}";
                }
                
                // Nếu có lỗi, cần lấy lại danh sách categories
                ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Use soft delete instead of permanent delete
            await _productService.SoftDeleteProductAsync(id);
            TempData["SuccessMessage"] = "Sản phẩm đã được ẩn thành công.";
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                await _productService.ToggleProductStatusAsync(id);
                TempData["SuccessMessage"] = "Trạng thái sản phẩm đã được cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Variants(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Get all sizes and colors for dropdowns
            ViewBag.Sizes = await _unitOfWork.Sizes.GetAllAsync();
            ViewBag.Colors = await _unitOfWork.Colors.GetAllAsync();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariant(int productId, int sizeId, int colorId, int stockQuantity, bool isActive)
        {
            try
            {
                var product = await _productService.GetProductWithDetailsAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                // Check if this variant combination already exists
                var existingVariant = product.ProductVariants?
                    .FirstOrDefault(v => v.SizeId == sizeId && v.ColorId == colorId);

                if (existingVariant != null)
                {
                    TempData["ErrorMessage"] = "Biến thể với kích thước và màu sắc này đã tồn tại.";
                    return RedirectToAction(nameof(Variants), new { id = productId });
                }

                // Create new variant
                var newVariant = new ProductVariant
                {
                    ProductId = productId,
                    SizeId = sizeId,
                    ColorId = colorId,
                    StockQuantity = stockQuantity,
                    IsActive = isActive
                };

                await _unitOfWork.ProductVariants.AddAsync(newVariant);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Biến thể mới đã được thêm thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm biến thể: {ex.Message}";
            }

            return RedirectToAction(nameof(Variants), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVariantStock(int productVariantId, int stockQuantity)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariants.GetByIdAsync(productVariantId);
                if (variant == null)
                {
                    return NotFound();
                }

                variant.StockQuantity = stockQuantity;
                _unitOfWork.ProductVariants.Update(variant);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Số lượng tồn kho đã được cập nhật thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật số lượng: {ex.Message}";
            }

            var variant2 = await _unitOfWork.ProductVariants.GetByIdAsync(productVariantId);
            return RedirectToAction(nameof(Variants), new { id = variant2.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVariantStatus(int productVariantId)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariants.GetByIdAsync(productVariantId);
                if (variant == null)
                {
                    return NotFound();
                }

                variant.IsActive = !variant.IsActive;
                _unitOfWork.ProductVariants.Update(variant);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = $"Trạng thái biến thể đã được thay đổi thành {(variant.IsActive ? "hiển thị" : "ẩn")}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thay đổi trạng thái: {ex.Message}";
            }

            var variant2 = await _unitOfWork.ProductVariants.GetByIdAsync(productVariantId);
            return RedirectToAction(nameof(Variants), new { id = variant2.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int productVariantId)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariants.GetByIdAsync(productVariantId);
                if (variant == null)
                {
                    return NotFound();
                }

                int productId = variant.ProductId;
                
                _unitOfWork.ProductVariants.Remove(variant);
                await _unitOfWork.CompleteAsync();

                TempData["SuccessMessage"] = "Biến thể đã được xóa thành công.";
                return RedirectToAction(nameof(Variants), new { id = productId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa biến thể: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> LowStock()
        {
            var products = await _productService.GetLowStockProductsAsync(20);
            return View(products);
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _productService.ProductExistsAsync(id);
        }
    }
}
