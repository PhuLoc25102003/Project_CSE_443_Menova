using System.Collections.Generic;
namespace Menova.Models.ViewModels
{
    public class CategoryViewModel
    {
        public Category CurrentCategory { get; set; }
        public List<Category> SubCategories { get; set; }
        public List<Product> Products { get; set; }
        public  PaginationInfo Pagination { get; set; }

    }
    public class PaginationInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
}
