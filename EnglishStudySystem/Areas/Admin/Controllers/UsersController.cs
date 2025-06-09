using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using EnglishStudySystem.Models;
using System.Data.Entity;
using EnglishStudySystem.Areas.Admin.ViewModel;
using Microsoft.Owin.Logging;
using System.Security.Claims;
using System.Net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
namespace EnglishStudySystem.Areas.Admin.Controllers
{

    [Authorize(Roles = "Administrator, Editor")]
    public class UsersController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public UsersController()
        {
            _context = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _userManager.Dispose();
                _roleManager.Dispose();
            }
            base.Dispose(disposing);
        }


        public async Task<ActionResult> ListUser()
        {
            var user_now = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (user_now == null)
            {
                return HttpNotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user_now.Id);
            bool isAdmin = userRoles[0].Contains("Admin");
            ViewBag.IsAdmin = isAdmin;
            ViewBag.User_now = user_now;
            System.Diagnostics.Debug.WriteLine($"Admin Roles: {string.Join(", ", userRoles)}");
            System.Diagnostics.Debug.WriteLine($"Admin Roles: {isAdmin}");

            var allUsersFromDb = await _userManager.Users.ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total users fetched from DB: {allUsersFromDb.Count}");

            var userViewModels = new List<UserViewModel>();
            if (allUsersFromDb.Any())
            {
                foreach (var user in allUsersFromDb)
                {
                    System.Diagnostics.Debug.WriteLine($"Processing user: {user.UserName}, IsActive: {user.IsActive}");

                    if (user.IsActive == true)
                    {
                        var roles = await _userManager.GetRolesAsync(user.Id);
                        userViewModels.Add(new UserViewModel
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FullName = user.FullName,
                            IsActive = user.IsActive,
                            CanManageUsers = user.CanManageUsers,
                            CanManageCategories = user.CanManageCategories,
                            CanManageNotifications = user.CanManageNotifications,

                            Roles = string.Join(", ", roles)
                        });
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No users found in the database.");
            }

            System.Diagnostics.Debug.WriteLine($"Number of active users being sent to view: {userViewModels.Count}");
            return View(userViewModels);
        }
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var roles = await _userManager.GetRolesAsync(user.Id);

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                AccountStatus = user.AccountStatus,
                CanManageCategories = user.CanManageCategories,
                CanManageNotifications = user.CanManageNotifications,
                CanManageUsers = user.CanManageUsers,
                Roles = string.Join(", ", roles)

            };

            if (roles.Contains("Customer"))
            {
                userViewModel.PurchasedCategories = await _context.Payments
                    .Where(p => p.UserId == user.Id && p.Status == "Completed" && p.Category != null)
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => p.Category)
                    .Distinct()
                    .ToListAsync();
            }

            if (roles.Contains("Editor") || roles.Contains("Editor"))
            {
                userViewModel.Categories = await _context.Categories
                    .Where(c => c.CreatedByUserId == user.Id && (c.IsDeleted == true || c.IsDeleted == false))
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();
            }

            return View(userViewModel);
        }
        public ActionResult Edit(string id)
        {
            var user = _userManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var roles = _userManager.GetRoles(user.Id);
            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                AccountStatus = user.AccountStatus,
                Roles = string.Join(", ", roles)
            };
            return View(userViewModel);
        }
        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var roles = _userManager.GetRoles(user.Id);

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                AccountStatus = user.AccountStatus,
                Roles = string.Join(", ", roles)
            };
            return View(userViewModel);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            user.IsActive = false;
            _context.SaveChanges();
            return RedirectToAction("ListUser");
        }

        public ActionResult CreateEditor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEditor(RegisterEditorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user.Id, "Editor");

                    TempData["CreateEditorSuccess"] = true;
                    TempData["CreateEditorMessage"] = "Tài khoản Editor đã được tạo thành công!";
                    return RedirectToAction("CreateEditor");
                }
            }

            return View(model);
        }
        public async Task<ActionResult> ListDeletedAccount()
        {

            var user_now = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (user_now == null)
            {
                return HttpNotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user_now.Id);
            bool isAdmin = userRoles[0].Contains("Admin");
            ViewBag.IsAdmin = isAdmin;
            ViewBag.User_now = user_now;
            var usersWithRoles = await _userManager.Users.ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in usersWithRoles)
            {
                var roles = await _userManager.GetRolesAsync(user.Id);
                if (user.IsActive == false)
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        IsActive = user.IsActive,
                        AccountStatus = user.AccountStatus,
                        Roles = string.Join(", ", roles)
                    });
            }

            return View(userViewModels);
        }
        [HttpGet]
        public async Task<ActionResult> Revive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var roles = _userManager.GetRoles(user.Id);

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                AccountStatus = user.AccountStatus,
                Roles = string.Join(", ", roles)
            };
            return View(userViewModel);
        }

        [HttpPost]
        [ActionName("Revive")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReviveComfirm(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            user.IsActive = true;
            _context.SaveChanges();
            return RedirectToAction("ListUser");
        }


        [HttpGet]
        public async Task<ActionResult> DeletePermanent(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var roles = _userManager.GetRoles(user.Id);

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                AccountStatus = user.AccountStatus,
                Roles = string.Join(", ", roles)
            };
            return View(userViewModel);
        }

        [HttpPost]
        [ActionName("DeletePermanent")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CofirmDeletePermanent(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Tài khoản đã được xóa thành công!";
                return RedirectToAction("ListDeletedAccount");
            }
            else
            {
                TempData["ErrorMessage"] = "Xảy ra lỗi khi xóa tài khoản!";
                return RedirectToAction("DeletePermanent", new { id = id });
            }
        }
        [HttpPost]
        [ActionName("LockUserManager")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LockUserManager(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.CanManageUsers = !user.CanManageUsers;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "User", "LockUserManager"));
            }

            return RedirectToAction("ListUser");
        }

        [HttpPost]
        [ActionName("LockCategoryManager")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LockCategoryManager(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.CanManageCategories = !user.CanManageCategories;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "User", "LockCategoryManager"));
            }

            return RedirectToAction("ListUser");
        }

        [HttpPost]
        [ActionName("LockNotificationManager")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LockNotificationManager(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.CanManageNotifications = !user.CanManageNotifications;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "User", "LockNotificationManager"));
            }

            return await ListUser();
        }
    }
}