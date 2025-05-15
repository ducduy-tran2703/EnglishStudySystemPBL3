using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// EnglishStudySystem/Areas/Admin/ViewModels/QuickStatsViewModel.cs
// EnglishStudySystem/Areas/Admin/ViewModels/QuickStatsViewModel.cs
namespace EnglishStudySystem.Areas.Admin.ViewModels
{
    public class QuickStatsViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalEditors { get; set; }
        public int TotalCourses { get; set; }
        public decimal TotalRevenue { get; set; } // Thuộc tính mới cho tổng lợi nhuận
    }
}