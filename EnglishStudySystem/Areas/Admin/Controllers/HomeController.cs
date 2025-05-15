
using EnglishStudySystem.Areas.Admin.ViewModel; 
using EnglishStudySystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EnglishStudySystem.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfYear = new DateTime(today.Year, 1, 1);

            var viewModel = new DashboardViewModel();

            // Lấy RoleId từ tên Role một lần để sử dụng lại
            string customerRoleId = null;
            string editorRoleId = null;
            try
            {
                customerRoleId = _db.Roles.FirstOrDefault(role => role.Name == "Customer")?.Id;
                editorRoleId = _db.Roles.FirstOrDefault(role => role.Name == "Editor")?.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL: Error fetching Role IDs: {ex.Message} \nStackTrace: {ex.StackTrace}");
                // Nếu không lấy được Role ID, các thống kê liên quan sẽ không chính xác.
                // Bạn có thể muốn ghi log nghiêm trọng hơn hoặc hiển thị lỗi cho admin.
            }


            // Thống kê người dùng
            try
            {
                // Đếm Customer
                if (!string.IsNullOrEmpty(customerRoleId))
                {
                    viewModel.UserStats.TotalCustomers = _db.Users
                        .Count(u => u.Roles.Any(ur => ur.RoleId == customerRoleId));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Role 'Customer' not found by name, TotalCustomers will be 0.");
                    viewModel.UserStats.TotalCustomers = 0;
                }

                // Đếm Editor
                if (!string.IsNullOrEmpty(editorRoleId))
                {
                    viewModel.UserStats.TotalEditors = _db.Users
                        .Count(u => u.Roles.Any(ur => ur.RoleId == editorRoleId));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Role 'Editor' not found by name, TotalEditors will be 0.");
                    viewModel.UserStats.TotalEditors = 0;
                }

                // Top Customer (chỉ thực hiện nếu có customerRoleId)
                if (!string.IsNullOrEmpty(customerRoleId))
                {
                    var topCustomerData = _db.Payments
                        .Where(p => p.Status == "Completed" && p.User != null && p.User.Roles.Any(r => r.RoleId == customerRoleId)) // Sử dụng customerRoleId
                        .GroupBy(p => p.User)
                        .OrderByDescending(g => g.Count())
                        .Select(g => new
                        {
                            User = g.Key,
                            TotalPurchases = g.Count(),
                            TotalSpent = g.Sum(p => (decimal?)p.Amount) ?? 0m,
                            PurchasedCategoriesData = g
                                .Where(p => p.Category != null)
                                .Select(p => p.Category)
                                .Distinct()
                                .ToList()
                        })
                        .FirstOrDefault();

                    if (topCustomerData != null)
                    {
                        viewModel.UserStats.TopCustomer = new TopCustomerViewModel
                        {
                            Name = topCustomerData.User.UserName,
                            TotalPurchases = topCustomerData.TotalPurchases,
                            TotalSpent = topCustomerData.TotalSpent,
                            PurchasedCategories = topCustomerData.PurchasedCategoriesData
                                                    .Select(c => new CategoryBasicViewModel { Name = c.Name })
                                                    .ToList()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error UserStats: {ex.Message} \nStackTrace: {ex.StackTrace}");
            }


            //Thống kê doanh thu
            var months = Enumerable.Range(0, 12)
                                 .Select(i => firstDayOfYear.AddMonths(i))
                                 .ToList();
            try
            {
                viewModel.RevenueStats.TotalRevenue = _db.Payments
                    .Where(p => p.Status == "Completed")
                    .Sum(p => (decimal?)p.Amount) ?? 0m;
                viewModel.RevenueStats.MonthlyRevenue = _db.Payments
                    .Where(p => p.Status == "Completed" && p.PaymentDate >= firstDayOfMonth)
                    .Sum(p => (decimal?)p.Amount) ?? 0m;
                viewModel.RevenueStats.YearlyRevenue = _db.Payments
                    .Where(p => p.Status == "Completed" && p.PaymentDate >= firstDayOfYear)
                    .Sum(p => (decimal?)p.Amount) ?? 0m;
                viewModel.RevenueStats.RevenueByMonth = months
                    .Select(monthDate => new MonthlyRevenueItemViewModel
                    {
                        Month = monthDate.ToString("MM/yyyy"),
                        Revenue = _db.Payments
                            .Where(p => p.Status == "Completed"
                                && p.PaymentDate.Year == monthDate.Year
                                && p.PaymentDate.Month == monthDate.Month)
                            .Sum(p => (decimal?)p.Amount) ?? 0m
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error RevenueStats: {ex.Message} \nStackTrace: {ex.StackTrace}");
            }


            // Thống kê khóa học (LessonStats)
            try
            {
                viewModel.LessonStats.TotalCategories = _db.Categories.Count();

                var popularCategoryData = _db.Categories
                    .Select(c_entity => new // Đổi tên biến để tránh nhầm lẫn
                    {
                        CategoryEntity = c_entity,
                        TotalPurchases = _db.Payments.Count(p => p.CategoryId == c_entity.Id && p.Status == "Completed"),
                        Revenue = _db.Payments.Where(p => p.CategoryId == c_entity.Id && p.Status == "Completed").Sum(p => (decimal?)p.Amount) ?? 0m
                    })
                    .OrderByDescending(x => x.TotalPurchases)
                    .FirstOrDefault();

                if (popularCategoryData != null && popularCategoryData.CategoryEntity != null)
                {
                    viewModel.LessonStats.MostPopularCategory = new CategorySummaryViewModel
                    {
                        Name = popularCategoryData.CategoryEntity.Name,
                        TotalPurchases = popularCategoryData.TotalPurchases,
                        Revenue = popularCategoryData.Revenue
                    };
                }

                var profitableCategoryData = _db.Categories
                    .Select(c_entity => new // Đổi tên biến
                    {
                        CategoryEntity = c_entity,
                        Revenue = _db.Payments.Where(p => p.CategoryId == c_entity.Id && p.Status == "Completed").Sum(p => (decimal?)p.Amount) ?? 0m
                    })
                    .OrderByDescending(x => x.Revenue)
                    .FirstOrDefault();

                if (profitableCategoryData != null && profitableCategoryData.CategoryEntity != null)
                {
                    viewModel.LessonStats.MostProfitableCategory = new CategorySummaryViewModel
                    {
                        Name = profitableCategoryData.CategoryEntity.Name,
                        Revenue = profitableCategoryData.Revenue
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LessonStats: {ex.Message} \nStackTrace: {ex.StackTrace}");
            }


            // Thống kê editor
            try
            {
                // Chỉ thực hiện nếu có editorRoleId
                if (!string.IsNullOrEmpty(editorRoleId))
                {
                    viewModel.EditorStats = _db.Users
                        .Where(u => u.Roles.Any(r => r.RoleId == editorRoleId)) // Sử dụng editorRoleId
                        .Select(u => new EditorStatItemViewModel
                        {
                            EditorName = u.UserName,
                            TotalCategoriesCreated = _db.Categories.Count(c => c.CreatedByUserId == u.Id), // Giả sử Category có CreatedByUserId
                            TotalRevenue = _db.Payments
                                .Where(p => p.Status == "Completed"
                                         && p.Category != null
                                         && p.Category.CreatedByUserId == u.Id) // Liên kết qua Category.CreatedByUserId
                                .Sum(p => (decimal?)p.Amount) ?? 0m
                        })
                        .ToList();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Role 'Editor' not found by name, EditorStats will be empty.");
                    viewModel.EditorStats = new List<EditorStatItemViewModel>(); // Trả về danh sách rỗng
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error EditorStats: {ex.Message} \nStackTrace: {ex.StackTrace}");
            }

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}