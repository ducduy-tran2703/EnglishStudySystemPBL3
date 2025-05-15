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
            var currentUserId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return PartialView("_NotificationDropdownPartial", new List<UserNotification>());
            }

            using (var db = new ApplicationDbContext())
            {
                var userNotifications = db.UserNotifications
                                          .Where(un => un.UserId == currentUserId)
                                          .Include(un => un.Notification)
                                          .OrderByDescending(un => un.Notification.CreatedDate) // 👉 Sắp xếp mới nhất đến cũ nhất
                                          .ToList();

                return PartialView("_NotificationDropdownPartial", userNotifications);
            }
        }

        public ActionResult MarkAsRead(int notification_id,string NAction,string NController,string area ,int id,string secondaryId)
        {
            try
            {
                var currentUserId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                using (var db = new ApplicationDbContext())
                {
                    var userNotification = db.UserNotifications
                                          .FirstOrDefault(un => un.Id == notification_id && un.UserId == currentUserId);

                    if (userNotification != null)
                    {
                        userNotification.IsRead = true;
                        db.SaveChanges();
                        return RedirectToAction(NAction, NController, new { area = area, id = id, secondaryId = secondaryId });
                    }
                    return Content("Thông báo không tồn tại hoặc đã được đánh dấu là đã đọc trước đó.");
                }
            }
            catch (Exception ex)
            {
                return Content("Thông báo không tồn tại hoặc đã được đánh dấu là đã đọc trước đó.");
            }
        }
    }
        
}