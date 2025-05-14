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
            System.Diagnostics.Debug.WriteLine("Entering POST EditProfile1...");
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("HomePage", "Home");
            }
            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
            System.Diagnostics.Debug.WriteLine($"Updating user {userId} with:");
            System.Diagnostics.Debug.WriteLine($"FullName: {model.FullName}");
            System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
            System.Diagnostics.Debug.WriteLine($"PhoneNumber: {model.PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"DateOfBirth: {model.DateOfBirth}");
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProfile(ProfileViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("Entering POST EditProfile...");
            
                var userId = User.Identity.GetUserId();
                var user = _context.Users.Find(userId);
                System.Diagnostics.Debug.WriteLine($"1:");        
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Login", "Account");
                }
                System.Diagnostics.Debug.WriteLine($"Updating user {userId} with:");
                System.Diagnostics.Debug.WriteLine($"FullName: {model.FullName}");
                System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
                System.Diagnostics.Debug.WriteLine($"PhoneNumber: {model.PhoneNumber}");
                System.Diagnostics.Debug.WriteLine($"DateOfBirth: {model.DateOfBirth}");
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.DateOfBirth = model.DateOfBirth;

                try
                {
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("EditProfile", "Customer");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ModelState.AddModelError("", "An error occurred while updating the profile.");
                    return View(model);
                }
            
            return View(model);
        }

    }
}