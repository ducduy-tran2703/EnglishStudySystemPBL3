using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }

        // Foreign Keys
        [Display(Name = "Lần làm bài kiểm tra")]
        public int UserTestAttemptId { get; set; }

        [Display(Name = "Câu hỏi")]
        public int QuestionId { get; set; }

        [Display(Name = "Đáp án đã chọn")]
        public int? SelectedAnswerId { get; set; }  // ID của đáp án mà người dùng đã chọn

        [Display(Name = "Là đúng?")]
        public bool IsCorrect { get; set; } // true nếu SelectedAnswer là đáp án đúng của Question

        // Navigation properties
        [ForeignKey("UserTestAttemptId")]
        public virtual UserTestAttempt UserTestAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        [ForeignKey("SelectedAnswerId")]
        public virtual Answer SelectedAnswer { get; set; }
    }
}