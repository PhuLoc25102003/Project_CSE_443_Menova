using Menova.Data.Services;
using Menova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetCategoryWithChildrenAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public async Task<IActionResult> Create(int? parentId = null)
        {
            // Get parent categories for dropdown
            ViewBag.ParentCategories = await GetParentCategoriesSelectList();
            
            // Nếu có parentId, đây là tạo danh mục con
            var model = new Category();
            if (parentId.HasValue)
            {
                var parentCategory = await _categoryService.GetCategoryByIdAsync(parentId.Value);
                if (parentCategory != null)
                {
                    model.ParentCategoryId = parentId;
                    ViewBag.ParentCategoryName = parentCategory.Name;
                    ViewBag.IsChildCategory = true;
                }
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                Console.WriteLine($"Attempting to create category: {category.Name}, ParentId: {category.ParentCategoryId}");
                
                // Initialize collections to prevent validation errors
                if (category.ChildCategories == null)
                    category.ChildCategories = new List<Category>();
                
                if (category.Products == null)
                    category.Products = new List<Product>();
                
                // Remove these properties from ModelState to prevent validation
                ModelState.Remove("ChildCategories");
                ModelState.Remove("Products");
                ModelState.Remove("ParentCategory");
                
                if (ModelState.IsValid)
                {
                    // Check if this is going to be both a parent and a child
                    if (category.ParentCategoryId.HasValue && category.ChildCategories.Count > 0)
                    {
                        ModelState.AddModelError("ParentCategoryId", "Danh mục con không thể trở thành danh mục cha. Hệ thống chỉ hỗ trợ phân cấp 2 cấp.");
                        ViewBag.ParentCategories = await GetParentCategoriesSelectList();
                        
                        // Re-populate ViewBag for parent category name if needed
                        if (category.ParentCategoryId.HasValue)
                        {
                            var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId.Value);
                            if (parentCategory != null)
                            {
                                ViewBag.ParentCategoryName = parentCategory.Name;
                                ViewBag.IsChildCategory = true;
                            }
                        }
                        
                        return View(category);
                    }
                    
                    Console.WriteLine("Model state is valid, adding category to database");
                    await _categoryService.AddCategoryAsync(category);
                    TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được tạo thành công.";
                    
                    // Refresh the parent category details if this is a child category
                    if (category.ParentCategoryId.HasValue)
                    {
                        Console.WriteLine($"Created child category for parent ID: {category.ParentCategoryId}");
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("Model state is invalid:");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine($"- {error.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Create action: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Lỗi khi tạo danh mục: {ex.Message}");
            }

            // If model state is invalid, get parent categories again
            ViewBag.ParentCategories = await GetParentCategoriesSelectList();
            
            // Nếu là danh mục con, hiển thị lại thông tin danh mục cha
            if (category.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId.Value);
                if (parentCategory != null)
                {
                    ViewBag.ParentCategoryName = parentCategory.Name;
                    ViewBag.IsChildCategory = true;
                }
            }
            
            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            // Get parent categories for dropdown, excluding the current category and its children
            ViewBag.ParentCategories = await GetParentCategoriesSelectList(excludeCategoryId: id);
            
            // Nếu là danh mục con, hiển thị thông tin danh mục cha
            if (category.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId.Value);
                if (parentCategory != null)
                {
                    ViewBag.ParentCategoryName = parentCategory.Name;
                    ViewBag.IsChildCategory = true;
                }
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            // Initialize collections to prevent validation errors
            if (category.ChildCategories == null)
                category.ChildCategories = new List<Category>();
            
            if (category.Products == null)
                category.Products = new List<Product>();
            
            // Remove these properties from ModelState to prevent validation
            ModelState.Remove("ChildCategories");
            ModelState.Remove("Products");
            ModelState.Remove("ParentCategory");

            if (ModelState.IsValid)
            {
                try
                {
                    // Prevent circular references
                    if (category.CategoryId == category.ParentCategoryId)
                    {
                        ModelState.AddModelError("ParentCategoryId", "A category cannot be its own parent.");
                        ViewBag.ParentCategories = await GetParentCategoriesSelectList(excludeCategoryId: id);
                        return View(category);
                    }
                    
                    // Get the complete category with its children
                    var existingCategory = await _categoryService.GetCategoryWithChildrenAsync(id);
                    
                    // Check if this is a child category trying to be a parent
                    if (category.ParentCategoryId.HasValue && 
                        existingCategory.ChildCategories != null && 
                        existingCategory.ChildCategories.Any())
                    {
                        ModelState.AddModelError("ParentCategoryId", 
                            "Danh mục con không thể trở thành danh mục cha. Hệ thống chỉ hỗ trợ phân cấp 2 cấp.");
                        ViewBag.ParentCategories = await GetParentCategoriesSelectList(excludeCategoryId: id);
                        
                        // Re-populate ViewBag for parent category name if needed
                        if (category.ParentCategoryId.HasValue)
                        {
                            var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId.Value);
                            if (parentCategory != null)
                            {
                                ViewBag.ParentCategoryName = parentCategory.Name;
                                ViewBag.IsChildCategory = true;
                            }
                        }
                        
                        return View(category);
                    }
                    
                    // Prevent making a category with children into a child category
                    if (category.ParentCategoryId.HasValue && 
                        (existingCategory.ChildCategories != null && existingCategory.ChildCategories.Any()))
                    {
                        ModelState.AddModelError("ParentCategoryId", 
                            "Danh mục có danh mục con không thể trở thành danh mục con. Vui lòng xóa các danh mục con trước.");
                        ViewBag.ParentCategories = await GetParentCategoriesSelectList(excludeCategoryId: id);
                        return View(category);
                    }

                    await _categoryService.UpdateCategoryAsync(category);
                    TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được cập nhật thành công.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR in Edit action: {ex.Message}");
                    ModelState.AddModelError("", $"Lỗi khi cập nhật danh mục: {ex.Message}");
                    
                    if (!await CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ParentCategories = await GetParentCategoriesSelectList(excludeCategoryId: id);
            
            // Nếu là danh mục con, hiển thị lại thông tin danh mục cha
            if (category.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId.Value);
                if (parentCategory != null)
                {
                    ViewBag.ParentCategoryName = parentCategory.Name;
                    ViewBag.IsChildCategory = true;
                }
            }
            
            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryWithChildrenAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Change to soft delete instead of hard delete
                await _categoryService.SoftDeleteCategoryAsync(id);
                TempData["SuccessMessage"] = "Danh mục đã được ẩn thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi ẩn danh mục: {ex.Message}";
                Console.WriteLine($"ERROR in Delete action: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                // Toggle the active status
                category.IsActive = !category.IsActive;
                await _categoryService.UpdateCategoryAsync(category);

                string statusMessage = category.IsActive ? "hiển thị" : "ẩn";
                TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được {statusMessage} thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thay đổi trạng thái danh mục: {ex.Message}";
                Console.WriteLine($"ERROR in ToggleStatus action: {ex.Message}");
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<SelectList> GetParentCategoriesSelectList(int? excludeCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            if (excludeCategoryId.HasValue)
            {
                // Exclude the current category and its children to prevent circular references
                var categoryToExclude = categories.FirstOrDefault(c => c.CategoryId == excludeCategoryId.Value);
                var childrenIds = GetAllChildrenIds(categories, excludeCategoryId.Value);
                
                categories = categories.Where(c => c.CategoryId != excludeCategoryId.Value && !childrenIds.Contains(c.CategoryId)).ToList();
            }
            
            return new SelectList(categories, "CategoryId", "Name");
        }
        
        private HashSet<int> GetAllChildrenIds(List<Category> allCategories, int parentId)
        {
            var result = new HashSet<int>();
            var directChildren = allCategories.Where(c => c.ParentCategoryId == parentId).Select(c => c.CategoryId);
            
            foreach (var childId in directChildren)
            {
                result.Add(childId);
                foreach (var id in GetAllChildrenIds(allCategories, childId))
                {
                    result.Add(id);
                }
            }
            
            return result;
        }

        private async Task<bool> CategoryExists(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return category != null;
        }
    }
} 