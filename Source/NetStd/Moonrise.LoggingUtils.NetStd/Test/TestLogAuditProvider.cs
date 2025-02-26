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

using System.Collections.Generic;
using Moonrise.Logging;
using Newtonsoft.Json;

namespace Moonrise.Utils.Test.Logging
{
    /// <summary>
    ///     A logging provider that can be utilised when you need to test that certain code DID log or audit something
    /// </summary>
    /// <seealso cref="ILoggingProvider" />
    public class TestLogAuditProvider : ILoggingProvider, IAuditProvider
    {
        /// <summary>
        ///     A log entry captures what was logged
        /// </summary>
        public class LogEntry
        {
            /// <summary>
            ///     The context in play
            /// </summary>
            public string Context { get; set; }

            /// <summary>
            ///     The logging level used.
            /// </summary>
            public LoggingLevel Level { get; set; }

            /// <summary>
            ///     The log tag used
            /// </summary>
            public LogTag LogTag { get; set; }

            /// <summary>
            ///     The logging message used
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            ///     The current thread id
            /// </summary>
            public string ThreadId { get; set; }
        }

        /// <summary>
        ///     A log buffer that you can query later if you need to check things were logged.
        /// </summary>
        public List<LogEntry> LogBuffer { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestLogAuditProvider" /> class using a specified initial size
        ///     <see cref="LogBuffer" />.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer.</param>
        public TestLogAuditProvider(int bufferSize) => LogBuffer = new List<LogEntry>(bufferSize);

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestLogAuditProvider" /> class and uses a default sizing
        ///     <see cref="LogBuffer" />.
        /// </summary>
        public TestLogAuditProvider() => LogBuffer = new List<LogEntry>();

        /// <summary>
        ///     Audits the message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="context">The context - if <see cref="Logger.UseContext" /> is false, this will be empty.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId" /> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AuditThis(
            string msg,
            string context,
            string threadId,
            LogTag logTag)
        {
            LogThis(LoggingLevel.Audit,
                context,
                threadId,
                logTag,
                msg);
        }

        /// <summary>
        ///     Audits an object. Can be used IF a specific object is to be audited by an implementation rather than simply a
        ///     string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="auditObject">The audit object.</param>
        /// <param name="auditLevel">The audit level.</param>
        /// <param name="context">The context - if <see cref="Logger.UseContext" /> is false, this will be empty.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId" /> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        public void AuditThisObject(
            string message,
            object auditObject,
            LoggingLevel auditLevel,
            string context,
            string threadId,
            LogTag logTag)
        {
            string json = JsonConvert.SerializeObject(auditObject);

            LogThis(LoggingLevel.Audit,
                context,
                threadId,
                logTag,
                $"{message} AuditLevel: {auditLevel} :{json}");
        }

        /// <summary>
        ///     The next auditor to pass the audit message on to. Allows additional auditors to be used. Don't create circular
        ///     links though eh!
        /// </summary>
        public IAuditProvider NextAuditor { get; set; }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            TestLogAuditProvider cloned = new TestLogAuditProvider(LogBuffer.Capacity);

            return cloned;
        }

        /// <summary>
        ///     Flush any buffers currently in use.
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
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId" /> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        /// <param name="msg">The message.</param>
        public void LogThis(
            LoggingLevel level,
            string context,
            string threadId,
            LogTag logTag,
            string msg)
        {
            LogBuffer.Add(new LogEntry
            {
                Context = context,
                LogTag = logTag,
                Level = level,
                Message = msg,
                ThreadId = threadId,
            });
        }

        /// <summary>
        ///     The next logger to pass the log message on to. Allows additional loggers to be used. Don't create circular links
        ///     though eh!
        /// </summary>
        public ILoggingProvider NextLogger { get; set; }
    }
}