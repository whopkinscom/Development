using System;
using Moonrise.Logging;
using System.Collections.Generic;
using System.Runtime;
using JetBrains.Annotations;
using Moonrise.Logging.LoggingProviders;
using Moonrise.Utils.Standard.Config;

namespace Moonrise.Samples
{
    /// <summary>
    /// Samples of how to use the Moonrise library.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            SuggestedInitialisation();

            SampleLogging();
        }

        /// <summary>
        /// Samples of how logging can be used. You can tweak what makes it out into the log file by changing the logging level in appsettings.json
        /// They are essentially "Debug", "Info", "Warning", "Error", "Fatal"
        /// </summary>
        private static void SampleLogging()
        {
            LoggingContexts();
            LoggingTags();
            LoggingExceptionsAndObjects();

            // Why don't XML Doc comments work here? Anyway.
            // This method shows how logging context works and is scoped. Scope COULD be method names but are probably better used in a wider way.
            // Up to you, but this is how they work.
            void LoggingContexts()
            {
                Logger.Info("This is outside any context");

                using (Logger.Context("Context 1"))
                {
                    Logger.Debug("Inside the first context");

                    using (Logger.Context("Context 2"))
                    {
                        Logger.Debug("Inside the second context");
                        LoggingContextsAlsoWorkInsideMethods();
                    }

                    Logger.Debug("Outside the second context but inside the first context");
                }

                Logger.Error("And now we're back outside any contexts");

                void LoggingContextsAlsoWorkInsideMethods()
                {
                    Logger.Warning("So, inside second context from inside a method");
                    Logger.LogMethodName = true;
                    Logger.Debug("We're now logging the method name");
                    bool areUsingContext = Logger.UseContext;
                    Logger.UseContext = false;
                    Logger.Debug("We're now logging the method name, but without the context");
                    Logger.LogMethodName = false;
                    Logger.UseContext = areUsingContext;
                    Logger.Debug("Now we're not logging the method name");
                }
            }

            // Log tags are a way to get finer grained logging that can be switched on or off in addition to the typical logging levels.
            // I've always tended to activate log tags at startup, based on config file settings, but they could be switched dynamically
            // once you have a way to communicate what tags should be active!
            void LoggingTags()
            {
                Logger.LogActiveLogTags();

                // All contained logging will be tagged, including any scoped methods 
                using (Logger.ScopedLogTag(Initialise.LogTags.Defined))
                {
                    Logger.Warning("It's worth knowing that only Info & Debug can get tagged & thus switched off. Anything more serious shouldn't be switchable!");
                    Logger.Debug("A debug message with the tag Defined");
                    Logger.Info("An info message with the tag Defined");
                    Logger.Debug("The previous two used the scoped tag but for this one I'll use a different tag", Initialise.LogTags.AreCurrently);
                    ButWhatAboutLoggingFromSubroutines();
                }

                using (Logger.ScopedLogTag())
                {
                    Logger.Debug("This one will use a tag of SampleLogging, NOT LoggingTags");
                    ButWhatAboutLoggingFromSubroutines();
                }

                LogTag madeUpOnTheFly = new LogTag("MadeUpOnTheFly");
                Logger.ActivateLogTag(madeUpOnTheFly);
                Logger.Info("Adding a new log tag", madeUpOnTheFly);
                Logger.DeactivateLogTag(madeUpOnTheFly);
                Logger.Info("This one won't appear!", madeUpOnTheFly);


                void ButWhatAboutLoggingFromSubroutines()
                {
                    Logger.Info("If they don't specify a tag, they will take on whatever tag scope is active");
                    Logger.Debug("Unless of course they use their own override", Initialise.LogTags.NoTags);
                }
            }

            // Here I show you how to log an exception and objects in general
            void LoggingExceptionsAndObjects()
            {
                try
                {
                    try
                    {
                        Logger.Info("You'll get a JSON representation of the Logging Configuration");
                        Logger.Info(Initialise.LoggingConfiguration);

                        throw new AmbiguousImplementationException("Now let's log an exception!");
                    }
                    catch (Exception excep)
                    {
                        Logger.Error(excep);

                        try
                        {
                            Logger.Info("It'll even handle nested exceptions");

                            throw new BadImageFormatException("The outer exception message", excep);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Info("Previously logged exceptions log less verbosely when logged again in a cascading catch");
                    Logger.Error(e);
                }
            }
        }

        /// <summary>
        /// This is how I suggest you initialise, as early as you can in your entry point.
        /// </summary>
        private static void SuggestedInitialisation()
        {
            Initialise.ConfigurationSettings();
            Initialise.Logging();
        }
    }

    /// <summary>
    ///     A static class to encompass initialisation of various cross-project entities.
    /// </summary>
    /// <remarks>
    /// Ordinarily this would of course be in a separate file but for ease of reference I've pulled it in here!
    /// </remarks>
    public static class Initialise
    {
        public struct LogTags
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
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider(new JsonConfigSettingsProvider.Config
            {
                ApplicationSettingsFilename = "Moonrise.Samples.appSettings.json"
            });
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
            Logger.Debug("Just logging the current logging settings");
            Logger.Debug(LoggingConfiguration);
            Logger.Title("Let the Samples Begin!");
        }
    }
}
