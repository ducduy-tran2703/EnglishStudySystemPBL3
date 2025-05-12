using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime SentDate { get; set; } = DateTime.Now;

        [Display(Name = "Đã đọc")]
        public bool IsRead { get; set; } = false;

        // Foreign Key
        [Display(Name = "Người nhận")]
        public string UserId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}