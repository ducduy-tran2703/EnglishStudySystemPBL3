// EnglishStudySystem.Areas.Admin.ViewModel/LessonViewModel.cs
// Hoặc EnglishStudySystem.Models.ViewModels/LessonViewModel.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.Areas.Admin.ViewModel // Hoặc EnglishStudySystem.Models.ViewModels
{
    public class LessonViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        // --- Thông tin Người tạo ---
        [Display(Name = "Người tạo")]
        public string CreatedByUserFullName { get; set; } // FullName của người tạo

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // --- Thông tin Người cập nhật ---
        // Người này cũng được coi là người thực hiện xóa mềm nếu IsDeleted = true
        [Display(Name = "Người cập nhật")]
        public string UpdatedByUserFullName { get; set; } // FullName của người cập nhật/người xóa

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // --- Thông tin Xóa mềm ---
        // Thuộc tính DeletedByUserFullName không còn cần thiết vì chúng ta dùng UpdatedByUserFullName
        // [Display(Name = "Người xóa")]
        // public string DeletedByUserFullName { get; set; }

        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; } // Thời điểm soft delete
    }
}