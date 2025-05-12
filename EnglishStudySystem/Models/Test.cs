using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class Test
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Tiêu đề bài kiểm tra")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Thời lượng (phút)")]
        public int DurationMinutes { get; set; }

        // Foreign Key
        [Display(Name = "Bài học liên quan")]
        public int LessonId { get; set; }

        // Navigation properties
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}