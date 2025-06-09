// EnglishStudySystem/Areas/Admin/Models/ViewModels/CategoryDetailsWithLessonsViewModel.cs
// Hoặc EnglishStudySystem/Models/ViewModels/CategoryDetailsWithLessonsViewModel.cs

using EnglishStudySystem.Models; // Cần để sử dụng Model Category và Lesson
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Đảm bảo namespace này khớp với vị trí bạn đặt file
namespace EnglishStudySystem.Areas.Admin.ViewModel // Hoặc EnglishStudySystem.Models.ViewModels
{
    public class CategoryDetailsWithLessonsViewModel
    {
        // --- Thông tin chi tiết của Danh mục (Lấy từ Model Category) ---
        public int Id { get; set; }

        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Giá")]
        public decimal? Price { get; set; }

        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }

        // Audit Fields
        [Display(Name = "ID người tạo")]
        public string CreatedByUserId { get; set; }

        [Display(Name = "Người tạo")]
        public string CreatedByUserFullName { get; set; } // FullName của người tạo

        [Display(Name = "Vai trò người tạo")]
        public string CreatedByUserRole { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "ID người cập nhật")]
        public string UpdatedByUserId { get; set; }

        [Display(Name = "Người cập nhật")]
        public string UpdatedByUserFullName { get; set; } // FullName của người cập nhật

        [Display(Name = "Vai trò người cập nhật")]
        public string UpdatedByUserRole { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // --- Danh sách các Bài học thuộc Danh mục này ---
        // Giữ nguyên là List<Lesson> nếu bạn truyền trực tiếp Model Lesson vào View
        // Tuy nhiên, để tuân thủ tách biệt Model và ViewModel tốt hơn,
        // bạn có thể cân nhắc tạo LessonViewModel và sử dụng List<LessonViewModel> ở đây.
        public List<LessonViewModel> Lessons { get; set; } 

        public bool ShowDeletedLessons { get; set; } = false;

        public CategoryDetailsWithLessonsViewModel()
        {
            Lessons = new List<LessonViewModel>();
        }
    }
}