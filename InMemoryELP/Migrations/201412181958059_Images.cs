namespace InMemoryELP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Images : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stories", "ImageUrls", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stories", "ImageUrls");
        }
    }
}
