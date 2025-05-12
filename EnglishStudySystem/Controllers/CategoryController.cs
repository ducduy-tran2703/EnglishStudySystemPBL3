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
        // GET: Category
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public ActionResult Index()
        {
            
            return View();
        }
        public ActionResult ShowAllCategories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
    }
}