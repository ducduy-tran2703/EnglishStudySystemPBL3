using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishStudySystem.Models
{
    public class UserPermission
    {
        public int Id { get; set; }

        // Foreign Keys
        [Display(Name = "Người dùng")]
        public string UserId { get; set; } // Thường sẽ là Editor

        [Display(Name = "Quyền")]
        public int PermissionId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }
    }
}