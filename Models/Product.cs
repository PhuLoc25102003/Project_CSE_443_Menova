using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menova.Models
{
    public class Product
    {
        public Product()
        {
            ProductVariants = new List<ProductVariant>();
            Images = new List<ProductImage>();
            Reviews = new List<Review>();
            OrderDetails = new List<OrderDetail>();
            WishLists = new List<WishList>();
            CartItems = new List<CartItem>();
            IsActive = true;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(50, ErrorMessage = "Tên sản phẩm không được vượt quá 50 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
        [StringLength(500, ErrorMessage = "Mô tả sản phẩm không được vượt quá 500 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0.")]
        public Decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn hoặc bằng 0.")]
        public Decimal DiscountPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã SKU.")]
        [StringLength(50, ErrorMessage = "Mã SKU không được vượt quá 50 ký tự.")]
        public string SKU { get; set; }

        [StringLength(200, ErrorMessage = "URL hình ảnh không được vượt quá 200 ký tự.")]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<WishList> WishLists { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
