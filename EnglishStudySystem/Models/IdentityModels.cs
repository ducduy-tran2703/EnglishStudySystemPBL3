using System; // Thêm dòng này cho DateTime
using System.Collections.Generic; // Thêm dòng này cho ICollection
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using static System.Net.Mime.MediaTypeNames;

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
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<UserPermission> UserPermissions { get; set; }
        // Nếu cần thêm các thuộc tính cho Admin/Editor, có thể thêm ở đây
        // Ví dụ: Public bool CanManageUsers { get; set; }
        // Public bool CanManageLessons { get; set; }

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
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Cấu hình quan hệ cho Notification (Người dùng - Notification)
            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
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
        }

        public System.Data.Entity.DbSet<EnglishStudySystem.Models.UserViewModel> UserViewModels { get; set; }
    }
}