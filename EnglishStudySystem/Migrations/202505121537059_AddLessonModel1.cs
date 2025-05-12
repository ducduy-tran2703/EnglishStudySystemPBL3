namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLessonModel1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            RenameColumn(table: "dbo.Notifications", name: "UserId", newName: "SenderId");
            RenameIndex(table: "dbo.Notifications", name: "IX_UserId", newName: "IX_SenderId");
            CreateTable(
                "dbo.UserNotifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        NotificationId = c.Int(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Notifications", t => t.NotificationId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.NotificationId);
            
            AddColumn("dbo.Notifications", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Notifications", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Notifications", "DeletedAt", c => c.DateTime());
            AddForeignKey("dbo.Notifications", "SenderId", "dbo.AspNetUsers", "Id");
            DropColumn("dbo.Notifications", "SentDate");
            DropColumn("dbo.Notifications", "IsRead");
            DropTable("dbo.UserViewModels");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserViewModels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        Email = c.String(),
                        FullName = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        AccountStatus = c.Int(nullable: false),
                        Roles = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Notifications", "IsRead", c => c.Boolean(nullable: false));
            AddColumn("dbo.Notifications", "SentDate", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.Notifications", "SenderId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserNotifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserNotifications", "NotificationId", "dbo.Notifications");
            DropIndex("dbo.UserNotifications", new[] { "NotificationId" });
            DropIndex("dbo.UserNotifications", new[] { "UserId" });
            DropColumn("dbo.Notifications", "DeletedAt");
            DropColumn("dbo.Notifications", "IsDeleted");
            DropColumn("dbo.Notifications", "CreatedDate");
            DropTable("dbo.UserNotifications");
            RenameIndex(table: "dbo.Notifications", name: "IX_SenderId", newName: "IX_UserId");
            RenameColumn(table: "dbo.Notifications", name: "SenderId", newName: "UserId");
            AddForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
