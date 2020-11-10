using System.Data.Entity;

namespace Moonrise.Utils.Database.Migration
{
    /// <summary>
    ///     Allows a migration to run a seed
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface ISeededMigration<in TContext>
        where TContext : DbContext
    {
        /// <summary>
        ///     Applies seeding for a particular migration
        /// </summary>
        /// <param name="context">The context to use for your seeding.</param>
        void Seed(TContext context);
    }
}
