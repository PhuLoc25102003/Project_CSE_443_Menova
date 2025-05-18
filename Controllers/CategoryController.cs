using Menova.Data;
using Menova.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Menova.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetTopLevelCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var category = await _categoryService.GetCategoryWithChildrenAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }






    }
}
