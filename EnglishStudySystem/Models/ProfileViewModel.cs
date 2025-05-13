using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.Models // Make sure to use the correct namespace for your project
{
    public class ProfileViewModel
    {
        [Display(Name = "Tên người dùng")]
        public string Fullname { get; set; }

        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        // Thêm các thuộc tính khác nếu bạn đã mở rộng ApplicationUser của mình
        // [Display(Name = "Tên đầy đủ")]
        // public string FullName { get; set; }
    }
}