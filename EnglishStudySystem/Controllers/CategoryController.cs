using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnglishStudySystem.Models;

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

            var lessons = _context.Lessons
                .Where(l => l.CategoryId == id && !l.IsDeleted)
                .OrderBy(l => l.CreatedDate)
                .ToList();

            ViewBag.Lessons = lessons;
            return View(category);
        }
    }
}