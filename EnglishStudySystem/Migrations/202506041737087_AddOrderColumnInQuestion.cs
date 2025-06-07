namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderColumnInQuestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "Order", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "Order");
        }
    }
}
