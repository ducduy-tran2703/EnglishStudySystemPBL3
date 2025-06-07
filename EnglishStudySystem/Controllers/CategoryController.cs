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
        public ActionResult Details(int id)
        {
            var category = _context.Categories
                .FirstOrDefault(c => c.Id == id && !c.IsDeleted);

            if (category == null)
            {
                return HttpNotFound();
            }

            // Sắp xếp bài học: ưu tiên IsFreeTrial = true, sau đó theo CreatedDate
            var lessons = _context.Lessons
                .Where(l => l.CategoryId == id && !l.IsDeleted)
                .OrderBy(l => l.CreatedDate)
                .Select(l => new {
                    l.Id,
                    l.Title,
                    l.Description,
                    l.IsFreeTrial,
                    l.CreatedDate
                })
                .ToList()
                .Select(l => new Lesson
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    IsFreeTrial = l.IsFreeTrial,
                    CreatedDate = l.CreatedDate
                })
                .ToList();

            // Kiểm tra người dùng đã mua CHÍNH XÁC category này chưa
            bool daMua = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                daMua = _context.Payments
                    .Any(p => p.UserId == userId &&
                             p.Status == "Completed" &&
                             p.CategoryId == id);
            }

            var categoriesQuery = _context.Categories
                .Where(c => !c.IsDeleted);
            var categories = categoriesQuery
                .Take(6)
                .ToList();

            ViewBag.ListCategories = categories;
            ViewBag.DaMua = daMua;
            ViewBag.Lessons = lessons;

            return View(category);
        }
        [ChildActionOnly]
        public ActionResult List()
        {
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToList();
            return PartialView("_CategoryDropdownPartial", categories);
        }

    }
}