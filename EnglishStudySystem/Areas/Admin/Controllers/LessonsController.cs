// EnglishStudySystem/Areas/Admin/Controllers/LessonsController.cs

using System;
using System.Data.Entity; // Cần cho DbContext, EntityState, Include, SingleOrDefaultAsync
using System.Linq; // Cần cho LINQ, Where, Select, ToList
using System.Net; // Cần cho HttpStatusCodeResult
using System.Threading.Tasks; // Cần cho async/await
using System.Web.Mvc; // Cần cho Controller, ActionResult, HttpNotFound, v.v.
using EnglishStudySystem.Models; // Cần cho ApplicationDbContext, Lesson, Category, Test, etc.
using EnglishStudySystem.Areas.Admin.ViewModel; // Cần cho các ViewModels mới <-- ĐẢM BẢO USING NÀY
using Microsoft.AspNet.Identity; // Cần cho User.Identity.GetUserId
using Microsoft.AspNet.Identity.Owin; // Cần cho UserManager
using System.Collections.Generic; // Cần cho List
using System.Data.Entity.Validation; // Cần cho DbEntityValidationException
using System.Diagnostics;
using System.Web;
using EnglishStudySystem; // Cần cho Debug.WriteLine

namespace EnglishStudySystem.Areas.Admin.Controllers
{
    // Chỉ cho phép Admin và Editor truy cập Controller này
    [Authorize(Roles = "Administrator, Editor")] // Đảm bảo Role là "Administrator" nếu đó là tên role Admin
    public class LessonsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // --- Actions cho Quản lý Bài học ---

        // GET: Admin/Lessons/Create?categoryId=5
        public ActionResult Create(int? categoryId)
        {
            if (categoryId == null) return RedirectToAction("Index", "Categories");
            var category = db.Categories.Find(categoryId.Value);
            if (category == null) return RedirectToAction("Index", "Categories");

            var viewModel = new LessonCreateViewModel { CategoryId = categoryId.Value };
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
                    CategoryId = viewModel.CategoryId
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

        // GET: Admin/Lessons/Details/5
        // Lấy Entity Lesson và truyền trực tiếp sang View
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy Bài học theo ID và NẠP (INCLUDE) các Entity liên quan bằng cú pháp EF6 chuẩn
            // - Category để hiển thị tên danh mục
            // - Tests để hiển thị danh sách bài kiểm tra (sẽ lọc ở View)
            var lesson = await db.Lessons
                                 .Include(l => l.Category) // <-- Include chuẩn EF6
                                 .Include(l => l.Tests) // <-- Include chuẩn EF6 (sẽ nạp TẤT CẢ Tests)
                                 .SingleOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return HttpNotFound();
            }

            // Truyền Entity Lesson trực tiếp sang View
            return View(lesson);
        }

        // GET: Admin/Lessons/Edit/5
        // Trả về View với LessonEditViewModel
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // --- SỬ DỤNG PHÉP CHIẾU (.Select()) ĐỂ NẠP BÀI HỌC VÀ BÀI KIỂM TRA ĐÃ LỌC ---
            // Truy vấn và chiếu (project) dữ liệu trực tiếp vào ViewModel.
            var viewModel = await db.Lessons
                .Where(l => l.Id == id) // Lọc bài học theo ID
                .Select(l => new LessonEditViewModel // Chiếu dữ liệu sang ViewModel
                {
                    // Ánh xạ các thuộc tính của Bài học
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    Video_URL = l.Video_URL, // <-- Ánh xạ Video_URL
                    CategoryId = l.CategoryId,

                    // Chiếu tên danh mục (cần include Category ngầm định)
                    CategoryName = l.Category.Name, // <-- Lấy tên danh mục

                    // Chiếu và lọc danh sách Bài kiểm tra
                    // .ToList() là cần thiết để thực thi truy vấn cho Tests và đưa kết quả vào bộ nhớ
                    Tests = l.Tests.Where(t => !t.IsDeleted).ToList(), // <-- Lọc bài kiểm tra chưa xóa mềm

                    // Ánh xạ Audit Fields từ Entity sang ViewModel để hiển thị
                    CreatedByUserId = l.CreatedByUserId,
                    CreatedByUserRole = l.CreatedByUserRole,
                    CreatedDate = l.CreatedDate,
                    UpdatedByUserId = l.UpdatedByUserId,
                    UpdatedByUserRole = l.UpdatedByUserRole,
                    UpdatedDate = l.UpdatedDate
                })
                .SingleOrDefaultAsync(); // Thực thi truy vấn và lấy một kết quả hoặc null

            // --- KẾT THÚC PHÉP CHIẾU ---


            if (viewModel == null)
            {
                return HttpNotFound(); // Trả về lỗi 404 nếu không tìm thấy bài học
            }

            // Truyền ViewModel sang View Edit
            return View(viewModel); // View sẽ sử dụng LessonEditViewModel
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
                    // Nếu lỗi, cần chuẩn bị lại dữ liệu cho View (CategoryName, Tests, Audit)
                    // Nạp lại Tests và CategoryName cho ViewModel
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
            // Nạp lại Tests và CategoryName cho ViewModel
            var lessonWithTestsOnFailInitial = await db.Lessons.Include(l => l.Tests.Where(t => !t.IsDeleted)).SingleOrDefaultAsync(l => l.Id == viewModel.Id);
            viewModel.Tests = lessonWithTestsOnFailInitial?.Tests.ToList() ?? new List<Test>();
            ViewBag.CategoryName = category?.Name;
            return View(viewModel);
        }


        // --- Actions SoftDelete, Restore, HardDelete (Giữ nguyên, làm việc với ID/Entity) ---
        // Sử dụng Entity Model, không dùng ViewModel form.
        // POST: Admin/Lessons/SoftDelete/5
        [HttpPost, ActionName("SoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SoftDeleteConfirmed(int id)
        {
            var lesson = await db.Lessons.FindAsync(id);
            if (lesson == null || lesson.IsDeleted) return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Categories"));

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

            db.Entry(lesson).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Categories"));
        }

        // POST: Admin/Lessons/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed(int id)
        {
            // Sử dụng Find(id) hoặc SingleOrDefaultAsync để truy vấn Entity
            // Cần đảm bảo phương thức này có thể lấy Entity kể cả khi IsDeleted = true
            // Nếu Find không hoạt động do Global Filter, hãy thử SqlQuery như đã thảo luận
            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == id); // <-- Thử dùng SingleOrDefaultAsync

            // Redirect nếu không tìm thấy hoặc chưa xóa mềm
            if (lesson == null || !lesson.IsDeleted) return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Categories"));


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

            db.Entry(lesson).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Categories"));
        }

        // POST: Admin/Lessons/HardDelete/5
        [HttpPost, ActionName("HardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HardDeleteConfirmed(int id)
        {
            // Sử dụng Find(id) hoặc SingleOrDefaultAsync để truy vấn Entity
            // Cần đảm bảo phương thức này có thể lấy Entity kể cả khi IsDeleted = true
            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == id); // <-- Thử dùng SingleOrDefaultAsync

            if (lesson == null) return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Categories"));

            var categoryId = lesson.CategoryId; // Lưu lại CategoryId trước khi xóa

            db.Lessons.Remove(lesson); // Xóa entity
            await db.SaveChangesAsync(); // Lưu thay đổi

            // Chuyển hướng về trang Chi tiết Danh mục
            return RedirectToAction("Details", "Categories", new { id = categoryId });
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

        // --- Phương thức để tìm entity kể cả đã xóa mềm (nếu cần) ---
        // Nếu SingleOrDefaultAsync(l => l.Id == id) hoạt động đúng với Global Filter của bạn,
        // bạn không cần phương thức FindDatabaseOnly này nữa.
        // Nếu Global Filter chặn, bạn cần triển khai logic bỏ qua filter ở đây.
        // Ví dụ:
        // private Lesson FindDatabaseOnly(int id)
        // {
        //     // Triển khai logic truy vấn bỏ qua filter
        //     throw new NotImplementedException("Implement logic to find entity bypassing global filters if needed.");
        // }
    }
}