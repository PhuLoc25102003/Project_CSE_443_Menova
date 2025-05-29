using Menova.Data;
using Menova.Data.Services;
using Menova.Models;
using Menova.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, int page = 1, int pageSize = 20)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentSort"] = sortOrder;

            try
            {
                // Get active products only by default, which will exclude deleted products
                var products = await _productService.GetAllActiveProductsAsync();

                // Ensure Category is loaded (this should be done in the service instead ideally)


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
                ViewBag.Categories = await _categoryService.GetSubCategoriesForSelectListAsync();

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

            // Lấy danh sách chỉ các danh mục con để hiển thị dropdown
            ViewBag.Categories = await _categoryService.GetSubCategoriesForSelectListAsync();

            // Sử dụng ViewModel thay vì Product
            return View(new ProductCreateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateEditViewModel viewModel)
        {
            // Đảm bảo ảnh được tải lên
            if (viewModel.ProductImage == null || viewModel.ProductImage.Length == 0)
            {
                ModelState.AddModelError("ProductImage", "Vui lòng tải lên hình ảnh sản phẩm.");
            }
            
            // Loại bỏ lỗi ImageUrl nếu có vì chúng ta xử lý bằng ProductImage
            ModelState.Remove("ImageUrl");

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
                ViewBag.Categories = await _categoryService.GetSubCategoriesForSelectListAsync();
                return View(viewModel);
            }

            try
            {
                // Xử lý upload ảnh
                // Đảm bảo thư mục tồn tại
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "productImages");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất để tránh trùng lặp
                string uniqueFileName = GetUniqueFileName(viewModel.ProductImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vào thư mục
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ProductImage.CopyToAsync(fileStream);
                }

                // Cập nhật đường dẫn ảnh vào viewModel
                viewModel.ImageUrl = $"/assets/productImages/{uniqueFileName}";

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
                ViewBag.Categories = await _categoryService.GetSubCategoriesForSelectListAsync();
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

            // Lấy danh sách chỉ các danh mục con để hiển thị dropdown
            ViewBag.Categories = await _categoryService.GetSubCategoriesForSelectListAsync();

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

            // Kiểm tra các trường ẩn để xác định hành động người dùng
            var isEditing = Request.Form.ContainsKey("IsEditing");
            var keepExistingImage = Request.Form.ContainsKey("KeepExistingImage");
            
            // Nếu đang chỉnh sửa mà không thay đổi ảnh, cho phép giữ nguyên ảnh cũ
            var skipImageValidation = (viewModel.ProductImage == null && !string.IsNullOrEmpty(viewModel.ImageUrl)) || 
                                       isEditing && keepExistingImage;
            
            // Check if ImageUrl is explicitly empty (user removed the image)
            var isImageRemoved = viewModel.ImageUrl == string.Empty;
            
            // Log current status
            Console.WriteLine($"DEBUG: ImageUrl: '{viewModel.ImageUrl ?? "null"}'");
            Console.WriteLine($"DEBUG: ProductImage: {(viewModel.ProductImage != null ? "has file" : "null")}");
            Console.WriteLine($"DEBUG: skipImageValidation: {skipImageValidation}");
            Console.WriteLine($"DEBUG: isImageRemoved: {isImageRemoved}");
            Console.WriteLine($"DEBUG: isEditing: {isEditing}");
            Console.WriteLine($"DEBUG: keepExistingImage: {keepExistingImage}");
            
            // Don't validate image if we're keeping old image or explicitly removing it
            // Kiểm tra xem có đang sửa sản phẩm không và tự động lấy ảnh cũ
            var currentProduct = await _productService.GetProductWithDetailsAsync(id);
            if (currentProduct != null)
            {
                if (viewModel.ProductImage == null && !string.IsNullOrEmpty(currentProduct.ImageUrl))
                {
                    // Tự động khôi phục URL ảnh cũ nếu không có ảnh mới
                    viewModel.ImageUrl = currentProduct.ImageUrl;
                    Console.WriteLine($"DEBUG: Auto-restored image URL: {viewModel.ImageUrl}");
                }
                
                // Trong trường hợp chỉnh sửa, xóa bỏ validation cho ProductImage
                // Vì chúng ta đang sửa sản phẩm hiện có nên không cần validation ảnh
                ModelState.Remove("ProductImage");
            }
            else if (viewModel.ProductImage == null || viewModel.ProductImage.Length == 0)
            {
                // Chỉ validate khi tạo mới sản phẩm
                ModelState.AddModelError("ProductImage", "Vui lòng tải lên hình ảnh sản phẩm.");
            }
            
            // Loại bỏ lỗi ImageUrl vì chúng ta xử lý bằng ProductImage hoặc giữ ảnh cũ
            ModelState.Remove("ImageUrl");

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
                ViewBag.Categories = await _categoryService.GetSubCategoriesForSelectListAsync();
                return View(viewModel);
            }

            try
            {
                // Sử dụng lại đối tượng currentProduct đã được truy vấn ở trên
                if (currentProduct == null)
                {
                    return NotFound();
                }
                
                // Gán lại cho biến existingProduct để giữ tên không đổi trong code phía dưới
                var existingProduct = currentProduct;

                // Lưu lại đường dẫn ảnh cũ để xóa nếu cần
                string oldImageUrl = existingProduct.ImageUrl;
                Console.WriteLine($"DEBUG: Old image URL: {oldImageUrl}");

                // Xử lý upload ảnh nếu có
                if (viewModel.ProductImage != null && viewModel.ProductImage.Length > 0)
                {
                    // Đảm bảo thư mục tồn tại
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "productImages");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file duy nhất để tránh trùng lặp
                    string uniqueFileName = GetUniqueFileName(viewModel.ProductImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file vào thư mục
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ProductImage.CopyToAsync(fileStream);
                    }

                    // Cập nhật đường dẫn ảnh vào viewModel
                    viewModel.ImageUrl = $"/assets/productImages/{uniqueFileName}";
                    Console.WriteLine($"DEBUG: New image URL: {viewModel.ImageUrl}");
                }

                // Xóa ảnh cũ nếu có sự thay đổi về ảnh
                try
                {
                    // Trường hợp 1: Người dùng đã tải lên ảnh mới thay thế ảnh cũ
                    if (!string.IsNullOrEmpty(oldImageUrl) && 
                        !string.IsNullOrEmpty(viewModel.ImageUrl) && 
                        oldImageUrl != viewModel.ImageUrl)
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImageUrl.TrimStart('/'));
                        Console.WriteLine($"DEBUG: Attempting to delete old image at: {oldImagePath}");
                        
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                            Console.WriteLine("DEBUG: Old image file deleted successfully");
                        }
                        else
                        {
                            Console.WriteLine("DEBUG: Old image file not found");
                        }
                    }
                    // Trường hợp 2: Người dùng đã xóa ảnh mà không tải lên ảnh mới
                    else if (!string.IsNullOrEmpty(oldImageUrl) && string.IsNullOrEmpty(viewModel.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImageUrl.TrimStart('/'));
                        Console.WriteLine($"DEBUG: Attempting to delete old image at: {oldImagePath}");
                        
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                            Console.WriteLine("DEBUG: Old image file deleted successfully");
                        }
                        else
                        {
                            Console.WriteLine("DEBUG: Old image file not found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ghi nhận lỗi nhưng không dừng quá trình cập nhật sản phẩm
                    Console.WriteLine($"ERROR: Failed to delete old image: {ex.Message}");
                }

                // Update only the fields that should be updated
                existingProduct.Name = viewModel.Name;
                existingProduct.Description = viewModel.Description;
                existingProduct.Price = viewModel.Price;
                existingProduct.DiscountPrice = viewModel.DiscountPrice > 0 ? viewModel.DiscountPrice : viewModel.Price;
                existingProduct.SKU = viewModel.SKU;
                existingProduct.CategoryId = viewModel.CategoryId;
                
                // Xử lý ImageUrl cho phù hợp với các tình huống
                Console.WriteLine($"DEBUG: Processing ImageUrl - viewModel.ImageUrl: '{viewModel.ImageUrl ?? "null"}'");
                
                var shouldKeepExistingImage = Request.Form.ContainsKey("KeepExistingImage");
                
                // Kiểm tra nếu cần giữ lại ảnh cũ
                if (shouldKeepExistingImage && !string.IsNullOrEmpty(oldImageUrl))
                {
                    // Chỉ giữ lại ảnh cũ nếu flag keepExistingImage được đặt
                    Console.WriteLine("DEBUG: Keeping existing image as requested by form flag");
                    existingProduct.ImageUrl = oldImageUrl;
                }
                else if (viewModel.ImageUrl == string.Empty)
                {
                    // Người dùng muốn xóa ảnh
                    Console.WriteLine("DEBUG: User explicitly removed the image");
                    
                    // Xóa tệp ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        try 
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImageUrl.TrimStart('/'));
                            Console.WriteLine($"DEBUG: Removing old image file at: {oldImagePath}");
                            
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                                Console.WriteLine("DEBUG: Old image file successfully deleted");
                            }
                            else
                            {
                                Console.WriteLine("DEBUG: Old image file not found at path: " + oldImagePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ERROR: Failed to delete old image file: {ex.Message}");
                        }
                    }
                    
                    existingProduct.ImageUrl = null;
                }
                else if (!string.IsNullOrEmpty(viewModel.ImageUrl))
                {
                    // Người dùng tải lên ảnh mới hoặc giữ ảnh cũ
                    existingProduct.ImageUrl = viewModel.ImageUrl;
                    Console.WriteLine($"DEBUG: Setting ImageUrl to: {existingProduct.ImageUrl}");
                }
                
                existingProduct.IsActive = viewModel.IsActive;
                existingProduct.UpdatedAt = DateTime.Now;

                // Update the product in the database
                await _productService.UpdateProductAsync(existingProduct);
                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception when updating product: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật sản phẩm: {ex.Message}";

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    TempData["ErrorMessage"] += $" Chi tiết lỗi: {ex.InnerException.Message}";
                }

                // If we got here, something failed, redisplay form
                ViewBag.Categories = await _categoryService.GetSubCategoriesForSelectListAsync();
                return View(viewModel);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try 
            {
                var product = await _productService.GetProductWithDetailsAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                
                // Use soft delete instead of permanent delete
                await _productService.SoftDeleteProductAsync(id);
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa sản phẩm: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
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

        [HttpGet]
        [Route("Admin/Product/Variants/{id:int}")]
        public async Task<IActionResult> Variants(int id, int page = 1)
        {
            // Thay thế phương thức GetByIdAsync đơn giản bằng GetProductWithDetailsAsync để load cả biến thể
            var product = await _productService.GetProductWithDetailsAsync(id);
            if (product == null)
                return NotFound();

            // Cấu hình phân trang
            int pageSize = 4; // Số biến thể mỗi trang
            
            // Tính toán thông tin phân trang nếu có biến thể
            if (product.ProductVariants != null && product.ProductVariants.Any())
            {
                var totalItems = product.ProductVariants.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                // Đảm bảo page nằm trong khoảng hợp lệ
                page = Math.Max(1, Math.Min(page, totalPages));
                
                // Sắp xếp và phân trang biến thể
                product.ProductVariants = product.ProductVariants
                    .OrderBy(v => v.Color?.Name)
                    .ThenBy(v => v.Size?.SortOrder) // Sort by SortOrder for logical size ordering
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.PageSize = pageSize;
            }

            var sizes = await _unitOfWork.Sizes.GetAllAsync();
            var colors = await _unitOfWork.Colors.GetAllAsync();

            // Sửa tên màu bị lỗi encoding
            foreach (var color in colors)
            {
                color.Name = FixVietnameseEncoding(color.Name);
            }

            ViewBag.Sizes = sizes;
            ViewBag.Colors = colors;

            return View(product);
        }

        private string FixVietnameseEncoding(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var replacements = new Dictionary<string, string>
        {
        { "Ð?", "Đỏ" },
        { "Xanh duong", "Xanh dương" },
        { "Tr?ng", "Trắng" },
        { "H?ng", "Hồng" },
        { "B?c", "Bạc" },
        { "Vàng d?ng", "Vàng đồng" },
        { "Xanh ng?c", "Xanh ngọc" },
        { "Ð? dô", "Đỏ đô" },
        { "H?ng pastel", "Hồng pastel" }
        };

            foreach (var pair in replacements)
            {
                input = input.Replace(pair.Key, pair.Value);
            }

            return input;
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
                    IsActive = isActive,
                    SKU = $"{product.SKU}-{sizeId}-{colorId}"
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

        // Helper method to generate unique file name
        private string GetUniqueFileName(string fileName)
        {
            // Tạo tên file duy nhất bằng cách thêm timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            
            // Loại bỏ các ký tự đặc biệt và dấu tiếng Việt trong tên file
            fileNameWithoutExtension = FixVietnameseEncoding(fileNameWithoutExtension)
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(";", "")
                .Replace(":", "")
                .ToLower();
                
            return $"{fileNameWithoutExtension}-{timestamp}{extension}";
        }
    }
}
