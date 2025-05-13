using EnglishStudySystem.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnglishStudySystem.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        [ChildActionOnly]
        public ActionResult List()
        {
            // Lấy User ID của người dùng hiện tại
            // Phương thức GetUserId() có thể nằm trong Microsoft.AspNet.Identity
            // hoặc là một extension method trong System.Security.Claims tùy phiên bản .NET Identity
            var currentUserId = User.Identity.GetUserId();

            // Nếu người dùng chưa đăng nhập, trả về partial view rỗng hoặc thông báo không có gì
            // (Điều này có thể xảy ra nếu phần hiển thị thông báo luôn có trên layout)
            if (string.IsNullOrEmpty(currentUserId))
            {
                // Trả về một partial view rỗng hoặc chỉ hiển thị thông báo "Không có thông báo nào"
                // Chúng ta sẽ tạo Partial View "_NotificationDropdownPartial" cho mục đích này
                return PartialView("_NotificationDropdownPartial", new List<UserNotification>());
            }


            using (var db = new ApplicationDbContext()) // Thay ApplicationDbContext bằng tên DbContext của bạn
            {
                // Truy vấn lấy các UserNotification của người dùng hiện tại
                // Sử dụng .Include(un => un.Notification) để tải thông tin chi tiết từ bảng Notification
                var userNotifications = db.UserNotifications
                                          .Where(un => un.UserId == currentUserId)
                                          // Thêm .OrderBy() nếu bạn muốn sắp xếp, ví dụ theo ngày tạo thông báo
                                          // .OrderByDescending(un => un.Notification.CreatedDate)
                                          .Include(un => un.Notification) // Rất quan trọng để có dữ liệu Notification
                                          .ToList();

                // Trả về Partial View, truyền danh sách userNotifications làm Model
                // Tên Partial View theo quy ước nên bắt đầu bằng dấu gạch dưới (_)
                return PartialView("_NotificationDropdownPartial", userNotifications);
            }
        }
    }
}