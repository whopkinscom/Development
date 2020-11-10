using System.Data.Entity;
using SampleDomainModel;

namespace SampleContext
{
    public class SweetContext : DbContext
    {
        public SweetContext() : base("name=SweetieDBConnectionString") { }

        public DbSet<ChocolateBox> ChocolateBoxes { get; set; }

        public DbSet<Chocolate> Chocolates { get; set; }
    }
}
