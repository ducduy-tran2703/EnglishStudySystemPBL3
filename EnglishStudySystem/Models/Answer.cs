using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nội dung đáp án")]
        public string Content { get; set; }

        [Display(Name = "Là đáp án đúng?")]
        public bool IsCorrect { get; set; }

        // Foreign Key
        [Display(Name = "Câu hỏi")]
        public int QuestionId { get; set; }

        // Navigation property
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }
}