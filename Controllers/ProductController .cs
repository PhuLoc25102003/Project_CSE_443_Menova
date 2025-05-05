using Menova.Data;
using Menova.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Menova.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string sortOrder, int page = 1)
        {
            int pageSize = 12;
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
                case "name_desc":
                    query = query.OrderByDescending(p => p.Name);
                    break;
                case "name_asc":
                    query = query.OrderBy(p => p.Name);
                    break;
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;

            }

            var totalItems = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();

           var currentCategory = categoryId.HasValue
                ? await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId.Value)
                : null;

            var subCategories = categoryId.HasValue
                ? await _context.Categories.Where(c => c.ParentCategoryId == categoryId.Value && c.IsActive).ToListAsync()
                : null;

            var viewModel = new CategoryViewModel
            {
                CurrentCategory = currentCategory,
                SubCategories = subCategories,
                Products = products,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems,
                }
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.Color)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if(product == null)
            {
                return NotFound();
            }

            var availableSizes = product.ProductVariants
                .Where(pv => pv.IsActive && pv.StockQuantity > 0)
                .Select(pv => pv.Size)
                .Distinct()
                .ToList();

            var availableColors = product.ProductVariants
               .Where(pv => pv.IsActive && pv.StockQuantity > 0)
               .Select(pv => pv.Color)
               .Distinct()
               .ToList();

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .Include(p => p.Images)
                .ToListAsync();

            var viewModel = new ProductViewModel
            {
                Product = product,
                AvailableSizes = availableSizes,
                AvailableColors = availableColors,
                Variants = product.ProductVariants.Where(v => v.IsActive).ToList(),
                RelatedProducts = relatedProducts,
                Reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList()
            };

            return View(viewModel);

        }

        public async Task<IActionResult> Category(int id, int page = 1)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", new {categoryId = id, page = page});
        }
    }
}
