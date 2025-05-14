using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Trạng thái tài khoản")]
        public UserAccountStatus AccountStatus { get; set; }

        [Display(Name = "Vai trò")]
        public string Roles { get; set; }
        [Display(Name = "Các bài viết đã tạo")]
        public List<Category> Categories { get; set; } = new List<Category>();
        [Display(Name = "Các bài viết đã mua")]
        public List<Category> PurchasedCategories { get; set; } = new List<Category>();
    }
}