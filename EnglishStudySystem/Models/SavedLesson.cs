using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class SavedLesson
    {
        public int Id { get; set; }

        [Display(Name = "Ngày lưu")]
        public DateTime SavedDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [Display(Name = "Bài học")]
        public int LessonId { get; set; }

        [Display(Name = "Người dùng")]
        public string UserId { get; set; }

        // Navigation properties
        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}