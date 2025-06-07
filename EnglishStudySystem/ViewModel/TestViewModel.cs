using EnglishStudySystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.ViewModel
{
    public class TestCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Display(Name = "Bài học")]
        public string LessonTitle { get; set; }

        [Required]
        [Display(Name = "Thời gian làm bài (phút)")]
        public int Duration { get; set; }

        public List<QuestionViewModel> Questions { get; set; }
    }

    public class QuestionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc.")]
        [StringLength(2000, ErrorMessage = "Nội dung câu hỏi không được vượt quá 2000 ký tự.")]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một đáp án đúng.")]
        [Range(0, 3, ErrorMessage = "Đáp án đúng phải là một trong 4 đáp án (A, B, C, D).")]
        [Display(Name = "Đáp án đúng")]
        public int IsCorrectAnswer { get; set; }

        public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
    }

    public class AnswerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung đáp án là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Nội dung đáp án không được vượt quá 1000 ký tự.")]
        [Display(Name = "Nội dung đáp án")]
        public string Content { get; set; }
    }

    // ViewModel cho trang danh sách hoặc chi tiết bài kiểm tra
    public class TestDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int LessonId { get; set; } // Đã có từ trước để hỗ trợ liên kết
        public string LessonTitle { get; set; }
        public int QuestionCount { get; set; } // Thêm thuộc tính QuestionCount
        public int Duration { get; set; } // Thêm thuộc tính Duration
        public List<QuestionDetailViewModel> Questions { get; set; } = new List<QuestionDetailViewModel>();
    }

    public class QuestionDetailViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public List<AnswerDetailViewModel> Answers { get; set; } = new List<AnswerDetailViewModel>();
    }

    public class AnswerDetailViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public bool IsCorrect { get; set; }
    }
}