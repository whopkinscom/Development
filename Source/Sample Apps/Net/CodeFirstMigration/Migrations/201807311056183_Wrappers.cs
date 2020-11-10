using System.Data.Entity.Migrations;
using System.Linq;
using Moonrise.Utils.Database.Migration;
using SampleDomainModel;

namespace CodeFirstMigration
{
    public partial class Wrappers : DbMigration, ISeededMigration<SweetContext4Migration>
    {
        public override void Down()
        {
            DropColumn("dbo.Chocolates", "HasWrapper");
        }

        public void Seed(SweetContext4Migration context)
        {
            IQueryable<Chocolate> chocs = from choc in context.Chocolates
                                          select choc;

            foreach (Chocolate chocolate in chocs)
            {
                if (chocolate.Name != "Caramel")
                {
                    chocolate.HasWrapper = true;
                }
            }

            context.SaveChanges();
        }

        public override void Up()
        {
            AddColumn("dbo.Chocolates", "HasWrapper", c => c.Boolean(false));
            SweetContextConfiguration.AddSeedingMigration(this);
        }
    }
}
