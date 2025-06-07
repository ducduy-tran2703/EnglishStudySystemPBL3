namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDurationColumnInTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tests", "Duration", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tests", "Duration");
        }
    }
}
