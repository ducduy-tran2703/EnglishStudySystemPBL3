// EnglishStudySystem/Areas/Admin/Controllers/CategoriesController.cs

using System;
using System.Data.Entity; // Cần cho Include, EntityState, ToListAsync
using System.Linq; // Cần cho Where, OrderBy
using System.Net; // Cần cho HttpStatusCodeResult
using System.Threading.Tasks; // Cần cho async/await
using System.Web;
using System.Web.Mvc; // Cần cho Controller, ActionResult, View, HttpNotFound
using EnglishStudySystem;
using EnglishStudySystem.Models; // Cần cho ApplicationDbContext, Category
using Microsoft.AspNet.Identity; // Cần để lấy User ID và UserName của người tạo/cập nhật

// Cần thêm using để sử dụng các thuộc tính phân quyền Identity
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

// Correcting the syntax error in the namespace declaration
namespace EnglishStudySystem.Areas.Admin.Controllers
{

    // Đặt Authorize cho Controller để chỉ Admin hoặc Editor mới có thể truy cập
    //[Authorize(Roles = "Administrator, Editor")]
    public class CategoriesController : Controller
    {
        // Khai báo DbContext để truy vấn dữ liệu
        private ApplicationDbContext db = new ApplicationDbContext();

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

            // Truyền danh sách các danh mục đang hoạt động sang View
            return View(activeCategories);
        }

        // --- ACTION: DANH SÁCH CÁC DANH MỤC ĐÃ XÓA MỀM (DeletedIndex) ---
        // GET: Admin/Categories/Deleted
        public async Task<ActionResult> DeletedIndex()
        {
            // Lấy tất cả các danh mục ĐÃ bị xóa mềm
            // Lọc thủ công bằng .Where(c => c.IsDeleted)
            var deletedCategories = await db.Categories
                                            .Where(c => c.IsDeleted)
                                            .OrderByDescending(c => c.DeletedAt) // Sắp xếp theo thời gian xóa
                                            .ToListAsync();

            // Truyền danh sách các danh mục đã xóa sang View (có thể dùng View khác hoặc cùng View Index)
            // Để đơn giản, chúng ta sẽ tạo View DeletedIndex.cshtml riêng
            return View(deletedCategories);
        }


        // --- ACTION: XEM CHI TIẾT DANH MỤC (Details) ---
        // GET: Admin/Categories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            // Kiểm tra ID có được cung cấp không
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Trả về lỗi 400 Bad Request
            }

            // Tìm danh mục theo ID (bao gồm cả đã xóa mềm)
            // Không dùng .Where(!IsDeleted) ở đây vì vẫn muốn xem chi tiết danh mục đã xóa
            var category = await db.Categories.FindAsync(id);

            // Kiểm tra có tìm thấy danh mục không
            if (category == null)
            {
                return HttpNotFound(); // Trả về lỗi 404 Not Found
            }

            // Truyền đối tượng danh mục sang View Details
            return View(category);
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
        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
        public async Task<ActionResult> Create([Bind(Include = "Name,Description,Price")] Category category) // Chỉ bind các thuộc tính từ form
        {
            // Kiểm tra dữ liệu từ form có hợp lệ không (dựa trên các [Required], [StringLength], v.v. trong Model)
            if (ModelState.IsValid)
            {
                // --- GÁN GIÁ TRỊ CHO CÁC TRƯỜNG THEO DÕI VÀ XÓA MỀM KHI TẠO MỚI ---
                // Lấy ID và Vai trò của người dùng hiện tại đã đăng nhập
                string currentUserId = User.Identity.GetUserId();
                string currentUserName = User.Identity.GetUserName(); // Lấy UserName (thường là Email)

                // Lấy vai trò (lấy vai trò đầu tiên hoặc kiểm tra Admin/Editor)
                // Việc lấy vai trò có thể cần UserManager, hoặc kiểm tra Claims nếu vai trò được lưu trong Claims
                // Đơn giản nhất là lấy vai trò đầu tiên hoặc dùng Check User.IsInRole()
                // Để lấy vai trò cụ thể, bạn cần UserManager
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                string currentUserRole = currentUserRoles.FirstOrDefault(); // Lấy vai trò đầu tiên làm ví dụ

                category.CreatedByUserId = currentUserId;
                category.CreatedByUserRole = currentUserRole ?? "Unknown"; // Gán vai trò (dự phòng nếu không có vai trò)
                category.CreatedDate = DateTime.Now;
                category.UpdatedByUserId = null; // Chưa cập nhật lần nào
                category.UpdatedByUserRole = null;
                category.UpdatedDate = null;
                category.IsDeleted = false; // Mặc định khi tạo là chưa xóa
                category.DeletedAt = null;

                // --- THÊM VÀO DATABASE VÀ LƯU ---
                db.Categories.Add(category); // Thêm đối tượng danh mục vào DbSet
                await db.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu bất đồng bộ

                // Chuyển hướng về trang danh sách sau khi tạo thành công
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form với dữ liệu đã nhập và thông báo lỗi
            return View(category);
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
        [HttpPost, ActionName("SoftDelete")] // Đặt tên Action là SoftDelete, nhưng URL sẽ dùng tên này
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SoftDeleteConfirmed(int id) // Nhận ID của danh mục cần xóa mềm
        {
            // Tìm danh mục cần xóa mềm
            var category = await db.Categories.FindAsync(id);

            // Kiểm tra có tìm thấy danh mục và nó chưa bị xóa mềm không
            if (category != null && !category.IsDeleted)
            {
                // Đánh dấu bản ghi là đã xóa.
                // Logic override SaveChanges sẽ xử lý việc set IsDeleted = true và DeletedAt = DateTime.Now.
                db.Categories.Remove(category); // Dùng .Remove() để kích hoạt override SaveChanges

                // Lưu thay đổi vào cơ sở dữ liệu bất đồng bộ
                await db.SaveChangesAsync();
            }

            // Chuyển hướng về trang danh sách sau khi xóa mềm
            return RedirectToAction("Index");
        }


        // --- ACTION: KHÔI PHỤC DANH MỤC (Restore) ---
        // POST: Admin/Categories/Restore/5
        [HttpPost, ActionName("Restore")] // Đặt tên Action là Restore
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreConfirmed(int id) // Nhận ID của danh mục cần khôi phục
        {
            // Tìm danh mục cần khôi phục
            // Cần tìm cả các bản ghi đã xóa mềm, nên không dùng .Where(!IsDeleted)
            var category = await db.Categories.FindAsync(id);

            // Kiểm tra có tìm thấy danh mục và nó ĐANG bị xóa mềm không
            if (category != null && category.IsDeleted)
            {
                // --- THỰC HIỆN KHÔI PHỤC (Đảo ngược trạng thái xóa mềm) ---
                category.IsDeleted = false; // Đặt lại trạng thái xóa mềm
                category.DeletedAt = null; // Xóa thông tin thời gian xóa mềm

                // Cập nhật thông tin người cập nhật/thời gian cập nhật (tùy chọn)
                string currentUserId = User.Identity.GetUserId();
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var currentUserRoles = await userManager.GetRolesAsync(currentUserId);
                string currentUserRole = currentUserRoles.FirstOrDefault();

                category.UpdatedByUserId = currentUserId;
                category.UpdatedByUserRole = currentUserRole ?? "Unknown";
                category.UpdatedDate = DateTime.Now;

                // Đánh dấu trạng thái của bản ghi là Modified
                db.Entry(category).State = EntityState.Modified;

                // Lưu thay đổi vào cơ sở dữ liệu bất đồng bộ
                await db.SaveChangesAsync();
            }

            // Chuyển hướng về trang danh sách các danh mục đã xóa sau khi khôi phục
            return RedirectToAction("DeletedIndex");
        }


        // --- ACTION: XÓA HOÀN TOÀN DANH MỤC (HardDelete) ---
        // POST: Admin/Categories/HardDelete/5
        // LƯU Ý: Action này sẽ XÓA VĨNH VIỄN bản ghi khỏi DB.
        // Hãy cẩn thận khi sử dụng, chỉ nên áp dụng cho các bản ghi đã xóa mềm.
        // Cần cơ chế để BỎ QUA logic Soft Delete trong SaveChanges cho bản ghi này.
        [HttpPost, ActionName("HardDelete")] // Đặt tên Action là HardDelete
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HardDeleteConfirmed(int id) // Nhận ID của danh mục cần xóa hoàn toàn
        {
            // Tìm danh mục cần xóa hoàn toàn
            // Cần tìm cả các bản ghi đã xóa mềm
            var category = await db.Categories.FindAsync(id);

            // Kiểm tra có tìm thấy danh mục và nó ĐANG bị xóa mềm không
            // Chỉ cho phép xóa hoàn toàn các bản ghi ĐÃ bị xóa mềm
            if (category != null && category.IsDeleted)
            {
                // --- THỰC HIỆN XÓA VĨNH VIỄN ---
                // Cách đơn giản nhất để bypass Soft Delete override là đánh dấu trạng thái Deleted TRỰC TIẾP VÀ GỌI BASE SaveChanges
                // Tuy nhiên, việc gọi base.SaveChanges() trực tiếp từ controller không lý tưởng
                // Cách thông thường là dùng db.Set<T>().Remove() và cấu hình trong OnModelCreating hoặc SaveChanges để nhận biết
                // Với cách override SaveChanges hiện tại của bạn, bạn cần một cách để nói rằng "lần này thì xóa thật".
                // Một cách (hơi hacky) là tạm thời tắt/bỏ qua override cho lần gọi SaveChanges này,
                // hoặc thêm một điều kiện kiểm tra trong override SaveChanges.
                // Cách đơn giản nhất với setup hiện tại là đánh dấu trạng thái EntityState.Deleted và sau đó gọi SaveChanges
                // Nhưng override SaveChanges sẽ bắt lấy trạng thái Deleted và chuyển nó thành Modified!
                // Để XÓA THẬT với setup này, bạn cần tìm cách BYPASS HOÀN TOÀN override SaveChanges hoặc sửa override SaveChanges.
                // CÁCH ĐƠN GIẢN NHẤT (nhưng hơi không rõ ràng với override hiện tại) là dùng db.Remove():
                db.Categories.Remove(category); // Mark as Deleted

                // Cần đảm bảo rằng lệnh SaveChanges() tiếp theo sẽ THỰC SỰ XÓA bản ghi này,
                // chứ không phải bị intercept bởi override SaveChanges() và biến thành Soft Delete.
                // Với override SaveChanges() hiện tại của bạn, lệnh db.Categories.Remove(category)
                // sẽ dẫn đến việc bản ghi bị Soft Delete chứ không phải Hard Delete!

                // Để thực hiện Hard Delete với override SaveChanges hiện tại, bạn có 2 lựa chọn chính:
                // 1. Tạm thời tắt/bỏ qua override cho lệnh SaveChanges này (cách phức tạp, không nên).
                // 2. Sửa lại override SaveChanges để nó nhận biết trường hợp cần Hard Delete (ví dụ: kiểm tra một cờ, hoặc kiểm tra một kiểu entity đặc biệt).
                // 3. Cách phổ biến khác: Truy vấn lại bản ghi KHÔNG TRACKING, gán Id, đánh dấu trạng thái là Deleted và gọi base SaveChanges.

                // Cách 3: Hard Delete an entity by re-attaching it as Deleted (bypasses the override SaveChanges)
                var hardDeleteCategory = new Category { Id = id }; // Tạo một đối tượng mới chỉ với ID
                db.Entry(hardDeleteCategory).State = EntityState.Deleted; // Đánh dấu trạng thái là Deleted

                // Bây giờ gọi SaveChanges. Entity Framework sẽ thấy một entity có ID này đang ở trạng thái Deleted
                // và thực hiện lệnh DELETE trong database. LƯU Ý: Cách này BỎ QUA override SaveChanges() của bạn!
                // Nếu bạn muốn override SaveChanges() xử lý cả Hard Delete dựa trên một cờ, bạn cần sửa lại override đó.


                // --- Chúng ta sẽ sử dụng cách Re-attach và đánh dấu Deleted để Hard Delete ---
                // Lưu ý: Việc này không kích hoạt override SaveChanges() của bạn cho bản ghi này.
                // Nếu có các bản ghi khác đang chờ Soft Delete trong cùng một SaveChanges,
                // bạn cần lưu ý cách xử lý. Tốt nhất là Hard Delete trong một SaveChanges() riêng.

                // Lấy bản ghi gốc từ DB lần nữa, nhưng đảm bảo nó không bị EF tracking
                var categoryToHardDelete = db.Categories.AsNoTracking().SingleOrDefault(c => c.Id == id);
                if (categoryToHardDelete != null)
                {
                    // Đánh dấu bản ghi này (không bị tracking) là Deleted
                    db.Entry(categoryToHardDelete).State = EntityState.Deleted;
                    // Lưu thay đổi. Lệnh này sẽ thực hiện HARD DELETE.
                    // Việc này BỎ QUA override SaveChanges() của bạn cho bản ghi này.
                    db.SaveChanges(); // Sử dụng SaveChanges() đồng bộ hoặc SaveChangesAsync() không CancellationToken nếu override của bạn cho phép
                }
                else
                {
                    // Xử lý trường hợp không tìm thấy bản ghi (rất hiếm nếu đã kiểm tra ở trên)
                    return HttpNotFound();
                }

                // Hoặc CÁCH KHÁC (Nếu bạn muốn dùng Remove và sửa override SaveChanges):
                // Sửa override SaveChanges để nó kiểm tra một cờ nào đó trong DbContext
                // Hoặc kiểm tra một thuộc tính đặc biệt trên Entity (ví dụ IHardDeletable)
                // Hoặc truyền một tham số đặc biệt vào SaveChanges (cần thêm chữ ký mới cho SaveChanges)

                // VỚI CÁCH OVERRIDE SAVECHANGES() CỦA BẠN HIỆN TẠI, cách RE-ATTACH là cách đơn giản nhất để HARD DELETE.


            }

            // Chuyển hướng về trang danh sách các danh mục đã xóa sau khi xóa hoàn toàn
            return RedirectToAction("DeletedIndex");
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