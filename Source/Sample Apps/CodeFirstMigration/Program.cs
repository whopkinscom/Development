using NDesk.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using Moonrise.Logging;
using Moonrise.Logging.LoggingProviders;

namespace CodeFirstMigration
{
    /// <summary>
    /// Runs migrations against a sample database.
    /// </summary>
    class Program
    {
        static string appName = "Migration";
        static string migrationTarget = string.Empty;

        static void Main(string[] args)
        {
            try
            {
                // Set up a basic file logger.
                Logger.LogProvider = new BasicFileLogProvider(new BasicFileLogProvider.Config());
                Logger.UseConsoleOutput = true;

                if (HandleArgs(args))
                {
                    Migrate<IMTDbContext, IMTMigrationState>(migrationTarget);
                    Logger.Seperate();
                }
            }
            catch (Exception excep)
            {
                Logger.Error(excep, "MIGRATION FAILED: An error occured:");
            }

            Console.Out.WriteLine("Press enter to finish...");
            Console.In.Read();
        }

        /// <summary>
        /// Generic function to apply migrations on a given database as detemined by the type parameters.
        /// </summary>
        /// <typeparam name="TContext">Database migration context</typeparam>
        /// <typeparam name="TMigrationState">The migration state for the database</typeparam>
        /// <param name="upToAndIncluding">if not empty indicates a migration to migrate up to, any later pending migrations are not done</param>
        private static void Migrate<TContext, TMigrationState>(string upToAndIncluding)
            where TContext : DbContext, new()
            where TMigrationState : MigrationState, new()
        {
            TContext db = new TContext();
            Logger.Title("{0} Database Migration", db.Database.Connection.Database);

            if (db.Database.Exists()) //only peform migrations on pre-existing databases, this allows multiple developers to work on multiple applications with a single migration tool
            {
                Logger.Log("Checking pending migrations for {0}{1}", db.Database.Connection.ConnectionString, Environment.NewLine);
                IEnumerable<string> databaseMigrations = (IEnumerable<string>)typeof(TMigrationState).GetProperty("AppliedMigrations", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetMethod.Invoke(null, null);

                foreach (string migration in databaseMigrations)
                {
                    Logger.Log("Migration {0} already applied.", migration);
                }

                int pendingMigrations = (int)typeof(TMigrationState).GetProperty("NumPendingMigrations", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetMethod.Invoke(null, null);


                if (pendingMigrations == 0)
                {
                    Logger.Log("No migrations are pending");
                }
                else
                {
                    Logger.Log("Applying the following {0} migration(s)", pendingMigrations);

                    string migrationTarget = string.Empty;
                    IEnumerable<string> migrations = (IEnumerable<string>)typeof(TMigrationState).GetProperty("PendingMigrations", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetMethod.Invoke(null, null);

                    foreach (string migration in migrations)
                    {
                        if (string.IsNullOrEmpty(migrationTarget) && !string.IsNullOrEmpty(upToAndIncluding) && migration.IndexOf(upToAndIncluding) != -1)
                        {
                            migrationTarget = migration;
                            Logger.Log("Migrating up to {0}", migrationTarget);
                        }

                        Logger.Log("{0}", migration);
                    }

                    if (!string.IsNullOrEmpty(upToAndIncluding) && string.IsNullOrEmpty(migrationTarget))
                    {
                        Logger.Log("Could not find a migration name including '{0}', no migrations applied!", upToAndIncluding);
                    }
                    else
                    {
                        typeof(TMigrationState).GetMethod("Migrate", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Invoke(null, new object[] { migrationTarget });
                        Logger.Log("Migration has completed");
                    }
                }
            }
            else
            {
                Logger.Log("{0} Database was not found. Migration will not occur.", db.Database.Connection.Database);
            }

        }

        /// <summary>
        /// Handle command line arguments
        /// </summary>
        /// Uses the NDesk OptionSet options processing
        /// <param name="args">Program's argument string</param>
        /// <returns>True if processing can continue</returns>
        static bool HandleArgs(string[] args)
        {
            bool retVal = true;

            bool showHelp = false;
            OptionSet options = new OptionSet
            {
                { "IMT=", "The (partial) name of the IMT migration to apply up to and including",
                  i => imtMigrationTarget = i },
                { "SYS=", "The (partial) name of the Syslog migration to apply up to and including",
                  i => syslogMigrationTarget = i },
                { "IDB=", "The (partial) name of the IDB migration to apply up to and including",
                  i => intelMigrationTarget = i },
                { "h|help",  "show this message and exit",
                  h => showHelp = h != null },
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
                Console.WriteLine(string.Format("{0} : {1} Try '{0} --help' for more information.", appName, excep.Message));
                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// Shows help options
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
