// EnglishStudySystem.Models/ViewModels/CategoryCreateViewModel.cs
// (Hoặc tạo thư mục ViewModels và đặt file trong đó)

using System.ComponentModel.DataAnnotations;
// Không cần using System.ComponentModel.DataAnnotations.Schema; cho ViewModel
namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên danh mục không được vượt quá 255 ký tự.")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        // [Column(TypeName = "decimal")] // Attributes liên quan đến DB không cần ở ViewModel
        [Display(Name = "Giá")]
        public decimal? Price { get; set; }

        // KHÔNG bao gồm các thuộc tính Audit (CreatedByUserId, CreatedDate, v.v.)
        // và các thuộc tính Soft Delete (IsDeleted, DeletedAt) vào ViewModel này
    }
}