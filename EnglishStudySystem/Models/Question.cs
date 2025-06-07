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

        public int Order { get; set; } // Thứ tự câu hỏi trong bài kiểm tra

        // Foreign Key
        [Display(Name = "Bài kiểm tra")]
        public int TestId { get; set; }

        // Navigation properties
        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }

        [NotMapped] // Thuộc tính này không ánh xạ vào database
        public int IsCorrectAnswer { get; set; } // Index của đáp án đúng (0, 1, 2, 3)

        public Question()
        {
            Answers = new HashSet<Answer>();
        }
    }
}