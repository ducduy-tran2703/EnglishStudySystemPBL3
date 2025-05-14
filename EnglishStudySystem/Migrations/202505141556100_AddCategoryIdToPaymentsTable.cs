namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryIdToPaymentsTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "CategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.Payments", "CategoryId");
            AddForeignKey("dbo.Payments", "CategoryId", "dbo.Categories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Payments", new[] { "CategoryId" });
            DropColumn("dbo.Payments", "CategoryId");
        }
    }
}
