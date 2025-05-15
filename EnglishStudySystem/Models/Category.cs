using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For [DatabaseGenerated] if needed, or other specific EF attributes

namespace EnglishStudySystem.Models
{
    // Interface cho Soft Delete
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }

    public class Category : ISoftDeletable
    {
        [Key] // Đặt Category_ID làm khóa chính
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Đảm bảo cột này tự động tăng trong DB
        public int Id { get; set; } // Đổi Category_ID thành Id theo quy ước Entity Framework

        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên danh mục không được vượt quá 255 ký tự.")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        // --- LƯU Ý ĐẶC BIỆT VỀ CỘT 'PRICE' ---
        // Cột Price (Giá) thường không nằm trong bảng Category (Danh mục).
        // Price thường thuộc về một đối tượng cụ thể trong danh mục (ví dụ: một Khóa học).
        // Vui lòng xác nhận lại mục đích của cột này.
        // Nếu nó liên quan đến giá của một khóa học cụ thể, nó nên được di chuyển sang model Course.
        [Column(TypeName = "decimal")] // Định dạng cột trong DB
        [Display(Name = "Giá")]
        public decimal? Price { get; set; } // Sử dụng decimal? cho phép giá trị null

        // --- THÔNG TIN THEO DÕI (AUDIT FIELDS) ---
        // Các trường CreatedBy, UpdatedBy nên là string để khớp với Id của ApplicationUser (GUID).
        [Required]
        [Display(Name = "Người tạo")]
        public string CreatedByUserId { get; set; } // Lưu ID của người tạo (string từ ApplicationUser.Id)

        [StringLength(15)]
        [Display(Name = "Vai trò người tạo")]
        public string CreatedByUserRole { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Người cập nhật")]
        public string UpdatedByUserId { get; set; } // Lưu ID của người cập nhật (string từ ApplicationUser.Id)

        [StringLength(15)]
        [Display(Name = "Vai trò người cập nhật")]
        public string UpdatedByUserRole { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; } // Cho phép null nếu chưa được cập nhật

        // --- CHỨC NĂNG XÓA MỀM (SOFT DELETE) ---
        [Required]
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false; // Mặc định là false (chưa xóa)

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; } // Lưu thời gian xóa mềm
        // --- THUỘC TÍNH ĐIỀU HƯỚNG ---
        public virtual ICollection<Lesson> Lessons { get; set; }

        // Constructor để khởi tạo collection, tránh lỗi null reference
        public Category()
        {
            Lessons = new HashSet<Lesson>();
        }
    }
}