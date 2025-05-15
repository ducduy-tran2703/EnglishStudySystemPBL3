// EnglishStudySystem/Models/Notification.cs

using System;
using System.Collections.Generic;
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
        public string SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }

        // --- Recipients (Customers) ---
        public virtual ICollection<UserNotification> UserNotifications { get; set; }

        // --- THÊM: Thông tin để điều hướng khi click thông báo (Tham số URL) ---
        // Các thuộc tính này được sử dụng để xây dựng URL đích trong View

        [Required] // Tên Controller là bắt buộc
        [StringLength(100)]
        [Display(Name = "Controller đích")]
        public string TargetController { get; set; }

        [Required] // Tên Action là bắt buộc
        [StringLength(100)]
        [Display(Name = "Action đích")]
        public string TargetAction { get; set; }

        [StringLength(50)]
        [Display(Name = "Area đích")]
        public string TargetArea { get; set; } = null; // Null nếu ở thư mục gốc


        // --- GỘP CHUNG các loại ID liên quan vào 2 thuộc tính tổng quát ---
        // Sử dụng 2 thuộc tính ID kiểu int? (vì các Entity chính dùng int ID)
        // RelatedEntityType sẽ giúp giải thích ý nghĩa của các ID này trong từng trường hợp cụ thể.

        [Display(Name = "ID liên quan chính")]
        // ID chính của Entity liên quan. Ví dụ:
        // - Đối với CommentReply: ID của Bài học (trang đích là trang Bài học)
        // - Đối với NewTestAvailable: ID của Bài kiểm tra
        // - Đối với AdminMessage: null (nếu link đến trang chung)
        // Nullable nếu trang đích không cần ID chính
        public int? PrimaryRelatedEntityId { get; set; }

        [Display(Name = "ID liên quan phụ")]
        // ID phụ, sử dụng cho các trường hợp cần nhiều hơn một ID. Ví dụ:
        // - Đối với CommentReply: ID của Bình luận (để cuộn trang đến bình luận đó dùng anchor)
        // - Đối với các loại thông báo khác: null (nếu chỉ cần 1 ID chính)
        // Nullable nếu không cần ID phụ
        public int? SecondaryRelatedEntityId { get; set; }


        [Required] // Loại Entity liên quan là bắt buộc để biết cách tạo URL và giải thích ID
        [StringLength(50)]
        [Display(Name = "Loại Entity liên quan")]
        // Ví dụ: "CommentReply", "NewTest", "AdminMessage", "LessonUpdate"
        public string RelatedEntityType { get; set; }


        // --- Các ID cụ thể trước đó đã được thay thế bằng Primary/SecondaryRelatedEntityId ---
        // [Display(Name = "ID Bình luận liên quan")] public int? RelatedCommentId { get; set; }
        // [Display(Name = "ID Bài học liên quan")] public int? RelatedLessonId { get; set; }
        // [Display(Name = "ID Bài kiểm tra liên quan")] public int? RelatedTestId { get; set; }
        // Thêm các ID cụ thể khác nếu cần (ví dụ: User ID nếu thông báo liên quan đến User cụ thể)


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