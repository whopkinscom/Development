// <copyright file="WindowsEventLogProvider.cs" company="Moonrise Media Ltd.">
// Originally written by WillH - with any acknowledgements as required. Once checked in to your version control you have full rights except for selling the source!
// </copyright>

using System.Diagnostics;
using Moonrise.Logging.LoggingProviders;

namespace Moonrise.Logging
{
    /// <summary>
    ///     Writes logging to the Windows Event logger - Please use this with care as writing to a file might be more
    ///     manageable!
    /// </summary>
    public class WindowsEventLogProvider : ILoggingProvider
    {
        /// <summary>
        ///     The event log application name
        /// </summary>
        private readonly string application;

        /// <summary>
        ///     Constructs a logger that will write to the Windows event log.
        /// </summary>
        /// <param name="applicationName">The application name for the event log</param>
        public WindowsEventLogProvider(string applicationName)
        {
            application = applicationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsEventLogProvider"/> class.
        /// </summary>
        /// <param name="logSource">The log source.</param>
        /// <param name="eventLogName">Name of the event log.</param>
        public WindowsEventLogProvider(string logSource, string eventLogName)
        {
            ApplicationEventLog = new EventLog
                                  {
                                      Source = logSource,
                                      Log = eventLogName
                                  };
        }

        /// <summary>
        ///     The next logger to pass the log message on to. Allows additional loggers to be used. Don't create circular links
        ///     though eh!
        /// </summary>
        public ILoggingProvider NextLogger { get; set; }

        /// <summary>
        ///     The output level for writing to the event log. Can be set to a higher threshold than
        ///     <see cref="Logger.OutputLevel" /> to reduce the event log clutter
        /// </summary>
        public Logger.ReportingLevel OutputLevel { get; set; }

        /// <summary>
        /// The application event log
        /// </summary>
        private EventLog ApplicationEventLog { get; set; }

        /// <summary>
        ///     Logs the appropriate level of message.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="msg">The message.</param>
        public void LogThis(LoggingLevel level, string msg)
        {
            // This logger can have it's own reporting level that overrides the Logger.OutputLevel
            if (OutputLevel <= (Logger.ReportingLevel)level)
            {
                if (ApplicationEventLog != null)
                {
                    ApplicationEventLog.WriteEntry(msg, ConvertToEventLogType(level));
                }
                else
                {
                    EventLog.WriteEntry(application, msg, ConvertToEventLogType(level));
                }
            }
        }

        /// <summary>
        ///     Converts the Moonrise logging level to the event log entry type.
        /// </summary>
        /// <param name="level">The moonrise logging level.</param>
        /// <returns>Event log entry type</returns>
        private static EventLogEntryType ConvertToEventLogType(LoggingLevel level)
        {
            switch (level)
            {
                case LoggingLevel.Fatal:
                case LoggingLevel.Error:
                {
                    return EventLogEntryType.Error;
                }
                case LoggingLevel.Warning:
                {
                    return EventLogEntryType.Warning;
                }
                default:
                {
                    return EventLogEntryType.Information;
                }
            }
        }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return this;
        }
    }
}
