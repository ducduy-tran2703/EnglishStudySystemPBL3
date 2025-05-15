namespace EnglishStudySystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSelectedAnswerIdToNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserAnswers", "SelectedAnswerId", c => c.Int()); // Perment null
                                                                              // Có thể có các thay đổi khác nếu bạn có những thay đổi model khác
        }

        public override void Down()
        {
            AlterColumn("dbo.UserAnswers", "SelectedAnswerId", c => c.Int(nullable: false)); // Revert to not null
                                                                                             // Có thể có các thay đổi khác
        }
    }
}
