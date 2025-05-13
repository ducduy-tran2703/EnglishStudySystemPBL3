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
        public ActionResult List()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var notifications = db.UserNotifications
                 .Where(un => un.UserId == User.Identity.GetUserId())
                 .Include(un => un.Notification)
                 .ToList();
            return View(notifications);
        }
    }
}