using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnglishStudySystem.Models;
using EnglishStudySystem.ViewModel;
using Microsoft.AspNet.Identity;

namespace EnglishStudySystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController()
        {
            _context = new ApplicationDbContext();
        }

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public ActionResult CustomerDashBoard()
        {
            Session["Layout"] = null;
            Session["Layout"] = "~/Views/Shared/LayoutCustomer.cshtml";
            // Lấy danh sách categories giống như HomePage
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToList();

            // Lấy thông tin user names nếu cần
            var userIds = categories.Select(c => c.CreatedByUserId).Distinct().ToList();
            var userNames = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName);

            ViewBag.UserNames = userNames;
            ViewBag.ListCategory = categories;
            ViewBag.Layout = Session["Layout"];
            return View(categories);
        }
        public ActionResult Payment(int categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            if (category == null)
            {
                return HttpNotFound();
            }
            // Lấy thông tin bài học nếu cần
            var lessons = _context.Lessons
                .Where(l => l.CategoryId == categoryId && !l.IsDeleted)
                .ToList();
            ViewBag.Lessons = lessons;
            return View(category);
        }
        public ActionResult DetailUser()
        {
            var userId = User.Identity.GetUserId(); // This method is part of Microsoft.AspNet.Identity namespace
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }
            // Lấy thông tin bài học nếu cần
            var categories = _context.Categories
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .Take(6)
            .ToList();
            ViewBag.ListCategories = categories;
            return View(user);
        }
        public ActionResult EditProfile()
        {
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);
            if (user == null) return HttpNotFound("User not found");
            var model = new ProfileViewModel
            {
                Fullname = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
            
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Author,Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = _context.Users.Find(userId);
                if (user == null) return HttpNotFound("User not found");
                user.FullName = model.Fullname;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = model.DateOfBirth;
                _context.SaveChanges(); // Corrected the variable name to '_context'  
                return RedirectToAction("Infor");
            }
            return View(model);
        }
    }
}