namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Lessons", "UpdatedByUserRole", c => c.String(maxLength: 15));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Lessons", "UpdatedByUserRole", c => c.String(maxLength: 10));
        }
    }
}
