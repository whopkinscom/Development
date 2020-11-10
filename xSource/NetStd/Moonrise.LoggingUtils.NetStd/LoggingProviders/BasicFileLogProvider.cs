#region Apache-v2.0

//    Copyright 2017 Will Hopkins - Moonrise Media Ltd.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Moonrise.Logging.LoggingProviders
{
    /// <summary>
    ///     A Basic File Logger that will write logging text to a file. If the directory is invalid or anything it will simply
    ///     not write.
    /// </summary>
    public class BasicFileLogProvider : ILoggingProvider, IAuditProvider
    {
        /// <summary>
        ///     Configuration required by this class
        /// </summary>
        public class Config
        {
            /// <summary>
            ///     Gives the timestamp prefix to use, e.g. "{0: HH:mm:ss.fffff}"
            /// </summary>
            public string DateTimeFormatterPrefix { get; set; } = "{0:HH:mm:ss} ";

            /// <summary>
            ///     How should the log files be recycled
            /// </summary>
            public Cycle LogCycling { get; set; } = Cycle.Daily;

            /// <summary>
            ///     if set to true there is a different file per thread.
            /// </summary>
            public bool LogFilePerThread { get; set; }

            /// <summary>
            ///     The path of the filename to write to, please include an extension.
            /// </summary>
            public string LoggingFile { get; set; } = "./Logging.log";
        }

        /// <summary>
        ///     Allows control over the current <see cref="DateTime" /> to use - should you need to use it. Typically used for
        ///     testing.
        /// </summary>
        public interface IDateTimeProvider
        {
            /// <summary>
            ///     The current <see cref="DateTime" /> to use
            /// </summary>
            DateTime Now { get; }
        }

        /// <summary>
        ///     Provides an implementation of <see cref="BasicFileLogProvider.IDateTimeProvider" /> that uses current time.
        /// </summary>
        private class DateTimeProvider : IDateTimeProvider
        {
            /// <summary>
            ///     The current <see cref="DateTime" /> to use
            /// </summary>
            public DateTime Now => DateTime.Now;
        }

        /// <summary>
        ///     Indicates if an instance is a cloned instance or the original
        /// </summary>
        private readonly bool _cloned;

        /// <summary>
        ///     The date time provider, for generating timestamps and filenames.
        /// </summary>
        private readonly IDateTimeProvider _dateTimeProvider;

        /// <summary>
        ///     The retained initial configuration, in case of needing to <see cref="Clone" />
        /// </summary>
        private readonly Config config;

        /// <summary>
        ///     The lock object to use to single thread file writing.
        /// </summary>
        private readonly object lockObject;

        private IAuditProvider _nextAuditor;

        /// <summary>
        ///     The _next <see cref="ILoggingProvider" /> to pass logging messages on to.
        /// </summary>
        private ILoggingProvider _nextLogger;

        /// <summary>
        ///     The logging file extension
        /// </summary>
        private string extension;

        /// <summary>
        ///     Keeps track of consecutive log write failures, More than 5 means file logging becomes disabled!
        /// </summary>
        private int failCount;

        /// <summary>
        ///     Since log providers offer pass through with the <see cref="NextLogger" /> property we can choose to
        ///     disable file logging by having a blank filename - usually going to be passed through from configuration.
        ///     This field indicates that.
        /// </summary>
        private bool fileLoggingEnabled;

        /// <summary>
        ///     The main name part of the logging filename
        /// </summary>
        /// The actual filename is computed on each log statement in case we get a filename flip over
        private string filename;

        /// <summary>
        ///     A very basic text file logger that writes any logging to the specified text file. Does not give a timestamp prefix.
        /// </summary>
        /// <param name="_filename">The path of the filename to wrote to.</param>
        /// <param name="dateTimeProvider">
        ///     If you want to control the DateTime to use for the logfile name and for timestamps in
        ///     the log file
        /// </param>
        public BasicFileLogProvider(string _filename = "", IDateTimeProvider dateTimeProvider = null)
        {
            config = new Config
                     {
                         DateTimeFormatterPrefix = string.Empty,
                         LogCycling = Cycle.Monthly,
                         LogFilePerThread = false,
                         LoggingFile = _filename
                     };
            lockObject = new object();
            FilePerThread = config.LogFilePerThread;
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
            _cloned = false;

            if (string.IsNullOrWhiteSpace(_filename))
            {
                _filename = Process.GetCurrentProcess().ProcessName;
            }

            InitialiseLogFile(_filename, config.DateTimeFormatterPrefix, config.LogCycling);
        }

        /// <summary>
        ///     A very basic text file logger that writes any logging to the specified text file.
        /// </summary>
        /// <param name="_filename">The path of the filename to write to.</param>
        /// <param name="dateTimeFormatterPrefix">Gives the timestamp prefix to use, e.g. {0: HH:mm:ss.fffff}</param>
        /// <param name="recycle">How should the log files be recycled</param>
        /// <param name="filePerThread">if set to <c>true</c> there is a different file per thread.</param>
        /// <param name="dateTimeProvider">The date time provider.</param>
        /// <param name="cloned">Indicates if this construction is via an act of cloning. Please leave this false!</param>
        [Obsolete("Please use the constructor that takes the Config class. This one WILL get removed")]
        public BasicFileLogProvider(string _filename,
                                    string dateTimeFormatterPrefix,
                                    Cycle recycle,
                                    bool filePerThread = true,
                                    IDateTimeProvider dateTimeProvider = null,
                                    bool cloned = false)
        {
            lockObject = new object();
            FilePerThread = filePerThread;
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
            _cloned = cloned;
            InitialiseLogFile(_filename, dateTimeFormatterPrefix, recycle);
            config = new Config
                     {
                         DateTimeFormatterPrefix = dateTimeFormatterPrefix,
                         LogCycling = recycle,
                         LogFilePerThread = filePerThread,
                         LoggingFile = _filename
                     };
        }

        /// <summary>
        ///     A very basic text file logger that writes any logging to the specified text file.
        /// </summary>
        /// <param name="_config">The configuration.</param>
        /// <param name="dateTimeProvider">The date time provider.</param>
        /// <param name="cloned">Indicates if this construction is via an act of cloning. Please leave this false!</param>
        public BasicFileLogProvider(Config _config = null,
                                    IDateTimeProvider dateTimeProvider = null,
                                    bool cloned = false)
        {
            if (_config == null)
            {
                // Be forgiving
                _config = new Config();
            }

            config = _config;
            lockObject = new object();
            FilePerThread = config.LogFilePerThread;
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
            _cloned = cloned;
            InitialiseLogFile(config.LoggingFile, config.DateTimeFormatterPrefix, config.LogCycling);
        }

        /// <summary>
        ///     Determines how filenames will get recycled. When a filename is recycled any previous file of the same name is
        ///     overwritten.
        /// </summary>
        public enum Cycle
        {
            /// <summary>
            ///     Same filename is recycled every run - i.e. previous run's file is deleted. Nothing is added to the filename.
            /// </summary>
            Always,

            /// <summary>
            ///     Same filename is recycled every day. Nothing is added to the filename.
            /// </summary>
            Daily,

            /// <summary>
            ///     Same filename is recycled every week. "_ddd" for the 3 character day name is added before the file extension.
            /// </summary>
            Weekly,

            /// <summary>
            ///     Same filename is recycled every month. "_DD" for the day of the month is added before the file extension.
            /// </summary>
            Monthly,

            /// <summary>
            ///     Same filename is recycled every year. "_MMDD" for the month and day of the month is added before the file
            ///     extension.
            /// </summary>
            Yearly,

            /// <summary>
            ///     The filename is "never" recycled. "_YYYYMMDD" for the year, month and day of the month is added before the file
            ///     extension.
            /// </summary>
            Never
        }

        /// <summary>
        ///     Indicates if a separate file is used for each thread. This can maintain some consistency in logs at the expense of
        ///     more log files.
        /// </summary>
        public static bool FilePerThread { get; set; }

        /// <summary>
        ///     The most recent filename to have been created for writing a log message - Note that the filename can change based
        ///     on the <see cref="Cycle" /> setting in use.
        /// </summary>
        public string MostRecentFilename { get; set; }

        /// <summary>
        ///     The next auditor to pass the audit message on to. Allows additional auditors to be used. Don't create circular
        ///     links though eh!
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules",
            "SA1513:ClosingCurlyBracketMustBeFollowedByBlankLine",
            Justification = "Except for getter/setters!")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "Except for getter/setters!")]
        public IAuditProvider NextAuditor
        {
            get => _nextAuditor;
            set
            {
                if (value != this)
                {
                    _nextAuditor = value;
                }
            }
        }

        /// <summary>
        ///     The _next <see cref="ILoggingProvider" /> to pass logging messages on to.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules",
            "SA1513:ClosingCurlyBracketMustBeFollowedByBlankLine",
            Justification = "Except for getter/setters!")]
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "Except for getter/setters!")]
        public ILoggingProvider NextLogger
        {
            get => _nextLogger;
            set
            {
                if (value != this)
                {
                    _nextLogger = value;
                }
            }
        }

        /// <summary>
        ///     The current filename recycling setting
        /// </summary>
        public Cycle Recycle { get; set; }

        /// <summary>
        ///     The timestamp prefix
        /// </summary>
        public string TimestampPrefix { get; private set; }

        /// <summary>
        ///     Audits the message.
        /// </summary>
        /// <param name="msg">The message.</param>
        public void AuditThis(string msg)
        {
            // We simply log this. The way we're LIKELY to get used is that there will be a different FileLogger for logging and auditing with different filenames for each.
            LogMsg($"AUDIT: {msg}");
        }

        /// <summary>
        ///     Audits an object. Can be used IF a specific object is to be audited by an implementation rather than simply a
        ///     string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="auditObject">The object to audit - it will be JSONd</param>
        /// <param name="auditLevel">The level of audit</param>
        public void AuditThisObject(string message, object auditObject, LoggingLevel auditLevel)
        {
            // The basic implementation simply audits the Json version
            string auditMsg = auditLevel == LoggingLevel.Audit ? string.Empty : auditLevel + $" {message}\r\n{Logger.JsonIt(auditObject)}";
            AuditThis(auditMsg);
        }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            BasicFileLogProvider cloned = new BasicFileLogProvider(config,
                                                                   _dateTimeProvider,
                                                                   true);
            return cloned;
        }

        /// <summary>
        ///     Logs the appropriate level of message.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="msg">The message.</param>
        public void LogThis(LoggingLevel level, string msg)
        {
            if (fileLoggingEnabled)
            {
                if (level == LoggingLevel.Fatal)
                {
                    LogMsg("*********************\r\n" +
                           "***** FATAL Error: " + msg +
                           "\r\n*********************");
                }
                else if (level == LoggingLevel.Error)
                {
                    LogMsg("Error: " + msg);
                }
                else if (level == LoggingLevel.Warning)
                {
                    LogMsg("Warning: " + msg);
                }
                else if (level == LoggingLevel.Audit)
                {
                    LogMsg("Audit: " + msg);
                }
                else
                {
                    LogMsg(msg);
                }
            }
        }

        /// <summary>
        ///     Constructs the logging filename based on the required filename and the required recycling
        /// </summary>
        /// <param name="filename">Full path to the file, but without the extension</param>
        /// <param name="now">The datetime to use to create the filename - where required by the recycle setting.</param>
        /// <param name="extension">The extension</param>
        /// <returns>A filename that incorporates the required recycling additions based on the current datetime</returns>
        private string ConstructFilename(string filename, DateTime now, string extension)
        {
            // Initialise to daily value.
            string retVal = string.Empty;
            string threadId = string.Empty;

            if (FilePerThread && _cloned)
            {
                threadId = "-" + Thread.CurrentThread.ManagedThreadId;
            }

            switch (Recycle)
            {
                case Cycle.Daily:
                case Cycle.Always:
                {
                    retVal = string.Format("{0}{1}{2}", filename, threadId, extension);
                    break;
                }

                case Cycle.Weekly:
                {
                    retVal = string.Format("{0}_{1:ddd}{2}{3}", filename, now, threadId, extension);
                    break;
                }

                case Cycle.Monthly:
                {
                    retVal = string.Format("{0}_{1:dd}{2}{3}", filename, now, threadId, extension);
                    break;
                }

                case Cycle.Yearly:
                {
                    retVal = string.Format("{0}_{1:MMdd}{2}{3}", filename, now, threadId, extension);
                    break;
                }

                case Cycle.Never:
                {
                    retVal = string.Format("{0}_{1:yyyyMMdd}{2}{3}", filename, now, threadId, extension);
                    break;
                }
            }

            MostRecentFilename = retVal;

            return retVal;
        }

        /// <summary>
        ///     Initialises the log file.
        /// </summary>
        /// <param name="_filename">The _filename.</param>
        /// <param name="dateTimeFormatterPrefix">The date time formatter prefix.</param>
        /// <param name="recycle">The file recycle value.</param>
        private void InitialiseLogFile(string _filename, string dateTimeFormatterPrefix, Cycle recycle)
        {
            if (!string.IsNullOrWhiteSpace(_filename))
            {
                string path = Path.GetDirectoryName(_filename);

                // If the directory doesn't exist, try to create it
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filename = path + "/" + Path.GetFileNameWithoutExtension(_filename);
                extension = Path.GetExtension(_filename);
                TimestampPrefix = dateTimeFormatterPrefix;
                Recycle = recycle;
                fileLoggingEnabled = true;

                if (Recycle == Cycle.Always)
                {
                    try
                    {
                        string currentLoggingFilename = ConstructFilename(filename, _dateTimeProvider.Now, extension);
                        File.Delete(currentLoggingFilename);
                    }
                    catch (Exception excep)
                    {
                        // We won't log this exception, but at least stick it in the trace output
                        Trace.Write(excep.Message);

                        // Then turn off file logging
                        fileLoggingEnabled = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Logs a message with a configured datetime prefix.
        /// </summary>
        /// <param name="msg">The message to log</param>
        private void LogMsg(string msg)
        {
            try
            {
                // Get the current datetime to help determine the resultant filename
                DateTime now = _dateTimeProvider.Now;
                string currentLoggingFilename = ConstructFilename(filename, now, extension);

                lock (lockObject)
                {
                    // Check if the file exists but is for a previous period, if so delete it.
                    FileInfo logFileInfo = new FileInfo(currentLoggingFilename);

                    if (logFileInfo.Exists && (logFileInfo.LastWriteTime.DayOfYear != now.DayOfYear))
                    {
                        logFileInfo.Delete();
                    }

                    File.AppendAllText(currentLoggingFilename,
                                       string.Format(TimestampPrefix, DateTimeOffset.Now) + msg + Environment.NewLine);
                }

                failCount = 0;
            }
            catch (Exception excep)
            {
                // We can't log this exception, obviously, so at least stick it in the trace output
                Trace.Write(excep.Message);

                if (++failCount > 5)
                {
                    fileLoggingEnabled = false;
                }
            }
        }
    }
}
