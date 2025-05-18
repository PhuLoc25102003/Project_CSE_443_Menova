using Menova.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Menova.Data.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesByParentIdAsync(int parentId);
        Task<Category> GetCategoryWithChildrenAsync(int id);
        Task<IEnumerable<SelectListItem>> GetAllCategoriesForSelectListAsync();
    }
}
