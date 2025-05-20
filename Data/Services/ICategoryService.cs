using Menova.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Menova.Data.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesByParentIdAsync(int parentId);
        Task<Category> GetCategoryWithChildrenAsync(int id);
        Task<Category> GetCategoryByIdAsync(int id);
        Task<IEnumerable<SelectListItem>> GetAllCategoriesForSelectListAsync();
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task SoftDeleteCategoryAsync(int id);
    }
}
