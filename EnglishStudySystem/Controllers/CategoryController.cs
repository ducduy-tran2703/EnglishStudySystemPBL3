using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using EnglishStudySystem.Models;
using Microsoft.AspNet.Identity;

namespace EnglishStudySystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController()
        {
            _context = new ApplicationDbContext();
        }
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        // In CategoryController.cs (no changes needed, your existing code is fine)
        public ActionResult Details(int id) // `id` ở đây chính là ID của category
        {
            var category = _context.Categories
                .FirstOrDefault(c => c.Id == id && !c.IsDeleted);

            if (category == null)
            {
                return HttpNotFound();
            }

            var lessons = _context.Lessons
                .Where(l => l.CategoryId == id && !l.IsDeleted)
                .OrderBy(l => l.CreatedDate)
                .ToList();
            string ID = id.ToString();
            // Kiểm tra người dùng đã mua CHÍNH XÁC category này chưa
            bool daMua = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                daMua = _context.Payments
                    .Any(p => p.UserId == userId &&
                             p.Status == "Completed" &&
                             p.TransactionId == ID); // So sánh với ID category hiện tại
            }

            ViewBag.DaMua = daMua;
            ViewBag.Lessons = lessons;
            return View(category);
        }
    }
}