// EnglishStudySystem/Areas/Admin/Controllers/CategoriesController.cs

using System;
using System.Collections.Generic;
using System.Data.Entity; // Cần cho Include, EntityState, ToListAsync
using System.Linq; // Cần cho Where, OrderBy
using System.Net; // Cần cho HttpStatusCodeResult
using System.Threading.Tasks; // Cần cho async/await
using System.Web;
using System.Web.Mvc; // Cần cho Controller, ActionResult, View, HttpNotFound
using EnglishStudySystem;
using EnglishStudySystem.Areas.Admin.ViewModel;
using EnglishStudySystem.Models; // Cần cho ApplicationDbContext, Category
using Microsoft.AspNet.Identity; // Cần để lấy User ID và UserName của người tạo/cập nhật
using Microsoft.AspNet.Identity.EntityFramework;


// Cần thêm using để sử dụng các thuộc tính phân quyền Identity
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

// Correcting the syntax error in the namespace declaration
namespace EnglishStudySystem.Areas.Admin.Controllers
{

    // Đặt Authorize cho Controller để chỉ Admin hoặc Editor mới có thể truy cập
    [Authorize(Roles = "Administrator, Editor")]
    public class CategoriesController : Controller
    {
        // Khai báo DbContext để truy vấn dữ liệu
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> _userManager;
        public CategoriesController()
        {
            db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        // --- ACTION: DANH SÁCH CÁC DANH MỤC CHƯA XÓA MỀM (Index) ---
        // GET: Admin/Categories
        public async Task<ActionResult> Index()
        {
            // Lấy tất cả các danh mục CHƯA bị xóa mềm
            // Do không có Global Query Filter, chúng ta phải lọc thủ công bằng .Where(c => !c.IsDeleted)
            var activeCategories = await db.Categories
                                           .Where(c => !c.IsDeleted)
                                           .OrderBy(c => c.Name) // Sắp xếp theo tên danh mục
                                           .ToListAsync(); // Thực hiện truy vấn bất đồng bộ

            // --- BẮT ĐẦU PHẦN THÊM VÀO ĐỂ LẤY TÊN ĐẦY ĐỦ CỦA NGƯỜI TẠO ---

            // Lấy danh sách tất cả các CreatedByUserId duy nhất từ các danh mục
            var createdByUserIds = activeCategories.Select(c => c.CreatedByUserId)
                                                   .Where(id => id != null) // Chỉ lấy các ID không null
                                                   .Distinct() // Lọc các ID trùng lặp
                                                   .ToList();

            // Khởi tạo Dictionary để lưu trữ FullName của người dùng
            var userNames = new Dictionary<string, string>();

            // Nếu có User ID, truy vấn FullName của họ từ bảng Users
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

            // Truyền Dictionary này sang View thông qua ViewBag
            ViewBag.UserNames = userNames;

            // --- KẾT THÚC PHẦN THÊM VÀO ---

            // Truyền danh sách các danh mục đang hoạt động sang View
            return View(activeCategories);
        }

        // --- ACTION: DANH SÁCH CÁC DANH MỤC ĐÃ XÓA MỀM (DeletedIndex) ---
        // GET: Admin/Categories/Deleted
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
                // **THAY ĐỔI TỪ UserName SANG FullName TẠI ĐÂY**
                var users = await db.Users
                                    .Where(u => userIds.Contains(u.Id))
                                    .ToListAsync();

                foreach (var user in users)
                {
                    // Ưu tiên FullName, nếu FullName null/empty thì dùng UserName
                    userNames[user.Id] = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.UserName;
                }
            }

            ViewBag.UserNames = userNames;

            return View(deletedCategories);
        }

        // GET: Admin/Categories/GetLessonsPartial (hoặc Admin/Lessons/GetLessonsPartial)
        public ActionResult GetLessonsPartial(int categoryId, bool showDeleted = false)
        {
            // Truy vấn danh sách bài học
            // Eager load CreatedByUser và UpdatedByUser
            var lessons = db.Lessons
                                  .Include(l => l.CreatedByUser)
                                  .Include(l => l.UpdatedByUser) // Rất quan trọng để lấy thông tin người cập nhật
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

                CreatedDate = l.CreatedDate,
                CreatedByUserFullName = l.CreatedByUser?.FullName, // Lấy FullName từ navigation property

                UpdatedDate = l.UpdatedDate,
                UpdatedByUserFullName = l.UpdatedByUser?.FullName, // Lấy FullName từ navigation property
                                                                   // (Người này cũng sẽ là người xóa nếu IsDeleted = true)

                DeletedAt = l.DeletedAt
                // Không cần ánh xạ DeletedByUserFullName nữa
            }).OrderBy(l => l.CreatedDate).ToList();

            // Truyền CategoryId và ShowDeletedLessons vào Partial View thông qua ViewData
            ViewData["CategoryId"] = categoryId;
            ViewData["ShowDeletedLessons"] = showDeleted;

            return PartialView("_LessonsListPartial", lessonViewModels);
        }

        // --- ACTION: XEM CHI TIẾT DANH MỤC (Details) ---
        // GET: Admin/Categories/Details/5
        public async Task<ActionResult> Details(int? id) // Bỏ showDeleted ở đây vì nó được xử lý trong GetLessonsPartial
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 1. Lấy Category chính và Eager Load CreatedByUser và UpdatedByUser
            // Điều này sẽ tải FullName và các thông tin khác của người dùng trực tiếp
            var category = await db.Categories
                                   .Include(c => c.CreatedByUser)
                                   .Include(c => c.UpdatedByUser)
                                   .SingleOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            // 2. Lấy Roles cho người tạo và người cập nhật của Category
            // Chúng ta cần UserManager để lấy roles vì chúng không phải là navigation property trực tiếp từ Category
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
                // Lấy FullName từ navigation property
                CreatedByUserFullName = category.CreatedByUser?.FullName ?? category.CreatedByUser?.UserName ?? "N/A",
                // Gán Role đã lấy từ UserManager
                CreatedByUserRole = createdByUserRole,

                UpdatedByUserId = category.UpdatedByUserId,
                UpdatedDate = category.UpdatedDate,
                // Lấy FullName từ navigation property
                UpdatedByUserFullName = category.UpdatedByUser?.FullName ?? category.UpdatedByUser?.UserName ?? "N/A",
                // Gán Role đã lấy từ UserManager
                UpdatedByUserRole = updatedByUserRole,

                // Không cần tải Lessons ở đây nữa, vì chúng ta đã dùng AJAX trong GetLessonsPartial.
                // Để đảm bảo ViewModel không bị null khi truy cập Lessons, bạn có thể khởi tạo một List rỗng.
                Lessons = new List<LessonViewModel>() // Khởi tạo rỗng vì sẽ tải qua AJAX
                                                      // ShowDeletedLessons sẽ được quyết định trong JavaScript của View chính (CategoryDetails.cshtml)
                                                      // hoặc dựa vào trạng thái IsDeleted của Category ban đầu
                                                      // showDeleted = category.IsDeleted; // Điều này sẽ được xử lý trong JavaScript của View
            };

            // Gán biến initialShowDeletedLessons cho View để JS có thể đọc
            // Đây là cách tốt nhất để truyền trạng thái ban đầu cho JS
            ViewBag.InitialShowDeletedLessons = category.IsDeleted;

            return View(viewModel);
        }


        // --- ACTION: THÊM MỚI DANH MỤC (Create) ---
        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            // Action này chỉ đơn giản hiển thị form tạo mới
            return View();
        }

        // POST: Admin/Categories/Create
        // Để bảo vệ khỏi tấn công overposting, hãy bật các thuộc tính cụ thể mà bạn muốn bind. For
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Nhận ViewModel làm tham số
        public async Task<ActionResult> Create(CategoryCreateViewModel viewModel) // <-- Nhận ViewModel
        {
            // Kiểm tra ModelState.IsValid. Lần này, nó chỉ kiểm tra Name, Description, Price
            // vì đây là các thuộc tính có trong ViewModel và có Data Annotations
            if (ModelState.IsValid)
            {
                // Dữ liệu trong ViewModel hợp lệ, tiến hành tạo đối tượng Entity Model
                var category = new Category();

                // Ánh xạ dữ liệu từ ViewModel sang Entity Model
                category.Name = viewModel.Name;
                category.Description = viewModel.Description;
                category.Price = viewModel.Price;
                // ... ánh xạ các thuộc tính khác từ ViewModel nếu có ...

                // --- GÁN GIÁ TRỊ CHO CÁC TRƯỜNG THEO DÕI VÀ XÓA MỀM VÀO ENTITY MODEL ---
                // Lấy ID và Vai trò của người dùng hiện tại
                string currentUserId = User.Identity.GetUserId();

                // Lấy vai trò
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                string currentUserRole = currentUserRoles.FirstOrDefault();

                // Gán giá trị vào Entity Model (LÚC NÀY category.CreatedByUserId đã được gán giá trị từ currentUserId)
                category.CreatedByUserId = currentUserId; // <-- Gán giá trị vào Entity Model
                category.CreatedByUserRole = currentUserRole ?? "Unknown";
                category.CreatedDate = DateTime.Now;
                category.UpdatedByUserId = null;
                category.UpdatedByUserRole = null;
                category.UpdatedDate = null;
                category.IsDeleted = false;
                category.DeletedAt = null;

                // Lưu ý: Nếu bạn muốn các Data Annotations [Required] trên Entity Model
                // (như CreatedByUserId) được kiểm tra, bạn cần gọi TryValidateModel(category) ở đây
                // SAU KHI đã gán giá trị cho các trường Audit.
                // Tuy nhiên, với kịch bản này, thường chỉ kiểm tra validation ở ViewModel là đủ,
                // và dựa vào constraint NOT NULL ở DB cho CreatedByUserId.
                // Nếu bạn vẫn muốn kiểm tra, hãy thêm:
                // if (!TryValidateModel(category))
                // {
                //     // Copy errors from Entity Model ModelState back to ViewModel ModelState
                //     // hoặc xử lý lỗi ở đây. Việc này hơi phức tạp.
                //     // ModelState.AddModelError("", "Lỗi validation cuối cùng sau khi ánh xạ.");
                //      // Return View(viewModel);
                // }


                // --- THÊM VÀO DATABASE VÀ LƯU ---
                db.Categories.Add(category); // Thêm Entity Model vào DbSet
                await db.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu bất đồng bộ

                await CreateNewCourseNotification(category.Id, category.Name, currentUserId);

                // Chuyển hướng về trang danh sách sau khi tạo thành công
                return RedirectToAction("Index");
            }

            // Nếu ViewModel không hợp lệ, trả về lại View với ViewModel (với các lỗi validation của ViewModel)
            // Lấy lại categories cho ViewBag nếu cần
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
                    .Where(u => u.Roles.Any(r => r.RoleId == "3380b389-119c-4443-91e6-c9bb52bd8513")) // Giả sử role Student có ID là "Student"
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



        // --- ACTION: CHỈNH SỬA DANH MỤC (Edit) ---
        // GET: Admin/Categories/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            // Kiểm tra ID có được cung cấp không
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm danh mục theo ID
            // Lưu ý: Nếu bạn chỉ muốn chỉnh sửa các danh mục CHƯA bị xóa mềm, bạn cần thêm .Where(c => !c.IsDeleted) ở đây.
            // Tuy nhiên, FindAsync sẽ tìm cả các bản ghi đã xóa mềm. Tùy thuộc vào yêu cầu bạn có thể sửa lại.
            // Nếu bạn muốn chỉ cho phép sửa khi chưa xóa:
            var category = await db.Categories.Where(c => !c.IsDeleted).SingleOrDefaultAsync(c => c.Id == id);
            // Nếu bạn muốn cho phép sửa cả khi đã xóa mềm (ít phổ biến):
            // var category = await db.Categories.FindAsync(id);


            // Kiểm tra có tìm thấy danh mục không
            if (category == null)
            {
                return HttpNotFound();
            }

            // Truyền đối tượng danh mục sang View Edit
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // Để bảo vệ khỏi tấn công overposting, hãy bật các thuộc tính cụ thể mà bạn muốn bind. For
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Có thể cần nếu Description hoặc Content có chứa HTML
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,Price")] Category category) // Bind các thuộc tính được phép sửa
        {
            // LƯU Ý: Các trường audit fields (CreatedBy, CreatedDate, IsDeleted, DeletedAt) KHÔNG được bind từ form
            // Bạn cần lấy bản ghi gốc từ DB, cập nhật các trường được bind, và sau đó cập nhật các trường audit.

            if (ModelState.IsValid)
            {
                // Lấy bản ghi danh mục GỐC từ cơ sở dữ liệu
                var existingCategory = await db.Categories.FindAsync(category.Id);

                // Kiểm tra có tìm thấy bản ghi gốc không
                if (existingCategory == null)
                {
                    return HttpNotFound();
                }

                // --- CẬP NHẬT CÁC TRƯỜNG ĐƯỢC PHÉP SỬA TỪ FORM VÀO BẢN GHI GỐC ---
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.Price = category.Price;
                // ... cập nhật các thuộc tính khác nếu có ...


                // --- CẬP NHẬT CÁC TRƯỜNG THEO DÕI KHI CHỈNH SỬA ---
                string currentUserId = User.Identity.GetUserId();
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                string currentUserRole = currentUserRoles.FirstOrDefault();

                existingCategory.UpdatedByUserId = currentUserId;
                existingCategory.UpdatedByUserRole = currentUserRole ?? "Unknown";
                existingCategory.UpdatedDate = DateTime.Now;

                // KHÔNG thay đổi IsDeleted hoặc DeletedAt ở đây. Xóa mềm được xử lý bởi Action riêng.

                // Đánh dấu trạng thái của bản ghi gốc là Modified
                db.Entry(existingCategory).State = EntityState.Modified;

                // Lưu thay đổi vào cơ sở dữ liệu bất đồng bộ
                await db.SaveChangesAsync();

                // Chuyển hướng về trang danh sách sau khi chỉnh sửa thành công
                // Quyết định chuyển về Index (danh sách đang hoạt động) hay DeletedIndex (nếu đang sửa bản ghi đã xóa)
                if (existingCategory.IsDeleted)
                {
                    return RedirectToAction("DeletedIndex"); // Chuyển về danh sách đã xóa nếu bản ghi đang bị xóa mềm
                }
                else
                {
                    return RedirectToAction("Index"); // Chuyển về danh sách đang hoạt động
                }
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form với dữ liệu đã nhập và thông báo lỗi
            // LƯU Ý: Ở đây bạn sẽ hiển thị lại model CỦA FORM, không phải bản ghi gốc.
            // Các trường audit fields sẽ không được hiển thị.
            return View(category);
        }


        // --- ACTION: XÓA MỀM DANH MỤC (SoftDelete) ---
        // POST: Admin/Categories/SoftDelete/5
        [HttpPost, ActionName("SoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SoftDeleteConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id);

            if (category == null)
            {
                return HttpNotFound(); // Không tìm thấy danh mục
            }

            // Kiểm tra xem danh mục đã bị xóa mềm chưa để tránh xóa mềm lại
            if (category.IsDeleted)
            {
                // Có thể chuyển hướng hoặc trả về lỗi nếu danh mục đã xóa mềm
                return RedirectToAction("Index");
            }

            // --- THỰC HIỆN XÓA MỀM TRỰC TIẾP TRONG CONTROLLER ---
            category.IsDeleted = true;
            category.DeletedAt = DateTime.Now;

            // Lấy thông tin người dùng hiện tại để điền vào audit fields
            string currentUserId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            // Bạn có thể cần lấy FullName/UserName từ currentUser nếu muốn lưu vào UpdatedByUserFullName
            // var currentUser = await userManager.FindByIdAsync(currentUserId); 

            string currentUserRole = (await userManager.GetRolesAsync(currentUserId)).FirstOrDefault();

            category.UpdatedByUserId = currentUserId;
            // category.UpdatedByUserFullName = currentUser?.FullName ?? currentUser?.UserName; // Nếu bạn có trường này
            category.UpdatedByUserRole = currentUserRole ?? "Unknown";
            category.UpdatedDate = DateTime.Now; // Ghi nhận thời gian xóa mềm là thời gian cập nhật cuối

            // Đánh dấu entity là Modified để Entity Framework biết cần tạo lệnh UPDATE
            db.Entry(category).State = EntityState.Modified;

            await db.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToAction("Index"); // Chuyển hướng về trang danh sách chính
        }


        // --- ACTION: KHÔI PHỤC DANH MỤC (Restore) ---
        // POST: Admin/Categories/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id);

            if (category == null)
            {
                return HttpNotFound(); // Không tìm thấy danh mục
            }

            // Chỉ khôi phục nếu danh mục ĐANG bị xóa mềm
            if (!category.IsDeleted)
            {
                // Nếu không bị xóa mềm, có thể chuyển hướng hoặc hiển thị lỗi
                return RedirectToAction("Index");
            }

            // --- THỰC HIỆN KHÔI PHỤC TRỰC TIẾP TRONG CONTROLLER ---
            category.IsDeleted = false; // Đặt lại trạng thái xóa mềm
            category.DeletedAt = null;  // Xóa thông tin thời gian xóa mềm

            // Cập nhật thông tin người khôi phục vào audit fields
            string currentUserId = User.Identity.GetUserId();
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            // var currentUser = await userManager.FindByIdAsync(currentUserId); 
            string currentUserRole = (await userManager.GetRolesAsync(currentUserId)).FirstOrDefault();

            category.UpdatedByUserId = currentUserId;
            // category.UpdatedByUserFullName = currentUser?.FullName ?? currentUser?.UserName; // Nếu bạn có trường này
            category.UpdatedByUserRole = currentUserRole ?? "Unknown";
            category.UpdatedDate = DateTime.Now; // Ghi nhận thời gian khôi phục là thời gian cập nhật cuối

            // Đánh dấu entity là Modified
            db.Entry(category).State = EntityState.Modified;

            await db.SaveChangesAsync(); // Lưu thay đổi

            return RedirectToAction("DeletedIndex"); // Chuyển hướng về trang danh sách các danh mục đã xóa
        }


        // --- ACTION: XÓA HOÀN TOÀN DANH MỤC (HardDelete) ---
        // POST: Admin/Categories/HardDelete/5
        // LƯU Ý: Action này sẽ XÓA VĨNH VIỄN bản ghi khỏi DB.
        // Nên cẩn thận khi sử dụng, và thường chỉ nên áp dụng cho các bản ghi đã xóa mềm.
        [HttpPost, ActionName("HardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HardDeleteConfirmed(int id)
        {
            // Tìm danh mục cần xóa hoàn toàn.
            // Bạn có thể cần .AsNoTracking() nếu bản ghi đang được DbContext theo dõi từ trước,
            // nhưng FindAsync() sẽ trả về bản ghi đang được theo dõi nếu có.
            var category = await db.Categories.FindAsync(id);

            if (category == null)
            {
                return HttpNotFound(); // Không tìm thấy danh mục
            }

            // TÙY CHỌN: Chỉ cho phép xóa cứng các bản ghi đã bị xóa mềm
            if (!category.IsDeleted)
            {
                // Trả về lỗi hoặc chuyển hướng nếu không muốn xóa cứng bản ghi đang hoạt động
                // Ví dụ: return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chỉ có thể xóa cứng bản ghi đã bị xóa mềm.");
                // Hoặc đơn giản là chuyển hướng về Index nếu không hợp lệ
                return RedirectToAction("Index");
            }

            // --- THỰC HIỆN XÓA CỨNG (Vĩnh viễn khỏi DB) ---
            // Gọi .Remove() trực tiếp để Entity Framework tạo lệnh DELETE SQL.
            // Vì SaveChanges() override của bạn KHÔNG CÒN xử lý EntityState.Deleted
            // thành Modified nữa, lệnh này sẽ thực sự xóa bản ghi.
            db.Categories.Remove(category);

            await db.SaveChangesAsync(); // Lưu thay đổi, thực hiện Hard Delete

            return RedirectToAction("DeletedIndex"); // Chuyển hướng về trang danh sách đã xóa
        }


        // Phương thức Dispose để giải phóng tài nguyên DbContext
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Helper để lấy IAuthenticationManager (cần cho đăng xuất nếu có)
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}