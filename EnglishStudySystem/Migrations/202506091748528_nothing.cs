namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nothing : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Payments", new[] { "UserId" });
            AlterColumn("dbo.Payments", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Payments", "UserId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Payments", new[] { "UserId" });
            AlterColumn("dbo.Payments", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Payments", "UserId");
        }
    }
}
