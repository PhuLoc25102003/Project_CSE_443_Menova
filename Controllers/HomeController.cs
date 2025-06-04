using System.Diagnostics;
using System.Threading.Tasks;
using Menova.Data;
using Menova.Data.Services;
using Menova.Models;
using Menova.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Menova.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        public HomeController(ILogger<HomeController> logger, IProductService productService, ICategoryService categoryService)
        {
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;

        }

        public async Task<IActionResult> Index()
        {
            // Get top selling products
            var topSellingProducts = await _productService.GetTopSellingProductsAsync(8);
            
            var viewModel = new HomeViewModel
            {
                FeturedProducts = (await _productService.GetFeaturedProductsAsync(8)).ToList(),
                
                // Convert ProductSalesSummary to Product list
                BestSellingProducts = topSellingProducts.Select(p => p.Product).ToList(),

                Categories = (await _categoryService.GetTopLevelCategoriesAsync()).ToList(),
                
                Slides = new List<SlideModel>
                {
                    new SlideModel
                    {
                        Year = "2025",
                        Title = "NEW COLLECTION",
                        CollectionTitle = "Sánh Đôi",
                        CollectionSubtitle1 = "PHÍ VÍ",
                        CollectionSubtitle2 = "ĐAN ANH",
                        ImageUrl = "/assets/sliders/slider-1.webp",
                        IsActive = true
                    },
                    new SlideModel
                    {
                        Year = "2025",
                        Title = "SUMMER SERIES",
                        CollectionTitle = "Ocean Breeze",
                        CollectionSubtitle1 = "CASUAL WEAR",
                        CollectionSubtitle2 = "BEACH READY",
                        ImageUrl = "/assets/sliders/slider-2.webp",
                        IsActive = false
                    },
                    new SlideModel
                    {
                        Year = "2025",
                        Title = "AUTUMN COLLECTION",
                        CollectionTitle = "Warmth & Style",
                        CollectionSubtitle1 = "LAYERED FASHION",
                        CollectionSubtitle2 = "EARTH TONES",
                        ImageUrl = "/assets/sliders/slider-3.webp",
                        IsActive = false
                    }
                }
            };
            
            return View(viewModel);
        }
        
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Process contact form submission
                // Send email, store in database, etc.
                
                TempData["SuccessMessage"] = "Cảm ơn! Tin nhắn của bạn đã được gửi thành công.";
                return RedirectToAction(nameof(Contact));
            }
            
            return View(model);
        }
        
        [HttpGet]
        public IActionResult Search(string q)
        {
            // Since we're using the overlay for search, just redirect to home
            TempData["OpenSearchOverlay"] = true;
            TempData["SearchQuery"] = q;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
