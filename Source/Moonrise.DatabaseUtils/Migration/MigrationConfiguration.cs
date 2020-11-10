using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Moonrise.Logging;

namespace Moonrise.Utils.Database.Migration
{
    /// <summary>
    ///     The configuration for the migration that can handle migration seeding.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="System.Data.Entity.Migrations.DbMigrationsConfiguration{TContext}" />
    public class MigrationConfiguration<TContext> : DbMigrationsConfiguration<TContext>
        where TContext : DbContext, new()
    {
        /// <summary>
        ///     A list of the seeded migrations whose <see cref="ISeededMigration{TContext}.Seed(TContext)" /> if added by that
        ///     migration's .Up().
        /// </summary>
        protected static List<ISeededMigration<TContext>> SeededMigrations = new List<ISeededMigration<TContext>>();

        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }

        /// <summary>
        ///     Adds a migration into the list of migrations that require seeding.
        /// </summary>
        /// <param name="migration">The migration to add to the needed seeds</param>
        public static void AddSeedingMigration(ISeededMigration<TContext> migration)
        {
            // We OR this in to the "list" of seeded migrations
            SeededMigrations.Add(migration);
            Logger.Info($"Added {migration.GetType().Name} migration to the list of migration seeds to be run.");
        }

        /// <summary>
        ///     Called after any migrations that have been run to apply any data seeding required for those migrations.
        /// </summary>
        /// <param name="context">The context to seed</param>
        protected override void Seed(TContext context)
        {
            Logger.Info($"Applying {SeededMigrations.Count} migration seeds;");

            foreach (ISeededMigration<TContext> seededMigration in SeededMigrations)
            {
                Logger.Info($"Running {seededMigration.GetType().Name} migration seed.");
                seededMigration.Seed(context);
                Logger.Info($"{seededMigration.GetType().Name} migration seed finished.");
            }
        }
    }
}
