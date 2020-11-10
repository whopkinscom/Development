using Moonrise.Utils.Database.Migrations;

namespace CodeFirstMigration
{
    /// <summary>
    ///     Runs migrations against a sample database.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Just let the MigrationApp class do everything, all you need to supply is the specific context and migration configuration!
            MigrationApp<SweetContext4Migration, SweetContextConfiguration>.Main(args);
        }
    }
}
