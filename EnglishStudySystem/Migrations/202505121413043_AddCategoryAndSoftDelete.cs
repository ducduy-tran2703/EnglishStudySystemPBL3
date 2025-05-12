namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryAndSoftDelete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "Price", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Categories", "CreatedByUserId", c => c.String(nullable: false));
            AddColumn("dbo.Categories", "CreatedByUserRole", c => c.String(maxLength: 10));
            AddColumn("dbo.Categories", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Categories", "UpdatedByUserId", c => c.String());
            AddColumn("dbo.Categories", "UpdatedByUserRole", c => c.String(maxLength: 10));
            AddColumn("dbo.Categories", "UpdatedDate", c => c.DateTime());
            AddColumn("dbo.Categories", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "DeletedAt", c => c.DateTime());
            AlterColumn("dbo.Categories", "Description", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Categories", "Description", c => c.String(maxLength: 1000));
            DropColumn("dbo.Categories", "DeletedAt");
            DropColumn("dbo.Categories", "IsDeleted");
            DropColumn("dbo.Categories", "UpdatedDate");
            DropColumn("dbo.Categories", "UpdatedByUserRole");
            DropColumn("dbo.Categories", "UpdatedByUserId");
            DropColumn("dbo.Categories", "CreatedDate");
            DropColumn("dbo.Categories", "CreatedByUserRole");
            DropColumn("dbo.Categories", "CreatedByUserId");
            DropColumn("dbo.Categories", "Price");
        }
    }
}
