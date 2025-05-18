using Menova.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Menova.Data.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
        {
            return await _unitOfWork.Categories.GetTopLevelCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesByParentIdAsync(int parentId)
        {
            return await _unitOfWork.Categories.GetSubCategoriesByParentIdAsync(parentId);
        }

        public async Task<Category> GetCategoryWithChildrenAsync(int id)
        {
            return await _unitOfWork.Categories.GetCategoryWithChildrenAsync(id);
        }

        public async Task<IEnumerable<SelectListItem>> GetAllCategoriesForSelectListAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                });
        }
    }

}
