using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Thêm dòng này nếu bạn định có Questions trong Test

namespace EnglishStudySystem.Models // Đảm bảo namespace này khớp với dự án của bạn
{
    public class Test : ISoftDeletable // Kế thừa ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID bài học là bắt buộc.")]
        [Display(Name = "ID Bài học")]
        public int LessonId { get; set; } // Khóa ngoại tới Lesson

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } // Thuộc tính điều hướng tới Lesson

        [Required(ErrorMessage = "Tiêu đề bài kiểm tra là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Số lượng câu hỏi")]
        public int QuestionCount { get; set; } // Ví dụ: tổng số câu hỏi trong bài kiểm tra

        [Required]
        [Display(Name = "Thời gian lam bài")]
        public int Duration { get; set; } // Thời gian làm bài tính bằng phút

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // --- CHỨC NĂNG XÓA MỀM (SOFT DELETE) ---
        [Required]
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }

        // Bạn có thể thêm các thuộc tính điều hướng khác tại đây, ví dụ: một bài kiểm tra có nhiều câu hỏi
         public virtual ICollection<Question> Questions { get; set; }

        public Test()
        {
            Questions = new HashSet<Question>();
        }
    }
}