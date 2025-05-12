using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models // Đảm bảo namespace này khớp với dự án của bạn
{
    public class Comment : ISoftDeletable // Kế thừa ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID bài học là bắt buộc.")]
        [Display(Name = "ID Bài học")]
        public int LessonId { get; set; } // Khóa ngoại tới Lesson

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } // Thuộc tính điều hướng tới Lesson

        [Required(ErrorMessage = "Nội dung bình luận là bắt buộc.")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Người bình luận")]
        public string UserId { get; set; } // ID của người dùng bình luận (ApplicationUser.Id)

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Thuộc tính điều hướng tới ApplicationUser

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày bình luận")]
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Mặc định là thời gian hiện tại

        // --- CHỨC NĂNG XÓA MỀM (SOFT DELETE) ---
        [Required]
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }
    }
}