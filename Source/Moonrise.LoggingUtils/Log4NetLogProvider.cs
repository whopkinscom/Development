// <copyright file="Log4NetLogProvider.cs" company="Moonrise Media Ltd.">
// Originally written by WillH - with any acknowledgements as required. Once checked in to your version control you have full rights except for selling the source!
// </copyright>
// To use a Log4Net logging provider you will need to add a NuGet reference to log4net to this library and define UseLog4Net 
// in the Build section of this project's properties

#if UseLog4Net

using System;
using System.Diagnostics;
using log4net.Core;

namespace Moonrise.Utils.Standard.Logging
{
    /// <summary>
    ///     A logger that use Log4Net to handle the logging
    /// </summary>
    /// Taken out of the code for Common.Logging (https://github.com/net-commons/common-logging/blob/master/src/Common.Logging.Log4Net129/Logging/Log4Net/Log4NetLogger.cs)
    /// as it needs to establish the correct call stack for Log4Net to use and they'd already done it like so I nicked it!
    /// <seealso cref="Moonrise.Utils.Standard.Logging.ILoggingProvider" />
    public class Log4NetLogProvider : ILoggingProvider
    {
        /// <summary>
        ///     Used to retrieve a call stack.
        /// </summary>
        private static Type _callerStackBoundaryType;

        /// <summary>
        ///     The Log4Net Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        ///     Constructs a logger that will use Log4Net
        /// </summary>
        /// <param name="log">The Log4NetWrapper the provider will use</param>
        public Log4NetLogProvider(ILoggerWrapper log)
        {
            _logger = log.Logger;
        }

        /// <summary>
        ///     Implements the logging for a Moonrise ILogger.
        /// </summary>
        /// <param name="level">The level of the log required.</param>
        /// <param name="msg">The string message.</param>
        public void LogThis(LoggingLevel level, string msg)
        {
            LogUsingLog4Net(GetLog4NetLevel(level), msg);
        }

        /// <summary>
        ///     Converts a Moonrise logging level into the log4net level.
        /// </summary>
        /// <param name="level">The moonrise level.</param>
        /// <returns>The Log4Net level</returns>
        private Level GetLog4NetLevel(LoggingLevel level)
        {
            switch (level)
            {
                case LoggingLevel.Debug:
                    return Level.Debug;
                case LoggingLevel.Information:
                    return Level.Info;
                case LoggingLevel.Warning:
                    return Level.Warn;
                case LoggingLevel.Error:
                    return Level.Error;
                case LoggingLevel.Fatal:
                    return Level.Fatal;
                default:
                    return Level.Debug;
            }
        }

        /// <summary>
        ///     Determines whether the current type is somewhere within the checked type class hierarchy
        /// </summary>
        /// <param name="currentType">Type of the current.</param>
        /// <param name="checkType">Type of the check.</param>
        /// <returns>The answer!</returns>
        private bool IsInTypeHierarchy(Type currentType, Type checkType)
        {
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType == checkType)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }

        /// <summary>
        ///     Actually sends the message to the underlying log system.
        /// </summary>
        /// <param name="logLevel">the level of this log event.</param>
        /// <param name="message">the message to log</param>
        private void LogUsingLog4Net(Level logLevel, string message)
        {
            // determine correct caller - this might change due to jit optimizations with method inlining
            if (_callerStackBoundaryType == null)
            {
                lock (GetType())
                {
                    StackTrace stack = new StackTrace();
                    Type thisType = GetType();
                    _callerStackBoundaryType = typeof(Logger);

                    for (int i = 1; i < stack.FrameCount; i++)
                    {
                        if (!IsInTypeHierarchy(thisType, stack.GetFrame(i).GetMethod().DeclaringType))
                        {
                            _callerStackBoundaryType = stack.GetFrame(i - 1).GetMethod().DeclaringType;
                            break;
                        }
                    }
                }
            }

            _logger.Log(_callerStackBoundaryType, logLevel, message, null);
        }
    }
}

#endif
