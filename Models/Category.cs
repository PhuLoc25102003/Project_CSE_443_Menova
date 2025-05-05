using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Menova.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter the category's name!")]
        [StringLength(50)]
        public string Name { get; set; }


        [StringLength(500)]
        public string Description { get; set; }

   
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey("ParentCategoryId")]
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }

    }
}
