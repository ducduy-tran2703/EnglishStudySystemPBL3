using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnglishStudySystem.Areas.Admin.ViewModel
{
    public class UserStatsViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalEditors { get; set; }
        public TopCustomerViewModel TopCustomer { get; set; }
    }

    public class TopCustomerViewModel
    {
        public string Name { get; set; }
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; }
        public List<CategoryBasicViewModel> PurchasedCategories { get; set; }

        public TopCustomerViewModel()
        {
            PurchasedCategories = new List<CategoryBasicViewModel>();
        }
    }

    public class CategoryBasicViewModel
    {
        public string Name { get; set; }
        // public System.Guid Id { get; set; } // Thêm nếu cần Id
    }
}