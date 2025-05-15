using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models // Đảm bảo namespace này khớp với dự án của bạn
{
    // Lesson cũng sẽ implement ISoftDeletable để có chức năng xóa mềm
    public class Lesson : ISoftDeletable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tăng giá trị ID
        public int Id { get; set; } // Tương ứng với Lesson_ID

        [Required(ErrorMessage = "ID danh mục là bắt buộc.")]
        [Display(Name = "ID Danh mục")]
        public int CategoryId { get; set; } // Tương ứng với Category_ID

        [ForeignKey("CategoryId")] // Thiết lập khóa ngoại đến Category
        public virtual Category Category { get; set; } // Thuộc tính điều hướng tới Category

        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "URL Video")]
        public string Video_URL { get; set; } // Tương ứng với Video_URL

        // --- THÔNG TIN THEO DÕI (AUDIT FIELDS) ---
        // Các trường CreatedBy, UpdatedBy nên là string để khớp với Id của ApplicationUser (GUID).
        [Required]
        [Display(Name = "Người tạo")]
        public string CreatedByUserId { get; set; } // Lưu ID của người tạo (string từ ApplicationUser.Id)

        [StringLength(15)]
        [Display(Name = "Vai trò người tạo")]
        public string CreatedByUserRole { get; set; }

        [Required] // Theo hình ảnh, Created_Date không cho phép null
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Người cập nhật")]
        public string UpdatedByUserId { get; set; } // Lưu ID của người cập nhật (string từ ApplicationUser.Id)

        [StringLength(10)]
        [Display(Name = "Vai trò người cập nhật")]
        public string UpdatedByUserRole { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; } // Cho phép null nếu chưa được cập nhật

        // --- CHỨC NĂNG XÓA MỀM (SOFT DELETE) ---
        [Required] // Theo hình ảnh, Is_Deleted không cho phép null (bit, unchecked)
        [Display(Name = "Đã xóa")]
        public bool IsDeleted { get; set; } = false; // Mặc định là false (chưa xóa)

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xóa")]
        public DateTime? DeletedAt { get; set; } // Lưu thời gian xóa mềm

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
        public Lesson()
        {
            // Khởi tạo các collection để tránh lỗi null reference khi truy cập lần đầu
            Comments = new HashSet<Comment>();
            Tests = new HashSet<Test>();
        }
    }
}