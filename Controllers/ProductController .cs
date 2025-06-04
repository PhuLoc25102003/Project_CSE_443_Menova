using Menova.Data;
using Menova.Data.Services;
using Menova.Models;
using Menova.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Menova.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private const int PageSize = 12; // Set page size to 12 products

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? categoryId, string sortOrder, bool inStockOnly = false, decimal? priceFrom = null, decimal? priceTo = null, int page = 1, bool newArrivals = false)
        {
            ViewData["SortOrder"] = sortOrder;
            ViewBag.InStockOnly = inStockOnly;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;
            ViewBag.NewArrivals = newArrivals;
            
            // Get all categories for filter sidebar
            ViewBag.AllCategories = await _categoryService.GetAllCategoriesAsync();
            
            // Get category product counts for display in sidebar
            ViewBag.CategoryCounts = await _productService.GetProductCountByCategories();
            
            var viewModel = await _productService.GetProductsListAsync(categoryId, sortOrder, page, PageSize, inStockOnly, priceFrom, priceTo, newArrivals);

            return View(viewModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var viewModel = await _productService.GetProductDetailsAsync(id);

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Category(int id, string sortOrder = null, bool inStockOnly = false, decimal? priceFrom = null, decimal? priceTo = null, int page = 1)
        {
            return RedirectToAction("Index", new { 
                categoryId = id, 
                sortOrder = sortOrder,
                inStockOnly = inStockOnly,
                priceFrom = priceFrom,
                priceTo = priceTo,
                page = page 
            });
        }

        public async Task<IActionResult> Search(string q, string sortOrder = null, bool inStockOnly = false, decimal? priceFrom = null, decimal? priceTo = null, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index");
            }

            ViewData["SortOrder"] = sortOrder;
            ViewBag.InStockOnly = inStockOnly;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;
            ViewData["SearchTerm"] = q;
            
            // Get all categories for filter sidebar
            ViewBag.AllCategories = await _categoryService.GetAllCategoriesAsync();
            
            // Get category product counts for display in sidebar
            ViewBag.CategoryCounts = await _productService.GetProductCountByCategories();
            
            var products = await _productService.SearchProductsAsync(q, page, PageSize);
            var totalItems = await _productService.GetSearchProductsCountAsync(q);

            var viewModel = new CategoryViewModel
            {
                Products = new List<Product>(products),
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = totalItems
                }
            };

            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> QuickAdd(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            // Check if product is out of stock
            bool isOutOfStock = product.ProductVariants == null || 
                               !product.ProductVariants.Any(v => v.IsActive && v.StockQuantity > 0);
            
            if (isOutOfStock)
            {
                return BadRequest("Product is out of stock");
            }
            
            return PartialView("_QuickAddModal", product);
        }
    }
}

