using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Menova.Models.ViewModels
{
    public class ProductCreateEditViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(50, ErrorMessage = "Tên sản phẩm không được vượt quá 50 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
        [StringLength(500, ErrorMessage = "Mô tả sản phẩm không được vượt quá 500 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0.")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn hoặc bằng 0.")]
        public decimal DiscountPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã SKU.")]
        [StringLength(50, ErrorMessage = "Mã SKU không được vượt quá 50 ký tự.")]
        public string SKU { get; set; }

        [StringLength(200, ErrorMessage = "URL hình ảnh không được vượt quá 200 ký tự.")]
        public string ImageUrl { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "Vui lòng tải lên hình ảnh sản phẩm.")]
        public IFormFile ProductImage { get; set; }

        public bool IsActive { get; set; } = true;

        // Phương thức để chuyển đổi sang Product
        public Product ToProduct()
        {
            return new Product
            {
                ProductId = this.ProductId,
                Name = this.Name,
                Description = this.Description,
                Price = this.Price,
                DiscountPrice = this.DiscountPrice > 0 ? this.DiscountPrice : this.Price,
                CategoryId = this.CategoryId,
                SKU = this.SKU,
                ImageUrl = this.ImageUrl,
                IsActive = this.IsActive,
                UpdatedAt = DateTime.Now
            };
        }

        // Phương thức để tạo ViewModel từ Product
        public static ProductCreateEditViewModel FromProduct(Product product)
        {
            // Lưu ý: Không gán ProductImage do đây là trường bắt buộc nhưng không có trong model Product
            var viewModel = new ProductCreateEditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                CategoryId = product.CategoryId,
                SKU = product.SKU,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };
            
            return viewModel;
        }
    }
} 