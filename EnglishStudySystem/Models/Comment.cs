using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng [ForeignKey]

namespace EnglishStudySystem.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Nội dung bình luận")]
        public string Content { get; set; }

        [Display(Name = "Ngày bình luận")]
        public DateTime CommentDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [Display(Name = "Bài học")]
        public int LessonId { get; set; }

        [Display(Name = "Người bình luận")]
        public string UserId { get; set; } // UserId từ AspNetUsers

        // Navigation properties
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}