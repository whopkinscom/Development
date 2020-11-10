using System.Data.Entity.Migrations;

namespace CodeFirstMigration
{
    public partial class uncertain : DbMigration
    {
        public override void Down()
        {
            DropColumn("dbo.Chocolates", "Dark");
        }

        public override void Up()
        {
            AddColumn("dbo.Chocolates", "Dark", c => c.Boolean(false));
        }
    }
}
