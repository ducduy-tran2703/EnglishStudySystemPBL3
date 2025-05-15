using EnglishStudySystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

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
            // Trong action Details
            var lessonTests = _db.Tests
                .Where(t => t.LessonId == id && !t.IsDeleted)
                .OrderBy(t => t.CreatedDate)
                .ToList();
            ViewBag.LessonTests = lessonTests;
            ViewBag.Comments = lesson.Comments.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedDate).ToList();
            ViewBag.IsSaved = isSaved;
            ViewBag.RelatedLessons = relatedLessons;

            return View(lesson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult AddComment(Comment model) // Sửa thành model binding thay vì tham số riêng lẻ
        {
            if (ModelState.IsValid)
            {
                var comment = new Comment
                {
                    LessonId = model.LessonId,
                    Content = model.Content,
                    ParentCommentId = model.ParentCommentId, // Thêm dòng này
                    UserId = User.Identity.GetUserId(),
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                _db.Comments.Add(comment);
                _db.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
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