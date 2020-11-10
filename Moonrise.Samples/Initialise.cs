using System.Collections.Generic;
using JetBrains.Annotations;
using Moonrise.Logging;
using Moonrise.Logging.LoggingProviders;
using Moonrise.Utils.Standard.Config;

namespace Moonrise.Samples
{
    /// <summary>
    ///     A static class to encompass initialisation of various cross-project entities.
    /// </summary>
    public static class Initialise
    {
        public  struct LogTags
        {
            public static LogTag NoTags = new LogTag(nameof(NoTags));
            public static LogTag AreCurrently = new LogTag(nameof(AreCurrently));
            public static LogTag Defined = new LogTag(nameof(Defined));
        }

        /// <summary>
        ///     The logging configuration class/structure to be read from the appsettings.json or overrides.
        /// </summary>
        public class LoggingConfig
        {
            /// <summary>
            ///     The level of logging to report - messages of this level and higher priority will get logged
            /// </summary>
            [UsedImplicitly]
            public Logger.ReportingLevel Level { get; set; } = Logger.ReportingLevel.Information;
            /// <summary>
            ///     File logger configuration
            /// </summary>
            [UsedImplicitly]
            public BasicFileLogProvider.Config LogFile { get; set; } = new BasicFileLogProvider.Config();

            /// <summary>
            ///     A list of the log tag names that should be considered active to be logged
            /// </summary>
            [UsedImplicitly]
            public List<string> LogTags { get; set; }// = new List<string>{ "NoTags", "AreCurrently", "Defined"};

            /// <summary>
            ///     True if logging context is to be emitted
            /// </summary>
            [UsedImplicitly]
            public bool UseLoggingContext { get; set; } = true;
        }

        public static LoggingConfig LoggingConfiguration = new LoggingConfig();

        /// <summary>
        ///     Initialisation of the configuration settings infrastructure
        /// </summary>
        public static void ConfigurationSettings()
        {
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider();
        }

        /// <summary>
        ///     Initialisation of the logging infrastructure. We're using a <see cref="BasicFileLogProvider" /> to log to a file
        ///     specified in the <see cref="LoggingConfig" />.
        /// </summary>
        public static void Logging()
        {
            Settings.Application.Read("Logging", ref LoggingConfiguration, true);
            Logger.LogProvider = new BasicFileLogProvider(LoggingConfiguration.LogFile);
            Logger.OutputLevel = LoggingConfiguration.Level;
            Logger.UseContext = LoggingConfiguration.UseLoggingContext;
            Logger.ActivateLogTags(LoggingConfiguration.LogTags);
            Logger.Seperate('*');
            Logger.Debug(LoggingConfiguration);
        }
    }
}
