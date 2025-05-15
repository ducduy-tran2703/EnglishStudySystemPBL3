using System; // Cần cho DateTime
using System.Collections.Generic; // Cần cho ICollection
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
// using static System.Net.Mime.MediaTypeNames.Application; // <-- Dòng này không cần thiết, đã được xóa

namespace EnglishStudySystem.Models
{
    // Bạn có thể thêm dữ liệu hồ sơ cho người dùng bằng cách thêm thuộc tính vào lớp ApplicationUser
    public class ApplicationUser : IdentityUser
    {
        // Các thuộc tính bổ sung cho người dùng (học viên, editor, admin)
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true; // Mặc định là kích hoạt
        public UserAccountStatus AccountStatus { get; set; } = UserAccountStatus.Normal; // Trạng thái tài khoản (Normal/VIP)

        // Thuộc tính điều hướng (Navigation properties)
        public virtual ICollection<SavedLesson> SavedLessons { get; set; }
        public virtual ICollection<LessonHistory> LessonHistories { get; set; }
        public virtual ICollection<Notification> SentNotifications { get; set; } // Thông báo được gửi bởi người dùng này
        public virtual ICollection<UserNotification> UserNotifications { get; set; } // Thông báo mà người dùng này nhận được (qua bảng trung gian)
        public virtual ICollection<Comment> Comments { get; set; } // <-- Thuộc tính này đã có sẵn và được giữ nguyên

        public virtual ICollection<UserPermission> UserPermissions { get; set; }
        // Nếu cần thêm các thuộc tính cho Admin/Editor, có thể thêm ở đây
        // Ví dụ: Public bool CanManageUsers { get; set; }
        // Public bool CanManageLessons { get; set; }

        public ApplicationUser()
        {
            // Khởi tạo các collections để tránh lỗi null reference
            SavedLessons = new HashSet<SavedLesson>();
            LessonHistories = new HashSet<LessonHistory>();
            SentNotifications = new HashSet<Notification>();
            UserNotifications = new HashSet<UserNotification>();
            Comments = new HashSet<Comment>(); // <-- Đảm bảo được khởi tạo tại đây
            UserPermissions = new HashSet<UserPermission>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Lưu ý rằng authenticationType phải khớp với loại được xác định trong CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Thêm các thuộc tính người dùng tùy chỉnh của bạn vào đây
            userIdentity.AddClaim(new Claim("FullName", this.FullName ?? string.Empty));
            userIdentity.AddClaim(new Claim("IsActive", this.IsActive.ToString()));
            userIdentity.AddClaim(new Claim("AccountStatus", this.AccountStatus.ToString()));
            return userIdentity;
        }
    }

    // Enum cho trạng thái tài khoản học viên
    public enum UserAccountStatus
    {
        Normal,
        VIP
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // Định nghĩa các bảng khác cho hệ thống học tiếng Anh
        public DbSet<Category> Categories { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SavedLesson> SavedLessons { get; set; }
        public DbSet<LessonHistory> LessonHistories { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserTestAttempt> UserTestAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; } // DbSet cho bảng trung gian UserNotification

        // --- Override SaveChanges cho Soft Delete ---
        public override int SaveChanges()
        {
            // Lấy tất cả các entry đang được đánh dấu là 'Deleted'
            var softDeletableEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDeletable);

            foreach (var entry in softDeletableEntries)
            {
                var entity = (ISoftDeletable)entry.Entity;
                entity.IsDeleted = true; // Đánh dấu là đã xóa mềm
                entity.DeletedAt = DateTime.Now; // Lưu thời gian xóa
                entry.State = EntityState.Modified; // Thay đổi trạng thái từ Deleted thành Modified
                                                    // Để Entity Framework thực hiện update thay vì delete
            }
            return base.SaveChanges();
        }

        // --- Async version của SaveChanges (quan trọng nếu bạn dùng async/await) ---
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var softDeletableEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDeletable);

            foreach (var entry in softDeletableEntries)
            {
                var entity = (ISoftDeletable)entry.Entity;
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.Now;
                entry.State = EntityState.Modified;
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        // MỚI: Thêm override này để hỗ trợ SaveChangesAsync() không tham số CancellationToken
        public override Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Quan hệ giữa payment-category
            modelBuilder.Entity<Payment>()
    .HasRequired(p => p.Category) // Mỗi Payment yêu cầu có một Category
    .WithMany() // Category có thể có nhiều Payments (Nếu không cần điều hướng ngược lại từ Category đến Payment)
                // .WithMany(c => c.Payments) // Nếu bạn thêm ICollection<Payment> Payments { get; set; } vào Category Model
    .HasForeignKey(p => p.CategoryId) // Khóa ngoại là CategoryId trong bảng Payment
    .WillCascadeOnDelete(false);

            // Cấu hình quan hệ cho SavedLesson (Người dùng - Bài học)
            modelBuilder.Entity<SavedLesson>()
                .HasRequired(sl => sl.User)
                .WithMany(u => u.SavedLessons)
                .HasForeignKey(sl => sl.UserId)
                .WillCascadeOnDelete(true); // Xóa tự động nếu người dùng bị xóa

            modelBuilder.Entity<SavedLesson>()
                .HasRequired(sl => sl.Lesson)
                .WithMany() // Không cần thuộc tính điều hướng ngược lại trong Lesson nếu không dùng
                .HasForeignKey(sl => sl.LessonId)
                .WillCascadeOnDelete(true); // Xóa tự động nếu bài học bị xóa

            // Cấu hình quan hệ cho LessonHistory (Người dùng - Bài học)
            modelBuilder.Entity<LessonHistory>()
                .HasRequired(lh => lh.User)
                .WithMany(u => u.LessonHistories)
                .HasForeignKey(lh => lh.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<LessonHistory>()
                .HasRequired(lh => lh.Lesson)
                .WithMany()
                .HasForeignKey(lh => lh.LessonId)
                .WillCascadeOnDelete(true);

            // Cấu hình quan hệ cho Comment (Người dùng - Bài học)
            modelBuilder.Entity<Comment>()
                .HasRequired(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false); // Không xóa tự động nếu người dùng bị xóa

            modelBuilder.Entity<Comment>()
                .HasRequired(c => c.Lesson)
                .WithMany(l => l.Comments)
                .HasForeignKey(c => c.LessonId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Comment>()
       .HasOptional(c => c.ParentComment) // Một Comment có thể có hoặc không có ParentComment (khóa ngoại ParentCommentId là nullable)
       .WithMany(c => c.Replies) // Một ParentComment có nhiều Comment con (Replies)
       .HasForeignKey(c => c.ParentCommentId) // Khóa ngoại là ParentCommentId trong bảng Comment
       .WillCascadeOnDelete(false); // QUAN TRỌNG: Không xóa các bình luận con nếu bình luận cha bị xóa.
                                    // Bạn cần xử lý việc này bằng logic ứng dụng (ví dụ: xóa mềm bình luận con hoặc gán ParentCommentId = null).

            // Cấu hình quan hệ cho Test (Bài học - Test)
            modelBuilder.Entity<Test>()
                .HasRequired(t => t.Lesson)
                .WithMany(l => l.Tests)
                .HasForeignKey(t => t.LessonId)
                .WillCascadeOnDelete(true);

            // Cấu hình quan hệ cho Question (Test - Question)
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .WillCascadeOnDelete(true);

            // Cấu hình quan hệ cho Answer (Question - Answer)
            modelBuilder.Entity<Answer>()
                .HasRequired(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .WillCascadeOnDelete(true);

            // Cấu hình quan hệ cho Payment (Người dùng - Payment)
            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.User)
                .WithMany() // Có thể thêm ICollection<Payment> trong ApplicationUser nếu cần điều hướng ngược lại
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(true);

            // Cấu hình quan hệ cho UserTestAttempt (Người dùng - Test)
            modelBuilder.Entity<UserTestAttempt>()
                .HasRequired(uta => uta.User)
                .WithMany() // Có thể thêm ICollection<UserTestAttempt> trong ApplicationUser
                .HasForeignKey(uta => uta.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UserTestAttempt>()
                .HasRequired(uta => uta.Test)
                .WithMany() // Có thể thêm ICollection<UserTestAttempt> trong Test
                .HasForeignKey(uta => uta.TestId)
                .WillCascadeOnDelete(false);

            // Cấu hình quan hệ cho UserAnswer (UserTestAttempt - Question - Answer)
            modelBuilder.Entity<UserAnswer>()
                .HasRequired(ua => ua.UserTestAttempt)
                .WithMany(uta => uta.UserAnswers)
                .HasForeignKey(ua => ua.UserTestAttemptId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UserAnswer>()
                .HasRequired(ua => ua.Question)
                .WithMany() // Có thể thêm ICollection<UserAnswer> trong Question
                .HasForeignKey(ua => ua.QuestionId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UserAnswer>()
                .HasRequired(ua => ua.SelectedAnswer)
                .WithMany() // Có thể thêm ICollection<UserAnswer> trong Answer
                .HasForeignKey(ua => ua.SelectedAnswerId)
                .WillCascadeOnDelete(false); // Không xóa câu trả lời nếu UserAnswer bị xóa

            // Cấu hình quan hệ cho UserPermission (Người dùng - Permission)
            modelBuilder.Entity<UserPermission>()
                .HasRequired(up => up.User)
                .WithMany(u => u.UserPermissions) // Thêm ICollection<UserPermission> UserPermissions { get; set; } vào ApplicationUser
                .HasForeignKey(up => up.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<UserPermission>()
                .HasRequired(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .WillCascadeOnDelete(true);

            // MỚI: Cấu hình quan hệ cho Notification (NGƯỜI GỬI - Sender)
            // Một Notification có một Sender (Editor/Admin)
            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.Sender) // Mỗi Notification phải có một người gửi
                .WithMany(u => u.SentNotifications) // Một người dùng có thể gửi nhiều Notification
                .HasForeignKey(n => n.SenderId) // Khóa ngoại là SenderId trong bảng Notification
                .WillCascadeOnDelete(false); // Không xóa thông báo nếu người gửi bị xóa (thường muốn giữ lại lịch sử)

            // MỚI: Cấu hình quan hệ nhiều-nhiều cho Notification và ApplicationUser (NGƯỜI NHẬN - Recipients) thông qua bảng UserNotification
            // 1. Notification <-> UserNotification (Một Notification có nhiều UserNotification)
            modelBuilder.Entity<UserNotification>()
                .HasRequired(un => un.Notification) // Mỗi UserNotification phải thuộc về một Notification
                .WithMany(n => n.UserNotifications) // Một Notification có nhiều UserNotification
                .HasForeignKey(un => un.NotificationId) // Khóa ngoại là NotificationId
                .WillCascadeOnDelete(true); // Nếu Notification bị xóa cứng, các liên kết UserNotification của nó cũng bị xóa

            // 2. ApplicationUser <-> UserNotification (Một ApplicationUser nhận nhiều UserNotification)
            modelBuilder.Entity<UserNotification>()
                .HasRequired(un => un.User) // Mỗi UserNotification phải thuộc về một User (người nhận)
                .WithMany(u => u.UserNotifications) // Một User có nhiều UserNotification
                .HasForeignKey(un => un.UserId) // Khóa ngoại là UserId
                .WillCascadeOnDelete(false); // Quan trọng: Không xóa UserNotification nếu User bị xóa (để tránh xóa User)

            // Lưu ý: Dòng này có vẻ không đúng nếu UserViewModel chỉ là một ViewModel để hiển thị.
            // Dbset chỉ nên dùng cho các Entity Model ánh xạ tới bảng trong database.
            // Nếu UserViewModel không phải là một Entity Model, hãy xóa dòng này.
            // public System.Data.Entity.DbSet<EnglishStudySystem.Models.UserViewModel> UserViewModels { get; set; }
        }
    }
}