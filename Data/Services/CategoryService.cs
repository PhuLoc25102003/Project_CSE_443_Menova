using Menova.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace Menova.Data.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories.Where(c => c.IsActive).ToList();
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetTopLevelCategoriesAsync();
            return categories.Where(c => c.IsActive);
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesByParentIdAsync(int parentId)
        {
            var categories = await _unitOfWork.Categories.GetSubCategoriesByParentIdAsync(parentId);
            return categories.Where(c => c.IsActive);
        }

        public async Task<Category> GetCategoryWithChildrenAsync(int id)
        {
            return await _unitOfWork.Categories.GetCategoryWithChildrenAsync(id);
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _unitOfWork.Categories.GetByIdAsync(id);
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

        public async Task AddCategoryAsync(Category category)
        {
            try
            {
                Console.WriteLine($"Adding category: {category.Name}, ParentId: {category.ParentCategoryId}");
                
                // Ensure collections are initialized
                if (category.ChildCategories == null)
                    category.ChildCategories = new List<Category>();
                
                if (category.Products == null)
                    category.Products = new List<Product>();
                
                // Set ParentCategory explicitly to null if not needed
                if (!category.ParentCategoryId.HasValue)
                    category.ParentCategory = null;
                
                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.CompleteAsync();
                Console.WriteLine($"Category added successfully with ID: {category.CategoryId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR adding category: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            try
            {
                Console.WriteLine($"Updating category: {category.Name}, ID: {category.CategoryId}");
                
                // Ensure collections are initialized
                if (category.ChildCategories == null)
                    category.ChildCategories = new List<Category>();
                
                if (category.Products == null)
                    category.Products = new List<Product>();
                
                // Set ParentCategory explicitly to null if not needed
                if (!category.ParentCategoryId.HasValue)
                    category.ParentCategory = null;
                
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.CompleteAsync();
                Console.WriteLine($"Category updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR updating category: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                try
                {
                    Console.WriteLine($"Deleting category: {category.Name}, ID: {category.CategoryId}");
                    _unitOfWork.Categories.Remove(category);
                    await _unitOfWork.CompleteAsync();
                    Console.WriteLine($"Category deleted successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR deleting category: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
        }

        public async Task SoftDeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                try
                {
                    Console.WriteLine($"Soft deleting category: {category.Name}, ID: {category.CategoryId}");
                    category.IsActive = false;
                    
                    // Also deactivate all child categories
                    await DeactivateChildCategories(category.CategoryId);
                    
                    _unitOfWork.Categories.Update(category);
                    await _unitOfWork.CompleteAsync();
                    Console.WriteLine($"Category soft deleted successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR soft deleting category: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
        }
        
        private async Task DeactivateChildCategories(int parentId)
        {
            var childCategories = await _unitOfWork.Categories.GetSubCategoriesByParentIdAsync(parentId);
            foreach (var child in childCategories)
            {
                child.IsActive = false;
                _unitOfWork.Categories.Update(child);
                
                // Recursive call to deactivate grandchildren
                await DeactivateChildCategories(child.CategoryId);
            }
        }
    }
}
