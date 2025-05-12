using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class UserTestAttempt
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ngày làm bài")]
        public DateTime AttemptDate { get; set; } = DateTime.Now;

        [Display(Name = "Điểm số")]
        public int Score { get; set; } // Điểm số người dùng đạt được

        [Display(Name = "Hoàn thành?")]
        public bool IsCompleted { get; set; } = false;

        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Display(Name = "Thời gian kết thúc")]
        public DateTime? EndTime { get; set; }

        // Foreign Keys
        [Display(Name = "Người dùng")]
        public string UserId { get; set; }

        [Display(Name = "Bài kiểm tra")]
        public int TestId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }

        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}