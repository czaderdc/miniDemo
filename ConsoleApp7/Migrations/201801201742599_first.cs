namespace ConsoleApp7.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Wiadomoscs",
                c => new
                    {
                        WiadomoscID = c.Int(nullable: false, identity: true),
                        Nadawca = c.String(),
                        MailNadawcy = c.String(),
                        Temat = c.String(),
                        TrescWiadomoscTempiTekst = c.String(),
                        TrescWiadomoscTempiHTML = c.String(),
                    })
                .PrimaryKey(t => t.WiadomoscID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Wiadomoscs");
        }
    }
}
