using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.Models // Make sure to use the correct namespace for your project
{
    public class ProfileViewModel
    {
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; }

        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        // Thêm các thuộc tính khác nếu bạn đã mở rộng ApplicationUser của mình
        // [Display(Name = "Tên đầy đủ")]
        // public string FullName { get; set; }
    }
}