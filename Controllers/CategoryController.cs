using Menova.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Menova.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var categories = _context.Categories
                .Where(c => c.IsActive && (c.ParentCategory == null || c.ParentCategoryId == 0))
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ChildCategories.Where(cs => cs.IsActive))
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);
                

                if(category == null)
            {
                return NotFound();
            }

            return View(category);
        }




    }
}
