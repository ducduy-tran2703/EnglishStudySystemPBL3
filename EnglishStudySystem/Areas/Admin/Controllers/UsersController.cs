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
            // Lấy TẤT CẢ user từ database
            var allUsersFromDb = await _userManager.Users.ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total users fetched from DB: {allUsersFromDb.Count}"); // Ghi log số lượng

            var userViewModels = new List<UserViewModel>();
            if (allUsersFromDb.Any()) // Chỉ lặp nếu có user
            {
                foreach (var user in allUsersFromDb)
                {
                    System.Diagnostics.Debug.WriteLine($"Processing user: {user.UserName}, IsActive: {user.IsActive}"); // Ghi log từng user

                    if (user.IsActive == true) // Điều kiện lọc
                    {
                        var roles = await _userManager.GetRolesAsync(user.Id);
                        userViewModels.Add(new UserViewModel
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FullName = user.FullName,
                            IsActive = user.IsActive,
                            // AccountStatus = user.AccountStatus, // Đảm bảo ApplicationUser có thuộc tính này
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
            if (string.IsNullOrEmpty(id)) // Thêm kiểm tra id null hoặc rỗng
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Get roles of the user asynchronously
            var roles = await _userManager.GetRolesAsync(user.Id);

            // UserViewModel của bạn (từ EnglishStudySystem.Models)
            // đã tự khởi tạo Categories và PurchasedCategories thành List<Category> rỗng
            // trong constructor của nó.
            var userViewModel = new UserViewModel // UserViewModel từ namespace Models hoặc Admin.ViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,           // Đảm bảo ApplicationUser có FullName
                IsActive = user.IsActive,
                AccountStatus = user.AccountStatus, // Đảm bảo ApplicationUser có AccountStatus và kiểu khớp
                Roles = string.Join(", ", roles)
                // Categories và PurchasedCategories sẽ được điền bên dưới
            };

            // Lấy khóa học ĐÃ MUA nếu user là Customer
            if (roles.Contains("Customer"))
            {
                userViewModel.PurchasedCategories = await _context.Payments
                    .Where(p => p.UserId == user.Id && p.Status == "Completed" && p.Category != null)
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => p.Category) // Lấy đối tượng Category (EF model) trực tiếp từ Payment
                    .Distinct() // Nếu bạn cần đảm bảo mỗi category chỉ xuất hiện một lần
                    .ToListAsync(); // Entity Framework sẽ lấy các đối tượng Category này từ CSDL
            }

            // Lấy khóa học ĐÃ TẠO nếu user là Editor
            if (roles.Contains("Editor")|| roles.Contains("Editor"))

            {
                // GIẢ SỬ:
                // 1. Model Category (EF model) có thuộc tính CreatedByUserId (string, là Id của người tạo).
                // 2. Model Category có IsDeleted (bool hoặc bool?) để không hiển thị các khóa đã xóa mềm.

                userViewModel.Categories = await _context.Categories
     .Where(c => c.CreatedByUserId == user.Id && (c.IsDeleted == true || c.IsDeleted == false))
     .OrderByDescending(c => c.CreatedDate)
     .ToListAsync();
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
                    //_context.SaveChanges();
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
            await _userManager.UpdateAsync(user);
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