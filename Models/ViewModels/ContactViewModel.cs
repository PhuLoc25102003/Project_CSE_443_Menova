using System.ComponentModel.DataAnnotations;

namespace Menova.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên của bạn")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email của bạn")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
        public string Message { get; set; }
    }
} 