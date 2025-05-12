using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EnglishStudySystem.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên quyền")]
        public string Name { get; set; } // Ví dụ: "CanManageUsers", "CanManageLessons", "CanManageCategories"

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        // Navigation property
        public virtual ICollection<UserPermission> UserPermissions { get; set; }
    }
}