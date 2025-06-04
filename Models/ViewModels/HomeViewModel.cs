using System.Collections.Generic;
namespace Menova.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> FeturedProducts { get; set; }
        public List<Product> BestSellingProducts { get; set; }
        public List<Category> Categories { get; set; }
        public List<SlideModel> Slides { get; set; } 
    }
    public class SlideModel
    {
        public string Year { get; set; }
        public string Title { get; set; }
        public string CollectionTitle { get; set; }
        public string CollectionSubtitle1 { get; set; }
        public string CollectionSubtitle2 { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
