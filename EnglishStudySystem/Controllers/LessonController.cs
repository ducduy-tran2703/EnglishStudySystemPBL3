using EnglishStudySystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using static System.Net.Mime.MediaTypeNames;

namespace EnglishStudySystem.Controllers
{
    public class LessonController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        [HttpGet]
        public ActionResult Details(int id)
        {
            var currentUserId = User.Identity.GetUserId(); // Lấy ID người dùng hiện tại
            ViewBag.UserId = currentUserId; // Truyền vào ViewBag
            var lesson = _db.Lessons
            .Include(l => l.Category)
            .Include(l => l.Comments.Select(c => c.User))
            .Include(l => l.Comments.Select(c => c.Replies)) // Thêm dòng này
            .FirstOrDefault(l => l.Id == id);
            if (lesson != null)
            {
                // Lấy thông tin người tạo
                var creator = _db.Users.Find(lesson.CreatedByUserId);
                ViewBag.CreatorName = creator?.FullName ?? "Không xác định";

                // Lấy thông tin người cập nhật (nếu có)
                if (!string.IsNullOrEmpty(lesson.UpdatedByUserId))
                {
                    var updater = _db.Users.Find(lesson.UpdatedByUserId);
                    ViewBag.UpdaterName = updater?.FullName ?? "Không xác định";
                }
            }
            var userId = User.Identity.GetUserId();
            if (lesson == null)
            {
                return HttpNotFound();
            }
            if (lesson.IsFreeTrial==false)
            {
                // Kiểm tra xem người dùng đã mua khóa học chứa bài học này chưa
                bool hasPurchased = _db.Payments.Any(p =>
                p.UserId == userId &&
                    p.CategoryId == lesson.CategoryId &&
                    p.Status == "Completed" &&
                    p.PaymentDate <= DateTime.Now);
                bool CanView;
                if (User.IsInRole("Administrator") || User.IsInRole("Editor"))
                    CanView = true;
                else
                    CanView = false;
                @ViewBag.CanView = CanView; 
                if (!hasPurchased && !CanView)
                {
                    return RedirectToAction("AccessDenied", "Error", new { message = "Bạn cần mua khóa học để xem bài học này" });
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                var existingHistory = _db.LessonHistories
                    .FirstOrDefault(h => h.LessonId == id && h.UserId == userId);

                if (existingHistory == null)
                {
                    // Nếu chưa có trong lịch sử thì thêm mới
                    _db.LessonHistories.Add(new LessonHistory
                    {
                        LessonId = id,
                        UserId = userId,
                        ViewDate = DateTime.Now
                    });
                }
                else
                {
                    // Nếu đã có thì cập nhật thời gian xem
                    existingHistory.ViewDate = DateTime.Now;
                }

                _db.SaveChanges();
            }

            var isSaved = false;

            if (!string.IsNullOrEmpty(userId))
            {
                isSaved = _db.SavedLessons.Any(s => s.LessonId == id && s.UserId == userId);
            }

            var relatedLessons = _db.Lessons
                .Where(l => l.CategoryId == lesson.CategoryId && l.Id != id)
                .OrderByDescending(l => l.CreatedDate)
                .Take(5)
                .ToList();

               ViewBag.Comments = lesson.Comments.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedDate).ToList();
            var lessonTests = _db.Tests
    .Where(t => t.LessonId == id && !t.IsDeleted)
    .OrderBy(t => t.CreatedDate)
    .ToList();

            ViewBag.LessonTests = lessonTests;
            ViewBag.IsSaved = isSaved;
            ViewBag.RelatedLessons = relatedLessons;

            return View(lesson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult AddComment(Comment model)
        {
            try
            {
                var comment = new Comment
                {
                    LessonId = model.LessonId,
                    Content = model.Content,
                    ParentCommentId = model.ParentCommentId,
                    UserId = User.Identity.GetUserId(),
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                _db.Comments.Add(comment);
                _db.SaveChanges();

                // Tạo thông báo nếu đây là phản hồi (reply)
                if (model.ParentCommentId.HasValue)
                {
                    CreateCommentReplyNotification(comment);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private void CreateCommentReplyNotification(Comment reply)
        {
            try
            {
                // Lấy thông tin comment gốc
                var parentComment = _db.Comments
                    .Include(c => c.User)
                    .Include(c => c.Lesson)
                    .FirstOrDefault(c => c.Id == reply.ParentCommentId);

                if (parentComment == null || parentComment.UserId == reply.UserId)
                {
                    return; // Không tạo thông báo nếu tự phản hồi chính mình
                }

                // Tạo thông báo
                var notification = new Notification
                {
                    Title = "Bạn có phản hồi mới",
                    Content = "Có người đã phản hồi bình luận của bạn",
                    CreatedDate = DateTime.Now,
                    SenderId = reply.UserId,
                    IsDeleted = false,
                    RelatedEntityType = "CommentReply",
                    TargetController = "Lesson",
                    TargetAction = "Details",
                    PrimaryRelatedEntityId = parentComment.LessonId, // ID bài học
                    SecondaryRelatedEntityId = parentComment.Id // ID comment gốc (để scroll tới)
                };

                _db.Notifications.Add(notification);
                _db.SaveChanges();

                // Tạo UserNotification liên kết
                var userNotification = new UserNotification
                {
                    UserId = parentComment.UserId, // Người nhận là tác giả comment gốc
                    NotificationId = notification.Id,
                    IsRead = false,
                    IsDeleted = false
                };

                _db.UserNotifications.Add(userNotification);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi tạo thông báo phản hồi: " + ex.Message);
            }
        }

        private string TruncateContent(string content, int maxLength)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;
            return content.Length <= maxLength ? content : content.Substring(0, maxLength) + "...";
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteComment(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var comment = _db.Comments.Find(id);

                if (comment == null || comment.UserId != userId)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận hoặc không có quyền xóa" });
                }

                comment.IsDeleted = true;
                comment.DeletedAt = DateTime.Now;
                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult ToggleSaveLesson(int lessonId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var savedLesson = _db.SavedLessons.FirstOrDefault(s => s.LessonId == lessonId && s.UserId == userId);

                if (savedLesson == null)
                {
                    // Thêm vào danh sách đã lưu
                    _db.SavedLessons.Add(new SavedLesson
                    {
                        LessonId = lessonId,
                        UserId = userId,
                        SavedDate = DateTime.Now
                    });
                }
                else
                {
                    // Xóa khỏi danh sách đã lưu
                    _db.SavedLessons.Remove(savedLesson);
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public JsonResult IsLessonSaved(int lessonId)
        {
            var userId = User.Identity.GetUserId();
            var isSaved = _db.SavedLessons.Any(s => s.LessonId == lessonId && s.UserId == userId);
            return Json(new { isSaved = isSaved }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}