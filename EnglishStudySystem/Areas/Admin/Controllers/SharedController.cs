
using EnglishStudySystem.Models;
using EnglishStudySystem.Areas.Admin.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace EnglishStudySystem.Areas.Admin.Controllers
{
    public class SharedController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        [ChildActionOnly]
        public ActionResult QuickStats()
        {
            var stats = new QuickStatsViewModel();
            try
            {
                // Lấy RoleId của Customer và Editor
                string customerRoleId = _db.Roles.FirstOrDefault(role => role.Name == "Customer")?.Id;
                string editorRoleId = _db.Roles.FirstOrDefault(role => role.Name == "Editor")?.Id;

                // Đếm Customer
                if (!string.IsNullOrEmpty(customerRoleId))
                {
                    stats.TotalCustomers = _db.Users.Count(u => u.Roles.Any(ur => ur.RoleId == customerRoleId));
                }
                else
                {
                    stats.TotalCustomers = 0;
                    System.Diagnostics.Debug.WriteLine("WARNING (SharedController): Role 'Customer' not found by name for QuickStats.");
                }

                // Đếm Editor
                if (!string.IsNullOrEmpty(editorRoleId))
                {
                    stats.TotalEditors = _db.Users.Count(u => u.Roles.Any(ur => ur.RoleId == editorRoleId));
                }
                else
                {
                    stats.TotalEditors = 0;
                    System.Diagnostics.Debug.WriteLine("WARNING (SharedController): Role 'Editor' not found by name for QuickStats.");
                }

                // Đếm Khóa học (điều chỉnh nếu tên bảng/model khác)
                // Giả sử Categories là Khóa học, hoặc bạn có bảng Courses riêng
                stats.TotalCourses = _db.Categories.Count(); // Hoặc _db.Courses.Count();

                // Tính tổng lợi nhuận từ các thanh toán đã hoàn thành
                stats.TotalRevenue = _db.Payments
                                       .Where(p => p.Status == "Completed")
                                       .Sum(p => (decimal?)p.Amount) ?? 0m; // Nếu Amount có thể null, ?? 0m
                                                                            // Nếu Amount không thể null, chỉ cần .Sum(p => p.Amount)

            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching quick stats in SharedController: {ex.Message}\nStackTrace: {ex.StackTrace}");
                // Gán giá trị mặc định nếu có lỗi
                stats.TotalCustomers = 0;
                stats.TotalEditors = 0;
                stats.TotalCourses = 0;
                stats.TotalRevenue = 0m; // Mặc định cho decimal
            }

            return PartialView("_QuickStats", stats);
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