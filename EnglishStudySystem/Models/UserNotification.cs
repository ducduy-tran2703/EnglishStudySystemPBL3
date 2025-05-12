// EnglishStudySystem/Models/UserNotification.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class UserNotification : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // FK to ApplicationUser (Recipient)
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Navigation property to the recipient user

        [Required]
        public int NotificationId { get; set; } // FK to Notification
        [ForeignKey("NotificationId")]
        public virtual Notification Notification { get; set; } // Navigation property to the notification

        [Required]
        [Display(Name = "Đã đọc")]
        public bool IsRead { get; set; } = false; // Track if the user has read this notification

        // --- SOFT DELETE PROPERTIES ---
        [Required]
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }
    }
}