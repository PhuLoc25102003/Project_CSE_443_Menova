using System.Collections.Generic;
namespace Menova.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }
        public List<Size> AvailableSizes { get; set; }
        public List<Color> AvailableColors { get; set; }
        public List<ProductVariant> Variants { get; set; }
        public List<Product> RelatedProducts { get; set; }
        public List<Review> Reviews { get; set; }




    }
}
