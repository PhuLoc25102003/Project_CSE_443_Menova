using System.ComponentModel.DataAnnotations;

namespace Menova.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter your username!")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter your Email!")]
        [EmailAddress(ErrorMessage = "Your email is invalid!")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password!")]
        [StringLength(200)]
        public string passwordHash  { get; set; }

        [Required]
        [StringLength(200)]
        public string salt { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; }


        [Required]
        [StringLength(10, ErrorMessage = "Please enter 10 numbers")]
        public string PhoneNumber   { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Customer";

        public bool isActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual Cart Cart { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public virtual ICollection<WishList> WishLists { get; set; }
    }
}
