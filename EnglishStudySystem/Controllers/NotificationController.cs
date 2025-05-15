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
                                          .ToList();

                return PartialView("_NotificationDropdownPartial", userNotifications);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsRead(int id)
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
                                          .FirstOrDefault(un => un.Id == id && un.UserId == currentUserId);

                    if (userNotification != null)
                    {
                        userNotification.IsRead = true;
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    return Json(new { success = false, message = "Notification not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult SeedLessons()
            {
                try
                {
                ApplicationDbContext db = new ApplicationDbContext();
                    // Kiểm tra xem đã có dữ liệu Lesson chưa
                    if (db.Lessons.Any())
                    {
                        return Content("Dữ liệu Lesson đã tồn tại.");
                    }

                    // Danh sách tên bài học tiếng Anh
                    var lessonNames = new[]
                    {
                    "Greetings and Introductions",
                    "Daily Routines",
                    "Food and Drinks",
                    "Travel and Transportation",
                    "Shopping and Money",
                    "Hobbies and Interests",
                    "Weather and Seasons",
                    "Health and Fitness",
                    "Work and Careers",
                    "Technology and Social Media",
                    "Culture and Traditions",
                    "Education and Learning",
                    "Home and Family",
                    "Sports and Activities",
                    "Nature and Environment"
                };

                    var random = new Random();
                    var currentUserId = "system"; // ID của user hệ thống
                    var currentDate = DateTime.Now;

                    // Tạo 3 bài học cho mỗi CategoryId từ 3 đến 7
                    for (int categoryId = 3; categoryId <= 7; categoryId++)
                    {
                        // Lấy ngẫu nhiên 3 tên bài học khác nhau
                        var selectedLessons = lessonNames.OrderBy(x => random.Next()).Take(3).ToList();

                        for (int i = 0; i < 3; i++)
                        {
                            var lesson = new Lesson
                            {
                                CategoryId = categoryId,
                                Title = selectedLessons[i],
                                Description = $"This is a lesson about {selectedLessons[i].ToLower()}.",
                                Video_URL = $"https://example.com/videos/{selectedLessons[i].Replace(" ", "-").ToLower()}",
                                CreatedByUserId = currentUserId,
                                CreatedByUserRole = "Admin",
                                CreatedDate = currentDate,
                                IsDeleted = false
                            };

                            db.Lessons.Add(lesson);
                        }
                    }

                    db.SaveChanges();
                    return Content("Dữ liệu Lesson đã được khởi tạo thành công.");
                }
                catch (Exception ex)
                {
                    return Content($"Lỗi khi khởi tạo dữ liệu: {ex.Message}");
                }
            }
        public ActionResult SeedLessonTrackingData()
        {
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                string userId = "581b7f8c-0129-44fc-961f-a190484fdcd6"; // ID người dùng cụ thể

                // Kiểm tra xem đã có dữ liệu Lesson chưa
                if (!db.Lessons.Any())
                {
                    return Content("Vui lòng chạy SeedLessons trước để tạo dữ liệu bài học.");
                }

                // Lấy tất cả bài học từ database
                var allLessons = db.Lessons.ToList();
                var random = new Random();
                var currentDate = DateTime.Now;

                // Tạo dữ liệu cho LessonHistory (lịch sử xem bài học)
                if (!db.LessonHistories.Any())
                {
                    foreach (var lesson in allLessons)
                    {
                        // Tạo 1-3 bản ghi lịch sử cho mỗi bài học
                        int historyCount = random.Next(1, 4);
                        for (int i = 0; i < historyCount; i++)
                        {
                            var history = new LessonHistory
                            {
                                LessonId = lesson.Id,
                                UserId = userId,
                                ViewDate = currentDate.AddDays(-random.Next(0, 30)) // Ngẫu nhiên trong 30 ngày qua
                            };
                            db.LessonHistories.Add(history);
                        }
                    }
                }

                // Tạo dữ liệu cho SavedLesson (bài học đã lưu)
                if (!db.SavedLessons.Any())
                {
                    // Lưu ngẫu nhiên 5-8 bài học
                    var lessonsToSave = allLessons.OrderBy(x => random.Next()).Take(random.Next(5, 9)).ToList();

                    foreach (var lesson in lessonsToSave)
                    {
                        var savedLesson = new SavedLesson
                        {
                            LessonId = lesson.Id,
                            UserId = userId,
                            SavedDate = currentDate.AddDays(-random.Next(0, 15)) // Ngẫu nhiên trong 15 ngày qua
                        };
                        db.SavedLessons.Add(savedLesson);
                    }
                }

                db.SaveChanges();
                return Content("Dữ liệu LessonHistory và SavedLesson đã được khởi tạo thành công.");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi khi khởi tạo dữ liệu: {ex.Message}");
            }
        }
        public ActionResult SeedTestData()
        {
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var random = new Random();

                // Kiểm tra xem đã có dữ liệu Lesson chưa
                var lessons = db.Lessons.ToList();
                if (!lessons.Any())
                {
                    return Content("Vui lòng chạy SeedLessons trước để tạo dữ liệu bài học.");
                }

                // Kiểm tra nếu đã có dữ liệu Test
                if (db.Tests.Any())
                {
                    return Content("Dữ liệu Test đã tồn tại.");
                }

                // Danh sách các loại bài test
                var testTypes = new[] { "Vocabulary", "Grammar", "Reading", "Listening", "Writing", "Speaking" };

                // Tạo 9 bài test
                for (int i = 0; i < 9; i++)
                {
                    // Chọn ngẫu nhiên 1 bài học
                    var lesson = lessons[random.Next(lessons.Count)];

                    // Chọn ngẫu nhiên loại test (lặp lại để có 6 bài kiểm tra bất kỳ)
                    var testType = testTypes[random.Next(testTypes.Length)];

                    var test = new Test
                    {
                        LessonId = lesson.Id,
                        Title = $"{testType} Test for '{lesson.Title}'",
                        Description = $"This is a {testType.ToLower()} test for the lesson: {lesson.Title}",
                        QuestionCount = 10,
                        CreatedDate = DateTime.Now.AddDays(-random.Next(0, 30)),
                        IsDeleted = false,
                        Questions = new List<Question>()
                    };

                    // Tạo 10 câu hỏi cho mỗi bài test
                    for (int q = 1; q <= 10; q++)
                    {
                        var question = new Question
                        {
                            Content = GenerateQuestionContent(testType, q),
                            Answers = new List<Answer>()
                        };

                        // Tạo 4 đáp án cho mỗi câu hỏi (1 đúng, 3 sai)
                        var answers = GenerateAnswers(testType, q);
                        foreach (var answer in answers)
                        {
                            question.Answers.Add(new Answer
                            {
                                Content = answer.Content,
                                IsCorrect = answer.IsCorrect
                            });
                        }

                        test.Questions.Add(question);
                    }

                    db.Tests.Add(test);
                }

                db.SaveChanges();
                return Content("Dữ liệu Test, Question và Answer đã được khởi tạo thành công.");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi khi khởi tạo dữ liệu: {ex.Message}");
            }
        }

        // Hàm tạo nội dung câu hỏi theo loại test
        private string GenerateQuestionContent(string testType, int questionNumber)
        {
            switch (testType)
            {
                case "Vocabulary":
                    var vocabWords = new[] { "apple", "run", "happy", "quickly", "beautiful", "understand", "friendly", "dangerous", "celebrate", "knowledge" };
                    return $"What is the meaning of the word '{vocabWords[questionNumber - 1]}'?";

                case "Grammar":
                    var grammarQuestions = new[]
                    {
                "Which sentence is in the present perfect tense?",
                "Choose the correct form of the verb: She ___ to school every day.",
                "Identify the adverb in the sentence: 'He ran quickly to catch the bus.'",
                "Which of these is a preposition?",
                "What is the plural form of 'child'?",
                "Which sentence is in the passive voice?",
                "Choose the correct comparative form: 'This book is ___ than that one.'",
                "Which word is a conjunction?",
                "Identify the direct object in the sentence: 'She gave him a gift.'",
                "Which sentence uses the correct article?"
            };
                    return grammarQuestions[questionNumber - 1];

                case "Reading":
                    return $"Reading comprehension question #{questionNumber} about the text";

                case "Listening":
                    return $"Listening question #{questionNumber} about the audio";

                case "Writing":
                    return $"Writing prompt #{questionNumber}: Write a short paragraph about...";

                case "Speaking":
                    return $"Speaking question #{questionNumber}: How would you respond to...";

                default:
                    return $"Question #{questionNumber} about the lesson";
            }
        }

        // Hàm tạo đáp án theo loại test
        private List<(string Content, bool IsCorrect)> GenerateAnswers(string testType, int questionNumber)
        {
            var answers = new List<(string Content, bool IsCorrect)>();
            var random = new Random();

            switch (testType)
            {
                case "Vocabulary":
                    // Tạo 1 đáp án đúng và 3 đáp án sai cho từ vựng
                    var vocabCorrect = new[]
                    {
                "A fruit", "To move fast on foot", "Feeling joy", "In a fast manner",
                "Pleasing the senses", "To comprehend", "Kind and pleasant",
                "Possessing risk", "To observe an occasion", "Information and understanding"
            };

                    answers.Add((vocabCorrect[questionNumber - 1], true));

                    // Thêm 3 đáp án sai
                    var vocabWrong = new[]
                    {
                "A vegetable", "To jump high", "Feeling sad", "In a slow manner",
                "Ugly appearance", "To forget", "Mean and rude",
                "Very safe", "To ignore an event", "Ignorance"
            };

                    for (int i = 0; i < 3; i++)
                    {
                        answers.Add((vocabWrong[(questionNumber - 1 + i) % vocabWrong.Length], false));
                    }
                    break;

                case "Grammar":
                    // Tạo đáp án cho câu hỏi ngữ pháp
                    if (questionNumber == 1)
                    {
                        answers.Add(("I have eaten breakfast", true));
                        answers.Add(("I eat breakfast", false));
                        answers.Add(("I will eat breakfast", false));
                        answers.Add(("I ate breakfast", false));
                    }
                    else if (questionNumber == 2)
                    {
                        answers.Add(("goes", true));
                        answers.Add(("go", false));
                        answers.Add(("is going", false));
                        answers.Add(("gone", false));
                    }
                    // Thêm các trường hợp khác tương tự...
                    break;

                default:
                    // Đáp án mẫu cho các loại test khác
                    answers.Add(("Correct answer", true));
                    answers.Add(("Wrong answer 1", false));
                    answers.Add(("Wrong answer 2", false));
                    answers.Add(("Wrong answer 3", false));
                    break;
            }

            // Trộn các đáp án để không phải lúc nào đáp án đúng cũng ở vị trí đầu tiên
            return answers.OrderBy(x => random.Next()).ToList();
        }
        public ActionResult Seed()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var userId = "581b7f8c-0129-44fc-961f-a190484fdcd6"; // ID người dùng nhận thông báo
            var senderId = "8c442efe-67d7-4ec6-a238-6adb7700b15b"; // ID người gửi (admin)
            var now = DateTime.Now;

            // Tạo danh sách thông báo
            var notifications = new List<Notification>
    {
        new Notification
        {
            Title = "Hoạt động học tập mới",
            Content = "Bạn có một hoạt động học tập mới đang chờ hoàn thành.",
            CreatedDate = now.AddDays(-2),
            SenderId = senderId, // Sử dụng senderId đã chỉ định
            TargetController = "Customer",
            TargetAction = "LearningActivities",
            TargetArea = "", // Không sử dụng area
            PrimaryRelatedEntityId = 1, // ID bài học/hoạt động liên quan
            SecondaryRelatedEntityId = null,
            RelatedEntityType = "LearningActivity",
            IsDeleted = false
        },
        new Notification
        {
            Title = "Nhắc nhở hoàn thành bài học",
            Content = "Bạn có bài học chưa hoàn thành trong 3 ngày qua.",
            CreatedDate = now.AddDays(-1),
            SenderId = senderId, // Sử dụng senderId đã chỉ định
            TargetController = "Customer",
            TargetAction = "LearningActivities",
            TargetArea = "",
            PrimaryRelatedEntityId = 2,
            SecondaryRelatedEntityId = null,
            RelatedEntityType = "LearningActivity",
            IsDeleted = false
        },
        new Notification
        {
            Title = "Thông báo khuyến mãi",
            Content = "Chương trình khuyến mãi đặc biệt dành cho học viên.",
            CreatedDate = now,
            SenderId = senderId, // Sử dụng senderId đã chỉ định
            TargetController = "Customer",
            TargetAction = "LearningActivities",
            TargetArea = "",
            PrimaryRelatedEntityId = null,
            SecondaryRelatedEntityId = null,
            RelatedEntityType = "Promotion",
            IsDeleted = false
        }
    };

            context.Notifications.AddRange(notifications);
            context.SaveChanges();

            // Tạo liên kết UserNotification cho người dùng
            var userNotifications = new List<UserNotification>();
            foreach (var notification in notifications)
            {
                userNotifications.Add(new UserNotification
                {
                    UserId = userId,
                    NotificationId = notification.Id,
                    IsRead = false,
                    IsDeleted = false
                });
            }

            context.UserNotifications.AddRange(userNotifications);
            context.SaveChanges();
            return Content("Dữ liệu thông báo đã được khởi tạo thành công.");
        }
    }
        
}