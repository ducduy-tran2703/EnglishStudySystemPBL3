// File: EnglishStudySystem.Areas.Admin.ViewModel/LessonDetailsViewModel.cs
using EnglishStudySystem.Models; // Đảm bảo bạn có Lesson và Test models
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class LessonDetailsViewModel
    {
        // --- Thông tin chi tiết của Bài học (từ Model Lesson) ---
        public int Id { get; set; }

        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Đường dẫn Video")]
        public string Video_URL { get; set; }

        [Display(Name = "Miễn phí")] // Thuộc tính IsFree độc lập
        public bool IsFree { get; set; } // Giờ đây IsFree là một thuộc tính có thể set giá trị trực tiếp

        // Foreign Key to Category
        public int CategoryId { get; set; }

        // --- Thông tin Danh mục liên quan ---
        [Display(Name = "Danh mục")]
        public string CategoryName { get; set; } // Tên của danh mục cha

        // Audit Fields
        [Display(Name = "ID người tạo")]
        public string CreatedByUserId { get; set; }

        [Display(Name = "Người tạo")] // <-- Thuộc tính mới cho FullName
        public string CreatedByUserFullName { get; set; }

        [Display(Name = "Vai trò người tạo")]
        public string CreatedByUserRole { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "ID người cập nhật")]
        public string UpdatedByUserId { get; set; }

        [Display(Name = "Người cập nhật")] // <-- Thuộc tính mới cho FullName
        public string UpdatedByUserFullName { get; set; }

        [Display(Name = "Vai trò người cập nhật")]
        public string UpdatedByUserRole { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // --- Thông tin Xóa mềm ---
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }

        // --- Danh sách các Bài kiểm tra liên quan ---
        // Sử dụng List<Test> trực tiếp hoặc List<TestViewModel> tùy nhu cầu hiển thị chi tiết Test
        public List<Test> Tests { get; set; }

        public LessonDetailsViewModel()
        {
            Tests = new List<Test>();
        }
    }
}