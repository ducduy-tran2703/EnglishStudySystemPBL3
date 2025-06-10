using System;
using System.Data.Entity; 
using System.Linq; 
using System.Net; 
using System.Threading.Tasks; 
using System.Web.Mvc; 
using EnglishStudySystem.Models; 
using EnglishStudySystem.Areas.Admin.ViewModel; 
using Microsoft.AspNet.Identity; 
using Microsoft.AspNet.Identity.Owin; 
using System.Collections.Generic; 
using System.Data.Entity.Validation; 
using System.Diagnostics;
using System.Web;
using EnglishStudySystem;
using Microsoft.AspNet.Identity.EntityFramework; 

namespace EnglishStudySystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator, Editor")] 
    public class LessonsController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> _userManager;

        public LessonsController()
        {
            db = new ApplicationDbContext(); 
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)); 
        }

        public ActionResult Create(int? categoryId)
        {
            if (categoryId == null) return RedirectToAction("Index", "Categories");
            var category = db.Categories.Find(categoryId.Value);
            if (category == null) return RedirectToAction("Index", "Categories");

            var viewModel = new LessonCreateViewModel { CategoryId = categoryId.Value };
            // Không cần gán IsFree ở đây, vì nó đã có giá trị mặc định là false trong ViewModel
            ViewBag.CategoryName = category.Name;
            return View(viewModel);
        }

        // POST: Admin/Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LessonCreateViewModel viewModel)
        {
            var category = await db.Categories.FindAsync(viewModel.CategoryId);
            if (category == null) ModelState.AddModelError("CategoryId", "Danh mục được chọn không tồn tại.");

            if (ModelState.IsValid)
            {
                var lesson = new Lesson
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Video_URL = viewModel.Video_URL,
                    CategoryId = viewModel.CategoryId,
                    // THÊM THUỘC TÍNH ISFREE VÀO ĐÂY
                    IsFreeTrial = viewModel.IsFree // Gán giá trị từ ViewModel vào Model Entity
                };

                string currentUserId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(currentUserId)) { ModelState.AddModelError("", "Người dùng chưa xác thực."); }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        var currentUserRoles = await userManager.GetRolesAsync(currentUserId);

                        lesson.CreatedByUserId = currentUserId;
                        lesson.CreatedByUserRole = currentUserRoles.FirstOrDefault() ?? "Unknown";
                        lesson.CreatedDate = DateTime.Now;
                        lesson.UpdatedByUserId = null;
                        lesson.UpdatedByUserRole = null;
                        lesson.UpdatedDate = null;
                        lesson.IsDeleted = false;
                        lesson.DeletedAt = null;

                        db.Lessons.Add(lesson);
                        await db.SaveChangesAsync();
                        await CreateNewLessonNotification(lesson.Id, lesson.Title, lesson.CategoryId, currentUserId);

                        return RedirectToAction("Details", "Categories", new { id = lesson.CategoryId });
                    }
                    catch (DbEntityValidationException ex)
                    {
                        Debug.WriteLine("DbEntityValidationException occurred during Create:");
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                                ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                            }
                        }
                        // Nếu lỗi, cần chuẩn bị lại dữ liệu cho View (CategoryName)
                        ViewBag.CategoryName = category?.Name;
                        return View(viewModel);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi lưu bài học: " + ex.Message);
                        ViewBag.CategoryName = category?.Name;
                        return View(viewModel);
                    }
                }
            }

            // Nếu ModelState ban đầu không hợp lệ
            ViewBag.CategoryName = category?.Name;
            return View(viewModel);
        }
        private async Task CreateNewLessonNotification(int lessonId, string lessonTitle, int categoryId, string senderId)
        {
            try
            {
                // Tạo thông báo
                var notification = new Notification
                {
                    Title = "Bài học mới đã được thêm vào!",
                    Content = $"Bài học '{lessonTitle}' đã sẵn sàng. Hãy bắt đầu học ngay!",
                    CreatedDate = DateTime.Now,
                    SenderId = senderId,
                    TargetController = "Lesson",
                    TargetAction = "Details",
                    PrimaryRelatedEntityId = lessonId,
                    RelatedEntityType = "NewLesson",
                    IsDeleted = false
                };

                // Lấy danh sách người dùng đã mua khóa học này
                var paidUsers = await db.Payments
                    .Where(p => p.CategoryId == categoryId &&
                                 p.Status == "Completed" &&
                                 p.PaymentDate <= DateTime.Now)
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToListAsync();

                // Tạo UserNotification cho từng người dùng đã mua
                foreach (var userId in paidUsers)
                {
                    notification.UserNotifications.Add(new UserNotification
                    {
                        UserId = userId,
                        IsRead = false,
                        IsDeleted = false,
                    });
                }

                // Chỉ lưu thông báo nếu có người nhận
                if (notification.UserNotifications.Any())
                {
                    db.Notifications.Add(notification);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tạo thông báo bài học mới: {ex.Message}");
            }
        }

        // GET: Admin/Lessons/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lesson = await db.Lessons
                                 .Include(l => l.Category)
                                 .Include(l => l.Tests) // Tải tất cả Tests liên quan
                                 .Include(l => l.CreatedByUser) // Tải người tạo
                                 .Include(l => l.UpdatedByUser) // Tải người cập nhật (người xóa)
                                 .SingleOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return HttpNotFound();
            }

            string createdByUserRole = "N/A";
            if (!string.IsNullOrEmpty(lesson.CreatedByUserId))
            {
                var roles = await _userManager.GetRolesAsync(lesson.CreatedByUserId);
                createdByUserRole = roles.FirstOrDefault() ?? "N/A";
            }

            string updatedByUserRole = "N/A";
            if (!string.IsNullOrEmpty(lesson.UpdatedByUserId))
            {
                var roles = await _userManager.GetRolesAsync(lesson.UpdatedByUserId);
                updatedByUserRole = roles.FirstOrDefault() ?? "N/A";
            }

            // --- Ánh xạ Entity Lesson sang LessonDetailsViewModel ---
            var viewModel = new LessonDetailsViewModel
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Video_URL = lesson.Video_URL,
                IsFree = lesson.IsFreeTrial,
                CategoryId = lesson.CategoryId,
                CategoryName = lesson.Category?.Name, 

                // Thông tin Người tạo
                CreatedByUserId = lesson.CreatedByUserId,
                CreatedByUserFullName = lesson.CreatedByUser?.FullName ?? lesson.CreatedByUser?.UserName ?? "N/A", 
                CreatedByUserRole = createdByUserRole, // Gán vai trò đã lấy
                CreatedDate = lesson.CreatedDate,

                // Thông tin Người cập nhật (cũng là người xóa)
                UpdatedByUserId = lesson.UpdatedByUserId,
                UpdatedByUserFullName = lesson.UpdatedByUser?.FullName ?? lesson.UpdatedByUser?.UserName ?? "N/A", 
                UpdatedByUserRole = updatedByUserRole, // Gán vai trò đã lấy
                UpdatedDate = lesson.UpdatedDate,

                // Thông tin Xóa mềm
                IsDeleted = lesson.IsDeleted,
                DeletedAt = lesson.DeletedAt,
                
                Tests = lesson.Tests.ToList() // Ánh xạ trực tiếp danh sách Tests đã được Include
            };

            // Truyền LessonDetailsViewModel sang View
            return View(viewModel);
        }

        // GET: Admin/Lessons/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lesson = await db.Lessons
                                 .Include(l => l.Category) 
                                 .Include(l => l.Tests)    
                                 .Include(l => l.CreatedByUser)
                                 .Include(l => l.UpdatedByUser)
                                 .SingleOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return HttpNotFound();
            }

            string createdByUserRole = "N/A";
            if (!string.IsNullOrEmpty(lesson.CreatedByUserId))
            {
                var roles = await _userManager.GetRolesAsync(lesson.CreatedByUserId);
                createdByUserRole = roles.FirstOrDefault() ?? "N/A";
            }

            string updatedByUserRole = "N/A";
            if (!string.IsNullOrEmpty(lesson.UpdatedByUserId))
            {
                var roles = await _userManager.GetRolesAsync(lesson.UpdatedByUserId);
                updatedByUserRole = roles.FirstOrDefault() ?? "N/A";
            }



            // Ánh xạ từ Lesson entity sang LessonEditViewModel
            var viewModel = new LessonEditViewModel
            {
                Id = lesson.Id,
                CategoryId = lesson.CategoryId, // Vẫn giữ CategoryId trong ViewModel để bind với form
                Title = lesson.Title,
                Description = lesson.Description,
                Video_URL = lesson.Video_URL,
                IsFreeTrial = lesson.IsFreeTrial,

                CategoryName = lesson.Category?.Name, // Vẫn gán CategoryName để hiển thị

                CreatedByUserId = lesson.CreatedByUserId,
                CreatedByUserFullName = lesson.CreatedByUser?.FullName ?? lesson.CreatedByUser?.UserName ?? "N/A",
                CreatedByUserRole = createdByUserRole,
                CreatedDate = lesson.CreatedDate,

                UpdatedByUserId = lesson.UpdatedByUserId,
                UpdatedByUserFullName = lesson.UpdatedByUser?.FullName ?? lesson.UpdatedByUser?.UserName ?? "N/A",
                UpdatedByUserRole = updatedByUserRole,
                UpdatedDate = lesson.UpdatedDate,

                Tests = lesson.Tests.ToList()
            };

            return View(viewModel);
        }


        // POST: Admin/Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LessonEditViewModel viewModel)
        {
            var category = await db.Categories.FindAsync(viewModel.CategoryId);
            if (category == null) ModelState.AddModelError("CategoryId", "Danh mục được chọn không tồn tại.");

            if (ModelState.IsValid)
            {
                try
                {
                    var originalLesson = await db.Lessons.FindAsync(viewModel.Id);
                    if (originalLesson == null) return HttpNotFound();

                    // Cập nhật các thuộc tính từ ViewModel vào đối tượng gốc
                    originalLesson.Title = viewModel.Title;
                    originalLesson.Description = viewModel.Description;
                    originalLesson.Video_URL = viewModel.Video_URL;
                    originalLesson.CategoryId = viewModel.CategoryId;
                    originalLesson.IsFreeTrial = viewModel.IsFreeTrial; // Cập nhật thuộc tính IsFreeTrial

                    string currentUserId = User.Identity.GetUserId();
                    if (string.IsNullOrEmpty(currentUserId)) { ModelState.AddModelError("", "Người dùng chưa xác thực."); }

                    if (ModelState.IsValid) // Kiểm tra lại sau khi thêm lỗi tùy chỉnh
                    {
                        var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        var currentUserRoles = await userManager.GetRolesAsync(currentUserId);

                        originalLesson.UpdatedByUserId = currentUserId;
                        originalLesson.UpdatedByUserRole = currentUserRoles.FirstOrDefault() ?? "Unknown";
                        originalLesson.UpdatedDate = DateTime.Now;

                        await db.SaveChangesAsync();

                        return RedirectToAction("Details", "Categories", new { id = originalLesson.CategoryId });
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    Debug.WriteLine("DbEntityValidationException occurred during Edit:");
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                    var lessonWithTestsOnFail = await db.Lessons.Include(l => l.Tests.Where(t => !t.IsDeleted)).SingleOrDefaultAsync(l => l.Id == viewModel.Id);
                    viewModel.Tests = lessonWithTestsOnFail?.Tests.ToList() ?? new List<Test>();
                    ViewBag.CategoryName = category?.Name;
                    // Audit fields đã có trong ViewModel
                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi lưu bài học: " + ex.Message);
                    // Nạp lại Tests và CategoryName nếu lỗi
                    var lessonWithTestsOnFail = await db.Lessons.Include(l => l.Tests.Where(t => !t.IsDeleted)).SingleOrDefaultAsync(l => l.Id == viewModel.Id);
                    viewModel.Tests = lessonWithTestsOnFail?.Tests.ToList() ?? new List<Test>();
                    ViewBag.CategoryName = category?.Name;
                    return View(viewModel);
                }
            }

            // Nếu ModelState ban đầu không hợp lệ
            var lessonWithTestsOnFailInitial = await db.Lessons.Include(l => l.Tests.Where(t => !t.IsDeleted)).SingleOrDefaultAsync(l => l.Id == viewModel.Id);
            viewModel.Tests = lessonWithTestsOnFailInitial?.Tests.ToList() ?? new List<Test>();
            ViewBag.CategoryName = category?.Name;
            return View(viewModel);
        }


        // --- ACTION: XÓA MỀM BÀI HỌC (SoftDelete) ---
        [HttpPost, ActionName("SoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SoftDeleteConfirmed(int id)
        {
            var lesson = await db.Lessons.FindAsync(id);

            if (lesson == null)
            {
                // Thay vì HttpNotFound(), trả về JSON lỗi
                return Json(new { success = false, message = "Bài học không tồn tại." });
            }
            if (lesson.IsDeleted)
            {
                // Nếu đã xóa mềm, trả về JSON thành công nhưng có thông báo đặc biệt
                return Json(new { success = true, message = "Bài học đã được xóa mềm trước đó.", lessonId = id });
            }

            lesson.IsDeleted = true;
            lesson.DeletedAt = DateTime.Now;

            string currentUserId = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                lesson.UpdatedByUserId = currentUserId;
                lesson.UpdatedByUserRole = currentUserRoles.FirstOrDefault() ?? "Unknown";
                lesson.UpdatedDate = DateTime.Now;
            }
            else
            {
                // Xử lý trường hợp không tìm thấy người dùng (ví dụ: phiên hết hạn)
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại." });
            }

            db.Entry(lesson).State = EntityState.Modified;
            await db.SaveChangesAsync();

            // TRẢ VỀ JSON KHI THÀNH CÔNG
            return Json(new { success = true, message = "Xóa mềm bài học thành công.", lessonId = id });
        }

        // --- ACTION: KHÔI PHỤC BÀI HỌC (Restore) ---
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed(int id)
        {
            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return Json(new { success = false, message = "Bài học không tồn tại." });
            }
            if (!lesson.IsDeleted)
            {
                // Nếu chưa xóa mềm, trả về JSON thành công nhưng có thông báo đặc biệt
                return Json(new { success = true, message = "Bài học không đang bị xóa mềm.", lessonId = id });
            }

            lesson.IsDeleted = false;
            lesson.DeletedAt = null;

            string currentUserId = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                lesson.UpdatedByUserId = currentUserId;
                lesson.UpdatedByUserRole = currentUserRoles.FirstOrDefault() ?? "Unknown";
                lesson.UpdatedDate = DateTime.Now;
            }
            else
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại." });
            }

            db.Entry(lesson).State = EntityState.Modified;
            await db.SaveChangesAsync();

            // TRẢ VỀ JSON KHI THÀNH CÔNG
            return Json(new { success = true, message = "Khôi phục bài học thành công.", lessonId = id });
        }

        // --- ACTION: XÓA HOÀN TOÀN BÀI HỌC (HardDelete) ---
        [HttpPost, ActionName("HardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HardDeleteConfirmed(int id)
        {
            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return Json(new { success = false, message = "Bài học không tồn tại." });
            }

            if (!lesson.IsDeleted)
            {
                return Json(new { success = false, message = "Chỉ có thể xóa cứng bài học đã bị xóa mềm." });
            }

            // var categoryId = lesson.CategoryId; // Không cần thiết nếu không redirect

            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync();

            // TRẢ VỀ JSON KHI THÀNH CÔNG
            return Json(new { success = true, message = "Xóa cứng bài học thành công.", lessonId = id });
        }


        // --- Xử lý Dispose DbContext ---
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // }
    }
}