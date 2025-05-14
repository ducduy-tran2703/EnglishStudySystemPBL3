using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Số tiền")]
        [Column(TypeName = "decimal")] // Lưu ý: Sử dụng decimal cho tiền tệ
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Ngày thanh toán")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } // Enum: Pending, Completed, Failed, Refunded

        [StringLength(255)]
        [Display(Name = "Mã giao dịch")]
        public string TransactionId { get; set; } // ID từ cổng thanh toán

        [StringLength(100)]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } // Ví dụ: Credit Card, PayPal, VNPay

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } // Mô tả khoản thanh toán (vd: "Nâng cấp VIP 1 tháng", "Mua gói bài học nâng cao")

        // Foreign Key to ApplicationUser
        [Display(Name = "Người dùng")]
        public string UserId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        // Foreign Key to Category
        [Display(Name = "Danh mục Khóa học")]
        public int CategoryId { get; set; } // Giả định Category là đơn vị có thể thanh toán
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

    }



    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }
}