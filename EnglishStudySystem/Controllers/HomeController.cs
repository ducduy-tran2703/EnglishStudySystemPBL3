using EnglishStudySystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;

namespace EnglishStudySystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController()
        {
             _context = new ApplicationDbContext();
        }
        public HomeController(ApplicationDbContext context)
        {
             _context = context;
        }
        public ActionResult HomePage()
        {
            // Lấy danh sách categories
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToList();

            // Lấy danh sách userIds từ các categories
            var userIds = categories.Select(c => c.CreatedByUserId).Distinct().ToList();

            // Truy vấn bảng Users để lấy thông tin người dùng
            var users = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName);

            // Truyền dữ liệu qua ViewBag
            ViewBag.UserNames = users;

            return View(categories);
        }

    }
}