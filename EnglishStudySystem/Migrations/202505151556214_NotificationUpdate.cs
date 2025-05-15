namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "TargetController", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Notifications", "TargetAction", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Notifications", "TargetArea", c => c.String(maxLength: 50));
            AddColumn("dbo.Notifications", "PrimaryRelatedEntityId", c => c.Int());
            AddColumn("dbo.Notifications", "SecondaryRelatedEntityId", c => c.Int());
            AlterColumn("dbo.Notifications", "RelatedEntityType", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.Notifications", "RelatedCommentId");
            DropColumn("dbo.Notifications", "RelatedLessonId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "RelatedLessonId", c => c.Int());
            AddColumn("dbo.Notifications", "RelatedCommentId", c => c.Int());
            AlterColumn("dbo.Notifications", "RelatedEntityType", c => c.String(maxLength: 50));
            DropColumn("dbo.Notifications", "SecondaryRelatedEntityId");
            DropColumn("dbo.Notifications", "PrimaryRelatedEntityId");
            DropColumn("dbo.Notifications", "TargetArea");
            DropColumn("dbo.Notifications", "TargetAction");
            DropColumn("dbo.Notifications", "TargetController");
        }
    }
}
