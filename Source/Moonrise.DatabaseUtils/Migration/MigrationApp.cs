using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Reflection;
using Moonrise.Logging;
using Moonrise.Logging.LoggingProviders;
using NDesk.Options;

namespace Moonrise.Utils.Database.Migrations
{
    public static class MigrationApp<TContext, TConfiguration>
        where TContext : DbContext, new()
        where TConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        public class TMigrationState : MigrationState<TContext, TConfiguration> { }

        private static readonly string appName = "Migration";
        private static string migrationTarget = string.Empty;

        public static void Main(string[] args)
        {
            try
            {
                // Set up a basic file logger.
                Logger.LogProvider = new BasicFileLogProvider(
                    new BasicFileLogProvider.Config
                    {
                        LogCycling = BasicFileLogProvider.Cycle.Always
                    });
                Logger.OutputLevel = Logger.ReportingLevel.All;
                Logger.UseConsoleOutput = true;

                if (HandleArgs(args))
                {
                    Migrate<TContext, TMigrationState>(migrationTarget);
                    Logger.Seperate();
                }
            }
            catch (Exception excep)
            {
                Logger.Error(excep, "MIGRATION FAILED: An error occured:");
            }

            Console.WriteLine("Press a key to finish...");
            Console.ReadKey();
        }

        /// <summary>
        ///     Handle command line arguments
        /// </summary>
        /// Uses the NDesk OptionSet options processing
        /// <param name="args">Program's argument string</param>
        /// <returns>True if processing can continue</returns>
        private static bool HandleArgs(string[] args)
        {
            bool retVal = true;
            bool showHelp = false;
            OptionSet options = new OptionSet
                                {
                                    {
                                        "M=", "The (partial) name of the migration to apply up to and including",
                                        i => migrationTarget = i
                                    },
                                    {
                                        "h|help", "show this message and exit",
                                        h => showHelp = h != null
                                    }
                                };

            try
            {
                options.Parse(args);

                if (showHelp)
                {
                    ShowHelp(options);
                    retVal = false;
                }
            }
            catch (OptionException excep)
            {
                Console.WriteLine("{0} : {1} Try '{0} --help' for more information.", appName, excep.Message);
                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        ///     Generic function to apply migrations on a given database as detemined by the type parameters.
        /// </summary>
        /// <typeparam name="TContext">Database migration context</typeparam>
        /// <typeparam name="TMigrationState">The migration state for the database</typeparam>
        /// <param name="upToAndIncluding">
        ///     if not empty indicates a migration to migrate up to, any later pending migrations are
        ///     not done
        /// </param>
        private static void Migrate<TContext, TMigrationState>(string upToAndIncluding)
            where TContext : DbContext, new()
            where TMigrationState : MigrationState
        {
            TContext db = new TContext();
            Logger.Title($"{db.Database.Connection.Database} Database Migration");

            if (db.Database.Exists()) //only peform migrations on pre-existing databases, this allows multiple developers to work on multiple applications with a single migration tool
            {
                Logger.Info($"Checking pending migrations for {db.Database.Connection.ConnectionString}");
                IEnumerable<string> databaseMigrations = (IEnumerable<string>)typeof(TMigrationState)
                                                                              .GetProperty("AppliedMigrations", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                                                              .GetMethod.Invoke(null, null);

                foreach (string migration in databaseMigrations)
                {
                    Logger.Info($"Migration {migration} already applied.");
                }

                int pendingMigrations = (int)typeof(TMigrationState).GetProperty("NumPendingMigrations", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                                                    .GetMethod.Invoke(null, null);

                if (pendingMigrations == 0)
                {
                    Logger.Info("No migrations are pending");
                }
                else
                {
                    Logger.Info($"Applying the following {pendingMigrations} migration(s)");

                    string migrationTarget = string.Empty;
                    IEnumerable<string> migrations = (IEnumerable<string>)typeof(TMigrationState)
                                                                          .GetProperty("PendingMigrations", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                                                          .GetMethod.Invoke(null, null);

                    foreach (string migration in migrations)
                    {
                        if (string.IsNullOrEmpty(migrationTarget) && !string.IsNullOrEmpty(upToAndIncluding) && (migration.IndexOf(upToAndIncluding) != -1))
                        {
                            migrationTarget = migration;
                            Logger.Info($"Migrating up to {migrationTarget}");
                        }

                        Logger.Info($"{migration}");
                    }

                    if (!string.IsNullOrEmpty(upToAndIncluding) && string.IsNullOrEmpty(migrationTarget))
                    {
                        Logger.Info("Could not find a migration name including '{upToAndIncluding}', no migrations applied!");
                    }
                    else
                    {
                        typeof(TMigrationState).GetMethod("Migrate", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Invoke(null, new object[] {migrationTarget});
                        Logger.Info("Migration has completed");
                    }
                }
            }
            else
            {
                Logger.Info($"{db.Database.Connection.Database} Database was not found. Migration will not occur.");
            }
        }

        /// <summary>
        ///     Shows help options
        /// </summary>
        /// <param name="p">The options</param>
        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + appName + " [OPTIONS]");
            Console.WriteLine("Migrates the CyberDev databases to the latest or a particular version.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
