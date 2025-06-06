﻿using EnglishStudySystem.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using System.Data.Entity;
using System.Web.Services.Description;
using static System.Net.Mime.MediaTypeNames;
namespace EnglishStudySystem.Controllers
{
    public class TestController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Test/Start/5 (5 là testId)
        [Authorize]
        public ActionResult Start(int id)
        {
            string userId = User.Identity.GetUserId();

            // Kiểm tra xem đã có attempt hoàn thành chưa
            var completedAttempt = db.UserTestAttempts
                                  .Include(a => a.Test)
                                  .Include(a => a.UserAnswers.Select(ua => ua.Question))
                                  .Include(a => a.UserAnswers.Select(ua => ua.SelectedAnswer))
                                  .FirstOrDefault(a => a.TestId == id && a.UserId == userId && a.IsCompleted);

            if (completedAttempt != null)
            {
                TempData["Message"] = "Bạn đã hoàn thành bài kiểm tra này trước đây.";
                return RedirectToAction("Details", new { id = completedAttempt.Id });
            }

            var test = db.Tests.Include(t => t.Questions).FirstOrDefault(t => t.Id == id);
            if (test == null) return HttpNotFound();

            if (!test.Lesson.IsFreeTrial)
            {
                // Kiểm tra xem người dùng đã mua khóa học chứa bài học này chưa
                bool hasPurchased = db.Payments.Any(p =>
                    p.UserId == userId &&
                    p.CategoryId == test.Lesson.CategoryId &&
                    p.Status == "Completed" &&
                    p.PaymentDate <= DateTime.Now);

                if (!hasPurchased)
                {
                    return RedirectToAction("AccessDenied", "Error", new { message = "Bạn cần mua khóa học này trước khi làm bài kiểm tra." });
                }
            }

            // Tạo attempt mới
            var attempt = new UserTestAttempt
            {
                UserId = userId,
                TestId = id,
                StartTime = DateTime.Now,
                AttemptDate = DateTime.Now,
                IsCompleted = false
            };

            db.UserTestAttempts.Add(attempt);
            db.SaveChanges();

            return RedirectToAction("TakeTest", new { attemptId = attempt.Id });
        }

        // GET: Test/TakeTest/5 (5 là attemptId)
        [Authorize]
        public ActionResult TakeTest(int attemptId)
        {
            // Lấy UserId trước khi thực hiện truy vấn
            string userId = User.Identity.GetUserId();

            // Lấy thông tin attempt
            var attempt = db.UserTestAttempts
                          .Include(a => a.Test)
                          .Include(a => a.Test.Questions.Select(q => q.Answers))
                          .FirstOrDefault(a => a.Id == attemptId && a.UserId == userId); // Sử dụng biến userId đã lấy trước

            if (attempt == null || attempt.IsCompleted)
            {
                return RedirectToAction("Details", "Lesson",new { id = attempt.Test.LessonId });
            }
            int time = attempt.Test.QuestionCount * 1 + 5;
            // Tính thời gian còn lại
            var timeRemaining = TimeSpan.FromMinutes(time) - (DateTime.Now - attempt.StartTime);
            ViewBag.TimeRemaining = timeRemaining.TotalSeconds > 0 ? timeRemaining : TimeSpan.Zero;

            return View(attempt);
        }

        // POST: Test/SubmitTest
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitTest(int attemptId, FormCollection form)
        {
            string userId = User.Identity.GetUserId();
            var attempt = db.UserTestAttempts
                          .Include(a => a.Test.Questions.Select(q => q.Answers))
                          .FirstOrDefault(a => a.Id == attemptId && a.UserId == userId);

            if (attempt == null || attempt.IsCompleted)
            {
                return RedirectToAction("Details", "Lesson", new { id = attempt.Test.LessonId });
            }

            int score = 0;
            int totalQuestions = attempt.Test.Questions.Count;

            // Lưu từng câu trả lời và tính điểm
            foreach (var question in attempt.Test.Questions)
            {
                string answerKey = "answer_" + question.Id;
                UserAnswer userAnswer;

                if (int.TryParse(form[answerKey], out int selectedAnswerId))
                {
                    var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == selectedAnswerId);
                    bool isCorrect = selectedAnswer?.IsCorrect ?? false;

                    if (isCorrect) score++;

                    userAnswer = new UserAnswer
                    {
                        UserTestAttemptId = attempt.Id,
                        QuestionId = question.Id,
                        SelectedAnswerId = selectedAnswerId,
                        IsCorrect = isCorrect
                    };
                }
                else
                {
                    // Người dùng không chọn câu trả lời
                    userAnswer = new UserAnswer
                    {
                        UserTestAttemptId = attempt.Id,
                        QuestionId = question.Id,
                        SelectedAnswerId = null, // Không chọn đáp án nào
                        IsCorrect = false // Không đúng vì không trả lời
                    };
                }

                db.UserAnswers.Add(userAnswer);
            }

            // Cập nhật trạng thái attempt
            attempt.EndTime = DateTime.Now;
            attempt.IsCompleted = true;
            attempt.Score = (int)Math.Round((double)score / totalQuestions * 100);
            attempt.AttemptDate = DateTime.Now;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = attempt.Id });
        }


        // GET: Test/Details/5 (Xem chi tiết bài kiểm tra)
        [Authorize]
        public ActionResult Details(int id)
        {
            string userId = User.Identity.GetUserId();
            var attempt = db.UserTestAttempts
                          .Include(a => a.Test)
                          .Include(a => a.UserAnswers.Select(ua => ua.Question))
                          .Include(a => a.UserAnswers.Select(ua => ua.SelectedAnswer))
                          .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (attempt == null)
            {
                return HttpNotFound();
            }
            var lesson = db.Lessons.Include(l => l.Category).FirstOrDefault(l => l.Id == attempt.Test.LessonId);
            var test = db.Tests.Include(t => t.Questions).FirstOrDefault(t => t.Id == id);
            if (!lesson.IsFreeTrial)
            {
                // Kiểm tra xem người dùng đã mua khóa học chứa bài học này chưa
                bool hasPurchased = db.Payments.Any(p =>
                p.UserId == userId &&
                    p.CategoryId == test.Lesson.CategoryId &&
                    p.Status == "Completed" &&
                    p.PaymentDate <= DateTime.Now);

                if (!hasPurchased)
                {
                    return RedirectToAction("AccessDenied", "Error", new { message = "Bạn cần mua khóa học này trước khi làm bài kiểm tra." });
                }
            }
            ViewBag.UrlBack = Request.UrlReferrer?.ToString();
            return View(attempt);
        }
    }
}