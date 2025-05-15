namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddParentCommentIdToComments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "ParentCommentId", c => c.Int());
            AddColumn("dbo.Notifications", "RelatedCommentId", c => c.Int());
            AddColumn("dbo.Notifications", "RelatedLessonId", c => c.Int());
            AddColumn("dbo.Notifications", "RelatedEntityType", c => c.String(maxLength: 50));
            AlterColumn("dbo.Categories", "CreatedByUserRole", c => c.String(maxLength: 15));
            AlterColumn("dbo.Categories", "UpdatedByUserRole", c => c.String(maxLength: 15));
            CreateIndex("dbo.Comments", "ParentCommentId");
            AddForeignKey("dbo.Comments", "ParentCommentId", "dbo.Comments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "ParentCommentId", "dbo.Comments");
            DropIndex("dbo.Comments", new[] { "ParentCommentId" });
            AlterColumn("dbo.Categories", "UpdatedByUserRole", c => c.String(maxLength: 10));
            AlterColumn("dbo.Categories", "CreatedByUserRole", c => c.String(maxLength: 10));
            DropColumn("dbo.Notifications", "RelatedEntityType");
            DropColumn("dbo.Notifications", "RelatedLessonId");
            DropColumn("dbo.Notifications", "RelatedCommentId");
            DropColumn("dbo.Comments", "ParentCommentId");
        }
    }
}
