using System;
using System.Collections.Generic;
using System.Data.Entity; 
using System.Linq; 
using System.Net; 
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EnglishStudySystem;
using EnglishStudySystem.Areas.Admin.ViewModel;
using EnglishStudySystem.Models; 
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace EnglishStudySystem.Areas.Admin.Controllers
{

    [Authorize(Roles = "Administrator, Editor")]
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public CategoriesController()
        {
            db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }
        public async Task<ActionResult> Index()
        {
            
            var user_now = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (user_now == null)
            {
                return HttpNotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user_now.Id);
            ViewBag.CanEdit = User.IsInRole("Administrator")||user_now.CanManageCategories;
      
            var activeCategories = await db.Categories
                                           .Where(c => !c.IsDeleted)
                                           .OrderBy(c => c.Name) 
                                           .ToListAsync(); 


            var createdByUserIds = activeCategories.Select(c => c.CreatedByUserId)
                                                   .Where(id => id != null) 
                                                   .Distinct()
                                                   .ToList();

            var userNames = new Dictionary<string, string>();

            if (createdByUserIds.Any())
            {
                var users = await db.Users
                                    .Where(u => createdByUserIds.Contains(u.Id))
                                    .ToListAsync();

                foreach (var user in users)
                {
                    // Ưu tiên FullName, nếu FullName null/empty thì dùng UserName
                    userNames[user.Id] = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.UserName;
                }
            }


            ViewBag.UserNames = userNames;

            // Truyền danh sách các danh mục đang hoạt động sang View
            return View(activeCategories);
        }

        public async Task<ActionResult> DeletedIndex()
        {
            var deletedCategories = await db.Categories
                                            .Where(c => c.IsDeleted)
                                            .OrderByDescending(c => c.DeletedAt)
                                            .ToListAsync();

            var userIds = deletedCategories.Select(c => c.CreatedByUserId)
                                           .Where(id => id != null)
                                           .ToList();

            var updatedByUserIds = deletedCategories.Select(c => c.UpdatedByUserId)
                                                    .Where(id => id != null)
                                                    .ToList();

            userIds.AddRange(updatedByUserIds);
            userIds = userIds.Distinct().ToList();

            var userNames = new Dictionary<string, string>();

            if (userIds.Any())
            {
                var users = await db.Users
                                    .Where(u => userIds.Contains(u.Id))
                                    .ToListAsync();

                foreach (var user in users)
                {
                    userNames[user.Id] = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.UserName;
                }
            }

            ViewBag.UserNames = userNames;

            return View(deletedCategories);
        }


        public ActionResult GetLessonsPartial(int categoryId, bool showDeleted = false)
        {

            var lessons = db.Lessons
                                  .Include(l => l.CreatedByUser)
                                  .Include(l => l.UpdatedByUser) 
                                  .Where(l => l.CategoryId == categoryId)
                                  .AsQueryable();

            // Lọc theo trạng thái xóa mềm
            if (!showDeleted)
            {
                lessons = lessons.Where(l => !l.IsDeleted);
            }
            else
            {
                lessons = lessons.Where(l => l.IsDeleted);
            }

            // Ánh xạ từ Lesson entity sang LessonViewModel
            var lessonViewModels = lessons.ToList().Select(l => new LessonViewModel
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                IsDeleted = l.IsDeleted,
                IsFree = l.IsFreeTrial,

                CreatedDate = l.CreatedDate,
                CreatedByUserFullName = l.CreatedByUser?.FullName, 

                UpdatedDate = l.UpdatedDate,
                UpdatedByUserFullName = l.UpdatedByUser?.FullName, 

                DeletedAt = l.DeletedAt

            }).OrderBy(l => l.CreatedDate).ToList();

            ViewData["CategoryId"] = categoryId;
            ViewData["ShowDeletedLessons"] = showDeleted;

            return PartialView("_LessonsListPartial", lessonViewModels);
        }

        public async Task<ActionResult> Details(int? id)
        {
            // Kiểm tra ID có được cung cấp không
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = await db.Categories
                                   .Include(c => c.CreatedByUser)
                                   .Include(c => c.UpdatedByUser)
                                   .SingleOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return HttpNotFound();
            }
            // Lấy thông tin người dùng hiện tại để kiểm tra quyền chỉnh sửa
            var user_now = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (user_now == null)
            {
                return HttpNotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user_now.Id);
            ViewBag.CanEdit = User.IsInRole("Administrator") || user_now.CanManageCategories;

            string createdByUserRole = "N/A";
            if (!string.IsNullOrEmpty(category.CreatedByUserId))
            {
                var roles = await _userManager.GetRolesAsync(category.CreatedByUserId);
                createdByUserRole = roles.FirstOrDefault() ?? "N/A";
            }

            string updatedByUserRole = "N/A";
            if (!string.IsNullOrEmpty(category.UpdatedByUserId))
            {
                var roles = await _userManager.GetRolesAsync(category.UpdatedByUserId);
                updatedByUserRole = roles.FirstOrDefault() ?? "N/A";
            }

            // 3. Tạo và populate ViewModel
            var viewModel = new CategoryDetailsWithLessonsViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Price = category.Price,
                IsDeleted = category.IsDeleted,
                DeletedAt = category.DeletedAt,

                CreatedByUserId = category.CreatedByUserId,
                CreatedDate = category.CreatedDate,

                CreatedByUserFullName = category.CreatedByUser?.FullName ?? category.CreatedByUser?.UserName ?? "N/A",
                CreatedByUserRole = createdByUserRole,

                UpdatedByUserId = category.UpdatedByUserId,
                UpdatedDate = category.UpdatedDate,
                UpdatedByUserFullName = category.UpdatedByUser?.FullName ?? category.UpdatedByUser?.UserName ?? "N/A",
                UpdatedByUserRole = updatedByUserRole,
                Lessons = new List<LessonViewModel>() 
            };

            ViewBag.InitialShowDeletedLessons = category.IsDeleted;

            return View(viewModel);
        }

        public ActionResult Create()
        {
            return View();
        }

        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Nhận ViewModel làm tham số
        public async Task<ActionResult> Create(CategoryCreateViewModel viewModel) 
        {

            if (ModelState.IsValid)
            {
                var category = new Category();
                category.Name = viewModel.Name;
                category.Description = viewModel.Description;
                category.Price = viewModel.Price;
                string currentUserId = User.Identity.GetUserId();

                // Lấy vai trò
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                string currentUserRole = currentUserRoles.FirstOrDefault();
                category.CreatedByUserId = currentUserId; // <-- Gán giá trị vào Entity Model
                category.CreatedByUserRole = currentUserRole ?? "Unknown";
                category.CreatedDate = DateTime.Now;
                category.UpdatedByUserId = null;
                category.UpdatedByUserRole = null;
                category.UpdatedDate = null;
                category.IsDeleted = false;
                category.DeletedAt = null;

                // --- THÊM VÀO DATABASE VÀ LƯU ---
                db.Categories.Add(category);
                await db.SaveChangesAsync(); 

                await CreateNewCourseNotification(category.Id, category.Name, currentUserId);

                // Chuyển hướng về trang danh sách sau khi tạo thành công
                return RedirectToAction("Index");
            }

            var categoriesViewBagOnFail = db.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();
            ViewBag.ListCategories = categoriesViewBagOnFail;

            return View(viewModel); // <-- Trả về View với ViewModel
        }
        private async Task CreateNewCourseNotification(int categoryId, string categoryName, string senderId)
        {
            try
            {
                // Tạo thông báo
                var notification = new Notification
                {
                    Title = "Khóa học mới đã được thêm vào!",
                    Content = $"Khóa học '{categoryName}' đã sẵn sàng. Hãy khám phá ngay!",
                    CreatedDate = DateTime.Now,
                    SenderId = senderId,
                    TargetController = "Category",
                    TargetAction = "Details",
                    PrimaryRelatedEntityId = categoryId,
                    RelatedEntityType = "NewCourse",
                    IsDeleted = false
                };

                // Lấy tất cả người dùng có role là "Student" (học viên)
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var students = await userManager.Users
                    .Where(u => u.Roles.Any(r => r.RoleId == "3380b389-119c-4443-91e6-c9bb52bd8513")) 
                    .ToListAsync();

                // Tạo UserNotification cho từng học viên
                foreach (var student in students)
                {
                    notification.UserNotifications.Add(new UserNotification
                    {
                        UserId = student.Id,
                        IsRead = false,
                        IsDeleted = false
                    });
                }

                db.Notifications.Add(notification);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tạo thông báo: {ex.Message}");
            }
        }

        public async Task<ActionResult> Edit(int? id)
        {
            // Kiểm tra ID có được cung cấp không
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = await db.Categories.Where(c => !c.IsDeleted).SingleOrDefaultAsync(c => c.Id == id);


            // Kiểm tra có tìm thấy danh mục không
            if (category == null)
            {
                return HttpNotFound();
            }

            // Truyền đối tượng danh mục sang View Edit
            return View(category);
        }


        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Có thể cần nếu Description hoặc Content có chứa HTML
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,Price")] Category category) // Bind các thuộc tính được phép sửa
        {

            if (ModelState.IsValid)
            {
                // Lấy bản ghi danh mục GỐC từ cơ sở dữ liệu
                var existingCategory = await db.Categories.FindAsync(category.Id);

                // Kiểm tra có tìm thấy bản ghi gốc không
                if (existingCategory == null)
                {
                    return HttpNotFound();
                }


                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.Price = category.Price;

                string currentUserId = User.Identity.GetUserId();
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                string currentUserRole = currentUserRoles.FirstOrDefault();

                existingCategory.UpdatedByUserId = currentUserId;
                existingCategory.UpdatedByUserRole = currentUserRole ?? "Unknown";
                existingCategory.UpdatedDate = DateTime.Now;
                db.Entry(existingCategory).State = EntityState.Modified;

                // Lưu thay đổi vào cơ sở dữ liệu bất đồng bộ
                await db.SaveChangesAsync();

                if (existingCategory.IsDeleted)
                {
                    return RedirectToAction("DeletedIndex"); // Chuyển về danh sách đã xóa nếu bản ghi đang bị xóa mềm
                }
                else
                {
                    return RedirectToAction("Index"); // Chuyển về danh sách đang hoạt động
                }
            }

            return View(category);
        }
        [HttpPost, ActionName("SoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SoftDeleteConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id);

            if (category == null)
            {
                return HttpNotFound(); 
            }

            if (category.IsDeleted)
            {

                return RedirectToAction("Index");
            }

            category.IsDeleted = true;
            category.DeletedAt = DateTime.Now;


            string currentUserId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            string currentUserRole = (await userManager.GetRolesAsync(currentUserId)).FirstOrDefault();

            category.UpdatedByUserId = currentUserId;

            category.UpdatedByUserRole = currentUserRole ?? "Unknown";
            category.UpdatedDate = DateTime.Now; 

            db.Entry(category).State = EntityState.Modified;

            await db.SaveChangesAsync(); 

            return RedirectToAction("Index"); 
        }
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id);

            if (category == null)
            {
                return HttpNotFound(); 
            }

            if (!category.IsDeleted)
            {
                return RedirectToAction("Index");
            }

            category.IsDeleted = false; 
            category.DeletedAt = null;  

            string currentUserId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            string currentUserRole = (await userManager.GetRolesAsync(currentUserId)).FirstOrDefault();

            category.UpdatedByUserId = currentUserId;
            category.UpdatedByUserRole = currentUserRole ?? "Unknown";
            category.UpdatedDate = DateTime.Now; 

            // Đánh dấu entity là Modified
            db.Entry(category).State = EntityState.Modified;

            await db.SaveChangesAsync(); // Lưu thay đổi

            return RedirectToAction("DeletedIndex");
        }

        [HttpPost, ActionName("HardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HardDeleteConfirmed(int id)
        {

            var category = await db.Categories.FindAsync(id);

            if (category == null)
            {
                return HttpNotFound(); 
            }

            if (!category.IsDeleted)
            {

                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);

            await db.SaveChangesAsync(); 

            return RedirectToAction("DeletedIndex"); 
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}