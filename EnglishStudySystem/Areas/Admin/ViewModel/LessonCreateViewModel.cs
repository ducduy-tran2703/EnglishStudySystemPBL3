// EnglishStudySystem/Models/ViewModels/LessonCreateViewModel.cs

using System.ComponentModel.DataAnnotations;

// Sử dụng namespace phù hợp với vị trí file của bạn
namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class LessonCreateViewModel
    {
        // CategoryId cần thiết để biết bài học thuộc danh mục nào.
        // Nó sẽ được truyền từ URL hoặc hidden field.
        [Required(ErrorMessage = "ID Danh mục là bắt buộc.")] // Đánh dấu Required ở ViewModel
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "URL Video")]
        // Dựa trên Lesson.cs, Video_URL không có [Required]
        public string Video_URL { get; set; } // <-- Thay thế Content bằng Video_URL

        // THÊM THUỘC TÍNH ISFREE VÀO ĐÂY
        [Display(Name = "Là bài học miễn phí")]
        public bool IsFree { get; set; } = false; // Mặc định là false (không miễn phí)

        // Không bao gồm các thuộc tính Audit (CreatedByUserId, CreatedDate, v.v.)
        // hoặc Soft Delete (IsDeleted, DeletedAt) trong ViewModel này.
    }
}