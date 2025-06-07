using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EnglishStudySystem.Models;
using EnglishStudySystem.ViewModel;
using Microsoft.AspNet.Identity;


namespace EnglishStudySystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController()
        {
            _context = new ApplicationDbContext();
        }

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public ActionResult CustomerDashBoard()
        {
            // Lấy danh sách categories giống như HomePage
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToList();

            // Lấy thông tin user names nếu cần
            var userIds = categories.Select(c => c.CreatedByUserId).Distinct().ToList();
            var userNames = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName);

            ViewBag.UserNames = userNames;
            ViewBag.ListCategory = categories;
            return View(categories);
        }
        public ActionResult Payment(int categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            if (category == null)
            {
                return HttpNotFound();
            }
            // Lấy thông tin bài học nếu cần
            var lessons = _context.Lessons
                .Where(l => l.CategoryId == categoryId && !l.IsDeleted)
                .ToList();
            ViewBag.Lessons = lessons;
            return View(category);
        }
        public ActionResult DetailUser()
        {
            var userId = User.Identity.GetUserId(); // This method is part of Microsoft.AspNet.Identity namespace
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Lấy thông tin bài học nếu cần
            var categories = _context.Categories
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .Take(6)
            .ToList();
            ViewBag.ListCategories = categories;
            return View(user);
        }

        public ActionResult EditProfile()
        {
            System.Diagnostics.Debug.WriteLine("Entering POST EditProfile1...");
            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("HomePage", "Home");
            }
            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
            System.Diagnostics.Debug.WriteLine($"Updating user {userId} with:");
            System.Diagnostics.Debug.WriteLine($"FullName: {model.FullName}");
            System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
            System.Diagnostics.Debug.WriteLine($"PhoneNumber: {model.PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"DateOfBirth: {model.DateOfBirth}");
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProfile(ProfileViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("Entering POST EditProfile...");

            var userId = User.Identity.GetUserId();
            var user = _context.Users.Find(userId);
            System.Diagnostics.Debug.WriteLine($"1:");
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }
            System.Diagnostics.Debug.WriteLine($"Updating user {userId} with:");
            System.Diagnostics.Debug.WriteLine($"FullName: {model.FullName}");
            System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
            System.Diagnostics.Debug.WriteLine($"PhoneNumber: {model.PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"DateOfBirth: {model.DateOfBirth}");
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("EditProfile", "Customer");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ModelState.AddModelError("", "An error occurred while updating the profile.");
                return View(model);
            }

            return View(model);
        }
        public ActionResult LearningActivities()
        {
            var userId = User.Identity.GetUserId();

            using (var _context = new ApplicationDbContext())
            {
                // 1. Số lượng khóa học đã mua
                var boughtCoursesCount = _context.Payments
                    .Where(p => p.UserId == userId && p.Status == "Completed")
                    .Select(p => p.CategoryId)
                    .Distinct()
                    .Count();

                // 2. Số lượng bài học đã xem
                var viewedLessonsCount = _context.LessonHistories
                    .Where(h => h.UserId == userId && !h.Lesson.IsDeleted)
                    .Select(h => h.LessonId)
                    .Distinct()
                    .Count();

                // 3. Số lượng bài học yêu thích
                var favoriteLessonsCount = _context.SavedLessons
                    .Where(s => s.UserId == userId)
                    .Select(s => s.LessonId)
                    .Distinct()
                    .Count();

                // 4. Số lượng bài kiểm tra đã làm
                var takenTestsCount = _context.UserTestAttempts
                    .Where(uta => uta.UserId == userId)
                    .Select(uta => uta.TestId)
                    .Distinct()
                    .Count();

                // Tạo ViewModel hoặc sử dụng ViewBag để truyền dữ liệu

                ViewBag.BoughtCoursesCount = boughtCoursesCount;
                ViewBag.ViewedLessonsCount = viewedLessonsCount;
                ViewBag.FavoriteLessonsCount = favoriteLessonsCount;
                ViewBag.TakenTestsCount = takenTestsCount;
                return View();
            }
        }
        public ActionResult GetBoughtCoursesStats()
        {
            var userId = User.Identity.GetUserId();

            ApplicationDbContext _context = new ApplicationDbContext();

            var boughtCategories = _context.Payments
                .Where(p => p.UserId == userId && p.Status == "Completed")
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => p.Category)
                .Distinct()
                .ToList();

            var creatorUserIds = boughtCategories.Select(c => c.CreatedByUserId).Distinct().ToList();

            var users = _context.Users
                .Where(u => creatorUserIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.FullName);

            ViewBag.UserNames = users;

            return PartialView("_BoughtCoursesStats", boughtCategories);
        }

        public ActionResult GetLessonsHistoryStats()
        {
            var userId = User.Identity.GetUserId();

            using (ApplicationDbContext _context = new ApplicationDbContext())
            {
                var lessonHistories = _context.LessonHistories
                    .Where(h => h.UserId == userId && !h.Lesson.IsDeleted)
                    .Include(h => h.Lesson.Category) // Thêm include để load thông tin khóa học
                    .OrderByDescending(h => h.ViewDate)
                    .ToList();

                var viewModel = lessonHistories.Select(h => new LessonHistoryViewModel
                {
                    LessonId = h.LessonId,
                    LessonTitle = h.Lesson.Title,
                    LessonDescription = h.Lesson.Description,
                    CreatedDate = h.Lesson.CreatedDate,
                    ViewDate = h.ViewDate,
                    CourseName = h.Lesson.Category?.Name, // Lấy tên khóa học
                    CourseId = h.Lesson.CategoryId // Lấy ID khóa học để tạo link
                }).ToList();

                return PartialView("_LessonsHistoryStats", viewModel);
            }
        }

        public ActionResult GetFavoriteLessonsStats()
        {
            var userId = User.Identity.GetUserId();
            using (var _context = new ApplicationDbContext())
            {
                var favoriteLessons = _context.SavedLessons
                    .Where(s => s.UserId == userId)
                    .Include(s => s.Lesson.Category) // Load thông tin khóa học
                    .OrderByDescending(s => s.SavedDate)
                    .Select(s => new FavoriteLessonViewModel
                    {
                        LessonId = s.Lesson.Id,
                        LessonTitle = s.Lesson.Title,
                        LessonDescription = s.Lesson.Description,
                        CreatedDate = s.Lesson.CreatedDate,
                        SavedDate = s.SavedDate,
                        CourseName = s.Lesson.Category != null ? s.Lesson.Category.Name : null,
                        CourseId = s.Lesson.CategoryId
                    })
                    .Distinct()
                    .ToList();

                return PartialView("_FavoriteLessonsStats", favoriteLessons);
            }
        }
        public ActionResult GetTestHistoryStats()
        {
            var userId = User.Identity.GetUserId();
            using (var _context = new ApplicationDbContext())
            {
                // Lấy danh sách các lần làm bài kiểm tra của user
                var testAttempts = _context.UserTestAttempts
                    .Where(uta => uta.UserId == userId)
                    .Include(uta => uta.Test) // Load Test
                    .Include(uta => uta.Test.Lesson) // Load Lesson từ Test (EF6 hỗ trợ đường dẫn)
                    .Include(uta => uta.UserAnswers.Select(ua => ua.Question.Answers)) // Load sâu vào UserAnswers -> Question -> Answers
                    .OrderByDescending(uta => uta.AttemptDate)
                    .ToList();

                // Tạo view model chứa các thông tin cần hiển thị
                var viewModel = testAttempts.Select(uta => new TestHistoryViewModel
                {
                    TestId = uta.TestId,
                    TestTitle = uta.Test?.Title,
                    LessonName = uta.Test?.Lesson?.Title,
                    AttemptDate = uta.AttemptDate,
                    Score = uta.Score,
                    TotalQuestions = uta.Test?.QuestionCount ?? 0,
                    CorrectAnswers = uta.UserAnswers?.Count(ua => ua.IsCorrect) ?? 0,
                    IsCompleted = uta.IsCompleted,
                    Duration = uta.EndTime.HasValue ? (uta.EndTime.Value - uta.StartTime).TotalMinutes : 0
                }).ToList();

                return PartialView("_TestHistoryStats", viewModel);
            }
        }
        public ActionResult PaymentHistory()
        {
            string currentUserId = User.Identity.GetUserId();

            var payments = _context.Payments
                             .Where(p => p.UserId == currentUserId)
                             .Include("Category")
                             .OrderByDescending(p => p.PaymentDate)
                             .ToList();

            return View(payments);
        }

    }
}