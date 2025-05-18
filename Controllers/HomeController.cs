using System.Diagnostics;
using System.Threading.Tasks;
using Menova.Data;
using Menova.Data.Services;
using Menova.Models;
using Menova.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var viewModel = new HomeViewModel
            {
                FeturedProducts = (List<Product>) await _productService.GetFeaturedProductsAsync(8),


                Categories = (List<Category>) await _categoryService.GetTopLevelCategoriesAsync(),
                
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
