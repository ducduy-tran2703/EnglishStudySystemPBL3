using EnglishStudySystem.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;

namespace EnglishStudySystem.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Test()
        {
            return View();
        }
        public ActionResult GetBoughtCoursesStats()
        {
            ApplicationDbContext _context = new ApplicationDbContext();
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToList();

            var userIds = categories.Select(c => c.CreatedByUserId).Distinct().ToList();

            var users = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName);
            ViewBag.UserNames = users;

            return PartialView("_BoughtCoursesStats", categories);
        }
        public ActionResult GetLessonsHistoryStats()
        {
            ApplicationDbContext _context = new ApplicationDbContext();
            var lessons = _context.Lessons
                .Where(l => !l.IsDeleted)
                .OrderByDescending(l => l.CreatedDate)
                .Take(6)
                .ToList();
            return PartialView("_LessonsHistoryStats", lessons);

        }
        //public ActionResult SeedLessons()
        //{
        //    ApplicationDbContext db = new ApplicationDbContext();
        //    if (db.Lessons.Any()) // Tránh thêm trùng nếu đã có dữ liệu
        //        return Content("Dữ liệu đã tồn tại.");

        //    var lessons = new List<Lesson>();
        //    var random = new Random();
        //    string userId = "aac1d442-dfc3-4725-8585-332a80f63f37"; // Thay bằng ID người tạo (ApplicationUser.Id)
        //    string userRole = "Admin";

        //    for (int i = 1; i <= 20; i++)
        //    {
        //        lessons.Add(new Lesson
        //        {
        //            CategoryId = 1, // ID danh mục (giả sử đã tồn tại)
        //            Title = $"Bài học số {i}",
        //            Description = $"Mô tả cho bài học {i}",
        //            Video_URL = $"https://example.com/video/{i}",
        //            CreatedByUserId = userId,
        //            CreatedByUserRole = userRole,
        //            CreatedDate = DateTime.Now,
        //            IsDeleted = false
        //        });
        //    }

        //    db.Lessons.AddRange(lessons);
        //    db.SaveChanges();

        //    return Content("Đã thêm 20 bài học thành công.");
        //}
    }
}