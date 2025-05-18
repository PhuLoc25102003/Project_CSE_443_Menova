using Menova.Data.Services;
using Menova.Models;
using Microsoft.AspNetCore.Mvc;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, int page = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentSort"] = sortOrder;

            // Lấy danh sách sản phẩm với phân trang và sắp xếp
            var products = await _productService.GetFilteredProductsAsync(searchString, categoryId, sortOrder, page, pageSize);

            // Lấy danh sách categories để hiển thị dropdown filter
            ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

            return View(products);
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
            // Lấy danh sách categories để hiển thị dropdown
            ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;

                await _productService.AddProductAsync(product);
                return RedirectToAction(nameof(Index));
            }

            // Nếu model state không hợp lệ, cần lấy lại danh sách categories
            ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách categories để hiển thị dropdown
            ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.UpdatedAt = DateTime.Now;
                    await _productService.UpdateProductAsync(product);
                }
                catch (Exception)
                {
                    if (!await ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu model state không hợp lệ, cần lấy lại danh sách categories
            ViewBag.Categories = await _categoryService.GetAllCategoriesForSelectListAsync();

            return View(product);
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
            await _productService.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Variants(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
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
