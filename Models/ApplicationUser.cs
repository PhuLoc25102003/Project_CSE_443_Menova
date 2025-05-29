using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Menova.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Order> Orders { get; set; }
        public virtual Cart Cart { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public virtual ICollection<WishList> WishLists { get; set; }
    }
} 