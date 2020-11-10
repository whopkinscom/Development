using System.Data.Entity.Migrations;

namespace CodeFirstMigration
{
    public partial class WrapperAdded : DbMigration
    {
        public override void Down()
        {
            DropColumn("dbo.ChocolateBoxes", "Wrapper");
        }

        public override void Up()
        {
            AddColumn("dbo.ChocolateBoxes", "Wrapper", c => c.String());
        }
    }
}
