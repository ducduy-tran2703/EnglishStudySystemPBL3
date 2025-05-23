namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateConstrainstDB : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Comments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Payments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserAnswers", "QuestionId", "dbo.Questions");
            AddColumn("dbo.AspNetUsers", "CanManageUsers", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "CanManageCategories", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "CanManageNotifications", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Lessons", "CreatedByUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Lessons", "UpdatedByUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Categories", "CreatedByUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Categories", "UpdatedByUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Lessons", "CreatedByUserId");
            CreateIndex("dbo.Lessons", "UpdatedByUserId");
            CreateIndex("dbo.Categories", "CreatedByUserId");
            CreateIndex("dbo.Categories", "UpdatedByUserId");
            AddForeignKey("dbo.Categories", "CreatedByUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Categories", "UpdatedByUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Lessons", "CreatedByUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Lessons", "UpdatedByUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Comments", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Payments", "UserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.UserAnswers", "QuestionId", "dbo.Questions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAnswers", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.Payments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Comments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Lessons", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Lessons", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Categories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Categories", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Categories", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Categories", new[] { "CreatedByUserId" });
            DropIndex("dbo.Lessons", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Lessons", new[] { "CreatedByUserId" });
            AlterColumn("dbo.Categories", "UpdatedByUserId", c => c.String());
            AlterColumn("dbo.Categories", "CreatedByUserId", c => c.String(nullable: false));
            AlterColumn("dbo.Lessons", "UpdatedByUserId", c => c.String());
            AlterColumn("dbo.Lessons", "CreatedByUserId", c => c.String(nullable: false));
            DropColumn("dbo.AspNetUsers", "CanManageNotifications");
            DropColumn("dbo.AspNetUsers", "CanManageCategories");
            DropColumn("dbo.AspNetUsers", "CanManageUsers");
            AddForeignKey("dbo.UserAnswers", "QuestionId", "dbo.Questions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Payments", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Comments", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}
