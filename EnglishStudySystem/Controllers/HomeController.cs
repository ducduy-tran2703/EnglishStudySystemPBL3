using EnglishStudySystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            var categories = _context.Categories
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .Take(6)
            .ToList();

            return View(categories);
        }

    }
}