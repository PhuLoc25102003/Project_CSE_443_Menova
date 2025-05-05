using System.ComponentModel.DataAnnotations;

namespace Menova.Models
{
    public class Size
    {
        [Key]
        public int SizeId { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public int SortOrder { get; set; }


        public bool IsActive { get; set; }


        // Navigation property
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }



    }
}
