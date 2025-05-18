using Menova.Models;
using Microsoft.EntityFrameworkCore;

namespace Menova.Data.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive && (c.ParentCategoryId == null || c.ParentCategoryId == 0))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesByParentIdAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryWithChildrenAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ChildCategories.Where(sc => sc.IsActive))
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);
        }
    }

}
