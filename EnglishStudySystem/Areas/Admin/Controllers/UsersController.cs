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

namespace EnglishStudySystem.Areas.Admin.Controllers
{
    // Chỉ cho phép người dùng có vai trò "Administrator" truy cập Controller này
    [Authorize(Roles = "Administrator")]
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
        public async Task<ActionResult> Index()
        {
            // Lấy tất cả người dùng từ cơ sở dữ liệu
            // Lấy luôn các vai trò của từng người dùng để hiển thị
            var usersWithRoles = await _userManager.Users.ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in usersWithRoles)
            {
                var roles = await _userManager.GetRolesAsync(user.Id);
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

        // Các Action khác như Create, Edit, Delete sẽ được thêm sau

        // Action để hiển thị chi tiết quyền của Editor và cấp quyền
        // Public async Task<ActionResult> ManagePermissions(string id) { ... }
    }
}