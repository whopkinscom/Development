using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Moonrise.Logging;

namespace Moonrise.Utils.Database.Migrations
{
    public class MigrationState
    {
        public static DbMigrator Migrator { get; set; }
    }

    /// <summary>
    /// Used to manage the current migration state of the database.
    /// </summary>
    public class MigrationState<TContext, TConfiguration> : MigrationState
        where TContext : DbContext
        where TConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        /// <summary>
        /// Static constructor - will get called on first reference to a static member.
        /// </summary>
        static MigrationState()
        {
            configuration = new TConfiguration();
            Migrator = new DbMigrator(configuration);
        }

        private static TConfiguration configuration;

        /// <summary>
        /// The number of pending migrations
        /// </summary>
        public static int NumPendingMigrations
        {
            get
            {
                return Migrator.GetPendingMigrations().Count();
            }
        }

        /// <summary>
        /// The List of pending migrations
        /// </summary>
        public static IEnumerable<string> PendingMigrations
        {
            get
            {
                return Migrator.GetPendingMigrations();
            }
        }

        /// <summary>
        /// The list of migrations already applied to the database
        /// </summary>
        public static IEnumerable<string> AppliedMigrations
        {
            get
            {
                return Migrator.GetDatabaseMigrations();
            }
        }

        /// <summary>
        /// Checks the database migration state
        /// </summary>
        /// <exception cref="Exception">If the database needs migrating, throws a base exception with a message indicating that the IMTMigration tool needs to be run</exception>
        public static void CheckDatabase()
        {
            Logger.Log("Checking for pending migrations");

            if (NumPendingMigrations > 0)
                throw new Exception("A database migration is required, please backup the current database and run the migration application to apply the following migrations;\n " +
                    Migrator.GetPendingMigrations().ToList().CSL(Environment.NewLine));
        }

        /// <summary>
        /// Migrates the database by applying all the pending migrations.
        /// </summary>
        /// <param name="migrationTarget">The full migration target name - Migrations up to and including this one will only be applied</param>
        public static void Migrate(string migrationTarget)
        {
            if (string.IsNullOrEmpty(migrationTarget))
            {
                Migrator.Update();
            }
            else
            {
                Migrator.Update(migrationTarget);
            }
        }
    }
}
