using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnglishStudySystem.Models
{
    public class TestHistoryViewModel
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestTitle { get; set; }
        public string LessonName { get; set; }
        public DateTime AttemptDate { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public bool IsCompleted { get; set; }
        public double Duration { get; set; } // Thời gian làm bài tính bằng phút

        // Có thể thêm các thuộc tính tính toán
        public double AccuracyRate => TotalQuestions > 0 ? (CorrectAnswers * 100.0 / TotalQuestions) : 0;
    }
}