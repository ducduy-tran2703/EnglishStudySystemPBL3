using EnglishStudySystem;
using EnglishStudySystem.Models;
using EnglishStudySystem.ViewModel;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity; 
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc; 

namespace EnglishStudySystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator, Editor")] 
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public TestController()
        {
            _dbContext = new ApplicationDbContext();
        }

        public async Task<ActionResult> Index(int lessonId)
        {
            var lesson = await _dbContext.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                return HttpNotFound();
            }

            ViewBag.LessonId = lessonId;
            ViewBag.LessonTitle = lesson.Title;

            var tests = await _dbContext.Tests
                                        .Where(t => t.LessonId == lessonId)
                                        .Select(t => new TestDetailViewModel
                                        {
                                            Id = t.Id,
                                            Title = t.Title,
                                            Description = t.Description,
                                            LessonTitle = t.Lesson.Title // Lấy tên bài học
                                        })
                                        .ToListAsync();

            return View(tests);
        }

        // GET: Admin/Test/Create?lessonId={lessonId}
        public async Task<ActionResult> Create(int lessonId)
        {
            var lesson = await _dbContext.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                return HttpNotFound();
            }

            ViewBag.LessonId = lessonId;
            ViewBag.LessonTitle = lesson.Title;

            var model = new TestCreateEditViewModel
            {
                LessonId = lessonId,
                Duration = 30, // Giá trị mặc định cho Thời gian làm bài (có thể thay đổi)
                Questions = new System.Collections.Generic.List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Answers = new System.Collections.Generic.List<AnswerViewModel>
                        {
                            new AnswerViewModel(), new AnswerViewModel(), new AnswerViewModel(), new AnswerViewModel()
                        }
                    }
                }
            };

            return View(model);
        }

        // POST: Admin/Test/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TestCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Logic kiểm tra số lượng câu hỏi và đáp án tối thiểu/tối đa
                if (model.Questions == null || model.Questions.Count == 0)
                {
                    ModelState.AddModelError("", "Bài kiểm tra phải có ít nhất một câu hỏi.");
                }
                else
                {
                    // Kiểm tra chi tiết hơn từng câu hỏi/đáp án
                    foreach (var qVm in model.Questions)
                    {
                        if (string.IsNullOrWhiteSpace(qVm.Content))
                        {
                            ModelState.AddModelError("", "Nội dung câu hỏi không được để trống.");
                            break;
                        }
                        if (qVm.Answers == null || qVm.Answers.Count != 4)
                        {
                            ModelState.AddModelError("", "Mỗi câu hỏi phải có đúng 4 đáp án.");
                            break;
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            if (string.IsNullOrWhiteSpace(qVm.Answers[i].Content))
                            {
                                ModelState.AddModelError("", "Nội dung đáp án không được để trống.");
                                break;
                            }
                        }
                    }
                }

                // Kiểm tra Duration
                if (model.Duration <= 0)
                {
                    ModelState.AddModelError("Duration", "Thời gian làm bài phải lớn hơn 0 phút.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.LessonId = model.LessonId;
                var lesson = await _dbContext.Lessons.FindAsync(model.LessonId);
                ViewBag.LessonTitle = lesson?.Title;
                return View(model);
            }

            try
            {
                var test = new Test
                {
                    Title = model.Title,
                    Description = model.Description,
                    LessonId = model.LessonId,
                    QuestionCount = model.Questions.Count, 
                    Duration = model.Duration,
                    CreatedDate = DateTime.Now
                };

                // Ánh xạ QuestionViewModel sang Question Model
                int questionOrder = 1;
                foreach (var qVm in model.Questions)
                {
                    var question = new Question
                    {
                        Content = qVm.Content,
                        Order = questionOrder++,
                        IsCorrectAnswer = qVm.IsCorrectAnswer,
                    };

                    // Ánh xạ AnswerViewModel sang Answer Model
                    for (int i = 0; i < qVm.Answers.Count; i++)
                    {
                        var aVm = qVm.Answers[i];
                        var answer = new Answer
                        {
                            Content = aVm.Content,
                            IsCorrect = (i == qVm.IsCorrectAnswer)
                        };
                        question.Answers.Add(answer);
                    }
                    test.Questions.Add(question);
                }

                _dbContext.Tests.Add(test);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bài kiểm tra đã được tạo thành công.";
                return RedirectToAction("Details", "Lessons", new { id = model.LessonId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating test: {ex.Message} - Inner: {ex.InnerException?.Message}");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài kiểm tra. Vui lòng thử lại.");
                ViewBag.LessonId = model.LessonId;
                var lesson = await _dbContext.Lessons.FindAsync(model.LessonId);
                ViewBag.LessonTitle = lesson?.Title;
                return View(model);
            }
        }

        // GET: Admin/Test/Details/{id}
        public async Task<ActionResult> Details(int id)
        {
            var test = await _dbContext.Tests
                                       .Include(t => t.Lesson)
                                       .Include(t => t.Questions.Select(q => q.Answers)) // Eager load Questions và Answers
                                       .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return HttpNotFound();
            }

            var model = new TestDetailViewModel
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,
                LessonId = test.LessonId,
                LessonTitle = test.Lesson.Title,
                QuestionCount = test.QuestionCount,
                Duration = test.Duration,
                Questions = test.Questions
                                .OrderBy(q => q.Order)
                                .Select(q => new QuestionDetailViewModel
                                {
                                    Id = q.Id,
                                    Content = q.Content,
                                    Order = q.Order,
                                    CorrectAnswerIndex = q.IsCorrectAnswer,
                                    Answers = q.Answers.Select(a => new AnswerDetailViewModel
                                    {
                                        Id = a.Id,
                                        Content = a.Content,
                                        IsCorrect = a.IsCorrect
                                    }).ToList()
                                }).ToList()
            };
            ViewBag.LessonId = test.LessonId;
            return View(model);
        }

        // GET: Admin/Test/Edit/{id}
        public async Task<ActionResult> Edit(int id)
        {
            var test = await _dbContext.Tests
                                       .Include(t => t.Lesson)
                                       .Include(t => t.Questions.Select(q => q.Answers))
                                       .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return HttpNotFound();
            }

            ViewBag.LessonId = test.LessonId;
            ViewBag.LessonTitle = test.Lesson.Title;

            var model = new TestCreateEditViewModel
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,
                LessonId = test.LessonId,
                Duration = test.Duration, // Hiển thị thời gian làm bài hiện tại
                Questions = test.Questions
                                .OrderBy(q => q.Order)
                                .Select(q => new QuestionViewModel
                                {
                                    Id = q.Id,
                                    Content = q.Content,
                                    IsCorrectAnswer = q.IsCorrectAnswer,
                                    Answers = q.Answers.Select(a => new AnswerViewModel
                                    {
                                        Id = a.Id,
                                        Content = a.Content
                                    }).ToList()
                                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/Test/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TestCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra validation logic cho câu hỏi/đáp án
                if (model.Questions == null || model.Questions.Count == 0)
                {
                    ModelState.AddModelError("", "Bài kiểm tra phải có ít nhất một câu hỏi.");
                }
                else
                {
                    foreach (var qVm in model.Questions)
                    {
                        if (string.IsNullOrWhiteSpace(qVm.Content))
                        {
                            ModelState.AddModelError("", "Nội dung câu hỏi không được để trống.");
                            break;
                        }
                        if (qVm.Answers == null || qVm.Answers.Count != 4)
                        {
                            ModelState.AddModelError("", "Mỗi câu hỏi phải có đúng 4 đáp án.");
                            break;
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            if (string.IsNullOrWhiteSpace(qVm.Answers[i].Content))
                            {
                                ModelState.AddModelError("", "Nội dung đáp án không được để trống.");
                                break;
                            }
                        }
                    }
                }

                // Kiểm tra Duration
                if (model.Duration <= 0)
                {
                    ModelState.AddModelError("Duration", "Thời gian làm bài phải lớn hơn 0 phút.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.LessonId = model.LessonId;
                var lesson = await _dbContext.Lessons.FindAsync(model.LessonId);
                ViewBag.LessonTitle = lesson?.Title;
                return View(model);
            }

            try
            {
                var existingTest = await _dbContext.Tests
                                                   .Include(t => t.Questions.Select(q => q.Answers))
                                                   .FirstOrDefaultAsync(t => t.Id == model.Id);

                if (existingTest == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin Test chính
                existingTest.Title = model.Title;
                existingTest.Description = model.Description;
                existingTest.QuestionCount = model.Questions.Count; // Tự động xác định từ số lượng câu hỏi
                existingTest.Duration = model.Duration;

                var questionIdsToKeep = model.Questions.Where(q => q.Id > 0).Select(q => q.Id).ToList();
                var questionsToRemove = existingTest.Questions.Where(q => !questionIdsToKeep.Contains(q.Id)).ToList();
                _dbContext.Questions.RemoveRange(questionsToRemove);

                // 2. Cập nhật hoặc thêm mới Questions và Answers
                int questionOrder = 1;
                foreach (var qVm in model.Questions)
                {
                    Question question;
                    if (qVm.Id > 0) 
                    {
                        question = existingTest.Questions.FirstOrDefault(q => q.Id == qVm.Id);
                        if (question != null)
                        {
                            question.Content = qVm.Content;
                            question.Order = questionOrder;
                            question.IsCorrectAnswer = qVm.IsCorrectAnswer;

                            // Xóa các đáp án không còn tồn tại cho câu hỏi này
                            var answerIdsToKeep = qVm.Answers.Where(a => a.Id > 0).Select(a => a.Id).ToList();
                            var answersToRemove = question.Answers.Where(a => !answerIdsToKeep.Contains(a.Id)).ToList();
                            _dbContext.Answers.RemoveRange(answersToRemove);

                            // Cập nhật hoặc thêm mới Answers
                            for (int i = 0; i < qVm.Answers.Count; i++)
                            {
                                var aVm = qVm.Answers[i];
                                Answer answer;
                                if (aVm.Id > 0) // Cập nhật đáp án hiện có
                                {
                                    answer = question.Answers.FirstOrDefault(a => a.Id == aVm.Id);
                                    if (answer != null)
                                    {
                                        answer.Content = aVm.Content;
                                        answer.IsCorrect = (i == qVm.IsCorrectAnswer);
                                    }
                                }
                                else // Thêm mới đáp án
                                {
                                    answer = new Answer
                                    {
                                        Content = aVm.Content,
                                        IsCorrect = (i == qVm.IsCorrectAnswer),
                                        QuestionId = question.Id
                                    };
                                    question.Answers.Add(answer);
                                }
                            }
                        }
                    }
                    else // Thêm mới câu hỏi
                    {
                        question = new Question
                        {
                            Content = qVm.Content,
                            Order = questionOrder,
                            IsCorrectAnswer = qVm.IsCorrectAnswer,
                        };
                        for (int i = 0; i < qVm.Answers.Count; i++)
                        {
                            var aVm = qVm.Answers[i];
                            question.Answers.Add(new Answer
                            {
                                Content = aVm.Content,
                                IsCorrect = (i == qVm.IsCorrectAnswer)
                            });
                        }
                        existingTest.Questions.Add(question);
                    }
                    questionOrder++;
                }

                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bài kiểm tra đã được cập nhật thành công.";
                return RedirectToAction("Details", "Lessons", new { id = existingTest.LessonId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating test: {ex.Message} - Inner: {ex.InnerException?.Message}");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật bài kiểm tra. Vui lòng thử lại.");
                ViewBag.LessonId = model.LessonId;
                var lesson = await _dbContext.Lessons.FindAsync(model.LessonId);
                ViewBag.LessonTitle = lesson?.Title;
                return View(model);
            }
        }

        // POST: Admin/Test/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var test = await _dbContext.Tests
                                       .Include(t => t.Questions.Select(q => q.Answers)) // Eager load để xóa cascading
                                       .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return HttpNotFound();
            }

            var lessonId = test.LessonId; // Lưu LessonId để redirect

            try
            {
                // Xóa tất cả Answers liên quan
                foreach (var question in test.Questions)
                {
                    _dbContext.Answers.RemoveRange(question.Answers);
                }
                // Xóa tất cả Questions liên quan
                _dbContext.Questions.RemoveRange(test.Questions);
                // Xóa Test
                _dbContext.Tests.Remove(test);

                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bài kiểm tra đã được xóa thành công.";
                return RedirectToAction("Details", "Lesson", new { id = lessonId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting test: {ex.Message} - Inner: {ex.InnerException?.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa bài kiểm tra. Vui lòng thử lại.";
                return RedirectToAction("Details", "Lesson", new { id = lessonId });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
        // POST: Admin/Test/SoftDelete/{id}
        [HttpPost, ActionName("SoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SoftDelete(int id)
        {
            var test = await _dbContext.Tests
                                       .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return HttpNotFound();
            }

            var lessonId = test.LessonId; // Lưu LessonId để redirect

            try
            {
                // Thực hiện xóa mềm
                test.IsDeleted = true;
                test.DeletedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bài kiểm tra đã được xóa mềm thành công.";
                return RedirectToAction("Details", "Lessons", new { id = lessonId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error soft deleting test: {ex.Message} - Inner: {ex.InnerException?.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa mềm bài kiểm tra. Vui lòng thử lại.";
                return RedirectToAction("Details", "Lessons", new { id = lessonId });
            }
        }
    }
}