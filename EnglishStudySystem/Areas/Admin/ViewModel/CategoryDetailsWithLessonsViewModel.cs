// EnglishStudySystem/Areas/Admin/Models/ViewModels/CategoryDetailsWithLessonsViewModel.cs
// Hoặc EnglishStudySystem/Models/ViewModels/CategoryDetailsWithLessonsViewModel.cs

using EnglishStudySystem.Models; // Cần để sử dụng Model Category và Lesson
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Cần cho Display

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
        [Display(Name = "Người tạo")]
        public string CreatedByUserId { get; set; } // Có thể thêm thuộc tính để hiển thị tên người tạo nếu cần
        [Display(Name = "Vai trò người tạo")]
        public string CreatedByUserRole { get; set; }
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Người cập nhật")]
        public string UpdatedByUserId { get; set; } // Có thể thêm thuộc tính để hiển thị tên người cập nhật
        [Display(Name = "Vai trò người cập nhật")]
        public string UpdatedByUserRole { get; set; }
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // --- Danh sách các Bài học thuộc Danh mục này ---
        // Sử dụng trực tiếp Model Lesson ở đây để đơn giản,
        // vì trang này chủ yếu hiển thị và chuyển hướng sang quản lý Lesson
        public List<Lesson> Lessons { get; set; } // Cần using EnglishStudySystem.Models;

        // Constructor để khởi tạo danh sách Bài học
        public CategoryDetailsWithLessonsViewModel()
        {
            Lessons = new List<Lesson>();
        }
    }
}