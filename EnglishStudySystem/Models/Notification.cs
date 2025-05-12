// EnglishStudySystem/Models/Notification.cs
// Assuming this file already exists. Update its content as follows:

using System;
using System.Collections.Generic; // Make sure this is present
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class Notification : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề thông báo là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung thông báo là bắt buộc.")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // --- Sender (Editor/Admin) ---
        [Required]
        [Display(Name = "Người gửi")]
        public string SenderId { get; set; } // ID của người gửi (Editor/Admin)
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; } // Thuộc tính điều hướng tới người gửi

        // --- Recipients (Customers) ---
        // Navigation property for the many-to-many relationship with ApplicationUser (Recipients)
        public virtual ICollection<UserNotification> UserNotifications { get; set; }

        // --- SOFT DELETE PROPERTIES ---
        [Required]
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; }

        public Notification()
        {
            UserNotifications = new HashSet<UserNotification>();
        }
    }
}