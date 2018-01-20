namespace ConsoleApp7.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class second : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Wiadomoscs", "MessageID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Wiadomoscs", "MessageID");
        }
    }
}
