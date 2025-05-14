using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.Models // Make sure to use the correct namespace for your project
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

           
            [DataType(DataType.Date)]
            [Display(Name = "Ngày sinh")]
            public DateTime? DateOfBirth { get; set; }

            
        

        // Thêm các thuộc tính khác nếu bạn đã mở rộng ApplicationUser của mình
        // [Display(Name = "Tên đầy đủ")]
        // public string FullName { get; set; }
    }
}