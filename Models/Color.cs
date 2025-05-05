using System.ComponentModel.DataAnnotations;

namespace Menova.Models
{
    public class Color
    {
        [Key]
        public int ColorId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string ColorCode { get; set; }

        public bool IsActive { get; set; }

        // Navigation property
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }

    }
}
