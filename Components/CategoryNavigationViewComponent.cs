using Menova.Data.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Menova.Components
{
    public class CategoryNavigationViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoryNavigationViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }
    }
} 