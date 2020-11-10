using System.Data.Entity;
using Moonrise.Utils.Database.Migration;
using SampleContext;

namespace CodeFirstMigration
{
    /// <summary>
    ///     The Context has been inherited from the target context so as to keep ALL migrated related code away from the
    ///     day-to-day production context
    /// </summary>
    public class SweetContext4Migration : SweetContext
    {
        /// <summary>
        ///     Allows specific rules to be created for when creating the migration code and database.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Placeholder for anything specific
            base.OnModelCreating(modelBuilder);
        }
    }

    public class SweetContextConfiguration : MigrationConfiguration<SweetContext4Migration> { }
}
