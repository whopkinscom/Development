using System.Collections.Generic;
using System.Data.Entity.Migrations;
using Moonrise.Logging;
using Moonrise.Utils.Database.Migration;
using SampleDomainModel;

namespace CodeFirstMigration
{
    public partial class Test : DbMigration, ISeededMigration<SweetContext4Migration>
    {
        public override void Down()
        {
            DropForeignKey("dbo.Chocolates", "ChocolateBox_Id", "dbo.ChocolateBoxes");
            DropIndex("dbo.Chocolates", new[] {"ChocolateBox_Id"});
            DropTable("dbo.Chocolates");
            DropTable("dbo.ChocolateBoxes");
        }

        public void Seed(SweetContext4Migration context)
        {
            ChocolateBox box = context.ChocolateBoxes.Add(
                new ChocolateBox
                {
                    Name = "Milk Tray",
                    Wrapper = "Flowery",
                    Contents = new List<Chocolate>
                               {
                                   new Chocolate
                                   {
                                       Name = "Caramel",
                                       Filling = "Hard"
                                   },
                                   new Chocolate
                                   {
                                       Name = "Strawberry Squish",
                                       Filling = "Soft"
                                   },
                                   new Chocolate
                                   {
                                       Name = "Turkish Delight",
                                       Filling = "Jelly"
                                   }
                               }
                });

            context.SaveChanges();
        }

        public override void Up()
        {
            Logger.Info("Test migration - Up");
            CreateTable(
                    "dbo.ChocolateBoxes",
                    c => new
                         {
                             Id = c.Long(false, true),
                             Name = c.String()
                         })
                .PrimaryKey(t => t.Id);

            CreateTable(
                    "dbo.Chocolates",
                    c => new
                         {
                             Id = c.Long(false, true),
                             Name = c.String(),
                             Filling = c.String(),
                             ChocolateBox_Id = c.Long()
                         })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChocolateBoxes", t => t.ChocolateBox_Id)
                .Index(t => t.ChocolateBox_Id);

            SweetContextConfiguration.AddSeedingMigration(this);
        }
    }
}
