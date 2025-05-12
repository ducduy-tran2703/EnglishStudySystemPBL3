using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; }

        // Foreign Key
        [Display(Name = "Bài kiểm tra")]
        public int TestId { get; set; }

        // Navigation properties
        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
    }
}