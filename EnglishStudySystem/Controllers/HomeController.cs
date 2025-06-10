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
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult HomePage(string sortOrder)
        {
            
            var categoriesQuery = _context.Categories
                .Where(c => !c.IsDeleted);

            // Áp dụng thứ tự sắp xếp
            switch (sortOrder)
            {
                case "oldest":
                    categoriesQuery = categoriesQuery.OrderBy(c => c.CreatedDate);
                    break;
                case "name":
                    categoriesQuery = categoriesQuery.OrderBy(c => c.Name);
                    break;
                default:
                    categoriesQuery = categoriesQuery.OrderByDescending(c => c.CreatedDate); // Mặc định: Mới nhất
                    break;
            }

            var categories = categoriesQuery
                 // giữ nếu bạn chỉ muốn 6 kết quả, có thể bỏ nếu muốn toàn bộ
                .ToList();

            // Lấy danh sách userIds từ các categories
            var userIds = categories
                .Select(c => c.CreatedByUserId)
                .Distinct()
                .ToList();

            // Truy vấn bảng Users
            var users = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName);

            // Truyền dữ liệu qua ViewBag
            ViewBag.UserNames = users;
            ViewBag.ListCategories = categories;
            ViewBag.CurrentSort = sortOrder;

            return View(categories);
        }
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult FindHomePage(string keyword)
        {
            // Lấy danh sách categories
            var ViewBagcategories = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToList();
            var categoriesQuery = _context.Categories
            .Where(c => !c.IsDeleted);

            if (!string.IsNullOrEmpty(keyword))
            {
                categoriesQuery = categoriesQuery
                    .Where(c => c.Name.ToLower().Contains(keyword.ToLower()));
            }

            var categories = categoriesQuery
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToList();
            var userIds = categories.Select(c => c.CreatedByUserId).Distinct().ToList();

            var users = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName);

            ViewBag.UserNames = users;
            ViewBag.ListCategories = ViewBagcategories;
            ViewBag.Keyword = keyword;
            return View(categories);
        }
    }
}