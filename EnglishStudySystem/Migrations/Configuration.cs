namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using EnglishStudySystem.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity;

    internal sealed class Configuration : DbMigrationsConfiguration<EnglishStudySystem.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

protected override void Seed(EnglishStudySystem.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            // 1. Tạo các vai trò (Roles) nếu chúng chưa tồn tại
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists("Administrator"))
            {
                roleManager.Create(new IdentityRole("Administrator"));
            }
            if (!roleManager.RoleExists("Editor"))
            {
                roleManager.Create(new IdentityRole("Editor"));
            }
            if (!roleManager.RoleExists("Customer"))
            {
                roleManager.Create(new IdentityRole("Customer"));
            }

            // 2. Tạo tài khoản Administrator ban đầu nếu chưa tồn tại
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Kiểm tra xem tài khoản Administrator có tồn tại chưa
            if (userManager.FindByName("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    FullName = "Administrator System", // Đặt FullName cho admin
                    IsActive = true,
                    AccountStatus = UserAccountStatus.VIP // Admin có thể coi là VIP hoặc trạng thái riêng
                };

                // Tạo người dùng với mật khẩu
                var result = userManager.Create(adminUser, "Admin@123"); // Thay đổi mật khẩu mặc định này!

                if (result.Succeeded)
                {
                    // Gán vai trò "Administrator" cho người dùng này
                    userManager.AddToRole(adminUser.Id, "Administrator");
                }
            }

            // 3. Thêm các quyền ban đầu (Permissions) nếu chưa tồn tại
            // Các quyền này sẽ được dùng để gán cho Editor
            context.Permissions.AddOrUpdate(
                p => p.Name,
                new Models.Permission { Name = "ManageUsers", Description = "Cho phép quản lý tài khoản người dùng (học viên và editor)." },
                new Models.Permission { Name = "ManageCategories", Description = "Cho phép quản lý danh mục bài học." },
                new Models.Permission { Name = "ManageLessons", Description = "Cho phép quản lý các bài học." },
                new Models.Permission { Name = "ManagePayments", Description = "Cho phép quản lý các giao dịch thanh toán." },
                new Models.Permission { Name = "ManageTests", Description = "Cho phép quản lý các bài kiểm tra." },
                new Models.Permission { Name = "ViewStatistics", Description = "Cho phép xem các báo cáo thống kê." }
            );

            context.SaveChanges(); // Lưu thay đổi để các quyền được thêm vào DB trước khi sử dụng
        }
    }
}
