using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesByParentIdAsync(int parentId);
        Task<Category> GetCategoryWithChildrenAsync(int id);
    }
}
