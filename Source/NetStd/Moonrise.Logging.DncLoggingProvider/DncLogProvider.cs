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
using Microsoft.Extensions.Logging;

namespace Moonrise.Logging
{
    /// <summary>
    ///     Uses the Microsoft Extensions Logging Abstractions logger to log from the Moonrise logging.
    /// </summary>
    /// <seealso cref="Moonrise.Logging.ILoggingProvider" />
    public class DncLogProvider : ILoggingProvider
    {
        /// <summary>
        ///     The logger factory
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DncLogProvider" /> class.
        /// </summary>
        /// <param name="_loggerFactory">The logger factory.</param>
        public DncLogProvider(ILoggerFactory _loggerFactory)
        {
            loggerFactory = _loggerFactory;
        }

        /// <summary>
        ///     The next logger to pass the log message on to. Allows additional loggers to be used. Don't create circular links
        ///     though eh!
        /// </summary>
        public ILoggingProvider NextLogger { get; set; }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns>
        ///     A new instance with the same contents values as itself
        /// </returns>
        public object Clone()
        {
            return new DncLogProvider(loggerFactory);
        }

        /// <summary>
        /// Flush any buffers currently in use.
        /// </summary>
        public void FlushBuffers()
        {
            // Nothing to do here!
        }

        /// <summary>
        ///     Logs the appropriate level of message.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="context">The context - if <see cref="Logger.UseContext" /> is false, this will be empty.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId"/> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        /// <param name="msg">The message.</param>
        public void LogThis(LoggingLevel level, string context, string threadId, LogTag logTag, string msg)
        {
            ILogger logger = loggerFactory.CreateLogger("Moonrise");

            if (!string.IsNullOrWhiteSpace(context))
            {
                msg = $"{context}: {msg}";
            }

            switch (level)
            {
                case LoggingLevel.Debug:
                    logger.LogDebug(msg);
                    break;
                case LoggingLevel.Information:
                    logger.LogInformation(msg);
                    break;
                case LoggingLevel.Warning:
                    logger.LogWarning(msg);
                    break;
                case LoggingLevel.Error:
                    logger.LogError(msg);
                    break;
                case LoggingLevel.Fatal:
                    logger.LogError($"***** FATAL ***** - {msg}");
                    break;
                case LoggingLevel.Audit:
                    logger.LogInformation($"***** AUDIT ***** - {msg}");
                    break;
            }
        }
    }
}
