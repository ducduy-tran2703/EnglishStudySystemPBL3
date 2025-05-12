namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLessonModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tests", "QuestionCount", c => c.Int(nullable: false));
            AddColumn("dbo.Tests", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Tests", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Tests", "DeletedAt", c => c.DateTime());
            AddColumn("dbo.Lessons", "Description", c => c.String());
            AddColumn("dbo.Lessons", "Video_URL", c => c.String());
            AddColumn("dbo.Lessons", "CreatedByUserId", c => c.String(nullable: false));
            AddColumn("dbo.Lessons", "CreatedByUserRole", c => c.String(maxLength: 10));
            AddColumn("dbo.Lessons", "UpdatedByUserId", c => c.String());
            AddColumn("dbo.Lessons", "UpdatedByUserRole", c => c.String(maxLength: 10));
            AddColumn("dbo.Lessons", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Lessons", "DeletedAt", c => c.DateTime());
            AddColumn("dbo.Comments", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Comments", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "DeletedAt", c => c.DateTime());
            AlterColumn("dbo.Comments", "Content", c => c.String(nullable: false));
            DropColumn("dbo.Tests", "DurationMinutes");
            DropColumn("dbo.Lessons", "Content");
            DropColumn("dbo.Lessons", "IsPublic");
            DropColumn("dbo.Lessons", "Views");
            DropColumn("dbo.Comments", "CommentDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Comments", "CommentDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Lessons", "Views", c => c.Int(nullable: false));
            AddColumn("dbo.Lessons", "IsPublic", c => c.Boolean(nullable: false));
            AddColumn("dbo.Lessons", "Content", c => c.String(nullable: false));
            AddColumn("dbo.Tests", "DurationMinutes", c => c.Int(nullable: false));
            AlterColumn("dbo.Comments", "Content", c => c.String(nullable: false, maxLength: 1000));
            DropColumn("dbo.Comments", "DeletedAt");
            DropColumn("dbo.Comments", "IsDeleted");
            DropColumn("dbo.Comments", "CreatedDate");
            DropColumn("dbo.Lessons", "DeletedAt");
            DropColumn("dbo.Lessons", "IsDeleted");
            DropColumn("dbo.Lessons", "UpdatedByUserRole");
            DropColumn("dbo.Lessons", "UpdatedByUserId");
            DropColumn("dbo.Lessons", "CreatedByUserRole");
            DropColumn("dbo.Lessons", "CreatedByUserId");
            DropColumn("dbo.Lessons", "Video_URL");
            DropColumn("dbo.Lessons", "Description");
            DropColumn("dbo.Tests", "DeletedAt");
            DropColumn("dbo.Tests", "IsDeleted");
            DropColumn("dbo.Tests", "CreatedDate");
            DropColumn("dbo.Tests", "QuestionCount");
        }
    }
}
