using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // Để sử dụng [AllowHtml]

namespace EnglishStudySystem.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Tiêu đề bài học")]
        public string Title { get; set; }

        [Required]
        [AllowHtml] // Cho phép HTML để soạn thảo nội dung phong phú
        [Display(Name = "Nội dung")]
        public string Content { get; set; } // Ví dụ: văn bản, âm thanh, hình ảnh, video

        [Display(Name = "Là bài học công khai?")]
        public bool IsPublic { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        [Display(Name = "Số lượt xem")]
        public int Views { get; set; } = 0;

        // Foreign Key
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}