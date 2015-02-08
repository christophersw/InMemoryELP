namespace InMemoryELP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stories", "AuthorName", c => c.String());
            AddColumn("dbo.Stories", "AuthorEmail", c => c.String());
            AddColumn("dbo.Stories", "Public", c => c.Boolean(nullable: false));
            AddColumn("dbo.Stories", "Approved", c => c.Boolean(nullable: false));
            DropColumn("dbo.Stories", "LeadImageURL");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Stories", "LeadImageURL", c => c.String());
            DropColumn("dbo.Stories", "Approved");
            DropColumn("dbo.Stories", "Public");
            DropColumn("dbo.Stories", "AuthorEmail");
            DropColumn("dbo.Stories", "AuthorName");
        }
    }
}
