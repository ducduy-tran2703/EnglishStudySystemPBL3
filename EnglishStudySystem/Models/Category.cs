using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnglishStudySystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        // Navigation property
        public virtual ICollection<Lesson> Lessons { get; set; }
    }
}