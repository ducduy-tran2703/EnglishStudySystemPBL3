namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateClass_IsfreeTrial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lessons", "IsFreeTrial", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lessons", "IsFreeTrial");
        }
    }
}
