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
        public ActionResult Load(int id)
        {
            Session["Layout"] = null;
            if (User.Identity.GetUserId() == null)
            {
                Session["Layout"] = "~/Views/Shared/_Layout.cshtml";
            }
            else
            {
                Session["Layout"] = "~/Views/Shared/LayoutCustomer.cshtml";
            }
            return RedirectToAction("Details", "Category", new {id = id});

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
            var categoriesQuery = _context.Categories
                .Where(c => !c.IsDeleted);
            var categories = categoriesQuery
                .Take(6) // giữ nếu bạn chỉ muốn 6 kết quả, có thể bỏ nếu muốn toàn bộ
                .ToList();
            ViewBag.Layout = Session["layout"];
            ViewBag.ListCategories = categories;
            ViewBag.Lessons = lessons;
            return View(category);
        }
    }
}