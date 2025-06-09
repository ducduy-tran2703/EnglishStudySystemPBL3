// EnglishStudySystem/Models/ViewModels/LessonEditViewModel.cs

using System; // Cần cho DateTime?
using System.Collections.Generic; // Cần cho List
using System.ComponentModel.DataAnnotations; // Cần cho Display
using EnglishStudySystem.Models; // Cần cho Model Test

// Sử dụng namespace phù hợp với vị trí file của bạn
namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class LessonEditViewModel
    {
        // ID của Bài học cần thiết để xác định Bài học nào đang được sửa
        public int Id { get; set; }

        // CategoryId cần thiết (người dùng có thể thay đổi danh mục)
        [Required(ErrorMessage = "ID Danh mục là bắt buộc.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "URL Video")]
        public string Video_URL { get; set; }

        [Display(Name = "Cho phép học thử miễn phí")]
        public bool IsFreeTrial { get; set; }


        // --- Thông tin về Danh mục cha (để hiển thị tên trên View) ---
        public string CategoryName { get; set; }


        // --- Thông tin về Bài kiểm tra liên quan (để hiển thị danh sách) ---
        public List<Test> Tests { get; set; }


        // --- Hiển thị Audit Fields (Dựa trên Lesson.cs) ---
        [Display(Name = "ID người tạo")]
        public string CreatedByUserId { get; set; }

        [Display(Name = "Người tạo")]
        public string CreatedByUserFullName { get; set; } // Đã thêm lại theo trao đổi trước đó

        [Display(Name = "Vai trò người tạo")]
        public string CreatedByUserRole { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "ID người cập nhật")]
        public string UpdatedByUserId { get; set; }

        [Display(Name = "Người cập nhật")]
        public string UpdatedByUserFullName { get; set; } // Đã thêm lại theo trao đổi trước đó

        [Display(Name = "Vai trò người cập nhật")]
        public string UpdatedByUserRole { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Constructor để khởi tạo danh sách Tests
        public LessonEditViewModel()
        {
            Tests = new List<Test>();
        }
    }
}