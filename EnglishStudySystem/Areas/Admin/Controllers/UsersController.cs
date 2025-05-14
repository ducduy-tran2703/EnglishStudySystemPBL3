using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using EnglishStudySystem.Models;
using System.Data.Entity; // Đảm bảo đã thêm để truy cập các models của bạn
using EnglishStudySystem.Areas.Admin.ViewModel;
using Microsoft.Owin.Logging;
using System.Security.Claims;
using System.Net;
namespace EnglishStudySystem.Areas.Admin.Controllers
{
    // Chỉ cho phép người dùng có vai trò "Administrator" truy cập Controller này
    [Authorize(Roles = "Administrator, Editor" )]
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

        // GET: Admin/Users
        public async Task<ActionResult> ListUser()
        {
       
            var usersWithRoles = await _userManager.Users.ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in usersWithRoles)
            {
                var roles = await _userManager.GetRolesAsync(user.Id);
                if(user.IsActive == true)
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
        public async Task<ActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Get roles of the user asynchronously
            var roles = await _userManager.GetRolesAsync(user.Id);

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
            
            // Add purchased courses if user is a Customer
            if (roles.Contains("Customer"))
            {
                var purchasedCourses = await _context.Categories
                    .Where(uc => uc.CreatedByUserId == user.Id && !uc.IsDeleted)
                    .Select(uc => new Category
                    {
                        Id = uc.Id,
                        Name = uc.Name,
                        Description = uc.Description,
                        Price = uc.Price,
                        CreatedDate = uc.CreatedDate
                    })
                    .ToListAsync(); // Make it asynchronous

                userViewModel.PurchasedCategories = purchasedCourses;
            }

            // Add created courses if user is an Editor
            if (roles.Contains("Editor"))
            {
                var createdCourses = await _context.Categories
                    .Where(c => c.CreatedByUserId == id && !c.IsDeleted)
                    .Select(c => new Category
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Price = c.Price,
                        CreatedDate = c.CreatedDate
                    })
                    .ToListAsync(); // Make it asynchronous

                userViewModel.Categories = createdCourses;
            }

            return View(userViewModel);
        }

        public ActionResult Edit(string id)//chưa sửa
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
                // Handle errors
              
                //return RedirectToAction("Delete", new { id });
        }
            
        
        //[Authorize(Roles = "Admin")]
        public ActionResult CreateEditor()
        {
            return View();
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
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
                   
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán quyền Editor
                    await _userManager.AddToRoleAsync(user.Id, "Editor");

                    // Gửi email thông báo (nếu cần)
                    // await SendEditorAccountEmail(user.Email, model.UserName, model.Password);

                    TempData["SuccessMessage"] = "Đã tạo tài khoản Editor thành công!";
                    return RedirectToAction("CreateEditor");
                }

                
            }

            // Nếu có lỗi, hiển thị lại form với thông báo lỗi
            return View(model);
        }
        public async Task<ActionResult> ListDeletedAccount()
        {

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
            // Handle errors

            //return RedirectToAction("Delete", new { id });
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
                return RedirectToAction("ListUser");  // Chuyển hướng về trang danh sách người dùng
            }
            else
            {
                TempData["ErrorMessage"] = "Xảy ra lỗi khi xóa tài khoản!";
                return RedirectToAction("DeletePermanent", new { id = id });  // Quay lại trang xóa với thông báo lỗi
            }
            // Handle errors

            //return RedirectToAction("Delete", new { id });
        }
        
    }
}