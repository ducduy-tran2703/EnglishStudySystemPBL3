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
            var lesson = _db.Lessons
                .Include(l => l.Category)
                .Include(l => l.Comments.Select(c => c.User))
                .FirstOrDefault(l => l.Id == id);

            if (lesson == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
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
            ViewBag.IsSaved = isSaved;
            ViewBag.RelatedLessons = relatedLessons;

            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public JsonResult AddComment(int lessonId, string content)
        {
            try
            {
                var comment = new Comment
                {
                    LessonId = lessonId,
                    Content = content,
                    UserId = User.Identity.GetUserId(),
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                _db.Comments.Add(comment);
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