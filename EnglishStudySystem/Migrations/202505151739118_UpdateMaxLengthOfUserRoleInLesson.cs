namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMaxLengthOfUserRoleInLesson : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Lessons", "CreatedByUserRole", c => c.String(maxLength: 15));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Lessons", "CreatedByUserRole", c => c.String(maxLength: 10));
        }
    }
}
