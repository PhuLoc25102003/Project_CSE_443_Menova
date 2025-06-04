using Menova.Data.Services;
using Menova.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Menova.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public SearchController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet("products")]
        public async Task<IActionResult> SearchProducts([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<object>());
            }

            var products = await _productService.SearchProductsAsync(q, 1, 30); // Return top 30 results
            
            // Format products for the API response
            var result = products.Select(p => new
            {
                p.ProductId,
                p.Name,
                p.Description,
                MainImage = p.ImageUrl,
                p.Price,
                p.DiscountPrice,
                InStock = true // Simplify with a default value
            });

            return Ok(result);
        }

        [HttpGet("collections")]
        public async Task<IActionResult> SearchCollections([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<object>());
            }

            // Get all categories and filter manually
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            var categories = allCategories.Where(c => 
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || 
                (c.Description != null && c.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
            ).Take(10);
            
            var result = categories.Select(c => new
            {
                c.CategoryId,
                c.Name,
                c.Description,
                Image = "/img/category-placeholder.jpg", // Use placeholder image
                ProductCount = c.Products != null ? c.Products.Count : 0
            });

            return Ok(result);
        }
    }
} 