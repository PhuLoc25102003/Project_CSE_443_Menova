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


        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(int? categoryId, string sortOrder, int page = 1)
        {
            ViewData["SortOrder"] = sortOrder;
            var viewModel = await _productService.GetProductsListAsync(categoryId, sortOrder, page, 12);

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

        public async Task<IActionResult> Category(int id, int page = 1)
        {
            return RedirectToAction("Index", new { categoryId = id, page = page });
        }

        public async Task<IActionResult> Search(string q, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index");
            }

            var products = await _productService.SearchProductsAsync(q, page, 12);
            var totalItems = await _productService.GetSearchProductsCountAsync(q);

            var viewModel = new Menova.Models.ViewModels.CategoryViewModel
            {
                Products = new System.Collections.Generic.List<Menova.Models.Product>(products),
                Pagination = new Menova.Models.ViewModels.PaginationInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = 12,
                    TotalItems = totalItems
                }
            };

            ViewData["SearchTerm"] = q;
            return View("Index", viewModel);
        }
    }


}

